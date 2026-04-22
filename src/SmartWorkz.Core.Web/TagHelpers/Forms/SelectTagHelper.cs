using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering HTML select elements with support for list items, enum binding, blank option handling, and Bootstrap styling.
/// Targets the &lt;select-tag&gt; element and generates &lt;select class="form-control"&gt; with options.
/// </summary>
/// <remarks>
/// Generates: &lt;select id="fieldName" class="form-control"&gt;&lt;option value=""&gt;-- Select --&lt;/option&gt;...&lt;/select&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .form-control: Applied to the select element for consistent form styling
/// - .is-invalid: Automatically added if ModelState contains errors for this field
/// - .is-valid: Can be added to show successful validation
///
/// Validation: Automatically adds .is-invalid if ModelState contains errors for the bound field.
/// Use with FormGroupTagHelper for complete form row (label + input + help text + error).
///
/// Multiple File Selection: Note - This TagHelper does NOT currently support the HTML5 "multiple" attribute.
/// For multi-select functionality, consider using a custom component or implementing a specialized multi-select TagHelper.
///
/// The select can be populated either via:
/// 1. Items property with SelectListItem collection
/// 2. EnumType property with an enum type (values are automatically enumerated)
/// 3. Both - Items takes precedence if both are provided
///
/// By default, a blank option is added at the top (customizable via AddBlank and BlankText).
/// </remarks>
/// <example>
/// &lt;!-- Simple select with SelectListItem collection --&gt;
/// &lt;select-tag for="User.CountryId" items="@countries" /&gt;
///
/// &lt;!-- Select with enum binding --&gt;
/// &lt;select-tag for="Order.Status" enum-type="typeof(OrderStatus)" /&gt;
///
/// &lt;!-- Select without blank option --&gt;
/// &lt;select-tag for="Product.Category" items="@categories" add-blank="false" /&gt;
///
/// &lt;!-- Select with custom blank text --&gt;
/// &lt;select-tag for="User.Department" items="@departments" blank-text="Choose a department..." /&gt;
///
/// &lt;!-- With initial selection --&gt;
/// @{ var selected = categories.First(c =&gt; c.Value == "electronics"); selected.Selected = true; }
/// &lt;select-tag for="Product.Category" items="@categories" /&gt;
///
/// &lt;!-- In form-group wrapper --&gt;
/// &lt;form-group for="User.CountryId" label="Country" required="true" help-text="Select your country"&gt;
///   &lt;select-tag for="User.CountryId" items="@countries" /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("select-tag", Attributes = nameof(For))]
public class SelectTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the field name for the select element.
    /// Used to derive the field ID and corresponds to the model property name.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public string? For { get; set; }

    /// <summary>
    /// Gets or sets the collection of SelectListItem objects to render as options.
    /// Each SelectListItem provides the option text, value, and selected state.
    /// Takes precedence over EnumType if both are provided.
    /// </summary>
    [HtmlAttributeName(nameof(Items))]
    public IEnumerable<SelectListItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the enum type to use for auto-generating options.
    /// When set, all enum values are enumerated and converted to SelectListItem objects.
    /// The enum name is used as both text and value.
    /// Ignored if Items property is also provided.
    /// </summary>
    [HtmlAttributeName(nameof(EnumType))]
    public Type? EnumType { get; set; }

    /// <summary>
    /// Gets or sets whether to add a blank placeholder option at the beginning of the select.
    /// Default is true. When true, renders a default &lt;option&gt; with empty value.
    /// </summary>
    [HtmlAttributeName(nameof(AddBlank))]
    public bool AddBlank { get; set; } = true;

    /// <summary>
    /// Gets or sets the text for the blank placeholder option.
    /// Default is "-- Select --". Only used if AddBlank is true.
    /// Maps to the text content of the blank &lt;option&gt; element.
    /// </summary>
    [HtmlAttributeName(nameof(BlankText))]
    public string BlankText { get; set; } = "-- Select --";

    private readonly IFormComponentProvider _formComponentProvider;

    /// <summary>
    /// Initializes a new instance of the SelectTagHelper class.
    /// </summary>
    /// <param name="formComponentProvider">Provider for form component styling configuration.</param>
    public SelectTagHelper(IFormComponentProvider formComponentProvider)
    {
        _formComponentProvider = formComponentProvider;
    }

    /// <summary>
    /// Processes the select-tag element and renders an HTML select element with options.
    /// </summary>
    /// <param name="context">The TagHelperContext.</param>
    /// <param name="output">The TagHelperOutput.</param>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = _formComponentProvider.GetConfiguration();
        var fieldName = For ?? "select";
        var fieldId = $"field_{fieldName.ToLowerInvariant()}";

        var items = GetSelectItems();
        var optionsHtml = "";

        if (AddBlank)
            optionsHtml += $"<option value=\"\">{BlankText}</option>";

        foreach (var item in items)
        {
            var selectedAttr = item.Selected ? " selected" : "";
            optionsHtml += $"<option value=\"{item.Value}\"{selectedAttr}>{item.Text}</option>";
        }

        var html = $"<select id=\"{fieldId}\" class=\"{config.InputClass}\">{optionsHtml}</select>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }

    /// <summary>
    /// Gets the select items from either the Items property or EnumType property.
    /// If both Items and EnumType are provided, Items takes precedence.
    /// </summary>
    /// <returns>A list of SelectListItem objects to render as options.</returns>
    private List<SelectListItem> GetSelectItems()
    {
        if (Items != null)
            return Items.ToList();

        if (EnumType != null && EnumType.IsEnum)
        {
            return Enum.GetValues(EnumType)
                .Cast<object>()
                .Select(v => new SelectListItem(
                    text: v.ToString()!,
                    value: v.ToString()!
                ))
                .ToList();
        }

        return new();
    }
}
