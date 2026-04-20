using Microsoft.AspNetCore.Mvc;
using SmartWorkz.Core.Abstractions;
using SmartWorkz.Core.Shared.Mapping;
using SmartWorkz.Sample.ECommerce.Application.DTOs;
using SmartWorkz.Sample.ECommerce.Application.Services;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Web.Models;

namespace SmartWorkz.Sample.ECommerce.Web.Controllers;

public class CatalogController(
    IRepository<Category, int> categoryRepo,
    IRepository<Product, int> productRepo,
    ProductService productService,
    CatalogSearchService searchService,
    IMapper mapper) : Controller
{
    private const int PageSize = 6;

    public async Task<IActionResult> Index(int page = 1)
    {
        var categories = await categoryRepo.GetAllAsync();
        var categoryDtos = categories.Select(c => mapper.Map<Category, CategoryDto>(c)).ToList();
        return View(categoryDtos);
    }

    [HttpGet("/catalog/products")]
    public async Task<IActionResult> Products(int page = 1, string sortBy = "name")
    {
        var allProducts = await productService.GetAllAsync();
        var products = allProducts.Data?.ToList() ?? new List<ProductDto>();

        // Sorting
        products = sortBy switch
        {
            "price-asc" => products.OrderBy(p => p.Price).ToList(),
            "price-desc" => products.OrderByDescending(p => p.Price).ToList(),
            _ => products.OrderBy(p => p.Name).ToList()
        };

        // Paging
        var totalItems = products.Count;
        var paginatedProducts = products.Skip((page - 1) * PageSize).Take(PageSize).ToList();

        var viewModel = new ProductListViewModel
        {
            Products = paginatedProducts,
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

    [HttpGet("/catalog/category/{slug}")]
    public async Task<IActionResult> Category(string slug, int page = 1, string sortBy = "name")
    {
        var categories = await categoryRepo.GetAllAsync();
        var category = categories.FirstOrDefault(c => c.Slug == slug);
        if (category == null) return NotFound();

        var allProducts = await productService.GetByCategoryAsync(category.Id);
        var products = allProducts.ToList();

        // Sorting
        products = sortBy switch
        {
            "price-asc" => products.OrderBy(p => p.Price).ToList(),
            "price-desc" => products.OrderByDescending(p => p.Price).ToList(),
            _ => products.OrderBy(p => p.Name).ToList()
        };

        // Paging
        var totalItems = products.Count;
        var paginatedProducts = products.Skip((page - 1) * PageSize).Take(PageSize).ToList();

        var viewModel = new ProductListViewModel
        {
            Products = paginatedProducts,
            Paging = new PagingModel
            {
                PageNumber = page,
                PageSize = PageSize,
                TotalItems = totalItems
            },
            SortBy = sortBy,
            CategorySlug = slug
        };

        return View("CategoryProducts", viewModel);
    }

    [HttpGet("/search")]
    public async Task<IActionResult> Search(string q, int page = 1, string sortBy = "name")
    {
        if (string.IsNullOrEmpty(q))
            return View(new ProductListViewModel());

        var results = await searchService.SearchAsync(q);
        var products = results.ToList();

        // Sorting
        products = sortBy switch
        {
            "price-asc" => products.OrderBy(p => p.Price).ToList(),
            "price-desc" => products.OrderByDescending(p => p.Price).ToList(),
            _ => products.OrderBy(p => p.Name).ToList()
        };

        // Paging
        var totalItems = products.Count;
        var paginatedProducts = products.Skip((page - 1) * PageSize).Take(PageSize).ToList();

        var viewModel = new ProductListViewModel
        {
            Products = paginatedProducts,
            Paging = new PagingModel
            {
                PageNumber = page,
                PageSize = PageSize,
                TotalItems = totalItems
            },
            SearchQuery = q,
            SortBy = sortBy
        };

        return View(viewModel);
    }
}
