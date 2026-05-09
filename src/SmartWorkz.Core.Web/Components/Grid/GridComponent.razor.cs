using Microsoft.AspNetCore.Components;
using SmartWorkz.Shared;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

/// <summary>
/// A generic, high-performance data grid component for displaying, sorting, filtering, and paginating tabular data.
/// Supports virtualization for datasets of 10K+ rows, row selection, and custom row templates.
/// </summary>
/// <typeparam name="T">The data type to display in the grid (must be a class).</typeparam>
/// <remarks>
/// This component integrates with DataContext{T} for data management and GridStateManager for state handling.
/// For large datasets (10K+ rows), enable virtualization to improve rendering performance.
/// Columns are configured via the Columns parameter using GridColumn definitions.
///
/// Features:
/// - Sorting: Click column headers to sort (if IsSortable is true)
/// - Pagination: Navigate between pages via the paging controls
/// - Virtual scrolling: For 10K+ row datasets, reduces DOM elements and improves performance
/// - Row selection: Enable checkboxes for multi-select operations
/// - Custom row rendering: Use RowTemplate parameter for custom cell rendering
/// </remarks>
/// <example>
/// <code>
/// &lt;GridComponent TItem="Product" DataContext="productContext" Columns="gridColumns" AllowRowSelection="true"&gt;
///   &lt;RowTemplate Context="product"&gt;
///     &lt;td&gt;@product.Name&lt;/td&gt;
///     &lt;td&gt;@product.Price&lt;/td&gt;
///   &lt;/RowTemplate&gt;
/// &lt;/GridComponent&gt;
/// </code>
/// </example>
public partial class GridComponent<T> : ComponentBase, IAsyncDisposable where T : class
{
    /// <summary>
    /// Gets or sets the DataContext that provides data, paging, and filtering logic.
    /// </summary>
    [Parameter]
    public IDataContext<T>? DataContext { get; set; }

    /// <summary>
    /// Gets or sets the collection of GridColumn definitions that define which properties to display
    /// and how each column should be rendered (sorting, filtering, width, etc.).
    /// </summary>
    [Parameter]
    public List<GridColumn> Columns { get; set; } = [];

    /// <summary>
    /// Gets or sets optional custom CSS classes to apply to the grid wrapper element.
    /// </summary>
    [Parameter]
    public string? CustomCssClass { get; set; }

    /// <summary>
    /// Gets or sets an optional RenderFragment for custom row rendering.
    /// When provided, this template is used instead of the default cell rendering logic.
    /// </summary>
    [Parameter]
    public RenderFragment<T>? RowTemplate { get; set; }

    /// <summary>Enable virtual scrolling for large datasets (10K+ rows)</summary>
    [Parameter]
    public bool EnableVirtualization { get; set; } = false;

    /// <summary>Row count threshold to enable virtualization (default 10,000)</summary>
    [Parameter]
    public int VirtualizationThreshold { get; set; } = 10_000;

    /// <summary>Height of each row in pixels for virtualization (default 40px)</summary>
    [Parameter]
    public int ItemHeight { get; set; } = 40;

    /// <summary>Container height in pixels for virtualized grid</summary>
    [Parameter]
    public int ContainerHeight { get; set; } = 600;

    /// <summary>Allow row selection via checkboxes</summary>
    [Parameter]
    public bool AllowRowSelection { get; set; } = false;

    /// <summary>All data (used for virtualization)</summary>
    public List<T>? AllData { get; set; }

    /// <summary>State manager for grid (pagination, sorting, filtering)</summary>
    protected GridStateManager? StateManager { get; set; }

    /// <summary>Current page data for pagination</summary>
    protected List<T>? CurrentPageData { get; set; }

    /// <summary>Current response from data context</summary>
    protected GridResponse<T>? CurrentResponse { get; set; }

    protected List<GridColumn> VisibleColumns => Columns.Where(c => c.IsVisible).ToList();

    /// <summary>
    /// Initializes the grid component and subscribes to DataContext state changes.
    /// </summary>
    protected override void OnInitialized()
    {
        if (DataContext != null)
        {
            DataContext.OnStateChanged += StateHasChanged;
        }
    }

    /// <summary>
    /// Handles sort request when a column header is clicked.
    /// Toggles sort direction if the same column is clicked again; otherwise, resets to ascending.
    /// </summary>
    /// <param name="propertyName">The property name of the column being sorted.</param>
    protected async Task OnSortClick(string propertyName)
    {
        if (DataContext == null)
            return;

        var column = Columns.FirstOrDefault(c => c.PropertyName == propertyName);
        if (column?.IsSortable != true)
            return;

        var isCurrentSort = DataContext.CurrentRequest.SortBy == propertyName;
        var newDescending = isCurrentSort ? !DataContext.CurrentRequest.SortDescending : false;

        await DataContext.UpdateSort(propertyName, newDescending);
    }

    /// <summary>
    /// Handles page change requests from pagination controls.
    /// </summary>
    /// <param name="pageNumber">The page number to navigate to (1-based).</param>
    protected async Task OnPageChange(int pageNumber)
    {
        if (DataContext != null)
        {
            await DataContext.UpdatePagination(pageNumber, DataContext.CurrentRequest.PageSize);
        }
    }

    /// <summary>
    /// Toggles the selection state of a single row.
    /// </summary>
    /// <param name="rowId">The unique identifier of the row to select/deselect.</param>
    /// <param name="isChecked">True to select the row; false to deselect.</param>
    protected void OnRowSelect(object rowId, bool isChecked)
    {
        if (DataContext != null)
        {
            DataContext.ToggleRowSelection(rowId);
        }
    }

    /// <summary>
    /// Toggles selection state for all rows in the current page or dataset.
    /// </summary>
    /// <param name="e">The checkbox ChangeEventArgs containing the checked state.</param>
    protected async Task SelectAllRows(ChangeEventArgs e)
    {
        if (DataContext != null)
        {
            var isChecked = (bool?)e.Value ?? false;
            DataContext.ToggleSelectAll(isChecked);
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Extracts the unique identifier from a data row by reading its first property.
    /// </summary>
    /// <param name="item">The data item to extract the ID from.</param>
    /// <returns>The ID value from the first property, or the item itself if no ID property exists.</returns>
    protected object GetRowId(T item)
    {
        var firstProperty = typeof(T).GetProperties().FirstOrDefault();
        return firstProperty?.GetValue(item) ?? item;
    }

    /// <summary>
    /// Determines whether a given row ID is currently selected.
    /// </summary>
    /// <param name="rowId">The row ID to check.</param>
    /// <returns>True if the row is selected; otherwise, false.</returns>
    protected bool IsRowSelected(object rowId)
    {
        return DataContext?.SelectedRowIds.Contains(rowId) ?? false;
    }

    /// <summary>
    /// Renders the content of a grid cell using either a custom row template or reflection-based property access.
    /// </summary>
    /// <param name="item">The data item being rendered.</param>
    /// <param name="column">The grid column definition for this cell.</param>
    /// <returns>A RenderFragment that renders the cell content.</returns>
    protected RenderFragment RenderCellContent(T item, GridColumn column)
    {
        return builder =>
        {
            var property = typeof(T).GetProperty(column.PropertyName);
            var value = property?.GetValue(item);

            if (RowTemplate != null)
            {
                builder.AddContent(0, RowTemplate(item));
            }
            else
            {
                builder.AddContent(0, value?.ToString() ?? "-");
            }
        };
    }

    /// <summary>
    /// Generates inline CSS styles for a grid column based on its Width property.
    /// </summary>
    /// <param name="column">The grid column to generate styles for.</param>
    /// <returns>A CSS style string (e.g., "width: 200px;"), or empty string if no width is defined.</returns>
    protected string GetColumnStyle(GridColumn column)
    {
        var style = "";
        if (!string.IsNullOrEmpty(column.Width))
            style += $"width: {column.Width};";
        return style;
    }

    /// <summary>
    /// Disposes the grid component and unsubscribes from DataContext state change notifications.
    /// </summary>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (DataContext != null)
        {
            DataContext.OnStateChanged -= StateHasChanged;
        }
        await ValueTask.CompletedTask;
    }
}

