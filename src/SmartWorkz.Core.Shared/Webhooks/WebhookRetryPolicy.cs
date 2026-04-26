namespace SmartWorkz.Core.Shared.Webhooks;

/// <summary>
/// Retry policy configuration for webhook delivery failures.
/// </summary>
public class WebhookRetryPolicy
{
    /// <summary>
    /// Maximum number of retry attempts (default: 5).
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Initial delay before first retry in milliseconds (default: 1000ms = 1s).
    /// </summary>
    public int InitialDelayMs { get; set; } = 1000;

    /// <summary>
    /// Multiplier for exponential backoff (default: 2.0x).
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Maximum delay between retries in milliseconds (default: 300000ms = 5min).
    /// </summary>
    public int MaxDelayMs { get; set; } = 300_000;

    /// <summary>
    /// Calculate the delay for the nth retry attempt.
    /// Uses exponential backoff: delay = initialDelay * (backoffMultiplier ^ attempt).
    /// </summary>
    public int GetRetryDelayMs(int attemptNumber)
    {
        var delayMs = (int)(InitialDelayMs * Math.Pow(BackoffMultiplier, attemptNumber - 1));
        return Math.Min(delayMs, MaxDelayMs);
    }
}
