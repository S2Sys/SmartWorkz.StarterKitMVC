namespace SmartWorkz.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;
using System.Net;
using System.Net.Http.Headers;

/// <summary>
/// Interceptor that automatically refreshes JWT tokens when 401 Unauthorized responses are encountered.
/// Prevents multiple concurrent refresh attempts by tracking refresh state.
/// </summary>
public sealed class TokenRefreshInterceptor : ITokenRefreshInterceptor
{
    private readonly IAuthenticationHandler _authHandler;
    private readonly ILogger<TokenRefreshInterceptor> _logger;
    private bool _refreshAttempted;

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
    /// <returns>True if the response was a 401 and token refresh succeeded (request should retry); false otherwise.</returns>
    public async Task<bool> OnResponseAsync(HttpResponseMessage response)
    {
        // If not 401, pass through - no action needed
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return false;
        }

        // Guard: prevent multiple refresh attempts in flight
        if (_refreshAttempted)
        {
            _logger.LogDebug("Token refresh already attempted, skipping duplicate");
            return false;
        }

        _refreshAttempted = true;
        try
        {
            var refreshResult = await _authHandler.RefreshTokenAsync(default);
            if (!refreshResult.Succeeded)
            {
                _logger.LogError("Token refresh failed: {Error}", refreshResult.Error?.Message);
                return false;
            }

            // Get the new token and update the original request
            var newToken = await _authHandler.GetTokenAsync(default);
            if (!string.IsNullOrWhiteSpace(newToken) && response.RequestMessage != null)
            {
                response.RequestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", newToken);
            }

            _logger.LogDebug("Token refresh successful, retrying original request");
            return true;  // Retry the request
        }
        finally
        {
            _refreshAttempted = false;
        }
    }
}
