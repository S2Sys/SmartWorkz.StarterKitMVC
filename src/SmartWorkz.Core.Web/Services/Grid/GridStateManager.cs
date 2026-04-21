using SmartWorkz.Shared;

namespace SmartWorkz.Web;

/// <summary>
/// Manages grid state (current page, sorting, filters, selected rows).
/// Optionally persists state to browser localStorage.
/// </summary>
public class GridStateManager
{
    private GridRequest _currentRequest = new();
    private List<object> _selectedRowIds = [];
    private string? _errorMessage;
    private bool _isLoading;

    public GridRequest CurrentRequest => _currentRequest;
    public IReadOnlyList<object> SelectedRowIds => _selectedRowIds.AsReadOnly();
    public string? ErrorMessage => _errorMessage;
    public bool IsLoading => _isLoading;

    public event Action? OnStateChanged;

    /// <summary>Update the current grid request and notify listeners.</summary>
    public void UpdateRequest(GridRequest request)
    {
        _currentRequest = request;
        RaiseStateChanged();
    }

    /// <summary>Update pagination (page and pageSize).</summary>
    public void UpdatePagination(int page, int pageSize)
    {
        _currentRequest = _currentRequest with { Page = page, PageSize = pageSize };
        RaiseStateChanged();
    }

    /// <summary>Update sorting.</summary>
    public void UpdateSort(string? sortBy, bool descending)
    {
        _currentRequest = _currentRequest with { SortBy = sortBy, SortDescending = descending };
        RaiseStateChanged();
    }

    /// <summary>Update filters (replaces entire filter dictionary).</summary>
    public void UpdateFilters(Dictionary<string, object>? filters)
    {
        _currentRequest = _currentRequest with { Filters = filters };
        RaiseStateChanged();
    }

    /// <summary>Add or update a single filter.</summary>
    public void SetFilter(string columnName, object value)
    {
        var filters = _currentRequest.Filters ?? new Dictionary<string, object>();
        filters[columnName] = value;
        _currentRequest = _currentRequest with { Filters = filters };
        RaiseStateChanged();
    }

    /// <summary>Remove a filter by column name.</summary>
    public void RemoveFilter(string columnName)
    {
        var filters = _currentRequest.Filters ?? new Dictionary<string, object>();
        filters.Remove(columnName);
        _currentRequest = _currentRequest with { Filters = filters.Any() ? filters : null };
        RaiseStateChanged();
    }

    /// <summary>Clear all filters.</summary>
    public void ClearFilters()
    {
        _currentRequest = _currentRequest with { Filters = null };
        RaiseStateChanged();
    }

    /// <summary>Update selected row IDs.</summary>
    public void SetSelectedRows(List<object> rowIds)
    {
        _selectedRowIds = rowIds;
        RaiseStateChanged();
    }

    /// <summary>Toggle row selection.</summary>
    public void ToggleRowSelection(object rowId)
    {
        if (_selectedRowIds.Contains(rowId))
            _selectedRowIds.Remove(rowId);
        else
            _selectedRowIds.Add(rowId);

        RaiseStateChanged();
    }

    /// <summary>Set loading state.</summary>
    public void SetLoading(bool isLoading)
    {
        _isLoading = isLoading;
        RaiseStateChanged();
    }

    /// <summary>Set error message.</summary>
    public void SetError(string? errorMessage)
    {
        _errorMessage = errorMessage;
        RaiseStateChanged();
    }

    /// <summary>Clear error message.</summary>
    public void ClearError()
    {
        _errorMessage = null;
        RaiseStateChanged();
    }

    /// <summary>Reset all state to defaults.</summary>
    public void Reset()
    {
        _currentRequest = new();
        _selectedRowIds.Clear();
        _errorMessage = null;
        _isLoading = false;
        RaiseStateChanged();
    }

    private void RaiseStateChanged() => OnStateChanged?.Invoke();
}

