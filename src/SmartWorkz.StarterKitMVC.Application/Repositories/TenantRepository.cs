using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant> GetBySlugAsync(string slug);
    Task<List<Tenant>> GetActiveTenantAsync();
}

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    private readonly MasterDbContext _masterContext;

    public TenantRepository(MasterDbContext context) : base(context)
    {
        _masterContext = context;
    }

    public async Task<Tenant> GetBySlugAsync(string slug)
    {
        return await _masterContext.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug && !t.IsDeleted);
    }

    public async Task<List<Tenant>> GetActiveTenantAsync()
    {
        return await _masterContext.Tenants
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync();
    }
}
