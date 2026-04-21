using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

[HtmlTargetElement("alert", Attributes = nameof(Type))]
public class AlertTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(Type))]
    public string Type { get; set; } = "info";

    [HtmlAttributeName(nameof(Message))]
    public string? Message { get; set; }

    [HtmlAttributeName(nameof(Dismissible))]
    public bool Dismissible { get; set; } = true;

    private readonly IIconProvider _iconProvider;

    public AlertTagHelper(IIconProvider iconProvider)
    {
        _iconProvider = iconProvider ?? throw new ArgumentNullException(nameof(iconProvider));
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var alertClass = Type switch
        {
            "success" => "alert-success",
            "danger" => "alert-danger",
            "warning" => "alert-warning",
            _ => "alert-info"
        };

        var iconType = Type switch
        {
            "success" => IconType.Success,
            "danger" => IconType.Error,
            "warning" => IconType.Warning,
            _ => IconType.Info
        };

        var classAttr = $"alert {alertClass} d-flex align-items-center";
        if (Dismissible)
            classAttr += " alert-dismissible fade show";

        var closeBtn = Dismissible
            ? "<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>"
            : "";

        var icon = _iconProvider.GetIconHtml(iconType, "me-2 flex-shrink-0");
        var messageContent = string.IsNullOrEmpty(Message) ? "" : $"<div>{Message}</div>";

        var html = $"<div class=\"{classAttr}\">{icon}{messageContent}{closeBtn}</div>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
