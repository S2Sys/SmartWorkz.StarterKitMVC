using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Application.Services;

public interface ISeoMetaService
{
    Task<SeoMeta> GetByEntityAsync(string tenantId, string entityType, int entityId);
    Task<SeoMeta> GetBySlugAsync(string tenantId, string slug);
    Task<SeoMeta> CreateOrUpdateSeoMetaAsync(SeoMeta seoMeta);
    Task<bool> DeleteSeoMetaAsync(int seoMetaId);
}
