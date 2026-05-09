namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SmartWorkz.Shared;

public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionChecker _connectionChecker;
    private readonly IAuthenticationHandler _authenticationHandler;
    private readonly IEnumerable<IRequestInterceptor> _requestInterceptors;
    private readonly IErrorHandler _errorHandler;
    private readonly MobileApiConfig _config;
    private readonly ILogger _logger;

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        IConnectionChecker connectionChecker,
        IAuthenticationHandler authenticationHandler,
        IEnumerable<IRequestInterceptor> requestInterceptors,
        IErrorHandler errorHandler,
        IOptions<MobileApiConfig> apiConfig,
        ILogger logger)
    {
        _httpClientFactory = Guard.NotNull(httpClientFactory, nameof(httpClientFactory));
        _connectionChecker = Guard.NotNull(connectionChecker, nameof(connectionChecker));
        _authenticationHandler = Guard.NotNull(authenticationHandler, nameof(authenticationHandler));
        _requestInterceptors = Guard.NotNull(requestInterceptors, nameof(requestInterceptors));
        _errorHandler = Guard.NotNull(errorHandler, nameof(errorHandler));
        Guard.NotNull(apiConfig, nameof(apiConfig));
        _config = apiConfig.Value;
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Gets data from an endpoint and deserializes to type T.
    /// </summary>
    public async Task<Result<T>> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        Guard.NotEmpty(endpoint, nameof(endpoint));
        return await ExecuteRequestAsync<T>(HttpMethod.Get, endpoint, null, ct);
    }

    /// <summary>
    /// Posts data to an endpoint and deserializes the response to type T.
    /// </summary>
    public async Task<Result<T>> PostAsync<T>(string endpoint, object data, CancellationToken ct = default)
    {
        Guard.NotEmpty(endpoint, nameof(endpoint));
        Guard.NotNull(data, nameof(data));
        return await ExecuteRequestAsync<T>(HttpMethod.Post, endpoint, data, ct);
    }

    /// <summary>
    /// Puts data to an endpoint and deserializes the response to type T.
    /// </summary>
    public async Task<Result<T>> PutAsync<T>(string endpoint, object data, CancellationToken ct = default)
    {
        Guard.NotEmpty(endpoint, nameof(endpoint));
        Guard.NotNull(data, nameof(data));
        return await ExecuteRequestAsync<T>(HttpMethod.Put, endpoint, data, ct);
    }

    /// <summary>
    /// Deletes a resource at the endpoint.
    /// </summary>
    public async Task<Result> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        Guard.NotEmpty(endpoint, nameof(endpoint));

        ct.ThrowIfCancellationRequested();

        try
        {
            var isOnline = await _connectionChecker.IsOnlineAsync();
            if (!isOnline)
            {
                return Result.Fail(new Error("MOBILE.OFFLINE", "No network connection"));
            }

            var client = _httpClientFactory.CreateClient("MobileApiClient");
            var url = new Uri(new Uri(_config.BaseUrl), endpoint).ToString();
            var request = new HttpRequestMessage(HttpMethod.Delete, url);

            await ApplyRequestInterceptorsAsync(request, ct);
            await _authenticationHandler.InjectHeadersAsync(request, ct);

            var response = await client.SendAsync(request, ct);

            // Handle 401 responses with token refresh interceptors
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var tokenRefreshInterceptor = _requestInterceptors
                    .OfType<ITokenRefreshInterceptor>()
                    .FirstOrDefault();

                if (tokenRefreshInterceptor != null)
                {
                    var shouldRetry = await tokenRefreshInterceptor.OnResponseAsync(response, ct);
                    if (shouldRetry)
                    {
                        // Recreate the request with the new token and retry once
                        var retryRequest = new HttpRequestMessage(HttpMethod.Delete, url);

                        // Copy original headers from the request
                        foreach (var header in request.Headers)
                        {
                            retryRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        // The token should already be in response.RequestMessage.Headers.Authorization
                        // Copy it to the retry request
                        if (response.RequestMessage?.Headers.Authorization != null)
                        {
                            retryRequest.Headers.Authorization = response.RequestMessage.Headers.Authorization;
                        }

                        response.Dispose();
                        response = await client.SendAsync(retryRequest, ct);
                    }
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                // Attempt to parse error response body
                try
                {
                    using var contentStream = await response.Content.ReadAsStreamAsync() as MemoryStream
                        ?? new MemoryStream(await response.Content.ReadAsByteArrayAsync(ct));

                    contentStream.Seek(0, SeekOrigin.Begin);
                    var errorEnvelope = await System.Text.Json.JsonSerializer.DeserializeAsync<ApiResponse>(
                        contentStream,
                        cancellationToken: ct
                    );

                    if (errorEnvelope?.Error != null)
                    {
                        var errorCode = errorEnvelope.Error.Code ?? "UNKNOWN_ERROR";
                        var errorMessage = errorEnvelope.Error.Message ?? "Unknown error";
                        return Result.Fail(new Error(errorCode, errorMessage));
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // If parsing fails, use generic HTTP error
                }

                return Result.Fail(new Error($"HTTP.{(int)response.StatusCode}", response.ReasonPhrase ?? "Unknown"));
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return _errorHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Gets a raw stream from an endpoint without deserialization.
    /// </summary>
    public async Task<Result<Stream>> GetStreamAsync(string endpoint, CancellationToken ct = default)
    {
        Guard.NotEmpty(endpoint, nameof(endpoint));

        ct.ThrowIfCancellationRequested();

        try
        {
            var isOnline = await _connectionChecker.IsOnlineAsync();
            if (!isOnline)
            {
                return Result.Fail<Stream>(new Error("MOBILE.OFFLINE", "No network connection"));
            }

            var client = _httpClientFactory.CreateClient("MobileApiClient");
            var url = new Uri(new Uri(_config.BaseUrl), endpoint).ToString();
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            await ApplyRequestInterceptorsAsync(request, ct);
            await _authenticationHandler.InjectHeadersAsync(request, ct);

            var response = await client.SendAsync(request, ct);

            // Handle 401 responses with token refresh interceptors
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var tokenRefreshInterceptor = _requestInterceptors
                    .OfType<ITokenRefreshInterceptor>()
                    .FirstOrDefault();

                if (tokenRefreshInterceptor != null)
                {
                    var shouldRetry = await tokenRefreshInterceptor.OnResponseAsync(response, ct);
                    if (shouldRetry)
                    {
                        // Recreate the request with the new token and retry once
                        var retryRequest = new HttpRequestMessage(HttpMethod.Get, url);

                        // Copy original headers from the request
                        foreach (var header in request.Headers)
                        {
                            retryRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        // The token should already be in response.RequestMessage.Headers.Authorization
                        // Copy it to the retry request
                        if (response.RequestMessage?.Headers.Authorization != null)
                        {
                            retryRequest.Headers.Authorization = response.RequestMessage.Headers.Authorization;
                        }

                        response.Dispose();
                        response = await client.SendAsync(retryRequest, ct);
                    }
                }
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    return Result.Fail<Stream>(new Error($"HTTP.{(int)response.StatusCode}", response.ReasonPhrase ?? "Unknown"));
                }

                // Copy the response stream to a MemoryStream so the HttpResponseMessage can be disposed
                var memoryStream = new MemoryStream();
                await response.Content.CopyToAsync(memoryStream, ct);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return Result.Ok<Stream>(memoryStream);
            }
        }
        catch (Exception ex)
        {
            return _errorHandler.HandleException<Stream>(ex);
        }
    }

    /// <summary>
    /// Gets data with retry, timeout, and optional cancellation.
    /// Delegates to error handler's centralized retry policy.
    /// </summary>
    public async Task<Result<T>> GetAsync<T>(string endpoint, int retryCount, TimeSpan timeout, CancellationToken ct = default)
    {
        Guard.NotEmpty(endpoint, nameof(endpoint));
        Guard.Requires(retryCount > 0, nameof(retryCount), "Retry count must be greater than 0");

        ct.ThrowIfCancellationRequested();

        // Create a linked CancellationTokenSource for the timeout
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        // Use errorHandler.HandleWithRetryAsync to execute the request with retries
        // The handler will apply exponential backoff: 100 * 2^attempt ms
        Result<T> finalResult = null!;
        var retryResult = await _errorHandler.HandleWithRetryAsync(
            async () =>
            {
                finalResult = await GetAsync<T>(endpoint, cts.Token);
                if (!finalResult.Succeeded)
                    throw new HttpRequestException(finalResult.Error?.Message ?? "Request failed");
            },
            retryCount,
            cts.Token
        );

        // Return the result from the retry handler, or use the final result if retry succeeded
        if (!retryResult.Succeeded)
        {
            return Result.Fail<T>(retryResult.Error ?? new Error("HTTP.RETRY_FAILED", "Request failed after retries"));
        }

        return finalResult ?? Result.Fail<T>(new Error("HTTP.RETRY_FAILED", "Request failed after retries"));
    }

    private async Task<Result<T>> ExecuteRequestAsync<T>(HttpMethod method, string endpoint, object? data, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var isOnline = await _connectionChecker.IsOnlineAsync();
            if (!isOnline)
            {
                return Result.Fail<T>(new Error("MOBILE.OFFLINE", "No network connection"));
            }

            var client = _httpClientFactory.CreateClient("MobileApiClient");
            var url = new Uri(new Uri(_config.BaseUrl), endpoint).ToString();
            var request = new HttpRequestMessage(method, url);

            if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                request.Content = JsonContent.Create(data);
            }

            await ApplyRequestInterceptorsAsync(request, ct);
            await _authenticationHandler.InjectHeadersAsync(request, ct);

            var response = await client.SendAsync(request, ct);

            // Handle 401 responses with token refresh interceptors
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var tokenRefreshInterceptor = _requestInterceptors
                    .OfType<ITokenRefreshInterceptor>()
                    .FirstOrDefault();

                if (tokenRefreshInterceptor != null)
                {
                    var shouldRetry = await tokenRefreshInterceptor.OnResponseAsync(response, ct);
                    if (shouldRetry)
                    {
                        // Recreate the request with the new token and retry once
                        var retryRequest = new HttpRequestMessage(method, url);

                        if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put))
                        {
                            retryRequest.Content = JsonContent.Create(data);
                        }

                        // Copy original headers from the request
                        foreach (var header in request.Headers)
                        {
                            retryRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        // The token should already be in response.RequestMessage.Headers.Authorization
                        // Copy it to the retry request
                        if (response.RequestMessage?.Headers.Authorization != null)
                        {
                            retryRequest.Headers.Authorization = response.RequestMessage.Headers.Authorization;
                        }

                        response.Dispose();
                        response = await client.SendAsync(retryRequest, ct);
                    }
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail<T>(new Error($"HTTP.{(int)response.StatusCode}", response.ReasonPhrase ?? "Unknown"));
            }

            using var contentStream = await response.Content.ReadAsStreamAsync() as MemoryStream
                ?? new MemoryStream(await response.Content.ReadAsByteArrayAsync(ct));

            // Phase 1: Try to deserialize as ApiResponse<T> envelope
            try
            {
                contentStream.Seek(0, SeekOrigin.Begin);
                var envelope = await System.Text.Json.JsonSerializer.DeserializeAsync<ApiResponse<T>>(
                    contentStream,
                    cancellationToken: ct
                );

                if (envelope?.Success == true && envelope.Data != null)
                {
                    return Result.Ok(envelope.Data);
                }
                else if (envelope?.Error != null)
                {
                    var errorCode = envelope.Error.Code ?? "UNKNOWN_ERROR";
                    var errorMessage = envelope.Error.Message ?? "Unknown error";
                    return Result.Fail<T>(new Error(errorCode, errorMessage));
                }
                else if (envelope?.Success == false)
                {
                    return Result.Fail<T>(new Error("UNKNOWN_ERROR", "API returned failure without error details"));
                }
            }
            catch (System.Text.Json.JsonException)
            {
                _logger.LogDebug("Phase 1 envelope deserialization failed, attempting Phase 2 fallback");
            }

            // Phase 2: Fallback to direct T deserialization for backward compatibility
            contentStream.Seek(0, SeekOrigin.Begin);
            var deserializedData = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(
                contentStream,
                cancellationToken: ct
            );

            return deserializedData != null
                ? Result.Ok(deserializedData)
                : Result.Fail<T>(new Error("DESERIALIZATION_ERROR", "Failed to deserialize response data"));
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            return _errorHandler.HandleException<T>(jsonEx);
        }
        catch (Exception ex)
        {
            return _errorHandler.HandleException<T>(ex);
        }
    }

    private async Task ApplyRequestInterceptorsAsync(HttpRequestMessage request, CancellationToken ct)
    {
        foreach (var interceptor in _requestInterceptors)
        {
            await interceptor.InterceptAsync(request, ct);
        }
    }
}


