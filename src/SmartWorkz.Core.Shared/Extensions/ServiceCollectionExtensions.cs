using Microsoft.Extensions.DependencyInjection;

namespace SmartWorkz.Shared;

/// <summary>
/// Specifies the publisher type for event publishing.
/// </summary>
public enum PublisherType
{
    /// <summary>In-memory event publisher (synchronous, suitable for testing and small applications).</summary>
    InMemory,
    /// <summary>MassTransit event publisher (distributed, suitable for production with message brokers).</summary>
    MassTransit
}

/// <summary>
/// Extension methods for IServiceCollection to register Core.Shared services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Core.Shared services including TemplateEngine for template rendering.
    /// </summary>
    public static IServiceCollection AddCoreSharedServices(this IServiceCollection services)
    {
        services.AddScoped<ITemplateEngine, TemplateEngine>();
        return services;
    }

    /// <summary>
    /// Adds event publishing services to the dependency injection container.
    /// Supports switching between in-memory and MassTransit publishers based on application needs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="publisherType">The publisher type to use (defaults to InMemory).</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when an unknown publisher type is specified.</exception>
    public static IServiceCollection AddEventPublishing(
        this IServiceCollection services,
        PublisherType publisherType = PublisherType.InMemory)
    {
        switch (publisherType)
        {
            case PublisherType.InMemory:
                services.AddSingleton<InMemoryEventSubscriber>();
                services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
                break;
            case PublisherType.MassTransit:
                // Assumes MassTransit is configured elsewhere via AddMassTransit()
                services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
                break;
            default:
                throw new ArgumentException($"Unknown publisher type: {publisherType}");
        }

        return services;
    }
}
