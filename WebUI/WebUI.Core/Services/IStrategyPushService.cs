namespace WebUI.Core.Services;

/// <summary>
/// Service for pushing strategy execution updates via SignalR.
/// 通过 SignalR 推送策略执行更新的服务接口。
/// </summary>
public interface IStrategyPushService
{
    /// <summary>
    /// Push strategy log message to all subscribers.
    /// 向订阅指定策略的所有客户端推送日志消息。
    /// </summary>
    Task PushStrategyLogAsync(string strategyId, StrategyLogMessage log);

    /// <summary>
    /// Push strategy status update to all subscribers.
    /// 向订阅指定策略的所有客户端推送状态更新。
    /// </summary>
    Task PushStrategyStatusAsync(string strategyId, StrategyStatusUpdate status);

    /// <summary>
    /// Push multiple log messages in batch.
    /// 批量推送日志消息。
    /// </summary>
    Task PushStrategyLogBatchAsync(string strategyId, IEnumerable<StrategyLogMessage> logs);
}

/// <summary>
/// Strategy log message model.
/// 策略日志消息模型。
/// </summary>
public class StrategyLogMessage
{
    /// <summary>
    /// Log level (Debug, Info, Warning, Error)
    /// 日志级别
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Log message
    /// 日志消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp (UTC)
    /// 时间戳 (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Source (e.g., algorithm name, component)
    /// 来源
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Optional additional data
    /// 附加数据
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Strategy status update model.
/// 策略状态更新模型。
/// </summary>
public class StrategyStatusUpdate
{
    /// <summary>
    /// Strategy ID
    /// 策略 ID
    /// </summary>
    public string StrategyId { get; set; } = string.Empty;

    /// <summary>
    /// Strategy status (Running, Stopped, Error, Completed)
    /// 策略状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Current equity
    /// 当前权益
    /// </summary>
    public decimal? Equity { get; set; }

    /// <summary>
    /// Total profit/loss
    /// 总盈亏
    /// </summary>
    public decimal? TotalPnL { get; set; }

    /// <summary>
    /// Number of open positions
    /// 持仓数量
    /// </summary>
    public int? OpenPositions { get; set; }

    /// <summary>
    /// Total number of trades
    /// 总交易次数
    /// </summary>
    public int? TotalTrades { get; set; }

    /// <summary>
    /// Timestamp (UTC)
    /// 时间戳 (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Status message
    /// 状态消息
    /// </summary>
    public string? Message { get; set; }
}
