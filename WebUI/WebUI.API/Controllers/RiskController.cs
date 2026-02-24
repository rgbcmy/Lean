using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Core.Services;

namespace WebUI.API.Controllers;

/// <summary>
/// Risk management controller / 风险管理控制器
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/risk")]
[Produces("application/json")]
public class RiskController : ControllerBase
{
    private readonly IRiskService _riskService;
    private readonly ILogger<RiskController> _logger;

    public RiskController(
        IRiskService riskService,
        ILogger<RiskController> logger)
    {
        _riskService = riskService;
        _logger = logger;
    }

    /// <summary>
    /// Get risk configuration / 获取风险配置
    /// </summary>
    /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Risk configuration / 风险配置</returns>
    /// <response code="200">Returns risk configuration / 返回风险配置</response>
    /// <response code="401">Unauthorized / 未授权</response>
    /// <response code="404">Configuration not found / 配置未找到</response>
    [HttpGet("config")]
    [ProducesResponseType(typeof(RiskConfig), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiskConfig>> GetConfig(
        [FromQuery] int brokerAccountId = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting risk config for broker account {BrokerAccountId}", brokerAccountId);

            var config = await _riskService.GetRiskConfigAsync(brokerAccountId, cancellationToken);
            if (config == null)
            {
                return NotFound(ErrorResponse.NotFound("RiskConfig", "Risk configuration not found"));
            }

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting risk config");
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve risk configuration"));
        }
    }

    /// <summary>
    /// Create or update risk configuration / 创建或更新风险配置
    /// </summary>
    /// <param name="request">Risk configuration request / 风险配置请求</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Updated risk configuration / 更新后的风险配置</returns>
    /// <response code="200">Configuration saved successfully / 配置保存成功</response>
    /// <response code="400">Invalid request / 无效请求</response>
    /// <response code="401">Unauthorized / 未授权</response>
    [HttpPost("config")]
    [HttpPut("config")]
    [ProducesResponseType(typeof(RiskConfig), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RiskConfig>> SaveConfig(
        [FromBody] RiskConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Saving risk config for broker account {BrokerAccountId}", request.BrokerAccountId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ErrorResponse.ValidationError("Invalid risk configuration", errors));
            }

            var config = await _riskService.SaveRiskConfigAsync(request, cancellationToken);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving risk config");
            return StatusCode(500, ErrorResponse.ServerError("Failed to save risk configuration"));
        }
    }

    /// <summary>
    /// Generate risk report / 生成风险报告
    /// </summary>
    /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Comprehensive risk report / 综合风险报告</returns>
    /// <response code="200">Returns risk report / 返回风险报告</response>
    /// <response code="401">Unauthorized / 未授权</response>
    [HttpGet("report")]
    [ProducesResponseType(typeof(RiskReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RiskReport>> GetReport(
        [FromQuery] int brokerAccountId = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating risk report for broker account {BrokerAccountId}", brokerAccountId);

            var report = await _riskService.GenerateRiskReportAsync(brokerAccountId, cancellationToken);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating risk report");
            return StatusCode(500, ErrorResponse.ServerError("Failed to generate risk report"));
        }
    }

    /// <summary>
    /// Get risk metrics / 获取风险指标
    /// </summary>
    /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
    /// <param name="startDate">Start date for calculation / 计算起始日期</param>
    /// <param name="endDate">End date for calculation / 计算结束日期</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Risk metrics (VaR, Sharpe, etc.) / 风险指标</returns>
    /// <response code="200">Returns risk metrics / 返回风险指标</response>
    /// <response code="401">Unauthorized / 未授权</response>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(RiskMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RiskMetrics>> GetMetrics(
        [FromQuery] int brokerAccountId = 1,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Calculating risk metrics for broker account {BrokerAccountId}, period {StartDate} - {EndDate}",
                brokerAccountId, startDate, endDate);

            var metrics = await _riskService.CalculateRiskMetricsAsync(
                brokerAccountId,
                startDate,
                endDate,
                cancellationToken);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating risk metrics");
            return StatusCode(500, ErrorResponse.ServerError("Failed to calculate risk metrics"));
        }
    }

    /// <summary>
    /// Check order risk before submission / 提交订单前检查风险
    /// </summary>
    /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
    /// <param name="symbol">Stock symbol / 股票代码</param>
    /// <param name="side">Order side (BUY/SELL) / 订单方向</param>
    /// <param name="quantity">Order quantity / 订单数量</param>
    /// <param name="price">Order price (for limit orders) / 订单价格</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Risk check result / 风险检查结果</returns>
    /// <response code="200">Returns risk check result / 返回风险检查结果</response>
    /// <response code="401">Unauthorized / 未授权</response>
    [HttpGet("check-order")]
    [ProducesResponseType(typeof(RiskCheckResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RiskCheckResult>> CheckOrderRisk(
        [FromQuery] int brokerAccountId,
        [FromQuery] string symbol,
        [FromQuery] string side,
        [FromQuery] int quantity,
        [FromQuery] decimal? price = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Checking order risk: {Symbol} {Side} {Quantity} @ {Price}",
                symbol, side, quantity, price);

            var result = await _riskService.CheckOrderRiskAsync(
                brokerAccountId,
                symbol,
                side,
                quantity,
                price,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking order risk");
            return StatusCode(500, ErrorResponse.ServerError("Failed to check order risk"));
        }
    }

    /// <summary>
    /// Check PDT (Pattern Day Trader) rule / 检查 PDT 规则
    /// </summary>
    /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
    /// <param name="symbol">Stock symbol / 股票代码</param>
    /// <param name="isClosing">Is this a closing trade / 是否为平仓交易</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>PDT check result / PDT 检查结果</returns>
    /// <response code="200">Returns PDT check result / 返回 PDT 检查结果</response>
    /// <response code="401">Unauthorized / 未授权</response>
    [HttpGet("check-pdt")]
    [ProducesResponseType(typeof(PdtCheckResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PdtCheckResult>> CheckPdt(
        [FromQuery] int brokerAccountId,
        [FromQuery] string symbol,
        [FromQuery] bool isClosing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Checking PDT rule for broker account {BrokerAccountId}, symbol {Symbol}",
                brokerAccountId, symbol);

            var result = await _riskService.CheckPdtRuleAsync(
                brokerAccountId,
                symbol,
                isClosing,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking PDT rule");
            return StatusCode(500, ErrorResponse.ServerError("Failed to check PDT rule"));
        }
    }

    /// <summary>
    /// Monitor positions for stop-loss and take-profit / 监控持仓的止损和止盈
    /// </summary>
    /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Success message / 成功消息</returns>
    /// <response code="200">Monitoring completed / 监控完成</response>
    /// <response code="401">Unauthorized / 未授权</response>
    [HttpPost("monitor")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> MonitorPositions(
        [FromQuery] int brokerAccountId = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Monitoring positions for broker account {BrokerAccountId}", brokerAccountId);

            await _riskService.MonitorPositionsAsync(brokerAccountId, cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Position monitoring completed",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring positions");
            return StatusCode(500, ErrorResponse.ServerError("Failed to monitor positions"));
        }
    }

    /// <summary>
    /// Execute stop-loss for a position / 为持仓执行止损
    /// </summary>
    /// <param name="symbol">Stock symbol / 股票代码</param>
    /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Success message / 成功消息</returns>
    /// <response code="200">Stop-loss executed / 止损已执行</response>
    /// <response code="401">Unauthorized / 未授权</response>
    [HttpPost("stop-loss/{symbol}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ExecuteStopLoss(
        string symbol,
        [FromQuery] int brokerAccountId = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Manually executing stop-loss for {Symbol}", symbol);

            await _riskService.ExecuteStopLossAsync(brokerAccountId, symbol, cancellationToken);

            return Ok(new
            {
                success = true,
                message = $"Stop-loss executed for {symbol}",
                symbol,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing stop-loss for {Symbol}", symbol);
            return StatusCode(500, ErrorResponse.ServerError($"Failed to execute stop-loss for {symbol}"));
        }
    }

    /// <summary>
    /// Execute take-profit for a position / 为持仓执行止盈
    /// </summary>
    /// <param name="symbol">Stock symbol / 股票代码</param>
    /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Success message / 成功消息</returns>
    /// <response code="200">Take-profit executed / 止盈已执行</response>
    /// <response code="401">Unauthorized / 未授权</response>
    [HttpPost("take-profit/{symbol}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ExecuteTakeProfit(
        string symbol,
        [FromQuery] int brokerAccountId = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Manually executing take-profit for {Symbol}", symbol);

            await _riskService.ExecuteTakeProfitAsync(brokerAccountId, symbol, cancellationToken);

            return Ok(new
            {
                success = true,
                message = $"Take-profit executed for {symbol}",
                symbol,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing take-profit for {Symbol}", symbol);
            return StatusCode(500, ErrorResponse.ServerError($"Failed to execute take-profit for {symbol}"));
        }
    }
}
