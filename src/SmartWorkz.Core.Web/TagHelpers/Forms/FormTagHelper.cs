namespace SmartWorkz.Core.Web.TagHelpers.Forms;

using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("form-tag")]
public class FormTagHelper : TagHelper
{
    [HtmlAttributeName("method")]
    public string Method { get; set; } = "post";

    [HtmlAttributeName("action")]
    public string? Action { get; set; }

    [HtmlAttributeName("novalidate")]
    public bool NoValidate { get; set; } = false;

    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var classes = "needs-validation";
        if (!string.IsNullOrEmpty(CssClass))
            classes += $" {CssClass}";

        output.TagName = "form";
        output.Attributes.Clear();
        output.Attributes.SetAttribute("method", Method);
        if (!string.IsNullOrEmpty(Action))
            output.Attributes.SetAttribute("action", Action);
        output.Attributes.SetAttribute("class", classes);
        if (NoValidate)
            output.Attributes.SetAttribute("novalidate", "novalidate");
    }
}
