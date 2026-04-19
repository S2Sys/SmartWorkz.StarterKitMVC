namespace SmartWorkz.Core.Shared.Caching;

/// <summary>
/// Service for caching with tenant isolation and L1/L2 hybrid support.
/// Implementations may use memory cache (L1) and distributed cache (L2).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key for current tenant.
    /// </summary>
    /// <typeparam name="T">Type of cached value.</typeparam>
    /// <param name="key">Cache key (will be tenant-scoped automatically).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cached value or null if not found or expired.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with optional TTL for current tenant.
    /// </summary>
    /// <typeparam name="T">Type of value to cache.</typeparam>
    /// <param name="key">Cache key (will be tenant-scoped automatically).</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="ttl">Time to live. If null, uses default (usually 30 minutes).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached value by key for current tenant.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values matching a key prefix for current tenant.
    /// Example: RemoveByPrefixAsync("user:") removes "user:1", "user:2", etc.
    /// </summary>
    /// <param name="prefix">Key prefix to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in cache for current tenant.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if key exists and is not expired.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cache for current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
