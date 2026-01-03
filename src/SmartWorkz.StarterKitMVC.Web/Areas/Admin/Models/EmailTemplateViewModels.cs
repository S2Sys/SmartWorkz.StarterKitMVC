using System.ComponentModel.DataAnnotations;
using SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

/// <summary>
/// View model for email template list.
/// </summary>
public class EmailTemplateListViewModel
{
    public List<EmailTemplateItemViewModel> Templates { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? CategoryFilter { get; set; }
    public List<string> Categories { get; set; } = new();
}

/// <summary>
/// View model for a single template in the list.
/// </summary>
public class EmailTemplateItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int PlaceholderCount { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// View model for creating/editing an email template.
/// </summary>
public class EmailTemplateFormViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "ID must be lowercase letters, numbers, and hyphens only")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(500)]
    public string Subject { get; set; } = string.Empty;

    public string? HeaderId { get; set; }
    public string? FooterId { get; set; }

    [Required]
    public string BodyContent { get; set; } = string.Empty;

    public string? PlainTextContent { get; set; }

    public string? Category { get; set; }
    public string? TagsInput { get; set; }
    public bool IsActive { get; set; } = true;

    public List<PlaceholderFormViewModel> Placeholders { get; set; } = new();

    // For dropdowns
    public List<EmailTemplateSectionOptionViewModel> AvailableHeaders { get; set; } = new();
    public List<EmailTemplateSectionOptionViewModel> AvailableFooters { get; set; } = new();
    public List<TemplatePlaceholder> SystemPlaceholders { get; set; } = new();

    public bool IsEditMode { get; set; }
    public bool IsSystem { get; set; }
}

/// <summary>
/// View model for a placeholder in the form.
/// </summary>
public class PlaceholderFormViewModel
{
    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? DefaultValue { get; set; }
    public string? SampleValue { get; set; }
    public PlaceholderType Type { get; set; } = PlaceholderType.Text;
    public bool IsRequired { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// View model for section dropdown options.
/// </summary>
public class EmailTemplateSectionOptionViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

/// <summary>
/// View model for section list.
/// </summary>
public class EmailTemplateSectionListViewModel
{
    public List<EmailTemplateSectionItemViewModel> Headers { get; set; } = new();
    public List<EmailTemplateSectionItemViewModel> Footers { get; set; } = new();
}

/// <summary>
/// View model for a single section in the list.
/// </summary>
public class EmailTemplateSectionItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public SectionType Type { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// View model for creating/editing a section.
/// </summary>
public class EmailTemplateSectionFormViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "ID must be lowercase letters, numbers, and hyphens only")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public SectionType Type { get; set; }

    [Required]
    public string HtmlContent { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsEditMode { get; set; }
    public List<TemplatePlaceholder> SystemPlaceholders { get; set; } = new();
}

/// <summary>
/// View model for template preview.
/// </summary>
public class EmailTemplatePreviewViewModel
{
    public string TemplateId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// View model for import/export.
/// </summary>
public class EmailTemplateImportViewModel
{
    [Required]
    public string JsonContent { get; set; } = string.Empty;
    public bool Overwrite { get; set; }
}
