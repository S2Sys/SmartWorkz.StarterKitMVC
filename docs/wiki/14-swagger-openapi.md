# Swagger/OpenAPI Documentation

## Overview

The `SwaggerServiceExtension` provides automatic API documentation through Swagger/OpenAPI, making your REST endpoints discoverable and testable without manual documentation. It integrates JWT Bearer authentication, automatically hides internal/health endpoints, and includes XML doc comment support for rich endpoint descriptions. Use Swagger when you want to expose interactive API documentation to frontend teams, API consumers, or for internal testing of API contracts.

---

## Architecture

The Swagger implementation follows a **two-call pattern**: initialization in the dependency injection container followed by middleware registration in the application pipeline.

```
┌─────────────────────────────────────────────────────────────┐
│ Program.cs Startup                                          │
└─────────────────────────────────────────────────────────────┘
                          │
                          ├─ builder.Services.AddSwaggerDocumentation(configuration)
                          │  - Checks Features:Swagger:Enabled
                          │  - Registers SwaggerGen with SwaggerDoc("v1", ...)
                          │  - Adds Bearer security scheme (JWT)
                          │  - Includes XML comments (if .xml file exists)
                          │  - Applies HiddenEndpointsFilter
                          │
                          └─ app.UseSwaggerDocumentation(configuration)
                             - Checks Features:Swagger:Enabled (second guard)
                             - Registers Swagger middleware
                             - Serves UI at /api-docs
                             - Exposes JSON at /swagger/v1/swagger.json

┌─────────────────────────────────────────────────────────────┐
│ Runtime Behavior                                            │
├─────────────────────────────────────────────────────────────┤
│ Client: GET /api-docs → Returns Swagger UI (HTML/JS/CSS)   │
│ Client: GET /swagger/v1/swagger.json → Returns OpenAPI spec│
│ Client: Interact with Swagger UI to test endpoints         │
└─────────────────────────────────────────────────────────────┘
```

Both extension methods check the `Enabled` flag independently. While it's common to wrap the middleware call in an `if (app.Environment.IsDevelopment())` guard in `Program.cs`, the `UseSwaggerDocumentation()` method performs its own check. This double-guard is harmless and provides flexibility for non-development environments.

---

## Quick Start

Get Swagger documentation running in 5 lines:

```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddSwaggerDocumentation(builder.Configuration);

var app = builder.Build();
app.UseSwaggerDocumentation(app.Configuration);
app.Run();
```

Navigate to `http://localhost:5000/api-docs` to view the Swagger UI.

---

## Configuration

Configure Swagger behavior via `appsettings.json` under the `Features:Swagger` section:

| Key | Type | Default | Example |
|-----|------|---------|---------|
| `Enabled` | `bool` | `true` | `true` or `false` |
| `Title` | `string` | `SmartWorkz API` | `"My API"` |
| `Version` | `string` | `v1` | `"v2"` or `"2024.04.01"` |
| `Description` | `string` | `Core API for SmartWorkz products` | `"Public REST API for partners"` |
| `ContactName` | `string` | `SmartWorkz Support` | `"API Support Team"` |
| `ContactEmail` | `string` | `support@smartworkz.com` | `"api-team@example.com"` |

### appsettings.json Example

```json
{
  "Features": {
    "Swagger": {
      "Enabled": true,
      "Title": "SmartWorkz Core API",
      "Version": "v1",
      "Description": "REST API for SmartWorkz tenant management, users, and operations",
      "ContactName": "SmartWorkz API Support",
      "ContactEmail": "api-support@smartworkz.com"
    }
  }
}
```

### Environment-Specific Settings

Disable Swagger in production by overriding `appsettings.Production.json`:

```json
{
  "Features": {
    "Swagger": {
      "Enabled": false
    }
  }
}
```

---

## Usage Examples

### Sample 1: Basic Setup with JWT Bearer Authentication

Integrate Swagger into `Program.cs` with automatic JWT authentication:

```csharp
using SmartWorkz.StarterKitMVC.Infrastructure.Extensions;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add core services
builder.Services.AddControllers();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });

// Register Swagger with all features
builder.Services.AddSwaggerDocumentation(builder.Configuration);

var app = builder.Build();

// Use Swagger middleware (guards on Enabled config)
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Features:Swagger:Enabled"))
{
    app.UseSwaggerDocumentation(app.Configuration);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Sample 2: Adding XML Doc Comments to Controllers

Enable rich descriptions in Swagger by adding XML comments to controllers and methods:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Features.Users
{
    /// <summary>
    /// Manages user accounts and profiles
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves a user by ID
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>The user profile</returns>
        /// <remarks>
        /// Only accessible to authenticated users.
        /// 
        /// Example request:
        /// 
        ///     GET /api/users/123
        /// </remarks>
        /// <response code="200">User found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("{id}")]
        [ProduceResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProduceResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserAsync(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            
            if (result.IsFailure)
                return NotFound();

            return Ok(new UserDto 
            { 
                Id = result.Value.Id,
                Email = result.Value.Email,
                Name = result.Value.Name
            });
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="request">User creation request</param>
        /// <returns>The newly created user</returns>
        /// <response code="201">User created successfully</response>
        /// <response code="400">Invalid request</response>
        [HttpPost]
        [ProduceResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProduceResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> CreateUserAsync([FromBody] CreateUserRequest request)
        {
            var result = await _userService.CreateUserAsync(request);
            
            if (result.IsFailure)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetUserAsync), new { id = result.Value.Id }, result.Value);
        }
    }

    /// <summary>
    /// Request model for creating a user
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User full name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User password (minimum 8 characters)
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Response model for user data
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
```

Generate XML comments by enabling in your `.csproj`:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

### Sample 3: Enabling Swagger in Production (with Security Considerations)

For public or partner APIs, enable Swagger in production but protect it:

```csharp
using Microsoft.AspNetCore.Authorization;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation(builder.Configuration);

var app = builder.Build();

// Enable Swagger based on configuration
if (builder.Configuration.GetValue<bool>("Features:Swagger:Enabled"))
{
    // SECURITY: Add basic authentication to Swagger UI in production
    if (!app.Environment.IsDevelopment())
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartWorkz API v1");
            options.RoutePrefix = "api-docs";
            // Optionally add Swagger-specific authorization middleware here
        });
        
        // Consider adding a custom middleware to require API key for /api-docs access
        app.UseWhen(
            context => context.Request.Path.StartsWithSegments("/api-docs"),
            appBuilder => appBuilder.Use(async (httpContext, next) =>
            {
                var apiKey = httpContext.Request.Headers["X-API-Key"].ToString();
                if (string.IsNullOrEmpty(apiKey) || apiKey != builder.Configuration["Security:ApiDocKey"])
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("Unauthorized");
                    return;
                }
                await next();
            })
        );
    }
    else
    {
        app.UseSwaggerDocumentation(app.Configuration);
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## API Reference

### SwaggerServiceExtension Methods

#### AddSwaggerDocumentation

```csharp
public static IServiceCollection AddSwaggerDocumentation(
    this IServiceCollection services,
    IConfiguration configuration)
```

**Purpose**: Registers Swagger/OpenAPI document generation services.

**Behavior**:
- Reads `Features:Swagger` configuration section
- Returns services unchanged if `Enabled` is `false`
- Registers SwaggerGen with OpenApiInfo (title, version, description, contact)
- Adds JWT Bearer security definition and requirement
- Includes XML doc comments from entry assembly if available
- Applies `HiddenEndpointsFilter` to hide `/internal/*` and `/health` endpoints

**Returns**: The `IServiceCollection` for method chaining.

---

#### UseSwaggerDocumentation

```csharp
public static WebApplication UseSwaggerDocumentation(
    this WebApplication app,
    IConfiguration configuration)
```

**Purpose**: Adds Swagger middleware and serves the Swagger UI.

**Behavior**:
- Reads `Features:Swagger` configuration section
- Returns app unchanged if `Enabled` is `false` or not specified
- Registers Swagger middleware (serves JSON schema at `/swagger/v1/swagger.json`)
- Registers SwaggerUI middleware (serves interactive UI at `/api-docs`)
- Sets document title to "SmartWorkz API Documentation"

**Returns**: The `WebApplication` for method chaining.

---

### HiddenEndpointsFilter Class

```csharp
public class HiddenEndpointsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
}
```

**Purpose**: Removes internal and health check endpoints from the Swagger document.

**Behavior**:
- Filters out all paths containing `/internal/` (e.g., `/internal/metrics`, `/internal/debug`)
- Filters out all paths containing `/health` (e.g., `/health`, `/health/live`, `/health/ready`)
- Applied automatically by `AddSwaggerDocumentation()`

**Example excluded paths**:
```
/internal/metrics          (removed)
/internal/debug/logs       (removed)
/health                    (removed)
/health/live               (removed)
/health/ready              (removed)
/api/users                 (included)
```

---

## Integration Notes

### Related Wiki Pages

- **[Result Pattern](./04-result-pattern.md)** — Document `Result<T>` responses in Swagger using `ProduceResponseType`
- **[Multi-Tenant Architecture](./11-multi-tenant-architecture.md)** — Consider tenant ID in API design and documentation
- **[Cache Attribute](./12-cache-attribute.md)** — Document cache behavior for GET endpoints
- **[Template Engine](./13-template-engine.md)** — For rendering dynamic API documentation

### Integration with Authentication

Swagger automatically includes JWT Bearer authentication:

1. Login endpoint generates JWT token
2. User copies token from login response
3. User clicks "Authorize" button in Swagger UI
4. Enters token in popup (with "Bearer " prefix)
5. All subsequent requests include `Authorization: Bearer <token>` header

### Integration with Logging

Log Swagger configuration at startup:

```csharp
if (builder.Configuration.GetValue<bool>("Features:Swagger:Enabled"))
{
    _logger.Information("Swagger documentation enabled at /api-docs");
}
```

### Combining with Custom Security

For APIs with custom security requirements:

```csharp
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api-docs") && app.Environment.IsProduction(),
    appBuilder => appBuilder.UseMiddleware<ApiDocumentationSecurityMiddleware>()
);
app.UseSwaggerDocumentation(app.Configuration);
```

---

## Troubleshooting

### Issue 1: Swagger UI Shows Empty Endpoints

**Symptom**: Navigate to `/api-docs` and see no endpoints listed.

**Common Causes**:
- Controllers are not decorated with `[ApiController]` attribute
- Actions don't have `[HttpGet]`, `[HttpPost]`, etc. attributes
- `AddControllers()` or `AddMvc()` not called in dependency injection

**Fix**:

```csharp
// Program.cs
builder.Services.AddControllers();  // Required!
builder.Services.AddSwaggerDocumentation(builder.Configuration);
```

**Controller Requirements**:

```csharp
[ApiController]                              // Required!
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]                        // Required!
    public ActionResult<Product> GetProduct(int id) { ... }
}
```

---

### Issue 2: Bearer Token Not Working in Swagger UI

**Symptom**: Click "Authorize" in Swagger UI, enter token, but requests still fail with 401.

**Common Causes**:
- Token not prefixed with "Bearer " in the authorization header
- Token is expired or invalid
- Authentication middleware not configured

**Fix**:

1. Ensure authentication is registered:

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });
```

2. Ensure middleware is added:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

3. In Swagger UI, enter token with the prefix format expected by your authentication scheme. The UI automatically adds "Bearer " if configured correctly.

---

### Issue 3: Double Guard on `Enabled` Config

**Symptom**: Both `Program.cs` guard and `UseSwaggerDocumentation()` check the `Enabled` flag.

**Explanation**: This is intentional design. The `UseSwaggerDocumentation()` method performs an independent check, allowing the middleware registration to be controlled by configuration alone. An external `if (app.Environment.IsDevelopment())` guard is optional and provides an additional safety layer.

**Pattern** (both guards are fine):

```csharp
// Safe: Configuration guard inside middleware method
app.UseSwaggerDocumentation(app.Configuration);

// Also safe: Extra environment guard outside (redundant but harmless)
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation(app.Configuration);
}

// Not recommended: Conflicting guards
if (app.Environment.IsDevelopment())
{
    // This only works if BOTH Enabled=true AND IsDevelopment=true
    app.UseSwaggerDocumentation(app.Configuration);
}
```

---

### Issue 4: XML Comments Not Appearing in Swagger

**Symptom**: Added `///` doc comments to controller methods, but they don't show in Swagger UI.

**Common Causes**:
- `GenerateDocumentationFile` not enabled in `.csproj`
- XML file not generated in build output
- `HiddenEndpointsFilter` hiding the endpoint

**Fix**:

1. Enable XML documentation in your `.csproj`:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

2. Verify `.xml` file exists in build output:

```bash
# After building, check bin/Debug/net8.0/
ls bin/Debug/net8.0/MyApp.xml
```

3. Confirm controller is not hidden:

```csharp
// Not on /internal/ path
[Route("api/users")]  // ✓ Will appear in Swagger
public class UsersController { }

[Route("api/internal/debug")]  // ✗ Will be filtered out
public class DebugController { }
```

---

## Summary

- **Two-call pattern**: `AddSwaggerDocumentation()` (services) → `UseSwaggerDocumentation()` (middleware)
- **Configuration**: Control via `Features:Swagger` section in `appsettings.json`
- **Security**: JWT Bearer auth integrated automatically; disable Swagger in production by setting `Enabled: false`
- **Documentation**: Add XML comments to controllers and actions; ensure `GenerateDocumentationFile=true` in `.csproj`
- **Hidden endpoints**: `/internal/*` and `/health` paths automatically excluded from Swagger
