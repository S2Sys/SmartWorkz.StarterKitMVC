namespace SmartWorkz.Core.Web.TagHelpers.Forms;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Core.Web.Services.Components;

[HtmlTargetElement("label-tag", Attributes = nameof(For))]
public class LabelTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName(nameof(Text))]
    public string? Text { get; set; }

    [HtmlAttributeName(nameof(Required))]
    public bool Required { get; set; }

    private readonly IFormComponentProvider _formComponentProvider;
    private readonly IAccessibilityService _accessibilityService;

    public LabelTagHelper(IFormComponentProvider formComponentProvider, IAccessibilityService accessibilityService)
    {
        _formComponentProvider = formComponentProvider;
        _accessibilityService = accessibilityService;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = _formComponentProvider.GetConfiguration();
        var fieldName = For?.Name ?? Text ?? "field";
        var fieldId = _accessibilityService.GenerateFieldId(fieldName);
        var labelText = Text ?? fieldName;

        var requiredSpan = Required ? "<span class=\"text-danger\">*</span>" : "";
        var html = $"<label for=\"{fieldId}\" class=\"{config.LabelClass}\">{labelText} {requiredSpan}</label>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
