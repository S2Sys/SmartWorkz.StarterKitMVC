namespace SmartWorkz.StarterKitMVC.Application.Repositories;

using SmartWorkz.StarterKitMVC.Shared.DTOs;

/// <summary>
/// Repository interface for blog posts (Master.BlogPost table)
/// </summary>
public interface IBlogPostRepository : IDapperRepository<BlogPostDto>
{
    /// <summary>Get published blog posts for a tenant</summary>
    Task<IEnumerable<BlogPostDto>> GetPublishedAsync(string tenantId);

    /// <summary>Get blog posts by author</summary>
    Task<IEnumerable<BlogPostDto>> GetByAuthorAsync(string authorId, string tenantId);

    /// <summary>Search blog posts by title or content</summary>
    Task<IEnumerable<BlogPostDto>> SearchAsync(string searchTerm, string tenantId);

    /// <summary>Get paged blog posts</summary>
    Task<(IEnumerable<BlogPostDto> Items, int Total)> GetPagedAsync(
        string tenantId, bool? published = null, int pageNumber = 1, int pageSize = 20);

    /// <summary>Get blog post by slug</summary>
    Task<BlogPostDto?> GetBySlugAsync(string slug, string tenantId);
}
