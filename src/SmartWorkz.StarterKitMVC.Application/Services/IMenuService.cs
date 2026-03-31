using SmartWorkz.StarterKitMVC.Domain.Entities.Master;

namespace SmartWorkz.StarterKitMVC.Application.Services;

public interface IMenuService
{
    Task<Menu> GetMenuByNameAsync(string tenantId, string name);
    Task<List<MenuItem>> GetMenuItemsByMenuIdAsync(int menuId);
    Task<Menu> CreateMenuAsync(string tenantId, Menu menu);
    Task<MenuItem> AddMenuItemAsync(int menuId, MenuItem menuItem);
    Task<bool> DeleteMenuAsync(int menuId);
    Task<bool> UpdateMenuItemOrderAsync(int menuItemId, int newOrder);
}
