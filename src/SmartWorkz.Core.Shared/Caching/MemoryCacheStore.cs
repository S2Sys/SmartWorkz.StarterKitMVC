namespace SmartWorkz.Core.Shared.Caching;

/// <summary>
/// In-memory implementation of ICacheStore with TTL support and thread-safe operations.
/// </summary>
public class MemoryCacheStore : ICacheStore
{
    private readonly Dictionary<string, CacheEntryWrapper> _cache = new();
    private readonly object _lockObject = new();
    private readonly CacheOptions _defaultOptions;

    /// <summary>
    /// Creates a new instance of MemoryCacheStore with default options.
    /// </summary>
    public MemoryCacheStore()
    {
        _defaultOptions = new CacheOptions();
    }

    /// <summary>
    /// Creates a new instance of MemoryCacheStore with specified default options.
    /// </summary>
    public MemoryCacheStore(CacheOptions defaultOptions)
    {
        _defaultOptions = defaultOptions ?? new CacheOptions();
    }

    /// <summary>
    /// Retrieves a value from the cache.
    /// </summary>
    public Task<Result<T?>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        Guard.NotEmpty(key, nameof(key));

        lock (_lockObject)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                // Check if expired and remove if necessary
                if (entry.IsExpired)
                {
                    _cache.Remove(key);
                    return Task.FromResult(Result.Ok<T?>(default));
                }

                // Renew expiry if sliding expiration is enabled
                if (entry.SlidingExpiration && entry.CacheStrategy == CacheStrategy.Sliding)
                {
                    entry.RenewExpiry();
                }

                var value = entry.Data is T typedValue ? typedValue : default;
                return Task.FromResult(Result.Ok(value));
            }

            return Task.FromResult(Result.Ok<T?>(default));
        }
    }

    /// <summary>
    /// Sets a value in the cache with optional TTL.
    /// </summary>
    public Task<Result<bool>> SetAsync<T>(string key, T value, int? ttlMinutes = null, CancellationToken ct = default)
    {
        var options = new CacheOptions(ttlMinutes ?? _defaultOptions.TtlMinutes);
        return SetAsync(key, value, options, ct);
    }

    /// <summary>
    /// Sets a value in the cache with cache options.
    /// </summary>
    public Task<Result<bool>> SetAsync<T>(string key, T value, CacheOptions options, CancellationToken ct = default)
    {
        Guard.NotEmpty(key, nameof(key));
        Guard.NotNull(options, nameof(options));

        lock (_lockObject)
        {
            var ttlMinutes = options.TtlMinutes ?? _defaultOptions.TtlMinutes;
            DateTime? expiryTime = null;

            if (ttlMinutes.HasValue)
            {
                expiryTime = DateTime.UtcNow.AddMinutes(ttlMinutes.Value);
            }

            var entry = new CacheEntryWrapper(
                value,
                expiryTime,
                ttlMinutes,
                options.CacheStrategy,
                options.SlidingExpiration
            );

            _cache[key] = entry;
            return Task.FromResult(Result.Ok(true));
        }
    }

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    public Task<Result<bool>> RemoveAsync(string key, CancellationToken ct = default)
    {
        Guard.NotEmpty(key, nameof(key));

        lock (_lockObject)
        {
            var removed = _cache.Remove(key);
            return Task.FromResult(Result.Ok(removed));
        }
    }

    /// <summary>
    /// Removes all values from the cache that match the specified key prefix.
    /// </summary>
    public Task<Result<int>> RemoveByPrefixAsync(string keyPrefix, CancellationToken ct = default)
    {
        Guard.NotEmpty(keyPrefix, nameof(keyPrefix));

        lock (_lockObject)
        {
            var keysToRemove = _cache.Keys
                .Where(k => k.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            return Task.FromResult(Result.Ok(keysToRemove.Count));
        }
    }

    /// <summary>
    /// Checks if a key exists in the cache and is not expired.
    /// </summary>
    public Task<Result<bool>> ExistsAsync(string key, CancellationToken ct = default)
    {
        Guard.NotEmpty(key, nameof(key));

        lock (_lockObject)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                // Remove if expired
                if (entry.IsExpired)
                {
                    _cache.Remove(key);
                    return Task.FromResult(Result.Ok(false));
                }

                return Task.FromResult(Result.Ok(true));
            }

            return Task.FromResult(Result.Ok(false));
        }
    }

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    public Task<Result<bool>> ClearAsync(CancellationToken ct = default)
    {
        lock (_lockObject)
        {
            _cache.Clear();
            return Task.FromResult(Result.Ok(true));
        }
    }

    /// <summary>
    /// Gets the current count of items in the cache (including expired items not yet removed).
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lockObject)
            {
                return _cache.Count;
            }
        }
    }

    /// <summary>
    /// Performs cleanup of expired entries. This is useful for periodic maintenance.
    /// </summary>
    public int CleanupExpiredEntries()
    {
        lock (_lockObject)
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }

            return expiredKeys.Count;
        }
    }
}
