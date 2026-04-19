using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for managing hierarchical lookup values with caching support.
/// Provides access to currencies, languages, timezones, and custom categories.
/// </summary>
public interface ILookupService
{
    /// <summary>
    /// Gets all currency lookup values for a tenant.
    /// Results are cached for 24 hours.
    /// </summary>
    Task<IEnumerable<LookupDto>> GetCurrenciesAsync(string tenantId);

    /// <summary>
    /// Gets all language lookup values for a tenant.
    /// Results are cached for 24 hours.
    /// </summary>
    Task<IEnumerable<LookupDto>> GetLanguagesAsync(string tenantId);

    /// <summary>
    /// Gets all timezone lookup values for a tenant.
    /// Results are cached for 24 hours.
    /// </summary>
    Task<IEnumerable<LookupDto>> GetTimeZonesAsync(string tenantId);

    /// <summary>
    /// Gets lookup values by category key with caching.
    /// Cache key: lookups_{categoryKey}_{tenantId}
    /// </summary>
    Task<IEnumerable<LookupDto>> GetByCategoryAsync(string categoryKey, string tenantId);

    /// <summary>
    /// Creates or updates a lookup value and invalidates related cache entries.
    /// </summary>
    Task<LookupDto> UpsertAsync(LookupDto lookup);

    /// <summary>
    /// Deletes a lookup value by ID and invalidates cache.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}
