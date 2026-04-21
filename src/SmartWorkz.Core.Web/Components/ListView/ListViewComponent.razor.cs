using Microsoft.AspNetCore.Components;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

public partial class ListViewComponent<T> : ComponentBase, IAsyncDisposable where T : class
{
    [Parameter]
    public IDataContext<T>? DataContext { get; set; }

    [Parameter]
    public ViewConfiguration? Configuration { get; set; }

    [Parameter]
    public RenderFragment<T>? ItemTemplate { get; set; }

    [Inject]
    public IListViewFormatter Formatter { get; set; } = new ListViewFormatter();

    protected override void OnInitialized()
    {
        if (DataContext != null)
        {
            DataContext.OnStateChanged += StateHasChanged;
        }
    }

    private async Task OnPageChange(int pageNumber)
    {
        if (DataContext != null)
        {
            await DataContext.UpdatePagination(pageNumber, DataContext.CurrentRequest.PageSize);
        }
    }

    private void OnRowSelect(object rowId, bool isChecked)
    {
        if (DataContext != null)
        {
            DataContext.ToggleRowSelection(rowId);
        }
    }

    private void OnSelectAll(bool isChecked)
    {
        if (DataContext != null)
        {
            DataContext.ToggleSelectAll(isChecked);
        }
    }

    private async Task OnRetry()
    {
        if (DataContext != null)
        {
            await DataContext.ClearFilters();
        }
    }

    private object GetRowId(T item)
    {
        var firstProperty = typeof(T).GetProperties().FirstOrDefault();
        return firstProperty?.GetValue(item) ?? item;
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
