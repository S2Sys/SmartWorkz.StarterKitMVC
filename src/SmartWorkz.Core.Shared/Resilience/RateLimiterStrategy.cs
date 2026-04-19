namespace SmartWorkz.Core.Shared.Resilience;

/// <summary>
/// Specifies the strategy used by the rate limiter to control request flow.
/// </summary>
public enum RateLimiterStrategy
{
    /// <summary>
    /// Token bucket algorithm: tokens are refilled at a constant rate, requests consume tokens.
    /// Most suitable for burstable traffic patterns.
    /// </summary>
    TokenBucket = 0,

    /// <summary>
    /// Sliding window algorithm: tracks request timestamps within a time window.
    /// More precise but consumes more memory.
    /// </summary>
    SlidingWindow = 1
}
