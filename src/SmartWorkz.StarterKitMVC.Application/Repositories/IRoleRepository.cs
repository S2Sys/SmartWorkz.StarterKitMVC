using SmartWorkz.StarterKitMVC.Shared.DTOs;
namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for roles (Auth.Role table)
/// </summary>
public interface IRoleRepository : IDapperRepository<Shared.DTOs.Shared.DTOs.RoleDto>
{
    /// <summary>Get role by ID with tenant context</summary>
    Task<Shared.DTOs.RoleDto?> GetByIdAsync(object id, string tenantId);

    /// <summary>Get role by name</summary>
    Task<Shared.DTOs.RoleDto?> GetByNameAsync(string name, string tenantId);

    /// <summary>Get all roles for a tenant with pagination</summary>
    Task<(IEnumerable<Shared.DTOs.RoleDto> Items, int Total)> GetPagedAsync(string tenantId, int pageNumber, int pageSize);

    /// <summary>Get permissions assigned to a role</summary>
    Task<IEnumerable<PermissionDto>> GetPermissionsAsync(Guid roleId, string tenantId);

    /// <summary>Assign permissions to a role</summary>
    Task AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds, string tenantId);

    /// <summary>Remove all permissions from a role</summary>
    Task RemoveAllPermissionsAsync(Guid roleId);
}

/// <summary>DTO for Role entity</summary>
public class Shared.DTOs.RoleDto
{
    public Guid RoleId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
