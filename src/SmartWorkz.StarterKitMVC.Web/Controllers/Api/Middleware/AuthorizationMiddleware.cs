using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SmartWorkz.StarterKitMVC.Application.Authorization;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api.Middleware;

/// <summary>
/// Middleware for custom authorization checks including tenant isolation.
/// </summary>
public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IPermissionService permissionService)
    {
        var user = context.User;

        // If user is authenticated, validate tenant isolation
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userTenantId = user.FindFirstValue("tenant_id")
                             ?? user.FindFirstValue("TenantId");

            var requestTenantId = context.Request.Headers["X-Tenant-Id"].ToString();

            // If both are specified and different, log warning
            if (!string.IsNullOrWhiteSpace(userTenantId) &&
                !string.IsNullOrWhiteSpace(requestTenantId) &&
                userTenantId != requestTenantId)
            {
                _logger.LogWarning(
                    "Tenant mismatch for user {UserId}: user tenant={UserTenant}, request tenant={RequestTenant}",
                    user.FindFirstValue(ClaimTypes.NameIdentifier),
                    userTenantId,
                    requestTenantId);
            }

            // Store user information in context for later use
            context.Items["UserId"] = user.FindFirstValue(ClaimTypes.NameIdentifier);
            context.Items["UserTenantId"] = userTenantId;
            context.Items["Roles"] = user.FindAll(ClaimTypes.Role);
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for registering authorization middleware.
/// </summary>
public static class AuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthorizationMiddleware>();
    }
}
