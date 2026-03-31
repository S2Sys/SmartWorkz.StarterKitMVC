using SmartWorkz.StarterKitMVC.Domain.Entities.Master;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Application.Services;

public interface IMenuService
{
    Task<Menu> GetMenuBySlugAsync(string tenantId, string slug);
    Task<List<MenuItem>> GetMenuItemsByMenuIdAsync(int menuId);
    Task<Menu> CreateMenuAsync(string tenantId, Menu menu);
    Task<MenuItem> AddMenuItemAsync(int menuId, MenuItem menuItem);
    Task<bool> DeleteMenuAsync(int menuId);
    Task<bool> UpdateMenuItemOrderAsync(int menuItemId, int newOrder);
}

public class MenuService : IMenuService
{
    private readonly MasterDbContext _context;

    public MenuService(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<Menu> GetMenuBySlugAsync(string tenantId, string slug)
    {
        return await _context.Menus
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Slug == slug);
    }

    public async Task<List<MenuItem>> GetMenuItemsByMenuIdAsync(int menuId)
    {
        return await _context.MenuItems
            .Where(mi => mi.MenuId == menuId && !mi.IsDeleted)
            .OrderBy(mi => mi.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Menu> CreateMenuAsync(string tenantId, Menu menu)
    {
        menu.TenantId = tenantId;
        menu.CreatedAt = DateTime.UtcNow;

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        return menu;
    }

    public async Task<MenuItem> AddMenuItemAsync(int menuId, MenuItem menuItem)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
            throw new ArgumentException($"Menu with ID {menuId} not found");

        menuItem.MenuId = menuId;
        menuItem.CreatedAt = DateTime.UtcNow;

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        return menuItem;
    }

    public async Task<bool> DeleteMenuAsync(int menuId)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
            return false;

        menu.IsDeleted = true;
        menu.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMenuItemOrderAsync(int menuItemId, int newOrder)
    {
        var menuItem = await _context.MenuItems.FindAsync(menuItemId);
        if (menuItem == null)
            return false;

        menuItem.DisplayOrder = newOrder;
        menuItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
