using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Telemetry;

/// <summary>
/// Configures OpenTelemetry distributed tracing for the application.
/// </summary>
/// <remarks>
/// Initializes OpenTelemetry with service resource configuration.
/// Instrumentation can be added by configuring WithTracing() with specific instrumentation types.
/// Configuration can be extended to support OTLP exporters via environment variables.
/// </remarks>
public sealed class OpenTelemetryConfigurator : ITelemetryConfigurator
{
    public void ConfigureTelemetry(IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["Telemetry:ServiceName"] ?? "SmartWorkz.StarterKitMVC";
        var endpoint = configuration["Telemetry:Endpoint"];

        // Add basic OpenTelemetry with service resource
        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource
                    .AddService(serviceName)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        { "service.version", "1.0.0" },
                        { "deployment.environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production" }
                    }));

        // WithTracing configuration can be extended by calling AddOpenTelemetry() again or by configuring
        // individual instrumentation packages (AspNetCore, Http, SqlClient, etc.) in separate extensions
        // This provides a foundation for distributed tracing that can be built upon.
    }
}
