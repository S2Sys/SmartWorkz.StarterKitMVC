namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for countries (Master.Country table)
/// </summary>
public interface ICountryRepository : IDapperRepository<CountryDto>
{
    /// <summary>Get country by ISO code</summary>
    Task<CountryDto?> GetByCodeAsync(string code, string tenantId);

    /// <summary>Get country by name</summary>
    Task<CountryDto?> GetByNameAsync(string name, string tenantId);

    /// <summary>Get all active countries</summary>
    Task<IEnumerable<CountryDto>> GetAllActiveAsync(string tenantId);

    /// <summary>Search countries by code or name</summary>
    Task<IEnumerable<CountryDto>> SearchAsync(string searchTerm, string tenantId);
}

/// <summary>DTO for Country entity</summary>
public class CountryDto
{
    public int CountryId { get; set; }
    public string Code { get; set; } // ISO 3166-1 alpha-2
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string FlagEmoji { get; set; }
    public string TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
