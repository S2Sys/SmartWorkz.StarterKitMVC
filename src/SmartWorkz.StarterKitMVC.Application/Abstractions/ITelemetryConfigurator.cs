namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

public interface ITelemetryConfigurator
{
    void ConfigureTelemetry(IServiceCollection services, IConfiguration configuration);
}
