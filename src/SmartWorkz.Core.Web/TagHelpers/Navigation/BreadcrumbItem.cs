namespace SmartWorkz.Web;

/// <summary>
/// Data model representing a single item in a breadcrumb navigation trail.
/// Used with BreadcrumbTagHelper to construct hierarchical navigation paths.
/// </summary>
/// <remarks>
/// The BreadcrumbItem represents one level in the site hierarchy within a breadcrumb.
/// Breadcrumb trails guide users through the information architecture, showing:
/// - Where they are in the site (current/active item, last in the list)
/// - How they got there (previous items as navigation links)
/// - How to navigate back up the hierarchy (clickable parent links)
///
/// Typical Usage Pattern:
/// 1. Create a list of BreadcrumbItem objects representing the navigation path
/// 2. Set Label for each item (visible text)
/// 3. Set Url for all items except the last (which represents current page)
/// 4. Pass the list to BreadcrumbTagHelper via items attribute
/// 5. Last item is automatically marked as active (no link, .active class)
///
/// Example Hierarchy:
/// Home (Url="/") > Products (Url="/products") > Category (Url="/category/electronics") > Item (Url=null)
/// The Item would be the current page with no link, others are clickable.
///
/// Creating Breadcrumbs:
/// - From route parameters: Extract category, subcategory from route and build list
/// - From action context: Use controller/action names to generate breadcrumb
/// - Statically in view: Hard-code common navigation paths in layout
/// - Dynamically: Construct from model or database hierarchies
/// </remarks>
/// <example>
/// &lt;!-- Create breadcrumb items for product details page --&gt;
/// @{
///   var breadcrumbs = new List&lt;BreadcrumbItem&gt;
///   {
///     new() { Label = "Home", Url = "/" },
///     new() { Label = "Products", Url = "/products" },
///     new() { Label = "Electronics", Url = "/products/electronics" },
///     new() { Label = "Laptops", Url = "/products/electronics/laptops" },
///     new() { Label = "Dell XPS 13" }  // No URL - this is the current page
///   };
/// }
/// &lt;breadcrumb items="breadcrumbs" /&gt;
///
/// &lt;!-- Create from dynamic data --&gt;
/// @{
///   var breadcrumbs = new List&lt;BreadcrumbItem&gt;
///   {
///     new() { Label = "Dashboard", Url = "/dashboard" }
///   };
///
///   foreach (var folder in Model.FolderHierarchy)
///   {
///     breadcrumbs.Add(new()
///     {
///       Label = folder.Name,
///       Url = folder.Depth &lt; Model.FolderHierarchy.Count - 1 ? folder.Url : null
///     });
///   }
/// }
/// &lt;breadcrumb items="breadcrumbs" /&gt;
/// </example>
public class BreadcrumbItem
{
    /// <summary>
    /// Gets or sets the display text for this breadcrumb item.
    /// Shown to the user in the breadcrumb navigation.
    /// Examples: "Home", "Products", "Electronics", "My Orders"
    /// Default is empty string.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL/hyperlink for this breadcrumb item.
    /// When set, the breadcrumb item is rendered as a clickable link.
    /// When null, the item is rendered as inactive text (typically for the current/active page).
    /// Should be set for all items except the last item in the breadcrumb list.
    /// Examples: "/", "/products", "/products/electronics"
    /// Default is null.
    /// </summary>
    public string? Url { get; set; }
}
