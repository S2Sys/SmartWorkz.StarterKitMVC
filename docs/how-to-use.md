# SmartWorkz.StarterKitMVC – How to Use Guide

This guide provides clear examples for using the key components of the StarterKitMVC boilerplate.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Result Pattern](#result-pattern)
3. [Extension Methods](#extension-methods)
4. [HTTP Service](#http-service)
5. [Settings System](#settings-system)
6. [LoV (List of Values) System](#lov-list-of-values-system)
7. [Identity Service](#identity-service)
8. [Event Bus](#event-bus)
9. [Notification Hub](#notification-hub)
10. [Feature Flags](#feature-flags)
11. [Background Jobs](#background-jobs)
12. [Local Storage](#local-storage)
13. [Logging & Correlation](#logging--correlation)
14. [Multi-Tenancy](#multi-tenancy)
15. [Generating Documentation](#generating-documentation)

---

## Getting Started

### 1. Clone and Rename

```powershell
# Clone the repository
git clone https://github.com/smartworkz/starterkitmvc.git

# Rename to your project
cd starterkitmvc
.\rename-project.ps1 -NewCompany "YourCompany" -NewProject "YourProject"
```

### 2. Build and Run

```bash
dotnet build
dotnet run --project src/SmartWorkz.StarterKitMVC.Web
```

### 3. Access Admin Panel

Navigate to: `https://localhost:5001/Admin/Dashboard`

---

## Result Pattern

The `Result` and `Result<T>` types provide a clean way to handle success/failure without exceptions.

### Basic Usage

```csharp
using SmartWorkz.StarterKitMVC.Shared.Primitives;

// Success case
var success = Result.Success();

// Failure case
var failure = Result.Failure(new Error("VALIDATION_ERROR", "Email is invalid."));

// Check result
if (success.IsSuccess)
{
    Console.WriteLine("Operation succeeded!");
}
else
{
    Console.WriteLine($"Error: {failure.Error.Code} - {failure.Error.Message}");
}
```

### With Return Value

```csharp
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _repository.FindAsync(id);
    
    if (user is null)
        return Result<User>.Failure(new Error("NOT_FOUND", "User not found."));
    
    return Result<User>.Success(user);
}

// Usage
var result = await GetUserAsync(123);
if (result.IsSuccess)
{
    Console.WriteLine($"User: {result.Value.Name}");
}
```

---

## Extension Methods

### String Extensions

```csharp
using SmartWorkz.StarterKitMVC.Shared.Extensions;

string? input = "  hello world  ";

// Check if null or whitespace
if (input.IsNullOrWhiteSpace())
    Console.WriteLine("Input is empty!");

// Safe trim (handles null)
var trimmed = input.SafeTrim(); // "hello world"

string? nullInput = null;
var safe = nullInput.SafeTrim(); // "" (empty string, not null)
```

### DateTime Extensions

```csharp
using SmartWorkz.StarterKitMVC.Shared.Extensions;

var date = DateTime.Now;

// Convert to UTC kind
var utc = date.ToUtcKind();

// Check if date is in range
var start = new DateTime(2024, 1, 1);
var end = new DateTime(2024, 12, 31);

if (date.IsBetween(start, end))
    Console.WriteLine("Date is in 2024!");
```

### JSON Extensions

```csharp
using SmartWorkz.StarterKitMVC.Shared.Extensions;

// Serialize to JSON
var user = new { Name = "John", Age = 30 };
var json = user.ToJson();
// Result: {"name":"John","age":30}

// Deserialize from JSON
var jsonString = "{\"name\":\"Jane\",\"age\":25}";
var person = jsonString.FromJson<Person>();
Console.WriteLine(person?.Name); // Jane
```

### Collection Extensions

```csharp
using SmartWorkz.StarterKitMVC.Shared.Extensions;

var list = new List<string> { "a", "b", "c" };
IReadOnlyCollection<string> readOnly = list.ToReadOnlyCollection();
```

### Enum Extensions

```csharp
using System.ComponentModel;
using SmartWorkz.StarterKitMVC.Shared.Extensions;

public enum Status
{
    [Description("Currently Active")]
    Active,
    [Description("Temporarily Inactive")]
    Inactive
}

var status = Status.Active;
var description = status.GetDescription(); // "Currently Active"
```

---

## HTTP Service

Make HTTP requests with built-in correlation, logging, and error handling.

### Setup (already configured in Program.cs)

```csharp
builder.Services.AddHttpClient<IHttpService, HttpService>();
```

### Usage

```csharp
using SmartWorkz.StarterKitMVC.Application.Abstractions;
using SmartWorkz.StarterKitMVC.Infrastructure.Http;

public class ExternalApiService
{
    private readonly IHttpService _http;

    public ExternalApiService(IHttpService http) => _http = http;

    public async Task<User?> GetUserAsync(int id)
    {
        var request = new ApiRequest
        {
            Method = HttpMethod.Get,
            Path = $"/api/users/{id}",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer your-token"
            }
        };

        var response = await _http.SendAsync<User>(request);

        if (response.IsSuccess)
            return response.Data;

        Console.WriteLine($"Error: {response.Error?.Message}");
        return null;
    }

    public async Task<bool> CreateUserAsync(User user)
    {
        var request = new ApiRequest
        {
            Method = HttpMethod.Post,
            Path = "/api/users",
            Body = user
        };

        var response = await _http.SendAsync<User>(request);
        return response.IsSuccess;
    }
}
```

---

## Settings System

Manage application settings with System → Tenant → User override hierarchy.

### Get a Setting

```csharp
using SmartWorkz.StarterKitMVC.Application.Settings;
using SmartWorkz.StarterKitMVC.Domain.Settings;

public class ThemeService
{
    private readonly ISettingsService _settings;

    public ThemeService(ISettingsService settings) => _settings = settings;

    public async Task<string> GetUserThemeAsync(string userId)
    {
        // Tries User scope first, falls back to Tenant, then System
        var setting = await _settings.GetAsync("app.theme", SettingScope.User, userId: userId);
        return setting?.Value ?? "light";
    }
}
```

### Set a Setting

```csharp
public async Task SetUserThemeAsync(string userId, string theme)
{
    await _settings.SetAsync("app.theme", theme, SettingScope.User, userId: userId);
}
```

### Get All Categories

```csharp
public async Task<IReadOnlyCollection<SettingCategory>> GetCategoriesAsync()
{
    return await _settings.GetCategoriesAsync();
}
```

---

## LoV (List of Values) System

Manage hierarchical dropdowns with localization and tenant support.

### Get Items for a Dropdown

```csharp
using SmartWorkz.StarterKitMVC.Application.LoV;

public class DropdownService
{
    private readonly ILovService _lov;

    public DropdownService(ILovService lov) => _lov = lov;

    public async Task<IReadOnlyCollection<LovItem>> GetCountriesAsync(string locale = "en-US")
    {
        return await _lov.GetItemsAsync("countries", locale: locale);
    }

    public async Task<IReadOnlyCollection<LovItem>> GetStatesAsync(string countryCode)
    {
        return await _lov.GetItemsAsync("states", tags: new[] { countryCode });
    }
}
```

### Use Dynamic Dropdown Service

```csharp
using SmartWorkz.StarterKitMVC.Application.LoV;

public class FormController : Controller
{
    private readonly IDropdownService _dropdown;

    public FormController(IDropdownService dropdown) => _dropdown = dropdown;

    public async Task<IActionResult> GetCountryDropdown()
    {
        var items = await _dropdown.GetDropdownAsync("countries", locale: "en-US");
        return Ok(items.Select(i => new { value = i.Value, text = i.Text }));
    }
}
```

---

## Identity Service

Handle user authentication and registration.

### Login

```csharp
using SmartWorkz.StarterKitMVC.Application.Identity;

public class AuthController : Controller
{
    private readonly IIdentityService _identity;

    public AuthController(IIdentityService identity) => _identity = identity;

    [HttpPost("login")]
    public async Task<IActionResult> Login(string userName, string password)
    {
        var result = await _identity.LoginAsync(userName, password);

        if (result.IsSuccess)
        {
            return Ok(new
            {
                accessToken = result.Value.AccessToken,
                refreshToken = result.Value.RefreshToken,
                expiresAt = result.Value.ExpiresAt
            });
        }

        return Unauthorized(result.Error.Message);
    }
}
```

### Register

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(string userName, string email, string password)
{
    var result = await _identity.RegisterAsync(userName, email, password);

    if (result.IsSuccess)
        return Ok("Registration successful!");

    return BadRequest(result.Error.Message);
}
```

### Get Current User

```csharp
[Authorize]
public async Task<IActionResult> Profile()
{
    var user = await _identity.GetCurrentUserAsync();
    return user is not null ? Ok(user) : NotFound();
}
```

---

## Event Bus

Publish and subscribe to events for decoupled communication.

### Define an Event

```csharp
using SmartWorkz.StarterKitMVC.Application.Events;

public record OrderCreatedEvent(Guid OrderId, string CustomerEmail, decimal Total) : BaseEvent;
```

### Publish an Event

```csharp
using SmartWorkz.StarterKitMVC.Application.Events;

public class OrderService
{
    private readonly IEventPublisher _events;

    public OrderService(IEventPublisher events) => _events = events;

    public async Task CreateOrderAsync(Order order)
    {
        // ... create order logic ...

        // Publish event for other services
        await _events.PublishAsync(new OrderCreatedEvent(order.Id, order.CustomerEmail, order.Total));
    }
}
```

### Subscribe to an Event

```csharp
using SmartWorkz.StarterKitMVC.Application.Events;

public class EmailNotificationHandler
{
    private readonly IEventSubscriber _events;

    public EmailNotificationHandler(IEventSubscriber events)
    {
        _events = events;
        _events.SubscribeAsync<OrderCreatedEvent>(HandleOrderCreatedAsync);
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent @event, CancellationToken ct)
    {
        // Send confirmation email
        Console.WriteLine($"Sending email to {@event.CustomerEmail} for order {@event.OrderId}");
        await Task.CompletedTask;
    }
}
```

---

## Notification Hub

Send notifications via Email, SMS, or Push.

### Send a Notification

```csharp
using SmartWorkz.StarterKitMVC.Application.Notifications;

public class NotificationService
{
    private readonly INotificationRouter _notifications;

    public NotificationService(INotificationRouter notifications) => _notifications = notifications;

    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        var message = new NotificationMessage(
            Channel: NotificationChannel.Email,
            Recipient: email,
            Subject: "Welcome to SmartWorkz!",
            Body: $"Hello {name}, welcome to our platform!"
        );

        await _notifications.SendAsync(message);
    }

    public async Task SendSmsAsync(string phone, string text)
    {
        var message = new NotificationMessage(
            Channel: NotificationChannel.Sms,
            Recipient: phone,
            Subject: "",
            Body: text
        );

        await _notifications.SendAsync(message);
    }
}
```

---

## Feature Flags

Enable/disable features dynamically.

### Check a Feature Flag

```csharp
using SmartWorkz.StarterKitMVC.Application.Abstractions;

public class FeatureController : Controller
{
    private readonly IFeatureFlagService _features;

    public FeatureController(IFeatureFlagService features) => _features = features;

    public IActionResult NewDashboard()
    {
        if (_features.IsEnabled("new-dashboard"))
            return View("NewDashboard");

        return View("OldDashboard");
    }
}
```

---

## Background Jobs

Schedule background tasks.

### Enqueue a Job

```csharp
using SmartWorkz.StarterKitMVC.Application.Abstractions;

public class ReportService
{
    private readonly IBackgroundJobScheduler _jobs;

    public ReportService(IBackgroundJobScheduler jobs) => _jobs = jobs;

    public string GenerateReportAsync()
    {
        var jobId = _jobs.Enqueue(async ct =>
        {
            // Long-running report generation
            await Task.Delay(5000, ct);
            Console.WriteLine("Report generated!");
        }, "Generate Monthly Report");

        return jobId;
    }
}
```

---

## Local Storage

Store data locally (JSON files, SQLite-ready).

### Save and Load Data

```csharp
using SmartWorkz.StarterKitMVC.Infrastructure.Storage;

public class CacheService
{
    private readonly ILocalStorage _storage;

    public CacheService(ILocalStorage storage) => _storage = storage;

    public async Task SaveUserPreferencesAsync(string userId, object preferences)
    {
        var json = preferences.ToJson();
        await _storage.SaveAsync($"prefs_{userId}", json);
    }

    public async Task<T?> LoadUserPreferencesAsync<T>(string userId)
    {
        var json = await _storage.LoadAsync($"prefs_{userId}");
        return json?.FromJson<T>();
    }

    public async Task ClearUserPreferencesAsync(string userId)
    {
        await _storage.DeleteAsync($"prefs_{userId}");
    }
}
```

---

## Logging & Correlation

Use structured logging with correlation IDs.

### Inject Logger

```csharp
using SmartWorkz.StarterKitMVC.Infrastructure.Logging;

public class OrderService
{
    private readonly ILoggerAdapter<OrderService> _logger;

    public OrderService(ILoggerAdapter<OrderService> logger) => _logger = logger;

    public void ProcessOrder(Guid orderId)
    {
        _logger.LogInformation("Processing order {OrderId}", orderId);

        try
        {
            // ... process order ...
            _logger.LogInformation("Order {OrderId} processed successfully", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", orderId);
            throw;
        }
    }
}
```

### Access Correlation ID

```csharp
using SmartWorkz.StarterKitMVC.Shared.Primitives;

public class AuditService
{
    private readonly ICorrelationContext _correlation;

    public AuditService(ICorrelationContext correlation) => _correlation = correlation;

    public void LogAuditEvent(string action)
    {
        Console.WriteLine($"[{_correlation.CorrelationId}] Action: {action}");
    }
}
```

---

## Multi-Tenancy

Access tenant context in your services.

### Get Current Tenant

```csharp
using SmartWorkz.StarterKitMVC.Application.MultiTenancy;

public class TenantAwareService
{
    private readonly ITenantContext _tenant;

    public TenantAwareService(ITenantContext tenant) => _tenant = tenant;

    public void DoSomething()
    {
        var tenantId = _tenant.TenantId;
        Console.WriteLine($"Operating for tenant: {tenantId}");
    }
}
```

### Tenant-Specific Connection

```csharp
using SmartWorkz.StarterKitMVC.Application.MultiTenancy;

public class DatabaseService
{
    private readonly ITenantConnectionResolver _connections;

    public DatabaseService(ITenantConnectionResolver connections) => _connections = connections;

    public string GetConnectionString(string tenantId)
    {
        return _connections.GetConnectionString(tenantId);
    }
}
```

---

## Generating Documentation

The boilerplate includes a documentation generator that extracts XML comments into markdown.

### Run the Generator

```bash
# From solution root
dotnet run --project tools/DocGenerator -- --source src --output docs/api-reference.md
```

### Add Documentation to Your Code

```csharp
/// <summary>
/// Calculates the total price including tax.
/// </summary>
/// <param name="price">The base price.</param>
/// <param name="taxRate">The tax rate as a decimal (e.g., 0.1 for 10%).</param>
/// <returns>The total price with tax.</returns>
/// <example>
/// <code>
/// var total = CalculateTotal(100, 0.1); // Returns 110
/// </code>
/// </example>
public decimal CalculateTotal(decimal price, decimal taxRate)
{
    return price * (1 + taxRate);
}
```

The generator will automatically extract this into the API reference document.

---

## Need Help?

- Check `docs/architecture.md` for system design
- Check `docs/coding-standards.md` for coding guidelines
- Check `docs/savings-analysis.md` for ROI information

For issues, create a GitHub issue or contact the SmartWorkz team.
