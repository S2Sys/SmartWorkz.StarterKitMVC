using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Web;

[HtmlTargetElement("breadcrumb")]
public class BreadcrumbTagHelper : TagHelper
{
    [HtmlAttributeName("items")]
    public List<BreadcrumbItem> Items { get; set; } = new();

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!Items.Any())
        {
            output.SuppressOutput();
            return;
        }

        var html = "<nav aria-label=\"breadcrumb\"><ol class=\"breadcrumb\">";

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var isActive = i == Items.Count - 1;

            if (isActive)
                html += $"<li class=\"breadcrumb-item active\" aria-current=\"page\">{item.Label}</li>";
            else
                html += $"<li class=\"breadcrumb-item\"><a href=\"{item.Url}\">{item.Label}</a></li>";
        }

        html += "</ol></nav>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
