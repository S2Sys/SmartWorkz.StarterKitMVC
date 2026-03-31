namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class Tenant
{
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public ICollection<Country> Countries { get; set; }
    public ICollection<Currency> Currencies { get; set; }
    public ICollection<Language> Languages { get; set; }
    public ICollection<TimeZone> TimeZones { get; set; }
    public ICollection<Configuration> Configurations { get; set; }
    public ICollection<FeatureFlag> FeatureFlags { get; set; }
    public ICollection<Menu> Menus { get; set; }
    public ICollection<Category> Categories { get; set; }
    public ICollection<Product> Products { get; set; }
    public ICollection<GeoHierarchy> GeoHierarchies { get; set; }
    public ICollection<GeolocationPage> GeolocationPages { get; set; }
    public ICollection<CustomPage> CustomPages { get; set; }
    public ICollection<BlogPost> BlogPosts { get; set; }
    public ICollection<Customer> Customers { get; set; }
    public ICollection<Supplier> Suppliers { get; set; }
}
