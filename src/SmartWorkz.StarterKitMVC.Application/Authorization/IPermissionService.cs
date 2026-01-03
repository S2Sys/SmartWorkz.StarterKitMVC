using SmartWorkz.StarterKitMVC.Domain.Authorization;

namespace SmartWorkz.StarterKitMVC.Application.Authorization;

/// <summary>
/// Service for managing permissions and role-based access control
/// </summary>
public interface IPermissionService
{
    #region Features
    
    Task<List<Feature>> GetAllFeaturesAsync(CancellationToken ct = default);
    Task<Feature?> GetFeatureByIdAsync(Guid id, CancellationToken ct = default);
    Task<Feature?> GetFeatureByKeyAsync(string key, CancellationToken ct = default);
    Task<List<Feature>> GetFeatureTreeAsync(CancellationToken ct = default);
    Task<Feature> CreateFeatureAsync(Feature feature, CancellationToken ct = default);
    Task<Feature> UpdateFeatureAsync(Feature feature, CancellationToken ct = default);
    Task DeleteFeatureAsync(Guid id, CancellationToken ct = default);
    
    #endregion
    
    #region Permissions
    
    Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);
    Task<List<Permission>> GetPermissionsByEntityAsync(string entity, CancellationToken ct = default);
    Task<Permission?> GetPermissionByIdAsync(Guid id, CancellationToken ct = default);
    Task<Permission?> GetPermissionByKeyAsync(string key, CancellationToken ct = default);
    Task<Permission> CreatePermissionAsync(Permission permission, CancellationToken ct = default);
    Task<Permission> UpdatePermissionAsync(Permission permission, CancellationToken ct = default);
    Task DeletePermissionAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Generate standard CRUD permissions for an entity
    /// </summary>
    Task<List<Permission>> GenerateEntityPermissionsAsync(string entity, string displayName, CancellationToken ct = default);
    
    #endregion
    
    #region Role Permissions
    
    Task<List<RolePermission>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default);
    Task<List<string>> GetRolePermissionKeysAsync(string roleId, CancellationToken ct = default);
    Task SetRolePermissionsAsync(string roleId, List<Guid> permissionIds, CancellationToken ct = default);
    Task GrantPermissionAsync(string roleId, Guid permissionId, string? condition = null, CancellationToken ct = default);
    Task RevokePermissionAsync(string roleId, Guid permissionId, CancellationToken ct = default);
    
    /// <summary>
    /// Check if a role has a specific permission
    /// </summary>
    Task<bool> HasPermissionAsync(string roleId, string permissionKey, CancellationToken ct = default);
    
    /// <summary>
    /// Check if any of the roles has a specific permission
    /// </summary>
    Task<bool> HasPermissionAsync(IEnumerable<string> roleIds, string permissionKey, CancellationToken ct = default);
    
    /// <summary>
    /// Get all permission keys for a set of roles (for claims)
    /// </summary>
    Task<HashSet<string>> GetPermissionKeysForRolesAsync(IEnumerable<string> roleIds, CancellationToken ct = default);
    
    #endregion
}

/// <summary>
/// Permission check result
/// </summary>
public class PermissionCheckResult
{
    public bool IsGranted { get; set; }
    public string? DenialReason { get; set; }
    public Dictionary<string, object>? Conditions { get; set; }
    
    public static PermissionCheckResult Granted() => new() { IsGranted = true };
    public static PermissionCheckResult Denied(string reason) => new() { IsGranted = false, DenialReason = reason };
}
