namespace SmartWorkz.StarterKitMVC.Domain.MultiTenancy;

/// <summary>
/// Represents a tenant in a multi-tenant application.
/// </summary>
/// <example>
/// <code>
/// var tenant = new Tenant
/// {
///     Id = "acme-corp",
///     Name = "Acme Corporation",
///     Subdomain = "acme",
///     IsActive = true
/// };
/// </code>
/// </example>
public sealed class Tenant
{
    /// <summary>Unique tenant identifier.</summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>Tenant display name.</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>Subdomain for tenant-specific URLs (e.g., "acme" for acme.example.com).</summary>
    public string? Subdomain { get; init; }
    
    /// <summary>Whether the tenant is active.</summary>
    public bool IsActive { get; init; } = true;
}
