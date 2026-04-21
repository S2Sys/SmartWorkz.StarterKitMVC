namespace SmartWorkz.Mobile;

public interface ITokenRefreshInterceptor : IRequestInterceptor
{
    /// <summary>
    /// Handles response processing, specifically checking for 401 Unauthorized responses
    /// and attempting to refresh the token if needed.
    /// </summary>
    /// <param name="response">The HTTP response message to process.</param>
    /// <returns>True if the interceptor handled the response and the request should be retried; false otherwise.</returns>
    Task<bool> OnResponseAsync(HttpResponseMessage response);
}
