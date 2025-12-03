using SmartWorkz.StarterKitMVC.Shared.Primitives;

namespace SmartWorkz.StarterKitMVC.Web.Middleware;

/// <summary>
/// Middleware that extracts or generates a correlation ID for distributed tracing.
/// The correlation ID is read from the X-Correlation-ID header or generated if not present.
/// </summary>
/// <example>
/// <code>
/// // In Program.cs
/// app.UseMiddleware&lt;CorrelationIdMiddleware&gt;();
/// 
/// // Request with correlation ID
/// GET /api/users
/// X-Correlation-ID: abc-123
/// 
/// // Response includes the same correlation ID
/// X-Correlation-ID: abc-123
/// </code>
/// </example>
public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var values)
            ? values.ToString()
            : Guid.NewGuid().ToString();

        correlationContext.CorrelationId = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);
    }
}
