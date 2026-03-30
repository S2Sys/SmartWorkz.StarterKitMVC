# SmartWorkz StarterKitMVC v4 - Implementation Plan

**Date:** 2026-03-31
**Status:** Ready for Execution
**Total Estimated Effort:** 90-120 hours across 4 phases

---

## Executive Summary

This document outlines the detailed execution plan to upgrade SmartWorkz StarterKitMVC from v1 (single-client MVC boilerplate with no real database) to v4 (multi-client enterprise platform with 62-table database, REST API, and support for MVC, Razor Pages, Blazor WASM, and .NET MAUI).

**Key milestones:**
- Phase 1: Foundation (40-50 hours) — Database, entities, EF Core DbContexts, API layer
- Phase 2: Documentation & Cleanup (8-10 hours) — Move files, update docs
- Phase 3: MVC Integration (20-30 hours) — Connect MVC to real database
- Phase 4: API Polish (10-15 hours) — Swagger, versioning, rate limiting, CORS

**Go-live readiness:** After Phase 1 completion, v4 is production-ready for API clients. Phases 2-4 add polish and full MVC integration.

---

## 1. Phase 1: Foundation (40-50 hours) — CRITICAL PATH

### 1.1 Database Layer (12-15 hours)

**Tasks:**

| # | Task | Effort | Dependencies | Deliverables |
|---|------|--------|--------------|--------------|
| 1.1.1 | Create `.gitignore` (standard .NET) | 0.5h | None | `.gitignore` file |
| 1.1.2 | Create `database/v4/001_CreateSchemas.sql` | 1h | None | 6 schemas (Master, Core, Transaction, Report, Auth, Sales) |
| 1.1.3 | Create `database/v4/002_CreateTables_Master.sql` | 2h | 1.1.2 | 17 tables: Countries, States, Cities, Languages, Translations, Lookups, Categories, EntityStates, EntityStateTransitions, NotificationChannels, TemplateGroups, Templates, SubscriptionPlans, PreferenceDefinitions, SeoMeta, UrlRedirects, AuditLogs, ActivityLogs |
| 1.1.4 | Create `database/v4/003_CreateTables_Core.sql` | 3h | 1.1.2 | 18 tables: Tenants, TenantSubscriptions, TenantSettings, FeatureFlags, Products, Customers, Vendors, Projects, Teams, Departments, Employees, Assets, Contracts, Addresses, Attachments, Tags, Comments, StateHistory |
| 1.1.5 | Create `database/v4/004_CreateTables_Transaction.sql` | 2h | 1.1.2 | 8 tables: Orders, OrderLines, Invoices, Payments, PurchaseOrders, PurchaseOrderLines, Receipts, CreditNotes |
| 1.1.6 | Create `database/v4/005_CreateTables_Report.sql` | 1.5h | 1.1.2 | 5 tables: ReportDefinitions, ReportSchedules, ReportResults, ReportAuditLogs, DashboardWidgets |
| 1.1.7 | Create `database/v4/006_CreateTables_Auth.sql` | 2h | 1.1.2 | 13 tables: Users, UserProfiles, UserPreferences, Roles, Permissions, RolePermissions, UserRoles, RefreshTokens, VerificationCodes, ExternalLogins, AuditLogs, ActivityLogs, NotificationLogs |
| 1.1.8 | Create `database/v4/007_CreateTables_Sales.sql` | 0.5h | 1.1.2 | 1 table: SalesOrders |
| 1.1.9 | Create `database/v4/008_SeedData.sql` | 2h | 1.1.3-1.1.8 | Seed: Countries, States, Cities, Languages, Lookups, Categories, EntityStates, Roles, Permissions, Templates, SubscriptionPlans |
| 1.1.10 | Create `database/v4/009_CreateIndexes.sql` | 1.5h | 1.1.3-1.1.8 | Indexes on: TenantId, CreatedAt, UpdatedAt, EntityType+EntityId (polymorphic), HierarchyId paths, FK columns |
| **Subtotal** | | **15.5h** | | **All v4 database structures** |

**Success criteria:**
- All 62 tables created successfully
- All indexes created for performance
- Seed data inserted without errors
- `dotnet ef database update` would execute cleanly (even though manual SQL)

---

### 1.2 Domain Layer (8-10 hours)

**Tasks:**

| # | Task | Effort | Dependencies | Deliverables |
|---|------|--------|--------------|--------------|
| 1.2.1 | Create base entity classes (AuditableEntity, SoftDeletableEntity, TenantEntity, TenantSoftDeletableEntity) | 1.5h | None | 4 base classes in `Domain/Common/` |
| 1.2.2 | Create Master schema entities (17 entities) | 2h | 1.2.1 | Country, State, City, Language, Translation, Lookup, Category, EntityState, EntityStateTransition, NotificationChannel, TemplateGroup, Template, SubscriptionPlan, PreferenceDefinition, SeoMeta, UrlRedirect, AuditLog, ActivityLog |
| 1.2.3 | Create Core schema entities (18 entities) | 2.5h | 1.2.1 | Tenant, TenantSubscription, TenantSetting, FeatureFlag, Product, Customer, Vendor, Project, Team, Department, Employee, Asset, Contract, Address, Attachment, Tag, Comment, StateHistory |
| 1.2.4 | Create Transaction schema entities (8 entities) | 1.5h | 1.2.1 | Order, OrderLine, Invoice, Payment, PurchaseOrder, PurchaseOrderLine, Receipt, CreditNote |
| 1.2.5 | Create Report schema entities (5 entities) | 1h | 1.2.1 | ReportDefinition, ReportSchedule, ReportResult, ReportAuditLog, DashboardWidget |
| 1.2.6 | Create Auth schema entities (13 entities) | 1.5h | 1.2.1 | User, UserProfile, UserPreference, Role, Permission, RolePermission, UserRole, RefreshToken, VerificationCode, ExternalLogin, AuditLog, ActivityLog, NotificationLog |
| 1.2.7 | Create Sales schema entities (1 entity) | 0.5h | 1.2.1 | SalesOrder |
| **Subtotal** | | **10.5h** | | **All 62 domain entities with XML docs** |

**Success criteria:**
- All entities have proper navigation properties
- All entities map to database tables correctly
- All entities inherit from appropriate base classes
- XML documentation on all public properties

---

### 1.3 Infrastructure Layer - EF Core (10-12 hours)

**Tasks:**

| # | Task | Effort | Dependencies | Deliverables |
|---|------|--------|--------------|--------------|
| 1.3.1 | Create MasterDbContext | 2h | 1.2.2 | DbContext with 17 DbSets, fluent configurations for all Master tables |
| 1.3.2 | Create CoreDbContext | 2h | 1.2.3 | DbContext with 18 DbSets, fluent configurations, HierarchyId setup for Tenants |
| 1.3.3 | Create TransactionDbContext | 1.5h | 1.2.4 | DbContext with 8 DbSets, fluent configurations with FKs to Core entities |
| 1.3.4 | Create ReportDbContext | 1h | 1.2.5 | DbContext with 5 DbSets, fluent configurations |
| 1.3.5 | Create AuthDbContext | 1.5h | 1.2.6 | DbContext with 13 DbSets, fluent configurations, identity integrations |
| 1.3.6 | Create SalesDbContext (optional, may defer if 1 table) | 0.5h | 1.2.7 | DbContext with 1 DbSet, or consolidate into CoreDbContext |
| 1.3.7 | Create generic Repository<T> pattern | 1.5h | All DbContexts | IRepository<T>, IQueryableRepository<T> with common CRUD + filtering |
| 1.3.8 | Create tenant-scoped repositories (ITenantRepository<T>) | 1.5h | 1.3.7 | Tenant-filtered queries automatically via IQueryable |
| 1.3.9 | Create HierarchyId repositories (IHierarchyRepository<T>) | 1h | 1.3.7 | Tree query helpers (GetAncestors, GetDescendants, GetLevel) |
| 1.3.10 | Wire DbContexts in DI container (FeatureServiceExtensions or new helper) | 1h | 1.3.1-1.3.6 | Register all 4-6 DbContexts with connection string |
| **Subtotal** | | **12.5h** | | **All DbContexts + repositories + DI wiring** |

**Success criteria:**
- `dotnet build` succeeds with no EF Core warnings
- All DbContexts can be instantiated via DI
- Repository pattern provides clean data access interface
- Tenant filtering works transparently

---

### 1.4 Application Layer Services (6-8 hours)

**Tasks:**

| # | Task | Effort | Dependencies | Deliverables |
|---|------|--------|--------------|--------------|
| 1.4.1 | Create IService interfaces for each schema (IMasterService, ICoreService, ITransactionService, IReportService, IAuthService) | 2h | 1.3.7 | 5 high-level service interfaces with CRUD + domain logic method contracts |
| 1.4.2 | Create service implementations (MasterService, CoreService, TransactionService, ReportService, AuthService) | 3h | 1.4.1, 1.3.7 | Implementations using repositories, with domain logic |
| 1.4.3 | Create DTOs for API requests/responses (CreateXxxDto, UpdateXxxDto, XxxDto) | 1.5h | 1.2.2-1.2.7 | ~60 DTO classes covering all entities |
| **Subtotal** | | **6.5h** | | **Services + DTOs** |

**Success criteria:**
- All services inject repositories
- DTOs map cleanly to/from entities
- Services handle validation & error cases

---

### 1.5 API Layer (8-10 hours)

**Tasks:**

| # | Task | Effort | Dependencies | Deliverables |
|---|------|--------|--------------|--------------|
| 1.5.1 | Create Api project structure (`SmartWorkz.StarterKitMVC.Api`) | 0.5h | None | Project file + Program.cs + appsettings |
| 1.5.2 | Create BaseApiController (versioning, error handling, auth) | 1h | 1.4.1 | Abstract controller with common patterns |
| 1.5.3 | Create Auth controllers (Login, Register, RefreshToken, Profile) | 1.5h | 1.4.2, JWT config | POST /api/v1/auth/login, /register, /refresh, GET /profile |
| 1.5.4 | Create User management controllers (Users, Roles, Permissions CRUD) | 2h | 1.4.2 | POST/GET/PUT/DELETE /api/v1/users, /roles, /permissions |
| 1.5.5 | Create Tenant controllers (Tenants, TenantSettings, Features) | 1.5h | 1.4.2 | POST/GET/PUT /api/v1/tenants, /tenants/{id}/settings, /tenants/{id}/features |
| 1.5.6 | Create lookup controllers (Countries, States, Cities, Languages) | 1h | 1.4.2 | GET /api/v1/lookups/countries, /states, /cities, /languages |
| 1.5.7 | Create business entity controllers (Products, Customers, Orders, Invoices) | 1.5h | 1.4.2 | POST/GET/PUT/DELETE /api/v1/products, /customers, /orders, /invoices |
| 1.5.8 | Create template/notification controllers | 1h | 1.4.2 | POST/GET /api/v1/templates, /notifications |
| 1.5.9 | Configure JWT authentication middleware + authorization attributes | 1.5h | 1.3.10 | [Authorize] attributes on controllers, JWT bearer token validation |
| 1.5.10 | Configure API versioning (/api/v1/) | 0.5h | 1.5.2 | ApiVersion attribute, version routing |
| **Subtotal** | | **10.5h** | | **Full REST API with auth + core endpoints** |

**Success criteria:**
- POST /api/v1/auth/login returns JWT token
- GET /api/v1/users requires [Authorize]
- All endpoints return 200/400/401/500 appropriately
- API can be called from external clients (Blazor, MAUI)

---

### 1.6 Configuration & Database Connection (2-3 hours)

**Tasks:**

| # | Task | Effort | Dependencies | Deliverables |
|---|------|--------|--------------|--------------|
| 1.6.1 | Update `appsettings.json` with correct connection string to v4 database | 0.5h | 1.1.1-1.1.9 | ConnectionStrings.DefaultConnection pointing to StarterKitMVC database |
| 1.6.2 | Create database initialization script (if first run, create database + run all scripts) | 1h | 1.1.1-1.1.9 | PowerShell script or .NET CLI command to initialize v4 database |
| 1.6.3 | Update Web project Startup.cs to use real repositories instead of no-op implementations | 1.5h | 1.3.10, 1.4.2 | Wire real services in DI container, remove mock implementations |
| **Subtotal** | | **3h** | | **Database ready, connection validated** |

**Success criteria:**
- `dotnet run` connects to v4 database successfully
- Health check endpoint returns database: ok
- First query returns real data from database

---

### **Phase 1 Total: 41.5-48 hours**

**Go-live criteria:**
✅ v4 database with 62 tables
✅ All 62 domain entities created
✅ 4 DbContexts with repositories
✅ 5 main services (Master, Core, Transaction, Report, Auth)
✅ REST API with 20+ endpoints covering auth, users, tenants, lookups, business entities
✅ JWT authentication working
✅ Database connection validated
✅ Web project connected to real database

**Post-Phase 1 state:** v4 is production-ready for API clients (Blazor, MAUI). MVC views need updating to work with new entity models (Phase 3).

---

## 2. Phase 2: Documentation & Cleanup (8-10 hours)

### 2.1 File Organization (2-3 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 2.1.1 | Verify all v1 .md files moved to `docs/old/` with `_v1` suffix | 0.5h | CHANGELOG_v1.md, SETUP_v1.md, TODO_v1.md, tasks_v1.md already done |
| 2.1.2 | Create `docs/README.md` index listing all docs | 0.5h | Links to: SRS-v4.md, old-vs-proposed.md, v4-schema-analysis.md, architecture.md, how-to-use.md |
| 2.1.3 | Update root `README.md` with v4 overview | 1h | New features, quick start, client support (MVC/Razor/Blazor/MAUI) |
| **Subtotal** | | **2h** | |

---

### 2.2 Core Documentation Updates (5-7 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 2.2.1 | Rewrite `CHANGELOG.md` for v4 release | 1.5h | Sections: Breaking Changes (v1→v4), New Features, Database Schema, Architecture |
| 2.2.2 | Rewrite `docs/architecture.md` with v4 diagrams | 1.5h | Update: 6 schemas, 4 DbContexts, API layer, multi-client diagram |
| 2.2.3 | Create `docs/DATABASE.md` with schema and table reference | 1h | Tables per schema, key relationships, HierarchyId patterns |
| 2.2.4 | Create `docs/API.md` with endpoint reference | 1h | All 20+ endpoints, request/response examples, auth patterns |
| 2.2.5 | Update `docs/how-to-use.md` with v4 examples (migrations from v1) | 1h | Example: "Query products" using new service interfaces |
| **Subtotal** | | **6h** | |

---

### **Phase 2 Total: 8 hours**

**Deliverables:**
✅ Root directory cleaned (old docs in docs/old/)
✅ README.md updated for v4
✅ CHANGELOG.md documenting v1→v4 changes
✅ docs/architecture.md with 6-schema diagram
✅ docs/DATABASE.md with full schema reference
✅ docs/API.md with endpoint documentation

---

## 3. Phase 3: MVC Integration (20-30 hours) — CAN RUN IN PARALLEL WITH PHASE 2

### 3.1 View Model Updates (5-7 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 3.1.1 | Update existing Admin Dashboard views to use new entity models | 2h | Dependency on Phase 1 services; update @Model bindings |
| 3.1.2 | Update Settings Management views (v4 TenantSettings instead of v1 SettingValues) | 1.5h | Wire to new SettingService |
| 3.1.3 | Update LoV Management views (v4 Lookups + Categories instead of v1 LovItems) | 1.5h | Wire to new LookupService; show HierarchyId trees |
| 3.1.4 | Update Identity Admin views (v4 Users, Roles, Permissions instead of v1 Claims) | 1.5h | Wire to new AuthService |
| **Subtotal** | | **6.5h** | |

---

### 3.2 Controller Updates (5-7 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 3.2.1 | Update Admin controllers to inject real services (not no-op mocks) | 2h | HomeController, SettingsController, LoVController, IdentityController |
| 3.2.2 | Update controller actions to use service methods returning entities/DTOs | 2.5h | Add proper error handling, validation, redirect logic |
| 3.2.3 | Create new controllers for new features (Tenants, Products, Customers, Orders) | 1.5h | Basic CRUD controllers for business entities |
| **Subtotal** | | **6h** | |

---

### 3.3 Testing & Integration (8-10 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 3.3.1 | Create integration tests for Admin controllers | 2h | Test: create, read, update, delete operations with real database |
| 3.3.2 | Create integration tests for new business entity controllers | 2h | Test: Orders, Invoices, Products flows |
| 3.3.3 | Manual regression testing: Admin Dashboard, Settings, LoV, Identity | 2h | Smoke test all existing admin views |
| 3.3.4 | Performance testing: Query profiling for large datasets (e.g., 10k orders) | 1.5h | Profile with SQL Server Profiler; identify Dapper candidates |
| 3.3.5 | Load testing: API endpoints with concurrent requests (100 simultaneous) | 1.5h | Verify rate limiting, connection pooling, response times |
| **Subtotal** | | **8.5h** | |

---

### 3.4 Dapper Integration (Optional, if profiling shows need) (1-5 hours)

**Tasks:**

| # | Task | Effort | Condition | Notes |
|---|------|--------|-----------|-------|
| 3.4.1 | Create Dapper query layer for Transaction schema (Order queries) | 2h | If 100+ concurrent requests show >200ms latency | Complex JOINS optimized with hand-written SQL |
| 3.4.2 | Create Dapper query layer for Report schema (Report aggregations) | 1.5h | If report generation >5 seconds | Bulk aggregations, GROUP BY, window functions |
| 3.4.3 | Create test coverage for Dapper queries | 1.5h | If Dapper implemented | Unit tests for SQL correctness |
| **Subtotal** | | **0-5h** | Conditional | Defer unless performance issues found |

---

### **Phase 3 Total: 20.5-30 hours**

**Deliverables:**
✅ All Admin MVC views updated to work with v4 entities
✅ All Admin controllers wired to real services
✅ New business entity controllers (Products, Customers, Orders)
✅ Integration tests for all Admin flows
✅ Regression testing completed
✅ Performance profiling completed (triggers Phase 3.4 if needed)
✅ Dapper integration added for high-throughput queries (optional)

---

## 4. Phase 4: API Polish (10-15 hours) — FINAL POLISH

### 4.1 Swagger & Documentation (3-4 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 4.1.1 | Configure Swagger/OpenAPI with XML comments from all API controllers | 1.5h | SwaggerGen from all controller XML docs |
| 4.1.2 | Add Swagger authentication UI (Bearer token input) | 1h | JWT token paste in Swagger UI |
| 4.1.3 | Generate Swagger schema for all DTOs | 0.5h | Automatic from DTO properties |
| 4.1.4 | Test Swagger UI: try all endpoints, verify examples | 1h | Manual verification of /swagger/ page |
| **Subtotal** | | **4h** | |

---

### 4.2 API Versioning & Rate Limiting (2-3 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 4.2.1 | Verify API versioning works (/api/v1/, /api/v2/ ready for future) | 0.5h | Test header, query param, URL path versioning |
| 4.2.2 | Configure rate limiting per endpoint (100 req/min for most, 10 req/min for auth) | 1h | Serilog + custom rate limit middleware |
| 4.2.3 | Test rate limiting: verify 429 response when exceeded | 0.5h | Load test with 150+ req/sec |
| 4.2.4 | Configure CORS for Blazor & MAUI clients | 1h | AllowedOrigins, AllowedMethods, AllowCredentials |
| **Subtotal** | | **3h** | |

---

### 4.3 Error Handling & Validation (2-3 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 4.3.1 | Create global exception handler middleware (catches all unhandled exceptions) | 1h | Returns standardized error response {error, message, statusCode} |
| 4.3.2 | Add request validation middleware (ModelState, FluentValidation) | 1h | Validates all DTOs before controller action |
| 4.3.3 | Test error scenarios: invalid input, 404, 500, 401, 403 | 1h | Verify error responses are consistent |
| **Subtotal** | | **3h** | |

---

### 4.4 Health Checks & Monitoring (2-3 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 4.4.1 | Configure /health endpoint with database check | 1h | Returns {status: "healthy"} or {status: "degraded", details: ...} |
| 4.4.2 | Setup Application Insights telemetry (if monitoring enabled in config) | 1h | Request traces, exception tracking, dependency monitoring |
| 4.4.3 | Create /health-ui dashboard (if enabled) | 0.5h | Visual health status, response times, error rates |
| **Subtotal** | | **2.5h** | |

---

### 4.5 Security Hardening (2-3 hours)

**Tasks:**

| # | Task | Effort | Notes |
|---|------|--------|-------|
| 4.5.1 | Verify HTTPS redirect working | 0.5h | All traffic redirects to HTTPS |
| 4.5.2 | Verify HSTS headers set correctly | 0.5h | Strict-Transport-Security header |
| 4.5.3 | Verify antiforgery tokens on POST/PUT/DELETE in MVC | 0.5h | [ValidateAntiForgeryToken] on Admin controllers |
| 4.5.4 | Review API security: no secrets in code, secure headers | 1h | Remove hard-coded JWT secret, use configuration |
| **Subtotal** | | **2.5h** | |

---

### **Phase 4 Total: 12-14 hours**

**Deliverables:**
✅ Swagger UI working with all endpoints documented
✅ API versioning (/api/v1/) ready for future versions
✅ Rate limiting configured and tested
✅ CORS enabled for Blazor & MAUI
✅ Global error handling middleware
✅ Request validation middleware
✅ Health checks endpoint
✅ Application Insights monitoring (optional)
✅ Security hardening complete (HTTPS, HSTS, antiforgery)

---

## 5. Critical Path & Dependencies

```
Phase 1 Foundation (41.5-48h) — BLOCKING
├─ 1.1 Database (12-15h)
│  ├─ 1.1.1 .gitignore (0.5h)
│  ├─ 1.1.2 Schemas (1h) ← blocks 1.1.3-8
│  ├─ 1.1.3-8 Tables (11h) ← blocks 1.1.9-10
│  ├─ 1.1.9 Seed data (2h)
│  └─ 1.1.10 Indexes (1.5h)
│
├─ 1.2 Domain entities (8-10h) ← depends on 1.1.2 schema design
│
├─ 1.3 EF Core (10-12h) ← depends on 1.2 entities
│
├─ 1.4 Services (6-8h) ← depends on 1.3 DbContexts
│
├─ 1.5 API (8-10h) ← depends on 1.4 services
│
└─ 1.6 Configuration (2-3h) ← depends on all above

Phase 2 Documentation (8h) — CAN RUN IN PARALLEL WITH PHASE 3
├─ 2.1 File org (2h)
└─ 2.2 Docs (6h)

Phase 3 MVC Integration (20-30h) — DEPENDS ON PHASE 1
├─ 3.1 Views (5-7h) ← depends on Phase 1 services
├─ 3.2 Controllers (5-7h) ← depends on Phase 1
├─ 3.3 Testing (8-10h)
└─ 3.4 Dapper (0-5h, conditional on perf profiling)

Phase 4 API Polish (10-15h) — FINAL POLISH, AFTER PHASE 1
├─ 4.1 Swagger (4h)
├─ 4.2 Versioning & Rate Limiting (3h)
├─ 4.3 Error Handling (3h)
├─ 4.4 Health Checks (2.5h)
└─ 4.5 Security (2.5h)
```

**Parallel execution opportunities:**
- Phase 2 (Docs) runs during Phase 3 (MVC Integration) — documentation team can work independently
- Phase 4 (Polish) runs after Phase 1, doesn't block Phase 3
- Phase 3.4 (Dapper) runs only if performance profiling (3.3.4) identifies bottlenecks

**Recommended sequence:**
1. **Start:** Phase 1 (weeks 1-2) — foundation
2. **Week 2:** Phase 2 + Phase 3 in parallel (docs team) + (dev team)
3. **Week 3:** Phase 3 continues (testing/profiling)
4. **Week 4:** Phase 4 (polish) + Phase 3.4 (if needed)

---

## 6. Schema Enhancement Strategy (Phase 1 + Future Phases)

### Current Phase 1 Deliverable: 62 tables across 6 schemas

**v4 Release includes:**
- Master: 17 tables (lookups, translations, templates, audit)
- Core: 18 tables (tenants, business entities, shared infrastructure)
- Transaction: 8 tables (orders, invoices, payments, purchase orders)
- Report: 5 tables (reports, dashboards)
- Auth: 13 tables (users, roles, permissions, auth logs)
- Sales: 1 table (SalesOrders)

### Future Enhancement Phases (DEFER AFTER v4 RELEASE)

**Phase 1+ (v4.1 release, optional):** Add 8 critical missing tables
- Workflows, WorkflowSteps, WorkflowInstances, WorkflowApprovals (Core/Transaction)
- Notifications inbox table (Auth)
- Logs table for app event logging (Auth)
- ApiKeys for service authentication (Auth)
- AuditTrail for column-level change tracking (Core)
- Total: 70 tables

**Phase 2+ (v4.2 release, if needed):** Add 13 recommended e-commerce/CRM tables
- Wishlists, Reviews, Coupons, CouponUsage, Bundles, BundleItems (Core)
- ShippingMethods, ShippingRates, WarehouseLocations (Master/Core)
- StockMovements, StockAdjustments (Transaction)
- Queues, QueueItems (Core)
- Total: 83 tables

**Phase 3+ (v4.3+, industry-specific):** Add 14 optional tables per domain
- CRM: Contacts, Agents, Territories, Opportunities
- Accounting: GlAccounts, JournalEntries, Adjustments
- Advanced inventory: InventoryAllocations, ReturnAuthorizations, PriceListVersions
- Others: Subscriptions, ServiceAgreements, AccountHierarchy

### Recommendation

**v4.0 (this project):** Deploy with 62 tables — covers ~80% of enterprise apps
**v4.1 (next sprint):** If workflows are critical, add 8 Phase 1+ tables → 70 tables
**v4.2+ (future):** Add domain-specific tables as needed per project

This staged approach ensures v4 launches on time while keeping future extensibility path clear.

---

## 7. Dapper Integration Strategy

### When to Use Dapper

**Trigger:** Performance profiling (Phase 3.3.4) identifies queries with >200ms latency or throughput <100 req/sec.

**Candidates:**
- **Transaction schema:** Complex ORDER queries with JOINs to OrderLines, Products, Customers, Invoices (hand-written SQL with proper indexes beats EF Core lazy loading)
- **Report schema:** Aggregation queries (GROUP BY, window functions, SUM/COUNT across large result sets)
- **Auth schema:** User login query with roles/permissions (currently N+1 problem)

### Implementation Pattern

```csharp
// Create IDapperRepository<T> alongside IRepository<T>
public interface IDapperRepository<T> where T : class
{
    Task<IEnumerable<T>> QueryAsync(string sql, object parameters);
    Task<T> QuerySingleAsync(string sql, object parameters);
    Task<int> ExecuteAsync(string sql, object parameters);
}

// Implement for specific high-throughput queries
public class OrderDapperRepository : IDapperRepository<Order>
{
    // Hand-optimized SQL for:
    // - GetOrdersWithDetails (joins OrderLines, Products, Customers)
    // - GetOrdersByCustomer (indexed queries)
    // - GetOrderSummary (aggregations)
}

// Inject alongside EF Repository
services.AddScoped<IRepository<Order>, OrderRepository>();
services.AddScoped<IDapperRepository<Order>, OrderDapperRepository>();

// Use in service based on query type
public class OrderService
{
    private readonly IRepository<Order> _efRepo;
    private readonly IDapperRepository<Order> _dapperRepo;

    public async Task<Order> GetOrderAsync(Guid id)
    {
        // Simple query → EF Core
        return await _efRepo.GetByIdAsync(id);
    }

    public async Task<IEnumerable<OrderWithDetails>> GetOrdersAsync(OrderFilter filter)
    {
        // Complex query → Dapper
        return await _dapperRepo.QueryAsync(
            "SELECT o.*, ol.*, p.*, c.* FROM core.Orders o ...",
            new { CustomerId = filter.CustomerId }
        );
    }
}
```

### When NOT to Use Dapper

- Simple CRUD (Create, Read by ID, soft delete) → EF Core is fine
- Single table queries without JOINs → EF Core is cleaner
- Frequent schema changes → EF Core's abstraction is safer
- When team comfort with raw SQL is low → EF Core is better for maintainability

### Recommendation

**Phase 1:** Deploy with 100% EF Core. No Dapper.
**Phase 3:** Profile during testing (3.3.4). If latency >200ms or throughput <100 req/sec on any query, implement Dapper for that specific query.
**Phase 3.4:** Optional Dapper integration only if profiling shows need.

---

## 8. Database Migration Strategy (v1 → v4)

### Option A: Fresh v4 Install (Recommended for MVP)

**When:** New projects, new tenants, or small v1 databases

**Steps:**
1. Backup existing v1 database
2. Run v4 database scripts (001-009) to create new v4 database
3. Optionally migrate v1 data (via custom scripts if needed)
4. Point app to v4 connection string
5. Deploy

**Effort:** Phase 1 + manual data migration (1-2 hours if needed)
**Risk:** Low — v4 is fresh, no v1 baggage

---

### Option B: In-Place Migration (v1 → v4, for existing users)

**When:** Existing v1 users want to upgrade

**Pre-migration:**
1. Backup v1 database
2. Run `database/migrations/pre-migration.sql` (create v4 schemas in parallel, validate data)

**Migration:**
1. Run `database/migrations/migrate_v1_to_v4.sql`
   - Map v1 tables to v4 tables
   - Transform v1 Users → v4 Users + UserProfiles
   - Transform v1 LovItems → v4 Lookups + Categories
   - Transform v1 SettingValues → v4 TenantSettings
   - Handle data type changes (NVARCHAR(128) PK → UNIQUEIDENTIFIER)

**Post-migration:**
1. Run `database/migrations/post-migration.sql` (cleanup, verify integrity, update sequences)
2. Update app code to use v4 services
3. Test thoroughly with v1 data

**Effort:** 3-4 hours prep + migration scripts + testing
**Risk:** Medium — data transformation, rollback plan needed

**Rollback plan:**
- Keep v1 database backup
- Keep v1 code branch
- Test rollback procedure before production migration

---

## 9. Testing Strategy

### Unit Tests (Phase 1 + 3)

**Coverage:**
- Repository layer: CRUD operations, filtering, pagination
- Service layer: business logic, validation, error handling
- Domain entities: property validation, navigation relationships

**Tools:** xUnit + Moq
**Effort:** 5-6 hours (included in Phase 1.2-1.4)
**Target:** 70%+ code coverage on services

---

### Integration Tests (Phase 3.3)

**Coverage:**
- Controllers: POST/GET/PUT/DELETE endpoints
- Admin workflows: Settings management, LoV management, Identity management
- Business entity workflows: Create order → Create invoice → Create payment
- Auth flows: Login → Logout → Refresh token

**Tools:** xUnit + TestContainers (SQL Server in Docker)
**Effort:** Phase 3.3.1-2 (4 hours)
**Target:** All happy paths + error cases

---

### Performance Tests (Phase 3.3)

**Coverage:**
- Query latency: Simple queries <50ms, complex queries <200ms
- Throughput: API supports 100+ concurrent requests
- Memory: No memory leaks with 1000+ iterations
- Database: Index utilization, query plans

**Tools:** Apache JMeter or custom load test
**Effort:** Phase 3.3.4-5 (3 hours)
**Target:** Identify Dapper candidates

---

### Regression Tests (Phase 3.3)

**Coverage:**
- All existing Admin views work with v4 entities
- All existing Admin controllers return correct responses
- All existing workflows (Settings, LoV, Identity) work end-to-end

**Tools:** Manual + Playwright (optional)
**Effort:** Phase 3.3.3 (2 hours)
**Target:** 100% existing functionality preserved

---

## 10. Success Criteria & Sign-Off

### Phase 1 Sign-Off
- ✅ All 62 tables created and seeded
- ✅ All 62 entities created with proper relationships
- ✅ All 4 DbContexts created and wired
- ✅ All 5 services created with business logic
- ✅ REST API with 20+ endpoints working
- ✅ JWT authentication working (login returns token, protected endpoints enforce [Authorize])
- ✅ Database integration tests passing (70%+ code coverage)
- ✅ Health check endpoint returns database: ok

### Phase 2 Sign-Off
- ✅ Root directory cleaned (old docs moved to docs/old/)
- ✅ README.md updated for v4
- ✅ CHANGELOG.md documents v1→v4 migration
- ✅ docs/architecture.md shows 6-schema diagram
- ✅ docs/DATABASE.md lists all 62 tables
- ✅ docs/API.md documents all 20+ endpoints

### Phase 3 Sign-Off
- ✅ All Admin MVC views updated and tested
- ✅ All Admin controllers wired to real services
- ✅ New business entity controllers created (Products, Customers, Orders)
- ✅ Integration tests passing
- ✅ Manual regression testing completed (all Admin features work)
- ✅ Performance profiling completed (Dapper candidates identified, if any)

### Phase 4 Sign-Off
- ✅ Swagger UI works, all endpoints documented
- ✅ API versioning (/api/v1/) working
- ✅ Rate limiting configured (100 req/min default)
- ✅ CORS configured for Blazor & MAUI
- ✅ Global error handling middleware working
- ✅ Security hardening complete (HTTPS, HSTS, antiforgery)
- ✅ Health checks working
- ✅ Load test passed (100+ concurrent requests)

---

## 11. Risk Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|-----------|
| EF Core DbContext complexity (4 contexts, 1 DB) | MEDIUM | MEDIUM | Document DbContext setup, create unit tests for each context |
| HierarchyId tree queries slow | MEDIUM | LOW | Index HierarchyId paths, test tree queries in performance profiling |
| Polymorphic queries (EntityType+EntityId) slow | LOW | LOW | Add indexes on EntityType+EntityId, test in Phase 3.3.4 |
| v1→v4 data migration fails | HIGH | MEDIUM | Backup v1 database, test migration scripts on copy, rollback procedure |
| API breaks existing MVC functionality | HIGH | MEDIUM | Phase 3 regression testing, test both MVC + API simultaneously |
| Performance degrades under load | MEDIUM | MEDIUM | Phase 3.3 load testing, Dapper integration for slow queries |
| JWT secret exposed in code | HIGH | MEDIUM | Move to appsettings.json + Azure Key Vault in prod, rotate regularly |

---

## 12. Timeline & Effort Summary

| Phase | Duration | Effort | Team |
|-------|----------|--------|------|
| Phase 1: Foundation | Week 1-2 | 41.5-48h | Backend (1-2 devs) |
| Phase 2: Docs | Week 2 (parallel) | 8h | Tech writer or senior dev |
| Phase 3: MVC Integration | Week 2-3 | 20.5-30h | Full-stack dev + QA |
| Phase 4: Polish | Week 4 | 10-15h | Backend dev |
| **Total** | **4 weeks** | **90-120h** | **1-3 devs** |

**Recommended team:**
- 1 backend dev (Phases 1, 4) — database, EF Core, services, API
- 1 full-stack dev (Phase 3) — MVC views/controllers, testing
- 1 tech writer or senior dev (Phase 2) — documentation
- Optional: QA tester (Phase 3 regression testing)

---

## 13. Appendix: Deferrable Features

**NOT in Phase 1-4 (for future releases):**
- ❌ Workflows & approvals (Phase 1+ schema addition)
- ❌ Advanced CRM (Contacts, Leads, Opportunities)
- ❌ E-commerce specifics (Wishlists, Coupons, Bundles)
- ❌ Advanced inventory (Stock movements, warehouse locations)
- ❌ Advanced reporting (Custom dashboards, scheduled reports)
- ❌ Advanced notification (Push, SMS, WhatsApp delivery)
- ❌ Subscription billing (Recurring charges, usage-based billing)
- ❌ Accounting GL integration (Journal entries, general ledger)

These can be added in v4.1+, v4.2+, v4.3+ releases as needed.

---

## Summary

**v4 is a complete upgrade from v1, delivering:**

✅ **Phase 1 (Foundation):** v4 production-ready with 62-table database, real EF Core integration, REST API for multi-client support
✅ **Phase 2 (Documentation):** Clean root directory, comprehensive v4 docs, migration guide from v1
✅ **Phase 3 (MVC Integration):** Existing MVC Admin fully functional with v4 database, regression tested
✅ **Phase 4 (Polish):** Swagger documentation, API versioning, rate limiting, security hardening

**Total effort: 90-120 hours (~4 weeks with 1-3 developers)**

**Go-live readiness:** After Phase 1 for API clients (Blazor, MAUI); after Phase 3 for MVC Admin clients.

---

**Document Version:** 1.0
**Date:** 2026-03-31
**Status:** Ready for Implementation
**Author:** SmartWorkz Architecture Team
