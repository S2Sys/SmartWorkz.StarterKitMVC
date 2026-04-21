using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering HTML input elements with Bootstrap styling, optional icons, and form component styling configuration.
/// </summary>
[HtmlTargetElement("input-tag", Attributes = nameof(For))]
public class InputTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the field name for the input field.
    /// Used to derive the field name and ID.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public string? For { get; set; }

    /// <summary>
    /// Gets or sets the HTML input type. Default is "text".
    /// </summary>
    [HtmlAttributeName(nameof(Type))]
    public string Type { get; set; } = "text";

    /// <summary>
    /// Gets or sets the placeholder text for the input.
    /// </summary>
    [HtmlAttributeName(nameof(Placeholder))]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the input is required.
    /// </summary>
    [HtmlAttributeName(nameof(Required))]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the icon to display before the input.
    /// When set, input is wrapped in an input-group div.
    /// </summary>
    [HtmlAttributeName(nameof(IconPrefix))]
    public IconType? IconPrefix { get; set; }

    /// <summary>
    /// Gets or sets the icon to display after the input.
    /// When set, input is wrapped in an input-group div.
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
