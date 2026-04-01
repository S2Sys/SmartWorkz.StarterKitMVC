using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Products;

[Authorize(Policy = "RequireAdmin")]
public class EditModel : BasePage
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public EditModel(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public int ProductId { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IEnumerable<Category> Categories { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null || product.TenantId != TenantId)
            return NotFound();

        ProductId = product.ProductId;
        Input = new InputModel
        {
            SKU = product.SKU,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            CostPrice = product.CostPrice,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            IsFeatured = product.IsFeatured
        };

        Categories = await _categoryRepository.GetAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _categoryRepository.GetAllAsync();
            return Page();
        }

        var product = await _productRepository.GetByIdAsync(ProductId);

        if (product == null || product.TenantId != TenantId)
            return NotFound();

        product.SKU = Input.SKU;
        product.Name = Input.Name;
        product.Slug = Input.Slug;
        product.Description = Input.Description;
        product.Price = Input.Price;
        product.CostPrice = Input.CostPrice;
        product.Stock = Input.Stock;
        product.CategoryId = Input.CategoryId;
        product.IsFeatured = Input.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = CurrentUserId;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();

        ToastSuccess("toast.success");
        return RedirectToPage("./Index");
    }

    public class InputModel
    {
        [Required(ErrorMessage = "validation.required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "validation.string_length")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "validation.required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "validation.string_length")]
        public string Name { get; set; }

        [Required(ErrorMessage = "validation.required")]
        [StringLength(250, ErrorMessage = "validation.max_length")]
        public string Slug { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "validation.required")]
        [Range(0.01, 999999.99, ErrorMessage = "validation.invalid_format")]
        public decimal Price { get; set; }

        [Range(0, 999999.99)]
        public decimal? CostPrice { get; set; }

        [Required(ErrorMessage = "validation.required")]
        [Range(0, 999999, ErrorMessage = "validation.invalid_format")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "validation.required")]
        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }
    }
}
