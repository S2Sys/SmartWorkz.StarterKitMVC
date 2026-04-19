namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Menu : AuditableEntity<int>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string MenuType { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; }
    public ICollection<MenuItem> MenuItems { get; set; }
}
