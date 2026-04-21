namespace SmartWorkz.Core.Mobile;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring SmartWorkz.Core.Mobile services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SmartWorkz.Core.Mobile services to the dependency injection container.
    ///
    /// Registers platform services, connectivity checks, storage, authentication, and synchronization capabilities.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureApi">Optional action to configure mobile API settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddSmartWorkzCoreMobile(
        this IServiceCollection services,
        Action<MobileApiConfig>? configureApi = null)
    {
        Guard.NotNull(services, nameof(services));

        // Create and register MobileApiConfig
        var mobileApiConfig = new MobileApiConfig { BaseUrl = "https://localhost" };
        configureApi?.Invoke(mobileApiConfig);
        services.AddSingleton(mobileApiConfig);

        // Configure HttpClient factory for mobile API
        services.AddHttpClient("MobileApiClient", client =>
        {
            client.BaseAddress = new Uri(mobileApiConfig.BaseUrl);
            client.Timeout = mobileApiConfig.Timeout;
            client.DefaultRequestHeaders.Add("User-Agent", mobileApiConfig.UserAgent);
        });

        // Register Singleton Services
        // These services are stateless or maintain platform-level state
        // - IConnectionChecker: Monitors network connectivity (platform event-driven)
        // - IPermissionService: Manages device permissions (platform integration)
        // - ILocalStorageService: Provides file-based storage (single instance shared)
        // - ISecureStorageService: Provides encrypted storage (single instance)
        // - IAnalyticsService: Collects telemetry (stateless, single track)
        // - IMobileService: Coordinates platform capabilities (stateless)
        // - IMobileContext: Provides app-wide context (single app instance)
        services.AddSingleton<IConnectionChecker, ConnectionChecker>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<ILocalStorageService, LocalStorageService>();
        services.AddSingleton<ISecureStorageService, SecureStorageService>();
        services.AddSingleton<IAnalyticsService, BackendAnalyticsService>();
        services.AddSingleton<IMobileService, MobileService>();
        services.AddSingleton<IMobileContext, MobileContext>();

        // Register Scoped Services
        // These services have per-page/screen or per-request lifetime
        // - IAuthenticationHandler: Manages auth state per user session
        // - ISyncService: Handles data synchronization per operation
        // - IOfflineService: Manages offline behavior per context
        // - IBiometricService: Handles biometric verification per request
        // - IErrorHandler: Processes errors per request context
        // - IApiClient: Makes HTTP requests with scoped client instance
        // - IPushNotificationClientService: Handles push notification registration/unregistration
        services.AddScoped<IAuthenticationHandler, AuthenticationHandler>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IOfflineService, OfflineService>();
        services.AddScoped<IBiometricService, BiometricService>();
        services.AddScoped<IErrorHandler, ErrorHandler>();
        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<IPushNotificationClientService, PushNotificationClientService>();
        services.AddScoped(provider => new Lazy<IApiClient>(() => provider.GetRequiredService<IApiClient>()));

        // NOTE: IRequestInterceptor is NOT registered by default
        // Applications should implement custom interceptors and register them:
        //
        //   var customInterceptor = new AuthenticationInterceptor(authService);
        //   services.AddScoped<IRequestInterceptor>(_ => customInterceptor);
        //
        // Or use a factory for dependency injection:
        //
        //   services.AddScoped<IRequestInterceptor, CustomRequestInterceptor>();
        //
        // The ApiClient will use any registered IRequestInterceptor if available

        return services;
    }
}
