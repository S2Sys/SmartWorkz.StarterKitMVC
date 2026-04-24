namespace SmartWorkz.Shared.Security.RateLimit;

/// <summary>
/// Defines the contract for rate limiting service implementations.
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Determines whether a request from the specified client is allowed based on rate limiting rules.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client making the request.</param>
    /// <param name="maxRequests">The maximum number of requests allowed within the window.</param>
    /// <param name="windowSeconds">The size of the time window in seconds.</param>
    /// <returns>True if the request is allowed; false if the rate limit has been exceeded.</returns>
    Task<bool> IsRequestAllowedAsync(string clientId, int maxRequests, int windowSeconds);

    /// <summary>
    /// Gets the current rate limiting status for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client.</param>
    /// <param name="maxRequests">The maximum number of requests allowed within the window.</param>
    /// <returns>A RateLimitStatus object containing the current status information.</returns>
    Task<RateLimitStatus> GetStatusAsync(string clientId, int maxRequests);

    /// <summary>
    /// Resets the rate limit for a specific client, clearing all accumulated requests.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client.</param>
    /// <returns>True if the reset was successful; false otherwise.</returns>
    Task<bool> ResetAsync(string clientId);
}
