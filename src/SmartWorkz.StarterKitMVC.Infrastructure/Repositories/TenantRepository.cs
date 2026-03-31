using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    private readonly MasterDbContext _masterContext;

    public TenantRepository(MasterDbContext context) : base(context)
    {
        _masterContext = context;
    }

    public async Task<Tenant> GetByNameAsync(string name)
    {
        return await _masterContext.Tenants
            .FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted);
    }

    public async Task<List<Tenant>> GetActiveTenantAsync()
    {
        return await _masterContext.Tenants
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync();
    }
}
