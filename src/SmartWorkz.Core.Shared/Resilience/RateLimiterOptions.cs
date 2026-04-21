namespace SmartWorkz.Shared;

/// <summary>
/// Configuration options for the rate limiter.
/// </summary>
public sealed class RateLimiterOptions
{
    /// <summary>
    /// Gets or sets the maximum number of requests allowed within the specified time window.
    /// Default: 100 requests per window.
    /// </summary>
    public int MaxRequests { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window duration in milliseconds.
    /// Default: 60000 milliseconds (1 minute).
    /// </summary>
    public int WindowMilliseconds { get; set; } = 60000;

    /// <summary>
    /// Gets or sets the rate limiting strategy.
    /// Default: TokenBucket.
    /// </summary>
    public RateLimiterStrategy Strategy { get; set; } = RateLimiterStrategy.TokenBucket;

    /// <summary>
    /// Gets or sets an optional identifier for this rate limiter instance.
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// Gets the token refill rate (tokens per millisecond).
    /// Calculated as: MaxRequests / WindowMilliseconds.
    /// </summary>
    public double RefillRate => MaxRequests / (double)WindowMilliseconds;

    /// <summary>
    /// Validates the options for correctness.
    /// </summary>
    /// <returns>True if options are valid; otherwise false.</returns>
    public bool IsValid()
    {
        return MaxRequests > 0 && WindowMilliseconds > 0;
    }
}
