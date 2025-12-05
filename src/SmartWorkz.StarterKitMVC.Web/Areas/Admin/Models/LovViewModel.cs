using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

/// <summary>
/// View model for LOV category list
/// </summary>
public class LovCategoryListViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// View model for creating/editing LOV category
/// </summary>
public class LovCategoryFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Key is required")]
    [StringLength(128, MinimumLength = 2)]
    [RegularExpression(@"^[a-z0-9_]+$", ErrorMessage = "Key can only contain lowercase letters, numbers, and underscores")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// View model for LOV item list
/// </summary>
public class LovItemListViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string? SubCategoryKey { get; set; }
}

/// <summary>
/// View model for creating/editing LOV item
/// </summary>
public class LovItemFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public Guid CategoryId { get; set; }

    public Guid? SubCategoryId { get; set; }

    [Required(ErrorMessage = "Key is required")]
    [StringLength(128, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Key can only contain letters, numbers, underscores, and hyphens")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display name is required")]
    [StringLength(256, MinimumLength = 1)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Value { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    [StringLength(20)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Invalid color format. Use hex format like #0d6efd")]
    public string? Color { get; set; }

    public Guid? ParentId { get; set; }

    public int SortOrder { get; set; }

    [StringLength(500)]
    public string? Tags { get; set; } // Comma-separated

    public string? Metadata { get; set; } // JSON

    [StringLength(128)]
    public string? TenantId { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    // Localizations
    public List<LovItemLocalizationViewModel> Localizations { get; set; } = [];

    // For dropdowns
    public List<LovCategoryListViewModel> AvailableCategories { get; set; } = [];
    public List<LovSubCategoryViewModel> AvailableSubCategories { get; set; } = [];
    public List<LovItemListViewModel> AvailableParentItems { get; set; } = [];
}

public class LovItemLocalizationViewModel
{
    [Required]
    [StringLength(10)]
    public string CultureCode { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class LovSubCategoryViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}

/// <summary>
/// View model for LOV page with category and items
/// </summary>
public class LovPageViewModel
{
    public List<LovCategoryListViewModel> Categories { get; set; } = [];
    public LovCategoryListViewModel? SelectedCategory { get; set; }
    public List<LovItemListViewModel> Items { get; set; } = [];
}
