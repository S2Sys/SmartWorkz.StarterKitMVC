using SmartWorkz.Core;

namespace SmartWorkz.Sample.ECommerce.Domain.Entities;

public class OrderItem : AuditableEntity<int>, IEntity<int>
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Money? UnitPrice { get; set; }

    public decimal? GetLineTotal()
    {
        return UnitPrice?.Amount * Quantity;
    }
}
