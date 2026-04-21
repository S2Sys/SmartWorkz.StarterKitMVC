namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("checkbox-tag", Attributes = nameof(For))]
public class CheckboxTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName(nameof(Label))]
    public string? Label { get; set; }

    [HtmlAttributeName(nameof(Value))]
    public string? Value { get; set; } = "true";

    [HtmlAttributeName(nameof(Checked))]
    public bool Checked { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var fieldName = For?.Name ?? Label ?? "checkbox";
        var fieldId = $"field_{fieldName.ToLowerInvariant()}";
        var checkedAttr = Checked ? "checked" : "";

        var html = $"<div class=\"form-check\">";
        html += $"<input class=\"form-check-input\" type=\"checkbox\" id=\"{fieldId}\" value=\"{Value}\" {checkedAttr} />";
        if (!string.IsNullOrEmpty(Label))
            html += $"<label class=\"form-check-label\" for=\"{fieldId}\">{Label}</label>";
        html += "</div>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
