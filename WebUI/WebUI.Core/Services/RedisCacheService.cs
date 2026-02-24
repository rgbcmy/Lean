using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace WebUI.Core.Services;

/// <summary>
/// Redis cache service implementation / Redis 缓存服务实现
/// Production-ready distributed cache / 生产级分布式缓存
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisCacheService(
        ILogger<RedisCacheService> logger,
        IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            
            if (value.HasValue)
            {
                var result = JsonSerializer.Deserialize<T>(value.ToString());
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return result;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cached value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, jsonValue, expiration);
            _logger.LogDebug("Cached value for key: {Key}, Expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Removed cache key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check key existence: {Key}", key);
            return false;
        }
    }

    public async Task<Dictionary<string, T>> GetManyAsync<T>(
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default) where T : class
    {
        var result = new Dictionary<string, T>();

        try
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            var values = await _database.StringGetAsync(redisKeys);

            for (int i = 0; i < redisKeys.Length; i++)
            {
                if (values[i].HasValue)
                {
                    var obj = JsonSerializer.Deserialize<T>(values[i].ToString());
                    if (obj != null)
                    {
                        result[redisKeys[i].ToString()] = obj;
                    }
                }
            }

            _logger.LogDebug("Retrieved {Count}/{Total} values from cache", result.Count, keys.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get multiple cache values");
        }

        return result;
    }

    public async Task SetManyAsync<T>(
        Dictionary<string, T> values,
        TimeSpan expiration,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var batch = _database.CreateBatch();
            var tasks = new List<Task>();

            foreach (var kvp in values)
            {
                var jsonValue = JsonSerializer.Serialize(kvp.Value);
                tasks.Add(batch.StringSetAsync(kvp.Key, jsonValue, expiration));
            }

            batch.Execute();
            await Task.WhenAll(tasks);

            _logger.LogDebug("Cached {Count} values, Expiration: {Expiration}", values.Count, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache multiple values");
        }
    }
}
