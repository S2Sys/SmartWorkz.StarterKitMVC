namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Configuration : AuditableEntity<int>
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string ConfigType { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; }
}
