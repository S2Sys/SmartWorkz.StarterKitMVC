namespace SmartWorkz.Core.Shared.Metrics;

using System.Diagnostics;
using SmartWorkz.Core.Shared.Guards;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// ASP.NET Core middleware for automatic HTTP request/response metrics collection.
/// Records operation duration, status, and errors for all HTTP requests.
/// </summary>
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMetricsCollector _metricsCollector;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, IMetricsCollector metricsCollector, ILogger<MetricsMiddleware> logger)
    {
        _next = Guard.NotNull(next, nameof(next));
        _metricsCollector = Guard.NotNull(metricsCollector, nameof(metricsCollector));
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Guard.NotNull(context, nameof(context));

        var stopwatch = Stopwatch.StartNew();
        var operationName = $"{context.Request.Method} {context.Request.Path}";

        try
        {
            await _next(context);

            stopwatch.Stop();
            var status = context.Response.StatusCode >= 400 ? "error" : "success";
            _metricsCollector.RecordOperationDuration(operationName, stopwatch.ElapsedMilliseconds, status);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metricsCollector.RecordError(operationName, ex);
            throw;
        }
    }
}

/// <summary>
/// Extension methods for registering MetricsMiddleware in the application pipeline.
/// </summary>
public static class MetricsMiddlewareExtensions
{
    /// <summary>
    /// Adds MetricsMiddleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseApplicationMetrics(this IApplicationBuilder app)
    {
        Guard.NotNull(app, nameof(app));
        return app.UseMiddleware<MetricsMiddleware>();
    }
}
