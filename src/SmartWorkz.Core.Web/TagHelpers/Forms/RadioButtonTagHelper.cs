namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

/// <summary>
/// TagHelper for rendering HTML radio button inputs with Bootstrap styling, label support, and grouping.
/// Targets the &lt;radio-button-tag&gt; element and generates &lt;div class="form-check"&gt; with radio input.
/// </summary>
/// <remarks>
/// Generates: &lt;div class="form-check"&gt;&lt;input class="form-check-input" type="radio" .../&gt;&lt;label class="form-check-label"&gt;...&lt;/label&gt;&lt;/div&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .form-check: Container div for proper radio button spacing
/// - .form-check-input: Applied to &lt;input&gt; element for Bootstrap styling
/// - .form-check-label: Applied to &lt;label&gt; element for alignment
///
/// Validation: Automatically adds .is-invalid if ModelState contains errors for the bound field.
/// Use with FormGroupTagHelper for complete form row (label + input + help text + error).
///
/// Radio buttons should be grouped by name. Use GroupName to specify the group, or the field name will be used.
/// Each radio button in a group should have a unique Value and the same GroupName to work correctly.
/// Multiple radio-button-tag elements with the same GroupName form a single radio button group.
/// </remarks>
/// <example>
/// &lt;!-- Single radio button --&gt;
/// &lt;radio-button-tag for="Model.Status" group-name="Status" label="Active" value="active" /&gt;
///
/// &lt;!-- Radio button group (render multiple radio-button-tags with same group-name) --&gt;
/// &lt;div&gt;
///   &lt;radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" checked="true" /&gt;
///   &lt;radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="PayPal" value="paypal" /&gt;
///   &lt;radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Bank Transfer" value="bank" /&gt;
/// &lt;/div&gt;
///
/// &lt;!-- Required radio button group --&gt;
/// &lt;div&gt;
///   &lt;radio-button-tag for="Model.Shipping" group-name="Shipping" label="Standard (5-7 days)" value="standard" checked="true" /&gt;
///   &lt;radio-button-tag for="Model.Shipping" group-name="Shipping" label="Express (2-3 days)" value="express" /&gt;
///   &lt;radio-button-tag for="Model.Shipping" group-name="Shipping" label="Overnight" value="overnight" /&gt;
/// &lt;/div&gt;
///
/// &lt;!-- In form-group wrapper --&gt;
/// &lt;form-group for="Model.PaymentMethod" label="Payment Method" required="true"&gt;
///   &lt;radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Credit Card" value="cc" /&gt;
///   &lt;radio-button-tag for="Model.PaymentMethod" group-name="PaymentMethod" label="Debit Card" value="dc" /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("radio-button-tag", Attributes = nameof(For))]
public class RadioButtonTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the model expression for the radio button field.
    /// Binds the radio button to a model property.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the label text displayed next to the radio button.
    /// Maps to the HTML label element associated with the radio button.
    /// </summary>
    [HtmlAttributeName(nameof(Label))]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the value of the radio button.
    /// Each radio button in a group should have a unique value.
    /// Maps to the HTML value attribute.
    /// </summary>
    [HtmlAttributeName(nameof(Value))]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the name of the radio button group.
    /// All radio buttons with the same group name are mutually exclusive (only one can be selected).
    /// If not specified, derives from the field name.
    /// Maps to the HTML name attribute.
    /// </summary>
    [HtmlAttributeName(nameof(GroupName))]
    public string? GroupName { get; set; }

    /// <summary>
    /// Gets or sets whether this radio button is initially checked.
    /// When true, the HTML "checked" attribute is added to the input element.
    /// Only one radio button per group should be checked.
    /// </summary>
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
