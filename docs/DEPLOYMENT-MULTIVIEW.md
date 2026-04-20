# Multi-View Data Components - Deployment & Integration Guide

## Integration Steps

### 1. Register Services (Program.cs)

```csharp
// Add this to your Program.cs
services.AddSmartWorkzWebComponents();
```

### 2. Import Components

In any Razor page or component:

```razor
@using SmartWorkz.Core.Web.Components.DataViewer
@using SmartWorkz.Core.Web.Components.DataContext
@using SmartWorkz.Core.Shared.Grid
```

### 3. Basic Implementation

```razor
<DataViewerComponent @typeparam="YourEntity"
                    DataSource="YourData"
                    Columns="YourColumns"
                    DefaultView="ViewType.Grid"
                    AutoFetch="true" />

@code {
    private List<YourEntity> YourData { get; set; } = [];
    private List<GridColumn> YourColumns { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        YourData = await YourService.GetAll();
        SetupColumns();
    }

    private void SetupColumns()
    {
        YourColumns = new()
        {
            new() { PropertyName = "Id", Header = "ID", IsSortable = true },
            new() { PropertyName = "Name", Header = "Name", IsSortable = true },
            // Add more columns...
        };
    }
}
```

## Configuration Options

### DataViewerComponent Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| DataSource | IEnumerable<T> | null | Initial data to display |
| Columns | List<GridColumn> | [] | Column definitions |
| DefaultView | ViewType | Grid | Starting view (Grid/List) |
| AutoFetch | bool | true | Load data on initialization |
| ItemTemplate | RenderFragment<T> | null | Custom card template for List view |

### ViewConfiguration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| VisibleColumns | List<string> | [] | Columns to show in List view |
| ItemsPerRow | int | 2 | Responsive grid columns (1-4) |
| ShowHeaders | bool | true | Display column headers |
| CardCssClass | string | "card h-100" | CSS classes for cards |
| AllowRowSelection | bool | true | Show checkboxes |
| DefaultPageSize | int | 20 | Rows per page |

## Advanced: Manual DataContext

```csharp
// For advanced scenarios, create DataContext manually
var context = new DataContext<Product>();
await context.Initialize(products);

// Subscribe to state changes
context.OnStateChanged += () => Console.WriteLine("State changed!");

// Perform operations
await context.UpdateSort("Name", false);
await context.UpdateFilter("Category", "equals", "Electronics");
```

## Using Filter Builder

```razor
<FilterBuilderComponent @typeparam="Product"
                       DataContext="DataContext"
                       AvailableColumns="FilterColumns"
                       OnFiltersChanged="HandleFiltersChanged" />

@code {
    private async Task HandleFiltersChanged(List<FilterBuilderComponent.FilterDefinition> filters)
    {
        Console.WriteLine($"Active filters: {filters.Count}");
    }
}
```

## Demo Page

A working example is available at: `/demo/data-viewer`

Features demonstrated:
- DataViewerComponent with sample products
- Grid and List view toggle
- Sorting and pagination
- Custom card templates
- Row selection

## Troubleshooting

### "DataContext is not initialized"
- Ensure AutoFetch is true or call Initialize() explicitly
- Verify DataSource is not null

### Views not syncing state
- Confirm both views receive the same DataContext instance
- Check that OnStateChanged event is being subscribed

### Performance issues with large datasets
- Consider server-side pagination (implement in GridDataProvider)
- Reduce ItemsPerRow for List view
- Implement virtual scrolling for 1000+ rows

## Performance Optimization

For datasets > 1000 rows:

1. **Server-Side Pagination:**
   - Implement GridDataProvider.FetchAsync()
   - Return only current page data

2. **Virtual Scrolling:**
   - Use library like Virtualize component
   - Improves rendering performance

3. **Lazy Loading:**
   - Load images on demand
   - Defer secondary data fetches

## Next Steps

- See README-MultiView.md for API documentation
- Check test files for usage examples
- Implement custom ItemTemplate for domain-specific formatting

## Support

For issues or questions:
1. Review test examples: `tests/SmartWorkz.Core.Web.Tests/`
2. Check demo page: `/demo/data-viewer`
3. Review API reference: `src/SmartWorkz.Core.Web/README-MultiView.md`
