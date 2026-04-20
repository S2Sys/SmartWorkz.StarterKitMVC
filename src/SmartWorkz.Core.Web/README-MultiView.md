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
- `ToggleSelectAll(bool isChecked)` - Select/deselect all
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
- `tests/SmartWorkz.Core.Web.Tests/Components/DataContextTests.cs` - State management tests
- `tests/SmartWorkz.Core.Web.Tests/Services/ListViewFormatterTests.cs` - Formatter tests
- `tests/SmartWorkz.Core.Web.Tests/Integration/MultiViewIntegrationTests.cs` - Integration tests

## State Synchronization

The key feature: state automatically syncs across Grid and List views:

1. User sorts in Grid view
2. DataContext.UpdateSort() is called
3. CurrentRequest is updated
4. OnStateChanged event fires
5. Both Grid and List components re-render
6. User switches to List view - data is already sorted!

## Error Handling

Each view component displays:
- **Loading spinner** while data is being fetched
- **Error message** if a fetch fails, with Retry button
- **Empty state** if no data matches the current filters

## Performance Notes

- DataContext uses reflection to detect row IDs (caches result)
- ListViewFormatter uses switch expressions for type-based formatting
- Grid/List components subscribe to StateChanged events for efficient re-renders
- No data re-fetching when switching views

## Next Steps (Phase 2)

- MapViewComponent with geospatial display
- Real API data fetching with HttpClient
- Export to CSV/Excel functionality
- Real-time updates with SignalR
- Advanced filtering UI builder
