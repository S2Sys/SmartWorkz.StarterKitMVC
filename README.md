# SmartWorkz StarterKitMVC v4.1.0

**Multi-tenant Enterprise Platform with .NET 9 Clean Architecture**

Latest: **v4.1.0** (2026-04-02) — Demo pages & comprehensive wiki documentation | [Changelog](CHANGELOG.md)

## Quick Links

### 📚 Getting Started
- **🎯 Demo Pages** (New v4.1.0)
  - [Translation System Demo](/Public/Pages/Demo/Translations.cshtml) — View all translation keys and values
  - [Validation Demo](/Public/Pages/Demo/Validation.cshtml) — See validation attributes in action

- **📖 Wiki Documentation** (New v4.1.0)
  - [01 — Translation System](docs/wiki/01-translation-system.md) — DB-backed translations, T() helper, multi-locale
  - [02 — Localized Validation](docs/wiki/02-localized-validation.md) — MessageKey validation attributes
  - [03 — Base Page Pattern](docs/wiki/03-base-page-pattern.md) — BasePage, TenantId, T() method, toast helpers
  - [04 — Result Pattern](docs/wiki/04-result-pattern.md) — Result/Result<T> for explicit success/failure
  - [05 — HTMX List Pattern](docs/wiki/05-htmx-list-pattern.md) — Dynamic search/filter/pagination with HTMX

### 🏗️ Architecture & Design
- **📋 Schema Design:** [`docs/srs/SCHEMA-REVIEW-v2.md`](docs/srs/SCHEMA-REVIEW-v2.md) — 42 LEAN tables across 5 schemas
- **📊 Schema Summary:** [`SCHEMA-SUMMARY-LEAN.md`](SCHEMA-SUMMARY-LEAN.md) — Overview and extensibility patterns
- **✅ Implementation Plan:** [`docs/srs/IMPLEMENTATION-PLAN.md`](docs/srs/IMPLEMENTATION-PLAN.md) — 4-phase roadmap (34-45 hours)
- **🔍 Geo Design Analysis:** [`docs/srs/GEO-HIERARCHY-ANALYSIS.md`](docs/srs/GEO-HIERARCHY-ANALYSIS.md) — Option C (Hybrid) approach
- **📋 Review Checklist:** [`REVIEW-CHECKLIST.md`](REVIEW-CHECKLIST.md) — Pre-implementation validation
- **🗺️ Menu System Guide:** [`docs/srs/MENU-SYSTEM-GUIDE.md`](docs/srs/MENU-SYSTEM-GUIDE.md) — Complete navigation implementation
- **⚡ Quick Reference:** [`docs/srs/QUICK-REFERENCE-v4.md`](docs/srs/QUICK-REFERENCE-v4.md) — One-page schema overview
- **📝 Updates Summary:** [`docs/srs/UPDATES-SUMMARY-v4.md`](docs/srs/UPDATES-SUMMARY-v4.md) — What changed in v4

## Architecture Overview

### Schemas (5 total, 42 LEAN tables)

| Schema | Tables | Purpose |
|--------|--------|---------|
| **Master** | 19 | Global reference data (Geo, i18n, Hierarchies, Tenants, SEO, Config, Navigation) |
| **Shared** | 5 | Polymorphic infrastructure (Addresses, Attachments, Comments, StateHistory, Preferences) |
| **Transaction** | 1 | Orders (extensible pattern for transactions) |
| **Report** | 4 | Production-ready reporting (Definitions, Schedules, Executions, Metadata) |
| **Auth** | 13 | Complete identity + RBAC + logging |

### Key Design Patterns

✅ **Option C Hybrid Geo:** Countries + GeoHierarchy (flexible hierarchy with HierarchyId)
✅ **Polymorphic Infrastructure:** Addresses, Attachments, Comments, StateHistory (any entity)
✅ **Multi-Tenancy:** Row-level TenantId isolation
✅ **HierarchyId Trees:** Unlimited nesting for Tenants, Lookups, Categories, EntityStates, GeoHierarchy, MenuItems
✅ **Dynamic Navigation:** Menus + MenuItems with role-based visibility, breadcrumbs, auto-sitemap
✅ **Production-Ready Reports:** SQL + Dashboards + Scheduling + Caching + Audit Trail
✅ **Soft Delete & Audit:** IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy on all entities

## Project Structure

```
SmartWorkz.StarterKitMVC/
├── src/
│   ├── SmartWorkz.StarterKitMVC.Domain/          # Domain entities
│   ├── SmartWorkz.StarterKitMVC.Application/     # Application services
│   ├── SmartWorkz.StarterKitMVC.Infrastructure/  # EF Core DbContexts
│   ├── SmartWorkz.StarterKitMVC.Shared/          # DTOs, common utilities
│   └── SmartWorkz.StarterKitMVC.Web/             # MVC + API
├── tests/
│   ├── SmartWorkz.StarterKitMVC.Tests.Unit/
│   └── SmartWorkz.StarterKitMVC.Tests.Integration/
├── docs/
│   ├── srs/                                       # Schema & implementation specs
│   │   ├── SCHEMA-REVIEW-v2.md                   # Complete schema documentation
│   │   ├── IMPLEMENTATION-PLAN.md                # 4-phase implementation roadmap
│   │   ├── GEO-HIERARCHY-ANALYSIS.md             # Geo design options & rationale
│   │   └── README.md                             # Spec index
│   └── old/                                       # v1 documentation (archived)
├── database/v4/                                   # SQL migration scripts (Phase 1)
│   ├── 001_CreateSchemas.sql
│   ├── 002_CreateTables_Master.sql
│   ├── 003_CreateTables_Shared.sql
│   ├── 004_CreateTables_Transaction.sql
│   ├── 005_CreateTables_Report.sql
│   ├── 006_CreateTables_Auth.sql
│   ├── 007_SeedData.sql
│   └── 008_CreateIndexes.sql
├── SCHEMA-SUMMARY-LEAN.md                        # Quick reference guide
└── REVIEW-CHECKLIST.md                           # Pre-implementation validation
```

## Getting Started

### Phase 1: Foundation (34-45 hours)
1. Create database scripts (001-008)
2. Generate domain entities (42 entities)
3. Create EF Core DbContexts (3-4 contexts)
4. Create application services (including MenuService)
5. Create REST API endpoints (25+)

**Result:** v4 API operational with real database

### Phase 2: Documentation (8-10 hours)
1. Archive v1 docs
2. Update project documentation
3. Create migration guide

### Phase 3: MVC Integration (20-30 hours)
1. Update Admin views/controllers
2. Integration testing
3. Performance profiling

### Phase 4: API Polish (10-15 hours)
1. Swagger documentation
2. API versioning
3. Rate limiting, health checks
4. Security hardening

## Column Naming Convention

Simplified column names (removed redundant table prefixes):

| Example | Old | New |
|---------|-----|-----|
| TenantSubscriptions | SubscriptionPlanCode | PlanCode |
| TenantSettings | SettingKey | Key |
| ReportDefinitions | ReportCode | Code |
| Addresses | AddressType | Type |
| Comments | CommentText | Text |
| StateHistory | FromStateCode | FromState |

**Benefit:** Cleaner entity naming, shorter DTOs, better API contracts

## Multi-Tenancy

All tables have `TenantId` for row-level isolation:
- **Master schema:** TenantId NULLABLE (global defaults + tenant overrides)
- **Other schemas:** TenantId NOT NULL (complete row-level isolation)

Supports agencies → clients → sub-clients hierarchy via HierarchyId.

## Database

**Single SQL Server database:** `StarterKitMVC`
**Connection String:** `Server=localhost;Database=StarterKitMVC;Trusted_Connection=True;TrustServerCertificate=True`

**DbContexts (6):**
- MasterDbContext (14 tables)
- SharedDbContext (5 tables)
- CoreDbContext (3 tables)
- TransactionDbContext (1 table)
- ReportDbContext (4 tables)
- AuthDbContext (13 tables)

## Clients Supported

- ✅ MVC Web (direct service calls)
- ✅ Razor Pages (direct service calls)
- ✅ Blazor WASM (REST API)
- ✅ .NET MAUI (REST API)

## Component Versions (v4.1.0)

| Component | Version | Status | Reference |
|-----------|---------|--------|-----------|
| **Translation System** | v1.1.0 | ✅ Complete | [Wiki 01](docs/wiki/01-translation-system.md) \| [Demo](/Demo/Translations) |
| **Localized Validation** | v1.1.0 | ✅ Complete | [Wiki 02](docs/wiki/02-localized-validation.md) \| [Demo](/Demo/Validation) |
| **Base Page Pattern** | v1.1.0 | ✅ Complete | [Wiki 03](docs/wiki/03-base-page-pattern.md) |
| **Result Pattern** | v1.1.0 | ✅ Complete | [Wiki 04](docs/wiki/04-result-pattern.md) |
| **HTMX List Pattern** | v1.1.0 | ✅ Complete | [Wiki 05](docs/wiki/05-htmx-list-pattern.md) |
| **Public Demo Pages** | v1.0.0 | ✅ Complete | [Translations](/Demo/Translations) \| [Validation](/Demo/Validation) |
| **Wiki Documentation** | v1.0.0 | ✅ Complete | [docs/wiki/](docs/wiki/) |
| **Admin Products CRUD** | v1.0.0 | ✅ Complete | [Admin Pages](src/SmartWorkz.StarterKitMVC.Admin/Pages/Products/) |
| **Public Product Catalog** | v1.0.0 | ✅ Complete | [Public Pages](src/SmartWorkz.StarterKitMVC.Public/Pages/Products/) |

## Key Features

### v4.1.0 Additions

#### Demo Pages
- **Translation System Demo:** Interactive showcase of all MessageKeys with current translations
- **Validation Demo:** Form demonstrating all validation attribute types with translated error messages

#### Comprehensive Documentation
- **5 Wiki Files (1951 lines):** Complete guides for Translation System, Validation, Base Page, Result Pattern, HTMX Lists
- **Real Code Examples:** Every wiki page includes working examples from actual codebase
- **Developer-Focused:** Purpose → Quick Start → How It Works → Patterns → Common Mistakes → See Also

#### Pattern Implementations
- **Simple Validation:** MessageKey constants in error messages (no custom attributes needed)
- **Translation System:** Database-backed with 60-minute cache, per-tenant overrides, multi-locale support
- **Result Pattern:** Explicit success/failure outcomes (Result/Result<T> classes)
- **HTMX Integration:** Progressive enhancement for search/filter/pagination
- **Base Page Pattern:** TenantId, T() translation helper, toast messages, error handling

### v4.0.0 Foundation

#### Geo Hierarchy (Option C Hybrid)
- Countries table for fast lookups (indexed)
- GeoHierarchy table with HierarchyId for flexible State/City/District nesting
- No schema changes to add new geo levels (neighborhoods, regions, etc.)
- Handles varying depths per country (USA States, UK Districts)

#### Polymorphic Infrastructure
- **Addresses:** Link to any entity (Customer, Order, Employee, etc.)
- **Attachments:** File references for any entity
- **Comments:** Discussion threads for any entity
- **StateHistory:** Workflow tracking for any entity
- **PreferenceDefinitions:** System/tenant/user configuration

#### Production-Ready Reporting
- **ReportDefinitions:** SQL, Dashboard, Stored Procedure, API report types
- **ReportSchedules:** Cron-based background execution with email delivery
- **ReportExecutions:** Audit trail, performance metrics, result caching
- **ReportMetadata:** Filters, drill-downs, conditional formatting (extensible JSON)

## Extensibility

Teams can extend with Phase 1+ additions:
- Custom business entities (Products, Customers, Employees)
- Additional transaction types (Invoices, Payments, POs)
- Report distribution (Email, Slack, Teams)
- Advanced workflows
- Custom dashboards

No schema changes needed—use polymorphic pattern for linking.

## Old Documentation

v1 documentation archived in [`docs/old/`](docs/old/):
- README-v1.md (original project overview)
- SETUP.md (v1 setup instructions)
- architecture.md (v1 architecture)
- etc.

## Current Status

**v4.1.0 Features Complete:**
- ✅ Translation System (v1.1.0) — Production-ready, fully documented
- ✅ Localized Validation (v1.1.0) — Simple MessageKey approach, working examples
- ✅ Base Page Pattern (v1.1.0) — TenantId, T(), toast helpers, form handling
- ✅ Result Pattern (v1.1.0) — Explicit success/failure for all services
- ✅ HTMX List Pattern (v1.1.0) — Dynamic search/filter/pagination
- ✅ Demo Pages (v1.0.0) — Translations and Validation showcases
- ✅ Wiki Documentation (v1.0.0) — 5 comprehensive guides with examples
- ✅ Admin Products CRUD (v1.0.0) — Sample pages for admin area
- ✅ Public Product Catalog (v1.0.0) — Index and details pages

**Next Phase:**
- ⏳ Connect mock implementations to real repositories
- ⏳ Build additional admin feature areas
- ⏳ Implement frontend-specific features (cart, checkout)
- ⏳ Phase 1+ Extensions: Custom business entities, transaction types, advanced workflows

## Effort Estimate

- **Phase 1:** 35-46 hours (database, entities, services, REST API)
- **Phase 2:** 8-10 hours (documentation)
- **Phase 3:** 20-30 hours (MVC integration, testing)
- **Phase 4:** 10-15 hours (API polish, security)
- **Total:** 72-99 hours (3-4 weeks for 1-3 developers)

## References

- **SRS Document:** [`docs/srs/SRS-v4.md`](docs/srs/SRS-v4.md)
- **Implementation Plan:** [`docs/srs/IMPLEMENTATION-PLAN.md`](docs/srs/IMPLEMENTATION-PLAN.md)
- **Schema Review:** [`docs/srs/SCHEMA-REVIEW-v2.md`](docs/srs/SCHEMA-REVIEW-v2.md)

---

**SmartWorkz v4** — Enterprise-grade starter kit for multi-tenant .NET applications
