using Microsoft.Extensions.DependencyInjection;

namespace SmartWorkz.Extensions;

/// <summary>
/// Dependency injection extensions for SmartWorkz SDK.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register SmartWorkz API client in the DI container with typed HTTP client.
    ///
    /// <para><strong>Usage in Program.cs</strong>:
    /// <code>
    /// // Option 1: With inline configuration
    /// services.AddSmartWorkzClient(options =>
    /// {
    ///     options.BaseUrl = "https://api.smartworkz.com";
    ///     options.BearerToken = configuration["SmartWorkz:Token"];
    /// });
    ///
    /// // Option 2: With configuration section
    /// services.AddSmartWorkzClient(
    ///     configuration.GetSection("SmartWorkz"));
    /// </code>
    /// </para>
    ///
    /// <para><strong>Usage in Service</strong>:
    /// <code>
    /// public class OrderService
    /// {
    ///     private readonly ISmartWorkzClient _client;
    ///
    ///     public OrderService(ISmartWorkzClient client)
    ///     {
    ///         _client = client;
    ///     }
    ///
    ///     public async Task ProcessOrderAsync(string orderId)
    ///     {
    ///         var order = await _client.Transactions.GetAsync(orderId);
    ///         // Process order...
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Configuration action for SmartWorkzClientOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSmartWorkzClient(
        this IServiceCollection services,
        Action<SmartWorkzClientOptions>? configureOptions = null)
    {
        var options = new SmartWorkzClientOptions();
        configureOptions?.Invoke(options);

        services.AddHttpClient<ISmartWorkzClient, SmartWorkzClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = options.Timeout;

                // Add User-Agent header
                client.DefaultRequestHeaders.Add("User-Agent", "SmartWorkz.SDK/1.0.0");

                // Configure authentication headers
                if (!string.IsNullOrEmpty(options.ApiKey))
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey);
                }
                else if (!string.IsNullOrEmpty(options.BearerToken))
                {
                    var scheme = options.BearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? string.Empty
                        : "Bearer ";
                    client.DefaultRequestHeaders.Add(
                        "Authorization",
                        $"{scheme}{options.BearerToken}");
                }
            });

        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Register SmartWorkz API client from configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration section containing SmartWorkz settings.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Expected configuration structure:
    /// <code>
    /// {
    ///   "SmartWorkz": {
    ///     "BaseUrl": "https://api.smartworkz.com",
    ///     "BearerToken": "your-jwt-token",
    ///     "Timeout": "00:00:30"
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public static IServiceCollection AddSmartWorkzClient(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfigurationSection configuration)
    {
        var baseUrl = configuration["BaseUrl"] ?? "https://api.smartworkz.com";
        var apiKey = configuration["ApiKey"];
        var bearerToken = configuration["BearerToken"];
        var timeoutSeconds = int.TryParse(configuration["TimeoutSeconds"], out var seconds) ? seconds : 30;

        return services.AddSmartWorkzClient(opts =>
        {
            opts.BaseUrl = baseUrl;
            opts.ApiKey = apiKey;
            opts.BearerToken = bearerToken;
            opts.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });
    }
}
