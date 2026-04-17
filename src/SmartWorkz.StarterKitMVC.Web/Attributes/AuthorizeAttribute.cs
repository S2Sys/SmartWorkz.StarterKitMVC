using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartWorkz.StarterKitMVC.Application.Authorization;

namespace SmartWorkz.StarterKitMVC.Web.Attributes;

/// <summary>
/// Custom authorization attribute for role and permission-based authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CustomAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[]? _roles;
    private readonly string[]? _permissions;

    public CustomAuthorizeAttribute()
    {
        _roles = null;
        _permissions = null;
    }

    public CustomAuthorizeAttribute(string roles)
    {
        _roles = roles.Split(",").Select(r => r.Trim()).ToArray();
        _permissions = null;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Check if user is authenticated
        if (!user?.Identity?.IsAuthenticated == true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check roles if specified
        if (_roles != null && _roles.Length > 0)
        {
            var hasRole = _roles.Any(role => user.IsInRole(role));

            if (!hasRole)
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        // Check permissions if specified
        if (_permissions != null && _permissions.Length > 0)
        {
            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();

            if (permissionService != null)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                foreach (var permission in _permissions)
                {
                    var hasPermission = await permissionService.HasPermissionAsync(userId, permission);

                    if (!hasPermission)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
            }
        }

        // Validate tenant isolation
        var userTenantId = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("TenantId");
        var requestTenantId = context.HttpContext.Request.Headers["X-Tenant-Id"].ToString();

        if (!string.IsNullOrWhiteSpace(userTenantId) &&
            !string.IsNullOrWhiteSpace(requestTenantId) &&
            userTenantId != requestTenantId)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
