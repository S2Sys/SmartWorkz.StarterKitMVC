namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class GeoHierarchy
{
    public int GeoId { get; set; }
    public int? ParentGeoId { get; set; }
    public string Name { get; set; }
    public string GeoType { get; set; }
    public string NodePath { get; set; }
    public int Level { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public GeoHierarchy ParentGeo { get; set; }
    public ICollection<GeoHierarchy> ChildGeos { get; set; }
    public Tenant Tenant { get; set; }
}
