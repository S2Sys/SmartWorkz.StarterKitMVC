using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Consumer for handling user profile updated events.
/// Logs profile changes and optionally sends a notification.
/// </summary>
public class UserProfileUpdatedEventConsumer : IConsumer<UserProfileUpdatedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserProfileUpdatedEventConsumer> _logger;

    public UserProfileUpdatedEventConsumer(
        IEmailSender emailSender,
        ILogger<UserProfileUpdatedEventConsumer> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<UserProfileUpdatedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing user profile updated event - UserId: {UserId}, Email: {Email}",
                @event.UserId,
                @event.Email);

            // Send profile update confirmation email
            var subject = "Your Profile Has Been Updated";
            var body = $@"
                <h2>Profile Update Confirmation</h2>
                <p>Dear {@event.FirstName} {@event.LastName},</p>
                <p>This is to confirm that your profile has been successfully updated.</p>
                <p><strong>Updated Information:</strong></p>
                <ul>
                    <li>Name: {@event.FirstName} {@event.LastName}</li>
                    <li>Email: {@event.Email}</li>
                    {(!string.IsNullOrEmpty(@event.PhoneNumber) ? $"<li>Phone: {@event.PhoneNumber}</li>" : "")}
                </ul>
                <p>If you didn't make these changes or have concerns about your account security, please contact our support team immediately.</p>
                <br/>
                <p>Best regards,<br/>The SmartWorkz Team</p>
            ";

            await _emailSender.SendAsync(@event.Email, subject, body, isHtml: true);

            _logger.LogInformation(
                "Profile update confirmation email sent to user {UserId}",
                @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing user profile updated event for user {UserId}",
                @event.UserId);
            throw;
        }
    }
}
