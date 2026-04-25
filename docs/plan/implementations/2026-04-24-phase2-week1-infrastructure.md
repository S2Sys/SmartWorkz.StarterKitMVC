# SmartWorkz.Core.SLN Phase 2.0 Week 1: Infrastructure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement three critical infrastructure gaps (Swagger/OpenAPI, EF Core migrations, MassTransit consumers) to unblock API team, CI/CD pipeline, and async processing.

**Architecture:** 
- **Swagger**: Extension pattern with middleware registration in Program.cs, configuration-driven via FeatureOptions
- **EF Migrations**: One migration folder per DbContext (Auth, Master, Shared, Transaction, Report) with centralized MigrationManager service
- **MassTransit**: Consumer implementations for three key events (UserRegistered, OrderProcessed, PaymentCompleted) with transport abstraction (InMemory → RabbitMQ/AzureServiceBus)

**Tech Stack:** Swashbuckle.AspNetCore 6.10.0, EF Core 9.0, MassTransit 8.2.3, .NET 9.0

---

## File Structure Map

### Swagger/OpenAPI Files
- Create: `src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/SwaggerServiceExtension.cs`
- Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs` (add swagger registration)
- Modify: `appsettings.json` (add FeatureOptions:Swagger config)

### EF Core Migrations Files
- Create: `src/SmartWorkz.StarterKitMVC.Data/Migrations/Auth/` (folder structure)
- Create: `src/SmartWorkz.StarterKitMVC.Data/Migrations/Master/` (folder structure)
- Create: `src/SmartWorkz.StarterKitMVC.Data/Migrations/Shared/` (folder structure)
- Create: `src/SmartWorkz.StarterKitMVC.Data/Migrations/Transaction/` (folder structure)
- Create: `src/SmartWorkz.StarterKitMVC.Data/Migrations/Report/` (folder structure)
- Create: `src/SmartWorkz.StarterKitMVC.Data/Services/MigrationManager.cs`
- Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs` (add migration runner)

### MassTransit Consumer Files
- Create: `src/SmartWorkz.Core.Events/Events/UserRegisteredEvent.cs`
- Create: `src/SmartWorkz.Core.Events/Events/OrderProcessedEvent.cs`
- Create: `src/SmartWorkz.Core.Events/Events/PaymentCompletedEvent.cs`
- Create: `src/SmartWorkz.Core.Events/Consumers/SendWelcomeEmailConsumer.cs`
- Create: `src/SmartWorkz.Core.Events/Consumers/SendOrderConfirmationConsumer.cs`
- Create: `src/SmartWorkz.Core.Events/Consumers/PublishAnalyticsEventConsumer.cs`
- Create: `src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/MassTransitExtension.cs`
- Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs` (register consumers)
- Modify: `appsettings.json` (add MessageBroker config)

---

## Task 1: Swagger/OpenAPI Integration (1-2 days)

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/SwaggerServiceExtension.cs`
- Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs`
- Modify: `appsettings.json`

### Step 1: Install Swashbuckle NuGet Package

```bash
cd src/SmartWorkz.StarterKitMVC.Infrastructure
dotnet add package Swashbuckle.AspNetCore --version 6.10.0
```

Expected: Package installed successfully, `obj/project.assets.json` updated

### Step 2: Create SwaggerServiceExtension

Create file: `src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/SwaggerServiceExtension.cs`

```csharp
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using SmartWorkz.Infrastructure.Options;

namespace SmartWorkz.Infrastructure.Extensions;

public static class SwaggerServiceExtension
{
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var swaggerOptions = configuration
            .GetSection("FeatureOptions:Swagger")
            .Get<SwaggerFeatureOptions>()
            ?? new SwaggerFeatureOptions();

        if (!swaggerOptions.Enabled)
            return services;

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = swaggerOptions.Title ?? "SmartWorkz API",
                Version = swaggerOptions.Version ?? "v1",
                Description = swaggerOptions.Description ?? "Core API for SmartWorkz products",
                Contact = new OpenApiContact
                {
                    Name = swaggerOptions.ContactName,
                    Email = swaggerOptions.ContactEmail
                }
            });

            // Add JWT bearer authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using Bearer scheme",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference 
                        { 
                            Type = ReferenceType.SecurityScheme, 
                            Id = "Bearer" 
                        }
                    },
                    new string[] { }
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            // Filter out internal endpoints
            options.DocumentFilter<HiddenEndpointsFilter>();
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(
        this WebApplication app,
        IConfiguration configuration)
    {
        var swaggerOptions = configuration
            .GetSection("FeatureOptions:Swagger")
            .Get<SwaggerFeatureOptions>();

        if (swaggerOptions?.Enabled != true)
            return app;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartWorkz API v1");
            options.RoutePrefix = string.Empty;
            options.DocumentTitle = "SmartWorkz API Documentation";
        });

        return app;
    }
}

public class HiddenEndpointsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathsToRemove = swaggerDoc.Paths
            .Where(p => p.Key.Contains("/internal/") || p.Key.Contains("/health"))
            .ToList();

        foreach (var path in pathsToRemove)
            swaggerDoc.Paths.Remove(path.Key);
    }
}
```

### Step 3: Verify SwaggerFeatureOptions exists in FeatureOptions

Check: `src/SmartWorkz.StarterKitMVC.Infrastructure/Options/FeatureOptions.cs`

If SwaggerFeatureOptions class doesn't exist, add it:

```csharp
public class SwaggerFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Title { get; set; } = "SmartWorkz API";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = "Core API for SmartWorkz products";
    public string ContactName { get; set; } = "SmartWorkz Support";
    public string ContactEmail { get; set; } = "support@smartworkz.com";
}
```

### Step 4: Update Program.cs

Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs`

Add after `builder.Services.AddLogging()` and before `var app = builder.Build()`:

```csharp
// Add Swagger documentation
builder.Services.AddSwaggerDocumentation(builder.Configuration);
```

Add after `var app = builder.Build()`:

```csharp
// Use Swagger documentation
app.UseSwaggerDocumentation(app.Configuration);
```

### Step 5: Update appsettings.json

Modify: `appsettings.json`

Add or update the `FeatureOptions` section:

```json
{
  "FeatureOptions": {
    "Swagger": {
      "Enabled": true,
      "Title": "SmartWorkz API",
      "Version": "v1",
      "Description": "Core API for all SmartWorkz products",
      "ContactName": "SmartWorkz Support",
      "ContactEmail": "support@smartworkz.com"
    }
  }
}
```

### Step 6: Build project

```bash
cd src/SmartWorkz.StarterKitMVC.Infrastructure
dotnet build
```

Expected: Build succeeds with 0 errors

### Step 7: Run application and verify Swagger UI

```bash
cd src/SmartWorkz.StarterKitMVC.Infrastructure
dotnet run
```

Navigate to: `https://localhost:5001/swagger` (or your configured port)

Expected: Swagger UI loads with "SmartWorkz API v1" title and endpoints list

### Step 8: Add XML documentation to a sample controller

Add summary comments to any API controller:

```csharp
/// <summary>
/// Get user by ID
/// </summary>
/// <param name="userId">The user ID</param>
/// <returns>User details</returns>
/// <response code="200">User found</response>
/// <response code="404">User not found</response>
[HttpGet("{userId}")]
public async Task<IActionResult> GetUser(int userId)
{
    // Implementation
}
```

### Step 9: Verify Swagger shows documentation

Reload Swagger UI and confirm endpoint has description and response codes

Expected: Endpoint shows summary, parameter description, and response codes

### Step 10: Commit

```bash
git add src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/SwaggerServiceExtension.cs
git add src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs
git add appsettings.json
git commit -m "feat: add Swagger/OpenAPI documentation with Swashbuckle integration"
```

**Acceptance Criteria:**
- [ ] Swashbuckle.AspNetCore 6.10.0 installed
- [ ] SwaggerServiceExtension created with registration methods
- [ ] Swagger middleware wired in Program.cs
- [ ] FeatureOptions:Swagger config in appsettings.json
- [ ] Swagger UI accessible at `/swagger`
- [ ] JWT Bearer auth shown in Swagger UI
- [ ] Sample controller endpoints documented with XML comments
- [ ] Build succeeds with 0 errors

---

## Task 2: EF Core Migrations Setup (2-3 days)

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Data/Migrations/Auth/20260424_InitialCreate.cs` (and similar for each context)
- Create: `src/SmartWorkz.StarterKitMVC.Data/Services/MigrationManager.cs`
- Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs`

### Step 1: Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
```

Expected: dotnet-ef command available globally, package installed

### Step 2: Create Migration Directories

```bash
mkdir -p src/SmartWorkz.StarterKitMVC.Data/Migrations/Auth
mkdir -p src/SmartWorkz.StarterKitMVC.Data/Migrations/Master
mkdir -p src/SmartWorkz.StarterKitMVC.Data/Migrations/Shared
mkdir -p src/SmartWorkz.StarterKitMVC.Data/Migrations/Transaction
mkdir -p src/SmartWorkz.StarterKitMVC.Data/Migrations/Report
```

### Step 3: Create Initial Migrations

For each DbContext, create migration:

```bash
cd src/SmartWorkz.StarterKitMVC.Data

dotnet ef migrations add InitialCreate \
  -c AuthDbContext \
  -o Migrations/Auth

dotnet ef migrations add InitialCreate \
  -c MasterDbContext \
  -o Migrations/Master

dotnet ef migrations add InitialCreate \
  -c SharedDbContext \
  -o Migrations/Shared

dotnet ef migrations add InitialCreate \
  -c TransactionDbContext \
  -o Migrations/Transaction

dotnet ef migrations add InitialCreate \
  -c ReportDbContext \
  -o Migrations/Report
```

Expected: Migration files created in each `Migrations/[Context]/` folder

### Step 4: Verify Migration Files

Check that each context has:
- `20260424000000_InitialCreate.cs` (Up/Down methods)
- `AuthDbContextModelSnapshot.cs` (or equivalent for each context)

Expected: All 5 contexts have migration files

### Step 5: Create MigrationManager Service

Create file: `src/SmartWorkz.StarterKitMVC.Data/Services/MigrationManager.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartWorkz.Data.Context;

namespace SmartWorkz.Data.Services;

public interface IMigrationManager
{
    Task MigrateAsync();
    Task RollbackAsync(string migrationName);
    Task GetPendingMigrationsAsync();
}

public class MigrationManager : IMigrationManager
{
    private readonly AuthDbContext _authDb;
    private readonly MasterDbContext _masterDb;
    private readonly SharedDbContext _sharedDb;
    private readonly TransactionDbContext _transactionDb;
    private readonly ReportDbContext _reportDb;
    private readonly ILogger<MigrationManager> _logger;

    public MigrationManager(
        AuthDbContext authDb,
        MasterDbContext masterDb,
        SharedDbContext sharedDb,
        TransactionDbContext transactionDb,
        ReportDbContext reportDb,
        ILogger<MigrationManager> logger)
    {
        _authDb = authDb;
        _masterDb = masterDb;
        _sharedDb = sharedDb;
        _transactionDb = transactionDb;
        _reportDb = reportDb;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migrations...");

            await _authDb.Database.MigrateAsync();
            _logger.LogInformation("✓ AuthDbContext migrated");

            await _masterDb.Database.MigrateAsync();
            _logger.LogInformation("✓ MasterDbContext migrated");

            await _sharedDb.Database.MigrateAsync();
            _logger.LogInformation("✓ SharedDbContext migrated");

            await _transactionDb.Database.MigrateAsync();
            _logger.LogInformation("✓ TransactionDbContext migrated");

            await _reportDb.Database.MigrateAsync();
            _logger.LogInformation("✓ ReportDbContext migrated");

            _logger.LogInformation("Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed");
            throw;
        }
    }

    public async Task RollbackAsync(string migrationName)
    {
        try
        {
            _logger.LogWarning($"Rolling back to migration: {migrationName}");
            
            // For SQL Server - adjust for other databases
            await _authDb.Database.ExecuteSqlRawAsync(
                $"DELETE FROM [dbo].[__EFMigrationsHistory] WHERE MigrationId > '{migrationName}'");

            _logger.LogInformation("Rollback completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback failed");
            throw;
        }
    }

    public async Task GetPendingMigrationsAsync()
    {
        var authPending = await _authDb.Database.GetPendingMigrationsAsync();
        var masterPending = await _masterDb.Database.GetPendingMigrationsAsync();
        var sharedPending = await _sharedDb.Database.GetPendingMigrationsAsync();
        var transactionPending = await _transactionDb.Database.GetPendingMigrationsAsync();
        var reportPending = await _reportDb.Database.GetPendingMigrationsAsync();

        _logger.LogInformation($"Auth pending: {authPending.Count()}");
        _logger.LogInformation($"Master pending: {masterPending.Count()}");
        _logger.LogInformation($"Shared pending: {sharedPending.Count()}");
        _logger.LogInformation($"Transaction pending: {transactionPending.Count()}");
        _logger.LogInformation($"Report pending: {reportPending.Count()}");
    }
}
```

### Step 6: Register MigrationManager in DI

Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs`

Add after data service registrations:

```csharp
services.AddScoped<IMigrationManager, MigrationManager>();
```

### Step 7: Run migrations on startup

Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs`

Add after `var app = builder.Build()`:

```csharp
// Run database migrations
using (var scope = app.Services.CreateScope())
{
    try
    {
        var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();
        await migrationManager.MigrateAsync();
        _logger.LogInformation("✓ All database migrations completed successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "✗ Database migration failed - application will not start");
        throw;
    }
}
```

### Step 8: Build and verify

```bash
cd src/SmartWorkz.StarterKitMVC.Infrastructure
dotnet build
```

Expected: Build succeeds with 0 errors

### Step 9: Test migration on startup

```bash
dotnet run
```

Expected: Application logs show:
```
Starting database migrations...
✓ AuthDbContext migrated
✓ MasterDbContext migrated
✓ SharedDbContext migrated
✓ TransactionDbContext migrated
✓ ReportDbContext migrated
Database migrations completed successfully
```

### Step 10: Verify __EFMigrationsHistory table

Query database:

```sql
SELECT * FROM [dbo].[__EFMigrationsHistory]
```

Expected: 5 rows (one per context)

### Step 11: Commit

```bash
git add src/SmartWorkz.StarterKitMVC.Data/Migrations/
git add src/SmartWorkz.StarterKitMVC.Data/Services/MigrationManager.cs
git add src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs
git commit -m "feat: create EF Core migrations for all DbContexts with MigrationManager service"
```

**Acceptance Criteria:**
- [ ] EF Core tools installed globally
- [ ] Microsoft.EntityFrameworkCore.Design 9.0.0 added
- [ ] Migration files created for Auth, Master, Shared, Transaction, Report contexts
- [ ] MigrationManager service created and registered
- [ ] Migrations run automatically on application startup
- [ ] __EFMigrationsHistory table created with all migrations
- [ ] Build succeeds with 0 errors
- [ ] Application logs migration progress on startup

---

## Task 3: MassTransit Message Queue Consumers (3-4 days)

**Files:**
- Create: `src/SmartWorkz.Core.Events/Events/UserRegisteredEvent.cs`
- Create: `src/SmartWorkz.Core.Events/Events/OrderProcessedEvent.cs`
- Create: `src/SmartWorkz.Core.Events/Events/PaymentCompletedEvent.cs`
- Create: `src/SmartWorkz.Core.Events/Consumers/SendWelcomeEmailConsumer.cs`
- Create: `src/SmartWorkz.Core.Events/Consumers/SendOrderConfirmationConsumer.cs`
- Create: `src/SmartWorkz.Core.Events/Consumers/PublishAnalyticsEventConsumer.cs`
- Create: `src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/MassTransitExtension.cs`
- Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs`
- Modify: `appsettings.json`

### Step 1: Verify MassTransit is installed

```bash
dotnet list package --outdated | grep MassTransit
```

Expected: MassTransit.AspNetCore v8.2.3 (or compatible)

If not installed:

```bash
dotnet add package MassTransit.AspNetCore --version 8.2.3
```

### Step 2: Create Event Classes

Create file: `src/SmartWorkz.Core.Events/Events/UserRegisteredEvent.cs`

```csharp
namespace SmartWorkz.Core.Events;

/// <summary>
/// Published when a user registers for the platform
/// </summary>
public class UserRegisteredEvent
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
```

Create file: `src/SmartWorkz.Core.Events/Events/OrderProcessedEvent.cs`

```csharp
namespace SmartWorkz.Core.Events;

/// <summary>
/// Published when an order is processed
/// </summary>
public class OrderProcessedEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
```

Create file: `src/SmartWorkz.Core.Events/Events/PaymentCompletedEvent.cs`

```csharp
namespace SmartWorkz.Core.Events;

/// <summary>
/// Published when a payment is completed
/// </summary>
public class PaymentCompletedEvent
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string TransactionId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
```

### Step 3: Create Consumers

Create file: `src/SmartWorkz.Core.Events/Consumers/SendWelcomeEmailConsumer.cs`

```csharp
using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared.Communications;

namespace SmartWorkz.Core.Events.Consumers;

/// <summary>
/// Sends welcome email when user registers
/// </summary>
public class SendWelcomeEmailConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailSender _emailService;
    private readonly ILogger<SendWelcomeEmailConsumer> _logger;

    public SendWelcomeEmailConsumer(IEmailSender emailService, ILogger<SendWelcomeEmailConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        try
        {
            var message = context.Message;
            _logger.LogInformation($"Processing welcome email for user {message.UserId} ({message.Email})");

            var emailBody = $"""
                <h1>Welcome {message.FirstName}!</h1>
                <p>Thank you for registering with SmartWorkz.</p>
                <p>Your account is ready to use.</p>
                <p><a href="https://smartworkz.com/login">Login here</a></p>
                """;

            await _emailService.SendAsync(
                to: message.Email,
                subject: $"Welcome to SmartWorkz, {message.FirstName}!",
                body: emailBody,
                isHtml: true);

            _logger.LogInformation($"✓ Welcome email sent to {message.Email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send welcome email for user {context.Message.UserId}");
            throw;
        }
    }
}
```

Create file: `src/SmartWorkz.Core.Events/Consumers/SendOrderConfirmationConsumer.cs`

```csharp
using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared.Communications;
using SmartWorkz.Data.Repositories;

namespace SmartWorkz.Core.Events.Consumers;

/// <summary>
/// Sends order confirmation email when order is processed
/// </summary>
public class SendOrderConfirmationConsumer : IConsumer<OrderProcessedEvent>
{
    private readonly IEmailSender _emailService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SendOrderConfirmationConsumer> _logger;

    public SendOrderConfirmationConsumer(
        IEmailSender emailService,
        IUserRepository userRepository,
        ILogger<SendOrderConfirmationConsumer> logger)
    {
        _emailService = emailService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderProcessedEvent> context)
    {
        try
        {
            var message = context.Message;
            var user = await _userRepository.GetByIdAsync(message.UserId);

            if (user == null)
            {
                _logger.LogWarning($"User {message.UserId} not found for order {message.OrderId}");
                return;
            }

            var emailBody = $"""
                <h1>Order Confirmation #{message.OrderId}</h1>
                <p>Order Amount: ${message.Amount:N2}</p>
                <p>Status: Processing</p>
                <p>We will send you a shipping notification shortly.</p>
                <p><a href="https://smartworkz.com/orders/{message.OrderId}">View Order</a></p>
                """;

            await _emailService.SendAsync(
                to: user.Email,
                subject: $"Order Confirmation #{message.OrderId}",
                body: emailBody,
                isHtml: true);

            _logger.LogInformation($"✓ Order confirmation sent for order {message.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send order confirmation for order {context.Message.OrderId}");
            throw;
        }
    }
}
```

Create file: `src/SmartWorkz.Core.Events/Consumers/PublishAnalyticsEventConsumer.cs`

```csharp
using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared.Analytics;

namespace SmartWorkz.Core.Events.Consumers;

/// <summary>
/// Publishes analytics events when user registers
/// </summary>
public class PublishAnalyticsEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<PublishAnalyticsEventConsumer> _logger;

    public PublishAnalyticsEventConsumer(
        IAnalyticsService analyticsService,
        ILogger<PublishAnalyticsEventConsumer> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        try
        {
            var message = context.Message;
            await _analyticsService.TrackEventAsync("UserRegistered", new
            {
                UserId = message.UserId,
                Email = message.Email,
                RegisteredAt = message.RegisteredAt
            });

            _logger.LogInformation($"✓ Analytics event published for user {message.UserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish analytics event");
            // Don't throw - analytics should not block main flow
        }
    }
}
```

### Step 4: Create MassTransit Extension

Create file: `src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/MassTransitExtension.cs`

```csharp
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Events;
using SmartWorkz.Core.Events.Consumers;

namespace SmartWorkz.Infrastructure.Extensions;

public static class MassTransitExtension
{
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var transportType = configuration["MessageBroker:Type"] ?? "InMemory";

        services.AddMassTransit(x =>
        {
            // Register all consumers
            x.AddConsumer<SendWelcomeEmailConsumer>();
            x.AddConsumer<SendOrderConfirmationConsumer>();
            x.AddConsumer<PublishAnalyticsEventConsumer>();

            // Configure transport based on settings
            switch (transportType.ToLower())
            {
                case "rabbitmq":
                    ConfigureRabbitMq(x, configuration);
                    break;
                case "azureservicebus":
                    ConfigureAzureServiceBus(x, configuration);
                    break;
                default:
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                    break;
            }
        });

        return services;
    }

    private static void ConfigureRabbitMq(IBusRegistrationConfigurator x, IConfiguration configuration)
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var host = configuration["MessageBroker:RabbitMQ:Host"] ?? "localhost";
            var username = configuration["MessageBroker:RabbitMQ:Username"] ?? "guest";
            var password = configuration["MessageBroker:RabbitMQ:Password"] ?? "guest";

            cfg.Host(host, h =>
            {
                h.Username(username);
                h.Password(password);
            });

            cfg.ConfigureEndpoints(context);
        });
    }

    private static void ConfigureAzureServiceBus(IBusRegistrationConfigurator x, IConfiguration configuration)
    {
        x.UsingAzureServiceBus((context, cfg) =>
        {
            var connectionString = configuration.GetConnectionString("AzureServiceBus");
            cfg.Host(connectionString);
            cfg.ConfigureEndpoints(context);
        });
    }
}
```

### Step 5: Register consumers in Program.cs

Modify: `src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs`

Add after other service registrations:

```csharp
builder.Services.AddMassTransitMessaging(builder.Configuration);
```

### Step 6: Update appsettings.json

Modify: `appsettings.json`

Add:

```json
{
  "MessageBroker": {
    "Type": "InMemory",
    "RabbitMQ": {
      "Host": "localhost",
      "Username": "guest",
      "Password": "guest"
    }
  }
}
```

### Step 7: Create test consumer publisher

In a test controller or service, publish an event:

```csharp
private readonly IPublishEndpoint _publishEndpoint;

public async Task RegisterUserExample()
{
    await _publishEndpoint.Publish(new UserRegisteredEvent
    {
        UserId = 123,
        Email = "user@example.com",
        FirstName = "John",
        LastName = "Doe"
    });
}
```

### Step 8: Build and verify

```bash
cd src/SmartWorkz.StarterKitMVC.Infrastructure
dotnet build
```

Expected: Build succeeds with 0 errors

### Step 9: Run application and verify consumers start

```bash
dotnet run
```

Expected: Application logs show MassTransit bus starting with registered consumers

### Step 10: Publish test event

Create a test endpoint or use Swagger to publish event:

```csharp
[HttpPost("test-user-registration")]
public async Task TestUserRegistration([FromServices] IPublishEndpoint publishEndpoint)
{
    await publishEndpoint.Publish(new UserRegisteredEvent
    {
        UserId = 999,
        Email = "test@example.com",
        FirstName = "Test",
        LastName = "User"
    });
    
    return Ok("Event published");
}
```

Expected: Application logs show:
```
Processing welcome email for user 999 (test@example.com)
✓ Welcome email sent to test@example.com
✓ Analytics event published for user 999
```

### Step 11: Commit

```bash
git add src/SmartWorkz.Core.Events/
git add src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/MassTransitExtension.cs
git add src/SmartWorkz.StarterKitMVC.Infrastructure/Program.cs
git add appsettings.json
git commit -m "feat: implement MassTransit consumers for async event processing"
```

**Acceptance Criteria:**
- [ ] Event classes created (UserRegistered, OrderProcessed, PaymentCompleted)
- [ ] Consumer classes created (SendWelcomeEmail, SendOrderConfirmation, PublishAnalytics)
- [ ] MassTransitExtension created with transport abstraction
- [ ] Consumers registered in DI
- [ ] MessageBroker config in appsettings.json
- [ ] Application starts with MassTransit bus initialized
- [ ] Test event publishing works
- [ ] Consumers execute asynchronously without blocking
- [ ] Can switch transport to RabbitMQ via config
- [ ] Build succeeds with 0 errors

---

## Integration Testing Checklist

After completing all three tasks:

- [ ] Swagger UI loads at `/swagger`
- [ ] All API endpoints appear in Swagger documentation
- [ ] JWT Bearer auth is configurable in Swagger
- [ ] Application auto-runs migrations on startup
- [ ] Database migrations track in `__EFMigrationsHistory`
- [ ] MassTransit consumers process events without blocking API
- [ ] Events can be published from command handlers
- [ ] Error handling works for all three components
- [ ] Application builds in Release configuration
- [ ] Startup time < 5 seconds (all migrations + consumers)

---

## Rollback Procedures

### Swagger Rollback
- Remove `AddSwaggerDocumentation()` and `UseSwaggerDocumentation()` from Program.cs
- Delete `SwaggerServiceExtension.cs`
- Remove `FeatureOptions:Swagger` from appsettings.json
- Remove Swashbuckle.AspNetCore package
- Revert commit

### EF Migrations Rollback
- Remove migration files from `Migrations/` folders
- Remove `MigrationManager.cs` and DI registration
- Delete migration folders
- Run: `dotnet ef database update 0` for each context (removes all tables)
- Revert commit

### MassTransit Rollback
- Remove consumer classes from `Consumers/` folder
- Remove event classes from `Events/` folder
- Remove `AddMassTransitMessaging()` from Program.cs
- Delete `MassTransitExtension.cs`
- Remove `MessageBroker` from appsettings.json
- Revert commit

---

## Success Metrics

After Week 1 completion:

```
BEFORE          →  AFTER
No Swagger      →  Full API documentation ✓
Manual Deploy   →  Automated migrations ✓
No Consumers    →  Async event processing ✓
65% Ready       →  80% Production ready (+15%) ✓
```

---

