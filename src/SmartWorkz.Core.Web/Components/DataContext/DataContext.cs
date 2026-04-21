using SmartWorkz.Shared;
using SmartWorkz.Shared;
using SmartWorkz.Web;
using System.Reflection;

namespace SmartWorkz.Web;

public class DataContext<T> : IDataContext<T> where T : class
{
    private GridRequest _currentRequest;
    private GridResponse<T>? _currentResponse;
    private readonly List<object> _selectedRowIds = [];
    private bool _isLoading;
    private string? _error;
    private PropertyInfo? _cachedIdProperty;

    public GridRequest CurrentRequest => _currentRequest;
    public GridResponse<T>? CurrentResponse => _currentResponse;
    public List<object> SelectedRowIds => _selectedRowIds;
    public bool IsLoading => _isLoading;
    public string? Error => _error;

    public event Action? OnStateChanged;

    public DataContext()
    {
        _currentRequest = new GridRequest(Page: 1, PageSize: 20);
        CacheIdProperty();
    }

    private void CacheIdProperty()
    {
        var properties = typeof(T).GetProperties();

        // First, look for [Key] attribute
        _cachedIdProperty = properties.FirstOrDefault(p =>
            p.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null);

        // Fall back to property named "Id" or "id"
        if (_cachedIdProperty == null)
        {
            _cachedIdProperty = properties.FirstOrDefault(p =>
                p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        }

        // If still not found, use first property but warn
        if (_cachedIdProperty == null)
        {
            _cachedIdProperty = properties.FirstOrDefault();
        }

        if (_cachedIdProperty == null)
        {
            throw new InvalidOperationException(
                $"Type '{typeof(T).Name}' has no properties. Cannot determine row ID property.");
        }
    }

    public async Task Initialize(IEnumerable<T> dataSource)
    {
        await ExecuteWithStateManagement(async () =>
        {
            var gridLogic = GridDataProvider.ApplyGridLogic(dataSource, _currentRequest);

            _currentResponse = new GridResponse<T>
            {
                Data = gridLogic,
                Columns = []
            };
        });
    }

    public async Task UpdateSort(string propertyName, bool isDescending)
    {
        await ExecuteWithStateManagement(async () =>
        {
            _currentRequest = _currentRequest with
            {
                SortBy = propertyName,
                SortDescending = isDescending,
                Page = 1
            };

            await RefreshData();
        });
    }

    public async Task UpdateFilter(string property, string filterOperator, object? value)
    {
        await ExecuteWithStateManagement(async () =>
        {
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
        });
    }

    public async Task UpdatePagination(int pageNumber, int pageSize)
    {
        await ExecuteWithStateManagement(async () =>
        {
            _currentRequest = _currentRequest with
            {
                Page = Math.Max(1, pageNumber),
                PageSize = pageSize
            };

            await RefreshData();
        });
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
        await ExecuteWithStateManagement(async () =>
        {
            _currentRequest = _currentRequest with
            {
                Filters = null,
                Page = 1
            };

            await RefreshData();
        });
    }

    private async Task ExecuteWithStateManagement(Func<Task> operation)
    {
        try
        {
            SetLoading(true);
            ClearError();
            await operation();
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
        if (_cachedIdProperty == null)
        {
            throw new InvalidOperationException("Row ID property was not found during initialization.");
        }

        var value = _cachedIdProperty.GetValue(item);
        return value ?? item;
    }
}

public class GridFilter
{
    public string Property { get; set; } = "";
    public string Operator { get; set; } = "equals";
    public string? Value { get; set; }
}

