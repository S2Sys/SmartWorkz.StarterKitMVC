using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

[ApiController]
[Route("api/{tenantId}/[controller]")]
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
    public async Task<ActionResult<List<Product>>> GetProductsByCategory(string tenantId, int categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(tenantId, categoryId);
        return Ok(products);
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
    public async Task<ActionResult<List<Product>>> SearchProducts(string tenantId, [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query is required");

        var products = await _productRepository.SearchAsync(tenantId, q);
        return Ok(products);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> CreateProduct(string tenantId, [FromBody] Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Product name is required");

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
