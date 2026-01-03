using System.ComponentModel.DataAnnotations;
using SmartWorkz.StarterKitMVC.Application.Localization;
using SmartWorkz.StarterKitMVC.Domain.Localization;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

#region Index

public class ResourcesIndexViewModel
{
    public List<LanguageViewModel> Languages { get; set; } = [];
    public List<ResourceViewModel> Resources { get; set; } = [];
    public List<TranslationStats> Stats { get; set; } = [];
    public int TotalResources { get; set; }
}

#endregion

#region Languages

public class LanguageViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsRtl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class LanguageFormViewModel
{
    [Required(ErrorMessage = "Language code is required")]
    [StringLength(10, MinimumLength = 2)]
    [RegularExpression(@"^[a-z]{2}(-[A-Z]{2})?$", ErrorMessage = "Invalid language code format (e.g., en, en-US)")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Native name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string NativeName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Icon { get; set; }

    public bool IsRtl { get; set; }

    public bool IsDefault { get; set; }

    public int SortOrder { get; set; }

    public bool IsEditMode { get; set; }
}

#endregion

#region Resources

public class ResourceListViewModel
{
    public List<ResourceViewModel> Resources { get; set; } = [];
    public List<string> Languages { get; set; } = [];
    public string? ModuleFilter { get; set; }
    public string? CategoryFilter { get; set; }
    public List<string> Modules { get; set; } = [];
    public List<string> Categories { get; set; } = [];
}

public class ResourceViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public int TranslationCount { get; set; }
    public List<ResourceViewModel> Children { get; set; } = [];
}

public class ResourceFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Key is required")]
    [StringLength(256, MinimumLength = 1)]
    public string Key { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100)]
    public string Category { get; set; } = "General";

    [StringLength(100)]
    public string? Module { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? MaxLength { get; set; }

    public bool SupportsPluralForms { get; set; }

    public string? PlaceholdersInput { get; set; }

    public int SortOrder { get; set; }

    public bool IsSystem { get; set; }

    /// <summary>
    /// Default language value (for quick creation)
    /// </summary>
    public string? DefaultValue { get; set; }

    public string? PluralValue { get; set; }

    public List<SelectOption> AvailableParents { get; set; } = [];
    public List<string> Categories { get; set; } = [];
}

#endregion

#region Translations

public class TranslationsViewModel
{
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public List<LanguageViewModel> Languages { get; set; } = [];
    public List<ResourceTranslationItemViewModel> Resources { get; set; } = [];
    public TranslationStats Stats { get; set; } = new();
}

public class ResourceTranslationItemViewModel
{
    public Guid ResourceId { get; set; }
    public string ResourceKey { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Placeholders { get; set; } = [];
    public string? Value { get; set; }
    public string? PluralValue { get; set; }
    public TranslationStatus? Status { get; set; }
    public bool SupportsPluralForms { get; set; }
}

#endregion

#region Import/Export

public class ResourceImportViewModel
{
    [Required(ErrorMessage = "JSON content is required")]
    public string JsonContent { get; set; } = string.Empty;

    public bool Overwrite { get; set; }
}

#endregion
