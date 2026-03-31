using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

[ApiController]
[Route("api/{tenantId}/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Tag>>> GetByEntity(string tenantId, string entityType, int entityId)
    {
        var tags = await _tagService.GetTagsByEntityAsync(tenantId, entityType, entityId);
        return Ok(tags);
    }

    [HttpGet("name/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Tag>>> GetByName(string tenantId, string name)
    {
        var tags = await _tagService.GetTagsByNameAsync(tenantId, name);
        return Ok(tags);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Tag>> CreateTag(string tenantId, [FromBody] Tag tag)
    {
        if (string.IsNullOrWhiteSpace(tag.TagName))
            return BadRequest("Tag name is required");

        var createdTag = await _tagService.CreateTagAsync(tenantId, tag);
        return CreatedAtAction(nameof(GetByName), new { tenantId, name = tag.TagName }, createdTag);
    }

    [HttpPost("{tagId}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTagToEntity(
        string tenantId,
        int tagId,
        [FromBody] AssignTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType) || request.EntityId == 0)
            return BadRequest("Entity type and ID are required");

        var success = await _tagService.AssignTagToEntityAsync(tagId, request.EntityType, request.EntityId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{tagId}/unassign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTagFromEntity(string tenantId, int tagId)
    {
        var success = await _tagService.RemoveTagFromEntityAsync(tagId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{tagId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(string tenantId, int tagId)
    {
        var success = await _tagService.DeleteTagAsync(tagId);
        if (!success)
            return NotFound();

        return NoContent();
    }
}

public class AssignTagRequest
{
    public string EntityType { get; set; }
    public int EntityId { get; set; }
}
