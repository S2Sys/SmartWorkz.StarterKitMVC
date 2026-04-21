using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper that wraps form controls with Bootstrap form-group styling, label, and help text support.
/// Targets custom &lt;form-group&gt; element and renders a div wrapper with optional label and help text.
/// </summary>
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
