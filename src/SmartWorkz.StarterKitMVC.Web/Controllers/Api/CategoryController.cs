using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Validation;
using SmartWorkz.StarterKitMVC.Web.Middleware;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

[ApiController]
[Route("api/{tenantId}/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> GetCategoryById(string tenantId, int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category?.TenantId != tenantId || category.IsDeleted)
            return NotFound();

        return Ok(category);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> GetCategoryBySlug(string tenantId, string slug)
    {
        var category = await _categoryRepository.GetBySlugAsync(tenantId, slug);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpGet("root")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginationResponse<Category>>> GetRootCategories(
        string tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            throw new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "page", ErrorMessage = "Page must be >= 1" },
                new() { PropertyName = "pageSize", ErrorMessage = "PageSize must be between 1 and 100" }
            });

        var categories = await _categoryRepository.GetRootCategoriesAsync(tenantId);
        var paginated = categories
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var totalCount = categories.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (decimal)pageSize);

        return Ok(new PaginationResponse<Category>(paginated, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("{parentId}/children")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginationResponse<Category>>> GetChildCategories(
        string tenantId,
        int parentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            throw new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "page", ErrorMessage = "Page must be >= 1" },
                new() { PropertyName = "pageSize", ErrorMessage = "PageSize must be between 1 and 100" }
            });

        var categories = await _categoryRepository.GetChildCategoriesAsync(tenantId, parentId);
        var paginated = categories
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var totalCount = categories.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (decimal)pageSize);

        return Ok(new PaginationResponse<Category>(paginated, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("{id}/hierarchy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> GetCategoryHierarchy(string tenantId, int id)
    {
        var category = await _categoryRepository.GetWithChildrenAsync(id);
        if (category?.TenantId != tenantId)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Category>> CreateCategory(string tenantId, [FromBody] Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = nameof(category.Name), ErrorMessage = "Category name is required" }
            });

        category.TenantId = tenantId;
        category.CreatedAt = DateTime.UtcNow;

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategoryById), new { tenantId, id = category.CategoryId }, category);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(string tenantId, int id, [FromBody] Category category)
    {
        var existing = await _categoryRepository.GetByIdAsync(id);
        if (existing == null || existing.TenantId != tenantId)
            return NotFound();

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.Slug = category.Slug;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.UpdatedAt = DateTime.UtcNow;

        _categoryRepository.Update(existing);
        await _categoryRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(string tenantId, int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null || category.TenantId != tenantId)
            return NotFound();

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        return NoContent();
    }
}
