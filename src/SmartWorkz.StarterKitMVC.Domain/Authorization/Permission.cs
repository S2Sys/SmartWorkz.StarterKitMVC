namespace SmartWorkz.StarterKitMVC.Domain.Authorization;

/// <summary>
/// Represents a permission that can be assigned to roles.
/// Permissions are entity-based and define what actions can be performed.
/// </summary>
public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Unique permission key (e.g., "users.create", "orders.view", "reports.export")
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for the permission
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of what this permission allows
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// The entity/module this permission belongs to (e.g., "Users", "Orders", "Reports")
    /// </summary>
    public string Entity { get; set; } = string.Empty;
    
    /// <summary>
    /// The action type (View, Create, Edit, Delete, Export, etc.)
    /// </summary>
    public PermissionAction Action { get; set; } = PermissionAction.View;
    
    /// <summary>
    /// Group for organizing permissions in UI
    /// </summary>
    public string? Group { get; set; }
    
    /// <summary>
    /// Sort order within the group
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Whether this is a system permission that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; }
    
    /// <summary>
    /// Whether this permission is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Standard permission actions
/// </summary>
public enum PermissionAction
{
    View = 1,
    Create = 2,
    Edit = 3,
    Delete = 4,
    Export = 5,
    Import = 6,
    Approve = 7,
    Reject = 8,
    Publish = 9,
    Archive = 10,
    Restore = 11,
    ManagePermissions = 12,
    ManageSettings = 13,
    FullAccess = 99
}

/// <summary>
/// Represents a feature/module in the system
/// </summary>
public class Feature
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Unique feature key (e.g., "users", "orders", "reports")
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the feature
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Icon class (Bootstrap Icons)
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// Parent feature ID for hierarchical features
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// Navigation property to parent
    /// </summary>
    public Feature? Parent { get; set; }
    
    /// <summary>
    /// Child features
    /// </summary>
    public List<Feature> Children { get; set; } = [];
    
    /// <summary>
    /// Permissions available for this feature
    /// </summary>
    public List<Permission> Permissions { get; set; } = [];
    
    /// <summary>
    /// Sort order
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Whether this is a system feature that cannot be deleted
    /// </summary>
    public bool IsSystem { get; set; }
    
    /// <summary>
    /// Whether this feature is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Maps roles to permissions
/// </summary>
public class RolePermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Role identifier (can be role name or ID)
    /// </summary>
    public string RoleId { get; set; } = string.Empty;
    
    /// <summary>
    /// Permission ID
    /// </summary>
    public Guid PermissionId { get; set; }
    
    /// <summary>
    /// Navigation property
    /// </summary>
    public Permission? Permission { get; set; }
    
    /// <summary>
    /// Whether this permission is granted (true) or denied (false)
    /// </summary>
    public bool IsGranted { get; set; } = true;
    
    /// <summary>
    /// Optional condition for the permission (JSON format)
    /// e.g., {"ownOnly": true} - can only access own records
    /// </summary>
    public string? Condition { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Permission requirement for authorization
/// </summary>
public class PermissionRequirement
{
    public string Entity { get; set; } = string.Empty;
    public PermissionAction Action { get; set; }
    public string? Condition { get; set; }
    
    public PermissionRequirement(string entity, PermissionAction action)
    {
        Entity = entity;
        Action = action;
    }
    
    /// <summary>
    /// Gets the permission key for this requirement
    /// </summary>
    public string GetPermissionKey() => $"{Entity.ToLowerInvariant()}.{Action.ToString().ToLowerInvariant()}";
}
