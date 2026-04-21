namespace SmartWorkz.Shared;

/// <summary>
/// Represents an HTTP response with status code, typed data, error information, and response headers.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public sealed class HttpResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request succeeded (2xx status code).
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code (200, 404, 500, etc.).
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the deserialized response data. Only valid when IsSuccess is true.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the error message if the request failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the response headers.
    /// </summary>
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();
}
