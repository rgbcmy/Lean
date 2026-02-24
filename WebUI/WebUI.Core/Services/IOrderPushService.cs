namespace WebUI.Core.Services;

/// <summary>
/// Service for pushing order and position updates via SignalR.
/// 通过 SignalR 推送订单和持仓更新的服务接口。
/// </summary>
public interface IOrderPushService
{
    /// <summary>
    /// Push order update to a specific user.
    /// 向指定用户推送订单更新。
    /// </summary>
    Task PushOrderUpdateAsync(string userId, OrderUpdate update);

    /// <summary>
    /// Push position update to a specific user.
    /// 向指定用户推送持仓更新。
    /// </summary>
    Task PushPositionUpdateAsync(string userId, PositionUpdate update);

    /// <summary>
    /// Push multiple order updates to a user.
    /// 向用户推送多个订单更新。
    /// </summary>
    Task PushOrderBatchAsync(string userId, IEnumerable<OrderUpdate> updates);
}

/// <summary>
/// Order update model.
/// 订单更新模型。
/// </summary>
public class OrderUpdate
{
    /// <summary>
    /// Order ID
    /// 订单 ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Stock symbol
    /// 股票代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Order side (Buy/Sell)
    /// 买卖方向
    /// </summary>
    public string Side { get; set; } = string.Empty;

    /// <summary>
    /// Order type (Market/Limit)
    /// 订单类型
    /// </summary>
    public string OrderType { get; set; } = string.Empty;

    /// <summary>
    /// Order quantity
    /// 数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Limit price (for limit orders)
    /// 限价 (限价单)
    /// </summary>
    public decimal? LimitPrice { get; set; }

    /// <summary>
    /// Order status (Pending, PartiallyFilled, Filled, Cancelled, Rejected)
    /// 订单状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Filled quantity
    /// 已成交数量
    /// </summary>
    public int FilledQuantity { get; set; }

    /// <summary>
    /// Average fill price
    /// 平均成交价
    /// </summary>
    public decimal? AverageFillPrice { get; set; }

    /// <summary>
    /// Timestamp of the update (UTC)
    /// 更新时间戳 (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Status message (e.g., rejection reason)
    /// 状态消息
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Position update model.
/// 持仓更新模型。
/// </summary>
public class PositionUpdate
{
    /// <summary>
    /// Stock symbol
    /// 股票代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Position quantity (positive for long, negative for short)
    /// 持仓数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Average cost per share
    /// 平均成本
    /// </summary>
    public decimal AverageCost { get; set; }

    /// <summary>
    /// Current market price
    /// 当前市价
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Market value of position
    /// 市值
    /// </summary>
    public decimal MarketValue { get; set; }

    /// <summary>
    /// Unrealized profit/loss
    /// 未实现盈亏
    /// </summary>
    public decimal UnrealizedPnL { get; set; }

    /// <summary>
    /// Unrealized profit/loss percentage
    /// 未实现盈亏百分比
    /// </summary>
    public decimal UnrealizedPnLPercent { get; set; }

    /// <summary>
    /// Timestamp of the update (UTC)
    /// 更新时间戳 (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }
}
