using SmartWorkz.StarterKitMVC.Shared.DTOs;

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
