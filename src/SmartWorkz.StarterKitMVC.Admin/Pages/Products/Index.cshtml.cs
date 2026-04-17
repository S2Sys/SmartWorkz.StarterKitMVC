using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Shared.Extensions;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Products;

// [Authorize(Policy = "RequireAdmin")]
public class IndexModel : BasePage
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public IndexModel(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public new int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 20;
    [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "CreatedAt";
    [BindProperty(SupportsGet = true)] public bool Desc { get; set; } = true;

    public IEnumerable<Product> Products { get; private set; } = [];
    public PaginationModel Pagination { get; private set; } = PaginationModel.From(0, 1, 20);

    public async Task OnGetAsync()
    {
        await LoadProductsAsync();
    }

    public async Task<IActionResult> OnGetTableAsync()
    {
        await LoadProductsAsync();
        return Request.IsHtmx() ? Partial("_ProductTableRows", this) : Page();
    }

    private async Task LoadProductsAsync()
    {
        var results = await _productRepository.SearchAsync(TenantId, Search ?? string.Empty);

        // Apply sorting
        var sorted = SortBy switch
        {
            "Name" => Desc ? results.OrderByDescending(x => x.Name) : results.OrderBy(x => x.Name),
            "Price" => Desc ? results.OrderByDescending(x => x.Price) : results.OrderBy(x => x.Price),
            "Stock" => Desc ? results.OrderByDescending(x => x.Stock) : results.OrderBy(x => x.Stock),
            _ => Desc ? results.OrderByDescending(x => x.CreatedAt) : results.OrderBy(x => x.CreatedAt)
        };

        var total = sorted.Count();
        var items = sorted
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Products = items;

        Pagination = PaginationModel.FromDto(total, Page, PageSize,
            routeValues: new Dictionary<string, string?>
            {
                ["search"] = Search,
                ["sortBy"] = SortBy,
                ["desc"] = Desc.ToString().ToLower(),
            },
            htmxTarget: "#products-table-container");
    }
}
