using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Products;

// [Authorize(Policy = "RequireAdmin")]
public class CreateModel : BasePage
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateModel(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IEnumerable<Category> Categories { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Categories = await _categoryRepository.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _categoryRepository.GetAllAsync();
            return Page();
        }

        var product = new Product
        {
            SKU = Input.SKU,
            Name = Input.Name,
            Slug = Input.Slug,
            Description = Input.Description,
            Price = Input.Price,
            CostPrice = Input.CostPrice,
            Stock = Input.Stock,
            CategoryId = Input.CategoryId,
            Status = "Active",
            IsFeatured = Input.IsFeatured,
            TenantId = TenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = CurrentUserId
        };

        await _productRepository.AddAsync(product);
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
