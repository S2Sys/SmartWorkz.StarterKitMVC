# SmartWorkz v4 - LEAN Schema Summary (42 Tables)

**Date:** 2026-03-31
**Status:** Optimized & Ready for Phase 1 Implementation
**Total Tables:** 42 (down from original 62) - Option C Hybrid geo + Production-ready Reports + Dynamic Navigation
**Total Effort:** 34-45 hours (Phase 1 - includes comprehensive reporting + navigation framework)
**Schemas:** 5 (Master, Shared, Transaction, Report, Auth) — Core schema merged into Master

---

## What Changed - Simplification Strategy

### ✅ Consolidation Moves
- **Tags** → Moved from Core to Master (global reusable tagging)
- **Tenants** → Moved from Core to Master (reference data, hierarchical)
- **TenantSubscriptions, TenantSettings, FeatureFlags** → Moved from Core to Master (configuration reference)
- **Menus, MenuItems** → New tables in Master (dynamic navigation, sitemap generation)
- **Geo** → Option C Hybrid: Countries (reference) + GeoHierarchy (HierarchyId tree) replaces 3 separate tables

### ✅ Transaction Schema - Minimized to 1 Table
| Old (8 tables) | New (1 table) | Rationale |
|---|---|---|
| Orders, OrderLines, Invoices, Payments, POs, PO Lines, Receipts, CreditNotes | **Orders** (dummy) | Demonstrate transactional pattern; teams add others in Phase 1+ |

### ✅ Report Schema - Minimized to 1 Table
| Old (5 tables) | New (1 table) | Rationale |
|---|---|---|
| ReportDefinitions, ReportSchedules, ReportResults, ReportAuditLogs, DashboardWidgets | **ReportDefinitions** (dummy) | Demonstrate reporting pattern; teams add others in Phase 1+ |

### ✅ Core Schema - Simplified
| Removed (9 entities) | Reason |
|---|---|
| Products, Customers, Vendors, Projects, Teams, Departments, Employees, Assets, Contracts | Business entities are project-specific; teams add their own in Phase 1+ |
| SubscriptionPlans, PreferenceDefinitions | Now in Master |

### ✅ Shared Infrastructure - KEPT INTACT
| Polymorphic Tables | Use |
|---|---|
| Addresses | Link to any entity (Orders, Customers, Vendors, Employees, etc.) |
| Attachments | Link to any entity (Orders, Invoices, Projects, etc.) |
| Comments | Link to any entity for discussion/notes |
| Tags | Link to any entity for categorization/filtering |
| StateHistory | Track state changes for any workflow entity |

---

## Final Schema Structure

```
MASTER (19 tables) - Global + Tenant Reference Data
├─ Geo: Countries, GeoHierarchy (Option C Hybrid - 2 tables instead of 3)
├─ i18n: Languages, Translations
├─ Hierarchies: Lookups, Categories, EntityStates, EntityStateTransitions
├─ Notifications: NotificationChannels, TemplateGroups, Templates
├─ Global Reference: Tags
├─ Tenants: Tenants (HierarchyId tree)
├─ SEO: SeoMeta, UrlRedirects
├─ Config: TenantSubscriptions, TenantSettings, FeatureFlags ← MOVED FROM CORE
└─ Navigation: Menus, MenuItems (hierarchical, role-based, auto-sitemap) ← NEW

SHARED (5 tables) - Polymorphic Infrastructure (reusable across ALL schemas)
├─ Addresses (links to any entity: Customer, Order, Employee, etc.)
├─ Attachments (file references for any entity)
├─ Comments (discussion threads for any entity)
├─ StateHistory (workflow tracking for any entity)
└─ PreferenceDefinitions (configuration for system/tenant/user)

TRANSACTION (1 table - LEAN)
└─ Orders (dummy - represents transactional pattern)

REPORT (4 tables - PRODUCTION-READY)
├─ ReportDefinitions (SQL + Dashboard + Scheduled reports metadata)
├─ ReportSchedules (Background job scheduling with cron)
├─ ReportExecutions (Audit trail, caching, result storage)
└─ ReportMetadata (Filters, drill-downs, conditional formatting, extensible config)

AUTH (13 tables - COMPLETE)
├─ Identity: Users, UserProfiles, UserPreferences
├─ RBAC: Roles, Permissions, RolePermissions, UserRoles
├─ Sessions: RefreshTokens, VerificationCodes, ExternalLogins
└─ Logging: AuditLogs, ActivityLogs, NotificationLogs

TOTAL: 42 TABLES (5 schemas: cleaner separation + production-ready reporting + dynamic navigation)
```

---

## How Polymorphic Linking Enables Extensibility

Even with LEAN schema, you can build any business domain:

### Example: E-Commerce
```
Add to Core (or new schema):
├─ Products (use with existing Tags for categorization)
├─ Customers (use with existing Addresses, Comments)
├─ Reviews (new table, use with existing StateHistory for approval workflow)

Automatically available:
├─ Orders (already in Transaction) ← Link to Customers, Products
├─ Attachments ← Link to Orders (invoices, shipping labels)
├─ Comments ← Link to Orders (customer notes, support threads)
├─ Tags ← Link to Orders (VIP orders, Rush orders)
└─ StateHistory ← Track order workflow (Draft → Shipped → Delivered)

NO SCHEMA CHANGES needed as you extend!
```

### Example: HR System
```
Add to Core (or new schema):
├─ Employees
├─ Departments
├─ EmployeeRequests (leave, expense)

Automatically available:
├─ Addresses ← Link to Employees (home, office)
├─ Attachments ← Link to Employees (ID proofs, certifications)
├─ Comments ← Link to Employees (performance notes)
├─ Tags ← Link to Employees (Department, Role, Status)
├─ StateHistory ← Track request workflow (Submitted → Approved → Completed)
└─ Orders (in Transaction) ← Link to PO for supplies
```

---

## Phase 1 Effort Breakdown (LEAN)

| Component | Effort | Deliverables |
|-----------|--------|--------------|
| Database Scripts | 8-10h | 8 scripts (001-008) with Report + Navigation schemas |
| Domain Entities | 5-7h | 42 entities (Master 19, Shared 5, Trans 1, Report 4, Auth 13) |
| EF Core DbContexts | 6-8h | 3-4 DbContexts (Reference, Transaction, Report, Auth) + repositories |
| Services | 5-7h | 5 main services + MenuService + ~50 DTOs (incl. Report/Menu services) |
| REST API | 6-8h | 25+ endpoints (auth, users, tenants, lookups, orders, **reports, menus, sitemap**) |
| Configuration | 2-3h | Connection string, DI wiring, Startup |
| **TOTAL** | **34-45h** | **Production-ready API with Reports, Navigation & Dynamic Sitemap** |

**Added production-ready reporting** with 4-table schema supporting SQL reports, dashboards, scheduling, execution history, and caching.

**Added dynamic navigation** with Menus + MenuItems (HierarchyId trees, role-based visibility, auto-sitemap generation).

---

## Report Schema Capabilities (Phase 1 ready)

**Supports all reporting patterns:**
- SQL-based reports (custom queries, stored procedures)
- Dashboard/KPI APIs (real-time metrics)
- Master-detail reports (with drill-down)
- Scheduled reports (daily/weekly/monthly with email)
- Result caching (configurable TTL)
- Export formats (PDF, Excel, CSV, JSON)
- Audit trail (execution history, performance metrics)
- Extensible metadata (filters, conditional formatting, visualizations)

**Phase 1+ Extensions:**
- ReportDistribution (email, Slack, Teams)
- ReportPermissions (row-level security)
- ReportTemplates (pre-built dashboards)
- DashboardWidgets (custom components)

---

## Teams Can Extend in Phase 1+

Once v4.0 is live, teams add their own tables per domain:

### E-Commerce Extension (Phase 1+)
```sql
-- Add to Core or new schema
Products (ProductCode, Name, Price, etc.)
OrderLines (link to Orders)
Invoices (link to Orders)
Payments (link to Invoices)
Reviews (link to Products)
Wishlists (link to Customers, Products)
Coupons (link to Orders)
```

### Financial Extension (Phase 1+)
```sql
-- Add to new Finance schema
Accounts (GL accounts)
JournalEntries (GL postings)
Adjustments (debit/credit notes)
CostCenters (department allocation)
```

### HR Extension (Phase 1+)
```sql
-- Add to new HR schema
Employees, Departments, Teams
Leaves (LeaveRequests, LeaveBalances)
Payroll (Salaries, Deductions)
Performance (Reviews, Goals)
```

All use the same:
- Addresses, Attachments, Comments, Tags, StateHistory (polymorphic)
- Users, Roles, Permissions from Auth
- TenantId for isolation
- Soft delete + audit columns

---

## Key Design Principles - PRESERVED ✅

✅ **Polymorphic Linking** — EntityType+EntityId allows any entity to use Addresses, Tags, Comments, Attachments, StateHistory
✅ **HierarchyId Trees** — Unlimited nesting in Tenants, Lookups, Categories, EntityStates
✅ **Soft Delete** — IsDeleted, DeletedAt, DeletedBy on all business entities
✅ **Audit Columns** — CreatedAt, UpdatedAt, CreatedBy, UpdatedBy on all entities
✅ **Multi-Tenancy** — TenantId partitioning for row-level isolation
✅ **State Machines** — EntityStates + StateHistory framework for any workflow
✅ **Template System** — Email, SMS, WhatsApp, Push, InApp channels + multi-language
✅ **Flexible Config** — TenantSettings key-value + FeatureFlags for customization

---

## SQL Scripts to Create (8 scripts)

```sql
001_CreateSchemas.sql
├─ CREATE SCHEMA Master;
├─ CREATE SCHEMA Shared;
├─ CREATE SCHEMA [Transaction];
├─ CREATE SCHEMA Report;
└─ CREATE SCHEMA Auth;

002_CreateTables_Master.sql (20 tables)
├─ Geo: Countries, GeoHierarchy
├─ i18n: Languages, Translations
├─ Hierarchies: Lookups, Categories, EntityStates, EntityStateTransitions
├─ Notifications: NotificationChannels, TemplateGroups, Templates
├─ Reference: Tags, Tenants, SeoMeta, UrlRedirects
├─ Config: TenantSubscriptions, TenantSettings, FeatureFlags ← FROM CORE
└─ Navigation: Menus, MenuItems ← NEW

003_CreateTables_Shared.sql (5 tables)
├─ Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions

004_CreateTables_Transaction.sql (1 table)
└─ Orders

005_CreateTables_Report.sql (4 tables)
├─ ReportDefinitions, ReportSchedules, ReportExecutions, ReportMetadata

006_CreateTables_Auth.sql (13 tables)
├─ Users, UserProfiles, UserPreferences
├─ Roles, Permissions, RolePermissions, UserRoles
├─ RefreshTokens, VerificationCodes, ExternalLogins
├─ AuditLogs, ActivityLogs, NotificationLogs

007_SeedData.sql
├─ Seed: Countries, Languages, Lookups, Categories, EntityStates
├─ Seed: Roles, Permissions, Templates, Menus

008_CreateIndexes.sql
└─ Indexes on: TenantId, EntityType+EntityId, HierarchyId paths, FKs, CreatedAt
```

---

## Domain Entities (42 total)

| Schema | Entities | Count |
|--------|----------|-------|
| **Master** | Country, GeoHierarchy, Language, Translation, Lookup, Category, EntityState, EntityStateTransition, NotificationChannel, TemplateGroup, Template, Tag, Tenant, SeoMeta, UrlRedirect, TenantSubscription, TenantSetting, FeatureFlag, Menu, MenuItem | 20 |
| **Shared** | Address, Attachment, Comment, StateHistory, PreferenceDefinition | 5 |
| **Transaction** | Order | 1 |
| **Report** | ReportDefinition, ReportSchedule, ReportExecution, ReportMetadata | 4 |
| **Auth** | User, UserProfile, UserPreference, Role, Permission, RolePermission, UserRole, RefreshToken, VerificationCode, ExternalLogin, AuditLog, ActivityLog, NotificationLog | 13 |
| **TOTAL** | | **42** |

Note: 42 entities = 42 tables total (Master 20, Shared 5, Trans 1, Report 4, Auth 13)

---

## Single Database Approach

**Database:** StarterKitMVC (SQL Server 2019+)

**Connection String:**
```
Server=localhost;Database=StarterKitMVC;Trusted_Connection=True;TrustServerCertificate=True
```

**DbContexts (3-4) - Optimized:**
```csharp
public class ReferenceDbContext : DbContext { }      // Master (20) + Shared (5) = 25 tables
public class TransactionDbContext : DbContext { }    // Transaction (1) table
public class ReportDbContext : DbContext { }         // Report (4) tables
public class AuthDbContext : DbContext { }           // Auth (13) tables
```

OR simpler (3 contexts):
```csharp
public class ReferenceDbContext : DbContext { }      // Master (20) + Shared (5) = 25 tables
public class TransactionDbContext : DbContext { }    // Transaction (1) + Report (4) = 5 tables
public class AuthDbContext : DbContext { }           // Auth (13) tables
```

---

## Ready for Implementation ✅

**All decisions made:**
✓ Single database with 5 schemas (Master, Shared, Transaction, Report, Auth)
✓ 42 tables with Option C Hybrid geo approach (Countries + GeoHierarchy instead of 3 separate)
✓ Master schema now includes: Config (TenantSubscriptions, TenantSettings, FeatureFlags) + Navigation (Menus, MenuItems)
✓ Production-ready Report schema (SQL reports, Dashboards, Scheduling, Audit trail)
✓ Dynamic Navigation system (role-based menus, auto-sitemap, hierarchical MenuItems with HierarchyId)
✓ Polymorphic linking (Shared infrastructure) for future extensibility
✓ 3-4 DbContexts instead of 6 (cleaner, lighter)
✓ 34-45 hours Phase 1 effort (much faster!)

**Next:** Start Phase 1 → Create database scripts

---

**Documents:**
- Full spec: `docs/srs/SCHEMA-REVIEW-v2.md` (LEAN schema detailed)
- Implementation plan: `docs/srs/IMPLEMENTATION-PLAN.md` (updated for 38 tables)
- This summary: `SCHEMA-SUMMARY-LEAN.md`
