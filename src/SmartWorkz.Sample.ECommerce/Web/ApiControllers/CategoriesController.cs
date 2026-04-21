using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(
    IRepository<Category, int> categoryRepository) : ControllerBase
{
    /// <summary>
    /// Gets all categories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await categoryRepository.GetAllAsync(cancellationToken);

            var categoryDtos = categories
                .Select(c => new CategoryDto(
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.Description,
                    c.Products?.Count ?? 0
                ))
                .ToList();

            return Ok(categoryDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve categories", details = ex.Message });
        }
    }
}
