namespace SmartWorkz.Core.Shared.Caching;

/// <summary>
/// Configuration options for cache operations.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Time-to-live in minutes. Default is 5 minutes.
    /// Null means no expiration.
    /// </summary>
    public int? TtlMinutes { get; set; } = 5;

    /// <summary>
    /// Strategy for cache expiration. Default is Absolute.
    /// </summary>
    public CacheStrategy CacheStrategy { get; set; } = CacheStrategy.Absolute;

    /// <summary>
    /// If true, each access extends the TTL (only relevant when CacheStrategy is Sliding).
    /// Default is false.
    /// </summary>
    public bool SlidingExpiration { get; set; } = false;

    /// <summary>
    /// Creates a new CacheOptions instance with default values.
    /// </summary>
    public CacheOptions()
    {
    }

    /// <summary>
    /// Creates a new CacheOptions instance with specified TTL.
    /// </summary>
    public CacheOptions(int? ttlMinutes)
    {
        TtlMinutes = ttlMinutes;
    }

    /// <summary>
    /// Creates a new CacheOptions instance with specified TTL and cache strategy.
    /// </summary>
    public CacheOptions(int? ttlMinutes, CacheStrategy cacheStrategy)
    {
        TtlMinutes = ttlMinutes;
        CacheStrategy = cacheStrategy;
    }

    /// <summary>
    /// Creates a new CacheOptions instance with all parameters.
    /// </summary>
    public CacheOptions(int? ttlMinutes, CacheStrategy cacheStrategy, bool slidingExpiration)
    {
        TtlMinutes = ttlMinutes;
        CacheStrategy = cacheStrategy;
        SlidingExpiration = slidingExpiration;
    }
}
