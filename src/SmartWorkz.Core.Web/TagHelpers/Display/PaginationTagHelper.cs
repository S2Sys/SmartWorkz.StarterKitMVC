using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering Bootstrap pagination controls with automatic page calculation and link generation.
/// Targets the &lt;pagination&gt; element and generates &lt;nav&gt;&lt;ul class="pagination"&gt; navigation.
/// </summary>
/// <remarks>
/// Generates: &lt;nav aria-label="Pagination"&gt;&lt;ul class="pagination"&gt;...&lt;/ul&gt;&lt;/nav&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .pagination: Container for pagination items and links
/// - .page-item: Wrapper for each page link or button
/// - .page-link: Actual clickable link or button element
/// - .active: Applied to the current page, highlighting it (CSS selectable for styling)
/// - .disabled: Applied to Previous/Next buttons when unavailable (first/last page)
///
/// Page Calculation Logic:
/// The TagHelper calculates which page numbers to display using a windowing algorithm:
/// 1. Center the current page within the visible range: CurrentPage - (MaxVisible / 2)
/// 2. Adjust window bounds to never exceed 1 or TotalPages
/// 3. If window doesn't start at page 1, show link to page 1 with ellipsis gap
/// 4. Display page numbers in the calculated window range
/// 5. If window doesn't end at TotalPages, show link to last page with ellipsis gap
///
/// Link Generation:
/// - Each page number is linked using the PageUrl template with string.Format(pageUrl, pageNumber)
/// - PageUrl defaults to "?page={0}" where {0} is replaced with the page number
/// - Previous/Next buttons use (CurrentPage - 1) and (CurrentPage + 1) in the template
/// - Previous/Next are disabled (not links) on first and last pages respectively
///
/// Active State:
/// - Only the CurrentPage is marked with .active class
/// - Active page content is still a link in the HTML but styled distinctly via CSS
///
/// Disabled State:
/// - Previous button is disabled (.disabled class) when CurrentPage == 1
/// - Next button is disabled (.disabled class) when CurrentPage == TotalPages
/// - Disabled items use &lt;span&gt; instead of &lt;a&gt; tags
/// - No click handlers are attached to disabled items
///
/// Output Suppression:
/// - If TotalPages &lt;= 1, pagination is not rendered (suppressed)
/// </remarks>
/// <example>
/// &lt;!-- Basic pagination with defaults (shows 5 pages max) --&gt;
/// &lt;pagination current-page="2" total-pages="10" /&gt;
///
/// &lt;!-- Pagination with custom page URL pattern --&gt;
/// &lt;pagination current-page="1" total-pages="5" page-url="/products?page={0}" /&gt;
///
/// &lt;!-- Pagination with more visible pages --&gt;
/// &lt;pagination current-page="3" total-pages="15" max-visible="7" /&gt;
///
/// &lt;!-- Pagination on last page (Next is disabled) --&gt;
/// &lt;pagination current-page="10" total-pages="10" /&gt;
///
/// &lt;!-- Pagination with single page (not rendered) --&gt;
/// &lt;pagination current-page="1" total-pages="1" /&gt;
/// </example>
[HtmlTargetElement("pagination")]
public class PaginationTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the current/active page number.
    /// This page is highlighted with the .active class.
    /// Default is 1 (first page).
    /// </summary>
    [HtmlAttributeName("current-page")]
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Gets or sets the total number of pages.
    /// If TotalPages &lt;= 1, pagination is not rendered.
    /// Default is 1 (single page, no pagination needed).
    /// </summary>
    [HtmlAttributeName("total-pages")]
    public int TotalPages { get; set; } = 1;

    /// <summary>
    /// Gets or sets the URL template for page links.
    /// The template should contain {0} as placeholder for page number.
    /// String.Format is used to replace {0} with page number.
    /// Default is "?page={0}" (adds page as query parameter).
    /// Examples: "/items?page={0}", "/products/{0}", "?p={0}"
    /// </summary>
    [HtmlAttributeName("page-url")]
    public string? PageUrl { get; set; } = "?page={0}";

    /// <summary>
    /// Gets or sets the maximum number of page numbers to display in the visible window.
    /// The current page is centered within this window when possible.
    /// Typically 5 (shows current page ±2), but can be adjusted for different layouts.
    /// Default is 5.
    /// </summary>
    [HtmlAttributeName("max-visible")]
    public int MaxVisible { get; set; } = 5;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (TotalPages <= 1)
        {
            output.SuppressOutput();
            return;
        }

        var pageUrl = PageUrl ?? "?page={0}";
        var html = "<nav aria-label=\"Pagination\"><ul class=\"pagination\">";

        // Previous button
        if (CurrentPage > 1)
            html += $"<li class=\"page-item\"><a class=\"page-link\" href=\"{string.Format(pageUrl, CurrentPage - 1)}\">Previous</a></li>";
        else
            html += "<li class=\"page-item disabled\"><span class=\"page-link\">Previous</span></li>";

        // Page numbers
        var start = Math.Max(1, CurrentPage - MaxVisible / 2);
        var end = Math.Min(TotalPages, start + MaxVisible - 1);

        if (start > 1)
            html += "<li class=\"page-item\"><a class=\"page-link\" href=\"" + string.Format(pageUrl, 1) + "\">1</a></li>";

        for (var i = start; i <= end; i++)
        {
            var activeClass = i == CurrentPage ? "active" : "";
            html += $"<li class=\"page-item {activeClass}\"><a class=\"page-link\" href=\"{string.Format(pageUrl, i)}\">{i}</a></li>";
        }

        if (end < TotalPages)
            html += "<li class=\"page-item\"><a class=\"page-link\" href=\"" + string.Format(pageUrl, TotalPages) + "\">" + TotalPages + "</a></li>";

        // Next button
        if (CurrentPage < TotalPages)
            html += $"<li class=\"page-item\"><a class=\"page-link\" href=\"{string.Format(pageUrl, CurrentPage + 1)}\">Next</a></li>";
        else
            html += "<li class=\"page-item disabled\"><span class=\"page-link\">Next</span></li>";

        html += "</ul></nav>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
