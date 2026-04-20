# SmartWorkz.Core.Web

ASP.NET Core web components, Razor components, and UI utilities for SmartWorkz applications.

## Getting Started

### Prerequisites
- .NET 9.0 or higher
- ASP.NET Core 9.0+
- Visual Studio 2022+ or VS Code

### Installation

Add to your Blazor or Razor Pages project:

```xml
<ProjectReference Include="path/to/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj" />
```

Register in `Program.cs`:

```csharp
builder.Services.AddSmartWorkzWeb();
```

### Basic Usage

```csharp
@using SmartWorkz.Core.Web.Components

<GridComponent Data="@items" />
<ListViewComponent Items="@data" />
<DataViewerComponent Content="@details" />
```

## Project Structure

- **Components/** — Reusable Razor components
  - `GridComponent` — Data grid with sorting, filtering, pagination
  - `ListViewComponent` — List display with custom templates
  - `DataViewerComponent` — Generic data presentation
- **Utilities/** — UI helper utilities
- **Extensions/** — Web extension methods
- **Models/** — UI and view models

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Components | 2.3.0 | Blazor component support |
| Microsoft.AspNetCore.Mvc.Razor | 2.3.0 | Razor view engine |
| Microsoft.AspNetCore.Mvc.ViewFeatures | 2.3.0 | View feature support |

## Configuration

### Grid Component

```csharp
<GridComponent Data="@employees" PageSize="25" Sortable="true">
    <Columns>
        <GridColumn Property="Name" Header="Employee Name" />
        <GridColumn Property="Department" Header="Dept" />
    </Columns>
</GridComponent>
```

### List View Component

```csharp
<ListViewComponent Items="@products">
    <Template>
        <div class="product-card">
            <h3>@context.Name</h3>
            <p>@context.Description</p>
        </div>
    </Template>
</ListViewComponent>
```

### Data Viewer Component

```csharp
<DataViewerComponent Content="@userData" />
```

## Features

### GridComponent
- **Pagination** — Configurable page sizes
- **Sorting** — Click headers to sort ascending/descending
- **Filtering** — Built-in column filtering
- **Responsive** — Mobile-friendly layout
- **Virtual Scrolling** — For large datasets (10K+ rows)

### ListViewComponent
- **Custom Templates** — RenderFragment support
- **Styling** — CSS class customization
- **Selection** — Single/multi-select modes
- **Virtualization** — Optimized rendering

### DataViewerComponent
- **Flexible Display** — Supports nested objects
- **Formatting** — Custom value formatters
- **Validation** — Display validation errors

## Performance Best Practices

### Virtual Scrolling for Large Datasets

```csharp
<GridComponent Data="@largeDataSet" EnableVirtualization="true" />
```

For datasets with 10K+ rows, virtual scrolling prevents DOM bloat.

### Component Re-render Optimization

```csharp
@if (someCondition)
{
    <GridComponent Data="@data" key="@data.Id" />
}
```

## Styling

Components use Bootstrap 5 by default. Customize with CSS:

```css
.grid-container {
    --grid-row-height: 40px;
    --grid-header-bg: #f8f9fa;
}
```

## Accessibility

- ARIA labels on interactive elements
- Keyboard navigation support
- Screen reader friendly
- High contrast mode compatible

## Testing

```csharp
[Test]
public void GridComponent_DisplaysData()
{
    var cut = RenderComponent<GridComponent>(
        parameters => parameters
            .Add(p => p.Data, testData));
    
    cut.MarkupMatches("<div class=\"grid\">...</div>");
}
```

## Troubleshooting

**Component not rendering:**
- Ensure project is registered in `Program.cs`
- Check that namespace is included in `_Imports.razor`
- Verify data binding syntax

**Grid scrolling issues:**
- Set container height explicitly
- Enable virtual scrolling for large datasets
- Check CSS height constraints

## Contributing

- Follow Razor component naming conventions
- Implement `IAsyncDisposable` for resource cleanup
- Test with various screen sizes
- Document component parameters
