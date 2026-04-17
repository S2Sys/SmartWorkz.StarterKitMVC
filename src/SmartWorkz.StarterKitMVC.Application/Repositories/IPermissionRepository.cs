namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for permissions (Auth.Permission table)
/// </summary>
public interface IPermissionRepository : IDapperRepository<PermissionDto>
{
    /// <summary>Get permission by name</summary>
    Task<PermissionDto?> GetByNameAsync(string name, string tenantId);

    /// <summary>Get all permissions for a tenant</summary>
    Task<IEnumerable<PermissionDto>> GetAllForTenantAsync(string tenantId);

    /// <summary>Get permissions for a role</summary>
    Task<IEnumerable<PermissionDto>> GetByRoleAsync(Guid roleId, string tenantId);

    /// <summary>Get permissions for a user (direct + via roles)</summary>
    Task<IEnumerable<PermissionDto>> GetByUserAsync(Guid userId, string tenantId);

    /// <summary>Check if user has a specific permission</summary>
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionName, string tenantId);

    /// <summary>Check if role has a specific permission</summary>
    Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName, string tenantId);

    /// <summary>Assign a permission to a role</summary>
    Task AssignToRoleAsync(object roleId, object permissionId);

    /// <summary>Remove a permission from a role</summary>
    Task RemoveRolePermissionAsync(object roleId, object permissionId);
}

/// <summary>DTO for Permission entity</summary>
public class PermissionDto
{
    public Guid PermissionId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Category { get; set; } // e.g., "Users", "Reports", "Settings"
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
