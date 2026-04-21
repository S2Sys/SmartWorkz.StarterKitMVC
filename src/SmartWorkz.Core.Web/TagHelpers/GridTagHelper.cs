using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Shared;

namespace SmartWorkz.Web;

/// <summary>
/// High-level TagHelper for simple grid markup. Generates GridComponent under the hood.
/// </summary>
[HtmlTargetElement("grid")]
public class GridTagHelper : TagHelper
{
    [HtmlAttributeName("data-source")]
    public string? DataSource { get; set; }

    [HtmlAttributeName("data-page-size")]
    public int PageSize { get; set; } = 20;

    [HtmlAttributeName("data-allow-selection")]
    public bool AllowRowSelection { get; set; }

    [HtmlAttributeName("data-allow-export")]
    public bool AllowExport { get; set; }

    [HtmlAttributeName("data-allow-column-toggle")]
    public bool AllowColumnVisibilityToggle { get; set; }

    [HtmlAttributeName("data-css-class")]
    public string? CustomCssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // This is a placeholder. In actual implementation, this would need to:
        // 1. Parse child <column> elements
        // 2. Generate the GridComponent markup
        // 3. Inject required services

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "grid-wrapper");
        output.Content.SetContent($"<!-- Grid: {DataSource} Page Size: {PageSize} -->");
    }
}

