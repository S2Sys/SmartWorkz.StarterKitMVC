namespace SmartWorkz.StarterKitMVC.Shared.Models;

/// <summary>
/// Pagination state passed to the _Pagination partial.
/// Build via PaginationModel.From(total, page, pageSize) in BaseListPage.
///
/// Renders: First | Prev | 1 2 [3] 4 5 | Next | Last
/// Compact window: shows MaxPageButtons pages around current, with ellipsis.
/// </summary>
public class PaginationModel
{
    public int  CurrentPage  { get; init; }
    public int  PageSize     { get; init; }
    public int  TotalItems   { get; init; }
    public int  TotalPages   => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrev      => CurrentPage > 1;
    public bool HasNext      => CurrentPage < TotalPages;
    public int  FirstItem    => TotalItems == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int  LastItem     => Math.Min(CurrentPage * PageSize, TotalItems);

    /// <summary>How many page buttons to show around the current page (default 5).</summary>
    public int MaxPageButtons { get; init; } = 5;

    /// <summary>Route values to merge into paging links (e.g. search query, sort column).</summary>
    public Dictionary<string, string?> RouteValues { get; init; } = [];

    /// <summary>HTMX target selector for paged updates (null = full page navigation).</summary>
    public string? HtmxTarget { get; init; }

    /// <summary>HTMX handler name for partial result (e.g. "Table").</summary>
    public string? HtmxHandler { get; init; }

    public static PaginationModel From(int total, int page, int pageSize,
        Dictionary<string, string?>? routeValues = null,
        string? htmxTarget = null, string? htmxHandler = null)
        => new()
        {
            TotalItems  = total,
            CurrentPage = page,
            PageSize    = pageSize,
            RouteValues = routeValues ?? [],
            HtmxTarget  = htmxTarget,
            HtmxHandler = htmxHandler
        };

    /// <summary>
    /// Factory method: convert pagination count and current page to PaginationModel for rendering.
    /// Use this when mapping DTO responses to view models in Razor Pages.
    /// Example: PaginationModel.FromDto(result.TotalCount, result.CurrentPage, pageSize);
    /// </summary>
    public static PaginationModel FromDto(int totalCount, int currentPage, int pageSize,
        Dictionary<string, string?>? routeValues = null,
        string? htmxTarget = null, string? htmxHandler = null)
        => new()
        {
            TotalItems  = totalCount,
            CurrentPage = currentPage,
            PageSize    = pageSize,
            RouteValues = routeValues ?? [],
            HtmxTarget  = htmxTarget,
            HtmxHandler = htmxHandler
        };

    /// <summary>Returns the visible page numbers to render, including -1 as ellipsis sentinel.</summary>
    public IEnumerable<int> PageWindow()
    {
        if (TotalPages <= MaxPageButtons)
        {
            for (var i = 1; i <= TotalPages; i++) yield return i;
            yield break;
        }

        var half  = MaxPageButtons / 2;
        var start = Math.Max(1, CurrentPage - half);
        var end   = Math.Min(TotalPages, start + MaxPageButtons - 1);

        if (end - start < MaxPageButtons - 1)
            start = Math.Max(1, end - MaxPageButtons + 1);

        if (start > 1) { yield return 1; if (start > 2) yield return -1; }

        for (var i = start; i <= end; i++) yield return i;

        if (end < TotalPages) { if (end < TotalPages - 1) yield return -1; yield return TotalPages; }
    }
}
