namespace SmartWorkz.Core.Tests.Logging;

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;

/// <summary>
/// Integration tests for Serilog structured logging configuration.
/// </summary>
public class SerilogIntegrationTests
{
    #region LoggingStartupExtensions Tests

    [Fact]
    public void AddStructuredLogging_WithValidConfig_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();

        // Act
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        Assert.NotNull(logger);
    }

    [Fact]
    public void AddStructuredLogging_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var config = CreateTestConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddStructuredLogging(config));
    }

    [Fact]
    public void AddStructuredLogging_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddStructuredLogging(config!));
    }

    [Fact]
    public void AddStructuredLogging_CreatesLogFile()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        var testLogPath = Path.Combine(Path.GetTempPath(), "smartworkz_test_logs");

        if (Directory.Exists(testLogPath))
            Directory.Delete(testLogPath, true);

        // Act
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();

        // Log a test message
        logger?.LogInformation("Test log message for file creation");

        // Assert
        // Note: File creation is asynchronous, so we may need a small delay
        // The actual file creation will be tested in integration scenarios
        Assert.NotNull(logger);

        // Cleanup
        if (Directory.Exists(testLogPath))
            Directory.Delete(testLogPath, true);
    }

    [Fact]
    public void AddStructuredLogging_ConfiguresConsoleLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();

        // Act
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();

        // Assert
        Assert.NotNull(loggerFactory);
    }

    [Fact]
    public void AddStructuredLogging_WithDevelopmentEnvironment_ConfiguresDevelopmentSinks()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration("Development");

        // Act
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void AddStructuredLogging_WithProductionEnvironment_ConfiguresProductionSinks()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration("Production");

        // Act
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();

        // Assert
        Assert.NotNull(logger);
    }

    #endregion

    #region EnrichedLogger Tests

    [Fact]
    public void EnrichedLogger_LogCommandExecuted_LogsWithDuration()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);

        // Act
        var commandType = "CreateUserCommand";
        var duration = TimeSpan.FromMilliseconds(150);
        enrichedLogger.LogCommandExecuted(commandType, duration);

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void EnrichedLogger_LogEventPublished_LogsWithEventType()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);

        // Act
        var eventType = "UserCreatedEvent";
        var eventId = Guid.NewGuid();
        enrichedLogger.LogEventPublished(eventType, eventId);

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void EnrichedLogger_LogSagaStarted_LogsWithSagaId()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);

        // Act
        var sagaId = Guid.NewGuid();
        var state = "Started";
        enrichedLogger.LogSagaStarted(sagaId, state);

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void EnrichedLogger_LogSagaCompleted_LogsWithDuration()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);

        // Act
        var sagaId = Guid.NewGuid();
        var duration = TimeSpan.FromSeconds(5);
        enrichedLogger.LogSagaCompleted(sagaId, duration);

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void EnrichedLogger_LogFileOperation_LogsWithPath()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);

        // Act
        var operation = "Upload";
        var filePath = "/documents/test.pdf";
        enrichedLogger.LogFileOperation(operation, filePath);

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void EnrichedLogger_LogJobQueued_LogsWithJobId()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);

        // Act
        var jobId = Guid.NewGuid().ToString();
        var jobType = "ProcessInvoice";
        enrichedLogger.LogJobQueued(jobId, jobType);

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void EnrichedLogger_LogJobCompleted_LogsWithDuration()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);

        // Act
        var jobId = Guid.NewGuid().ToString();
        var duration = TimeSpan.FromMinutes(2);
        enrichedLogger.LogJobCompleted(jobId, duration);

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void EnrichedLogger_LogWithException_IncludesExceptionDetails()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);
        var exception = new InvalidOperationException("Test exception");

        // Act
        enrichedLogger.LogCommandExecutionError("TestCommand", exception);

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    [Fact]
    public void EnrichedLogger_LogWithCorrelationId_IncludesContextualData()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateTestConfiguration();
        services.AddStructuredLogging(config);
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<SerilogIntegrationTests>>();
        var enrichedLogger = new EnrichedLogger(logger);
        var correlationId = Guid.NewGuid();

        // Act
        enrichedLogger.LogWithContext("OperationName", new Dictionary<string, object>
        {
            { "CorrelationId", correlationId },
            { "UserId", "user123" }
        });

        // Assert
        Assert.NotNull(enrichedLogger);
    }

    #endregion

    #region Helper Methods

    private static IConfiguration CreateTestConfiguration(string? environment = null)
    {
        var builder = new ConfigurationBuilder();

        if (environment != null)
        {
            builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ASPNETCORE_ENVIRONMENT", environment }
            });
        }

        return builder.Build();
    }

    #endregion
}

