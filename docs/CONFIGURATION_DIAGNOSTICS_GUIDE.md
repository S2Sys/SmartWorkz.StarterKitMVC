# Configuration & Diagnostics Guide

Complete reference for accessing application configuration, monitoring system health, collecting metrics, and implementing distributed tracing in SmartWorkz applications.

---

## Overview

SmartWorkz provides four complementary tools for application configuration and observability:

1. **`IConfigurationHelper`** — Typed, validated configuration access with type conversion
2. **`DiagnosticsHelper`** — Real-time system monitoring (CPU, memory, disk, health)
3. **`MetricsHelper`** — Performance metrics collection (timers, execution tracking)
4. **`ICorrelationContext`** — Distributed request tracing across service boundaries

---

## Configuration Helper

### IConfigurationHelper Interface

Access application configuration with automatic type conversion and validation.

```csharp
public interface IConfigurationHelper
{
    T GetRequired<T>(string key);           // Throws if missing
    T GetOptional<T>(string key, T defaultValue);
    bool TryGet<T>(string key, out T? value);
    bool Exists(string key);
}
```

### DI Registration

```csharp
builder.Services.AddScoped<IConfigurationHelper>(sp =>
    new ConfigurationHelper(sp.GetRequiredService<IConfiguration>())
);
```

### GetRequired<T> — Get Value or Throw

Returns a configuration value with automatic type conversion. Throws `ConfigurationValidationException` if missing or conversion fails.

```csharp
private readonly IConfigurationHelper _config;

public MyService(IConfigurationHelper config)
{
    _config = config;
}

// Get string
var appName = _config.GetRequired<string>("App:Name");

// Get int
var maxRetries = _config.GetRequired<int>("Http:MaxRetries");

// Get bool
var isProduction = _config.GetRequired<bool>("App:IsProduction");

// Get TimeSpan
var timeout = _config.GetRequired<TimeSpan>("Http:Timeout");

// Get enum
var logLevel = _config.GetRequired<LogLevel>("Logging:Level");
```

**Configuration in appsettings.json:**

```json
{
  "App": {
    "Name": "SmartWorkz",
    "IsProduction": false
  },
  "Http": {
    "MaxRetries": 3,
    "Timeout": "00:00:30"
  },
  "Logging": {
    "Level": "Information"
  }
}
```

### GetOptional<T> — Get Value with Default

Returns a configuration value, or a default if missing.

```csharp
var appVersion = _config.GetOptional<string>("App:Version", "1.0.0");
var pageSize = _config.GetOptional<int>("Paging:DefaultPageSize", 20);
var enableCache = _config.GetOptional<bool>("Cache:Enabled", true);
```

### TryGet<T> — Safe Retrieval

Returns `true` if key exists and can be converted, `false` otherwise. Safe pattern for optional config.

```csharp
if (_config.TryGet<string>("Features:ApiKey", out var apiKey) && !string.IsNullOrEmpty(apiKey))
{
    // API key is configured and valid
    _httpClient.WithHeader("X-API-Key", apiKey);
}
else
{
    // API key not configured or invalid
    _logger.LogWarning("API key not configured");
}
```

### Exists — Check if Key Exists

```csharp
if (_config.Exists("Database:ConnectionString"))
{
    // Database configured
    var connectionString = _config.GetRequired<string>("Database:ConnectionString");
}
```

### Complete Initialization Example

```csharp
public class AppInitializer
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var config = builder.Services.BuildServiceProvider()
            .GetRequiredService<IConfigurationHelper>();
        
        // Get required settings at startup
        var dbConnection = config.GetRequired<string>("Database:ConnectionString");
        var jwtSecret = config.GetRequired<string>("Jwt:Secret");
        var appName = config.GetRequired<string>("App:Name");
        
        // Get optional settings with defaults
        var logLevel = config.GetOptional<string>("Logging:Level", "Information");
        var cacheEnabled = config.GetOptional<bool>("Cache:Enabled", true);
        var maxConnections = config.GetOptional<int>("Database:MaxConnections", 100);
        
        // Validate critical settings
        if (string.IsNullOrWhiteSpace(dbConnection) || string.IsNullOrWhiteSpace(jwtSecret))
        {
            throw new InvalidOperationException("Critical configuration missing");
        }
        
        // Continue setup...
    }
}
```

---

## Diagnostics Helper

### DiagnosticsHelper — System Monitoring

Real-time monitoring of system resources and application health.

```csharp
using SmartWorkz.Core.Shared.Diagnostics;

var systemInfo = DiagnosticsHelper.GetSystemInfo();
var memory = DiagnosticsHelper.GetMemoryUsage();
var cpu = DiagnosticsHelper.GetCpuUsage();
var disk = DiagnosticsHelper.GetDiskSpace();
var health = DiagnosticsHelper.GetApplicationHealth();
var isHealthy = DiagnosticsHelper.IsHealthy();
```

### GetSystemInfo — Complete System Snapshot

Returns a snapshot of system resources at a point in time.

```csharp
public class SystemInfo
{
    public CpuUsage CpuUsage { get; set; }
    public MemoryUsage MemoryUsage { get; set; }
    public DiskSpace DiskSpace { get; set; }
    public int ProcessorCount { get; set; }
    public DateTime GatheredAt { get; set; }
}
```

**Example:**

```csharp
var systemInfo = DiagnosticsHelper.GetSystemInfo();

Console.WriteLine($"Processors: {systemInfo.ProcessorCount}");
Console.WriteLine($"CPU Load: {systemInfo.CpuUsage.LoadAverage:P}");
Console.WriteLine($"Memory Used: {systemInfo.MemoryUsage.UsedPercentage:P}");
Console.WriteLine($"Disk Free: {systemInfo.DiskSpace.FreeSpace / 1_000_000_000} GB");
```

### GetMemoryUsage — Memory Details

```csharp
public class MemoryUsage
{
    public long TotalMemory { get; set; }       // Bytes
    public long UsedMemory { get; set; }        // Bytes
    public long AvailableMemory { get; set; }   // Bytes
    public double UsedPercentage { get; set; }  // 0-100
    public double AvailablePercentage { get; set; }
}
```

**Usage:**

```csharp
var memory = DiagnosticsHelper.GetMemoryUsage();

if (memory.UsedPercentage > 80)
{
    _logger.LogWarning("Memory usage high: {Percentage:P}", memory.UsedPercentage);
}

Console.WriteLine($"Memory: {memory.UsedMemory / 1_000_000} MB / {memory.TotalMemory / 1_000_000} MB");
```

### GetCpuUsage — CPU Load

```csharp
public class CpuUsage
{
    public double Percentage { get; set; }  // Current CPU usage
    public double LoadAverage { get; set; } // Average over time
}
```

### GetDiskSpace — Disk Usage

```csharp
public class DiskSpace
{
    public long TotalSpace { get; set; }     // Bytes
    public long FreeSpace { get; set; }      // Bytes
    public long UsedSpace { get; set; }      // Bytes
    public string DriveLabel { get; set; }
    public double UsedPercentage { get; set; }
    public double FreePercentage { get; set; }
}
```

### GetApplicationHealth — Health Check Status

```csharp
public class ApplicationHealth
{
    public HealthStatus Status { get; set; } // Healthy, Warning, Critical
    public List<HealthCheck> Checks { get; set; }
    public DateTime AsOfTime { get; set; }
}

public class HealthCheck
{
    public string Name { get; set; }
    public HealthStatus Status { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum HealthStatus
{
    Healthy = 0,
    Warning = 1,
    Critical = 2
}
```

**Usage:**

```csharp
var health = DiagnosticsHelper.GetApplicationHealth();

Console.WriteLine($"Overall Health: {health.Status}");
foreach (var check in health.Checks)
{
    Console.WriteLine($"  {check.Name}: {check.Status}");
}

// Check specific component
if (health.Status == HealthStatus.Critical)
{
    _logger.LogError("Application health is critical!");
}
```

### Health Check Endpoint Example

```csharp
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        var health = DiagnosticsHelper.GetApplicationHealth();
        
        var response = new
        {
            status = health.Status.ToString(),
            checks = health.Checks.Select(c => new
            {
                name = c.Name,
                status = c.Status.ToString(),
                message = c.Message
            }),
            asOf = health.AsOfTime
        };
        
        var statusCode = health.Status switch
        {
            HealthStatus.Healthy => StatusCodes.Status200OK,
            HealthStatus.Warning => StatusCodes.Status200OK,
            HealthStatus.Critical => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
        
        return StatusCode(statusCode, response);
    }
}
```

---

## Metrics Helper

### MetricsHelper — Performance Tracking

Collect execution time, memory usage, and custom metrics.

```csharp
using SmartWorkz.Core.Shared.Diagnostics;
```

### StartTimer — Measure Execution Time

```csharp
public async Task<List<Product>> GetProductsAsync()
{
    using (var timer = MetricsHelper.StartTimer("GetProducts"))
    {
        var products = await _repository.GetAllAsync();
        
        // Timer auto-logs elapsed time when disposed
        return products;
    }
    
    // Output: "GetProducts completed in 234ms"
}
```

### TrackExecution<T> — Track Function Execution

```csharp
public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
{
    var result = await MetricsHelper.TrackExecution(
        async () =>
        {
            // Business logic here
            var order = new Order { /* ... */ };
            await _repository.AddAsync(order);
            return order;
        },
        "CreateOrder"
    );
    
    // Logs execution time and result
    return result;
}
```

### MeasureMemory — Track Memory Usage

```csharp
public void ProcessLargeDataSet()
{
    var snapshot = DiagnosticsHelper.GetMemoryUsage();
    var beforeMemory = snapshot.UsedMemory;
    
    // Process data...
    ProcessData();
    
    snapshot = DiagnosticsHelper.GetMemoryUsage();
    var afterMemory = snapshot.UsedMemory;
    var allocated = (afterMemory - beforeMemory) / 1_000_000;
    
    _logger.LogInformation("Data processing allocated {MB}MB", allocated);
}
```

### MetricsCollector — Aggregate Metrics

```csharp
var collector = new MetricsHelper.MetricsCollector();

for (int i = 0; i < 100; i++)
{
    using (var timer = collector.StartTimer("query"))
    {
        // Run query...
    }
}

var metrics = collector.GetMetrics();
Console.WriteLine($"Average query time: {metrics.AverageMs}ms");
Console.WriteLine($"Min: {metrics.MinMs}ms, Max: {metrics.MaxMs}ms");
```

---

## Correlation Context

### ICorrelationContext — Distributed Tracing

Track requests across service boundaries for debugging and logging.

```csharp
public interface ICorrelationContext
{
    string GetCorrelationId();
    string GetParentCorrelationId();
    string GetUserId();
    string GetTenantId();
    
    void SetCorrelationId(string correlationId);
    void SetUserId(string userId);
    void SetTenantId(string tenantId);
    
    ICorrelationContext CreateChildContext();
}
```

### TenantContext Implementation

```csharp
public class CorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<string> _correlationId = new();
    private static readonly AsyncLocal<string> _parentCorrelationId = new();
    private static readonly AsyncLocal<string> _userId = new();
    private static readonly AsyncLocal<string> _tenantId = new();
    
    public string GetCorrelationId() => _correlationId.Value ?? "";
    public string GetParentCorrelationId() => _parentCorrelationId.Value ?? "";
    public string GetUserId() => _userId.Value ?? "";
    public string GetTenantId() => _tenantId.Value ?? "";
    
    public void SetCorrelationId(string id) => _correlationId.Value = id;
    public void SetUserId(string id) => _userId.Value = id;
    public void SetTenantId(string id) => _tenantId.Value = id;
    
    public ICorrelationContext CreateChildContext()
    {
        var child = new CorrelationContext();
        child._parentCorrelationId.Value = GetCorrelationId();
        child._correlationId.Value = Guid.NewGuid().ToString();
        // Inherit user and tenant
        child._userId.Value = GetUserId();
        child._tenantId.Value = GetTenantId();
        return child;
    }
}
```

### Usage in Request Middleware

```csharp
public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorrelationContext _correlationContext;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create correlation ID from header
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();
        
        _correlationContext.SetCorrelationId(correlationId);
        
        // Extract user and tenant from JWT
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value ?? "";
            var tenantId = context.User.FindFirst("tenant")?.Value ?? "";
            
            _correlationContext.SetUserId(userId);
            _correlationContext.SetTenantId(tenantId);
        }
        
        // Add to response headers
        context.Response.Headers.Add("X-Correlation-Id", correlationId);
        
        await _next(context);
    }
}
```

### Structured Logging with Correlation

```csharp
public class OrderService
{
    private readonly ICorrelationContext _correlation;
    private readonly ILogger<OrderService> _logger;
    
    public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        // Correlation ID automatically included in logs
        _logger.LogInformation("Creating order for user {UserId} in tenant {TenantId}",
            _correlation.GetUserId(),
            _correlation.GetTenantId());
        
        var order = new Order { /* ... */ };
        
        _logger.LogInformation("Order {OrderId} created",
            order.Id);
        
        return Result.Ok(order);
    }
}
```

With Serilog enrichment:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("CorrelationId", correlationContext.GetCorrelationId())
    .Enrich.WithProperty("UserId", correlationContext.GetUserId())
    .Enrich.WithProperty("TenantId", correlationContext.GetTenantId())
    .WriteTo.Console()
    .CreateLogger();
```

### Child Context for Service Calls

```csharp
public async Task<Result<OrderSummary>> GetOrderSummaryAsync(int orderId)
{
    // Create child context for sub-operation
    var childContext = _correlationContext.CreateChildContext();
    
    using (var scope = _serviceProvider.CreateScope())
    {
        // Child operations inherit user/tenant but get new correlation ID
        var result = await someService.GetDetailsAsync(orderId);
        
        return result;
    }
}
```

---

## Complete Monitoring Example

```csharp
[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly IConfigurationHelper _config;
    private readonly ICorrelationContext _correlation;
    private readonly ILogger<DiagnosticsController> _logger;
    
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        using (var timer = MetricsHelper.StartTimer("GetStatus"))
        {
            var systemInfo = DiagnosticsHelper.GetSystemInfo();
            var health = DiagnosticsHelper.GetApplicationHealth();
            var appName = _config.GetOptional<string>("App:Name", "Unknown");
            
            var response = new
            {
                app = appName,
                health = health.Status.ToString(),
                uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime,
                system = new
                {
                    processors = systemInfo.ProcessorCount,
                    cpu = $"{systemInfo.CpuUsage.Percentage:P}",
                    memory = $"{systemInfo.MemoryUsage.UsedPercentage:P}",
                    disk = $"{systemInfo.DiskSpace.UsedPercentage:P}",
                    checks = systemInfo.Checks
                },
                correlation = new
                {
                    id = _correlation.GetCorrelationId(),
                    userId = _correlation.GetUserId(),
                    tenantId = _correlation.GetTenantId()
                },
                asOf = DateTime.UtcNow
            };
            
            _logger.LogInformation("Status check requested by {UserId}",
                _correlation.GetUserId());
            
            var statusCode = health.Status == HealthStatus.Healthy
                ? StatusCodes.Status200OK
                : StatusCodes.Status503ServiceUnavailable;
            
            return StatusCode(statusCode, response);
        }
    }
}
```

---

## Best Practices

### 1. Always Use GetRequired for Critical Settings

```csharp
// ✓ Good — fail fast at startup if missing
var dbConnection = _config.GetRequired<string>("Database:ConnectionString");

// ✗ Bad — null exception later
var dbConnection = _config.GetOptional<string>("Database:ConnectionString", null);
```

### 2. Capture Correlation ID Early

```csharp
// ✓ Good — middleware captures at request start
public class CorrelationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var val)
            ? val.ToString()
            : Guid.NewGuid().ToString();
        
        _correlationContext.SetCorrelationId(correlationId);
        await _next(context);
    }
}

// ✗ Bad — correlation ID not available to downstream code
```

### 3. Monitor in Loops

```csharp
// ✓ Good — detect performance degradation
var metrics = new MetricsHelper.MetricsCollector();

foreach (var item in items)
{
    using (var timer = metrics.StartTimer("ProcessItem"))
    {
        ProcessItem(item);
    }
}

if (metrics.GetMetrics().AverageMs > 100)
{
    _logger.LogWarning("Processing slower than expected");
}
```

---

## Troubleshooting

### "Configuration key not found"

**Solution:** Use GetOptional with default or check Exists first:

```csharp
if (_config.Exists("MyKey"))
{
    var value = _config.GetRequired<string>("MyKey");
}
else
{
    _logger.LogWarning("MyKey not configured");
}
```

### "Can't convert configuration value to type X"

**Solution:** Check appsettings.json format matches the type:

```json
{
  "Timeout": "00:00:30"  // Valid TimeSpan format
}
```

### Correlation ID not propagating to logs

**Solution:** Ensure middleware runs before logging middleware:

```csharp
// In Program.cs, order matters
app.UseCorrelationMiddleware(); // Must come first
app.UseSerilogRequestLogging();
```

---

## See Also

- [SmartWorkz Core Developer Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md) — Full infrastructure overview
- [Security Guide](SECURITY_GUIDE.md) — Secure configuration handling
- [Serilog Documentation](https://serilog.net/)
- [Microsoft Configuration Documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)
