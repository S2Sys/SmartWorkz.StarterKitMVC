using System.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Localization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Translation repository for loading message keys and translations.
///
/// NOTE: The Translations table schema (EntityType, EntityId, LanguageId, FieldName, TranslatedValue)
/// doesn't match the ITranslationRepository interface contract (Key, Value, TenantId, Locale).
///
/// For now, this returns an empty list. The TranslationService will fall back to returning the key itself
/// as the translation, which is acceptable for development/testing.
///
/// TODO: Either:
/// 1. Create a proper MessageKeys table with Key/Value columns, or
/// 2. Rewrite this repository to transform Translations table data to TranslationEntry format
/// </summary>
public class DapperTranslationRepository : CachedDapperRepository, ITranslationRepository
{
    public DapperTranslationRepository(IDbConnection connection, IMemoryCache cache, ILogger<DapperTranslationRepository> logger)
        : base(connection, cache, logger)
    {
    }

    /// <summary>
    /// Returns empty list for now due to schema mismatch.
    /// TranslationService will return the key itself as fallback.
    /// </summary>
    public async Task<IEnumerable<TranslationEntry>> GetAllAsync(string tenantId, string locale)
    {
        // Return empty list to avoid SP execution error
        // The TranslationService will fall back to returning the key itself
        return await Task.FromResult(Enumerable.Empty<TranslationEntry>());
    }
}
