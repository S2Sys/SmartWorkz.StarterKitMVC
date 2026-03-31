# SmartWorkz v4 - Quick Reference

**Updated:** 2026-03-31 | **Phase:** 1 Ready | **Status:** ✅ Complete

---

## Schema at a Glance

```
5 Schemas | 42 Tables | 3-4 DbContexts | 8 Scripts | 34-45 hours Phase 1

MASTER (20)              SHARED (5)          TRANSACTION (1)
├─ Geo (2)              ├─ Addresses         └─ Orders
├─ i18n (2)             ├─ Attachments
├─ Lookups (4)          ├─ Comments          REPORT (4)
├─ Templates (3)        ├─ StateHistory      ├─ ReportDefinitions
├─ Tags (1)             └─ Preferences       ├─ ReportSchedules
├─ Tenants (1)                              ├─ ReportExecutions
├─ SEO (2)              AUTH (13)            └─ ReportMetadata
├─ Config (3) ← NEW     ├─ Users
└─ Navigation (2) ← NEW ├─ Roles
                        ├─ Sessions
                        └─ Audit/Logs
```

---

## What Changed

| Item | Before | After | Change |
|------|--------|-------|--------|
| Schemas | 6 | 5 | Core → Master |
| Master Tables | 14 | 20 | +Config, +Navigation |
| Total Tables | 40 | 42 | +Menus, +MenuItems |
| Scripts | 9 | 8 | Removed Core script |
| DbContexts | 6 | 3-4 | Consolidated |
| Phase 1 Hours | 35-46 | 34-45 | Slightly better |
| Menu Features | — | ✅ 7 features | Dynamic nav, role-based, sitemap |

---

## Key Files

### 📋 Must Read (in order)
1. **[README.md](README.md)** — Project overview & quick start
2. **[SCHEMA-SUMMARY-LEAN.md](SCHEMA-SUMMARY-LEAN.md)** — Schema overview (30 min)
3. **[docs/srs/SCHEMA-REVIEW-v2.md](docs/srs/SCHEMA-REVIEW-v2.md)** — Full table definitions
4. **[MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md)** — Menu implementation guide

### 📊 Reference
- **[UPDATES-SUMMARY-v4.md](UPDATES-SUMMARY-v4.md)** — This update summary
- **[docs/srs/IMPLEMENTATION-PLAN.md](docs/srs/IMPLEMENTATION-PLAN.md)** — Phase 1-4 roadmap
- **[docs/srs/GEO-HIERARCHY-ANALYSIS.md](docs/srs/GEO-HIERARCHY-ANALYSIS.md)** — Geo design rationale
- **[REVIEW-CHECKLIST.md](REVIEW-CHECKLIST.md)** — Pre-implementation checklist

### 🗂️ Archived
- **[docs/old/](docs/old/)** — v1 documentation (reference only)

---

## New Tables at a Glance

### Menus
```sql
MenuId (PK) | Code | Name | TenantId | IsActive | DisplayOrder | ...Audit
```
- **Purpose:** Define menu groups (Main, Admin, Footer)
- **Multi-Tenant:** TenantId nullable (global or tenant-specific)
- **Example Codes:** "main", "admin", "footer", "sidebar"

### MenuItems
```sql
MenuItemId (PK) | MenuId (FK) | NodePath (HierarchyId) | Code | Name
| Url | Icon | RequiredRole | BadgeText | BadgeColor | ...Audit
```
- **Purpose:** Hierarchical menu items
- **Hierarchy:** /1/, /1/1/, /1/1/1/ (unlimited depth)
- **Visibility:** RequiredRole (admin, manager) + RequiredPermission (granular)
- **Features:** Badges (3, NEW), icons (fa-home), open in new tab
- **Queries:** GetChildren, GetBreadcrumb, GetAncestors (HierarchyId optimized)

---

## Config Tables (Moved from Core → Master)

### TenantSubscriptions
```sql
TenantSubscriptionId | TenantId | PlanCode | StartDate | EndDate
| Status | AutoRenew | ...
```
Plans: "Starter", "Professional", "Enterprise"
Status: "Active", "Suspended", "Expired", "Cancelled"

### TenantSettings (Key-Value)
```sql
TenantSettingId | TenantId | Key | Value | DataType | IsEncrypted
```
Examples: EmailFromAddress, TimeZone, DateFormat, CustomCss

### FeatureFlags
```sql
FeatureFlagId | TenantId | Name | IsEnabled | RolloutPercent | ValidFrom | ValidTo
```
Examples: AdvancedReporting, CustomDomain, 2FA, DarkMode

---

## Database Scripts (8 total)

```
001_CreateSchemas.sql
  └─ Master, Shared, Transaction, Report, Auth (NO Core)

002_CreateTables_Master.sql (20 tables)
  ├─ Geo: Countries, GeoHierarchy (Option C Hybrid)
  ├─ i18n: Languages, Translations
  ├─ Lookups, Categories, EntityStates, EntityStateTransitions
  ├─ Templates, NotificationChannels, TemplateGroups
  ├─ Tags, Tenants, SeoMeta, UrlRedirects
  ├─ Config: TenantSubscriptions, TenantSettings, FeatureFlags ← MOVED
  └─ Navigation: Menus, MenuItems ← NEW

003_CreateTables_Shared.sql → 5 tables
004_CreateTables_Transaction.sql → 1 table
005_CreateTables_Report.sql → 4 tables
006_CreateTables_Auth.sql → 13 tables
007_SeedData.sql (includes menu seeds)
008_CreateIndexes.sql (includes menu indexes)
```

---

## Domain Entities (42 total)

**Master (20):** Country, GeoHierarchy, Language, Translation, Lookup, Category, EntityState, EntityStateTransition, NotificationChannel, TemplateGroup, Template, Tag, Tenant, SeoMeta, UrlRedirect, TenantSubscription, TenantSetting, FeatureFlag, **Menu**, **MenuItem**

**Shared (5):** Address, Attachment, Comment, StateHistory, PreferenceDefinition

**Transaction (1):** Order

**Report (4):** ReportDefinition, ReportSchedule, ReportExecution, ReportMetadata

**Auth (13):** User, UserProfile, UserPreference, Role, Permission, RolePermission, UserRole, RefreshToken, VerificationCode, ExternalLogin, AuditLog, ActivityLog, NotificationLog

---

## DbContexts (Choose One)

### Option A: 4 DbContexts (Recommended)
```csharp
ReferenceDbContext   // Master (20) + Shared (5) = 25 tables
TransactionDbContext // Transaction (1)
ReportDbContext      // Report (4)
AuthDbContext        // Auth (13)
```
✅ Lighter, focused, easier to test
✅ Reports can be scaled independently

### Option B: 3 DbContexts (Maximum Simplicity)
```csharp
ReferenceDbContext   // Master (20) + Shared (5) = 25 tables
TransactionDbContext // Transaction (1) + Report (4) = 5 tables
AuthDbContext        // Auth (13)
```
✅ Fewer contexts to manage
✅ Simple dependency injection

---

## API Endpoints (25+)

### Menu Endpoints (NEW)
```
GET    /api/v1/menus                    # List all menus
GET    /api/v1/menus/{code}             # Get menu by code
GET    /api/v1/menus/{menuId}/items     # Flat list
GET    /api/v1/menus/{menuId}/tree      # Hierarchical tree
GET    /api/v1/menus/{itemId}/breadcrumb # Breadcrumb path
POST   /api/v1/menus                    # Create menu
PUT    /api/v1/menus/{menuId}           # Update menu
DELETE /api/v1/menus/{menuId}           # Delete menu
POST   /api/v1/menuitems                # Create item
PUT    /api/v1/menuitems/{itemId}       # Update item
DELETE /api/v1/menuitems/{itemId}       # Delete item
```

### Sitemap (NEW)
```
GET    /sitemap.xml                     # Auto-generated SEO sitemap
```

### Existing Endpoints (20+)
Auth, Users, Tenants, Lookups, Orders, Reports, etc. (unchanged)

---

## Menu Features

| Feature | Capability |
|---------|-----------|
| **Dynamic Navigation** | Modify menus in database, no code deploy |
| **Hierarchical** | /1/, /1/1/, /1/1/1/ — unlimited depth |
| **Role-Based** | RequiredRole: "Admin", "Manager", null=all |
| **Granular Security** | RequiredPermission: "reports.read", "users.delete" |
| **Breadcrumbs** | Query HierarchyId path for breadcrumb trail |
| **Badges** | Show counts ("3"), status ("NEW"), color-coded |
| **Sitemap** | Auto-generate sitemap.xml from menu items |
| **Multi-Tenant** | Global menus + tenant-specific overrides |
| **Soft Delete** | Archive menus without losing history |
| **Audit Trail** | CreatedBy, UpdatedBy, CreatedAt, UpdatedAt |

---

## Example Menu Hierarchy

```
Main Menu (/1/)
├─ /1/1/ Home (route: /, icon: fa-home)
├─ /1/2/ Products (route: /products, icon: fa-box)
│  ├─ /1/2/1/ Electronics (route: /products/electronics)
│  ├─ /1/2/2/ Clothing (route: /products/clothing)
│  └─ /1/2/3/ Books (route: /products/books)
├─ /1/3/ Orders (route: /orders, icon: fa-shopping-cart, role: "Customer")
└─ /1/4/ Admin (group, icon: fa-cog, role: "Admin")
   ├─ /1/4/1/ Dashboard (route: /admin/dashboard)
   ├─ /1/4/2/ Users (route: /admin/users)
   └─ /1/4/3/ Settings (route: /admin/settings)
```

---

## Common Queries

### Get Menu Tree (Role-Based)
```csharp
var userRoles = User.GetRoles();
var items = await dbContext.MenuItems
    .Where(m => m.MenuId == menuId
             && (m.RequiredRole == null || userRoles.Contains(m.RequiredRole))
             && m.IsDeleted == false
             && m.IsVisible)
    .OrderBy(m => m.NodePath)
    .ToListAsync();
```

### Get Children of MenuItem
```csharp
var children = await dbContext.MenuItems
    .Where(m => m.NodePath.IsDescendantOf(parentPath)
             && m.NodePath.GetLevel() == parentPath.GetLevel() + 1)
    .OrderBy(m => m.DisplayOrder)
    .ToListAsync();
```

### Get Breadcrumb Path
```csharp
var breadcrumb = await dbContext.MenuItems
    .Where(m => itemPath.IsDescendantOf(m.NodePath) || m.NodePath == itemPath)
    .OrderBy(m => m.NodePath.GetLevel())
    .ToListAsync();
```

### Resolve Tenant Menu (Tenant-First)
```csharp
var menu = await dbContext.Menus
    .FirstOrDefaultAsync(m => m.Code == code && m.TenantId == tenantId)
    ?? await dbContext.Menus.FirstOrDefaultAsync(m => m.Code == code && m.TenantId == null);
```

---

## Phase 1 Effort Breakdown

| Item | Hours | Notes |
|------|-------|-------|
| Database Scripts | 8-10 | 8 scripts, including menu schema |
| Domain Entities | 5-7 | 42 entities total |
| DbContexts | 6-8 | 3-4 contexts (lighter) |
| Services | 6-8 | +MenuService with HierarchyId queries |
| API + Controllers | 6-8 | 25+ endpoints, menu API |
| Configuration | 2-3 | DI, connection strings |
| **TOTAL** | **34-45** | Production-ready API |

---

## Implementation Checklist

- [ ] Run database scripts (001-008)
- [ ] Create Menu and MenuItem entities
- [ ] Add DbSet to ReferenceDbContext
- [ ] Configure HierarchyId in OnModelCreating
- [ ] Implement MenuService with 10+ methods
- [ ] Create MenusController with 10+ endpoints
- [ ] Create SitemapController with /sitemap.xml
- [ ] Test menu creation and hierarchy
- [ ] Test role-based filtering
- [ ] Test sitemap generation
- [ ] Integration test full flow
- [ ] Add menu seed data

---

## Decision Reference

| Decision | Approved | Notes |
|----------|----------|-------|
| **Core → Master** | ✅ | Consolidate config tables |
| **Menu System** | ✅ | Dynamic navigation + sitemap |
| **HierarchyId Trees** | ✅ | For MenuItems (unlimited depth) |
| **Role-Based Visibility** | ✅ | RequiredRole + RequiredPermission |
| **Multi-Tenant Menus** | ✅ | Global + tenant-specific overrides |
| **3-4 DbContexts** | ✅ | Instead of 6 (lighter) |
| **8 Database Scripts** | ✅ | Instead of 9 (cleaner) |
| **42 Total Tables** | ✅ | 40 → 42 (added Menus, MenuItems) |

---

## Links

- 📄 **Full Schema:** [SCHEMA-REVIEW-v2.md](docs/srs/SCHEMA-REVIEW-v2.md)
- 📋 **Implementation:** [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md)
- 🗺️ **Overview:** [SCHEMA-SUMMARY-LEAN.md](SCHEMA-SUMMARY-LEAN.md)
- 📚 **Plan:** [IMPLEMENTATION-PLAN.md](docs/srs/IMPLEMENTATION-PLAN.md)
- ✅ **Checklist:** [REVIEW-CHECKLIST.md](REVIEW-CHECKLIST.md)
- 🏠 **Home:** [README.md](README.md)

---

**Status: ✅ Ready for Phase 1**
**Last Updated:** 2026-03-31
**Commits:** b9e7ae1, 00c60be, 9d4bdc5

