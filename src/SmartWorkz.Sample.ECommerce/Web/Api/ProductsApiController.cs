using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly ProductService _products;

    public ProductsApiController(ProductService products)
    {
        _products = products;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int categoryId = 0)
    {
        var result = await _products.GetAllAsync();
        if (!result.Succeeded) return BadRequest(result.Error?.Message);
        var items = result.Data ?? Enumerable.Empty<object>();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _products.GetByIdAsync(id);
        if (!result.Succeeded) return NotFound(result.Error?.Message);
        return Ok(result.Data);
    }
}
