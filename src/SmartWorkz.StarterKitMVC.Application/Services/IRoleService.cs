using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for managing roles and role-based permissions.
/// Handles role CRUD operations, permission assignment, and role caching.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Gets a role by ID.
    /// Results are cached for 1 hour.
    /// </summary>
    Task<RoleDto?> GetByIdAsync(string roleId, string tenantId);

    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<RoleDto?> GetByNameAsync(string roleName, string tenantId);

    /// <summary>
    /// Gets all roles for a tenant with caching.
    /// Cache key: roles_{tenantId}
    /// </summary>
    Task<IEnumerable<RoleDto>> GetAllAsync(string tenantId);

    /// <summary>
    /// Creates a new role.
    /// Invalidates role cache for the tenant.
    /// </summary>
    Task<RoleDto> CreateAsync(RoleDto role);

    /// <summary>
    /// Updates an existing role.
    /// Invalidates role cache for the tenant.
    /// </summary>
    Task<RoleDto> UpdateAsync(RoleDto role);

    /// <summary>
    /// Deletes a role by ID.
    /// Invalidates role cache for the tenant.
    /// </summary>
    Task<bool> DeleteAsync(string roleId);

    /// <summary>
    /// Assigns permissions to a role.
    /// Invalidates user permission cache for all users with this role.
    /// </summary>
    Task<bool> AssignPermissionsAsync(string roleId, IEnumerable<string> permissionIds, string tenantId);

    /// <summary>
    /// Gets all permissions assigned to a role.
    /// </summary>
    Task<IEnumerable<PermissionDto>> GetPermissionsAsync(string roleId, string tenantId);

    /// <summary>
    /// Removes a permission from a role.
    /// </summary>
    Task<bool> RemovePermissionAsync(string roleId, string permissionId, string tenantId);
}

