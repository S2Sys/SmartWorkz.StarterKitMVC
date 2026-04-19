using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

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
