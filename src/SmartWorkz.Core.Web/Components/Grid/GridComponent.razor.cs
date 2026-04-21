using Microsoft.AspNetCore.Components;
using SmartWorkz.Shared.Grid;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

public partial class GridComponent<T> : ComponentBase, IAsyncDisposable where T : class
{
    [Parameter]
    public IDataContext<T>? DataContext { get; set; }

    [Parameter]
    public List<GridColumn> Columns { get; set; } = [];

    [Parameter]
    public string? CustomCssClass { get; set; }

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

    protected override void OnInitialized()
    {
        if (DataContext != null)
        {
            DataContext.OnStateChanged += StateHasChanged;
        }
    }

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

    protected async Task OnPageChange(int pageNumber)
    {
        if (DataContext != null)
        {
            await DataContext.UpdatePagination(pageNumber, DataContext.CurrentRequest.PageSize);
        }
    }

    protected void OnRowSelect(object rowId, bool isChecked)
    {
        if (DataContext != null)
        {
            DataContext.ToggleRowSelection(rowId);
        }
    }

    protected async Task SelectAllRows(ChangeEventArgs e)
    {
        if (DataContext != null)
        {
            var isChecked = (bool?)e.Value ?? false;
            DataContext.ToggleSelectAll(isChecked);
        }
        await Task.CompletedTask;
    }

    protected object GetRowId(T item)
    {
        var firstProperty = typeof(T).GetProperties().FirstOrDefault();
        return firstProperty?.GetValue(item) ?? item;
    }

    protected bool IsRowSelected(object rowId)
    {
        return DataContext?.SelectedRowIds.Contains(rowId) ?? false;
    }

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

    protected string GetColumnStyle(GridColumn column)
    {
        var style = "";
        if (!string.IsNullOrEmpty(column.Width))
            style += $"width: {column.Width};";
        return style;
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (DataContext != null)
        {
            DataContext.OnStateChanged -= StateHasChanged;
        }
        await ValueTask.CompletedTask;
    }
}
