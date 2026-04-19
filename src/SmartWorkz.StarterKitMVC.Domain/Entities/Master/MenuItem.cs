namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class MenuItem : AuditableEntity<int>
{
    public int MenuId { get; set; }
    public string Title { get; set; }
    public string URL { get; set; }
    public string Icon { get; set; }
    public int DisplayOrder { get; set; }
    public string NodePath { get; set; }
    public string RequiredRole { get; set; }
    public bool IsActive { get; set; } = true;

    public Menu Menu { get; set; }
    public Tenant Tenant { get; set; }
}
