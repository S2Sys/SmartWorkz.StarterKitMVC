# SmartWorkz v4 - LEAN Schema Summary (38 Tables)

**Date:** 2026-03-31
**Status:** Optimized & Ready for Phase 1 Implementation
**Total Tables:** 37 (down from original 62) - Option C Hybrid geo approach
**Total Effort:** 68-95 hours (down from 90-120)

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

REPORT (1 table - LEAN)
└─ ReportDefinitions (dummy - represents reporting pattern)

AUTH (13 tables - COMPLETE)
├─ Identity: Users, UserProfiles, UserPreferences
├─ RBAC: Roles, Permissions, RolePermissions, UserRoles
├─ Sessions: RefreshTokens, VerificationCodes, ExternalLogins
└─ Logging: AuditLogs, ActivityLogs, NotificationLogs

TOTAL: 37 TABLES (6 schemas: cleaner separation of concerns)
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
| Database Scripts | 8-10h | 8 scripts (001-008) |
| Domain Entities | 5-7h | 37 entities (Master 14, Core 8, Trans 1, Report 1, Auth 13) |
| EF Core DbContexts | 6-8h | 5 DbContexts + repositories |
| Services | 4-6h | 5 main services + ~40 DTOs |
| REST API | 6-8h | 15+ endpoints covering auth, users, tenants, lookups, orders |
| Configuration | 2-3h | Connection string, DI wiring, Startup |
| **TOTAL** | **30-40h** | **Production-ready API** |

**Saved 11 hours** by removing non-essential tables, business entities, and using Option C Hybrid geo approach (2 tables instead of 3).

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
✓ Single database with 5 schemas
✓ 37 lean tables with Option C Hybrid geo approach (Countries + GeoHierarchy instead of 3 separate)
✓ Polymorphic linking for future extensibility
✓ One dummy table per Transaction/Report schema
✓ Tags and Tenants moved to Master
✓ 68-95 hours effort (3 weeks)

**Next:** Start Phase 1 → Create database scripts

---

**Documents:**
- Full spec: `docs/srs/SCHEMA-REVIEW-v2.md` (LEAN schema detailed)
- Implementation plan: `docs/srs/IMPLEMENTATION-PLAN.md` (updated for 38 tables)
- This summary: `SCHEMA-SUMMARY-LEAN.md`
