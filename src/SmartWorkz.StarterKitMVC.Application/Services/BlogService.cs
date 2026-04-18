using SmartWorkz.StarterKitMVC.Shared.DTOs;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using System.Text.RegularExpressions;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of blog post management service.
/// Handles publishing workflow, slug generation, and content management.
/// </summary>
public class BlogService : IBlogService
{
    private readonly IBlogPostRepository _repository;
    private readonly ILogger<BlogService> _logger;

    public BlogService(
        IBlogPostRepository repository,
        ILogger<BlogService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<BlogPostDto> Posts, int Total)> GetPublishedAsync(
        string tenantId, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        try
        {
            var (posts, total) = await _repository.GetPublishedAsync(tenantId, page, pageSize);
            _logger.LogDebug("Retrieved {Count} published posts for tenant {TenantId}", posts.Count(), tenantId);
            return (posts.OrderByDescending(p => p.PublishedAt), total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving published posts for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BlogPostDto>> GetByAuthorAsync(string authorId, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(authorId))
            throw new ArgumentException("Author ID cannot be empty", nameof(authorId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var posts = await _repository.GetByAuthorAsync(authorId, tenantId);
            _logger.LogDebug("Retrieved {Count} posts for author {AuthorId}", posts.Count(), authorId);
            return posts.OrderByDescending(p => p.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts for author: {AuthorId}", authorId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BlogPostDto?> GetBySlugAsync(string slug, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be empty", nameof(slug));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        try
        {
            var post = await _repository.GetBySlugAsync(slug, tenantId);

            if (post != null && post.IsPublished)
            {
                // Increment view count asynchronously
                _ = IncrementViewCountAsync(post.PostId);
            }

            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post by slug: {Slug}", slug);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BlogPostDto>> SearchAsync(string searchTerm, string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<BlogPostDto>();

        try
        {
            var posts = await _repository.SearchAsync(searchTerm, tenantId);
            _logger.LogDebug("Found {Count} posts matching search term: {SearchTerm}",
                posts.Count(), searchTerm);
            return posts.OrderByDescending(p => p.PublishedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching posts with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BlogPostDto> CreateAsync(BlogPostDto post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));
        if (string.IsNullOrWhiteSpace(post.Title))
            throw new ArgumentException("Post title is required", nameof(post.Title));

        try
        {
            // Generate slug from title if not provided
            if (string.IsNullOrWhiteSpace(post.Slug))
            {
                post.Slug = GenerateSlug(post.Title);
            }

            post.PostId = Guid.NewGuid();
            post.CreatedAt = DateTime.UtcNow;
            post.IsPublished = false; // Default to draft

            var created = await _repository.UpsertAsync(post);

            _logger.LogInformation(
                "Blog post created: {PostId} ({Title}) by {AuthorId}",
                created.PostId, created.Title, created.AuthorId);

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post: {Title}", post.Title);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BlogPostDto> UpdateAsync(BlogPostDto post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));
        if (post.PostId == Guid.Empty)
            throw new ArgumentException("Post ID is required", nameof(post.PostId));
        if (string.IsNullOrWhiteSpace(post.Title))
            throw new ArgumentException("Post title is required", nameof(post.Title));

        try
        {
            post.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpsertAsync(post);

            _logger.LogInformation(
                "Blog post updated: {PostId} ({Title})",
                updated.PostId, updated.Title);

            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog post: {PostId}", post.PostId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Post ID is required", nameof(id));

        try
        {
            var result = await _repository.DeleteAsync(id);

            if (result)
            {
                _logger.LogInformation("Blog post deleted: {PostId}", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog post: {PostId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> PublishAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Post ID is required", nameof(id));

        try
        {
            var post = await _repository.GetByIdAsync(id);
            if (post == null)
                return false;

            post.IsPublished = true;
            post.PublishedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            await _repository.UpsertAsync(post);

            _logger.LogInformation("Blog post published: {PostId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing blog post: {PostId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UnpublishAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Post ID is required", nameof(id));

        try
        {
            var post = await _repository.GetByIdAsync(id);
            if (post == null)
                return false;

            post.IsPublished = false;
            post.UpdatedAt = DateTime.UtcNow;

            await _repository.UpsertAsync(post);

            _logger.LogInformation("Blog post unpublished: {PostId}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing blog post: {PostId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        if (id == Guid.Empty)
            return false;

        try
        {
            var post = await _repository.GetByIdAsync(id);
            if (post == null)
                return false;

            post.ViewCount++;
            await _repository.UpsertAsync(post);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for post: {PostId}", id);
            return false;
        }
    }

    /// <summary>
    /// Generates a URL-friendly slug from a title.
    /// Converts to lowercase, replaces spaces with hyphens, removes special characters.
    /// </summary>
    private static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Guid.NewGuid().ToString().Substring(0, 8);

        // Convert to lowercase
        var slug = title.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove special characters
        slug = Regex.Replace(slug, @"[^\w\-]", "");

        // Remove consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Limit to 100 characters
        if (slug.Length > 100)
            slug = slug.Substring(0, 100).TrimEnd('-');

        return slug;
    }
}
