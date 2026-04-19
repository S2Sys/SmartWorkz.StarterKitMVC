namespace SmartWorkz.Core.Web.TagHelpers.Forms;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("radio-button-tag", Attributes = nameof(For))]
public class RadioButtonTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName(nameof(Label))]
    public string? Label { get; set; }

    [HtmlAttributeName(nameof(Value))]
    public string? Value { get; set; }

    [HtmlAttributeName(nameof(GroupName))]
    public string? GroupName { get; set; }

    [HtmlAttributeName(nameof(Checked))]
    public bool Checked { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var fieldName = GroupName ?? For?.Name ?? "radio";
        var fieldId = $"field_{fieldName.ToLowerInvariant()}_{Value?.ToLowerInvariant()}";
        var checkedAttr = Checked ? "checked" : "";

        var html = $"<div class=\"form-check\">";
        html += $"<input class=\"form-check-input\" type=\"radio\" id=\"{fieldId}\" name=\"{fieldName}\" value=\"{Value}\" {checkedAttr} />";
        if (!string.IsNullOrEmpty(Label))
            html += $"<label class=\"form-check-label\" for=\"{fieldId}\">{Label}</label>";
        html += "</div>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
