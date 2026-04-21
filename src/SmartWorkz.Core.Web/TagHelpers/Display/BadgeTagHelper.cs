using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Web;

[HtmlTargetElement("badge", Attributes = nameof(Type))]
public class BadgeTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(Type))]
    public string Type { get; set; } = "secondary";

    [HtmlAttributeName(nameof(Text))]
    public string? Text { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var badgeClass = Type switch
        {
            "primary" => "bg-primary",
            "success" => "bg-success",
            "danger" => "bg-danger",
            "warning" => "bg-warning",
            "info" => "bg-info",
            "light" => "bg-light text-dark",
            "dark" => "bg-dark",
            _ => "bg-secondary"
        };

        var html = $"<span class=\"badge {badgeClass}\">{Text}</span>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
