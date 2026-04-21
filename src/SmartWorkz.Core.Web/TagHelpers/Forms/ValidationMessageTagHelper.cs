namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

[HtmlTargetElement("validation-message", Attributes = nameof(For))]
public class ValidationMessageTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName(nameof(Message))]
    public string? Message { get; set; }

    private readonly IAccessibilityService _accessibilityService;

    public ValidationMessageTagHelper(IAccessibilityService accessibilityService)
    {
        _accessibilityService = accessibilityService;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var fieldName = For?.Name ?? "field";
        var errorId = _accessibilityService.GenerateErrorId(fieldName);

        var html = string.IsNullOrEmpty(Message)
            ? $"<div id=\"{errorId}\" class=\"invalid-feedback\"></div>"
            : $"<div id=\"{errorId}\" class=\"invalid-feedback\">{Message}</div>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
