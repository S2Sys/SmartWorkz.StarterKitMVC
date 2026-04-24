using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Services;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Publishes analytics events when user registers
/// </summary>
public class PublishAnalyticsEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<PublishAnalyticsEventConsumer> _logger;

    public PublishAnalyticsEventConsumer(
        IAnalyticsService analyticsService,
        ILogger<PublishAnalyticsEventConsumer> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        try
        {
            var message = context.Message;
            await _analyticsService.TrackEventAsync("UserRegistered", new
            {
                UserId = message.UserId,
                Email = message.Email,
                RegisteredAt = message.RegisteredAt
            });

            _logger.LogInformation($"✓ Analytics event published for user {message.UserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish analytics event");
            // Don't throw - analytics should not block main flow
        }
    }
}
