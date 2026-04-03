using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration   _configuration;
    private string[]? _supportedLocales;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

    public TranslationService(IServiceProvider services, IMemoryCache cache, IConfiguration configuration)
    {
        _services = services;
        _cache    = cache;
        _configuration = configuration;
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

    public async Task RefreshCacheAsync(string tenantId, string locale)
    {
        _cache.Remove(BucketKey(tenantId, locale));
        await WarmCacheAsync(tenantId, locale);
    }

    public void InvalidateCache(string tenantId)
    {
        // IMemoryCache has no prefix scan — evict all known locale+tenant pairs
        // Read supported locales from config, fallback to default if not configured
        _supportedLocales ??= _configuration
            .GetSection("Features:Localization:SupportedCultures")
            .Get<string[]>() ?? new[] { "en-US" };

        var tenants = new[] { "DEFAULT", "DEMO" }; // Known seeded tenants

        foreach (var locale in _supportedLocales)
            foreach (var tenant in tenants)
                _cache.Remove(BucketKey(tenant, locale));
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
