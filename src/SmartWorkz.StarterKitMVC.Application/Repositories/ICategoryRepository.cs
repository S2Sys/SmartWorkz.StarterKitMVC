using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category> GetBySlugAsync(string tenantId, string slug);
    Task<List<Category>> GetRootCategoriesAsync(string tenantId);
    Task<List<Category>> GetChildCategoriesAsync(string tenantId, int parentCategoryId);
    Task<Category> GetWithChildrenAsync(int categoryId);
}
