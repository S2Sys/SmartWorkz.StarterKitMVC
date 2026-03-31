using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using SmartWorkz.StarterKitMVC.Shared.Validation;
using SmartWorkz.StarterKitMVC.Web.Middleware;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

/// <summary>
/// Product management endpoints for CRUD operations and product search/filtering.
/// </summary>
[ApiController]
[Route("api/{tenantId}/[controller]")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProductById(string tenantId, int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product?.TenantId != tenantId || product.IsDeleted)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProductBySlug(string tenantId, string slug)
    {
        var product = await _productRepository.GetBySlugAsync(tenantId, slug);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("sku/{sku}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProductBySku(string tenantId, string sku)
    {
        var product = await _productRepository.GetBySkuAsync(tenantId, sku);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginationResponse<Product>>> GetProductsByCategory(
        string tenantId,
        int categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            throw new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "page", ErrorMessage = "Page must be >= 1" },
                new() { PropertyName = "pageSize", ErrorMessage = "PageSize must be between 1 and 100" }
            });

        var products = await _productRepository.GetByCategoryAsync(tenantId, categoryId);
        var paginated = products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var totalCount = products.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (decimal)pageSize);

        return Ok(new PaginationResponse<Product>(paginated, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("featured")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Product>>> GetFeaturedProducts(string tenantId, [FromQuery] int take = 10)
    {
        var products = await _productRepository.GetFeaturedProductsAsync(tenantId, take);
        return Ok(products);
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginationResponse<Product>>> SearchProducts(
        string tenantId,
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            throw new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "q", ErrorMessage = "Search query is required" }
            });

        if (page < 1 || pageSize < 1 || pageSize > 100)
            throw new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "page", ErrorMessage = "Page must be >= 1" },
                new() { PropertyName = "pageSize", ErrorMessage = "PageSize must be between 1 and 100" }
            });

        var products = await _productRepository.SearchAsync(tenantId, q);
        var paginated = products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var totalCount = products.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (decimal)pageSize);

        return Ok(new PaginationResponse<Product>(paginated, page, pageSize, totalCount, totalPages));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> CreateProduct(string tenantId, [FromBody] Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = nameof(product.Name), ErrorMessage = "Product name is required" }
            });

        if (product.Price < 0)
            throw new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = nameof(product.Price), ErrorMessage = "Price cannot be negative" }
            });

        product.TenantId = tenantId;
        product.CreatedAt = DateTime.UtcNow;

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductById), new { tenantId, id = product.ProductId }, product);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(string tenantId, int id, [FromBody] Product product)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null || existing.TenantId != tenantId)
            return NotFound();

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.Slug = product.Slug;
        existing.IsFeatured = product.IsFeatured;
        existing.UpdatedAt = DateTime.UtcNow;

        _productRepository.Update(existing);
        await _productRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(string tenantId, int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null || product.TenantId != tenantId)
            return NotFound();

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();

        return NoContent();
    }
}
