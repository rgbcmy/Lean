using Microsoft.AspNetCore.SignalR;
using WebUI.API.Hubs;
using WebUI.Core.Services;

namespace WebUI.API.Services;

/// <summary>
/// Implementation of market data push service using SignalR.
/// 使用 SignalR 实现的行情数据推送服务。
/// </summary>
public class MarketDataPushService : IMarketDataPushService
{
    private readonly IHubContext<MarketDataHub> _hubContext;
    private readonly ILogger<MarketDataPushService> _logger;
    private readonly Dictionary<string, DateTime> _lastPushTimestamps = new();
    private readonly SemaphoreSlim _throttleLock = new(1, 1);
    private readonly TimeSpan _minPushInterval = TimeSpan.FromSeconds(1); // Throttle: 1 update per second per symbol

    public MarketDataPushService(
        IHubContext<MarketDataHub> hubContext,
        ILogger<MarketDataPushService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PushMarketDataAsync(string symbol, MarketDataUpdate update)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            _logger.LogWarning("Attempted to push market data with empty symbol");
            return;
        }

        var normalizedSymbol = symbol.ToUpperInvariant();

        // Throttle: Check if we should push this update
        if (!await ShouldPushAsync(normalizedSymbol))
        {
            _logger.LogDebug("Throttled market data push for {Symbol}", normalizedSymbol);
            return;
        }

        try
        {
            var groupName = $"MarketData:{normalizedSymbol}";
            
            await _hubContext.Clients.Group(groupName).SendAsync("MarketDataUpdate", new
            {
                symbol = update.Symbol,
                lastPrice = update.LastPrice,
                bidPrice = update.BidPrice,
                askPrice = update.AskPrice,
                volume = update.Volume,
                change = update.Change,
                changePercent = update.ChangePercent,
                marketStatus = update.MarketStatus,
                timestamp = update.Timestamp
            });

            _logger.LogDebug("Pushed market data update for {Symbol}: LastPrice={LastPrice}", normalizedSymbol, update.LastPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing market data for {Symbol}", normalizedSymbol);
        }
    }

    public async Task PushMarketDataBatchAsync(Dictionary<string, MarketDataUpdate> updates)
    {
        if (updates == null || updates.Count == 0)
        {
            return;
        }

        var tasks = updates.Select(kvp => PushMarketDataAsync(kvp.Key, kvp.Value));
        await Task.WhenAll(tasks);

        _logger.LogDebug("Pushed batch of {Count} market data updates", updates.Count);
    }

    private async Task<bool> ShouldPushAsync(string symbol)
    {
        await _throttleLock.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;

            if (_lastPushTimestamps.TryGetValue(symbol, out var lastPush))
            {
                if (now - lastPush < _minPushInterval)
                {
                    return false; // Too soon, throttle this update
                }
            }

            _lastPushTimestamps[symbol] = now;
            return true;
        }
        finally
        {
            _throttleLock.Release();
        }
    }
}
