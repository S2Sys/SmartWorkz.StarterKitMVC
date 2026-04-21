using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly ProductService _products;

    public ProductsApiController(ProductService products)
    {
        _products = Guard.NotNull(products, nameof(products));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _products.GetAllAsync();
        if (!result.Succeeded) return BadRequest(result.Error?.Message);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _products.GetByIdAsync(id);
        if (!result.Succeeded) return NotFound(result.Error?.Message);
        return Ok(result.Data);
    }
}
