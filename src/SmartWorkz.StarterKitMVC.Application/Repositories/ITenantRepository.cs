using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant> GetByNameAsync(string name);
    Task<List<Tenant>> GetActiveTenantAsync();
}
