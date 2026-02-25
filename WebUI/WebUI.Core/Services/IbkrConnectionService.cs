/*
 * IBKR Connection Management Service
 * 使用官方 IBApi 管理到 TWS/Gateway 的长连接生命周期
 * Manages the TWS/Gateway connection lifecycle using the official IBApi client.
 *
 * 职责 / Responsibilities:
 *   - 连接 / 断开 / 自动重连（指数退避）
 *   - 心跳监控（每 10 秒 reqCurrentTime）
 *   - 对上层暴露账户/持仓/订单/行情/历史数据全套 API
 *   - 代理 IbkrTwsApiClient 的所有流式事件
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IBApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;

namespace WebUI.Core.Services
{
    // 
    // IIbkrConnectionService  对外接口（完整版）
    // 

    /// <summary>
    /// High-level IBKR connection service.
    /// Manages connection lifecycle and exposes the full TWS API surface.
    ///
    /// 高层 IBKR 连接服务。
    /// 管理连接生命周期并暴露完整的 TWS API 功能。
    /// </summary>
    public interface IIbkrConnectionService
    {
        //  State 

        /// <summary>Current connection state snapshot. 当前连接状态快照。</summary>
        IbkrConnectionState ConnectionState { get; }

        /// <summary>
        /// The active connection configuration (null when disconnected).
        /// 当前连接配置（未连接时为 null）。
        /// </summary>
        IbkrConnectionConfig? CurrentConfig { get; }

        //  Lifecycle 

        /// <summary>
        /// Connect to IBKR TWS/Gateway using the given configuration.
        /// 使用指定配置连接到 IBKR TWS/Gateway。
        /// </summary>
        Task<bool> ConnectAsync(IbkrConnectionConfig config, CancellationToken cancellationToken = default);

        /// <summary>Gracefully disconnect. 优雅断开连接。</summary>
        Task DisconnectAsync();

        //  Account 

        /// <summary>
        /// Get structured account summary (net liq, buying power, cash, etc.).
        /// 获取账户摘要（净值、购买力、现金等）。
        /// </summary>
        Task<IbkrAccountSummary?> GetAccountSummaryAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get raw account update key-value pairs (full detail).
        /// 获取原始账户更新键值对（完整详情）。
        /// </summary>
        Task<List<IbkrAccountValue>> GetAccountUpdatesAsync(CancellationToken cancellationToken = default);

        //  Positions 

        /// <summary>Fetch all portfolio positions. 获取所有持仓。</summary>
        Task<List<IbkrPositionData>> GetPositionsAsync(CancellationToken cancellationToken = default);

        //  Orders 

        /// <summary>
        /// Place an order. Returns the result containing the assigned order ID.
        /// 下单。返回包含订单 ID 的结果。
        /// </summary>
        Task<IbkrPlaceOrderResult> PlaceOrderAsync(Contract contract, Order order, CancellationToken cancellationToken = default);

        /// <summary>Cancel an open order. 撤销订单。</summary>
        void CancelOrder(int orderId);

        /// <summary>Get all open orders for this client session. 获取本 session 的未成交订单。</summary>
        Task<List<IbkrOrderInfo>> GetOpenOrdersAsync(CancellationToken cancellationToken = default);

        //  Contracts 

        /// <summary>Validate a contract and fetch its details from TWS. 验证合约并获取详情。</summary>
        Task<List<ContractDetails>> GetContractDetailsAsync(Contract contract, CancellationToken cancellationToken = default);

        //  Historical Data 

        /// <summary>
        /// Fetch historical OHLCV bars.
        /// barSizeSetting examples: "1 min", "5 mins", "1 hour", "1 day".
        /// durationStr examples: "1 D", "1 W", "1 M", "1 Y".
        /// 获取历史 K 线数据。
        /// </summary>
        Task<List<IbkrBar>> GetHistoricalDataAsync(
            Contract contract,
            string endDateTime,
            string durationStr,
            string barSizeSetting,
            string whatToShow = "TRADES",
            int useRTH = 1,
            CancellationToken cancellationToken = default);

        //  Market Data 

        /// <summary>
        /// Subscribe to real-time Level 1 market data.
        /// Returns reqId for later unsubscription.
        /// 订阅实时行情，返回 reqId 用于取消订阅。
        /// </summary>
        int SubscribeMarketData(Contract contract, string genericTickList = "");

        /// <summary>Unsubscribe from a real-time market data stream. 取消行情订阅。</summary>
        void UnsubscribeMarketData(int reqId);

        //  Diagnostics 

        /// <summary>Get detailed diagnostics including error history. 获取诊断信息（含错误历史）。</summary>
        IbkrDiagnostics GetDiagnostics();

        //  Events 

        /// <summary>Fired when connection status changes. 连接状态改变时触发。</summary>
        event EventHandler<IbkrConnectionState>? ConnectionStateChanged;

        /// <summary>Fired on every real-time market data tick. 收到行情 tick 时触发。</summary>
        event Action<IbkrMarketDataTick>? OnMarketDataTick;

        /// <summary>Fired when an order status changes (fill, cancel, etc.). 订单状态变化时触发。</summary>
        event Action<IbkrOrderInfo>? OnOrderStatusChanged;

        /// <summary>Fired when a position update is received. 持仓更新时触发。</summary>
        event Action<IbkrPositionData>? OnPositionUpdated;
    }

    // 
    // IbkrConnectionService  实现
    // 

    /// <summary>
    /// Background service that manages a persistent connection to IBKR TWS/Gateway.
    ///
    /// Features:
    ///    Connect / Disconnect with timeout
    ///    Heartbeat via reqCurrentTime every 10 seconds
    ///    Exponential-backoff auto-reconnect (1s  2s  4s    max 60s)
    ///    Full account/position/order/market-data/historical-data API surface
    ///    Error history (last 50 errors)
    ///    Request rate limiting (50 req/s IBKR limit)
    ///
    /// 功能：
    ///    带超时的连接 / 断开
    ///    每 10 秒通过 reqCurrentTime 进行心跳检测
    ///    指数退避自动重连（最大 60 秒）
    ///    完整的账户/持仓/订单/行情/历史数据 API
    ///    错误历史记录（最近 50 条）
    ///    请求频率限制（IBKR 上限 50 req/s）
    /// </summary>
    public class IbkrConnectionService : BackgroundService, IIbkrConnectionService
    {
        private readonly ILogger<IbkrConnectionService> _logger;
        private readonly IIbkrTwsApiClient _twsClient;

        private IbkrConnectionConfig? _config;
        private IbkrConnectionState   _connectionState;
        private readonly ConcurrentQueue<IbkrErrorRecord> _errorHistory = new();
        private Timer? _heartbeatTimer;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private int _reconnectAttempts;
        private bool _reconnecting;

        // Rate limiting
        private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
        private const int MaxRequestsPerSecond = 50;

        //  Public properties 

        /// <inheritdoc />
        public IbkrConnectionState ConnectionState => _connectionState;

        /// <inheritdoc />
        public IbkrConnectionConfig? CurrentConfig => _config;

        //  Events 

        /// <inheritdoc />
        public event EventHandler<IbkrConnectionState>? ConnectionStateChanged;

        /// <inheritdoc />
        public event Action<IbkrMarketDataTick>? OnMarketDataTick;

        /// <inheritdoc />
        public event Action<IbkrOrderInfo>? OnOrderStatusChanged;

        /// <inheritdoc />
        public event Action<IbkrPositionData>? OnPositionUpdated;

        //  Constructor 

        public IbkrConnectionService(
            ILogger<IbkrConnectionService> logger,
            IIbkrTwsApiClient twsClient)
        {
            _logger     = logger;
            _twsClient  = twsClient;

            _connectionState = new IbkrConnectionState
            {
                Status = IbkrConnectionStatus.Disconnected
            };

            // Wire streaming events from the low-level client
            _twsClient.OnMarketDataTick    += tick  => OnMarketDataTick?.Invoke(tick);
            _twsClient.OnOrderStatusChanged+= order => OnOrderStatusChanged?.Invoke(order);
            _twsClient.OnPositionUpdated   += pos   => OnPositionUpdated?.Invoke(pos);
            _twsClient.OnConnectionLost    += reason => HandleConnectionLost(reason);
            _twsClient.OnError             += (id, code, msg) => RecordError(code, msg);
        }

        // 
        // Lifecycle
        // 

        /// <inheritdoc />
        public async Task<bool> ConnectAsync(
            IbkrConnectionConfig config,
            CancellationToken cancellationToken = default)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation(
                    "Connecting to IBKR at {Host}:{Port} account={AccountId} type={AccountType}",
                    config.Host, config.Port, config.AccountId, config.AccountType);

                _config = config;
                UpdateConnectionState(IbkrConnectionStatus.Connecting);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(config.TimeoutSeconds));

                // Delegate to the real IBApi client
                await _twsClient.ConnectAsync(config.Host, config.Port, config.ClientId, cts.Token);

                // Confirm connection and capture server version
                _connectionState.TwsVersion = _twsClient.ServerVersion.ToString();
                _connectionState.ApiVersion  = "Official IBApi";
                _connectionState.AccountId   = config.AccountId;
                _connectionState.AccountType = config.AccountType;
                _connectionState.ConnectedAt = DateTime.UtcNow;
                _connectionState.LastHeartbeatAt = DateTime.UtcNow;
                _reconnectAttempts = 0;
                _reconnecting      = false;

                UpdateConnectionState(IbkrConnectionStatus.Connected);
                StartHeartbeat();

                _logger.LogInformation(
                    "Connected to IBKR TWS v{TwsVer} (account={AccountId})",
                    _connectionState.TwsVersion, config.AccountId);

                return true;
            }
            catch (OperationCanceledException)
            {
                var msg = $"Connection timeout after {config.TimeoutSeconds}s  TWS/Gateway not responding at {config.Host}:{config.Port}";
                _logger.LogWarning(msg);
                RecordError(408, msg);
                UpdateConnectionState(IbkrConnectionStatus.Error, msg);
                return false;
            }
            catch (Exception ex)
            {
                var msg = $"Connection failed: {ex.Message}";
                _logger.LogError(ex, "Failed to connect to IBKR");
                RecordError(500, msg, ex.ToString());
                UpdateConnectionState(IbkrConnectionStatus.Error, msg);
                return false;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <inheritdoc />
        public async Task DisconnectAsync()
        {
            await _connectionLock.WaitAsync();
            try
            {
                _logger.LogInformation("Disconnecting from IBKR");
                StopHeartbeat();
                _reconnecting = false;
                _twsClient.Disconnect();
                UpdateConnectionState(IbkrConnectionStatus.Disconnected);
                _connectionState.ConnectedAt    = null;
                _connectionState.LastHeartbeatAt = null;
                _logger.LogInformation("Disconnected from IBKR");
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        // 
        // Account
        // 

        /// <inheritdoc />
        public async Task<IbkrAccountSummary?> GetAccountSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureConnected("GetAccountSummaryAsync");
            await RateLimitAsync(cancellationToken);

            try
            {
                // Request all available account summary tags via the official API helper
                var values = await _twsClient.GetAccountSummaryAsync(
                    group: "All",
                    tags: AccountSummaryTags.GetAllTags(),
                    ct: cancellationToken);

                // Parse common tags into the structured model
                var summary = new IbkrAccountSummary
                {
                    AccountId   = _config!.AccountId,
                    AccountType = _config.AccountType,
                    UpdatedAt   = DateTime.UtcNow
                };

                foreach (var v in values)
                {
                    if (!decimal.TryParse(v.Val,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out decimal d)) continue;

                    switch (v.Key)
                    {
                        case "NetLiquidation":    summary.NetLiquidation    = d; break;
                        case "TotalCashValue":    summary.CashBalance       = d; break;
                        case "AvailableFunds":    summary.AvailableFunds    = d; break;
                        case "BuyingPower":       summary.BuyingPower       = d; break;
                        case "GrossPositionValue":summary.GrossPositionValue = d; break;
                        case "MaintMarginReq":    summary.MarginRequirement = d; break;
                        case "ExcessLiquidity":   summary.ExcessLiquidity   = d; break;
                    }

                    if (v.Key == "NetLiquidation" && !string.IsNullOrEmpty(v.Currency))
                        summary.BaseCurrency = v.Currency;
                }

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccountSummaryAsync failed");
                RecordError(998, "Failed to get account summary", ex.Message);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<List<IbkrAccountValue>> GetAccountUpdatesAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureConnected("GetAccountUpdatesAsync");
            await RateLimitAsync(cancellationToken);
            return await _twsClient.GetAccountUpdatesAsync(_config!.AccountId, cancellationToken);
        }

        // 
        // Positions
        // 

        /// <inheritdoc />
        public async Task<List<IbkrPositionData>> GetPositionsAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureConnected("GetPositionsAsync");
            await RateLimitAsync(cancellationToken);
            return await _twsClient.GetPositionsAsync(cancellationToken);
        }

        // 
        // Orders
        // 

        /// <inheritdoc />
        public async Task<IbkrPlaceOrderResult> PlaceOrderAsync(
            Contract contract, Order order,
            CancellationToken cancellationToken = default)
        {
            EnsureConnected("PlaceOrderAsync");
            await RateLimitAsync(cancellationToken);
            return await _twsClient.PlaceOrderAsync(contract, order, cancellationToken);
        }

        /// <inheritdoc />
        public void CancelOrder(int orderId)
        {
            EnsureConnected("CancelOrder");
            _twsClient.CancelOrder(orderId);
        }

        /// <inheritdoc />
        public async Task<List<IbkrOrderInfo>> GetOpenOrdersAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureConnected("GetOpenOrdersAsync");
            await RateLimitAsync(cancellationToken);
            return await _twsClient.GetOpenOrdersAsync(cancellationToken);
        }

        // 
        // Contracts
        // 

        /// <inheritdoc />
        public async Task<List<ContractDetails>> GetContractDetailsAsync(
            Contract contract,
            CancellationToken cancellationToken = default)
        {
            EnsureConnected("GetContractDetailsAsync");
            await RateLimitAsync(cancellationToken);
            return await _twsClient.GetContractDetailsAsync(contract, cancellationToken);
        }

        // 
        // Historical Data
        // 

        /// <inheritdoc />
        public async Task<List<IbkrBar>> GetHistoricalDataAsync(
            Contract contract,
            string endDateTime,
            string durationStr,
            string barSizeSetting,
            string whatToShow = "TRADES",
            int useRTH = 1,
            CancellationToken cancellationToken = default)
        {
            EnsureConnected("GetHistoricalDataAsync");
            await RateLimitAsync(cancellationToken);
            return await _twsClient.GetHistoricalDataAsync(
                contract, endDateTime, durationStr,
                barSizeSetting, whatToShow, useRTH, cancellationToken);
        }

        // 
        // Market Data
        // 

        /// <inheritdoc />
        public int SubscribeMarketData(Contract contract, string genericTickList = "")
        {
            EnsureConnected("SubscribeMarketData");
            return _twsClient.SubscribeMarketData(contract, genericTickList);
        }

        /// <inheritdoc />
        public void UnsubscribeMarketData(int reqId) =>
            _twsClient.UnsubscribeMarketData(reqId);

        // 
        // Diagnostics
        // 

        /// <inheritdoc />
        public IbkrDiagnostics GetDiagnostics()
        {
            var maskedConfig = _config == null ? null : new IbkrConnectionConfig
            {
                Host                = _config.Host,
                Port                = _config.Port,
                AccountId           = MaskAccountId(_config.AccountId),
                AccountType         = _config.AccountType,
                ClientId            = _config.ClientId,
                TimeoutSeconds      = _config.TimeoutSeconds,
                EnableAutoReconnect = _config.EnableAutoReconnect,
                MaxReconnectAttempts= _config.MaxReconnectAttempts
            };

            return new IbkrDiagnostics
            {
                ConnectionState = _connectionState,
                ErrorHistory    = _errorHistory.ToList(),
                Configuration   = maskedConfig,
                RateLimitStatus = new IbkrRateLimitStatus
                {
                    CurrentRequestsPerSecond = GetCurrentRequestRate(),
                    MaxRequestsPerSecond     = MaxRequestsPerSecond,
                    QueuedRequests           = 0
                }
            };
        }

        // 
        // Background Service
        // 

        /// <summary>
        /// Background loop: periodically cleans up old rate-limit timestamps.
        /// 后台循环：定期清理过期的限速时间戳。
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IbkrConnectionService background loop started");
            while (!stoppingToken.IsCancellationRequested)
            {
                CleanupOldRequestTimestamps();
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            _logger.LogInformation("IbkrConnectionService background loop stopped");
        }

        // 
        // Heartbeat
        // 

        /// <summary>
        /// Start the heartbeat timer (10-second interval).
        /// 启动心跳定时器（每 10 秒）。
        /// </summary>
        private void StartHeartbeat()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = new Timer(
                async _ => await PerformHeartbeatAsync(),
                state: null,
                dueTime:  TimeSpan.FromSeconds(10),
                period:   TimeSpan.FromSeconds(10));
        }

        private void StopHeartbeat()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
        }

        /// <summary>
        /// Heartbeat via reqCurrentTime. Updates LastHeartbeatAt on success.
        /// Triggers reconnect if the call fails or times out.
        /// 通过 reqCurrentTime 进行心跳检测。成功时更新 LastHeartbeatAt；失败时触发重连。
        /// </summary>
        private async Task PerformHeartbeatAsync()
        {
            if (_connectionState.Status != IbkrConnectionStatus.Connected) return;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _twsClient.RequestCurrentTimeAsync(cts.Token);
                _connectionState.LastHeartbeatAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Heartbeat failed  connection may be lost");
                HandleConnectionLost($"Heartbeat failed: {ex.Message}");
            }
        }

        // 
        // Auto-Reconnect
        // 

        /// <summary>
        /// Called when the underlying client detects a disconnection.
        /// 当底层客户端检测到断线时调用。
        /// </summary>
        private void HandleConnectionLost(string reason)
        {
            if (_connectionState.Status == IbkrConnectionStatus.Disconnected) return; // intentional disconnect
            if (_reconnecting) return;

            _logger.LogWarning("Connection lost: {Reason}", reason);
            RecordError(504, $"Connection lost: {reason}");

            if (_config?.EnableAutoReconnect == true)
            {
                _ = Task.Run(TriggerReconnectAsync);
            }
            else
            {
                UpdateConnectionState(IbkrConnectionStatus.Error, reason);
            }
        }

        /// <summary>
        /// Auto-reconnect loop with exponential backoff.
        /// 带指数退避的自动重连循环。
        /// </summary>
        private async Task TriggerReconnectAsync()
        {
            _reconnecting = true;
            StopHeartbeat();
            UpdateConnectionState(IbkrConnectionStatus.Reconnecting);

            int maxAttempts = _config?.MaxReconnectAttempts ?? 10;

            while (_reconnectAttempts < maxAttempts && _reconnecting)
            {
                _reconnectAttempts++;

                // Exponential backoff: 1s, 2s, 4s, 8s,  capped at 60s
                double delaySec = Math.Min(Math.Pow(2, _reconnectAttempts - 1), 60);
                _connectionState.NextReconnectDelaySeconds = (int)delaySec;
                OnConnectionStateChanged();

                _logger.LogInformation(
                    "Auto-reconnect attempt {Attempt}/{Max} in {Delay}s",
                    _reconnectAttempts, maxAttempts, delaySec);

                await Task.Delay(TimeSpan.FromSeconds(delaySec));

                if (!_reconnecting || _config == null) break;

                try
                {
                    await _twsClient.ConnectAsync(
                        _config.Host, _config.Port, _config.ClientId);

                    _connectionState.ConnectedAt    = DateTime.UtcNow;
                    _connectionState.LastHeartbeatAt = DateTime.UtcNow;
                    _connectionState.TwsVersion      = _twsClient.ServerVersion.ToString();
                    _reconnectAttempts = 0;
                    _reconnecting      = false;

                    UpdateConnectionState(IbkrConnectionStatus.Connected);
                    StartHeartbeat();

                    _logger.LogInformation(
                        "Auto-reconnect succeeded (attempt {Attempt})", _reconnectAttempts);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Auto-reconnect attempt {Attempt}/{Max} failed", _reconnectAttempts, maxAttempts);
                    RecordError(500, $"Reconnect attempt {_reconnectAttempts} failed", ex.Message);
                }
            }

            _reconnecting = false;
            _logger.LogError("All {Max} reconnect attempts exhausted", maxAttempts);
            UpdateConnectionState(IbkrConnectionStatus.Error, "Max reconnect attempts reached");
        }

        // 
        // Rate Limiting (IBKR limit: 50 req/s)
        // 

        /// <summary>
        /// Enforce the 50 req/s IBKR rate limit. Waits 1 second if limit is reached.
        /// 强制执行 IBKR 50 req/s 限制。达到限制时等待 1 秒。
        /// </summary>
        private async Task RateLimitAsync(CancellationToken ct = default)
        {
            _requestTimestamps.Enqueue(DateTime.UtcNow);
            if (GetCurrentRequestRate() >= MaxRequestsPerSecond)
            {
                _logger.LogDebug("Rate limit reached ({Max} req/s)  throttling 1s", MaxRequestsPerSecond);
                await Task.Delay(1000, ct);
            }
        }

        private int GetCurrentRequestRate()
        {
            var since = DateTime.UtcNow.AddSeconds(-1);
            return _requestTimestamps.Count(ts => ts > since);
        }

        private void CleanupOldRequestTimestamps()
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-2);
            while (_requestTimestamps.TryPeek(out var ts) && ts < cutoff)
                _requestTimestamps.TryDequeue(out _);
        }

        // 
        // Helpers
        // 

        private void EnsureConnected(string operation)
        {
            if (_connectionState.Status != IbkrConnectionStatus.Connected)
                throw new InvalidOperationException(
                    $"{operation}: not connected to IBKR (status={_connectionState.Status}).");
        }

        private void UpdateConnectionState(IbkrConnectionStatus status, string? error = null)
        {
            _connectionState.Status            = status;
            _connectionState.LastError         = error;
            _connectionState.ReconnectAttempts = _reconnectAttempts;
            if (status != IbkrConnectionStatus.Reconnecting)
                _connectionState.NextReconnectDelaySeconds = null;
            OnConnectionStateChanged();
        }

        private void OnConnectionStateChanged() =>
            ConnectionStateChanged?.Invoke(this, _connectionState);

        private void RecordError(int code, string message, string? details = null)
        {
            _errorHistory.Enqueue(new IbkrErrorRecord
            {
                Timestamp = DateTime.UtcNow,
                ErrorCode = code,
                Message   = message,
                Details   = details
            });
            // Keep only the last 50 errors
            while (_errorHistory.Count > 50)
                _errorHistory.TryDequeue(out _);
        }

        private static string MaskAccountId(string accountId)
        {
            if (string.IsNullOrEmpty(accountId) || accountId.Length <= 4)
                return "****";
            return "****" + accountId[^4..];
        }

        public override void Dispose()
        {
            StopHeartbeat();
            _reconnecting = false;
            _connectionLock.Dispose();
            base.Dispose();
        }
    }
}
