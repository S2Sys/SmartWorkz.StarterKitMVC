using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

[HtmlTargetElement("icon", Attributes = nameof(Name))]
public class IconTagHelper : TagHelper
{
    private readonly IIconProvider _iconProvider;

    [HtmlAttributeName(nameof(Name))]
    public string Name { get; set; } = "";

    [HtmlAttributeName(nameof(Size))]
    public string? Size { get; set; }

    [HtmlAttributeName(nameof(CssClass))]
    public string? CssClass { get; set; }

    public IconTagHelper(IIconProvider iconProvider)
    {
        _iconProvider = iconProvider ?? throw new ArgumentNullException(nameof(iconProvider));
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        if (!Enum.TryParse<IconType>(Name, ignoreCase: true, out var iconType))
        {
            output.Content.SetHtmlContent($"<!-- Unknown icon: {Name} -->");
            return;
        }

        var sizeClass = Size switch
        {
            "sm" => "me-1",
            "lg" => "fs-5",
            _ => ""
        };

        var cssClass = string.IsNullOrEmpty(CssClass)
            ? sizeClass
            : $"{CssClass} {sizeClass}".Trim();

        var html = _iconProvider.GetIconHtml(iconType, cssClass);
        output.Content.SetHtmlContent(html);
    }
}
