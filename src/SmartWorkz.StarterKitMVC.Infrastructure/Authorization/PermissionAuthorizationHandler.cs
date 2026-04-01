using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that validates permission claims against required permissions.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>
    /// Handles the authorization request by checking if the user has the required permission.
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        if (permissions.Contains(requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
