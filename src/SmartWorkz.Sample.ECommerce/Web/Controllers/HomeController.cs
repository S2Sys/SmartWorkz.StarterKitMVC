using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core.Shared.Diagnostics;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class HomeController(ProductService productService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var result = await productService.GetAllAsync();
        var products = result.Data ?? new List<ProductDto>();
        return View(products.Take(6).ToList());
    }

    [HttpGet("/health")]
    public IActionResult Health()
    {
        var health = DiagnosticsHelper.GetApplicationHealth();
        return health.Succeeded ? Ok(health.Data) : StatusCode(503, health.Error);
    }
}
