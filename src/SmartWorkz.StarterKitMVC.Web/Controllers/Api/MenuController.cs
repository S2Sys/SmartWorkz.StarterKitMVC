using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api;

[ApiController]
[Route("api/{tenantId}/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Menu>> GetMenuBySlug(string tenantId, string slug)
    {
        var menu = await _menuService.GetMenuBySlugAsync(tenantId, slug);
        if (menu == null)
            return NotFound();

        return Ok(menu);
    }

    [HttpGet("{menuId}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MenuItem>>> GetMenuItems(int menuId)
    {
        var items = await _menuService.GetMenuItemsByMenuIdAsync(menuId);
        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Menu>> CreateMenu(string tenantId, [FromBody] Menu menu)
    {
        if (string.IsNullOrWhiteSpace(menu.Name))
            return BadRequest("Menu name is required");

        var createdMenu = await _menuService.CreateMenuAsync(tenantId, menu);
        return CreatedAtAction(nameof(GetMenuBySlug), new { tenantId, slug = menu.Slug }, createdMenu);
    }

    [HttpPost("{menuId}/items")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MenuItem>> AddMenuItem(int menuId, [FromBody] MenuItem menuItem)
    {
        if (string.IsNullOrWhiteSpace(menuItem.Label))
            return BadRequest("Menu item label is required");

        var createdItem = await _menuService.AddMenuItemAsync(menuId, menuItem);
        return CreatedAtAction(nameof(GetMenuItems), new { menuId }, createdItem);
    }

    [HttpPut("{menuId}/items/{itemId}/order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMenuItemOrder(int menuId, int itemId, [FromBody] int newOrder)
    {
        var success = await _menuService.UpdateMenuItemOrderAsync(itemId, newOrder);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{menuId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMenu(int menuId)
    {
        var success = await _menuService.DeleteMenuAsync(menuId);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
