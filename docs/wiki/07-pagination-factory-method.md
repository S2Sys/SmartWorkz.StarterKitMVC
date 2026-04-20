# Pagination Factory Method

## Purpose
Convert API `PaginationResponse<T>` DTOs (with paged data) to view-model `PaginationModel` (for rendering pagination UI). Simplifies Razor Page `OnGet` methods and supports both full-page and HTMX partial updates.

## Quick Start

### In Razor Page OnGet
```csharp
public async Task OnGetAsync()
{
    var result = await _repository.SearchPagedAsync(TenantId, search, sort, page, pageSize);
    
    // Old way: manually copy fields
    Pagination = PaginationModel.From(result.TotalCount, result.CurrentPage, pageSize);
    
    // New way: use FromDto factory
    Pagination = PaginationModel.FromDto(result, pageSize);
}
```

## How It Works

### PaginationModel.FromDto<T>()

```csharp
public static PaginationModel FromDto<T>(PaginationResponse<T> dto, int pageSize,
    Dictionary<string, string?>? routeValues = null,
    string? htmxTarget = null, string? htmxHandler = null)
    => new()
    {
        TotalItems = dto.TotalCount,
        CurrentPage = dto.CurrentPage,
        PageSize = pageSize,
        RouteValues = routeValues ?? [],
        HtmxTarget = htmxTarget,
        HtmxHandler = htmxHandler
    };
```

**Parameters:**
- `dto` — `PaginationResponse<T>` from API/repository (contains `TotalCount`, `CurrentPage`, items)
- `pageSize` — items per page (must match request)
- `routeValues` — optional query parameters to preserve in pagination links (e.g., search, sort)
- `htmxTarget` — optional HTMX target selector for partial updates
- `htmxHandler` — optional handler name for HTMX (not used in this implementation, reserved for future)

**Returns:** Fully initialized `PaginationModel` ready for Razor `@Model.Pagination` binding.

### Example: Full Implementation

```csharp
[Authorize(Policy = "RequireAdmin")]
public class ProductsIndexModel : BasePage
{
    private readonly IProductRepository _repository;
    
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;
    
    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;
    
    public IEnumerable<Product> Products { get; set; }
    public PaginationModel Pagination { get; set; }
    
    public async Task OnGetAsync()
    {
        await LoadAsync();
    }
    
    public async Task<IActionResult> OnGetTableAsync()
    {
        // HTMX request for partial
        await LoadAsync();
        return Request.IsHtmx() ? Partial("_ProductTableRows", this) : Page();
    }
    
    private async Task LoadAsync()
    {
        // Repository returns PaginationResponse<Product>
        var result = await _repository.SearchPagedAsync(
            TenantId, Search, Page, PageSize);
        
        Products = result.Items;
        
        // Use FromDto to map DTO to view model
        Pagination = PaginationModel.FromDto(result, PageSize,
            routeValues: new Dictionary<string, string?>
            {
                ["search"] = Search
            },
            htmxTarget: "#products-table-container");
    }
}
```

## Rendered Output

`@Model.Pagination` properties in Razor:
```html
<!-- Pagination info -->
<small>Showing @Model.Pagination.FirstItem to @Model.Pagination.LastItem of @Model.Pagination.TotalItems</small>

<!-- Page buttons -->
@foreach (var pageNum in Model.Pagination.PageWindow())
{
    @if (pageNum == -1)
    {
        <span>…</span>  <!-- Ellipsis -->
    }
    else if (pageNum == Model.Pagination.CurrentPage)
    {
        <span class="active">@pageNum</span>
    }
    else
    {
        <a href="@Url.Page("./Index", new { page = pageNum })">@pageNum</a>
    }
}
```

## Comparison: From() vs FromDto()

| Method | Use Case | Source |
|--------|----------|--------|
| `PaginationModel.From(total, page, pageSize)` | Paging results in-memory (simple lists) | Manual counts |
| `PaginationModel.FromDto(dto, pageSize)` | Paging results from API/Repository | `PaginationResponse<T>` |

### Example: When to Use Each

```csharp
// From() — client-side sorting of in-memory list
var allUsers = await _repository.GetAllAsync(TenantId);
var sorted = allUsers.OrderBy(u => u.Name).ToList();
Pagination = PaginationModel.From(sorted.Count, 1, 20);

// FromDto() — server-side paging from repository
var pagedResult = await _repository.SearchPagedAsync(TenantId, search, page, 20);
Pagination = PaginationModel.FromDto(pagedResult, 20);
```

## HTMX Integration

When used with HTMX partial updates:

```html
<input type="search"
       hx-get="/Products?handler=Table"
       hx-target="#products-table-container"
       hx-swap="outerHTML" />
```

Set `htmxTarget` in `FromDto()`:
```csharp
Pagination = PaginationModel.FromDto(result, pageSize,
    htmxTarget: "#products-table-container");
```

Then in `_ProductTableRows.cshtml` (partial), use pagination as normal:
```html
<a href="@Url.Page("./Index", new { page = 2 })">Next</a>
```

## Customization

### Change Window Size
By default, shows 5 page buttons around current (configurable per request):

```csharp
var pagination = PaginationModel.FromDto(result, pageSize);
pagination.MaxPageButtons = 7; // Show 7 instead of 5
```

Or set once when creating:
```csharp
Pagination = new PaginationModel
{
    TotalItems = result.TotalCount,
    CurrentPage = result.CurrentPage,
    PageSize = pageSize,
    MaxPageButtons = 7
};
```

### Preserve Multiple Query Parameters
Pass all search/sort/filter parameters to `routeValues`:

```csharp
Pagination = PaginationModel.FromDto(result, pageSize,
    routeValues: new Dictionary<string, string?>
    {
        ["search"] = Search,
        ["sortBy"] = SortBy,
        ["filterCategory"] = FilterCategory,
        ["desc"] = Desc.ToString()
    });
```

Then pagination links automatically include: `/Products?page=2&search=foo&sortBy=Name&desc=true`

## Common Mistakes

❌ **Not passing `pageSize` to `FromDto():`**
```csharp
Pagination = PaginationModel.FromDto(result); // WRONG — pageSize is required
```
✅ Always pass `pageSize`:
```csharp
Pagination = PaginationModel.FromDto(result, 20);
```

❌ **Forgetting `routeValues` for filtered lists:**
```csharp
// User searches "foo", navigates page 2, sees unfiltered results
Pagination = PaginationModel.FromDto(result, pageSize);
```
✅ Include search/sort in `routeValues`:
```csharp
Pagination = PaginationModel.FromDto(result, pageSize,
    routeValues: new Dictionary<string, string?> { ["search"] = Search });
```

❌ **Mixing `From()` and `FromDto()` in same method:**
```csharp
// Inconsistent — confuses readers
if (useApi) Pagination = PaginationModel.FromDto(dto, 20);
else Pagination = PaginationModel.From(localCount, 1, 20);
```
✅ Use consistent pattern per feature area.

## See Also
- [Base Page Pattern](03-base-page-pattern.md) — `BasePage` parent class
- [HTMX List Pattern](05-htmx-list-pattern.md) — HTMX integration details
- [PaginationModel](../../src/SmartWorkz.StarterKitMVC.Shared/Models/PaginationModel.cs) — full source
