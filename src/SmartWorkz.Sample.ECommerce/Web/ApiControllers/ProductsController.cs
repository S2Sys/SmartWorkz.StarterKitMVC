namespace SmartWorkz.Sample.ECommerce.Web.ApiControllers;

using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Requests;
using SmartWorkz.Sample.ECommerce.Application.Services;
using SmartWorkz.Sample.ECommerce.Application.Validators;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    ProductService productService,
    CreateProductValidator createValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedList<ProductDto>>>> GetAll(
        [FromQuery] PagedQuery query)
    {
        query = query.Normalize();
        var allResult = await productService.GetAllAsync();
        if (!allResult.Succeeded)
            return BadRequest(ApiResponse<PagedList<ProductDto>>.Fail(
                ApiError.FromError(allResult.Error!)));

        var items = allResult.Data!.AsEnumerable();

        // Apply search term filtering
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            items = items.Where(p => p.Name.Contains(
                query.SearchTerm, StringComparison.OrdinalIgnoreCase));

        // Apply sorting
        items = query.SortBy?.ToLowerInvariant() switch
        {
            "price" => query.SortDescending
                ? items.OrderByDescending(p => p.Price)
                : items.OrderBy(p => p.Price),
            "name" => query.SortDescending
                ? items.OrderByDescending(p => p.Name)
                : items.OrderBy(p => p.Name),
            _ => items.OrderBy(p => p.Name)
        };

        var list = items.ToList();
        var paged = PagedList<ProductDto>.Create(
            list.Skip(query.Skip).Take(query.Take),
            query.Page, query.PageSize, list.Count);

        var response = ApiResponse<PagedList<ProductDto>>.Ok(paged);
        response.Pagination = new ApiResponse<PagedList<ProductDto>>.PaginationMetadata(
            paged.Page, paged.PageSize, paged.TotalCount, paged.TotalPages);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
    {
        var result = await productService.GetByIdAsync(id);
        if (!result.Succeeded)
            return NotFound(ApiResponse<ProductDto>.Fail(ApiError.FromError(result.Error!)));
        return Ok(ApiResponse<ProductDto>.Ok(result.Data!));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
        [FromBody] CreateProductRequest request)
    {
        var result = await productService.CreateProductAsync(request, createValidator);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<ProductDto>.Fail(ApiError.FromError(result.Error!)));
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id },
            ApiResponse<ProductDto>.Ok(result.Data!));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(
        int id, [FromBody] UpdateProductRequest request)
    {
        var result = await productService.UpdateProductAsync(id, request);
        if (!result.Succeeded)
            return result.Error?.Code.EndsWith("NOT_FOUND") == true
                ? NotFound(ApiResponse<ProductDto>.Fail(ApiError.FromError(result.Error!)))
                : BadRequest(ApiResponse<ProductDto>.Fail(ApiError.FromError(result.Error!)));
        return Ok(ApiResponse<ProductDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var result = await productService.DeleteAsync(id);
        if (!result.Succeeded)
            return NotFound(ApiResponse.Fail(ApiError.FromError(result.Error!)));
        return Ok(ApiResponse.Ok("Product deleted"));
    }
}
