using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using System.Web;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Consumer for handling order processed events.
/// Sends order confirmation email and logs the order processing.
/// </summary>
public class SendOrderConfirmationConsumer : IConsumer<OrderProcessedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SendOrderConfirmationConsumer> _logger;

    public SendOrderConfirmationConsumer(
        IEmailSender emailSender,
        IUserRepository userRepository,
        ILogger<SendOrderConfirmationConsumer> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

            // Fetch user details for email
            var user = await _userRepository.GetByIdAsync(@event.UserId);
            if (user == null)
            {
                _logger.LogWarning(
                    "User {UserId} not found for order {OrderId}",
                    @event.UserId,
                    @event.OrderId);
                return;
            }

            // Send order confirmation email
            var subject = $"Order Confirmation - Order #{@event.OrderId}";
            var escapedUserName = HttpUtility.HtmlEncode(user.DisplayName ?? user.Username);

            var body = $@"
                <h2>Order Confirmation</h2>
                <p>Dear {escapedUserName},</p>
                <p>Thank you for your order!</p>
                <p><strong>Order ID:</strong> {@event.OrderId}</p>
                <p><strong>Amount:</strong> ${@event.Amount:F2}</p>
                <p>You can track your order status in your account dashboard.</p>
                <p>If you have any questions about your order, please contact our customer support team.</p>
                <br/>
                <p>Best regards,<br/>The SmartWorkz Team</p>
            ";

            var result = await _emailSender.SendAsync(user.Email, subject, body, isHtml: true);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Failed to send order confirmation email for order {OrderId}: {Error}",
                    @event.OrderId,
                    result.MessageKey);
                throw new InvalidOperationException($"Email sending failed: {result.MessageKey}");
            }

            _logger.LogInformation(
                "Order confirmation sent for order {OrderId}",
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
