using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Authorization helper extensions for checking roles, claims, and permissions
/// Use these methods in pages, controllers, and services to validate user access
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Checks if the principal has a specific role
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal user, string role)
    {
        if (user?.Claims == null) return false;
        return user.FindAll(ClaimTypes.Role)
            .Any(c => c.Value.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the principal has any of the specified roles
    /// </summary>
    public static bool HasAnyRole(this ClaimsPrincipal user, params string[] roles)
    {
        if (user?.Claims == null || roles?.Length == 0) return false;
        var userRoles = user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value.ToLowerInvariant())
            .ToHashSet();

        return roles.Any(r => userRoles.Contains(r.ToLowerInvariant()));
    }

    /// <summary>
    /// Checks if the principal has all of the specified roles
    /// </summary>
    public static bool HasAllRoles(this ClaimsPrincipal user, params string[] roles)
    {
        if (user?.Claims == null || roles?.Length == 0) return false;
        var userRoles = user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value.ToLowerInvariant())
            .ToHashSet();

        return roles.All(r => userRoles.Contains(r.ToLowerInvariant()));
    }

    /// <summary>
    /// Checks if the principal has a specific claim (type only, any value)
    /// </summary>
    public static bool HasClaim(this ClaimsPrincipal user, string claimType)
    {
        if (user?.Claims == null || string.IsNullOrEmpty(claimType)) return false;
        return user.Claims.Any(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the principal has a specific claim with a specific value
    /// </summary>
    public static bool HasClaim(this ClaimsPrincipal user, string claimType, string claimValue)
    {
        if (user?.Claims == null || string.IsNullOrEmpty(claimType) || string.IsNullOrEmpty(claimValue)) return false;
        return user.Claims.Any(c =>
            c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
            c.Value.Equals(claimValue, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the principal has any of the specified claim values for a given claim type
    /// </summary>
    public static bool HasAnyClaimValue(this ClaimsPrincipal user, string claimType, params string[] claimValues)
    {
        if (user?.Claims == null || string.IsNullOrEmpty(claimType) || claimValues?.Length == 0) return false;
        var userClaimValues = user.Claims
            .Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value.ToLowerInvariant())
            .ToHashSet();

        return claimValues.Any(cv => userClaimValues.Contains(cv.ToLowerInvariant()));
    }

    /// <summary>
    /// Checks if the principal has all of the specified claim values for a given claim type
    /// </summary>
    public static bool HasAllClaimValues(this ClaimsPrincipal user, string claimType, params string[] claimValues)
    {
        if (user?.Claims == null || string.IsNullOrEmpty(claimType) || claimValues?.Length == 0) return false;
        var userClaimValues = user.Claims
            .Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value.ToLowerInvariant())
            .ToHashSet();

        return claimValues.All(cv => userClaimValues.Contains(cv.ToLowerInvariant()));
    }

    /// <summary>
    /// Gets all values of a specific claim type for the principal
    /// </summary>
    public static List<string> GetClaimValues(this ClaimsPrincipal user, string claimType)
    {
        if (user?.Claims == null || string.IsNullOrEmpty(claimType)) return [];
        return user.Claims
            .Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Checks if the principal has a specific permission
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        if (user?.Claims == null || string.IsNullOrEmpty(permission)) return false;
        return user.FindAll("permission")
            .Any(c => c.Value.Equals(permission, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the principal has any of the specified permissions
    /// </summary>
    public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissions)
    {
        if (user?.Claims == null || permissions?.Length == 0) return false;
        var userPermissions = user.FindAll("permission")
            .Select(c => c.Value.ToLowerInvariant())
            .ToHashSet();

        return permissions.Any(p => userPermissions.Contains(p.ToLowerInvariant()));
    }

    /// <summary>
    /// Checks if the principal has all of the specified permissions
    /// </summary>
    public static bool HasAllPermissions(this ClaimsPrincipal user, params string[] permissions)
    {
        if (user?.Claims == null || permissions?.Length == 0) return false;
        var userPermissions = user.FindAll("permission")
            .Select(c => c.Value.ToLowerInvariant())
            .ToHashSet();

        return permissions.All(p => userPermissions.Contains(p.ToLowerInvariant()));
    }

    /// <summary>
    /// Gets all permissions for the principal
    /// </summary>
    public static List<string> GetPermissions(this ClaimsPrincipal user)
    {
        if (user?.Claims == null) return [];
        return user.FindAll("permission")
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Gets all roles for the principal
    /// </summary>
    public static List<string> GetRoles(this ClaimsPrincipal user)
    {
        if (user?.Claims == null) return [];
        return user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    /// <summary>
    /// Comprehensive access check: validates role AND claim AND permission requirements
    /// Pass null for any requirement you want to skip
    /// </summary>
    /// <param name="user">The claims principal to check</param>
    /// <param name="requiredRole">Role the user must have (case-insensitive)</param>
    /// <param name="requiredClaim">Claim type the user must have</param>
    /// <param name="requiredClaimValue">Specific claim value the user must have (requires requiredClaim)</param>
    /// <param name="requiredPermission">Permission the user must have (case-insensitive)</param>
    /// <returns>True if all non-null requirements are satisfied</returns>
    public static bool HasAccess(this ClaimsPrincipal user,
        string requiredRole = null,
        string requiredClaim = null,
        string requiredClaimValue = null,
        string requiredPermission = null)
    {
        if (user == null) return false;

        // Check role requirement
        if (!string.IsNullOrEmpty(requiredRole))
        {
            if (!user.HasRole(requiredRole))
                return false;
        }

        // Check claim requirement (type only - any value accepted)
        if (!string.IsNullOrEmpty(requiredClaim) && string.IsNullOrEmpty(requiredClaimValue))
        {
            if (!user.HasClaim(requiredClaim))
                return false;
        }

        // Check claim requirement (type + specific value)
        if (!string.IsNullOrEmpty(requiredClaim) && !string.IsNullOrEmpty(requiredClaimValue))
        {
            if (!user.HasClaim(requiredClaim, requiredClaimValue))
                return false;
        }

        // Check permission requirement
        if (!string.IsNullOrEmpty(requiredPermission))
        {
            if (!user.HasPermission(requiredPermission))
                return false;
        }

        return true;
    }
}
