# SmartWorkz.Core Quick Reference

**One-page cheat sheet for common tasks**

---

## Result<T> Pattern (Error Handling)

```csharp
// Return success
return Result<User>.Success(user);

// Return failure
return Result<User>.Failure(new Error("CODE", "Message"));

// Check result
if (result.IsSuccess)
    return result.Value;
else
    Console.WriteLine(result.Error.Message);

// Chain operations
var result = await GetUserAsync()
    .Map(user => new UserDto { Name = user.Name })
    .Bind(dto => ValidateAsync(dto));
```

---

## Caching

```csharp
// Get from cache
var cached = await _cache.GetAsync<User>("user:1");
if (cached.IsSuccess) return cached.Value;

// Set in cache
await _cache.SetAsync("user:1", user, new CacheOptions
{
    Duration = TimeSpan.FromMinutes(10),
    Strategy = CacheStrategy.Sliding
});

// Invalidate
await _cache.RemoveAsync("user:1");
await _cache.RemoveByPrefixAsync("user:");
```

---

## [Cache] Attribute (Phase 1)

```csharp
// Automatic response caching — no boilerplate
[Cache(Seconds = 60)]
[HttpGet("{id}")]
public async Task<IActionResult> GetProduct(int id)
{
    var product = await _service.GetProductAsync(id);
    return Ok(product);
}

// Custom key and sliding expiration
[Cache(Seconds = 300, Key = "AllProducts", SlidingExpiration = true)]
[HttpGet]
public async Task<IActionResult> ListProducts()
{
    return Ok(await _service.ListAsync());
}
```

---

## Grid Component

### Setup
```csharp
// Program.cs
services.AddScoped<IGridDataProvider, GridDataProvider>();
services.AddScoped<GridExportService>();
```

### Razor Page
```html
<grid asp-data-source="/api/products/grid"
      asp-columns="@Columns"
      asp-page-size="25"
      asp-allow-selection="true"
      asp-allow-export="true">
</grid>
```

### API Endpoint
```csharp
[HttpPost("grid")]
public async Task<Result<GridResponse<T>>> GetGridData(GridRequest request)
{
    var items = ApplyFilters(ApplySorting(GetData(), request), request);
    var paged = new PagedList<T>(items, request.Page, request.PageSize, total);
    return Result<GridResponse<T>>.Success(new GridResponse<T> { Data = paged });
}
```

---

## Multi-Tenancy

```csharp
// Program.cs
services.AddScoped<ITenantContext, TenantContext>();

// Middleware
app.Use(async (context, next) =>
{
    var tenant = context.RequestServices.GetRequiredService<ITenantContext>();
    tenant.TenantId = context.Request.Headers["X-Tenant-ID"];
    await next();
});

// Usage
var tenantId = _tenantContext.TenantId;
var products = await _repo.Where(p => p.TenantId == tenantId).ToListAsync();
```

---

## Feature Flags

```csharp
// Setup
services.AddSingleton<IFeatureFlagService, DefaultFeatureFlagService>();

// Enable/disable
flagService.SetFeatureFlag("feature:beta_ui", true);

// Check in code
if (_flagService.IsFeatureEnabled("feature:beta_ui"))
{
    // Use new feature
}

// Check in Razor
@if (FlagService.IsFeatureEnabled("feature:beta_ui"))
{
    <NewUI />
}
```

---

## Event Bus

```csharp
// Define event
public class UserCreatedEvent
{
    public int UserId { get; set; }
}

// Publish
await _eventPublisher.PublishAsync(new UserCreatedEvent { UserId = user.Id });

// Subscribe
_eventSubscriber.Subscribe<UserCreatedEvent>(OnUserCreatedAsync);

private async Task OnUserCreatedAsync(UserCreatedEvent @event)
{
    await _emailSender.SendWelcomeEmailAsync(@event.UserId);
}
```

---

## Pagination

```csharp
// Request
var query = new PagedQuery(
    page: 1,
    pageSize: 20,
    sortBy: "Name",
    sortDescending: false,
    searchTerm: "laptop");

// Get paged data
var paged = await _repo.GetPagedAsync(query);

// Response properties
paged.Items           // List<T>
paged.PageNumber      // Current page
paged.TotalPages      // Total pages
paged.HasNextPage     // bool
paged.HasPreviousPage // bool
```

---

## Validation

```csharp
// Define validator
public class CreateUserValidator : ValidatorBase<CreateUserRequest>
{
    public CreateUserValidator()
    {
        AddRule("Email", new EmailRule())
            .AddRule("Password", new PasswordRule());
    }
}

// Use validator
var result = validator.Validate(request);
if (!result.IsValid)
    return BadRequest(result.Failures);
```

---

## Security

### Password
```csharp
var hash = PasswordHelper.HashPassword("MyPassword123!");
var valid = PasswordHelper.VerifyPassword("MyPassword123!", hash);
var strong = PasswordHelper.IsStrongPassword("MyPassword123!");
```

### Encryption
```csharp
var encrypted = encryptionHelper.Encrypt("sensitive-data");
var decrypted = encryptionHelper.Decrypt(encrypted);
```

### Hashing
```csharp
var hash = HashHelper.Hash("data");        // SHA256
var hmac = HmacHelper.Hash("data", "key"); // HMAC-SHA256
```

---

## Logging & Audit

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
var health = await healthCheck.CheckHealthAsync();
Console.WriteLine(health.IsHealthy ? "Healthy" : "Unhealthy");
```

---

## Email & SMS

```csharp
// Email
await _emailSender.SendAsync(
    to: "user@example.com",
    subject: "Welcome!",
    body: "<h1>Welcome</h1>");

// SMS
await _smsService.SendAsync(
    phoneNumber: "+1234567890",
    message: "Your code is 123456");
```

---

## Template Engine (Phase 1)

```csharp
// Render string with placeholders
var template = "Hello {Name}, your balance is {Balance}";
var result = _templateEngine.Render(template, new { Name = "Alice", Balance = "$50.00" });
// Result: "Hello Alice, your balance is $50.00"

// Render file with data
var result = await _templateEngine.RenderFileAsync(
    "~/Templates/Emails/welcome.html",
    new { UserName = "Bob", ActivationLink = "https://..." }
);

// Translation key placeholders
var result = _templateEngine.Render(
    "{{GREETING}} {{OFFER}}",
    new Dictionary<string, string>
    {
        { "GREETING", "Welcome" },
        { "OFFER", "Save 10%" }
    }
);
```

---

## Resilience

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

### Retry (Phase 1: HttpStatusCode Enum)
```csharp
// Type-safe retry policy with enums
var result = await _httpClient
    .Get<User>("https://api.example.com/users/123")
    .WithRetry(new RetryPolicy
    {
        MaxAttempts = 3,
        Strategy = RetryStrategy.Exponential,
        RetryableStatusCodes = [
            HttpStatusCode.RequestTimeout,      // 408
            HttpStatusCode.TooManyRequests,     // 429
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.BadGateway,          // 502
            HttpStatusCode.ServiceUnavailable   // 503
        ]
    })
    .ExecuteAsync();
```

---

## String Extensions

```csharp
"text".IsNullOrEmpty()           // false
"text".IsNullOrWhiteSpace()      // false
"text".NullIfEmpty()             // "text"
"Very long text".Truncate(10)    // "Very lo..."
"My Product Name".ToSlug()       // "my-product-name"
```

---

## DateTime Extensions

```csharp
var date = DateTime.UtcNow.AddHours(-2);
date.IsBetween(start, end)       // true/false
date.DaysSince()                 // 0
date.ToFriendlyString()          // "2 hours ago"
```

---

## Collection Extensions

```csharp
list.IsNullOrEmpty()             // true if null or no items
list.Chunk(10)                   // Split into groups of 10
list.Pairs()                     // Group into pairs: (a,b), (c,d)
```

---

## Data Access (Phase 1)

### DbProviderFactory with Enum
```csharp
// Type-safe provider lookup
var provider = DbProviderFactory.GetProvider(DatabaseProvider.SqlServer);
var connection = provider.CreateConnection("Server=localhost;Database=MyDb");
```

### DbExtensions Aliases
```csharp
// ADO helpers — simpler names
var users = await dbProvider.QueryAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = 1 }
);

var count = await dbProvider.ScalarAsync<int>(
    "SELECT COUNT(*) FROM Users"
);

var affected = await dbProvider.NonQueryAsync(
    "UPDATE Users SET Status = @Status WHERE Id = @Id",
    new { Status = "Active", Id = 1 }
);

// Dapper helpers
var user = await dbProvider.QuerySingleAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = 1 }
);
```

---

## Repository Pattern

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

---

## Unit of Work Pattern

```csharp
public interface IUnitOfWork
{
    IRepository<Product> Products { get; }
    IRepository<Order> Orders { get; }
    Task<int> SaveChangesAsync();
    Task<Result<bool>> BeginTransactionAsync();
    Task<Result<bool>> CommitAsync();
    Task<Result<bool>> RollbackAsync();
}

// Usage
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

## Dependency Injection (Program.cs)

```csharp
// Core services
services.AddScoped<ITenantContext, TenantContext>();
services.AddScoped<ICacheService, MemoryCacheService>();
services.AddSingleton<IFeatureFlagService, DefaultFeatureFlagService>();
services.AddSingleton<InMemoryEventPublisher>();

// Grid
services.AddScoped<IGridDataProvider, GridDataProvider>();
services.AddScoped<GridExportService>();

// Repositories
services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
services.AddScoped<IEmailSender, EmailSender>();
services.AddScoped<ISmsService, TwilioSmsService>();
services.AddScoped<IAuditLogger, DatabaseAuditLogger>();
```

---

## Common Patterns

### Tenant-Aware Cache Key
```csharp
var key = $"tenant:{_tenantContext.TenantId}:product:{id}";
await _cache.GetAsync<Product>(key);
```

### Safe API Result Handling
```csharp
var result = await service.GetAsync(id);
return result.IsSuccess
    ? Ok(result.Value)
    : result.Error?.Code == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error);
```

### Transaction Wrapper
```csharp
var transaction = await _unitOfWork.BeginTransactionAsync();
if (!transaction.IsSuccess) return transaction;

try { /* operations */ }
catch { return await _unitOfWork.RollbackAsync(); }

return await _unitOfWork.CommitAsync();
```

### Paginated API Response
```csharp
[HttpGet("paged")]
public async Task<ActionResult<PagedList<T>>> GetPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null)
{
    var query = new PagedQuery(page, pageSize, sortBy);
    return await repository.GetPagedAsync(query);
}
```

---

**For detailed examples, see:** `docs/SMARTWORKZ_CORE_DEVELOPER_GUIDE.md`
