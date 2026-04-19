namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Currency : AuditableEntity<int>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public int DecimalPlaces { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Tenant Tenant { get; set; }
}
