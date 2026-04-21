using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Shared;
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

    [HttpGet("/debug/products")]
    public async Task<IActionResult> DebugProducts()
    {
        try
        {
            var result = await productService.GetAllAsync();
            if (!result.Succeeded)
                return BadRequest(new { error = result.Error?.ToString() });
            return Ok(new { count = result.Data?.Count, products = result.Data?.Take(3) });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.GetType().Name, message = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [Route("/Home/Error")]
    public IActionResult Error()
    {
        return View();
    }
}
