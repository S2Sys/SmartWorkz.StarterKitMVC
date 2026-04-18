using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for custom pages (Master.CustomPage table)
/// </summary>
public interface ICustomPageRepository : IDapperRepository<CustomPageDto>
{
    /// <summary>Get custom page by slug</summary>
    Task<CustomPageDto?> GetBySlugAsync(string slug, string tenantId);

    /// <summary>Get custom page by name</summary>
    Task<CustomPageDto?> GetByNameAsync(string name, string tenantId);

    /// <summary>Get all published pages</summary>
    Task<IEnumerable<CustomPageDto>> GetPublishedAsync(string tenantId);

    /// <summary>Search custom pages</summary>
    Task<IEnumerable<CustomPageDto>> SearchAsync(string searchTerm, string tenantId);
}
