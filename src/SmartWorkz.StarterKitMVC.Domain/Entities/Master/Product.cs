namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Product : AuditableEntity<int>
{
    public int CategoryId { get; set; }
    public string SKU { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; }
    public bool IsFeatured { get; set; }
    public int Views { get; set; }
    public int Downloads { get; set; }
    public bool IsActive { get; set; } = true;

    public Category Category { get; set; }
    public Tenant Tenant { get; set; }
    public ICollection<Inventory> Inventories { get; set; }
}
