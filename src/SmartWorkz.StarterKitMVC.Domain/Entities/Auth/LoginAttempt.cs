namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class LoginAttempt
{
    public long LoginAttemptId { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
    public bool IsSuccessful { get; set; }
    public string FailureReason { get; set; }
    public string TenantId { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; }
}
