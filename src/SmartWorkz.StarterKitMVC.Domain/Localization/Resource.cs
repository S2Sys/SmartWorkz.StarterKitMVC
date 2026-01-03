namespace SmartWorkz.StarterKitMVC.Domain.Localization;

/// <summary>
/// Represents a localization resource with parent-child self-join structure.
/// This allows hierarchical organization of resources that can be reused across languages.
/// </summary>
public class Resource
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Unique resource key (e.g., "common.buttons.save", "errors.validation.required")
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Parent resource ID for hierarchical structure
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// Navigation property to parent resource
    /// </summary>
    public Resource? Parent { get; set; }
    
    /// <summary>
    /// Child resources
    /// </summary>
    public List<Resource> Children { get; set; } = [];
    
    /// <summary>
    /// Resource category for grouping (e.g., "Labels", "Messages", "Errors", "Buttons")
    /// </summary>
    public string Category { get; set; } = "General";
    
    /// <summary>
    /// Module/feature this resource belongs to (e.g., "Common", "Users", "Orders")
    /// </summary>
    public string? Module { get; set; }
    
    /// <summary>
    /// Description or context for translators
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Maximum length hint for translators
    /// </summary>
    public int? MaxLength { get; set; }
    
    /// <summary>
    /// Whether this resource supports pluralization
    /// </summary>
    public bool SupportsPluralForms { get; set; }
    
    /// <summary>
    /// Placeholder tokens used in this resource (e.g., "{0}", "{userName}")
    /// </summary>
    public List<string> Placeholders { get; set; } = [];
    
    /// <summary>
    /// Sort order within parent
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Whether this is a system resource
    /// </summary>
    public bool IsSystem { get; set; }
    
    /// <summary>
    /// Whether this resource is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Translations for this resource
    /// </summary>
    public List<ResourceTranslation> Translations { get; set; } = [];
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets the full hierarchical path of this resource
    /// </summary>
    public string GetFullPath()
    {
        var parts = new List<string> { Key };
        var current = Parent;
        while (current != null)
        {
            parts.Insert(0, current.Key);
            current = current.Parent;
        }
        return string.Join(".", parts);
    }
}

/// <summary>
/// Represents a translation of a resource in a specific language
/// </summary>
public class ResourceTranslation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Resource ID
    /// </summary>
    public Guid ResourceId { get; set; }
    
    /// <summary>
    /// Navigation property
    /// </summary>
    public Resource? Resource { get; set; }
    
    /// <summary>
    /// Language code (e.g., "en", "en-US", "fr", "de")
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Translated value
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Plural form value (if applicable)
    /// </summary>
    public string? PluralValue { get; set; }
    
    /// <summary>
    /// Zero form value (if applicable, for languages that have special zero forms)
    /// </summary>
    public string? ZeroValue { get; set; }
    
    /// <summary>
    /// Translation status
    /// </summary>
    public TranslationStatus Status { get; set; } = TranslationStatus.Draft;
    
    /// <summary>
    /// Whether this translation has been reviewed
    /// </summary>
    public bool IsReviewed { get; set; }
    
    /// <summary>
    /// Reviewer notes
    /// </summary>
    public string? ReviewNotes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Translation status
/// </summary>
public enum TranslationStatus
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Published = 3,
    NeedsReview = 4
}

/// <summary>
/// Represents a supported language
/// </summary>
public class Language
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsRtl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
