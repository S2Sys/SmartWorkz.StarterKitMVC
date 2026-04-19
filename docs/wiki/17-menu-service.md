# Menu Service

Tenant-aware navigation menus with ordered items. Used by Admin for the sidebar, by Public for header/footer links, and by any REST client (SPA / mobile) that wants to render menus from the server rather than hard-code them.

## Purpose

- **One menu definition → many clients.** Same `Menu` + `MenuItem` rows drive Razor layouts, SPA nav, and mobile drawer.
- **Per-tenant menus.** The same menu `Name` can resolve differently per tenant.
- **Role-aware items.** Each `MenuItem.RequiredRole` lets the renderer hide items the user can't reach.
- **Stable ordering.** `DisplayOrder` on both menu and menu-item, plus `UpdateMenuItemOrderAsync` for drag-drop reorder.
- **Soft delete.** Delete flips `IsDeleted = true`; history preserved.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `IMenuService` | Contract | [`Application/Services/IMenuService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Services/IMenuService.cs) |
| `MenuService` | EF Core implementation against `MasterDbContext` | [`Infrastructure/Services/MenuService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/MenuService.cs) |
| `Menu` | Master.Menus row | [`Domain/Entities/Master/Menu.cs`](../../src/SmartWorkz.StarterKitMVC.Domain/Entities/Master/Menu.cs) |
| `MenuItem` | Master.MenuItems row | [`Domain/Entities/Master/MenuItem.cs`](../../src/SmartWorkz.StarterKitMVC.Domain/Entities/Master/MenuItem.cs) |

### Data model

`Menu`

| Field | Notes |
|-------|-------|
| `MenuId` | `int` identity |
| `Name` | Logical key — e.g. `AdminSidebar`, `PublicHeader`, `PublicFooter` |
| `MenuType` | Free-form classifier (`sidebar` / `header` / `footer` / `drawer`) |
| `DisplayOrder` | Sort among menus of the same type |
| `TenantId` | Multi-tenant scope |
| `IsActive` / `IsDeleted` | Soft toggle + soft delete |

`MenuItem`

| Field | Notes |
|-------|-------|
| `MenuItemId` / `MenuId` | PK / FK |
| `Title`, `URL`, `Icon` | Rendering fields |
| `DisplayOrder` | Used by `GetMenuItemsByMenuIdAsync` and `UpdateMenuItemOrderAsync` |
| `NodePath` | Reserved for hierarchical menus (HierarchyId). Populate if you need trees. |
| `RequiredRole` | Single role name; renderer checks against `User.IsInRole(...)` |
| `TenantId` | Multi-tenant scope |

## DI Registration

Wired by `AddApplicationServices`:

```csharp
services.AddScoped<IMenuService, MenuService>();
```

## Quick Start

### Build a menu at startup / in a seeder

```csharp
var menu = await _menus.CreateMenuAsync("acme", new Menu
{
    Name = "AdminSidebar",
    MenuType = "sidebar",
    Description = "Main admin left nav",
    DisplayOrder = 0
});

foreach (var (title, url, icon, order, role) in new[]
{
    ("Dashboard", "/Admin",          "bi-speedometer",     10, (string?)null),
    ("Products",  "/Admin/Products", "bi-box-seam",        20, "products.view"),
    ("Orders",    "/Admin/Orders",   "bi-cart",            30, "orders.view"),
    ("Users",     "/Admin/Users",    "bi-people",          40, "users.view"),
})
    await _menus.AddMenuItemAsync(menu.MenuId, new MenuItem
    {
        Title = title, URL = url, Icon = icon,
        DisplayOrder = order, RequiredRole = role,
        TenantId = "acme"
    });
```

### Render in a Razor Page (Admin / Public)

```csharp
public class _LayoutModel : BasePage
{
    private readonly IMenuService _menus;
    public List<MenuItem> SidebarItems { get; private set; } = [];

    public async Task LoadSidebarAsync()
    {
        var menu = await _menus.GetMenuByNameAsync(TenantId, "AdminSidebar");
        if (menu is null) return;

        var items = await _menus.GetMenuItemsByMenuIdAsync(menu.MenuId);
        SidebarItems = items
            .Where(mi => mi.IsActive
                && (mi.RequiredRole is null || User.IsInRole(mi.RequiredRole)))
            .ToList();
    }
}
```

In the view:

```razor
@foreach (var item in Model.SidebarItems)
{
    <a asp-page="@item.URL"><i class="bi @item.Icon"></i> @item.Title</a>
}
```

### Expose via REST for SPA / mobile

```csharp
[ApiController, Route("api/menus")]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menus;

    [HttpGet("{name}")]
    public async Task<ActionResult<MenuDto>> Get(string name)
    {
        var tenantId = HttpContext.Items["TenantId"] as string ?? "DEFAULT";
        var menu = await _menus.GetMenuByNameAsync(tenantId, name);
        if (menu is null) return NotFound();

        var items = await _menus.GetMenuItemsByMenuIdAsync(menu.MenuId);
        return new MenuDto(menu.Name, menu.MenuType,
            items.Where(i => i.IsActive).Select(i =>
                new MenuItemDto(i.Title, i.URL, i.Icon, i.DisplayOrder, i.RequiredRole)));
    }
}
```

Angular / React / MAUI / WPF clients call `GET /api/menus/AdminSidebar` on navigation and render the result. Hide items whose `requiredRole` the user doesn't hold.

## Method Reference

```csharp
Task<Menu>         GetMenuByNameAsync(string tenantId, string name);
Task<List<MenuItem>> GetMenuItemsByMenuIdAsync(int menuId);
Task<Menu>         CreateMenuAsync(string tenantId, Menu menu);
Task<MenuItem>     AddMenuItemAsync(int menuId, MenuItem menuItem);
Task<bool>         DeleteMenuAsync(int menuId);
Task<bool>         UpdateMenuItemOrderAsync(int menuItemId, int newOrder);
```

### `GetMenuByNameAsync(tenantId, name)` → `Menu?`

Returns the first matching `Menu` for the tenant. **Returns `null` on miss** (not an exception). Does not populate `MenuItems` — call `GetMenuItemsByMenuIdAsync` explicitly.

### `GetMenuItemsByMenuIdAsync(menuId)` → `List<MenuItem>`

Returns items ordered by `DisplayOrder` ascending. Filters out `IsDeleted = true`. Does **not** filter `IsActive` — renderer decides.

### `CreateMenuAsync(tenantId, menu)` → `Menu`

Persists the menu with `TenantId` and `CreatedAt` set. Returns the saved entity (now has `MenuId`).

### `AddMenuItemAsync(menuId, menuItem)` → `MenuItem`

Throws `ArgumentException` if the parent menu doesn't exist. Sets `MenuId` and `CreatedAt`, returns the saved item.

### `DeleteMenuAsync(menuId)` → `bool`

Soft delete — flips `IsDeleted = true`. Returns `true` when a row was found, `false` otherwise. **Does not** delete child `MenuItem` rows; they remain queryable. If you need cascading soft-delete, loop the children first.

### `UpdateMenuItemOrderAsync(menuItemId, newOrder)` → `bool`

Single-row update of `DisplayOrder`. Returns `false` when the item doesn't exist. For drag-drop bulk reorder, call it per item in a `foreach` — there's no batch endpoint yet. Wrap in a transaction at the call site if you need atomicity.

## Cross-Client Notes

Menu is a **public surface** for every client renderer. When you change any of:

- `Menu.Name` values (they're logical keys)
- `MenuItem.URL` scheme (relative vs absolute)
- `MenuItem.RequiredRole` format (role name vs permission key)
- Introduction of hierarchy via `NodePath`

…update the SPA/mobile/desktop clients in the same PR, and note the change here.

## Common Mistakes

- **Missing `TenantId` when adding items** — every item carries its own `TenantId`. The service sets it on `Menu` during `CreateMenuAsync` but **not** on `MenuItem` during `AddMenuItemAsync`. Set it yourself.
- **Relying on `GetMenuByNameAsync` to return items** — it doesn't. Make the second call.
- **Hard-delete instead of soft-delete** — if you `Remove()` a menu via the generic `IRepository`, you lose history and break any cached renders that captured the menu id.
- **Forgetting `OrderBy(DisplayOrder)` on the client** — `GetMenuItemsByMenuIdAsync` already orders, but if you re-sort on the client (e.g. by title) you lose the admin-controlled order.
- **Using `RequiredRole` for permission checks** — it's a single role name. For permission-based visibility, switch the field or add a complementary `RequiredPermission` property; either way, update this wiki.
- **Treating `NodePath` as required** — nothing populates it today. If you start using it for hierarchies, update the service + this page in the same PR.

## See Also

- [11 — EF Core Repository](./11-ef-core-repository.md) — the `MasterDbContext` this service uses
- [14 — Auth Service](./14-auth-service.md) — `User.IsInRole` backing the `RequiredRole` check
- [15 — Permission Service](./15-permission-service.md) — consider swapping role-gated items to permission-gated
- [00 — Getting Started](./00-getting-started.md) — DI wiring
