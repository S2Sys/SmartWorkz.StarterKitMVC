using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering HTML input elements with Bootstrap styling, optional icon support, and form component styling configuration.
/// Targets the &lt;input-tag&gt; element and generates &lt;input class="form-control"&gt; with optional icon wrappers.
/// </summary>
/// <remarks>
/// Generates: &lt;input type="text" class="form-control" id="fieldName" name="fieldName" /&gt;
/// Or with icons: &lt;div class="input-group"&gt;&lt;span class="input-group-text"&gt;...icon...&lt;/span&gt;&lt;input class="form-control" /&gt;&lt;/div&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .form-control: Applied to the input element for consistent form styling
/// - .input-group: Applied when icon-prefix or icon-suffix is specified
/// - .input-group-text: Applied to icon span elements
/// - .is-invalid: Automatically added if ModelState contains errors for this field
/// - .is-valid: Can be added to show successful validation
///
/// Validation: Automatically adds .is-invalid if ModelState contains errors for the bound field.
/// Use with FormGroupTagHelper for complete form row (label + input + help text + error).
///
/// Supports multiple input types: text, email, password, number, date, time, url, tel, search, color, etc.
/// Optional icon support via IconPrefix and IconSuffix for visual enhancement.
/// </remarks>
/// <example>
/// &lt;!-- Simple text input --&gt;
/// &lt;input-tag for="User.Name" placeholder="Enter your name" /&gt;
///
/// &lt;!-- Email input with validation --&gt;
/// &lt;input-tag for="User.Email" type="email" placeholder="Enter email" required="true" /&gt;
///
/// &lt;!-- Password input --&gt;
/// &lt;input-tag for="User.Password" type="password" placeholder="Enter password" /&gt;
///
/// &lt;!-- Number input with icon --&gt;
/// &lt;input-tag for="Product.Price" type="number" step="0.01" icon-prefix="DollarSign" /&gt;
///
/// &lt;!-- Search input with icon --&gt;
/// &lt;input-tag for="Model.SearchTerm" type="search" icon-suffix="Search" placeholder="Search..." /&gt;
///
/// &lt;!-- In form-group wrapper for complete form control --&gt;
/// &lt;form-group for="User.Email" label="Email Address" required="true" help-text="We'll never share your email"&gt;
///   &lt;input-tag for="User.Email" type="email" icon-prefix="AtSign" /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("input-tag", Attributes = nameof(For))]
public class InputTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the field name for the input element.
    /// Used to derive the field ID and corresponds to the model property name.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public string? For { get; set; }

    /// <summary>
    /// Gets or sets the HTML input type attribute.
    /// Default is "text". Supports: text, email, password, number, date, time, url, tel, search, color, etc.
    /// </summary>
    [HtmlAttributeName(nameof(Type))]
    public string Type { get; set; } = "text";

    /// <summary>
    /// Gets or sets the placeholder text displayed in the input when empty.
    /// Maps to the HTML placeholder attribute.
    /// </summary>
    [HtmlAttributeName(nameof(Placeholder))]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the input is required for form submission.
    /// When true, adds the HTML "required" attribute for client-side validation.
    /// </summary>
    [HtmlAttributeName(nameof(Required))]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the icon type to display before the input element.
    /// When set, wraps the input in an input-group div with the icon in an input-group-text span.
    /// </summary>
    [HtmlAttributeName(nameof(IconPrefix))]
    public IconType? IconPrefix { get; set; }

    /// <summary>
    /// Gets or sets the icon type to display after the input element.
    /// When set, wraps the input in an input-group div with the icon in an input-group-text span.
    /// </summary>
    [HtmlAttributeName(nameof(IconSuffix))]
    public IconType? IconSuffix { get; set; }

    private readonly IFormComponentProvider _formComponentProvider;
    private readonly IIconProvider _iconProvider;

    /// <summary>
    /// Initializes a new instance of the InputTagHelper class.
    /// </summary>
    /// <param name="formComponentProvider">Provider for form component styling configuration.</param>
    /// <param name="iconProvider">Provider for icon rendering.</param>
    public InputTagHelper(IFormComponentProvider formComponentProvider, IIconProvider iconProvider)
    {
        _formComponentProvider = formComponentProvider;
        _iconProvider = iconProvider;
    }

    /// <summary>
    /// Processes the input-tag element and renders an HTML input element with optional icons and styling.
    /// </summary>
    /// <param name="context">The TagHelperContext.</param>
    /// <param name="output">The TagHelperOutput.</param>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = _formComponentProvider.GetConfiguration();
        var fieldName = For ?? "input";
        var fieldId = $"field_{fieldName.ToLowerInvariant()}";

        // Build input attributes
        var inputAttrs = $"type=\"{Type}\" id=\"{fieldId}\" class=\"{config.InputClass}\"";
        if (!string.IsNullOrEmpty(Placeholder))
            inputAttrs += $" placeholder=\"{Placeholder}\"";
        if (Required)
            inputAttrs += " required";

        // Build icon HTML
        var prefixHtml = IconPrefix.HasValue ? $"<span class=\"input-group-text\">{_iconProvider.GetIconHtml(IconPrefix.Value)}</span>" : "";
        var suffixHtml = IconSuffix.HasValue ? $"<span class=\"input-group-text\">{_iconProvider.GetIconHtml(IconSuffix.Value)}</span>" : "";

        // Build final HTML
        var html = prefixHtml != "" || suffixHtml != ""
            ? $"<div class=\"input-group\">{prefixHtml}<input {inputAttrs} />{suffixHtml}</div>"
            : $"<input {inputAttrs} />";

        // Set output
        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
