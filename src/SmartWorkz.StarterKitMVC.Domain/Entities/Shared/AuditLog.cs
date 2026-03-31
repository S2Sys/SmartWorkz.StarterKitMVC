namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public string Action { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string IPAddress { get; set; }
    public string TenantId { get; set; }
}
