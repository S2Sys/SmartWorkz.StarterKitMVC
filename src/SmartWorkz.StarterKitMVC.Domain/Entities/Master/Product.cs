namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class Product
{
    public int ProductId { get; set; }
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
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Category Category { get; set; }
    public Tenant Tenant { get; set; }
    public ICollection<Inventory> Inventories { get; set; }
}
