namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class TimeZone
{
    public int TimeZoneId { get; set; }
    public string Identifier { get; set; }
    public string DisplayName { get; set; }
    public string StandardName { get; set; }
    public int OffsetHours { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Tenant Tenant { get; set; }
}
