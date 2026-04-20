# SmartWorkz.Core - Junior Developer Quick Start

**Copy-paste ready code examples for every system**  
*For developers new to the SmartWorkz ecosystem*

---

## Table of Contents

1. [Complete Result<T> Pattern Example](#complete-resultt-pattern-example)
2. [Complete Cache Service Example](#complete-cache-service-example)
3. [Complete Grid Component Example](#complete-grid-component-example)
4. [Complete Multi-Tenancy Example](#complete-multi-tenancy-example)
5. [Complete Feature Flags Example](#complete-feature-flags-example)
6. [Complete Event Bus Example](#complete-event-bus-example)
7. [Complete Validation Example](#complete-validation-example)
8. [Complete Repository Pattern Example](#complete-repository-pattern-example)
9. [Complete Unit of Work Example](#complete-unit-of-work-example)
10. [Complete Integration Example](#complete-integration-example)

---

## Complete Result<T> Pattern Example

### Step 1: Define Your Domain Model

```csharp
// Models/Product.cs
namespace YourApp.Domain.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Sku { get; set; } = "";
    public int StockQuantity { get; set; }
    public string Category { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Step 2: Create Your Service

```csharp
// Services/ProductService.cs
using SmartWorkz.Core.Shared.Results;
using YourApp.Domain.Models;

namespace YourApp.Application.Services;

public class ProductService
{
    private readonly List<Product> _products = new(); // Mock database

    // Success case
    public async Task<Result<Product>> GetProductAsync(int productId)
    {
        await Task.Delay(10); // Simulate DB call
        
        var product = _products.FirstOrDefault(p => p.Id == productId);
        
        if (product == null)
            return Result<Product>.Failure(new Error
            {
                Code = "PRODUCT_NOT_FOUND",
                Message = $"Product with ID {productId} not found"
            });

        return Result<Product>.Success(product);
    }

    // Failure case with validation
    public async Task<Result<Product>> CreateProductAsync(string name, decimal price, string sku)
    {
        await Task.Delay(10);

        // Validation check
        if (string.IsNullOrWhiteSpace(name))
            return Result<Product>.Failure(new Error
            {
                Code = "INVALID_NAME",
                Message = "Product name cannot be empty"
            });

        if (price <= 0)
            return Result<Product>.Failure(new Error
            {
                Code = "INVALID_PRICE",
                Message = "Product price must be greater than 0"
            });

        // Check for duplicate SKU
        if (_products.Any(p => p.Sku == sku))
            return Result<Product>.Failure(new Error
            {
                Code = "DUPLICATE_SKU",
                Message = $"A product with SKU '{sku}' already exists"
            });

        // Create new product
        var product = new Product
        {
            Id = _products.Count + 1,
            Name = name,
            Price = price,
            Sku = sku
        };

        _products.Add(product);
        return Result<Product>.Success(product);
    }
}
```

### Step 3: Use in Controller/Page

```csharp
// Pages/Products/Create.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YourApp.Application.Services;

namespace YourApp.Pages.Products;

public class CreateModel : PageModel
{
    private readonly ProductService _productService;

    [BindProperty]
    public string ProductName { get; set; } = "";

    [BindProperty]
    public decimal ProductPrice { get; set; }

    [BindProperty]
    public string ProductSku { get; set; } = "";

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public CreateModel(ProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Call service
        var result = await _productService.CreateProductAsync(
            ProductName,
            ProductPrice,
            ProductSku
        );

        // Handle result
        if (result.IsSuccess)
        {
            SuccessMessage = $"Product '{result.Value!.Name}' created successfully!";
            return RedirectToPage("Index");
        }
        else
        {
            // Show error to user
            ErrorMessage = result.Error?.Message;
            return Page();
        }
    }
}
```

### Step 4: Display in Razor

```html
<!-- Pages/Products/Create.cshtml -->
@page
@model CreateModel

<div class="container mt-5">
    <h2>Create Product</h2>

    @if (!string.IsNullOrEmpty(Model.ErrorMessage))
    {
        <div class="alert alert-danger">
            <i class="bi bi-exclamation-circle"></i>
            @Model.ErrorMessage
        </div>
    }

    <form method="post">
        <div class="mb-3">
            <label for="productName">Product Name</label>
            <input type="text" class="form-control" id="productName" 
                   asp-for="ProductName" required />
        </div>

        <div class="mb-3">
            <label for="productPrice">Price</label>
            <input type="number" class="form-control" id="productPrice" 
                   asp-for="ProductPrice" step="0.01" required />
        </div>

        <div class="mb-3">
            <label for="productSku">SKU</label>
            <input type="text" class="form-control" id="productSku" 
                   asp-for="ProductSku" required />
        </div>

        <button type="submit" class="btn btn-primary">Create</button>
    </form>
</div>
```

---

## Complete Cache Service Example

### Step 1: Setup in Program.cs

```csharp
// Program.cs
using SmartWorkz.Core.Shared.Caching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Add cache service
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

// Optional: Configure cache options
builder.Services.Configure<CacheOptions>(options =>
{
    options.Duration = TimeSpan.FromMinutes(30);
    options.Strategy = CacheStrategy.Sliding;
});

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();
app.Run();
```

### Step 2: Use Cache in Service

```csharp
// Services/UserService.cs
using SmartWorkz.Core.Shared.Caching;
using YourApp.Domain.Models;

namespace YourApp.Application.Services;

public class UserService
{
    private readonly ICacheService _cacheService;
    private readonly List<User> _users = new(); // Mock DB

    public UserService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        // Try cache first
        var cacheKey = $"user:{userId}";
        var cachedResult = await _cacheService.GetAsync<User>(cacheKey);

        if (cachedResult.IsSuccess && cachedResult.Value != null)
        {
            Console.WriteLine($"✓ Cache hit for {cacheKey}");
            return cachedResult.Value;
        }

        // Cache miss - get from "database"
        Console.WriteLine($"✗ Cache miss for {cacheKey} - fetching from DB");
        var user = _users.FirstOrDefault(u => u.Id == userId);

        if (user != null)
        {
            // Cache for 15 minutes
            await _cacheService.SetAsync(cacheKey, user, new CacheOptions
            {
                Duration = TimeSpan.FromMinutes(15),
                Strategy = CacheStrategy.Sliding
            });
        }

        return user;
    }

    public async Task<User?> UpdateUserAsync(int userId, string newName)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return null;

        user.Name = newName;

        // Invalidate cache
        var cacheKey = $"user:{userId}";
        await _cacheService.RemoveAsync(cacheKey);
        Console.WriteLine($"✓ Cache invalidated for {cacheKey}");

        return user;
    }

    public async Task<List<User>> GetUsersByDepartmentAsync(string department)
    {
        var cacheKey = $"users:dept:{department}";
        var cachedResult = await _cacheService.GetAsync<List<User>>(cacheKey);

        if (cachedResult.IsSuccess && cachedResult.Value != null)
            return cachedResult.Value;

        var users = _users.Where(u => u.Department == department).ToList();

        await _cacheService.SetAsync(cacheKey, users, new CacheOptions
        {
            Duration = TimeSpan.FromMinutes(20)
        });

        return users;
    }

    // Bulk invalidate
    public async Task ClearDepartmentCacheAsync(string department)
    {
        await _cacheService.RemoveByPrefixAsync($"users:dept:{department}");
        Console.WriteLine($"✓ All department caches cleared for {department}");
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Department { get; set; } = "";
}
```

### Step 3: Use in Page

```csharp
// Pages/Users/Details.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using YourApp.Application.Services;
using YourApp.Domain.Models;

namespace YourApp.Pages.Users;

public class DetailsModel : PageModel
{
    private readonly UserService _userService;
    public User? User { get; set; }

    public DetailsModel(UserService userService)
    {
        _userService = userService;
    }

    public async Task OnGetAsync(int id)
    {
        User = await _userService.GetUserAsync(id);
    }
}
```

---

## Complete Grid Component Example

### Step 1: Create Data Model

```csharp
// Models/OrderDto.cs
namespace YourApp.Models;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "";
    public string CustomerName { get; set; } = "";
}
```

### Step 2: Setup Grid in Page Model

```csharp
// Pages/Orders/Index.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartWorkz.Core.Shared.Grid;
using YourApp.Models;

namespace YourApp.Pages.Orders;

public class IndexModel : PageModel
{
    public string ApiEndpoint { get; set; } = "/api/orders/grid";
    public List<GridColumn> Columns { get; set; } = new();

    public void OnGet()
    {
        // Define columns
        Columns = new List<GridColumn>
        {
            new GridColumn
            {
                PropertyName = "OrderNumber",
                DisplayName = "Order #",
                IsSortable = true,
                IsFilterable = true,
                FilterType = "text",
                Width = "120px",
                Order = 1
            },
            new GridColumn
            {
                PropertyName = "OrderDate",
                DisplayName = "Date",
                IsSortable = true,
                IsFilterable = true,
                FilterType = "date",
                Order = 2
            },
            new GridColumn
            {
                PropertyName = "CustomerName",
                DisplayName = "Customer",
                IsSortable = true,
                IsFilterable = true,
                FilterType = "text",
                Order = 3
            },
            new GridColumn
            {
                PropertyName = "Total",
                DisplayName = "Total Amount",
                IsSortable = true,
                IsFilterable = true,
                FilterType = "range",
                Order = 4
            },
            new GridColumn
            {
                PropertyName = "Status",
                DisplayName = "Status",
                IsSortable = true,
                IsFilterable = true,
                FilterType = "dropdown",
                Order = 5
            }
        };
    }
}
```

### Step 3: Create Razor View

```html
<!-- Pages/Orders/Index.cshtml -->
@page
@model IndexModel
@using SmartWorkz.Core.Shared.Grid;

<div class="container-fluid mt-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Orders</h2>
        <a href="/orders/create" class="btn btn-primary">New Order</a>
    </div>

    <grid asp-data-source="@Model.ApiEndpoint"
          asp-columns="@Model.Columns"
          asp-page-size="25"
          asp-allow-selection="true"
          asp-allow-export="true"
          asp-allow-column-visibility-toggle="true">
    </grid>
</div>
```

### Step 4: Create API Endpoint

```csharp
// Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core.Shared.Grid;
using SmartWorkz.Core.Shared.Results;
using YourApp.Models;

namespace YourApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly List<OrderDto> _orders = new()
    {
        new OrderDto { Id = 1, OrderNumber = "ORD-001", OrderDate = DateTime.UtcNow.AddDays(-5), Total = 1250.50m, Status = "Completed", CustomerName = "John Doe" },
        new OrderDto { Id = 2, OrderNumber = "ORD-002", OrderDate = DateTime.UtcNow.AddDays(-3), Total = 890.25m, Status = "Pending", CustomerName = "Jane Smith" },
        new OrderDto { Id = 3, OrderNumber = "ORD-003", OrderDate = DateTime.UtcNow.AddDays(-1), Total = 2100.00m, Status = "Shipped", CustomerName = "Bob Johnson" },
    };

    [HttpPost("grid")]
    public async Task<Result<GridResponse<OrderDto>>> GetGridData([FromBody] GridRequest request)
    {
        try
        {
            // Get base query
            var query = _orders.AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(o =>
                    o.OrderNumber.Contains(request.SearchTerm) ||
                    o.CustomerName.Contains(request.SearchTerm)
                );
            }

            // Apply filters
            if (request.Filters != null)
            {
                if (request.Filters.TryGetValue("Status", out var statusFilter))
                    query = query.Where(o => o.Status == statusFilter.ToString());

                if (request.Filters.TryGetValue("TotalMin", out var minTotal) &&
                    decimal.TryParse(minTotal.ToString(), out var minVal))
                    query = query.Where(o => o.Total >= minVal);

                if (request.Filters.TryGetValue("TotalMax", out var maxTotal) &&
                    decimal.TryParse(maxTotal.ToString(), out var maxVal))
                    query = query.Where(o => o.Total <= maxVal);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                query = request.SortBy.ToLower() switch
                {
                    "ordernumber" => request.SortDescending 
                        ? query.OrderByDescending(o => o.OrderNumber)
                        : query.OrderBy(o => o.OrderNumber),
                    "orderdate" => request.SortDescending
                        ? query.OrderByDescending(o => o.OrderDate)
                        : query.OrderBy(o => o.OrderDate),
                    "total" => request.SortDescending
                        ? query.OrderByDescending(o => o.Total)
                        : query.OrderBy(o => o.Total),
                    _ => query.OrderByDescending(o => o.OrderDate)
                };
            }

            var totalCount = query.Count();

            // Apply paging
            var skip = (request.Page - 1) * request.PageSize;
            var pagedData = query.Skip(skip).Take(request.PageSize).ToList();

            var response = new GridResponse<OrderDto>
            {
                Data = new PagedList<OrderDto>(pagedData, request.Page, request.PageSize, totalCount),
                Columns = GetGridColumns()
            };

            return Result<GridResponse<OrderDto>>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GridResponse<OrderDto>>.Failure(new Error
            {
                Code = "GRID_ERROR",
                Message = ex.Message
            });
        }
    }

    private List<GridColumn> GetGridColumns()
    {
        return new List<GridColumn>
        {
            new() { PropertyName = "OrderNumber", DisplayName = "Order #" },
            new() { PropertyName = "OrderDate", DisplayName = "Date" },
            new() { PropertyName = "CustomerName", DisplayName = "Customer" },
            new() { PropertyName = "Total", DisplayName = "Total" },
            new() { PropertyName = "Status", DisplayName = "Status" }
        };
    }
}
```

---

## Complete Multi-Tenancy Example

### Step 1: Tenant Context Setup

```csharp
// Infrastructure/TenantMiddleware.cs
using SmartWorkz.Core.Shared.MultiTenancy;

namespace YourApp.Infrastructure;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Extract tenant from subdomain
        var host = context.Request.Host.Host;
        var tenant = host.Split('.')[0];

        if (tenant != "localhost" && tenant != "www")
        {
            tenantContext.TenantId = tenant;
            tenantContext.TenantName = tenant.ToUpper();
        }

        // Or from header
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
        {
            tenantContext.TenantId = tenantId.ToString();
        }

        // Or from claims (JWT)
        var claim = context.User?.FindFirst("tenant_id");
        if (claim != null)
        {
            tenantContext.TenantId = claim.Value;
        }

        await _next(context);
    }
}
```

### Step 2: Register in Program.cs

```csharp
// Program.cs
using SmartWorkz.Core.Shared.MultiTenancy;
using YourApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Register tenant context
builder.Services.AddScoped<ITenantContext, TenantContext>();

var app = builder.Build();

// Add tenant middleware
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseStaticFiles();
app.MapRazorPages();
app.Run();
```

### Step 3: Use Tenant Context in Service

```csharp
// Services/CompanyService.cs
using SmartWorkz.Core.Shared.MultiTenancy;

namespace YourApp.Application.Services;

public class CompanyService
{
    private readonly ITenantContext _tenantContext;
    private readonly List<Company> _allCompanies = new()
    {
        new Company { Id = 1, TenantId = "acme", Name = "ACME Corp", City = "New York" },
        new Company { Id = 2, TenantId = "globex", Name = "Globex Corp", City = "Los Angeles" },
    };

    public CompanyService(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public List<Company> GetCompaniesForTenant()
    {
        var tenantId = _tenantContext.TenantId;
        Console.WriteLine($"Fetching companies for tenant: {tenantId}");

        return _allCompanies
            .Where(c => c.TenantId == tenantId)
            .ToList();
    }

    public Company? GetCompanyDetails()
    {
        var tenantId = _tenantContext.TenantId;
        return _allCompanies.FirstOrDefault(c => c.TenantId == tenantId);
    }
}

public class Company
{
    public int Id { get; set; }
    public string TenantId { get; set; } = "";
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
}
```

### Step 4: Use in Page

```csharp
// Pages/Dashboard/Index.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using YourApp.Application.Services;

namespace YourApp.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly CompanyService _companyService;
    public Company? Company { get; set; }
    public List<Company> Companies { get; set; } = new();

    public IndexModel(CompanyService companyService)
    {
        _companyService = companyService;
    }

    public void OnGet()
    {
        Company = _companyService.GetCompanyDetails();
        Companies = _companyService.GetCompaniesForTenant();
    }
}
```

---

## Complete Feature Flags Example

### Step 1: Setup in Program.cs

```csharp
// Program.cs
using SmartWorkz.Core.Shared.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IFeatureFlagService, DefaultFeatureFlagService>();

// Seed feature flags
var flagService = new DefaultFeatureFlagService();
flagService.SetFeatureFlag("feature:advanced_search", false);
flagService.SetFeatureFlag("feature:bulk_operations", true);
flagService.SetFeatureFlag("feature:beta_ui", false);

builder.Services.AddSingleton(flagService);

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();
app.Run();
```

### Step 2: Use in Service

```csharp
// Services/SearchService.cs
using SmartWorkz.Core.Shared.Features;

namespace YourApp.Application.Services;

public class SearchService
{
    private readonly IFeatureFlagService _flagService;

    public SearchService(IFeatureFlagService flagService)
    {
        _flagService = flagService;
    }

    public List<string> Search(string query)
    {
        if (_flagService.IsFeatureEnabled("feature:advanced_search"))
        {
            Console.WriteLine("✓ Using ADVANCED search with ML ranking");
            return AdvancedSearchWithAI(query);
        }
        else
        {
            Console.WriteLine("✓ Using BASIC SQL search");
            return BasicSearchWithSQL(query);
        }
    }

    private List<string> AdvancedSearchWithAI(string query)
    {
        // ML-powered search
        return new() { "AI Result 1", "AI Result 2" };
    }

    private List<string> BasicSearchWithSQL(string query)
    {
        // Simple LIKE search
        return new() { "Result 1", "Result 2" };
    }

    public bool CanBulkExport()
    {
        return _flagService.IsFeatureEnabled("feature:bulk_operations");
    }
}
```

### Step 3: Use in Razor View

```html
<!-- Pages/Search/Index.cshtml -->
@page
@model SearchModel
@inject IFeatureFlagService FlagService

<div class="container mt-4">
    <h2>Search</h2>

    <form method="post" class="mb-4">
        <div class="input-group">
            <input type="text" class="form-control" asp-for="Query" placeholder="Search...">
            <button class="btn btn-primary" type="submit">Search</button>
        </div>
    </form>

    @if (FlagService.IsFeatureEnabled("feature:beta_ui"))
    {
        <div class="alert alert-info">
            You're using the BETA UI! (feature:beta_ui is ON)
        </div>
    }

    @if (FlagService.IsFeatureEnabled("feature:bulk_operations"))
    {
        <div class="btn-group mb-3">
            <button class="btn btn-sm btn-secondary">Export All</button>
            <button class="btn btn-sm btn-secondary">Bulk Edit</button>
        </div>
    }

    @if (Model.Results != null && Model.Results.Count > 0)
    {
        <ul class="list-group">
            @foreach (var result in Model.Results)
            {
                <li class="list-group-item">@result</li>
            }
        </ul>
    }
</div>
```

---

## Complete Event Bus Example

### Step 1: Define Events

```csharp
// Events/UserSignedUpEvent.cs
namespace YourApp.Domain.Events;

public class UserSignedUpEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime SignedUpAt { get; set; } = DateTime.UtcNow;
}

public class OrderPlacedEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}
```

### Step 2: Setup in Program.cs

```csharp
// Program.cs
using SmartWorkz.Core.Shared.Events;
using YourApp.Application.Services;
using YourApp.Domain.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Register event bus
var eventPublisher = new InMemoryEventPublisher();
builder.Services.AddSingleton<IEventPublisher>(eventPublisher);
builder.Services.AddSingleton<IEventSubscriber>(eventPublisher);

// Register services that subscribe to events
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<AuditLoggingService>();
builder.Services.AddScoped<AnalyticsService>();

var app = builder.Build();

// Subscribe to events
var subscriber = app.Services.GetRequiredService<IEventSubscriber>();
var emailService = app.Services.GetRequiredService<EmailNotificationService>();
var auditService = app.Services.GetRequiredService<AuditLoggingService>();
var analyticsService = app.Services.GetRequiredService<AnalyticsService>();

subscriber.Subscribe<UserSignedUpEvent>(emailService.OnUserSignedUpAsync);
subscriber.Subscribe<UserSignedUpEvent>(auditService.OnUserSignedUpAsync);
subscriber.Subscribe<UserSignedUpEvent>(analyticsService.OnUserSignedUpAsync);

subscriber.Subscribe<OrderPlacedEvent>(emailService.OnOrderPlacedAsync);
subscriber.Subscribe<OrderPlacedEvent>(auditService.OnOrderPlacedAsync);
subscriber.Subscribe<OrderPlacedEvent>(analyticsService.OnOrderPlacedAsync);

app.UseStaticFiles();
app.MapRazorPages();
app.Run();
```

### Step 3: Implement Event Subscribers

```csharp
// Services/EmailNotificationService.cs
using SmartWorkz.Core.Shared.Events;
using YourApp.Domain.Events;

namespace YourApp.Application.Services;

public class EmailNotificationService
{
    public async Task OnUserSignedUpAsync(UserSignedUpEvent @event)
    {
        Console.WriteLine($"📧 Sending welcome email to {@event.Email}");
        await Task.Delay(500); // Simulate email sending
        Console.WriteLine($"✓ Welcome email sent to {@event.Email}");
    }

    public async Task OnOrderPlacedAsync(OrderPlacedEvent @event)
    {
        Console.WriteLine($"📧 Sending order confirmation for order #{@event.OrderId}");
        await Task.Delay(500);
        Console.WriteLine($"✓ Order confirmation sent");
    }
}

// Services/AuditLoggingService.cs
using YourApp.Domain.Events;

namespace YourApp.Application.Services;

public class AuditLoggingService
{
    public async Task OnUserSignedUpAsync(UserSignedUpEvent @event)
    {
        Console.WriteLine($"📝 Audit: User signed up - ID:{@event.UserId}, Email:{@event.Email}");
        await Task.Delay(100);
    }

    public async Task OnOrderPlacedAsync(OrderPlacedEvent @event)
    {
        Console.WriteLine($"📝 Audit: Order placed - OrderID:{@event.OrderId}, Total:{@event.Total}");
        await Task.Delay(100);
    }
}

// Services/AnalyticsService.cs
using YourApp.Domain.Events;

namespace YourApp.Application.Services;

public class AnalyticsService
{
    public async Task OnUserSignedUpAsync(UserSignedUpEvent @event)
    {
        Console.WriteLine($"📊 Analytics: New signup from {@event.Email}");
        await Task.Delay(100);
    }

    public async Task OnOrderPlacedAsync(OrderPlacedEvent @event)
    {
        Console.WriteLine($"📊 Analytics: Order value ${@event.Total}");
        await Task.Delay(100);
    }
}
```

### Step 4: Publish Events from Service

```csharp
// Services/UserService.cs
using SmartWorkz.Core.Shared.Events;
using SmartWorkz.Core.Shared.Results;
using YourApp.Domain.Events;
using YourApp.Domain.Models;

namespace YourApp.Application.Services;

public class UserService
{
    private readonly IEventPublisher _eventPublisher;
    private readonly List<User> _users = new();

    public UserService(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<User>> SignUpUserAsync(string email, string name)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.Failure(new Error("INVALID_EMAIL", "Email required"));

        // Create user
        var user = new User
        {
            Id = _users.Count + 1,
            Email = email,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);
        Console.WriteLine($"✓ User created: {email}");

        // Publish event - automatically triggers all subscribers
        await _eventPublisher.PublishAsync(new UserSignedUpEvent
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name
        });

        Console.WriteLine($"✓ UserSignedUpEvent published");

        return Result<User>.Success(user);
    }
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
```

---

## Complete Validation Example

### Step 1: Create Validation Rules

```csharp
// Validation/EmailRule.cs
using SmartWorkz.Core.Shared.Validation;

namespace YourApp.Application.Validation;

public class EmailRule : IValidationRule
{
    public ValidationFailure? Validate(object? value)
    {
        if (value == null)
            return new ValidationFailure("Email is required");

        var email = value.ToString();
        if (string.IsNullOrWhiteSpace(email))
            return new ValidationFailure("Email cannot be empty");

        // Simple email validation
        if (!email.Contains("@") || !email.Contains("."))
            return new ValidationFailure("Invalid email format");

        return null; // Valid
    }
}

// Validation/PasswordRule.cs
using System.Text.RegularExpressions;
using SmartWorkz.Core.Shared.Validation;

namespace YourApp.Application.Validation;

public class PasswordRule : IValidationRule
{
    public ValidationFailure? Validate(object? value)
    {
        if (value == null)
            return new ValidationFailure("Password is required");

        var password = value.ToString();

        if (password.Length < 8)
            return new ValidationFailure("Password must be at least 8 characters");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return new ValidationFailure("Password must contain at least one uppercase letter");

        if (!Regex.IsMatch(password, @"[a-z]"))
            return new ValidationFailure("Password must contain at least one lowercase letter");

        if (!Regex.IsMatch(password, @"[0-9]"))
            return new ValidationFailure("Password must contain at least one number");

        if (!Regex.IsMatch(password, @"[!@#$%^&*]"))
            return new ValidationFailure("Password must contain at least one special character (!@#$%^&*)");

        return null; // Valid
    }
}

// Validation/LengthRule.cs
using SmartWorkz.Core.Shared.Validation;

namespace YourApp.Application.Validation;

public class LengthRule : IValidationRule
{
    private readonly int _minLength;
    private readonly int _maxLength;

    public LengthRule(int min = 0, int max = int.MaxValue)
    {
        _minLength = min;
        _maxLength = max;
    }

    public ValidationFailure? Validate(object? value)
    {
        if (value == null)
            return null;

        var text = value.ToString() ?? "";

        if (text.Length < _minLength)
            return new ValidationFailure($"Must be at least {_minLength} characters");

        if (text.Length > _maxLength)
            return new ValidationFailure($"Cannot exceed {_maxLength} characters");

        return null;
    }
}
```

### Step 2: Create Validator

```csharp
// Validators/RegisterValidator.cs
using SmartWorkz.Core.Shared.Validation;
using YourApp.Application.Validation;

namespace YourApp.Application.Validators;

public class RegisterValidator : ValidatorBase<RegisterRequest>
{
    public RegisterValidator()
    {
        AddRule("Email", new EmailRule())
            .AddRule("Password", new PasswordRule())
            .AddRule("Name", new LengthRule(min: 2, max: 100));
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Name { get; set; } = "";
}
```

### Step 3: Use in Controller

```csharp
// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core.Shared.Validation;
using YourApp.Application.Validators;

namespace YourApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly RegisterValidator _validator;

    public AccountController()
    {
        _validator = new RegisterValidator();
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        Console.WriteLine($"Register attempt: {request.Email}");

        // Validate
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid)
        {
            Console.WriteLine($"✗ Validation failed with {validationResult.Failures.Count} errors");
            return BadRequest(new
            {
                success = false,
                errors = validationResult.Failures
                    .Select(f => new { field = f.PropertyName, message = f.Message })
                    .ToList()
            });
        }

        Console.WriteLine($"✓ Validation passed");

        // Register user...
        return Ok(new { success = true, message = "Registration successful" });
    }
}
```

### Step 4: Test Validation

```
POST /api/account/register
{
  "email": "invalid-email",
  "password": "weak",
  "name": ""
}

Response:
{
  "success": false,
  "errors": [
    { "field": "Email", "message": "Invalid email format" },
    { "field": "Password", "message": "Password must be at least 8 characters" },
    { "field": "Name", "message": "Must be at least 2 characters" }
  ]
}
```

---

## Complete Repository Pattern Example

### Step 1: Define Interface

```csharp
// Repositories/IProductRepository.cs
using SmartWorkz.Core.Shared.Pagination;
using SmartWorkz.Core.Shared.Results;

namespace YourApp.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetAllAsync();
    Task<PagedList<Product>> GetPagedAsync(PagedQuery query);
    Task<Result<Product>> AddAsync(Product product);
    Task<Result<Product>> UpdateAsync(Product product);
    Task<Result<bool>> DeleteAsync(Product product);
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

### Step 2: Implement Repository

```csharp
// Repositories/ProductRepository.cs
using SmartWorkz.Core.Shared.Pagination;
using SmartWorkz.Core.Shared.Results;
using YourApp.Domain.Repositories;

namespace YourApp.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    // Mock database
    private readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop", Price = 999.99m, StockQuantity = 10 },
        new Product { Id = 2, Name = "Mouse", Price = 29.99m, StockQuantity = 50 },
        new Product { Id = 3, Name = "Keyboard", Price = 79.99m, StockQuantity = 30 },
        new Product { Id = 4, Name = "Monitor", Price = 299.99m, StockQuantity = 5 },
    };

    public async Task<Product?> GetByIdAsync(int id)
    {
        await Task.Delay(10); // Simulate DB call
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public async Task<List<Product>> GetAllAsync()
    {
        await Task.Delay(10);
        return _products.ToList();
    }

    public async Task<PagedList<Product>> GetPagedAsync(PagedQuery query)
    {
        await Task.Delay(10);

        var result = _products.AsQueryable();

        // Apply search
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            result = result.Where(p => p.Name.Contains(query.SearchTerm));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            result = query.SortBy.ToLower() switch
            {
                "name" => query.SortDescending
                    ? result.OrderByDescending(p => p.Name)
                    : result.OrderBy(p => p.Name),
                "price" => query.SortDescending
                    ? result.OrderByDescending(p => p.Price)
                    : result.OrderBy(p => p.Price),
                _ => result.OrderBy(p => p.Name)
            };
        }

        var totalCount = result.Count();
        var skip = (query.Page - 1) * query.PageSize;
        var items = result.Skip(skip).Take(query.PageSize).ToList();

        return new PagedList<Product>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<Result<Product>> AddAsync(Product product)
    {
        try
        {
            await Task.Delay(10);
            product.Id = _products.Max(p => p.Id) + 1;
            _products.Add(product);
            return Result<Product>.Success(product);
        }
        catch (Exception ex)
        {
            return Result<Product>.Failure(new Error("ADD_FAILED", ex.Message));
        }
    }

    public async Task<Result<Product>> UpdateAsync(Product product)
    {
        try
        {
            await Task.Delay(10);
            var existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing == null)
                return Result<Product>.Failure(new Error("NOT_FOUND", "Product not found"));

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.StockQuantity = product.StockQuantity;

            return Result<Product>.Success(existing);
        }
        catch (Exception ex)
        {
            return Result<Product>.Failure(new Error("UPDATE_FAILED", ex.Message));
        }
    }

    public async Task<Result<bool>> DeleteAsync(Product product)
    {
        try
        {
            await Task.Delay(10);
            var removed = _products.Remove(product);
            return Result<bool>.Success(removed);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new Error("DELETE_FAILED", ex.Message));
        }
    }
}
```

### Step 3: Use Repository

```csharp
// Services/ProductService.cs
using SmartWorkz.Core.Shared.Pagination;
using SmartWorkz.Core.Shared.Results;
using YourApp.Domain.Repositories;

namespace YourApp.Application.Services;

public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product?> GetProductAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<PagedList<Product>> SearchProductsAsync(string? searchTerm, int page = 1, int pageSize = 20)
    {
        var query = new PagedQuery(page, pageSize, searchTerm: searchTerm);
        return await _repository.GetPagedAsync(query);
    }

    public async Task<Result<Product>> CreateProductAsync(string name, decimal price, int stock)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            StockQuantity = stock
        };

        return await _repository.AddAsync(product);
    }
}
```

---

## Complete Integration Example

**Real-world scenario: User signs up → email sent → logged → analytics tracked**

### All Pieces Working Together

```csharp
// Program.cs - Complete Setup
using SmartWorkz.Core.Shared.Caching;
using SmartWorkz.Core.Shared.Events;
using SmartWorkz.Core.Shared.Features;
using SmartWorkz.Core.Shared.MultiTenancy;
using YourApp.Application.Services;
using YourApp.Domain.Repositories;
using YourApp.Infrastructure;
using YourApp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Core services
builder.Services.AddRazorPages();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

// 2. Feature flags
var flagService = new DefaultFeatureFlagService();
flagService.SetFeatureFlag("feature:email_notifications", true);
flagService.SetFeatureFlag("feature:analytics", true);
builder.Services.AddSingleton(flagService);

// 3. Event bus
var eventPublisher = new InMemoryEventPublisher();
builder.Services.AddSingleton<IEventPublisher>(eventPublisher);
builder.Services.AddSingleton<IEventSubscriber>(eventPublisher);

// 4. Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// 5. Application services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<AuditLoggingService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CompanyService>();

var app = builder.Build();

// Subscribe to events
var subscriber = app.Services.GetRequiredService<IEventSubscriber>();
var emailService = app.Services.GetRequiredService<EmailNotificationService>();
var auditService = app.Services.GetRequiredService<AuditLoggingService>();
var analyticsService = app.Services.GetRequiredService<AnalyticsService>();

subscriber.Subscribe<UserSignedUpEvent>(emailService.OnUserSignedUpAsync);
subscriber.Subscribe<UserSignedUpEvent>(auditService.OnUserSignedUpAsync);
subscriber.Subscribe<UserSignedUpEvent>(analyticsService.OnUserSignedUpAsync);

// Middlewares
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseStaticFiles();
app.MapRazorPages();

app.Run();
```

### Test Scenario

```csharp
// Example: User registers from tenant "acme"
// Automatically triggers:
// 1. Result<T> validation
// 2. Caching of user profile
// 3. Multi-tenant isolation (data scoped to "acme")
// 4. Feature flag check (email notifications enabled?)
// 5. Event published → triggers emails, audit logs, analytics

var result = await userService.SignUpUserAsync("john@acme.com", "John Doe");
// Output:
// ✓ User created: john@acme.com
// ✓ UserSignedUpEvent published
// 📧 Sending welcome email to john@acme.com
// 📝 Audit: User signed up - ID:1, Email:john@acme.com
// 📊 Analytics: New signup from john@acme.com
// ✓ Welcome email sent to john@acme.com
```

---

## Summary

| System | Key File | Copy From |
|--------|----------|-----------|
| **Result<T>** | `ProductService.cs` | ✓ Has success/failure examples |
| **Caching** | `UserService.cs` | ✓ Cache hits/misses shown |
| **Grid** | `OrdersController.cs` | ✓ API endpoint with filtering |
| **Multi-Tenancy** | `CompanyService.cs` | ✓ Tenant isolation implemented |
| **Feature Flags** | `SearchService.cs` | ✓ Conditional logic shown |
| **Events** | `UserService.cs` | ✓ Publishing events |
| **Validation** | `RegisterValidator.cs` | ✓ Multiple validation rules |
| **Repository** | `ProductRepository.cs` | ✓ CRUD with paging |
| **Integration** | `Program.cs` | ✓ Everything wired together |

**Each example is self-contained and runnable!** Junior developers can copy any service and use it immediately in their projects.
