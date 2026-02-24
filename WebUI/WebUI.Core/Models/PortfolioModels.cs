using System.ComponentModel.DataAnnotations;

namespace WebUI.Core.Models;

/// <summary>
/// Order response (alias for OrderDto) / 订单响应
/// </summary>
public class OrderResponse
{
    public int OrderId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

/// <summary>
/// Position response / 持仓响应
/// </summary>
public class PositionResponse
{
    /// <summary>
    /// Position ID / 持仓 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Stock symbol / 股票代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Position quantity / 持仓数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Average cost per share / 平均成本价
    /// </summary>
    public decimal AverageCost { get; set; }

    /// <summary>
    /// Current market price / 当前市场价
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Current market value / 当前市值
    /// </summary>
    public decimal MarketValue => Quantity * CurrentPrice;

    /// <summary>
    /// Total cost basis / 总成本
    /// </summary>
    public decimal CostBasis => Quantity * AverageCost;

    /// <summary>
    /// Unrealized profit/loss / 未实现盈亏
    /// </summary>
    public decimal UnrealizedPnL { get; set; }

    /// <summary>
    /// Unrealized profit/loss percentage / 未实现盈亏百分比
    /// </summary>
    public decimal UnrealizedPnLPercent => AverageCost > 0 ? (UnrealizedPnL / CostBasis) * 100 : 0;

    /// <summary>
    /// Realized profit/loss / 已实现盈亏
    /// </summary>
    public decimal RealizedPnL { get; set; }

    /// <summary>
    /// First purchase date / 首次买入日期
    /// </summary>
    public DateTime FirstPurchaseDate { get; set; }

    /// <summary>
    /// Holding period in days / 持有天数
    /// </summary>
    public int HoldingDays => (DateTime.UtcNow - FirstPurchaseDate).Days;

    /// <summary>
    /// Last updated time / 最后更新时间
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }
}

/// <summary>
/// Portfolio positions response / 投资组合持仓响应
/// </summary>
public class PortfolioPositionsResponse
{
    /// <summary>
    /// List of positions / 持仓列表
    /// </summary>
    public List<PositionResponse> Positions { get; set; } = new();

    /// <summary>
    /// Total market value / 总市值
    /// </summary>
    public decimal TotalMarketValue { get; set; }

    /// <summary>
    /// Total cost basis / 总成本
    /// </summary>
    public decimal TotalCostBasis { get; set; }

    /// <summary>
    /// Total unrealized P&L / 总未实现盈亏
    /// </summary>
    public decimal TotalUnrealizedPnL { get; set; }

    /// <summary>
    /// Total unrealized P&L percentage / 总未实现盈亏百分比
    /// </summary>
    public decimal TotalUnrealizedPnLPercent => TotalCostBasis > 0 ? (TotalUnrealizedPnL / TotalCostBasis) * 100 : 0;

    /// <summary>
    /// Total realized P&L / 总已实现盈亏
    /// </summary>
    public decimal TotalRealizedPnL { get; set; }

    /// <summary>
    /// Account cash balance / 账户现金余额
    /// </summary>
    public decimal CashBalance { get; set; }

    /// <summary>
    /// Total portfolio value (positions + cash) / 总投资组合价值（持仓+现金）
    /// </summary>
    public decimal TotalPortfolioValue => TotalMarketValue + CashBalance;
}

/// <summary>
/// Position detail response / 持仓详情响应
/// </summary>
public class PositionDetailResponse : PositionResponse
{
    /// <summary>
    /// Related trade history / 相关交易历史
    /// </summary>
    public List<TradeHistoryItem> TradeHistory { get; set; } = new();
}

/// <summary>
/// Trade history item / 交易历史记录
/// </summary>
public class TradeHistoryItem
{
    /// <summary>
    /// Trade date / 交易日期
    /// </summary>
    public DateTime TradeDate { get; set; }

    /// <summary>
    /// Trade side (Buy/Sell) / 交易方向
    /// </summary>
    public string Side { get; set; } = string.Empty;

    /// <summary>
    /// Quantity / 数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Execution price / 成交价格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Total trade value / 交易总额
    /// </summary>
    public decimal TotalValue => Quantity * Price;

    /// <summary>
    /// Realized P&L (for sells) / 已实现盈亏（卖出时）
    /// </summary>
    public decimal? RealizedPnL { get; set; }
}

/// <summary>
/// Close position request / 平仓请求
/// </summary>
public class ClosePositionRequest
{
    /// <summary>
    /// Quantity to close (optional, default is entire position) / 平仓数量（可选，默认全部）
    /// </summary>
    [Range(1, 1000000, ErrorMessage = "Quantity must be between 1 and 1,000,000 / 数量必须在 1 到 1,000,000 之间")]
    public int? Quantity { get; set; }

    /// <summary>
    /// Order type (Market/Limit) default is Market / 订单类型（市价/限价）默认市价
    /// </summary>
    [RegularExpression("^(Market|Limit)$", ErrorMessage = "Order type must be Market or Limit / 订单类型必须是 Market 或 Limit")]
    public string OrderType { get; set; } = "Market";

    /// <summary>
    /// Limit price (required if OrderType is Limit) / 限价价格（限价单必填）
    /// </summary>
    public decimal? LimitPrice { get; set; }
}

/// <summary>
/// Portfolio allocation item / 持仓配置项
/// </summary>
public class AllocationItem
{
    /// <summary>
    /// Symbol or category name / 股票代码或类别名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Market value / 市值
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Percentage of total portfolio / 占总投资组合的百分比
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Unrealized P&L / 未实现盈亏
    /// </summary>
    public decimal UnrealizedPnL { get; set; }
}

/// <summary>
/// Portfolio allocation response / 投资组合配置响应
/// </summary>
public class PortfolioAllocationResponse
{
    /// <summary>
    /// Position allocations / 持仓配置
    /// </summary>
    public List<AllocationItem> Positions { get; set; } = new();

    /// <summary>
    /// Cash allocation / 现金配置
    /// </summary>
    public AllocationItem Cash { get; set; } = new();

    /// <summary>
    /// Total portfolio value / 总投资组合价值
    /// </summary>
    public decimal TotalValue { get; set; }
}

/// <summary>
/// Position filter and sort options / 持仓筛选和排序选项
/// </summary>
public class PositionQueryOptions
{
    /// <summary>
    /// Filter by symbol (partial match) / 按股票代码筛选（部分匹配）
    /// </summary>
    public string? SymbolFilter { get; set; }

    /// <summary>
    /// Show only profitable positions / 仅显示盈利持仓
    /// </summary>
    public bool? OnlyProfitable { get; set; }

    /// <summary>
    /// Show only losing positions / 仅显示亏损持仓
    /// </summary>
    public bool? OnlyLosing { get; set; }

    /// <summary>
    /// Sort field / 排序字段
    /// </summary>
    public string SortBy { get; set; } = "Symbol";

    /// <summary>
    /// Sort direction (asc/desc) / 排序方向
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}

/// <summary>
/// Equity curve data point / 收益曲线数据点
/// </summary>
public class EquityCurvePoint
{
    /// <summary>
    /// Date / 日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Account equity / 账户净值
    /// </summary>
    public decimal Equity { get; set; }

    /// <summary>
    /// Cash balance / 现金余额
    /// </summary>
    public decimal Cash { get; set; }

    /// <summary>
    /// Positions value / 持仓市值
    /// </summary>
    public decimal PositionsValue { get; set; }

    /// <summary>
    /// Cumulative return percentage / 累计收益率
    /// </summary>
    public decimal CumulativeReturn { get; set; }
}

/// <summary>
/// Equity curve response / 收益曲线响应
/// </summary>
public class EquityCurveResponse
{
    /// <summary>
    /// Equity curve data points / 收益曲线数据点
    /// </summary>
    public List<EquityCurvePoint> DataPoints { get; set; } = new();

    /// <summary>
    /// Initial equity / 初始净值
    /// </summary>
    public decimal InitialEquity { get; set; }

    /// <summary>
    /// Current equity / 当前净值
    /// </summary>
    public decimal CurrentEquity { get; set; }

    /// <summary>
    /// Total return / 总收益率
    /// </summary>
    public decimal TotalReturn { get; set; }

    /// <summary>
    /// Maximum drawdown / 最大回撤
    /// </summary>
    public decimal MaxDrawdown { get; set; }
}

/// <summary>
/// Position export format / 持仓导出格式
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// CSV format / CSV 格式
    /// </summary>
    CSV,

    /// <summary>
    /// Excel format / Excel 格式
    /// </summary>
    Excel
}
