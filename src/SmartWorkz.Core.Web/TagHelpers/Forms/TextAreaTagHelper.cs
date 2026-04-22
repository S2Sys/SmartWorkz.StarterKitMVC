namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering HTML textarea elements with Bootstrap styling, row configuration, and validation support.
/// Targets the &lt;textarea-tag&gt; element and generates &lt;textarea class="form-control"&gt;.
/// </summary>
/// <remarks>
/// Generates: &lt;textarea id="fieldName" class="form-control" rows="3"&gt;&lt;/textarea&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .form-control: Applied to the textarea element for consistent form styling
/// - .is-invalid: Automatically added if ModelState contains errors for this field
/// - .is-valid: Can be added to show successful validation
///
/// Validation: Automatically adds .is-invalid if ModelState contains errors for the bound field.
/// Use with FormGroupTagHelper for complete form row (label + textarea + help text + error).
///
/// The rows attribute controls the visible height of the textarea. Default is 3 rows.
/// Use the Rows property to customize the initial visible height based on your needs.
/// </remarks>
/// <example>
/// &lt;!-- Simple textarea with default 3 rows --&gt;
/// &lt;textarea-tag for="Model.Comments" placeholder="Enter your comments..." /&gt;
///
/// &lt;!-- Textarea with custom height --&gt;
/// &lt;textarea-tag for="Model.Description" rows="6" placeholder="Enter detailed description" /&gt;
///
/// &lt;!-- Required textarea --&gt;
/// &lt;textarea-tag for="Model.Feedback" placeholder="Your feedback is important" required="true" /&gt;
///
/// &lt;!-- Large textarea for longer content --&gt;
/// &lt;textarea-tag for="Model.BioOrNotes" rows="10" placeholder="Tell us about yourself..." /&gt;
///
/// &lt;!-- In form-group wrapper for complete form control --&gt;
/// &lt;form-group for="Model.Comments" label="Comments" required="true" help-text="Please provide at least 10 characters"&gt;
///   &lt;textarea-tag for="Model.Comments" rows="5" placeholder="Share your thoughts..." /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("textarea-tag", Attributes = nameof(For))]
public class TextAreaTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the model expression for the textarea field.
    /// Binds the textarea to a model property.
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed in the textarea when empty.
    /// Maps to the HTML placeholder attribute.
    /// </summary>
    [HtmlAttributeName(nameof(Placeholder))]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the number of visible rows for the textarea.
    /// Default is 3. Higher values make the textarea taller, lower values make it more compact.
    /// Maps to the HTML rows attribute.
    /// </summary>
    [HtmlAttributeName(nameof(Rows))]
    public int Rows { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether the textarea is required for form submission.
    /// When true, adds the HTML "required" attribute for client-side validation.
    /// </summary>
    [HtmlAttributeName(nameof(Required))]
    public bool Required { get; set; }

    private readonly IFormComponentProvider _formComponentProvider;

    /// <summary>
    /// Creates a new TextAreaTagHelper with dependency injection.
    /// </summary>
    /// <param name="formComponentProvider">Service for form component styling and configuration.</param>
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
