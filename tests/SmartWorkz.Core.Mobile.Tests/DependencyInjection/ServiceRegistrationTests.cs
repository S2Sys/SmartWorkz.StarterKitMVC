using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile.Data.Database;
using SmartWorkz.Mobile.Diagnostics;
using SmartWorkz.Mobile.Notifications;
using SmartWorkz.Mobile.Notifications.Handlers;
using SmartWorkz.Mobile.State.Store;
using SmartWorkz.Mobile.State.Actions;

namespace SmartWorkz.Mobile.Tests.DependencyInjection;

/// <summary>
/// Tests for Phase 3A service registration in the MauiProgram dependency injection container.
/// Verifies that all infrastructure services are registered with correct scopes and can be resolved.
/// </summary>
public class ServiceRegistrationTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _testDbPath;

    public ServiceRegistrationTests()
    {
        // Create a minimal MauiApp builder for testing (without UI initialization)
        var builder = MauiApp.CreateBuilder();

        // Configure minimal fonts to avoid UI errors
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });

        // Add logging for tests
        builder.Services.AddLogging(cfg =>
        {
            cfg.AddDebug();
        });

        // Register SmartWorkz.Core.Mobile services
        builder.Services.AddSmartWorkzCoreMobile(cfg =>
        {
            cfg.BaseUrl = "https://localhost:7000";
            cfg.RetryCount = 3;
        });

        // === PHASE 3A Infrastructure Services ===

        // 1. Redux State Management
        builder.Services.AddSingleton<IAppStore, AppStore>();

        // 2. Database Services
        _testDbPath = Path.Combine(FileSystem.AppDataDirectory, "smartworkz_test.db");
        builder.Services.AddSingleton<IDatabase>(sp => new SqliteDatabase(_testDbPath));

        // 3. Crash Reporting Service
        var sentryDsn = "https://test@test.ingest.sentry.io/0";
        builder.Services.AddSingleton<ICrashReportingService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SentryCrashReporter>>();
            var service = new SentryCrashReporter(logger);

            try
            {
                service.InitializeAsync(sentryDsn, "Development").GetAwaiter().GetResult();
            }
            catch
            {
                // Sentry init may fail in test environment, which is acceptable
            }

            return service;
        });

        // 4. Push Notification Services
        builder.Services.AddSingleton<PushMessageRouter>();
        builder.Services.AddSingleton<PushNotificationPermissionHelper>();

        // Register platform-specific push handler (use Android for testing)
#if !WINDOWS
        builder.Services.AddSingleton<IPushNotificationHandler>(sp =>
            new AndroidPushHandler(sp.GetRequiredService<ILogger<AndroidPushHandler>>()));
#endif

        var app = builder.Build();
        _serviceProvider = app.Services;
    }

    public void Dispose()
    {
        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void AppStore_ServiceRegistered_CanResolveFromContainer()
    {
        // Arrange & Act
        var appStore = _serviceProvider.GetRequiredService<IAppStore>();

        // Assert
        Assert.NotNull(appStore);
        Assert.IsType<AppStore>(appStore);
    }

    [Fact]
    public void AppStore_IsSingleton_ReturnsSameInstance()
    {
        // Arrange & Act
        var appStore1 = _serviceProvider.GetRequiredService<IAppStore>();
        var appStore2 = _serviceProvider.GetRequiredService<IAppStore>();

        // Assert
        Assert.Same(appStore1, appStore2);
    }

    [Fact]
    public void Database_ServiceRegistered_CanResolveFromContainer()
    {
        // Arrange & Act
        var database = _serviceProvider.GetRequiredService<IDatabase>();

        // Assert
        Assert.NotNull(database);
        Assert.IsType<SqliteDatabase>(database);
    }

    [Fact]
    public void Database_IsSingleton_ReturnsSameInstance()
    {
        // Arrange & Act
        var db1 = _serviceProvider.GetRequiredService<IDatabase>();
        var db2 = _serviceProvider.GetRequiredService<IDatabase>();

        // Assert
        Assert.Same(db1, db2);
    }

    [Fact]
    public void Database_HasCorrectPath_ContainsAppDataDirectory()
    {
        // Arrange & Act
        var database = _serviceProvider.GetRequiredService<IDatabase>() as SqliteDatabase;

        // Assert
        Assert.NotNull(database);
        Assert.NotNull(database!.DatabasePath);
        Assert.Contains("smartworkz", database.DatabasePath);
    }

    [Fact]
    public void CrashReportingService_ServiceRegistered_CanResolveFromContainer()
    {
        // Arrange & Act
        var crashService = _serviceProvider.GetRequiredService<ICrashReportingService>();

        // Assert
        Assert.NotNull(crashService);
        Assert.IsType<SentryCrashReporter>(crashService);
    }

    [Fact]
    public void CrashReportingService_IsSingleton_ReturnsSameInstance()
    {
        // Arrange & Act
        var crashService1 = _serviceProvider.GetRequiredService<ICrashReportingService>();
        var crashService2 = _serviceProvider.GetRequiredService<ICrashReportingService>();

        // Assert
        Assert.Same(crashService1, crashService2);
    }

    [Fact]
    public void CrashReportingService_IsInitialized()
    {
        // Arrange & Act
        var crashService = _serviceProvider.GetRequiredService<ICrashReportingService>();

        // Assert - Service should be initialized
        Assert.True(crashService.IsInitialized);
    }

    [Fact]
    public void PushMessageRouter_ServiceRegistered_CanResolveFromContainer()
    {
        // Arrange & Act
        var router = _serviceProvider.GetRequiredService<PushMessageRouter>();

        // Assert
        Assert.NotNull(router);
    }

    [Fact]
    public void PushMessageRouter_IsSingleton_ReturnsSameInstance()
    {
        // Arrange & Act
        var router1 = _serviceProvider.GetRequiredService<PushMessageRouter>();
        var router2 = _serviceProvider.GetRequiredService<PushMessageRouter>();

        // Assert
        Assert.Same(router1, router2);
    }

    [Fact]
    public void PushNotificationPermissionHelper_ServiceRegistered_CanResolveFromContainer()
    {
        // Arrange & Act
        var helper = _serviceProvider.GetRequiredService<PushNotificationPermissionHelper>();

        // Assert
        Assert.NotNull(helper);
    }

    [Fact]
    public void PushNotificationPermissionHelper_IsSingleton_ReturnsSameInstance()
    {
        // Arrange & Act
        var helper1 = _serviceProvider.GetRequiredService<PushNotificationPermissionHelper>();
        var helper2 = _serviceProvider.GetRequiredService<PushNotificationPermissionHelper>();

        // Assert
        Assert.Same(helper1, helper2);
    }

    [Fact]
    public void PushNotificationHandler_ServiceRegistered_CanResolveFromContainer()
    {
#if !WINDOWS
        // Arrange & Act
        var handler = _serviceProvider.GetRequiredService<IPushNotificationHandler>();

        // Assert
        Assert.NotNull(handler);
        Assert.IsType<AndroidPushHandler>(handler);
#else
        // Windows platform: skip Android-specific test
        Assert.True(true);
#endif
    }

    [Fact]
    public void PushNotificationHandler_IsSingleton_ReturnsSameInstance()
    {
#if !WINDOWS
        // Arrange & Act
        var handler1 = _serviceProvider.GetRequiredService<IPushNotificationHandler>();
        var handler2 = _serviceProvider.GetRequiredService<IPushNotificationHandler>();

        // Assert
        Assert.Same(handler1, handler2);
#else
        // Windows platform: skip Android-specific test
        Assert.True(true);
#endif
    }

    [Fact]
    public void Logger_ServiceRegistered_CanResolveFromContainer()
    {
        // Arrange & Act
        var logger = _serviceProvider.GetRequiredService<ILogger<ServiceRegistrationTests>>();

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void AllPhase3AServices_CanBeResolved()
    {
        // This test ensures that all Phase 3A services can be resolved without errors

        // Arrange & Act & Assert - should not throw
        var appStore = _serviceProvider.GetRequiredService<IAppStore>();
        var database = _serviceProvider.GetRequiredService<IDatabase>();
        var crashService = _serviceProvider.GetRequiredService<ICrashReportingService>();
        var router = _serviceProvider.GetRequiredService<PushMessageRouter>();
        var helper = _serviceProvider.GetRequiredService<PushNotificationPermissionHelper>();

        Assert.NotNull(appStore);
        Assert.NotNull(database);
        Assert.NotNull(crashService);
        Assert.NotNull(router);
        Assert.NotNull(helper);
    }

    [Fact]
    public void AppStore_InitialState_HasDefaultValues()
    {
        // Arrange & Act
        var appStore = _serviceProvider.GetRequiredService<IAppStore>();
        var state = appStore.State;

        // Assert
        Assert.NotNull(state);
        Assert.NotNull(state.InitState);
        Assert.NotNull(state.AuthState);
        Assert.NotNull(state.SyncState);
        Assert.NotNull(state.NotificationState);
        Assert.NotNull(state.ErrorState);
    }

    [Fact]
    public void AppStore_DispatchAction_UpdatesState()
    {
        // Arrange
        var appStore = _serviceProvider.GetRequiredService<IAppStore>();

        // Act
        appStore.Dispatch(new InitializeAppAction());
        var state = appStore.State;

        // Assert
        Assert.True(state.InitState.IsInitializing);
    }

    [Fact]
    public void AppStore_Subscribe_CallsSubscriberOnStateChange()
    {
        // Arrange
        var appStore = _serviceProvider.GetRequiredService<IAppStore>();
        var stateChanges = new List<AppState>();

        // Act
        using (appStore.Subscribe(newState => stateChanges.Add(newState)))
        {
            appStore.Dispatch(new InitializeAppAction());
        }

        // Assert
        Assert.NotEmpty(stateChanges);
    }

    [Fact]
    public void Database_FilePathIsAccessible()
    {
        // Arrange
        var database = _serviceProvider.GetRequiredService<IDatabase>();

        // Act
        var path = database.DatabasePath;
        var directory = Path.GetDirectoryName(path);

        // Assert
        Assert.NotNull(path);
        Assert.NotEmpty(path);
        Assert.NotNull(directory);
        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public async Task Database_CanOpenAndCloseConnection()
    {
        // Arrange
        var database = _serviceProvider.GetRequiredService<IDatabase>();

        // Act & Assert
        await database.OpenAsync();
        Assert.True(database.IsConnected);

        await database.CloseAsync();
        Assert.False(database.IsConnected);
    }

    [Fact]
    public async Task Database_CanRunMigrations()
    {
        // Arrange
        var database = _serviceProvider.GetRequiredService<IDatabase>();

        // Act & Assert
        await database.OpenAsync();
        await database.RunMigrationsAsync();
        await database.CloseAsync();

        // Assert - should not throw
        Assert.True(true);
    }

    [Fact]
    public void InitializeAppAction_CanBeDispatched()
    {
        // Arrange
        var appStore = _serviceProvider.GetRequiredService<IAppStore>();

        // Act & Assert - should not throw
        appStore.Dispatch(new InitializeAppAction());
        Assert.True(true);
    }
}
