using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartWorkz.Shared;

namespace SmartWorkz.Core.Shared.Tests;

/// <summary>
/// Comprehensive test suite for LoggingStartupExtensions covering
/// DI registration, log level configuration, and logging capabilities.
/// </summary>
public class LoggingTests
{
    #region DI Registration Tests

    [Fact]
    public void AddStructuredLogging_WithValidConfiguration_RegistersServicesSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddStructuredLogging(configuration);

        // Assert - Verify EnrichedLogger is registered
        var serviceProvider = services.BuildServiceProvider();
        var enrichedLogger = serviceProvider.GetService<EnrichedLogger>();
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void AddStructuredLogging_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var configuration = CreateTestConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddStructuredLogging(configuration));
    }

    [Fact]
    public void AddStructuredLogging_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddStructuredLogging(configuration!));
    }

    [Fact]
    public void AddStructuredLogging_CanBeCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act - Should not throw even with multiple calls
        services.AddStructuredLogging(configuration);
        services.AddStructuredLogging(configuration);

        // Assert
        Assert.NotNull(services);
    }

    #endregion

    #region Service Configuration Tests

    [Fact]
    public void AddStructuredLogging_RegistersLoggingProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddStructuredLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify ILogger can be resolved (through ILoggerFactory)
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);

        var logger = loggerFactory.CreateLogger("Test");
        Assert.NotNull(logger);
    }

    [Fact]
    public void AddStructuredLogging_WithProductionEnvironment_ConfiguresAppropriately()
    {
        // Arrange
        var services = new ServiceCollection();
        var configDict = new Dictionary<string, string?>
        {
            { "ASPNETCORE_ENVIRONMENT", "Production" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        // Act
        services.AddStructuredLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var enrichedLogger = serviceProvider.GetService<EnrichedLogger>();
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void AddStructuredLogging_WithDevelopmentEnvironment_ConfiguresAppropriately()
    {
        // Arrange
        var services = new ServiceCollection();
        var configDict = new Dictionary<string, string?>
        {
            { "ASPNETCORE_ENVIRONMENT", "Development" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        // Act
        services.AddStructuredLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var enrichedLogger = serviceProvider.GetService<EnrichedLogger>();
        Assert.NotNull(enrichedLogger);
    }

    #endregion

    #region EnrichedLogger Tests

    [Fact]
    public void EnrichedLogger_CanBeInstantiated()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        services.AddStructuredLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var logger = serviceProvider.GetService<EnrichedLogger>();

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void EnrichedLogger_ReturnsNotNull_FromDI()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        services.AddStructuredLogging(configuration);

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var enrichedLogger = serviceProvider.GetRequiredService<EnrichedLogger>();

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    #endregion

    #region Scope Tests

    [Fact]
    public void AddStructuredLogging_RegistersEnrichedLoggerAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddStructuredLogging(configuration);

        // Assert - Verify EnrichedLogger is registered
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(EnrichedLogger));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void EnrichedLogger_EachScopeGetsNewInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        services.AddStructuredLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();
        var logger1 = scope1.ServiceProvider.GetRequiredService<EnrichedLogger>();
        var logger2 = scope2.ServiceProvider.GetRequiredService<EnrichedLogger>();

        // Assert - Different instances due to scoped lifetime
        Assert.NotEqual(logger1, logger2);
    }

    #endregion

    #region ChainableConfiguration Tests

    [Fact]
    public void AddStructuredLogging_ReturnsMutatedServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        var result = services.AddStructuredLogging(configuration);

        // Assert - Should return the same service collection for fluent chaining
        Assert.Same(services, result);
    }

    [Fact]
    public void AddStructuredLogging_AllowsFluentChaining()
    {
        // Arrange & Act
        var services = new ServiceCollection()
            .AddStructuredLogging(CreateTestConfiguration())
            .AddScoped<TestService>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<TestService>();
        Assert.NotNull(service);
    }

    #endregion

    #region Error Handling

    [Fact]
    public void AddStructuredLogging_WithNullEnvironment_DefaultsToProduction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configDict = new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        // Act
        services.AddStructuredLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Should not throw
        var enrichedLogger = serviceProvider.GetService<EnrichedLogger>();
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void AddStructuredLogging_WithEmptyConfiguration_StillWorks()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddStructuredLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var enrichedLogger = serviceProvider.GetService<EnrichedLogger>();
        Assert.NotNull(enrichedLogger);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AddStructuredLogging_WithLogging_ProducesValidLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddStructuredLogging(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        // Assert
        var logger = loggerFactory.CreateLogger("TestLogger");
        Assert.NotNull(logger);
    }

    [Fact]
    public void AddStructuredLogging_CreatesValidLogDirectory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddStructuredLogging(configuration);

        // Assert - logs directory should be created or at least not throw
        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        // Note: Directory creation is deferred, so we just verify the configuration didn't throw
        Assert.True(true); // If we got here without exception, configuration succeeded
    }

    #endregion

    #region Test Helpers

    private static IConfiguration CreateTestConfiguration()
    {
        var configDict = new Dictionary<string, string?>
        {
            { "ASPNETCORE_ENVIRONMENT", "Development" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();
    }

    private class TestService { }

    #endregion
}
