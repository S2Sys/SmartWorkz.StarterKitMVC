using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Tenant-aware authorization extension methods.
/// Checks roles and permissions within tenant scope, with super admin bypass.
/// </summary>
public static class TenantAuthorizationExtensions
{
    /// <summary>
    /// Checks if user is a super admin (has super_admin role globally, across all tenants)
    /// </summary>
    public static bool IsSuperAdmin(this ClaimsPrincipal user)
    {
        if (user?.Claims == null) return false;
        return user.FindAll(ClaimTypes.Role)
            .Any(c => c.Value.Equals("super_admin", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if user is admin for the current tenant
    /// Super admins automatically pass this check
    /// </summary>
    public static bool IsTenantAdmin(this ClaimsPrincipal user, string? currentTenantId = null)
    {
        if (user?.Claims == null) return false;

        // Super admin has access to all tenants
        if (user.IsSuperAdmin()) return true;

        // Check for tenant-specific admin role
        // Role format: "admin:{tenantId}" stored as claim
        var userTenantId = user.FindFirst("TenantId")?.Value ?? currentTenantId;
        if (string.IsNullOrEmpty(userTenantId)) return false;

        return user.FindAll(ClaimTypes.Role)
            .Any(c => c.Value.Equals($"admin:{userTenantId}", StringComparison.OrdinalIgnoreCase) ||
                      c.Value.Equals("admin", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if user has a specific role within their tenant scope
    /// Super admins automatically pass this check
    /// </summary>
    public static bool HasTenantRole(this ClaimsPrincipal user, string role, string? currentTenantId = null)
    {
        if (user?.Claims == null || string.IsNullOrEmpty(role)) return false;

        // Super admin has all roles across all tenants
        if (user.IsSuperAdmin()) return true;

        var userTenantId = user.FindFirst("TenantId")?.Value ?? currentTenantId;
        if (string.IsNullOrEmpty(userTenantId)) return false;

        var normalizedRole = role.ToLowerInvariant();

        // Check both tenant-scoped and global role formats
        return user.FindAll(ClaimTypes.Role)
            .Any(c => c.Value.Equals($"{normalizedRole}:{userTenantId}", StringComparison.OrdinalIgnoreCase) ||
                      c.Value.Equals(normalizedRole, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if user has a specific permission within their tenant scope
    /// Permission format: "permission:{tenantId}" stored as claim value
    /// Super admins automatically pass this check
    /// </summary>
    public static bool HasTenantPermission(this ClaimsPrincipal user, string permission, string? currentTenantId = null)
    {
        if (user?.Claims == null || string.IsNullOrEmpty(permission)) return false;

        // Super admin has all permissions across all tenants
        if (user.IsSuperAdmin()) return true;

        var userTenantId = user.FindFirst("TenantId")?.Value ?? currentTenantId;
        if (string.IsNullOrEmpty(userTenantId)) return false;

        var normalizedPermission = permission.ToLowerInvariant();

        // Permissions stored as: claim type = "permission", claim value = "{permission}:{tenantId}"
        return user.FindAll("permission")
            .Any(c => c.Value.Equals($"{normalizedPermission}:{userTenantId}", StringComparison.OrdinalIgnoreCase) ||
                      c.Value.Equals(normalizedPermission, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if user has ANY of the specified permissions within tenant scope
    /// </summary>
    public static bool HasAnyTenantPermission(this ClaimsPrincipal user, string? currentTenantId = null, params string[] permissions)
    {
        if (user?.Claims == null || permissions?.Length == 0) return false;

        return permissions.Any(p => user.HasTenantPermission(p, currentTenantId));
    }

    /// <summary>
    /// Checks if user has ALL of the specified permissions within tenant scope
    /// </summary>
    public static bool HasAllTenantPermissions(this ClaimsPrincipal user, string? currentTenantId = null, params string[] permissions)
    {
        if (user?.Claims == null || permissions?.Length == 0) return false;

        return permissions.All(p => user.HasTenantPermission(p, currentTenantId));
    }

    /// <summary>
    /// Gets the user's current tenant ID from claims
    /// </summary>
    public static string? GetTenantId(this ClaimsPrincipal user)
    {
        if (user?.Claims == null) return null;
        return user.FindFirst("TenantId")?.Value ?? user.FindFirst("tenant")?.Value;
    }

    /// <summary>
    /// Gets all tenant IDs the user has access to (multi-tenant support)
    /// For now returns current tenant, future enhancement: list all assigned tenants
    /// </summary>
    public static List<string> GetTenantIds(this ClaimsPrincipal user)
    {
        if (user?.Claims == null) return [];

        // Super admin has access to all tenants
        if (user.IsSuperAdmin()) return ["*"]; // "*" means all tenants

        var tenantId = user.GetTenantId();
        return string.IsNullOrEmpty(tenantId) ? [] : [tenantId];
    }

    /// <summary>
    /// Comprehensive tenant-aware access check
    /// </summary>
    public static bool HasTenantAccess(this ClaimsPrincipal user,
        string? requiredRole = null,
        string? requiredPermission = null,
        string? currentTenantId = null)
    {
        if (user == null || !user.Identity?.IsAuthenticated == true) return false;

        // Super admin has full access
        if (user.IsSuperAdmin()) return true;

        // Check role requirement
        if (!string.IsNullOrEmpty(requiredRole))
        {
            if (!user.HasTenantRole(requiredRole, currentTenantId))
                return false;
        }

        // Check permission requirement
        if (!string.IsNullOrEmpty(requiredPermission))
        {
            if (!user.HasTenantPermission(requiredPermission, currentTenantId))
                return false;
        }

        return true;
    }
}
