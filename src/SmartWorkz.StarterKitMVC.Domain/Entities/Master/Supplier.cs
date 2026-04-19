namespace SmartWorkz.StarterKitMVC.Domain.Entities.Master;

using SmartWorkz.Core.Entities;

public class Supplier : AuditableEntity<int>
{
    public string Name { get; set; }
    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; }
    public ICollection<Inventory> Inventories { get; set; }
}
