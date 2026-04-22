using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering HTML button and link elements with Bootstrap button styling, size variants, and loading states.
/// Targets the &lt;button&gt; and &lt;a&gt; elements when the Variant attribute is present and applies Bootstrap button CSS classes.
/// </summary>
/// <remarks>
/// Generates: &lt;button class="btn btn-{variant} btn-{size}" ...&gt;...&lt;/button&gt;
/// Or: &lt;a class="btn btn-{variant} btn-{size}" ...&gt;...&lt;/a&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .btn: Base button class required for all Bootstrap buttons
/// - .btn-{variant}: Color variant class applied based on the Variant property
///   * .btn-primary: Primary blue button (default action)
///   * .btn-secondary: Secondary gray button (default)
///   * .btn-success: Success green button
///   * .btn-danger: Danger red button
///   * .btn-warning: Warning yellow button
///   * .btn-info: Info cyan button
///   * .btn-light: Light gray button
///   * .btn-dark: Dark gray button
/// - .btn-{size}: Size modifier class based on Size property
///   * (default): Normal button size
///   * .btn-sm: Small button (reduced padding and font size)
///   * .btn-lg: Large button (increased padding and font size)
/// - .disabled: Added when IsLoading is true; disables user interaction
///
/// Button Types: Works with standard HTML button types (submit, reset, button) and anchor links.
/// Use on &lt;button type="submit"&gt; for form submission, &lt;button type="reset"&gt; to reset form fields,
/// or &lt;button type="button"&gt; for custom JavaScript actions. Can also be applied to &lt;a&gt; elements
/// to style links as buttons.
///
/// Loading State: When IsLoading is true, the button is automatically disabled and receives the .disabled class
/// to prevent multiple submissions. Useful for async operations like API calls or form submissions.
///
/// Usage: Apply to button and anchor elements with the Variant attribute. All other button attributes
/// (type, class, onclick, etc.) are preserved and merged with Bootstrap classes.
/// </remarks>
/// <example>
/// &lt;!-- Primary button (default submit button) --&gt;
/// &lt;button type="submit" variant="primary"&gt;Submit&lt;/button&gt;
/// &lt;!-- Generates: &lt;button class="btn btn-primary" type="submit"&gt;Submit&lt;/button&gt; --&gt;
///
/// &lt;!-- Small secondary button --&gt;
/// &lt;button variant="secondary" size="sm"&gt;Cancel&lt;/button&gt;
/// &lt;!-- Generates: &lt;button class="btn btn-secondary btn-sm"&gt;Cancel&lt;/button&gt; --&gt;
///
/// &lt;!-- Large danger button --&gt;
/// &lt;button variant="danger" size="lg"&gt;Delete&lt;/button&gt;
/// &lt;!-- Generates: &lt;button class="btn btn-danger btn-lg" disabled="disabled"&gt;Delete&lt;/button&gt; --&gt;
///
/// &lt;!-- Success button with loading state --&gt;
/// &lt;button variant="success" is-loading="true"&gt;Processing...&lt;/button&gt;
/// &lt;!-- Generates: &lt;button class="btn btn-success disabled" disabled="disabled"&gt;Processing...&lt;/button&gt; --&gt;
///
/// &lt;!-- Link styled as a button --&gt;
/// &lt;a href="/dashboard" variant="info"&gt;Go to Dashboard&lt;/a&gt;
/// &lt;!-- Generates: &lt;a class="btn btn-info" href="/dashboard"&gt;Go to Dashboard&lt;/a&gt; --&gt;
///
/// &lt;!-- Warning button with custom CSS class --&gt;
/// &lt;button variant="warning" class="mt-2"&gt;Warning Action&lt;/button&gt;
/// &lt;!-- Generates: &lt;button class="mt-2 btn btn-warning"&gt;Warning Action&lt;/button&gt; --&gt;
/// </example>
[HtmlTargetElement("button", Attributes = nameof(Variant))]
[HtmlTargetElement("a", Attributes = nameof(Variant))]
public class ButtonTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the button color variant from Bootstrap button variants.
    /// Maps to Bootstrap button CSS classes: primary, secondary, success, danger, warning, info, light, dark.
    /// Default is "secondary" (gray button).
    /// </summary>
    [HtmlAttributeName(nameof(Variant))]
    public string Variant { get; set; } = "secondary";

    /// <summary>
    /// Gets or sets the button size modifier: "sm" for small or "lg" for large.
    /// Default is null (normal size). Maps to Bootstrap .btn-sm and .btn-lg classes.
    /// </summary>
    [HtmlAttributeName(nameof(Size))]
    public string? Size { get; set; }

    /// <summary>
    /// Gets or sets whether the button is in a loading/disabled state.
    /// When true, adds the .disabled class and disabled attribute to prevent user interaction.
    /// Useful for indicating async operations in progress (form submission, API calls, etc.).
    /// Default is false.
    /// </summary>
    [HtmlAttributeName(nameof(IsLoading))]
    public bool IsLoading { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var variant = Variant switch
        {
            "primary" => "btn-primary",
            "secondary" => "btn-secondary",
            "danger" => "btn-danger",
            "success" => "btn-success",
            "warning" => "btn-warning",
            "info" => "btn-info",
            "light" => "btn-light",
            "dark" => "btn-dark",
            _ => "btn-secondary"
        };

        var sizeClass = Size switch
        {
            "sm" => "btn-sm",
            "lg" => "btn-lg",
            _ => ""
        };

        var classes = $"btn {variant}";
        if (!string.IsNullOrEmpty(sizeClass))
            classes += $" {sizeClass}";

        if (IsLoading)
        {
            classes += " disabled";
            output.Attributes.SetAttribute("disabled", "disabled");
        }

        if (output.Attributes.ContainsName("class"))
        {
            var existing = output.Attributes["class"].Value.ToString();
            output.Attributes.SetAttribute("class", $"{existing} {classes}");
        }
        else
        {
            output.Attributes.SetAttribute("class", classes);
        }
    }
}
