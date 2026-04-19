# SmartWorkz Grid Component System - Complete Wiki

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Components](#components)
4. [Services](#services)
5. [Models](#models)
6. [Usage Examples](#usage-examples)
7. [Best Practices](#best-practices)
8. [Real-World Scenarios](#real-world-scenarios)

---

## Overview

The SmartWorkz Grid Component System is a reusable, enterprise-grade data grid solution built on ASP.NET Core 9.0 with Razor Components and TagHelpers. It provides:

- **Server-side paging, sorting, and filtering**
- **Row selection with checkboxes**
- **CSV export functionality**
- **Column customization (show/hide, reorder)**
- **Both API and in-memory data sources**
- **Bootstrap 5 responsive styling**
- **Comprehensive error handling**

### Key Benefits
✅ **Code Reuse** — Use across all SmartWorkz projects (Web, MAUI, Desktop)  
✅ **Performance** — Server-side operations scale to 100k+ rows  
✅ **Flexibility** — Adapts to any data model or UI requirement  
✅ **Maintainability** — Centralized grid logic reduces duplicate code  

---

## Architecture

### Layered Design

```
┌─────────────────────────────────────────────────┐
│  Presentation Layer (Razor Components/Views)    │
│  - GridComponent, GridTagHelper                 │
└────────────┬────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────┐
│  Service Layer (Core.Web)                       │
│  - GridDataProvider, GridExportService          │
│  - GridStateManager                             │
└────────────┬────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────┐
│  Data Models (Core.Shared)                      │
│  - GridColumn, GridRequest, GridResponse        │
│  - IGridDataProvider (interface)                │
└────────────┬────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────┐
│  Data Source Layer                              │
│  - API Endpoints / Database / In-Memory         │
└─────────────────────────────────────────────────┘
```

### Platform Independence

The Grid system is designed to work across platforms:

```
SmartWorkz.Core.Shared (Models & Interfaces)
    ↓
    ├─→ SmartWorkz.Core.Web (Web/Razor implementation)
    ├─→ SmartWorkz.Core.Maui (Mobile implementation - future)
    └─→ SmartWorkz.Core.Desktop (Desktop implementation - future)
```

---

## Components

### 1. GridComponent

**Full Name:** `SmartWorkz.Core.Web.Components.Grid.GridComponent`

**Purpose:** Main grid rendering engine that handles all data display, pagination, sorting, and row selection.

**Type Parameters:**
- `<T>` — Data model type (must be a class)

**Parameters:**

| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| DataSource | IEnumerable<T> | [] | Source of grid data |
| Columns | List<GridColumn> | [] | Column definitions |
| PageSize | int | 20 | Rows per page |
| AllowRowSelection | bool | false | Enable checkboxes |
| AllowExport | bool | false | Enable export buttons |
| AllowColumnVisibilityToggle | bool | false | Show/hide columns |
| CustomCssClass | string | null | Additional CSS classes |
| RowTemplate | RenderFragment<T> | null | Custom row rendering |
| OnStateChanged | EventCallback | - | State change notifications |

**Usage:**

```razor
<GridComponent TItem="ProductDto" 
               DataSource="@Products" 
               Columns="@GridColumns"
               PageSize="25"
               AllowRowSelection="true"
               AllowExport="true"
               OnStateChanged="@HandleStateChange">
</GridComponent>

@code {
    private List<ProductDto> Products { get; set; } = [];
    private List<GridColumn> GridColumns { get; set; } = [];

    protected override void OnInitialized()
    {
        GridColumns = new()
        {
            new() { PropertyName = "Id", DisplayName = "ID", IsSortable = true, Width = "10%" },
            new() { PropertyName = "Name", DisplayName = "Product Name", IsSortable = true },
            new() { PropertyName = "Price", DisplayName = "Price", IsFilterable = true, FilterType = "range" },
            new() { PropertyName = "Stock", DisplayName = "Stock Qty", IsSortable = true }
        };
        LoadProducts();
    }

    private void LoadProducts()
    {
        // Load from database or API
        Products = GetProductData();
    }

    private void HandleStateChange(GridStateChangedArgs args)
    {
        Console.WriteLine($"Page: {args.Request.Page}, Sort: {args.Request.SortBy}");
    }
}
```

### 2. GridColumnComponent

**Full Name:** `SmartWorkz.Core.Web.Components.Grid.GridColumnComponent`

**Purpose:** Renders individual grid cells with support for custom templates.

**Parameters:**

| Parameter | Type | Purpose |
|-----------|------|---------|
| Column | GridColumn | Column metadata |
| Item | object | Data item for this row |
| CustomTemplate | RenderFragment<GridCellContext> | Custom cell renderer |

### 3. GridFilterComponent

**Full Name:** `SmartWorkz.Core.Web.Components.Grid.GridFilterComponent`

**Purpose:** Renders filter UI (text, dropdown, date) based on column configuration.

**Supported Filter Types:**
- `"text"` — Text input box
- `"dropdown"` — Select list
- `"date"` — Date picker
- `"range"` — Min/max range

**Parameters:**

| Parameter | Type | Purpose |
|-----------|------|---------|
| Column | GridColumn | Column definition with FilterType |
| FilterOptions | List<object> | Dropdown options |
| OnFilterApplied | EventCallback<object> | Filter change callback |

### 4. GridRowSelectorComponent

**Full Name:** `SmartWorkz.Core.Web.Components.Grid.GridRowSelectorComponent`

**Purpose:** Manages row checkbox selection state.

**Parameters:**

| Parameter | Type | Purpose |
|-----------|------|---------|
| RowId | object | Unique identifier for row |
| IsSelected | bool | Current selection state |
| OnSelectionChanged | EventCallback<bool> | Selection change callback |

---

## Services

### 1. GridDataProvider

**Full Name:** `SmartWorkz.Core.Web.Services.Grid.GridDataProvider`

**Purpose:** Fetches grid data from API endpoints or in-memory sources.

**Key Methods:**

```csharp
// Async fetch from API
public async Task<Result<GridResponse<T>>> GetDataAsync<T>(
    GridRequest request, 
    CancellationToken cancellationToken = default)

// In-memory operation (static)
public static PagedList<T> ApplyGridLogic<T>(
    IEnumerable<T> source, 
    GridRequest request)
```

**API Endpoint Contract:**

Your backend should implement an endpoint that:
- Accepts `GridRequest` (page, pageSize, sortBy, sortDescending, filters)
- Returns `Result<GridResponse<T>>`

**Example Backend Implementation:**

```csharp
[HttpPost("api/products/grid")]
public async Task<ActionResult<Result<GridResponse<ProductDto>>>> GetProductGrid(
    [FromBody] GridRequest request)
{
    try
    {
        var query = _dbContext.Products.AsQueryable();

        // Apply filters
        if (request.Filters?.ContainsKey("Status") == true)
            query = query.Where(p => p.Status == request.Filters["Status"].ToString());

        if (request.Filters?.ContainsKey("MinPrice") == true && 
            decimal.TryParse(request.Filters["MinPrice"].ToString(), out var minPrice))
            query = query.Where(p => p.Price >= minPrice);

        // Apply search
        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.Where(p => 
                p.Name.Contains(request.SearchTerm) ||
                p.Description.Contains(request.SearchTerm));

        // Apply sorting
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            query = request.SortDescending
                ? query.OrderByDescending(p => EF.Property<object>(p, request.SortBy))
                : query.OrderBy(p => EF.Property<object>(p, request.SortBy));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.StockQuantity,
                Status = p.Status
            })
            .ToListAsync();

        var response = new GridResponse<ProductDto>
        {
            Data = PagedList<ProductDto>.Create(items, request.Page, request.PageSize, totalCount),
            Columns = GetProductColumns(),
            FilterOptions = GetProductFilterOptions()
        };

        return Ok(Result<GridResponse<ProductDto>>.Success(response));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Grid data fetch failed");
        return BadRequest(Result<GridResponse<ProductDto>>.Failure(
            new Error("GridError", ex.Message)));
    }
}
```

### 2. GridExportService

**Full Name:** `SmartWorkz.Core.Web.Services.Grid.GridExportService`

**Purpose:** Exports grid data to CSV format.

**Key Methods:**

```csharp
// Export to CSV string
public string ExportToCsv<T>(
    List<T> data,
    List<GridColumn> columns,
    GridExportOptions options)

// Export to Excel (placeholder for Phase 2)
public byte[] ExportToExcel<T>(
    List<T> data,
    List<GridColumn> columns,
    GridExportOptions options)
```

**Example Usage:**

```csharp
private async Task ExportToCsv()
{
    var service = new GridExportService();
    var csv = service.ExportToCsv(
        data: CurrentPageData,
        columns: GridColumns,
        options: new GridExportOptions
        {
            Format = "csv",
            FileName = "products_export",
            IncludeHeaders = true,
            SelectedRowsOnly = SelectedRowIds.Any(),
            IncludeColumns = ["Id", "Name", "Price"],
            ExcludeColumns = ["InternalNotes"]
        });

    // Trigger browser download
    await JS.InvokeVoidAsync("downloadFile", csv, "products.csv");
}
```

### 3. GridStateManager

**Full Name:** `SmartWorkz.Core.Web.Services.Grid.GridStateManager`

**Purpose:** Manages grid state (pagination, sorting, filtering, selection) and notifies listeners of changes.

**Key Methods:**

```csharp
// Update entire request
public void UpdateRequest(GridRequest request)

// Pagination
public void UpdatePagination(int page, int pageSize)

// Sorting
public void UpdateSort(string? sortBy, bool descending)

// Filtering
public void UpdateFilters(Dictionary<string, object>? filters)
public void SetFilter(string columnName, object value)
public void RemoveFilter(string columnName)
public void ClearFilters()

// Row selection
public void SetSelectedRows(List<object> rowIds)
public void ToggleRowSelection(object rowId)

// State
public void SetLoading(bool isLoading)
public void SetError(string? errorMessage)
public void Reset()

// Events
public event Action? OnStateChanged;
```

**Properties:**

```csharp
public GridRequest CurrentRequest { get; }
public IReadOnlyList<object> SelectedRowIds { get; }
public string? ErrorMessage { get; }
public bool IsLoading { get; }
```

---

## Models

### 1. GridColumn

**Namespace:** `SmartWorkz.Core.Shared.Grid`

**Purpose:** Defines metadata for a grid column.

```csharp
public class GridColumn
{
    public string PropertyName { get; set; }           // Maps to data property
    public string DisplayName { get; set; }            // Header label
    public bool IsSortable { get; set; } = true;      // Enable sorting
    public bool IsFilterable { get; set; } = true;    // Enable filtering
    public bool IsEditable { get; set; } = false;     // Phase 2
    public string? FilterType { get; set; }            // "text"|"dropdown"|"date"|"range"
    public string? Width { get; set; }                 // "20%"|"200px"
    public string? CellTemplate { get; set; }          // Custom rendering hint
    public int Order { get; set; }                     // Display order
    public bool IsVisible { get; set; } = true;       // Show/hide toggle
}
```

### 2. GridRequest

**Namespace:** `SmartWorkz.Core.Shared.Grid`

**Purpose:** Data transfer object for grid request parameters.

```csharp
public record GridRequest(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false,
    string? SearchTerm = null,
    Dictionary<string, object>? Filters = null)
    : PagedQuery(Page, PageSize, SortBy, SortDescending, SearchTerm)
{
    // Filters keyed by column property name
    public Dictionary<string, object>? Filters { get; }
}
```

### 3. GridResponse<T>

**Namespace:** `SmartWorkz.Core.Shared.Grid`

**Purpose:** Response wrapper containing paged data and metadata.

```csharp
public class GridResponse<T>
{
    public required PagedList<T> Data { get; set; }          // Paged items
    public List<GridColumn> Columns { get; set; } = [];      // Column definitions
    public Dictionary<string, List<object>>? FilterOptions { get; set; }  // Dropdown options
}
```

### 4. GridExportOptions

**Namespace:** `SmartWorkz.Core.Shared.Grid`

**Purpose:** Configuration for data export operations.

```csharp
public class GridExportOptions
{
    public string Format { get; set; } = "csv";              // "csv"|"excel"
    public bool SelectedRowsOnly { get; set; } = false;      // Export selection only
    public List<string>? IncludeColumns { get; set; }        // Columns to include
    public List<string>? ExcludeColumns { get; set; }        // Columns to exclude
    public string FileName { get; set; } = "export";         // Without extension
    public bool IncludeHeaders { get; set; } = true;         // Include column headers
}
```

---

## Usage Examples

### Example 1: Product Inventory Grid (In-Memory)

**Scenario:** Display product inventory with local data source.

```razor
@page "/products"
@inject ILogger<ProductsPage> Logger

<div class="container-fluid mt-4">
    <h2>Product Inventory</h2>
    
    <GridComponent TItem="ProductDto"
                   DataSource="@Products"
                   Columns="@GridColumns"
                   PageSize="20"
                   AllowRowSelection="true"
                   AllowExport="true"
                   AllowColumnVisibilityToggle="true"
                   OnStateChanged="@OnGridStateChanged">
    </GridComponent>
</div>

@code {
    private List<ProductDto> Products { get; set; } = [];
    private List<GridColumn> GridColumns { get; set; } = [];
    private GridStateManager StateManager { get; set; } = new();

    protected override void OnInitialized()
    {
        InitializeColumns();
        LoadProducts();
    }

    private void InitializeColumns()
    {
        GridColumns = new()
        {
            new() 
            { 
                PropertyName = "Id", 
                DisplayName = "Product ID", 
                IsSortable = true,
                Width = "10%"
            },
            new() 
            { 
                PropertyName = "Name", 
                DisplayName = "Product Name", 
                IsSortable = true,
                IsFilterable = true,
                FilterType = "text"
            },
            new() 
            { 
                PropertyName = "Category", 
                DisplayName = "Category", 
                IsSortable = true,
                IsFilterable = true,
                FilterType = "dropdown"
            },
            new() 
            { 
                PropertyName = "Price", 
                DisplayName = "Unit Price", 
                IsSortable = true,
                IsFilterable = true,
                FilterType = "range",
                Width = "15%"
            },
            new() 
            { 
                PropertyName = "StockQuantity", 
                DisplayName = "Stock", 
                IsSortable = true,
                Width = "10%"
            },
            new() 
            { 
                PropertyName = "Status", 
                DisplayName = "Status", 
                IsSortable = true,
                IsFilterable = true,
                FilterType = "dropdown"
            }
        };
    }

    private void LoadProducts()
    {
        // In real scenario, fetch from database
        Products = new()
        {
            new() { Id = 1, Name = "Laptop", Category = "Electronics", Price = 999.99m, StockQuantity = 15, Status = "Active" },
            new() { Id = 2, Name = "Mouse", Category = "Electronics", Price = 29.99m, StockQuantity = 150, Status = "Active" },
            new() { Id = 3, Name = "Keyboard", Category = "Electronics", Price = 79.99m, StockQuantity = 85, Status = "Active" },
            new() { Id = 4, Name = "Monitor", Category = "Electronics", Price = 299.99m, StockQuantity = 20, Status = "Discontinued" }
        };
    }

    private void OnGridStateChanged(GridStateChangedArgs args)
    {
        Logger.LogInformation("Grid state changed - Page: {Page}, Sort: {Sort}, Filters: {FilterCount}",
            args.Request.Page,
            args.Request.SortBy,
            args.Request.Filters?.Count ?? 0);
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
```

### Example 2: Customer Orders Grid (API Source)

**Scenario:** Display customer orders with server-side paging from API.

```razor
@page "/orders"
@inject HttpClient HttpClient
@inject IJSRuntime JS

<div class="container-fluid mt-4">
    <h2>Customer Orders</h2>
    
    <div class="mb-3">
        <button class="btn btn-success me-2" @onclick="ExportOrders">
            📥 Export to CSV
        </button>
        <span class="text-muted">
            @if (SelectedOrderIds.Any())
            {
                <text>@SelectedOrderIds.Count selected</text>
            }
        </span>
    </div>

    <GridComponent TItem="OrderDto"
                   DataSource="@CurrentOrders"
                   Columns="@GridColumns"
                   PageSize="25"
                   AllowRowSelection="true"
                   AllowExport="true"
                   OnStateChanged="@OnGridStateChanged">
    </GridComponent>
</div>

@code {
    private List<OrderDto> CurrentOrders { get; set; } = [];
    private List<GridColumn> GridColumns { get; set; } = [];
    private List<object> SelectedOrderIds { get; set; } = [];
    private GridStateManager StateManager { get; set; } = new();
    private GridExportService ExportService { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        InitializeColumns();
        await FetchOrders();
    }

    private void InitializeColumns()
    {
        GridColumns = new()
        {
            new() { PropertyName = "Id", DisplayName = "Order #", IsSortable = true, Width = "12%" },
            new() { PropertyName = "CustomerName", DisplayName = "Customer", IsSortable = true, IsFilterable = true, FilterType = "text" },
            new() { PropertyName = "OrderDate", DisplayName = "Order Date", IsSortable = true, IsFilterable = true, FilterType = "date" },
            new() { PropertyName = "Total", DisplayName = "Total Amount", IsSortable = true, Width = "15%" },
            new() { PropertyName = "Status", DisplayName = "Status", IsSortable = true, IsFilterable = true, FilterType = "dropdown" }
        };
    }

    private async Task FetchOrders()
    {
        try
        {
            var request = new GridRequest(
                Page: StateManager.CurrentRequest.Page,
                PageSize: StateManager.CurrentRequest.PageSize,
                SortBy: StateManager.CurrentRequest.SortBy,
                SortDescending: StateManager.CurrentRequest.SortDescending,
                Filters: StateManager.CurrentRequest.Filters
            );

            var response = await HttpClient.PostAsJsonAsync("api/orders/grid", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<Result<GridResponse<OrderDto>>>();
                if (result.IsSuccess)
                {
                    CurrentOrders = result.Value.Data.Items.ToList();
                }
            }
        }
        catch (Exception ex)
        {
            StateManager.SetError($"Failed to load orders: {ex.Message}");
        }
    }

    private async Task ExportOrders()
    {
        var csv = ExportService.ExportToCsv(
            data: CurrentOrders,
            columns: GridColumns,
            options: new GridExportOptions
            {
                Format = "csv",
                FileName = "orders_export",
                SelectedRowsOnly = SelectedOrderIds.Any(),
                IncludeHeaders = true
            });

        await JS.InvokeVoidAsync("downloadFile", csv, "orders.csv");
    }

    private async Task OnGridStateChanged(GridStateChangedArgs args)
    {
        StateManager = new GridStateManager();
        StateManager.UpdateRequest(args.Request);
        SelectedOrderIds = args.SelectedRowIds;
        await FetchOrders();
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
```

### Example 3: Using GridTagHelper (Simplified)

**Scenario:** Quick grid setup with TagHelper syntax.

```html
@{
    var columns = new List<GridColumn>
    {
        new() { PropertyName = "Id", DisplayName = "ID" },
        new() { PropertyName = "Name", DisplayName = "Name", IsSortable = true },
        new() { PropertyName = "Email", DisplayName = "Email", IsSortable = true }
    };
}

<grid data-source="@Model.Users" 
      data-page-size="25"
      data-allow-selection="true"
      data-allow-export="true">
    <!-- TagHelper will generate grid automatically -->
</grid>
```

---

## Best Practices

### 1. **Column Design**

✅ **DO:**
```csharp
var columns = new List<GridColumn>
{
    new() 
    { 
        PropertyName = "Id",
        DisplayName = "User ID",
        IsSortable = true,
        IsFilterable = false,
        Width = "10%"
    },
    new() 
    { 
        PropertyName = "Email",
        DisplayName = "Email Address",
        IsSortable = true,
        IsFilterable = true,
        FilterType = "text"
    }
};
```

❌ **DON'T:**
```csharp
// Missing DisplayName, inconsistent naming
new() { PropertyName = "id", IsSortable = true }
```

### 2. **Data Source Strategy**

| Scenario | Approach | Benefits |
|----------|----------|----------|
| < 500 rows | In-Memory | Simple, no API needed |
| 500-10k rows | Server-Side Paging | Scalable, responsive |
| 10k+ rows | Server-Side Pagination + Indexing | Optimal performance |

### 3. **Filtering**

```csharp
// ✅ DO: Normalize inputs
if (request.Filters?.ContainsKey("Price") == true &&
    decimal.TryParse(request.Filters["Price"].ToString(), out var price))
{
    query = query.Where(p => p.Price >= price);
}

// ❌ DON'T: Trust user input blindly
var price = Convert.ToDecimal(request.Filters["Price"]);  // Will crash
```

### 4. **Performance Optimization**

```csharp
// ✅ DO: Use IQueryable for lazy evaluation
var query = _db.Products.AsQueryable();
if (request.Filters?.ContainsKey("Category") == true)
    query = query.Where(p => p.Category == filterValue);

// Get count before pagination
var totalCount = await query.CountAsync();

// Then paginate
var items = await query.Skip(request.Skip).Take(request.Take).ToListAsync();

// ❌ DON'T: Materialize before filtering
var allProducts = await _db.Products.ToListAsync();  // Loads everything
var filtered = allProducts.Where(p => ...).Skip(request.Skip).Take(request.Take);
```

### 5. **Error Handling**

```csharp
// ✅ DO: Use Result<T> pattern
try
{
    var data = await FetchGridData(request);
    return Ok(Result<GridResponse<T>>.Success(data));
}
catch (ArgumentException ex)
{
    return BadRequest(Result<GridResponse<T>>.Failure(
        new Error("InvalidRequest", ex.Message)));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Grid data fetch failed");
    return StatusCode(500, Result<GridResponse<T>>.Failure(
        new Error("InternalError", "An unexpected error occurred")));
}
```

---

## Real-World Scenarios

### Scenario 1: Employee Directory

**Use Case:** HR department needs searchable, sortable employee list.

**Setup:**
```csharp
var columns = new List<GridColumn>
{
    new() { PropertyName = "EmployeeId", DisplayName = "ID", Width = "10%" },
    new() { PropertyName = "FirstName", DisplayName = "First Name", IsSortable = true, IsFilterable = true },
    new() { PropertyName = "LastName", DisplayName = "Last Name", IsSortable = true, IsFilterable = true },
    new() { PropertyName = "Department", DisplayName = "Dept", IsFilterable = true, FilterType = "dropdown" },
    new() { PropertyName = "Email", DisplayName = "Email" },
    new() { PropertyName = "HireDate", DisplayName = "Hired", IsSortable = true, IsFilterable = true, FilterType = "date" }
};
```

**Features Used:**
- Text filtering on names
- Dropdown filtering by department
- Date range filtering on hire date
- 50 rows per page
- Server-side sorting by any column

### Scenario 2: Sales Dashboard

**Use Case:** Sales team monitors daily order activity with export capability.

**Setup:**
```csharp
var columns = new List<GridColumn>
{
    new() { PropertyName = "OrderNumber", DisplayName = "Order #", IsSortable = true },
    new() { PropertyName = "SalesRep", DisplayName = "Rep", IsFilterable = true, FilterType = "dropdown" },
    new() { PropertyName = "Amount", DisplayName = "Amount", IsSortable = true },
    new() { PropertyName = "CommissionRate", DisplayName = "Commission %", IsFilterable = true, FilterType = "range" },
    new() { PropertyName = "Status", DisplayName = "Status", IsFilterable = true, FilterType = "dropdown" }
};
```

**Features Used:**
- Row selection for bulk operations
- CSV export for reporting
- Status filtering (Open, Closed, Pending)
- Commission range filtering
- Real-time updates every 60 seconds

### Scenario 3: Audit Log Viewer

**Use Case:** Security team reviews system audit logs with advanced filtering.

**Setup:**
```csharp
var columns = new List<GridColumn>
{
    new() { PropertyName = "Timestamp", DisplayName = "Time", IsSortable = true, IsFilterable = true, FilterType = "date", Width = "15%" },
    new() { PropertyName = "User", DisplayName = "User", IsFilterable = true, FilterType = "dropdown" },
    new() { PropertyName = "Action", DisplayName = "Action", IsFilterable = true, FilterType = "dropdown" },
    new() { PropertyName = "Resource", DisplayName = "Resource", IsFilterable = true, FilterType = "text" },
    new() { PropertyName = "Result", DisplayName = "Result", IsFilterable = true, FilterType = "dropdown" },
    new() { PropertyName = "Details", DisplayName = "Details" }
};
```

**Features Used:**
- Date range filtering for audit periods
- Multi-filter (User + Action + Result)
- 100+ rows per page with pagination
- CSV export for compliance reports
- No sorting allowed on Details column

---

## Integration Checklist

When integrating Grid into your SmartWorkz project:

- [ ] Add `SmartWorkz.Core.Shared` NuGet reference
- [ ] Add `SmartWorkz.Core.Web` NuGet reference
- [ ] Add grid.css to layout: `<link href="~/css/grid.css" rel="stylesheet" />`
- [ ] Implement `IGridDataProvider` for your data source
- [ ] Create grid columns based on your data model
- [ ] Test sorting, paging, and filtering end-to-end
- [ ] Implement API endpoint accepting `GridRequest`
- [ ] Add error handling in your data provider
- [ ] Test export functionality (if enabled)
- [ ] Verify responsive design on mobile devices

---

## FAQ

**Q: Can I use Grid with my existing data models?**  
A: Yes! Grid works with any C# class model via reflection.

**Q: What's the maximum row count Grid can handle?**  
A: Server-side paging handles unlimited rows efficiently.

**Q: Can I customize column rendering?**  
A: Yes, use `RowTemplate` parameter for custom cell content.

**Q: Does Grid support real-time updates?**  
A: Phase 1 uses polling/manual refresh. Phase 2 will add SignalR support.

**Q: Can I export to Excel?**  
A: CSV is implemented. Excel export (Phase 2) requires EPPlus NuGet package.

---

## Support & Contribution

For issues or contributions, contact the SmartWorkz development team.

**Documentation Location:** `docs/GRID_COMPONENT_WIKI.md`  
**Implementation Location:** `src/SmartWorkz.Core.Web/Components/Grid/`  
**Models Location:** `src/SmartWorkz.Core.Shared/Grid/`
