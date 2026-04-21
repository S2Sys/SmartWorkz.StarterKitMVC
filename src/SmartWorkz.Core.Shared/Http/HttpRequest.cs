namespace SmartWorkz.Shared;

/// <summary>
/// Represents an HTTP request with configuration for URL, method, headers, body, timeout, and retry policy.
/// </summary>
public sealed class HttpRequest
{
    /// <summary>
    /// Gets or sets the request URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method (GET, POST, PUT, DELETE).
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Gets or sets the request headers.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// Gets or sets the request body (for POST/PUT operations).
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// Gets or sets the request timeout duration (default 30 seconds).
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the retry policy (optional).
    /// </summary>
    public RetryPolicy? RetryPolicy { get; set; }
}
