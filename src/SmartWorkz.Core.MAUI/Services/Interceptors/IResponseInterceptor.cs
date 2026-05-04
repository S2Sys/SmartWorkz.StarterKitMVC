namespace SmartWorkz.Mobile;

/// <summary>
/// Extends IRequestInterceptor to provide response handling capabilities.
/// Implementations can process HTTP responses for logging, retry logic, token refresh, and other cross-cutting concerns.
/// </summary>
public interface IResponseInterceptor : IRequestInterceptor
{
    /// <summary>
    /// Called after a response is received. Processes response for logging, retry logic, token refresh, etc.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the interceptor handled the response and the request should be retried; false otherwise.</returns>
    Task<bool> OnResponseAsync(HttpResponseMessage response, CancellationToken ct = default);
}
