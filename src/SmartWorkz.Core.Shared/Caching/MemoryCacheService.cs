using System.Collections.Concurrent;
using SmartWorkz.Core.Shared.Results;

namespace SmartWorkz.Shared;

/// <summary>
/// In-memory L1 cache service implementation with thread-safe operations and tenant isolation.
/// Suitable for single-process deployments with TTL and expiration support.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, (object? Value, DateTime? ExpiresAt)> _storage = new();

    /// <summary>
    /// Builds a tenant-scoped cache key.
    /// </summary>
    /// <param name="key">Original cache key.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <returns>Tenant-scoped key in format "{tenantId}:{key}".</returns>
    private static string BuildKey(string key, string? tenantId = null)
    {
        var tenant = tenantId ?? "default";
        return $"{tenant}:{key}";
    }

    /// <summary>
    /// Gets a cached value by key with tenant isolation. Returns failure if not found or expired.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing cached value, or failure if not found or expired.</returns>
    public async Task<Result<T?>> GetAsync<T>(string key, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(key, nameof(key));

        var fullKey = BuildKey(key, tenantId);

        if (_storage.TryGetValue(fullKey, out var entry))
        {
            // Check if expired
            if (entry.ExpiresAt.HasValue && DateTime.UtcNow >= entry.ExpiresAt.Value)
            {
                _storage.TryRemove(fullKey, out _);
                return await Task.FromResult(Result.Fail<T?>("Cache.KeyNotFound", "Cache key not found"));
            }

            return await Task.FromResult(Result.Ok<T?>((T?)entry.Value));
        }

        return await Task.FromResult(Result.Fail<T?>("Cache.KeyNotFound", "Cache key not found"));
    }

    /// <summary>
    /// Sets a cached value with optional TTL expiration and tenant isolation.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="ttlMinutes">Time to live in minutes. If null, no expiration.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    public async Task<Result> SetAsync<T>(string key, T value, int? ttlMinutes = null, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(key, nameof(key));

        var fullKey = BuildKey(key, tenantId);
        DateTime? expiresAt = ttlMinutes.HasValue ? DateTime.UtcNow.AddMinutes(ttlMinutes.Value) : null;

        _storage[fullKey] = (value, expiresAt);
        return await Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Removes a single cache entry with tenant isolation.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    public async Task<Result> RemoveAsync(string key, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(key, nameof(key));

        var fullKey = BuildKey(key, tenantId);
        _storage.TryRemove(fullKey, out _);
        return await Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Removes all cache entries matching a prefix pattern with tenant isolation.
    /// Example: RemoveByPrefixAsync("user:*", "tenant1") removes "tenant1:user:*" entries.
    /// </summary>
    /// <param name="prefix">Key prefix to match.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    public async Task<Result> RemoveByPrefixAsync(string prefix, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(prefix, nameof(prefix));

        var tenant = tenantId ?? "default";
        string searchPrefix = prefix.EndsWith("*")
            ? $"{tenant}:{prefix.TrimEnd('*')}"
            : $"{tenant}:{prefix}";

        var keysToRemove = _storage.Keys.Where(k => k.StartsWith(searchPrefix)).ToList();
        foreach (var key in keysToRemove)
        {
            _storage.TryRemove(key, out _);
        }

        return await Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Checks if a key exists in the cache with tenant isolation (ignores expiration check).
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if key exists and is not expired; false otherwise.</returns>
    public async Task<bool> ExistsAsync(string key, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(key, nameof(key));

        var fullKey = BuildKey(key, tenantId);

        if (!_storage.TryGetValue(fullKey, out var entry))
        {
            return await Task.FromResult(false);
        }

        // Check if expired
        if (entry.ExpiresAt.HasValue && DateTime.UtcNow >= entry.ExpiresAt.Value)
        {
            _storage.TryRemove(fullKey, out _);
            return await Task.FromResult(false);
        }

        return await Task.FromResult(true);
    }

    /// <summary>
    /// Clears all cache entries for the specified tenant (or "default" if not specified).
    /// Does not clear entries from other tenants.
    /// </summary>
    /// <param name="tenantId">Tenant identifier. Defaults to "default" if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    public async Task<Result> ClearAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var tenant = tenantId ?? "default";
        var prefixToMatch = $"{tenant}:";

        var keysToRemove = _storage.Keys.Where(k => k.StartsWith(prefixToMatch)).ToList();
        foreach (var key in keysToRemove)
        {
            _storage.TryRemove(key, out _);
        }

        return await Task.FromResult(Result.Ok());
    }
}
