// This file uses full namespace qualification to avoid collisions with the custom ILogger interface
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Core.Shared.Logging;

/// <summary>
/// Extension methods for configuring structured logging.
/// Provides environment-aware configuration with multiple sinks and enrichment.
/// </summary>
public static class LoggingStartupExtensions
{
    /// <summary>
    /// Adds structured logging to the dependency injection container.
    /// Configures console and file sinks with JSON formatting.
    /// Environment-specific configuration for development vs production.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null</exception>
    public static IServiceCollection AddStructuredLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        // Configure logging with Microsoft.Extensions.Logging
        services.AddLogging(builder =>
        {
            var environment = GetEnvironment(configuration);
            var logPath = Path.Combine(AppContext.BaseDirectory, "logs");

            // Clear default providers
            builder.ClearProviders();

            // Configure console logging
            builder.AddConsole();

            // Add Debug logging for development
            if (environment == "Development")
            {
                builder.AddDebug();
            }

            // Set minimum log level based on environment
            var minLevel = environment == "Development" ? Microsoft.Extensions.Logging.LogLevel.Debug : Microsoft.Extensions.Logging.LogLevel.Information;
            builder.SetMinimumLevel(minLevel);

            // Add File logging using Extension
            builder.AddSimpleFile(Path.Combine(logPath, "app.log"));
        });

        return services;
    }

    /// <summary>
    /// Extracts the environment name from configuration or environment variables.
    /// </summary>
    private static string GetEnvironment(IConfiguration configuration)
    {
        var env = configuration["ASPNETCORE_ENVIRONMENT"]
            ?? System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Production";

        return env;
    }
}

/// <summary>
/// Extension method to add simple file logging to ILoggingBuilder.
/// </summary>
public static class SimpleFileLoggingExtensions
{
    public static ILoggingBuilder AddSimpleFile(this ILoggingBuilder builder, string filePath)
    {
        builder.AddProvider(new SimpleFileLoggerProvider(filePath));
        return builder;
    }
}

/// <summary>
/// Simple file logger provider implementation.
/// </summary>
internal sealed class SimpleFileLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
{
    private readonly string _filePath;
    private readonly Dictionary<string, SimpleFileLogger> _loggers = new();
    private readonly object _lock = new object();

    public SimpleFileLoggerProvider(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        EnsureDirectoryExists();
    }

    Microsoft.Extensions.Logging.ILogger Microsoft.Extensions.Logging.ILoggerProvider.CreateLogger(string categoryName)
    {
        lock (_lock)
        {
            if (!_loggers.TryGetValue(categoryName, out var logger))
            {
                logger = new SimpleFileLogger(categoryName, _filePath);
                _loggers[categoryName] = logger;
            }

            return logger;
        }
    }

    void System.IDisposable.Dispose()
    {
        lock (_lock)
        {
            foreach (var logger in _loggers.Values)
            {
                ((System.IDisposable)logger).Dispose();
            }

            _loggers.Clear();
        }
    }

    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
    }
}

/// <summary>
/// Simple file logger implementation.
/// </summary>
internal sealed class SimpleFileLogger : Microsoft.Extensions.Logging.ILogger, System.IDisposable
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private readonly object _lock = new object();
    private System.IO.StreamWriter? _writer;

    public SimpleFileLogger(string categoryName, string filePath)
    {
        _categoryName = categoryName;
        _filePath = filePath;
        InitializeWriter();
    }

    System.IDisposable? Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state) => null;

    bool Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => logLevel != Microsoft.Extensions.Logging.LogLevel.None;

    void Microsoft.Extensions.Logging.ILogger.Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        Microsoft.Extensions.Logging.EventId eventId,
        TState state,
        System.Exception? exception,
        System.Func<TState, System.Exception?, string> formatter)
    {
        if (logLevel == Microsoft.Extensions.Logging.LogLevel.None)
            return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message))
            return;

        lock (_lock)
        {
            try
            {
                var logEntry = FormatLogEntry(logLevel, _categoryName, eventId, message, exception);
                _writer?.WriteLine(logEntry);
                _writer?.Flush();
            }
            catch
            {
                // Suppress logging errors
            }
        }
    }

    private static string FormatLogEntry(
        Microsoft.Extensions.Logging.LogLevel level,
        string category,
        Microsoft.Extensions.Logging.EventId eventId,
        string message,
        System.Exception? exception)
    {
        var timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var entry = $"[{timestamp}] [{level}] [{category}] {message}";

        if (exception != null)
        {
            entry += $"\nException: {exception}";
        }

        return entry;
    }

    private void InitializeWriter()
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            _writer = new System.IO.StreamWriter(
                new System.IO.FileStream(_filePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
            {
                AutoFlush = true
            };
        }
        catch
        {
            // Fail gracefully
        }
    }

    void System.IDisposable.Dispose()
    {
        lock (_lock)
        {
            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;
        }
    }
}
