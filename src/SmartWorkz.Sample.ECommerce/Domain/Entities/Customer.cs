using SmartWorkz.Core;

namespace SmartWorkz.Sample.ECommerce.Domain.Entities;

public class Customer : AuditableEntity<int>, IEntity<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public EmailAddress Email { get; set; } = null!;
    public string EmailValue { get; set; } = string.Empty;  // Persisted for queries
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
