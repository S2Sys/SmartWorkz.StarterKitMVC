namespace SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

/// <summary>
/// Represents a reusable email template section (header or footer).
/// Sections can be shared across multiple email templates.
/// </summary>
/// <example>
/// <code>
/// var header = new EmailTemplateSection
/// {
///     Id = "header-default",
///     Name = "Default Header",
///     Type = SectionType.Header,
///     HtmlContent = "&lt;div style='background:#007bff;padding:20px;'&gt;...&lt;/div&gt;",
///     IsDefault = true
/// };
/// </code>
/// </example>
public sealed class EmailTemplateSection
{
    /// <summary>
    /// Unique identifier for the section.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for the section.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of section (Header or Footer).
    /// </summary>
    public SectionType Type { get; set; }
    
    /// <summary>
    /// HTML content of the section.
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is the default section for its type.
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Tenant identifier for multi-tenant isolation.
    /// Null means system-wide (available to all tenants).
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Whether the section is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Date and time when the section was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date and time when the section was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who created the section.
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// User who last updated the section.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
