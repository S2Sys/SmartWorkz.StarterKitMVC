namespace SmartWorkz.StarterKitMVC.Shared.DTOs;

/// <summary>
/// Generic API response wrapper for standard responses.
/// </summary>
public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message = null,
    List<string>? Errors = null);

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
