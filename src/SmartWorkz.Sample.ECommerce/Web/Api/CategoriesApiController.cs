using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Api;

[ApiController]
[Route("api/categories")]
public class CategoriesApiController : ControllerBase
{
    private readonly CatalogSearchService _catalog;

    public CategoriesApiController(CatalogSearchService catalog)
    {
        _catalog = Guard.NotNull(catalog, nameof(catalog));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _catalog.GetAllCategoriesAsync();
        if (!result.Succeeded) return BadRequest(result.Error?.Message);
        return Ok(result.Data);
    }
}
