namespace SmartWorkz.Mobile.Models;

/// <summary>
/// Configuration for retry policy with exponential backoff.
/// </summary>
/// <remarks>
/// Defines parameters for automatic retry of transient failures with exponential backoff strategy.
/// Default values:
/// - MaxRetries: 5
/// - InitialDelay: 100ms
/// - MaxDelay: 30s
/// - BackoffMultiplier: 2.0
/// - UseJitter: true (±20% random variation)
/// </remarks>
public sealed record RetryConfig(
    int MaxRetries = 5,
    TimeSpan? InitialDelay = null,
    TimeSpan? MaxDelay = null,
    double BackoffMultiplier = 2.0,
    bool UseJitter = true)
{
    /// <summary>
    /// Gets the initial retry delay, or default if not specified.
    /// </summary>
    /// <returns>Initial delay in milliseconds (default: 100ms).</returns>
    public TimeSpan GetInitialDelay() => InitialDelay ?? TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Gets the maximum retry delay, or default if not specified.
    /// </summary>
    /// <returns>Maximum delay in milliseconds (default: 30s).</returns>
    public TimeSpan GetMaxDelay() => MaxDelay ?? TimeSpan.FromSeconds(30);

    /// <summary>
    /// Validates that the configuration is valid for use.
    /// </summary>
    /// <remarks>
    /// Valid configuration requires:
    /// - MaxRetries must be >= 0 (0 means no retries, just one attempt)
    /// - BackoffMultiplier must be >= 1.0
    /// </remarks>
    public bool IsValid => MaxRetries >= 0 && BackoffMultiplier >= 1.0;
}
