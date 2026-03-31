using Microsoft.AspNetCore.Authorization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for permission-based access control.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the required permission code.
    /// </summary>
    public string PermissionCode { get; }

    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}
