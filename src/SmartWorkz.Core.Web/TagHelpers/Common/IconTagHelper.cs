using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering Bootstrap Icon library icons with size and color customization.
/// Targets the &lt;icon&gt; element and generates Bootstrap Icon HTML markup (&lt;i class="bi bi-{name}"&gt;&lt;/i&gt;).
/// </summary>
/// <remarks>
/// Generates: &lt;i class="bi bi-{name} {size} {cssClass}"&gt;&lt;/i&gt;
///
/// Icon Library: Bootstrap Icons (https://icons.getbootstrap.com/)
/// Icons are referenced by their IconType enum name (e.g., Success, Error, Search, Home, etc.)
/// and rendered as &lt;i&gt; elements with appropriate Bootstrap Icon CSS classes.
///
/// Icon Sizing:
/// - (default): Normal icon size (inherits font-size from context)
/// - Size="sm": Small size icon using Bootstrap utility class .me-1 (margin-right)
/// - Size="lg": Large size icon using Bootstrap utility class .fs-5 (font-size)
/// Size property controls spacing and visual prominence without requiring custom CSS.
///
/// Icon Colors: Icons inherit text color from parent element (default). Apply Bootstrap text color
/// utilities via the CssClass property for color variants:
/// - text-primary, text-danger, text-success, text-warning, text-info, text-muted, etc.
/// Examples: CssClass="text-primary" (blue), CssClass="text-danger" (red), CssClass="text-success" (green)
///
/// Icon Usage Patterns:
/// - Standalone icons for visual indicators (status icons, decorations)
/// - Icons in buttons: &lt;button&gt;&lt;icon name="Search" /&gt; Search&lt;/button&gt;
/// - Icons in form inputs: &lt;input-tag icon-prefix="Search" /&gt; (via InputTagHelper)
/// - Icons in navigation (header, sidebar, menu items)
/// - Semantic icons for better accessibility (Help, Warning, Success indicators)
///
/// Bootstrap CSS Classes Applied:
/// - .bi: Base Bootstrap Icon class
/// - .bi-{iconName}: Specific icon name (e.g., bi-search, bi-check-circle-fill)
/// - Size classes: .me-1 (small), .fs-5 (large) based on Size property
/// - Custom classes: Any Bootstrap utility classes passed via CssClass property
///
/// Integration: Works with IIconProvider service which maps IconType enum values to
/// Bootstrap Icon CSS classes and HTML markup. Resolves via dependency injection.
/// </remarks>
/// <example>
/// &lt;!-- Standalone success icon --&gt;
/// &lt;icon name="Success" /&gt;
/// &lt;!-- Generates: &lt;i class="bi bi-check-circle-fill"&gt;&lt;/i&gt; --&gt;
///
/// &lt;!-- Small icon with custom CSS class --&gt;
/// &lt;icon name="Info" size="sm" css-class="text-info" /&gt;
/// &lt;!-- Generates: &lt;i class="bi bi-info-circle me-1 text-info"&gt;&lt;/i&gt; --&gt;
///
/// &lt;!-- Large error icon in red --&gt;
/// &lt;icon name="Error" size="lg" css-class="text-danger" /&gt;
/// &lt;!-- Generates: &lt;i class="bi bi-exclamation-circle fs-5 text-danger"&gt;&lt;/i&gt; --&gt;
///
/// &lt;!-- Search icon in a button --&gt;
/// &lt;button type="button" class="btn btn-primary"&gt;
///   &lt;icon name="Search" size="sm" /&gt; Search
/// &lt;/button&gt;
///
/// &lt;!-- Warning icon with emphasis --&gt;
/// &lt;icon name="Warning" size="lg" css-class="text-warning me-2" /&gt;
/// &lt;span&gt;Please verify your information&lt;/span&gt;
///
/// &lt;!-- Home navigation icon --&gt;
/// &lt;a href="/"&gt;&lt;icon name="Home" /&gt; Home&lt;/a&gt;
///
/// &lt;!-- User account icon --&gt;
/// &lt;a href="/settings"&gt;&lt;icon name="User" size="sm" /&gt; Settings&lt;/a&gt;
/// </example>
[HtmlTargetElement("icon", Attributes = nameof(Name))]
public class IconTagHelper : TagHelper
{
    private readonly IIconProvider _iconProvider;

    /// <summary>
    /// Gets or sets the icon name from the IconType enum (e.g., "Success", "Error", "Search", "Home").
    /// Required attribute. Maps to a Bootstrap Icon class via IIconProvider.
    /// Supports: Success, Error, Warning, Info, CheckCircle, ExclamationTriangle, ExclamationCircle,
    /// InformationCircle, Search, Menu, Close, ChevronLeft, ChevronRight, ChevronUp, ChevronDown,
    /// User, Settings, Home, Logout, Plus, Minus, Edit, Delete, Save
    /// </summary>
    [HtmlAttributeName(nameof(Name))]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the icon size modifier: "sm" for small or "lg" for large.
    /// Default is null (normal size). Maps to Bootstrap utility classes for spacing/sizing.
    /// - sm: Adds .me-1 class (margin-right for spacing in buttons/inline contexts)
    /// - lg: Adds .fs-5 class (larger font-size)
    /// </summary>
    [HtmlAttributeName(nameof(Size))]
    public string? Size { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the icon element.
    /// Use for color variants (text-primary, text-danger, text-success) or custom spacing (me-2, ms-1).
    /// Classes are merged with icon and size classes.
    /// </summary>
    [HtmlAttributeName(nameof(CssClass))]
    public string? CssClass { get; set; }

    public IconTagHelper(IIconProvider iconProvider)
    {
        _iconProvider = iconProvider ?? throw new ArgumentNullException(nameof(iconProvider));
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        if (!Enum.TryParse<IconType>(Name, ignoreCase: true, out var iconType))
        {
            output.Content.SetHtmlContent($"<!-- Unknown icon: {Name} -->");
            return;
        }

        var sizeClass = Size switch
        {
            "sm" => "me-1",
            "lg" => "fs-5",
            _ => ""
        };

        var cssClass = string.IsNullOrEmpty(CssClass)
            ? sizeClass
            : $"{CssClass} {sizeClass}".Trim();

        var html = _iconProvider.GetIconHtml(iconType, cssClass);
        output.Content.SetHtmlContent(html);
    }
}
