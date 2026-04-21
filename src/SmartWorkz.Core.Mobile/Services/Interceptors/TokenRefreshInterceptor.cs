namespace SmartWorkz.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;
using System.Net;
using System.Net.Http.Headers;

/// <summary>
/// Interceptor that automatically refreshes JWT tokens when 401 Unauthorized responses are encountered.
/// Prevents multiple concurrent refresh attempts using a SemaphoreSlim for thread-safe mutual exclusion.
/// </summary>
public sealed class TokenRefreshInterceptor : ITokenRefreshInterceptor
{
    private readonly IAuthenticationHandler _authHandler;
    private readonly ILogger<TokenRefreshInterceptor> _logger;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    public TokenRefreshInterceptor(IAuthenticationHandler authHandler, ILogger<TokenRefreshInterceptor> logger)
    {
        _authHandler = Guard.NotNull(authHandler, nameof(authHandler));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Called before the request is sent. Currently a no-op for token refresh.
    /// </summary>
    public Task InterceptAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called after a response is received. Detects 401 status codes and attempts token refresh.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the response was a 401 and token refresh succeeded (request should retry); false otherwise.</returns>
    public async Task<bool> OnResponseAsync(HttpResponseMessage response, CancellationToken ct = default)
    {
        // If not 401, pass through - no action needed
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return false;
        }

        // Try to acquire the semaphore without waiting (non-blocking)
        if (!await _refreshSemaphore.WaitAsync(0, ct))
        {
            _logger.LogDebug("Token refresh already in progress, skipping duplicate attempt");
            return false;  // Another thread is already refreshing
        }

        try
        {
            var refreshResult = await _authHandler.RefreshTokenAsync(ct);
            if (!refreshResult.Succeeded)
            {
                _logger.LogError("Token refresh failed: {Error}", refreshResult.Error?.Message);
                return false;
            }

            var newToken = await _authHandler.GetTokenAsync(ct);
            if (newToken == null || string.IsNullOrWhiteSpace(newToken))
            {
                _logger.LogError("Token refresh succeeded but returned null or empty token");
                return false;
            }

            if (response.RequestMessage != null)
            {
                response.RequestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", newToken);
            }
            else
            {
                _logger.LogError("Cannot set new token: response.RequestMessage is null");
                return false;
            }

            _logger.LogDebug("Token refresh successful, retrying original request");
            return true;
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }
}
