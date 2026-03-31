# Menu Management System - Quick Guide

**Feature:** Dynamic, role-based navigation without code changes
**Status:** Phase 1 Ready
**Tables:** Master.Menus + Master.MenuItems (HierarchyId tree)
**Effort:** ~1.5 hours (minimal Phase 1 addition)

---

## What Is Menu Management?

Instead of hardcoding navigation in HTML/Razor templates, **all menus are stored in the database** and rendered dynamically:

✅ Change menu structure without redeploying code
✅ Show different menus per role (Admin, Customer, Manager)
✅ Support unlimited nesting (Home > Products > Electronics > Laptops)
✅ Auto-generate sitemap.xml for SEO
✅ Add badges/notifications (3 pending orders, 2 messages)

---

## Menu Hierarchy Example

### Main Menu (Public Navigation)
```
Main
├─ Home              /              (fa-home, all users)
├─ Products         /products       (fa-box, all users)
│  ├─ Electronics   /products/electronics
│  ├─ Clothing      /products/clothing
│  └─ Books         /products/books
├─ Orders           /orders         (fa-shopping-cart, logged-in users)
├─ My Account       /account        (fa-user, logged-in users)
└─ Contact          /contact        (fa-envelope, all users)
```

### Admin Menu (Admin Only)
```
Admin                                (RequiredRole: "Admin")
├─ Dashboard        /admin/dashboard (fa-chart-line)
├─ Users            /admin/users     (fa-users)
│  ├─ Manage Users  /admin/users
│  └─ Roles         /admin/roles
├─ Tenants          /admin/tenants   (fa-building, RequiredRole: "SuperAdmin")
├─ Reports          /admin/reports   (fa-chart-bar)
│  ├─ Sales         /admin/reports/sales
│  ├─ Users         /admin/reports/users
│  └─ Activity      /admin/reports/activity
├─ Settings         /admin/settings  (fa-cog)
└─ Notifications    (badge: "5", badge-color: "red")
   ├─ Email         /admin/notifications/email
   ├─ SMS           /admin/notifications/sms
   └─ Push          /admin/notifications/push
```

### Footer Menu (Public)
```
Footer
├─ Privacy          /privacy
├─ Terms            /terms
├─ Security         /security
└─ Sitemap          /sitemap
```

---

## Database Schema

### Menus Table

```sql
Master.Menus
├─ MenuId (GUID, PK)
├─ Code (VARCHAR 50, unique) ──────── 'Main', 'Admin', 'Footer', 'Sidebar'
├─ Name (NVARCHAR 200) ───────────── Display name (shown to admin)
├─ Description (NVARCHAR 500, null)
├─ TenantId (GUID, nullable) ──────── NULL=global, GUID=tenant override
├─ IsActive (BIT, default 1)
├─ DisplayOrder (INT)
└─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
```

### MenuItems Table

```sql
Master.MenuItems
├─ MenuItemId (GUID, PK)
├─ MenuId (GUID, FK) ───────────── Parent menu
├─ NodePath (HierarchyId) ──────── /1/, /1/1/, /1/1/1/ (tree structure)
├─ Code (VARCHAR 100) ──────────── 'dashboard', 'products', 'laptops'
├─ Name (NVARCHAR 200) ────────── 'Dashboard', 'Products', 'Laptops'
├─ Url (NVARCHAR 500, null) ───── '/admin/dashboard' (NULL for groups)
├─ Icon (VARCHAR 100, null) ───── 'fa-chart-line' (Font Awesome)
├─ DisplayOrder (INT) ─────────── Sort order (0, 10, 20, ...)
├─ ParentMenuItemId (GUID, null) ─ For convenience (from HierarchyId)
├─ IsVisible (BIT, default 1) ──── Hide without deleting
├─ RequiredRole (VARCHAR 100, null) ─ 'Admin', 'Manager', NULL=all
├─ RequiredPermission (VARCHAR 200, null) ─ Fine-grained: 'reports.read'
├─ OpenInNewTab (BIT, default 0)
├─ CssClass (VARCHAR 200, null) ─ 'active', 'disabled'
├─ BadgeText (VARCHAR 50, null) ─ '3', 'NEW', 'Beta'
├─ BadgeColor (VARCHAR 50, null) ─ 'red', 'green', 'blue'
├─ TenantId (GUID, nullable) ───── NULL=global, GUID=tenant override
└─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
```

**Key Column Notes:**
- **NodePath:** HierarchyId (like file paths: `/1/2/3/`)
- **RequiredRole:** Single role check for basic access
- **RequiredPermission:** Granular permission check (e.g., "reports.read", "users.write")
- **TenantId:** Support global menus + tenant overrides (falls back to global if tenant-specific not found)

---

## API Endpoints

### Fetch Menus

**List all menus:**
```
GET /api/v1/menus
Response: [
  { MenuId: ..., Code: "Main", Name: "Main Navigation", ... },
  { MenuId: ..., Code: "Admin", Name: "Admin Menu", ... }
]
```

**Get menu by code (Main, Admin, Footer):**
```
GET /api/v1/menus/Main
Response: { MenuId: ..., Code: "Main", Name: "Main Navigation", ... }
```

**Get menu items (flat list):**
```
GET /api/v1/menus/{menuId}/items
Response: [
  { MenuItemId: ..., Code: "home", Name: "Home", Url: "/", Icon: "fa-home" },
  { MenuItemId: ..., Code: "products", Name: "Products", Url: "/products", ... },
  { MenuItemId: ..., Code: "electronics", Name: "Electronics", Url: "/products/electronics", ... }
]
```

**Get menu items (hierarchical tree, role-filtered):**
```
GET /api/v1/menus/{menuId}/tree
Response: [
  {
    MenuItemId: ...,
    Code: "home",
    Name: "Home",
    Url: "/",
    Icon: "fa-home",
    Children: []
  },
  {
    MenuItemId: ...,
    Code: "products",
    Name: "Products",
    Url: "/products",
    Icon: "fa-box",
    Children: [
      { Code: "electronics", Name: "Electronics", Url: "/products/electronics", Children: [] },
      { Code: "clothing", Name: "Clothing", Url: "/products/clothing", Children: [] }
    ]
  }
]
```

**Get breadcrumb path (for a specific menu item):**
```
GET /api/v1/menus/{itemId}/breadcrumb
Response: [
  { Code: "home", Name: "Home", Url: "/" },
  { Code: "products", Name: "Products", Url: "/products" },
  { Code: "electronics", Name: "Electronics", Url: "/products/electronics" }
]
```

### Manage Menus

**Create menu:**
```
POST /api/v1/menus
{
  "Code": "Custom",
  "Name": "Custom Menu",
  "Description": "For custom purposes",
  "IsActive": true
}
Response: { MenuId: ..., Code: "Custom", ... }
```

**Update menu:**
```
PUT /api/v1/menus/{menuId}
{
  "Name": "Updated Name",
  "IsActive": false
}
```

**Delete menu:**
```
DELETE /api/v1/menus/{menuId}
```

### Manage Menu Items

**Create menu item:**
```
POST /api/v1/menuitems
{
  "MenuId": "...",
  "Code": "newitem",
  "Name": "New Item",
  "Url": "/new-page",
  "Icon": "fa-star",
  "DisplayOrder": 10,
  "RequiredRole": "Manager"
}
Response: { MenuItemId: ..., NodePath: "/1/1/", ... }
```

**Update menu item:**
```
PUT /api/v1/menuitems/{itemId}
{
  "Name": "Updated Item Name",
  "BadgeText": "3",
  "BadgeColor": "red"
}
```

**Delete menu item:**
```
DELETE /api/v1/menuitems/{itemId}
```

### SEO Sitemap

**Generate sitemap.xml:**
```
GET /sitemap.xml
Response: (XML)
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <url>
    <loc>https://yoursite.com/</loc>
    <lastmod>2026-03-31T12:00:00Z</lastmod>
  </url>
  <url>
    <loc>https://yoursite.com/products</loc>
    <lastmod>2026-03-31T12:00:00Z</lastmod>
  </url>
  <!-- ... more URLs from menu items with non-null Urls ... -->
</urlset>
```

---

## Role-Based Visibility

### Query: Get menu items visible to user

```csharp
// Get user's roles from claims
var userRoles = User.GetRoles(); // ["Customer", "Premium"]

// Query menu items
var visibleItems = await dbContext.MenuItems
    .Where(m => m.MenuId == menuId
        && (m.RequiredRole == null || userRoles.Contains(m.RequiredRole))
        && m.IsDeleted == false
        && m.IsVisible)
    .OrderBy(m => m.NodePath)
    .ToListAsync();
```

### Example: Admin sees more items

**Customer (no special roles):**
```
Main Menu Items:
├─ Home (RequiredRole: null)
├─ Products (RequiredRole: null)
├─ Orders (RequiredRole: "Customer")
└─ My Account (RequiredRole: "Customer")

Admin Menu: NOT VISIBLE (RequiredRole: "Admin")
```

**Admin (role: "Admin"):**
```
Main Menu Items:
├─ Home (RequiredRole: null)
├─ Products (RequiredRole: null)
├─ Orders (RequiredRole: "Customer")
└─ My Account (RequiredRole: "Customer")

Admin Menu: VISIBLE (RequiredRole: "Admin") ✓
├─ Dashboard
├─ Users
├─ Reports
├─ Settings
└─ Notifications
```

---

## Breadcrumb Navigation

### Query: Get breadcrumb for "/products/electronics"

```csharp
var item = await dbContext.MenuItems
    .FirstOrDefaultAsync(m => m.Url == "/products/electronics");

var breadcrumb = await dbContext.MenuItems
    .Where(m => item.NodePath.IsDescendantOf(m.NodePath) || m.NodePath == item.NodePath)
    .OrderBy(m => m.NodePath.GetLevel())
    .ToListAsync();

// Result:
// [
//   { Code: "home", Name: "Home", Url: "/" },
//   { Code: "products", Name: "Products", Url: "/products" },
//   { Code: "electronics", Name: "Electronics", Url: "/products/electronics" }
// ]
```

### HTML Rendering:
```html
<nav aria-label="breadcrumb">
  <ol class="breadcrumb">
    <li class="breadcrumb-item"><a href="/">Home</a></li>
    <li class="breadcrumb-item"><a href="/products">Products</a></li>
    <li class="breadcrumb-item active" aria-current="page">Electronics</li>
  </ol>
</nav>
```

---

## Multi-Tenant Override Pattern

### Scenario: Acme Corp wants custom navigation

**Step 1: Create global Main menu (for all tenants)**
```sql
INSERT INTO Master.Menus (MenuId, Code, Name, TenantId, IsActive)
VALUES (NEWID(), 'Main', 'Main Navigation', NULL, 1);
-- Add menu items...
```

**Step 2: Create Acme-specific override**
```sql
INSERT INTO Master.Menus (MenuId, Code, Name, TenantId, IsActive)
VALUES (NEWID(), 'Main', 'ACME Custom Navigation', '{acme-tenant-id}', 1);
-- Add Acme-specific menu items...
```

**Step 3: Query resolution (tenant-first, fallback to global)**
```csharp
public async Task<MenuDto> GetMenuByCodeAsync(string code, Guid? tenantId)
{
    // Try tenant-specific first
    var menu = await dbContext.Menus
        .FirstOrDefaultAsync(m => m.Code == code && m.TenantId == tenantId);

    // Fall back to global
    if (menu == null)
    {
        menu = await dbContext.Menus
            .FirstOrDefaultAsync(m => m.Code == code && m.TenantId == null);
    }

    return menu;
}
```

**Result:**
- Acme Corp gets their custom menu
- Other tenants get the default menu
- No code changes needed

---

## HierarchyId Queries

### Get immediate children of a menu item

```csharp
var parent = await dbContext.MenuItems
    .FirstOrDefaultAsync(m => m.Code == "products");

var children = await dbContext.MenuItems
    .Where(m => m.MenuId == parent.MenuId
        && m.NodePath.IsDescendantOf(parent.NodePath)
        && m.NodePath.GetLevel() == parent.NodePath.GetLevel() + 1)
    .OrderBy(m => m.DisplayOrder)
    .ToListAsync();

// Result: [Electronics, Clothing, Books]
```

### Get all descendants of a menu item

```csharp
var descendants = await dbContext.MenuItems
    .Where(m => m.MenuId == parent.MenuId
        && m.NodePath.IsDescendantOf(parent.NodePath))
    .OrderBy(m => m.NodePath)
    .ToListAsync();

// Result: [Electronics, Clothing, Books, and any sub-items]
```

### Get all ancestors (for breadcrumb)

```csharp
var item = ...;
var ancestors = await dbContext.MenuItems
    .Where(m => m.MenuId == item.MenuId
        && item.NodePath.IsDescendantOf(m.NodePath))
    .OrderBy(m => m.NodePath)
    .ToListAsync();

// Result: [Home, Products] (parents of Electronics)
```

---

## Phase 1 Implementation Checklist

### Database (0.5h)
- [ ] Create Master.Menus table
- [ ] Create Master.MenuItems table with HierarchyId
- [ ] Create indexes on (MenuId, NodePath), (TenantId, MenuId), (RequiredRole)
- [ ] Seed default menus (Main, Admin, Footer)
- [ ] Seed default menu items

### Domain Entities (0.25h)
- [ ] Create Menu entity class
- [ ] Create MenuItem entity class with HierarchyId support

### EF Core Mapping (0.25h)
- [ ] Configure Menu → DbSet<Menu>
- [ ] Configure MenuItem → DbSet<MenuItem>
- [ ] Set up navigation properties (Menu.MenuItems, MenuItem.ParentMenuItem, MenuItem.ChildMenuItems)

### Service Layer (0.5h)
- [ ] Create IMenuService interface
- [ ] Implement MenuService with all CRUD + queries (breadcrumb, sitemap, role-filtering)

### API Endpoints (0.25h)
- [ ] Create MenusController (GET /api/v1/menus, POST, PUT, DELETE)
- [ ] Create MenuItemsController (GET, POST, PUT, DELETE)
- [ ] Create SitemapController (GET /sitemap.xml)

### DTOs (0.25h)
- [ ] Create MenuDto, CreateMenuDto, UpdateMenuDto
- [ ] Create MenuItemDto, CreateMenuItemDto, UpdateMenuItemDto

---

## Benefits Summary

| Benefit | Impact |
|---------|--------|
| **No Code Redeployment** | Change navigation in database, apply immediately |
| **Role-Based Access** | Show/hide menu items based on user roles |
| **Unlimited Nesting** | Products > Electronics > Laptops > Gaming (no depth limits) |
| **Breadcrumbs** | Automatically build breadcrumb trails from HierarchyId |
| **Badges** | Show counts/notifications ("3 pending", "2 messages") |
| **Multi-Tenant Override** | Global menus + tenant-specific customization |
| **SEO Sitemap** | Auto-generate sitemap.xml from visible menu items |
| **Performance** | Indexes on MenuId, NodePath, RequiredRole for fast queries |

---

## File Reference

- **Full Guide:** [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md)
- **Schema Docs:** [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md#19-navigation-new)
- **Architecture:** [V4-ARCHITECTURE-FINAL.md](V4-ARCHITECTURE-FINAL.md#master-schema)
