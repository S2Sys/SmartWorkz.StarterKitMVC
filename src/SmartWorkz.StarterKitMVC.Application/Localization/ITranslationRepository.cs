namespace SmartWorkz.StarterKitMVC.Application.Localization;

public interface ITranslationRepository
{
    /// <summary>Load all active translations for a tenant+locale pair (includes global fallbacks).</summary>
    Task<IEnumerable<TranslationEntry>> GetAllAsync(string tenantId, string locale);
}

public record TranslationEntry(
    string Key,
    string Value,
    string TenantId,
    string Locale);
