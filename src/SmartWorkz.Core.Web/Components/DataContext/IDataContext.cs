using SmartWorkz.Core.Shared.Grid;

namespace SmartWorkz.Core.Web.Components.DataContext;

/// <summary>
/// Provides unified state management for multi-view data components (Grid, List, etc).
/// Manages sorting, filtering, pagination, row selection, and loading/error states.
/// </summary>
public interface IDataContext<T> where T : class
{
    /// <summary>Current grid request with sort, filter, and pagination parameters.</summary>
    GridRequest CurrentRequest { get; }

    /// <summary>Current response containing data, columns, and metadata.</summary>
    GridResponse<T>? CurrentResponse { get; }

    /// <summary>Identifiers of currently selected rows.</summary>
    List<object> SelectedRowIds { get; }

    /// <summary>True when data fetch is in progress.</summary>
    bool IsLoading { get; }

    /// <summary>Error message from last failed operation, or null.</summary>
    string? Error { get; }

    /// <summary>Raised when any state changes (sort, filter, page, selection).</summary>
    event Action? OnStateChanged;

    /// <summary>Update sort column and direction; triggers data fetch.</summary>
    Task UpdateSort(string propertyName, bool isDescending);

    /// <summary>Add or replace a filter; resets to page 1; triggers data fetch.</summary>
    Task UpdateFilter(string property, string filterOperator, object? value);

    /// <summary>Change current page number; triggers data fetch.</summary>
    Task UpdatePagination(int pageNumber, int pageSize);

    /// <summary>Toggle selection state for a single row.</summary>
    void ToggleRowSelection(object rowId);

    /// <summary>Replace all selected rows.</summary>
    void SetSelectedRows(List<object> rowIds);

    /// <summary>Select/deselect all visible rows on current page.</summary>
    void ToggleSelectAll(bool isChecked);

    /// <summary>Reset filters to default state and refetch data.</summary>
    Task ClearFilters();

    /// <summary>Initialize data from datasource or API endpoint.</summary>
    Task Initialize(IEnumerable<T> dataSource);
}
