namespace SmartWorkz.Web;

/// <summary>
/// Service for generating accessible ARIA IDs and labels for form components.
/// Supports WCAG compliance by providing consistent, properly formatted identifiers.
/// </summary>
public interface IAccessibilityService
{
    /// <summary>
    /// Generate unique ID for form field (for aria-labelledby, aria-describedby).
    /// </summary>
    /// <param name="fieldName">The name of the form field.</param>
    /// <returns>A sanitized field ID in the format "field_{name}".</returns>
    string GenerateFieldId(string fieldName);

    /// <summary>
    /// Generate error message ID.
    /// </summary>
    /// <param name="fieldName">The name of the form field.</param>
    /// <returns>A sanitized error ID in the format "error_{name}".</returns>
    string GenerateErrorId(string fieldName);

    /// <summary>
    /// Generate hint ID.
    /// </summary>
    /// <param name="fieldName">The name of the form field.</param>
    /// <returns>A sanitized hint ID in the format "hint_{name}".</returns>
    string GenerateHintId(string fieldName);

    /// <summary>
    /// Generate ARIA label text.
    /// </summary>
    /// <param name="fieldName">The name of the form field.</param>
    /// <param name="required">Whether the field is required (appends "(required)" if true).</param>
    /// <returns>A formatted ARIA label text.</returns>
    string GenerateAriaLabel(string fieldName, bool required = false);
}
