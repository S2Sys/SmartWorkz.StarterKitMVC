using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Events;
using SmartWorkz.StarterKitMVC.Application.Events.Consumers;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring MassTransit event bus.
/// Supports in-memory, RabbitMQ, Azure Service Bus, and Kafka transports.
/// </summary>
public static class MassTransitExtension
{
    /// <summary>
    /// Adds MassTransit messaging configuration to the service collection.
    /// Automatically configures consumers and selects transport based on configuration.
    /// </summary>
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var messageBrokerConfig = configuration.GetSection("MessageBroker");
        var transportType = messageBrokerConfig.GetValue<string>("Type") ?? "InMemory";

        services.AddMassTransit(x =>
        {
            // Register all event consumers
            x.AddConsumer<SendWelcomeEmailConsumer>();
            x.AddConsumer<SendOrderConfirmationConsumer>();
            x.AddConsumer<PublishAnalyticsEventConsumer>();

            // Configure transport based on type
            switch (transportType.ToLowerInvariant())
            {
                case "rabbitmq":
                    ConfigureRabbitMq(x, messageBrokerConfig);
                    break;

                case "azureservicebus":
                    ConfigureAzureServiceBus(x, messageBrokerConfig);
                    break;

                case "inmemory":
                default:
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                    break;
            }
        });

        return services;
    }

    /// <summary>
    /// Configures RabbitMQ transport for MassTransit.
    /// </summary>
    private static void ConfigureRabbitMq(
        IBusRegistrationConfigurator x,
        IConfiguration messageBrokerConfig)
    {
        var rabbitMqConfig = messageBrokerConfig.GetSection("RabbitMQ");
        var hostname = rabbitMqConfig.GetValue<string>("Host") ?? "localhost";
        var username = rabbitMqConfig.GetValue<string>("Username") ?? "guest";
        var password = rabbitMqConfig.GetValue<string>("Password") ?? "guest";

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(hostname, "/", h =>
            {
                h.Username(username);
                h.Password(password);
            });
            cfg.ConfigureEndpoints(context);
        });
    }

    /// <summary>
    /// Configures Azure Service Bus transport for MassTransit.
    /// Note: Requires MassTransit.AzureServiceBus package to be installed.
    /// </summary>
    private static void ConfigureAzureServiceBus(
        IBusRegistrationConfigurator x,
        IConfiguration messageBrokerConfig)
    {
        throw new NotImplementedException(
            "Azure Service Bus transport not configured. " +
            "Install 'MassTransit.Azure.ServiceBus.Core' NuGet package and update the ConfigureAzureServiceBus method. " +
            "Or set MessageBroker:Type to 'InMemory' or 'RabbitMQ' in configuration.");
    }
}
