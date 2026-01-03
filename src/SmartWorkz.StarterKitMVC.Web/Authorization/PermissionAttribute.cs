using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmartWorkz.StarterKitMVC.Domain.Authorization;

namespace SmartWorkz.StarterKitMVC.Web.Authorization;

/// <summary>
/// Attribute to require specific permissions on controllers or actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string entity, PermissionAction action) 
        : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = new object[] { new PermissionRequirement(entity, action) };
    }
    
    public RequirePermissionAttribute(string permissionKey) 
        : base(typeof(PermissionKeyAuthorizationFilter))
    {
        Arguments = new object[] { permissionKey };
    }
}

/// <summary>
/// Authorization filter that checks for entity-action based permissions
/// </summary>
public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly PermissionRequirement _requirement;
    private readonly ILogger<PermissionAuthorizationFilter> _logger;

    public PermissionAuthorizationFilter(
        PermissionRequirement requirement,
        ILogger<PermissionAuthorizationFilter> logger)
    {
        _requirement = requirement;
        _logger = logger;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated == true)
        {
            context.Result = new ChallengeResult();
            return Task.CompletedTask;
        }

        var permissionKey = _requirement.GetPermissionKey();
        var hasPermission = user.HasClaim("permission", permissionKey);

        if (!hasPermission)
        {
            _logger.LogWarning(
                "User {User} denied access to {Entity}.{Action} - missing permission {Permission}",
                user.Identity?.Name, _requirement.Entity, _requirement.Action, permissionKey);
            
            context.Result = new ForbidResult();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization filter that checks for permission key directly
/// </summary>
public class PermissionKeyAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly string _permissionKey;
    private readonly ILogger<PermissionKeyAuthorizationFilter> _logger;

    public PermissionKeyAuthorizationFilter(
        string permissionKey,
        ILogger<PermissionKeyAuthorizationFilter> logger)
    {
        _permissionKey = permissionKey;
        _logger = logger;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated == true)
        {
            context.Result = new ChallengeResult();
            return Task.CompletedTask;
        }

        var hasPermission = user.HasClaim("permission", _permissionKey);

        if (!hasPermission)
        {
            _logger.LogWarning(
                "User {User} denied access - missing permission {Permission}",
                user.Identity?.Name, _permissionKey);
            
            context.Result = new ForbidResult();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Extension methods for checking permissions in code
/// </summary>
public static class PermissionExtensions
{
    /// <summary>
    /// Check if user has a specific permission
    /// </summary>
    public static bool HasPermission(this System.Security.Claims.ClaimsPrincipal user, string permissionKey)
    {
        return user.HasClaim("permission", permissionKey);
    }
    
    /// <summary>
    /// Check if user has permission for entity action
    /// </summary>
    public static bool HasPermission(this System.Security.Claims.ClaimsPrincipal user, string entity, PermissionAction action)
    {
        var key = $"{entity.ToLowerInvariant()}.{action.ToString().ToLowerInvariant()}";
        return user.HasClaim("permission", key);
    }
    
    /// <summary>
    /// Check if user can view entity
    /// </summary>
    public static bool CanView(this System.Security.Claims.ClaimsPrincipal user, string entity)
        => user.HasPermission(entity, PermissionAction.View);
    
    /// <summary>
    /// Check if user can create entity
    /// </summary>
    public static bool CanCreate(this System.Security.Claims.ClaimsPrincipal user, string entity)
        => user.HasPermission(entity, PermissionAction.Create);
    
    /// <summary>
    /// Check if user can edit entity
    /// </summary>
    public static bool CanEdit(this System.Security.Claims.ClaimsPrincipal user, string entity)
        => user.HasPermission(entity, PermissionAction.Edit);
    
    /// <summary>
    /// Check if user can delete entity
    /// </summary>
    public static bool CanDelete(this System.Security.Claims.ClaimsPrincipal user, string entity)
        => user.HasPermission(entity, PermissionAction.Delete);
}
