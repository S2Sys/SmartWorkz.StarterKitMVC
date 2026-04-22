namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering HTML label elements with Bootstrap styling and required field indicators.
/// Targets the &lt;label-tag&gt; element and generates &lt;label class="form-label"&gt; with optional required asterisk.
/// </summary>
/// <remarks>
/// Generates: &lt;label for="fieldId" class="form-label"&gt;Label Text &lt;span class="text-danger"&gt;*&lt;/span&gt;&lt;/label&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .form-label: Applied to the label element for consistent form styling
/// - .text-danger: Applied to the required asterisk span to highlight required fields in red
///
/// Validation: Works with ModelState validation to associate labels with form controls.
/// The label's "for" attribute connects to the input's ID for accessibility.
/// Use with FormGroupTagHelper for complete form row (label + input + help text + error).
///
/// The required parameter displays a red asterisk (*) to indicate required fields to users.
/// This is a visual indicator and works alongside HTML5 required attribute on inputs.
/// </remarks>
/// <example>
/// &lt;!-- Simple label --&gt;
/// &lt;label-tag for="User.Name" text="Full Name" /&gt;
///
/// &lt;!-- Required field with asterisk --&gt;
/// &lt;label-tag for="User.Email" text="Email Address" required="true" /&gt;
///
/// &lt;!-- Optional field without asterisk --&gt;
/// &lt;label-tag for="User.PhoneNumber" text="Phone Number" /&gt;
///
/// &lt;!-- In form-group wrapper --&gt;
/// &lt;form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email"&gt;
///   &lt;label-tag for="User.Email" text="Email Address" required="true" /&gt;
///   &lt;input-tag for="User.Email" type="email" /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("label-tag", Attributes = nameof(For))]
public class LabelTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the model expression for the label.
    /// Binds the label to a model property to generate the corresponding field ID.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the label text content.
    /// If not provided, derives the label from the field name.
    /// Maps to the visible text content of the HTML label element.
    /// </summary>
    [HtmlAttributeName(nameof(Text))]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets whether this is a required field.
    /// When true, appends a red asterisk (*) to indicate the field is required.
    /// </summary>
    [HtmlAttributeName(nameof(Required))]
    public bool Required { get; set; }

    private readonly IFormComponentProvider _formComponentProvider;
    private readonly IAccessibilityService _accessibilityService;

    /// <summary>
    /// Creates a new LabelTagHelper with dependency injection.
    /// </summary>
    /// <param name="formComponentProvider">Service for form component styling and configuration.</param>
    /// <param name="accessibilityService">Service for accessibility attributes and ARIA support.</param>
    public LabelTagHelper(IFormComponentProvider formComponentProvider, IAccessibilityService accessibilityService)
    {
        _formComponentProvider = formComponentProvider;
        _accessibilityService = accessibilityService;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = _formComponentProvider.GetConfiguration();
        var fieldName = For?.Name ?? Text ?? "field";
        var fieldId = _accessibilityService.GenerateFieldId(fieldName);
        var labelText = Text ?? fieldName;

        var requiredSpan = Required ? "<span class=\"text-danger\">*</span>" : "";
        var html = $"<label for=\"{fieldId}\" class=\"{config.LabelClass}\">{labelText} {requiredSpan}</label>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
