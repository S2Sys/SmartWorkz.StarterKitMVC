namespace SmartWorkz.Sample.ECommerce.Web.ApiControllers;

using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(
    IRepository<Category, int> categoryRepo,
    SmartWorkz.Shared.IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDto>>>> GetAll()
    {
        var categories = await categoryRepo.GetAllAsync();
        var dtos = categories
            .Select(c => mapper.Map<Category, CategoryDto>(c))
            .ToList()
            .AsReadOnly();
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(dtos));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
    {
        var category = await categoryRepo.GetByIdAsync(id);
        if (category == null)
            return NotFound(ApiResponse<CategoryDto>.Fail(
                ApiError.FromError(Error.NotFound("Category", id))));
        var dto = mapper.Map<Category, CategoryDto>(category);
        return Ok(ApiResponse<CategoryDto>.Ok(dto));
    }
}
