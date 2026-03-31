# SmartWorkz v4 - System Overview (Visual Guide)

**Complete View of How Menus, SEO, Tags, and Navigation Work Together**

---

## 1. Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                            │
│  (MVC Views, Razor Pages, Blazor WASM, .NET MAUI, REST APIs)   │
└─────────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────────┐
│                  APPLICATION LAYER                               │
│  Services: MenuService, SeoMetaService, TagService,             │
│  ReportService, UserService, TenantService, etc.               │
└─────────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────────┐
│              DOMAIN LAYER (Entity Classes)                        │
│  Menu, MenuItem, SeoMeta, Tag, Order, User, Role,              │
│  Tenant, Address, Attachment, Comment, etc.                     │
└─────────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────────┐
│            INFRASTRUCTURE LAYER (EF Core DbContexts)            │
│  ReferenceDbContext (Master + Shared)                           │
│  TransactionDbContext (Transaction)                             │
│  ReportDbContext (Report)                                       │
│  AuthDbContext (Auth)                                           │
└─────────────────────────────────────────────────────────────────┘
                              ↓ ↑
┌─────────────────────────────────────────────────────────────────┐
│                   DATABASE LAYER                                 │
│  Single SQL Server database: StarterKitMVC                       │
│  5 Schemas: Master, Shared, Transaction, Report, Auth           │
│  41 Tables total                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Database Schema at a Glance

```
┌─────────────────────────────────────────────────────────────────┐
│                    MASTER SCHEMA (17 tables)                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Geo Reference (2)          Localization (2)                    │
│  ├─ Countries              ├─ Languages                         │
│  └─ GeoHierarchy            └─ Translations                      │
│                                                                   │
│  Hierarchies (4)            Notifications (3)                   │
│  ├─ Lookups                ├─ NotificationChannels              │
│  ├─ Categories             ├─ TemplateGroups                    │
│  ├─ EntityStates           └─ Templates                         │
│  └─ EntityStateTransitions                                      │
│                                                                   │
│  Tenants (1)                Config (3)                          │
│  └─ Tenants                ├─ TenantSubscriptions               │
│     (HierarchyId tree)      ├─ TenantSettings                   │
│                             └─ FeatureFlags                     │
│                                                                   │
│  SEO & Navigation (2)                                           │
│  ├─ UrlRedirects (301/302 redirects)                           │
│  └─ Menus (Main, Admin, Footer)                                │
│     └─ MenuItems (HierarchyId tree, role-based)                │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────┐
│                   SHARED SCHEMA (7 tables)                       │
│                                                                   │
│  ALL support polymorphic linking: EntityType + EntityId         │
│                                                                   │
│  Reusable Infrastructure:                                        │
│  ├─ Addresses        (Customer, Order, Product, Employee)       │
│  ├─ Attachments      (Order, Invoice, Product, Employee)        │
│  ├─ Comments         (Order, Customer, BlogPost, Product)       │
│  ├─ StateHistory     (Order, Invoice, Leave Request)            │
│  ├─ PreferenceDefinitions  (Theme, Language, TimeZone)          │
│  ├─ SeoMeta          (Product, Category, MenuItem, BlogPost)   │
│  └─ Tags             (Product, Order, Customer, BlogPost)       │
│                                                                   │
│  Why Shared? Can be used by any entity type automatically.      │
│  No schema changes needed as you add new entities!              │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────┐
│              TRANSACTION SCHEMA (1 table - extensible)           │
├─────────────────────────────────────────────────────────────────┤
│  ├─ Orders (dummy, teams add: Invoices, Payments, POs)         │
│     Uses Shared: Addresses, Attachments, Comments,             │
│                StateHistory, Tags, SeoMeta                      │
└─────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────┐
│          REPORT SCHEMA (4 tables - production-ready)             │
├─────────────────────────────────────────────────────────────────┤
│  ├─ ReportDefinitions      (SQL, Dashboard, Stored Proc)       │
│  ├─ ReportSchedules        (Cron scheduling)                   │
│  ├─ ReportExecutions       (Audit trail, caching)              │
│  └─ ReportMetadata         (Filters, formatting, JSON config)  │
└─────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────┐
│              AUTH SCHEMA (13 tables - complete)                  │
├─────────────────────────────────────────────────────────────────┤
│  Identity (3): Users, UserProfiles, UserPreferences             │
│  RBAC (4): Roles, Permissions, RolePermissions, UserRoles      │
│  Sessions (3): RefreshTokens, VerificationCodes, ExternalLogins│
│  Logging (3): AuditLogs, ActivityLogs, NotificationLogs         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Request Flow: How Navigation Works

```
USER REQUEST: GET /products/electronics
             ↓
    ┌────────────────────────┐
    │ ROUTING MIDDLEWARE     │
    │ Extract "products"     │
    └────────────────────────┘
             ↓
    ┌────────────────────────┐
    │ SeoMetaService         │
    │ Lookup by slug         │
    │ "products/electronics" │
    └────────────────────────┘
             ↓
    ┌────────────────────────┐
    │ Shared.SeoMeta         │
    │ EntityType: "Category" │
    │ EntityId: guid-123     │
    │ → Found!               │
    └────────────────────────┘
             ↓
    ┌────────────────────────┐
    │ RouteResolutionService│
    │ Route to Controller    │
    │ CategoriesController   │
    │ Action: List           │
    │ Id: guid-123           │
    └────────────────────────┘
             ↓
    ┌────────────────────────┐
    │ CategoryService        │
    │ Get category + details │
    │ Load: Addresses, Tags, │
    │       Comments, SeoMeta│
    └────────────────────────┘
             ↓
    ┌────────────────────────┐
    │ MenuService            │
    │ Get breadcrumbs:       │
    │ Home > Products >      │
    │ Electronics            │
    └────────────────────────┘
             ↓
    ┌────────────────────────┐
    │ RENDER VIEW            │
    │ Display:               │
    │ - Navigation menu      │
    │ - Breadcrumbs          │
    │ - SEO meta tags        │
    │ - Product with tags    │
    │ - Comments, ratings    │
    └────────────────────────┘
```

---

## 4. Data Relationships: How Tables Connect

```
PUBLIC-FACING PRODUCT PAGE
═════════════════════════════════════════════════════════════════

Shared.SeoMeta (EntityType='Product', EntityId=prod-guid)
  ├─ MetaTitle: "Dell XPS 13 - Premium Laptop | Your Store"
  ├─ MetaDescription: "High-performance 13-inch ultrabook..."
  ├─ OgImage: URL for social media
  ├─ SchemaMarkup: Product structured data (JSON)
  └─ Slug: "dell-xps-13" (for routing)
             ↓
URL: /products/dell-xps-13 (routable)
             ↓
Core.Product (ProductId=prod-guid)
  ├─ Name: "Dell XPS 13"
  ├─ Price: 1099.99
  ├─ CategoryId: electronics-guid
  └─ TenantId: tenant-guid
             ↓
             ├─→ Shared.Addresses (EntityType='Product', EntityId=prod-guid)
             │    Warehouse locations, shipping addresses
             │
             ├─→ Shared.Attachments (EntityType='Product', EntityId=prod-guid)
             │    Product images, datasheets, manuals
             │
             ├─→ Shared.Comments (EntityType='Product', EntityId=prod-guid)
             │    Customer reviews and ratings
             │
             ├─→ Shared.Tags (EntityType='Product', EntityId=prod-guid)
             │    'Featured', 'On Sale', 'New Arrival', etc.
             │
             └─→ Master.Categories (CategoryId=electronics-guid)
                  ├─ Name: "Electronics"
                  ├─ NodePath: /1/ (HierarchyId)
                  └─ Shared.SeoMeta (EntityType='Category')
                       MetaTitle: "Electronics | Your Store"
                       SchemaMarkup: CollectionPage


NAVIGATION BREADCRUMB
═════════════════════════════════════════════════════════════════

Master.Menus (Code='Main')
  └─ Master.MenuItems
       ├─ /1/              Home → Shared.SeoMeta (slug: home)
       ├─ /2/              Products → Shared.SeoMeta (slug: products)
       │  ├─ /2/1/         Electronics → Shared.SeoMeta (slug: products/electronics)
       │  ├─ /2/2/         Clothing
       │  └─ /2/3/         Books
       └─ /3/              Orders

Query: MenuService.GetBreadcrumb(menuItem /2/1/)
Result: [Home, Products, Electronics]
Render: <nav> <a>Home</a> > <a>Products</a> > Electronics </nav>


ADMIN DASHBOARD
═════════════════════════════════════════════════════════════════

Master.Menus (Code='Admin', RequiredRole='Admin')
  └─ Master.MenuItems (filtered by user roles)
       ├─ /1/         Dashboard (RequiredRole: null → visible to Admin)
       ├─ /2/         Users (RequiredRole: "Admin" → visible)
       ├─ /3/         Reports (RequiredRole: "Manager" → not visible if just Admin)
       ├─ /4/         Settings (RequiredRole: "SuperAdmin" → not visible)
       └─ /5/         Notifications (badge: "5 pending")

Only items matching user.Roles are rendered to the page.
```

---

## 5. SEO & Polymorphism: One Table, Many Uses

```
Shared.SeoMeta (Single polymorphic table serves all entity types)
═════════════════════════════════════════════════════════════════

┌──────────────────────────────────────────────────────────────────┐
│ EntityType='Product'                                              │
├──────────────────────────────────────────────────────────────────┤
│ Dell XPS 13      MetaTitle: "Dell XPS 13 - Laptop"              │
│ MacBook Pro      MetaTitle: "Apple MacBook Pro"                 │
│ HP Pavilion      MetaTitle: "HP Pavilion 15 Laptop"             │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ EntityType='Category'                                             │
├──────────────────────────────────────────────────────────────────┤
│ Electronics      MetaTitle: "Electronics | Your Store"          │
│ Computers        MetaTitle: "Laptops & Desktops"                │
│ Clothing         MetaTitle: "Fashion & Clothing"                │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ EntityType='MenuItem'                                             │
├──────────────────────────────────────────────────────────────────┤
│ Home             MetaTitle: "Home | Your Store"                 │
│ Products         MetaTitle: "Products | Your Store"             │
│ Contact          MetaTitle: "Contact Us | Your Store"           │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ EntityType='BlogPost'  (Phase 1+)                               │
├──────────────────────────────────────────────────────────────────┤
│ Laptop Guide     MetaTitle: "How to Choose a Laptop"            │
│ Tech Trends      MetaTitle: "2026 Tech Trends Report"           │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│ EntityType='GeolocationPage'                                     │
├──────────────────────────────────────────────────────────────────┤
│ Electronics/NYC  MetaTitle: "Electronics in NYC"                │
│ Laptops/LA       MetaTitle: "Laptops in Los Angeles"            │
└──────────────────────────────────────────────────────────────────┘

KEY: No schema changes needed. Just add EntityType value and go!
```

---

## 6. Multi-Tenancy: Isolation & Override

```
GLOBAL + TENANT-SPECIFIC PATTERN
═════════════════════════════════════════════════════════════════

Scenario: Acme Corp (Tenant A) wants custom navigation
         All other tenants use default

STEP 1: CREATE GLOBAL MENU
┌────────────────────────────────┐
│ Master.Menus                   │
├────────────────────────────────┤
│ MenuId: guid-001               │
│ Code: "Main"                   │
│ Name: "Main Navigation"        │
│ TenantId: NULL ← GLOBAL        │
└────────────────────────────────┘

STEP 2: CREATE TENANT OVERRIDE
┌────────────────────────────────┐
│ Master.Menus                   │
├────────────────────────────────┤
│ MenuId: guid-002               │
│ Code: "Main" ← SAME CODE      │
│ Name: "ACME Custom Nav"        │
│ TenantId: "acme-guid" ← OVERRIDE
└────────────────────────────────┘

STEP 3: QUERY RESOLUTION
┌────────────────────────────────┐
│ GetMenuByCode("Main",           │
│   tenantId="acme-guid")        │
│                                 │
│ 1. Try tenant-specific first   │
│    → Found! Return guid-002    │
│ 2. If not found, fallback      │
│    → Return NULL fallback      │
│ 3. Never found? Use global     │
│    → Return guid-001           │
└────────────────────────────────┘

RESULT:
- Acme Corp sees custom menu (guid-002)
- Other tenants see default menu (guid-001)
- No code changes needed
- Easy to override any menu per tenant
```

---

## 7. Phase 1 Effort Breakdown

```
TOTAL: 34-45 HOURS (for complete production-ready system)

Database Scripts (8-10h)
  ├─ Schema creation
  ├─ 41 tables with relationships
  ├─ HierarchyId support
  ├─ Seed data (Countries, Languages, Lookups, Roles, Menus, etc.)
  └─ Indexes

Domain Entities (5-7h)
  ├─ 41 entity classes
  ├─ Navigation properties
  ├─ HierarchyId support
  └─ Soft delete, audit columns

EF Core DbContexts (6-8h)
  ├─ ReferenceDbContext (Master 18 + Shared 7 = 25 tables)
  ├─ TransactionDbContext (1 table)
  ├─ ReportDbContext (4 tables)
  ├─ AuthDbContext (13 tables)
  └─ Generic Repository pattern

Application Services (5-7h)
  ├─ MenuService (breadcrumbs, sitemap, role-filtering)
  ├─ SeoMetaService (polymorphic SEO)
  ├─ TagService (polymorphic tagging)
  ├─ TenantService, UserService, RoleService
  ├─ ReportService, TemplateService, etc.
  └─ ~50 DTOs

REST API (6-8h)
  ├─ 5 menu endpoints (GET, POST, PUT, DELETE, breadcrumb)
  ├─ 5 SEO endpoints (GET by slug, by entity, CRUD)
  ├─ 5+ tag endpoints (CRUD, filtering)
  ├─ Auth endpoints (login, register, refresh token)
  ├─ Tenant endpoints
  ├─ User management endpoints
  ├─ Reports endpoints
  └─ Sitemap.xml endpoint

Configuration (2-3h)
  ├─ Connection strings
  ├─ Dependency injection
  ├─ AutoMapper profiles
  ├─ Authentication middleware
  └─ CORS, logging, etc.

DELIVERABLES:
✓ Production-ready database (41 tables, optimized indexes)
✓ Complete domain layer (41 entity classes)
✓ EF Core 4 DbContexts with relationships
✓ Service layer with business logic
✓ 25+ REST API endpoints
✓ Dynamic menu system (role-based, breadcrumbs)
✓ Polymorphic SEO (Products, Categories, MenuItems, BlogPosts)
✓ Polymorphic tagging (flexible categorization)
✓ Sitemap generation
✓ Multi-tenant support
✓ Complete RBAC + Authentication
✓ Production-ready reporting framework
```

---

## 8. Technology Stack

```
FRAMEWORK & ARCHITECTURE
├─ .NET 9
├─ ASP.NET Core 9
├─ Entity Framework Core 9
├─ Clean Architecture (Domain, Application, Infrastructure, Web)
└─ Repository Pattern

DATABASE
├─ SQL Server 2019+ (single database)
├─ 5 Schemas
├─ 41 Tables
├─ HierarchyId support
├─ Full-text search ready
└─ Geo support (coordinates, hierarchy)

API & CLIENTS
├─ REST API (OpenAPI/Swagger)
├─ ASP.NET MVC Web
├─ Razor Pages
├─ Blazor WebAssembly (WASM)
└─ .NET MAUI (mobile)

AUTHENTICATION & AUTHORIZATION
├─ JWT tokens
├─ Refresh tokens (device-specific)
├─ Multi-factor authentication (OTP, email verify)
├─ OAuth (Google, Microsoft, GitHub, Facebook)
├─ Role-based access control (RBAC)
└─ Claim-based authorization

MULTI-TENANCY
├─ Row-level TenantId isolation
├─ Tenant hierarchy (Agency → Client → SubClient)
├─ Global + tenant-specific overrides
└─ Tenant-aware queries (automatic filtering)
```

---

## Summary: What Makes v4 Special

```
✅ 41 Tables (LEAN, down from 62 in v1)
✅ 5 Schemas (clear separation: Master, Shared, Transaction, Report, Auth)
✅ Polymorphic Infrastructure (1 SeoMeta table serves all entity types)
✅ Dynamic Navigation (menus in database, no code redeploy)
✅ Automatic SEO (role-based breadcrumbs, sitemap.xml)
✅ Role-Based Access (show/hide menu items per user role)
✅ Flexible Tagging (any entity can be tagged)
✅ Multi-Tenant Ready (global + per-tenant customization)
✅ Production-Ready Reporting (SQL, dashboards, scheduling, audit)
✅ Complete Authentication (JWT, MFA, OAuth, sessions)
✅ Extensible Design (add any entity type without schema changes)
✅ 34-45 Hours Phase 1 (fast to production)

NEXT: Begin Phase 1 implementation (database → entities → services → API)
```
