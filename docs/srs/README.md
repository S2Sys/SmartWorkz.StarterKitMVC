# SmartWorkz StarterKitMVC v4 - Specification Documents

**Date:** 2026-03-31
**Status:** Complete & Ready for Implementation Review

---

## Document Overview

This folder contains the complete v4 specification for SmartWorkz StarterKitMVC upgrade.

### 📋 Specification Documents

| Document | Purpose | Pages |
|----------|---------|-------|
| **SRS-v4.md** | Software Requirements Specification - Complete system design | 50+ |
| **old-vs-proposed.md** | Gap analysis: v1 (old) vs v4 (proposed) with risk assessment | 10 |
| **v4-schema-analysis.md** | Database schema review for generic enterprise coverage, identifies missing 26 tables in 3 phases | 15 |
| **SCHEMA-REVIEW.md** | Table-by-table documentation of all 62 tables with column definitions, relationships, indexes | 70+ |
| **SCHEMA-DIAGRAM.md** | Visual relationship diagrams showing key patterns and data flows | 30+ |
| **IMPLEMENTATION-PLAN.md** | 4-phase execution plan with detailed task breakdown and effort estimates | 50+ |

---

## Quick Start: Review Order

1. **Start here:** [SRS-v4.md](SRS-v4.md) (5-10 min reading)
   - Overall system design
   - Architecture overview
   - Database schema summary

2. **Visual understanding:** [SCHEMA-DIAGRAM.md](SCHEMA-DIAGRAM.md) (10-15 min)
   - See how schemas relate
   - Understand polymorphic patterns
   - Review key design principles

3. **Deep dive details:** [SCHEMA-REVIEW.md](SCHEMA-REVIEW.md) (20-30 min)
   - All 62 tables documented
   - Column-by-column definitions
   - Relationship mappings
   - Index strategy

4. **Plan review:** [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md) (15-20 min)
   - 4-phase rollout
   - Task breakdown
   - Effort estimates
   - Critical path

5. **Context & gaps:** [old-vs-proposed.md](old-vs-proposed.md) + [v4-schema-analysis.md](v4-schema-analysis.md) (10-15 min)
   - What changed from v1
   - What's missing (future phases)
   - Risk mitigation

---

## Key Numbers

| Metric | Value |
|--------|-------|
| **Schemas** | 6 (Master, Core, Transaction, Report, Auth, Sales) |
| **Tables** | 62 (17+18+8+5+13+1) |
| **Domain Entities** | 62 (one per table) |
| **Service Interfaces** | 40+ (organized by schema) |
| **API Endpoints** | 20+ (in Phase 1) |
| **DbContexts** | 4-6 (EF Core) |
| **Shared Tables** | 5 polymorphic (Addresses, Attachments, Tags, Comments, StateHistory) |
| **HierarchyId Trees** | 4 (Tenants, Lookups, Categories, EntityStates) |
| **Total Effort** | 90-120 hours over 4 weeks |

---

## Database Schema at a Glance

### Master Schema (17 tables) - Reference Data
**TenantId:** NULLABLE (global defaults + tenant overrides)

**Categories:**
- **Geo reference:** Countries, States, Cities (3 tbl)
- **Localization:** Languages, Translations (2 tbl)
- **Hierarchies:** Lookups, Categories, EntityStates, EntityStateTransitions (4 tbl)
- **Notifications:** NotificationChannels, TemplateGroups, Templates (3 tbl)
- **SaaS & Config:** SubscriptionPlans, PreferenceDefinitions (2 tbl)
- **SEO:** SeoMeta, UrlRedirects (2 tbl)
- **Logging:** AuditLogs, ActivityLogs (2 tbl)

### Core Schema (18 tables) - Configuration + Business
**TenantId:** NOT NULL (multi-tenant row isolation)

**Categories:**
- **Tenant Config:** Tenants (HierarchyId), TenantSubscriptions, TenantSettings, FeatureFlags (4 tbl)
- **Business Entities:** Products, Customers, Vendors, Projects, Teams, Departments, Employees, Assets, Contracts (9 tbl)
- **Shared Infrastructure:** Addresses, Attachments, Tags, Comments, StateHistory (5 polymorphic tbl)

### Transaction Schema (8 tables) - Financial Data
**TenantId:** NOT NULL

**Categories:**
- **Sales Cycle:** Orders, OrderLines, Invoices, Payments (4 tbl)
- **Purchasing Cycle:** PurchaseOrders, PurchaseOrderLines, Receipts, CreditNotes (4 tbl)

### Report Schema (5 tables) - Reporting Infrastructure
**TenantId:** NULLABLE

- ReportDefinitions, ReportSchedules, ReportResults, ReportAuditLogs, DashboardWidgets

### Auth Schema (13 tables) - Identity & RBAC
**TenantId:** NOT NULL

**Categories:**
- **Identity:** Users, UserProfiles, UserPreferences (3 tbl)
- **RBAC:** Roles, Permissions, RolePermissions, UserRoles (4 tbl)
- **Sessions:** RefreshTokens, VerificationCodes, ExternalLogins (3 tbl)
- **Logging:** AuditLogs, ActivityLogs, NotificationLogs (3 tbl)

### Sales Schema (1 table) - Team-Specific Example
**TenantId:** NOT NULL

- SalesOrders (extensible; teams can add Marketing, HR, Finance schemas)

---

## Design Principles

### ✅ Already Implemented in v4

1. **Polymorphic Linking** (Addresses, Attachments, Tags, Comments)
   - EntityType + EntityId pattern allows any table to link
   - Single table instead of separate per-entity versions

2. **HierarchyId Trees** (Tenants, Lookups, Categories, EntityStates)
   - Unlimited nesting depth
   - Efficient ancestor/descendant queries

3. **Soft Delete** (IsDeleted, DeletedAt, DeletedBy)
   - Reversible deletion
   - Maintains referential integrity
   - Audit trail preserved

4. **Audit Columns** (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
   - Who made changes and when
   - Compliance/traceability

5. **Multi-Tenancy** (TenantId partitioning)
   - Row-level tenant isolation
   - Secure data segregation
   - Nullable in Master (global + tenant overrides)

6. **State Machines** (EntityStates, EntityStateTransitions, StateHistory)
   - Model any workflow
   - Track state changes over time

7. **Template System** (Templates table)
   - Email, SMS, WhatsApp, Push, InApp channels
   - Global + tenant-specific templates
   - Multi-language support

8. **Flexible Configuration** (TenantSettings, FeatureFlags)
   - Key-value store (no hardcoding)
   - Tenant feature toggles
   - Easy customization per tenant

---

## Implementation Phases

### Phase 1: Foundation (41.5-48 hours) ✅ PLANNED
- Database scripts (12-15h)
- Domain entities (8-10h)
- EF Core DbContexts (10-12h)
- Services (6-8h)
- REST API (8-10h)
- Configuration (2-3h)

**Result:** v4 API operational with real database, ready for Blazor/MAUI clients

### Phase 2: Documentation & Cleanup (8 hours) — PARALLEL WITH PHASE 3
- File organization (2h)
- Documentation updates (6h)

**Result:** Clean root directory, comprehensive v4 docs

### Phase 3: MVC Integration (20.5-30 hours) — DEPENDS ON PHASE 1
- View updates (5-7h)
- Controller updates (5-7h)
- Testing & profiling (8-10h)
- Optional Dapper integration (0-5h, conditional)

**Result:** Existing MVC Admin fully functional with v4 database

### Phase 4: API Polish (10-15 hours) — FINAL POLISH
- Swagger documentation (4h)
- Versioning & rate limiting (3h)
- Error handling (3h)
- Health checks (2.5h)
- Security hardening (2.5h)

**Result:** Production-ready API with comprehensive documentation

---

## Future Enhancement Phases (NOT in v4.0)

### Phase 1+ (v4.1) - Critical Additions
8 tables: Workflows, WorkflowInstances, WorkflowApprovals, Notifications inbox, Logs, ApiKeys, AuditTrail
→ Total: 70 tables

### Phase 2+ (v4.2) - E-Commerce/CRM
13 tables: Wishlists, Reviews, Coupons, Bundles, ShippingMethods, WarehouseLocations, StockMovements, Queues, etc.
→ Total: 83 tables

### Phase 3+ (v4.3+) - Industry-Specific
14 tables: CRM (Contacts, Agents, Territories), Accounting (GL, JournalEntries), Advanced Logistics, etc.

---

## What You'll See After Implementation

### Database (SQL Server)
```
StarterKitMVC database
├─ Master schema (17 tables)
├─ Core schema (18 tables)
├─ Transaction schema (8 tables)
├─ Report schema (5 tables)
├─ Auth schema (13 tables)
└─ Sales schema (1 table)
```

### Code Structure
```
src/
├─ SmartWorkz.StarterKitMVC.Domain/ → 62 entities
├─ SmartWorkz.StarterKitMVC.Application/ → 5 main services, 60+ DTOs
├─ SmartWorkz.StarterKitMVC.Infrastructure/ → 4-6 DbContexts, repositories
├─ SmartWorkz.StarterKitMVC.Shared/ → Base entities, extensions
├─ SmartWorkz.StarterKitMVC.Web/ → MVC + Razor Pages (updated)
└─ SmartWorkz.StarterKitMVC.Api/ → REST API (new)
```

### API Endpoints (Phase 1)
```
/api/v1/
├─ auth/ (login, register, profile, refresh)
├─ users/ (CRUD user management)
├─ roles/ (CRUD role management)
├─ permissions/ (list, assign)
├─ tenants/ (CRUD tenant management)
├─ lookups/ (countries, states, cities, languages)
├─ categories/ (hierarchical categories)
├─ translations/ (i18n keys and values)
├─ products/ (CRUD)
├─ customers/ (CRUD)
├─ orders/ (CRUD)
├─ invoices/ (CRUD)
├─ payments/ (CRUD)
├─ templates/ (notification templates)
└─ settings/ (tenant settings)
```

---

## Sign-Off Checklist

### Before Implementation Starts

- [ ] Review SCHEMA-DIAGRAM.md - Confirm data structure makes sense
- [ ] Review SCHEMA-REVIEW.md - Check all table definitions
- [ ] Review SRS-v4.md - Approve overall architecture
- [ ] Review IMPLEMENTATION-PLAN.md - Agree on phases and effort
- [ ] Confirm database naming (PascalCase schemas: Master, Core, etc.)
- [ ] Confirm multi-tenancy strategy (TenantId nullable in Master)
- [ ] Confirm EF Core + optional Dapper approach
- [ ] Confirm 4-phase rollout (Foundation → Docs → MVC → Polish)

---

## Questions Before Starting?

Common clarifications:

**Q: Why 4 DbContexts (Master, Core, Transaction, Report, Auth, Sales)?**
A: One DB, multiple schemas. Each DbContext maps to a schema for better organization and maintainability. Can be consolidated if needed.

**Q: Why polymorphic linking (Addresses, Tags)?**
A: Single table that works with any entity. Avoids creating separate OrderAddresses, InvoiceAddresses, etc. Faster schema evolution.

**Q: What about Dapper?**
A: EF Core primary. Add Dapper only if profiling (Phase 3) shows query latency >200ms. Conditional, not in Phase 1.

**Q: Can I add my own business entities?**
A: Yes! Products, Customers, Orders are placeholders. Replace with your domain entities (same pattern: Code+Name+TenantId+IsDeleted).

**Q: What about workflow/approval tables?**
A: Deferred to Phase 1+ (v4.1). v4.0 has EntityStates/StateHistory framework ready; Phase 1+ adds explicit Workflows table.

---

## Documents Summary

| Document | Size | Depth | Best For |
|----------|------|-------|----------|
| SRS-v4.md | 50+ pages | Complete spec | Overall understanding |
| old-vs-proposed.md | 10 pages | Gap analysis | Context & changes |
| v4-schema-analysis.md | 15 pages | Future planning | Enhancement roadmap |
| **SCHEMA-REVIEW.md** | **70+ pages** | **Detailed** | **Technical review** |
| **SCHEMA-DIAGRAM.md** | **30+ pages** | **Visual** | **Pattern understanding** |
| IMPLEMENTATION-PLAN.md | 50+ pages | Execution | Task breakdown & effort |

---

**Status: All specifications complete and ready for review.**

**Next Step:** Review these documents and confirm you're comfortable with all table definitions, relationships, and design decisions. Then proceed to Phase 1 (database script generation).

**Estimated Review Time:** 60-90 minutes for thorough understanding.
