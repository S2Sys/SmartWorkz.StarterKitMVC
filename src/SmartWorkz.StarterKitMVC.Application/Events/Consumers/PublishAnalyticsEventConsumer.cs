using MassTransit;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Publishes analytics events when user registers
/// </summary>
public class PublishAnalyticsEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<PublishAnalyticsEventConsumer> _logger;

    public PublishAnalyticsEventConsumer(
        ILogger<PublishAnalyticsEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        try
        {
            var message = context.Message;

            // Track analytics event for user registration
            _logger.LogInformation(
                "Analytics event: UserRegistered - UserId: {UserId}, Email: {Email}, RegisteredAt: {RegisteredAt}",
                message.UserId,
                message.Email,
                message.RegisteredAt);

            // In a production system, this would send data to an analytics service
            // (e.g., Mixpanel, Segment, Google Analytics, etc.)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process analytics event");
            // Don't throw - analytics should not block main flow
        }
    }
}
