# Razor Pages + HTMX - Interactive Pattern Guide

**Date:** 2026-04-01
**Status:** Ready to implement
**Tech Stack:** ASP.NET Razor Pages + HTMX + Alpine.js (optional)
**Use Case:** Public Portal - interactive without page reloads

---

## Why Razor Pages + HTMX is Perfect

| Aspect | Benefit |
|--------|---------|
| **No SPA overhead** | ✅ No React/Vue bundle bloat |
| **Server-side rendering** | ✅ SEO-friendly, fast initial load |
| **Interactive UX** | ✅ AJAX calls, partial HTML swaps |
| **Simple backend** | ✅ Return PartialView, no JSON API needed |
| **Progressive enhancement** | ✅ Works without JS (forms submit normally) |
| **Bandwidth efficient** | ✅ Send only HTML (not JSON + JS framework) |
| **Mixed team friendly** | ✅ Junior devs understand easily |

---

## Architecture Pattern

```
HTMX INTERACTIVE FLOW:

User clicks button
    ↓
HTMX sends request (via AJAX)
    ↓
Server processes (C# PageModel)
    ↓
Return PartialView (HTML fragment)
    ↓
HTMX swaps HTML in DOM
    ↓
User sees update (no page reload)

═══════════════════════════════════════════════════════════════

TRADITIONAL FORM FLOW (FALLBACK):

User clicks button (JS disabled)
    ↓
Form submits normally
    ↓
Server processes (C# PageModel)
    ↓
Return full page (HTTP redirect/render)
    ↓
User sees new page
```

---

## Complete Implementation Example

### 1️⃣ Product Listing with HTMX Interactions

**Pages/Products/Index.cshtml.cs (PageModel):**
```csharp
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IHttpClientFactory httpFactory, ILogger<IndexModel> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public List<ProductDto> Products { get; set; } = new();
    public string SortBy { get; set; } = "name";
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    // GET: /Products
    public async Task OnGetAsync(string sortBy = "name", int page = 1)
    {
        SortBy = sortBy;
        CurrentPage = page;

        await LoadProductsAsync();
    }

    // HTMX: /Products/Filter (returns PartialView)
    public async Task<IActionResult> OnGetFilterAsync(
        string search = "",
        string sortBy = "name",
        int page = 1)
    {
        CurrentPage = page;
        SortBy = sortBy;

        // Call API to get filtered products
        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync(
            $"/api/v1/products?search={search}&sort={sortBy}&page={page}&pageSize={PageSize}");

        var json = await response.Content.ReadAsStringAsync();
        Products = JsonConvert.DeserializeObject<List<ProductDto>>(json);

        // Return PartialView (fragment, not full page)
        return Partial("_ProductGrid", Products);
    }

    // HTMX: /Products/QuickView (returns PartialView)
    public async Task<IActionResult> OnGetQuickViewAsync(Guid productId)
    {
        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync($"/api/v1/products/{productId}");

        var product = await response.Content.ReadAsAsync<ProductDto>();

        // Return modal/popup as PartialView
        return Partial("_ProductQuickView", product);
    }

    // HTMX: /Products/AddToCart (returns PartialView or status)
    public async Task<IActionResult> OnPostAddToCartAsync(Guid productId, int quantity)
    {
        var client = _httpFactory.CreateClient();
        var token = HttpContext.Session.GetString("AccessToken");

        if (string.IsNullOrEmpty(token))
        {
            // Return login modal
            return Partial("_LoginPrompt");
        }

        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var body = new { productId, quantity };
        var response = await client.PostAsync(
            "/api/v1/cart/add",
            new StringContent(JsonConvert.SerializeObject(body),
                Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var cart = await response.Content.ReadAsAsync<CartDto>();

            // Return updated cart summary + toast notification
            return Partial("_CartSummary", cart);
        }

        // Return error message
        return Partial("_ErrorMessage",
            new { message = "Failed to add to cart" });
    }

    private async Task LoadProductsAsync()
    {
        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync(
            $"/api/v1/products?sort={SortBy}&page={CurrentPage}&pageSize={PageSize}");

        var json = await response.Content.ReadAsStringAsync();
        Products = JsonConvert.DeserializeObject<List<ProductDto>>(json);
    }
}
```

**Pages/Products/Index.cshtml (Main Page):**
```html
@page
@model IndexModel

@{
    ViewData["Title"] = "Products";
    ViewData["Description"] = "Browse our products";
}

<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"]</title>

    <!-- HTMX Library -->
    <script src="https://unpkg.com/htmx.org@1.9.10"></script>

    <!-- Alpine.js (optional, for simple interactions) -->
    <script src="https://unpkg.com/alpinejs@3.x.x/dist/cdn.min.js" defer></script>

    <link rel="stylesheet" href="~/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/products.css" />
</head>
<body>
    <div class="container mt-5">
        <h1>@ViewData["Title"]</h1>

        <!-- SEARCH & FILTER (HTMX) -->
        <div class="row mb-4">
            <div class="col-md-6">
                <input type="text"
                       class="form-control"
                       placeholder="Search products..."
                       hx-get="/Products/Filter"
                       hx-target="#productGrid"
                       hx-trigger="keyup changed delay:500ms"
                       hx-include="[name='sortBy']" />
            </div>
            <div class="col-md-6">
                <select class="form-control"
                        name="sortBy"
                        hx-get="/Products/Filter"
                        hx-target="#productGrid"
                        hx-trigger="change"
                        hx-include="[name='search']">
                    <option value="name">Name (A-Z)</option>
                    <option value="price-low">Price (Low to High)</option>
                    <option value="price-high">Price (High to Low)</option>
                    <option value="newest">Newest First</option>
                </select>
            </div>
        </div>

        <!-- LOADING INDICATOR -->
        <div hx-indicator="#loading" style="display:none;" id="loading">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>

        <!-- PRODUCT GRID (swapped by HTMX) -->
        <div id="productGrid">
            @await Html.PartialAsync("_ProductGrid", Model.Products)
        </div>

        <!-- PAGINATION (HTMX) -->
        <nav aria-label="Page navigation" class="mt-4">
            <ul class="pagination">
                <li class="page-item">
                    <a class="page-link"
                       href="#"
                       hx-get="/Products/Filter?page=@(Model.CurrentPage - 1)"
                       hx-target="#productGrid">
                        Previous
                    </a>
                </li>
                <li class="page-item active">
                    <span class="page-link">Page @Model.CurrentPage</span>
                </li>
                <li class="page-item">
                    <a class="page-link"
                       href="#"
                       hx-get="/Products/Filter?page=@(Model.CurrentPage + 1)"
                       hx-target="#productGrid">
                        Next
                    </a>
                </li>
            </ul>
        </nav>
    </div>

    <!-- TOAST NOTIFICATIONS (Alpine.js) -->
    <div x-data="{ showToast: false, message: '' }"
         x-show="showToast"
         class="alert alert-success position-fixed bottom-0 end-0 m-3"
         role="alert">
        <span x-text="message"></span>
    </div>

    <script>
        // HTMX: Show toast on successful add-to-cart
        document.body.addEventListener('htmx:afterSwap', (event) => {
            if (event.detail.xhr.status === 200 && event.detail.target.id === 'cartSummary') {
                // Show success toast
                console.log('Product added to cart!');
            }
        });
    </script>
</body>
</html>
```

**Pages/Products/_ProductGrid.cshtml (Partial View):**
```html
@model List<ProductDto>

@if (Model?.Any() == true)
{
    <div class="row">
        @foreach (var product in Model)
        {
            <div class="col-md-4 mb-4">
                <div class="card h-100">
                    <!-- Product Image -->
                    <img src="@product.ImageUrl"
                         class="card-img-top"
                         alt="@product.Name"
                         hx-get="/Products/QuickView?productId=@product.ProductId"
                         hx-target="body"
                         hx-swap="beforeend"
                         style="cursor: pointer;" />

                    <!-- Product Info -->
                    <div class="card-body d-flex flex-column">
                        <h5 class="card-title">@product.Name</h5>
                        <p class="card-text">@product.Description.Substring(0, 50)...</p>
                        <p class="card-text">
                            <strong class="text-success">${{product.Price}}</strong>
                        </p>

                        <!-- Rating -->
                        @if (product.Rating > 0)
                        {
                            <div class="mb-2">
                                @for (int i = 0; i < (int)product.Rating; i++)
                                {
                                    <span class="text-warning">★</span>
                                }
                            </div>
                        }

                        <!-- Actions -->
                        <div class="btn-group mt-auto" role="group">
                            <!-- View Details -->
                            <a asp-page="Details"
                               asp-route-id="@product.ProductId"
                               class="btn btn-sm btn-outline-primary flex-grow-1">
                                View
                            </a>

                            <!-- Add to Cart (HTMX) -->
                            <form hx-post="/Products/AddToCart"
                                  hx-target="#cartSummary"
                                  hx-swap="innerHTML"
                                  style="width: 100%;">
                                <input type="hidden" name="productId" value="@product.ProductId" />
                                <input type="hidden" name="quantity" value="1" />
                                <button type="submit"
                                        class="btn btn-sm btn-success flex-grow-1"
                                        hx-prompt="Quantity (1-10):"
                                        hx-confirm="Add to cart?">
                                    Add to Cart
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        }
    </div>
}
else
{
    <div class="alert alert-info">
        No products found. <a href="/Products">View all products</a>
    </div>
}
```

**Pages/Products/_ProductQuickView.cshtml (Modal Partial):**
```html
@model ProductDto

<!-- Modal -->
<div class="modal fade" id="quickViewModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">@Model.Name</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div class="row">
                    <div class="col-md-6">
                        <img src="@Model.ImageUrl" class="img-fluid" alt="@Model.Name" />
                    </div>
                    <div class="col-md-6">
                        <h3>@Model.Name</h3>
                        <p class="text-muted">SKU: @Model.Sku</p>

                        <!-- Price -->
                        <div class="mb-3">
                            <span class="h4 text-success">${{Model.Price}}</span>
                            @if (Model.DiscountPrice > 0)
                            {
                                <span class="text-muted text-decoration-line-through">${{Model.OriginalPrice}}</span>
                            }
                        </div>

                        <!-- Rating -->
                        <div class="mb-3">
                            @for (int i = 0; i < (int)Model.Rating; i++)
                            {
                                <span class="text-warning">★</span>
                            }
                            <span class="text-muted">(@Model.ReviewCount reviews)</span>
                        </div>

                        <!-- Description -->
                        <p>@Model.Description</p>

                        <!-- Stock Status -->
                        @if (Model.InStock)
                        {
                            <p class="text-success">✓ In Stock</p>
                        }
                        else
                        {
                            <p class="text-danger">Out of Stock</p>
                        }

                        <!-- Add to Cart -->
                        <form hx-post="/Products/AddToCart"
                              hx-target="#cartSummary"
                              hx-swap="innerHTML"
                              class="mt-4">
                            <input type="hidden" name="productId" value="@Model.ProductId" />
                            <div class="input-group mb-3">
                                <input type="number"
                                       name="quantity"
                                       class="form-control"
                                       value="1"
                                       min="1"
                                       max="10" />
                                <button type="submit"
                                        class="btn btn-success">
                                    Add to Cart
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Auto-show modal -->
<script>
    const quickViewModal = new bootstrap.Modal(document.getElementById('quickViewModal'));
    quickViewModal.show();
</script>
```

**Pages/Products/_CartSummary.cshtml (Mini Cart Partial):**
```html
@model CartDto

<div id="cartSummary" class="alert alert-success">
    <p><strong>✓ Added to cart!</strong></p>
    <p>Items: @Model.ItemCount | Total: $@Model.Total</p>
    <a href="/Cart" class="btn btn-sm btn-primary">View Cart</a>
</div>
```

---

## 2️⃣ Shopping Cart with HTMX Updates

**Pages/Cart/Index.cshtml.cs:**
```csharp
public class CartIndexModel : PageModel
{
    private readonly IHttpClientFactory _httpFactory;

    public CartIndexModel(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    public CartDto Cart { get; set; }

    public async Task OnGetAsync()
    {
        await LoadCartAsync();
    }

    // HTMX: Update quantity
    public async Task<IActionResult> OnPostUpdateQuantityAsync(Guid cartItemId, int quantity)
    {
        var client = _httpFactory.CreateClient();
        var token = HttpContext.Session.GetString("AccessToken");
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.PutAsync(
            $"/api/v1/cart/items/{cartItemId}",
            new StringContent(JsonConvert.SerializeObject(new { quantity }),
                Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            Cart = await response.Content.ReadAsAsync<CartDto>();
            return Partial("_CartTable", Cart);
        }

        return Partial("_ErrorMessage", new { message = "Update failed" });
    }

    // HTMX: Remove item
    public async Task<IActionResult> OnPostRemoveItemAsync(Guid cartItemId)
    {
        var client = _httpFactory.CreateClient();
        var token = HttpContext.Session.GetString("AccessToken");
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.DeleteAsync($"/api/v1/cart/items/{cartItemId}");

        if (response.IsSuccessStatusCode)
        {
            Cart = await response.Content.ReadAsAsync<CartDto>();
            return Partial("_CartTable", Cart);
        }

        return Partial("_ErrorMessage", new { message = "Remove failed" });
    }

    // HTMX: Apply coupon
    public async Task<IActionResult> OnPostApplyCouponAsync(string couponCode)
    {
        var client = _httpFactory.CreateClient();
        var token = HttpContext.Session.GetString("AccessToken");
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.PostAsync(
            $"/api/v1/cart/coupon",
            new StringContent(JsonConvert.SerializeObject(new { couponCode }),
                Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            Cart = await response.Content.ReadAsAsync<CartDto>();
            return Partial("_CartSummary", Cart);
        }

        return Partial("_ErrorMessage", new { message = "Invalid coupon" });
    }

    private async Task LoadCartAsync()
    {
        var client = _httpFactory.CreateClient();
        var token = HttpContext.Session.GetString("AccessToken");
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await client.GetAsync("/api/v1/cart");
        Cart = await response.Content.ReadAsAsync<CartDto>();
    }
}
```

**Pages/Cart/Index.cshtml:**
```html
@page
@model CartIndexModel

<div class="container mt-5">
    <h1>Shopping Cart</h1>

    <div class="row">
        <!-- Cart Items -->
        <div class="col-md-8">
            <div id="cartTable">
                @await Html.PartialAsync("_CartTable", Model.Cart)
            </div>
        </div>

        <!-- Cart Summary -->
        <div class="col-md-4">
            <div id="cartSummary" class="card">
                @await Html.PartialAsync("_CartSummary", Model.Cart)
            </div>
        </div>
    </div>
</div>
```

**Pages/Cart/_CartTable.cshtml:**
```html
@model CartDto

<table class="table">
    <thead>
        <tr>
            <th>Product</th>
            <th>Quantity</th>
            <th>Price</th>
            <th>Total</th>
            <th>Action</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td>@item.ProductName</td>
                <td>
                    <form hx-post="/Cart/UpdateQuantity"
                          hx-target="#cartTable"
                          hx-swap="innerHTML"
                          style="display:inline;">
                        <input type="hidden" name="cartItemId" value="@item.CartItemId" />
                        <input type="number"
                               name="quantity"
                               value="@item.Quantity"
                               min="1"
                               max="10"
                               class="form-control"
                               style="width: 60px;"
                               hx-trigger="change" />
                    </form>
                </td>
                <td>${{item.UnitPrice}}</td>
                <td>${{item.Total}}</td>
                <td>
                    <button hx-post="/Cart/RemoveItem"
                            hx-vals="js:{cartItemId: '@item.CartItemId'}"
                            hx-target="#cartTable"
                            hx-swap="innerHTML"
                            hx-confirm="Remove from cart?"
                            class="btn btn-sm btn-danger">
                        Remove
                    </button>
                </td>
            </tr>
        }
    </tbody>
</table>

@if (!Model.Items.Any())
{
    <div class="alert alert-info">
        Your cart is empty. <a href="/Products">Continue shopping</a>
    </div>
}
```

**Pages/Cart/_CartSummary.cshtml:**
```html
@model CartDto

<div class="card-body">
    <h5 class="card-title">Order Summary</h5>

    <div class="mb-3">
        <label class="form-label">Coupon Code</label>
        <form hx-post="/Cart/ApplyCoupon"
              hx-target="#cartSummary"
              hx-swap="innerHTML"
              class="input-group">
            <input type="text"
                   name="couponCode"
                   class="form-control"
                   placeholder="Enter code" />
            <button type="submit" class="btn btn-outline-primary">Apply</button>
        </form>
    </div>

    <hr />

    <div class="d-flex justify-content-between mb-2">
        <span>Subtotal:</span>
        <span>${{Model.Subtotal}}</span>
    </div>

    @if (Model.Discount > 0)
    {
        <div class="d-flex justify-content-between mb-2 text-success">
            <span>Discount:</span>
            <span>-$@Model.Discount</span>
        </div>
    }

    <div class="d-flex justify-content-between mb-2">
        <span>Shipping:</span>
        <span>${{Model.ShippingCost}}</span>
    </div>

    <hr />

    <div class="d-flex justify-content-between mb-3">
        <strong>Total:</strong>
        <strong class="text-success">${{Model.Total}}</strong>
    </div>

    <a href="/Checkout" class="btn btn-success w-100">Proceed to Checkout</a>
</div>
```

---

## 3️⃣ Form with Real-Time Validation (HTMX)

**Pages/Auth/Register.cshtml.cs:**
```csharp
public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpFactory;

    public RegisterModel(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    [BindProperty]
    public RegisterRequest Input { get; set; }

    // HTMX: Validate email in real-time
    public async Task<IActionResult> OnGetValidateEmailAsync(string email)
    {
        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync($"/api/v1/auth/check-email?email={email}");

        if (response.IsSuccessStatusCode)
        {
            var exists = await response.Content.ReadAsAsync<bool>();

            if (exists)
            {
                return Partial("_ValidationError",
                    new { field = "email", message = "Email already registered" });
            }

            return Partial("_ValidationSuccess", new { field = "email" });
        }

        return Partial("_ValidationError", new { field = "email", message = "Error checking email" });
    }

    // Traditional POST: Register
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var client = _httpFactory.CreateClient();
        var response = await client.PostAsync(
            "/api/v1/auth/register",
            new StringContent(JsonConvert.SerializeObject(Input),
                Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("RegisterSuccess");
        }

        ModelState.AddModelError("", "Registration failed");
        return Page();
    }
}
```

**Pages/Auth/Register.cshtml:**
```html
@page
@model RegisterModel

<div class="container mt-5">
    <div class="row">
        <div class="col-md-6">
            <h1>Register</h1>

            <form method="post">
                <!-- Email (with real-time validation) -->
                <div class="mb-3">
                    <label class="form-label">Email</label>
                    <input type="email"
                           name="Input.Email"
                           class="form-control"
                           hx-get="/Auth/Register/ValidateEmail"
                           hx-trigger="change"
                           hx-target="#emailValidation"
                           required />
                    <div id="emailValidation"></div>
                </div>

                <!-- Password -->
                <div class="mb-3">
                    <label class="form-label">Password</label>
                    <input type="password"
                           name="Input.Password"
                           class="form-control"
                           required />
                </div>

                <!-- First Name -->
                <div class="mb-3">
                    <label class="form-label">First Name</label>
                    <input type="text"
                           name="Input.FirstName"
                           class="form-control"
                           required />
                </div>

                <!-- Last Name -->
                <div class="mb-3">
                    <label class="form-label">Last Name</label>
                    <input type="text"
                           name="Input.LastName"
                           class="form-control"
                           required />
                </div>

                <button type="submit" class="btn btn-primary w-100">Register</button>
            </form>

            <p class="mt-3">
                Already have an account? <a href="/Auth/Login">Login here</a>
            </p>
        </div>
    </div>
</div>
```

**_ValidationError.cshtml:**
```html
@model dynamic

<div class="alert alert-danger mt-2" role="alert">
    ❌ @Model.message
</div>
```

**_ValidationSuccess.cshtml:**
```html
@model dynamic

<div class="alert alert-success mt-2" role="alert">
    ✓ @Model.field is available
</div>
```

---

## HTMX Attributes Reference

### Core Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `hx-get` | Fetch via GET | `hx-get="/api/products"` |
| `hx-post` | Submit via POST | `hx-post="/cart/add"` |
| `hx-put` | Submit via PUT | `hx-put="/cart/items/123"` |
| `hx-delete` | Submit via DELETE | `hx-delete="/cart/items/123"` |
| `hx-patch` | Submit via PATCH | `hx-patch="/items/123"` |
| `hx-target` | Where to swap HTML | `hx-target="#cartTable"` |
| `hx-swap` | How to swap | `hx-swap="innerHTML"` |
| `hx-trigger` | When to request | `hx-trigger="click"`, `"change"`, `"keyup"` |
| `hx-confirm` | Show confirmation | `hx-confirm="Delete?"` |
| `hx-prompt` | Ask for input | `hx-prompt="Enter quantity:"` |

### Swap Strategies

| Strategy | Effect |
|----------|--------|
| `innerHTML` | Replace inner HTML only |
| `outerHTML` | Replace entire element |
| `beforebegin` | Insert before element |
| `afterbegin` | Insert at start of element |
| `beforeend` | Insert at end of element |
| `afterend` | Insert after element |
| `delete` | Remove element |
| `none` | Don't swap |

### Trigger Options

| Trigger | When |
|---------|------|
| `click` | Element clicked |
| `change` | Form input changed |
| `submit` | Form submitted |
| `keyup` | Key released |
| `keyup changed delay:500ms` | Typed + wait 500ms |
| `mouseenter` | Mouse hovers |
| `load` | Page loads |

---

## Performance Tips

### 1️⃣ Debounce Search (avoid too many requests)

```html
<!-- Wait 500ms after user stops typing before sending request -->
<input type="text"
       placeholder="Search products..."
       hx-get="/Products/Filter"
       hx-trigger="keyup changed delay:500ms"
       hx-target="#results" />
```

### 2️⃣ Show Loading Indicator

```html
<!-- Show spinner while loading -->
<div hx-get="/products"
     hx-indicator="#loading">
    Load products
</div>

<div id="loading" class="spinner" style="display:none;">
    <div class="spinner-border"></div>
</div>
```

### 3️⃣ Cache Responses

```html
<!-- Cache result for 10 minutes -->
<button hx-get="/expensive-operation"
        hx-cache="10m">
    Load Data
</button>
```

### 4️⃣ Progressive Enhancement (forms work without JS)

```html
<!-- Works with traditional form submit if JS disabled -->
<form method="post" asp-page="AddToCart">
    <input type="hidden" name="productId" value="@product.ProductId" />
    <button type="submit"
            hx-post="/Products/AddToCart"
            hx-target="#cart"
            hx-swap="innerHTML">
        Add to Cart
    </button>
</form>
```

---

## Comparison: HTMX vs REST API vs Blazor

| Aspect | HTMX + Razor | REST API + SPA | Blazor |
|--------|-------------|----------------|--------|
| **Bundle Size** | ⚡ 13KB HTMX | ❌ 500KB+ React | ❌ 2-3MB Blazor |
| **Time to Interactive** | ✅ Fast | ⚠️ Medium | ❌ Slow |
| **SEO Friendly** | ✅ Yes | ❌ No | ⚠️ Limited |
| **Bandwidth** | ✅ Low (HTML) | ❌ High (JSON+JS) | ⚠️ Medium |
| **Learning Curve** | ✅ Easy | ❌ Hard | ⚠️ Medium |
| **Real-Time** | ⚠️ Polling | ✅ WebSocket | ✅ SignalR |
| **Offline Support** | ❌ No | ✅ Yes | ✅ Yes |
| **Mobile Friendly** | ✅ Yes | ✅ Yes | ⚠️ No WASM mobile |
| **Team Mixed Skills** | ✅ BEST | ❌ Worst | ⚠️ OK |

---

## Recommended Architecture

```
PUBLIC PORTAL (Public users):
┌─────────────────────────────────┐
│ Razor Pages + HTMX              │
├─────────────────────────────────┤
│ ✅ Lightweight (13KB HTMX)      │
│ ✅ SEO-friendly (server-render) │
│ ✅ Fast (minimal JS)            │
│ ✅ Progressive enhancement      │
│ ✅ Easy for mixed teams         │
└─────────────────────────────────┘

ADMIN PORTAL (Admins):
┌─────────────────────────────────┐
│ Blazor Server or Razor + HTMX   │
├─────────────────────────────────┤
│ ✅ Rich UI (components)         │
│ ✅ Real-time updates (SignalR)  │
│ ✅ Data-intensive (dashboards)  │
│ ✅ Stateful interactions        │
└─────────────────────────────────┘

SHARED:
┌─────────────────────────────────┐
│ REST API (/api/v1/*)            │
│ Services, DbContexts            │
│ Single Database (43 tables)      │
└─────────────────────────────────┘
```

---

## Summary

✅ **YES - PartialView + HTMX is PERFECT for:**
- Public portal (lightweight, SEO-friendly)
- Form validation (real-time, no page reload)
- Shopping cart (update quantities, remove items)
- Product filtering (search, sort, pagination)
- Notifications (toast alerts)
- Modal interactions

❌ **NOT ideal for:**
- Complex real-time dashboards (use Blazor instead)
- Heavy data visualizations (use Blazor + charts)
- Offline support (no offline capability)

---

## Next Steps

1. ✅ Create Razor Pages for Public Portal
2. ✅ Add HTMX for interactive features
3. ✅ Create PartialView fragments
4. ✅ Implement form validation
5. ✅ Add Alpine.js for simple state management (optional)
6. ✅ Test with API endpoints

Ready to start implementing? Let me know!
