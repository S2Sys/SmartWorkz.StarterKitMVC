namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

using SmartWorkz.Core.Entities;

public class Notification : AuditableEntity<int>
{
    public string NotificationType { get; set; }
    public string RecipientType { get; set; }
    public string RecipientId { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
