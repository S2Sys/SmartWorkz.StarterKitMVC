using SmartWorkz.Core;

namespace SmartWorkz.Sample.ECommerce.Domain.Entities;

public class Customer : AuditEntity<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public EmailAddress Email { get; set; } = null!;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
