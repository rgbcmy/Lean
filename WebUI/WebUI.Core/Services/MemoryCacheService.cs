using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WebUI.Core.Services;

/// <summary>
/// In-memory cache service implementation / 内存缓存服务实现
/// Can be replaced with Redis for production / 生产环境可替换为 Redis
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly Timer _cleanupTimer;

    private class CacheEntry
    {
        public required string JsonValue { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public MemoryCacheService(ILogger<MemoryCacheService> logger)
    {
        _logger = logger;
        
        // Cleanup expired entries every minute
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                try
                {
                    var value = JsonSerializer.Deserialize<T>(entry.JsonValue);
                    return Task.FromResult(value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize cached value for key: {Key}", key);
                    _cache.TryRemove(key, out _);
                }
            }
            else
            {
                // Entry expired, remove it
                _cache.TryRemove(key, out _);
            }
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var entry = new CacheEntry
            {
                JsonValue = jsonValue,
                ExpiresAt = DateTime.UtcNow.Add(expiration)
            };

            _cache[key] = entry;
            _logger.LogDebug("Cached value for key: {Key}, Expires: {ExpiresAt}", key, entry.ExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache value for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        _logger.LogDebug("Removed cache key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult(true);
            }
            else
            {
                _cache.TryRemove(key, out _);
            }
        }

        return Task.FromResult(false);
    }

    public async Task<Dictionary<string, T>> GetManyAsync<T>(
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default) where T : class
    {
        var result = new Dictionary<string, T>();

        foreach (var key in keys)
        {
            var value = await GetAsync<T>(key, cancellationToken);
            if (value != null)
            {
                result[key] = value;
            }
        }

        return result;
    }

    public async Task SetManyAsync<T>(
        Dictionary<string, T> values,
        TimeSpan expiration,
        CancellationToken cancellationToken = default) where T : class
    {
        foreach (var kvp in values)
        {
            await SetAsync(kvp.Key, kvp.Value, expiration, cancellationToken);
        }
    }

    private void CleanupExpiredEntries(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.ExpiresAt <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
