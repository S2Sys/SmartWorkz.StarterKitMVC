# SmartWorkz v4 - Specifications & Implementation Documentation

**Date:** 2026-03-31
**Status:** Phase 1 Ready
**Total Documentation:** 12 files, ~180KB

---

## 📚 Documentation Index

### 🚀 Start Here (Pick One)

| Document | Purpose | Time | Use When |
|----------|---------|------|----------|
| **[QUICK-REFERENCE-v4.md](QUICK-REFERENCE-v4.md)** | One-page schema overview | 5 min | Need quick facts (tables, endpoints, metrics) |
| **[../SCHEMA-SUMMARY-LEAN.md](../SCHEMA-SUMMARY-LEAN.md)** | LEAN schema explanation | 15 min | Want to understand design choices |
| **[README.md](#)** | Index & navigation | 5 min | New to the project |

---

### 🏗️ Architecture & Design

| Document | Purpose | Pages |
|----------|---------|-------|
| **[SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md)** | Complete table definitions with SQL examples | 27 |
| **[SCHEMA-DIAGRAM.md](SCHEMA-DIAGRAM.md)** | Visual relationship diagrams | 19 |
| **[SRS-v4.md](SRS-v4.md)** | Full system requirements specification | 24 |
| **[GEO-HIERARCHY-ANALYSIS.md](GEO-HIERARCHY-ANALYSIS.md)** | Geo-hierarchy design rationale | 15 |

**Read for:** Understanding the complete database design and architecture decisions

---

### 🛠️ Implementation Guides

| Document | Purpose | Pages |
|----------|---------|-------|
| **[IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md)** | 4-phase roadmap with detailed tasks | 28 |
| **[MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md)** | Complete menu system implementation | 30 |

**Read for:** Building Phase 1 (database scripts, entities, services, API)

---

### 📋 Summary & Updates

| Document | Purpose | Pages |
|----------|---------|-------|
| **[UPDATES-SUMMARY-v4.md](UPDATES-SUMMARY-v4.md)** | What changed in v4 | 20 |
| **[old-vs-proposed.md](old-vs-proposed.md)** | Comparison of v1 vs v4 | 8 |
| **[v4-schema-analysis.md](v4-schema-analysis.md)** | v4 design analysis | 7 |

**Read for:** Understanding changes from v1 and what's new

---

### ✅ Pre-Implementation

| Document | Location | Purpose |
|----------|----------|---------|
| **[REVIEW-CHECKLIST.md](../../REVIEW-CHECKLIST.md)** | Root | Pre-implementation validation checklist |

**Read before:** Starting Phase 1 implementation

---

## 📖 Reading Guide by Role

### Architect / Project Lead
1. [QUICK-REFERENCE-v4.md](QUICK-REFERENCE-v4.md) — 5 min overview
2. [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md) — Full design validation
3. [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md) — Phase 1-4 roadmap
4. [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md) — Feature details

**Total Time:** 2-3 hours

### Backend Developer (Phase 1)
1. [QUICK-REFERENCE-v4.md](QUICK-REFERENCE-v4.md) — Schema overview
2. [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md) — Table definitions
3. [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md) — Phase 1 tasks
4. [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md) — Services + API examples

**Total Time:** 3-4 hours

### Frontend Developer
1. [QUICK-REFERENCE-v4.md](QUICK-REFERENCE-v4.md) — API endpoints reference
2. [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md) — Menu API details
3. [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md) — API section

**Total Time:** 1-2 hours

### New Team Member
1. Start: This file — Navigation
2. Read: [QUICK-REFERENCE-v4.md](QUICK-REFERENCE-v4.md) — Quick facts
3. Understand: [../SCHEMA-SUMMARY-LEAN.md](../SCHEMA-SUMMARY-LEAN.md) — Why LEAN design
4. Explore: [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md) — Details as needed

**Total Time:** 1-2 hours to get up to speed

---

## 🎯 Documentation Quality Metrics

| Metric | Value | Notes |
|--------|-------|-------|
| **Total Lines** | ~12,000 | All specifications + guides |
| **Total Size** | ~180 KB | Highly compressed, text-only |
| **Code Examples** | 50+ | SQL, C#, EF Core, API, service patterns |
| **Tables** | 42 | Fully documented with column definitions |
| **Schemas** | 5 | Master, Shared, Transaction, Report, Auth |
| **Entities** | 42 | All domain classes documented |
| **API Endpoints** | 25+ | Complete with request/response examples |
| **Menu Features** | 10 | Fully detailed with use cases |
| **Database Scripts** | 8 | Ready to run |

---

## 🔗 File Cross-References

### If You're Looking For...

**Table Definitions**
- → [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md) (Sections 1-6, all tables)
- → [QUICK-REFERENCE-v4.md](QUICK-REFERENCE-v4.md) (Quick lookup)

**Menu System**
- → [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md) (Complete guide)
- → [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md) (Section 1.9, Menus table)

**API Endpoints**
- → [QUICK-REFERENCE-v4.md](QUICK-REFERENCE-v4.md) (Endpoint list)
- → [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md) (Menu API details)
- → [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md) (API section)

**Implementation Tasks**
- → [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md) (4-phase plan)
- → [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md) (Menu implementation)

**Geo Design Rationale**
- → [GEO-HIERARCHY-ANALYSIS.md](GEO-HIERARCHY-ANALYSIS.md) (3 options analyzed)
- → [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md) (Section 1.1, Option C)

**Design Patterns**
- → [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md) (Polymorphic, HierarchyId, soft delete)
- → [QUICK-REFERENCE-v4.md](QUICK-REFERENCE-v4.md) (Pattern overview)

**What Changed from v1**
- → [UPDATES-SUMMARY-v4.md](UPDATES-SUMMARY-v4.md) (Complete changelog)
- → [old-vs-proposed.md](old-vs-proposed.md) (Side-by-side comparison)

**Multi-Tenancy Details**
- → [SCHEMA-REVIEW-v2.md](SCHEMA-REVIEW-v2.md) (Sections 1-2, TenantId columns)
- → [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md) (Multi-tenant menus section)

**EF Core Implementation**
- → [MENU-SYSTEM-GUIDE.md](MENU-SYSTEM-GUIDE.md) (Service code examples)
- → [IMPLEMENTATION-PLAN.md](IMPLEMENTATION-PLAN.md) (DbContext section)

---

## ✨ Key Features Documented

- ✅ Option C Hybrid Geo (Countries + GeoHierarchy)
- ✅ Polymorphic Infrastructure (Addresses, Comments, Attachments, etc.)
- ✅ HierarchyId Trees (5 uses: Tenants, Lookups, Categories, EntityStates, MenuItems)
- ✅ Multi-Tenancy with row-level isolation
- ✅ Role-Based & Permission-Based Visibility
- ✅ Dynamic Navigation with auto-sitemap
- ✅ Production-Ready Reporting (4 tables)
- ✅ Soft Delete & Audit Trails
- ✅ 3-4 DbContexts (optimized from 6)
- ✅ 42 Total Tables, 5 Schemas

---

**Status: ✅ All specifications complete, Phase 1 ready**

Last Updated: 2026-03-31 | Commits: 6 | Changes: All files organized in docs/srs/
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
