using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper that wraps form controls with Bootstrap form-group styling, label, and help text support.
/// Targets custom &lt;form-group&gt; element and renders a div wrapper with optional label and help text.
/// </summary>
/// <remarks>
/// Generates: &lt;div class="mb-3"&gt;&lt;label class="form-label"&gt;...&lt;/label&gt;&lt;input class="form-control" /&gt;&lt;small class="form-text text-muted"&gt;...&lt;/small&gt;&lt;/div&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .form-group (mb-3): Container div for proper form field spacing and layout
/// - .form-label: Applied to the label element for consistent label styling
/// - .text-danger: Applied to the required asterisk (*) to highlight required fields in red
/// - .form-text: Applied to help text for smaller, muted styling
/// - .text-muted: Applied to help text to distinguish it from the label and input
/// - .is-invalid: Automatically added to child form controls when ModelState contains errors
///
/// Label Styling and Required Field Indicator:
/// When a label is provided, it is rendered with the .form-label class and optionally includes a red asterisk (*)
/// to indicate a required field. The asterisk is displayed only when the Required property is true.
/// Labels are associated with form controls via the "for" attribute for accessibility.
///
/// Form Control Wrapping:
/// This TagHelper wraps any form control (input, select, textarea, checkbox, radio) in a container div.
/// The child content should include the actual form control element using other TagHelpers like input-tag, select-tag, etc.
/// The wrapper preserves validation states from child controls.
///
/// Validation Integration:
/// When a form field has validation errors in ModelState, the child form controls automatically receive the
/// .is-invalid CSS class, which triggers Bootstrap validation styling (red border, error state).
/// Help text and error messages work together to provide comprehensive feedback to users.
///
/// ModelState Integration:
/// The form-group works seamlessly with ASP.NET Core ModelState validation. If a field has errors,
/// child controls display the .is-invalid state. The For property helps establish the connection between
/// the label and the form field name for proper validation feedback.
///
/// Usage Guidelines:
/// 1. Use form-group as the container for each form field
/// 2. Provide a meaningful label text via the label attribute
/// 3. Set required="true" if the field is mandatory (displays red asterisk)
/// 4. Use help-text attribute for guidance text below the control
/// 5. Place the actual form control (input-tag, select-tag, etc.) inside the form-group
/// </remarks>
/// <example>
/// &lt;!-- Simple form-group with text input --&gt;
/// &lt;form-group for="Model.Name" label="Full Name" required="true"&gt;
///   &lt;input-tag for="Model.Name" placeholder="Enter your full name" /&gt;
/// &lt;/form-group&gt;
///
/// &lt;!-- Form-group with help text --&gt;
/// &lt;form-group for="Model.Email" label="Email Address" required="true" help-text="We'll never share your email"&gt;
///   &lt;input-tag for="Model.Email" type="email" placeholder="you@example.com" /&gt;
/// &lt;/form-group&gt;
///
/// &lt;!-- Form-group with select control --&gt;
/// &lt;form-group for="Model.CountryId" label="Country" required="true" help-text="Select your country of residence"&gt;
///   &lt;select-tag for="Model.CountryId" items="@countries" /&gt;
/// &lt;/form-group&gt;
///
/// &lt;!-- Form-group with textarea --&gt;
/// &lt;form-group for="Model.Comments" label="Comments" help-text="Optional feedback (max 500 characters)"&gt;
///   &lt;textarea-tag for="Model.Comments" rows="4" placeholder="Share your thoughts..." /&gt;
/// &lt;/form-group&gt;
///
/// &lt;!-- Form-group with checkbox --&gt;
/// &lt;form-group for="Model.IsSubscribed" label="Newsletter"&gt;
///   &lt;checkbox-tag for="Model.IsSubscribed" label="Subscribe to newsletter" /&gt;
/// &lt;/form-group&gt;
///
/// &lt;!-- Form-group with validation error state --&gt;
/// &lt;!-- When ModelState contains error for Model.Age, child controls show .is-invalid state --&gt;
/// &lt;form-group for="Model.Age" label="Age" required="true" help-text="Must be 18 or older"&gt;
///   &lt;input-tag for="Model.Age" type="number" placeholder="Enter your age" /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("form-group")]
public class FormGroupTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the ModelExpression for the form field (used for field name resolution).
    /// </summary>
    [HtmlAttributeName("for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the label text to display for the form field.
    /// If not provided, no label is rendered.
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets whether the field is required.
    /// If true, a red asterisk is appended to the label.
    /// </summary>
    [HtmlAttributeName("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the help text to display below the form control.
    /// If not provided, no help text is rendered.
    /// </summary>
    [HtmlAttributeName("help-text")]
    public string? HelpText { get; set; }

    private readonly IFormComponentProvider _formComponentProvider;
    private readonly IAccessibilityService _accessibilityService;

    /// <summary>
    /// Initializes a new instance of the FormGroupTagHelper.
    /// </summary>
    /// <param name="formComponentProvider">Provider for form component configuration.</param>
    /// <param name="accessibilityService">Service for generating accessible ARIA IDs.</param>
    public FormGroupTagHelper(IFormComponentProvider formComponentProvider, IAccessibilityService accessibilityService)
    {
        _formComponentProvider = formComponentProvider ?? throw new ArgumentNullException(nameof(formComponentProvider));
        _accessibilityService = accessibilityService ?? throw new ArgumentNullException(nameof(accessibilityService));
    }

    /// <summary>
    /// Processes the form-group tag and renders a div wrapper with label and help text.
    /// </summary>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = _formComponentProvider.GetConfiguration();

        // Resolve field name from For or Label
        var fieldName = For?.Name ?? Label ?? "field";

        // Generate accessible IDs
        var fieldId = _accessibilityService.GenerateFieldId(fieldName);
        var hintId = _accessibilityService.GenerateHintId(fieldName);

        // Render label if provided
        var labelHtml = !string.IsNullOrEmpty(Label)
            ? $"<label for=\"{fieldId}\" class=\"{config.LabelClass}\">{Label}{(Required ? "<span class=\"text-danger\">*</span>" : "")}</label>"
            : "";

        // Render help text if provided
        var hintHtml = !string.IsNullOrEmpty(HelpText)
            ? $"<small id=\"{hintId}\" class=\"form-text text-muted\">{HelpText}</small>"
            : "";

        // Get the body content (the form control inside this tag)
        var bodyContent = output.GetChildContentAsync().Result.GetContent();

        // Build complete HTML with wrapper div
        var html = $"<div class=\"{config.FormGroupClass}\">{labelHtml}{bodyContent}{hintHtml}</div>";

        // Replace the form-group tag with a div and set content
        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
