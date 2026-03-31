using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> GetBySkuAsync(string tenantId, string sku);
    Task<Product> GetBySlugAsync(string tenantId, string slug);
    Task<List<Product>> GetByCategoryAsync(string tenantId, int categoryId);
    Task<List<Product>> GetFeaturedProductsAsync(string tenantId, int take = 10);
    Task<List<Product>> SearchAsync(string tenantId, string searchTerm);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly MasterDbContext _masterContext;

    public ProductRepository(MasterDbContext context) : base(context)
    {
        _masterContext = context;
    }

    public async Task<Product> GetBySkuAsync(string tenantId, string sku)
    {
        return await _masterContext.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.SKU == sku && !p.IsDeleted);
    }

    public async Task<Product> GetBySlugAsync(string tenantId, string slug)
    {
        return await _masterContext.Products
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Slug == slug && !p.IsDeleted);
    }

    public async Task<List<Product>> GetByCategoryAsync(string tenantId, int categoryId)
    {
        return await _masterContext.Products
            .Where(p => p.TenantId == tenantId && p.CategoryId == categoryId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Product>> GetFeaturedProductsAsync(string tenantId, int take = 10)
    {
        return await _masterContext.Products
            .Where(p => p.TenantId == tenantId && p.IsFeatured && !p.IsDeleted)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchAsync(string tenantId, string searchTerm)
    {
        return await _masterContext.Products
            .Where(p => p.TenantId == tenantId
                && !p.IsDeleted
                && (p.Name.Contains(searchTerm)
                    || p.Description.Contains(searchTerm)
                    || p.SKU.Contains(searchTerm)))
            .ToListAsync();
    }
}
