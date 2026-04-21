
namespace SmartWorkz.Shared;

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
    : PagedQuery(Page, PageSize, SortBy, SortDescending, SearchTerm);
