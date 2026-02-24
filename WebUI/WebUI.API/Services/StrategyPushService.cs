using Microsoft.AspNetCore.SignalR;
using WebUI.API.Hubs;
using WebUI.Core.Services;

namespace WebUI.API.Services;

/// <summary>
/// Implementation of strategy push service using SignalR.
/// 使用 SignalR 实现的策略推送服务。
/// </summary>
public class StrategyPushService : IStrategyPushService
{
    private readonly IHubContext<StrategyHub> _hubContext;
    private readonly ILogger<StrategyPushService> _logger;
    private readonly Dictionary<string, List<StrategyLogMessage>> _logBuffer = new();
    private readonly SemaphoreSlim _bufferLock = new(1, 1);
    private readonly int _maxBufferSize = 100; // Buffer up to 100 log messages per strategy
    private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(2); // Flush every 2 seconds

    public StrategyPushService(
        IHubContext<StrategyHub> hubContext,
        ILogger<StrategyPushService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PushStrategyLogAsync(string strategyId, StrategyLogMessage log)
    {
        if (string.IsNullOrWhiteSpace(strategyId))
        {
            _logger.LogWarning("Attempted to push strategy log with empty strategyId");
            return;
        }

        await _bufferLock.WaitAsync();
        try
        {
            // Add to buffer
            if (!_logBuffer.ContainsKey(strategyId))
            {
                _logBuffer[strategyId] = new List<StrategyLogMessage>();
            }

            _logBuffer[strategyId].Add(log);

            // Flush if buffer is full
            if (_logBuffer[strategyId].Count >= _maxBufferSize)
            {
                await FlushLogBufferAsync(strategyId);
            }
        }
        finally
        {
            _bufferLock.Release();
        }
    }

    public async Task PushStrategyStatusAsync(string strategyId, StrategyStatusUpdate status)
    {
        if (string.IsNullOrWhiteSpace(strategyId))
        {
            _logger.LogWarning("Attempted to push strategy status with empty strategyId");
            return;
        }

        try
        {
            var groupName = $"StrategyStatus:{strategyId}";
            
            await _hubContext.Clients.Group(groupName).SendAsync("StrategyStatusUpdate", new
            {
                strategyId = status.StrategyId,
                status = status.Status,
                equity = status.Equity,
                totalPnL = status.TotalPnL,
                openPositions = status.OpenPositions,
                totalTrades = status.TotalTrades,
                timestamp = status.Timestamp,
                message = status.Message
            });

            _logger.LogInformation(
                "Pushed strategy status for {StrategyId}: Status={Status}",
                strategyId,
                status.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing strategy status for {StrategyId}", strategyId);
        }
    }

    public async Task PushStrategyLogBatchAsync(string strategyId, IEnumerable<StrategyLogMessage> logs)
    {
        if (string.IsNullOrWhiteSpace(strategyId) || logs == null)
        {
            return;
        }

        var logMessages = logs.ToList();
        if (logMessages.Count == 0)
        {
            return;
        }

        try
        {
            var groupName = $"StrategyLog:{strategyId}";
            
            await _hubContext.Clients.Group(groupName).SendAsync("StrategyLogBatch", new
            {
                strategyId = strategyId,
                logs = logMessages.Select(l => new
                {
                    level = l.Level,
                    message = l.Message,
                    timestamp = l.Timestamp,
                    source = l.Source,
                    data = l.Data
                }),
                count = logMessages.Count,
                timestamp = DateTime.UtcNow
            });

            _logger.LogDebug("Pushed batch of {Count} logs for strategy {StrategyId}", logMessages.Count, strategyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing strategy log batch for {StrategyId}", strategyId);
        }
    }

    /// <summary>
    /// Flush buffered logs for a strategy.
    /// 刷新策略的缓冲日志。
    /// </summary>
    private async Task FlushLogBufferAsync(string strategyId)
    {
        if (!_logBuffer.ContainsKey(strategyId) || _logBuffer[strategyId].Count == 0)
        {
            return;
        }

        var logs = _logBuffer[strategyId].ToList();
        _logBuffer[strategyId].Clear();

        await PushStrategyLogBatchAsync(strategyId, logs);
    }

    /// <summary>
    /// Periodically flush all log buffers.
    /// 定期刷新所有日志缓冲区。
    /// </summary>
    public async Task FlushAllBuffersAsync()
    {
        await _bufferLock.WaitAsync();
        try
        {
            var strategyIds = _logBuffer.Keys.ToList();
            foreach (var strategyId in strategyIds)
            {
                await FlushLogBufferAsync(strategyId);
            }
        }
        finally
        {
            _bufferLock.Release();
        }
    }
}
