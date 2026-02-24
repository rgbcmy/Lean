using WebUI.Core.Services;

namespace WebUI.API.Services;

/// <summary>
/// Background service to periodically flush strategy log buffers.
/// 后台服务：定期刷新策略日志缓冲区。
/// </summary>
public class StrategyLogFlushService : BackgroundService
{
    private readonly IStrategyPushService _strategyPushService;
    private readonly ILogger<StrategyLogFlushService> _logger;
    private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(2);

    public StrategyLogFlushService(
        IStrategyPushService strategyPushService,
        ILogger<StrategyLogFlushService> logger)
    {
        _strategyPushService = strategyPushService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Strategy log flush service started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_flushInterval, stoppingToken);

                try
                {
                    if (_strategyPushService is StrategyPushService service)
                    {
                        await service.FlushAllBuffersAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error flushing strategy log buffers");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
            _logger.LogInformation("Strategy log flush service is stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Strategy log flush service encountered an unexpected error");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Strategy log flush service stopping - flushing remaining buffers");

        // Flush all remaining logs before shutting down
        if (_strategyPushService is StrategyPushService service)
        {
            await service.FlushAllBuffersAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
