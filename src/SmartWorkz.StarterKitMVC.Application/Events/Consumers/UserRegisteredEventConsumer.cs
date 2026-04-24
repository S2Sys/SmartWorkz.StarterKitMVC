using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared.Communications;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Consumer for handling user registration events.
/// Sends a welcome email to the newly registered user.
/// </summary>
public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserRegisteredEventConsumer> _logger;

    public UserRegisteredEventConsumer(
        IEmailSender emailSender,
        IUserRepository userRepository,
        ILogger<UserRegisteredEventConsumer> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing user registration event for user {UserId} with email {Email}",
                @event.UserId,
                @event.Email);

            // Send welcome email to the new user
            var subject = "Welcome to SmartWorkz";
            var body = $@"
                <h2>Welcome to SmartWorkz!</h2>
                <p>Dear {@event.FirstName} {@event.LastName},</p>
                <p>Thank you for registering with us. Your account has been successfully created.</p>
                <p>If you have any questions, please don't hesitate to contact our support team.</p>
                <br/>
                <p>Best regards,<br/>The SmartWorkz Team</p>
            ";

            await _emailSender.SendEmailAsync(@event.Email, subject, body);

            _logger.LogInformation(
                "Welcome email sent successfully to user {UserId} ({Email})",
                @event.UserId,
                @event.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing user registration event for user {UserId}",
                @event.UserId);
            throw;
        }
    }
}
