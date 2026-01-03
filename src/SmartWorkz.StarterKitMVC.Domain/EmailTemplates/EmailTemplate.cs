namespace SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

/// <summary>
/// Represents an email template with placeholders, header, footer, and body content.
/// Templates can be rendered with dynamic data to produce personalized emails.
/// </summary>
/// <example>
/// <code>
/// var template = new EmailTemplate
/// {
///     Id = "welcome-email",
///     Name = "Welcome Email",
///     Subject = "Welcome to {{AppName}}, {{UserName}}!",
///     HeaderId = "header-default",
///     FooterId = "footer-default",
///     BodyContent = "&lt;h1&gt;Hello {{UserName}}&lt;/h1&gt;&lt;p&gt;Thank you for joining...&lt;/p&gt;",
///     Placeholders = new List&lt;TemplatePlaceholder&gt;
///     {
///         new() { Key = "{{UserName}}", DisplayName = "User Name", IsRequired = true }
///     }
/// };
/// </code>
/// </example>
public sealed class EmailTemplate
{
    /// <summary>
    /// Unique identifier for the template (slug format recommended).
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for the template.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Brief description of the template's purpose.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Email subject line (can contain placeholders).
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference to the header section ID.
    /// </summary>
    public string? HeaderId { get; set; }
    
    /// <summary>
    /// Reference to the footer section ID.
    /// </summary>
    public string? FooterId { get; set; }
    
    /// <summary>
    /// HTML body content of the email (can contain placeholders).
    /// </summary>
    public string BodyContent { get; set; } = string.Empty;
    
    /// <summary>
    /// Plain text version of the email body (for email clients that don't support HTML).
    /// </summary>
    public string? PlainTextContent { get; set; }
    
    /// <summary>
    /// List of placeholders used in this template.
    /// </summary>
    public List<TemplatePlaceholder> Placeholders { get; set; } = new();
    
    /// <summary>
    /// Whether the template is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether this is a system template (cannot be deleted).
    /// </summary>
    public bool IsSystem { get; set; }
    
    /// <summary>
    /// Category/group for organizing templates.
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Tags for filtering and searching templates.
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Tenant identifier for multi-tenant isolation.
    /// Null means system-wide (available to all tenants).
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Date and time when the template was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date and time when the template was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who created the template.
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// User who last updated the template.
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Version number for tracking changes.
    /// </summary>
    public int Version { get; set; } = 1;
}
