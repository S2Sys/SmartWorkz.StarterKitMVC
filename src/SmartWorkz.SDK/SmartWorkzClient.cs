using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using SmartWorkz.Clients;

namespace SmartWorkz;

/// <summary>
/// Main client for interacting with SmartWorkz APIs.
///
/// <para><strong>Purpose</strong>: Provides strongly-typed access to SmartWorkz REST API endpoints
/// including Authentication, Users, Transactions, Products, Reports, and Webhooks.</para>
///
/// <para><strong>Key Features</strong>:
/// • Async/await APIs with CancellationToken support
/// • Automatic API key or bearer token authentication
/// • Built-in request/response logging
/// • Typed DTOs for all endpoints
/// • Exception handling with meaningful error messages
/// </para>
///
/// <para><strong>Dependency Injection Usage</strong>:
/// <code>
/// services.AddSmartWorkzClient(options =>
/// {
///     options.BaseUrl = "https://api.smartworkz.com";
///     options.BearerToken = "your-jwt-token";  // or use ApiKey instead
///     options.Timeout = TimeSpan.FromSeconds(30);
/// });
/// </code>
/// </para>
///
/// <para><strong>Injected Service Usage</strong>:
/// <code>
/// public class UserService
/// {
///     private readonly ISmartWorkzClient _client;
///
///     public UserService(ISmartWorkzClient client) => _client = client;
///
///     public async Task&lt;GetUserResponse&gt; GetUserAsync(string userId)
///     {
///         return await _client.Users.GetAsync(userId);
///     }
/// }
/// </code>
/// </para>
///
/// <para><strong>Direct Instantiation</strong>:
/// <code>
/// var httpClient = new HttpClient();
/// var options = new SmartWorkzClientOptions
/// {
///     BaseUrl = "https://api.smartworkz.com",
///     BearerToken = "your-jwt-token"
/// };
/// var client = new SmartWorkzClient(httpClient, options);
/// var user = await client.Users.GetAsync("user-123");
/// </code>
/// </para>
/// </summary>
public interface ISmartWorkzClient : IDisposable
{
    /// <summary>
    /// Gets the Authentication API endpoint client.
    /// </summary>
    IAuthenticationClient Authentication { get; }

    /// <summary>
    /// Gets the Users API endpoint client.
    /// </summary>
    IUsersClient Users { get; }

    /// <summary>
    /// Gets the Transactions API endpoint client.
    /// </summary>
    ITransactionsClient Transactions { get; }

    /// <summary>
    /// Gets the Products API endpoint client.
    /// </summary>
    IProductsClient Products { get; }

    /// <summary>
    /// Gets the Reports API endpoint client.
    /// </summary>
    IReportsClient Reports { get; }

    /// <summary>
    /// Gets the Webhooks API endpoint client.
    /// </summary>
    IWebhooksClient Webhooks { get; }
}

/// <summary>
/// Default implementation of <see cref="ISmartWorkzClient"/>.
/// </summary>
public class SmartWorkzClient : ISmartWorkzClient
{
    private readonly HttpClient _httpClient;
    private readonly SmartWorkzClientOptions _options;
    private readonly ILogger<SmartWorkzClient>? _logger;
    private bool _disposed;

    public IAuthenticationClient Authentication { get; }
    public IUsersClient Users { get; }
    public ITransactionsClient Transactions { get; }
    public IProductsClient Products { get; }
    public IReportsClient Reports { get; }
    public IWebhooksClient Webhooks { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartWorkzClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance.</param>
    /// <param name="options">Configuration options for the client.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or options is null.</exception>
    public SmartWorkzClient(
        HttpClient httpClient,
        SmartWorkzClientOptions options,
        ILogger<SmartWorkzClient>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        // Set base address and default headers
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = _options.Timeout;

        // Add default headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SmartWorkz.SDK/1.0.0");

        // Configure authentication
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
            _logger?.LogDebug("SmartWorkzClient configured with API key authentication");
        }
        else if (!string.IsNullOrEmpty(_options.BearerToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.BearerToken);
            _logger?.LogDebug("SmartWorkzClient configured with bearer token authentication");
        }
        else
        {
            _logger?.LogWarning("SmartWorkzClient initialized without authentication credentials");
        }

        // Initialize endpoint clients (pass null logger - each client can have their own)
        Authentication = new AuthenticationClient(_httpClient, null);
        Users = new UsersClient(_httpClient, null);
        Transactions = new TransactionsClient(_httpClient, null);
        Products = new ProductsClient(_httpClient, null);
        Reports = new ReportsClient(_httpClient, null);
        Webhooks = new WebhooksClient(_httpClient, null);

        _logger?.LogInformation("SmartWorkzClient initialized with base URL: {BaseUrl}", _options.BaseUrl);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _httpClient?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Configuration options for <see cref="SmartWorkzClient"/>.
/// </summary>
public class SmartWorkzClientOptions
{
    /// <summary>
    /// Base URL of the SmartWorkz API (e.g., https://api.smartworkz.com).
    /// Default: https://api.smartworkz.com
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.smartworkz.com";

    /// <summary>
    /// API key for authentication. Use either ApiKey or BearerToken, not both.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Bearer token (JWT) for authentication. Use either ApiKey or BearerToken, not both.
    /// </summary>
    public string? BearerToken { get; set; }

    /// <summary>
    /// Request timeout. Default: 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
