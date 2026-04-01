namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

/// <summary>
/// Two-level hybrid cache abstraction.
/// L1 = IMemoryCache (in-process, short TTL).
/// L2 = IDistributedCache (Redis primary, SQL Server fallback).
/// All keys are automatically namespaced as {tenantId}:{key}.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by checking L1 (in-memory) first, then L2 (distributed).
    /// Returns null on cache miss.
    /// </summary>
    Task<T?> GetAsync<T>(string tenantId, string key, CancellationToken ct = default)
        where T : class;

    /// <summary>
    /// Sets a value in both L1 (in-memory) and L2 (distributed) caches.
    /// L1 TTL is 2 minutes; L2 TTL defaults to 30 minutes or the provided absoluteExpiry.
    /// </summary>
    Task SetAsync<T>(string tenantId, string key, T value,
                     TimeSpan? absoluteExpiry = null,
                     CancellationToken ct = default)
        where T : class;

    /// <summary>
    /// Removes a key from both L1 and L2 caches.
    /// </summary>
    Task RemoveAsync(string tenantId, string key, CancellationToken ct = default);

    /// <summary>
    /// Removes all L2 cache entries matching the prefix {tenantId}:{prefix}*.
    /// L1 eviction relies on natural expiry since IMemoryCache has no prefix scan.
    /// Implementation deferred for full Redis SCAN support.
    /// </summary>
    Task RemoveByPrefixAsync(string tenantId, string prefix, CancellationToken ct = default);
}
