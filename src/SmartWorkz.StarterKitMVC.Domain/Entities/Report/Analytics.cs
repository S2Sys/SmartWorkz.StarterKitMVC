namespace SmartWorkz.StarterKitMVC.Domain.Entities.Report;

public class Analytics
{
    public long AnalyticsId { get; set; }
    public string EventName { get; set; }
    public string EntityType { get; set; }
    public int? EntityId { get; set; }
    public string UserId { get; set; }
    public string EventData { get; set; }
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
