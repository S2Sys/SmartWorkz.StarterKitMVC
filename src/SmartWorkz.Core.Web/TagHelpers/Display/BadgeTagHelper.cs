using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering Bootstrap badge components for labels, counts, and status indicators.
/// Targets the &lt;badge&gt; element and generates &lt;span class="badge bg-{type}"&gt;.
/// </summary>
/// <remarks>
/// Generates: &lt;span class="badge bg-{type}"&gt;{text}&lt;/span&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .badge: Base badge styling with padding, border-radius, and inline display
/// - .bg-primary: Blue badge background (Type="primary")
/// - .bg-success: Green badge background (Type="success")
/// - .bg-danger: Red badge background (Type="danger")
/// - .bg-warning: Yellow/orange badge background (Type="warning")
/// - .bg-info: Light blue badge background (Type="info")
/// - .bg-light: Light gray badge background (Type="light") with .text-dark for contrast
/// - .bg-dark: Dark gray/black badge background (Type="dark")
/// - .bg-secondary: Gray badge background (Type="secondary", default)
///
/// Common Use Cases:
/// - Count badges: Display item counts, notification counts, or category counts
/// - Status badges: Show status indicators (Active, Inactive, Pending, etc.)
/// - Label badges: Tag items with category labels or keywords
/// - Badge Pills: Add .rounded-pill class for pill-shaped badges (via CSS)
///
/// Default Type: If Type is not specified or is an unknown value, "secondary" type is used.
/// </remarks>
/// <example>
/// &lt;!-- Simple primary badge --&gt;
/// &lt;badge type="primary" text="New" /&gt;
///
/// &lt;!-- Count badge --&gt;
/// &lt;badge type="success" text="5 items" /&gt;
///
/// &lt;!-- Danger badge for inactive status --&gt;
/// &lt;badge type="danger" text="Inactive" /&gt;
///
/// &lt;!-- Warning badge --&gt;
/// &lt;badge type="warning" text="Pending Review" /&gt;
///
/// &lt;!-- Default secondary badge --&gt;
/// &lt;badge text="Badge" /&gt;
/// </example>
[HtmlTargetElement("badge", Attributes = nameof(Type))]
public class BadgeTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the badge type/color, which determines the Bootstrap background class.
    /// Supported values: "primary", "secondary", "success", "danger", "warning", "info", "light", "dark".
    /// Default is "secondary".
    /// Maps to Bootstrap background classes: .bg-primary, .bg-secondary, .bg-success, .bg-danger, .bg-warning, .bg-info, .bg-light, .bg-dark
    /// </summary>
    [HtmlAttributeName(nameof(Type))]
    public string Type { get; set; } = "secondary";

    /// <summary>
    /// Gets or sets the text content displayed in the badge.
    /// Can contain counts, labels, status values, or other short text.
    /// </summary>
    [HtmlAttributeName(nameof(Text))]
    public string? Text { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var badgeClass = Type switch
        {
            "primary" => "bg-primary",
            "success" => "bg-success",
            "danger" => "bg-danger",
            "warning" => "bg-warning",
            "info" => "bg-info",
            "light" => "bg-light text-dark",
            "dark" => "bg-dark",
            _ => "bg-secondary"
        };

        var html = $"<span class=\"badge {badgeClass}\">{Text}</span>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
