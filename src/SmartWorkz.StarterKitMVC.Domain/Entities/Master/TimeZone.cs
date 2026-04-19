namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class TimeZone : AuditableEntity<int>
{
    public string Identifier { get; set; }
    public string DisplayName { get; set; }
    public string StandardName { get; set; }
    public int OffsetHours { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; }
}
