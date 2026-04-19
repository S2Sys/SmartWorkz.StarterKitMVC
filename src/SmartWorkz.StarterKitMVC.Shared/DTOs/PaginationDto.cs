namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

/// <summary>
/// Request for paginated list queries.
/// </summary>
public record PaginationRequest(
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    string? SearchTerm = null);

/// <summary>
/// Response wrapper for paginated list results.
/// </summary>
public record PaginationResponse<T>(
    List<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    /// <summary>
    /// Indicates if there are more pages available.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Indicates if there are previous pages available.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Creates a paginated response.
    /// </summary>
    public static PaginationResponse<T> Create(
        List<T> data,
        int page,
        int pageSize,
        int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (decimal)pageSize);
        return new(data, page, pageSize, totalCount, totalPages);
    }
}

/// <summary>
/// Alias for PaginationResponse to match common naming convention.
/// </summary>
public record PaginatedResponse<T>(
    List<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages) : PaginationResponse<T>(Data, Page, PageSize, TotalCount, TotalPages)
{
    /// <summary>
    /// Creates a paginated response.
    /// </summary>
    public static new PaginatedResponse<T> Create(
        List<T> data,
        int page,
        int pageSize,
        int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (decimal)pageSize);
        return new(data, page, pageSize, totalCount, totalPages);
    }
}
