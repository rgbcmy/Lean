using Microsoft.AspNetCore.SignalR;

namespace WebUI.API.Hubs;

/// <summary>
/// SignalR Hub for real-time order updates.
/// 实时订单更新推送 Hub。
/// </summary>
public class OrderHub : BaseHub
{
    private static readonly Dictionary<string, string> _userConnections = new();
    private static readonly SemaphoreSlim _connectionLock = new(1, 1);

    public OrderHub(ILogger<OrderHub> logger) : base(logger)
    {
    }

    /// <summary>
    /// Subscribe to order updates for the current user.
    /// 订阅当前用户的订单更新。
    /// </summary>
    public async Task SubscribeToOrderUpdates()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", new
            {
                message = "User ID not found in token",
                timestamp = DateTime.UtcNow
            });
            return;
        }

        await _connectionLock.WaitAsync();
        try
        {
            var groupName = $"OrderUpdates:{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _userConnections[Context.ConnectionId] = userId;

            _logger.LogInformation(
                "Client subscribed to order updates: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                userId
            );

            await Clients.Caller.SendAsync("OrderSubscriptionConfirmed", new
            {
                success = true,
                userId = userId,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Subscribe to position updates for the current user.
    /// 订阅当前用户的持仓更新。
    /// </summary>
    public async Task SubscribeToPositionUpdates()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", new
            {
                message = "User ID not found in token",
                timestamp = DateTime.UtcNow
            });
            return;
        }

        await _connectionLock.WaitAsync();
        try
        {
            var groupName = $"PositionUpdates:{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Client subscribed to position updates: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                userId
            );

            await Clients.Caller.SendAsync("PositionSubscriptionConfirmed", new
            {
                success = true,
                userId = userId,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Unsubscribe from order updates.
    /// 取消订阅订单更新。
    /// </summary>
    public async Task UnsubscribeFromOrderUpdates()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        await _connectionLock.WaitAsync();
        try
        {
            var groupName = $"OrderUpdates:{userId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Client unsubscribed from order updates: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                userId
            );

            await Clients.Caller.SendAsync("OrderUnsubscriptionConfirmed", new
            {
                success = true,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Unsubscribe from position updates.
    /// 取消订阅持仓更新。
    /// </summary>
    public async Task UnsubscribeFromPositionUpdates()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        await _connectionLock.WaitAsync();
        try
        {
            var groupName = $"PositionUpdates:{userId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Client unsubscribed from position updates: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                userId
            );

            await Clients.Caller.SendAsync("PositionUnsubscriptionConfirmed", new
            {
                success = true,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        // Auto-subscribe to order and position updates on connection
        await SubscribeToOrderUpdates();
        await SubscribeToPositionUpdates();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up user connection tracking
        await _connectionLock.WaitAsync();
        try
        {
            if (_userConnections.ContainsKey(Context.ConnectionId))
            {
                var userId = _userConnections[Context.ConnectionId];
                _userConnections.Remove(Context.ConnectionId);

                _logger.LogInformation(
                    "Cleaned up order/position subscriptions for disconnected client: ConnectionId={ConnectionId}, UserId={UserId}",
                    Context.ConnectionId,
                    userId
                );
            }
        }
        finally
        {
            _connectionLock.Release();
        }

        await base.OnDisconnectedAsync(exception);
    }
}
