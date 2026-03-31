using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;

    public TenantController(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tenant>> GetTenantById(string id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tenant>> GetTenantBySlug(string slug)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(slug);
        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }

    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Tenant>>> GetActiveTenants()
    {
        var tenants = await _tenantRepository.GetActiveTenantAsync();
        return Ok(tenants);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Tenant>>> GetAllTenants()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        return Ok(tenants);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Tenant>> CreateTenant([FromBody] Tenant tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.Name))
            return BadRequest("Tenant name is required");

        tenant.CreatedAt = DateTime.UtcNow;
        await _tenantRepository.AddAsync(tenant);
        await _tenantRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTenantById), new { id = tenant.TenantId }, tenant);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenant(string id, [FromBody] Tenant tenant)
    {
        var existing = await _tenantRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        existing.Name = tenant.Name;
        existing.Slug = tenant.Slug;
        existing.IsActive = tenant.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        _tenantRepository.Update(existing);
        await _tenantRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTenant(string id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
            return NotFound();

        tenant.IsDeleted = true;
        tenant.UpdatedAt = DateTime.UtcNow;

        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync();

        return NoContent();
    }
}
