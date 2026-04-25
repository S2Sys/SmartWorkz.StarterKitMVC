# Multi-View Data Components Design
**Date:** 2026-04-20  
**Status:** Approved  
**Scope:** Extend SmartWorkz.Core.Web with Grid, List, and Map view components sharing unified state

---

## Problem Statement
SmartWorkz.Core.Web has Grid component for tabular data display, but web projects need **multiple view modes (Grid, List) for the same dataset** with consistent state synchronization. Without a unified state context:
- Each view manages its own sort/filter/pagination independently
- Switching views requires re-fetching or loses state
- Developers duplicate filtering/sorting logic across views

## Solution Overview
Implement **Approach B: Unified DataContext with Composable View Components**
- Single `DataContext<T>` manages all state (sort, filter, pagination, selection, loading, errors)
- Two independent view components (Grid, List) consume the shared context
- View components are thin rendering layers—no state logic
- Filtering/sorting applies consistently across both views
- State syncs automatically when users switch views
- **Phase 2 (Future):** Map view component with geographic display

---

## Architecture

### Core Components

#### 1. DataContext<T> (New)
Non-Razor service class managing application state for data-heavy views.

**Properties:**
- `CurrentRequest: GridRequest` — active sort, filter, pagination parameters
- `CurrentResponse: GridResponse<T>` — current data, metadata, totals
- `SelectedRowIds: List<object>` — row selection state
- `IsLoading: bool` — data fetch in progress
- `Error: string` — error message from failed requests
- `ViewConfig: ViewConfiguration` — column visibility, field mappings per view type

**Methods:**
- `UpdateSort(propertyName, isDescending)` — change sort; triggers data fetch
- `UpdateFilter(property, operator, value)` — add/update filter; resets to page 1
- `UpdatePagination(pageNumber, pageSize)` — change page
- `ToggleRowSelection(rowId)` — toggle single row
- `SetSelectedRows(rowIds)` — bulk select
- `ClearFilters()` — reset to default state
- `SelectAll()` — select all visible rows on current page

**Events:**
- `OnStateChanged: event Action()` — fired when any state changes; views subscribe to re-render

**Lifecycle:**
- Created by parent `DataViewerComponent<T>` at initialization
- Persists for component lifetime
- Disposed when parent is disposed

---

#### 2. GridViewComponent<T> (Enhanced)
Existing Grid component refactored to consume DataContext.

**Changes:**
- Inject `DataContext<T>` instead of managing local state
- Remove local `GridStateManager`—delegate to context
- Click handlers call context methods (`UpdateSort`, `UpdateFilter`, etc.)
- Display context properties (`IsLoading`, `Error`, `SelectedRowIds`)

**Rendering:**
- Renders table with `DataContext.CurrentResponse.Data`
- Column headers show sort indicators from `CurrentRequest.SortBy`
- Pagination controls bound to context page properties
- Checkboxes reflect `SelectedRowIds` from context

---

#### 3. ListViewComponent<T> (New)
Card or row-based list display for the same data.

**Parameters:**
- `DataContext: DataContext<T>` — shared state
- `Fields: List<GridColumn>` — which columns to display (subset of full schema)
- `CardTemplate: RenderFragment<T>?` — optional custom card layout
- `ItemsPerRow: int` — responsive grid layout (1, 2, 3 cols)

**Rendering:**
- Renders `CurrentResponse.Data` as cards or list items
- Shows only specified fields
- Pagination uses context pagination
- Filters apply from context (user doesn't re-filter)
- Click item → optionally show detail panel or navigate

**Services Used:**
- `ListViewFormatter` — format data for card display (dates, currency, truncation)

---

---

#### 5. DataViewerComponent<T> (New)
Parent container orchestrating both views.

**Parameters:**
- `DataSource: IEnumerable<T>` — initial data or API endpoint
- `Columns: List<GridColumn>` — full column schema
- `DefaultView: ViewType` — starting view (default: Grid)
- `AutoFetch: bool` — initialize data on load (default: true)

**Responsibilities:**
- Create and maintain `DataContext<T>` instance
- Render view toggle (buttons/tabs for Grid/List)
- Switch active view component based on selection
- Pass context to active view component
- Handle initialization and cleanup

**Rendering:**
```html
[View Toggle Buttons]
Grid | List

[Active View Component]
<GridViewComponent/> or <ListViewComponent/>

[Shared Info]
Loading spinner, error banner, selection count
```

---

## Data Flow

### State Synchronization Sequence

**User sorts Grid**
1. Grid header click → `DataContext.UpdateSort("Name", false)`
2. DataContext updates `CurrentRequest.SortBy`, fires `OnStateChanged`
3. DataContext calls `GridDataProvider.FetchData()` with updated request
4. Response populates `CurrentResponse` with sorted data
5. Grid re-renders with new data

**User switches to List view**
6. User clicks "List" button in toggle
7. DataViewerComponent renders `<ListViewComponent/>`
8. ListViewComponent reads same `CurrentResponse` (already sorted)
9. List renders sorted data—no re-fetch needed ✅

**User filters in List view**
10. List filter input → `DataContext.UpdateFilter("Category", "equals", "Electronics")`
11. DataContext updates `CurrentRequest.Filters`, resets page to 1
12. DataProvider fetches filtered data
13. Grid and List both receive updated data automatically
14. Both views display filtered results

---

## Dependencies & Integration

### Reuse from Existing Code
- `GridDataProvider` — fetch data with filters/sort/pagination
- `GridExportService` — export selected rows to CSV
- `GridStateManager` — (optional) complement DataContext for advanced state tracking
- `GridColumn`, `GridRequest`, `GridResponse<T>` — data structures
- Form TagHelpers — reuse in filter UI

### New Services
- **`DataContext<T>`** — state container (non-Razor)
- **`ListViewFormatter`** — format data for card display (dates, currency, text truncation)
- **`ViewConfiguration`** — store view-specific settings (visible columns, item layout)

### NuGet Dependencies
- Existing: `Microsoft.AspNetCore.Components`, `Microsoft.AspNetCore.Mvc.Razor`
- No new external dependencies for Phase 1

---

## Error Handling

| Scenario | Handling |
|----------|----------|
| **API fails** | DataContext.Error = message; views show error banner; retry button calls `UpdateFilter()`|
| **No data** | CurrentResponse.Data = []; views render empty state message |
| **Large dataset (1000+ rows)** | GridDataProvider enforces server-side pagination; views don't break |
| **Filter syntax invalid** | DataContext.Error set; API validation feedback shown to user |

---

## Testing Strategy

### Unit Tests (DataContext)
- `UpdateSort()` changes `CurrentRequest` correctly
- `UpdateFilter()` adds/replaces filters, resets pagination
- `ToggleRowSelection()` toggles selection state
- `ClearFilters()` resets to default request
- `OnStateChanged` event fires on state changes

### Component Tests (Grid/List Views)
- View renders data from context
- Click handlers delegate to context methods
- View reflects context properties (`IsLoading`, `Error`, `SelectedRowIds`)
- View updates when context state changes (via event subscription)

### Integration Tests
- DataViewerComponent initializes DataContext and active view
- Switching views preserves state
- Filter in Grid → List view shows filtered data (no re-fetch)
- Sort in List → Grid shows same sorted data
- Selection syncs across views

### E2E Tests (Manual or Selenium)
- User sorts Grid → switches to List → data is sorted
- User filters in List → Grid shows same filters applied
- User selects rows in Grid → count displayed; switches to List → same rows indicated

---

## Scope Boundaries

**Included (Phase 1):**
- DataContext state management
- Grid, List view components
- View toggle/switching
- State synchronization across views
- Basic error handling

**Not Included (Phase 2+):**
- Map view component with geospatial display
- Real-time WebSocket updates
- Custom chart/dashboard components
- Drag-drop row reordering
- Column drag-drop reordering
- Advanced reporting/export formats (PDF, Excel with formatting)

---

## Success Criteria

1. ✅ Grid, List views display same data correctly
2. ✅ Sort/filter in one view applies to both views automatically
3. ✅ Row selection syncs across both views
4. ✅ View toggle preserves state (no data loss)
5. ✅ Existing Grid component continues to work
6. ✅ Unit tests ≥80% code coverage
7. ✅ No performance regression on large datasets (1000+ rows)

---

## Implementation Roadmap

**Phase 1 (Current):** DataContext + enhance GridViewComponent + ListViewComponent + integration tests
**Phase 2 (Future):** MapViewComponent with geographic display
**Phase 3 (Future):** Advanced features (charting, exports, real-time updates)

---

## References
- Existing: `/src/SmartWorkz.Core.Web/Components/Grid/`
- Existing: `/src/SmartWorkz.Core.Web/Services/Grid/`
- New: `/src/SmartWorkz.Core.Web/Components/DataContext/`
- New: `/src/SmartWorkz.Core.Web/Components/ListView/`
