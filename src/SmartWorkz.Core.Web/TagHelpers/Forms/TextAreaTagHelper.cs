namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

[HtmlTargetElement("textarea-tag", Attributes = nameof(For))]
public class TextAreaTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName(nameof(Placeholder))]
    public string? Placeholder { get; set; }

    [HtmlAttributeName(nameof(Rows))]
    public int Rows { get; set; } = 3;

    [HtmlAttributeName(nameof(Required))]
    public bool Required { get; set; }

    private readonly IFormComponentProvider _formComponentProvider;

    public TextAreaTagHelper(IFormComponentProvider formComponentProvider)
    {
        _formComponentProvider = formComponentProvider;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = _formComponentProvider.GetConfiguration();
        var fieldName = For?.Name ?? "textarea";
        var fieldId = $"field_{fieldName.ToLowerInvariant()}";

        var attrs = $"id=\"{fieldId}\" class=\"{config.InputClass}\" rows=\"{Rows}\"";
        if (!string.IsNullOrEmpty(Placeholder))
            attrs += $" placeholder=\"{Placeholder}\"";
        if (Required)
            attrs += " required";

        var html = $"<textarea {attrs}></textarea>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
