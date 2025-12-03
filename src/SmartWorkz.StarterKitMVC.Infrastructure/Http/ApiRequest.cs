namespace SmartWorkz.StarterKitMVC.Infrastructure.Http;

/// <summary>
/// Represents an HTTP API request.
/// </summary>
/// <example>
/// <code>
/// var request = new ApiRequest
/// {
///     Method = HttpMethod.Post,
///     Path = "/api/users",
///     Body = new { Name = "John", Email = "john@example.com" },
///     Headers = new Dictionary&lt;string, string&gt;
///     {
///         ["Authorization"] = "Bearer token123"
///     }
/// };
/// </code>
/// </example>
public sealed class ApiRequest
{
    /// <summary>HTTP method (GET, POST, PUT, DELETE, etc.).</summary>
    public HttpMethod Method { get; init; } = HttpMethod.Get;
    
    /// <summary>Request path (e.g., "/api/users/123").</summary>
    public string Path { get; init; } = string.Empty;
    
    /// <summary>Request body (will be serialized to JSON).</summary>
    public object? Body { get; init; }
    
    /// <summary>Additional HTTP headers.</summary>
    public IDictionary<string, string>? Headers { get; init; }
}
