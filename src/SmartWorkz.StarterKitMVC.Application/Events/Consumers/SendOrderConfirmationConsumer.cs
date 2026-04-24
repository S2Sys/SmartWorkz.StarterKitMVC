using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Consumer for handling order processed events.
/// Sends order confirmation email and logs the order processing.
/// </summary>
public class SendOrderConfirmationConsumer : IConsumer<OrderProcessedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendOrderConfirmationConsumer> _logger;

    public SendOrderConfirmationConsumer(
        IEmailSender emailSender,
        ILogger<SendOrderConfirmationConsumer> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<OrderProcessedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing order processed event - OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}",
                @event.OrderId,
                @event.UserId,
                @event.Amount);

            // Send order confirmation email
            // Note: In a real scenario, you would fetch user email from repository
            var subject = $"Order Confirmation - Order #{@event.OrderId}";
            var body = $@"
                <h2>Order Confirmation</h2>
                <p>Thank you for your order!</p>
                <p><strong>Order ID:</strong> {@event.OrderId}</p>
                <p><strong>Amount:</strong> ${@event.Amount:F2}</p>
                <p>You can track your order status in your account dashboard.</p>
                <p>If you have any questions about your order, please contact our customer support team.</p>
                <br/>
                <p>Best regards,<br/>The SmartWorkz Team</p>
            ";

            // TODO: Get user email from user repository and send confirmation
            // await _emailSender.SendEmailAsync(userEmail, subject, body);

            _logger.LogInformation(
                "Order created event processed successfully - OrderId: {OrderId}",
                @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing order created event - OrderId: {OrderId}",
                @event.OrderId);
            throw;
        }
    }
}
