namespace SmartWorkz.StarterKitMVC.Application.Repositories;

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

/// <summary>DTO for BlogPost entity</summary>
public class BlogPostDto
{
    public Guid BlogPostId { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public string AuthorId { get; set; }
    public string Tags { get; set; } // Comma-separated or JSON array
    public string TenantId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
