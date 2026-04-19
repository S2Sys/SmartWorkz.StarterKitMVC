using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for tenants (Shared.Tenant table)
/// </summary>
public interface ITenantRepository : IRepository<Tenant>, IDapperRepository<TenantDto>
{
    Task<Tenant> GetByNameAsync(string name);
    Task<List<Tenant>> GetActiveTenantAsync();

    /// <summary>Get tenant by code/key</summary>
    Task<TenantDto?> GetByCodeAsync(string code);

    /// <summary>Check if tenant is active</summary>
    Task<bool> IsActiveAsync(string tenantId);

    /// <summary>Get tenant with full details</summary>
    Task<TenantDto?> GetWithDetailsAsync(string tenantId);
}

/// <summary>DTO for Tenant entity</summary>
public class TenantDto
{
    public string TenantId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string LogoUrl { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Website { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string CountryCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public string SubscriptionTier { get; set; } // Free, Standard, Premium
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
