using SmartWorkz.Core.Shared.Pagination;

namespace SmartWorkz.Core.Shared.Grid;

/// <summary>
/// Response from a grid data request, including paged data, column metadata, and filter options.
/// </summary>
public class GridResponse<T>
{
    /// <summary>The paged list of items.</summary>
    public required PagedList<T> Data { get; set; }

    /// <summary>Column definitions (may differ from request if server applies defaults).</summary>
    public List<GridColumn> Columns { get; set; } = [];

    /// <summary>
    /// Optional pre-computed filter options for dropdowns, grouped by column name.
    /// Example: { "Status": ["Active", "Inactive", "Pending"] }
    /// </summary>
    public Dictionary<string, List<object>>? FilterOptions { get; set; }
}
