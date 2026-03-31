using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Application.Services;

public interface ISeoMetaService
{
    Task<SeoMeta> GetByEntityAsync(string tenantId, string entityType, int entityId);
    Task<SeoMeta> GetBySlugAsync(string tenantId, string slug);
    Task<SeoMeta> CreateOrUpdateSeoMetaAsync(SeoMeta seoMeta);
    Task<bool> DeleteSeoMetaAsync(int seoMetaId);
}

public class SeoMetaService : ISeoMetaService
{
    private readonly SharedDbContext _context;

    public SeoMetaService(SharedDbContext context)
    {
        _context = context;
    }

    public async Task<SeoMeta> GetByEntityAsync(string tenantId, string entityType, int entityId)
    {
        return await _context.SeoMetas
            .FirstOrDefaultAsync(s => s.TenantId == tenantId
                && s.EntityType == entityType
                && s.EntityId == entityId
                && !s.IsDeleted);
    }

    public async Task<SeoMeta> GetBySlugAsync(string tenantId, string slug)
    {
        return await _context.SeoMetas
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Slug == slug && !s.IsDeleted);
    }

    public async Task<SeoMeta> CreateOrUpdateSeoMetaAsync(SeoMeta seoMeta)
    {
        var existing = await GetByEntityAsync(seoMeta.TenantId, seoMeta.EntityType, seoMeta.EntityId);

        if (existing != null)
        {
            existing.Title = seoMeta.Title;
            existing.Description = seoMeta.Description;
            existing.Keywords = seoMeta.Keywords;
            existing.Slug = seoMeta.Slug;
            existing.OgImage = seoMeta.OgImage;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.SeoMetas.Update(existing);
        }
        else
        {
            seoMeta.CreatedAt = DateTime.UtcNow;
            _context.SeoMetas.Add(seoMeta);
        }

        await _context.SaveChangesAsync();
        return existing ?? seoMeta;
    }

    public async Task<bool> DeleteSeoMetaAsync(int seoMetaId)
    {
        var seoMeta = await _context.SeoMetas.FindAsync(seoMetaId);
        if (seoMeta == null)
            return false;

        seoMeta.IsDeleted = true;
        seoMeta.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
