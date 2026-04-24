namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published when a user requests a password reset.
/// </summary>
public class PasswordResetRequestedEvent
{
    public PasswordResetRequestedEvent(string userId, string email, string resetToken, DateTime expiresAt)
    {
        UserId = userId;
        Email = email;
        ResetToken = resetToken;
        ExpiresAt = expiresAt;
        RequestedAt = DateTime.UtcNow;
    }

    public string UserId { get; }
    public string Email { get; }
    public string ResetToken { get; }
    public DateTime ExpiresAt { get; }
    public DateTime RequestedAt { get; }
}
