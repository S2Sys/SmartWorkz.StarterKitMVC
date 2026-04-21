namespace SmartWorkz.Core.Mobile;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartWorkz.Core.Shared.Resilience;
using SmartWorkz.Core.Shared.Security;

/// <summary>
/// Extension methods for configuring SmartWorkz.Core.Mobile services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SmartWorkz.Core.Mobile services to the dependency injection container with Phase B configuration.
    ///
    /// Registers platform services, connectivity checks, storage, authentication, and synchronization capabilities.
    /// Supports configurable interceptors, analytics, and rate limiting.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureApi">Optional action to configure mobile API settings.</param>
    /// <param name="enableBuiltinInterceptors">Whether to register built-in interceptors (CorrelationInterceptor and DeviceInfoInterceptor). Default: true.</param>
    /// <param name="enableRealAnalytics">Whether to register BackendAnalyticsService. If false, registers stub AnalyticsService. Default: false.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddSmartWorkzCoreMobile(
        this IServiceCollection services,
        Action<MobileApiConfig>? configureApi = null,
        bool enableBuiltinInterceptors = true,
        bool enableRealAnalytics = false)
    {
        Guard.NotNull(services, nameof(services));

        // Register configuration using IOptions pattern
        var config = new MobileApiConfig { BaseUrl = "https://localhost" };
        configureApi?.Invoke(config);
        services.Configure<MobileApiConfig>(options =>
        {
            options.BaseUrl = config.BaseUrl;
            options.UserAgent = config.UserAgent;
            options.Timeout = config.Timeout;
            options.RetryCount = config.RetryCount;
            options.RetryStrategy = config.RetryStrategy;
            options.EnableCompression = config.EnableCompression;
        });

        // Register platform-neutral services
        services.AddScoped<IMobileContext, MobileContext>();
        services.AddSingleton<IRateLimiter, RateLimiter>(provider =>
            new RateLimiter(new RateLimiterOptions { MaxRequests = 100, WindowMilliseconds = 60000 }));
        services.AddScoped<ISecureStorageService, SecureStorageService>();
        services.AddScoped<IAuthenticationHandler, AuthenticationHandler>();

        // Register ApiClient with interceptor support
        services.AddScoped<IApiClient, ApiClient>();

        // Register built-in interceptors (if enabled)
        if (enableBuiltinInterceptors)
        {
            services.AddScoped<IRequestInterceptor, CorrelationInterceptor>();
            services.AddScoped<IRequestInterceptor, DeviceInfoInterceptor>();
        }

        // Register analytics service
        if (enableRealAnalytics)
        {
            services.AddSingleton<IAnalyticsService, BackendAnalyticsService>();
        }
        else
        {
            // Fallback to stub implementation if real analytics not enabled
            services.AddSingleton<IAnalyticsService, AnalyticsService>();
        }

        // Register push notifications
        services.AddScoped<IPushNotificationClientService, PushNotificationClientService>();

        // Register Lazy<IApiClient> to break circular dependencies
        services.AddScoped(provider => new Lazy<IApiClient>(() => provider.GetRequiredService<IApiClient>()));

        // Register JWT settings (if not already present)
        if (!services.Any(x => x.ServiceType == typeof(JwtSettings)))
        {
            services.AddSingleton<JwtSettings>(new JwtSettings
            {
                Secret = "SmartWorkz-Mobile-Default-Secret-Must-Be-Changed-In-Production-At-Least-32-Chars",
                Issuer = "SmartWorkz.Mobile",
                Audience = "SmartWorkz.Mobile.Users",
                ExpiryMinutes = 60,
                RefreshTokenExpiryDays = 7
            });
        }

        // Keep existing singleton services for backward compatibility
        services.AddSingleton<IConnectionChecker, ConnectionChecker>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<ILocalStorageService, LocalStorageService>();
        services.AddSingleton<IMobileService, MobileService>();

        // Keep existing scoped services for backward compatibility
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IOfflineService, OfflineService>();
        services.AddScoped<IBiometricService, BiometricService>();
        services.AddScoped<IErrorHandler, ErrorHandler>();

        return services;
    }
}
