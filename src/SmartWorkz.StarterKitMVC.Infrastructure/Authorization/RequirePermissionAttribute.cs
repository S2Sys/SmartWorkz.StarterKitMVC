using Microsoft.AspNetCore.Authorization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Authorization;

/// <summary>
/// Attribute to enforce permission-based authorization on controller actions.
/// Usage: [RequirePermission("PRODUCT_READ")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initializes a new instance with the required permission code.
    /// </summary>
    /// <param name="permissionCode">The permission code required for access.</param>
    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = $"Permission:{permissionCode}";
    }
}
