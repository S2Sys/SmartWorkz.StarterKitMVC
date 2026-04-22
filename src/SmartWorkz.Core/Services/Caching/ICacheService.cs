namespace SmartWorkz.Core;

/// <summary>
/// Service for caching application data with support for expiration and key-based operations.
/// </summary>
/// <remarks>
/// Purpose: Provides distributed caching capabilities for frequently accessed data,
/// improving application performance by reducing database queries and computation.
///
/// Error Handling: Uses Task-based methods with no exceptions for expected cache operations
/// (miss, expiration, etc.). Failed operations are communicated through return values
/// (null for Get, false for Exists). Unexpected failures (e.g., Redis connection loss)
/// may throw exceptions.
///
/// Async Behavior: All methods are Task-based and support CancellationToken. Callers must
/// await these operations; they are not fire-and-forget.
///
/// Cache Key Conventions: Use hierarchical, dot-separated keys (e.g., "user.profile.123",
/// "product.details.456"). This convention facilitates prefix-based operations and makes
/// keys more discoverable in debugging.
///
/// TTL/Expiration: Set expiration times for cache entries to prevent serving stale data.
/// null expiration means no automatic expiration (entry persists until manually cleared
/// or evicted by cache policy).
///
/// Typical Usage: Injected into services that need to cache expensive operations.
/// Common patterns: Check cache first, if miss load from source, cache the result.
///
/// Concurrency: Thread-safe for distributed cache backends (Redis, Memcached). Local
/// in-memory caches may have different guarantees.
///
/// Performance: Cache hit is very fast (microseconds to milliseconds depending on backend).
/// Cache miss has network latency. Prefer longer TTLs for rarely-changing data.
/// </remarks>
public interface ICacheService : IService
{
    /// <summary>
    /// Retrieves a cached value by key with generic type support.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key. Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// The cached value of type T if the key exists and is not expired.
    /// null if the key does not exist, has expired, or cannot be deserialized to type T.
    /// </returns>
    /// <remarks>
    /// Cache Miss: Deserialization errors, type mismatches, or expired entries return null.
    /// Caller must check for null and treat as cache miss.
    ///
    /// Serialization: Values must be serializable. Typically uses JSON serialization for
    /// distributed caches.
    /// </remarks>
    /// <example>
    /// <code>
    /// var cachedUser = await cacheService.GetAsync&lt;UserDto&gt;("user.profile.123");
    /// if (cachedUser != null)
    /// {
    ///     logger.LogInformation($"Cache hit for user {cachedUser.Id}");
    ///     return cachedUser;
    /// }
    ///
    /// // Cache miss - load from database
    /// var user = await userService.GetByIdAsync(123);
    /// if (user.IsSuccess)
    /// {
    ///     await cacheService.SetAsync("user.profile.123", user.Value, TimeSpan.FromMinutes(30));
    /// }
    /// return user;
    /// </code>
    /// </example>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a value in the cache with optional expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key. Must not be null or empty.</param>
    /// <param name="value">The value to cache. Can be null.</param>
    /// <param name="expiration">Optional time-to-live. null means no automatic expiration.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A completed task when the value has been stored.</returns>
    /// <remarks>
    /// Overwrite: If the key already exists, its value is replaced.
    ///
    /// Expiration: The expiration TimeSpan is measured from the time of Set, not from
    /// any previous expiration. Null expiration means the entry persists indefinitely
    /// (subject to cache eviction policies).
    ///
    /// Null Values: Null values can be cached (useful for negative caching to avoid
    /// repeated queries for non-existent entities).
    ///
    /// Typical TTLs:
    /// - Configuration data: 1 hour or longer
    /// - User profile: 15-30 minutes
    /// - Database query results: 5-15 minutes
    /// - Real-time data: 30 seconds to 1 minute
    /// </remarks>
    /// <example>
    /// <code>
    /// var userDto = await userService.GetByIdAsync(123);
    /// if (userDto.IsSuccess)
    /// {
    ///     // Cache for 30 minutes
    ///     await cacheService.SetAsync("user.profile.123", userDto.Value, TimeSpan.FromMinutes(30));
    /// }
    ///
    /// // Cache with no expiration
    /// await cacheService.SetAsync("config.app.settings", appSettings);
    ///
    /// // Negative caching (cache the absence of a value)
    /// await cacheService.SetAsync&lt;UserDto&gt;("user.profile.999", null, TimeSpan.FromMinutes(5));
    /// </code>
    /// </example>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a single cache entry by key.
    /// </summary>
    /// <param name="key">The cache key to remove. Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A completed task when the key has been removed (or confirmed as not existing).</returns>
    /// <remarks>
    /// Idempotent: Removing a non-existent key does not cause an error.
    /// Useful for cache invalidation when a resource is updated or deleted.
    /// </remarks>
    /// <example>
    /// <code>
    /// // After updating a user, invalidate the cache
    /// var updateResult = await userService.UpdateAsync(userId, updatedDto);
    /// if (updateResult.IsSuccess)
    /// {
    ///     await cacheService.RemoveAsync($"user.profile.{userId}");
    /// }
    /// </code>
    /// </example>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries matching a key prefix.
    /// </summary>
    /// <param name="keyPrefix">The key prefix to match (e.g., "user.profile."). Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A completed task when all matching keys have been removed.</returns>
    /// <remarks>
    /// Bulk Invalidation: Useful for invalidating related cache entries when a resource
    /// is deleted or significantly changed. For example, removing all entries starting
    /// with "user.profile." when a user account is deleted.
    ///
    /// Performance: Prefix-based removal may be slower than single-key removal on some
    /// cache backends. Use judiciously.
    ///
    /// Prefix Format: Use hierarchical keys with consistent separators to make
    /// prefix matching efficient.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Clear all user profile caches
    /// await cacheService.RemoveByPrefixAsync("user.profile.");
    ///
    /// // Clear all product detail caches
    /// await cacheService.RemoveByPrefixAsync("product.details.");
    ///
    /// // Clear all caches for a specific user
    /// await cacheService.RemoveByPrefixAsync($"user.{userId}.");
    /// </code>
    /// </example>
    Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A completed task when all cache entries have been removed.</returns>
    /// <remarks>
    /// Destructive Operation: Removes ALL cached data. Use with caution in production.
    /// Useful for cache invalidation during application startup, after data imports,
    /// or for testing.
    ///
    /// Performance Impact: Application performance may degrade immediately after clearing
    /// the cache as frequently accessed data must be reloaded.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Clear cache on application startup
    /// if (environment.IsDevelopment())
    /// {
    ///     await cacheService.ClearAsync();
    ///     logger.LogInformation("Cache cleared on startup");
    /// }
    /// </code>
    /// </example>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a cache entry exists and has not expired.
    /// </summary>
    /// <param name="key">The cache key to check. Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// true if the key exists in the cache and has not expired.
    /// false if the key does not exist or has expired.
    /// </returns>
    /// <remarks>
    /// Use Case: Useful for checking if a value is cached without retrieving it,
    /// or for cache warming/preloading scenarios.
    ///
    /// Performance: Typically faster than GetAsync since no deserialization is needed.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (await cacheService.ExistsAsync("user.profile.123"))
    /// {
    ///     logger.LogInformation("User profile is cached");
    /// }
    /// else
    /// {
    ///     logger.LogInformation("User profile needs to be loaded from database");
    /// }
    /// </code>
    /// </example>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
