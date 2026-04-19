namespace SmartWorkz.Core.Shared.Caching;

/// <summary>
/// Enumeration of cache expiration strategies.
/// </summary>
public enum CacheStrategy
{
    /// <summary>
    /// Absolute expiration: Entry expires at a fixed time after creation.
    /// </summary>
    Absolute = 0,

    /// <summary>
    /// Sliding expiration: Entry TTL is extended with each access.
    /// </summary>
    Sliding = 1
}
