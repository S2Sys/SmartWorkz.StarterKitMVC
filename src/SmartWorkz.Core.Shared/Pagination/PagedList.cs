namespace SmartWorkz.Core.Shared.Pagination;

/// <summary>
/// A page of items with metadata.
/// Replaces PaginationResponse&lt;T&gt; in StarterKitMVC.Shared.DTOs.
///
/// Migration path: PaginationResponse&lt;T&gt; has the same fields under different names.
/// PagedList&lt;T&gt;.Create() is a drop-in replacement for PaginationResponse&lt;T&gt;.Create().
/// </summary>
public sealed class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    private PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (decimal)pageSize) : 0;
    }

    public static PagedList<T> Create(IEnumerable<T> items, int page, int pageSize, int totalCount)
        => new(items.ToList().AsReadOnly(), page, pageSize, totalCount);

    /// <summary>Create an empty result set (e.g., when no rows match).</summary>
    public static PagedList<T> Empty(int pageSize = 20)
        => new([], 1, pageSize, 0);

    /// <summary>Project items to a different type without changing pagination metadata.</summary>
    public PagedList<TOut> Map<TOut>(Func<T, TOut> selector)
        => new(Items.Select(selector).ToList().AsReadOnly(), Page, PageSize, TotalCount);
}
