using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using SmartWorkz.StarterKitMVC.Application.Authorization;

namespace SmartWorkz.StarterKitMVC.Admin.Middleware;

/// <summary>
/// Middleware that validates permissions based on claims.
/// Adds permission claims to the user's identity based on their roles.
/// Caches permissions per user to avoid redundant DB calls.
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

    public async Task InvokeAsync(HttpContext context, IPermissionService permissionService, IMemoryCache cache)
    {
        try
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Get user ID and roles
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roles = context.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();

                if (!string.IsNullOrEmpty(userId) && roles.Count > 0)
                {
                    // Cache key: perms:{userId}:{roles-hash}
                    var rolesKey = string.Join(",", roles.Order());
                    var cacheKey = $"perms:{userId}:{rolesKey}";

                    try
                    {
                        // Try to get from cache; if not found, query and cache for 5 minutes
                        if (!cache.TryGetValue(cacheKey, out List<string>? cachedPermissions))
                        {
                            cachedPermissions = (await permissionService.GetPermissionKeysForRolesAsync(roles)).ToList();
                            cache.Set(cacheKey, cachedPermissions, TimeSpan.FromMinutes(5));
                        }

                        // Add permission claims to the identity
                        if (cachedPermissions?.Count > 0 && context.User.Identity is ClaimsIdentity identity)
                        {
                            foreach (var permissionKey in cachedPermissions)
                            {
                                if (!identity.HasClaim("permission", permissionKey))
                                {
                                    identity.AddClaim(new Claim("permission", permissionKey));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // If permission lookup fails, log but continue - permissions are optional for some pages
                        _logger.LogWarning(ex, "Failed to load permissions for user {UserId} with roles {Roles}",
                            userId, string.Join(",", roles));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log unhandled middleware exceptions but continue
            _logger.LogError(ex, "Unhandled error in PermissionMiddleware");
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
