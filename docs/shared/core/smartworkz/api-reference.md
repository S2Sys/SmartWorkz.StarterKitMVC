# SmartWorkz API Reference

## Classes & Interfaces

### ISmartWorkzClient

- **Namespace:** `SmartWorkz.ISmartWorkzClient`
- **Summary:** Main client for interacting with SmartWorkz APIs.
            
             Purpose: Provides strongly-typed access to SmartWorkz REST API endpoints
             including Authentication, Users, Transactions, Products, Reports, and Webhooks.Key Features:
             • Async/await APIs with CancellationToken support
             • Automatic API key or bearer token authentication
             • Built-in request/response logging
             • Typed DTOs for all endpoints
             • Exception handling with meaningful error messages
             Dependency Injection Usage:
             
             services.AddSmartWorkzClient(options =>
             {
                 options.BaseUrl = "https://api.smartworkz.com";
                 options.BearerToken = "your-jwt-token";  // or use ApiKey instead
                 options.Timeout = TimeSpan.FromSeconds(30);
             });
             Injected Service Usage:
             
             public class UserService
             {
                 private readonly ISmartWorkzClient _client;
            
                 public UserService(ISmartWorkzClient client) => _client = client;
            
                 public async Task<GetUserResponse> GetUserAsync(string userId)
                 {
                     return await _client.Users.GetAsync(userId);
                 }
             }
             Direct Instantiation:
             
             var httpClient = new HttpClient();
             var options = new SmartWorkzClientOptions
             {
                 BaseUrl = "https://api.smartworkz.com",
                 BearerToken = "your-jwt-token"
             };
             var client = new SmartWorkzClient(httpClient, options);
             var user = await client.Users.GetAsync("user-123");

### SmartWorkzClient

- **Namespace:** `SmartWorkz.SmartWorkzClient`
- **Summary:** Default implementation of .

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `httpClient`: The HTTP client instance.
    - `options`: Configuration options for the client.
    - `logger`: Optional logger for diagnostic information.

### SmartWorkzClientOptions

- **Namespace:** `SmartWorkz.SmartWorkzClientOptions`
- **Summary:** Configuration options for .

### ISmartWorkzClient

- **Namespace:** `SmartWorkz.ISmartWorkzClient`
- **Summary:** Main client for interacting with SmartWorkz APIs.
            
             Purpose: Provides strongly-typed access to SmartWorkz REST API endpoints
             including Authentication, Users, Transactions, Products, Reports, and Webhooks.Key Features:
             • Async/await APIs with CancellationToken support
             • Automatic API key or bearer token authentication
             • Built-in request/response logging
             • Typed DTOs for all endpoints
             • Exception handling with meaningful error messages
             Dependency Injection Usage:
             
             services.AddSmartWorkzClient(options =>
             {
                 options.BaseUrl = "https://api.smartworkz.com";
                 options.BearerToken = "your-jwt-token";  // or use ApiKey instead
                 options.Timeout = TimeSpan.FromSeconds(30);
             });
             Injected Service Usage:
             
             public class UserService
             {
                 private readonly ISmartWorkzClient _client;
            
                 public UserService(ISmartWorkzClient client) => _client = client;
            
                 public async Task<GetUserResponse> GetUserAsync(string userId)
                 {
                     return await _client.Users.GetAsync(userId);
                 }
             }
             Direct Instantiation:
             
             var httpClient = new HttpClient();
             var options = new SmartWorkzClientOptions
             {
                 BaseUrl = "https://api.smartworkz.com",
                 BearerToken = "your-jwt-token"
             };
             var client = new SmartWorkzClient(httpClient, options);
             var user = await client.Users.GetAsync("user-123");

### SmartWorkzClient

- **Namespace:** `SmartWorkz.SmartWorkzClient`
- **Summary:** Default implementation of .

#### Methods & Properties

- **#ctor** - Initializes a new instance of the  class.
  - Parameters:
    - `httpClient`: The HTTP client instance.
    - `options`: Configuration options for the client.
    - `logger`: Optional logger for diagnostic information.

### SmartWorkzClientOptions

- **Namespace:** `SmartWorkz.SmartWorkzClientOptions`
- **Summary:** Configuration options for .

