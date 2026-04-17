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

/// <summary>DTO for Lookup entity</summary>
public class LookupDto
{
    public Guid LookupId { get; set; }
    public string CategoryKey { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string DisplayName { get; set; }
    public int SortOrder { get; set; } = 0;
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
