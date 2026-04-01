using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Products;

public class DetailsModel : BasePage
{
    private readonly IProductRepository _productRepository;

    public DetailsModel(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Product? Product { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? slug)
    {
        if (string.IsNullOrEmpty(slug))
            return NotFound();

        var product = await _productRepository.GetBySlugAsync(TenantId, slug);

        if (product == null || !product.IsActive)
            return NotFound();

        Product = product;
        return Page();
    }
}
