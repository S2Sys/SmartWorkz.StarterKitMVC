using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

/// <summary>
/// View model for displaying role in list
/// </summary>
public class RoleListViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// View model for creating/editing role
/// </summary>
public class RoleFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Role name is required")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 256 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(128)]
    public string? TenantId { get; set; }

    public bool IsSystemRole { get; set; }

    // Selected permissions
    public List<Guid> SelectedPermissionIds { get; set; } = [];

    // Available permissions grouped by category
    public List<PermissionGroupViewModel> PermissionGroups { get; set; } = [];
}

public class PermissionGroupViewModel
{
    public string Category { get; set; } = string.Empty;
    public List<PermissionSelectItem> Permissions { get; set; } = [];
}

public class PermissionSelectItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSelected { get; set; }
}

/// <summary>
/// View model for role details
/// </summary>
public class RoleDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public IEnumerable<string> Permissions { get; set; } = [];
    public IEnumerable<UserListViewModel> Users { get; set; } = [];
}
