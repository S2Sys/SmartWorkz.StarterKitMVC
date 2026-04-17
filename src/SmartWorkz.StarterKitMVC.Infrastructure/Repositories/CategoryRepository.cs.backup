using Microsoft.EntityFrameworkCore;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly MasterDbContext _masterContext;

    public CategoryRepository(MasterDbContext context) : base(context)
    {
        _masterContext = context;
    }

    public async Task<Category> GetBySlugAsync(string tenantId, string slug)
    {
        return await _masterContext.Categories
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Slug == slug && !c.IsDeleted);
    }

    public async Task<List<Category>> GetRootCategoriesAsync(string tenantId)
    {
        return await _masterContext.Categories
            .Where(c => c.TenantId == tenantId && c.ParentCategoryId == null && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Category>> GetChildCategoriesAsync(string tenantId, int parentCategoryId)
    {
        return await _masterContext.Categories
            .Where(c => c.TenantId == tenantId && c.ParentCategoryId == parentCategoryId && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<Category> GetWithChildrenAsync(int categoryId)
    {
        return await _masterContext.Categories
            .Include(c => c.ChildCategories)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId && !c.IsDeleted);
    }
}
