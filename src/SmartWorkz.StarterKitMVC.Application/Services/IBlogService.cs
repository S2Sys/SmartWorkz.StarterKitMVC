using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Application.Repositories;
namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for managing blog posts and content.
/// Handles publishing workflow, search, and content management.
/// </summary>
public interface IBlogService
{
    /// <summary>
    /// Gets published blog posts with pagination.
    /// </summary>
    Task<(IEnumerable<BlogPostDto> Posts, int Total)> GetPublishedAsync(
        string tenantId, int page = 1, int pageSize = 10);

    /// <summary>
    /// Gets all blog posts by a specific author.
    /// </summary>
    Task<IEnumerable<BlogPostDto>> GetByAuthorAsync(string authorId, string tenantId);

    /// <summary>
    /// Gets a single blog post by URL slug.
    /// Increments view count when accessed.
    /// </summary>
    Task<BlogPostDto?> GetBySlugAsync(string slug, string tenantId);

    /// <summary>
    /// Searches blog posts by title, content, or tags.
    /// </summary>
    Task<IEnumerable<BlogPostDto>> SearchAsync(string searchTerm, string tenantId);

    /// <summary>
    /// Creates a new blog post (defaults to draft status).
    /// </summary>
    Task<BlogPostDto> CreateAsync(BlogPostDto post);

    /// <summary>
    /// Updates an existing blog post.
    /// </summary>
    Task<BlogPostDto> UpdateAsync(BlogPostDto post);

    /// <summary>
    /// Deletes a blog post by ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Publishes a draft blog post, making it publicly visible.
    /// </summary>
    Task<bool> PublishAsync(Guid id);

    /// <summary>
    /// Unpublishes a published blog post, reverting to draft.
    /// </summary>
    Task<bool> UnpublishAsync(Guid id);

    /// <summary>
    /// Increments the view count for a blog post.
    /// </summary>
    Task<bool> IncrementViewCountAsync(Guid id);
}
