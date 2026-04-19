namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Language : AuditableEntity<int>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string NativeName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; }
}
