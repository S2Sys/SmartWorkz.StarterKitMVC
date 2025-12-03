using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Telemetry;

public sealed class OpenTelemetryConfigurator : ITelemetryConfigurator
{
    public void ConfigureTelemetry(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("SmartWorkz.StarterKitMVC"))
            .WithTracing(b =>
            {
                b.AddAspNetCoreInstrumentation();
                b.AddHttpClientInstrumentation();
            });
    }
}
