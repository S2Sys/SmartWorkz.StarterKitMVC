namespace SmartWorkz.Core.Web.Services.Components;

using System.Text.RegularExpressions;

/// <summary>
/// Service for generating accessible ARIA IDs and labels for form components.
/// Provides WCAG-compliant identifiers and labels for accessible form rendering.
/// </summary>
public class AccessibilityService : IAccessibilityService
{
    /// <summary>
    /// Generate unique ID for form field (for aria-labelledby, aria-describedby).
    /// </summary>
    /// <param name="fieldName">The name of the form field.</param>
    /// <returns>A sanitized field ID in the format "field_{name}".</returns>
    public string GenerateFieldId(string fieldName)
    {
        return $"field_{SanitizeName(fieldName)}";
    }

    /// <summary>
    /// Generate error message ID.
    /// </summary>
    /// <param name="fieldName">The name of the form field.</param>
    /// <returns>A sanitized error ID in the format "error_{name}".</returns>
    public string GenerateErrorId(string fieldName)
    {
        return $"error_{SanitizeName(fieldName)}";
    }

    /// <summary>
    /// Generate hint ID.
    /// </summary>
    /// <param name="fieldName">The name of the form field.</param>
    /// <returns>A sanitized hint ID in the format "hint_{name}".</returns>
    public string GenerateHintId(string fieldName)
    {
        return $"hint_{SanitizeName(fieldName)}";
    }

    /// <summary>
    /// Generate ARIA label text.
    /// </summary>
    /// <param name="fieldName">The name of the form field.</param>
    /// <param name="required">Whether the field is required (appends "(required)" if true).</param>
    /// <returns>A formatted ARIA label text.</returns>
    public string GenerateAriaLabel(string fieldName, bool required = false)
    {
        var label = fieldName;
        if (required)
            label += " (required)";
        return label;
    }

    /// <summary>
    /// Sanitize field names by converting to lowercase and replacing special characters with underscores.
    /// </summary>
    /// <param name="name">The field name to sanitize.</param>
    /// <returns>A sanitized field name containing only lowercase alphanumeric characters, underscores, and hyphens.</returns>
    private static string SanitizeName(string name)
    {
        return Regex.Replace(
            name.ToLowerInvariant(),
            @"[^a-z0-9_-]",
            "_"
        );
    }
}
