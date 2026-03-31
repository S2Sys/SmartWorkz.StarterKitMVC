namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public int Quantity { get; set; }
    public DateTime LastRestockDate { get; set; }
    public decimal CostPrice { get; set; }
    public string WarehouseLocation { get; set; }
    public string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public Product Product { get; set; }
    public Supplier Supplier { get; set; }
    public Tenant Tenant { get; set; }
}
