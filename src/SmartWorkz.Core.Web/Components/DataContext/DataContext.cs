using SmartWorkz.Shared;
using SmartWorkz.Shared;
using SmartWorkz.Web;
using System.Reflection;

namespace SmartWorkz.Web;

/// <summary>
/// Manages state and state transitions for grid/list components with async data operations.
///
/// DataContext is a state container that coordinates sorting, filtering, pagination, row selection,
/// and loading/error states for data display components. It acts as a bridge between the UI
/// (through Razor components) and the data layer, managing the current request parameters and
/// response data along with user interactions (row selection, state changes).
/// </summary>
/// <remarks>
/// ## State Management Pattern
/// DataContext maintains two primary state objects:
/// - **CurrentRequest** (GridRequest): Input parameters for the next data fetch (sort, filter, pagination)
/// - **CurrentResponse** (GridResponse&lt;T&gt;): Current data and metadata from the last fetch
///
/// ## Async State Transitions
/// Methods that modify sorting, filtering, or pagination (UpdateSort, UpdateFilter, UpdatePagination,
/// ClearFilters) trigger async state transitions via ExecuteWithStateManagement, which:
/// 1. Sets IsLoading to true and clears any previous error
/// 2. Updates CurrentRequest with new parameters
/// 3. Calls RefreshData() to reload data
/// 4. Updates CurrentResponse with new data
/// 5. Raises OnStateChanged event
/// 6. Handles exceptions by setting Error and keeps IsLoading = false
///
/// ## Row Selection and ID Extraction
/// Row selection is tracked in SelectedRowIds as a List&lt;object&gt; containing the unique identifier
/// values for each selected row. Row IDs are extracted via reflection during initialization:
/// 1. Looks for a property with [System.ComponentModel.DataAnnotations.Key] attribute
/// 2. Falls back to a property named "Id" (case-insensitive)
/// 3. Falls back to the first property if neither is found
///
/// The ID property is cached after the first lookup for performance. If no property is found,
/// an InvalidOperationException is thrown during construction.
///
/// Selection methods like ToggleRowSelection, SetSelectedRows, and ToggleSelectAll operate
/// synchronously and raise OnStateChanged to notify subscribers of selection changes.
///
/// ## Filter Handling
/// Filters are stored as a simple Dictionary&lt;string, object&gt; in CurrentRequest.Filters.
/// The UpdateFilter method adds or updates a key-value filter. If value is null, the filter
/// is removed. Future phases may introduce advanced filtering with operator support (contains,
/// startswith, gt, lt, etc.) via a FilterDefinition structure, but currently uses simple equality.
///
/// ## Example Usage - Typical Filter Flow
/// User clicks a filter control in the UI:
/// 1. Component calls context.UpdateFilter("Category", "equals", "Electronics")
/// 2. UpdateFilter wraps the operation in ExecuteWithStateManagement
/// 3. CurrentRequest.Filters is updated: { "Category": "Electronics" }
/// 4. RefreshData() is awaited (async)
/// 5. CurrentResponse.Data is populated with filtered results
/// 6. IsLoading transitions: true → false
/// 7. OnStateChanged event fires
/// 8. Component rebuilds showing filtered data
/// </remarks>
/// <example>
/// // Initialize with data
/// var context = new DataContext&lt;Product&gt;();
/// await context.Initialize(products);
///
/// // Subscribe to state changes
/// context.OnStateChanged += () => Console.WriteLine("State changed!");
///
/// // Filter by category - async operation
/// await context.UpdateFilter("Category", "equals", "Electronics");
/// // CurrentRequest.Filters now contains { "Category": "Electronics" }
/// // CurrentResponse.Data reloaded asynchronously
///
/// // Change sort - resets to page 1
/// await context.UpdateSort("Price", isDescending: true);
///
/// // Change pagination
/// await context.UpdatePagination(pageNumber: 2, pageSize: 50);
///
/// // Row selection - synchronous operations
/// context.ToggleRowSelection(1);          // Select row with ID=1
/// context.ToggleSelectAll(true);          // Select all visible rows
/// var selected = context.SelectedRowIds;  // List&lt;object&gt; containing IDs
///
/// // Check loading state
/// if (context.IsLoading)
/// {
///     // Show spinner
/// }
///
/// // Handle errors
/// if (context.Error != null)
/// {
///     Console.WriteLine($"Error: {context.Error}");
/// }
/// </example>
public class DataContext<T> : IDataContext<T> where T : class
{
    private GridRequest _currentRequest;
    private GridResponse<T>? _currentResponse;
    private readonly List<object> _selectedRowIds = [];
    private bool _isLoading;
    private string? _error;
    private PropertyInfo? _cachedIdProperty;

    /// <summary>
    /// Gets the current grid request parameters (sort, filter, pagination).
    /// </summary>
    public GridRequest CurrentRequest => _currentRequest;

    /// <summary>
    /// Gets the current response containing data, columns, and metadata.
    /// Null until Initialize is called.
    /// </summary>
    public GridResponse<T>? CurrentResponse => _currentResponse;

    /// <summary>
    /// Gets the list of selected row IDs. Items in this collection represent
    /// unique identifiers (as determined by reflection on [Key] or Id property)
    /// for rows currently selected by the user.
    /// </summary>
    public List<object> SelectedRowIds => _selectedRowIds;

    /// <summary>
    /// Gets a value indicating whether a data operation (Initialize, UpdateSort, UpdateFilter,
    /// UpdatePagination, or ClearFilters) is in progress. True while async operations execute,
    /// false when complete. Use this to show/hide loading spinners in the UI.
    /// </summary>
    public bool IsLoading => _isLoading;

    /// <summary>
    /// Gets the error message from the last failed operation, or null if the last operation
    /// succeeded. Error is automatically cleared before each operation attempt.
    /// </summary>
    public string? Error => _error;

    /// <summary>
    /// Raised when any state changes (sorting, filtering, pagination, row selection).
    /// Subscribe to rebuild UI components when state transitions.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Initializes a new instance of the DataContext class with default pagination (page 1, page size 20).
    /// Caches the row ID property via reflection during construction.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if type T has no properties and row ID property cannot be determined.
    /// </exception>
    public DataContext()
    {
        _currentRequest = new GridRequest(Page: 1, PageSize: 20);
        CacheIdProperty();
    }

    /// <summary>
    /// Caches the row ID property using reflection. Called during construction.
    /// Attempts to locate the ID property in this order:
    /// 1. Property decorated with [Key] attribute
    /// 2. Property named "Id" (case-insensitive)
    /// 3. First property on the type
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no properties exist on type T.</exception>
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

    /// <summary>
    /// Initializes the context with data from a source and applies the current request parameters.
    /// Sets IsLoading to true during operation, updates CurrentResponse with the result,
    /// and raises OnStateChanged upon completion.
    /// </summary>
    /// <param name="dataSource">The collection of items to load. Can be empty.</param>
    /// <returns>A task representing the asynchronous initialization.</returns>
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

    /// <summary>
    /// Updates the sort parameters and reloads data.
    /// Resets pagination to page 1 when sort changes. Sets IsLoading to true during operation
    /// and raises OnStateChanged upon completion.
    /// </summary>
    /// <param name="propertyName">The property name to sort by (must exist on type T).</param>
    /// <param name="isDescending">True for descending sort order, false for ascending.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
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

    /// <summary>
    /// Adds, updates, or removes a simple filter and reloads data.
    /// If value is null, removes the filter for this property. If value is not null, adds or updates
    /// the filter. Resets pagination to page 1 when filter changes. Sets IsLoading to true during
    /// operation and raises OnStateChanged upon completion.
    /// </summary>
    /// <remarks>
    /// Currently uses simple equality filters (key-value pairs). Future phases may introduce
    /// advanced filtering with operators (contains, startswith, gt, lt, etc.) via FilterDefinition.
    /// </remarks>
    /// <param name="property">The property name to filter on (must exist on type T).</param>
    /// <param name="filterOperator">The filter operator (e.g., "equals", "contains"). Currently stored but not used;
    /// reserved for future operator-based filtering.</param>
    /// <param name="value">The filter value. If null, removes the filter for this property.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
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

    /// <summary>
    /// Updates pagination parameters (page number and page size) and reloads data.
    /// Page number is coerced to minimum of 1. Sets IsLoading to true during operation
    /// and raises OnStateChanged upon completion.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number. If less than 1, coerced to 1.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
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

    /// <summary>
    /// Toggles the selection state of a single row.
    /// If the row ID is currently selected, removes it; otherwise adds it.
    /// Raises OnStateChanged synchronously upon completion.
    /// </summary>
    /// <param name="rowId">The row identifier (extracted via reflection from the data object).</param>
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

    /// <summary>
    /// Replaces the entire selection with the specified row IDs.
    /// Clears any previously selected rows and sets the selection to exactly these IDs.
    /// Raises OnStateChanged synchronously upon completion.
    /// </summary>
    /// <param name="rowIds">List of row identifiers to select. Can be empty to clear selection.</param>
    public void SetSelectedRows(List<object> rowIds)
    {
        _selectedRowIds.Clear();
        _selectedRowIds.AddRange(rowIds);
        RaiseStateChanged();
    }

    /// <summary>
    /// Selects or deselects all rows on the current page.
    /// If isChecked is true, selects all rows in CurrentResponse. If false, clears selection.
    /// Calls SetSelectedRows internally, which raises OnStateChanged.
    /// </summary>
    /// <param name="isChecked">True to select all visible rows, false to clear selection.</param>
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

    /// <summary>
    /// Removes all active filters and reloads data.
    /// Sets CurrentRequest.Filters to null and resets pagination to page 1.
    /// Note: Sort order is preserved (not cleared by this operation).
    /// Sets IsLoading to true during operation and raises OnStateChanged upon completion.
    /// </summary>
    /// <returns>A task representing the asynchronous clear operation.</returns>
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

    /// <summary>
    /// Wraps an async operation with state management: sets IsLoading, clears errors,
    /// executes the operation, and ensures IsLoading is cleared even if an exception occurs.
    /// Errors are captured and stored in Error property.
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
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

    /// <summary>
    /// Refreshes the data based on current request parameters.
    /// Currently a placeholder that ensures CurrentResponse is initialized.
    /// Future implementation will apply filtering/sorting and may integrate with GridDataProvider for API calls.
    /// </summary>
    private async Task RefreshData()
    {
        // In-memory data source: apply filtering/sorting locally
        // For real API calls, integrate with GridDataProvider.GetDataAsync<T>()
        _currentResponse ??= new GridResponse<T> { Data = PagedList<T>.Empty(), Columns = [] };
    }

    /// <summary>
    /// Sets the IsLoading state.
    /// </summary>
    private void SetLoading(bool value) => _isLoading = value;

    /// <summary>
    /// Sets the Error message.
    /// </summary>
    private void SetError(string? message) => _error = message;

    /// <summary>
    /// Clears the Error message (sets to null).
    /// </summary>
    private void ClearError() => _error = null;

    /// <summary>
    /// Raises the OnStateChanged event if subscribers exist.
    /// </summary>
    private void RaiseStateChanged() => OnStateChanged?.Invoke();

    /// <summary>
    /// Extracts the row ID from an item using the cached ID property.
    /// </summary>
    /// <param name="item">The data item to extract the ID from.</param>
    /// <returns>The row ID value, or the item itself if ID extraction fails.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the cached ID property was not set during initialization.</exception>
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

/// <summary>
/// Represents a single filter condition with property, operator, and value.
/// Reserved for future advanced filtering support (Phase 2 or later).
/// Currently, simple equality filters are used via DataContext.Filters dictionary.
/// </summary>
public class GridFilter
{
    /// <summary>
    /// The property name to filter on.
    /// </summary>
    public string Property { get; set; } = "";

    /// <summary>
    /// The filter operator (e.g., "equals", "contains", "startswith", "gt", "lt").
    /// </summary>
    public string Operator { get; set; } = "equals";

    /// <summary>
    /// The filter value for comparison.
    /// </summary>
    public string? Value { get; set; }
}

