using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for roles (Auth.Role table)
/// </summary>
public interface IRoleRepository : IDapperRepository<RoleDto>
{
    /// <summary>Get role by name</summary>
    Task<RoleDto?> GetByNameAsync(string name, string tenantId);

    /// <summary>Get all roles for a tenant with pagination</summary>
    Task<(IEnumerable<RoleDto> Items, int Total)> GetPagedAsync(string tenantId, int pageNumber, int pageSize);

    /// <summary>Get permissions assigned to a role</summary>
    Task<IEnumerable<PermissionDto>> GetPermissionsAsync(Guid roleId, string tenantId);

    /// <summary>Assign permissions to a role</summary>
    Task AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds, string tenantId);

    /// <summary>Remove all permissions from a role</summary>
    Task RemoveAllPermissionsAsync(Guid roleId);
}
