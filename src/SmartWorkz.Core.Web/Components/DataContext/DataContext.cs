using SmartWorkz.Core.Shared.Grid;
using SmartWorkz.Core.Shared.Pagination;
using SmartWorkz.Core.Web.Services.Grid;

namespace SmartWorkz.Core.Web.Components.DataContext;

public class DataContext<T> : IDataContext<T> where T : class
{
    private readonly GridDataProvider _dataProvider;
    private GridRequest _currentRequest;
    private GridResponse<T>? _currentResponse;
    private readonly List<object> _selectedRowIds = [];
    private bool _isLoading;
    private string? _error;

    public GridRequest CurrentRequest => _currentRequest;
    public GridResponse<T>? CurrentResponse => _currentResponse;
    public List<object> SelectedRowIds => _selectedRowIds;
    public bool IsLoading => _isLoading;
    public string? Error => _error;

    public event Action? OnStateChanged;

    public DataContext()
    {
        _dataProvider = new GridDataProvider(new HttpClient());
        _currentRequest = new GridRequest(Page: 1, PageSize: 20);
    }

    public async Task Initialize(IEnumerable<T> dataSource)
    {
        try
        {
            SetLoading(true);
            ClearError();

            var gridLogic = GridDataProvider.ApplyGridLogic(dataSource, _currentRequest);

            _currentResponse = new GridResponse<T>
            {
                Data = gridLogic,
                Columns = []
            };

            RaiseStateChanged();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task UpdateSort(string propertyName, bool isDescending)
    {
        try
        {
            SetLoading(true);
            ClearError();

            _currentRequest = _currentRequest with
            {
                SortBy = propertyName,
                SortDescending = isDescending,
                Page = 1
            };

            await RefreshData();
            RaiseStateChanged();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task UpdateFilter(string property, string filterOperator, object? value)
    {
        try
        {
            SetLoading(true);
            ClearError();

            var filters = _currentRequest.Filters ?? new Dictionary<string, object>();

            // Add or update filter
            if (value != null)
            {
                filters[property] = value;
            }
            else if (filters.ContainsKey(property))
            {
                filters.Remove(property);
            }

            _currentRequest = _currentRequest with
            {
                Filters = filters,
                Page = 1
            };

            await RefreshData();
            RaiseStateChanged();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task UpdatePagination(int pageNumber, int pageSize)
    {
        try
        {
            SetLoading(true);
            ClearError();

            _currentRequest = _currentRequest with
            {
                Page = Math.Max(1, pageNumber),
                PageSize = pageSize
            };

            await RefreshData();
            RaiseStateChanged();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    public void ToggleRowSelection(object rowId)
    {
        if (_selectedRowIds.Contains(rowId))
        {
            _selectedRowIds.Remove(rowId);
        }
        else
        {
            _selectedRowIds.Add(rowId);
        }

        RaiseStateChanged();
    }

    public void SetSelectedRows(List<object> rowIds)
    {
        _selectedRowIds.Clear();
        _selectedRowIds.AddRange(rowIds);
        RaiseStateChanged();
    }

    public void ToggleSelectAll(bool isChecked)
    {
        if (isChecked && _currentResponse?.Data?.Items != null)
        {
            var allIds = _currentResponse.Data.Items
                .Select(item => GetRowId(item))
                .ToList();
            SetSelectedRows(allIds);
        }
        else
        {
            SetSelectedRows([]);
        }
    }

    public async Task ClearFilters()
    {
        try
        {
            SetLoading(true);
            ClearError();

            _currentRequest = _currentRequest with
            {
                Filters = null,
                Page = 1
            };

            await RefreshData();
            RaiseStateChanged();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task RefreshData()
    {
        // In-memory data source: apply filtering/sorting locally
        // For real API calls, integrate with GridDataProvider.GetDataAsync<T>()
        _currentResponse ??= new GridResponse<T> { Data = PagedList<T>.Empty(), Columns = [] };
    }

    private void SetLoading(bool value) => _isLoading = value;

    private void SetError(string? message) => _error = message;

    private void ClearError() => _error = null;

    private void RaiseStateChanged() => OnStateChanged?.Invoke();

    private object GetRowId(T item)
    {
        var firstProperty = typeof(T).GetProperties().FirstOrDefault();
        return firstProperty?.GetValue(item) ?? item;
    }
}
