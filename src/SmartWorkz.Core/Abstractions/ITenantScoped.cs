namespace SmartWorkz.Core;

/// <summary>
/// Marks an entity that belongs to a tenant.
/// Infrastructure layer should automatically scope queries to the current tenant.
/// </summary>
public interface ITenantScoped
{
    int? TenantId { get; set; }
}
