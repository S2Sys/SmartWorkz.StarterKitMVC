namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Country : AuditableEntity<int>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string FlagEmoji { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Tenant Tenant { get; set; }
}
