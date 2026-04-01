using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.StarterKitMVC.Shared.TagHelpers;

/// <summary>
/// Renders a &lt;select&gt; populated from an enum type.
///
/// Usage:
///   &lt;select asp-for="Status" enum-select-for="typeof(UserStatus)"&gt;&lt;/select&gt;
///
/// Or from a string type name (useful when enum is in another assembly):
///   &lt;select asp-for="Status" enum-type="SmartWorkz.StarterKitMVC.Shared.Enums.UserStatus"&gt;&lt;/select&gt;
///
/// Adds a blank "-- Select --" option by default. Suppress with add-blank="false".
/// </summary>
[HtmlTargetElement("select", Attributes = "enum-select-for")]
public class EnumSelectTagHelper : TagHelper
{
    [HtmlAttributeName("enum-select-for")]
    public Type? EnumType { get; set; }

    [HtmlAttributeName("add-blank")]
    public bool AddBlank { get; set; } = true;

    [HtmlAttributeName("blank-text")]
    public string BlankText { get; set; } = "-- Select --";

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (EnumType == null || !EnumType.IsEnum) return;

        var items = Enum.GetValues(EnumType)
            .Cast<object>()
            .Select(v => new SelectListItem(
                text:  FormatName(v.ToString()!),
                value: v.ToString()!))
            .ToList();

        if (AddBlank)
            items.Insert(0, new SelectListItem(BlankText, ""));

        var existingValue = GetCurrentValue(output);

        var sb = new System.Text.StringBuilder();
        foreach (var item in items)
        {
            var selected = !string.IsNullOrEmpty(existingValue)
                && string.Equals(item.Value, existingValue, StringComparison.OrdinalIgnoreCase)
                ? " selected" : "";
            sb.Append($"<option value=\"{item.Value}\"{selected}>{item.Text}</option>");
        }

        output.Content.SetHtmlContent(sb.ToString());
    }

    private static string FormatName(string name)
    {
        // PascalCase → "Pascal Case"
        var result = new System.Text.StringBuilder();
        foreach (var ch in name)
        {
            if (char.IsUpper(ch) && result.Length > 0)
                result.Append(' ');
            result.Append(ch);
        }
        return result.ToString();
    }

    private string? GetCurrentValue(TagHelperOutput output)
    {
        output.Attributes.TryGetAttribute("value", out var attr);
        return attr?.Value?.ToString();
    }
}
