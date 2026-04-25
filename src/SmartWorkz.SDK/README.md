# SmartWorkz SDK for C#

Official C# SDK for SmartWorkz APIs. Provides strongly-typed async/await clients for Users, Transactions, Products, Reports, Webhooks, and Authentication endpoints.

## Features

- **Strongly-typed** API clients with full IntelliSense support
- **Async/await** with CancellationToken support throughout
- **Two authentication modes**: API Key and Bearer Token (JWT)
- **Comprehensive logging** for diagnostics
- **Pagination support** for list endpoints
- **Exception handling** with meaningful error messages
- **Dependency Injection** integration ready
- **Full XML documentation** for all public APIs

## Installation

```bash
dotnet add package SmartWorkz.SDK
```

Or via NuGet Package Manager:

```powershell
Install-Package SmartWorkz.SDK
```

## Quick Start

### Option 1: Dependency Injection

Register the client in your DI container (Program.cs):

```csharp
using SmartWorkz.Extensions;

builder.Services.AddSmartWorkzClient(options =>
{
    options.BaseUrl = "https://api.smartworkz.com";
    options.BearerToken = configuration["SmartWorkz:Token"];
});
```

Then inject and use:

```csharp
public class UserService
{
    private readonly ISmartWorkzClient _client;

    public UserService(ISmartWorkzClient client) => _client = client;

    public async Task<GetUserResponse> GetUserAsync(string userId)
    {
        return await _client.Users.GetAsync(userId);
    }
}
```

### Option 2: Direct Instantiation

```csharp
using SmartWorkz;

var httpClient = new HttpClient();
var options = new SmartWorkzClientOptions
{
    BaseUrl = "https://api.smartworkz.com",
    BearerToken = "your-jwt-token"
};

using var client = new SmartWorkzClient(httpClient, options);
var user = await client.Users.GetAsync("user-123");
```

## API Examples

### Authentication

```csharp
// Login
var loginResponse = await client.Authentication.LoginAsync(
    new LoginRequest("user@example.com", "password123"));

var token = loginResponse.Token;

// Register
var registerResponse = await client.Authentication.RegisterAsync(
    new RegisterRequest(
        Email: "newuser@example.com",
        FirstName: "John",
        LastName: "Doe",
        Password: "SecurePass123!"));

// Refresh Token
var refreshResponse = await client.Authentication.RefreshTokenAsync(
    new RefreshTokenRequest(currentRefreshToken));
```

### Users

```csharp
// List users with pagination
var listRequest = new ListUsersRequest(
    PageNumber: 1,
    PageSize: 25,
    SearchTerm: "john");
var result = await client.Users.ListAsync(listRequest);

Console.WriteLine($"Total: {result.TotalCount}");
foreach (var user in result.Items ?? new())
{
    Console.WriteLine($"- {user.FirstName} {user.LastName} ({user.Email})");
}

// Get user by ID
var user = await client.Users.GetAsync("user-123");

// Create user
var newUser = await client.Users.CreateAsync(
    new CreateUserRequest(
        Email: "alice@example.com",
        FirstName: "Alice",
        LastName: "Smith"));

// Update user
var updated = await client.Users.UpdateAsync(
    "user-123",
    new UpdateUserRequest(FirstName: "Jane"));

// Delete user
await client.Users.DeleteAsync("user-123");
```

### Transactions

```csharp
// List transactions
var transactions = await client.Transactions.ListAsync(
    new ListTransactionsRequest(
        PageNumber: 1,
        PageSize: 25,
        Status: "completed"));

// Create transaction
var transaction = await client.Transactions.CreateAsync(
    new CreateTransactionRequest(
        UserId: "user-123",
        Amount: 99.99m,
        Currency: "USD",
        Type: "payment",
        Description: "Monthly subscription"));

// Get transaction status
var status = await client.Transactions.GetStatusAsync("txn-456");
```

### Products

```csharp
// Search products
var products = await client.Products.ListAsync(
    new ListProductsRequest(
        SearchTerm: "laptop",
        MinPrice: 500m,
        MaxPrice: 2000m));

// Get product details
var product = await client.Products.GetAsync("prod-789");

// Create product
var newProduct = await client.Products.CreateAsync(
    new CreateProductRequest(
        Name: "USB-C Cable",
        Price: 29.99m,
        StockQuantity: 100));

// Update product
var updated = await client.Products.UpdateAsync(
    "prod-789",
    new UpdateProductRequest(
        Price: 34.99m,
        StockQuantity: 95));
```

### Reports

```csharp
// Generate report
var report = await client.Reports.GenerateAsync(
    new GenerateReportRequest(
        Title: "April 2026 Sales",
        Type: "sales",
        Format: "pdf",
        Filters: new Dictionary<string, object>
        {
            { "month", 4 },
            { "year", 2026 }
        }));

Console.WriteLine($"Report ID: {report.Id}");
Console.WriteLine($"Records: {report.RecordCount}");
Console.WriteLine($"Download: {report.DataUrl}");

// List reports
var reports = await client.Reports.ListAsync(
    new ListReportsRequest(Type: "sales"));
```

### Webhooks

```csharp
// Register webhook
var webhook = await client.Webhooks.CreateAsync(
    new CreateWebhookRequest(
        Url: "https://myapp.com/webhooks/smartworkz",
        Events: new List<string> 
        { 
            "user.created", 
            "transaction.completed" 
        },
        RetryAttempts: 3,
        TimeoutSeconds: 10));

// Test webhook
var testResult = await client.Webhooks.TestAsync(
    new TestWebhookRequest(
        WebhookId: webhook.Id,
        EventType: "user.created"));

// Update webhook
var updated = await client.Webhooks.UpdateAsync(
    webhook.Id,
    new UpdateWebhookRequest(
        Events: new List<string> 
        { 
            "user.created", 
            "user.deleted" 
        }));

// Delete webhook
await client.Webhooks.DeleteAsync(webhook.Id);
```

## Configuration

### With appsettings.json

```json
{
  "SmartWorkz": {
    "BaseUrl": "https://api.smartworkz.com",
    "BearerToken": "your-jwt-token",
    "Timeout": "00:00:30"
  }
}
```

Then in Program.cs:

```csharp
services.AddSmartWorkzClient(
    configuration.GetSection("SmartWorkz"));
```

### Authentication Methods

**API Key (Header-based):**
```csharp
options.ApiKey = "your-api-key";
```

**Bearer Token (JWT):**
```csharp
options.BearerToken = "your-jwt-token";
```

## Error Handling

All API methods use `EnsureSuccessStatusCode()` which throws `HttpRequestException` on failure:

```csharp
try
{
    var user = await client.Users.GetAsync("user-123");
}
catch (HttpRequestException ex)
{
    // Handle API errors
    Console.WriteLine($"API Error: {ex.Message}");
}
catch (ArgumentNullException ex)
{
    // Handle validation errors
    Console.WriteLine($"Validation Error: {ex.ParamName}");
}
```

## Logging

Enable detailed logging:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});
```

Or configure in appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "SmartWorkz": "Debug"
    }
  }
}
```

## Sample Application

See the `SmartWorkz.SDK.Sample` project for comprehensive usage examples.

```bash
cd src/SmartWorkz.SDK/Sample
dotnet run
```

## Project Structure

```
SmartWorkz.SDK/
├── SmartWorkz.SDK.csproj              # Main NuGet package
├── SmartWorkzClient.cs                # Core client interface and implementation
├── Clients/                           # Endpoint-specific clients
│   ├── AuthenticationClient.cs
│   ├── UsersClient.cs
│   ├── TransactionsClient.cs
│   ├── ProductsClient.cs
│   ├── ReportsClient.cs
│   └── WebhooksClient.cs
├── Models/
│   └── CommonModels.cs                # All DTOs and request/response models
├── Extensions/
│   └── ServiceCollectionExtensions.cs # DI registration
├── Generated/                          # Placeholder for code generation
└── Sample/                             # Sample console application
    ├── SmartWorkz.SDK.Sample.csproj
    ├── Program.cs
    └── appsettings.json
```

## Targets and Compatibility

- **.NET 8.0 and higher** (.NET 8, 9, etc.)
- **Windows, Linux, macOS** (all platforms supported by .NET 8+)
- **C# 11.0+**
- **Nullable reference types enabled**

## Contributing

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

## License

MIT License - see LICENSE file for details

## Support

For issues, questions, or feature requests:
- GitHub Issues: https://github.com/S2Sys/SmartWorkz/issues
- Email: support@smartworkz.com

## Version History

### 1.0.0 (Initial Release)
- Authentication endpoints (login, register, refresh, change password)
- Users API (list, get, create, update, delete)
- Transactions API (list, get, create, status)
- Products API (list, get, create, update, delete)
- Reports API (list, get, generate, delete)
- Webhooks API (list, get, create, update, delete, test)
- Full async/await support
- Comprehensive XML documentation
- Sample console application
