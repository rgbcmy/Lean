using System.ComponentModel.DataAnnotations;

namespace WebUI.Core.Models;

/// <summary>
/// Risk configuration / 风险配置
/// </summary>
public class RiskConfig
{
    /// <summary>
    /// Configuration ID / 配置 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Broker account ID / 券商账户 ID
    /// </summary>
    public int BrokerAccountId { get; set; }

    /// <summary>
    /// Stop-loss rules / 止损规则
    /// </summary>
    public StopLossConfig StopLoss { get; set; } = new();

    /// <summary>
    /// Take-profit rules / 止盈规则
    /// </summary>
    public TakeProfitConfig TakeProfit { get; set; } = new();

    /// <summary>
    /// Position limits / 仓位限制
    /// </summary>
    public PositionLimitsConfig PositionLimits { get; set; } = new();

    /// <summary>
    /// Trading frequency limits / 交易频率限制
    /// </summary>
    public TradingFrequencyConfig TradingFrequency { get; set; } = new();

    /// <summary>
    /// Margin monitoring / 保证金监控
    /// </summary>
    public MarginMonitoringConfig MarginMonitoring { get; set; } = new();

    /// <summary>
    /// Portfolio concentration limits / 投资组合集中度限制
    /// </summary>
    public ConcentrationConfig Concentration { get; set; } = new();

    /// <summary>
    /// Circuit breaker rules / 熔断规则
    /// </summary>
    public CircuitBreakerConfig CircuitBreaker { get; set; } = new();

    /// <summary>
    /// PDT rule enforcement / PDT 规则执行
    /// </summary>
    public bool EnforcePdtRule { get; set; } = true;

    /// <summary>
    /// Created at / 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated at / 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Stop-loss configuration / 止损配置
/// </summary>
public class StopLossConfig
{
    /// <summary>
    /// Enable stop-loss / 启用止损
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Stop-loss percentage (e.g., 5.0 for 5%) / 止损百分比
    /// </summary>
    [Range(0.1, 100.0)]
    public decimal Percentage { get; set; } = 5.0m;

    /// <summary>
    /// Apply stop-loss to all positions / 应用到所有持仓
    /// </summary>
    public bool ApplyToAll { get; set; } = true;

    /// <summary>
    /// Symbol-specific overrides / 特定股票覆盖
    /// </summary>
    public Dictionary<string, decimal> SymbolOverrides { get; set; } = new();
}

/// <summary>
/// Take-profit configuration / 止盈配置
/// </summary>
public class TakeProfitConfig
{
    /// <summary>
    /// Enable take-profit / 启用止盈
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Take-profit percentage (e.g., 10.0 for 10%) / 止盈百分比
    /// </summary>
    [Range(0.1, 1000.0)]
    public decimal Percentage { get; set; } = 10.0m;

    /// <summary>
    /// Apply take-profit to all positions / 应用到所有持仓
    /// </summary>
    public bool ApplyToAll { get; set; } = true;

    /// <summary>
    /// Symbol-specific overrides / 特定股票覆盖
    /// </summary>
    public Dictionary<string, decimal> SymbolOverrides { get; set; } = new();
}

/// <summary>
/// Position limits configuration / 仓位限制配置
/// </summary>
public class PositionLimitsConfig
{
    /// <summary>
    /// Enable position limits / 启用仓位限制
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum position value per stock (USD) / 单个股票最大持仓金额
    /// </summary>
    public decimal? MaxPositionValuePerStock { get; set; } = 10000m;

    /// <summary>
    /// Maximum percentage per stock (e.g., 10.0 for 10% of portfolio) / 单个股票最大持仓百分比
    /// </summary>
    [Range(0.1, 100.0)]
    public decimal MaxPercentagePerStock { get; set; } = 10.0m;

    /// <summary>
    /// Maximum total number of positions / 最大持仓数量
    /// </summary>
    [Range(1, 1000)]
    public int MaxTotalPositions { get; set; } = 20;

    /// <summary>
    /// Minimum cash ratio (e.g., 10.0 for 10% cash reserve) / 最小现金比例
    /// </summary>
    [Range(0.0, 100.0)]
    public decimal MinCashRatio { get; set; } = 10.0m;
}

/// <summary>
/// Trading frequency configuration / 交易频率配置
/// </summary>
public class TradingFrequencyConfig
{
    /// <summary>
    /// Enable trading frequency limits / 启用交易频率限制
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum orders per day / 每日最大订单数
    /// </summary>
    [Range(1, 10000)]
    public int MaxOrdersPerDay { get; set; } = 50;

    /// <summary>
    /// Maximum day trades per 5 business days (PDT rule: 3) / 5 个交易日内最大日内交易次数
    /// </summary>
    [Range(0, 100)]
    public int MaxDayTradesPerFiveDays { get; set; } = 3;

    /// <summary>
    /// Maximum trades per symbol per day / 每个股票每日最大交易次数
    /// </summary>
    [Range(1, 1000)]
    public int MaxTradesPerSymbolPerDay { get; set; } = 10;
}

/// <summary>
/// Margin monitoring configuration / 保证金监控配置
/// </summary>
public class MarginMonitoringConfig
{
    /// <summary>
    /// Enable margin monitoring / 启用保证金监控
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Warning threshold (e.g., 80.0 for 80% margin usage) / 警告阈值
    /// </summary>
    [Range(0.0, 100.0)]
    public decimal WarningThreshold { get; set; } = 80.0m;

    /// <summary>
    /// Critical threshold (e.g., 90.0 for 90% margin usage) / 严重阈值
    /// </summary>
    [Range(0.0, 100.0)]
    public decimal CriticalThreshold { get; set; } = 90.0m;

    /// <summary>
    /// Automatic position liquidation on critical / 严重时自动平仓
    /// </summary>
    public bool AutoLiquidateOnCritical { get; set; } = false;
}

/// <summary>
/// Concentration configuration / 集中度配置
/// </summary>
public class ConcentrationConfig
{
    /// <summary>
    /// Enable concentration checks / 启用集中度检查
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum sector concentration (e.g., 30.0 for 30%) / 最大行业集中度
    /// </summary>
    [Range(0.0, 100.0)]
    public decimal MaxSectorConcentration { get; set; } = 30.0m;

    /// <summary>
    /// Maximum asset class concentration (e.g., 90.0 for 90% in equities) / 最大资产类别集中度
    /// </summary>
    [Range(0.0, 100.0)]
    public decimal MaxAssetClassConcentration { get; set; } = 90.0m;
}

/// <summary>
/// Circuit breaker configuration / 熔断配置
/// </summary>
public class CircuitBreakerConfig
{
    /// <summary>
    /// Enable circuit breaker / 启用熔断
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum price movement percentage to trigger (e.g., 20.0 for ±20%) / 触发熔断的最大价格波动百分比
    /// </summary>
    [Range(0.1, 100.0)]
    public decimal MaxPriceMovementPercent { get; set; } = 20.0m;

    /// <summary>
    /// Time window for price movement (minutes) / 价格波动时间窗口（分钟）
    /// </summary>
    [Range(1, 1440)]
    public int TimeWindowMinutes { get; set; } = 5;

    /// <summary>
    /// Maximum order count in time window / 时间窗口内最大订单数
    /// </summary>
    [Range(1, 10000)]
    public int MaxOrderCountInWindow { get; set; } = 100;

    /// <summary>
    /// Circuit breaker cooldown period (minutes) / 熔断冷却期（分钟）
    /// </summary>
    [Range(1, 1440)]
    public int CooldownMinutes { get; set; } = 30;
}

/// <summary>
/// Risk configuration request / 风险配置请求
/// </summary>
public class RiskConfigRequest
{
    [Required]
    public int BrokerAccountId { get; set; }
    
    public StopLossConfig? StopLoss { get; set; }
    public TakeProfitConfig? TakeProfit { get; set; }
    public PositionLimitsConfig? PositionLimits { get; set; }
    public TradingFrequencyConfig? TradingFrequency { get; set; }
    public MarginMonitoringConfig? MarginMonitoring { get; set; }
    public ConcentrationConfig? Concentration { get; set; }
    public CircuitBreakerConfig? CircuitBreaker { get; set; }
    public bool EnforcePdtRule { get; set; } = true;
}

/// <summary>
/// Risk metrics / 风险指标
/// </summary>
public class RiskMetrics
{
    /// <summary>
    /// Value at Risk (95% confidence, 1 day) / 风险价值
    /// </summary>
    public decimal ValueAtRisk { get; set; }

    /// <summary>
    /// Sharpe Ratio / 夏普比率
    /// </summary>
    public decimal SharpeRatio { get; set; }

    /// <summary>
    /// Maximum Drawdown (percentage) / 最大回撤
    /// </summary>
    public decimal MaxDrawdown { get; set; }

    /// <summary>
    /// Current drawdown (percentage) / 当前回撤
    /// </summary>
    public decimal CurrentDrawdown { get; set; }

    /// <summary>
    /// Sortino Ratio / 索提诺比率
    /// </summary>
    public decimal SortinoRatio { get; set; }

    /// <summary>
    /// Beta (vs market benchmark) / 贝塔值
    /// </summary>
    public decimal Beta { get; set; }

    /// <summary>
    /// Portfolio volatility (annualized) / 投资组合波动率
    /// </summary>
    public decimal Volatility { get; set; }

    /// <summary>
    /// Calculation timestamp / 计算时间戳
    /// </summary>
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Risk report / 风险报告
/// </summary>
public class RiskReport
{
    /// <summary>
    /// Broker account ID / 券商账户 ID
    /// </summary>
    public int BrokerAccountId { get; set; }

    /// <summary>
    /// Report generation time / 报告生成时间
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Risk metrics / 风险指标
    /// </summary>
    public RiskMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Current risk violations / 当前风险违规
    /// </summary>
    public List<RiskViolation> Violations { get; set; } = new();

    /// <summary>
    /// Risk warnings / 风险警告
    /// </summary>
    public List<RiskWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Position risk breakdown / 持仓风险分解
    /// </summary>
    public List<PositionRisk> PositionRisks { get; set; } = new();

    /// <summary>
    /// Overall risk score (0-100, higher = more risk) / 总体风险评分
    /// </summary>
    public decimal OverallRiskScore { get; set; }

    /// <summary>
    /// Risk level / 风险等级
    /// </summary>
    public RiskLevel RiskLevel { get; set; }
}

/// <summary>
/// Risk violation / 风险违规
/// </summary>
public class RiskViolation
{
    /// <summary>
    /// Violation type / 违规类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Severity level / 严重程度
    /// </summary>
    public RiskSeverity Severity { get; set; }

    /// <summary>
    /// Violation message / 违规消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Related symbol (if applicable) / 相关股票
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Detected at / 检测时间
    /// </summary>
    public DateTime DetectedAt { get; set; }

    /// <summary>
    /// Current value / 当前值
    /// </summary>
    public decimal? CurrentValue { get; set; }

    /// <summary>
    /// Threshold value / 阈值
    /// </summary>
    public decimal? ThresholdValue { get; set; }
}

/// <summary>
/// Risk warning / 风险警告
/// </summary>
public class RiskWarning
{
    /// <summary>
    /// Warning type / 警告类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Warning message / 警告消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Related symbol (if applicable) / 相关股票
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Triggered at / 触发时间
    /// </summary>
    public DateTime TriggeredAt { get; set; }
}

/// <summary>
/// Position risk / 持仓风险
/// </summary>
public class PositionRisk
{
    /// <summary>
    /// Symbol / 股票代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Position value / 持仓价值
    /// </summary>
    public decimal PositionValue { get; set; }

    /// <summary>
    /// Portfolio percentage / 组合百分比
    /// </summary>
    public decimal PortfolioPercentage { get; set; }

    /// <summary>
    /// Unrealized P&L / 未实现盈亏
    /// </summary>
    public decimal UnrealizedPnL { get; set; }

    /// <summary>
    /// Unrealized P&L percentage / 未实现盈亏百分比
    /// </summary>
    public decimal UnrealizedPnLPercent { get; set; }

    /// <summary>
    /// Days to stop-loss trigger / 距离止损触发
    /// </summary>
    public decimal? DistanceToStopLoss { get; set; }

    /// <summary>
    /// Days to take-profit trigger / 距离止盈触发
    /// </summary>
    public decimal? DistanceToTakeProfit { get; set; }

    /// <summary>
    /// Position volatility / 持仓波动率
    /// </summary>
    public decimal Volatility { get; set; }

    /// <summary>
    /// Risk score for this position (0-100) / 该持仓的风险评分
    /// </summary>
    public decimal RiskScore { get; set; }
}

/// <summary>
/// Risk check result / 风险检查结果
/// </summary>
public class RiskCheckResult
{
    /// <summary>
    /// Is the action allowed / 是否允许操作
    /// </summary>
    public bool IsAllowed { get; set; }

    /// <summary>
    /// Reason for denial (if not allowed) / 拒绝原因
    /// </summary>
    public string? DenialReason { get; set; }

    /// <summary>
    /// Violations detected / 检测到的违规
    /// </summary>
    public List<RiskViolation> Violations { get; set; } = new();

    /// <summary>
    /// Warnings (action allowed but with warnings) / 警告
    /// </summary>
    public List<RiskWarning> Warnings { get; set; } = new();
}

/// <summary>
/// PDT check result / PDT 检查结果
/// </summary>
public class PdtCheckResult
{
    /// <summary>
    /// Is PDT rule violated / 是否违反 PDT 规则
    /// </summary>
    public bool IsViolated { get; set; }

    /// <summary>
    /// Day trades count in last 5 business days / 最近 5 个交易日内的日内交易次数
    /// </summary>
    public int DayTradesCount { get; set; }

    /// <summary>
    /// Maximum allowed day trades / 最大允许日内交易次数
    /// </summary>
    public int MaxAllowedDayTrades { get; set; }

    /// <summary>
    /// Account equity / 账户净值
    /// </summary>
    public decimal AccountEquity { get; set; }

    /// <summary>
    /// PDT minimum equity requirement (USD 25,000) / PDT 最小净值要求
    /// </summary>
    public decimal MinimumEquityRequirement { get; set; } = 25000m;

    /// <summary>
    /// Is pattern day trader / 是否为模式日内交易者
    /// </summary>
    public bool IsPatternDayTrader { get; set; }

    /// <summary>
    /// Message / 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Risk severity / 风险严重程度
/// </summary>
public enum RiskSeverity
{
    /// <summary>
    /// Low severity / 低
    /// </summary>
    Low,

    /// <summary>
    /// Medium severity / 中
    /// </summary>
    Medium,

    /// <summary>
    /// High severity / 高
    /// </summary>
    High,

    /// <summary>
    /// Critical severity / 严重
    /// </summary>
    Critical
}

/// <summary>
/// Risk level / 风险等级
/// </summary>
public enum RiskLevel
{
    /// <summary>
    /// Very Low risk / 很低
    /// </summary>
    VeryLow,

    /// <summary>
    /// Low risk / 低
    /// </summary>
    Low,

    /// <summary>
    /// Medium risk / 中
    /// </summary>
    Medium,

    /// <summary>
    /// High risk / 高
    /// </summary>
    High,

    /// <summary>
    /// Very High risk / 很高
    /// </summary>
    VeryHigh
}
