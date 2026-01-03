namespace SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

/// <summary>
/// Represents a placeholder definition within an email template.
/// Placeholders are dynamic values that get replaced when rendering the template.
/// </summary>
/// <example>
/// <code>
/// var placeholder = new TemplatePlaceholder
/// {
///     Key = "{{UserName}}",
///     DisplayName = "User Name",
///     Description = "The recipient's full name",
///     DefaultValue = "Valued Customer",
///     Type = PlaceholderType.Text,
///     IsRequired = true
/// };
/// </code>
/// </example>
public sealed class TemplatePlaceholder
{
    /// <summary>
    /// The placeholder key used in templates (e.g., "{{UserName}}").
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable display name for the placeholder.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of what this placeholder represents.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Default value used when no value is provided during rendering.
    /// </summary>
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// The data type of the placeholder value.
    /// </summary>
    public PlaceholderType Type { get; set; } = PlaceholderType.Text;
    
    /// <summary>
    /// Whether this placeholder must have a value when rendering.
    /// </summary>
    public bool IsRequired { get; set; }
    
    /// <summary>
    /// Display order in the placeholder list.
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Sample value for preview purposes.
    /// </summary>
    public string? SampleValue { get; set; }
}
