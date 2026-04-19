using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for hierarchical lookup values (Master.Lookup table)
/// </summary>
public interface ILookupRepository : IDapperRepository<LookupDto>
{
    /// <summary>Get lookup values by category key</summary>
    Task<IEnumerable<LookupDto>> GetByCategoryAsync(string categoryKey, string tenantId);

    /// <summary>Get all currency lookups</summary>
    Task<IEnumerable<LookupDto>> GetCurrenciesAsync(string tenantId);

    /// <summary>Get all language lookups</summary>
    Task<IEnumerable<LookupDto>> GetLanguagesAsync(string tenantId);

    /// <summary>Get all timezone lookups</summary>
    Task<IEnumerable<LookupDto>> GetTimeZonesAsync(string tenantId);

    /// <summary>Get lookup ID by category and key</summary>
    Task<Guid?> GetIdByCategoryAndKeyAsync(string categoryKey, string key, string tenantId);
}
