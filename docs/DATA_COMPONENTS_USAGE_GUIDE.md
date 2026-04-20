# SmartWorkz.Core.Web - Data Components Usage Guide

Complete reference for using all data components with real-world examples.

---

## Table of Contents

1. [Setup & DI Registration](#setup--di-registration)
2. [CardComponent](#cardcomponent)
3. [DashboardComponent](#dashboardcomponent)
4. [TableComponent](#tablecomponent)
5. [TabsComponent](#tabscomponent)
6. [AccordionComponent](#accordioncomponent)
7. [TreeViewComponent](#treeviewcomponent)
8. [TimelineComponent](#timelinecomponent)
9. [Services](#services)
10. [Best Practices](#best-practices)

---

## Setup & DI Registration

### Step 1: Add Services to DI Container

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSmartWorkzCoreWeb()
    .AddSmartWorkzWebComponents();

// Register data component services
builder.Services
    .AddScoped<ITableDataService, TableDataService>()
    .AddScoped<ITreeViewService, TreeViewService>()
    .AddScoped<IDataFormatterService, DataFormatterService>();

var app = builder.Build();
```

### Step 2: Reference Components in Razor Pages

```razor
@page "/products"
@using SmartWorkz.Core.Web.Components.Data
@using SmartWorkz.Core.Web.Components.Data.Models
@inject ITableDataService TableService
@inject IDataFormatterService Formatter

<h2>Products</h2>

<!-- Components go here -->
```

---

## CardComponent

Display entity data in card format with optional image, icon, and action buttons.

### Basic Usage

```razor
<sw:Card Title="Product" Subtitle="Premium Package" IconClass="fa-box">
    <CardContent>
        <p>Price: <strong>$99.99</strong></p>
        <p>Stock: <strong>50 units</strong></p>
    </CardContent>
    <CardActions>
        <button class="btn btn-sm btn-primary" @onclick="@HandleEdit">Edit</button>
        <button class="btn btn-sm btn-danger" @onclick="@HandleDelete">Delete</button>
    </CardActions>
</sw:Card>
```

### With Image and Badge

```razor
<sw:Card Title="Featured Product"
         ImageUrl="/images/product.jpg"
         BadgeText="NEW"
         BadgeColor="danger"
         IsClickable="true"
         OnClick="@HandleCardClick">
    <CardContent>
        <p>This is a featured product with special pricing.</p>
        <p class="text-muted"><small>Limited time offer</small></p>
    </CardContent>
</sw:Card>
```

### Loading State

```razor
<sw:Card Title="Product Details" IsLoading="@isLoading">
    @* Content will be hidden while loading *@
</sw:Card>

@code {
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(2000);
        isLoading = false;
    }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Title | string? | null | Card header title |
| Subtitle | string? | null | Subtitle under title |
| ImageUrl | string? | null | Header image URL |
| IconClass | string? | null | Icon CSS class |
| BadgeText | string? | null | Badge label |
| BadgeColor | string | "success" | Badge color |
| IsClickable | bool | false | Enable click effects |
| IsLoading | bool | false | Show skeleton loading |
| ElevationLevel | int | 1 | Shadow elevation (1-3) |
| OnClick | EventCallback | - | Click callback |

---

## DashboardComponent

Display key metrics and statistics with trend indicators.

### Basic Usage

```razor
<sw:Dashboard Stats="@stats" ColumnsPerRow="4" />

@code {
    private List<StatCardData> stats = new()
    {
        new StatCardData
        {
            Label = "Total Users",
            Value = "1,234",
            Trend = 12,
            TrendUp = true,
            Icon = "fa-users",
            Color = "primary"
        },
        new StatCardData
        {
            Label = "Revenue",
            Value = "$45,678",
            Trend = 8.5m,
            TrendUp = true,
            Icon = "fa-dollar-sign",
            Color = "success"
        },
        new StatCardData
        {
            Label = "Conversion Rate",
            Value = "3.2%",
            Trend = -1.2m,
            TrendUp = false,
            Icon = "fa-chart-line",
            Color = "warning"
        }
    };
}
```

### With Custom Formatting

```razor
<sw:Dashboard Stats="@stats" />

@code {
    private List<StatCardData> stats = new();

    protected override async Task OnInitializedAsync()
    {
        var metrics = await GetMetricsAsync();
        stats = metrics.Select(m => new StatCardData
        {
            Label = m.Label,
            Value = m.Label.Contains("Revenue") ? $"${m.Value:N2}" : m.Value.ToString(),
            Trend = m.Trend,
            TrendUp = m.TrendUp,
            Icon = m.Icon,
            Color = m.TrendUp ? "success" : "danger"
        }).ToList();
    }
}
```

### With Custom Content

```razor
<sw:Dashboard Stats="@stats">
    <CustomContent>
        <div class="row mt-4">
            <div class="col-md-6">
                <h5>Recent Activity</h5>
                <!-- Chart or custom content -->
            </div>
        </div>
    </CustomContent>
</sw:Dashboard>
```

---

## TableComponent

Display tabular data with sorting, pagination, and selection.

### Basic Usage

```razor
<sw:Table Data="@products" Columns="@columns" PageSize="10" />

@code {
    private List<Product> products = new();
    private List<TableColumn> columns = new()
    {
        new TableColumn { Title = "Name", PropertyName = "Name", IsSortable = true },
        new TableColumn { Title = "Price", PropertyName = "Price", Align = "right" },
        new TableColumn { Title = "Stock", PropertyName = "Stock", Align = "right" },
        new TableColumn { Title = "Status", PropertyName = "Status" }
    };

    protected override async Task OnInitializedAsync()
    {
        products = await GetProductsAsync();
    }
}
```

### With Row Selection

```razor
<sw:Table Data="@products"
          Columns="@columns"
          AllowRowSelection="true"
          AllowMultiSelect="true"
          OnSelectionChanged="@HandleSelectionChanged">
</sw:Table>

@code {
    private async Task HandleSelectionChanged(List<Product> selectedRows)
    {
        await DeleteSelectedAsync(selectedRows);
    }
}
```

### With Custom Formatting

```razor
@code {
    private List<TableColumn> columns = new()
    {
        new TableColumn
        {
            Title = "Product",
            PropertyName = "Name",
            IsSortable = true
        },
        new TableColumn
        {
            Title = "Price",
            PropertyName = "Price",
            Align = "right",
            Formatter = (value) => $"${value:N2}"
        },
        new TableColumn
        {
            Title = "Added",
            PropertyName = "CreatedAt",
            Formatter = (value) => ((DateTime)value).ToString("MMM dd, yyyy")
        },
        new TableColumn
        {
            Title = "Actions",
            PropertyName = null
        }
    };
}
```

### With Sorting and Pagination

```razor
<sw:Table Data="@currentPageData"
          Columns="@columns"
          TotalItems="@totalItems"
          PageSize="@pageSize"
          OnSort="@HandleSort"
          OnPageChanged="@HandlePageChange" />

@code {
    private TableRequest request = new() { Page = 1, PageSize = 10 };
    private int totalItems;

    private async Task HandleSort(TableRequest sortRequest)
    {
        request.SortBy = sortRequest.SortBy;
        request.SortDescending = sortRequest.SortDescending;
        await RefreshDataAsync();
    }

    private async Task HandlePageChange(int page)
    {
        request.Page = page;
        await RefreshDataAsync();
    }
}
```

---

## TabsComponent

Organize content into tabs with optional lazy loading.

### Basic Usage

```razor
<sw:Tabs Items="@tabs" OnTabChanged="@HandleTabChanged" />

@code {
    private List<TabItem> tabs = new()
    {
        new TabItem
        {
            Title = "Details",
            Content = @<div>Product details content</div>
        },
        new TabItem
        {
            Title = "Reviews",
            Badge = "3",
            Content = @<div>Customer reviews</div>
        },
        new TabItem
        {
            Title = "Settings",
            Disabled = false,
            Content = @<div>Product settings</div>
        }
    };

    private Task HandleTabChanged(TabChangedEventArgs args)
    {
        Console.WriteLine($"Changed to tab: {args.ActiveTab.Title}");
        return Task.CompletedTask;
    }
}
```

### With Lazy Loading

```razor
@code {
    private List<TabItem> tabs = new()
    {
        new TabItem
        {
            Title = "Overview",
            Content = @<div>Always loaded</div>
        },
        new TabItem
        {
            Title = "Analytics",
            LazyLoad = true,
            Content = null // Will load when tab is activated
        }
    };

    private Task HandleTabChanged(TabChangedEventArgs args)
    {
        if (args.ActiveTab.LazyLoad && !args.ActiveTab.ContentLoaded)
        {
            // Load content for this tab
            args.ActiveTab.Content = @<AnalyticsComponent />;
            args.ActiveTab.ContentLoaded = true;
        }

        return Task.CompletedTask;
    }
}
```

### With Icons and Styles

```razor
<sw:Tabs Items="@tabs" Style="TabStyle.Pills" Layout="TabLayout.Horizontal" />

@code {
    private List<TabItem> tabs = new()
    {
        new TabItem
        {
            Title = "Home",
            IconClass = "fa-home",
            Content = @<HomeTabContent />
        },
        new TabItem
        {
            Title = "Settings",
            IconClass = "fa-gear",
            Content = @<SettingsTabContent />
        }
    };
}
```

---

## AccordionComponent

Collapsible sections for expandable content.

### Basic Usage

```razor
<sw:Accordion Items="@items" />

@code {
    private List<AccordionItem> items = new()
    {
        new AccordionItem
        {
            Title = "What is SmartWorkz?",
            Content = @<div>SmartWorkz is a comprehensive framework...</div>
        },
        new AccordionItem
        {
            Title = "How do I get started?",
            Content = @<div>Getting started is easy...</div>
        },
        new AccordionItem
        {
            Title = "Where's the documentation?",
            Content = @<div>Documentation is available at...</div>
        }
    };
}
```

### With Single Expand

```razor
<sw:Accordion Items="@items" AllowMultiple="false" />
```

### With Icons and Callbacks

```razor
<sw:Accordion Items="@items"
              OnExpanded="@HandleExpanded"
              OnCollapsed="@HandleCollapsed"
              ShowToggleIcon="true" />

@code {
    private Task HandleExpanded(AccordionExpandedEventArgs args)
    {
        Console.WriteLine($"Expanded: {args.Item.Title}");
        return Task.CompletedTask;
    }

    private Task HandleCollapsed(AccordionCollapsedEventArgs args)
    {
        Console.WriteLine($"Collapsed: {args.Item.Title}");
        return Task.CompletedTask;
    }
}
```

---

## TreeViewComponent

Hierarchical data display with lazy loading and selection.

### Basic Usage

```razor
<sw:TreeView RootNodes="@treeData" OnNodeSelected="@HandleNodeSelected" />

@code {
    private List<TreeNode> treeData = new();

    protected override async Task OnInitializedAsync()
    {
        treeData = new()
        {
            new TreeNode
            {
                Id = "1",
                Label = "Documents",
                IconClass = "fa-folder",
                IsLeaf = false,
                Children = new()
                {
                    new TreeNode { Id = "1.1", Label = "Proposals", IconClass = "fa-file" },
                    new TreeNode { Id = "1.2", Label = "Reports", IconClass = "fa-file" }
                }
            },
            new TreeNode
            {
                Id = "2",
                Label = "Images",
                IconClass = "fa-folder",
                IsLeaf = false,
                Children = new()
            }
        };
    }

    private Task HandleNodeSelected(TreeNodeSelectedEventArgs args)
    {
        Console.WriteLine($"Selected: {args.SelectedNode.Label}");
        return Task.CompletedTask;
    }
}
```

### With Lazy Loading

```razor
@code {
    private List<TreeNode> treeData = new()
    {
        new TreeNode
        {
            Id = "categories",
            Label = "Product Categories",
            IconClass = "fa-folder",
            IsLeaf = false,
            LoadChildrenAsync = async (parentId) =>
            {
                // Load categories from API
                var categories = await GetCategoriesAsync(parentId);
                return categories.Select(c => new TreeNode
                {
                    Id = c.Id.ToString(),
                    Label = c.Name,
                    IconClass = "fa-folder",
                    IsLeaf = false,
                    LoadChildrenAsync = async (id) => await GetProductsAsync(int.Parse(id))
                }).ToList();
            }
        }
    };
}
```

### With Multi-Select

```razor
<sw:TreeView RootNodes="@treeData"
             AllowMultiSelect="true"
             ShowCheckboxes="true"
             OnNodeSelected="@HandleMultiSelect" />

@code {
    private Task HandleMultiSelect(TreeNodeSelectedEventArgs args)
    {
        var selected = string.Join(", ", args.AllSelectedNodes.Select(n => n.Label));
        Console.WriteLine($"Selected nodes: {selected}");
        return Task.CompletedTask;
    }
}
```

---

## TimelineComponent

Display chronological events and activity.

### Basic Usage

```razor
<sw:Timeline Events="@events" DateFormat="MMM dd, yyyy" />

@code {
    private List<TimelineEvent> events = new()
    {
        new TimelineEvent
        {
            Title = "Order Created",
            Description = "Customer placed order #1234",
            Timestamp = DateTime.Now.AddDays(-10),
            IconClass = "fa-cart-shopping",
            IconColor = "info"
        },
        new TimelineEvent
        {
            Title = "Payment Received",
            Description = "Payment of $99.99 confirmed",
            Timestamp = DateTime.Now.AddDays(-9),
            IconClass = "fa-credit-card",
            IconColor = "success"
        },
        new TimelineEvent
        {
            Title = "Shipped",
            Description = "Package shipped via FedEx",
            Timestamp = DateTime.Now.AddDays(-5),
            IconClass = "fa-truck",
            IconColor = "primary"
        }
    };
}
```

### With Avatars and Actors

```razor
<sw:Timeline Events="@events" ShowAvatars="true" ReverseOrder="false" />

@code {
    private List<TimelineEvent> events = new()
    {
        new TimelineEvent
        {
            Title = "Order Approved",
            Actor = "John Manager",
            AvatarUrl = "/images/john.jpg",
            Timestamp = DateTime.Now.AddDays(-2),
            IconClass = "fa-check",
            IconColor = "success"
        }
    };
}
```

### With Badges and Metadata

```razor
<sw:Timeline Events="@auditTrail" IncludeTime="true" />

@code {
    private List<TimelineEvent> auditTrail = new()
    {
        new TimelineEvent
        {
            Title = "Record Modified",
            Description = "Email changed from old@email.com to new@email.com",
            BadgeText = "UPDATE",
            Actor = "Admin",
            Timestamp = DateTime.UtcNow,
            IconColor = "warning"
        }
    };
}
```

---

## Services

### ITableDataService

```csharp
@inject ITableDataService TableService

// Example usage in component code
private async Task RefreshDataAsync()
{
    var request = new TableRequest 
    { 
        Page = 1, 
        PageSize = 10,
        SortBy = "CreatedAt",
        SortDescending = true
    };

    var filtered = await TableService.ApplyFiltersAsync(allData, request);
    var sorted = await TableService.ApplySortingAsync(filtered.Items, request);
    var paged = await TableService.ApplyPaginationAsync(sorted, request);
}
```

### ITreeViewService

```csharp
@inject ITreeViewService TreeService

// Flatten tree
var flatList = TreeService.FlattenTree(rootNode);

// Find node
var node = TreeService.FindNodeById(rootNodes, "node-id");

// Search
var results = TreeService.SearchNodes(rootNodes, "search term");

// Filter
var filtered = TreeService.FilterByProperty(rootNodes, "CategoryId", 5);
```

### IDataFormatterService

```csharp
@inject IDataFormatterService Formatter

// Usage in templates
<p>Price: @Formatter.FormatCurrency(product.Price)</p>
<p>Created: @Formatter.FormatDate(product.CreatedAt)</p>
<p>Size: @Formatter.FormatBytes(1024 * 1024)</p>
<p>Activity: @Formatter.FormatTimeSpan(DateTime.UtcNow - lastActivity)</p>
```

---

## Best Practices

### 1. Performance

- **Pagination**: Always paginate large datasets (10K+ rows)
- **Lazy Loading**: Use lazy loading for tabs/accordion with heavy content
- **Virtual Scrolling**: For tables >5K rows, consider virtual scrolling library

### 2. Accessibility

- All components include proper ARIA labels
- Use semantic HTML (`<button>`, `<table>`, etc.)
- Keyboard navigation fully supported (Tab, Enter, Arrow keys)
- Color is not the only indicator (use icons + text)

### 3. Responsive Design

All components are mobile-optimized:

```css
/* Mobile rules are built-in */
@media (max-width: 768px) {
    /* Tables: horizontal scroll */
    /* Timeline: single column */
    /* Tabs: dropdown */
}
```

### 4. Styling

Override defaults with CSS variables:

```css
:root {
    --bs-primary: #007bff;
    --bs-border-color: #dee2e6;
    --bs-light: #f8f9fa;
}
```

### 5. Error Handling

Always handle edge cases:

```csharp
// Check for null/empty
if (data == null || data.Count == 0)
{
    // Show empty state
}

// Validate requests
if (request.PageSize > 1000)
{
    request.PageSize = 1000; // Max limit
}
```

### 6. Caching

Use caching for large datasets:

```csharp
[CacheAttribute(Seconds = 300)]
public async Task<List<Product>> GetProductsAsync()
{
    // This will be cached for 5 minutes
    return await _repository.GetAllAsync();
}
```

---

## Complete Example: Product Management Page

```razor
@page "/products"
@using SmartWorkz.Core.Web.Components.Data
@using SmartWorkz.Core.Web.Components.Data.Models
@inject ITableDataService TableService
@inject NavigationManager NavManager

<h2>Product Management</h2>

<sw:Tabs Items="@tabs" />

@code {
    private List<Product> products = new();
    private List<TableColumn> columns = new();
    private List<TabItem> tabs = new();
    private StatCardData[] stats = new StatCardData[4];

    protected override async Task OnInitializedAsync()
    {
        products = await GetProductsAsync();
        InitializeColumns();
        InitializeTabs();
        InitializeStats();
    }

    private void InitializeColumns()
    {
        columns = new()
        {
            new TableColumn { Title = "SKU", PropertyName = "Sku" },
            new TableColumn { Title = "Name", PropertyName = "Name", IsSortable = true },
            new TableColumn 
            { 
                Title = "Price", 
                PropertyName = "Price",
                Formatter = (v) => $"${v:N2}"
            },
            new TableColumn { Title = "Stock", PropertyName = "Stock" }
        };
    }

    private void InitializeTabs()
    {
        tabs = new()
        {
            new TabItem
            {
                Title = "All Products",
                Content = @<ProductTableTab Products="@products" />
            },
            new TabItem
            {
                Title = "Statistics",
                Content = @<DashboardTab Stats="@stats" />
            }
        };
    }

    private void InitializeStats()
    {
        stats = new[]
        {
            new StatCardData { Label = "Total Products", Value = products.Count.ToString(), Color = "primary" },
            new StatCardData { Label = "Low Stock", Value = products.Count(p => p.Stock < 10).ToString(), Color = "warning" },
            new StatCardData { Label = "Total Value", Value = $"${products.Sum(p => p.Price * p.Stock):N0}", Color = "success" },
            new StatCardData { Label = "Categories", Value = products.Select(p => p.CategoryId).Distinct().Count().ToString(), Color = "info" }
        };
    }

    private async Task<List<Product>> GetProductsAsync()
    {
        // Load from API
        return await Task.FromResult(new List<Product>());
    }
}
```

---

**For more information, see:** [UI Components Expansion Plan](./UI_COMPONENTS_EXPANSION_PLAN.md)

