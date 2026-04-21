namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for HTTP client operations with support for async/await and cancellation.
/// Implementations should handle retries, timeouts, and error responses gracefully.
/// </summary>
public interface IHttpClient
{
    /// <summary>
    /// Sends a GET request and returns a typed response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Result containing the HTTP response with typed data.</returns>
    Task<Result<HttpResponse<T>>> GetAsync<T>(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request with JSON body and returns a typed response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="body">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Result containing the HTTP response with typed data.</returns>
    Task<Result<HttpResponse<T>>> PostAsync<T>(string url, object? body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request with JSON body and returns a typed response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="body">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Result containing the HTTP response with typed data.</returns>
    Task<Result<HttpResponse<T>>> PutAsync<T>(string url, object? body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request and returns a typed response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="url">The request URL.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Result containing the HTTP response with typed data.</returns>
    Task<Result<HttpResponse<T>>> DeleteAsync<T>(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a GET request and returns a string response.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Result containing the HTTP response with string data.</returns>
    Task<Result<HttpResponse<string>>> GetAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request with JSON body and returns a string response.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="body">The request body to serialize as JSON.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Result containing the HTTP response with string data.</returns>
    Task<Result<HttpResponse<string>>> PostAsync(string url, object? body, CancellationToken cancellationToken = default);
}
