# HTTP Client & WebSocket Guide

Complete reference for making HTTP requests and establishing real-time WebSocket connections in SmartWorkz applications.

---

## Overview

SmartWorkz provides two key infrastructure components for network communication:

1. **`IHttpClient`** — Typed HTTP client with fluent builder pattern, automatic retry logic, and `Result<T>` error handling
2. **`IWebSocketClient`** — Managed WebSocket connection with automatic frame handling and UTF-8 text messaging

---

## HTTP Client

### IHttpClient Interface

The `IHttpClient` abstraction provides four core methods for HTTP operations, all returning `Result<HttpResponse<T>>`:

```csharp
public interface IHttpClient
{
    Task<Result<HttpResponse<TResponse>>> GetAsync<TResponse>(string url);
    Task<Result<HttpResponse<TResponse>>> PostAsync<TResponse>(string url, object? body = null);
    Task<Result<HttpResponse<TResponse>>> PutAsync<TResponse>(string url, object? body = null);
    Task<Result<HttpResponse<TResponse>>> DeleteAsync<TResponse>(string url);
}
```

### HttpClientHelper — Fluent Builder

`HttpClientHelper` is the concrete implementation using a **fluent builder pattern**:

```csharp
var result = await httpClient
    .Get<UserDto>("https://api.example.com/users/123")
    .WithHeader("Authorization", "Bearer token123")
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithRetry(new RetryPolicy 
    { 
        MaxAttempts = 3,
        Strategy = RetryStrategy.Exponential,
        RetryableStatusCodes = 
        [
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable
        ]
    })
    .ExecuteAsync();

if (result.Succeeded)
{
    var user = result.Data.Data; // TResponse deserialized from JSON
    Console.WriteLine($"User: {user.Name}");
}
else
{
    Console.WriteLine($"Error: {result.Error.Message}");
}
```

### Methods

#### `Get<T>(url)` — Send GET Request

```csharp
var result = await httpClient
    .Get<Product>("https://api.example.com/products/42")
    .ExecuteAsync();
```

#### `Post<T>(url, body)` — Send POST Request

```csharp
var newProduct = new { Name = "Widget", Price = 29.99 };

var result = await httpClient
    .Post<Product>("https://api.example.com/products", newProduct)
    .ExecuteAsync();
```

#### `Put<T>(url, body)` — Send PUT Request

```csharp
var updateProduct = new { Name = "Updated Widget", Price = 39.99 };

var result = await httpClient
    .Put<Product>("https://api.example.com/products/42", updateProduct)
    .ExecuteAsync();
```

#### `Delete<T>(url)` — Send DELETE Request

```csharp
var result = await httpClient
    .Delete<EmptyResponse>("https://api.example.com/products/42")
    .ExecuteAsync();
```

### Fluent Builder Methods

#### `.WithHeader(name, value)` — Add Custom Header

```csharp
var result = await httpClient
    .Get<UserDto>("https://api.example.com/users/123")
    .WithHeader("Authorization", "Bearer eyJhbGc...")
    .WithHeader("X-Custom-Header", "custom-value")
    .ExecuteAsync();
```

#### `.WithHeaders(dictionary)` — Add Multiple Headers

```csharp
var headers = new Dictionary<string, string>
{
    ["Authorization"] = "Bearer eyJhbGc...",
    ["X-API-Key"] = "secret-key",
    ["X-Request-Id"] = Guid.NewGuid().ToString()
};

var result = await httpClient
    .Get<UserDto>("https://api.example.com/users/123")
    .WithHeaders(headers)
    .ExecuteAsync();
```

#### `.WithTimeout(timespan)` — Set Request Timeout

```csharp
var result = await httpClient
    .Get<SlowResponse>("https://api.example.com/slow-endpoint")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .ExecuteAsync();
```

#### `.WithRetry(policy)` — Configure Retry Logic

See [Retry Policy](#retry-policy) section below.

---

### Retry Policy

Control automatic retry behavior with `RetryPolicy`:

```csharp
public class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public int BackoffMilliseconds { get; set; } = 1000;
    public RetryStrategy Strategy { get; set; } = RetryStrategy.Linear;
    public List<HttpStatusCode> RetryableStatusCodes { get; set; } = new()
    {
        HttpStatusCode.RequestTimeout,           // 408
        HttpStatusCode.TooManyRequests,          // 429
        HttpStatusCode.InternalServerError,      // 500
        HttpStatusCode.BadGateway,               // 502
        HttpStatusCode.ServiceUnavailable,       // 503
        HttpStatusCode.GatewayTimeout            // 504
    };
}

public enum RetryStrategy
{
    Linear,         // 1s, 2s, 3s
    Exponential,    // 1s, 2s, 4s
    Fibonacci       // 1s, 1s, 2s
}
```

#### Linear Strategy

Increases delay by a fixed amount (default 1000ms) for each retry:

```csharp
// Attempt 1: immediate
// Attempt 2: wait 1s, retry
// Attempt 3: wait 2s, retry
// Attempt 4: wait 3s, retry

var result = await httpClient
    .Get<UserDto>("https://api.example.com/users/123")
    .WithRetry(new RetryPolicy
    {
        MaxAttempts = 4,
        BackoffMilliseconds = 1000,
        Strategy = RetryStrategy.Linear,
        RetryableStatusCodes = [HttpStatusCode.RequestTimeout, HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable]
    })
    .ExecuteAsync();
```

#### Exponential Strategy

Doubles the delay for each retry (good for backing off from congested APIs):

```csharp
// Attempt 1: immediate
// Attempt 2: wait 1s, retry
// Attempt 3: wait 2s, retry
// Attempt 4: wait 4s, retry

var result = await httpClient
    .Get<UserDto>("https://api.example.com/users/123")
    .WithRetry(new RetryPolicy
    {
        MaxAttempts = 4,
        BackoffMilliseconds = 1000,
        Strategy = RetryStrategy.Exponential,
        RetryableStatusCodes = [HttpStatusCode.RequestTimeout, HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable]
    })
    .ExecuteAsync();
```

#### Fibonacci Strategy

Increases delay using the Fibonacci sequence:

```csharp
// Attempt 1: immediate
// Attempt 2: wait 1s, retry
// Attempt 3: wait 1s, retry
// Attempt 4: wait 2s, retry

var result = await httpClient
    .Get<UserDto>("https://api.example.com/users/123")
    .WithRetry(new RetryPolicy
    {
        MaxAttempts = 4,
        BackoffMilliseconds = 1000,
        Strategy = RetryStrategy.Fibonacci,
        RetryableStatusCodes = [HttpStatusCode.RequestTimeout, HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable]
    })
    .ExecuteAsync();
```

#### Retryable Status Codes

Only retry on specific HTTP status codes (network timeouts always retry):

```csharp
new RetryPolicy
{
    MaxAttempts = 3,
    RetryableStatusCodes = [
        HttpStatusCode.RequestTimeout,      // 408
        HttpStatusCode.TooManyRequests,     // 429
        HttpStatusCode.InternalServerError, // 500
        HttpStatusCode.BadGateway,          // 502
        HttpStatusCode.ServiceUnavailable,  // 503
        HttpStatusCode.GatewayTimeout       // 504
    ]
}
```

---

### HttpRequest & HttpResponse<T>

#### HttpRequest

```csharp
public class HttpRequest
{
    public string Url { get; set; }
    public HttpMethod Method { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public TimeSpan? Timeout { get; set; }
    public RetryPolicy? RetryPolicy { get; set; }
}
```

#### HttpResponse<T>

```csharp
public class HttpResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public Error? Error { get; set; }
    public Dictionary<string, string>? ResponseHeaders { get; set; }
}
```

---

### DI Registration

Register `IHttpClient` in `Program.cs`:

```csharp
builder.Services.AddHttpClient<IHttpClient, HttpClientHelper>();
```

This automatically configures:
- `IHttpClientFactory` for connection pooling
- Automatic JSON serialization/deserialization
- Timeout handling

---

### Complete Examples

#### Simple GET Request

```csharp
private readonly IHttpClient _httpClient;

public MyService(IHttpClient httpClient)
{
    _httpClient = httpClient;
}

public async Task<Result<User>> GetUserAsync(int userId)
{
    var result = await _httpClient
        .Get<User>($"https://api.example.com/users/{userId}")
        .ExecuteAsync();
    
    return result;
}
```

#### POST with Error Handling

```csharp
public async Task<Result<Product>> CreateProductAsync(CreateProductRequest request)
{
    var result = await _httpClient
        .Post<Product>("https://api.example.com/products", request)
        .WithHeader("Authorization", _token)
        .WithTimeout(TimeSpan.FromSeconds(5))
        .ExecuteAsync();
    
    if (!result.Succeeded)
    {
        _logger.LogError("Failed to create product: {Error}", result.Error.Message);
        return Result.Fail<Product>(result.Error);
    }
    
    return result;
}
```

#### With Automatic Retry

```csharp
public async Task<Result<List<Order>>> GetOrdersWithRetryAsync()
{
    return await _httpClient
        .Get<List<Order>>("https://api.example.com/orders")
        .WithHeader("Authorization", _token)
        .WithRetry(new RetryPolicy
        {
            MaxAttempts = 3,
            BackoffMilliseconds = 500,
            Strategy = RetryStrategy.Exponential,
            RetryableStatusCodes = [HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable]
        })
        .ExecuteAsync();
}
```

#### PUT with Custom Headers

```csharp
public async Task<Result<User>> UpdateUserAsync(int userId, UpdateUserRequest request)
{
    var headers = new Dictionary<string, string>
    {
        ["Authorization"] = $"Bearer {_token}",
        ["X-Request-Id"] = Guid.NewGuid().ToString(),
        ["X-Correlation-Id"] = _correlationContext.CorrelationId
    };
    
    return await _httpClient
        .Put<User>($"https://api.example.com/users/{userId}", request)
        .WithHeaders(headers)
        .WithTimeout(TimeSpan.FromSeconds(10))
        .ExecuteAsync();
}
```

---

## WebSocket Client

### IWebSocketClient Interface

```csharp
public interface IWebSocketClient : IAsyncDisposable
{
    bool IsConnected { get; }
    
    Task ConnectAsync(string uri, CancellationToken cancellationToken = default);
    Task SendAsync(string message, CancellationToken cancellationToken = default);
    Task<string?> ReceiveAsync(CancellationToken cancellationToken = default);
    Task CloseAsync(CancellationToken cancellationToken = default);
}
```

### WebSocketClient Implementation

Manages `System.Net.WebSockets.ClientWebSocket` with automatic UTF-8 text frame handling.

```csharp
private readonly IWebSocketClient _ws;

public WebSocketClient()
{
    _ws = new WebSocketClient();
}
```

### Methods

#### `.ConnectAsync(uri)` — Establish WebSocket Connection

```csharp
var uri = "wss://api.example.com/notifications"; // wss = WebSocket Secure (TLS)
await _ws.ConnectAsync(uri);

if (_ws.IsConnected)
{
    Console.WriteLine("Connected!");
}
```

#### `.SendAsync(message)` — Send Text Message

```csharp
var message = new { type = "subscribe", channel = "orders" };
var json = JsonSerializer.Serialize(message);

await _ws.SendAsync(json);
```

#### `.ReceiveAsync()` — Receive Text Message

```csharp
while (_ws.IsConnected)
{
    var message = await _ws.ReceiveAsync();
    
    if (message != null)
    {
        Console.WriteLine($"Received: {message}");
        
        var notification = JsonSerializer.Deserialize<Notification>(message);
        // Handle notification...
    }
}
```

#### `.CloseAsync()` — Close Connection

```csharp
await _ws.CloseAsync();
```

---

### Complete WebSocket Example

**Real-time order notifications:**

```csharp
public class NotificationService
{
    private readonly IWebSocketClient _ws;
    private readonly ILogger<NotificationService> _logger;
    
    public NotificationService(IWebSocketClient ws, ILogger<NotificationService> logger)
    {
        _ws = ws;
        _logger = logger;
    }
    
    public async Task ListenForOrderUpdatesAsync(string userId, CancellationToken ct)
    {
        try
        {
            // Connect to WebSocket server
            await _ws.ConnectAsync("wss://api.example.com/orders/live");
            _logger.LogInformation("Connected to order updates");
            
            // Subscribe to user's orders
            var subscription = new
            {
                type = "subscribe",
                userId = userId,
                channel = "order_updates"
            };
            await _ws.SendAsync(JsonSerializer.Serialize(subscription));
            
            // Listen for messages
            while (_ws.IsConnected && !ct.IsCancellationRequested)
            {
                var message = await _ws.ReceiveAsync(ct);
                
                if (message != null)
                {
                    var update = JsonSerializer.Deserialize<OrderUpdate>(message);
                    
                    _logger.LogInformation("Order {OrderId} status changed to {Status}",
                        update?.OrderId, update?.Status);
                    
                    // Notify UI or trigger business logic
                    OnOrderUpdated?.Invoke(update);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Order updates listener cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listening for order updates");
        }
        finally
        {
            await _ws.CloseAsync();
        }
    }
    
    public event Action<OrderUpdate?>? OnOrderUpdated;
}
```

**Usage in a hosted service:**

```csharp
public class WebSocketBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebSocketBackgroundService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
        
        var userId = "user-123";
        await notificationService.ListenForOrderUpdatesAsync(userId, stoppingToken);
    }
}
```

---

## Error Handling

### HTTP Client Errors

```csharp
var result = await _httpClient
    .Get<User>("https://api.example.com/users/999")
    .ExecuteAsync();

if (!result.Succeeded)
{
    // Check error type
    if (result.Error?.Code == "NotFound")
    {
        _logger.LogWarning("User not found");
    }
    else if (result.Error?.Code == "Timeout")
    {
        _logger.LogWarning("Request timed out after retry exhaustion");
    }
    else
    {
        _logger.LogError("HTTP error: {Code} - {Message}", 
            result.Error?.Code, result.Error?.Message);
    }
}
```

### Network Errors

```csharp
try
{
    var result = await _httpClient
        .Get<User>("https://api.example.com/users/123")
        .ExecuteAsync();
}
catch (HttpRequestException ex)
{
    // Network error (DNS, connection refused, etc.)
    _logger.LogError(ex, "Network error");
}
catch (TaskCanceledException ex)
{
    // Timeout exceeded
    _logger.LogError(ex, "Request timeout");
}
catch (JsonException ex)
{
    // JSON deserialization error
    _logger.LogError(ex, "Invalid JSON response");
}
```

### WebSocket Errors

```csharp
try
{
    await _ws.ConnectAsync("wss://api.example.com/live");
}
catch (WebSocketException ex)
{
    // WebSocket-specific error
    _logger.LogError(ex, "WebSocket connection failed");
}
catch (InvalidOperationException ex)
{
    // Already connected, etc.
    _logger.LogError(ex, "Invalid WebSocket operation");
}
```

---

## Best Practices

### 1. Always Provide Timeout

```csharp
// ✓ Good: Explicit timeout
var result = await _httpClient
    .Get<User>("https://api.example.com/users/123")
    .WithTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync();

// ✗ Bad: Could hang indefinitely
var result = await _httpClient
    .Get<User>("https://api.example.com/users/123")
    .ExecuteAsync();
```

### 2. Use Retry for Transient Failures

```csharp
// ✓ Good: Retry on transient errors
.WithRetry(new RetryPolicy
{
    MaxAttempts = 3,
    Strategy = RetryStrategy.Exponential,
    RetryableStatusCodes = [HttpStatusCode.RequestTimeout, HttpStatusCode.TooManyRequests, HttpStatusCode.InternalServerError, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable]
})

// ✗ Bad: No retry for intermittent errors
// (single attempt fails on temporary network issues)
```

### 3. Use Result Pattern

```csharp
// ✓ Good: Check result before using data
var result = await _httpClient.Get<User>("...").ExecuteAsync();
if (result.Succeeded)
{
    var user = result.Data.Data;
}

// ✗ Bad: Assume success
var user = (await _httpClient.Get<User>("...").ExecuteAsync()).Data?.Data;
```

### 4. Handle WebSocket Reconnection

```csharp
var maxRetries = 5;
var retryCount = 0;

while (retryCount < maxRetries)
{
    try
    {
        await _ws.ConnectAsync("wss://api.example.com/live");
        retryCount = 0; // Reset on successful connection
        
        // Listen for messages
        while (_ws.IsConnected)
        {
            var message = await _ws.ReceiveAsync(ct);
            // Process message...
        }
    }
    catch (Exception ex)
    {
        retryCount++;
        _logger.LogWarning(ex, "WebSocket error, retry {Attempt}/{Max}", 
            retryCount, maxRetries);
        
        if (retryCount < maxRetries)
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), ct);
        }
    }
}
```

---

## Troubleshooting

### `InvalidOperationException: No service for type 'IHttpClient'`

**Solution:** Ensure DI registration in `Program.cs`:

```csharp
builder.Services.AddHttpClient<IHttpClient, HttpClientHelper>();
```

### WebSocket Connection Fails with "wss:// not supported"

**Solution:** Ensure the server certificate is valid or use `wss://` (WebSocket Secure) with self-signed certs:

```csharp
var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true; // Dev only!

var client = new HttpClient(handler);
```

---

## See Also

- [SmartWorkz Result Pattern](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md#result-pattern) — Error handling with Result<T>
- [Resilience Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md#resilience) — Circuit breaker and rate limiter patterns
- [Microsoft HttpClient Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
- [WebSocket RFC 6455](https://tools.ietf.org/html/rfc6455)
