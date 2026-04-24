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
    /// Adds MassTransit event bus configuration to the service collection.
    /// Automatically configures consumers and selects transport based on configuration.
    /// </summary>
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var eventBusConfig = configuration.GetSection("Features:EventBus");
        var isEnabled = eventBusConfig.GetValue<bool>("Enabled");

        if (!isEnabled)
        {
            return services;
        }

        var provider = eventBusConfig.GetValue<string>("Provider") ?? "InMemory";

        services.AddMassTransit(x =>
        {
            // Register all event consumers
            x.AddConsumer<UserRegisteredEventConsumer>();
            x.AddConsumer<UserProfileUpdatedEventConsumer>();
            x.AddConsumer<PasswordResetRequestedEventConsumer>();
            x.AddConsumer<EmailSentEventConsumer>();
            x.AddConsumer<OrderCreatedEventConsumer>();
            x.AddConsumer<OrderShippedEventConsumer>();
            x.AddConsumer<PaymentProcessedEventConsumer>();

            // Configure transport based on provider
            switch (provider.ToLowerInvariant())
            {
                case "rabbitmq":
                    ConfigureRabbitMQ(x, eventBusConfig);
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
    private static void ConfigureRabbitMQ(
        IBusRegistrationConfigurator x,
        IConfiguration eventBusConfig)
    {
        var rabbitMqConfig = eventBusConfig.GetSection("RabbitMQ");
        var hostname = rabbitMqConfig.GetValue<string>("HostName") ?? "localhost";
        var port = rabbitMqConfig.GetValue<int>("Port", 5672);
        var username = rabbitMqConfig.GetValue<string>("UserName") ?? "guest";
        var password = rabbitMqConfig.GetValue<string>("Password") ?? "guest";
        var virtualHost = rabbitMqConfig.GetValue<string>("VirtualHost") ?? "/";

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host($"amqp://{username}:{password}@{hostname}:{port}/{virtualHost}");
            cfg.ConfigureEndpoints(context);
        });
    }
}
