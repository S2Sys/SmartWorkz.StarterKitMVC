namespace SmartWorkz.Mobile;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartWorkz.Mobile.Services;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Shared;

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

        // Step 1: Register configuration using simplified IOptions pattern
        services.Configure<MobileApiConfig>(options =>
        {
            options.BaseUrl ??= "https://api.example.com";
            configureApi?.Invoke(options);
        });

        // Step 2: Register platform-neutral base services
        services.AddScoped<IMobileContext, MobileContext>();
        services.AddSingleton<IRateLimiter, RateLimiter>(provider =>
            new RateLimiter(new RateLimiterOptions { MaxRequests = 100, WindowMilliseconds = 60000 }));
        services.AddScoped<ISecureStorageService, SecureStorageService>();

        // Step 3: Register authentication handler (needed by ApiClient)
        services.AddScoped<IAuthenticationHandler, AuthenticationHandler>();

        // Step 4: Register built-in interceptors (if enabled) - needed by ApiClient
        if (enableBuiltinInterceptors)
        {
            services.AddScoped<IRequestInterceptor, CorrelationInterceptor>();
            services.AddScoped<IRequestInterceptor, DeviceInfoInterceptor>();
            services.AddSingleton<IResponseInterceptor, RequestLoggingInterceptor>();
            services.AddSingleton<IRequestInterceptor>(sp => sp.GetRequiredService<IResponseInterceptor>());
            services.AddSingleton<ITokenRefreshInterceptor, TokenRefreshInterceptor>();
            services.AddSingleton<IResponseInterceptor>(sp => sp.GetRequiredService<ITokenRefreshInterceptor>());
        }

        // Step 5: Register error handler (needed by ApiClient)
        services.AddScoped<IErrorHandler, ErrorHandler>();

        // Step 6: Register HttpClientFactory (required by ApiClient)
        services.AddHttpClient("MobileApiClient")
            .ConfigureHttpClient((sp, client) =>
            {
                var config = sp.GetRequiredService<IOptions<MobileApiConfig>>();
                client.BaseAddress = new Uri(config.Value.BaseUrl);
                client.Timeout = config.Value.Timeout;
            });

        // Step 7: Register analytics service
        if (enableRealAnalytics)
        {
            services.AddSingleton<IAnalyticsService, BackendAnalyticsService>();
        }
        else
        {
            services.AddSingleton<IAnalyticsService, AnalyticsService>();
        }

        // Step 8: Register push notifications service
        services.AddScoped<IPushNotificationClientService, PushNotificationClientService>();

        // Step 9: Register ApiClient LAST (after all dependencies are registered)
        services.AddScoped<IApiClient, ApiClient>();

        // Step 10: Register Lazy<IApiClient> wrapper to break circular dependencies
        services.AddScoped(provider => new Lazy<IApiClient>(() => provider.GetRequiredService<IApiClient>()));

        // Step 11: Register JWT settings (if not already present)
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

        // Step 12: Register backward compatibility singleton services
        services.AddSingleton<IConnectionChecker, ConnectionChecker>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<ILocalStorageService, LocalStorageService>();
        services.AddSingleton<IMobileService, MobileService>();

        // Step 13: Register backward compatibility scoped services
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IOfflineService, OfflineService>();
        services.AddScoped<IBiometricService, BiometricService>();

        // Step 14: Register Phase 3 device services
        services.AddScoped<ICameraService, CameraService>();
        services.AddScoped<IMediaPickerService, MediaPickerService>();
        services.AddScoped<IContactsService, ContactsService>();
        services.AddScoped<ILocationService, LocationService>();

        // Step 15: Register Phase 1 library extensions
        services.AddSingleton<IResponsiveService, ResponsiveService>();
        services.AddScoped<IMobileCacheService, MobileCacheService>();
        services.AddSingleton<IRequestDeduplicationService, RequestDeduplicationService>();
        // INavigationService is app-specific — registered by the consuming app (not the library)
        // IMobileFormValidator<T> is open-generic — consuming app registers per-DTO:
        //   services.AddScoped<IMobileFormValidator<LoginDto>, MobileFormValidator<LoginDto>>();

        // Step 16: Register Phase 4 advanced device services
        services.AddScoped<INfcService, NfcService>();
        services.AddScoped<IBluetoothService, BluetoothService>();
        services.AddScoped<IAccelerometerService, AccelerometerService>();

        // Step 17: Register Phase 4.5 Bluetooth pairing service
        services.AddScoped<IBluetoothPairingService, BluetoothPairingService>();

        // Step 18: Register Phase 5 real-time communication service (Task 3)
        services.AddSingleton<IRealtimeService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RealtimeService>>();
            const string realtimeHubUrl = "https://api.smartworkz.com/realtimehub";
            return new RealtimeService(realtimeHubUrl, logger);
        });

        // Step 19: Register Phase 5.2 Change Data Capture service (Task 16)
        services.AddSingleton<IChangeDataCapture>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ChangeDataCapture>>();
            return (IChangeDataCapture)new ChangeDataCapture(logger);
        });

        // Step 20: Register Phase 5.2 Sync Batch Optimizer service (Task 17)
        services.AddSingleton<ISyncBatchOptimizer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SyncBatchOptimizer>>();
            return (ISyncBatchOptimizer)new SyncBatchOptimizer(logger);
        });

        return services;
    }
}

