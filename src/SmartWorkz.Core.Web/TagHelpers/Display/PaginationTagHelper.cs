using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Core.Web.TagHelpers.Display;

[HtmlTargetElement("pagination")]
public class PaginationTagHelper : TagHelper
{
    [HtmlAttributeName("current-page")]
    public int CurrentPage { get; set; } = 1;

    [HtmlAttributeName("total-pages")]
    public int TotalPages { get; set; } = 1;

    [HtmlAttributeName("page-url")]
    public string? PageUrl { get; set; } = "?page={0}";

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
