using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> GetBySkuAsync(string tenantId, string sku);
    Task<Product> GetBySlugAsync(string tenantId, string slug);
    Task<List<Product>> GetByCategoryAsync(string tenantId, int categoryId);
    Task<List<Product>> GetFeaturedProductsAsync(string tenantId, int take = 10);
    Task<List<Product>> SearchAsync(string tenantId, string searchTerm);
}
