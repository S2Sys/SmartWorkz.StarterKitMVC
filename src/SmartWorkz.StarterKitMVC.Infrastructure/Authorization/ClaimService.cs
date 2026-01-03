using System.Text.Json;
using SmartWorkz.StarterKitMVC.Application.Authorization;
using SmartWorkz.StarterKitMVC.Domain.Authorization;
using SmartWorkz.StarterKitMVC.Shared.Constants;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Authorization;

/// <summary>
/// JSON file-based implementation of claim service
/// </summary>
public class ClaimService : IClaimService
{
    private readonly string _dataPath;
    private readonly object _lock = new();
    
    private List<ClaimType> _claimTypes = [];
    private List<RoleClaim> _roleClaims = [];
    private List<UserClaim> _userClaims = [];
    private bool _isLoaded;

    public ClaimService(string? storagePath = null)
    {
        _dataPath = storagePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SmartWorkz", "StarterKitMVC", "claims");
        
        Directory.CreateDirectory(_dataPath);
    }

    private async Task EnsureLoadedAsync()
    {
        if (_isLoaded) return;
        
        lock (_lock)
        {
            if (_isLoaded) return;
            
            var claimTypesPath = Path.Combine(_dataPath, "claim-types.json");
            var roleClaimsPath = Path.Combine(_dataPath, "role-claims.json");
            var userClaimsPath = Path.Combine(_dataPath, "user-claims.json");
            
            if (File.Exists(claimTypesPath))
                _claimTypes = JsonSerializer.Deserialize<List<ClaimType>>(File.ReadAllText(claimTypesPath)) ?? [];
            
            if (File.Exists(roleClaimsPath))
                _roleClaims = JsonSerializer.Deserialize<List<RoleClaim>>(File.ReadAllText(roleClaimsPath)) ?? [];
            
            if (File.Exists(userClaimsPath))
                _userClaims = JsonSerializer.Deserialize<List<UserClaim>>(File.ReadAllText(userClaimsPath)) ?? [];
            
            // Seed defaults if empty
            if (_claimTypes.Count == 0)
                SeedDefaults();
            
            _isLoaded = true;
        }
        
        await Task.CompletedTask;
    }

    private void SeedDefaults()
    {
        // Create default claim types based on entities from AppConstants
        var entities = new[]
        {
            (AppConstants.Entities.Users, "Users", "bi-people"),
            (AppConstants.Entities.Roles, "Roles", "bi-shield"),
            (AppConstants.Entities.Claims, "Claims", "bi-key"),
            (AppConstants.Entities.Permissions, "Permissions", "bi-lock"),
            (AppConstants.Entities.Settings, "Settings", "bi-gear"),
            (AppConstants.Entities.Tenants, "Tenants", "bi-building"),
            (AppConstants.Entities.Notifications, "Notifications", "bi-bell"),
            (AppConstants.Entities.Resources, "Resources", "bi-translate"),
            (AppConstants.Entities.EmailTemplates, "Email Templates", "bi-envelope"),
            (AppConstants.Entities.Lov, "List of Values", "bi-list-ul"),
            (AppConstants.Entities.Categories, "Categories", "bi-folder"),
            (AppConstants.Entities.Audit, "Audit Log", "bi-journal-text"),
            (AppConstants.Entities.Dashboard, "Dashboard", "bi-speedometer2"),
            (AppConstants.Entities.Reports, "Reports", "bi-graph-up")
        };

        // Create permission claim type with all entity permissions
        var permissionClaimType = new ClaimType
        {
            Key = AppConstants.ClaimTypes.Permission,
            Name = "Permission",
            Description = "Entity-level permissions for CRUD operations",
            Icon = "bi-lock",
            Category = AppConstants.ClaimCategories.System,
            AllowMultiple = true,
            IsSystem = true,
            SortOrder = 1,
            PredefinedValues = []
        };

        // Generate CRUD claims for each entity using AppConstants.Actions
        foreach (var (key, name, _) in entities)
        {
            foreach (var action in AppConstants.Actions.Crud)
            {
                permissionClaimType.PredefinedValues.Add(new ClaimValue
                {
                    Value = $"{key}.{action}",
                    Label = $"{action.ToUpperInvariant()[0]}{action[1..]} {name}",
                    Description = $"Permission to {action} {name.ToLowerInvariant()}",
                    SortOrder = permissionClaimType.PredefinedValues.Count + 1
                });
            }
        }

        // Create feature claim type
        var featureClaimType = new ClaimType
        {
            Key = "feature",
            Name = "Feature Access",
            Description = "Access to specific features/modules",
            Icon = "bi-puzzle",
            Category = "Authorization",
            AllowMultiple = true,
            IsSystem = true,
            SortOrder = 2,
            PredefinedValues = entities.Select((e, i) => new ClaimValue
            {
                Value = e.Item1,
                Label = e.Item2,
                Description = $"Access to {e.Item2} feature",
                SortOrder = i + 1
            }).ToList()
        };

        // Create department claim type
        var departmentClaimType = new ClaimType
        {
            Key = "department",
            Name = "Department",
            Description = "User's department affiliation",
            Icon = "bi-diagram-3",
            Category = "Organization",
            AllowMultiple = false,
            IsSystem = true,
            SortOrder = 3,
            PredefinedValues =
            [
                new() { Value = "engineering", Label = "Engineering", SortOrder = 1 },
                new() { Value = "sales", Label = "Sales", SortOrder = 2 },
                new() { Value = "marketing", Label = "Marketing", SortOrder = 3 },
                new() { Value = "hr", Label = "Human Resources", SortOrder = 4 },
                new() { Value = "finance", Label = "Finance", SortOrder = 5 },
                new() { Value = "operations", Label = "Operations", SortOrder = 6 }
            ]
        };

        // Create level claim type
        var levelClaimType = new ClaimType
        {
            Key = "level",
            Name = "Access Level",
            Description = "User's access level in the organization",
            Icon = "bi-bar-chart-steps",
            Category = "Organization",
            AllowMultiple = false,
            IsSystem = true,
            SortOrder = 4,
            PredefinedValues =
            [
                new() { Value = "executive", Label = "Executive", SortOrder = 1 },
                new() { Value = "manager", Label = "Manager", SortOrder = 2 },
                new() { Value = "team_lead", Label = "Team Lead", SortOrder = 3 },
                new() { Value = "senior", Label = "Senior", SortOrder = 4 },
                new() { Value = "junior", Label = "Junior", SortOrder = 5 },
                new() { Value = "intern", Label = "Intern", SortOrder = 6 }
            ]
        };

        // Create tenant claim type
        var tenantClaimType = new ClaimType
        {
            Key = "tenant",
            Name = "Tenant",
            Description = "Tenant/organization the user belongs to",
            Icon = "bi-building",
            Category = "Multi-Tenancy",
            AllowMultiple = false,
            IsSystem = true,
            SortOrder = 5
        };

        // Create locale claim type
        var localeClaimType = new ClaimType
        {
            Key = "locale",
            Name = "Locale",
            Description = "User's preferred locale/language",
            Icon = "bi-globe",
            Category = "Preferences",
            AllowMultiple = false,
            IsSystem = true,
            SortOrder = 6,
            PredefinedValues =
            [
                new() { Value = "en", Label = "English", SortOrder = 1 },
                new() { Value = "es", Label = "Spanish", SortOrder = 2 },
                new() { Value = "fr", Label = "French", SortOrder = 3 },
                new() { Value = "de", Label = "German", SortOrder = 4 },
                new() { Value = "ar", Label = "Arabic", SortOrder = 5 }
            ]
        };

        // Create timezone claim type
        var timezoneClaimType = new ClaimType
        {
            Key = "timezone",
            Name = "Timezone",
            Description = "User's preferred timezone",
            Icon = "bi-clock",
            Category = "Preferences",
            AllowMultiple = false,
            IsSystem = true,
            SortOrder = 7
        };

        _claimTypes =
        [
            permissionClaimType,
            featureClaimType,
            departmentClaimType,
            levelClaimType,
            tenantClaimType,
            localeClaimType,
            timezoneClaimType
        ];

        // Create default Admin role claims - grant all permissions
        foreach (var pv in permissionClaimType.PredefinedValues)
        {
            _roleClaims.Add(new RoleClaim
            {
                RoleId = AppConstants.Roles.Admin,
                ClaimType = AppConstants.ClaimTypes.Permission,
                ClaimValue = pv.Value,
                IsGranted = true
            });
        }

        // Grant all features to Admin
        foreach (var fv in featureClaimType.PredefinedValues)
        {
            _roleClaims.Add(new RoleClaim
            {
                RoleId = AppConstants.Roles.Admin,
                ClaimType = "feature",
                ClaimValue = fv.Value,
                IsGranted = true
            });
        }

        // Create Manager role claims - limited permissions
        var managerPermissions = new[] 
        { 
            $"{AppConstants.Entities.Users}.{AppConstants.Actions.View}", 
            $"{AppConstants.Entities.Roles}.{AppConstants.Actions.View}", 
            $"{AppConstants.Entities.Settings}.{AppConstants.Actions.View}", 
            $"{AppConstants.Entities.Reports}.{AppConstants.Actions.View}", 
            $"{AppConstants.Entities.Dashboard}.{AppConstants.Actions.View}" 
        };
        foreach (var perm in managerPermissions)
        {
            _roleClaims.Add(new RoleClaim
            {
                RoleId = AppConstants.Roles.Manager,
                ClaimType = AppConstants.ClaimTypes.Permission,
                ClaimValue = perm,
                IsGranted = true
            });
        }

        // Create User role claims - basic permissions
        var userPermissions = new[] { $"{AppConstants.Entities.Dashboard}.{AppConstants.Actions.View}" };
        foreach (var perm in userPermissions)
        {
            _roleClaims.Add(new RoleClaim
            {
                RoleId = AppConstants.Roles.User,
                ClaimType = AppConstants.ClaimTypes.Permission,
                ClaimValue = perm,
                IsGranted = true
            });
        }

        SaveAll();
    }

    private void SaveAll()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(Path.Combine(_dataPath, "claim-types.json"), JsonSerializer.Serialize(_claimTypes, options));
        File.WriteAllText(Path.Combine(_dataPath, "role-claims.json"), JsonSerializer.Serialize(_roleClaims, options));
        File.WriteAllText(Path.Combine(_dataPath, "user-claims.json"), JsonSerializer.Serialize(_userClaims, options));
    }

    #region Claim Types

    public async Task<List<ClaimType>> GetAllClaimTypesAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _claimTypes.OrderBy(c => c.SortOrder).ToList();
    }

    public async Task<List<ClaimType>> GetActiveClaimTypesAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _claimTypes.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToList();
    }

    public async Task<ClaimType?> GetClaimTypeByIdAsync(Guid id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _claimTypes.FirstOrDefault(c => c.Id == id);
    }

    public async Task<ClaimType?> GetClaimTypeByKeyAsync(string key, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _claimTypes.FirstOrDefault(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<ClaimType>> GetClaimTypesByCategoryAsync(string category, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _claimTypes.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && c.IsActive)
            .OrderBy(c => c.SortOrder).ToList();
    }

    public async Task<ClaimType> CreateClaimTypeAsync(ClaimType claimType, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        claimType.Id = Guid.NewGuid();
        claimType.CreatedAt = DateTime.UtcNow;
        _claimTypes.Add(claimType);
        SaveAll();
        return claimType;
    }

    public async Task<ClaimType> UpdateClaimTypeAsync(ClaimType claimType, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var existing = _claimTypes.FirstOrDefault(c => c.Id == claimType.Id);
        if (existing == null) throw new InvalidOperationException("Claim type not found");
        
        existing.Name = claimType.Name;
        existing.Description = claimType.Description;
        existing.Icon = claimType.Icon;
        existing.Category = claimType.Category;
        existing.AllowMultiple = claimType.AllowMultiple;
        existing.SortOrder = claimType.SortOrder;
        existing.IsActive = claimType.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        
        SaveAll();
        return existing;
    }

    public async Task DeleteClaimTypeAsync(Guid id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var claimType = _claimTypes.FirstOrDefault(c => c.Id == id);
        if (claimType == null) return;
        if (claimType.IsSystem) throw new InvalidOperationException("Cannot delete system claim type");
        
        _claimTypes.Remove(claimType);
        _roleClaims.RemoveAll(rc => rc.ClaimType.Equals(claimType.Key, StringComparison.OrdinalIgnoreCase));
        _userClaims.RemoveAll(uc => uc.ClaimType.Equals(claimType.Key, StringComparison.OrdinalIgnoreCase));
        SaveAll();
    }

    public async Task<ClaimValue> AddClaimValueAsync(Guid claimTypeId, ClaimValue value, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var claimType = _claimTypes.FirstOrDefault(c => c.Id == claimTypeId);
        if (claimType == null) throw new InvalidOperationException("Claim type not found");
        
        value.Id = Guid.NewGuid();
        claimType.PredefinedValues.Add(value);
        SaveAll();
        return value;
    }

    public async Task RemoveClaimValueAsync(Guid claimTypeId, Guid valueId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var claimType = _claimTypes.FirstOrDefault(c => c.Id == claimTypeId);
        if (claimType == null) return;
        
        claimType.PredefinedValues.RemoveAll(v => v.Id == valueId);
        SaveAll();
    }

    #endregion

    #region Role Claims

    public async Task<List<RoleClaim>> GetRoleClaimsAsync(string roleId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _roleClaims.Where(rc => rc.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<RoleClaim>> GetRoleClaimsByTypeAsync(string roleId, string claimType, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _roleClaims.Where(rc => 
            rc.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) &&
            rc.ClaimType.Equals(claimType, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<RoleClaim> AddRoleClaimAsync(string roleId, string claimType, string claimValue, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        
        // Check if already exists
        var existing = _roleClaims.FirstOrDefault(rc =>
            rc.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) &&
            rc.ClaimType.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
            rc.ClaimValue.Equals(claimValue, StringComparison.OrdinalIgnoreCase));
        
        if (existing != null)
        {
            existing.IsGranted = true;
            SaveAll();
            return existing;
        }
        
        var claim = new RoleClaim
        {
            RoleId = roleId,
            ClaimType = claimType,
            ClaimValue = claimValue,
            IsGranted = true
        };
        
        _roleClaims.Add(claim);
        SaveAll();
        return claim;
    }

    public async Task RemoveRoleClaimAsync(Guid claimId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        _roleClaims.RemoveAll(rc => rc.Id == claimId);
        SaveAll();
    }

    public async Task SetRoleClaimsAsync(string roleId, string claimType, List<string> claimValues, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        
        // Remove existing claims of this type for the role
        _roleClaims.RemoveAll(rc =>
            rc.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) &&
            rc.ClaimType.Equals(claimType, StringComparison.OrdinalIgnoreCase));
        
        // Add new claims
        foreach (var value in claimValues)
        {
            _roleClaims.Add(new RoleClaim
            {
                RoleId = roleId,
                ClaimType = claimType,
                ClaimValue = value,
                IsGranted = true
            });
        }
        
        SaveAll();
    }

    public async Task<bool> RoleHasClaimAsync(string roleId, string claimType, string claimValue, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _roleClaims.Any(rc =>
            rc.RoleId.Equals(roleId, StringComparison.OrdinalIgnoreCase) &&
            rc.ClaimType.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
            rc.ClaimValue.Equals(claimValue, StringComparison.OrdinalIgnoreCase) &&
            rc.IsGranted);
    }

    public async Task<HashSet<string>> GetClaimValuesForRolesAsync(IEnumerable<string> roleIds, string claimType, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        var roleIdSet = roleIds.Select(r => r.ToLowerInvariant()).ToHashSet();
        
        return _roleClaims
            .Where(rc => roleIdSet.Contains(rc.RoleId.ToLowerInvariant()) &&
                        rc.ClaimType.Equals(claimType, StringComparison.OrdinalIgnoreCase) &&
                        rc.IsGranted)
            .Select(rc => rc.ClaimValue)
            .ToHashSet();
    }

    #endregion

    #region User Claims

    public async Task<List<UserClaim>> GetUserClaimsAsync(Guid userId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        return _userClaims.Where(uc => uc.UserId == userId).ToList();
    }

    public async Task<UserClaim> AddUserClaimAsync(Guid userId, string claimType, string claimValue, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        
        var claim = new UserClaim
        {
            UserId = userId,
            ClaimType = claimType,
            ClaimValue = claimValue,
            IsGranted = true
        };
        
        _userClaims.Add(claim);
        SaveAll();
        return claim;
    }

    public async Task RemoveUserClaimAsync(Guid claimId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        _userClaims.RemoveAll(uc => uc.Id == claimId);
        SaveAll();
    }

    public async Task SetUserClaimsAsync(Guid userId, string claimType, List<string> claimValues, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        
        // Remove existing claims of this type for the user
        _userClaims.RemoveAll(uc =>
            uc.UserId == userId &&
            uc.ClaimType.Equals(claimType, StringComparison.OrdinalIgnoreCase));
        
        // Add new claims
        foreach (var value in claimValues)
        {
            _userClaims.Add(new UserClaim
            {
                UserId = userId,
                ClaimType = claimType,
                ClaimValue = value,
                IsGranted = true
            });
        }
        
        SaveAll();
    }

    #endregion

    #region Entity Claims Generation

    public async Task<List<ClaimValue>> GenerateEntityClaimsAsync(string entity, string displayName, CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        
        var permissionType = _claimTypes.FirstOrDefault(c => c.Key == "permission");
        if (permissionType == null) throw new InvalidOperationException("Permission claim type not found");
        
        var actions = new[] { "view", "create", "edit", "delete" };
        var newValues = new List<ClaimValue>();
        
        foreach (var action in actions)
        {
            var key = $"{entity.ToLowerInvariant()}.{action}";
            if (permissionType.PredefinedValues.Any(v => v.Value == key)) continue;
            
            var value = new ClaimValue
            {
                Value = key,
                Label = $"{action.ToUpperInvariant()[0]}{action[1..]} {displayName}",
                Description = $"Permission to {action} {displayName.ToLowerInvariant()}",
                SortOrder = permissionType.PredefinedValues.Count + 1
            };
            
            permissionType.PredefinedValues.Add(value);
            newValues.Add(value);
        }
        
        // Also add to feature claim type
        var featureType = _claimTypes.FirstOrDefault(c => c.Key == "feature");
        if (featureType != null && !featureType.PredefinedValues.Any(v => v.Value == entity.ToLowerInvariant()))
        {
            featureType.PredefinedValues.Add(new ClaimValue
            {
                Value = entity.ToLowerInvariant(),
                Label = displayName,
                Description = $"Access to {displayName} feature",
                SortOrder = featureType.PredefinedValues.Count + 1
            });
        }
        
        SaveAll();
        return newValues;
    }

    public async Task<List<string>> GetEntitiesWithClaimsAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync();
        
        var permissionType = _claimTypes.FirstOrDefault(c => c.Key == "permission");
        if (permissionType == null) return [];
        
        return permissionType.PredefinedValues
            .Select(v => v.Value.Split('.')[0])
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }

    #endregion
}
