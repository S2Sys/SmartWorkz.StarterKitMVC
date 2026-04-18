using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Core.Web.Services.Components;

namespace SmartWorkz.Core.Web.TagHelpers.Forms;

/// <summary>
/// TagHelper for rendering HTML select elements with support for list items, enum binding, and blank option handling.
/// </summary>
[HtmlTargetElement("select-tag", Attributes = nameof(For))]
public class SelectTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the field name for the select element.
    /// Used to derive the field name and ID.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public string? For { get; set; }

    /// <summary>
    /// Gets or sets the list of SelectListItem objects to render as options.
    /// </summary>
    [HtmlAttributeName(nameof(Items))]
    public IEnumerable<SelectListItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the enum type to use for generating options.
    /// When set, enum values are enumerated and converted to SelectListItem objects.
    /// </summary>
    [HtmlAttributeName(nameof(EnumType))]
    public Type? EnumType { get; set; }

    /// <summary>
    /// Gets or sets whether to add a blank option at the beginning. Default is true.
    /// </summary>
    [HtmlAttributeName(nameof(AddBlank))]
    public bool AddBlank { get; set; } = true;

    /// <summary>
    /// Gets or sets the text for the blank option. Default is "-- Select --".
    /// </summary>
    [HtmlAttributeName(nameof(BlankText))]
    public string BlankText { get; set; } = "-- Select --";

    private readonly IFormComponentProvider _formComponentProvider;

    /// <summary>
    /// Initializes a new instance of the SelectTagHelper class.
    /// </summary>
    /// <param name="formComponentProvider">Provider for form component styling configuration.</param>
    public SelectTagHelper(IFormComponentProvider formComponentProvider)
    {
        _formComponentProvider = formComponentProvider;
    }

    /// <summary>
    /// Processes the select-tag element and renders an HTML select element with options.
    /// </summary>
    /// <param name="context">The TagHelperContext.</param>
    /// <param name="output">The TagHelperOutput.</param>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = _formComponentProvider.GetConfiguration();
        var fieldName = For ?? "select";
        var fieldId = $"field_{fieldName.ToLowerInvariant()}";

        var items = GetSelectItems();
        var optionsHtml = "";

        if (AddBlank)
            optionsHtml += $"<option value=\"\">{BlankText}</option>";

        foreach (var item in items)
        {
            var selectedAttr = item.Selected ? " selected" : "";
            optionsHtml += $"<option value=\"{item.Value}\"{selectedAttr}>{item.Text}</option>";
        }

        var html = $"<select id=\"{fieldId}\" class=\"{config.InputClass}\">{optionsHtml}</select>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }

    /// <summary>
    /// Gets the select items from either the Items property or EnumType property.
    /// If both Items and EnumType are provided, Items takes precedence.
    /// </summary>
    /// <returns>A list of SelectListItem objects to render as options.</returns>
    private List<SelectListItem> GetSelectItems()
    {
        if (Items != null)
            return Items.ToList();

        if (EnumType != null && EnumType.IsEnum)
        {
            return Enum.GetValues(EnumType)
                .Cast<object>()
                .Select(v => new SelectListItem(
                    text: v.ToString()!,
                    value: v.ToString()!
                ))
                .ToList();
        }

        return new();
    }
}
