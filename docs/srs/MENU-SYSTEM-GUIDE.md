# SmartWorkz v4 - Dynamic Navigation System Guide

**Date:** 2026-03-31
**Status:** Phase 1 Ready
**New Feature:** Menus + MenuItems with Role-Based Visibility & Auto-Sitemap

---

## Overview

The Menu system enables **dynamic, role-based navigation without code changes** and **automatic sitemap generation** for SEO.

### What's New

| Item | Details |
|------|---------|
| **Tables Added** | Menus (groups), MenuItems (hierarchical with HierarchyId) |
| **Schema** | Master (now 20 tables instead of 14) |
| **Database Scripts** | 001-008 (down from 009, Core schema removed) |
| **Features** | Role-based visibility, badges, breadcrumbs, auto-sitemap |
| **DbContexts** | 3-4 instead of 6 (Reference, Transaction, Report, Auth) |
| **API Endpoints** | 5 new menu endpoints + sitemap endpoint |
| **Phase 1 Impact** | +1.5h effort (34-45h total) |

---

## Schema Details

### Master.Menus Table

**Purpose:** Define menu groups (Main, Admin, Footer, Sidebar, etc.)

```sql
CREATE TABLE Master.Menus (
    MenuId UNIQUEIDENTIFIER PRIMARY KEY,
    Code VARCHAR(50) UNIQUE NOT NULL,          -- 'Main', 'Admin', 'Footer'
    Name NVARCHAR(200) NOT NULL,               -- Display name
    Description NVARCHAR(500) NULL,
    TenantId UNIQUEIDENTIFIER NULL,            -- NULL=global, GUID=tenant-specific
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    UpdatedBy UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,

    INDEX IX_MenuCode (Code),
    INDEX IX_MenuTenant (TenantId, Code),
    INDEX IX_MenuDisplayOrder (DisplayOrder)
);
```

---

### Master.MenuItems Table

**Purpose:** Hierarchical menu items with role-based visibility

```sql
CREATE TABLE Master.MenuItems (
    MenuItemId UNIQUEIDENTIFIER PRIMARY KEY,
    MenuId UNIQUEIDENTIFIER NOT NULL,          -- FK → Menus
    NodePath HIERARCHYID NOT NULL,             -- /1/, /1/1/, /1/1/1/, etc.
    Code VARCHAR(100) NOT NULL,                -- 'dashboard', 'products'
    Name NVARCHAR(200) NOT NULL,               -- 'Dashboard', 'Products'
    Url NVARCHAR(500) NULL,                    -- '/admin/dashboard', NULL for groups
    Icon VARCHAR(100) NULL,                    -- 'fa-home', 'fa-box'
    DisplayOrder INT NOT NULL DEFAULT 0,
    ParentMenuItemId UNIQUEIDENTIFIER NULL,    -- FK: for convenience (from HierarchyId)
    IsVisible BIT NOT NULL DEFAULT 1,          -- Hide without deleting
    RequiredRole VARCHAR(100) NULL,            -- 'Admin', 'Manager', NULL=all
    RequiredPermission VARCHAR(200) NULL,      -- Fine-grained: 'reports.read'
    OpenInNewTab BIT NOT NULL DEFAULT 0,
    CssClass VARCHAR(200) NULL,                -- 'active', 'disabled'
    BadgeText VARCHAR(50) NULL,                -- '3', 'NEW', 'Beta'
    BadgeColor VARCHAR(50) NULL,               -- 'red', 'green', 'blue'
    TenantId UNIQUEIDENTIFIER NULL,            -- NULL=global, GUID=tenant-specific
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    UpdatedBy UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,

    CONSTRAINT FK_MenuItem_Menu FOREIGN KEY (MenuId) REFERENCES Menus(MenuId),
    CONSTRAINT FK_MenuItem_Parent FOREIGN KEY (ParentMenuItemId) REFERENCES MenuItems(MenuItemId),

    INDEX IX_MenuItemMenu (MenuId, NodePath),
    INDEX IX_MenuItemDisplayOrder (MenuId, DisplayOrder),
    INDEX IX_MenuItemTenant (TenantId, MenuId),
    INDEX IX_MenuItemRole (RequiredRole)
);
```

---

## Data Examples

### Main Menu Hierarchy

```
Menu: "Main" (Code: "main")
├─ /1/              Home (route: /, icon: fa-home)
├─ /2/              Products (route: /products, icon: fa-box)
│  ├─ /2/1/         Electronics (route: /products/electronics)
│  ├─ /2/2/         Clothing (route: /products/clothing)
│  └─ /2/3/         Books (route: /products/books)
├─ /3/              Orders (route: /orders, icon: fa-shopping-cart)
├─ /4/              My Account (route: /account, icon: fa-user)
└─ /5/              Contact (route: /contact, icon: fa-envelope)

Visibility: All users
```

### Admin Menu Hierarchy

```
Menu: "Admin" (Code: "admin", RequiredRole: "Admin")
├─ /1/              Dashboard (route: /admin/dashboard, icon: fa-chart-line)
├─ /2/              Users (route: /admin/users, icon: fa-users)
│  ├─ /2/1/         Manage Users (route: /admin/users)
│  └─ /2/2/         Roles (route: /admin/roles)
├─ /3/              Tenants (route: /admin/tenants, icon: fa-building, role: "SuperAdmin")
├─ /4/              Reports (route: /admin/reports, icon: fa-chart-bar)
│  ├─ /4/1/         Sales (route: /admin/reports/sales)
│  ├─ /4/2/         Users (route: /admin/reports/users)
│  └─ /4/3/         Activity (route: /admin/reports/activity)
├─ /5/              Settings (route: /admin/settings, icon: fa-cog)
└─ /6/              Notifications (group, icon: fa-bell, badge: "5", badge-color: "red")
   ├─ /6/1/         Email (route: /admin/notifications/email)
   ├─ /6/2/         SMS (route: /admin/notifications/sms)
   └─ /6/3/         Push (route: /admin/notifications/push)

Visibility: Admin users only
```

### Footer Menu

```
Menu: "Footer" (Code: "footer")
├─ /1/              Privacy (route: /privacy)
├─ /2/              Terms (route: /terms)
├─ /3/              Security (route: /security)
└─ /4/              Sitemap (route: /sitemap)

Visibility: All users
```

---

## Phase 1 Implementation Tasks

### 1. Database (0.5h addition)
- Add Config section to Master (TenantSubscriptions, TenantSettings, FeatureFlags from Core)
- Add Navigation section to Master (Menus, MenuItems)
- Create seed data for default menus (Main, Admin, Footer)
- Create indexes on (MenuId, NodePath), (TenantId, MenuId), (RequiredRole)

### 2. Domain Entities (0.5h addition)
```csharp
public class Menu
{
    public Guid MenuId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation
    public virtual ICollection<MenuItem> MenuItems { get; set; }
    public virtual Tenant Tenant { get; set; }
}

public class MenuItem
{
    public Guid MenuItemId { get; set; }
    public Guid MenuId { get; set; }
    public HierarchyId NodePath { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Icon { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentMenuItemId { get; set; }
    public bool IsVisible { get; set; }
    public string RequiredRole { get; set; }
    public string RequiredPermission { get; set; }
    public bool OpenInNewTab { get; set; }
    public string CssClass { get; set; }
    public string BadgeText { get; set; }
    public string BadgeColor { get; set; }
    public Guid? TenantId { get; set; }

    // Navigation
    public virtual Menu Menu { get; set; }
    public virtual MenuItem ParentMenuItem { get; set; }
    public virtual ICollection<MenuItem> ChildMenuItems { get; set; }
}
```

### 3. Services (1h addition)
```csharp
public interface IMenuService
{
    Task<IEnumerable<MenuDto>> GetAllMenusAsync(int? tenantId = null);
    Task<MenuDto> GetMenuByCodeAsync(string code, int? tenantId = null);
    Task<IEnumerable<MenuItemDto>> GetMenuItemsAsync(Guid menuId, bool visibleOnly = true);
    Task<IEnumerable<MenuItemDto>> GetMenuItemTreeAsync(Guid menuId, ClaimsPrincipal user);
    Task<IEnumerable<MenuItemDto>> GetBreadcrumbAsync(Guid menuItemId);
    Task<MenuDto> CreateMenuAsync(CreateMenuDto dto);
    Task UpdateMenuAsync(Guid menuId, UpdateMenuDto dto);
    Task DeleteMenuAsync(Guid menuId);
    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto);
    Task UpdateMenuItemAsync(Guid menuItemId, UpdateMenuItemDto dto);
    Task DeleteMenuItemAsync(Guid menuItemId);
    Task<XmlDocument> GenerateSitemapAsync(int? tenantId = null);
}
```

### 4. API Endpoints (5 new endpoints)

```
GET    /api/v1/menus                          List all menus
GET    /api/v1/menus/{code}                   Get menu by code
GET    /api/v1/menus/{menuId}/items           Get menu items (flat)
GET    /api/v1/menus/{menuId}/tree            Get menu items (hierarchical tree)
GET    /api/v1/menus/{itemId}/breadcrumb      Get breadcrumb path
POST   /api/v1/menus                          Create menu
PUT    /api/v1/menus/{menuId}                 Update menu
DELETE /api/v1/menus/{menuId}                 Delete menu

POST   /api/v1/menuitems                      Create menu item
PUT    /api/v1/menuitems/{itemId}             Update menu item
DELETE /api/v1/menuitems/{itemId}             Delete menu item

GET    /sitemap.xml                           Generate SEO sitemap
```

### 5. Controller

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuDto>>> GetMenus()
    {
        var tenantId = User.GetTenantId(); // from claims
        var menus = await _menuService.GetAllMenusAsync(tenantId);
        return Ok(menus);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<MenuDto>> GetMenuByCode(string code)
    {
        var tenantId = User.GetTenantId();
        var menu = await _menuService.GetMenuByCodeAsync(code, tenantId);
        if (menu == null) return NotFound();
        return Ok(menu);
    }

    [HttpGet("{menuId}/tree")]
    public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuTree(Guid menuId)
    {
        var items = await _menuService.GetMenuItemTreeAsync(menuId, User);
        return Ok(items);
    }

    // ... more endpoints
}

[Route("sitemap.xml")]
public class SitemapController : ControllerBase
{
    private readonly IMenuService _menuService;

    [HttpGet]
    [Produces("application/xml")]
    public async Task<XmlDocument> GetSitemap()
    {
        var tenantId = User.GetTenantId();
        return await _menuService.GenerateSitemapAsync(tenantId);
    }
}
```

---

## Key Queries (EF Core)

### Get All Items in a Menu
```csharp
var items = await dbContext.MenuItems
    .Where(m => m.MenuId == menuId && m.IsDeleted == false && m.IsVisible)
    .OrderBy(m => m.NodePath)
    .ToListAsync();
```

### Get Children of a MenuItem
```csharp
var parentPath = parentItem.NodePath;
var children = await dbContext.MenuItems
    .Where(m => m.NodePath.IsDescendantOf(parentPath)
             && m.NodePath.GetLevel() == parentPath.GetLevel() + 1
             && m.IsDeleted == false)
    .OrderBy(m => m.DisplayOrder)
    .ToListAsync();
```

### Get Breadcrumb Path
```csharp
var itemPath = item.NodePath;
var breadcrumb = await dbContext.MenuItems
    .Where(m => itemPath.IsDescendantOf(m.NodePath) || m.NodePath == itemPath)
    .OrderBy(m => m.NodePath.GetLevel())
    .ToListAsync();
```

### Filter by Role
```csharp
var userRoles = User.GetRoles(); // from claims
var items = await dbContext.MenuItems
    .Where(m => m.MenuId == menuId
             && (m.RequiredRole == null || userRoles.Contains(m.RequiredRole))
             && m.IsDeleted == false
             && m.IsVisible)
    .OrderBy(m => m.NodePath)
    .ToListAsync();
```

---

## Sitemap Generation

### Example Output (sitemap.xml)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <url>
    <loc>https://yourdomain.com/</loc>
    <lastmod>2026-03-31T12:00:00Z</lastmod>
  </url>
  <url>
    <loc>https://yourdomain.com/products</loc>
    <lastmod>2026-03-31T12:00:00Z</lastmod>
  </url>
  <url>
    <loc>https://yourdomain.com/products/electronics</loc>
    <lastmod>2026-03-31T12:00:00Z</lastmod>
  </url>
  <url>
    <loc>https://yourdomain.com/orders</loc>
    <lastmod>2026-03-31T12:00:00Z</lastmod>
  </url>
  <!-- ... more URLs ... -->
</urlset>
```

### Service Implementation

```csharp
public async Task<XmlDocument> GenerateSitemapAsync(int? tenantId = null)
{
    var mainMenu = await _repository.GetMenuByCodeAsync("Main", tenantId);
    if (mainMenu == null) return new XmlDocument(); // Return empty sitemap

    var visibleItems = await _menuService.GetMenuItemsAsync(mainMenu.MenuId, visibleOnly: true);

    var sitemap = new XmlDocument();
    var urlset = sitemap.CreateElement("urlset");
    urlset.SetAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");

    foreach (var item in visibleItems.Where(i => !string.IsNullOrEmpty(i.Url)))
    {
        var url = sitemap.CreateElement("url");

        var loc = sitemap.CreateElement("loc");
        loc.InnerText = $"https://yourdomain.com{item.Url}";
        url.AppendChild(loc);

        var lastmod = sitemap.CreateElement("lastmod");
        lastmod.InnerText = item.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
        url.AppendChild(lastmod);

        urlset.AppendChild(url);
    }

    sitemap.AppendChild(urlset);
    return sitemap;
}
```

---

## Multi-Tenant Override Pattern

### Global Main Menu (TenantId = NULL)
```sql
INSERT INTO Master.Menus (MenuId, Code, Name, TenantId, IsActive)
VALUES (NEWID(), 'Main', 'Main Navigation', NULL, 1);
```

### Tenant-Specific Override (TenantId = specific)
```sql
-- Acme Corp wants different navigation
INSERT INTO Master.Menus (MenuId, Code, Name, TenantId, IsActive)
VALUES (NEWID(), 'Main', 'ACME Navigation', '12345-tenant-id', 1);
```

### Query Resolution (Tenant-First, Falls Back to Global)
```csharp
public async Task<MenuDto> GetMenuByCodeAsync(string code, int? tenantId = null)
{
    // Try tenant-specific first
    var menu = await dbContext.Menus
        .FirstOrDefaultAsync(m => m.Code == code && m.TenantId == tenantId && m.IsDeleted == false);

    // Fall back to global
    if (menu == null)
    {
        menu = await dbContext.Menus
            .FirstOrDefaultAsync(m => m.Code == code && m.TenantId == null && m.IsDeleted == false);
    }

    return _mapper.Map<MenuDto>(menu);
}
```

---

## Benefits Summary

| Benefit | Use Case |
|---------|----------|
| **Dynamic Navigation** | Change menu structure in database, no code deployment |
| **Role-Based Visibility** | Show different menus to Admin vs. Customer vs. Manager |
| **Hierarchical Structure** | Unlimited depth (Nav > Products > Electronics > Laptops) |
| **Breadcrumb Support** | Navigate HierarchyId path for breadcrumb trail |
| **Badges & Notifications** | Show counts ("3 pending", "2 messages") on menu items |
| **SEO Sitemap** | Auto-generate sitemap.xml for search engines |
| **Multi-Tenant** | Global defaults + tenant-specific overrides |
| **Soft Delete** | Archive menus without losing history |
| **Audit Trail** | CreatedBy, UpdatedBy, CreatedAt, UpdatedAt |

---

## Database Scripts Update

### Script Changes
- ~~004_CreateTables_Core.sql~~ **DELETED** (moved to Master)
- **002_CreateTables_Master.sql** expanded to include Config + Navigation sections
- **007_SeedData.sql** includes default menu seeds (Main, Admin, Footer)
- **008_CreateIndexes.sql** includes menu indexes

### Seed Data Example
```sql
-- Menus
INSERT INTO Master.Menus VALUES
  (NEWID(), 'main', 'Main Navigation', NULL, 1, 0, ...),
  (NEWID(), 'admin', 'Admin Menu', NULL, 1, 0, ...),
  (NEWID(), 'footer', 'Footer Links', NULL, 1, 0, ...);

-- Main Menu Items
INSERT INTO Master.MenuItems VALUES
  (NEWID(), @mainMenuId, '/1/', 'home', 'Home', '/', 'fa-home', 0, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, ...),
  (NEWID(), @mainMenuId, '/2/', 'products', 'Products', '/products', 'fa-box', 1, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, ...),
  (NEWID(), @mainMenuId, '/2/1/', 'electronics', 'Electronics', '/products/electronics', NULL, 0, @productsItemId, NULL, NULL, 1, NULL, NULL, NULL, NULL, ...);
```

---

## Updated Project Statistics

| Metric | Old | New | Change |
|--------|-----|-----|--------|
| **Schemas** | 6 | 5 | -1 (Core merged to Master) |
| **Master Tables** | 14 | 20 | +6 (Config + Navigation) |
| **Total Tables** | 40 | 42 | +2 (Menus, MenuItems) |
| **DbContexts** | 6 | 3-4 | -2 to 3 (lighter, more focused) |
| **Database Scripts** | 9 | 8 | -1 (Core script removed) |
| **Phase 1 Hours** | 35-46 | 34-45 | -1h (cleaner org, fewer scripts) |
| **API Endpoints** | 20+ | 25+ | +5 (menu endpoints) |

---

## Implementation Checklist

- [ ] Add Menus table to 002_CreateTables_Master.sql
- [ ] Add MenuItems table to 002_CreateTables_Master.sql
- [ ] Remove 004_CreateTables_Core.sql
- [ ] Move Config tables to 002_CreateTables_Master.sql
- [ ] Renumber database scripts (004→004, 005→005, etc.)
- [ ] Create Menu and MenuItem domain entities
- [ ] Add menu seed data to 007_SeedData.sql
- [ ] Create IMenuService interface
- [ ] Implement MenuService with all methods
- [ ] Create MenusController with 5+ endpoints
- [ ] Create SitemapController for /sitemap.xml endpoint
- [ ] Create DTOs (CreateMenuDto, UpdateMenuDto, MenuDto, MenuItemDto, etc.)
- [ ] Add menu indexes to 008_CreateIndexes.sql
- [ ] Write unit tests for MenuService
- [ ] Test HierarchyId queries (GetChildren, GetBreadcrumb, GetAncestors)
- [ ] Test role-based filtering
- [ ] Test multi-tenant menu resolution
- [ ] Test sitemap generation
- [ ] Integration test: Create menu → Add items → Query tree → Generate sitemap

---

**Status:** ✅ Documentation complete, ready for Phase 1 implementation

