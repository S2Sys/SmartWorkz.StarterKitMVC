# SmartWorkz v4 - Final Architecture Summary

**Date:** 2026-03-31 (Updated: Core schema added for business-domain configuration)
**Status:** Phase 0 Complete (Design finalized, all decisions made)
**Total Effort:** 34-45 hours Phase 1 (database, entities, services, REST API)
**Total Tables:** 43 (Master 15, Core 3, Shared 7, Transaction 1, Report 4, Auth 13)

---

## Schema Overview (43 Tables, 6 Schemas)

### Master Schema (15 tables) - Global Reference Data

```
Master
├─ Geo (2 tables) ─── Option C Hybrid
│  ├─ Countries (fast lookups: CountryCode, CurrencyCode, TimeZone)
│  └─ GeoHierarchy (HierarchyId tree: State → City → District)
│
├─ Localization (2 tables) ─── Complete i18n
│  ├─ Languages (LanguageCode, LanguageName, IsRtl)
│  └─ Translations (polymorphic: Namespace, EntityType, EntityId)
│
├─ Hierarchies (4 tables) ─── HierarchyId Trees
│  ├─ Lookups (NodePath, LookupCode, DisplayText)
│  ├─ Categories (NodePath, CategoryType, Slug) ← for Products, Blog, etc.
│  ├─ EntityStates (NodePath, EntityType, StateCode) ← workflow definitions
│  └─ EntityStateTransitions (FromState → ToState rules)
│
├─ Notifications (3 tables) ─── Multi-channel Templates
│  ├─ NotificationChannels (Email, SMS, WhatsApp, Push, InApp)
│  ├─ TemplateGroups (Welcome, Password Reset, Order Confirmation)
│  └─ Templates (localized, tenant-specific)
│
├─ Tenants (1 table)
│  └─ Tenants (HierarchyId: Agency → Client → SubClient, reference data)
│
└─ Config (2 tables)
   ├─ TenantSubscriptions (PlanCode, StartDate, EndDate, Status)
   └─ TenantSettings (Key-value, encrypted optional)
```

**Master Schema Totals:** 15 tables (removed Menus, MenuItems, FeatureFlags → moved to Core)

---

### Core Schema (3 tables) - Business-Domain Configuration (per-tenant)

```
Core
├─ Navigation (2 tables) ← Business-domain, not global reference
│  ├─ Menus (MenuId, Code, Name, TenantId) ← Main, Admin, Footer, Sidebar
│  │  └─ Each tenant manages their own menu structure
│  └─ MenuItems (NodePath HierarchyId, Code, Url, Icon, TenantId)
│     ├─ Role-based visibility (RequiredRole, RequiredPermission)
│     ├─ Badges for notifications
│     └─ Auto-generates sitemap.xml
│
└─ Features (1 table)
   └─ TenantFeatures (TenantId, FeatureCode, IsEnabled)
      └─ Which features enabled per tenant (not global)
```

**Core Schema Rationale:** Menus and FeatureFlags are operational decisions that:
- Change frequently as the business evolves
- Are customized per tenant (each tenant's own navigation)
- Are NOT static reference data like Countries or Languages

---

### Shared Schema (7 tables) - Polymorphic Infrastructure

```
Shared (Reusable across ALL schemas via EntityType + EntityId)
├─ Addresses (polymorphic → Customer, Order, Employee, Vendor, Supplier, Product)
├─ Attachments (file references for any entity)
├─ Comments (discussion threads for any entity, nested replies)
├─ StateHistory (workflow tracking for any entity, audit trail)
├─ PreferenceDefinitions (System/Tenant/User config, flexible types)
├─ SeoMeta ← MOVED FROM MASTER (polymorphic SEO for all entities)
│  ├─ EntityType: Product → Products
│  ├─ EntityType: Category → Categories (hierarchical with breadcrumbs)
│  ├─ EntityType: MenuItem → MenuItems
│  ├─ EntityType: BlogPost → BlogPosts (Phase 1+)
│  ├─ EntityType: GeolocationPage → Location-based listings
│  └─ EntityType: CustomPage → Terms, Privacy, etc.
│
└─ Tags ← MOVED FROM MASTER (polymorphic tagging for all entities)
   ├─ EntityType: Product → 'Featured', 'Sale', 'New Arrival'
   ├─ EntityType: Order → 'VIP', 'Rush', 'Urgent'
   ├─ EntityType: Customer → 'Premium', 'At-Risk', 'New'
   ├─ EntityType: BlogPost → 'Pinned', 'Trending', 'Archive'
   └─ EntityType: Any other entity → Flexible categorization
```

**Shared Schema Totals:** 7 tables (was 6 before moving Tags)

---

### Transaction Schema (1 table) - Transactional Data

```
Transaction (LEAN: 1 dummy table, teams extend in Phase 1+)
└─ Orders (demonstrate transactional pattern)
   ├─ Can add: Invoices, Payments, POs, PO Lines
   ├─ Can extend with: Receipts, CreditNotes, ShipmentTracking
   └─ Uses Shared: Addresses, Attachments, Comments, StateHistory
```

**Transaction Schema Totals:** 1 table

---

### Report Schema (4 tables) - Production-Ready Reporting

```
Report (Complete reporting framework for any report/dashboard type)
├─ ReportDefinitions (SQL, Dashboard API, Stored Procedure, Custom)
│  └─ Metadata: Columns, Filters, Sort order, Grouping
├─ ReportSchedules (Cron-based execution: daily, weekly, monthly)
│  └─ Delivery: Email, FTP, Webhook, Slack (extensible)
├─ ReportExecutions (Audit trail, performance metrics, result caching)
│  └─ Tracks: start time, end time, row count, errors, TTL
└─ ReportMetadata (Extensible JSON: drill-downs, conditional formatting, visualizations)
```

**Report Schema Totals:** 4 tables

---

### Auth Schema (13 tables) - Identity + RBAC + Logging

```
Auth (Complete authentication, authorization, session management)
├─ Identity (3)
│  ├─ Users (email, phone, IsActive, LastLoginAt)
│  ├─ UserProfiles (picture, department, job title, preferences)
│  └─ UserPreferences (theme, language, timezone)
│
├─ RBAC (4)
│  ├─ Roles (RoleCode, RoleName, TenantId nullable = system + tenant roles)
│  ├─ Permissions (PermissionCode, Module, Action)
│  ├─ RolePermissions (junction: Role ↔ Permission)
│  └─ UserRoles (junction: User ↔ Role, TenantId)
│
├─ Sessions (3)
│  ├─ RefreshTokens (long-lived, per device, rotation tracking)
│  ├─ VerificationCodes (OTP, email verify, password reset)
│  └─ ExternalLogins (OAuth: Google, Microsoft, GitHub, Facebook)
│
└─ Logging (3)
   ├─ AuditLogs (Login, Logout, ChangePassword, RoleChange)
   ├─ ActivityLogs (PageView, DataExport, ReportGeneration)
   └─ NotificationLogs (Email/SMS/Push delivery status)
```

**Auth Schema Totals:** 13 tables

---

## Total Summary

| Schema | Tables | Purpose |
|--------|--------|---------|
| **Master** | 15 | Global reference data only (Geo, i18n, Hierarchies, Notifications, Tenants, Config) |
| **Core** | 3 | Business-domain configuration per-tenant (Navigation, Features) |
| **Shared** | 7 | Polymorphic infrastructure (Addresses, Comments, Attachments, StateHistory, Prefs, **SeoMeta, Tags**) |
| **Transaction** | 1 | Extensible transactional pattern (Orders dummy) |
| **Report** | 4 | Production-ready reporting (SQL, Dashboards, Scheduling, Execution history) |
| **Auth** | 13 | Complete identity + RBAC + sessions + logging |
| **TOTAL** | **43** | Single database, 6 schemas, LEAN design, maximum flexibility |

---

## Key Design Decisions

### 1. Geo: Option C (Hybrid)
- ✅ **Countries:** Reference data (fast lookups for currency, phone, timezone)
- ✅ **GeoHierarchy:** HierarchyId tree (flexible State/City/District nesting)
- ✅ **Benefit:** Handles varying depths per country (USA States, UK Districts, etc.)
- ✅ **Flexible:** Add new geo types (Neighborhood, Region, etc.) without schema changes

### 2. HierarchyId Trees (Unlimited Nesting)
- **Lookups:** Group → Value → SubValue (dynamic lookup hierarchies)
- **Categories:** Electronics → Computers → Laptops (product categorization)
- **EntityStates:** Workflow state machines with validation rules
- **Tenants:** Agency → Client → SubClient (multi-level hierarchy)
- **MenuItems:** Main menu → Submenu → Sub-submenu (navigation hierarchy)
- **GeoHierarchy:** Country → State → City → District (geo hierarchy)

### 3. Polymorphic Infrastructure (EntityType + EntityId)
Applies to: **Addresses, Attachments, Comments, StateHistory, Translations, SeoMeta, Tags**

```
Example: Shared.Addresses for any entity type
├─ (EntityType='Customer', EntityId=guid) → Customer addresses
├─ (EntityType='Order', EntityId=guid) → Order shipping/billing addresses
├─ (EntityType='Employee', EntityId=guid) → Employee home/office addresses
└─ (EntityType='Vendor', EntityId=guid) → Vendor addresses

No schema changes needed as business domains grow!
```

### 4. SeoMeta in Shared Schema (Moved from Master)

**Why?**
- ✅ Consistent with polymorphic pattern (like Addresses, Comments)
- ✅ Single table serves Products, Categories, MenuItems, BlogPosts, GeolocationPages
- ✅ TenantId NOT NULL ensures proper multi-tenant isolation
- ✅ New entity types automatically get SEO (no schema changes)

**Supports:**
- Meta tags (title, description, keywords)
- Open Graph (OgTitle, OgDescription, OgImage, OgType)
- Schema.org structured data (Product, BreadcrumbList, LocalBusiness, BlogPosting)
- Canonical URLs and slug-based routing
- 301/302 redirects (UrlRedirects in Master)

### 5. Single Database Strategy
- **Database:** StarterKitMVC (SQL Server 2019+)
- **DbContexts:** 3-4 (ReferenceDbContext, TransactionDbContext, ReportDbContext, AuthDbContext)
- **Benefits:** Simple deployment, consistent transactions, shared reference data

### 6. Core Schema → Master Migration
**Moved to Master:**
- TenantSubscriptions, TenantSettings, FeatureFlags (configuration reference data)
- Tenants (tenant definitions, hierarchical)
- Tags (global tagging)

**Moved to Shared:**
- Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions
- **SeoMeta** (polymorphic infrastructure)

**Result:** Cleaner separation: Master = reference data, Shared = infrastructure

---

## Phase 1 Implementation (34-45 hours)

### Step 1: Database Scripts (8-10 hours)
```
001_CreateSchemas.sql
002_CreateTables_Master.sql (18 tables)
003_CreateTables_Shared.sql (6 tables)
004_CreateTables_Transaction.sql (1 table)
005_CreateTables_Report.sql (4 tables)
006_CreateTables_Auth.sql (13 tables)
007_SeedData.sql (Countries, Languages, Lookups, Roles, Menus, Templates)
008_CreateIndexes.sql (TenantId, EntityType+EntityId, HierarchyId paths, FKs)
```

### Step 2: Domain Entities (5-7 hours)
- 43 domain entity classes (Master: 15, Core: 3, Shared: 7, Transaction: 1, Report: 4, Auth: 13)
- Relationships (FK, HierarchyId, polymorphic EntityType+EntityId)
- Audit columns (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Soft delete (IsDeleted)

### Step 3: EF Core DbContexts (6-8 hours)
- ReferenceDbContext (Master 18 + Shared 6 = 24 tables)
- TransactionDbContext (1 table)
- ReportDbContext (4 tables)
- AuthDbContext (13 tables)

**Or 3-context approach:**
- ReferenceDbContext (Master 18 + Shared 6 = 24 tables)
- TransactionDbContext + ReportDbContext (5 tables)
- AuthDbContext (13 tables)

### Step 4: Application Services (5-7 hours)
- Repository pattern + generic IRepository<T>
- Business logic services:
  - TenantService, UserService, RoleService
  - MenuService (breadcrumbs, role-based visibility, auto-sitemap)
  - **SeoMetaService** (polymorphic SEO access), RouteResolutionService
  - ReportService (definition, scheduling, execution)
  - etc.

### Step 5: REST API (6-8 hours)
- 25+ endpoints:
  - Auth: /api/auth/login, /api/auth/register, /api/auth/refresh
  - Users: /api/users, /api/users/{id}, /api/users/{id}/roles
  - Tenants: /api/tenants, /api/tenants/{id}
  - Lookups: /api/lookups/{lookupCode}
  - **SEO:** /api/seometa, /api/seometa/by-slug/{slug}, /api/seometa/resolve
  - Menus: /api/menus, /api/menus/{menuId}/items
  - Reports: /api/reports, /api/reports/{reportId}/execute

---

## Testing Strategy

### Unit Tests
- Service layer logic (TenantService, UserService, SeoMetaService, etc.)
- Validation rules, business logic errors
- Repository patterns

### Integration Tests
- Database operations (CRUD, HierarchyId queries, polymorphic lookups)
- Multi-tenant isolation (TenantId filtering)
- Transaction handling, rollback scenarios
- SEO slug lookup, route resolution

### API Tests
- Endpoint contracts (request/response format)
- Authentication/Authorization (token validation, role-based access)
- Multi-tenant request isolation
- Error handling (400, 401, 403, 404, 500)

---

## Extensibility (Phase 1+)

Teams can extend v4 with domain-specific entities:

### E-Commerce (Phase 1+)
```
Core schema additions:
├─ Products (linked to Categories, Tags, SeoMeta)
├─ OrderLines (linked to Products, Orders)
├─ Invoices (linked to Orders, StateHistory, SeoMeta)
├─ Reviews (linked to Products, Comments, StateHistory)

Automatic access to:
├─ Addresses (billing, shipping)
├─ Attachments (invoices, labels)
├─ Comments (customer notes)
├─ StateHistory (workflow tracking)
└─ SeoMeta (product pages, category pages, geo-based listings)
```

### HR System (Phase 1+)
```
Core schema additions:
├─ Employees (linked to Users, Departments, Tags, SeoMeta)
├─ Departments (hierarchical, linked to Categories for org structure)
├─ EmployeeRequests (Leave, Expense, linked to StateHistory)
├─ Payroll (Salaries, Deductions, Adjustments)

Automatic access to:
├─ Addresses (home, office)
├─ Attachments (ID proofs, certifications, payslips)
├─ Comments (performance notes, feedback)
└─ StateHistory (request workflow, approval tracking)
```

### Financial System (Phase 1+)
```
Finance schema additions:
├─ Accounts (GL accounts, account charts)
├─ JournalEntries (GL postings, debit/credit)
├─ Adjustments (corrections, reversals)
├─ CostCenters (department allocation)
├─ BankReconciliation (bank transaction matching)

Automatic access to:
├─ Comments (transaction notes)
├─ Attachments (receipts, invoices, bank statements)
└─ StateHistory (posting status, approval workflow)
```

**Key Principle:** Use Shared.SeoMeta, Addresses, Comments, Attachments for ANY entity type. No schema changes needed!

---

## Summary: What Makes v4 Different

| Feature | v1-v3 | v4 |
|---------|-------|-----|
| **Tables** | 60+ (bloated) | 42 (LEAN) |
| **Geo** | 3 separate tables | Option C Hybrid (2 tables, HierarchyId) |
| **Menu System** | Embedded in code | Dynamic Menus + MenuItems (hierarchical, role-based, auto-sitemap) |
| **SEO** | Embedded in MenuItems | Shared.SeoMeta (polymorphic, supports all entities) |
| **Reporting** | None | Production-ready (SQL, Dashboards, Scheduling, Execution history) |
| **Configuration** | Scattered | Centralized (TenantSubscriptions, TenantSettings, FeatureFlags in Master) |
| **Extensibility** | Hard-coded | Polymorphic infrastructure (Addresses, Comments, Attachments, StateHistory, SeoMeta) |
| **Multi-tenancy** | Row-level (TenantId) | ✅ Enhanced with hierarchy (Tenants as HierarchyId tree) |
| **Localization** | Basic | Complete i18n with polymorphic Translations |
| **Setup Time** | 2-3 weeks | 34-45 hours Phase 1 |

---

## Files & Documentation

```
docs/srs/
├─ README.md ─── Index of all documentation
├─ SCHEMA-REVIEW-v2.md ─── Complete schema definitions (all 42 tables)
├─ SEO-POLYMORPHIC-DESIGN.md ─── Complete SEO design (Products, Categories, Geo, etc.)
├─ SEO-SHARED-IMPLEMENTATION-CHECKLIST.md ─── Phase 1 tasks + code examples
├─ SEO-QUICK-REFERENCE.md ─── Visual reference for SEO routing
├─ MENU-SYSTEM-GUIDE.md ─── Complete menu implementation guide
├─ IMPLEMENTATION-PLAN.md ─── 4-phase roadmap (34-45 hours Phase 1)
├─ GEO-HIERARCHY-ANALYSIS.md ─── Geo design options analysis (Option C rationale)
├─ QUICK-REFERENCE-v4.md ─── One-page schema overview
├─ UPDATES-SUMMARY-v4.md ─── Changelog (what changed from v3)
└─ V4-ARCHITECTURE-FINAL.md ─── This document

database/v4/
├─ 001_CreateSchemas.sql
├─ 002_CreateTables_Master.sql
├─ 003_CreateTables_Shared.sql
├─ 004_CreateTables_Transaction.sql
├─ 005_CreateTables_Report.sql
├─ 006_CreateTables_Auth.sql
├─ 007_SeedData.sql
└─ 008_CreateIndexes.sql
```

---

## Ready for Phase 1

✅ All architectural decisions made
✅ 41 tables designed and documented
✅ SQL scripts prepared
✅ Entity relationships mapped
✅ Polymorphic infrastructure patterns established (7 shared tables: Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions, SeoMeta, Tags)
✅ Multi-tenant strategy confirmed
✅ Dynamic navigation system designed
✅ SEO approach finalized (polymorphic Shared.SeoMeta)
✅ Tagging approach finalized (polymorphic Shared.Tags)
✅ Reporting framework designed
✅ Effort estimate: 34-45 hours Phase 1

🚀 **Next:** Begin Phase 1 implementation (database scripts → entities → services → REST API)

---

**Document:** V4-ARCHITECTURE-FINAL.md
**Version:** 2026-03-31
**Status:** Final Design (Ready for implementation)
