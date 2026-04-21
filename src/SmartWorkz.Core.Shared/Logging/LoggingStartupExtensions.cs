namespace SmartWorkz.Core.Shared.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Extension methods for configuring structured logging with Serilog.
/// Provides environment-aware configuration with JSON output and enrichment.
/// </summary>
public static class LoggingStartupExtensions
{
    /// <summary>
    /// Adds Serilog structured logging to the dependency injection container.
    /// Configures console and rolling file sinks with JSON formatting.
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
        Guard.NotNull(services, nameof(services));
        Guard.NotNull(configuration, nameof(configuration));

        var environment = GetEnvironment(configuration);
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app-.log");

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithProperty("Application", "SmartWorkz");

        // Console sink (all environments) with readable format
        loggerConfig = loggerConfig.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        );

        // File sink with JSON formatting
        if (environment == "Production")
        {
            // Production: aggressive retention (30 days, 100 MB files)
            loggerConfig = loggerConfig.WriteTo.File(
                new CompactJsonFormatter(),
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 100_000_000  // 100 MB
            );
        }
        else
        {
            // Development: shorter retention (7 days)
            loggerConfig = loggerConfig.WriteTo.File(
                new CompactJsonFormatter(),
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7
            );
        }

        // Set as global logger
        Log.Logger = loggerConfig.CreateLogger();

        // Add to DI
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });

        services.AddScoped<EnrichedLogger>();

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
