using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api.Middleware;

/// <summary>
/// Middleware for extracting tenant information from headers or JWT claims.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = ExtractTenantId(context);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            context.Items["TenantId"] = tenantId;
            context.Request.Headers["X-Tenant-Id"] = tenantId;
            _logger.LogDebug("Tenant ID set in context: {TenantId}", tenantId);
        }
        else
        {
            _logger.LogDebug("No tenant ID found in headers or claims");
        }

        await _next(context);
    }

    private static string? ExtractTenantId(HttpContext context)
    {
        // Try to get from X-Tenant-Id header first
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantFromHeader))
        {
            if (!string.IsNullOrWhiteSpace(tenantFromHeader))
            {
                return tenantFromHeader.ToString();
            }
        }

        // Try to get from JWT claims if user is authenticated
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = user.FindFirstValue("tenant_id")
                           ?? user.FindFirstValue("TenantId");
            if (!string.IsNullOrWhiteSpace(tenantClaim))
            {
                return tenantClaim;
            }
        }

        return null;
    }
}

/// <summary>
/// Extension methods for registering tenant middleware.
/// </summary>
public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenant(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
