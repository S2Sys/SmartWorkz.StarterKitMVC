using SmartWorkz.Core.Shared.Pagination;

namespace SmartWorkz.Core.Shared.Grid;

/// <summary>
/// Request parameters for grid data fetching, extending PagedQuery with filtering support.
/// </summary>
public record GridRequest(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false,
    string? SearchTerm = null,
    Dictionary<string, object>? Filters = null)
    : PagedQuery(Page, PageSize, SortBy, SortDescending, SearchTerm)
{
    /// <summary>
    /// Column-specific filters. Key is property name, value is filter criteria.
    /// Example: { "Status": "Active", "DateRange": "2024-01-01,2024-12-31" }
    /// </summary>
    public Dictionary<string, object>? Filters { get; } = Filters;
}
