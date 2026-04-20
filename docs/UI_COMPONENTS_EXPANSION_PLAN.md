# SmartWorkz.Core.Web - UI Components Expansion Plan

**Status:** Phase 1 - Data Components
**Target:** Enterprise-grade reusable component library

---

## Current Component Inventory

### ✅ Existing Components

#### Razor Components (7)
- `GridComponent` - Advanced data grid with sorting/filtering
- `ListViewComponent` - Card/list layout display
- `DataViewerComponent` - Multi-view orchestration
- `GridColumnComponent` - Grid column definition
- `GridFilterComponent` - Grid filtering UI
- `GridRowSelectorComponent` - Row selection
- `FilterBuilderComponent` - Dynamic filter builder

#### TagHelpers (18)
**Forms:** Input, Label, FormGroup, Select, Textarea, Checkbox, RadioButton, FileInput, ValidationMessage
**Display:** Alert, Badge, Pagination
**Navigation:** Breadcrumb, Button, Icon
**Grid:** GridTagHelper

---

## Phase 1: Data Components (PRIORITY)

### 1. CardComponent ⭐
**Purpose:** Display entity data in card format (statistics, profiles, products)

```
┌─────────────────────────┐
│ ┌─────────────┐ Header  │
│ │   Image     │ Title   │
│ │   (Opt)     │ Subtitle│
│ └─────────────┘         │
│                         │
│ Card Body               │
│ (Flexible content)      │
│                         │
│ ┌─────────────────────┐ │
│ │ Action Buttons      │ │
│ └─────────────────────┘ │
└─────────────────────────┘
```

**Features:**
- Header with icon/image
- Flexible body content
- Action buttons
- Loading state
- Click handlers
- Hover effects
- Responsive grid layout

**Usage:**
```html
<sw:Card Title="Product" Subtitle="Premium Package">
    <CardContent>Price: $99.99</CardContent>
    <CardActions>
        <Button Text="View" OnClick="@HandleView" />
    </CardActions>
</sw:Card>
```

---

### 2. TableComponent ⭐⭐
**Purpose:** Display tabular data with sorting, pagination, selection

```
┌─────────────────────────────────────┐
│ Name │ Email │ Status │ Actions     │
├─────────────────────────────────────┤
│ John │ j@... │ Active │ [Edit] [X]  │
│ Jane │ j@... │ Active │ [Edit] [X]  │
│ Bob  │ b@... │ Inactive│[Edit] [X] │
└─────────────────────────────────────┘
```

**Features:**
- Sortable columns
- Pagination
- Row selection (checkbox)
- Custom cell rendering
- Striped/hover rows
- Empty state handling
- Responsive (horizontal scroll mobile)
- Action column (Edit, Delete, etc.)

**Usage:**
```html
<sw:Table Data="@users" PageSize="10">
    <TableColumn Property="Name" Sortable="true" />
    <TableColumn Property="Email" />
    <TableColumn Title="Actions">
        <ActionButton Variant="Primary" OnClick="@((u) => Edit(u))">Edit</ActionButton>
    </TableColumn>
</sw:Table>
```

---

### 3. DashboardComponent ⭐
**Purpose:** Display key metrics and statistics

```
┌──────────────────────────┐
│ Total Users   │ Revenue  │
│     1,234     │ $45,678  │
│     ↑12%      │ ↑8%      │
└──────────────────────────┘
```

**Features:**
- Stat cards (value + label + trend)
- Multiple metrics per card
- Trend indicators (↑/↓)
- Responsive grid
- Custom formatters
- Color coding by status
- Mini charts (optional Apex Charts integration)

**Usage:**
```html
<sw:Dashboard>
    <StatCard Label="Total Users" Value="1234" Trend="12" TrendUp="true" />
    <StatCard Label="Revenue" Value="45678" Formatter="@CurrencyFormatter" />
</sw:Dashboard>
```

---

### 4. TabsComponent ⭐
**Purpose:** Organize content into tabs

```
┌─────────────────────────────┐
│ Details │ Orders │ Settings │
├─────────────────────────────┤
│ Tab content here            │
│                             │
└─────────────────────────────┘
```

**Features:**
- Lazy loading
- Icons on tabs
- Disable tabs
- Tab badges/counters
- Keyboard navigation
- Responsive (mobile: dropdown)
- Before/after callbacks

**Usage:**
```html
<sw:Tabs>
    <Tab Title="Details" Badge="3">@* Content *@</Tab>
    <Tab Title="Orders">@* Content *@</Tab>
    <Tab Title="Settings" Disabled="true">@* Content *@</Tab>
</sw:Tabs>
```

---

### 5. AccordionComponent ⭐
**Purpose:** Collapsible sections for expandable content

```
┌─────────────────────────┐
│ [+] Section 1           │
├─────────────────────────┤
│ [-] Section 2           │
│ Content visible here    │
├─────────────────────────┤
│ [+] Section 3           │
└─────────────────────────┘
```

**Features:**
- Single or multi-expand
- Icons/indicators
- Smooth animations
- Disabled items
- Custom headers
- Callback on expand/collapse
- Auto-collapse siblings (option)

**Usage:**
```html
<sw:Accordion AllowMultiple="false">
    <AccordionItem Title="FAQ 1" DefaultExpanded="false">
        Answer here...
    </AccordionItem>
    <AccordionItem Title="FAQ 2">
        Answer here...
    </AccordionItem>
</sw:Accordion>
```

---

### 6. TreeViewComponent ⭐⭐
**Purpose:** Display hierarchical data (folders, categories, organization)

```
├─ Root
│  ├─ Category 1
│  │  ├─ Item 1.1
│  │  └─ Item 1.2
│  └─ Category 2
│     └─ Item 2.1
└─ Root 2
```

**Features:**
- Multi-level nesting
- Lazy loading children
- Selection (single/multiple)
- Icons per node type
- Drag-drop support (optional)
- Context menu support
- Search/filter nodes
- Expand/collapse animations

**Usage:**
```html
<sw:TreeView Data="@hierarchicalData" 
             OnSelect="@HandleNodeSelect"
             AllowMultiSelect="true">
    <TreeNodeTemplate Context="node">
        <Icon Name="@GetIconForNode(node)" />
        <span>@node.Label</span>
    </TreeNodeTemplate>
</sw:TreeView>
```

---

### 7. TimelineComponent ⭐
**Purpose:** Display chronological events (audit logs, activity, milestones)

```
⭐ 2024-01-01  Created order
│
● 2024-01-05  Payment received
│
● 2024-01-10  Shipped
│
● 2024-01-15  Delivered ✓
```

**Features:**
- Vertical/horizontal layout
- Date labels
- Status indicators
- Custom content per event
- Icons/avatars
- Color coding
- Alternating sides (desktop)
- Responsive (single column mobile)

**Usage:**
```html
<sw:Timeline Data="@events" Layout="Vertical">
    <TimelineEventTemplate Context="evt">
        <TimelineIcon Icon="@evt.Icon" Color="@evt.Color" />
        <TimelineContent>
            <h5>@evt.Title</h5>
            <p>@evt.Description</p>
            <small>@evt.Date.ToString("MMM dd, yyyy")</small>
        </TimelineContent>
    </TimelineEventTemplate>
</sw:Timeline>
```

---

## Phase 2: Modal & Overlay (Future)
- Modal Dialog
- Drawer/Sidebar
- Tooltip
- Popover
- Toast Notifications

## Phase 3: Advanced Form Components (Future)
- Date Picker
- Time Picker
- Color Picker
- Range Slider
- Tags Input
- Autocomplete

---

## Implementation Architecture

### Folder Structure
```
src/SmartWorkz.Core.Web/Components/
├── Data/
│   ├── CardComponent.razor
│   ├── CardComponent.razor.cs
│   ├── TableComponent.razor
│   ├── TableComponent.razor.cs
│   ├── DashboardComponent.razor
│   ├── DashboardComponent.razor.cs
│   ├── TabsComponent.razor
│   ├── TabsComponent.razor.cs
│   ├── AccordionComponent.razor
│   ├── AccordionComponent.razor.cs
│   ├── TreeViewComponent.razor
│   ├── TreeViewComponent.razor.cs
│   ├── TimelineComponent.razor
│   └── TimelineComponent.razor.cs
├── Models/
│   ├── CardOptions.cs
│   ├── TableOptions.cs
│   ├── StatCardData.cs
│   ├── TreeNodeData.cs
│   ├── TimelineEventData.cs
│   └── TabItemData.cs
└── Services/
    ├── ITableService.cs
    ├── ITreeViewService.cs
    ├── IComponentRenderer.cs
    └── ComponentFormatter.cs
```

### Service Architecture

```csharp
// ICardService - not needed (simple component)

// ITableService
public interface ITableService
{
    Task<PagedResult<T>> GetTableDataAsync<T>(
        TableRequest request, 
        IQueryable<T> data);
    
    Task<(List<PropertyInfo> Columns, List<T> Data)> 
        ProcessTableDataAsync<T>(List<T> data, TableRequest request);
}

// ITreeViewService
public interface ITreeViewService
{
    Task<List<TreeNode>> LoadChildrenAsync(string parentId);
    Task<List<TreeNode>> SearchNodesAsync(string query);
    List<TreeNode> FlattenTree(TreeNode root);
}

// ITimelineService
public interface ITimelineService
{
    Task<List<TimelineEvent>> GetAuditTrailAsync<T>(int entityId) 
        where T : class, IAuditable;
}
```

---

## CSS/Styling Strategy

### Bootstrap Integration
All components use Bootstrap 5 utility classes for consistency:
- Cards: `card`, `card-header`, `card-body`, `card-footer`
- Table: `table`, `table-hover`, `table-striped`
- Alerts: `alert`, `alert-success`, `alert-danger`

### Custom CSS Variables
```css
:root {
    --sw-primary-color: #007bff;
    --sw-danger-color: #dc3545;
    --sw-card-border-radius: 8px;
    --sw-card-shadow: 0 2px 8px rgba(0,0,0,0.1);
    --sw-table-hover-bg: #f8f9fa;
    --sw-transition-speed: 0.2s ease;
}
```

---

## Testing Strategy

### Component Tests
- Unit tests for logic components (.razor.cs)
- Integration tests for rendering
- Snapshot tests for HTML output
- Event callback tests

### Example Test
```csharp
[Fact]
public async Task CardComponent_WithAction_FiresOnClick()
{
    // Arrange
    var clicked = false;
    var component = new CardComponent 
    { 
        Title = "Test", 
        OnAction = async () => { clicked = true; }
    };

    // Act
    await component.InvokeAsync(async () =>
    {
        component.RaiseOnAction();
    });

    // Assert
    Assert.True(clicked);
}
```

---

## Success Criteria

✅ **Phase 1 Completion:**
1. All 7 data components implemented and tested
2. 100% XML documentation
3. Usage examples for each component
4. Sample demo page showing all components
5. Responsive on mobile (480px+), tablet, desktop
6. Accessibility (ARIA labels, keyboard nav)
7. Performance optimized (lazy rendering where applicable)

---

## Estimated Effort

| Component | Complexity | Effort | Status |
|-----------|-----------|--------|--------|
| CardComponent | Low | 2 hours | TODO |
| TableComponent | High | 8 hours | TODO |
| DashboardComponent | Medium | 4 hours | TODO |
| TabsComponent | Medium | 4 hours | TODO |
| AccordionComponent | Medium | 4 hours | TODO |
| TreeViewComponent | High | 10 hours | TODO |
| TimelineComponent | Medium | 5 hours | TODO |
| Services & Models | Medium | 6 hours | TODO |
| Tests & Docs | Medium | 8 hours | TODO |
| **TOTAL** | | **51 hours** | |

**Timeline:** 2-3 weeks (assuming 20 hours/week)

