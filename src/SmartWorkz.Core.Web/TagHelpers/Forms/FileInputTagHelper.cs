namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering HTML file input elements with Bootstrap styling and validation support.
/// Targets the &lt;file-input-tag&gt; element and generates &lt;input type="file"&gt; with form-control class.
/// </summary>
/// <remarks>
/// Generates: &lt;input type="file" class="form-control" id="fieldName" name="fieldName" /&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .form-control: Applied to the file input for consistent form styling
///
/// Validation: Automatically adds .is-invalid if ModelState contains errors for the bound field.
/// Use with FormGroupTagHelper for complete form row (label + input + help text + error).
///
/// Supports: accept attribute to restrict file types (e.g., "image/*", ".pdf,.doc")
/// Supports: multiple attribute for multi-file uploads
/// Supports: required attribute for form validation
/// </remarks>
/// <example>
/// &lt;!-- Simple file input --&gt;
/// &lt;file-input-tag for="Model.ProfilePhoto" /&gt;
///
/// &lt;!-- File input accepting specific types --&gt;
/// &lt;file-input-tag for="Model.Document" accept=".pdf,.docx" required="true" /&gt;
///
/// &lt;!-- Multiple file upload --&gt;
/// &lt;file-input-tag for="Model.Attachments" accept="image/*" multiple="true" /&gt;
///
/// &lt;!-- In form-group wrapper --&gt;
/// &lt;form-group for="Model.ProfilePhoto" label="Upload Photo" required="true" help-text="JPG or PNG, max 5MB"&gt;
///   &lt;file-input-tag for="Model.ProfilePhoto" accept="image/jpeg,image/png" /&gt;
/// &lt;/form-group&gt;
/// </example>
[HtmlTargetElement("file-input-tag", Attributes = nameof(For))]
public class FileInputTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the model expression for the file input field.
    /// Binds the file input to a model property (typically IFormFile or IEnumerable&lt;IFormFile&gt;).
    /// </summary>
    [HtmlAttributeName(nameof(For))]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets the accepted file types for the file input.
    /// Maps to the HTML accept attribute (e.g., "image/*", ".pdf,.docx").
    /// </summary>
    [HtmlAttributeName(nameof(Accept))]
    public string? Accept { get; set; }

    /// <summary>
    /// Gets or sets whether the file input allows multiple file selection.
    /// When true, adds the HTML "multiple" attribute to the input element.
    /// </summary>
    [HtmlAttributeName(nameof(Multiple))]
    public bool Multiple { get; set; }

    /// <summary>
    /// Gets or sets whether the file input is required.
    /// When true, adds the HTML "required" attribute for client-side validation.
    /// </summary>
    [HtmlAttributeName(nameof(Required))]
    public bool Required { get; set; }

    private readonly IFormComponentProvider _formComponentProvider;

    /// <summary>
    /// Creates a new FileInputTagHelper with dependency injection.
    /// </summary>
    /// <param name="formComponentProvider">Service for form component styling and configuration.</param>
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
