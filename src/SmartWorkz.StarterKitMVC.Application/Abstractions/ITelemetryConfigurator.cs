using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

/// <summary>
/// Interface for configuring telemetry services.
/// </summary>
public interface ITelemetryConfigurator
{
    /// <summary>Configures telemetry services.</summary>
    void ConfigureTelemetry(IServiceCollection services, IConfiguration configuration);
}
