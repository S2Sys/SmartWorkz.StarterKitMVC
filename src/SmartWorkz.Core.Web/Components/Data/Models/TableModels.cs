namespace SmartWorkz.Core.Web.Components.Data.Models;

using System.Linq.Expressions;

/// <summary>Configuration for TableComponent.</summary>
public class TableOptions
{
    /// <summary>Number of rows per page (default 10).</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Enable row striping for better readability.</summary>
    public bool IsStriped { get; set; } = true;

    /// <summary>Enable hover effect on rows.</summary>
    public bool HasHoverEffect { get; set; } = true;

    /// <summary>Enable row selection with checkboxes.</summary>
    public bool AllowRowSelection { get; set; } = false;

    /// <summary>Enable multi-row selection (only if AllowRowSelection is true).</summary>
    public bool AllowMultiSelect { get; set; } = true;

    /// <summary>Show/hide table borders.</summary>
    public bool HasBorders { get; set; } = true;

    /// <summary>Make table responsive (horizontal scroll on mobile).</summary>
    public bool IsResponsive { get; set; } = true;

    /// <summary>Compact table density (smaller padding).</summary>
    public bool IsCompact { get; set; } = false;

    /// <summary>Empty state message when no data.</summary>
    public string EmptyMessage { get; set; } = "No data available.";

    /// <summary>Loading message while fetching data.</summary>
    public string LoadingMessage { get; set; } = "Loading...";
}

/// <summary>Column definition for table display.</summary>
public class TableColumn
{
    /// <summary>Column header text.</summary>
    public required string Title { get; set; }

    /// <summary>Property name or expression to bind data.</summary>
    public string? PropertyName { get; set; }

    /// <summary>Width as percentage or fixed (e.g., "20%", "200px").</summary>
    public string? Width { get; set; }

    /// <summary>Enable sorting on this column.</summary>
    public bool IsSortable { get; set; } = true;

    /// <summary>Enable filtering on this column.</summary>
    public bool IsFilterable { get; set; } = false;

    /// <summary>Text alignment (left, center, right).</summary>
    public string Align { get; set; } = "left";

    /// <summary>Custom CSS class for column.</summary>
    public string? CssClass { get; set; }

    /// <summary>Hide column on mobile (responsive).</summary>
    public bool HideOnMobile { get; set; } = false;

    /// <summary>Format function for cell values.</summary>
    public Func<object?, string>? Formatter { get; set; }
}

/// <summary>Table sort/filter request.</summary>
public class TableRequest
{
    /// <summary>Current page number (1-based).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Rows per page.</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Column name to sort by.</summary>
    public string? SortBy { get; set; }

    /// <summary>Sort in descending order.</summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>Filter value for search.</summary>
    public string? FilterValue { get; set; }

    /// <summary>Column to filter on.</summary>
    public string? FilterColumn { get; set; }
}

/// <summary>Paged result for table display.</summary>
public class PagedResult<T>
{
    /// <summary>Items for current page.</summary>
    public required List<T> Items { get; set; }

    /// <summary>Total count of all items (across all pages).</summary>
    public int TotalCount { get; set; }

    /// <summary>Total pages.</summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>Current page number.</summary>
    public int CurrentPage { get; set; }

    /// <summary>Items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Has more pages.</summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>Has previous pages.</summary>
    public bool HasPreviousPage => CurrentPage > 1;
}
