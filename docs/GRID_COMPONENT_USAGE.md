# Grid Component Usage Guide

## Overview

The Grid Component system provides reusable, customizable data grids for Razor Pages and MVC applications. It supports sorting, paging, filtering, row selection, and export—with both API and in-memory data sources.

## Basic Usage (TagHelper)

```html
<grid data-source="@Model.Products" data-page-size="25">
    <column property-name="Id" display-name="Product ID" sortable="true" />
    <column property-name="Name" display-name="Product Name" sortable="true" filterable="true" />
    <column property-name="Price" display-name="Price" sortable="true" />
    <column property-name="StockQuantity" display-name="Stock" />
</grid>
```

## Advanced Usage (Razor Component)

```razor
<GridComponent TItem="ProductDto" 
               DataSource="@Products" 
               Columns="@GridColumns"
               PageSize="20"
               AllowRowSelection="true"
               AllowExport="true"
               CustomCssClass="table-sm"
               OnStateChanged="@HandleGridStateChange">
</GridComponent>

@code {
    private List<ProductDto> Products { get; set; } = [];
    private List<GridColumn> GridColumns { get; set; } = [];

    protected override void OnInitialized()
    {
        GridColumns = new()
        {
            new() { PropertyName = "Name", DisplayName = "Product Name", IsSortable = true },
            new() { PropertyName = "Price", DisplayName = "Price", IsFilterable = true, FilterType = "range" }
        };
        Products = GetProductData();
    }

    private void HandleGridStateChange(GridStateChangedArgs args)
    {
        // Handle sorting, filtering, pagination changes
    }
}
```

## API Integration (Server-Side)

Create an API endpoint that accepts `GridRequest`:

```csharp
[HttpPost("api/grid/data")]
public async Task<ActionResult<Result<GridResponse<ProductDto>>>> GetGridData([FromBody] GridRequest request)
{
    try
    {
        var query = _dbContext.Products.AsQueryable();

        // Apply filters
        if (request.Filters?.ContainsKey("Status") == true)
            query = query.Where(p => p.Status == request.Filters["Status"].ToString());

        // Apply sorting
        if (!string.IsNullOrEmpty(request.SortBy))
            query = request.SortDescending
                ? query.OrderByDescending(p => EF.Property<object>(p, request.SortBy))
                : query.OrderBy(p => EF.Property<object>(p, request.SortBy));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync();

        var response = new GridResponse<ProductDto>
        {
            Data = PagedList<ProductDto>.Create(
                items.Select(p => new ProductDto { ... }).ToList(),
                request.Page,
                request.PageSize,
                totalCount),
            Columns = GetGridColumns()
        };

        return Ok(Result<GridResponse<ProductDto>>.Success(response));
    }
    catch (Exception ex)
    {
        return BadRequest(Result<GridResponse<ProductDto>>.Failure(
            new Error("GridDataError", ex.Message)));
    }
}
```

## Column Configuration

### Properties

| Property | Type | Description |
|----------|------|-------------|
| PropertyName | string | Maps to data object property |
| DisplayName | string | Header label |
| IsSortable | bool | Enable sort (default: true) |
| IsFilterable | bool | Enable filter (default: true) |
| FilterType | string | "text", "dropdown", "date", "range" |
| Width | string | "20%", "200px", etc. |
| IsEditable | bool | Enable editing (Phase 2) |
| CellTemplate | string | Custom rendering hint |
| IsVisible | bool | Show/hide column (default: true) |

### Filter Types

```csharp
new GridColumn 
{ 
    PropertyName = "Status",
    FilterType = "dropdown",
    // Server provides filter options in GridResponse.FilterOptions
}

new GridColumn 
{ 
    PropertyName = "CreatedDate",
    FilterType = "date"
}

new GridColumn 
{ 
    PropertyName = "Price",
    FilterType = "range"
    // Client interprets as min/max input
}
```

## Export

```csharp
// In component
private async Task ExportToCsv()
{
    var service = new GridExportService();
    var csv = service.ExportToCsv(
        data: CurrentPageData,
        columns: Columns,
        options: new GridExportOptions
        {
            Format = "csv",
            FileName = "products",
            IncludeHeaders = true,
            SelectedRowsOnly = StateManager.SelectedRowIds.Any()
        });

    // Trigger download
    await JS.InvokeVoidAsync("downloadFile", csv, "products.csv");
}
```

## Styling

### Bootstrap Classes

The grid uses standard Bootstrap 5 classes:
- `.table`, `.table-striped`, `.table-hover`
- `.pagination`, `.btn`, `.alert`
- `.form-control`, `.form-select`

### Custom CSS

Override or extend styles via `custom-grid.css`:

```css
.grid-container thead th {
    background-color: #2c3e50;
    color: white;
}

.grid-container tbody tr:hover {
    background-color: #ecf0f1;
}
```

## Error Handling

The grid displays errors inline with a dismissible alert:

```
[Error] API request failed. [Retry button]
```

Errors are caught and logged; request can be retried without reloading.

## Testing

### Unit Tests

```csharp
[Fact]
public void GridDataProvider_ShouldApplySorting()
{
    var data = new[] { /*...*/ };
    var request = new GridRequest(SortBy: "Name");
    var result = GridDataProvider.ApplyGridLogic(data, request);
    Assert.Equal("A", result.Items.First().Name);
}
```

### Integration Tests

- Mock HTTP API responses
- Verify `GridResponse<T>` serialization
- Test paging boundary conditions (empty result, single page, etc.)

## Performance Considerations

- **Server-side paging:** Always use for datasets > 1000 rows
- **In-memory filtering:** Safe for < 500 rows
- **Lazy loading:** Not yet supported (Phase 2)
- **Virtual scrolling:** Not yet supported (Phase 2)

## Browser Support

- Chrome, Firefox, Safari, Edge (latest)
- IE11 not supported (uses modern JS features)

## Known Limitations

- **Phase 1:** Inline editing not yet implemented
- **Phase 1:** Real-time updates (WebSocket) not supported
- **Phase 1:** Drag-to-reorder columns not supported
- **Phase 1:** Multi-column sorting not supported

See `2026-04-19-grid-component-design.md` for full roadmap.
