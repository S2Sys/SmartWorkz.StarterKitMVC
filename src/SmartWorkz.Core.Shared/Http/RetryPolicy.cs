namespace SmartWorkz.Core.Shared.Http;

using System.Net;

/// <summary>
/// Specifies the backoff strategy to use when retrying failed HTTP requests.
/// </summary>
public enum RetryStrategy
{
    /// <summary>
    /// Wait X ms between each retry attempt (constant interval).
    /// </summary>
    Linear,

    /// <summary>
    /// Wait X, X*2, X*4... ms between retry attempts (exponential backoff).
    /// </summary>
    Exponential,

    /// <summary>
    /// Wait based on Fibonacci sequence: 1, 1, 2, 3, 5, 8... multiplied by X ms.
    /// </summary>
    Fibonacci
}

/// <summary>
/// Configures automatic retry behavior for failed HTTP requests.
/// </summary>
public sealed class RetryPolicy
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts (default 3).
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial backoff interval in milliseconds (default 1000).
    /// </summary>
    public int BackoffMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the retry strategy to use (default exponential).
    /// </summary>
    public RetryStrategy Strategy { get; set; } = RetryStrategy.Exponential;

    /// <summary>
    /// Gets or sets the list of HTTP status codes that trigger a retry.
    /// Defaults to: 408 (Timeout), 429 (Too Many Requests), 500 (Internal Server Error),
    /// 502 (Bad Gateway), 503 (Service Unavailable), 504 (Gateway Timeout).
    /// </summary>
    public List<HttpStatusCode> RetryableStatusCodes { get; set; } = new()
    {
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout
    };
}
