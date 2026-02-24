using Microsoft.AspNetCore.SignalR;
using WebUI.API.Hubs;
using WebUI.Core.Services;

namespace WebUI.API.Services;

/// <summary>
/// Implementation of order push service using SignalR.
/// 使用 SignalR 实现的订单推送服务。
/// </summary>
public class OrderPushService : IOrderPushService
{
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly ILogger<OrderPushService> _logger;

    public OrderPushService(
        IHubContext<OrderHub> hubContext,
        ILogger<OrderPushService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PushOrderUpdateAsync(string userId, OrderUpdate update)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Attempted to push order update with empty userId");
            return;
        }

        try
        {
            var groupName = $"OrderUpdates:{userId}";
            
            await _hubContext.Clients.Group(groupName).SendAsync("OrderUpdate", new
            {
                orderId = update.OrderId,
                symbol = update.Symbol,
                side = update.Side,
                orderType = update.OrderType,
                quantity = update.Quantity,
                limitPrice = update.LimitPrice,
                status = update.Status,
                filledQuantity = update.FilledQuantity,
                averageFillPrice = update.AverageFillPrice,
                timestamp = update.Timestamp,
                message = update.Message
            });

            _logger.LogInformation(
                "Pushed order update to user {UserId}: OrderId={OrderId}, Status={Status}",
                userId,
                update.OrderId,
                update.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing order update for user {UserId}, OrderId={OrderId}", userId, update.OrderId);
        }
    }

    public async Task PushPositionUpdateAsync(string userId, PositionUpdate update)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Attempted to push position update with empty userId");
            return;
        }

        try
        {
            var groupName = $"PositionUpdates:{userId}";
            
            await _hubContext.Clients.Group(groupName).SendAsync("PositionUpdate", new
            {
                symbol = update.Symbol,
                quantity = update.Quantity,
                averageCost = update.AverageCost,
                currentPrice = update.CurrentPrice,
                marketValue = update.MarketValue,
                unrealizedPnL = update.UnrealizedPnL,
                unrealizedPnLPercent = update.UnrealizedPnLPercent,
                timestamp = update.Timestamp
            });

            _logger.LogDebug(
                "Pushed position update to user {UserId}: Symbol={Symbol}, PnL={PnL}",
                userId,
                update.Symbol,
                update.UnrealizedPnL
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing position update for user {UserId}, Symbol={Symbol}", userId, update.Symbol);
        }
    }

    public async Task PushOrderBatchAsync(string userId, IEnumerable<OrderUpdate> updates)
    {
        if (string.IsNullOrWhiteSpace(userId) || updates == null)
        {
            return;
        }

        var orderUpdates = updates.ToList();
        if (orderUpdates.Count == 0)
        {
            return;
        }

        try
        {
            var groupName = $"OrderUpdates:{userId}";
            
            await _hubContext.Clients.Group(groupName).SendAsync("OrderBatchUpdate", new
            {
                orders = orderUpdates.Select(o => new
                {
                    orderId = o.OrderId,
                    symbol = o.Symbol,
                    side = o.Side,
                    orderType = o.OrderType,
                    quantity = o.Quantity,
                    limitPrice = o.LimitPrice,
                    status = o.Status,
                    filledQuantity = o.FilledQuantity,
                    averageFillPrice = o.AverageFillPrice,
                    timestamp = o.Timestamp,
                    message = o.Message
                }),
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Pushed batch of {Count} order updates to user {UserId}", orderUpdates.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing order batch to user {UserId}", userId);
        }
    }
}
