# SmartWorkz.Sample.ECommerce Integration Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate all newly built SmartWorkz.Core.Web TagHelpers + SmartWorkz.Core.External export services into the Sample.ECommerce MVC project, fixing 4 known bugs in the process.

**Architecture:** The ECommerce project is pure ASP.NET Core MVC (Razor Views, .cshtml). We use the existing MVC TagHelpers from SmartWorkz.Core.Web (BreadcrumbTagHelper, BadgeTagHelper, AlertTagHelper, PaginationTagHelper, ButtonTagHelper) rather than enabling Blazor Server — this avoids SignalR overhead and the TagHelpers produce identical Bootstrap HTML. Export services (CSV, Excel, PDF) are added via SmartWorkz.Core.External. Four bugs are fixed: OrderController returning raw entities, CheckoutController DTO mismatch, missing IDbConnection DI, and AdminController using AutoMapper which is never registered.

**Tech Stack:** ASP.NET Core MVC, SmartWorkz.Core.Web TagHelpers, SmartWorkz.Core.External (CsvHelper, ClosedXML, iText7), SmartWorkz.Shared.IMapper, Bootstrap 5, Bootstrap Icons CDN

---

## Context

The user built 18 Blazor components (Phase 1), export services (Phase 2), tests (Phase 3), and docs (Phase 4) for SmartWorkz.Core. The Sample.ECommerce project is a working demo that currently uses none of these features. This plan wires up the integration so the demo showcases: breadcrumb navigation, status badges, contextual alerts, pagination, toast notifications, and CSV/Excel/PDF export of order data.

Four pre-existing bugs must be fixed during integration or the app will crash at runtime.

---

## Phase 1: Infrastructure (Tasks 1-3)

### Task 1: Add Core.External project reference and DI registrations

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/SmartWorkz.Sample.ECommerce.csproj`
- Modify: `src/SmartWorkz.Sample.ECommerce/ECommerceServiceExtensions.cs`

- [ ] **Step 1: Add ProjectReference to Core.External in .csproj**

Open `src/SmartWorkz.Sample.ECommerce/SmartWorkz.Sample.ECommerce.csproj`. Add inside the existing `<ItemGroup>` that has ProjectReferences:

```xml
<ProjectReference Include="..\SmartWorkz.Core.External\SmartWorkz.Core.External.csproj" />
```

- [ ] **Step 2: Register IDbConnection and export services in ECommerceServiceExtensions.cs**

Read the current `ECommerceServiceExtensions.cs` file. Add the following using statements at the top if not already present:

```csharp
using Microsoft.Data.Sqlite;
using System.Data;
using SmartWorkz.Core.External.Export;
using SmartWorkz.Core.Web.Extensions;
```

In the `AddECommerceServices` extension method, add before `return services;`:

```csharp
// IDbConnection required by CatalogSearchService
var connStr = configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=ecommerce.db";
services.AddScoped<IDbConnection>(_ => new SqliteConnection(connStr));

// Export services
services.AddScoped<IExportService, CsvExportService>();
services.AddScoped<IExcelExportService, ExcelExportService>();
services.AddScoped<IPdfExportService, PdfExportService>();

// SmartWorkz TagHelpers DI (if AddSmartWorkzCoreWeb exists)
services.AddSmartWorkzCoreWeb();
```

- [ ] **Step 3: Verify build compiles**

```bash
cd src/SmartWorkz.Sample.ECommerce
dotnet build
```
Expected: Build succeeded, 0 errors

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/SmartWorkz.Sample.ECommerce.csproj
git add src/SmartWorkz.Sample.ECommerce/ECommerceServiceExtensions.cs
git commit -m "feat(ecommerce): add Core.External reference and export service DI registrations"
```

---

### Task 2: Create _ViewImports.cshtml to register TagHelpers

**Files:**
- Create: `src/SmartWorkz.Sample.ECommerce/Views/_ViewImports.cshtml`

- [ ] **Step 1: Check if _ViewImports.cshtml exists**

```bash
ls src/SmartWorkz.Sample.ECommerce/Views/_ViewImports.cshtml
```

- [ ] **Step 2: Create (or update) _ViewImports.cshtml**

The file should contain:

```cshtml
@using SmartWorkz.Sample.ECommerce
@using SmartWorkz.Sample.ECommerce.Web.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, SmartWorkz.Core.Web
```

- [ ] **Step 3: Verify build still compiles**

```bash
dotnet build src/SmartWorkz.Sample.ECommerce
```
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/_ViewImports.cshtml
git commit -m "feat(ecommerce): register SmartWorkz TagHelpers in _ViewImports"
```

---

### Task 3: Add Bootstrap Icons CDN and TempData toast to _Layout.cshtml

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Shared/_Layout.cshtml`

- [ ] **Step 1: Add Bootstrap Icons CDN link in `<head>`**

In `_Layout.cshtml`, find the existing `<link>` tags in `<head>`. Add after the Bootstrap CSS link:

```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
```

- [ ] **Step 2: Add TempData toast partial before `</body>`**

Find the closing `</body>` tag. Before it, add:

```html
@if (TempData["ToastMessage"] != null)
{
    <div class="toast-container position-fixed bottom-0 end-0 p-3" style="z-index:9999">
        <div id="liveToast" class="toast show align-items-center text-bg-success border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi bi-check-circle me-2"></i>@TempData["ToastMessage"]
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    </div>
    <script>
        setTimeout(function() {
            var el = document.getElementById('liveToast');
            if (el) { var toast = new bootstrap.Toast(el, {delay:3000}); toast.show(); }
        }, 100);
    </script>
}
```

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/Shared/_Layout.cshtml
git commit -m "feat(ecommerce): add Bootstrap Icons CDN and TempData toast notification"
```

---

## Phase 2: Bug Fixes (Tasks 4-7)

### Task 4: Fix OrderController — entity-to-DTO mapping bug

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Web/Controllers/OrderController.cs`

**Bug:** `History()` and `Detail()` actions return raw `Order` entities but views declare `@model List<OrderDto>` / `@model OrderDto`. This causes a runtime `InvalidOperationException`.

- [ ] **Step 1: Inject SmartWorkz.Shared.IMapper**

In `OrderController.cs`, find the constructor. Add `SmartWorkz.Shared.IMapper mapper` parameter and store it:

```csharp
private readonly IOrderRepository _orderRepository;
private readonly SmartWorkz.Shared.IMapper _mapper;

public OrderController(IOrderRepository orderRepository, SmartWorkz.Shared.IMapper mapper)
{
    _orderRepository = orderRepository;
    _mapper = mapper;
}
```

Add using if needed: `using SmartWorkz.Shared;`

- [ ] **Step 2: Fix History() action to return mapped DTOs**

Find the `History()` action. Change the return to map entities:

```csharp
public async Task<IActionResult> History()
{
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var orders = await _orderRepository.GetByCustomerIdAsync(userId ?? string.Empty);
    var dtos = orders.Select(o => _mapper.Map<OrderDto>(o)).ToList();
    return View(dtos);
}
```

- [ ] **Step 3: Fix Detail() action to return mapped DTO**

Find the `Detail(int id)` action. Change to:

```csharp
public async Task<IActionResult> Detail(int id)
{
    var order = await _orderRepository.GetByIdAsync(id);
    if (order == null) return NotFound();
    var dto = _mapper.Map<OrderDto>(order);
    return View(dto);
}
```

- [ ] **Step 4: Verify build**

```bash
dotnet build src/SmartWorkz.Sample.ECommerce
```

- [ ] **Step 5: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Web/Controllers/OrderController.cs
git commit -m "fix(ecommerce): map Order entities to OrderDto before passing to views"
```

---

### Task 5: Fix CheckoutController — DTO field mismatch bug

**Files:**
- Create: `src/SmartWorkz.Sample.ECommerce/Web/Models/CheckoutViewModel.cs`
- Modify: `src/SmartWorkz.Sample.ECommerce/Web/Controllers/CheckoutController.cs`
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Checkout/Index.cshtml`

**Bug:** The Checkout form uses `firstName`, `lastName`, `email`, `address`, `zipCode` field names, but `CheckoutDto` has `Street`, `City`, `State`, `PostalCode`, `Country`. Model binding fails silently.

- [ ] **Step 1: Create CheckoutViewModel.cs**

Create `src/SmartWorkz.Sample.ECommerce/Web/Models/CheckoutViewModel.cs`:

```csharp
namespace SmartWorkz.Sample.ECommerce.Web.Models;

public class CheckoutViewModel
{
    public CartDto Cart { get; set; } = new();
    public CheckoutDto Checkout { get; set; } = new();
}
```

- [ ] **Step 2: Update CheckoutController to pass CheckoutViewModel**

In `CheckoutController.cs`, find the `Index()` GET action. Update it:

```csharp
public async Task<IActionResult> Index()
{
    var cart = await _cartService.GetCartAsync(GetUserId());
    var vm = new CheckoutViewModel { Cart = cart };
    return View(vm);
}
```

Find the `Index(CheckoutDto dto)` POST action. Change signature and body:

```csharp
[HttpPost]
public async Task<IActionResult> Index(CheckoutViewModel vm)
{
    if (!ModelState.IsValid)
    {
        vm.Cart = await _cartService.GetCartAsync(GetUserId());
        return View(vm);
    }
    var orderId = await _orderService.PlaceOrderAsync(GetUserId(), vm.Checkout);
    TempData["ToastMessage"] = "Order placed successfully!";
    return RedirectToAction("Detail", "Order", new { id = orderId });
}
```

- [ ] **Step 3: Rewrite Views/Checkout/Index.cshtml with correct field names**

Rewrite the model declaration and form fields in `Views/Checkout/Index.cshtml`:

```cshtml
@model CheckoutViewModel

@{
    ViewData["Title"] = "Checkout";
}

<breadcrumb items='@(new[] { 
    new { Text = "Home", Url = "/" }, 
    new { Text = "Cart", Url = "/Cart" }, 
    new { Text = "Checkout", Url = "" } 
})'></breadcrumb>

<h2>Checkout</h2>

<div class="row">
    <div class="col-md-8">
        <form asp-action="Index" method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>

            <h4>Shipping Address</h4>
            <div class="mb-3">
                <label asp-for="Checkout.Street" class="form-label">Street Address</label>
                <input asp-for="Checkout.Street" class="form-control" />
                <span asp-validation-for="Checkout.Street" class="text-danger"></span>
            </div>
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="Checkout.City" class="form-label">City</label>
                    <input asp-for="Checkout.City" class="form-control" />
                    <span asp-validation-for="Checkout.City" class="text-danger"></span>
                </div>
                <div class="col-md-3 mb-3">
                    <label asp-for="Checkout.State" class="form-label">State</label>
                    <input asp-for="Checkout.State" class="form-control" />
                </div>
                <div class="col-md-3 mb-3">
                    <label asp-for="Checkout.PostalCode" class="form-label">ZIP Code</label>
                    <input asp-for="Checkout.PostalCode" class="form-control" />
                    <span asp-validation-for="Checkout.PostalCode" class="text-danger"></span>
                </div>
            </div>
            <div class="mb-3">
                <label asp-for="Checkout.Country" class="form-label">Country</label>
                <input asp-for="Checkout.Country" class="form-control" value="US" />
            </div>

            <button variant="success" type="submit">Place Order</button>
        </form>
    </div>
    <div class="col-md-4">
        <h4>Order Summary</h4>
        @if (Model.Cart?.Items != null)
        {
            @foreach (var item in Model.Cart.Items)
            {
                <div class="d-flex justify-content-between">
                    <span>@item.ProductName × @item.Quantity</span>
                    <span>@item.TotalPrice.ToString("C")</span>
                </div>
            }
            <hr />
            <div class="d-flex justify-content-between fw-bold">
                <span>Total</span>
                <span>@Model.Cart.Total.ToString("C")</span>
            </div>
        }
    </div>
</div>
```

- [ ] **Step 4: Verify build**

```bash
dotnet build src/SmartWorkz.Sample.ECommerce
```

- [ ] **Step 5: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Web/Models/CheckoutViewModel.cs
git add src/SmartWorkz.Sample.ECommerce/Web/Controllers/CheckoutController.cs
git add src/SmartWorkz.Sample.ECommerce/Views/Checkout/Index.cshtml
git commit -m "fix(ecommerce): fix checkout DTO field mismatch and create CheckoutViewModel"
```

---

### Task 6: Fix AdminController — AutoMapper not registered bug

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Web/Controllers/AdminController.cs`

**Bug:** AdminController references `AutoMapper.IMapper` but `AddAutoMapper()` is never called in DI. Causes `InvalidOperationException: Unable to resolve service for type 'AutoMapper.IMapper'` on startup.

- [ ] **Step 1: Replace AutoMapper.IMapper with SmartWorkz.Shared.IMapper**

In `AdminController.cs`, find the constructor. Replace `AutoMapper.IMapper` with `SmartWorkz.Shared.IMapper`:

```csharp
private readonly IProductRepository _productRepository;
private readonly SmartWorkz.Shared.IMapper _mapper;

public AdminController(IProductRepository productRepository, SmartWorkz.Shared.IMapper mapper)
{
    _productRepository = productRepository;
    _mapper = mapper;
}
```

Remove any `using AutoMapper;` and add `using SmartWorkz.Shared;` if needed.

- [ ] **Step 2: Verify all _mapper.Map<> calls still compile**

Check that `_mapper.Map<ProductDto>(product)` and similar calls use the same API signature as SmartWorkz.Shared.IMapper (should be `Map<TDest>(source)`).

- [ ] **Step 3: Verify build**

```bash
dotnet build src/SmartWorkz.Sample.ECommerce
```

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Web/Controllers/AdminController.cs
git commit -m "fix(ecommerce): replace AutoMapper.IMapper with SmartWorkz.Shared.IMapper in AdminController"
```

---

### Task 7: Add toast notification to CartController

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Web/Controllers/CartController.cs`

- [ ] **Step 1: Add TempData toast on successful cart add**

In `CartController.cs`, find the `AddToCart` (or `Add`) POST action. After the cart service call succeeds, add:

```csharp
TempData["ToastMessage"] = $"'{productName}' added to cart!";
```

Use whatever variable holds the product name. If not available inline, use a generic message: `TempData["ToastMessage"] = "Item added to cart!";`

- [ ] **Step 2: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Web/Controllers/CartController.cs
git commit -m "feat(ecommerce): add TempData toast notification on cart add"
```

---

## Phase 3: Export Actions (Tasks 8-9)

### Task 8: Create flat OrderExportDto and add export actions to OrderController

**Files:**
- Create: `src/SmartWorkz.Sample.ECommerce/Web/Models/OrderExportDto.cs`
- Modify: `src/SmartWorkz.Sample.ECommerce/Web/Controllers/OrderController.cs`

- [ ] **Step 1: Create flat OrderExportDto**

Create `src/SmartWorkz.Sample.ECommerce/Web/Models/OrderExportDto.cs`:

```csharp
namespace SmartWorkz.Sample.ECommerce.Web.Models;

public record OrderExportDto(
    int Id,
    string CustomerId,
    string Status,
    decimal Total,
    string Currency,
    DateTime PlacedAt,
    int ItemCount
);
```

This avoids nested `List<OrderItemDto> Items` serializing as "System.Collections.Generic.List`1" in Excel/CSV.

- [ ] **Step 2: Inject export services in OrderController**

Add to OrderController constructor parameters:

```csharp
private readonly IExcelExportService _excelExport;
private readonly IPdfExportService _pdfExport;
private readonly IExportService _csvExport;

public OrderController(
    IOrderRepository orderRepository, 
    SmartWorkz.Shared.IMapper mapper,
    IExcelExportService excelExport,
    IPdfExportService pdfExport,
    IExportService csvExport)
{
    _orderRepository = orderRepository;
    _mapper = mapper;
    _excelExport = excelExport;
    _pdfExport = pdfExport;
    _csvExport = csvExport;
}
```

Add usings: `using SmartWorkz.Core.External.Export;`

- [ ] **Step 3: Add ExportExcel action**

```csharp
[HttpGet]
public async Task<IActionResult> ExportExcel()
{
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var orders = await _orderRepository.GetByCustomerIdAsync(userId ?? string.Empty);
    var exportData = orders.Select(o => new OrderExportDto(
        o.Id, o.CustomerId, o.Status.ToString(), o.Total, o.Currency,
        o.PlacedAt, o.Items?.Count ?? 0)).ToList();
    var result = await _excelExport.ExportAsync(exportData);
    if (!result.IsSuccess) return BadRequest(result.Error);
    return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
}
```

- [ ] **Step 4: Add ExportPdf action**

```csharp
[HttpGet]
public async Task<IActionResult> ExportPdf()
{
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var orders = await _orderRepository.GetByCustomerIdAsync(userId ?? string.Empty);
    var exportData = orders.Select(o => new OrderExportDto(
        o.Id, o.CustomerId, o.Status.ToString(), o.Total, o.Currency,
        o.PlacedAt, o.Items?.Count ?? 0)).ToList();
    var result = await _pdfExport.ExportAsync(exportData, "My Orders");
    if (!result.IsSuccess) return BadRequest(result.Error);
    return File(result.Value, "application/pdf", "orders.pdf");
}
```

- [ ] **Step 5: Add ExportCsv action**

```csharp
[HttpGet]
public async Task<IActionResult> ExportCsv()
{
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var orders = await _orderRepository.GetByCustomerIdAsync(userId ?? string.Empty);
    var exportData = orders.Select(o => new OrderExportDto(
        o.Id, o.CustomerId, o.Status.ToString(), o.Total, o.Currency,
        o.PlacedAt, o.Items?.Count ?? 0)).ToList();
    var result = await _csvExport.ExportAsync(exportData);
    if (!result.IsSuccess) return BadRequest(result.Error);
    return File(result.Value, "text/csv", "orders.csv");
}
```

- [ ] **Step 6: Verify build**

```bash
dotnet build src/SmartWorkz.Sample.ECommerce
```

- [ ] **Step 7: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Web/Models/OrderExportDto.cs
git add src/SmartWorkz.Sample.ECommerce/Web/Controllers/OrderController.cs
git commit -m "feat(ecommerce): add order export actions (CSV, Excel, PDF) with flat OrderExportDto"
```

---

### Task 9: Add export actions to AdminController

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Web/Controllers/AdminController.cs`

- [ ] **Step 1: Inject export services in AdminController**

Add to AdminController constructor:

```csharp
private readonly IExcelExportService _excelExport;
private readonly IPdfExportService _pdfExport;

public AdminController(
    IProductRepository productRepository,
    SmartWorkz.Shared.IMapper mapper,
    IExcelExportService excelExport,
    IPdfExportService pdfExport)
{
    _productRepository = productRepository;
    _mapper = mapper;
    _excelExport = excelExport;
    _pdfExport = pdfExport;
}
```

- [ ] **Step 2: Add ExportProductsExcel action**

```csharp
[HttpGet]
public async Task<IActionResult> ExportProductsExcel()
{
    var products = await _productRepository.GetAllAsync();
    var dtos = products.Select(p => _mapper.Map<ProductDto>(p)).ToList();
    var result = await _excelExport.ExportAsync(dtos);
    if (!result.IsSuccess) return BadRequest(result.Error);
    return File(result.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx");
}
```

- [ ] **Step 3: Add ExportProductsCsv action**

```csharp
[HttpGet]  
public async Task<IActionResult> ExportProductsCsv()
{
    var products = await _productRepository.GetAllAsync();
    var dtos = products.Select(p => _mapper.Map<ProductDto>(p)).ToList();
    var result = await _csvExport.ExportAsync(dtos);
    if (!result.IsSuccess) return BadRequest(result.Error);
    return File(result.Value, "text/csv", "products.csv");
}
```

Note: also inject `IExportService _csvExport` in constructor.

- [ ] **Step 4: Verify build**

```bash
dotnet build src/SmartWorkz.Sample.ECommerce
```

- [ ] **Step 5: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Web/Controllers/AdminController.cs
git commit -m "feat(ecommerce): add product export actions to AdminController"
```

---

## Phase 4: UI TagHelper Integration (Tasks 10-16)

### Task 10: Update Admin Products view with export buttons, badges, pagination

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Admin/Products.cshtml`

- [ ] **Step 1: Add export buttons and badge to Products.cshtml**

At the top of the page (after the `<h2>` heading), add export buttons:

```cshtml
<div class="d-flex justify-content-between align-items-center mb-3">
    <h2>Products</h2>
    <div>
        <a href="/Admin/ExportProductsExcel" class="btn btn-success btn-sm me-1">
            <i class="bi bi-file-earmark-excel me-1"></i>Export Excel
        </a>
        <a href="/Admin/ExportProductsCsv" class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-filetype-csv me-1"></i>Export CSV
        </a>
    </div>
</div>
```

- [ ] **Step 2: Add stock status badge to product table rows**

In the `@foreach` loop that renders product rows, add a badge in the stock column:

```cshtml
@if (item.StockQuantity > 10)
{
    <badge variant="success">In Stock</badge>
}
else if (item.StockQuantity > 0)
{
    <badge variant="warning">Low Stock</badge>
}
else
{
    <badge variant="danger">Out of Stock</badge>
}
```

- [ ] **Step 3: Add pagination taghelper if page model supports it**

If the view model has `TotalPages`/`CurrentPage` properties, add at the bottom:

```cshtml
<pagination current-page="Model.CurrentPage" total-pages="Model.TotalPages" action="Products" controller="Admin"></pagination>
```

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/Admin/Products.cshtml
git commit -m "feat(ecommerce): add export buttons, stock badges, and pagination to Admin Products view"
```

---

### Task 11: Update Order History view with breadcrumb, badges, export buttons

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Order/History.cshtml`

- [ ] **Step 1: Add breadcrumb at top of History.cshtml**

Replace or add above the `<h2>` heading:

```cshtml
<breadcrumb items='new[] { new { Text="Home", Url="/" }, new { Text="My Orders", Url="" } }'></breadcrumb>
```

Note: The exact BreadcrumbTagHelper attribute name depends on the implementation. If it uses `asp-items`, adjust accordingly. Check `src/SmartWorkz.Core.Web/TagHelpers/Navigation/BreadcrumbTagHelper.cs` for the correct attribute name.

- [ ] **Step 2: Add export buttons**

```cshtml
<div class="d-flex justify-content-between align-items-center mb-3">
    <h2>My Orders</h2>
    <div>
        <a href="/Order/ExportExcel" class="btn btn-success btn-sm me-1">
            <i class="bi bi-file-earmark-excel me-1"></i>Export Excel
        </a>
        <a href="/Order/ExportPdf" class="btn btn-danger btn-sm me-1">
            <i class="bi bi-file-earmark-pdf me-1"></i>Export PDF
        </a>
        <a href="/Order/ExportCsv" class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-filetype-csv me-1"></i>Export CSV
        </a>
    </div>
</div>
```

- [ ] **Step 3: Add order status badges in table rows**

In the `@foreach` loop, replace plain text status with a badge:

```cshtml
@{
    var badgeVariant = item.Status switch {
        "Delivered" => "success",
        "Shipped" => "info",
        "Processing" => "warning",
        "Cancelled" => "danger",
        _ => "secondary"
    };
}
<badge variant="@badgeVariant">@item.Status</badge>
```

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/Order/History.cshtml
git commit -m "feat(ecommerce): add breadcrumb, status badges, and export buttons to Order History view"
```

---

### Task 12: Update Order Detail view with breadcrumb and status badge

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Order/Detail.cshtml`

- [ ] **Step 1: Add breadcrumb**

Add at the top of `Detail.cshtml`:

```cshtml
<breadcrumb items='new[] { 
    new { Text="Home", Url="/" }, 
    new { Text="My Orders", Url="/Order/History" }, 
    new { Text=$"Order #{Model.Id}", Url="" } 
}'></breadcrumb>
```

- [ ] **Step 2: Replace plain status text with badge**

Find where `Model.Status` is displayed and replace with:

```cshtml
@{
    var statusVariant = Model.Status switch {
        "Delivered" => "success",
        "Shipped" => "info",
        "Processing" => "warning",
        "Cancelled" => "danger",
        _ => "secondary"
    };
}
<badge variant="@statusVariant" is-pill="true">@Model.Status</badge>
```

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/Order/Detail.cshtml
git commit -m "feat(ecommerce): add breadcrumb and status badge to Order Detail view"
```

---

### Task 13: Update Product Detail view with breadcrumb, stock badge, out-of-stock alert

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Product/Detail.cshtml`

- [ ] **Step 1: Add breadcrumb**

```cshtml
<breadcrumb items='new[] { 
    new { Text="Home", Url="/" }, 
    new { Text="Products", Url="/Catalog/Products" }, 
    new { Text=Model.Name, Url="" } 
}'></breadcrumb>
```

- [ ] **Step 2: Add stock badge near product title**

```cshtml
@if (Model.StockQuantity > 10)
{
    <badge variant="success">In Stock</badge>
}
else if (Model.StockQuantity > 0)
{
    <badge variant="warning">Only @Model.StockQuantity left</badge>
}
else
{
    <badge variant="danger">Out of Stock</badge>
}
```

- [ ] **Step 3: Add out-of-stock alert when stock is 0**

```cshtml
@if (Model.StockQuantity == 0)
{
    <alert type="warning" dismissible="false">
        This product is currently out of stock. Check back soon!
    </alert>
}
```

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/Product/Detail.cshtml
git commit -m "feat(ecommerce): add breadcrumb, stock badge, and out-of-stock alert to Product Detail"
```

---

### Task 14: Update Catalog views with breadcrumb, badges, pagination

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Catalog/Products.cshtml`
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Catalog/CategoryProducts.cshtml`

- [ ] **Step 1: Add breadcrumb to Catalog/Products.cshtml**

```cshtml
<breadcrumb items='new[] { 
    new { Text="Home", Url="/" }, 
    new { Text="All Products", Url="" } 
}'></breadcrumb>
```

- [ ] **Step 2: Add stock badges to product cards in Products.cshtml**

In the product card loop, add stock badge:

```cshtml
<badge variant="@(item.StockQuantity > 0 ? "success" : "danger")">
    @(item.StockQuantity > 0 ? "In Stock" : "Out of Stock")
</badge>
```

- [ ] **Step 3: Add pagination to Products.cshtml bottom**

If model has paging info:
```cshtml
<pagination current-page="Model.CurrentPage" total-pages="Model.TotalPages" action="Products" controller="Catalog"></pagination>
```

- [ ] **Step 4: Add breadcrumb to CategoryProducts.cshtml**

```cshtml
<breadcrumb items='new[] { 
    new { Text="Home", Url="/" }, 
    new { Text="Products", Url="/Catalog/Products" }, 
    new { Text=Model.CategoryName, Url="" } 
}'></breadcrumb>
```

- [ ] **Step 5: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/Catalog/Products.cshtml
git add src/SmartWorkz.Sample.ECommerce/Views/Catalog/CategoryProducts.cshtml
git commit -m "feat(ecommerce): add breadcrumbs, stock badges, and pagination to Catalog views"
```

---

### Task 15: Update Cart view with breadcrumb and empty cart alert

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Cart/Index.cshtml`

- [ ] **Step 1: Add breadcrumb**

```cshtml
<breadcrumb items='new[] { 
    new { Text="Home", Url="/" }, 
    new { Text="Shopping Cart", Url="" } 
}'></breadcrumb>
```

- [ ] **Step 2: Add empty cart alert**

Find the empty cart conditional check (where `Model.Items.Count == 0` or similar):

```cshtml
@if (!Model.Items.Any())
{
    <alert type="info" dismissible="false">
        Your cart is empty. <a href="/Catalog/Products">Continue shopping</a>
    </alert>
}
```

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/Cart/Index.cshtml
git commit -m "feat(ecommerce): add breadcrumb and empty cart alert to Cart view"
```

---

### Task 16: Update Admin Create/Edit views and Home with button taghelpers

**Files:**
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Admin/Create.cshtml`
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Admin/Edit.cshtml`
- Modify: `src/SmartWorkz.Sample.ECommerce/Views/Home/Index.cshtml`

- [ ] **Step 1: Replace submit button in Admin/Create.cshtml**

Find `<button type="submit"` and replace with ButtonTagHelper syntax:

```cshtml
<button variant="success" type="submit">
    <i class="bi bi-plus-circle me-1"></i>Create Product
</button>
```

- [ ] **Step 2: Replace submit button in Admin/Edit.cshtml**

```cshtml
<button variant="primary" type="submit">
    <i class="bi bi-pencil-square me-1"></i>Save Changes
</button>
```

- [ ] **Step 3: Update Home/Index.cshtml Shop Now button**

Find the primary CTA button and replace with:

```cshtml
<button variant="success" asp-controller="Catalog" asp-action="Products">
    <i class="bi bi-shop me-1"></i>Shop Now
</button>
```

Note: If ButtonTagHelper doesn't render as `<a>` when asp-controller is present, use standard `<a>` with Bootstrap classes instead.

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.Sample.ECommerce/Views/Admin/Create.cshtml
git add src/SmartWorkz.Sample.ECommerce/Views/Admin/Edit.cshtml
git add src/SmartWorkz.Sample.ECommerce/Views/Home/Index.cshtml
git commit -m "feat(ecommerce): use ButtonTagHelper in Admin Create/Edit and Home views"
```

---

## Verification

After all tasks complete, verify the integration end-to-end:

### 1. Build verification
```bash
dotnet build src/SmartWorkz.Sample.ECommerce
```
Expected: Build succeeded, 0 errors, 0 warnings

### 2. Runtime startup
```bash
dotnet run --project src/SmartWorkz.Sample.ECommerce
```
Expected: App starts on https://localhost:5XXX with no `InvalidOperationException` about unresolved services (no AutoMapper, no IDbConnection errors)

### 3. TagHelper rendering
- Navigate to `/Catalog/Products` — verify breadcrumb renders as `Home > All Products`
- Navigate to any product detail — verify stock badge appears (green/yellow/red)
- Add item to cart — verify toast notification appears at bottom-right

### 4. Export functionality
- Navigate to `/Order/History` — click "Export Excel" → browser downloads `orders.xlsx`
- Click "Export PDF" → browser downloads `orders.pdf`
- Click "Export CSV" → browser downloads `orders.csv`
- Navigate to `/Admin/Products` — click "Export Products Excel" → downloads `products.xlsx`

### 5. Bug fix verification
- Navigate to `/Order/History` — verify page loads without runtime error (entity→DTO bug fix)
- Navigate to `/Checkout` — verify form fields match DTO properties (no silent binding failures)
- Navigate to `/Admin/Products` — verify page loads without AutoMapper DI error
