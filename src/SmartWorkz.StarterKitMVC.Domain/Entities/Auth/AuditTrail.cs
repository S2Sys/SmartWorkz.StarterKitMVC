namespace SmartWorkz.StarterKitMVC.Domain.Entities.Auth;

public class AuditTrail
{
    public long AuditTrailId { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public int? EntityId { get; set; }
    public string Changes { get; set; }
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; }
}
