using System.Security.Claims;
using SmartWorkz.StarterKitMVC.Application.Authorization;

namespace SmartWorkz.StarterKitMVC.Public.Middleware;

/// <summary>
/// Middleware that validates permissions based on claims.
/// Adds permission claims to the user's identity based on their roles.
/// </summary>
public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionMiddleware> _logger;

    public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IPermissionService permissionService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Get user's roles
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            if (roles.Count > 0)
            {
                // Get all permission keys for the user's roles
                var permissionKeys = await permissionService.GetPermissionKeysForRolesAsync(roles);

                // Add permission claims to the identity
                if (permissionKeys.Count > 0 && context.User.Identity is ClaimsIdentity identity)
                {
                    foreach (var permissionKey in permissionKeys)
                    {
                        if (!identity.HasClaim("permission", permissionKey))
                        {
                            identity.AddClaim(new Claim("permission", permissionKey));
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for permission middleware
/// </summary>
public static class PermissionMiddlewareExtensions
{
    public static IApplicationBuilder UsePermissions(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PermissionMiddleware>();
    }
}
