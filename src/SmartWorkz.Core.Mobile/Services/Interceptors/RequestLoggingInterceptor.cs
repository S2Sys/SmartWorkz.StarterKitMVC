namespace SmartWorkz.Mobile;

using Microsoft.Extensions.Logging;
using System.Diagnostics;

/// <summary>
/// Interceptor that logs HTTP request and response details including method, URI, status code, and elapsed time.
/// Optional body logging is disabled by default and should only be enabled in Development environment.
/// </summary>
public sealed class RequestLoggingInterceptor : IRequestInterceptor
{
    private readonly ILogger<RequestLoggingInterceptor> _logger;
    private readonly bool _enableBodyLogging;
    private Stopwatch? _stopwatch;

    public RequestLoggingInterceptor(
        ILogger<RequestLoggingInterceptor> logger,
        bool enableBodyLogging = false)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
        _enableBodyLogging = enableBodyLogging;
    }

    /// <summary>
    /// Called before the request is sent. Logs request method and URI, starts timing.
    /// </summary>
    public async Task InterceptAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        _stopwatch = Stopwatch.StartNew();
        _logger.LogDebug(
            "HTTP {Method} {Uri}",
            request.Method.Method,
            request.RequestUri?.AbsoluteUri ?? "unknown");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Called after a response is received. Logs status code, elapsed time, and optional body.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>False (never retries, this is just logging).</returns>
    public async Task<bool> OnResponseAsync(HttpResponseMessage response, CancellationToken ct = default)
    {
        _stopwatch?.Stop();
        var elapsed = _stopwatch?.ElapsedMilliseconds ?? 0;

        var logLevel = response.IsSuccessStatusCode ? LogLevel.Debug : LogLevel.Error;
        var message = $"HTTP {(int)response.StatusCode} in {elapsed}ms";

        if (_enableBodyLogging && response.Content != null)
        {
            try
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                if (!string.IsNullOrWhiteSpace(body))
                {
                    if (body.Length > 5000)
                        body = body.Substring(0, 5000) + "... [truncated]";
                    message += $" - Body: {body}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read response body for logging");
            }
        }

        _logger.Log(logLevel, message);
        return false;  // Don't retry (this is just logging)
    }
}
