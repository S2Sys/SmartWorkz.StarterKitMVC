namespace SmartWorkz.Web;

/// <summary>
/// Stores view-specific configuration (visible columns, item layout, formatting rules).
/// Allows different views to display the same data differently.
/// </summary>
public class ViewConfiguration
{
    /// <summary>Column names to display in the view.</summary>
    public List<string> VisibleColumns { get; set; } = [];

    /// <summary>Number of items per row in card/grid layout (1, 2, 3, etc).</summary>
    public int ItemsPerRow { get; set; } = 2;

    /// <summary>Whether to show column headers (relevant for List view).</summary>
    public bool ShowHeaders { get; set; } = true;

    /// <summary>Custom CSS classes for card containers.</summary>
    public string CardCssClass { get; set; } = "card h-100";

    /// <summary>Whether to enable row checkboxes for selection.</summary>
    public bool AllowRowSelection { get; set; } = true;

    /// <summary>Default page size for pagination.</summary>
    public int DefaultPageSize { get; set; } = 20;
}
