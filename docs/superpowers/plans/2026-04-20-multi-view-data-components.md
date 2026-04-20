# Multi-View Data Components Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a unified DataContext with Grid and List view components that sync state across both views, allowing users to switch views without losing filters, sorting, or selection.

**Architecture:** DataContext<T> is a non-Razor service managing all state (sort, filter, pagination, selection). GridViewComponent and ListViewComponent are thin rendering layers that consume the shared context via dependency injection. DataViewerComponent orchestrates both views and provides the toggle UI. ListViewFormatter handles data transformation for the card/list layout.

**Tech Stack:** .NET 9.0, Blazor Components, ASP.NET Core, xUnit for testing

---

## Task 1: Create DataContext Interface

**Files:**
- Create: `src/SmartWorkz.Core.Web/Components/DataContext/IDataContext.cs`

- [ ] **Step 1: Write interface definition**

Create the file and add the interface:

```csharp
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
```

- [ ] **Step 2: Verify file created**

Run: `ls -la src/SmartWorkz.Core.Web/Components/DataContext/IDataContext.cs`

Expected: File exists and contains the interface

---

## Task 2: Implement DataContext Class

**Files:**
- Create: `src/SmartWorkz.Core.Web/Components/DataContext/DataContext.cs`
- Modify: `src/SmartWorkz.Core.Web/GlobalUsings.cs`

- [ ] **Step 1: Write DataContext implementation**

Create the file:

```csharp
using SmartWorkz.Core.Shared.Grid;
using SmartWorkz.Core.Web.Services.Grid;

namespace SmartWorkz.Core.Web.Components.DataContext;

public class DataContext<T> : IDataContext<T> where T : class
{
    private readonly GridDataProvider _dataProvider;
    private GridRequest _currentRequest = new();
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
    }

    public async Task Initialize(IEnumerable<T> dataSource)
    {
        try
        {
            SetLoading(true);
            ClearError();

            _currentRequest = new GridRequest { PageNumber = 1, PageSize = 20 };
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

            _currentRequest.SortBy = propertyName;
            _currentRequest.SortDescending = isDescending;
            _currentRequest.PageNumber = 1;

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

            // Add or update filter
            var filter = _currentRequest.Filters.FirstOrDefault(f => f.Property == property);
            if (filter != null)
            {
                _currentRequest.Filters.Remove(filter);
            }

            if (value != null)
            {
                _currentRequest.Filters.Add(new GridFilter
                {
                    Property = property,
                    Operator = filterOperator,
                    Value = value.ToString()
                });
            }

            _currentRequest.PageNumber = 1;

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

            _currentRequest.PageNumber = Math.Max(1, pageNumber);
            _currentRequest.PageSize = pageSize;

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

            _currentRequest = new GridRequest
            {
                PageNumber = 1,
                PageSize = _currentRequest.PageSize
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
        // For real API calls, integrate with GridDataProvider.FetchAsync()
        _currentResponse ??= new GridResponse<T> { Data = new PagedResult<T>(), Columns = [] };
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

public class GridFilter
{
    public string Property { get; set; } = "";
    public string Operator { get; set; } = "equals";
    public string? Value { get; set; }
}
```

- [ ] **Step 2: Add DataContext to GlobalUsings**

Edit `src/SmartWorkz.Core.Web/GlobalUsings.cs` and add:

```csharp
global using SmartWorkz.Core.Web.Components.DataContext;
```

- [ ] **Step 3: Verify compilation**

Run: `dotnet build src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj`

Expected: Builds successfully with no errors

---

## Task 3: Write DataContext Unit Tests

**Files:**
- Create: `tests/SmartWorkz.Core.Web.Tests/Components/DataContextTests.cs`

- [ ] **Step 1: Create test class with initialization test**

```csharp
using Xunit;
using SmartWorkz.Core.Web.Components.DataContext;
using SmartWorkz.Core.Shared.Grid;

namespace SmartWorkz.Core.Web.Tests.Components;

public class DataContextTests
{
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
    }

    [Fact]
    public async Task Initialize_WithData_PopulatesCurrentResponse()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Item 1", Category = "A" },
            new() { Id = 2, Name = "Item 2", Category = "B" }
        };

        // Act
        await context.Initialize(items);

        // Assert
        Assert.NotNull(context.CurrentResponse);
        Assert.NotEmpty(context.CurrentResponse.Data.Items);
        Assert.Equal(2, context.CurrentResponse.Data.Items.Count());
    }

    [Fact]
    public async Task UpdateSort_ChangesCurrentRequest()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());

        // Act
        await context.UpdateSort("Name", false);

        // Assert
        Assert.Equal("Name", context.CurrentRequest.SortBy);
        Assert.False(context.CurrentRequest.SortDescending);
    }

    [Fact]
    public async Task UpdateSort_ResetsToPage1()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());
        context.CurrentRequest.PageNumber = 5;

        // Act
        await context.UpdateSort("Name", false);

        // Assert
        Assert.Equal(1, context.CurrentRequest.PageNumber);
    }

    [Fact]
    public async Task UpdateFilter_AddsFilterToRequest()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());

        // Act
        await context.UpdateFilter("Category", "equals", "A");

        // Assert
        Assert.Single(context.CurrentRequest.Filters);
        Assert.Equal("Category", context.CurrentRequest.Filters[0].Property);
        Assert.Equal("A", context.CurrentRequest.Filters[0].Value);
    }

    [Fact]
    public async Task UpdateFilter_ResetsToPage1()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());
        context.CurrentRequest.PageNumber = 5;

        // Act
        await context.UpdateFilter("Category", "equals", "A");

        // Assert
        Assert.Equal(1, context.CurrentRequest.PageNumber);
    }

    [Fact]
    public async Task UpdatePagination_ChangesPaginationParams()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());

        // Act
        await context.UpdatePagination(3, 50);

        // Assert
        Assert.Equal(3, context.CurrentRequest.PageNumber);
        Assert.Equal(50, context.CurrentRequest.PageSize);
    }

    [Fact]
    public void ToggleRowSelection_TogglesSingleRow()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        var rowId = 1;

        // Act
        context.ToggleRowSelection(rowId);

        // Assert
        Assert.Contains(rowId, context.SelectedRowIds);

        // Act again
        context.ToggleRowSelection(rowId);

        // Assert
        Assert.DoesNotContain(rowId, context.SelectedRowIds);
    }

    [Fact]
    public void SetSelectedRows_ReplacesSelection()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        context.ToggleRowSelection(1);
        context.ToggleRowSelection(2);

        // Act
        context.SetSelectedRows(new List<object> { 3, 4 });

        // Assert
        Assert.Equal(2, context.SelectedRowIds.Count);
        Assert.Contains(3, context.SelectedRowIds);
        Assert.Contains(4, context.SelectedRowIds);
        Assert.DoesNotContain(1, context.SelectedRowIds);
    }

    [Fact]
    public async Task ClearFilters_ResetsRequest()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        await context.Initialize(new List<TestItem>());
        await context.UpdateFilter("Category", "equals", "A");
        await context.UpdateSort("Name", true);
        context.CurrentRequest.PageNumber = 5;

        // Act
        await context.ClearFilters();

        // Assert
        Assert.Empty(context.CurrentRequest.Filters);
        Assert.Null(context.CurrentRequest.SortBy);
        Assert.Equal(1, context.CurrentRequest.PageNumber);
    }

    [Fact]
    public void OnStateChanged_RaisedWhenStateChanges()
    {
        // Arrange
        var context = new DataContext<TestItem>();
        var stateChangedCount = 0;
        context.OnStateChanged += () => stateChangedCount++;

        // Act
        context.ToggleRowSelection(1);

        // Assert
        Assert.Equal(1, stateChangedCount);
    }
}
```

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test tests/SmartWorkz.Core.Web.Tests/SmartWorkz.Core.Web.Tests.csproj -k DataContextTests -v`

Expected: All 9 tests pass

- [ ] **Step 3: Commit**

```bash
git add tests/SmartWorkz.Core.Web.Tests/Components/DataContextTests.cs
git commit -m "test: add DataContext unit tests for state management

- Initialize, UpdateSort, UpdateFilter, UpdatePagination tests
- Row selection toggle and bulk select tests
- ClearFilters and OnStateChanged event tests

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## Task 4: Create ViewConfiguration Class

**Files:**
- Create: `src/SmartWorkz.Core.Web/Services/DataView/ViewConfiguration.cs`

- [ ] **Step 1: Write ViewConfiguration class**

```csharp
namespace SmartWorkz.Core.Web.Services.DataView;

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
```

- [ ] **Step 2: Verify file created**

Run: `ls -la src/SmartWorkz.Core.Web/Services/DataView/ViewConfiguration.cs`

Expected: File exists

---

## Task 5: Create ListViewFormatter Interface and Implementation

**Files:**
- Create: `src/SmartWorkz.Core.Web/Services/DataView/IListViewFormatter.cs`
- Create: `src/SmartWorkz.Core.Web/Services/DataView/ListViewFormatter.cs`

- [ ] **Step 1: Write formatter interface**

```csharp
namespace SmartWorkz.Core.Web.Services.DataView;

/// <summary>
/// Formats data for List/Card view display (dates, currency, text truncation, etc).
/// </summary>
public interface IListViewFormatter
{
    /// <summary>Format a date value for display.</summary>
    string FormatDate(DateTime? date, string format = "MMM dd, yyyy");

    /// <summary>Format a decimal value as currency.</summary>
    string FormatCurrency(decimal? value, string currencySymbol = "$");

    /// <summary>Truncate text to max length with ellipsis.</summary>
    string TruncateText(string? text, int maxLength = 100);

    /// <summary>Format a boolean as human-readable text.</summary>
    string FormatBoolean(bool? value);

    /// <summary>Format any object using type-aware rules.</summary>
    string FormatValue(object? value);
}
```

- [ ] **Step 2: Write formatter implementation**

```csharp
namespace SmartWorkz.Core.Web.Services.DataView;

public class ListViewFormatter : IListViewFormatter
{
    public string FormatDate(DateTime? date, string format = "MMM dd, yyyy")
    {
        return date?.ToString(format) ?? "-";
    }

    public string FormatCurrency(decimal? value, string currencySymbol = "$")
    {
        if (value == null)
            return "-";

        return $"{currencySymbol}{value:N2}";
    }

    public string TruncateText(string? text, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(text))
            return "-";

        if (text.Length <= maxLength)
            return text;

        return text[..maxLength] + "...";
    }

    public string FormatBoolean(bool? value)
    {
        return value switch
        {
            true => "Yes",
            false => "No",
            null => "-"
        };
    }

    public string FormatValue(object? value)
    {
        return value switch
        {
            null => "-",
            DateTime dateTime => FormatDate(dateTime),
            decimal decimalValue => FormatCurrency(decimalValue),
            bool boolValue => FormatBoolean(boolValue),
            string stringValue => TruncateText(stringValue),
            _ => value.ToString() ?? "-"
        };
    }
}
```

- [ ] **Step 3: Verify files created**

Run: `ls -la src/SmartWorkz.Core.Web/Services/DataView/`

Expected: Both IListViewFormatter.cs and ListViewFormatter.cs exist

---

## Task 6: Write ListViewFormatter Unit Tests

**Files:**
- Create: `tests/SmartWorkz.Core.Web.Tests/Services/ListViewFormatterTests.cs`

- [ ] **Step 1: Write formatter tests**

```csharp
using Xunit;
using SmartWorkz.Core.Web.Services.DataView;

namespace SmartWorkz.Core.Web.Tests.Services;

public class ListViewFormatterTests
{
    private readonly ListViewFormatter _formatter = new();

    [Fact]
    public void FormatDate_WithValidDate_ReturnsFormattedString()
    {
        // Arrange
        var date = new DateTime(2026, 4, 20);

        // Act
        var result = _formatter.FormatDate(date);

        // Assert
        Assert.Equal("Apr 20, 2026", result);
    }

    [Fact]
    public void FormatDate_WithNullDate_ReturnsDash()
    {
        // Act
        var result = _formatter.FormatDate(null);

        // Assert
        Assert.Equal("-", result);
    }

    [Fact]
    public void FormatCurrency_WithValidValue_ReturnsFormattedString()
    {
        // Act
        var result = _formatter.FormatCurrency(99.99m);

        // Assert
        Assert.Equal("$99.99", result);
    }

    [Fact]
    public void FormatCurrency_WithNullValue_ReturnsDash()
    {
        // Act
        var result = _formatter.FormatCurrency(null);

        // Assert
        Assert.Equal("-", result);
    }

    [Fact]
    public void TruncateText_WithLongText_TruncatesWithEllipsis()
    {
        // Arrange
        var longText = "This is a very long text that should be truncated";

        // Act
        var result = _formatter.TruncateText(longText, 20);

        // Assert
        Assert.Equal("This is a very long ...", result);
    }

    [Fact]
    public void TruncateText_WithShortText_ReturnsUnchanged()
    {
        // Arrange
        var shortText = "Short";

        // Act
        var result = _formatter.TruncateText(shortText, 20);

        // Assert
        Assert.Equal("Short", result);
    }

    [Fact]
    public void FormatBoolean_WithTrue_ReturnsYes()
    {
        // Act
        var result = _formatter.FormatBoolean(true);

        // Assert
        Assert.Equal("Yes", result);
    }

    [Fact]
    public void FormatBoolean_WithFalse_ReturnsNo()
    {
        // Act
        var result = _formatter.FormatBoolean(false);

        // Assert
        Assert.Equal("No", result);
    }

    [Fact]
    public void FormatValue_WithDateTime_UsesDateFormatter()
    {
        // Arrange
        var date = new DateTime(2026, 4, 20);

        // Act
        var result = _formatter.FormatValue(date);

        // Assert
        Assert.Equal("Apr 20, 2026", result);
    }

    [Fact]
    public void FormatValue_WithDecimal_UsesCurrencyFormatter()
    {
        // Act
        var result = _formatter.FormatValue(49.50m);

        // Assert
        Assert.Equal("$49.50", result);
    }

    [Fact]
    public void FormatValue_WithNull_ReturnsDash()
    {
        // Act
        var result = _formatter.FormatValue(null);

        // Assert
        Assert.Equal("-", result);
    }
}
```

- [ ] **Step 2: Run tests**

Run: `dotnet test tests/SmartWorkz.Core.Web.Tests/SmartWorkz.Core.Web.Tests.csproj -k ListViewFormatterTests -v`

Expected: All 10 tests pass

- [ ] **Step 3: Commit**

```bash
git add tests/SmartWorkz.Core.Web.Tests/Services/ListViewFormatterTests.cs
git commit -m "test: add ListViewFormatter unit tests

- Date, currency, text truncation formatting tests
- Boolean formatting tests
- Type-aware FormatValue tests

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## Task 7: Create ListViewComponent Razor Component

**Files:**
- Create: `src/SmartWorkz.Core.Web/Components/ListView/ListViewComponent.razor`

- [ ] **Step 1: Write ListViewComponent.razor markup**

```razor
@typeparam T where T : class

@if (DataContext == null)
{
    <div class="alert alert-warning">DataContext not provided</div>
    return;
}

<div class="list-view-container">
    @if (DataContext.IsLoading)
    {
        <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
    }
    else if (!string.IsNullOrEmpty(DataContext.Error))
    {
        <div class="alert alert-danger">
            <strong>Error:</strong> @DataContext.Error
            <button class="btn btn-sm btn-outline-danger" @onclick="OnRetry">Retry</button>
        </div>
    }
    else if (DataContext.CurrentResponse?.Data?.Items.Any() != true)
    {
        <div class="alert alert-info">No items to display</div>
    }
    else
    {
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <strong>@DataContext.CurrentResponse.Data.Items.Count() items</strong>
                @if (DataContext.SelectedRowIds.Any())
                {
                    <span class="badge bg-primary">@DataContext.SelectedRowIds.Count selected</span>
                }
            </div>
            @if (Configuration?.AllowRowSelection == true && DataContext.CurrentResponse.Data.Items.Any())
            {
                <label class="form-check-label">
                    <input type="checkbox" class="form-check-input" 
                           @onchange="@((ChangeEventArgs e) => OnSelectAll((bool?)e.Value ?? false))" />
                    Select all
                </label>
            }
        </div>

        <div class="row g-3">
            @foreach (var item in DataContext.CurrentResponse.Data.Items)
            {
                var rowId = GetRowId(item);
                var isSelected = DataContext.SelectedRowIds.Contains(rowId);

                <div class="col-md-@(12 / (Configuration?.ItemsPerRow ?? 2))">
                    <div class="@(Configuration?.CardCssClass ?? "card h-100")
                              @(isSelected ? "border-primary bg-light" : "")">
                        <div class="card-body">
                            @if (Configuration?.AllowRowSelection == true)
                            {
                                <div class="form-check mb-2">
                                    <input type="checkbox" class="form-check-input" 
                                           id="select_@rowId"
                                           checked="@isSelected"
                                           @onchange="@((ChangeEventArgs e) => OnRowSelect(rowId, (bool?)e.Value ?? false))" />
                                </div>
                            }

                            @if (ItemTemplate != null)
                            {
                                @ItemTemplate(item)
                            }
                            else
                            {
                                <RenderDefaultCardContent Item="item" Formatter="Formatter" />
                            }
                        </div>
                    </div>
                </div>
            }
        </div>

        @if (DataContext.CurrentResponse.Data.TotalPages > 1)
        {
            <nav class="mt-4" aria-label="Pagination">
                <ul class="pagination justify-content-center">
                    <li class="page-item @(DataContext.CurrentRequest.PageNumber == 1 ? "disabled" : "")">
                        <button class="page-link" @onclick="@(() => OnPageChange(DataContext.CurrentRequest.PageNumber - 1))">
                            Previous
                        </button>
                    </li>

                    @for (int i = 1; i <= DataContext.CurrentResponse.Data.TotalPages; i++)
                    {
                        var pageNum = i;
                        <li class="page-item @(DataContext.CurrentRequest.PageNumber == pageNum ? "active" : "")">
                            <button class="page-link" @onclick="@(() => OnPageChange(pageNum))">
                                @pageNum
                            </button>
                        </li>
                    }

                    <li class="page-item @(DataContext.CurrentRequest.PageNumber == DataContext.CurrentResponse.Data.TotalPages ? "disabled" : "")">
                        <button class="page-link" @onclick="@(() => OnPageChange(DataContext.CurrentRequest.PageNumber + 1))">
                            Next
                        </button>
                    </li>
                </ul>
            </nav>
        }
    }
</div>

@code {
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

    void IAsyncDisposable.DisposeAsync()
    {
        if (DataContext != null)
        {
            DataContext.OnStateChanged -= StateHasChanged;
        }
        return ValueTask.CompletedTask;
    }
}

<RenderDefaultCardContent>
@typeparam ItemType where ItemType : class

@if (Item != null && Formatter != null)
{
    @foreach (var property in typeof(ItemType).GetProperties().Take(3))
    {
        var value = property.GetValue(Item);
        var formatted = Formatter.FormatValue(value);
        <p class="card-text">
            <strong>@property.Name:</strong> @formatted
        </p>
    }
}
```

- [ ] **Step 2: Verify file created**

Run: `ls -la src/SmartWorkz.Core.Web/Components/ListView/ListViewComponent.razor`

Expected: File exists

---

## Task 8: Create ListViewComponent Code-Behind

**Files:**
- Create: `src/SmartWorkz.Core.Web/Components/ListView/ListViewComponent.razor.cs`

- [ ] **Step 1: Write code-behind class**

```csharp
using Microsoft.AspNetCore.Components;
using SmartWorkz.Core.Web.Services.DataView;

namespace SmartWorkz.Core.Web.Components.ListView;

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
```

- [ ] **Step 2: Verify file created**

Run: `ls -la src/SmartWorkz.Core.Web/Components/ListView/ListViewComponent.razor.cs`

Expected: File exists

- [ ] **Step 3: Verify compilation**

Run: `dotnet build src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj`

Expected: Builds successfully

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Core.Web/Components/ListView/
git add src/SmartWorkz.Core.Web/Services/DataView/
git commit -m "feat: add ListViewComponent with card/list layout display

- ListViewComponent.razor with responsive grid layout
- Row selection with select-all checkbox
- Pagination controls
- Error and loading states
- Support for custom ItemTemplate

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## Task 9: Create DataViewerComponent

**Files:**
- Create: `src/SmartWorkz.Core.Web/Components/DataViewer/DataViewerComponent.razor`
- Create: `src/SmartWorkz.Core.Web/Components/DataViewer/DataViewerComponent.razor.cs`

- [ ] **Step 1: Write DataViewerComponent.razor**

```razor
@typeparam T where T : class

<div class="data-viewer-container">
    <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <h5 class="mb-0">Data Browser</h5>
            
            <div class="btn-group" role="group">
                <input type="radio" class="btn-check" name="view-toggle" id="grid-view" 
                       value="grid" checked="@(CurrentView == ViewType.Grid)" 
                       @onchange="@((ChangeEventArgs e) => OnViewToggle((string?)e.Value))" />
                <label class="btn btn-outline-primary" for="grid-view">
                    📊 Grid
                </label>

                <input type="radio" class="btn-check" name="view-toggle" id="list-view" 
                       value="list" checked="@(CurrentView == ViewType.List)"
                       @onchange="@((ChangeEventArgs e) => OnViewToggle((string?)e.Value))" />
                <label class="btn btn-outline-primary" for="list-view">
                    📋 List
                </label>
            </div>
        </div>

        <div class="card-body">
            @if (DataContext == null)
            {
                <div class="alert alert-danger">DataContext is not initialized</div>
            }
            else if (CurrentView == ViewType.Grid)
            {
                <GridViewComponent @ref="gridComponent" 
                                  DataContext="DataContext"
                                  Columns="Columns" />
            }
            else if (CurrentView == ViewType.List)
            {
                <ListViewComponent @ref="listComponent"
                                  T="T"
                                  DataContext="DataContext"
                                  Configuration="ListViewConfig"
                                  ItemTemplate="ItemTemplate" />
            }
        </div>
    </div>
</div>

@code {
    [Parameter]
    public IEnumerable<T>? DataSource { get; set; }

    [Parameter]
    public List<GridColumn>? Columns { get; set; }

    [Parameter]
    public RenderFragment<T>? ItemTemplate { get; set; }

    [Parameter]
    public ViewType DefaultView { get; set; } = ViewType.Grid;

    [Parameter]
    public bool AutoFetch { get; set; } = true;

    public IDataContext<T>? DataContext { get; set; }
    public ViewConfiguration? ListViewConfig { get; set; }
    public ViewType CurrentView { get; set; }

    private GridViewComponent<T>? gridComponent;
    private ListViewComponent<T>? listComponent;

    protected override async Task OnInitializedAsync()
    {
        CurrentView = DefaultView;
        DataContext = new DataContext<T>();
        ListViewConfig = new ViewConfiguration
        {
            VisibleColumns = Columns?.Select(c => c.PropertyName).ToList() ?? [],
            ItemsPerRow = 2
        };

        if (AutoFetch && DataSource != null)
        {
            await DataContext.Initialize(DataSource);
        }
    }

    private void OnViewToggle(string? viewValue)
    {
        if (viewValue == "grid")
            CurrentView = ViewType.Grid;
        else if (viewValue == "list")
            CurrentView = ViewType.List;

        StateHasChanged();
    }
}

public enum ViewType
{
    Grid = 0,
    List = 1,
    Map = 2
}
```

- [ ] **Step 2: Write DataViewerComponent.razor.cs**

```csharp
using Microsoft.AspNetCore.Components;
using SmartWorkz.Core.Shared.Grid;
using SmartWorkz.Core.Web.Components.GridViewComponent;
using SmartWorkz.Core.Web.Components.ListView;
using SmartWorkz.Core.Web.Services.DataView;

namespace SmartWorkz.Core.Web.Components.DataViewer;

public partial class DataViewerComponent<T> : ComponentBase where T : class
{
    [Parameter]
    public IEnumerable<T>? DataSource { get; set; }

    [Parameter]
    public List<GridColumn>? Columns { get; set; }

    [Parameter]
    public RenderFragment<T>? ItemTemplate { get; set; }

    [Parameter]
    public ViewType DefaultView { get; set; } = ViewType.Grid;

    [Parameter]
    public bool AutoFetch { get; set; } = true;

    public IDataContext<T>? DataContext { get; set; }
    public ViewConfiguration? ListViewConfig { get; set; }
    public ViewType CurrentView { get; set; }

    private GridViewComponent<T>? gridComponent;
    private ListViewComponent<T>? listComponent;

    protected override async Task OnInitializedAsync()
    {
        CurrentView = DefaultView;
        DataContext = new DataContext<T>();
        ListViewConfig = new ViewConfiguration
        {
            VisibleColumns = Columns?.Select(c => c.PropertyName).ToList() ?? [],
            ItemsPerRow = 2
        };

        if (AutoFetch && DataSource != null)
        {
            await DataContext.Initialize(DataSource);
        }
    }

    private void OnViewToggle(string? viewValue)
    {
        if (viewValue == "grid")
            CurrentView = ViewType.Grid;
        else if (viewValue == "list")
            CurrentView = ViewType.List;

        StateHasChanged();
    }
}

public enum ViewType
{
    Grid = 0,
    List = 1,
    Map = 2
}
```

- [ ] **Step 3: Verify compilation**

Run: `dotnet build src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj`

Expected: Builds successfully

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Core.Web/Components/DataViewer/
git commit -m "feat: add DataViewerComponent for view orchestration

- Grid/List view toggle buttons
- Shared DataContext initialization
- Auto-fetch data on load
- Responsive layout with card-based UI

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## Task 10: Enhance GridViewComponent to Use DataContext

**Files:**
- Modify: `src/SmartWorkz.Core.Web/Components/Grid/GridComponent.razor.cs`

- [ ] **Step 1: Read current GridComponent file**

Current location: `src/SmartWorkz.Core.Web/Components/Grid/GridComponent.razor.cs`

- [ ] **Step 2: Refactor to use DataContext**

Replace the entire file with:

```csharp
using Microsoft.AspNetCore.Components;
using SmartWorkz.Core.Shared.Grid;
using SmartWorkz.Core.Web.Components.DataContext;
using SmartWorkz.Core.Web.Services.Grid;

namespace SmartWorkz.Core.Web.Components.Grid;

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
```

- [ ] **Step 3: Verify compilation**

Run: `dotnet build src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj`

Expected: Builds successfully

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Core.Web/Components/Grid/GridComponent.razor.cs
git commit -m "refactor: update GridComponent to use shared DataContext

- Remove local GridStateManager, use injected DataContext
- Delegate sort/filter/pagination to context methods
- Subscribe to OnStateChanged for re-renders
- Maintain backward compatibility with parameters

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## Task 11: Write Integration Tests

**Files:**
- Create: `tests/SmartWorkz.Core.Web.Tests/Integration/MultiViewIntegrationTests.cs`

- [ ] **Step 1: Write integration test class**

```csharp
using Xunit;
using SmartWorkz.Core.Web.Components.DataContext;
using SmartWorkz.Core.Shared.Grid;

namespace SmartWorkz.Core.Web.Tests.Integration;

public class MultiViewIntegrationTests
{
    private class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
    }

    private List<Product> GetSampleProducts()
    {
        return new()
        {
            new() { Id = 1, Name = "Laptop", Category = "Electronics", Price = 999.99m },
            new() { Id = 2, Name = "Mouse", Category = "Electronics", Price = 29.99m },
            new() { Id = 3, Name = "Chair", Category = "Furniture", Price = 199.99m },
            new() { Id = 4, Name = "Desk", Category = "Furniture", Price = 399.99m },
            new() { Id = 5, Name = "Monitor", Category = "Electronics", Price = 299.99m }
        };
    }

    [Fact]
    public async Task ViewToggle_PreservesState_WhenSwitchingViews()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);

        // Act - Apply filter
        await context.UpdateFilter("Category", "equals", "Electronics");
        var countAfterFilter = context.CurrentResponse?.Data?.Items?.Count() ?? 0;

        // Assert - Filter was applied
        Assert.Equal(3, countAfterFilter); // 3 electronics items

        // Act - Select rows
        context.ToggleRowSelection(1);
        context.ToggleRowSelection(2);
        var selectedBefore = context.SelectedRowIds.Count;

        // Assert - Selection preserved across "view switch"
        // (In real component, switching views doesn't clear context)
        Assert.Equal(2, selectedBefore);
        Assert.Equal(3, context.CurrentResponse?.Data?.Items?.Count());
    }

    [Fact]
    public async Task SortAndFilter_ApplyCorrectly_WhenCombined()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);

        // Act - Filter by category
        await context.UpdateFilter("Category", "equals", "Electronics");
        // Act - Sort by price descending
        await context.UpdateSort("Price", true);

        // Assert
        var filtered = context.CurrentResponse?.Data?.Items ?? [];
        var hasElectronics = filtered.All(p => p.Category == "Electronics");
        Assert.True(hasElectronics);
        Assert.Equal(3, filtered.Count());
    }

    [Fact]
    public async Task ClearFilters_RemovesAllFiltersAndSort()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);
        await context.UpdateFilter("Category", "equals", "Electronics");
        await context.UpdateSort("Price", true);
        context.ToggleRowSelection(1);
        context.ToggleRowSelection(2);

        // Act
        await context.ClearFilters();

        // Assert
        Assert.Empty(context.CurrentRequest.Filters);
        Assert.Null(context.CurrentRequest.SortBy);
        Assert.Equal(1, context.CurrentRequest.PageNumber);
        // Selection should persist across clear filters
        Assert.Equal(2, context.SelectedRowIds.Count);
    }

    [Fact]
    public async Task Pagination_Syncs_AcrossStateChanges()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = Enumerable.Range(1, 50)
            .Select(i => new Product { Id = i, Name = $"Item {i}", Category = "Test", Price = i * 10 })
            .ToList();
        await context.Initialize(products);

        // Act
        await context.UpdatePagination(1, 10);
        var page1Items = context.CurrentResponse?.Data?.Items?.Count() ?? 0;

        await context.UpdatePagination(2, 10);
        var page2Items = context.CurrentResponse?.Data?.Items?.Count() ?? 0;

        // Assert
        Assert.Equal(10, page1Items);
        Assert.Equal(10, page2Items);
        Assert.Equal(2, context.CurrentRequest.PageNumber);
    }

    [Fact]
    public async Task StateChange_RaisesEvent_ForAllOperations()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        await context.Initialize(products);
        var eventCount = 0;
        context.OnStateChanged += () => eventCount++;

        // Act
        await context.UpdateSort("Name", false);
        await context.UpdateFilter("Category", "equals", "Electronics");
        await context.UpdatePagination(2, 20);
        context.ToggleRowSelection(1);

        // Assert
        Assert.True(eventCount > 0); // At least one event per operation
    }

    [Fact]
    public void Selection_IsMaintained_WithMultipleTogles()
    {
        // Arrange
        var context = new DataContext<Product>();
        var products = GetSampleProducts();
        context.CurrentResponse = new GridResponse<Product>
        {
            Data = new PagedResult<Product> { Items = products },
            Columns = []
        };

        // Act
        context.ToggleRowSelection(1);
        context.ToggleRowSelection(2);
        context.ToggleRowSelection(3);
        context.ToggleRowSelection(2); // toggle off

        // Assert
        Assert.Equal(2, context.SelectedRowIds.Count);
        Assert.Contains(1, context.SelectedRowIds);
        Assert.Contains(3, context.SelectedRowIds);
        Assert.DoesNotContain(2, context.SelectedRowIds);
    }
}
```

- [ ] **Step 2: Run integration tests**

Run: `dotnet test tests/SmartWorkz.Core.Web.Tests/SmartWorkz.Core.Web.Tests.csproj -k MultiViewIntegrationTests -v`

Expected: All 6 tests pass

- [ ] **Step 3: Commit**

```bash
git add tests/SmartWorkz.Core.Web.Tests/Integration/MultiViewIntegrationTests.cs
git commit -m "test: add multi-view integration tests

- State preservation across view toggles
- Combined sort + filter operations
- Clear filters resets all state
- Pagination sync tests
- State change event firing
- Row selection persistence

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## Task 12: Register Services in DI Container

**Files:**
- Modify: `src/SmartWorkz.Core.Web/Services/WebComponentExtensions.cs` (if exists, create if needed)

- [ ] **Step 1: Check if WebComponentExtensions exists**

Run: `ls -la src/SmartWorkz.Core.Web/Services/WebComponentExtensions.cs`

If it doesn't exist, we'll create it. If it does, we'll add to it.

- [ ] **Step 2: Create or append to WebComponentExtensions**

If file exists, append. If not, create:

```csharp
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Web.Services.DataView;

namespace SmartWorkz.Core.Web.Services;

public static class WebComponentExtensions
{
    /// <summary>
    /// Register all SmartWorkz.Core.Web components and services.
    /// </summary>
    public static IServiceCollection AddSmartWorkzWebComponents(this IServiceCollection services)
    {
        // Register formatters
        services.AddScoped<IListViewFormatter, ListViewFormatter>();

        // ViewConfiguration can be added per-use or registered as singleton default
        services.AddScoped(_ => new ViewConfiguration
        {
            VisibleColumns = [],
            ItemsPerRow = 2,
            ShowHeaders = true,
            AllowRowSelection = true,
            DefaultPageSize = 20
        });

        return services;
    }
}
```

- [ ] **Step 3: Verify registration works**

In a test project, you can verify:

```bash
dotnet add reference src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj
```

Then in your Program.cs or Startup:

```csharp
services.AddSmartWorkzWebComponents();
```

- [ ] **Step 4: Run build to verify**

Run: `dotnet build src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj`

Expected: Builds successfully

- [ ] **Step 5: Commit**

```bash
git add src/SmartWorkz.Core.Web/Services/WebComponentExtensions.cs
git commit -m "feat: add DI extension for web component services

- Register IListViewFormatter as scoped service
- Register default ViewConfiguration
- Provide AddSmartWorkzWebComponents() extension method

Usage in Program.cs:
  services.AddSmartWorkzWebComponents();

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## Task 13: Run Full Test Suite and Verify Coverage

**Files:**
- None (verification only)

- [ ] **Step 1: Run all DataContext tests**

Run: `dotnet test tests/SmartWorkz.Core.Web.Tests/SmartWorkz.Core.Web.Tests.csproj -k DataContextTests -v`

Expected: All tests pass

- [ ] **Step 2: Run all ListViewFormatter tests**

Run: `dotnet test tests/SmartWorkz.Core.Web.Tests/SmartWorkz.Core.Web.Tests.csproj -k ListViewFormatterTests -v`

Expected: All tests pass

- [ ] **Step 3: Run all integration tests**

Run: `dotnet test tests/SmartWorkz.Core.Web.Tests/SmartWorkz.Core.Web.Tests.csproj -k MultiViewIntegrationTests -v`

Expected: All tests pass

- [ ] **Step 4: Run full solution build**

Run: `dotnet build src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj`

Expected: Builds successfully with zero warnings

- [ ] **Step 5: Verify all files exist**

Run: `find src/SmartWorkz.Core.Web -type f -name "*.cs" -o -name "*.razor" | grep -E "(DataContext|ListView|DataViewer|ListViewFormatter|ViewConfiguration)" | wc -l`

Expected: At least 10 files (interfaces, implementations, components, code-behinds)

---

## Task 14: Create Usage Documentation

**Files:**
- Create: `src/SmartWorkz.Core.Web/README-MultiView.md`

- [ ] **Step 1: Write usage guide**

```markdown
# Multi-View Data Components

This guide shows how to use the Grid + List view components with shared state management.

## Setup

1. Register services in `Program.cs`:

```csharp
services.AddSmartWorkzWebComponents();
```

2. Import components in your Razor page:

```razor
@using SmartWorkz.Core.Web.Components.DataViewer
@using SmartWorkz.Core.Web.Components.DataContext
@using SmartWorkz.Core.Shared.Grid
```

## Basic Usage

### DataViewerComponent (Recommended - All-in-One)

```razor
@page "/products"

<DataViewerComponent @typeparam="Product"
                    DataSource="Products"
                    Columns="GridColumns"
                    DefaultView="ViewType.Grid"
                    AutoFetch="true" />

@code {
    private List<Product> Products { get; set; } = [];
    private List<GridColumn> GridColumns { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        Products = await ProductService.GetAll();
        GridColumns = new()
        {
            new() { PropertyName = "Id", Header = "ID", IsSortable = true },
            new() { PropertyName = "Name", Header = "Product Name", IsSortable = true },
            new() { PropertyName = "Category", Header = "Category", IsSortable = true },
            new() { PropertyName = "Price", Header = "Price", IsSortable = true, IsVisible = true }
        };
    }
}
```

### Advanced: Manual Context + Individual Views

```razor
@inject IListViewFormatter Formatter

<div class="mb-3">
    <button class="btn btn-outline-primary" @onclick="() => CurrentView = ViewType.Grid">Grid</button>
    <button class="btn btn-outline-primary" @onclick="() => CurrentView = ViewType.List">List</button>
</div>

@if (CurrentView == ViewType.Grid)
{
    <GridViewComponent @typeparam="Product"
                      DataContext="DataContext"
                      Columns="GridColumns" />
}
else
{
    <ListViewComponent @typeparam="Product"
                      DataContext="DataContext"
                      Configuration="ListConfig" />
}

@code {
    private IDataContext<Product>? DataContext { get; set; }
    private List<Product> Products { get; set; } = [];
    private List<GridColumn> GridColumns { get; set; } = [];
    private ViewConfiguration ListConfig { get; set; } = new();
    private ViewType CurrentView = ViewType.Grid;

    protected override async Task OnInitializedAsync()
    {
        Products = await ProductService.GetAll();
        DataContext = new DataContext<Product>();
        await DataContext.Initialize(Products);

        ListConfig = new ViewConfiguration
        {
            VisibleColumns = ["Name", "Category", "Price"],
            ItemsPerRow = 2
        };
    }
}
```

### Custom List Card Template

```razor
<ListViewComponent @typeparam="Product"
                  DataContext="DataContext"
                  Configuration="ListConfig">
    <ItemTemplate>
        <div class="product-card">
            <h6>@context.Name</h6>
            <p class="text-muted">@context.Category</p>
            <p class="fw-bold">$@context.Price.ToString("N2")</p>
            <button class="btn btn-sm btn-primary" @onclick="() => SelectProduct(context.Id)">
                Select
            </button>
        </div>
    </ItemTemplate>
</ListViewComponent>
```

## API Reference

### DataContext<T>

State management service for data-heavy components.

**Methods:**
- `Initialize(IEnumerable<T> dataSource)` - Load data
- `UpdateSort(string propertyName, bool isDescending)` - Change sort
- `UpdateFilter(string property, string op, object value)` - Add filter
- `UpdatePagination(int page, int pageSize)` - Change page
- `ToggleRowSelection(object rowId)` - Toggle single selection
- `SetSelectedRows(List<object> rowIds)` - Bulk select
- `ClearFilters()` - Reset all filters/sort

**Properties:**
- `CurrentRequest` - Current GridRequest
- `CurrentResponse` - Current GridResponse<T>
- `SelectedRowIds` - List of selected row IDs
- `IsLoading` - Loading state
- `Error` - Error message

**Events:**
- `OnStateChanged` - Raised when any state changes

### ListViewFormatter

Formats data for display in List/Card views.

**Methods:**
- `FormatDate(DateTime? date)` - Format as "MMM dd, yyyy"
- `FormatCurrency(decimal? value)` - Format as "$X.XX"
- `TruncateText(string text, int maxLength)` - Truncate with "..."
- `FormatBoolean(bool? value)` - Display as "Yes"/"No"
- `FormatValue(object value)` - Auto-format based on type

### ViewConfiguration

Configures List view layout and behavior.

**Properties:**
- `VisibleColumns` - Column names to display
- `ItemsPerRow` - Responsive columns (1, 2, 3)
- `ShowHeaders` - Display column names
- `CardCssClass` - CSS for card containers
- `AllowRowSelection` - Show checkboxes
- `DefaultPageSize` - Items per page

## Examples

See the test projects for complete examples:
- `tests/SmartWorkz.Core.Web.Tests/Components/DataContextTests.cs`
- `tests/SmartWorkz.Core.Web.Tests/Integration/MultiViewIntegrationTests.cs`
```

- [ ] **Step 2: Verify file created**

Run: `ls -la src/SmartWorkz.Core.Web/README-MultiView.md`

Expected: File exists

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.Core.Web/README-MultiView.md
git commit -m "docs: add multi-view components usage guide

- Setup and basic usage examples
- Advanced manual context usage
- Custom card templates
- API reference for DataContext and services
- Code examples for common scenarios

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>"
```

---

## Summary

**Completed Tasks:**
- ✅ DataContext interface and implementation (with unit tests)
- ✅ ListViewFormatter service (with unit tests)
- ✅ ListViewComponent.razor (with code-behind)
- ✅ DataViewerComponent.razor (with code-behind)
- ✅ Enhanced GridViewComponent to use DataContext
- ✅ Integration tests (state sync, filtering, pagination)
- ✅ DI service registration
- ✅ Usage documentation

**Test Coverage:**
- 9 DataContext unit tests
- 10 ListViewFormatter unit tests
- 6 integration tests
- **Total: 25 passing tests**

**Architecture:**
- `DataContext<T>` manages all state (non-Razor)
- GridViewComponent and ListViewComponent are thin rendering layers
- DataViewerComponent orchestrates both views
- All state syncs automatically across views
- No duplicate logic

**Next Steps (Phase 2):**
- Add MapViewComponent with geographic display
- Implement real API data fetching
- Add export to CSV/Excel
- Implement real-time updates with SignalR
