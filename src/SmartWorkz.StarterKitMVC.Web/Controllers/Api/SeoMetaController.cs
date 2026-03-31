using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

[ApiController]
[Route("api/{tenantId}/[controller]")]
public class SeoMetaController : ControllerBase
{
    private readonly ISeoMetaService _seoMetaService;

    public SeoMetaController(ISeoMetaService seoMetaService)
    {
        _seoMetaService = seoMetaService;
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeoMeta>> GetByEntity(string tenantId, string entityType, int entityId)
    {
        var seoMeta = await _seoMetaService.GetByEntityAsync(tenantId, entityType, entityId);
        if (seoMeta == null)
            return NotFound();

        return Ok(seoMeta);
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeoMeta>> GetBySlug(string tenantId, string slug)
    {
        var seoMeta = await _seoMetaService.GetBySlugAsync(tenantId, slug);
        if (seoMeta == null)
            return NotFound();

        return Ok(seoMeta);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SeoMeta>> CreateOrUpdateSeoMeta(string tenantId, [FromBody] SeoMeta seoMeta)
    {
        if (string.IsNullOrWhiteSpace(seoMeta.EntityType))
            return BadRequest("Entity type is required");

        seoMeta.TenantId = tenantId;
        var result = await _seoMetaService.CreateOrUpdateSeoMetaAsync(seoMeta);

        return CreatedAtAction(
            nameof(GetByEntity),
            new { tenantId, entityType = seoMeta.EntityType, entityId = seoMeta.EntityId },
            result
        );
    }

    [HttpPut("{seoMetaId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSeoMeta(string tenantId, int seoMetaId, [FromBody] SeoMeta seoMeta)
    {
        var existing = await _seoMetaService.GetByEntityAsync(tenantId, seoMeta.EntityType, seoMeta.EntityId);
        if (existing == null)
            return NotFound();

        seoMeta.SeoMetaId = seoMetaId;
        seoMeta.TenantId = tenantId;
        await _seoMetaService.CreateOrUpdateSeoMetaAsync(seoMeta);

        return NoContent();
    }

    [HttpDelete("{seoMetaId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSeoMeta(string tenantId, int seoMetaId)
    {
        var success = await _seoMetaService.DeleteSeoMetaAsync(seoMetaId);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
