namespace SmartWorkz.Core.Shared.Pagination;

/// <summary>
/// Standard request parameters for any paginated query.
/// Replaces the existing PaginationRequest record in StarterKitMVC.Shared.DTOs.
///
/// Migration: PaginationRequest is structurally identical — alias it or replace it.
/// </summary>
public record PagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false,
    string? SearchTerm = null)
{
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;

    /// <summary>Clamp page and pageSize to safe bounds.</summary>
    public PagedQuery Normalize(int maxPageSize = 100)
        => this with
        {
            Page = Math.Max(1, Page),
            PageSize = Math.Clamp(PageSize, 1, maxPageSize)
        };
}
