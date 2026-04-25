# Extensions API Reference

## Classes & Interfaces

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Extensions.ServiceCollectionExtensions`
- **Summary:** Dependency injection extensions for SmartWorkz SDK.

#### Methods & Properties

- **AddSmartWorkzClient** - Register SmartWorkz API client in the DI container with typed HTTP client.
            
             Usage in Program.cs:
             
             // Option 1: With inline configuration
             services.AddSmartWorkzClient(options =>
             {
                 options.BaseUrl = "https://api.smartworkz.com";
                 options.BearerToken = configuration["SmartWorkz:Token"];
             });
            
             // Option 2: With configuration section
             services.AddSmartWorkzClient(
                 configuration.GetSection("SmartWorkz"));
             Usage in Service:
             
             public class OrderService
             {
                 private readonly ISmartWorkzClient _client;
            
                 public OrderService(ISmartWorkzClient client)
                 {
                     _client = client;
                 }
            
                 public async Task ProcessOrderAsync(string orderId)
                 {
                     var order = await _client.Transactions.GetAsync(orderId);
                     // Process order...
                 }
             }
  - Parameters:
    - `services`: The service collection.
    - `configureOptions`: Configuration action for SmartWorkzClientOptions.
  - Returns: The service collection for chaining.
- **AddSmartWorkzClient** - Register SmartWorkz API client from configuration section.
  - Parameters:
    - `services`: The service collection.
    - `configuration`: Configuration section containing SmartWorkz settings.
  - Returns: The service collection for chaining.

### ServiceCollectionExtensions

- **Namespace:** `SmartWorkz.Extensions.ServiceCollectionExtensions`
- **Summary:** Dependency injection extensions for SmartWorkz SDK.

#### Methods & Properties

- **AddSmartWorkzClient** - Register SmartWorkz API client in the DI container with typed HTTP client.
            
             Usage in Program.cs:
             
             // Option 1: With inline configuration
             services.AddSmartWorkzClient(options =>
             {
                 options.BaseUrl = "https://api.smartworkz.com";
                 options.BearerToken = configuration["SmartWorkz:Token"];
             });
            
             // Option 2: With configuration section
             services.AddSmartWorkzClient(
                 configuration.GetSection("SmartWorkz"));
             Usage in Service:
             
             public class OrderService
             {
                 private readonly ISmartWorkzClient _client;
            
                 public OrderService(ISmartWorkzClient client)
                 {
                     _client = client;
                 }
            
                 public async Task ProcessOrderAsync(string orderId)
                 {
                     var order = await _client.Transactions.GetAsync(orderId);
                     // Process order...
                 }
             }
  - Parameters:
    - `services`: The service collection.
    - `configureOptions`: Configuration action for SmartWorkzClientOptions.
  - Returns: The service collection for chaining.
- **AddSmartWorkzClient** - Register SmartWorkz API client from configuration section.
  - Parameters:
    - `services`: The service collection.
    - `configuration`: Configuration section containing SmartWorkz settings.
  - Returns: The service collection for chaining.

