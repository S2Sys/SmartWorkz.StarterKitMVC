using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Core.Web.TagHelpers.Common;

[HtmlTargetElement("button", Attributes = nameof(Variant))]
[HtmlTargetElement("a", Attributes = nameof(Variant))]
public class ButtonTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(Variant))]
    public string Variant { get; set; } = "secondary";

    [HtmlAttributeName(nameof(Size))]
    public string? Size { get; set; }

    [HtmlAttributeName(nameof(IsLoading))]
    public bool IsLoading { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var variant = Variant switch
        {
            "primary" => "btn-primary",
            "secondary" => "btn-secondary",
            "danger" => "btn-danger",
            "success" => "btn-success",
            "warning" => "btn-warning",
            "info" => "btn-info",
            "light" => "btn-light",
            "dark" => "btn-dark",
            _ => "btn-secondary"
        };

        var sizeClass = Size switch
        {
            "sm" => "btn-sm",
            "lg" => "btn-lg",
            _ => ""
        };

        var classes = $"btn {variant}";
        if (!string.IsNullOrEmpty(sizeClass))
            classes += $" {sizeClass}";

        if (IsLoading)
        {
            classes += " disabled";
            output.Attributes.SetAttribute("disabled", "disabled");
        }

        if (output.Attributes.ContainsName("class"))
        {
            var existing = output.Attributes["class"].Value.ToString();
            output.Attributes.SetAttribute("class", $"{existing} {classes}");
        }
        else
        {
            output.Attributes.SetAttribute("class", classes);
        }
    }
}
