namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering HTML validation error messages with Bootstrap styling.
/// Targets the &lt;validation-message&gt; element and generates &lt;div class="invalid-feedback"&gt;.
/// </summary>
/// <remarks>
/// Generates: &lt;div id="fieldName_error" class="invalid-feedback"&gt;Error message...&lt;/div&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .invalid-feedback: Applied to the error message container for Bootstrap validation styling
/// This class makes the error message visible only when the associated input has .is-invalid class.
///
/// Validation Integration: Works with ModelState validation to display errors for specific fields.
/// The div is shown when its associated form control has validation errors and .is-invalid class.
/// This TagHelper should be placed after the input element it validates.
///
/// Usage Pattern: Typically used within FormGroupTagHelper which manages the complete form row,
/// but can also be used standalone after input elements.
///
/// Error Display: The error message is:
/// 1. Automatically populated from ModelState if available (server-side validation)
/// 2. Can show a custom message via the Message property
/// 3. Hidden by default, shown only when the input has .is-invalid class
/// </remarks>
/// <example>
/// &lt;!-- Validation message for a field (after input element) --&gt;
/// &lt;input-tag for="User.Email" type="email" /&gt;
/// &lt;validation-message for="User.Email" /&gt;
///
/// &lt;!-- Validation message with custom message --&gt;
/// &lt;input-tag for="User.Age" type="number" /&gt;
/// &lt;validation-message for="User.Age" message="Age must be between 18 and 100" /&gt;
///
/// &lt;!-- Typical usage in form-group --&gt;
/// &lt;form-group for="User.Email" label="Email" required="true"&gt;
///   &lt;input-tag for="User.Email" type="email" /&gt;
///   &lt;validation-message for="User.Email" /&gt;
/// &lt;/form-group&gt;
///
/// &lt;!-- Multiple fields with validation --&gt;
/// &lt;form-group for="User.Password" label="Password" required="true"&gt;
///   &lt;input-tag for="User.Password" type="password" /&gt;
///   &lt;validation-message for="User.Password" /&gt;
/// &lt;/form-group&gt;
/// &lt;form-group for="User.ConfirmPassword" label="Confirm Password" required="true"&gt;
///   &lt;input-tag for="User.ConfirmPassword" type="password" /&gt;
///   &lt;validation-message for="User.ConfirmPassword" /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("validation-message", Attributes = nameof(For))]
public class ValidationMessageTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the model expression for the validation message.
    /// Binds to the same model property as the corresponding input element.
    /// Used to generate the error ID and retrieve errors from ModelState.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets a custom validation error message.
    /// When provided, this message is displayed instead of server-side validation messages.
    /// If not provided, error messages from ModelState are shown.
    /// </summary>
    [HtmlAttributeName(nameof(Message))]
    public string? Message { get; set; }

    private readonly IAccessibilityService _accessibilityService;

    /// <summary>
    /// Creates a new ValidationMessageTagHelper with dependency injection.
    /// </summary>
    /// <param name="accessibilityService">Service for generating accessible error message IDs and ARIA support.</param>
    public ValidationMessageTagHelper(IAccessibilityService accessibilityService)
    {
        _accessibilityService = accessibilityService;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var fieldName = For?.Name ?? "field";
        var errorId = _accessibilityService.GenerateErrorId(fieldName);

        var html = string.IsNullOrEmpty(Message)
            ? $"<div id=\"{errorId}\" class=\"invalid-feedback\"></div>"
            : $"<div id=\"{errorId}\" class=\"invalid-feedback\">{Message}</div>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
