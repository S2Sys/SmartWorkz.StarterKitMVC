namespace SmartWorkz.Core.Shared.Caching;

/// <summary>
/// Represents a cached entry with data, expiration time, and metadata.
/// </summary>
/// <typeparam name="T">The type of data being cached.</typeparam>
public class CacheEntry<T>
{
    /// <summary>
    /// The cached data value.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// The expiry time for this cache entry. Null means no expiration.
    /// </summary>
    public DateTime? ExpiryTime { get; set; }

    /// <summary>
    /// The time when this entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The cache strategy used for this entry.
    /// </summary>
    public CacheStrategy CacheStrategy { get; set; }

    /// <summary>
    /// The sliding expiration setting for this entry.
    /// </summary>
    public bool SlidingExpiration { get; set; }

    /// <summary>
    /// Creates a new CacheEntry instance.
    /// </summary>
    public CacheEntry()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new CacheEntry instance with data and expiration.
    /// </summary>
    public CacheEntry(T? data, DateTime? expiryTime = null, CacheStrategy strategy = CacheStrategy.Absolute, bool slidingExpiration = false)
    {
        Data = data;
        ExpiryTime = expiryTime;
        CreatedAt = DateTime.UtcNow;
        CacheStrategy = strategy;
        SlidingExpiration = slidingExpiration;
    }

    /// <summary>
    /// Determines if this cache entry has expired.
    /// </summary>
    public bool IsExpired
    {
        get
        {
            if (ExpiryTime == null)
                return false;

            return DateTime.UtcNow >= ExpiryTime;
        }
    }

    /// <summary>
    /// Renews the expiry time based on the cache strategy and TTL.
    /// </summary>
    public void RenewExpiry(int? ttlMinutes = null)
    {
        if (ttlMinutes.HasValue)
        {
            ExpiryTime = DateTime.UtcNow.AddMinutes(ttlMinutes.Value);
        }
    }
}

/// <summary>
/// Non-generic wrapper for CacheEntry to store in the cache dictionary.
/// </summary>
internal class CacheEntryWrapper
{
    public object? Data { get; set; }
    public DateTime? ExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public CacheStrategy CacheStrategy { get; set; }
    public bool SlidingExpiration { get; set; }
    public int? TtlMinutes { get; set; }

    public bool IsExpired => ExpiryTime.HasValue && DateTime.UtcNow >= ExpiryTime;

    public CacheEntryWrapper()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public CacheEntryWrapper(object? data, DateTime? expiryTime, int? ttlMinutes, CacheStrategy strategy, bool slidingExpiration)
    {
        Data = data;
        ExpiryTime = expiryTime;
        CreatedAt = DateTime.UtcNow;
        TtlMinutes = ttlMinutes;
        CacheStrategy = strategy;
        SlidingExpiration = slidingExpiration;
    }

    public void RenewExpiry()
    {
        if (TtlMinutes.HasValue)
        {
            ExpiryTime = DateTime.UtcNow.AddMinutes(TtlMinutes.Value);
        }
    }
}
