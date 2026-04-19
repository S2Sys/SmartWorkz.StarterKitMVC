# Grid Component System Design

**Date:** 2026-04-19  
**Project:** SmartWorkz.Core (shared grid infrastructure for Web, MAUI, Desktop)  
**Phase:** Phase 1 — Core.Shared + Core.Web  
**Status:** Design Approved

---

## Executive Summary

Build a reusable, customizable grid component system with sorting, paging, filtering, row selection, inline editing, and export capabilities. The architecture is platform-agnostic at the Core.Shared level, with Web-specific implementations in Core.Web (TagHelpers + Razor Components).

This enables code reuse across Web, MAUI (mobile), and Desktop platforms in future phases.

---

## 1. Architecture Overview

### Three-Layer Design

**1. Core.Shared Layer** — Platform-agnostic models and contracts:
- `GridColumn` (column definition)
- `GridRequest` (extends `PagedQuery` with filters)
- `GridResponse<T>` (wraps `PagedList<T>`)
- `IGridDataProvider` interface (contract for data fetching)
- Configuration models for customization

**2. Core.Web Layer** — Web-specific implementations:
- `GridComponent.razor` (parent component managing state)
- `GridColumnComponent.razor` (column rendering)
- `GridFilterComponent.razor` (filter UI)
- `GridRowSelectorComponent.razor` (row checkboxes)
- `GridTagHelper` (high-level wrapper for simple use cases)
- `GridDataProvider` (implements `IGridDataProvider` for Web)
- `GridExportService` (CSV/Excel export)
- `GridStateManager` (tracks sorting/paging/filtering state)

**3. Supporting Services** (Core.Web):
- Format utilities for cell rendering
- JavaScript interop for client-side interactions

### Platform Independence

Core.Shared contains zero UI code. All platform-specific rendering (Razor, MAUI, WPF) implements `IGridDataProvider` and consumes the shared models. Future Core.Maui and Core.Desktop projects will follow the same pattern.

---

## 2. Data Contracts

### GridColumn (Core.Shared)

```csharp
public class GridColumn
{
    public string PropertyName { get; set; }           // Maps to PagedQuery.SortBy
    public string DisplayName { get; set; }            // Header label
    public bool IsSortable { get; set; } = true;
    public bool IsFilterable { get; set; } = true;
    public bool IsEditable { get; set; } = false;
    public string? FilterType { get; set; }            // "text", "dropdown", "date", etc.
    public string? Width { get; set; }                 // CSS class or pixel value
    public string? CellTemplate { get; set; }          // Custom rendering hint
    public int Order { get; set; }                     // Display order
    public bool IsVisible { get; set; } = true;        // Show/hide toggle
}
```

### GridRequest (Core.Shared)

Extends `PagedQuery`:
```csharp
public record GridRequest(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false,
    string? SearchTerm = null,
    Dictionary<string, object>? Filters = null)
    : PagedQuery(Page, PageSize, SortBy, SortDescending, SearchTerm);
```

### GridResponse<T> (Core.Shared)

Wraps `PagedList<T>`:
```csharp
public class GridResponse<T>
{
    public PagedList<T> Data { get; set; }
    public List<GridColumn> Columns { get; set; }
    public Dictionary<string, List<object>>? FilterOptions { get; set; } // For dropdowns
}
```

---

## 3. Request/Response Flow

### User Interaction → API → Grid Update

1. **User Action** (sort header click, filter input, page change)
   ↓
2. **GridComponent captures event** and builds `GridRequest`
   ↓
3. **IGridDataProvider.GetDataAsync(GridRequest)** is called
   ↓
4. **Web Implementation** (GridDataProvider):
   - For API sources: POST `GridRequest` to server endpoint
   - For in-memory sources: Apply sorting/filtering/paging in-memory
   ↓
5. **Server Response**: `Result<GridResponse<T>>`
   ↓
6. **GridComponent updates state** and re-renders

### Data Source Configuration

**API Endpoint:**
```csharp
<Grid DataSource="https://api/products" Columns="@columns" />
```

**In-Memory (IEnumerable):**
```csharp
<Grid DataSource="@localProducts" Columns="@columns" IsServerSide="false" />
```

---

## 4. Component Structure (Core.Web)

### GridComponent.razor

**Responsibilities:**
- Manages grid state (current page, sort, filters, selected rows)
- Coordinates child components
- Handles API requests via `IGridDataProvider`
- Re-renders on state changes

**Public Parameters:**
```csharp
[Parameter] public IEnumerable<T> DataSource { get; set; }
[Parameter] public string? ApiEndpoint { get; set; }
[Parameter] public List<GridColumn> Columns { get; set; }
[Parameter] public int PageSize { get; set; } = 20;
[Parameter] public bool AllowRowSelection { get; set; } = false;
[Parameter] public bool AllowExport { get; set; } = false;
[Parameter] public bool AllowColumnVisibilityToggle { get; set; } = false;
[Parameter] public string? CustomCssClass { get; set; }
[Parameter] public RenderFragment? RowTemplate { get; set; }  // Custom row rendering
[Parameter] public EventCallback<GridStateChangedArgs> OnStateChanged { get; set; }
```

### GridColumnComponent.razor

**Responsibilities:**
- Renders column header with sort indicator
- Renders cells with optional custom templates
- Passes sort/filter events to parent

**Public Parameters:**
```csharp
[Parameter] public GridColumn Column { get; set; }
[Parameter] public object Item { get; set; }
[Parameter] public RenderFragment<GridCellContext>? CellTemplate { get; set; }
```

### GridFilterComponent.razor

**Responsibilities:**
- Renders filter UI (text input, dropdown, date picker based on FilterType)
- Sends filter change events to parent

### GridRowSelectorComponent.razor

**Responsibilities:**
- Renders row checkbox and "select all" header checkbox
- Manages selection state

### GridTagHelper (High-level Wrapper)

Simplifies markup for 80% of use cases:
```html
<grid data-source="@Model.Products" 
      data-page-size="25"
      data-allow-selection="true"
      data-allow-export="true">
    <column property-name="Name" display-name="Product Name" sortable="true" />
    <column property-name="Price" display-name="Price" filterable="true" filter-type="range" />
</grid>
```

Under the hood, generates `GridComponent` + `GridColumn` markup.

---

## 5. Supporting Services (Core.Web)

### GridDataProvider (implements IGridDataProvider)

**For API sources:**
- Serializes `GridRequest` to JSON
- POSTs to API endpoint
- Deserializes `Result<GridResponse<T>>` response
- Handles errors and retries

**For in-memory sources:**
- Applies `SortBy` to IEnumerable using reflection
- Applies `SearchTerm` across all searchable columns
- Applies `Filters` to each column
- Applies `Skip` and `Take` for paging
- Returns `GridResponse<T>` with computed `PagedList<T>`

### GridExportService

**CSV Export:**
- Serializes selected rows to CSV
- Includes column headers
- Handles text escaping and quoting

**Excel Export:**
- Uses EPPlus or OpenXML
- Formats as `.xlsx`
- Includes column headers and auto-fit columns

**Configuration:**
- Export selected rows only or all filtered data
- Include/exclude specific columns

### GridStateManager

Tracks and persists:
- Current page number
- Sort column and direction
- Applied filters
- Selected rows
- Column visibility (if toggling enabled)

Optional: Save state to browser localStorage for resuming sessions.

---

## 6. Error Handling & Edge Cases

### API Request Failures
- Display error message in grid body (red banner)
- Provide "Retry" button to re-fetch
- Log error details to browser console

### Invalid Input
- **Invalid sort column:** Fallback to first sortable column
- **Invalid page number:** Reset to page 1
- **Invalid filter value:** Ignore filter, reset to unfiltered state

### Empty Results
- Display "No data found" message with option to clear filters
- Disable pagination controls if only one page

### State Management
- **Filter while on page 3:** Automatically reset to page 1 (filtered results may differ)
- **Concurrent filter + sort:** Last action wins (discard stale requests)
- **Row selection + filtering:** Clear selection when filter applied (prevent orphaned selections)

### Performance
- **Large datasets:** Server-side paging is enforced; no in-memory loading of 100k+ rows
- **Large exports:** Stream export in chunks to avoid timeout
- **Slow API:** Show loading spinner, implement request timeout (default 30s)

### Result Pattern
All Web API responses use `Result<GridResponse<T>>`:
```csharp
Result<GridResponse<T>> response = await provider.GetDataAsync(request);
if (response.IsSuccess)
{
    GridState.Update(response.Value);
}
else
{
    GridState.ShowError(response.Error);
}
```

---

## 7. Configuration & Customization

### Column-Level Options

| Option | Type | Default | Purpose |
|--------|------|---------|---------|
| PropertyName | string | (required) | Maps to data property |
| DisplayName | string | PropertyName | Header label |
| IsSortable | bool | true | Enable sort |
| IsFilterable | bool | true | Enable filter |
| IsEditable | bool | false | Enable inline editing |
| FilterType | string | "text" | UI for filter (text, dropdown, date) |
| Width | string | null | CSS width |
| CellTemplate | RenderFragment | null | Custom cell rendering |
| Order | int | 0 | Display order |
| IsVisible | bool | true | Show/hide |

### Grid-Level Options

| Option | Type | Default | Purpose |
|--------|------|---------|---------|
| DataSource | object | (required) | IEnumerable or API endpoint |
| Columns | List<GridColumn> | (required) | Column definitions |
| PageSize | int | 20 | Rows per page |
| AllowRowSelection | bool | false | Show checkboxes |
| AllowExport | bool | false | Show export buttons |
| AllowColumnVisibilityToggle | bool | false | Show column picker |
| CustomCssClass | string | null | Bootstrap/custom CSS classes |
| RowTemplate | RenderFragment | null | Custom row rendering |

---

## 8. Integration with SmartWorkz Infrastructure

### Reused Components
- `PagedList<T>` (SmartWorkz.Core.Shared.Pagination)
- `PagedQuery` (SmartWorkz.Core.Shared.Pagination)
- `SortDirection` enum (SmartWorkz.Core.Enums)
- `Result<T>` pattern (SmartWorkz.Core.Results)

### Future Enhancements
- Tenant isolation via `ITenantContext` (multi-tenant column filtering)
- Feature flags via `IFeatureFlagService` (conditional export, editing)
- Audit logging via `IAuditLogger` (track row edits)
- Event bus via `IEventPublisher` (broadcast grid updates)

---

## 9. Testing Strategy

### Unit Tests (Core.Shared)
- GridColumn validation
- GridRequest normalization (Normalize() method)
- GridResponse serialization/deserialization

### Integration Tests (Core.Web)
- GridDataProvider with mock API responses
- In-memory data source filtering/sorting/paging
- GridExportService CSV/Excel output

### UI Tests (Core.Web)
- Sort header click triggers API call
- Filter input debouncing and API request
- Pagination controls navigate correctly
- Row selection checkbox state management

---

## 10. Success Criteria

✅ Grid accepts both API and in-memory data sources  
✅ Sorting, paging, filtering work server-side  
✅ Row selection and export functional  
✅ Column customization and visibility toggle working  
✅ Error handling with retry logic  
✅ Bootstrap styling matches existing Core.Web components  
✅ TagHelper simplifies 80% of common use cases  
✅ Razor components available for advanced scenarios  
✅ Models reusable in Core.Shared (for future MAUI/Desktop)  
✅ Integration tests pass  

---

## 11. Future Phases (Out of Scope)

- **Inline editing** (currently IsEditable flag prepared for future use)
- **Real-time updates** (WebSocket/SignalR integration)
- **Advanced filtering** (date ranges, complex operators)
- **Core.Maui** — MAUI mobile implementation
- **Core.Desktop** — WPF/WinForms desktop implementation
- **Drag-to-sort columns** (reordering)
- **Column grouping** (nested headers)

---

## 12. File Structure

```
src/SmartWorkz.Core.Shared/
  Pagination/
    PagedList.cs (existing)
    PagedQuery.cs (existing)
  Grid/
    GridColumn.cs (new)
    GridRequest.cs (new)
    GridResponse.cs (new)
    IGridDataProvider.cs (new interface)
    GridExportOptions.cs (new)

src/SmartWorkz.Core.Web/
  Components/
    Grid/
      GridComponent.razor (new)
      GridColumnComponent.razor (new)
      GridFilterComponent.razor (new)
      GridRowSelectorComponent.razor (new)
  Services/
    GridDataProvider.cs (new)
    GridExportService.cs (new)
    GridStateManager.cs (new)
  TagHelpers/
    GridTagHelper.cs (new)
  wwwroot/
    css/grid.css (new)
    js/grid.js (new)
```

---

## 13. Dependencies

**New NuGet Packages:**
- EPPlus (for Excel export) - optional, add if export needed
- No additional required packages for core functionality

**Internal Dependencies:**
- SmartWorkz.Core (SortDirection, Result<T>)
- SmartWorkz.Core.Shared (PagedList<T>, PagedQuery)
- Microsoft.AspNetCore.Mvc.Razor
- Microsoft.AspNetCore.Mvc.ViewFeatures

---

## 14. Migration Path

When MAUI or Desktop clients are built, they will:
1. Reference SmartWorkz.Core.Shared (models only)
2. Implement `IGridDataProvider` for their platform
3. Use platform-native controls (CollectionView, DataGrid) with Grid models

Web code remains unchanged.
