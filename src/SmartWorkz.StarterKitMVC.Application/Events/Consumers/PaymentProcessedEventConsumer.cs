using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Consumer for handling payment processed events.
/// Sends payment confirmation email and logs payment details.
/// </summary>
public class PaymentProcessedEventConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<PaymentProcessedEventConsumer> _logger;

    public PaymentProcessedEventConsumer(
        IEmailSender emailSender,
        ILogger<PaymentProcessedEventConsumer> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing payment processed event - PaymentId: {PaymentId}, OrderId: {OrderId}, Amount: {Amount}, Status: {Status}",
                @event.PaymentId,
                @event.OrderId,
                @event.Amount,
                @event.Status);

            if (@event.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                // Send payment confirmation email
                // Note: In a real scenario, you would fetch user email from repository
                var subject = $"Payment Confirmed - Order #{@event.OrderId}";
                var body = $@"
                    <h2>Payment Confirmation</h2>
                    <p>Your payment has been successfully processed.</p>
                    <p><strong>Order ID:</strong> {@event.OrderId}</p>
                    <p><strong>Payment ID:</strong> {@event.PaymentId}</p>
                    <p><strong>Amount:</strong> ${@event.Amount:F2}</p>
                    <p><strong>Status:</strong> {@event.Status}</p>
                    {(!string.IsNullOrEmpty(@event.TransactionId) ? $"<p><strong>Transaction ID:</strong> {@event.TransactionId}</p>" : "")}
                    <p>Thank you for your payment. Your order will be processed shortly.</p>
                    <br/>
                    <p>Best regards,<br/>The SmartWorkz Team</p>
                ";

                // TODO: Get user email from user repository and send payment confirmation
                // await _emailSender.SendEmailAsync(userEmail, subject, body);
            }
            else if (@event.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Payment processing failed - PaymentId: {PaymentId}, OrderId: {OrderId}",
                    @event.PaymentId,
                    @event.OrderId);
            }

            _logger.LogInformation(
                "Payment processed event handled successfully - PaymentId: {PaymentId}",
                @event.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing payment processed event - PaymentId: {PaymentId}",
                @event.PaymentId);
            throw;
        }
    }
}
