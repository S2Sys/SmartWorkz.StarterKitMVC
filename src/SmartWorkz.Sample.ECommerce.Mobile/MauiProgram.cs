namespace SmartWorkz.ECommerce.Mobile;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.Logging;
using SmartWorkz.ECommerce.Mobile.Services;
using SmartWorkz.ECommerce.Mobile.Repositories;
using SmartWorkz.Mobile;
using SmartWorkz.Mobile.Data.Database;
using SmartWorkz.Mobile.Diagnostics;
using SmartWorkz.Mobile.Notifications;
using SmartWorkz.Mobile.Notifications.Handlers;
using SmartWorkz.Mobile.State.Store;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // NOTE: UseMaui() extension will be called in platform-specific code
        // builder.UseMaui();

        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
        });

        // Register SmartWorkz.Core.Mobile services
        builder.Services.AddSmartWorkzCoreMobile(cfg =>
        {
            cfg.BaseUrl     = "https://localhost:7000"; // override in production via appsettings
            cfg.RetryCount  = 3;
        });

        // ===== PHASE 3A Infrastructure Services =====

        // 1. Redux State Management
        builder.Services.AddSingleton<IAppStore, AppStore>();

        // 2. Database Services
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "smartworkz.db");
        builder.Services.AddSingleton<IDatabase>(sp => new SqliteDatabase(dbPath));

        // 3. Crash Reporting Service
        var sentryDsn = GetSentryDsn(); // Read from environment, config, or default
        var environment = GetEnvironment();
        builder.Services.AddSingleton<ICrashReportingService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SentryCrashReporter>>();
            var service = new SentryCrashReporter(logger);

            // Initialize Sentry on startup (sync initialization for app startup)
            try
            {
                service.InitializeAsync(sentryDsn, environment).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to initialize Sentry crash reporter");
                // Continue - Sentry is not critical for app functionality
            }

            return service;
        });

        // 4. Push Notification Services
        builder.Services.AddSingleton<PushMessageRouter>();
        builder.Services.AddSingleton<PushNotificationPermissionHelper>();

        // Register platform-specific push notification handlers
#if __ANDROID__
        builder.Services.AddSingleton<IPushNotificationHandler>(sp =>
            new AndroidPushHandler(sp.GetRequiredService<ILogger<AndroidPushHandler>>()));
#elif __IOS__
        builder.Services.AddSingleton<IPushNotificationHandler>(sp =>
            new iOSPushHandler(sp.GetRequiredService<ILogger<iOSPushHandler>>()));
#elif WINDOWS
        builder.Services.AddSingleton<IPushNotificationHandler>(sp =>
            new WindowsPushHandler(sp.GetRequiredService<ILogger<WindowsPushHandler>>()));
#endif

        // ===== End Phase 3A Infrastructure Services =====

        // Register app-specific services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ProductRepository>();
        builder.Services.AddSingleton<OrderRepository>();

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddSingleton<CartViewModel>();
        builder.Services.AddTransient<CheckoutViewModel>();
        builder.Services.AddTransient<OrdersViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Register Pages
        builder.Services.AddTransient<Pages.LoginPage>();
        builder.Services.AddTransient<Pages.RegisterPage>();
        builder.Services.AddTransient<Pages.HomePage>();
        builder.Services.AddTransient<Pages.ProductDetailPage>();
        builder.Services.AddTransient<Pages.CartPage>();
        builder.Services.AddTransient<Pages.CheckoutPage>();
        builder.Services.AddTransient<Pages.OrdersPage>();
        builder.Services.AddTransient<Pages.ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    /// <summary>
    /// Gets the Sentry DSN from environment variable, appsettings, or returns a default.
    /// </summary>
    private static string GetSentryDsn()
    {
        // Try environment variable first
        var envDsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
        if (!string.IsNullOrWhiteSpace(envDsn))
            return envDsn;

        // Default DSN for development/testing
        // In production, ensure SENTRY_DSN environment variable is set
        return "https://test@test.ingest.sentry.io/0";
    }

    /// <summary>
    /// Gets the environment name (Development, Staging, Production).
    /// </summary>
    private static string GetEnvironment()
    {
#if DEBUG
        return "Development";
#else
        // Check environment variable, default to Production
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
#endif
    }
}
