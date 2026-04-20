# SmartWorkz.Core Developer Guide

**Complete reference for all SmartWorkz.Core systems and components**

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Core Patterns](#core-patterns)
3. [Data & Caching](#data--caching)
   - 3.1 [In-Memory Cache Service](#1-in-memory-cache-service)
   - 3.2 [Cache Attribute (Phase 1)](#2-cache-attribute--phase-1)
   - 3.3 [DbProviderFactory & DbExtensions (Phase 1)](#3-dbproviderfactory--dbextensions--phase-1)
4. [Grid Component System](#grid-component-system)
5. [Multi-Tenancy](#multi-tenancy)
6. [Feature Flags](#feature-flags)
7. [Event Bus](#event-bus)
8. [Pagination](#pagination)
9. [Security & Validation](#security--validation)
10. [Logging & Diagnostics](#logging--diagnostics)
11. [Communication Services](#communication-services)
    - 11.1 [Template Engine (Phase 1)](#3-template-engine--phase-1)
12. [Resilience Patterns](#resilience-patterns)
13. [HTTP Client (Phase 1)](#http-client--phase-1)
14. [Helper Libraries](#helper-libraries)
15. [Domain-Driven Design](#domain-driven-design)
16. [Integration Patterns](#integration-patterns)

---

## Architecture Overview

SmartWorkz.Core is a **layered, modular infrastructure** for .NET Core applications:

```
┌─────────────────────────────────────────────────────────────┐
│  Application Layer (User-Facing Controllers/Pages)          │
├─────────────────────────────────────────────────────────────┤
│  SmartWorkz.Core.Web                                        │
│  ├─ Grid Components (Razor Components, TagHelpers)          │
│  ├─ Services (GridDataProvider, GridExportService)          │
│  └─ Web-specific infrastructure                             │
├─────────────────────────────────────────────────────────────┤
│  SmartWorkz.Core.Shared                                     │
│  ├─ Models (GridRequest, GridResponse, PagedList)           │
│  ├─ Services (Caching, Events, Feature Flags)               │
│  ├─ Security (Encryption, JWT, Password Helpers)            │
│  ├─ Patterns (Result<T>, Validation, Specifications)        │
│  └─ Helpers (String, DateTime, JSON, Collections)           │
├─────────────────────────────────────────────────────────────┤
│  SmartWorkz.Core                                            │
│  ├─ Base Classes (Entities, AggregateRoot, ValueObjects)    │
│  ├─ Abstractions (IRepository, IService, IUnitOfWork)       │
│  ├─ Enums (EntityStatus, ResultStatus, SortDirection)       │
│  └─ Constants                                               │
├─────────────────────────────────────────────────────────────┤
│  SmartWorkz.Core.External                                   │
│  └─ External integrations (third-party APIs)                │
└─────────────────────────────────────────────────────────────┘
```

**Key Design Principles:**
- ✅ **Layered Architecture**: Clear separation of concerns
- ✅ **Platform Independence**: Core.Shared models used across Web, MAUI, Desktop
- ✅ **Result Pattern**: Consistent error handling without exceptions
- ✅ **Multi-Tenancy First**: Built-in tenant isolation
- ✅ **Feature Flags**: Runtime feature control
- ✅ **Event-Driven**: Event bus for inter-service communication
- ✅ **Resilience**: CircuitBreaker, RateLimiter, Retry policies
- ✅ **Domain-Driven Design**: Aggregates, Domain Events, Value Objects

---

## Core Patterns

### 1. Result<T> Pattern

**Purpose**: Unified error handling without exceptions for expected failures.

```csharp
// In SmartWorkz.Core.Shared/Results/Result.cs
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public Error? Error { get; set; }
    public ResultStatus Status { get; set; }
}

public class Error
{
    public string Code { get; set; }
    public string Message { get; set; }
}
```

**Usage Examples**:

```csharp
// Service method returns Result<T>
public async Task<Result<User>> GetUserAsync(int userId)
{
    var user = await _userRepository.GetByIdAsync(userId);
    
    if (user == null)
        return Result<User>.Failure(new Error 
        { 
            Code = "USER_NOT_FOUND",
            Message = $"User {userId} not found"
        });
    
    return Result<User>.Success(user);
}

// Caller handles result without try-catch
var result = await _userService.GetUserAsync(1);

if (result.IsSuccess)
{
    // Use result.Value
    Console.WriteLine($"User: {result.Value.Name}");
}
else
{
    // Handle error
    Console.WriteLine($"Error: {result.Error.Message}");
}

// Chaining results
var getUserResult = await _userService.GetUserAsync(userId);
var mappedResult = getUserResult
    .Map(user => new UserDto { Name = user.Name })
    .Bind(dto => ValidateUserDto(dto));
```

**Benefits:**
- No exception handling overhead
- Explicit error contracts
- Composable via extension methods (Map, Bind, Match)
- Works across async boundaries

---

### 2. Exception Hierarchy

For **exceptional** conditions (bugs, system failures), use typed exceptions:

```csharp
// All inherit from ApplicationException
public class ApplicationException : Exception { }
public class BusinessException : ApplicationException { }
public class NotFoundException : ApplicationException { }
public class UnauthorizedException : ApplicationException { }
public class ValidationException : ApplicationException { }
```

**When to use each:**

| Exception | When | Example |
|-----------|------|---------|
| `ApplicationException` | System-level failures | Database connection lost |
| `BusinessException` | Rule violations | Cannot delete last admin user |
| `NotFoundException` | Resource missing (unexpected) | Database corrupted, record vanished |
| `UnauthorizedException` | Access denied (unexpected) | Token validation failed |
| `ValidationException` | Input validation | Malformed request body |

**Usage**:

```csharp
// Use Result<T> for expected failures
public async Task<Result<bool>> DeleteUserAsync(int userId)
{
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
        return Result<bool>.Failure(new Error("USER_NOT_FOUND", "User not found"));
    
    if (!user.CanBeDeleted())
        return Result<bool>.Failure(new Error("USER_PROTECTED", "User cannot be deleted"));
    
    // Use exceptions for unexpected failures
    try
    {
        await _userRepository.DeleteAsync(user);
        return Result<bool>.Success(true);
    }
    catch (DbUpdateException ex)
    {
        throw new ApplicationException("Failed to delete user", ex);
    }
}
```

---

## Data & Caching

### 1. In-Memory Cache Service

**Purpose**: L1 cache for frequently accessed data with tenant isolation.

```csharp
// Service definition
public interface ICacheService
{
    Task<Result<T>> GetAsync<T>(string key);
    Task<Result<T>> SetAsync<T>(string key, T value, CacheOptions? options = null);
    Task<Result<bool>> RemoveAsync(string key);
    Task<Result<bool>> RemoveByPrefixAsync(string keyPrefix);
    Task<Result<bool>> ClearAsync();
}

// Configuration
public class CacheOptions
{
    public CacheStrategy Strategy { get; set; } = CacheStrategy.Absolute;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(10);
    public bool SlidingExpiration { get; set; } = false;
}

public enum CacheStrategy
{
    Absolute,      // Fixed duration
    Sliding,       // Reset on each access
    SlidingAbsolute // Max duration with sliding within window
}
```

**Dependency Injection Setup**:

```csharp
// Program.cs
services.AddScoped<ICacheService, MemoryCacheService>();

// Or with options
services.Configure<CacheOptions>(options =>
{
    options.Duration = TimeSpan.FromMinutes(30);
    options.Strategy = CacheStrategy.Sliding;
});
```

**Usage Examples**:

```csharp
private readonly ICacheService _cacheService;

// Get with fallback
public async Task<User> GetUserAsync(int userId)
{
    // Check cache first
    var cacheKey = $"user:{userId}";
    var cachedResult = await _cacheService.GetAsync<User>(cacheKey);
    
    if (cachedResult.IsSuccess && cachedResult.Value != null)
        return cachedResult.Value;
    
    // Cache miss - fetch from database
    var user = await _userRepository.GetByIdAsync(userId);
    
    // Cache for 10 minutes
    var cacheOptions = new CacheOptions
    {
        Duration = TimeSpan.FromMinutes(10),
        Strategy = CacheStrategy.Sliding
    };
    
    await _cacheService.SetAsync(cacheKey, user, cacheOptions);
    return user;
}

// Invalidate on update
public async Task<Result<User>> UpdateUserAsync(User user)
{
    var result = await _userRepository.UpdateAsync(user);
    
    if (result.IsSuccess)
    {
        // Clear cache
        await _cacheService.RemoveAsync($"user:{user.Id}");
    }
    
    return result;
}

// Bulk invalidation
public async Task<Result<bool>> ClearUserCacheAsync(int tenantId)
{
    return await _cacheService.RemoveByPrefixAsync($"tenant:{tenantId}:user:");
}
```

**Tenant-Aware Caching**:

```csharp
public class TenantAwareCacheService
{
    private readonly ICacheService _cache;
    private readonly ITenantContext _tenantContext;
    
    public async Task<Result<T>> GetAsync<T>(string key)
    {
        var tenantId = _tenantContext.TenantId;
        var tenantedKey = $"tenant:{tenantId}:{key}";
        return await _cache.GetAsync<T>(tenantedKey);
    }
    
    public async Task<Result<T>> SetAsync<T>(string key, T value, CacheOptions? options = null)
    {
        var tenantId = _tenantContext.TenantId;
        var tenantedKey = $"tenant:{tenantId}:{key}";
        return await _cache.SetAsync(tenantedKey, value, options);
    }
}
```

---

### 2. Cache Attribute (Phase 1)

**Purpose**: Automatic HTTP action result caching without boilerplate `ICacheService` code.

**When to use:**
- Simple endpoints returning the same data for all users
- Response rarely changes (> 5 minutes)
- Want automatic expiration without manual cache keys

**Configuration:**

```csharp
[Cache(Seconds = 60)]              // Default: 60 second absolute expiration
[Cache(Seconds = 300, SlidingExpiration = true)]  // Sliding: reset on each hit
[Cache(Seconds = 600, Key = "AllProducts")]       // Custom key
```

**Properties:**
- `Seconds` (int) — Cache duration in seconds (default: 60)
- `Key` (string?) — Custom cache key; if null, uses request path
- `SlidingExpiration` (bool) — Reset expiry on each hit (default: false)

**Example:**

```csharp
[ApiController]
public class ProductsController : ControllerBase
{
    // Cache GET /api/products/5 for 60 seconds
    [Cache(Seconds = 60)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _service.GetProductAsync(id);
        return Ok(product);
    }

    // Cache with custom key and sliding expiration
    [Cache(Seconds = 300, Key = "AllProducts", SlidingExpiration = true)]
    [HttpGet]
    public async Task<IActionResult> ListProducts()
    {
        var products = await _service.ListAsync();
        return Ok(products);
    }
}
```

**vs. ICacheService:**
- `[Cache]` — Simple decorator, no boilerplate, per-instance (not distributed)
- `ICacheService` — Programmatic control, distributed cache, complex invalidation

Use `[Cache]` for read-only endpoints. Use `ICacheService` when you need explicit control or distributed caching.

---

### 3. DbProviderFactory & DbExtensions (Phase 1)

**Purpose**: Type-safe database provider selection and simplified data access aliases.

**DbProviderFactory — Enum Overload:**

```csharp
// Type-safe provider lookup (instead of string)
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

**DbExtensions — Simplified Aliases:**

Extension methods on `IDbProvider` for shorter, more intuitive names:

```csharp
// ADO.NET helpers
var users = await provider.QueryAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = 1 }
);

var count = await provider.ScalarAsync<int>(
    "SELECT COUNT(*) FROM Users"
);

var affected = await provider.NonQueryAsync(
    "UPDATE Users SET Status = @Status WHERE Id = @Id",
    new { Status = "Active", Id = 1 }
);

// Dapper helpers  
var user = await provider.QuerySingleAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = 1 }
);
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

## Grid Component System

### Complete Usage Guide

**Purpose**: Flexible, reusable grid component for displaying tabular data with sorting, filtering, paging, selection, and export.

### Setup

**1. Register Services** (Program.cs):

```csharp
services.AddScoped<IGridDataProvider, GridDataProvider>();
services.AddScoped<GridExportService>();
services.AddScoped<GridStateManager>();
```

**2. Define Columns**:

```csharp
public class ProductGrid
{
    public static List<GridColumn> GetColumns()
    {
        return new List<GridColumn>
        {
            new GridColumn
            {
                PropertyName = "Id",
                DisplayName = "ID",
                IsSortable = true,
                IsFilterable = false,
                Width = "80px",
                Order = 1
            },
            new GridColumn
            {
                PropertyName = "Name",
                DisplayName = "Product Name",
                IsSortable = true,
                IsFilterable = true,
                FilterType = "text",
                Order = 2
            },
            new GridColumn
            {
                PropertyName = "Price",
                DisplayName = "Price",
                IsSortable = true,
                IsFilterable = true,
                FilterType = "range",
                Order = 3
            },
            new GridColumn
            {
                PropertyName = "Category",
                DisplayName = "Category",
                IsSortable = true,
                IsFilterable = true,
                FilterType = "dropdown",
                Order = 4
            }
        };
    }
}
```

### API-Based Grid (Server-Side)

**Razor Page / View**:

```html
@page "/products"
@model ProductsModel

<div class="container-fluid">
    <h2>Products</h2>
    
    <grid asp-data-source="@Model.ApiEndpoint"
          asp-columns="@Model.Columns"
          asp-page-size="25"
          asp-allow-selection="true"
          asp-allow-export="true"
          asp-allow-column-visibility-toggle="true">
    </grid>
</div>

@section Scripts {
    <script src="_framework/aspnetcore-blazor.web.js"></script>
}
```

**Code-Behind**:

```csharp
public class ProductsModel : PageModel
{
    public string ApiEndpoint { get; set; } = "/api/products/grid";
    public List<GridColumn> Columns { get; set; } = ProductGrid.GetColumns();
}
```

**API Endpoint** (Controller):

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    
    [HttpPost("grid")]
    public async Task<Result<GridResponse<ProductDto>>> GetGridData(
        [FromBody] GridRequest request)
    {
        try
        {
            // Get total count
            var totalCount = await _productService.GetTotalCountAsync();
            
            // Apply sorting
            var query = _productService.GetProductsQuery();
            
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                query = request.SortDescending
                    ? query.OrderByDescending(p => EF.Property<object>(p, request.SortBy))
                    : query.OrderBy(p => EF.Property<object>(p, request.SortBy));
            }
            
            // Apply filters
            if (request.Filters != null && request.Filters.Count > 0)
            {
                if (request.Filters.TryGetValue("Name", out var nameFilter))
                    query = query.Where(p => p.Name.Contains(nameFilter.ToString()));
                
                if (request.Filters.TryGetValue("Category", out var categoryFilter))
                    query = query.Where(p => p.Category == categoryFilter.ToString());
                
                if (request.Filters.TryGetValue("PriceMin", out var priceMin) &&
                    decimal.TryParse(priceMin.ToString(), out var minVal))
                    query = query.Where(p => p.Price >= minVal);
                
                if (request.Filters.TryGetValue("PriceMax", out var priceMax) &&
                    decimal.TryParse(priceMax.ToString(), out var maxVal))
                    query = query.Where(p => p.Price <= maxVal);
            }
            
            // Apply paging
            var skip = (request.Page - 1) * request.PageSize;
            var pagedData = await query
                .Skip(skip)
                .Take(request.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Category = p.Category
                })
                .ToListAsync();
            
            var pagedList = new PagedList<ProductDto>(
                items: pagedData,
                pageNumber: request.Page,
                pageSize: request.PageSize,
                totalCount: totalCount);
            
            var response = new GridResponse<ProductDto>
            {
                Data = pagedList,
                Columns = ProductGrid.GetColumns()
            };
            
            return Result<GridResponse<ProductDto>>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GridResponse<ProductDto>>.Failure(
                new Error("GRID_ERROR", $"Failed to fetch grid data: {ex.Message}"));
        }
    }
}
```

### In-Memory Grid (Client-Side)

**Usage**:

```csharp
@page "/employees"
@using SmartWorkz.Core.Shared.Grid

@inject IGridDataProvider GridDataProvider

<GridComponent TItem="EmployeeDto"
               Columns="@_columns"
               DataSource="@_employees"
               IsServerSide="false"
               AllowRowSelection="true"
               AllowExport="true"
               PageSize="10">
</GridComponent>

@code {
    private List<EmployeeDto> _employees = new();
    private List<GridColumn> _columns = new();
    
    protected override async Task OnInitializedAsync()
    {
        // Load all employees
        _employees = await _employeeService.GetAllAsync();
        
        // Define columns
        _columns = new List<GridColumn>
        {
            new GridColumn { PropertyName = "Id", DisplayName = "ID", Order = 1 },
            new GridColumn { PropertyName = "FirstName", DisplayName = "First Name", Order = 2 },
            new GridColumn { PropertyName = "LastName", DisplayName = "Last Name", Order = 3 },
            new GridColumn { PropertyName = "Department", DisplayName = "Department", Order = 4 },
            new GridColumn { PropertyName = "Salary", DisplayName = "Salary", Order = 5 }
        };
    }
}
```

### Export Functionality

```csharp
public class GridExportService
{
    public async Task<Result<byte[]>> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        List<GridColumn> columns,
        GridExportOptions? options = null)
    {
        options ??= new GridExportOptions();
        
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        // Write headers
        foreach (var column in columns.Where(c => c.IsVisible))
        {
            csv.WriteField(column.DisplayName);
        }
        await csv.NextRecordAsync();
        
        // Write rows
        foreach (var item in data)
        {
            foreach (var column in columns.Where(c => c.IsVisible))
            {
                var value = GetPropertyValue(item, column.PropertyName);
                csv.WriteField(value);
            }
            await csv.NextRecordAsync();
        }
        
        var bytes = Encoding.UTF8.GetBytes(writer.ToString());
        return Result<byte[]>.Success(bytes);
    }
    
    private object? GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType()
            .GetProperty(propertyName)
            ?.GetValue(obj);
    }
}

// Usage
var exportResult = await _exportService.ExportToCsvAsync(
    selectedRows,
    columns,
    new GridExportOptions
    {
        ExportSelectedRowsOnly = true,
        IncludeColumns = new[] { "Name", "Email", "Department" }
    });

if (exportResult.IsSuccess)
{
    return File(exportResult.Value, "text/csv", "export.csv");
}
```

---

## Multi-Tenancy

### 1. Tenant Context

**Purpose**: Track current tenant across requests.

```csharp
public interface ITenantContext
{
    string? TenantId { get; set; }
    string? TenantName { get; set; }
    Dictionary<string, object> Data { get; set; }
}

public class TenantContext : ITenantContext
{
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}
```

**Setup** (Program.cs):

```csharp
// Register tenant context
services.AddScoped<ITenantContext, TenantContext>();

// Middleware to extract tenant from request
app.Use(async (context, next) =>
{
    var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();
    
    // Extract from header, subdomain, or claims
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
    
    await next();
});
```

**Usage**:

```csharp
public class ProductService
{
    private readonly ITenantContext _tenantContext;
    private readonly IRepository<Product> _productRepository;
    
    public ProductService(ITenantContext tenantContext, IRepository<Product> repository)
    {
        _tenantContext = tenantContext;
        _productRepository = repository;
    }
    
    public async Task<List<Product>> GetProductsAsync()
    {
        var tenantId = _tenantContext.TenantId;
        
        return await _productRepository
            .AsQueryable()
            .Where(p => p.TenantId == tenantId)
            .ToListAsync();
    }
}
```

### 2. Tenant Feature Flags

**Purpose**: Enable/disable features per tenant.

```csharp
public interface ITenantFeatureFlags
{
    bool IsFeatureEnabled(string featureName);
    Task<bool> IsFeatureEnabledAsync(string featureName);
}

public class DefaultTenantFeatureFlags : ITenantFeatureFlags
{
    private readonly ITenantContext _tenantContext;
    private readonly IFeatureFlagService _flagService;
    
    public DefaultTenantFeatureFlags(
        ITenantContext tenantContext,
        IFeatureFlagService flagService)
    {
        _tenantContext = tenantContext;
        _flagService = flagService;
    }
    
    public bool IsFeatureEnabled(string featureName)
    {
        var key = $"{_tenantContext.TenantId}:{featureName}";
        return _flagService.IsFeatureEnabled(key);
    }
    
    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        var key = $"{_tenantContext.TenantId}:{featureName}";
        return await _flagService.IsFeatureEnabledAsync(key);
    }
}
```

**Usage**:

```csharp
public class OrderService
{
    private readonly ITenantFeatureFlags _featureFlags;
    
    public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        // Check if feature is enabled for tenant
        var advancedShippingEnabled = 
            await _featureFlags.IsFeatureEnabledAsync("advanced_shipping");
        
        if (advancedShippingEnabled)
        {
            // Use advanced shipping logic
            request.ShippingMethod = await GetAdvancedShippingAsync();
        }
        else
        {
            // Use basic shipping
            request.ShippingMethod = "standard";
        }
        
        return await CreateAsync(request);
    }
}
```

---

## Feature Flags

### 1. Feature Flag Service

**Purpose**: Runtime feature control across application.

```csharp
public interface IFeatureFlagService
{
    bool IsFeatureEnabled(string featureName);
    Task<bool> IsFeatureEnabledAsync(string featureName);
    void SetFeatureFlag(string featureName, bool enabled);
}

public class DefaultFeatureFlagService : IFeatureFlagService
{
    private readonly ConcurrentDictionary<string, bool> _flags = new();
    
    public DefaultFeatureFlagService()
    {
        // Initialize with defaults
        _flags.TryAdd("feature:advanced_search", false);
        _flags.TryAdd("feature:bulk_operations", true);
        _flags.TryAdd("feature:audit_logging", true);
    }
    
    public bool IsFeatureEnabled(string featureName)
    {
        return _flags.GetOrAdd(featureName, false);
    }
    
    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        return await Task.FromResult(IsFeatureEnabled(featureName));
    }
    
    public void SetFeatureFlag(string featureName, bool enabled)
    {
        _flags.AddOrUpdate(featureName, enabled, (_, __) => enabled);
    }
}
```

**Setup** (Program.cs):

```csharp
services.AddSingleton<IFeatureFlagService, DefaultFeatureFlagService>();

// Seed initial flags
var flagService = app.Services.GetRequiredService<IFeatureFlagService>();
flagService.SetFeatureFlag("feature:beta_ui", false);
flagService.SetFeatureFlag("feature:export_excel", true);
```

**Usage Examples**:

```csharp
public class SearchService
{
    private readonly IFeatureFlagService _flagService;
    
    public async Task<Result<SearchResponse>> SearchAsync(SearchRequest request)
    {
        if (_flagService.IsFeatureEnabled("feature:advanced_search"))
        {
            return await AdvancedSearchAsync(request);
        }
        else
        {
            return await BasicSearchAsync(request);
        }
    }
    
    private async Task<Result<SearchResponse>> AdvancedSearchAsync(SearchRequest request)
    {
        // AI-powered search with ML ranking
        return await _advancedSearchEngine.SearchAsync(request);
    }
    
    private async Task<Result<SearchResponse>> BasicSearchAsync(SearchRequest request)
    {
        // SQL LIKE search
        return await _basicSearch.SearchAsync(request);
    }
}

// In Razor components
@inject IFeatureFlagService FlagService

@if (FlagService.IsFeatureEnabled("feature:beta_ui"))
{
    <BetaUserInterface />
}
else
{
    <LegacyUserInterface />
}
```

**Conditional UI Rendering**:

```csharp
public class AdminPageModel : PageModel
{
    private readonly IFeatureFlagService _flagService;
    
    public bool ShowBulkExport { get; set; }
    public bool ShowAdvancedFilters { get; set; }
    
    public void OnGet()
    {
        ShowBulkExport = _flagService.IsFeatureEnabled("feature:bulk_export");
        ShowAdvancedFilters = _flagService.IsFeatureEnabled("feature:advanced_filters");
    }
}
```

---

## Event Bus

### 1. In-Memory Event Publishing

**Purpose**: Decoupled inter-service communication via events.

```csharp
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
}

public interface IEventSubscriber
{
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
}

public class InMemoryEventPublisher : IEventPublisher, IEventSubscriber
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    
    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(
            eventType,
            new List<Delegate> { handler },
            (_, handlers) =>
            {
                handlers.Add(handler);
                return handlers;
            });
    }
    
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
    
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                if (handler is Func<TEvent, Task> typedHandler)
                {
                    await typedHandler(@event);
                }
            }
        }
    }
}
```

**Setup** (Program.cs):

```csharp
services.AddSingleton<InMemoryEventPublisher>();
services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<InMemoryEventPublisher>());
services.AddSingleton<IEventSubscriber>(sp => sp.GetRequiredService<InMemoryEventPublisher>());
```

### 2. Domain Events

**Event Definition**:

```csharp
public class UserCreatedEvent
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class OrderPlacedEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}
```

**Publishing Events**:

```csharp
public class UserService
{
    private readonly IEventPublisher _eventPublisher;
    
    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User { Email = request.Email, Name = request.Name };
        
        var result = await _userRepository.AddAsync(user);
        
        if (result.IsSuccess)
        {
            // Publish event
            await _eventPublisher.PublishAsync(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email
            });
        }
        
        return result;
    }
}
```

**Subscribing to Events**:

```csharp
public class EmailNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly IEventSubscriber _eventSubscriber;
    
    public EmailNotificationService(
        IEmailSender emailSender,
        IEventSubscriber eventSubscriber)
    {
        _emailSender = emailSender;
        _eventSubscriber = eventSubscriber;
        
        // Subscribe to events
        _eventSubscriber.Subscribe<UserCreatedEvent>(OnUserCreatedAsync);
        _eventSubscriber.Subscribe<OrderPlacedEvent>(OnOrderPlacedAsync);
    }
    
    private async Task OnUserCreatedAsync(UserCreatedEvent @event)
    {
        await _emailSender.SendAsync(
            to: @event.Email,
            subject: "Welcome!",
            body: $"Welcome {NameExtraction.FromEmail(@event.Email)}!");
    }
    
    private async Task OnOrderPlacedAsync(OrderPlacedEvent @event)
    {
        // Send order confirmation email
        await _emailSender.SendAsync(
            to: "@event.CustomerId@domain.com",
            subject: "Order Confirmed",
            body: $"Your order #{@event.OrderId} for ${@event.Total} has been placed.");
    }
}
```

**Usage Pattern**:

```csharp
// Startup - Subscribe to events
app.Services.GetRequiredService<EmailNotificationService>();
app.Services.GetRequiredService<AuditLoggingService>();
app.Services.GetRequiredService<AnalyticsService>();

// During operation - Events propagate automatically
var userService = app.Services.GetRequiredService<UserService>();
await userService.CreateUserAsync(request); // Triggers emails, audit logs, analytics
```

---

## Pagination

### 1. PagedList<T> & PagedQuery

**Purpose**: Server-side pagination with metadata.

```csharp
// Request model
public record PagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false,
    string? SearchTerm = null);

// Response model
public class PagedList<T>
{
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

**Usage in Repository**:

```csharp
public interface IRepository<T> where T : class
{
    Task<PagedList<T>> GetPagedAsync(PagedQuery query);
}

public class ProductRepository : IRepository<Product>
{
    private readonly ApplicationDbContext _context;
    
    public async Task<PagedList<Product>> GetPagedAsync(PagedQuery query)
    {
        var queryable = _context.Products.AsQueryable();
        
        // Apply search
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            queryable = queryable.Where(p =>
                p.Name.Contains(query.SearchTerm) ||
                p.Description.Contains(query.SearchTerm));
        }
        
        // Apply sorting
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            var parameter = Expression.Parameter(typeof(Product), "p");
            var property = Expression.Property(parameter, query.SortBy);
            var lambda = Expression.Lambda(property, parameter);
            
            var orderByMethod = query.SortDescending
                ? "OrderByDescending"
                : "OrderBy";
            
            queryable = (IQueryable<Product>)typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == orderByMethod && m.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(Product), property.Type)
                .Invoke(null, new object[] { queryable, lambda })!;
        }
        
        // Get total count
        var totalCount = await queryable.CountAsync();
        
        // Apply paging
        var skip = (query.Page - 1) * query.PageSize;
        var items = await queryable
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync();
        
        return new PagedList<T>
        {
            Items = items,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}
```

**Usage in API Endpoint**:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRepository<Product> _repository;
    
    [HttpGet("paged")]
    public async Task<ActionResult<PagedList<ProductDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? searchTerm = null)
    {
        var query = new PagedQuery(page, pageSize, sortBy, sortDescending, searchTerm);
        var pagedList = await _repository.GetPagedAsync(query);
        
        var dtos = pagedList.Items
            .Select(p => new ProductDto { Id = p.Id, Name = p.Name })
            .ToList();
        
        return Ok(new PagedList<ProductDto>
        {
            Items = dtos,
            PageNumber = pagedList.PageNumber,
            PageSize = pagedList.PageSize,
            TotalCount = pagedList.TotalCount
        });
    }
}
```

---

## Security & Validation

### 1. Password Helper

```csharp
public class PasswordHelper
{
    public static string HashPassword(string password)
    {
        // Uses PBKDF2 with salt
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    
    public static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
    
    public static bool IsStrongPassword(string password)
    {
        return password.Length >= 8 &&
               Regex.IsMatch(password, @"[a-z]") &&
               Regex.IsMatch(password, @"[A-Z]") &&
               Regex.IsMatch(password, @"[0-9]") &&
               Regex.IsMatch(password, @"[!@#$%^&*]");
    }
}

// Usage
var hashedPassword = PasswordHelper.HashPassword("MySecurePass123!");
var isValid = PasswordHelper.VerifyPassword("MySecurePass123!", hashedPassword);
```

### 2. Validation Framework

**Define Validators**:

```csharp
public class CreateUserValidator : ValidatorBase<CreateUserRequest>
{
    public CreateUserValidator()
    {
        // Chain validation rules
        AddRule("Email", new EmailRule())
            .AddRule("Password", new PasswordRule())
            .AddRule("Name", new LengthRule(3, 100));
    }
}

// Custom rules
public class EmailRule : IValidationRule
{
    public ValidationFailure? Validate(object? value)
    {
        if (value == null)
            return new ValidationFailure("Email is required");
        
        if (!Regex.IsMatch(value.ToString(), @"^[\w\.-]+@[\w\.-]+\.\w+$"))
            return new ValidationFailure("Invalid email format");
        
        return null; // Valid
    }
}

public class PasswordRule : IValidationRule
{
    public ValidationFailure? Validate(object? value)
    {
        if (value == null)
            return new ValidationFailure("Password is required");
        
        var pwd = value.ToString();
        
        if (pwd.Length < 8)
            return new ValidationFailure("Password must be at least 8 characters");
        
        if (!Regex.IsMatch(pwd, @"[A-Z]"))
            return new ValidationFailure("Password must contain uppercase letter");
        
        if (!Regex.IsMatch(pwd, @"[0-9]"))
            return new ValidationFailure("Password must contain digit");
        
        return null;
    }
}
```

**Use Validators**:

```csharp
public class UserController : ControllerBase
{
    private readonly IValidator<CreateUserRequest> _validator;
    private readonly UserService _userService;
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Validate request
        var validationResult = _validator.Validate(request);
        
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                errors = validationResult.Failures.Select(f => f.Message)
            });
        }
        
        // Create user
        var result = await _userService.CreateAsync(request);
        
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}
```

### 3. Encryption Helper

```csharp
public class EncryptionHelper
{
    private readonly byte[] _encryptionKey;
    
    public EncryptionHelper(string keyBase64)
    {
        _encryptionKey = Convert.FromBase64String(keyBase64);
    }
    
    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        
        // Write IV
        ms.Write(aes.IV, 0, aes.IV.Length);
        
        // Write encrypted data
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cs))
        {
            writer.Write(plaintext);
        }
        
        return Convert.ToBase64String(ms.ToArray());
    }
    
    public string Decrypt(string ciphertext)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        
        var buffer = Convert.FromBase64String(ciphertext);
        
        // Read IV
        aes.IV = new byte[aes.IV.Length];
        Array.Copy(buffer, 0, aes.IV, 0, aes.IV.Length);
        
        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(buffer, aes.IV.Length, buffer.Length - aes.IV.Length);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        
        return reader.ReadToEnd();
    }
}

// Setup
var key = Convert.ToBase64String(new byte[32]); // 256-bit key
services.AddSingleton(new EncryptionHelper(key));

// Usage
var encrypted = encryptionHelper.Encrypt("sensitive-data");
var decrypted = encryptionHelper.Decrypt(encrypted);
```

---

## Logging & Diagnostics

### 1. Audit Logging

**Purpose**: Track all data modifications for compliance.

```csharp
public interface IAuditLogger
{
    Task LogAsync(AuditRecord record);
}

public class AuditRecord
{
    public string UserId { get; set; }
    public string Action { get; set; } // Create, Update, Delete
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
}
```

**Implementation**:

```csharp
public class DatabaseAuditLogger : IAuditLogger
{
    private readonly AuditDbContext _context;
    
    public async Task LogAsync(AuditRecord record)
    {
        _context.AuditLogs.Add(new AuditLogEntity
        {
            UserId = record.UserId,
            Action = record.Action,
            EntityType = record.EntityType,
            EntityId = record.EntityId,
            OldValues = record.OldValues,
            NewValues = record.NewValues,
            Timestamp = record.Timestamp,
            IpAddress = record.IpAddress,
            CorrelationId = record.CorrelationId
        });
        
        await _context.SaveChangesAsync();
    }
}
```

**Usage**:

```csharp
public class UserService
{
    private readonly IAuditLogger _auditLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public async Task<Result<User>> UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<User>.Failure(new Error("NOT_FOUND", "User not found"));
        
        // Capture old values
        var oldValues = JsonSerializer.Serialize(user);
        
        // Update
        user.Name = request.Name;
        user.Email = request.Email;
        await _userRepository.UpdateAsync(user);
        
        // Log audit
        await _auditLogger.LogAsync(new AuditRecord
        {
            UserId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value ?? "unknown",
            Action = "Update",
            EntityType = nameof(User),
            EntityId = userId.ToString(),
            OldValues = oldValues,
            NewValues = JsonSerializer.Serialize(user),
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
        
        return Result<User>.Success(user);
    }
}
```

### 2. System Diagnostics

```csharp
public class SystemInfo
{
    public string MachineName { get; set; }
    public string OsVersion { get; set; }
    public int ProcessorCount { get; set; }
    public long TotalMemory { get; set; }
    public DateTime StartTime { get; set; }
}

public class HealthCheck
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } // Healthy, Degraded, Unhealthy
    public List<ComponentHealth> Components { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

public class ComponentHealth
{
    public string Component { get; set; }
    public bool IsHealthy { get; set; }
    public string? Message { get; set; }
}

// Check application health
public class ApplicationHealthCheck
{
    private readonly IDbConnection _dbConnection;
    private readonly ICacheService _cache;
    
    public async Task<HealthCheck> CheckHealthAsync()
    {
        var health = new HealthCheck();
        
        // Check database
        try
        {
            await _dbConnection.OpenAsync();
            _dbConnection.Close();
            health.Components.Add(new ComponentHealth { Component = "Database", IsHealthy = true });
        }
        catch
        {
            health.Components.Add(new ComponentHealth
            {
                Component = "Database",
                IsHealthy = false,
                Message = "Cannot connect to database"
            });
        }
        
        // Check cache
        try
        {
            await _cache.SetAsync("health-check", "ok");
            var result = await _cache.GetAsync<string>("health-check");
            health.Components.Add(new ComponentHealth
            {
                Component = "Cache",
                IsHealthy = result.IsSuccess
            });
        }
        catch
        {
            health.Components.Add(new ComponentHealth
            {
                Component = "Cache",
                IsHealthy = false
            });
        }
        
        health.IsHealthy = health.Components.All(c => c.IsHealthy);
        health.Status = health.IsHealthy ? "Healthy" : "Unhealthy";
        
        return health;
    }
}
```

---

## Communication Services

### 1. Email Sender

```csharp
public interface IEmailSender
{
    Task<Result<bool>> SendAsync(
        string to,
        string subject,
        string body,
        string? from = null);
}

public class EmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    
    public async Task<Result<bool>> SendAsync(
        string to,
        string subject,
        string body,
        string? from = null)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };
            
            var message = new MailMessage(from ?? _settings.FromAddress, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            await client.SendMailAsync(message);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new Error("EMAIL_FAILED", $"Failed to send email: {ex.Message}"));
        }
    }
}

// Setup
services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
services.AddScoped<IEmailSender, EmailSender>();

// Usage
await emailSender.SendAsync(
    to: "user@example.com",
    subject: "Welcome!",
    body: "<h1>Welcome</h1><p>You have been registered.</p>");
```

### 2. SMS Service

```csharp
public interface ISmsService
{
    Task<Result<bool>> SendAsync(string phoneNumber, string message);
}

public class TwilioSmsService : ISmsService
{
    private readonly TwilioClient _client;
    
    public async Task<Result<bool>> SendAsync(string phoneNumber, string message)
    {
        try
        {
            var result = await _client.Messages.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_settings.FromNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber));
            
            return Result<bool>.Success(result.Sid != null);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new Error("SMS_FAILED", ex.Message));
        }
    }
}
```

### 3. Template Engine (Phase 1)

**Purpose**: File and string-based template rendering with placeholder substitution for dynamic email/SMS content.

**Interface:**

```csharp
public interface ITemplateEngine
{
    // Synchronous string rendering
    string Render(string content, IDictionary<string, string> values);
    string Render(string content, object model);

    // Asynchronous file-based rendering
    Task<Result<string>> RenderFileAsync(
        string filePath,
        IDictionary<string, string> values,
        CancellationToken ct = default);
    
    Task<Result<string>> RenderFileAsync(
        string filePath,
        object model,
        CancellationToken ct = default);

    // Batch directory loading/rendering
    Task<Result<Dictionary<string, string>>> LoadDirectoryAsync(
        string directoryPath,
        string searchPattern = "*.html",
        CancellationToken ct = default);
    
    Task<Result<Dictionary<string, string>>> RenderDirectoryAsync(
        string directoryPath,
        IDictionary<string, string> values,
        string searchPattern = "*.html",
        CancellationToken ct = default);
}
```

**Setup:**

```csharp
// Program.cs
services.AddScoped<ITemplateEngine, TemplateEngine>();
```

**Placeholder Syntax:**

```csharp
// Simple placeholders — match object properties
var result = _templateEngine.Render(
    "Hello {Name}! Your order {OrderNumber} is confirmed.",
    new { Name = "Alice", OrderNumber = "ORD-123" }
);
// Result: "Hello Alice! Your order ORD-123 is confirmed."

// Translation key placeholders — match dictionary keys
var result = _templateEngine.Render(
    "{{GREETING}} {{OFFER}}",
    new Dictionary<string, string>
    {
        { "GREETING", "Welcome" },
        { "OFFER", "Save 10%" }
    }
);
// Result: "Welcome Save 10%"
```

**File-Based Rendering:**

```csharp
// Load from file and render
var result = await _templateEngine.RenderFileAsync(
    "~/Templates/Emails/order-confirmation.html",
    new
    {
        CustomerName = "Bob Smith",
        OrderNumber = order.Id,
        Total = $"${order.Total:F2}",
        ShippingDate = order.ShippingDate?.ToString("MMMM dd, yyyy") ?? "TBD"
    }
);

if (result.IsFailure)
{
    _logger.Error($"Template render failed: {result.Error.Message}");
    return;
}

await _emailSender.SendAsync(order.Email, "Order Confirmed", result.Value);
```

**Bulk Rendering:**

```csharp
// Load all templates from a directory
var templates = await _templateEngine.LoadDirectoryAsync("~/Templates/Emails");

// Render directory with values applied to all templates
var rendered = await _templateEngine.RenderDirectoryAsync(
    "~/Templates/Emails",
    new Dictionary<string, string>
    {
        { "CompanyName", "ACME Corp" },
        { "SupportEmail", "support@acme.com" }
    }
);
```

**Benefits:**
- No hardcoded strings in C# code
- Supports both object properties and translation keys
- Parallel file I/O for performance
- Safe error handling via `Result<T>`
- Case-insensitive placeholder matching

See detailed guide: [TEMPLATE_ENGINE_GUIDE.md](TEMPLATE_ENGINE_GUIDE.md)

---

## HTTP Client (Phase 1)

### HttpStatusCode Enum for Retry Policies

**Purpose**: Type-safe HTTP status codes instead of magic integers in retry configuration.

```csharp
// Before Phase 1: Magic integers
new RetryPolicy
{
    RetryableStatusCodes = [408, 429, 500, 502, 503]
}

// Phase 1: Type-safe enums
new RetryPolicy
{
    RetryableStatusCodes = [
        HttpStatusCode.RequestTimeout,      // 408
        HttpStatusCode.TooManyRequests,     // 429
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway,          // 502
        HttpStatusCode.ServiceUnavailable   // 503
    ]
}
```

**Common Retryable Codes:**
- `RequestTimeout` (408) — Client timeout
- `TooManyRequests` (429) — Rate limit
- `InternalServerError` (500) — Server error
- `BadGateway` (502) — Gateway issue
- `ServiceUnavailable` (503) — Temporary unavailability
- `GatewayTimeout` (504) — Gateway timeout

See: [HTTP_CLIENT_GUIDE.md](HTTP_CLIENT_GUIDE.md#retry-policies)

---

## Resilience Patterns

### 1. Circuit Breaker

```csharp
public interface ICircuitBreaker
{
    Task<Result<T>> ExecuteAsync<T>(Func<Task<T>> operation);
}

public class CircuitBreaker : ICircuitBreaker
{
    private readonly CircuitBreakerOptions _options;
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    
    public async Task<Result<T>> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitBreakerState.Open)
        {
            // Check if timeout has passed
            if (DateTime.UtcNow - _lastFailureTime > _options.Timeout)
            {
                _state = CircuitBreakerState.HalfOpen;
            }
            else
            {
                return Result<T>.Failure(new Error("CIRCUIT_OPEN", "Circuit breaker is open"));
            }
        }
        
        try
        {
            var result = await operation();
            _failureCount = 0;
            _state = CircuitBreakerState.Closed;
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_failureCount >= _options.FailureThreshold)
            {
                _state = CircuitBreakerState.Open;
            }
            
            return Result<T>.Failure(new Error("OPERATION_FAILED", ex.Message));
        }
    }
}

// Setup
var circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions
{
    FailureThreshold = 5,
    Timeout = TimeSpan.FromSeconds(60)
});

// Usage
var result = await circuitBreaker.ExecuteAsync(async () =>
{
    return await externalApiClient.GetDataAsync();
});
```

### 2. Rate Limiter

```csharp
public interface IRateLimiter
{
    bool IsAllowed(string key);
}

public class RateLimiter : IRateLimiter
{
    private readonly RateLimiterOptions _options;
    private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
    
    public bool IsAllowed(string key)
    {
        var now = DateTime.UtcNow;
        
        if (!_requests.TryGetValue(key, out var requests))
        {
            _requests.TryAdd(key, new List<DateTime> { now });
            return true;
        }
        
        // Remove old requests
        requests.RemoveAll(r => now - r > _options.Window);
        
        if (requests.Count < _options.MaxRequests)
        {
            requests.Add(now);
            return true;
        }
        
        return false;
    }
}

// Usage
var rateLimiter = new RateLimiter(new RateLimiterOptions
{
    MaxRequests = 100,
    Window = TimeSpan.FromMinutes(1)
});

[HttpGet("data")]
public IActionResult GetData()
{
    var clientId = User.FindFirst("sub")?.Value;
    
    if (!rateLimiter.IsAllowed(clientId))
    {
        return StatusCode(429, "Rate limit exceeded");
    }
    
    return Ok(GetDataInternal());
}
```

---

## Helper Libraries

### String Helpers

```csharp
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value)
        => string.IsNullOrEmpty(value);
    
    public static bool IsNullOrWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value);
    
    public static string? NullIfEmpty(this string? value)
        => string.IsNullOrEmpty(value) ? null : value;
    
    public static string Truncate(this string value, int length, string suffix = "...")
        => value.Length > length ? value.Substring(0, length - suffix.Length) + suffix : value;
    
    public static string ToSlug(this string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var slug = Regex.Replace(normalized, @"[^\w\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-");
        return slug.ToLower().Trim('-');
    }
}

// Usage
var slug = "My Product Name".ToSlug(); // "my-product-name"
var truncated = "Very long text...".Truncate(10); // "Very lo..."
```

### DateTime Helpers

```csharp
public static class DateTimeExtensions
{
    public static bool IsBetween(
        this DateTime date,
        DateTime start,
        DateTime end)
        => date >= start && date <= end;
    
    public static int DaysSince(this DateTime date)
        => (DateTime.UtcNow - date).Days;
    
    public static string ToFriendlyString(this DateTime date)
    {
        var diff = DateTime.UtcNow - date;
        
        return diff.TotalSeconds < 60 ? "just now"
            : diff.TotalMinutes < 60 ? $"{(int)diff.TotalMinutes} minutes ago"
            : diff.TotalHours < 24 ? $"{(int)diff.TotalHours} hours ago"
            : diff.TotalDays < 30 ? $"{(int)diff.TotalDays} days ago"
            : date.ToString("MMM dd, yyyy");
    }
}

// Usage
var created = DateTime.UtcNow.AddHours(-2);
Console.WriteLine(created.ToFriendlyString()); // "2 hours ago"
```

### Collection Helpers

```csharp
public static class CollectionExtensions
{
    public static IEnumerable<T> Chunk<T>(this IEnumerable<T> source, int size)
    {
        var batch = new List<T>(size);
        
        foreach (var item in source)
        {
            batch.Add(item);
            
            if (batch.Count == size)
            {
                yield return (T)batch;
                batch = new List<T>(size);
            }
        }
        
        if (batch.Count > 0)
            yield return (T)batch;
    }
    
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        => source == null || !source.Any();
    
    public static IEnumerable<(T, T)> Pairs<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        
        while (enumerator.MoveNext())
        {
            var first = enumerator.Current;
            
            if (!enumerator.MoveNext())
                yield break;
            
            var second = enumerator.Current;
            yield return (first, second);
        }
    }
}
```

---

## Domain-Driven Design

### 1. Aggregate Root Pattern

```csharp
public abstract class AggregateRoot
{
    public int Id { get; set; }
    
    // Domain events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void RaiseDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Example: Order Aggregate**:

```csharp
public class Order : AggregateRoot
{
    public int CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; private set; } = new();
    
    public static Result<Order> Create(int customerId, List<OrderItem> items)
    {
        if (items.IsNullOrEmpty())
            return Result<Order>.Failure(new Error("NO_ITEMS", "Order must have items"));
        
        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            Items = items
        };
        
        order.TotalAmount = items.Sum(i => i.Price * i.Quantity);
        
        // Raise domain event
        order.RaiseDomainEvent(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = customerId,
            TotalAmount = order.TotalAmount
        });
        
        return Result<Order>.Success(order);
    }
    
    public Result<bool> Confirm()
    {
        if (Status != OrderStatus.Pending)
            return Result<bool>.Failure(new Error("INVALID_STATE", "Order cannot be confirmed"));
        
        Status = OrderStatus.Confirmed;
        
        RaiseDomainEvent(new OrderConfirmedEvent { OrderId = Id });
        
        return Result<bool>.Success(true);
    }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
```

### 2. Specification Pattern

```csharp
public abstract class Specification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int Take { get; protected set; }
    public int Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }
}

// Usage
public class ActiveProductsSpecification : Specification<Product>
{
    public ActiveProductsSpecification()
    {
        Criteria = p => p.IsActive && !p.IsDeleted;
        OrderBy = p => p.Name;
        Includes.Add(p => p.Category);
    }
}

public class ProductsInPriceRangeSpec : Specification<Product>
{
    public ProductsInPriceRangeSpec(decimal minPrice, decimal maxPrice, int page, int pageSize)
    {
        Criteria = p => p.Price >= minPrice && p.Price <= maxPrice;
        OrderByDescending = p => p.CreatedDate;
        ApplyPaging(pageSize * (page - 1), pageSize);
    }
}

// In repository
public class ProductRepository
{
    public async Task<List<Product>> GetBySpecificationAsync(Specification<Product> spec)
    {
        var query = _context.Products.AsQueryable();
        
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);
        
        // Include navigations
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        
        // Apply ordering
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);
        
        // Apply paging
        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip).Take(spec.Take);
        
        return await query.ToListAsync();
    }
}
```

---

## Integration Patterns

### 1. Repository Pattern

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<PagedList<T>> GetPagedAsync(PagedQuery query);
    Task<Result<T>> AddAsync(T entity);
    Task<Result<T>> UpdateAsync(T entity);
    Task<Result<bool>> DeleteAsync(T entity);
}

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
    
    public async Task<Result<T>> AddAsync(T entity)
    {
        try
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return Result<T>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(new Error("ADD_FAILED", ex.Message));
        }
    }
}
```

### 2. Unit of Work Pattern

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Product> Products { get; }
    IRepository<Order> Orders { get; }
    IRepository<Customer> Customers { get; }
    
    Task<int> SaveChangesAsync();
    Task<Result<bool>> BeginTransactionAsync();
    Task<Result<bool>> CommitAsync();
    Task<Result<bool>> RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public IRepository<Product> Products { get; private set; }
    public IRepository<Order> Orders { get; private set; }
    public IRepository<Customer> Customers { get; private set; }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task<Result<bool>> BeginTransactionAsync()
    {
        try
        {
            await _context.Database.BeginTransactionAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(new Error("TRANSACTION_FAILED", ex.Message));
        }
    }
}

// Usage
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();
        
        if (!transaction.IsSuccess)
            return Result<Order>.Failure(transaction.Error);
        
        try
        {
            // Create order
            var order = Order.Create(request.CustomerId, request.Items).Value;
            await _unitOfWork.Orders.AddAsync(order);
            
            // Update inventory
            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                product.StockQuantity -= item.Quantity;
                await _unitOfWork.Products.UpdateAsync(product);
            }
            
            // Save all changes
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            
            return Result<Order>.Success(order);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return Result<Order>.Failure(new Error("ORDER_FAILED", ex.Message));
        }
    }
}
```

---

## Complete Integration Example

**Scenario**: Create a new product with validation, caching, event publishing, and audit logging.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductRequest> _validator;
    private readonly IEventPublisher _eventPublisher;
    private readonly IAuditLogger _auditLogger;
    private readonly ICacheService _cacheService;
    private readonly ITenantContext _tenantContext;
    private readonly IFeatureFlagService _flagService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        // Validate input
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Failures });
        }
        
        // Check feature flag
        if (!_flagService.IsFeatureEnabled("feature:new_product_creation"))
        {
            return StatusCode(503, "Feature not available");
        }
        
        // Create product
        var result = await _productService.CreateAsync(request);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }
        
        var product = result.Value;
        
        // Publish event
        await _eventPublisher.PublishAsync(new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            CreatedAt = DateTime.UtcNow
        });
        
        // Invalidate cache
        await _cacheService.RemoveByPrefixAsync($"tenant:{_tenantContext.TenantId}:products");
        
        // Log audit
        await _auditLogger.LogAsync(new AuditRecord
        {
            UserId = User.FindFirst("sub")?.Value ?? "unknown",
            Action = "Create",
            EntityType = "Product",
            EntityId = product.Id.ToString(),
            NewValues = JsonSerializer.Serialize(product),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = HttpContext.TraceIdentifier
        });
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        // Try cache first
        var cacheKey = $"tenant:{_tenantContext.TenantId}:product:{id}";
        var cachedResult = await _cacheService.GetAsync<ProductDto>(cacheKey);
        
        if (cachedResult.IsSuccess && cachedResult.Value != null)
        {
            return Ok(cachedResult.Value);
        }
        
        // Get from service
        var result = await _productService.GetByIdAsync(id);
        
        if (!result.IsSuccess)
        {
            return NotFound(result.Error);
        }
        
        var product = result.Value;
        
        // Cache for 15 minutes
        await _cacheService.SetAsync(cacheKey, product, new CacheOptions
        {
            Duration = TimeSpan.FromMinutes(15),
            Strategy = CacheStrategy.Sliding
        });
        
        return Ok(product);
    }
}
```

---

## Summary & Best Practices

| Feature | When to Use | Example |
|---------|------------|---------|
| **Result<T> Pattern** | Expected failures | User not found, validation failed |
| **Exceptions** | Unexpected failures | Database corrupted, null reference |
| **Caching** | Frequently accessed data | User profiles, product catalogs |
| **Grid Component** | Tabular data display | Order lists, product inventory |
| **Multi-Tenancy** | SaaS applications | Separate data per customer |
| **Feature Flags** | Gradual rollouts | New UI, beta features |
| **Event Bus** | Decoupled services | Send email on user signup |
| **Validation** | User input | Registration form, API requests |
| **Pagination** | Large datasets | 10M products, show 20/page |
| **Audit Logging** | Compliance | Track all data modifications |
| **Encryption** | Sensitive data | PII, credit cards, API keys |
| **Circuit Breaker** | External APIs | Graceful degradation |
| **Rate Limiting** | API protection | Max 100 requests/minute |

---

**Last Updated:** 2026-04-19  
**Author:** SmartWorkz Development Team  
**License:** Internal Use Only
