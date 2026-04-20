using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core.Abstractions;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class CatalogController(
    IRepository<Category, int> categoryRepo,
    ProductService productService,
    CatalogSearchService searchService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var categories = await categoryRepo.GetAllAsync();
        return View(categories);
    }

    [HttpGet("/catalog/category/{slug}")]
    public async Task<IActionResult> Category(string slug)
    {
        var categories = await categoryRepo.GetAllAsync();
        var category = categories.FirstOrDefault(c => c.Slug == slug);
        if (category == null) return NotFound();

        var products = await productService.GetByCategoryAsync(category.Id);
        return View(new { Category = category, Products = products });
    }

    [HttpGet("/search")]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrEmpty(q))
            return View(new List<ProductDto>());

        var results = await searchService.SearchAsync(q);
        return View(results);
    }
}
