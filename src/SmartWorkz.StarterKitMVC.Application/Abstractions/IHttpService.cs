using SmartWorkz.StarterKitMVC.Infrastructure.Http;

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
///         var request = new ApiRequest
///         {
///             Method = HttpMethod.Get,
///             Path = $"/api/users/{id}"
///         };
///         
///         var response = await _http.SendAsync&lt;User&gt;(request);
///         return response.IsSuccess ? response.Data : null;
///     }
/// }
/// </code>
/// </example>
public interface IHttpService
{
    /// <summary>
    /// Sends an HTTP request and returns a typed response.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="request">The API request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>API response with data or error.</returns>
    Task<ApiResponse<T>> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken = default);
}
