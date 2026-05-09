using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Shared.Webhooks.Abstractions;
using SmartWorkz.Core.Shared.Webhooks.Implementations;
using IWebhookRegistry = SmartWorkz.Core.Shared.Webhooks.Abstractions.IWebhookRegistry;

namespace SmartWorkz.Core.Shared.Webhooks.Extensions;

/// <summary>
/// Extension methods for registering webhook services in the dependency injection container.
///
/// <para><strong>Purpose</strong>: Provides convenient registration of webhook infrastructure
/// in the service collection during application startup. Registers all webhook-related services
/// with appropriate lifetimes and configurations.</para>
///
/// <para><strong>Usage</strong>:
/// In Startup.cs or Program.cs:
/// <code>
/// services.AddWebhooks();
/// </code>
/// </para>
///
/// <para><strong>Services Registered</strong>:
/// • IWebhookPublisher (Scoped) - WebhookPublisher implementation
/// • IWebhookRegistry (Scoped) - Custom implementation (must be registered separately or inherited)
/// • IHttpClientFactory (Singleton) - Built-in .NET Core service
/// </para>
/// </summary>
public static class WebhookStartupExtensions
{
    /// <summary>
    /// Register webhook infrastructure services in the dependency injection container.
    ///
    /// <para>Registers the default WebhookPublisher implementation for the IWebhookPublisher
    /// interface. The IWebhookRegistry must be registered separately by the consuming application.</para>
    /// </summary>
    /// <param name="services">The service collection to add webhook services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    ///
    /// // Register webhook infrastructure
    /// builder.Services.AddWebhooks();
    ///
    /// // Register the specific IWebhookRegistry implementation
    /// builder.Services.AddScoped&lt;IWebhookRegistry, SqlWebhookRegistry&gt;();
    ///
    /// // Register HTTP client factory (usually done by other middleware, but ensure it's there)
    /// builder.Services.AddHttpClient();
    ///
    /// var app = builder.Build();
    /// // ... rest of app configuration ...
    /// </code>
    /// </example>
    public static IServiceCollection AddWebhooks(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register WebhookPublisher as the default implementation of IWebhookPublisher
        services.AddScoped<IWebhookPublisher, WebhookPublisher>();

        // Ensure HttpClientFactory is registered (safe to call multiple times)
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// Register webhook infrastructure services with a custom IWebhookRegistry implementation.
    ///
    /// <para>Registers both the WebhookPublisher implementation and a specific IWebhookRegistry
    /// implementation in a single call, simplifying the setup process.</para>
    /// </summary>
    /// <typeparam name="TRegistry">The concrete type implementing IWebhookRegistry.</typeparam>
    /// <param name="services">The service collection to add webhook services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    ///
    /// // Register webhook infrastructure with SqlWebhookRegistry in one call
    /// builder.Services.AddWebhooks&lt;SqlWebhookRegistry&gt;();
    ///
    /// var app = builder.Build();
    /// // ... rest of app configuration ...
    /// </code>
    /// </example>
    public static IServiceCollection AddWebhooks<TRegistry>(this IServiceCollection services)
        where TRegistry : class, IWebhookRegistry
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register WebhookPublisher as the default implementation of IWebhookPublisher
        services.AddScoped<IWebhookPublisher, WebhookPublisher>();

        // Register the specified IWebhookRegistry implementation
        services.AddScoped<IWebhookRegistry, TRegistry>();

        // Ensure HttpClientFactory is registered
        services.AddHttpClient();

        return services;
    }
}
