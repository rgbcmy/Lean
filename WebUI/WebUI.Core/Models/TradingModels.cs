using System.ComponentModel.DataAnnotations;

namespace WebUI.Core.Models;

/// <summary>
/// Stock search result / 股票搜索结果
/// </summary>
public class StockSearchResult
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
    /// Exchange / 交易所
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Asset type (Stock, ETF) / 资产类型
    /// </summary>
    public string AssetType { get; set; } = "Stock";

    /// <summary>
    /// Latest price / 最新价
    /// </summary>
    public decimal? LastPrice { get; set; }

    /// <summary>
    /// Currency / 货币
    /// </summary>
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Stock search response / 股票搜索响应
/// </summary>
public class StockSearchResponse
{
    /// <summary>
    /// Search results / 搜索结果
    /// </summary>
    public List<StockSearchResult> Results { get; set; } = new();

    /// <summary>
    /// Total count / 总数
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// Detailed stock information / 详细股票信息
/// </summary>
public class StockInfo
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
    /// Exchange / 交易所
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Asset type (Stock, ETF) / 资产类型
    /// </summary>
    public string AssetType { get; set; } = "Stock";

    /// <summary>
    /// Currency / 货币
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Sector / 行业
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// Industry / 子行业
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Market cap / 市值
    /// </summary>
    public decimal? MarketCap { get; set; }

    /// <summary>
    /// Description / 公司描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// CEO / 首席执行官
    /// </summary>
    public string? CEO { get; set; }

    /// <summary>
    /// Employees / 员工数
    /// </summary>
    public int? Employees { get; set; }

    /// <summary>
    /// Website / 网站
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Latest price / 最新价
    /// </summary>
    public decimal? LastPrice { get; set; }

    /// <summary>
    /// 52-week high / 52周最高价
    /// </summary>
    public decimal? Week52High { get; set; }

    /// <summary>
    /// 52-week low / 52周最低价
    /// </summary>
    public decimal? Week52Low { get; set; }

    /// <summary>
    /// Average volume / 平均成交量
    /// </summary>
    public long? AverageVolume { get; set; }

    /// <summary>
    /// P/E ratio / 市盈率
    /// </summary>
    public decimal? PERatio { get; set; }

    /// <summary>
    /// Dividend yield / 股息率
    /// </summary>
    public decimal? DividendYield { get; set; }
}

/// <summary>
/// Base order request / 订单请求基类
/// </summary>
public class OrderRequestBase
{
    /// <summary>
    /// Stock symbol / 股票代码
    /// </summary>
    [Required(ErrorMessage = "Symbol is required / 股票代码必填")]
    [MaxLength(20)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Order side (Buy/Sell) / 买卖方向
    /// </summary>
    [Required(ErrorMessage = "Side is required / 方向必填")]
    [RegularExpression("^(Buy|Sell)$", ErrorMessage = "Side must be Buy or Sell / 方向必须是 Buy 或 Sell")]
    public string Side { get; set; } = "Buy";

    /// <summary>
    /// Order quantity / 订单数量
    /// </summary>
    [Range(1, 1000000, ErrorMessage = "Quantity must be between 1 and 1,000,000 / 数量必须在 1 到 1,000,000 之间")]
    public int Quantity { get; set; }
}

/// <summary>
/// Market order request / 市价单请求
/// </summary>
public class MarketOrderRequest : OrderRequestBase
{
}

/// <summary>
/// Limit order request / 限价单请求
/// </summary>
public class LimitOrderRequest : OrderRequestBase
{
    /// <summary>
    /// Limit price / 限价价格
    /// </summary>
    [Required(ErrorMessage = "Limit price is required / 限价价格必填")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Limit price must be greater than 0 / 限价必须大于 0")]
    public decimal LimitPrice { get; set; }
}

/// <summary>
/// Order submission response / 订单提交响应
/// </summary>
public class OrderSubmissionResponse
{
    /// <summary>
    /// Success flag / 成功标识
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Order ID / 订单 ID
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Broker order ID / 券商订单 ID
    /// </summary>
    public string? BrokerOrderId { get; set; }

    /// <summary>
    /// Message / 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Estimated cost / 预估成本
    /// </summary>
    public decimal? EstimatedCost { get; set; }
}

/// <summary>
/// Order details DTO / 订单详情 DTO
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Order ID / 订单 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Broker order ID / 券商订单 ID
    /// </summary>
    public string BrokerOrderId { get; set; } = string.Empty;

    /// <summary>
    /// Stock symbol / 股票代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Order type / 订单类型
    /// </summary>
    public string OrderType { get; set; } = string.Empty;

    /// <summary>
    /// Order side / 买卖方向
    /// </summary>
    public string Side { get; set; } = string.Empty;

    /// <summary>
    /// Quantity / 数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Filled quantity / 已成交数量
    /// </summary>
    public int FilledQuantity { get; set; }

    /// <summary>
    /// Limit price / 限价价格
    /// </summary>
    public decimal? LimitPrice { get; set; }

    /// <summary>
    /// Filled price / 成交价
    /// </summary>
    public decimal? FilledPrice { get; set; }

    /// <summary>
    /// Order status / 订单状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Created at / 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Filled at / 成交时间
    /// </summary>
    public DateTime? FilledAt { get; set; }

    /// <summary>
    /// Cancelled at / 取消时间
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Error message / 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Order list response / 订单列表响应
/// </summary>
public class OrderListResponse
{
    /// <summary>
    /// Orders / 订单列表
    /// </summary>
    public List<OrderDto> Orders { get; set; } = new();

    /// <summary>
    /// Total count / 总数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Page number / 页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size / 每页数量
    /// </summary>
    public int PageSize { get; set; }
}

/// <summary>
/// Order cancel response / 取消订单响应
/// </summary>
public class OrderCancelResponse
{
    /// <summary>
    /// Success flag / 成功标识
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message / 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Order ID / 订单 ID
    /// </summary>
    public int OrderId { get; set; }
}

/// <summary>
/// Order cost estimate request / 订单成本预估请求
/// </summary>
public class OrderCostEstimateRequest : OrderRequestBase
{
    /// <summary>
    /// Limit price (optional, for limit orders) / 限价价格（可选，用于限价单）
    /// </summary>
    public decimal? LimitPrice { get; set; }
}

/// <summary>
/// Order cost estimate response / 订单成本预估响应
/// </summary>
public class OrderCostEstimateResponse
{
    /// <summary>
    /// Estimated cost / 预估成本
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Estimated commission / 预估佣金
    /// </summary>
    public decimal EstimatedCommission { get; set; }

    /// <summary>
    /// Total cost (cost + commission) / 总成本
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Available buying power / 可用购买力
    /// </summary>
    public decimal? AvailableBuyingPower { get; set; }

    /// <summary>
    /// Sufficient funds / 资金充足
    /// </summary>
    public bool SufficientFunds { get; set; }

    /// <summary>
    /// Warning message / 警告信息
    /// </summary>
    public string? Warning { get; set; }
}

/// <summary>
/// Order query parameters / 订单查询参数
/// </summary>
public class OrderQueryParameters
{
    /// <summary>
    /// Symbol filter / 股票代码筛选
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Status filter / 状态筛选
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Side filter (Buy/Sell) / 方向筛选
    /// </summary>
    public string? Side { get; set; }

    /// <summary>
    /// Order type filter / 订单类型筛选
    /// </summary>
    public string? OrderType { get; set; }

    /// <summary>
    /// Start date / 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date / 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Page number (default: 1) / 页码
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default: 50) / 每页数量
    /// </summary>
    [Range(1, 200)]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Sort by field / 排序字段
    /// </summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort descending / 降序排序
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
