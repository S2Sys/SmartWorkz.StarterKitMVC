namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

/// <summary>
/// TagHelper for rendering HTML checkbox inputs with Bootstrap styling and label support.
/// Targets the &lt;checkbox-tag&gt; element and generates &lt;div class="form-check"&gt; with checkbox input.
/// </summary>
/// <remarks>
/// Generates: &lt;div class="form-check"&gt;&lt;input class="form-check-input" type="checkbox" .../&gt;&lt;label class="form-check-label"&gt;...&lt;/label&gt;&lt;/div&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .form-check: Container div for proper checkbox spacing
/// - .form-check-input: Applied to &lt;input&gt; element for Bootstrap styling
/// - .form-check-label: Applied to &lt;label&gt; element for alignment
///
/// Validation: Automatically adds .is-invalid if ModelState contains errors for the bound field.
/// Use with FormGroupTagHelper for complete form row (label + input + help text + error).
///
/// The checkbox value defaults to "true" but can be customized. The Checked property controls
/// the initial checked state.
/// </remarks>
/// <example>
/// &lt;!-- Simple checkbox with label --&gt;
/// &lt;checkbox-tag for="User.IsSubscribed" label="Subscribe to newsletter" /&gt;
///
/// &lt;!-- Checkbox with custom value --&gt;
/// &lt;checkbox-tag for="User.AgreedToTerms" label="I agree to the terms" value="1" /&gt;
///
/// &lt;!-- Pre-checked checkbox --&gt;
/// &lt;checkbox-tag for="User.IsActive" label="Active" checked="true" /&gt;
///
/// &lt;!-- In form-group wrapper for complete form control --&gt;
/// &lt;form-group for="User.IsSubscribed" label="Newsletter Subscription" help-text="Get updates via email"&gt;
///   &lt;checkbox-tag for="User.IsSubscribed" label="Subscribe" /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("checkbox-tag", Attributes = nameof(For))]
public class CheckboxTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the model expression for the checkbox field.
    /// Binds the checkbox to a model property.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the label text displayed next to the checkbox.
    /// Maps to the HTML label element associated with the checkbox.
    /// </summary>
    [HtmlAttributeName(nameof(Label))]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the value attribute of the checkbox input.
    /// Default is "true". Used when the checkbox is submitted as form data.
    /// </summary>
    [HtmlAttributeName(nameof(Value))]
    public string? Value { get; set; } = "true";

    /// <summary>
    /// Gets or sets whether the checkbox is initially checked.
    /// When true, the HTML "checked" attribute is added to the input element.
    /// </summary>
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
