# HTMX List Pattern

The HTMX list pattern enables dynamic list updates without full page reloads. This starter kit uses HTMX for search/filter functionality on paginated lists, improving UX while keeping code simple.

## Purpose

- **Instant search:** Type to filter lists without page reload
- **Progressive enhancement:** Works with or without JavaScript
- **Minimal plumbing:** Reuse the same handler for GET and partial requests
- **Familiar stack:** HTML, ASP.NET, standard attributes — no client-side framework

## Architecture

### How It Works

1. **Initial Load:** `GET /products` → renders full page with search form and product list
2. **User Types:** JavaScript-less version submits the form normally
   - OR HTMX version sends `GET /products?search=X` with `HX-Request: true` header
3. **Server Route:** Same handler checks `Request.IsHtmx()` and returns:
   - `true` → partial view with just the list rows (`<partial>`)
   - `false` → full page
4. **HTMX Swaps:** Partial replaces the target element via `hx-swap="innerHTML"`

### Key Components

| Component | Role |
|-----------|------|
| `[SupportsGet]` param | Tells ASP.NET to populate from query string |
| `Request.IsHtmx()` check | Detects HTMX request header |
| `PageOrPartial()` | Returns page or partial based on request type |
| `hx-get` attribute | AJAX endpoint for dynamic requests |
| `hx-target` attribute | Which element to update |
| `_ItemRows.cshtml` partial | Template for list rows only |

## Quick Start

### Step 1: Page Model

Create a list page model with search support:

```csharp
public class ProductsPageModel : BasePage
{
    private readonly IProductRepository _repo;

    [BindProperty(SupportsGet = true)]
    public string Search { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    public List<ProductDto> Products { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();

    public async Task OnGetAsync()
    {
        var query = _repo.Query(TenantId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(p => p.Name.Contains(Search));

        // Get total count before pagination
        var total = await query.CountAsync();

        // Paginate
        var pageSize = 10;
        var products = await query
            .Skip((Page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Products = products;
        Pagination = PaginationModel.FromDto(total, Page, pageSize);
    }
}
```

### Step 2: Search Form

Create a form with HTMX attributes:

```razor
<!-- Search Form -->
<form method="get" hx-get="@Url.Page()" hx-target="#products-list" hx-select="#products-list" hx-indicator="#search-spinner">
    <div class="row g-2">
        <div class="col-md-8">
            <input type="text"
                   name="Search"
                   value="@Model.Search"
                   class="form-control"
                   placeholder="Search products..."
                   hx-trigger="keyup changed delay:500ms"
                   hx-boost="true" />
        </div>
        <div class="col-md-4">
            <button type="submit" class="btn btn-primary w-100">
                <span id="search-spinner" class="htmx-request:hidden">
                    <i class="bi bi-search"></i> Search
                </span>
                <span class="hidden htmx-request:inline">
                    <i class="bi bi-hourglass-split"></i> Searching...
                </span>
            </button>
        </div>
    </div>
</form>
```

### Step 3: List Container

Wrap the list in a container with an ID matching `hx-target`:

```razor
<div id="products-list">
    @if (Model.Products.Any())
    {
        <div class="row g-4">
            <partial name="_ProductRows" model="Model.Products" />
        </div>

        <!-- Pagination -->
        @if (Model.Pagination.TotalPages > 1)
        {
            <nav class="d-flex justify-content-center mt-5">
                <ul class="pagination">
                    <!-- Page links here -->
                </ul>
            </nav>
        }
    }
    else
    {
        <div class="alert alert-info">No products found.</div>
    }
</div>
```

### Step 4: Partial View

Create `_ProductRows.cshtml` with just the list items:

```razor
@model List<ProductDto>

@foreach (var product in Model)
{
    <div class="col-md-6 col-lg-4">
        <div class="card h-100 shadow-sm">
            <div class="card-body">
                <h5 class="card-title">@product.Name</h5>
                <p class="card-text">@product.Description</p>
                <span class="h5">${@product.Price:0.00}</span>
            </div>
        </div>
    </div>
}
```

### Step 5: Handler

Add the `PageOrPartial()` check (from `BaseListPage<T>`):

```csharp
public IActionResult PageOrPartial()
{
    return Request.IsHtmx() ? Partial("_ProductRows", Products) : Page();
}
```

Call this in `OnGet`:

```csharp
public async Task OnGetAsync()
{
    // ... load data ...
    return PageOrPartial();  // Returns partial for HTMX, full page otherwise
}
```

## HTMX Attributes

### hx-get

Endpoint to call for dynamic requests:

```html
<form hx-get="@Url.Page()" hx-target="#list">
```

Sends `GET` request to the same page with `HX-Request: true` header.

### hx-target

CSS selector of element to update:

```html
<form hx-target="#products-list">
```

Replaces content of `<div id="products-list">` with the response.

### hx-trigger

Event that triggers the request:

```html
<input hx-trigger="keyup changed delay:500ms" />
```

- `keyup` — fires on every keystroke
- `changed` — only if value changed
- `delay:500ms` — waits 500ms before sending

Other triggers:
- `blur` — when field loses focus
- `change` — when field changes
- `submit` — on form submission (default)

### hx-indicator

Show/hide spinner during request:

```html
<button class="btn">
    <span id="search-spinner" class="htmx-request:hidden">
        <i class="bi bi-search"></i> Search
    </span>
    <span class="hidden htmx-request:inline">
        <i class="bi bi-hourglass-split"></i> Loading...
    </span>
</button>
```

The `.htmx-request:*` classes are automatically applied:
- `.htmx-request:hidden` → hidden during request
- `.htmx-request:inline` → visible during request

### hx-swap

How to swap the response into the DOM:

```html
<div hx-swap="innerHTML">  <!-- Replace inner content (default) -->
<div hx-swap="outerHTML">  <!-- Replace entire element -->
<div hx-swap="beforebegin"><!-- Insert before element -->
<div hx-swap="afterbegin"><!-- Insert at start of element -->
<div hx-swap="beforeend"><!-- Insert at end of element -->
<div hx-swap="afterend"><!-- Insert after element -->
```

Default is `innerHTML`, which replaces the target's inner content.

### hx-select

Extract only part of the response:

```html
<form hx-target="#list" hx-select="#list">
```

If response contains:

```html
<div class="page">
  <div id="list">
    <!-- This is inserted -->
  </div>
  <sidebar><!-- Ignored --></sidebar>
</div>
```

Only the `#list` div is used.

### hx-boost

Enhance regular form submissions with HTMX:

```html
<form method="get" hx-boost="true">
```

Converts the form to HTMX without changing the HTML.

## Common Patterns

### Pattern 1: Simple Search

```html
<form method="get" hx-get="@Url.Page()" hx-target="#results">
    <input type="text" name="Search" hx-trigger="keyup changed delay:500ms" />
    <button type="submit">Search</button>
</form>

<div id="results">
    <partial name="_SearchResults" model="Model.Items" />
</div>
```

### Pattern 2: Filter + Pagination

```html
<form hx-get="@Url.Page()" hx-target="#list">
    <input type="text" name="Search" />
    <select name="Category" hx-trigger="change">
        <option value="">All</option>
        <option>Electronics</option>
        <option>Books</option>
    </select>
    <button type="submit">Search</button>
</form>

<div id="list">
    <partial name="_Items" model="Model.Items" />
    <!-- Pagination links -->
</div>
```

### Pattern 3: Load More Button

```html
<div id="items">
    <partial name="_ItemRows" model="Model.Items" />
</div>

<button hx-get="@Url.Page(Model.Page + 1)" 
        hx-target="#items" 
        hx-swap="beforeend"
        class="btn btn-secondary w-100">
    Load More
</button>
```

### Pattern 4: Polling

Refresh list every 30 seconds:

```html
<div hx-get="@Url.Page()" hx-trigger="every 30s" hx-target="#list" id="list">
    <partial name="_ItemRows" model="Model.Items" />
</div>
```

## Extension: BaseListPage\<T>

The starter kit provides a convenience base class:

```csharp
public abstract class BaseListPage<T> : BasePage where T : class
{
    public List<T> Items { get; protected set; } = new();
    public PaginationModel Pagination { get; protected set; } = new();

    public IActionResult PageOrPartial()
    {
        return Request.IsHtmx() ? Partial("_ItemRows", Items) : Page();
    }
}
```

Usage:

```csharp
public class ProductsPageModel : BaseListPage<ProductDto>
{
    public async Task OnGetAsync()
    {
        // Load Items and Pagination
        Items = await GetProductsAsync();
        Pagination = CalculatePagination();

        return PageOrPartial();  // Automatic partial/page detection
    }
}
```

## Client-Side Behavior

### Works Without JavaScript

The form submits normally if HTMX isn't loaded:

```html
<form method="get" hx-get="@Url.Page()">
    <!-- Regular form post with SupportsGet properties -->
</form>
```

### With JavaScript

HTMX intercepts and makes async request, updating target element.

### Progressive Enhancement

- Add `hx-*` attributes to existing forms
- Form still works without HTMX
- With HTMX, UX is enhanced (no page reload)

## Common Mistakes

### Mistake 1: Wrong hx-target

❌ **Wrong:**
```html
<div hx-target="products-list">  <!-- Missing # -->
```

✅ **Correct:**
```html
<div hx-target="#products-list">
```

CSS selectors must include `#` for ID, `.` for class.

### Mistake 2: Forgetting SupportsGet

❌ **Wrong:**
```csharp
public string Search { get; set; }  // Not in query string
```

✅ **Correct:**
```csharp
[BindProperty(SupportsGet = true)]
public string Search { get; set; }
```

Without `SupportsGet`, query string parameters are ignored.

### Mistake 3: Not Checking Request.IsHtmx()

❌ **Wrong:**
```csharp
public async Task OnGetAsync()
{
    return Page();  // Always returns full page
}
```

✅ **Correct:**
```csharp
public async Task OnGetAsync()
{
    return Request.IsHtmx() ? Partial("_Items", Items) : Page();
}
```

### Mistake 4: Partial View Includes Layout

❌ **Wrong** (`_ItemRows.cshtml` with layout):
```html
@{
    Layout = "Shared/_Layout.cshtml";
}
<!-- Item rows here -->
```

The layout gets included, creating broken HTML.

✅ **Correct** (no layout):
```html
@foreach (var item in Model)
{
    <div>@item.Name</div>
}
```

### Mistake 5: Wrong hx-swap

❌ **Wrong:**
```html
<div hx-swap="outerHTML">
    <!-- If outerHTML, the entire div is replaced! -->
</div>
```

Use `innerHTML` (default) to replace inner content, or `outerHTML` only if you want to replace the container itself.

## See Also

- [Products List Page](../../src/SmartWorkz.StarterKitMVC.Public/Pages/Products/Index.cshtml) — Real example
- [HTMX Documentation](https://htmx.org/) — Official reference
- [Base Page Pattern](./03-base-page-pattern.md) — `PageOrPartial()` method
