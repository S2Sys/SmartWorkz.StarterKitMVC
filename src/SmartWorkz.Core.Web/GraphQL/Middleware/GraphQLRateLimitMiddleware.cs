using System.Net;
using Microsoft.AspNetCore.Http;

namespace SmartWorkz.Core.Web.GraphQL.Middleware;

/// <summary>
/// GraphQL rate limiting middleware.
/// Enforces per-API-key rate limits on GraphQL requests.
/// </summary>
public static class GraphQLRateLimitMiddleware
{
    private const string ApiKeyHeader = "X-API-Key";
    private const string RateLimitHeaderPrefix = "X-RateLimit-";

    /// <summary>
    /// Extracts API key from request headers.
    /// First checks X-API-Key header, then falls back to Authorization header.
    /// </summary>
    public static string? ExtractApiKey(HttpContext context)
    {
        // Check X-API-Key header first
        if (context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyValues))
        {
            var apiKey = apiKeyValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKey))
            {
                return apiKey;
            }
        }

        // Fall back to Authorization header's Bearer token as API key
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader["Bearer ".Length..].Trim();
        }

        // If no API key found, use IP address as default identifier
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{remoteIp}";
    }

    /// <summary>
    /// Adds rate limit headers to response.
    /// </summary>
    public static void AddRateLimitHeaders(
        HttpResponse response,
        int limit,
        int remaining,
        DateTime resetTime)
    {
        response.Headers[$"{RateLimitHeaderPrefix}Limit"] = limit.ToString();
        response.Headers[$"{RateLimitHeaderPrefix}Remaining"] = remaining.ToString();
        response.Headers[$"{RateLimitHeaderPrefix}Reset"] = ((long)(resetTime - DateTime.UtcNow).TotalSeconds).ToString();
    }

    /// <summary>
    /// Checks if rate limit exceeded.
    /// Returns appropriate HTTP status code and headers.
    /// </summary>
    public static bool IsRateLimitExceeded(
        HttpContext context,
        bool rateLimitExceeded,
        int limit,
        int remaining,
        DateTime resetTime)
    {
        if (rateLimitExceeded)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            AddRateLimitHeaders(context.Response, limit, 0, resetTime);
            return true;
        }

        AddRateLimitHeaders(context.Response, limit, remaining, resetTime);
        return false;
    }
}
