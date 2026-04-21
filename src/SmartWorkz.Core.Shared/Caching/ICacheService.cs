
namespace SmartWorkz.Shared;

/// <summary>
/// Service for caching with tenant isolation and L1/L2 hybrid support.
/// Implementations may use memory cache (L1) and distributed cache (L2).
/// All cache operations are tenant-scoped with automatic key prefixing.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key with tenant isolation.
    /// </summary>
    /// <typeparam name="T">Type of cached value.</typeparam>
    /// <param name="key">Cache key (will be tenant-scoped automatically).</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing cached value, or failure if not found or expired.</returns>
    Task<Result<T?>> GetAsync<T>(string key, string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with optional TTL for the specified tenant.
    /// </summary>
    /// <typeparam name="T">Type of value to cache.</typeparam>
    /// <param name="key">Cache key (will be tenant-scoped automatically).</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="ttlMinutes">Time to live in minutes. If null, value never expires.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    Task<Result> SetAsync<T>(string key, T value, int? ttlMinutes = null, string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached value by key for the specified tenant.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    Task<Result> RemoveAsync(string key, string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values matching a key prefix for the specified tenant.
    /// Example: RemoveByPrefixAsync("user:") removes all "user:*" entries for that tenant.
    /// </summary>
    /// <param name="prefix">Key prefix to match (may include wildcard suffix like "user:*").</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    Task<Result> RemoveByPrefixAsync(string prefix, string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in cache for the specified tenant.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if key exists and is not expired; false otherwise.</returns>
    Task<bool> ExistsAsync(string key, string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cache entries for the specified tenant.
    /// Does not affect entries for other tenants.
    /// </summary>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    Task<Result> ClearAsync(string? tenantId = null, CancellationToken cancellationToken = default);
}
