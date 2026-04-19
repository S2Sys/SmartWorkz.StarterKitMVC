namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class GeoHierarchy : AuditableEntity<int>
{
    public int? ParentGeoId { get; set; }
    public string Name { get; set; }
    public string GeoType { get; set; }
    public string NodePath { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; } = true;

    public GeoHierarchy ParentGeo { get; set; }
    public ICollection<GeoHierarchy> ChildGeos { get; set; }
    public Tenant Tenant { get; set; }
}
