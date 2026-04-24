using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.Shared;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Consumer for handling password reset requested events.
/// Sends a password reset email with reset link to the user.
/// </summary>
public class PasswordResetRequestedEventConsumer : IConsumer<PasswordResetRequestedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PasswordResetRequestedEventConsumer> _logger;

    public PasswordResetRequestedEventConsumer(
        IEmailSender emailSender,
        IUserRepository userRepository,
        ILogger<PasswordResetRequestedEventConsumer> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<PasswordResetRequestedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing password reset request event for user {UserId} ({Email})",
                @event.UserId,
                @event.Email);

            // Send password reset email
            var subject = "Password Reset Request";
            var resetLink = $"https://localhost:7002/Account/ResetPassword?token={Uri.EscapeDataString(@event.ResetToken)}";

            var body = $@"
                <h2>Password Reset Request</h2>
                <p>We received a request to reset the password for your account.</p>
                <p>If you didn't make this request, you can safely ignore this email.</p>
                <p>To reset your password, <a href=""{resetLink}"">click here</a> or copy and paste this link into your browser:</p>
                <p>{resetLink}</p>
                <p>This link will expire at {Uri.EscapeDataString(@event.ExpiresAt.ToString("g"))}</p>
                <p>For security reasons, never share this link with anyone.</p>
                <br/>
                <p>Best regards,<br/>The SmartWorkz Team</p>
            ";

            await _emailSender.SendAsync(@event.Email, subject, body, isHtml: true);

            _logger.LogInformation(
                "Password reset email sent successfully to user {UserId} ({Email})",
                @event.UserId,
                @event.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing password reset request event for user {UserId}",
                @event.UserId);
            throw;
        }
    }
}
