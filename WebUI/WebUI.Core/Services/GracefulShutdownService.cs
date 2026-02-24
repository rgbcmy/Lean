using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebUI.Core.Services;

/// <summary>
/// 优雅关闭服务 - 处理应用程序关闭时的清理工作
/// </summary>
public class GracefulShutdownService : IHostedService
{
    private readonly ILogger<GracefulShutdownService> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public GracefulShutdownService(
        ILogger<GracefulShutdownService> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStopping.Register(OnStopping);
        _applicationLifetime.ApplicationStopped.Register(OnStopped);
        
        _logger.LogInformation("应用程序启动完成，优雅关闭服务已就绪");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在停止优雅关闭服务...");
        return Task.CompletedTask;
    }

    private void OnStopping()
    {
        _logger.LogInformation("应用程序正在关闭，开始清理资源...");
        
        // 这里可以添加清理逻辑
        // 例如：
        // - 刷新日志缓冲区
        // - 关闭数据库连接
        // - 取消后台任务
        // - 保存状态
        
        _logger.LogInformation("资源清理完成");
    }

    private void OnStopped()
    {
        _logger.LogInformation("应用程序已完全停止");
    }
}

/// <summary>
/// 请求跟踪服务 - 跟踪正在处理的请求数量
/// </summary>
public class RequestTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTrackingMiddleware> _logger;
    private static int _activeRequests = 0;

    public RequestTrackingMiddleware(
        RequestDelegate next,
        ILogger<RequestTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Interlocked.Increment(ref _activeRequests);
        
        try
        {
            await _next(context);
        }
        finally
        {
            var remaining = Interlocked.Decrement(ref _activeRequests);
            if (remaining == 0)
            {
                _logger.LogDebug("所有请求已处理完成");
            }
        }
    }

    public static int GetActiveRequestCount() => _activeRequests;
}
