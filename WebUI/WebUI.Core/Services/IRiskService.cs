namespace WebUI.Core.Services;

using WebUI.Core.Models;

/// <summary>
/// Risk management service interface / 风险管理服务接口
/// </summary>
public interface IRiskService
{
    /// <summary>
    /// Get risk configuration for a broker account / 获取券商账户的风险配置
    /// </summary>
    Task<RiskConfig?> GetRiskConfigAsync(int brokerAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update risk configuration / 创建或更新风险配置
    /// </summary>
    Task<RiskConfig> SaveRiskConfigAsync(RiskConfigRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an order passes risk checks / 检查订单是否通过风险检查
    /// </summary>
    Task<RiskCheckResult> CheckOrderRiskAsync(int brokerAccountId, string symbol, string side, int quantity, decimal? price = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Monitor all positions for stop-loss and take-profit triggers / 监控所有持仓的止损和止盈触发
    /// </summary>
    Task MonitorPositionsAsync(int brokerAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check position limits before trade / 交易前检查仓位限制
    /// </summary>
    Task<RiskCheckResult> CheckPositionLimitsAsync(int brokerAccountId, string symbol, decimal orderValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check trading frequency limits / 检查交易频率限制
    /// </summary>
    Task<RiskCheckResult> CheckTradingFrequencyAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check margin usage and alert if exceeds threshold / 检查保证金使用并在超过阈值时告警
    /// </summary>
    Task<RiskCheckResult> CheckMarginUsageAsync(int brokerAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check portfolio concentration / 检查投资组合集中度
    /// </summary>
    Task<RiskCheckResult> CheckConcentrationAsync(int brokerAccountId, string symbol, decimal positionValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check circuit breaker conditions / 检查熔断条件
    /// </summary>
    Task<RiskCheckResult> CheckCircuitBreakerAsync(int brokerAccountId, string symbol, decimal currentPrice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check PDT (Pattern Day Trader) rule / 检查 PDT 规则
    /// </summary>
    Task<PdtCheckResult> CheckPdtRuleAsync(int brokerAccountId, string symbol, bool isClosing, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate risk metrics (VaR, Sharpe Ratio, Max Drawdown) / 计算风险指标
    /// </summary>
    Task<RiskMetrics> CalculateRiskMetricsAsync(int brokerAccountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate comprehensive risk report / 生成综合风险报告
    /// </summary>
    Task<RiskReport> GenerateRiskReportAsync(int brokerAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute stop-loss for a position / 为持仓执行止损
    /// </summary>
    Task ExecuteStopLossAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute take-profit for a position / 为持仓执行止盈
    /// </summary>
    Task ExecuteTakeProfitAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default);
}
