# SmartWorkz v4 - LEAN Schema Summary (38 Tables)

**Date:** 2026-03-31
**Status:** Optimized & Ready for Phase 1 Implementation
**Total Tables:** 40 (down from original 62) - Option C Hybrid geo + Production-ready Reports
**Total Effort:** 72-99 hours (includes comprehensive reporting framework)

---

## What Changed - Simplification Strategy

### ✅ Consolidation Moves
- **Tags** → Moved from Core to Master (global reusable tagging)
- **Tenants** → Moved from Core to Master (reference data, hierarchical)
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
MASTER (14 tables) - Reference Data
├─ Geo: Countries, GeoHierarchy (Option C Hybrid - 2 tables instead of 3)
├─ i18n: Languages, Translations
├─ Hierarchies: Lookups, Categories, EntityStates, EntityStateTransitions
├─ Notifications: NotificationChannels, TemplateGroups, Templates
├─ Global Reference: Tags ← MOVED
├─ Tenants: Tenants ← MOVED (HierarchyId tree)
└─ SEO: SeoMeta, UrlRedirects

SHARED (5 tables) - Polymorphic Infrastructure (reusable across ALL schemas)
├─ Addresses (links to any entity: Customer, Order, Employee, etc.)
├─ Attachments (file references for any entity)
├─ Comments (discussion threads for any entity)
├─ StateHistory (workflow tracking for any entity)
└─ PreferenceDefinitions (configuration for system/tenant/user)

CORE (3 tables) - Tenant Configuration
├─ TenantSubscriptions (subscription plans)
├─ TenantSettings (key-value config)
└─ FeatureFlags (feature toggles)

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

TOTAL: 40 TABLES (6 schemas: cleaner separation + production-ready reporting)
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
| Database Scripts | 8-10h | 9 scripts (001-009) with Report schema |
| Domain Entities | 5-7h | 40 entities (Master 14, Shared 5, Core 3, Trans 1, Report 4, Auth 13) |
| EF Core DbContexts | 7-9h | 6 DbContexts (Master, Shared, Core, Trans, Report, Auth) + repositories |
| Services | 5-7h | 6 main services + ~50 DTOs (incl. Report/Dashboard services) |
| REST API | 8-10h | 20+ endpoints (auth, users, tenants, lookups, orders, **reports, dashboards**) |
| Configuration | 2-3h | Connection string, DI wiring, Startup |
| **TOTAL** | **35-46h** | **Production-ready API with Reports & Dashboards** |

**Added production-ready reporting** with 4-table schema supporting SQL reports, dashboards, scheduling, execution history, and caching.

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
├─ CREATE SCHEMA Core;
├─ CREATE SCHEMA [Transaction];
├─ CREATE SCHEMA Report;
└─ CREATE SCHEMA Auth;

002_CreateTables_Master.sql (15 tables)
├─ Countries, States, Cities
├─ Languages, Translations
├─ Lookups, Categories, EntityStates, EntityStateTransitions
├─ NotificationChannels, TemplateGroups, Templates
├─ Tags, Tenants, SeoMeta, UrlRedirects

003_CreateTables_Core.sql (8 tables)
├─ TenantSubscriptions, TenantSettings, FeatureFlags
├─ Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions

004_CreateTables_Transaction.sql (1 table)
└─ Orders

005_CreateTables_Report.sql (1 table)
└─ ReportDefinitions

006_CreateTables_Auth.sql (13 tables)
├─ Users, UserProfiles, UserPreferences
├─ Roles, Permissions, RolePermissions, UserRoles
├─ RefreshTokens, VerificationCodes, ExternalLogins
├─ AuditLogs, ActivityLogs, NotificationLogs

007_SeedData.sql
├─ Seed: Countries, States, Cities, Languages, Lookups, Categories, EntityStates
├─ Seed: Roles, Permissions, Templates

008_CreateIndexes.sql
└─ Indexes on: TenantId, EntityType+EntityId, HierarchyId paths, FKs, CreatedAt
```

---

## Domain Entities (38 total)

| Schema | Entities | Count |
|--------|----------|-------|
| **Master** | Country, GeoHierarchy, Language, Translation, Lookup, Category, EntityState, EntityStateTransition, NotificationChannel, TemplateGroup, Template, Tag, Tenant, SeoMeta, UrlRedirect | 15 |
| **Core** | TenantSubscription, TenantSetting, FeatureFlag, Address, Attachment, Comment, StateHistory, PreferenceDefinition | 8 |
| **Transaction** | Order | 1 |
| **Report** | ReportDefinition | 1 |
| **Auth** | User, UserProfile, UserPreference, Role, Permission, RolePermission, UserRole, RefreshToken, VerificationCode, ExternalLogin, AuditLog, ActivityLog, NotificationLog | 13 |
| **TOTAL** | | **38** |

Note: 38 entities = 37 tables (Option C Hybrid geo consolidates 3→2 tables)

---

## Single Database Approach

**Database:** StarterKitMVC (SQL Server 2019+)

**Connection String:**
```
Server=localhost;Database=StarterKitMVC;Trusted_Connection=True;TrustServerCertificate=True
```

**DbContexts (5):**
```csharp
public class MasterDbContext : DbContext { }    // 15 tables
public class CoreDbContext : DbContext { }      // 8 tables
public class TransactionDbContext : DbContext { } // 1 table
public class ReportDbContext : DbContext { }    // 1 table
public class AuthDbContext : DbContext { }      // 13 tables
```

---

## Ready for Implementation ✅

**All decisions made:**
✓ Single database with 6 schemas (Master, Shared, Core, Transaction, Report, Auth)
✓ 40 tables with Option C Hybrid geo approach (Countries + GeoHierarchy instead of 3 separate)
✓ Production-ready Report schema (SQL reports, Dashboards, Scheduling, Audit trail)
✓ Polymorphic linking (Shared infrastructure) for future extensibility
✓ Tags and Tenants moved to Master
✓ 72-99 hours effort (3-4 weeks with comprehensive reporting)

**Next:** Start Phase 1 → Create database scripts

---

**Documents:**
- Full spec: `docs/srs/SCHEMA-REVIEW-v2.md` (LEAN schema detailed)
- Implementation plan: `docs/srs/IMPLEMENTATION-PLAN.md` (updated for 38 tables)
- This summary: `SCHEMA-SUMMARY-LEAN.md`
