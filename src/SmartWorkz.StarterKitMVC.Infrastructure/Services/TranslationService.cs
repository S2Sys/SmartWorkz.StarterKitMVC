using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Localization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Services;

/// <summary>
/// Singleton translation service. Uses IServiceProvider to resolve the scoped
/// ITranslationRepository only when a cache miss occurs.
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly IServiceProvider _services;
    private readonly IMemoryCache     _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

    public TranslationService(IServiceProvider services, IMemoryCache cache)
    {
        _services = services;
        _cache    = cache;
    }

    public string Get(string key, string tenantId, string locale = "en-US")
    {
        var dict = GetCachedDict(tenantId, locale);

        // Lookup order: tenant+locale → global+locale → key itself
        return dict.TryGetValue(CacheKey(tenantId, locale, key), out var v1) ? v1
             : dict.TryGetValue(CacheKey("global",   locale, key), out var v2) ? v2
             : key;
    }

    public string Get(string key, string tenantId, string locale, params object[] args)
    {
        var template = Get(key, tenantId, locale);
        try   { return string.Format(template, args); }
        catch { return template; }
    }

    public async Task WarmCacheAsync(string tenantId, string locale)
    {
        var cacheKey = BucketKey(tenantId, locale);
        if (_cache.TryGetValue(cacheKey, out _)) return;

        using var scope   = _services.CreateScope();
        var repo          = scope.ServiceProvider.GetRequiredService<ITranslationRepository>();
        var entries       = await repo.GetAllAsync(tenantId, locale);
        var dict          = BuildDict(entries);
        _cache.Set(cacheKey, dict, CacheDuration);
    }

    public void InvalidateCache(string tenantId)
    {
        // IMemoryCache has no prefix scan — use a version token approach
        // For now evict known locales; in production use IMemoryCache with tagged keys
        foreach (var locale in new[] { "en-US", "en-GB", "fr-FR", "de-DE", "ar-SA" })
            _cache.Remove(BucketKey(tenantId, locale));
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private Dictionary<string, string> GetCachedDict(string tenantId, string locale)
    {
        var key = BucketKey(tenantId, locale);
        if (_cache.TryGetValue(key, out Dictionary<string, string>? dict) && dict != null)
            return dict;

        // Synchronous fallback on first miss — creates a short-lived scope
        using var scope = _services.CreateScope();
        var repo        = scope.ServiceProvider.GetRequiredService<ITranslationRepository>();
        var entries     = repo.GetAllAsync(tenantId, locale).GetAwaiter().GetResult();
        dict = BuildDict(entries);
        _cache.Set(key, dict, CacheDuration);
        return dict;
    }

    private static Dictionary<string, string> BuildDict(IEnumerable<TranslationEntry> entries)
        => entries.ToDictionary(
            e => CacheKey(e.TenantId, e.Locale, e.Key),
            e => e.Value,
            StringComparer.OrdinalIgnoreCase);

    private static string BucketKey(string tenantId, string locale)
        => $"translations:{tenantId}:{locale}";

    private static string CacheKey(string tenantId, string locale, string key)
        => $"{tenantId}:{locale}:{key}";
}
