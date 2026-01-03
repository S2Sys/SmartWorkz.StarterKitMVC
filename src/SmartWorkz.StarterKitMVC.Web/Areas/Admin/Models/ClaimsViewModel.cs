using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

/// <summary>
/// View model for Claims index page
/// </summary>
public class ClaimsIndexViewModel
{
    public List<ClaimTypeViewModel> ClaimTypes { get; set; } = [];
    public int TotalClaimTypes { get; set; }
    public int TotalRoleClaims { get; set; }
    public List<string> Categories { get; set; } = [];
}

/// <summary>
/// View model for displaying a claim type
/// </summary>
public class ClaimTypeViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool AllowMultiple { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public int ValueCount { get; set; }
    public List<ClaimValueViewModel> PredefinedValues { get; set; } = [];
}

/// <summary>
/// View model for displaying a claim value
/// </summary>
public class ClaimValueViewModel
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Form view model for creating/editing claim types
/// </summary>
public class ClaimTypeFormViewModel
{
    public Guid? Id { get; set; }
    
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-z][a-z0-9_]*$", ErrorMessage = "Key must be lowercase, start with a letter, and contain only letters, numbers, and underscores")]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Icon { get; set; }
    
    [Required]
    public string Category { get; set; } = "General";
    
    public bool AllowMultiple { get; set; } = true;
    
    public int SortOrder { get; set; }
    
    public bool IsSystem { get; set; }
    
    public List<string> AvailableCategories { get; set; } = [];
}

/// <summary>
/// Form view model for adding a claim value
/// </summary>
public class ClaimValueFormViewModel
{
    public Guid ClaimTypeId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Value { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Label { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public int SortOrder { get; set; }
}

/// <summary>
/// View model for role claims management
/// </summary>
public class RoleClaimsViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public List<string> AvailableRoles { get; set; } = [];
    public List<ClaimTypeWithValuesViewModel> ClaimTypes { get; set; } = [];
}

/// <summary>
/// View model for claim type with selectable values
/// </summary>
public class ClaimTypeWithValuesViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool AllowMultiple { get; set; }
    public List<ClaimValueCheckboxViewModel> Values { get; set; } = [];
}

/// <summary>
/// View model for claim value checkbox
/// </summary>
public class ClaimValueCheckboxViewModel
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsGranted { get; set; }
}

/// <summary>
/// View model for role claim item
/// </summary>
public class RoleClaimItemViewModel
{
    public Guid Id { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}

/// <summary>
/// View model for entity claims summary
/// </summary>
public class EntityClaimsSummaryViewModel
{
    public string Entity { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public List<string> Claims { get; set; } = [];
}

/// <summary>
/// Model for saving role claims
/// </summary>
public class SaveRoleClaimsModel
{
    public string RoleId { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public List<string> ClaimValues { get; set; } = [];
}
