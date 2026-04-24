namespace SmartWorkz.Shared.Security.RateLimit;

/// <summary>
/// Represents the current rate limiting status for a client.
/// </summary>
public class RateLimitStatus
{
    /// <summary>
    /// Gets or sets the number of remaining requests allowed within the current time window.
    /// </summary>
    public int RemainingRequests { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of requests allowed within the time window.
    /// </summary>
    public int MaxRequests { get; set; }

    /// <summary>
    /// Gets or sets the number of seconds remaining in the current time window.
    /// </summary>
    public int ResetAfterSeconds { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the rate limit window will reset.
    /// </summary>
    public DateTime ResetTime { get; set; }
}
