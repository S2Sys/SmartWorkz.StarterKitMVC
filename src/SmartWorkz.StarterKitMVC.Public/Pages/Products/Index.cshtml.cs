using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Products;

public class IndexModel : BasePage
{
    private readonly IProductRepository _productRepository;

    public IndexModel(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 12;

    public IEnumerable<Product> Products { get; private set; } = [];
    public PaginationModel Pagination { get; private set; } = PaginationModel.From(0, 1, 12);

    public async Task OnGetAsync()
    {
        var results = await _productRepository.SearchAsync(TenantId, Search ?? string.Empty);

        // Filter to active products only
        var active = results.Where(p => p.IsActive).OrderByDescending(p => p.CreatedAt).ToList();

        var total = active.Count;
        var items = active
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Products = items;

        Pagination = PaginationModel.FromDto(total, Page, PageSize,
            routeValues: new Dictionary<string, string?>
            {
                ["search"] = Search
            });
    }
}
