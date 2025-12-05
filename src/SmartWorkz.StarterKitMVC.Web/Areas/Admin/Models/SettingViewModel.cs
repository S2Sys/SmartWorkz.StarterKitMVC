using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

/// <summary>
/// View model for setting category
/// </summary>
public class SettingCategoryViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsSystem { get; set; }
    public int SettingCount { get; set; }
}

/// <summary>
/// View model for setting definition
/// </summary>
public class SettingDefinitionViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ValueType { get; set; } = "String";
    public string? DefaultValue { get; set; }
    public string? CurrentValue { get; set; }
    public bool IsRequired { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsSystem { get; set; }
    public string? ValidationRegex { get; set; }
    public string? MinValue { get; set; }
    public string? MaxValue { get; set; }
    public List<SettingOptionViewModel> Options { get; set; } = [];
}

public class SettingOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// View model for settings page grouped by category
/// </summary>
public class SettingsPageViewModel
{
    public List<SettingCategoryWithSettingsViewModel> Categories { get; set; } = [];
    public string? ActiveCategoryKey { get; set; }
}

public class SettingCategoryWithSettingsViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public List<SettingDefinitionViewModel> Settings { get; set; } = [];
}

/// <summary>
/// View model for saving settings
/// </summary>
public class SaveSettingsViewModel
{
    public string CategoryKey { get; set; } = string.Empty;
    public Dictionary<string, string?> Settings { get; set; } = [];
}

/// <summary>
/// View model for creating/editing setting category
/// </summary>
public class SettingCategoryFormViewModel
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

    public int SortOrder { get; set; }
}

/// <summary>
/// View model for creating/editing setting definition
/// </summary>
public class SettingDefinitionFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Key is required")]
    [StringLength(256, MinimumLength = 2)]
    [RegularExpression(@"^[a-z0-9_.]+$", ErrorMessage = "Key can only contain lowercase letters, numbers, dots, and underscores")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string ValueType { get; set; } = "String";

    public string? DefaultValue { get; set; }

    [StringLength(500)]
    public string? ValidationRegex { get; set; }

    [StringLength(50)]
    public string? MinValue { get; set; }

    [StringLength(50)]
    public string? MaxValue { get; set; }

    public string? Options { get; set; } // JSON array

    public bool IsRequired { get; set; }

    public bool IsEncrypted { get; set; }

    public int SortOrder { get; set; }

    // For dropdown
    public List<SettingCategoryViewModel> AvailableCategories { get; set; } = [];
}
