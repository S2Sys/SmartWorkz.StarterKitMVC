namespace SmartWorkz.Shared;

using SmartWorkz.Core.Shared.Guards;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering application metrics in dependency injection.
/// </summary>
public static class MetricsStartupExtensions
{
    /// <summary>
    /// Registers IMetricsCollector with OpenTelemetry implementation.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddApplicationMetrics(this IServiceCollection services)
    {
        Guard.NotNull(services, nameof(services));
        services.AddSingleton<IMetricsCollector, OpenTelemetryMetricsCollector>();
        return services;
    }
}
