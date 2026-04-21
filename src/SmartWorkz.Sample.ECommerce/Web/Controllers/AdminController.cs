using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Web.Models;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

[Route("/admin")]
public class AdminController(
    IRepository<Product, int> productRepo,
    IRepository<Category, int> categoryRepo,
    IMapper mapper) : Controller
{
    private const int PageSize = 10;

    [HttpGet("")]
    public IActionResult Index() => Redirect("/admin/products");

    [HttpGet("products")]
    public async Task<IActionResult> Products(int page = 1, string sortBy = "name")
    {
        var allProducts = await productRepo.GetAllAsync();
        var products = allProducts.ToList();

        // Sorting
        products = sortBy switch
        {
            "price-asc" => products.OrderBy(p => p.Price?.Amount ?? 0).ToList(),
            "price-desc" => products.OrderByDescending(p => p.Price?.Amount ?? 0).ToList(),
            _ => products.OrderBy(p => p.Name).ToList()
        };

        // Paging
        var totalItems = products.Count;
        var paginatedProducts = products.Skip((page - 1) * PageSize).Take(PageSize).ToList();
        var productDtos = paginatedProducts.Select(p => mapper.Map<Product, ProductDto>(p)).ToList();

        var viewModel = new ProductListViewModel
        {
            Products = productDtos,
            Paging = new PagingModel
            {
                PageNumber = page,
                PageSize = PageSize,
                TotalItems = totalItems
            },
            SortBy = sortBy
        };

        return View(viewModel);
    }

    [HttpGet("products/create")]
    public async Task<IActionResult> Create()
    {
        var categories = await categoryRepo.GetAllAsync();
        ViewBag.Categories = categories;
        return View();
    }

    [HttpPost("products/create")]
    public async Task<IActionResult> Create(string name, string slug, string description, decimal price, int stock, int categoryId)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(slug))
            return BadRequest("Name and Slug are required");

        var moneyResult = Money.Create(price, "USD");
        if (moneyResult.Data == null)
            return BadRequest("Invalid price");

        var product = new Product
        {
            Name = name,
            Slug = slug,
            Description = description,
            Price = moneyResult.Data,
            Stock = stock,
            CategoryId = categoryId,
            IsActive = true
        };

        await productRepo.AddAsync(product);
        return RedirectToAction(nameof(Products));
    }

    [HttpGet("products/{id}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await productRepo.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        var categories = await categoryRepo.GetAllAsync();
        ViewBag.Categories = categories;
        ViewBag.Product = product;
        return View();
    }

    [HttpPost("products/{id}/edit")]
    public async Task<IActionResult> Edit(int id, string name, string slug, string description, decimal price, int stock, int categoryId)
    {
        var product = await productRepo.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        product.Name = name;
        product.Slug = slug;
        product.Description = description;
        product.Stock = stock;
        product.CategoryId = categoryId;

        var moneyResult = Money.Create(price, "USD");
        if (moneyResult.Data != null)
            product.Price = moneyResult.Data;

        await productRepo.UpdateAsync(product);
        return RedirectToAction(nameof(Products));
    }

    [HttpPost("products/{id}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        await productRepo.DeleteAsync(id);
        return RedirectToAction(nameof(Products));
    }

    [HttpGet("products/search")]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrEmpty(q))
            return RedirectToAction(nameof(Products));

        var allProducts = await productRepo.GetAllAsync();
        var products = allProducts
            .Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                       p.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        var productDtos = products.Select(p => mapper.Map<Product, ProductDto>(p)).ToList();
        var viewModel = new ProductListViewModel
        {
            Products = productDtos,
            Paging = new PagingModel { TotalItems = productDtos.Count, PageSize = PageSize },
            SearchQuery = q
        };

        return View(nameof(Products), viewModel);
    }
}
