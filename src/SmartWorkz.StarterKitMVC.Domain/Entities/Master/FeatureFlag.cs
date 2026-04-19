namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class FeatureFlag : AuditableEntity<int>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; }

    public Tenant Tenant { get; set; }
}
