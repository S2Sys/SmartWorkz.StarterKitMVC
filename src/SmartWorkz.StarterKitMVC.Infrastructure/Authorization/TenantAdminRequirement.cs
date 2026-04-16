using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for tenant-scoped admin access.
/// Checks if user is:
/// 1. Super admin (across all tenants), OR
/// 2. Admin for the current tenant
/// </summary>
public class TenantAdminRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handler for TenantAdminRequirement
/// </summary>
public class TenantAdminRequirementHandler : AuthorizationHandler<TenantAdminRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantAdminRequirementHandler> _logger;

    public TenantAdminRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantAdminRequirementHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAdminRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogWarning("HttpContext is null in TenantAdminRequirementHandler");
            return Task.CompletedTask;
        }

        var user = context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User is not authenticated");
            return Task.CompletedTask;
        }

        // Get current tenant from HttpContext (set by TenantMiddleware)
        var currentTenantId = httpContext.Items.TryGetValue("TenantId", out var tenantObj) && tenantObj is string tenantId
            ? tenantId
            : null;

        _logger.LogDebug("Checking TenantAdmin requirement for user {UserId}, CurrentTenant={TenantId}",
            user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown",
            currentTenantId ?? "unknown");

        // Check if user is super admin (globally)
        var isSuperAdmin = user.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Any(c => c.Value.Equals("super_admin", StringComparison.OrdinalIgnoreCase));

        if (isSuperAdmin)
        {
            _logger.LogDebug("User is super admin - granting access to all tenants");
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user is admin for the current tenant
        if (!string.IsNullOrEmpty(currentTenantId))
        {
            var isTenantAdmin = user.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Any(c =>
                    // Check for tenant-scoped admin role: "admin:{tenantId}"
                    c.Value.Equals($"admin:{currentTenantId}", StringComparison.OrdinalIgnoreCase) ||
                    // Or just "admin" role (tenant-scoped from permissions middleware)
                    (c.Value.Equals("admin", StringComparison.OrdinalIgnoreCase) &&
                     user.FindFirst("TenantId")?.Value == currentTenantId));

            if (isTenantAdmin)
            {
                _logger.LogDebug("User is admin for tenant {TenantId}", currentTenantId);
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        _logger.LogWarning("User does not have tenant admin role for tenant {TenantId}", currentTenantId ?? "unknown");
        return Task.CompletedTask;
    }
}
