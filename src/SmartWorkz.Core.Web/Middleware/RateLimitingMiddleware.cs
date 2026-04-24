namespace SmartWorkz.Core.Web.Middleware;

using SmartWorkz.Shared.Security.RateLimit;

/// <summary>
/// ASP.NET Core middleware that enforces rate limiting on incoming HTTP requests.
/// </summary>
public class RateLimitingMiddleware
{
    /// <summary>
    /// Default maximum requests per window.
    /// </summary>
    private const int DefaultMaxRequests = 100;

    /// <summary>
    /// Default time window in seconds.
    /// </summary>
    private const int DefaultWindowSeconds = 60;

    /// <summary>
    /// The next middleware in the pipeline.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// The rate limiting service used to check and enforce rate limits.
    /// </summary>
    private readonly IRateLimitService _rateLimitService;

    /// <summary>
    /// Maximum number of requests allowed within the time window.
    /// </summary>
    private readonly int _maxRequests;

    /// <summary>
    /// Time window in seconds for rate limiting.
    /// </summary>
    private readonly int _windowSeconds;

    /// <summary>
    /// Initializes a new instance of the RateLimitingMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="rateLimitService">The rate limiting service.</param>
    /// <param name="maxRequests">Maximum requests allowed (default: 100).</param>
    /// <param name="windowSeconds">Time window in seconds (default: 60).</param>
    public RateLimitingMiddleware(
        RequestDelegate next,
        IRateLimitService rateLimitService,
        int maxRequests = DefaultMaxRequests,
        int windowSeconds = DefaultWindowSeconds)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
        _maxRequests = maxRequests > 0 ? maxRequests : DefaultMaxRequests;
        _windowSeconds = windowSeconds > 0 ? windowSeconds : DefaultWindowSeconds;
    }

    /// <summary>
    /// Processes the HTTP request and enforces rate limiting.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract client identifier from request
        var clientId = GetClientId(context);

        // Check if request is allowed
        var isAllowed = await _rateLimitService.IsRequestAllowedAsync(clientId, _maxRequests, _windowSeconds);

        if (!isAllowed)
        {
            // Get current status for headers
            var status = await _rateLimitService.GetStatusAsync(clientId, _maxRequests);

            // Set rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = status.ResetTime.ToUniversalTime().ToString("o");

            // Return 429 Too Many Requests
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                message = "Too many requests. Rate limit exceeded.",
                retryAfter = status.ResetAfterSeconds
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        // Get current status and add headers
        var currentStatus = await _rateLimitService.GetStatusAsync(clientId, _maxRequests);
        context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = currentStatus.RemainingRequests.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = currentStatus.ResetTime.ToUniversalTime().ToString("o");

        // Call next middleware
        await _next(context);
    }

    /// <summary>
    /// Extracts the client identifier from the HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The client identifier.</returns>
    private static string GetClientId(HttpContext context)
    {
        // Try to get from user claims if authenticated
        if (context.User?.FindFirst("sub") is { Value: not null and not "" } claim)
            return claim.Value;

        // Try to get from authenticated user identity
        if (!string.IsNullOrEmpty(context.User?.Identity?.Name))
            return context.User.Identity.Name;

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return ipAddress;
    }
}
