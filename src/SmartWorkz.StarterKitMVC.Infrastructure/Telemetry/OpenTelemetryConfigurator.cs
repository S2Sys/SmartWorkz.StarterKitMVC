using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Telemetry;

/// <summary>
/// Placeholder telemetry configurator. Add OpenTelemetry packages to enable.
/// </summary>
public sealed class NoOpTelemetryConfigurator : ITelemetryConfigurator
{
    public void ConfigureTelemetry(IServiceCollection services, IConfiguration configuration)
    {
        // OpenTelemetry can be enabled by adding the following packages:
        // - OpenTelemetry.Extensions.Hosting
        // - OpenTelemetry.Instrumentation.AspNetCore
        // - OpenTelemetry.Instrumentation.Http
        // Then uncomment and configure:
        // services.AddOpenTelemetry()
        //     .ConfigureResource(r => r.AddService("SmartWorkz.StarterKitMVC"))
        //     .WithTracing(b =>
        //     {
        //         b.AddAspNetCoreInstrumentation();
        //         b.AddHttpClientInstrumentation();
        //     });
    }
}
