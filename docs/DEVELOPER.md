# Developer Guide

**Complete reference for building with SmartWorkz.Core — from getting started to advanced patterns**

---

## 📖 Using This Guide

- **🟢 BEGINNER** — Copy-paste examples for learning (Sections 1-5)
- **🔵 ADVANCED** — Architecture, patterns, complex scenarios (Sections 6+)
- **⚙️ Reference** — Complete API reference for every system

---

## Table of Contents

### Getting Started (BEGINNER)
1. [Result<T> Pattern](#result-pattern)
2. [Cache Service](#cache-service)
3. [Grid Component](#grid-component)
4. [Multi-Tenancy](#multi-tenancy)
5. [Feature Flags](#feature-flags)

### Advanced Patterns (ADVANCED)
6. [Architecture Overview](#architecture-overview)
7. [Core Patterns Deep Dive](#core-patterns)
8. [Event Bus](#event-bus)
9. [Pagination](#pagination)
10. [Security & Validation](#security--validation)
11. [Logging & Diagnostics](#logging--diagnostics)
12. [Communication Services](#communication-services)
13. [Resilience Patterns](#resilience-patterns)
14. [Domain-Driven Design](#domain-driven-design)
15. [Integration Patterns](#integration-patterns)

---

# 🟢 GETTING STARTED SECTION

## Result<T> Pattern

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

        if (_products.Any(p => p.Sku == sku))
            return Result<Product>.Failure(new Error
            {
                Code = "DUPLICATE_SKU",
                Message = $"A product with SKU '{sku}' already exists"
            });

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
        var result = await _productService.CreateProductAsync(
            ProductName,
            ProductPrice,
            ProductSku
        );

        if (result.IsSuccess)
        {
            SuccessMessage = $"Product '{ProductName}' created successfully!";
            return RedirectToPage("./Index");
        }

        ErrorMessage = result.Error.Message;
        return Page();
    }
}
```

---

## Cache Service

### Step 1: Setup in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();
app.Run();
```

### Step 2: Use in Service

```csharp
public class UserService
{
    private readonly ICacheService _cacheService;
    private readonly List<User> _users = new();

    public UserService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        var cacheKey = $"user:{userId}";
        var cachedResult = await _cacheService.GetAsync<User>(cacheKey);

        if (cachedResult.IsSuccess && cachedResult.Value != null)
        {
            Console.WriteLine($"✓ Cache hit for {cacheKey}");
            return cachedResult.Value;
        }

        var user = _users.FirstOrDefault(u => u.Id == userId);

        if (user != null)
        {
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
        await _cacheService.RemoveAsync($"user:{userId}");
        return user;
    }
}
```

---

## [Cache] Attribute (Phase 1)

For simple endpoints with the same data for all users:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    [Cache(Seconds = 60)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _service.GetProductAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }

    [Cache(Seconds = 300, Key = "AllProducts")]
    [HttpGet]
    public async Task<IActionResult> ListProducts()
    {
        var products = await _service.ListProductsAsync();
        return Ok(products);
    }

    [Cache(Seconds = 600, SlidingExpiration = true)]
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopular()
    {
        return Ok(await _service.GetPopularAsync());
    }
}
```

**Configuration Options:**

| Property | Type | Default | Meaning |
|----------|------|---------|---------|
| `Seconds` | int | 60 | Cache duration in seconds |
| `Key` | string? | null | Custom cache key (null = use request path) |
| `SlidingExpiration` | bool | false | Reset expiry on each hit |

---

## Grid Component

### Step 1: Create Data Model

```csharp
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

namespace YourApp.Pages.Orders;

public class IndexModel : PageModel
{
    public string ApiEndpoint { get; set; } = "/api/orders/grid";
    public List<GridColumn> Columns { get; set; } = new();

    public void OnGet()
    {
        Columns = new List<GridColumn>
        {
            new GridColumn
            {
                PropertyName = "OrderNumber",
                DisplayName = "Order #",
                IsSortable = true,
                IsFilterable = true
            },
            new GridColumn
            {
                PropertyName = "CustomerName",
                DisplayName = "Customer",
                IsSortable = true
            },
            new GridColumn
            {
                PropertyName = "Total",
                DisplayName = "Amount",
                IsSortable = true,
                Format = "{0:C2}"
            }
        };
    }
}
```

### Step 3: API Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    [HttpPost("grid")]
    public async Task<IActionResult> GetGridData([FromBody] GridRequest request)
    {
        var items = await _service.GetOrdersAsync();
        var filtered = ApplyFilters(items, request);
        var sorted = ApplySorting(filtered, request);
        var paged = new PagedList<OrderDto>(sorted.ToList(), request.Page, request.PageSize, sorted.Count());
        
        return Ok(new GridResponse<OrderDto> { Data = paged });
    }
}
```

---

## Multi-Tenancy

### Step 1: Register Services

```csharp
// Program.cs
services.AddScoped<ITenantContext, TenantContext>();
```

### Step 2: Extract Tenant from Request

```csharp
app.Use(async (context, next) =>
{
    var tenant = context.RequestServices.GetRequiredService<ITenantContext>();
    var tenantId = context.Request.Headers["X-Tenant-ID"];
    tenant.TenantId = string.IsNullOrEmpty(tenantId) ? 1 : int.Parse(tenantId);
    await next();
});
```

### Step 3: Use in Service

```csharp
public class ProductRepository
{
    private readonly IDbConnection _connection;
    private readonly ITenantContext _tenantContext;

    public async Task<List<Product>> GetAllAsync()
    {
        var tenantId = _tenantContext.TenantId;
        var sql = "SELECT * FROM Products WHERE TenantId = @TenantId";
        return (await _connection.QueryAsync<Product>(sql, new { TenantId = tenantId })).ToList();
    }
}
```

---

## Feature Flags

```csharp
// Program.cs
services.AddSingleton<IFeatureFlagService, DefaultFeatureFlagService>();

// In code
if (_flagService.IsFeatureEnabled("feature:beta_ui"))
{
    // Use new feature
}

// In Razor
@if (FlagService.IsFeatureEnabled("feature:beta_ui"))
{
    <NewUI />
}
```

---

# 🔵 ADVANCED PATTERNS SECTION

---

## Architecture Overview

SmartWorkz.Core is a **layered, modular infrastructure**:

```
┌────────────────────────────────────┐
│  Application Layer                 │
├────────────────────────────────────┤
│  SmartWorkz.Core.Web               │
│  ├─ Grid Components                │
│  ├─ Services                       │
│  └─ Web Infrastructure             │
├────────────────────────────────────┤
│  SmartWorkz.Core.Shared            │
│  ├─ Models & Requests              │
│  ├─ Services & Caching             │
│  ├─ Security & Encryption          │
│  ├─ Patterns (Result<T>, etc)      │
│  └─ Helpers & Extensions           │
├────────────────────────────────────┤
│  SmartWorkz.Core                   │
│  ├─ Base Classes & Abstractions    │
│  └─ Domain Models                  │
├────────────────────────────────────┤
│  SmartWorkz.Core.External          │
│  └─ Third-party Integrations       │
└────────────────────────────────────┘
```

**Key Design Principles:**
- ✅ Layered Architecture — Clear separation of concerns
- ✅ Multi-Tenancy First — Built-in tenant isolation
- ✅ Result Pattern — Consistent error handling
- ✅ Feature Flags — Runtime feature control
- ✅ Event-Driven — Async inter-service communication
- ✅ Resilience — CircuitBreaker, RateLimiter, Retry
- ✅ Domain-Driven Design — Aggregates, Domain Events, Value Objects

---

## Core Patterns

### 1. Result<T> Pattern (Advanced)

**Exception Hierarchy for exceptional conditions:**

```csharp
public class ApplicationException : Exception { }
public class BusinessException : ApplicationException { }
public class NotFoundException : ApplicationException { }
public class UnauthorizedException : ApplicationException { }
public class ValidationException : ApplicationException { }
```

**Chaining results:**

```csharp
var result = await _userService.GetUserAsync(userId)
    .Map(user => new UserDto { Name = user.Name })
    .Bind(dto => ValidateUserDto(dto));
```

---

## Data & Caching (Advanced)

### DbProviderFactory — Type-Safe Enum Overload

```csharp
// Type-safe (preferred)
var provider = DbProviderFactory.GetProvider(DatabaseProvider.SqlServer);

// Supported providers
public enum DatabaseProvider
{
    SqlServer,
    MySql,
    PostgreSql,
    Sqlite
}
```

### DbExtensions — Simplified Aliases

```csharp
// ADO helpers
var users = await provider.QueryAsync<User>("SELECT * FROM Users");
var count = await provider.ScalarAsync<int>("SELECT COUNT(*) FROM Users");
var affected = await provider.NonQueryAsync("UPDATE Users SET Status = @Status", data);

// Dapper helpers
var user = await provider.QuerySingleAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = 1 });
var rows = await provider.ExecuteAsync("DELETE FROM Users WHERE Inactive = 1");
```

**Before vs. After:**

| Verbose | Alias |
|---------|-------|
| `AdoHelper.ExecuteQueryAsync<T>` | `provider.QueryAsync<T>` |
| `AdoHelper.ExecuteScalarAsync<T>` | `provider.ScalarAsync<T>` |
| `AdoHelper.ExecuteNonQueryAsync` | `provider.NonQueryAsync` |
| `DapperHelper.DapperQueryAsync<T>` | `provider.QueryAsync<T>` |
| `DapperHelper.DapperQuerySingleAsync<T>` | `provider.QuerySingleAsync<T>` |
| `DapperHelper.DapperExecuteAsync` | `provider.ExecuteAsync` |

---

## Template Engine (Phase 1)

**File-based templates with placeholders for email/SMS:**

```csharp
// Setup
services.AddScoped<ITemplateEngine, TemplateEngine>();

// Simple rendering
var result = _templateEngine.Render(
    "Hello {Name}, your order {OrderNumber} is confirmed.",
    new { Name = "Alice", OrderNumber = "ORD-123" }
);

// File-based rendering
var result = await _templateEngine.RenderFileAsync(
    "~/Templates/Emails/welcome.html",
    new { UserName = "Bob", ActivationLink = "https://..." }
);

// Translation keys
var result = _templateEngine.Render(
    "{{GREETING}} {{OFFER}}",
    new Dictionary<string, string>
    {
        { "GREETING", "Welcome" },
        { "OFFER", "Save 10%" }
    }
);

// Batch rendering
var rendered = await _templateEngine.RenderDirectoryAsync(
    "~/Templates/Emails",
    new { CompanyName = "ACME Corp" }
);
```

---

## Event Bus

```csharp
// Define event
public class UserCreatedEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
}

// Publish
await _eventPublisher.PublishAsync(new UserCreatedEvent { UserId = user.Id, Email = user.Email });

// Subscribe
_eventSubscriber.Subscribe<UserCreatedEvent>(async @event =>
{
    await _emailSender.SendWelcomeEmailAsync(@event.Email);
});
```

---

## Pagination

```csharp
// Request
var query = new PagedQuery(
    page: 1,
    pageSize: 20,
    sortBy: "Name",
    sortDescending: false);

// Get paged data
var paged = await _repo.GetPagedAsync(query);

// Response properties
var items = paged.Items;
var pageNumber = paged.PageNumber;
var totalPages = paged.TotalPages;
var hasNext = paged.HasNextPage;
var hasPrev = paged.HasPreviousPage;
```

---

## Security & Validation

### Password Security

```csharp
var hash = PasswordHelper.HashPassword("MyPassword123!");
var valid = PasswordHelper.VerifyPassword("MyPassword123!", hash);
var strong = PasswordHelper.IsStrongPassword("MyPassword123!");
```

### Encryption

```csharp
var encrypted = _encryptionHelper.Encrypt("sensitive-data");
var decrypted = _encryptionHelper.Decrypt(encrypted);
```

### Validation

```csharp
public class CreateUserValidator : ValidatorBase<CreateUserRequest>
{
    public CreateUserValidator()
    {
        AddRule("Email", new EmailRule())
            .AddRule("Password", new PasswordRule());
    }
}

var result = validator.Validate(request);
if (!result.IsValid)
    return BadRequest(result.Failures);
```

---

## Logging & Diagnostics

```csharp
// Audit log
await _auditLogger.LogAsync(new AuditRecord
{
    UserId = user.Id.ToString(),
    Action = "Update",
    EntityType = "Product",
    EntityId = product.Id.ToString(),
    OldValues = JsonSerializer.Serialize(oldProduct),
    NewValues = JsonSerializer.Serialize(newProduct),
    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
    CorrelationId = context.TraceIdentifier
});

// Health check
var health = await _healthCheck.CheckHealthAsync();
```

---

## Communication Services

### Email with Template Engine

```csharp
public class EmailConfirmationService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly IEmailSender _emailSender;

    public async Task SendConfirmationAsync(User user, string confirmationLink)
    {
        var result = await _templateEngine.RenderFileAsync(
            "~/Templates/Emails/email-confirmation.html",
            new { UserName = user.FullName, ConfirmationLink = confirmationLink }
        );

        if (result.IsFailure)
            throw new InvalidOperationException($"Template error: {result.Error.Message}");

        await _emailSender.SendAsync(user.Email, "Confirm Your Email", result.Value);
    }
}
```

### SMS

```csharp
await _smsService.SendAsync(
    phoneNumber: "+1234567890",
    message: "Your code is 123456"
);
```

---

## Resilience Patterns

### Circuit Breaker

```csharp
var result = await _circuitBreaker.ExecuteAsync(async () =>
{
    return await externalApi.GetDataAsync();
});
```

### Rate Limiter

```csharp
if (!_rateLimiter.IsAllowed(userId))
    return StatusCode(429, "Rate limit exceeded");
```

### Retry with HttpStatusCode Enum

```csharp
new RetryPolicy
{
    MaxAttempts = 3,
    Strategy = RetryStrategy.Exponential,
    RetryableStatusCodes = [
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable
    ]
}
```

---

## Domain-Driven Design

### Aggregate Root

```csharp
public class Order : AggregateRoot
{
    public string OrderNumber { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();
    public OrderStatus Status { get; private set; }

    public void AddItem(OrderItem item)
    {
        Items.Add(item);
        RaiseDomainEvent(new OrderItemAddedEvent(Id, item.ProductId));
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id));
    }
}
```

### Domain Event

```csharp
public class OrderCreatedEvent : DomainEvent
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public decimal Total { get; set; }
}
```

### Value Object

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new InvalidOperationException("Amount cannot be negative");
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

---

## Integration Patterns

### Repository Pattern

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<PagedList<T>> GetPagedAsync(PagedQuery query);
    Task<Result<T>> AddAsync(T entity);
    Task<Result<T>> UpdateAsync(T entity);
    Task<Result<bool>> DeleteAsync(T entity);
}
```

### Unit of Work Pattern

```csharp
var transaction = await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Products.AddAsync(product);
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitAsync();
}
catch
{
    await _unitOfWork.RollbackAsync();
}
```

---

## See Also

- **[Quick Reference](QUICK_REFERENCE.md)** — One-page cheat-sheet
- **[Data Access](DATA_ACCESS.md)** — Database patterns
- **[HTTP Client](HTTP_CLIENT.md)** — REST & WebSocket
- **[Utilities](UTILITIES.md)** — Extensions & helpers
- **[Templates](TEMPLATES.md)** — Template engine API
- **[Security](SECURITY.md)** — Authentication & encryption
