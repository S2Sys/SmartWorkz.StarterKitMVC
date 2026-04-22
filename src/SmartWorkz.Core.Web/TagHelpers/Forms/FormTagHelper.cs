namespace SmartWorkz.Web;

using Microsoft.AspNetCore.Razor.TagHelpers;

/// <summary>
/// TagHelper for rendering HTML form elements with Bootstrap validation styling and class management.
/// Targets the &lt;form-tag&gt; element and generates &lt;form class="needs-validation"&gt;.
/// </summary>
/// <remarks>
/// Generates: &lt;form class="needs-validation" method="post" action="..." novalidate=""&gt;...&lt;/form&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .needs-validation: Applied to the form element to enable Bootstrap form validation styling
/// - Additional custom classes can be added via the class attribute
///
/// Validation: The needs-validation class works with Bootstrap's validation CSS to display validation
/// feedback. Form inputs and textareas with validation errors automatically display .is-invalid state.
/// The form element supports the novalidate attribute to disable HTML5 client-side validation if needed.
///
/// Usage: Wrap all form inputs (input-tag, select-tag, textarea-tag, etc.) within this form element.
/// Typically uses FormGroupTagHelper to structure each form field with label, input, help text, and errors.
/// </remarks>
/// <example>
/// &lt;!-- Simple form with default POST method --&gt;
/// &lt;form-tag&gt;
///   &lt;form-group for="Model.Name" label="Full Name" required="true"&gt;
///     &lt;input-tag for="Model.Name" placeholder="Enter your name" /&gt;
///   &lt;/form-group&gt;
///   &lt;button type="submit" class="btn btn-primary"&gt;Submit&lt;/button&gt;
/// &lt;/form-tag&gt;
///
/// &lt;!-- Form with custom action and GET method --&gt;
/// &lt;form-tag method="get" action="/search"&gt;
///   &lt;input-tag for="Model.SearchTerm" placeholder="Search..." /&gt;
///   &lt;button type="submit" class="btn btn-primary"&gt;Search&lt;/button&gt;
/// &lt;/form-tag&gt;
///
/// &lt;!-- Form with custom CSS class and validation disabled --&gt;
/// &lt;form-tag class="login-form" novalidate="true"&gt;
///   &lt;form-group for="Model.Email" label="Email" required="true"&gt;
///     &lt;input-tag for="Model.Email" type="email" /&gt;
///   &lt;/form-group&gt;
///   &lt;button type="submit" class="btn btn-primary"&gt;Login&lt;/button&gt;
/// &lt;/form-tag&gt;
/// </example>
[HtmlTargetElement("form-tag")]
public class FormTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the HTTP method for the form.
    /// Maps to the HTML method attribute. Default is "post".
    /// Common values: "post", "get"
    /// </summary>
    [HtmlAttributeName("method")]
    public string Method { get; set; } = "post";

    /// <summary>
    /// Gets or sets the action URL for the form submission.
    /// Maps to the HTML action attribute.
    /// If empty, the form submits to the current page.
    /// </summary>
    [HtmlAttributeName("action")]
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets whether to disable HTML5 form validation.
    /// When true, adds the HTML "novalidate" attribute to suppress browser validation.
    /// Default is false (validation is enabled).
    /// </summary>
    [HtmlAttributeName("novalidate")]
    public bool NoValidate { get; set; } = false;

    /// <summary>
    /// Gets or sets additional CSS classes for the form element.
    /// These classes are appended to the required "needs-validation" class.
    /// </summary>
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
