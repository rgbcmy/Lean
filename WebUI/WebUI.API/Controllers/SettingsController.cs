using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Data;
using WebUI.Data.Entities;

namespace WebUI.API.Controllers
{
    /// <summary>
    /// System settings controller — persists all settings (theme, language, IBKR, notifications) to the database.
    /// </summary>
    [ApiController]
    [Route("api/v1/settings")]
    [Authorize]
    [Produces("application/json")]
    public class SettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingsController> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly WebUIDbContext _db;

        private static readonly JsonSerializerOptions _jsonOpts =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public SettingsController(
            IConfiguration configuration,
            ILogger<SettingsController> logger,
            IHostEnvironment hostEnvironment,
            WebUIDbContext db)
        {
            _configuration = configuration;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _db = db;
        }

        /// <summary>
        /// Get current system settings
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(SystemSettingsResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public IActionResult GetSettings()
        {
            try
            {
                var response = new SystemSettingsResponse
                {
                    Ibkr = GetSetting<IbkrSettingsDto>("ibkr") ?? BuildIbkrFromConfig(),
                    Theme = GetSetting<ThemeSettingsDto>("theme") ?? new ThemeSettingsDto
                    {
                        Mode = "light",
                        PrimaryColor = "#1890ff",
                        AccentColor = "#52c41a",
                        ChartColorScheme = "default",
                        PriceColorMode = "chinese",
                    },
                    Language = GetSetting<LanguageSettingsDto>("language") ?? new LanguageSettingsDto
                    {
                        Locale = "zh-CN",
                        DateFormat = "YYYY-MM-DD",
                        TimeFormat = "24h",
                        CurrencyFormat = "$",
                    },
                    Notifications = GetSetting<NotificationSettingsDto>("notifications") ?? new NotificationSettingsDto
                    {
                        Email = new NotificationEmailDto { Enabled = false, Address = "", Events = new NotificationEmailEventsDto() },
                        Push = new NotificationPushDto { Enabled = false, Events = new NotificationPushEventsDto() },
                        InApp = new NotificationInAppDto { Enabled = true, Sound = true, Events = new NotificationInAppEventsDto() },
                    },
                    UpdatedAt = DateTime.UtcNow.ToString("o"),
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settings");
                return StatusCode(500, ErrorResponse.ServerError("Failed to get settings", ex.Message));
            }
        }

        /// <summary>
        /// Update system settings
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(SystemSettingsResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public IActionResult UpdateSettings([FromBody] JsonElement body)
        {
            try
            {
                if (body.TryGetProperty("theme", out var theme))
                    UpsertSetting("theme", theme.GetRawText());

                if (body.TryGetProperty("language", out var language))
                    UpsertSetting("language", language.GetRawText());

                if (body.TryGetProperty("notifications", out var notifications))
                    UpsertSetting("notifications", notifications.GetRawText());

                if (body.TryGetProperty("ibkr", out var ibkrEl))
                {
                    UpsertSetting("ibkr", ibkrEl.GetRawText());
                    PersistIbkrToAppSettings(ibkrEl);
                }

                _db.SaveChanges();

                return GetSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings");
                return StatusCode(500, ErrorResponse.ServerError("Failed to update settings", ex.Message));
            }
        }

        /// <summary>
        /// Test IBKR connection — performs a real TCP probe to host:port with a 5-second timeout.
        /// </summary>
        [HttpPost("ibkr/test")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> TestIbkrConnection([FromBody] IbkrTestRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Host) || request.Port <= 0)
                return BadRequest(new { success = false, message = "请提供有效的主机地址和端口。" });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                using var tcp = new TcpClient();
                var connectTask = tcp.ConnectAsync(request.Host, request.Port);
                var completed = await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cts.Token));

                if (completed == connectTask && connectTask.IsCompletedSuccessfully)
                {
                    _logger.LogInformation("IBKR TCP probe succeeded: {Host}:{Port}", request.Host, request.Port);
                    return Ok(new { success = true, message = $"TCP 连接 {request.Host}:{request.Port} 成功，TWS/Gateway 正在监听。" });
                }

                _logger.LogWarning("IBKR TCP probe timed out: {Host}:{Port}", request.Host, request.Port);
                return Ok(new { success = false, message = $"连接 {request.Host}:{request.Port} 超时（5 秒），请确认 TWS/Gateway 已启动并开放了 API 端口。" });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("IBKR TCP probe timed out: {Host}:{Port}", request.Host, request.Port);
                return Ok(new { success = false, message = $"连接 {request.Host}:{request.Port} 超时（5 秒），请确认 TWS/Gateway 已启动并开放了 API 端口。" });
            }
            catch (SocketException ex)
            {
                _logger.LogWarning(ex, "IBKR TCP probe failed: {Host}:{Port}", request.Host, request.Port);
                return Ok(new { success = false, message = $"连接失败：{ex.Message}（主机 {request.Host}:{request.Port} 拒绝连接或不可达）。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during IBKR TCP probe");
                return Ok(new { success = false, message = $"未知错误：{ex.Message}" });
            }
        }

        // ── Helpers ──────────────────────────────────────────────────

        private T? GetSetting<T>(string key)
        {
            var row = _db.SystemSettings.Find(key);
            if (row == null) return default;
            try { return JsonSerializer.Deserialize<T>(row.Value, _jsonOpts); }
            catch { return default; }
        }

        private void UpsertSetting(string key, string jsonValue)
        {
            var row = _db.SystemSettings.Find(key);
            if (row == null)
            {
                _db.SystemSettings.Add(new SystemSetting { Key = key, Value = jsonValue, UpdatedAt = DateTime.UtcNow });
            }
            else
            {
                row.Value = jsonValue;
                row.UpdatedAt = DateTime.UtcNow;
            }
        }

        private IbkrSettingsDto BuildIbkrFromConfig()
        {
            var s = _configuration.GetSection("IBKR");
            var port = int.TryParse(s["TWSPort"], out var p) ? p : 4002;
            return new IbkrSettingsDto
            {
                Host = s["TWSHost"] ?? "127.0.0.1",
                Port = port,
                ClientId = int.TryParse(s["ClientId"], out var cid) ? cid : 1,
                AccountId = s["AccountId"] ?? "",
                UsePaperTrading = port == 4002 || port == 7497,
                AutoReconnect = true,
                ReconnectIntervalSeconds = 30,
                HeartbeatIntervalSeconds = 60,
            };
        }

        private void PersistIbkrToAppSettings(JsonElement ibkrEl)
        {
            try
            {
                var path = Path.Combine(_hostEnvironment.ContentRootPath, "appsettings.json");
                if (!System.IO.File.Exists(path)) return;

                var node = JsonNode.Parse(System.IO.File.ReadAllText(path))!;
                var ibkrNode = node["IBKR"] as JsonObject ?? new JsonObject();

                if (ibkrEl.TryGetProperty("host", out var h))      ibkrNode["TWSHost"]  = h.GetString();
                if (ibkrEl.TryGetProperty("port", out var po))     ibkrNode["TWSPort"]   = po.GetInt32();
                if (ibkrEl.TryGetProperty("clientId", out var c))  ibkrNode["ClientId"]  = c.GetInt32();
                if (ibkrEl.TryGetProperty("accountId", out var a)) ibkrNode["AccountId"] = a.GetString();

                node["IBKR"] = ibkrNode;
                System.IO.File.WriteAllText(path, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                _logger.LogInformation("IBKR settings persisted to appsettings.json");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist IBKR settings to appsettings.json");
            }
        }
    }

    public class IbkrSettingsDto
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 4002;
        public int ClientId { get; set; } = 1;
        public string AccountId { get; set; } = "";
        public bool UsePaperTrading { get; set; } = true;
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectIntervalSeconds { get; set; } = 30;
        public int HeartbeatIntervalSeconds { get; set; } = 60;
    }

    public class ThemeSettingsDto
    {
        public string Mode { get; set; } = "light";
        public string PrimaryColor { get; set; } = "#1890ff";
        public string AccentColor { get; set; } = "#52c41a";
        public string ChartColorScheme { get; set; } = "default";
        public string PriceColorMode { get; set; } = "chinese";
    }

    public class LanguageSettingsDto
    {
        public string Locale { get; set; } = "zh-CN";
        public string DateFormat { get; set; } = "YYYY-MM-DD";
        public string TimeFormat { get; set; } = "24h";
        public string CurrencyFormat { get; set; } = "$";
    }

    public class NotificationEmailEventsDto
    {
        public bool OrderFilled { get; set; } = true;
        public bool OrderCanceled { get; set; } = true;
        public bool StopLossTriggered { get; set; } = true;
        public bool TakeProfitTriggered { get; set; } = true;
        public bool MarginCall { get; set; } = true;
        public bool StrategyError { get; set; } = true;
        public bool DailyReport { get; set; }
    }

    public class NotificationPushEventsDto
    {
        public bool OrderFilled { get; set; } = true;
        public bool OrderCanceled { get; set; }
        public bool StopLossTriggered { get; set; } = true;
        public bool MarginCall { get; set; } = true;
        public bool RiskAlert { get; set; } = true;
    }

    public class NotificationInAppEventsDto
    {
        public bool All { get; set; } = true;
    }

    public class NotificationEmailDto
    {
        public bool Enabled { get; set; }
        public string Address { get; set; } = "";
        public NotificationEmailEventsDto Events { get; set; } = new();
    }

    public class NotificationPushDto
    {
        public bool Enabled { get; set; }
        public NotificationPushEventsDto Events { get; set; } = new();
    }

    public class NotificationInAppDto
    {
        public bool Enabled { get; set; } = true;
        public bool Sound { get; set; } = true;
        public NotificationInAppEventsDto Events { get; set; } = new();
    }

    public class NotificationSettingsDto
    {
        public NotificationEmailDto Email { get; set; } = new();
        public NotificationPushDto Push { get; set; } = new();
        public NotificationInAppDto InApp { get; set; } = new();
    }

    public class SystemSettingsResponse
    {
        public IbkrSettingsDto Ibkr { get; set; } = new();
        public ThemeSettingsDto Theme { get; set; } = new();
        public LanguageSettingsDto Language { get; set; } = new();
        public NotificationSettingsDto Notifications { get; set; } = new();
        public string UpdatedAt { get; set; } = "";
    }

    public class IbkrTestRequest
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public int ClientId { get; set; }
    }
}
