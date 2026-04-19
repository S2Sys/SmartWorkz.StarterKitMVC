namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

public class Currency
{
    public int CurrencyId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public int DecimalPlaces { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; }
}
