namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Inventory : AuditableEntity<int>
{
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public int Quantity { get; set; }
    public DateTime LastRestockDate { get; set; }
    public decimal CostPrice { get; set; }
    public string WarehouseLocation { get; set; }

    public Product Product { get; set; }
    public Supplier Supplier { get; set; }
    public Tenant Tenant { get; set; }
}
