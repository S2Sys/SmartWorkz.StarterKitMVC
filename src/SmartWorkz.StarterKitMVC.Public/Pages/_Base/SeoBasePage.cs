using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Public.Pages;

/// <summary>
/// Extends BasePage with automatic SEO meta loading from DB.
/// Sets ViewData Title, Description, Keywords, OG tags.
/// Usage: call LoadSeoAsync(entityType, entityId) in OnGetAsync.
/// For static pages: call SetSeo(title, description).
/// </summary>
public abstract class SeoBasePage : BasePage
{
    private ISeoMetaService? _seoMetaService;

    protected ISeoMetaService SeoMetaService =>
        _seoMetaService ??= (ISeoMetaService)HttpContext.RequestServices
            .GetService(typeof(ISeoMetaService))!;

    public SeoMeta? SeoMeta { get; private set; }

    protected async Task LoadSeoAsync(string entityType, int entityId)
    {
        SeoMeta = await SeoMetaService.GetByEntityAsync(TenantId, entityType, entityId);
        ApplySeo(SeoMeta);
    }

    protected async Task LoadSeoBySlugAsync(string slug)
    {
        SeoMeta = await SeoMetaService.GetBySlugAsync(TenantId, slug);
        ApplySeo(SeoMeta);
    }

    protected void SetSeo(string title, string? description = null)
    {
        ViewData["Title"]         = title;
        ViewData["Description"]   = description;
        ViewData["OgTitle"]       = title;
        ViewData["OgDescription"] = description;
    }

    private void ApplySeo(SeoMeta? meta)
    {
        if (meta == null) return;
        ViewData["Title"]         = meta.Title;
        ViewData["Description"]   = meta.Description;
        ViewData["Keywords"]      = meta.Keywords;
        ViewData["OgTitle"]       = !string.IsNullOrEmpty(meta.OgTitle)       ? meta.OgTitle       : meta.Title;
        ViewData["OgDescription"] = !string.IsNullOrEmpty(meta.OgDescription) ? meta.OgDescription : meta.Description;
        ViewData["OgImage"]       = meta.OgImageUrl;
        ViewData["SeoMeta"]       = meta;
    }
}
