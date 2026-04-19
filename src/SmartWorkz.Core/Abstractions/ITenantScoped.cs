namespace SmartWorkz.Core.Abstractions;

/// <summary>
/// Marks an entity that belongs to a tenant.
/// Infrastructure layer should automatically scope queries to the current tenant.
/// </summary>
public interface ITenantScoped
{
    string? TenantId { get; set; }
}
