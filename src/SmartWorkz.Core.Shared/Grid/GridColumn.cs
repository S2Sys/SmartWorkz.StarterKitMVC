namespace SmartWorkz.Shared;

/// <summary>
/// Defines a single column in a grid, including display options, sorting, filtering, and rendering hints.
/// </summary>
public class GridColumn
{
    /// <summary>The property name on the data object (maps to PagedQuery.SortBy).</summary>
    public string? PropertyName { get; set; }

    /// <summary>Display label for the column header.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Whether this column can be sorted by clicking the header.</summary>
    public bool IsSortable { get; set; } = true;

    /// <summary>Whether this column can be filtered.</summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>Whether this column supports inline editing (reserved for Phase 2).</summary>
    public bool IsEditable { get; set; } = false;

    /// <summary>
    /// Filter UI type: "text" (textbox), "dropdown" (select), "date" (date picker), "range" (min/max).
    /// Null means no filter UI.
    /// </summary>
    public string? FilterType { get; set; }

    /// <summary>CSS width (e.g., "20%", "200px"). Null means auto-width.</summary>
    public string? Width { get; set; }

    /// <summary>Custom cell rendering hint (e.g., "currency", "image", "badge"). UI implementation specific.</summary>
    public string? CellTemplate { get; set; }

    /// <summary>Display order (lower appears first). 0-based.</summary>
    public int Order { get; set; }

    /// <summary>Whether this column is visible (supports show/hide toggle).</summary>
    public bool IsVisible { get; set; } = true;
}
