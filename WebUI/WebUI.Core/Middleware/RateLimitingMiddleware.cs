using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;

namespace WebUI.Core.Middleware;

/// <summary>
/// 速率限制中间件 - 限制每用户每分钟的请求次数
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _requestsPerMinute;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts;
    private readonly Timer _cleanupTimer;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        int requestsPerMinute = 60)
    {
        _next = next;
        _logger = logger;
        _requestsPerMinute = requestsPerMinute;
        _requestCounts = new ConcurrentDictionary<string, RateLimitInfo>();

        // 每分钟清理一次过期的计数器
        _cleanupTimer = new Timer(CleanupExpiredCounters, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 跳过健康检查端点
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var rateLimitInfo = _requestCounts.GetOrAdd(clientId, _ => new RateLimitInfo
        {
            WindowStart = now,
            RequestCount = 0
        });

        bool shouldBlock = false;
        DateTime resetTime = now;
        int retryAfterSeconds = 0;

        lock (rateLimitInfo)
        {
            // 如果时间窗口已过，重置计数器
            if (now - rateLimitInfo.WindowStart >= TimeSpan.FromMinutes(1))
            {
                rateLimitInfo.WindowStart = now;
                rateLimitInfo.RequestCount = 0;
            }

            // 检查是否超过限制
            if (rateLimitInfo.RequestCount >= _requestsPerMinute)
            {
                shouldBlock = true;
                resetTime = rateLimitInfo.WindowStart.AddMinutes(1);
                retryAfterSeconds = (int)(resetTime - now).TotalSeconds;
            }
            else
            {
                // 递增计数器
                rateLimitInfo.RequestCount++;
                rateLimitInfo.LastRequestTime = now;
            }
        }

        if (shouldBlock)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId}. Limit: {Limit}",
                clientId, _requestsPerMinute);

            await WriteRateLimitResponse(context, resetTime, retryAfterSeconds);
            return;
        }

        // 添加速率限制头
        context.Response.OnStarting(() =>
        {
            var info = _requestCounts.GetValueOrDefault(clientId);
            if (info != null)
            {
                context.Response.Headers["X-RateLimit-Limit"] = _requestsPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = 
                    Math.Max(0, _requestsPerMinute - info.RequestCount).ToString();
                context.Response.Headers["X-RateLimit-Reset"] = 
                    new DateTimeOffset(info.WindowStart.AddMinutes(1)).ToUnixTimeSeconds().ToString();
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // 优先使用用户ID（已认证用户）
        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // 未认证用户使用 IP 地址
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private async Task WriteRateLimitResponse(HttpContext context, DateTime resetTime, int retryAfterSeconds)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
        context.Response.Headers["X-RateLimit-Limit"] = _requestsPerMinute.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = "0";
        context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(resetTime).ToUnixTimeSeconds().ToString();

        var errorResponse = ErrorResponse.RateLimitExceeded(
            $"请求频率超过限制。限制：{_requestsPerMinute} 请求/分钟",
            resetTime);

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private void CleanupExpiredCounters(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _requestCounts
                .Where(kvp => now - kvp.Value.LastRequestTime > TimeSpan.FromMinutes(5))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _requestCounts.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Cleaned up {Count} expired rate limit counters", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up rate limit counters");
        }
    }

    private class RateLimitInfo
    {
        public DateTime WindowStart { get; set; }
        public int RequestCount { get; set; }
        public DateTime LastRequestTime { get; set; }
    }
}
