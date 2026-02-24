using System.ComponentModel.DataAnnotations;

namespace WebUI.Core.Models;

/// <summary>
/// Strategy summary / 策略摘要
/// </summary>
public class StrategyDto
{
    /// <summary>
    /// Strategy ID / 策略 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Strategy name / 策略名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description / 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Strategy type (Custom, Template) / 策略类型
    /// </summary>
    public string StrategyType { get; set; } = string.Empty;

    /// <summary>
    /// Tags (comma-separated) / 标签（逗号分隔）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Is active / 是否激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Version number / 版本号
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Created date / 创建日期
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated date / 最后更新日期
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Last execution timestamp / 最后执行时间
    /// </summary>
    public DateTime? LastRunTime { get; set; }

    /// <summary>
    /// Cumulative return / 累计收益率
    /// </summary>
    public decimal? CumulativeReturn { get; set; }
}

/// <summary>
/// Create strategy request / 创建策略请求
/// </summary>
public class CreateStrategyRequest
{
    /// <summary>
    /// Strategy name / 策略名称
    /// </summary>
    [Required(ErrorMessage = "Name is required / 策略名称必填")]
    [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters / 名称不能超过 200 字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description / 描述
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters / 描述不能超过 1000 字符")]
    public string? Description { get; set; }

    /// <summary>
    /// Strategy type (Custom, Template) / 策略类型
    /// </summary>
    [Required(ErrorMessage = "Strategy type is required / 策略类型必填")]
    [MaxLength(100)]
    public string StrategyType { get; set; } = "Custom";

    /// <summary>
    /// Configuration JSON / 配置 JSON
    /// </summary>
    public string? ConfigurationJson { get; set; }

    /// <summary>
    /// Tags (comma-separated) / 标签（逗号分隔）
    /// </summary>
    [MaxLength(200)]
    public string? Tags { get; set; }
}

/// <summary>
/// Update strategy request / 更新策略请求
/// </summary>
public class UpdateStrategyRequest
{
    /// <summary>
    /// Strategy name / 策略名称
    /// </summary>
    [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters / 名称不能超过 200 字符")]
    public string? Name { get; set; }

    /// <summary>
    /// Description / 描述
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters / 描述不能超过 1000 字符")]
    public string? Description { get; set; }

    /// <summary>
    /// Configuration JSON / 配置 JSON
    /// </summary>
    public string? ConfigurationJson { get; set; }

    /// <summary>
    /// Tags (comma-separated) / 标签（逗号分隔）
    /// </summary>
    [MaxLength(200)]
    public string? Tags { get; set; }
}

/// <summary>
/// Strategy detail / 策略详情
/// </summary>
public class StrategyDetailDto : StrategyDto
{
    /// <summary>
    /// Configuration JSON / 配置 JSON
    /// </summary>
    public string? ConfigurationJson { get; set; }

    /// <summary>
    /// Code file path / 代码文件路径
    /// </summary>
    public string? CodeFilePath { get; set; }

    /// <summary>
    /// Recent executions / 最近执行记录
    /// </summary>
    public List<StrategyExecutionSummary> RecentExecutions { get; set; } = new();
}

/// <summary>
/// Strategy execution summary / 策略执行摘要
/// </summary>
public class StrategyExecutionSummary
{
    /// <summary>
    /// Execution ID / 执行 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Started at / 开始时间
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Stopped at / 停止时间
    /// </summary>
    public DateTime? StoppedAt { get; set; }

    /// <summary>
    /// Status (Running, Stopped, Failed, Completed) / 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Total return (%) / 总收益率
    /// </summary>
    public decimal? TotalReturn { get; set; }

    /// <summary>
    /// Orders executed / 执行订单数
    /// </summary>
    public int OrdersExecuted { get; set; }

    /// <summary>
    /// Error message / 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Strategy list response / 策略列表响应
/// </summary>
public class StrategyListResponse
{
    /// <summary>
    /// Strategies / 策略列表
    /// </summary>
    public List<StrategyDto> Strategies { get; set; } = new();

    /// <summary>
    /// Total count / 总数
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Clone strategy request / 克隆策略请求
/// </summary>
public class CloneStrategyRequest
{
    /// <summary>
    /// New strategy name / 新策略名称
    /// </summary>
    [Required(ErrorMessage = "Name is required / 策略名称必填")]
    [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters / 名称不能超过 200 字符")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Strategy version / 策略版本
/// </summary>
public class StrategyVersionDto
{
    /// <summary>
    /// Version ID / 版本 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Strategy ID / 策略 ID
    /// </summary>
    public int StrategyId { get; set; }

    /// <summary>
    /// Version number / 版本号
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Created date / 创建日期
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Change description / 变更说明
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// Configuration snapshot / 配置快照
    /// </summary>
    public string? ConfigurationJson { get; set; }

    /// <summary>
    /// Code file path / 代码文件路径
    /// </summary>
    public string? CodeFilePath { get; set; }
}

/// <summary>
/// Strategy performance summary / 策略性能摘要
/// </summary>
public class StrategyPerformanceDto
{
    /// <summary>
    /// Strategy ID / 策略 ID
    /// </summary>
    public int StrategyId { get; set; }

    /// <summary>
    /// Strategy name / 策略名称
    /// </summary>
    public string StrategyName { get; set; } = string.Empty;

    /// <summary>
    /// Cumulative return (%) / 累计收益率
    /// </summary>
    public decimal CumulativeReturn { get; set; }

    /// <summary>
    /// Win rate (%) / 胜率
    /// </summary>
    public decimal WinRate { get; set; }

    /// <summary>
    /// Sharpe ratio / 夏普比率
    /// </summary>
    public decimal? SharpeRatio { get; set; }

    /// <summary>
    /// Max drawdown (%) / 最大回撤
    /// </summary>
    public decimal MaxDrawdown { get; set; }

    /// <summary>
    /// Total trades / 总交易数
    /// </summary>
    public int TotalTrades { get; set; }

    /// <summary>
    /// Winning trades / 盈利交易数
    /// </summary>
    public int WinningTrades { get; set; }

    /// <summary>
    /// Losing trades / 亏损交易数
    /// </summary>
    public int LosingTrades { get; set; }

    /// <summary>
    /// Average win / 平均盈利
    /// </summary>
    public decimal? AverageWin { get; set; }

    /// <summary>
    /// Average loss / 平均亏损
    /// </summary>
    public decimal? AverageLoss { get; set; }

    /// <summary>
    /// Profit factor / 盈利因子
    /// </summary>
    public decimal? ProfitFactor { get; set; }
}

/// <summary>
/// Export strategy response / 导出策略响应
/// </summary>
public class ExportStrategyResponse
{
    /// <summary>
    /// File name / 文件名
    ///  </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size (bytes) / 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Download URL / 下载链接
    /// </summary>
    public string? DownloadUrl { get; set; }
}

// ==================== Strategy Execution Models ====================

/// <summary>
/// Start strategy request / 启动策略请求
/// </summary>
public class StartStrategyRequest
{
    /// <summary>
    /// Parameter overrides (JSON) / 参数覆盖（JSON）
    /// </summary>
    public string? ParameterOverrides { get; set; }

    /// <summary>
    /// Broker account ID / 券商账户 ID
    /// </summary>
    public int? BrokerAccountId { get; set; }

    /// <summary>
    /// Enable auto-restart on crash / 启用崩溃自动重启
    /// </summary>
    public bool AutoRestart { get; set; } = false;
}

/// <summary>
/// Stop strategy request / 停止策略请求
/// </summary>
public class StopStrategyRequest
{
    /// <summary>
    /// Force kill if not responding after timeout / 超时后强制终止
    /// </summary>
    public bool ForceKill { get; set; } = false;

    /// <summary>
    /// Timeout in seconds (default 30) / 超时秒数（默认 30）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Strategy execution detail / 策略执行详情
/// </summary>
public class StrategyExecutionDetailDto : StrategyExecutionSummary
{
    /// <summary>
    /// Strategy ID / 策略 ID
    /// </summary>
    public int StrategyId { get; set; }

    /// <summary>
    /// Process ID / 进程 ID
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// Log file path / 日志文件路径
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Initial capital / 初始资金
    /// </summary>
    public decimal? InitialCapital { get; set; }

    /// <summary>
    /// Final capital / 最终资金
    /// </summary>
    public decimal? FinalCapital { get; set; }

    /// <summary>
    /// Runtime duration / 运行时长（秒）
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Broker account ID / 券商账户 ID
    /// </summary>
    public int? BrokerAccountId { get; set; }

    /// <summary>
    /// Strategy name / 策略名称
    /// </summary>
    public string StrategyName { get; set; } = string.Empty;
}

/// <summary>
/// Strategy execution list response / 策略执行列表响应
/// </summary>
public class StrategyExecutionListResponse
{
    /// <summary>
    /// Executions / 执行列表
    /// </summary>
    public List<StrategyExecutionDetailDto> Executions { get; set; } = new();

    /// <summary>
    /// Total count / 总数
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Strategy runtime status / 策略运行时状态
/// </summary>
public class StrategyRuntimeStatusDto
{
    /// <summary>
    /// Strategy ID / 策略 ID
    /// </summary>
    public int StrategyId { get; set; }

    /// <summary>
    /// Is running / 是否运行中
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Current execution ID / 当前执行 ID
    /// </summary>
    public int? CurrentExecutionId { get; set; }

    /// <summary>
    /// Process ID / 进程 ID
    /// </summary>
    public int? ProcessId { get; set; }

    /// <summary>
    /// Started at / 启动时间
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Runtime duration / 运行时长（秒）
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// CPU usage (%) / CPU 使用率
    /// </summary>
    public double? CpuUsage { get; set; }

    /// <summary>
    /// Memory usage (MB) / 内存使用量（MB）
    /// </summary>
    public long? MemoryUsageMB { get; set; }

    /// <summary>
    /// Orders executed / 已执行订单数
    /// </summary>
    public int OrdersExecuted { get; set; }

    /// <summary>
    /// Current positions / 当前持仓数
    /// </summary>
    public int CurrentPositions { get; set; }

    /// <summary>
    /// Current P&L / 当前盈亏
    /// </summary>
    public decimal? CurrentPnL { get; set; }

    /// <summary>
    /// Health status / 健康状态
    /// </summary>
    public string HealthStatus { get; set; } = "Unknown"; // Healthy, Unhealthy, Unknown
}

/// <summary>
/// Schedule strategy execution request / 定时执行策略请求
/// </summary>
public class ScheduleStrategyExecutionRequest
{
    /// <summary>
    /// Schedule type (Once, Daily, Weekly) / 调度类型
    /// </summary>
    [Required(ErrorMessage = "Schedule type is required / 调度类型必填")]
    public string ScheduleType { get; set; } = "Daily";

    /// <summary>
    /// Start time (HH:mm format) / 启动时间（HH:mm 格式）
    /// </summary>
    [Required(ErrorMessage = "Start time is required / 启动时间必填")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid time format, use HH:mm / 时间格式无效，使用 HH:mm")]
    public string StartTime { get; set; } = "09:30";

    /// <summary>
    /// Days of week (for Weekly type, 0=Sunday, 6=Saturday) / 星期几（周调度，0=周日，6=周六）
    /// </summary>
    public List<int>? DaysOfWeek { get; set; }

    /// <summary>
    /// Parameter overrides / 参数覆盖
    /// </summary>
    public string? ParameterOverrides { get; set; }

    /// <summary>
    /// Auto-restart on crash / 崩溃自动重启
    /// </summary>
    public bool AutoRestart { get; set; } = false;

    /// <summary>
    /// Enabled / 启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
