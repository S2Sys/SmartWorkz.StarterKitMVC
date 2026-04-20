using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core.Abstractions;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class ProductController(
    IRepository<Product, int> productRepo) : Controller
{
    [HttpGet("/product/{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var product = await productRepo.GetByIdAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }
}
