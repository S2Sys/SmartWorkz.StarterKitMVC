using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering Bootstrap breadcrumb navigation showing the user's current location in site hierarchy.
/// Targets the &lt;breadcrumb&gt; element and generates &lt;nav&gt;&lt;ol class="breadcrumb"&gt; navigation.
/// </summary>
/// <remarks>
/// Generates: &lt;nav aria-label="breadcrumb"&gt;&lt;ol class="breadcrumb"&gt;...&lt;/ol&gt;&lt;/nav&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .breadcrumb: Container for breadcrumb items with semantic &lt;ol&gt; element
/// - .breadcrumb-item: Individual breadcrumb item wrapper &lt;li&gt;
/// - .active: Applied to the last breadcrumb item (current page), no link
/// - aria-current="page": Applied to active item for accessibility
///
/// BreadcrumbItem Model Structure:
/// Each breadcrumb is represented by a BreadcrumbItem with:
/// - Label: Display text shown to the user
/// - Url: Hyperlink destination (can be null for current/active page)
/// - Last item is automatically marked as active (no link rendered)
///
/// Navigation Behavior:
/// - First N-1 items are rendered as clickable links pointing to their respective Urls
/// - Last item is rendered as inactive text (no link), marked with .active class
/// - Users can click any link to navigate to that level in the hierarchy
/// - Breadcrumbs typically follow a hierarchical path (e.g., Home &gt; Products &gt; Category &gt; Item)
///
/// Typical Usage:
/// - Display in page header or layout to show navigation path
/// - Populate Items from action context and route parameters
/// - Last item represents current page/view
/// - First item typically links to Home
///
/// Output Suppression:
/// - If Items list is empty, breadcrumb is not rendered (suppressed)
/// </remarks>
/// <example>
/// &lt;!-- Basic breadcrumb navigation --&gt;
/// &lt;breadcrumb items="new List&lt;BreadcrumbItem&gt; {
///   new() { Label = "Home", Url = "/" },
///   new() { Label = "Products", Url = "/products" },
///   new() { Label = "Electronics" }
/// }" /&gt;
///
/// &lt;!-- Breadcrumb from controller action --&gt;
/// &lt;breadcrumb items="Model.Breadcrumbs" /&gt;
///
/// &lt;!-- Programmatically constructed breadcrumb in view --&gt;
/// @{
///   var breadcrumbs = new List&lt;BreadcrumbItem&gt;
///   {
///     new() { Label = "Dashboard", Url = "/dashboard" },
///     new() { Label = "Reports", Url = "/reports" },
///     new() { Label = "Monthly Summary" }
///   };
/// }
/// &lt;breadcrumb items="breadcrumbs" /&gt;
/// </example>
[HtmlTargetElement("breadcrumb")]
public class BreadcrumbTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the list of BreadcrumbItem objects representing the navigation path.
    /// The last item in the list is automatically marked as active (current page).
    /// All other items are rendered as clickable links.
    /// If empty, the breadcrumb is not rendered.
    /// </summary>
    [HtmlAttributeName("items")]
    public List<BreadcrumbItem> Items { get; set; } = new();

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!Items.Any())
        {
            output.SuppressOutput();
            return;
        }

        var html = "<nav aria-label=\"breadcrumb\"><ol class=\"breadcrumb\">";

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var isActive = i == Items.Count - 1;

            if (isActive)
                html += $"<li class=\"breadcrumb-item active\" aria-current=\"page\">{item.Label}</li>";
            else
                html += $"<li class=\"breadcrumb-item\"><a href=\"{item.Url}\">{item.Label}</a></li>";
        }

        html += "</ol></nav>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
