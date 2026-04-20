# Cache Attribute Pattern

## Overview

The `[Cache]` attribute provides a simple decorator pattern for caching HTTP action results without manual `ICacheService` code. It's ideal for frequently-accessed, read-only endpoints.

Use `[Cache]` when:
- The endpoint returns the same data for all users/tenants (or filters by route)
- Response rarely changes (> 5 minutes)
- You want automatic expiration without boilerplate

Use `ICacheService` when:
- You need explicit cache control in business logic
- Cache key depends on complex query parameters
- You need to invalidate cache programmatically

---

## Basic Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Cache response for 60 seconds (default)
    [Cache(Seconds = 60)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _service.GetProductAsync(id);
        return Ok(product);
    }

    // Cache for 5 minutes with a custom key
    [Cache(Seconds = 300, Key = "AllProducts")]
    [HttpGet]
    public async Task<IActionResult> ListProducts()
    {
        var products = await _service.ListAsync();
        return Ok(products);
    }

    // Cache with sliding expiration (resets on every request)
    [Cache(Seconds = 600, SlidingExpiration = true)]
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopular()
    {
        return Ok(await _service.GetPopularAsync());
    }
}
```

---

## Configuration Options

| Property | Type | Default | Meaning |
|----------|------|---------|---------|
| `Seconds` | int | 60 | How long to cache the response in seconds |
| `Key` | string? | null | Custom cache key. If null, uses request path (e.g., `/api/products/5`) |
| `SlidingExpiration` | bool | false | If true, expiration resets on every cache hit; if false, absolute expiration |

---

## Cache Key Strategy

### Default (Key = null)
Uses the request path as the cache key:
- GET `/api/products/5` → cache key: `/api/products/5`
- GET `/api/products` → cache key: `/api/products`

**Warning:** Query parameters are NOT included, so `GET /api/products?filter=active` and `GET /api/products` share the same cache. Use a custom Key if query parameters matter.

### Custom Key
```csharp
[Cache(Seconds = 60, Key = "AllProductsByCategory")]
public IActionResult GetByCategory([FromQuery] string category)
{
    // All requests share the same cache, regardless of category!
    // This is usually wrong. See below for multi-tenant workaround.
}
```

### Multi-Tenant Cache
For tenant-isolated data, include TenantId in the key:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ITenantContext _tenantContext;

    [Cache(Seconds = 300, Key = "TenantProducts")]
    [HttpGet]
    public async Task<IActionResult> ListProducts()
    {
        var tenantId = _tenantContext.TenantId;
        // ❌ WRONG: All tenants share the same cache
        var products = await _service.ListAsync();
        return Ok(products);
    }
}
```

For tenant isolation, add TenantId to the key manually:

```csharp
public class ProductsController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly ITenantContext _tenantContext;

    [HttpGet]
    public async Task<IActionResult> ListProducts()
    {
        var key = $"Products_Tenant{_tenantContext.TenantId}";
        if (_cache.TryGetValue(key, out var cached))
            return Ok(cached);

        var products = await _service.ListAsync();
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));
        _cache.Set(key, products, cacheOptions);
        return Ok(products);
    }
}
```

---

## Interaction with ICacheService

The `[Cache]` attribute uses `IMemoryCache` internally (in-process memory cache). It does NOT use `ICacheService` (the hybrid L1+L2 cache).

**If you also use ICacheService:**
- `[Cache]` stores only in local memory (fast, per-instance)
- `ICacheService` stores in L1 (local) + L2 (distributed)
- They use different keys and don't share state

**Best practice:** Use one or the other, not both, to avoid confusion:
- Use `[Cache]` for simple, read-only endpoints
- Use `ICacheService` when you need distributed caching or programmatic invalidation

---

## Absolute vs. Sliding Expiration

### Absolute Expiration (default)
Cache expires after exactly N seconds from the first cache hit.

```csharp
[Cache(Seconds = 60)]  // Expires 60 seconds after first request
public IActionResult GetData() => Ok(new { data = "foo" });
```

**Use for:** Static data that's refreshed on a schedule (every hour, every day).

### Sliding Expiration
Cache expiration resets every time the cache is hit.

```csharp
[Cache(Seconds = 600, SlidingExpiration = true)]  // 10 minutes, resets on each hit
public IActionResult GetData() => Ok(new { data = "foo" });
```

If the cache is accessed every 5 minutes, it never expires. If no requests come in for 10+ minutes, it expires.

**Use for:** Frequently-accessed data where you want "last-access-time" semantics (user sessions, activity feeds).

---

## Performance Considerations

**Pros:**
- Zero code in the action method
- Automatic handling of concurrent requests
- Minimal overhead

**Cons:**
- Only works on action results (not complex business logic)
- Cache key doesn't include query parameters by default
- Single-instance cache (not distributed in load-balanced scenarios)

---

## Testing

Unit test with a mock `IMemoryCache`:

```csharp
[Fact]
public async Task GetProduct_CachesResult()
{
    // Arrange
    var controller = new ProductsController(_service, _logger);
    var product = new { id = 1, name = "Widget" };
    _service.GetProductAsync(1).Returns(Task.FromResult(product));

    // Act - first call
    var result1 = await controller.GetProduct(1);

    // Act - second call (should hit cache)
    var result2 = await controller.GetProduct(1);

    // Assert
    Assert.IsType<OkObjectResult>(result1);
    Assert.IsType<OkObjectResult>(result2);
    // Service is called only once due to caching
    await _service.Received(1).GetProductAsync(1);
}
```

---

## Common Pitfalls

### 1. Forgetting Query Parameters
```csharp
[Cache]
[HttpGet]
public IActionResult Search([FromQuery] string q)
{
    // ❌ All searches share the same cache!
    // GET /api/search?q=widget → cache key: /api/search
    // GET /api/search?q=gadget → cache key: /api/search (same!)
}
```

**Fix:** Use a custom Key that includes parameters, or use `ICacheService`.

### 2. Caching Sensitive Data
```csharp
[Cache]
[Authorize]
[HttpGet("profile")]
public IActionResult GetProfile()
{
    // ⚠️ User profile cached and possibly leaked to other users
    // if the cache key doesn't include UserId
}
```

**Fix:** Include UserId in the Key, or don't cache sensitive data.

### 3. Not Checking Content-Type
The `[Cache]` attribute caches any `ObjectResult`, but fails silently for `StatusCodeResult` (204 No Content, 304 Not Modified).

```csharp
[Cache]
[HttpGet("check-exists")]
public IActionResult CheckExists(int id)
{
    var exists = _service.Exists(id);
    if (!exists)
        return NoContent();  // ❌ Not cached (no content to cache)
    return Ok(new { exists = true });
}
```

---

## Reference

- **[Template Engine Guide](../TEMPLATE_ENGINE_GUIDE.md)** — For email/notification template caching
- **[Configuration & Diagnostics Guide](../CONFIGURATION_DIAGNOSTICS_GUIDE.md)** — IMemoryCache configuration
- **[Utilities & Extensions Guide](../UTILITIES_EXTENSIONS_GUIDE.md)** — Other caching helpers
