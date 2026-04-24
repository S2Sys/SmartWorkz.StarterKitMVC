using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared.Communications;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Consumer for handling order shipped events.
/// Sends shipping notification email with tracking information.
/// </summary>
public class OrderShippedEventConsumer : IConsumer<OrderShippedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OrderShippedEventConsumer> _logger;

    public OrderShippedEventConsumer(
        IEmailSender emailSender,
        ILogger<OrderShippedEventConsumer> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing order shipped event - OrderId: {OrderId}, TrackingNumber: {TrackingNumber}, Carrier: {Carrier}",
                @event.OrderId,
                @event.TrackingNumber,
                @event.Carrier);

            // Send shipping notification email
            // Note: In a real scenario, you would fetch user email from repository
            var subject = $"Your Order Has Been Shipped - Order #{@event.OrderId}";
            var trackingLink = GetTrackingLink(@event.Carrier, @event.TrackingNumber);

            var body = $@"
                <h2>Your Order Has Been Shipped!</h2>
                <p>Good news! Your order has been dispatched and is on its way.</p>
                <p><strong>Order ID:</strong> {@event.OrderId}</p>
                <p><strong>Carrier:</strong> {@event.Carrier}</p>
                <p><strong>Tracking Number:</strong> {@event.TrackingNumber}</p>
                <p>Track your package: <a href=""{trackingLink}"">{trackingLink}</a></p>
                <p>You can also view tracking details in your account dashboard.</p>
                <p>Thank you for shopping with us!</p>
                <br/>
                <p>Best regards,<br/>The SmartWorkz Team</p>
            ";

            // TODO: Get user email from user repository and send shipping notification
            // await _emailSender.SendEmailAsync(userEmail, subject, body);

            _logger.LogInformation(
                "Order shipped event processed successfully - OrderId: {OrderId}",
                @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing order shipped event - OrderId: {OrderId}",
                @event.OrderId);
            throw;
        }
    }

    private static string GetTrackingLink(string carrier, string trackingNumber)
    {
        return carrier.ToLowerInvariant() switch
        {
            "fedex" => $"https://tracking.fedex.com/en/tracking/{trackingNumber}",
            "ups" => $"https://www.ups.com/track?tracknum={trackingNumber}",
            "usps" => $"https://tools.usps.com/go/TrackConfirmAction?tLabels={trackingNumber}",
            "dhl" => $"https://www.dhl.com/en/en/expressed/tracking.html?AWB={trackingNumber}",
            _ => $"https://tracking.example.com/?number={trackingNumber}"
        };
    }
}
