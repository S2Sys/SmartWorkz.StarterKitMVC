namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

[HtmlTargetElement("file-input-tag", Attributes = nameof(For))]
public class FileInputTagHelper : TagHelper
{
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName(nameof(Accept))]
    public string? Accept { get; set; }

    [HtmlAttributeName(nameof(Multiple))]
    public bool Multiple { get; set; }

    [HtmlAttributeName(nameof(Required))]
    public bool Required { get; set; }

    private readonly IFormComponentProvider _formComponentProvider;

    public FileInputTagHelper(IFormComponentProvider formComponentProvider)
    {
        _formComponentProvider = formComponentProvider;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = _formComponentProvider.GetConfiguration();
        var fieldName = For?.Name ?? "file";
        var fieldId = $"field_{fieldName.ToLowerInvariant()}";

        var attrs = $"type=\"file\" id=\"{fieldId}\" class=\"{config.InputClass}\"";
        if (!string.IsNullOrEmpty(Accept))
            attrs += $" accept=\"{Accept}\"";
        if (Multiple)
            attrs += " multiple";
        if (Required)
            attrs += " required";

        var html = $"<input {attrs} />";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
