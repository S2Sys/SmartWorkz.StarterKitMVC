using SmartWorkz.StarterKitMVC.Domain.Localization;

namespace SmartWorkz.StarterKitMVC.Application.Localization;

/// <summary>
/// Service for managing localization resources
/// </summary>
public interface IResourceService
{
    #region Languages
    
    Task<List<Language>> GetAllLanguagesAsync(CancellationToken ct = default);
    Task<List<Language>> GetActiveLanguagesAsync(CancellationToken ct = default);
    Task<Language?> GetLanguageByCodeAsync(string code, CancellationToken ct = default);
    Task<Language> CreateLanguageAsync(Language language, CancellationToken ct = default);
    Task<Language> UpdateLanguageAsync(Language language, CancellationToken ct = default);
    Task DeleteLanguageAsync(string code, CancellationToken ct = default);
    Task<string> GetDefaultLanguageCodeAsync(CancellationToken ct = default);
    
    #endregion
    
    #region Resources
    
    Task<List<Resource>> GetAllResourcesAsync(CancellationToken ct = default);
    Task<List<Resource>> GetResourceTreeAsync(CancellationToken ct = default);
    Task<List<Resource>> GetResourcesByModuleAsync(string module, CancellationToken ct = default);
    Task<List<Resource>> GetResourcesByCategoryAsync(string category, CancellationToken ct = default);
    Task<Resource?> GetResourceByIdAsync(Guid id, CancellationToken ct = default);
    Task<Resource?> GetResourceByKeyAsync(string key, CancellationToken ct = default);
    Task<Resource> CreateResourceAsync(Resource resource, CancellationToken ct = default);
    Task<Resource> UpdateResourceAsync(Resource resource, CancellationToken ct = default);
    Task DeleteResourceAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Synchronizes default resources with existing data.
    /// Adds any new resources that don't exist yet without overwriting existing ones.
    /// Call this on application startup to ensure all resources are available.
    /// </summary>
    Task SyncDefaultResourcesAsync(CancellationToken ct = default);
    
    #endregion
    
    #region Translations
    
    Task<ResourceTranslation?> GetTranslationAsync(Guid resourceId, string languageCode, CancellationToken ct = default);
    Task<ResourceTranslation?> GetTranslationByKeyAsync(string resourceKey, string languageCode, CancellationToken ct = default);
    Task<List<ResourceTranslation>> GetTranslationsForResourceAsync(Guid resourceId, CancellationToken ct = default);
    Task<List<ResourceTranslation>> GetTranslationsForLanguageAsync(string languageCode, CancellationToken ct = default);
    Task<ResourceTranslation> SetTranslationAsync(Guid resourceId, string languageCode, string value, string? pluralValue = null, CancellationToken ct = default);
    Task<ResourceTranslation> UpdateTranslationStatusAsync(Guid translationId, TranslationStatus status, string? notes = null, CancellationToken ct = default);
    Task DeleteTranslationAsync(Guid translationId, CancellationToken ct = default);
    
    /// <summary>
    /// Get translated value with fallback to default language
    /// </summary>
    Task<string> GetValueAsync(string resourceKey, string languageCode, CancellationToken ct = default);
    
    /// <summary>
    /// Get translated value with placeholder replacement
    /// </summary>
    Task<string> GetValueAsync(string resourceKey, string languageCode, params object[] args);
    
    /// <summary>
    /// Get all translations for a language as a dictionary (for caching)
    /// </summary>
    Task<Dictionary<string, string>> GetAllTranslationsAsync(string languageCode, CancellationToken ct = default);
    
    /// <summary>
    /// Get translation statistics
    /// </summary>
    Task<TranslationStats> GetTranslationStatsAsync(string languageCode, CancellationToken ct = default);
    
    #endregion
    
    #region Import/Export
    
    Task<string> ExportAsync(string? languageCode = null, CancellationToken ct = default);
    Task<int> ImportAsync(string json, bool overwrite = false, CancellationToken ct = default);
    
    #endregion
}

/// <summary>
/// Translation statistics for a language
/// </summary>
public class TranslationStats
{
    public string LanguageCode { get; set; } = string.Empty;
    public int TotalResources { get; set; }
    public int TranslatedCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int NeedsReviewCount { get; set; }
    public double CompletionPercentage => TotalResources > 0 ? (TranslatedCount * 100.0 / TotalResources) : 0;
}
