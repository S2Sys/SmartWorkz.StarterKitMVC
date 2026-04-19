using System.Collections.Concurrent;

namespace SmartWorkz.Core.Shared.Caching;

/// <summary>
/// In-memory L1 cache service implementation with thread-safe operations.
/// Suitable for single-process deployments with TTL and expiration support.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, (object? Value, DateTime? ExpiresAt)> _storage = new();

    /// <summary>
    /// Gets a cached value by key. Returns null if not found or expired.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cached value or null if not found or expired.</returns>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(key, nameof(key));

        if (_storage.TryGetValue(key, out var entry))
        {
            // Check if expired
            if (entry.ExpiresAt.HasValue && DateTime.UtcNow >= entry.ExpiresAt.Value)
            {
                _storage.TryRemove(key, out _);
                return await Task.FromResult(default(T?));
            }

            return await Task.FromResult((T?)entry.Value);
        }

        return await Task.FromResult(default(T?));
    }

    /// <summary>
    /// Sets a cached value with optional TTL expiration.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="ttl">Time to live. If null, no expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(key, nameof(key));

        DateTime? expiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null;

        _storage[key] = (value, expiresAt);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes a single cache entry.
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(key, nameof(key));

        _storage.TryRemove(key, out _);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Removes all cache entries matching a prefix pattern.
    /// Example: RemoveByPrefixAsync("user:") removes "user:1", "user:2", etc.
    /// </summary>
    /// <param name="prefix">Key prefix to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(prefix, nameof(prefix));

        string searchPrefix = prefix.EndsWith("*")
            ? prefix.TrimEnd('*')
            : prefix;

        var keysToRemove = _storage.Keys.Where(k => k.StartsWith(searchPrefix)).ToList();
        foreach (var key in keysToRemove)
        {
            _storage.TryRemove(key, out _);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Checks if a key exists in the cache (ignores expiration check).
    /// </summary>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if key exists; false otherwise.</returns>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        Guard.NotEmpty(key, nameof(key));

        if (!_storage.TryGetValue(key, out var entry))
        {
            return await Task.FromResult(false);
        }

        // Check if expired
        if (entry.ExpiresAt.HasValue && DateTime.UtcNow >= entry.ExpiresAt.Value)
        {
            _storage.TryRemove(key, out _);
            return await Task.FromResult(false);
        }

        return await Task.FromResult(true);
    }

    /// <summary>
    /// Clears all cache entries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _storage.Clear();
        await Task.CompletedTask;
    }
}
