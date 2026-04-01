namespace SmartWorkz.StarterKitMVC.Application.Localization;

/// <summary>
/// DB-backed translation service. Resolves message keys to localised strings.
/// Lookup order: tenant+locale → tenant+default → global+locale → global+default → key itself.
/// All results are memory-cached per tenant+locale pair.
/// </summary>
public interface ITranslationService
{
    /// <summary>Resolve a key for the given tenant and locale.</summary>
    string Get(string key, string tenantId, string locale = "en-US");

    /// <summary>Resolve with string.Format-style args.</summary>
    string Get(string key, string tenantId, string locale, params object[] args);

    /// <summary>Preload all translations for a tenant+locale into cache.</summary>
    Task WarmCacheAsync(string tenantId, string locale);

    /// <summary>Evict cache for a tenant (call after admin edits translations).</summary>
    void InvalidateCache(string tenantId);
}
