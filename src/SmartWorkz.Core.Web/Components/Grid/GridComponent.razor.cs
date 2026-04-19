using Microsoft.AspNetCore.Components;
using SmartWorkz.Core.Shared.Grid;

namespace SmartWorkz.Core.Web.Components.Grid;

public partial class GridComponent<T> : ComponentBase where T : class
{
    [Parameter]
    public IEnumerable<T> DataSource { get; set; } = [];

    [Parameter]
    public List<GridColumn> Columns { get; set; } = [];

    [Parameter]
    public int PageSize { get; set; } = 20;

    [Parameter]
    public bool AllowRowSelection { get; set; }

    [Parameter]
    public bool AllowExport { get; set; }

    [Parameter]
    public bool AllowColumnVisibilityToggle { get; set; }

    [Parameter]
    public string? CustomCssClass { get; set; }

    [Parameter]
    public RenderFragment<T>? RowTemplate { get; set; }

    [Parameter]
    public EventCallback<GridStateChangedArgs> OnStateChanged { get; set; }

    protected GridStateManager StateManager { get; private set; } = new();
    protected GridResponse<T>? CurrentResponse { get; private set; }
    protected List<T> CurrentPageData { get; private set; } = [];
    protected List<GridColumn> VisibleColumns => Columns.Where(c => c.IsVisible).ToList();

    protected override async Task OnInitializedAsync()
    {
        StateManager.OnStateChanged += OnGridStateChanged;
        StateManager.UpdatePagination(1, PageSize);
        await LoadData();
    }

    protected async Task LoadData()
    {
        StateManager.SetLoading(true);
        StateManager.ClearError();

        try
        {
            // In-memory data source: apply filtering/sorting locally
            var provider = new GridDataProvider(new HttpClient());
            var gridRequest = StateManager.CurrentRequest;
            var pagedResult = GridDataProvider.ApplyGridLogic(DataSource, gridRequest);

            CurrentResponse = new GridResponse<T>
            {
                Data = pagedResult,
                Columns = Columns
            };

            CurrentPageData = CurrentResponse.Data.Items.ToList();
        }
        catch (Exception ex)
        {
            StateManager.SetError(ex.Message);
        }
        finally
        {
            StateManager.SetLoading(false);
        }

        await InvokeAsync(StateHasChanged);
    }

    protected async Task OnSortClick(string propertyName)
    {
        var column = Columns.FirstOrDefault(c => c.PropertyName == propertyName);
        if (column?.IsSortable != true)
            return;

        // Toggle sort direction if already sorted by this column
        var isCurrentSort = StateManager.CurrentRequest.SortBy == propertyName;
        var newDescending = isCurrentSort ? !StateManager.CurrentRequest.SortDescending : false;

        StateManager.UpdateSort(propertyName, newDescending);
        StateManager.UpdatePagination(1, PageSize);
        await LoadData();
    }

    protected async Task OnPageChange(int pageNumber)
    {
        StateManager.UpdatePagination(pageNumber, PageSize);
        await LoadData();
    }

    protected void OnRowSelect(object rowId, bool isChecked)
    {
        StateManager.ToggleRowSelection(rowId);
    }

    protected async Task SelectAllRows(ChangeEventArgs e)
    {
        var isChecked = (bool?)e.Value ?? false;
        var allRowIds = CurrentPageData.Select(GetRowId).ToList();

        if (isChecked)
            StateManager.SetSelectedRows(allRowIds);
        else
            StateManager.SetSelectedRows([]);

        await Task.CompletedTask;
    }

    protected object GetRowId(T item)
    {
        // Simple default: assume first property is ID, can be overridden
        var firstProperty = typeof(T).GetProperties().FirstOrDefault();
        return firstProperty?.GetValue(item) ?? item;
    }

    protected bool IsRowSelected(object rowId)
    {
        return StateManager.SelectedRowIds.Contains(rowId);
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

    private async Task OnGridStateChanged()
    {
        await OnStateChanged.InvokeAsync(new GridStateChangedArgs
        {
            Request = StateManager.CurrentRequest,
            SelectedRowIds = StateManager.SelectedRowIds.ToList()
        });
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        StateManager.OnStateChanged -= OnGridStateChanged;
        await ValueTask.CompletedTask;
    }
}

public class GridStateChangedArgs
{
    public GridRequest Request { get; set; } = new();
    public List<object> SelectedRowIds { get; set; } = [];
}
