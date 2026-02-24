using System.ComponentModel.DataAnnotations;

namespace WebUI.Core.Models;

/// <summary>
/// ETF search result / ETF 搜索结果
/// </summary>
public class EtfSearchResult
{
    /// <summary>
    /// ETF symbol / ETF 代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// ETF name / ETF 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Exchange / 交易所
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Tracking index / 追踪指数
    /// </summary>
    public string? TrackingIndex { get; set; }

    /// <summary>
    /// Issuer / 发行商
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Category / 类别
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Latest price / 最新价
    /// </summary>
    public decimal? LastPrice { get; set; }

    /// <summary>
    /// Expense ratio (annual fee) / 费率（年费率）
    /// </summary>
    public decimal? ExpenseRatio { get; set; }

    /// <summary>
    /// Assets under management / 资产规模（美元）
    /// </summary>
    public decimal? Aum { get; set; }

    /// <summary>
    /// Average daily volume / 日均成交量
    /// </summary>
    public long? AvgVolume { get; set; }

    /// <summary>
    /// Is liquid (volume > 100k) / 是否有良好流动性
    /// </summary>
    public bool IsLiquid => AvgVolume.HasValue && AvgVolume.Value >= 100000;
}

/// <summary>
/// ETF search request / ETF 搜索请求
/// </summary>
public class EtfSearchRequest
{
    /// <summary>
    /// Search query (symbol or name) / 搜索查询（代码或名称）
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Filter by category / 按类别筛选
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Filter by issuer / 按发行商筛选
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Minimum average volume / 最小日均成交量
    /// </summary>
    public long? MinVolume { get; set; }

    /// <summary>
    /// Maximum expense ratio / 最大费率
    /// </summary>
    public decimal? MaxExpenseRatio { get; set; }

    /// <summary>
    /// Sort by field (name, expenseRatio, aum, etc.) / 排序字段
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc, desc) / 排序方向
    /// </summary>
    public string? SortDirection { get; set; } = "asc";

    /// <summary>
    /// Page number (1-based) / 页码（从 1 开始）
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size / 每页数量
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// ETF search response / ETF 搜索响应
/// </summary>
public class EtfSearchResponse
{
    /// <summary>
    /// Search results / 搜索结果
    /// </summary>
    public List<EtfSearchResult> Results { get; set; } = new();

    /// <summary>
    /// Total count / 总数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page / 当前页
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size / 每页数量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total pages / 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// ETF详细信息 / ETF detailed information
/// </summary>
public class EtfDetail
{
    /// <summary>
    /// ETF symbol / ETF 代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// ETF name / ETF 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Exchange / 交易所
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Tracking index / 追踪指数
    /// </summary>
    public string? TrackingIndex { get; set; }

    /// <summary>
    /// Issuer / 发行商
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Category / 类别
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Inception date / 成立日期
    /// </summary>
    public DateTime? InceptionDate { get; set; }

    /// <summary>
    /// Assets under management / 资产规模（美元）
    /// </summary>
    public decimal? Aum { get; set; }

    /// <summary>
    /// Expense ratio (annual fee) / 费率（年费率）
    /// </summary>
    public decimal? ExpenseRatio { get; set; }

    /// <summary>
    /// Average daily volume / 日均成交量
    /// </summary>
    public long? AvgVolume { get; set; }

    /// <summary>
    /// Latest price / 最新价
    /// </summary>
    public decimal? LastPrice { get; set; }

    /// <summary>
    /// Day change / 日涨跌幅
    /// </summary>
    public decimal? DayChange { get; set; }

    /// <summary>
    /// Week return / 周收益率
    /// </summary>
    public decimal? WeekReturn { get; set; }

    /// <summary>
    /// Month return / 月收益率
    /// </summary>
    public decimal? MonthReturn { get; set; }

    /// <summary>
    /// Year return / 年收益率
    /// </summary>
    public decimal? YearReturn { get; set; }

    /// <summary>
    /// Top 10 holdings / 前 10 大持仓
    /// </summary>
    public List<EtfHolding> Holdings { get; set; } = new();

    /// <summary>
    /// Dividend yield / 股息率
    /// </summary>
    public decimal? DividendYield { get; set; }

    /// <summary>
    /// Description / 描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// ETF holding / ETF 持仓
/// </summary>
public class EtfHolding
{
    /// <summary>
    /// Stock symbol / 股票代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Company name / 公司名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Weight percentage / 权重百分比
    /// </summary>
    public decimal Weight { get; set; }
}

/// <summary>
/// ETF比较请求 / ETF comparison request
/// </summary>
public class EtfCompareRequest
{
    /// <summary>
    /// ETF symbols to compare (2-5) / 要对比的 ETF 代码（2-5 个）
    /// </summary>
    [Required]
    [MinLength(2, ErrorMessage = "At least 2 ETFs required / 至少需要 2 个 ETF")]
    [MaxLength(5, ErrorMessage = "Maximum 5 ETFs allowed / 最多只能对比 5 个 ETF")]
    public List<string> Symbols { get; set; } = new();

    /// <summary>
    /// Include performance chart data / 包含业绩图表数据
    /// </summary>
    public bool IncludePerformanceData { get; set; } = false;

    /// <summary>
    /// Performance period (days) / 业绩对比周期（天）
    /// </summary>
    [Range(1, 3650)]
    public int PerformancePeriodDays { get; set; } = 365;
}

/// <summary>
/// ETF比较响应 / ETF comparison response
/// </summary>
public class EtfCompareResponse
{
    /// <summary>
    /// Comparison data / 对比数据
    /// </summary>
    public List<EtfComparisonItem> Comparisons { get; set; } = new();

    /// <summary>
    /// Performance chart data (if requested) / 业绩图表数据（如果请求）
    /// </summary>
    public Dictionary<string, List<PerformanceDataPoint>>? PerformanceData { get; set; }
}

/// <summary>
/// ETF对比项 / ETF comparison item
/// </summary>
public class EtfComparisonItem
{
    /// <summary>
    /// ETF symbol / ETF 代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// ETF name / ETF 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Expense ratio / 费率
    /// </summary>
    public decimal? ExpenseRatio { get; set; }

    /// <summary>
    /// Assets under management / 资产规模
    /// </summary>
    public decimal? Aum { get; set; }

    /// <summary>
    /// Tracking error / 追踪误差
    /// </summary>
    public decimal? TrackingError { get; set; }

    /// <summary>
    /// Year return / 年收益率
    /// </summary>
    public decimal? YearReturn { get; set; }

    /// <summary>
    /// Dividend yield / 股息率
    /// </summary>
    public decimal? DividendYield { get; set; }
}

/// <summary>
/// Performance data point / 业绩数据点
/// </summary>
public class PerformanceDataPoint
{
    /// <summary>
    /// Date / 日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Value (normalized return) / 价值（归一化收益）
    /// </summary>
    public decimal Value { get; set; }
}

/// <summary>
/// ETF分红信息 / ETF dividend information
/// </summary>
public class EtfDividend
{
    /// <summary>
    /// Ex-dividend date / 除息日
    /// </summary>
    public DateTime ExDate { get; set; }

    /// <summary>
    /// Payment date / 支付日
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Dividend amount per share / 每股分红金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency / 货币
    /// </summary>
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// ETF分红响应 / ETF dividend response
/// </summary>
public class EtfDividendResponse
{
    /// <summary>
    /// ETF symbol / ETF 代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Dividend history (last 12 months) / 分红历史（最近 12 个月）
    /// </summary>
    public List<EtfDividend> Dividends { get; set; } = new();

    /// <summary>
    /// Annualized dividend yield / 年化股息率
    /// </summary>
    public decimal AnnualizedYield { get; set; }

    /// <summary>
    /// Total dividends (last 12 months) / 总分红（最近 12 个月）
    /// </summary>
    public decimal TotalDividends { get; set; }
}

/// <summary>
/// Recurring investment plan request / 定投计划请求
/// </summary>
public class RecurringPlanRequest
{
    /// <summary>
    /// ETF symbol / ETF 代码
    /// </summary>
    [Required(ErrorMessage = "Symbol is required / 代码必填")]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// ETF name / ETF 名称
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// Investment amount per period / 每期投资金额
    /// </summary>
    [Required(ErrorMessage = "Amount is required / 金额必填")]
    [Range(1, 1000000, ErrorMessage = "Amount must be between 1 and 1,000,000 / 金额必须在 1 到 1,000,000 之间")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency (default: USD) / 货币（默认: USD）
    /// </summary>
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Frequency: Weekly, BiWeekly, Monthly / 频率：Weekly（每周）、BiWeekly（双周）、Monthly（每月）
    /// </summary>
    [Required(ErrorMessage = "Frequency is required / 频率必填")]
    [RegularExpression("^(Weekly|BiWeekly|Monthly)$", ErrorMessage = "Invalid frequency / 无效的频率")]
    public string Frequency { get; set; } = "Monthly";

    /// <summary>
    /// Start date / 开始日期
    /// </summary>
    [Required(ErrorMessage = "Start date is required / 开始日期必填")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date (optional) / 结束日期（可选）
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Notes / 备注
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Recurring investment plan response / 定投计划响应
/// </summary>
public class RecurringPlanResponse
{
    /// <summary>
    /// Plan ID / 计划 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ETF symbol / ETF 代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// ETF name / ETF 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Investment amount per period / 每期投资金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency / 货币
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Frequency / 频率
    /// </summary>
    public string Frequency { get; set; } = "Monthly";

    /// <summary>
    /// Start date / 开始日期
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date / 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Next execution date / 下次执行日期
    /// </summary>
    public DateTime NextExecutionDate { get; set; }

    /// <summary>
    /// Status / 状态
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Execution count / 已执行次数
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// Last execution date / 最后执行日期
    /// </summary>
    public DateTime? LastExecutionDate { get; set; }

    /// <summary>
    /// Last execution status / 最后执行状态
    /// </summary>
    public string? LastExecutionStatus { get; set; }

    /// <summary>
    /// Last execution message / 最后执行消息
    /// </summary>
    public string? LastExecutionMessage { get; set; }

    /// <summary>
    /// Notes / 备注
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Created at / 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated at / 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Update recurring plan request / 更新定投计划请求
/// </summary>
public class UpdateRecurringPlanRequest
{
    /// <summary>
    /// Investment amount per period / 每期投资金额
    /// </summary>
    [Range(1, 1000000, ErrorMessage = "Amount must be between 1 and 1,000,000 / 金额必须在 1 到 1,000,000 之间")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Frequency: Weekly, BiWeekly, Monthly / 频率
    /// </summary>
    [RegularExpression("^(Weekly|BiWeekly|Monthly)$", ErrorMessage = "Invalid frequency / 无效的频率")]
    public string? Frequency { get; set; }

    /// <summary>
    /// End date / 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Notes / 备注
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Status: Active or Paused / 状态：Active（活跃）或 Paused（暂停）
    /// </summary>
    [RegularExpression("^(Active|Paused)$", ErrorMessage = "Status must be Active or Paused / 状态必须是 Active 或 Paused")]
    public string? Status { get; set; }
}
