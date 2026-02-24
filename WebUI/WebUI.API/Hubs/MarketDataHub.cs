using Microsoft.AspNetCore.SignalR;

namespace WebUI.API.Hubs;

/// <summary>
/// SignalR Hub for real-time market data streaming.
/// 实时行情数据推送 Hub。
/// </summary>
public class MarketDataHub : BaseHub
{
    private static readonly Dictionary<string, HashSet<string>> _subscriptions = new();
    private static readonly SemaphoreSlim _subscriptionLock = new(1, 1);

    public MarketDataHub(ILogger<MarketDataHub> logger) : base(logger)
    {
    }

    /// <summary>
    /// Subscribe to market data for specific symbols.
    /// 订阅指定股票代码的行情数据。
    /// </summary>
    /// <param name="symbols">Array of stock symbols (e.g., ["AAPL", "MSFT"])</param>
    public async Task SubscribeToMarketData(string[] symbols)
    {
        if (symbols == null || symbols.Length == 0)
        {
            _logger.LogWarning("Empty symbols array for SubscribeToMarketData from {ConnectionId}", Context.ConnectionId);
            return;
        }

        await _subscriptionLock.WaitAsync();
        try
        {
            foreach (var symbol in symbols)
            {
                if (string.IsNullOrWhiteSpace(symbol))
                {
                    continue;
                }

                var normalizedSymbol = symbol.ToUpperInvariant();
                var groupName = $"MarketData:{normalizedSymbol}";

                // Add to SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                // Track subscription
                if (!_subscriptions.ContainsKey(Context.ConnectionId))
                {
                    _subscriptions[Context.ConnectionId] = new HashSet<string>();
                }
                _subscriptions[Context.ConnectionId].Add(normalizedSymbol);

                _logger.LogInformation(
                    "Client subscribed to market data: ConnectionId={ConnectionId}, Symbol={Symbol}",
                    Context.ConnectionId,
                    normalizedSymbol
                );
            }

            await Clients.Caller.SendAsync("SubscriptionConfirmed", new
            {
                success = true,
                symbols = symbols,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    /// <summary>
    /// Unsubscribe from market data for specific symbols.
    /// 取消订阅指定股票代码的行情数据。
    /// </summary>
    /// <param name="symbols">Array of stock symbols</param>
    public async Task UnsubscribeFromMarketData(string[] symbols)
    {
        if (symbols == null || symbols.Length == 0)
        {
            return;
        }

        await _subscriptionLock.WaitAsync();
        try
        {
            foreach (var symbol in symbols)
            {
                if (string.IsNullOrWhiteSpace(symbol))
                {
                    continue;
                }

                var normalizedSymbol = symbol.ToUpperInvariant();
                var groupName = $"MarketData:{normalizedSymbol}";

                // Remove from SignalR group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                // Remove from subscription tracking
                if (_subscriptions.ContainsKey(Context.ConnectionId))
                {
                    _subscriptions[Context.ConnectionId].Remove(normalizedSymbol);
                }

                _logger.LogInformation(
                    "Client unsubscribed from market data: ConnectionId={ConnectionId}, Symbol={Symbol}",
                    Context.ConnectionId,
                    normalizedSymbol
                );
            }

            await Clients.Caller.SendAsync("UnsubscriptionConfirmed", new
            {
                success = true,
                symbols = symbols,
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    /// <summary>
    /// Get current subscriptions for this connection.
    /// 获取当前连接的所有订阅。
    /// </summary>
    public async Task GetSubscriptions()
    {
        await _subscriptionLock.WaitAsync();
        try
        {
            var subscriptions = _subscriptions.ContainsKey(Context.ConnectionId)
                ? _subscriptions[Context.ConnectionId].ToArray()
                : Array.Empty<string>();

            await Clients.Caller.SendAsync("CurrentSubscriptions", new
            {
                symbols = subscriptions,
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
            if (_subscriptions.ContainsKey(Context.ConnectionId))
            {
                var symbols = _subscriptions[Context.ConnectionId];
                
                foreach (var symbol in symbols)
                {
                    var groupName = $"MarketData:{symbol}";
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                }

                _subscriptions.Remove(Context.ConnectionId);
                
                _logger.LogInformation(
                    "Cleaned up {Count} market data subscriptions for disconnected client: ConnectionId={ConnectionId}",
                    symbols.Count,
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
