using Microsoft.AspNetCore.SignalR;

namespace WebUI.API.Hubs;

/// <summary>
/// SignalR Hub for real-time strategy execution updates.
/// 实时策略执行更新推送 Hub。
/// </summary>
public class StrategyHub : BaseHub
{
    private static readonly Dictionary<string, HashSet<string>> _strategySubscriptions = new();
    private static readonly SemaphoreSlim _subscriptionLock = new(1, 1);

    public StrategyHub(ILogger<StrategyHub> logger) : base(logger)
    {
    }

    /// <summary>
    /// Subscribe to strategy logs for specific strategies.
    /// 订阅指定策略的日志推送。
    /// </summary>
    /// <param name="strategyIds">Array of strategy IDs</param>
    public async Task SubscribeToStrategyLogs(string[] strategyIds)
    {
        if (strategyIds == null || strategyIds.Length == 0)
        {
            _logger.LogWarning("Empty strategyIds array for SubscribeToStrategyLogs from {ConnectionId}", Context.ConnectionId);
            return;
        }

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

        await _subscriptionLock.WaitAsync();
        try
        {
            foreach (var strategyId in strategyIds)
            {
                if (string.IsNullOrWhiteSpace(strategyId))
                {
                    continue;
                }

                var groupName = $"StrategyLog:{strategyId}";

                // Add to SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                // Track subscription
                if (!_strategySubscriptions.ContainsKey(Context.ConnectionId))
                {
                    _strategySubscriptions[Context.ConnectionId] = new HashSet<string>();
                }
                _strategySubscriptions[Context.ConnectionId].Add(strategyId);

                _logger.LogInformation(
                    "Client subscribed to strategy logs: ConnectionId={ConnectionId}, StrategyId={StrategyId}, UserId={UserId}",
                    Context.ConnectionId,
                    strategyId,
                    userId
                );
            }

            await Clients.Caller.SendAsync("StrategySubscriptionConfirmed", new
            {
                success = true,
                strategyIds = strategyIds,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    /// <summary>
    /// Unsubscribe from strategy logs.
    /// 取消订阅策略日志。
    /// </summary>
    /// <param name="strategyIds">Array of strategy IDs</param>
    public async Task UnsubscribeFromStrategyLogs(string[] strategyIds)
    {
        if (strategyIds == null || strategyIds.Length == 0)
        {
            return;
        }

        await _subscriptionLock.WaitAsync();
        try
        {
            foreach (var strategyId in strategyIds)
            {
                if (string.IsNullOrWhiteSpace(strategyId))
                {
                    continue;
                }

                var groupName = $"StrategyLog:{strategyId}";

                // Remove from SignalR group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                // Remove from subscription tracking
                if (_strategySubscriptions.ContainsKey(Context.ConnectionId))
                {
                    _strategySubscriptions[Context.ConnectionId].Remove(strategyId);
                }

                _logger.LogInformation(
                    "Client unsubscribed from strategy logs: ConnectionId={ConnectionId}, StrategyId={StrategyId}",
                    Context.ConnectionId,
                    strategyId
                );
            }

            await Clients.Caller.SendAsync("StrategyUnsubscriptionConfirmed", new
            {
                success = true,
                strategyIds = strategyIds,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    /// <summary>
    /// Subscribe to strategy status updates.
    /// 订阅策略状态更新。
    /// </summary>
    /// <param name="strategyIds">Array of strategy IDs</param>
    public async Task SubscribeToStrategyStatus(string[] strategyIds)
    {
        if (strategyIds == null || strategyIds.Length == 0)
        {
            _logger.LogWarning("Empty strategyIds array for SubscribeToStrategyStatus from {ConnectionId}", Context.ConnectionId);
            return;
        }

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

        await _subscriptionLock.WaitAsync();
        try
        {
            foreach (var strategyId in strategyIds)
            {
                if (string.IsNullOrWhiteSpace(strategyId))
                {
                    continue;
                }

                var groupName = $"StrategyStatus:{strategyId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                _logger.LogInformation(
                    "Client subscribed to strategy status: ConnectionId={ConnectionId}, StrategyId={StrategyId}, UserId={UserId}",
                    Context.ConnectionId,
                    strategyId,
                    userId
                );
            }

            await Clients.Caller.SendAsync("StatusSubscriptionConfirmed", new
            {
                success = true,
                strategyIds = strategyIds,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    /// <summary>
    /// Get current strategy subscriptions for this connection.
    /// 获取当前连接的所有策略订阅。
    /// </summary>
    public async Task GetSubscriptions()
    {
        await _subscriptionLock.WaitAsync();
        try
        {
            var subscriptions = _strategySubscriptions.ContainsKey(Context.ConnectionId)
                ? _strategySubscriptions[Context.ConnectionId].ToArray()
                : Array.Empty<string>();

            await Clients.Caller.SendAsync("CurrentStrategySubscriptions", new
            {
                strategyIds = subscriptions,
                count = subscriptions.Length,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up subscriptions for this connection
        await _subscriptionLock.WaitAsync();
        try
        {
            if (_strategySubscriptions.ContainsKey(Context.ConnectionId))
            {
                var strategyIds = _strategySubscriptions[Context.ConnectionId];
                
                foreach (var strategyId in strategyIds)
                {
                    var logGroupName = $"StrategyLog:{strategyId}";
                    var statusGroupName = $"StrategyStatus:{strategyId}";
                    
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, logGroupName);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, statusGroupName);
                }

                _strategySubscriptions.Remove(Context.ConnectionId);
                
                _logger.LogInformation(
                    "Cleaned up {Count} strategy subscriptions for disconnected client: ConnectionId={ConnectionId}",
                    strategyIds.Count,
                    Context.ConnectionId
                );
            }
        }
        finally
        {
            _subscriptionLock.Release();
        }

        await base.OnDisconnectedAsync(exception);
    }
}
