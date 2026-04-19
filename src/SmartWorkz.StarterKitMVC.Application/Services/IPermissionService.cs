using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for permission checking and authorization.
/// Evaluates user permissions based on roles and direct assignments.
/// Includes caching with 15-minute TTL for performance.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// Results are cached for 15 minutes.
    /// </summary>
    Task<bool> UserHasPermissionAsync(string userId, string permissionName, string tenantId);

    /// <summary>
    /// Checks if a user has any of the specified permissions.
    /// </summary>
    Task<bool> UserHasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, string tenantId);

    /// <summary>
    /// Checks if a user has all of the specified permissions.
    /// </summary>
    Task<bool> UserHasAllPermissionsAsync(string userId, IEnumerable<string> permissionNames, string tenantId);

    /// <summary>
    /// Gets all permissions for a user.
    /// Combines permissions from roles and direct assignments.
    /// Results are cached for 15 minutes.
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, string tenantId);

    /// <summary>
    /// Gets all permissions for a specific role.
    /// </summary>
    Task<IEnumerable<PermissionDto>> GetByRoleAsync(string roleId, string tenantId);

    /// <summary>
    /// Checks if a role has a specific permission.
    /// </summary>
    Task<bool> RoleHasPermissionAsync(string roleId, string permissionName, string tenantId);

    /// <summary>
    /// Gets all available permissions for a tenant.
    /// </summary>
    Task<IEnumerable<PermissionDto>> GetAllAsync(string tenantId);
}
