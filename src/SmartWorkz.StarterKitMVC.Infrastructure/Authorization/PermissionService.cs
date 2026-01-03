using System.Text.Json;
using SmartWorkz.StarterKitMVC.Application.Authorization;
using SmartWorkz.StarterKitMVC.Domain.Authorization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Authorization;

/// <summary>
/// JSON file-based implementation of permission service
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly string _dataPath;
    private readonly object _lock = new();
    
    private List<Feature> _features = [];
    private List<Permission> _permissions = [];
    private List<RolePermission> _rolePermissions = [];
    private bool _isLoaded;

    public PermissionService(string? storagePath = null)
    {
        _dataPath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SmartWorkz", "StarterKitMVC", "permissions");
        
        Directory.CreateDirectory(_dataPath);
    }

    private async Task EnsureLoadedAsync()
    {
        if (_isLoaded) return;
        
        lock (_lock)
        {
            if (_isLoaded) return;
            
            var featuresPath = Path.Combine(_dataPath, "features.json");
            var permissionsPath = Path.Combine(_dataPath, "permissions.json");
            var rolePermissionsPath = Path.Combine(_dataPath, "role-permissions.json");
            
            if (File.Exists(featuresPath))
                _features = JsonSerializer.Deserialize<List<Feature>>(File.ReadAllText(featuresPath)) ?? [];
            
            if (File.Exists(permissionsPath))
                _permissions = JsonSerializer.Deserialize<List<Permission>>(File.ReadAllText(permissionsPath)) ?? [];
            
            if (File.Exists(rolePermissionsPath))
                _rolePermissions = JsonSerializer.Deserialize<List<RolePermission>>(File.ReadAllText(rolePermissionsPath)) ?? [];
            
            // Seed defaults if empty
            if (_features.Count == 0)
                SeedDefaults();
            
            _isLoaded = true;
        }
        
        await Task.CompletedTask;
    }

    private void SeedDefaults()
    {
        // Create default features and permissions
        var features = new List<Feature>
        {
            new() { Key = "dashboard", Name = "Dashboard", Icon = "bi-speedometer2", SortOrder = 1, IsSystem = true },
            new() { Key = "users", Name = "Users", Icon = "bi-people", SortOrder = 2, IsSystem = true },
            new() { Key = "roles", Name = "Roles", Icon = "bi-shield", SortOrder = 3, IsSystem = true },
            new() { Key = "settings", Name = "Settings", Icon = "bi-gear", SortOrder = 4, IsSystem = true },
            new() { Key = "tenants", Name = "Tenants", Icon = "bi-building", SortOrder = 5, IsSystem = true },
            new() { Key = "notifications", Name = "Notifications", Icon = "bi-bell", SortOrder = 6, IsSystem = true },
            new() { Key = "reports", Name = "Reports", Icon = "bi-graph-up", SortOrder = 7, IsSystem = true },
            new() { Key = "audit", Name = "Audit Log", Icon = "bi-journal-text", SortOrder = 8, IsSystem = true },
        };
        
        _features = features;
        
        // Generate permissions for each feature
        foreach (var feature in features)
        {
            var perms = GenerateStandardPermissions(feature.Key, feature.Name);
            _permissions.AddRange(perms);
        }
        
        // Create default admin role permissions
        var adminPermissions = _permissions.Select(p => new RolePermission
        {
            RoleId = "Admin",
            PermissionId = p.Id,
            IsGranted = true
        }).ToList();
        
        _rolePermissions = adminPermissions;
        
        SaveAll();
    }

    private List<Permission> GenerateStandardPermissions(string entity, string displayName)
    {
        var actions = new[] { PermissionAction.View, PermissionAction.Create, PermissionAction.Edit, PermissionAction.Delete };
        return actions.Select((action, index) => new Permission
        {
            Key = $"{entity}.{action.ToString().ToLowerInvariant()}",
            Name = $"{action} {displayName}",
            Entity = entity,
            Action = action,
            Group = displayName,
            SortOrder = index + 1,
            IsSystem = true,
            IsActive = true
        }).ToList();
    }

    private void SaveAll()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(Path.Combine(_dataPath, "features.json"), JsonSerializer.Serialize(_features, options));
        File.WriteAllText(Path.Combine(_dataPath, "permissions.json"), JsonSerializer.Serialize(_permissions, options));
        File.WriteAllText(Path.Combine(_dataPath, "role-permissions.json"), JsonSerializer.Serialize(_rolePermissions, options));
    }

    #region Features

    public async Task<List<Feature>> GetAllFeaturesAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _features.Where(f => f.IsActive).OrderBy(f => f.SortOrder).ToList();
    }

    public async Task<Feature?> GetFeatureByIdAsync(Guid id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _features.FirstOrDefault(f => f.Id == id);
    }

    public async Task<Feature?> GetFeatureByKeyAsync(string key, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _features.FirstOrDefault(f => f.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<Feature>> GetFeatureTreeAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var all = _features.Where(f => f.IsActive).ToList();
        var roots = all.Where(f => f.ParentId == null).OrderBy(f => f.SortOrder).ToList();
        
        foreach (var root in roots)
        {
            BuildFeatureTree(root, all);
            root.Permissions = _permissions.Where(p => p.Entity == root.Key && p.IsActive).ToList();
        }
        
        return roots;
    }

    private void BuildFeatureTree(Feature parent, List<Feature> all)
    {
        parent.Children = all.Where(f => f.ParentId == parent.Id).OrderBy(f => f.SortOrder).ToList();
        parent.Permissions = _permissions.Where(p => p.Entity == parent.Key && p.IsActive).ToList();
        foreach (var child in parent.Children)
            BuildFeatureTree(child, all);
    }

    public async Task<Feature> CreateFeatureAsync(Feature feature, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        feature.Id = Guid.NewGuid();
        feature.CreatedAt = DateTime.UtcNow;
        _features.Add(feature);
        SaveAll();
        return feature;
    }

    public async Task<Feature> UpdateFeatureAsync(Feature feature, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var existing = _features.FirstOrDefault(f => f.Id == feature.Id);
        if (existing == null) throw new InvalidOperationException("Feature not found");
        
        existing.Name = feature.Name;
        existing.Description = feature.Description;
        existing.Icon = feature.Icon;
        existing.ParentId = feature.ParentId;
        existing.SortOrder = feature.SortOrder;
        existing.IsActive = feature.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        
        SaveAll();
        return existing;
    }

    public async Task DeleteFeatureAsync(Guid id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var feature = _features.FirstOrDefault(f => f.Id == id);
        if (feature == null) return;
        if (feature.IsSystem) throw new InvalidOperationException("Cannot delete system feature");
        
        _features.Remove(feature);
        _permissions.RemoveAll(p => p.Entity == feature.Key);
        SaveAll();
    }

    #endregion

    #region Permissions

    public async Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _permissions.Where(p => p.IsActive).OrderBy(p => p.Group).ThenBy(p => p.SortOrder).ToList();
    }

    public async Task<List<Permission>> GetPermissionsByEntityAsync(string entity, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _permissions.Where(p => p.Entity.Equals(entity, StringComparison.OrdinalIgnoreCase) && p.IsActive)
            .OrderBy(p => p.SortOrder).ToList();
    }

    public async Task<Permission?> GetPermissionByIdAsync(Guid id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _permissions.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Permission?> GetPermissionByKeyAsync(string key, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _permissions.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Permission> CreatePermissionAsync(Permission permission, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        permission.Id = Guid.NewGuid();
        permission.CreatedAt = DateTime.UtcNow;
        _permissions.Add(permission);
        SaveAll();
        return permission;
    }

    public async Task<Permission> UpdatePermissionAsync(Permission permission, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var existing = _permissions.FirstOrDefault(p => p.Id == permission.Id);
        if (existing == null) throw new InvalidOperationException("Permission not found");
        
        existing.Name = permission.Name;
        existing.Description = permission.Description;
        existing.Group = permission.Group;
        existing.SortOrder = permission.SortOrder;
        existing.IsActive = permission.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        
        SaveAll();
        return existing;
    }

    public async Task DeletePermissionAsync(Guid id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var permission = _permissions.FirstOrDefault(p => p.Id == id);
        if (permission == null) return;
        if (permission.IsSystem) throw new InvalidOperationException("Cannot delete system permission");
        
        _permissions.Remove(permission);
        _rolePermissions.RemoveAll(rp => rp.PermissionId == id);
        SaveAll();
    }

    public async Task<List<Permission>> GenerateEntityPermissionsAsync(string entity, string displayName, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var perms = GenerateStandardPermissions(entity, displayName);
        _permissions.AddRange(perms);
        SaveAll();
        return perms;
    }

    #endregion

    #region Role Permissions

    public async Task<List<RolePermission>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var rps = _rolePermissions.Where(rp => rp.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var rp in rps)
            rp.Permission = _permissions.FirstOrDefault(p => p.Id == rp.PermissionId);
        return rps;
    }

    public async Task<List<string>> GetRolePermissionKeysAsync(string roleId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var permissionIds = _rolePermissions
            .Where(rp => rp.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) && rp.IsGranted)
            .Select(rp => rp.PermissionId)
            .ToHashSet();
        
        return _permissions.Where(p => permissionIds.Contains(p.Id)).Select(p => p.Key).ToList();
    }

    public async Task SetRolePermissionsAsync(string roleId, List<Guid> permissionIds, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        _rolePermissions.RemoveAll(rp => rp.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase));
        
        foreach (var permId in permissionIds)
        {
            _rolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permId,
                IsGranted = true
            });
        }
        
        SaveAll();
    }

    public async Task GrantPermissionAsync(string roleId, Guid permissionId, string? condition = null, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var existing = _rolePermissions.FirstOrDefault(rp => 
            rp.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) && rp.PermissionId == permissionId);
        
        if (existing != null)
        {
            existing.IsGranted = true;
            existing.Condition = condition;
        }
        else
        {
            _rolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                IsGranted = true,
                Condition = condition
            });
        }
        
        SaveAll();
    }

    public async Task RevokePermissionAsync(string roleId, Guid permissionId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        _rolePermissions.RemoveAll(rp => 
            rp.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) && rp.PermissionId == permissionId);
        SaveAll();
    }

    public async Task<bool> HasPermissionAsync(string roleId, string permissionKey, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var permission = _permissions.FirstOrDefault(p => p.Key.Equals(permissionKey, StringComparison.OrdinalIgnoreCase));
        if (permission == null) return false;
        
        return _rolePermissions.Any(rp => 
            rp.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) && 
            rp.PermissionId == permission.Id && 
            rp.IsGranted);
    }

    public async Task<bool> HasPermissionAsync(IEnumerable<string> roleIds, string permissionKey, CancellationToken ct = default)
    {
        foreach (var roleId in roleIds)
        {
            if (await HasPermissionAsync(roleId, permissionKey, ct))
                return true;
        }
        return false;
    }

    public async Task<HashSet<string>> GetPermissionKeysForRolesAsync(IEnumerable<string> roleIds, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var roleIdSet = roleIds.Select(r => r.ToLowerInvariant()).ToHashSet();
        
        var permissionIds = _rolePermissions
            .Where(rp => roleIdSet.Contains(rp.RoleId.ToLowerInvariant()) && rp.IsGranted)
            .Select(rp => rp.PermissionId)
            .ToHashSet();
        
        return _permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Key)
            .ToHashSet();
    }

    #endregion
}
