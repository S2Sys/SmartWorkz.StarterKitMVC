namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for a cache store with support for various operations including TTL and expiration strategies.
/// </summary>
public interface ICacheStore
{
    /// <summary>
    /// Retrieves a value from the cache.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the cached value or null if not found or expired.</returns>
    Task<Result<T?>> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a value in the cache with optional TTL.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ttlMinutes">Optional time-to-live in minutes. If null, uses default or no expiration.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result<bool>> SetAsync<T>(string key, T value, int? ttlMinutes = null, CancellationToken ct = default);

    /// <summary>
    /// Sets a value in the cache with cache options.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">Cache options including TTL, strategy, and sliding expiration.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result<bool>> SetAsync<T>(string key, T value, CacheOptions options, CancellationToken ct = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result<bool>> RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Removes all values from the cache that match the specified key prefix.
    /// </summary>
    /// <param name="keyPrefix">The prefix to match.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the number of entries removed.</returns>
    Task<Result<int>> RemoveByPrefixAsync(string keyPrefix, CancellationToken ct = default);

    /// <summary>
    /// Checks if a key exists in the cache and is not expired.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result indicating whether the key exists and is valid.</returns>
    Task<Result<bool>> ExistsAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result<bool>> ClearAsync(CancellationToken ct = default);
}
