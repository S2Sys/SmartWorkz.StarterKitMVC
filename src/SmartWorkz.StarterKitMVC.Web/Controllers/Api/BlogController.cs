using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using SmartWorkz.StarterKitMVC.Web.Middleware;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

/// <summary>
/// Blog API endpoints for managing blog posts with publication workflow.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;
    private readonly ILogger<BlogController> _logger;

    public BlogController(IBlogService blogService, ILogger<BlogController> logger)
    {
        _blogService = blogService;
        _logger = logger;
    }

    /// <summary>
    /// Get published blog posts with pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>Paginated list of published blog posts</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResponse<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1)
        {
            _logger.LogWarning("GetPublished called with invalid pagination: page={Page}, pageSize={PageSize}", page, pageSize);
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Page and pageSize must be greater than 0",
                new Dictionary<string, string[]>
                {
                    ["page"] = new[] { "Page must be >= 1" },
                    ["pageSize"] = new[] { "PageSize must be >= 1" }
                },
                Request.Path));
        }

        if (pageSize > 100)
        {
            pageSize = 100; // Cap max page size
        }

        _logger.LogInformation("Retrieving published blog posts: page={Page}, pageSize={PageSize}", page, pageSize);
        var result = await _blogService.GetPublishedPostsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a blog post by its slug.
    /// </summary>
    /// <param name="slug">The blog post slug</param>
    /// <returns>The blog post details</returns>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            _logger.LogWarning("GetBySlug called with empty slug");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Blog post slug is required",
                new Dictionary<string, string[]> { ["slug"] = new[] { "Slug cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Retrieving blog post by slug: {Slug}", slug);
        var result = await _blogService.GetPostBySlugAsync(slug);

        if (result == null)
        {
            _logger.LogWarning("Blog post not found: {Slug}", slug);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Blog post with slug '{slug}' not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new blog post (Editor and Admin roles).
    /// </summary>
    /// <param name="request">The blog post creation request</param>
    /// <returns>The created blog post</returns>
    [HttpPost]
    [Authorize(Roles = "Editor,Admin")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("Create blog post called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
        {
            _logger.LogWarning("Create blog post called with incomplete data");
            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(request.Title))
                errors["title"] = new[] { "Title is required" };
            if (string.IsNullOrWhiteSpace(request.Content))
                errors["content"] = new[] { "Content is required" };

            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Title and content are required",
                errors,
                Request.Path));
        }

        _logger.LogInformation("Creating blog post: {Title}", request.Title);

        var result = await _blogService.CreatePostAsync(request);
        return CreatedAtAction(nameof(GetBySlug), new { slug = result.Slug }, result);
    }

    /// <summary>
    /// Update an existing blog post (Editor and Admin roles).
    /// </summary>
    /// <param name="id">The blog post ID to update</param>
    /// <param name="request">The blog post update request</param>
    /// <returns>The updated blog post</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Editor,Admin")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlogPostRequest request)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Update blog post called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Valid blog post ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        if (request == null)
        {
            _logger.LogWarning("Update blog post called with null request");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Request body is required",
                new Dictionary<string, string[]> { ["body"] = new[] { "Request cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Updating blog post: {Id}", id);

        var result = await _blogService.UpdatePostAsync(id, request);

        if (result == null)
        {
            _logger.LogWarning("Blog post not found for update: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Blog post with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a blog post (Admin only).
    /// </summary>
    /// <param name="id">The blog post ID to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Delete blog post called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Valid blog post ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Deleting blog post: {Id}", id);

        var result = await _blogService.DeletePostAsync(id);

        if (!result)
        {
            _logger.LogWarning("Blog post not found for deletion: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Blog post with ID {id} not found",
                Request.Path));
        }

        return NoContent();
    }

    /// <summary>
    /// Publish a blog post (Admin only).
    /// </summary>
    /// <param name="id">The blog post ID to publish</param>
    /// <returns>The published blog post</returns>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Publish(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Publish blog post called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Valid blog post ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Publishing blog post: {Id}", id);

        var result = await _blogService.PublishPostAsync(id);

        if (result == null)
        {
            _logger.LogWarning("Blog post not found for publishing: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Blog post with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }

    /// <summary>
    /// Unpublish a blog post (Admin only).
    /// </summary>
    /// <param name="id">The blog post ID to unpublish</param>
    /// <returns>The unpublished blog post</returns>
    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Unpublish blog post called with empty ID");
            return BadRequest(ProblemDetailsResponse.ValidationError(
                "Valid blog post ID is required",
                new Dictionary<string, string[]> { ["id"] = new[] { "ID cannot be empty" } },
                Request.Path));
        }

        _logger.LogInformation("Unpublishing blog post: {Id}", id);

        var result = await _blogService.UnpublishPostAsync(id);

        if (result == null)
        {
            _logger.LogWarning("Blog post not found for unpublishing: {Id}", id);
            return NotFound(ProblemDetailsResponse.NotFound(
                $"Blog post with ID {id} not found",
                Request.Path));
        }

        return Ok(result);
    }
}

/// <summary>
/// Request model for creating a blog post.
/// </summary>
public record CreateBlogPostRequest(
    string Title,
    string Content,
    string? FeaturedImageUrl = null,
    string? Description = null,
    List<string>? Tags = null);

/// <summary>
/// Request model for updating a blog post.
/// </summary>
public record UpdateBlogPostRequest(
    string? Title = null,
    string? Content = null,
    string? FeaturedImageUrl = null,
    string? Description = null,
    List<string>? Tags = null);
