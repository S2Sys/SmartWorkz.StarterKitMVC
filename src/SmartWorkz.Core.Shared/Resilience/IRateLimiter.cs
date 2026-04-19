namespace SmartWorkz.Core.Shared.Resilience;

/// <summary>
/// Defines the contract for a thread-safe rate limiter.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Attempts to acquire tokens for a request identified by the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier for rate limiting (e.g., IP address, user ID).</param>
    /// <param name="cost">The number of tokens to acquire (default: 1).</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    /// A result containing true if tokens were acquired successfully; otherwise false.
    /// On failure, the Error will include retry-after information.
    /// </returns>
    Task<Result<bool>> TryAcquireAsync(
        string identifier,
        int cost = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of available tokens for a given identifier.
    /// </summary>
    /// <param name="identifier">The identifier to check.</param>
    /// <returns>The number of available tokens.</returns>
    Task<int> GetAvailableTokensAsync(string identifier);

    /// <summary>
    /// Resets the rate limit state for a given identifier.
    /// </summary>
    /// <param name="identifier">The identifier to reset.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all rate limit state.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
