namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

public class Notification
{
    public int NotificationId { get; set; }
    public string NotificationType { get; set; }
    public string RecipientType { get; set; }
    public string RecipientId { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
