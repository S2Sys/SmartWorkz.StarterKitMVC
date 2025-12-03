namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

/// <summary>
/// HTTP service abstraction for making API requests.
/// </summary>
/// <example>
/// <code>
/// // Inject IHttpService via DI
/// public class MyService
/// {
///     private readonly IHttpService _http;
///     
///     public MyService(IHttpService http) => _http = http;
///     
///     public async Task&lt;User?&gt; GetUserAsync(int id)
///     {
///         var response = await _http.GetAsync&lt;User&gt;($"/api/users/{id}");
///         return response.IsSuccess ? response.Data : null;
///     }
/// }
/// </code>
/// </example>
public interface IHttpService
{
    /// <summary>Sends a GET request.</summary>
    Task<HttpResult<T>> GetAsync<T>(string path, CancellationToken ct = default);
    
    /// <summary>Sends a POST request.</summary>
    Task<HttpResult<T>> PostAsync<T>(string path, object? body = null, CancellationToken ct = default);
    
    /// <summary>Sends a PUT request.</summary>
    Task<HttpResult<T>> PutAsync<T>(string path, object? body = null, CancellationToken ct = default);
    
    /// <summary>Sends a DELETE request.</summary>
    Task<HttpResult<T>> DeleteAsync<T>(string path, CancellationToken ct = default);
}

/// <summary>
/// Result of an HTTP operation.
/// </summary>
public record HttpResult<T>(bool IsSuccess, T? Data, string? Error, int StatusCode);
