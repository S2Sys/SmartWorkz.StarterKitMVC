namespace SmartWorkz.Core.Shared.Http;

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

/// <summary>
/// HTTP client helper for making async HTTP requests with support for retry logic, timeouts, and JSON serialization.
/// Provides fluent builder pattern for composing requests.
/// </summary>
public sealed class HttpClientHelper : IHttpClient
{
    private static readonly HttpClient _httpClient = new();
    private readonly HttpRequest _request = new();

    // --- Factory methods ---

    /// <summary>
    /// Creates a new HTTP GET request builder.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <returns>A configured HttpClientHelper instance.</returns>
    public static HttpClientHelper Get(string url)
        => new() { _request = { Url = url, Method = HttpMethod.Get } };

    /// <summary>
    /// Creates a new HTTP POST request builder.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <returns>A configured HttpClientHelper instance.</returns>
    public static HttpClientHelper Post(string url)
        => new() { _request = { Url = url, Method = HttpMethod.Post } };

    /// <summary>
    /// Creates a new HTTP PUT request builder.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <returns>A configured HttpClientHelper instance.</returns>
    public static HttpClientHelper Put(string url)
        => new() { _request = { Url = url, Method = HttpMethod.Put } };

    /// <summary>
    /// Creates a new HTTP DELETE request builder.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <returns>A configured HttpClientHelper instance.</returns>
    public static HttpClientHelper Delete(string url)
        => new() { _request = { Url = url, Method = HttpMethod.Delete } };

    // --- Fluent builder methods ---

    /// <summary>
    /// Sets the request body.
    /// </summary>
    /// <param name="body">The body object to serialize as JSON.</param>
    /// <returns>This instance for method chaining.</returns>
    public HttpClientHelper WithBody(object? body)
    {
        _request.Body = body;
        return this;
    }

    /// <summary>
    /// Adds a single header to the request.
    /// </summary>
    /// <param name="key">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>This instance for method chaining.</returns>
    public HttpClientHelper WithHeader(string key, string value)
    {
        _request.Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Sets all request headers, replacing existing headers.
    /// </summary>
    /// <param name="headers">Dictionary of headers to set.</param>
    /// <returns>This instance for method chaining.</returns>
    public HttpClientHelper WithHeaders(Dictionary<string, string> headers)
    {
        _request.Headers = headers;
        return this;
    }

    /// <summary>
    /// Sets the request timeout.
    /// </summary>
    /// <param name="milliseconds">The timeout duration in milliseconds.</param>
    /// <returns>This instance for method chaining.</returns>
    public HttpClientHelper WithTimeout(int milliseconds)
    {
        _request.Timeout = TimeSpan.FromMilliseconds(milliseconds);
        return this;
    }

    /// <summary>
    /// Configures automatic retry logic for the request.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts.</param>
    /// <param name="backoffMs">Initial backoff interval in milliseconds.</param>
    /// <param name="strategy">The backoff strategy to use.</param>
    /// <returns>This instance for method chaining.</returns>
    public HttpClientHelper WithRetry(int maxAttempts, int backoffMs, RetryStrategy strategy = RetryStrategy.Exponential)
    {
        _request.RetryPolicy = new RetryPolicy
        {
            MaxAttempts = maxAttempts,
            BackoffMilliseconds = backoffMs,
            Strategy = strategy
        };
        return this;
    }

    // --- Execution methods ---

    /// <summary>
    /// Executes the HTTP request and returns a string response.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Result containing the HTTP response with string data.</returns>
    public async Task<Result<HttpResponse<string>>> ExecuteAsync(CancellationToken cancellationToken = default)
        => await ExecuteAsync<string>(cancellationToken, isStringResponse: true);

    /// <summary>
    /// Executes the HTTP request and returns a typed response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Result containing the HTTP response with typed data.</returns>
    public async Task<Result<HttpResponse<T>>> ExecuteAsync<T>(CancellationToken cancellationToken = default)
        => await ExecuteAsync<T>(cancellationToken, isStringResponse: false);

    // --- IHttpClient interface implementations ---

    /// <summary>
    /// Sends a GET request and returns a typed response.
    /// </summary>
    public async Task<Result<HttpResponse<T>>> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        => await Get(url).ExecuteAsync<T>(cancellationToken);

    /// <summary>
    /// Sends a POST request with JSON body and returns a typed response.
    /// </summary>
    public async Task<Result<HttpResponse<T>>> PostAsync<T>(string url, object? body, CancellationToken cancellationToken = default)
        => await Post(url).WithBody(body).ExecuteAsync<T>(cancellationToken);

    /// <summary>
    /// Sends a PUT request with JSON body and returns a typed response.
    /// </summary>
    public async Task<Result<HttpResponse<T>>> PutAsync<T>(string url, object? body, CancellationToken cancellationToken = default)
        => await Put(url).WithBody(body).ExecuteAsync<T>(cancellationToken);

    /// <summary>
    /// Sends a DELETE request and returns a typed response.
    /// </summary>
    public async Task<Result<HttpResponse<T>>> DeleteAsync<T>(string url, CancellationToken cancellationToken = default)
        => await Delete(url).ExecuteAsync<T>(cancellationToken);

    /// <summary>
    /// Sends a GET request and returns a string response.
    /// </summary>
    public async Task<Result<HttpResponse<string>>> GetAsync(string url, CancellationToken cancellationToken = default)
        => await Get(url).ExecuteAsync(cancellationToken);

    /// <summary>
    /// Sends a POST request with JSON body and returns a string response.
    /// </summary>
    public async Task<Result<HttpResponse<string>>> PostAsync(string url, object? body, CancellationToken cancellationToken = default)
        => await Post(url).WithBody(body).ExecuteAsync(cancellationToken);

    // --- Private execution logic ---

    private async Task<Result<HttpResponse<T>>> ExecuteAsync<T>(CancellationToken cancellationToken = default, bool isStringResponse = false)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(_request.Url))
            return Result.Fail<HttpResponse<T>>("HTTP.INVALID_URL", "Request URL cannot be empty.");

        if (_request.Timeout <= TimeSpan.Zero)
            return Result.Fail<HttpResponse<T>>("HTTP.INVALID_TIMEOUT", "Request timeout must be greater than zero.");

        int maxAttempts = _request.RetryPolicy?.MaxAttempts ?? 1;
        int backoffMs = _request.RetryPolicy?.BackoffMilliseconds ?? 1000;
        RetryStrategy strategy = _request.RetryPolicy?.Strategy ?? RetryStrategy.Exponential;
        List<HttpStatusCode> retryableStatusCodes = _request.RetryPolicy?.RetryableStatusCodes ?? new()
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };

        HttpResponse<T>? response = null;
        Exception? lastException = null;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                // Create request message
                using var requestMessage = new HttpRequestMessage(_request.Method, _request.Url);

                // Add headers
                foreach (var header in _request.Headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }

                // Add body if present
                if (_request.Body != null)
                {
                    string jsonBody = JsonSerializer.Serialize(_request.Body);
                    requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                // Set timeout
                _httpClient.Timeout = _request.Timeout;

                // Send request
                using var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);

                // Read response body
                string responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

                // Extract response headers
                var responseHeaders = new Dictionary<string, string>();
                foreach (var header in responseMessage.Headers)
                {
                    responseHeaders[header.Key] = string.Join(", ", header.Value);
                }

                // Check status code
                if (responseMessage.IsSuccessStatusCode)
                {
                    // Parse response
                    object? data = null;
                    if (!isStringResponse && typeof(T) != typeof(string))
                    {
                        data = string.IsNullOrEmpty(responseBody)
                            ? null
                            : JsonSerializer.Deserialize<T>(responseBody);
                    }
                    else if (isStringResponse || typeof(T) == typeof(string))
                    {
                        data = responseBody;
                    }

                    response = new HttpResponse<T>
                    {
                        IsSuccess = true,
                        StatusCode = (int)responseMessage.StatusCode,
                        Data = (T?)data,
                        ResponseHeaders = responseHeaders
                    };
                    return Result.Ok(response);
                }

                // Check if status is retryable
                if (retryableStatusCodes.Contains(responseMessage.StatusCode) && attempt < maxAttempts - 1)
                {
                    response = new HttpResponse<T>
                    {
                        IsSuccess = false,
                        StatusCode = (int)responseMessage.StatusCode,
                        Error = responseBody,
                        ResponseHeaders = responseHeaders
                    };

                    // Calculate backoff and wait
                    int waitMs = CalculateBackoff(attempt, backoffMs, strategy);
                    await Task.Delay(waitMs, cancellationToken);
                    continue;
                }

                // Non-retryable error or last attempt
                response = new HttpResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = (int)responseMessage.StatusCode,
                    Error = responseBody,
                    ResponseHeaders = responseHeaders
                };
                return Result.Ok(response);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                // Request was cancelled by caller
                lastException = ex;
                return Result.Fail<HttpResponse<T>>("HTTP.CANCELLED", $"HTTP request was cancelled: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                // Timeout occurred
                lastException = ex;
                if (attempt < maxAttempts - 1)
                {
                    int waitMs = CalculateBackoff(attempt, backoffMs, strategy);
                    await Task.Delay(waitMs, cancellationToken);
                    continue;
                }

                return Result.Fail<HttpResponse<T>>("HTTP.TIMEOUT", $"HTTP request timed out after {_request.Timeout.TotalSeconds} seconds.");
            }
            catch (HttpRequestException ex)
            {
                // Network error
                lastException = ex;
                if (attempt < maxAttempts - 1)
                {
                    int waitMs = CalculateBackoff(attempt, backoffMs, strategy);
                    await Task.Delay(waitMs, cancellationToken);
                    continue;
                }

                return Result.Fail<HttpResponse<T>>("HTTP.REQUEST_FAILED", $"HTTP request failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Unexpected error
                lastException = ex;
                return Result.Fail<HttpResponse<T>>("HTTP.UNEXPECTED_ERROR", $"Unexpected error during HTTP request: {ex.Message}");
            }
        }

        // Should not reach here, but handle just in case
        return Result.Fail<HttpResponse<T>>("HTTP.MAX_RETRIES_EXCEEDED", $"HTTP request failed after {maxAttempts} attempts.");
    }

    /// <summary>
    /// Calculates the backoff delay in milliseconds based on the strategy and attempt number.
    /// </summary>
    private static int CalculateBackoff(int attemptIndex, int baseMs, RetryStrategy strategy)
    {
        return strategy switch
        {
            RetryStrategy.Linear => baseMs,
            RetryStrategy.Exponential => baseMs * (int)Math.Pow(2, attemptIndex),
            RetryStrategy.Fibonacci => baseMs * GetFibonacciNumber(attemptIndex),
            _ => baseMs
        };
    }

    /// <summary>
    /// Gets the nth Fibonacci number (0-indexed).
    /// </summary>
    private static int GetFibonacciNumber(int index)
    {
        if (index <= 0) return 1;
        if (index == 1) return 1;

        int prev = 1, curr = 1;
        for (int i = 2; i <= index; i++)
        {
            int next = prev + curr;
            prev = curr;
            curr = next;
        }

        return curr;
    }
}
