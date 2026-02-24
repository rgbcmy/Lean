namespace WebUI.Core.Services;

/// <summary>
/// Cache service interface for market data / 市场数据缓存服务接口
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached value / 获取缓存值
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set cached value with expiration / 设置缓存值（带过期时间）
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove cached value / 删除缓存值
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if key exists / 检查键是否存在
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get multiple values / 获取多个缓存值
    /// </summary>
    Task<Dictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set multiple values / 设置多个缓存值
    /// </summary>
    Task SetManyAsync<T>(Dictionary<string, T> values, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;
}
