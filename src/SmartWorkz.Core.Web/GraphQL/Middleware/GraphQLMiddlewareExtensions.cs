using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace SmartWorkz.Core.Web.GraphQL.Middleware;

/// <summary>
/// Extension methods for GraphQL middleware integration.
/// Provides methods to apply rate limiting and authentication to GraphQL endpoints.
/// </summary>
public static class GraphQLMiddlewareExtensions
{
    /// <summary>
    /// Adds GraphQL middleware to the application pipeline.
    /// Applies rate limiting before GraphQL execution.
    /// </summary>
    public static IApplicationBuilder UseGraphQLMiddleware(this IApplicationBuilder app)
    {
        app.Use(GraphQLMiddlewareDelegate);
        return app;
    }

    /// <summary>
    /// The actual middleware delegate that handles GraphQL requests.
    /// Enforces rate limiting on /graphql endpoint.
    /// </summary>
    private static async Task GraphQLMiddlewareDelegate(HttpContext context, Func<Task> next)
    {
        // Only apply middleware to GraphQL endpoint
        if (context.Request.Path.StartsWithSegments("/graphql"))
        {
            try
            {
                // Get rate limit store from service provider
                var rateLimitStore = context.RequestServices.GetRequiredService<GraphQLRateLimitStore>();

                // Extract API key from request
                var apiKey = GraphQLRateLimitMiddleware.ExtractApiKey(context);
                if (string.IsNullOrEmpty(apiKey))
                {
                    apiKey = "anonymous";
                }

                // Check rate limit
                if (!rateLimitStore.IsAllowed(apiKey, out int remaining, out DateTime resetTime))
                {
                    // Rate limit exceeded
                    context.Response.StatusCode = 429; // Too Many Requests
                    GraphQLRateLimitMiddleware.AddRateLimitHeaders(context.Response, 1000, 0, resetTime);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Rate limit exceeded" }));
                    return;
                }

                // Request is allowed, add rate limit headers
                GraphQLRateLimitMiddleware.AddRateLimitHeaders(context.Response, 1000, remaining, resetTime);
            }
            catch (Exception ex)
            {
                // If there's an error with rate limiting, log it but don't block the request
                // This ensures rate limiting failures don't break GraphQL
                System.Diagnostics.Debug.WriteLine($"Rate limiting error: {ex.Message}");
            }
        }

        await next();
    }
}
