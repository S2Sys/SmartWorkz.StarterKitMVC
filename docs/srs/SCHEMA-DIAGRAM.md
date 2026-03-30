# SmartWorkz v4 - Schema Relationship Diagrams

**Date:** 2026-03-31
**Purpose:** Visual representation of key relationships across schemas

---

## 1. Multi-Schema Overview

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    StarterKitMVC Database (SQL Server)                   │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │   MASTER    │  │    CORE     │  │ TRANSACTION  │  │    REPORT    │ │
│  │  (17 tbl)   │  │  (18 tbl)   │  │   (8 tbl)    │  │   (5 tbl)    │ │
│  │             │  │             │  │              │  │              │ │
│  │ Reference   │  │ Config +    │  │ Financial    │  │ Reporting    │ │
│  │ Data        │  │ Business    │  │ Data         │  │ Data         │ │
│  │             │  │             │  │              │  │              │ │
│  │ TenantId:   │  │ TenantId:   │  │ TenantId:    │  │ TenantId:    │ │
│  │ NULLABLE    │  │ NOT NULL    │  │ NOT NULL     │  │ NULLABLE     │ │
│  └─────────────┘  └─────────────┘  └──────────────┘  └──────────────┘ │
│         △               △                 △                  △          │
│         │ (seed data)   │ (FK)            │ (FK)             │ (FK)    │
│         └───────────────┴─────────────────┴──────────────────┘          │
│                                                                          │
│  ┌─────────────┐  ┌──────────────┐                                      │
│  │    AUTH     │  │    SALES     │                                      │
│  │  (13 tbl)   │  │   (1 tbl)    │                                      │
│  │             │  │              │                                      │
│  │ Identity +  │  │ Team-scoped  │                                      │
│  │ RBAC +      │  │ (extensible) │                                      │
│  │ Logs        │  │              │                                      │
│  │             │  │ TenantId:    │                                      │
│  │ TenantId:   │  │ NOT NULL     │                                      │
│  │ NOT NULL    │  │              │                                      │
│  └─────────────┘  └──────────────┘                                      │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Master Schema (Reference Data)

```
Master Schema - Reference Data (TenantId NULLABLE)
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│  Countries ──────┬──→ States ──────┬──→ Cities                     │
│  (3 tables)      │     (geo)       │     (geo)                    │
│                  │                 │                              │
│                  └─────────────────┘                              │
│                                                                     │
│  Languages ────→ Translations  (single table for ALL i18n)        │
│  (2 tables)      ├─ Resource keys (null EntityType)              │
│                  ├─ Entity translations (EntityType+EntityId)    │
│                  └─ Tenant overrides (TenantId)                  │
│                                                                     │
│  Lookups (HierarchyId) ──┐                                        │
│     ├─ StatusCodes      │                                        │
│     ├─ Currencies       │  Hierarchical Reference Data           │
│     └─ JobTitles        │  (4 tables)                           │
│                          │                                        │
│  Categories (HierarchyId)┤  • Tree structure (/1/2/3/)          │
│     ├─ ProductCategory  │  • Multi-level nesting               │
│     └─ BlogCategory     │  • Flexible hierarchy                │
│                          │                                        │
│  EntityStates (HierarchyId)┐                                      │
│     ├─ Order states     │  State Machine Definitions            │
│     ├─ Invoice states   │  (2 tables)                           │
│     └─ ...             │                                        │
│                                                                     │
│  EntityStateTransitions ──→ (who can transition from → to)      │
│                                                                     │
│  NotificationChannels ──┐                                        │
│  TemplateGroups ────────┼─→ Templates (multi-channel)           │
│  (3 tables)             │     (Email, SMS, WhatsApp, Push, InApp) │
│                         │     (multi-language)                  │
│                         │     (global + tenant overrides)       │
│                         └─                                       │
│                                                                     │
│  SubscriptionPlans ──┐                                           │
│  PreferenceDefinitions┤  SaaS & Configuration (2 tables)        │
│                       └─                                        │
│                                                                     │
│  SeoMeta ──┐                                                      │
│  UrlRedirects┴─ SEO Management (2 tables)                      │
│                                                                     │
│  AuditLogs ──┐                                                    │
│  ActivityLogs┴─ Logging (2 tables)                              │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 3. Core Schema (Configuration + Business + Shared Infrastructure)

```
Core Schema - Multi-Tenant (TenantId NOT NULL)
┌──────────────────────────────────────────────────────────────────────┐
│                                                                      │
│  CONFIG & TENANTS                                                   │
│  ┌──────────────────────────────────────────┐                       │
│  │                                          │                       │
│  │  Tenants (HierarchyId)                  │  4 config tables      │
│  │   ├─ Agency (root)                      │  ┌─────────────────┐ │
│  │   ├─ Client (parent)                    │  │ Tenant settings │ │
│  │   └─ Sub-client (child)                 │  │ Feature flags   │ │
│  │                                          │  │ Subscriptions   │ │
│  │  TenantSubscriptions (FK→SubscriptionPlans from Master)         │
│  │  TenantSettings (key-value store)       │  └─────────────────┘ │
│  │  FeatureFlags (per-tenant feature toggles)                      │
│  │                                          │                       │
│  └──────────────────────────────────────────┘                       │
│                                                                      │
│  BUSINESS ENTITIES (Dummy placeholders for starter kit)            │
│  ┌──────────────────────────────────────────┐                       │
│  │ Products ──→ (Category from Master)      │  9 business           │
│  │ Customers                                │  entity tables        │
│  │ Vendors                                  │  (extend as needed)   │
│  │ Projects ──→ Customers                   │  ┌─────────────────┐ │
│  │ Teams, Departments, Employees (HR)       │  │ Extend w/your   │ │
│  │ Assets ──→ Employees                     │  │ own entities    │ │
│  │ Contracts ──→ Vendors/Customers          │  │                 │ │
│  │                                          │  │ Pattern:        │ │
│  └──────────────────────────────────────────┘  │ Code+Name+     │ │
│                                                 │ Status+TenantId │ │
│  POLYMORPHIC SHARED INFRASTRUCTURE             └─────────────────┘ │
│  ┌──────────────────────────────────────────┐                       │
│  │                                          │  All use EntityType   │
│  │  Addresses                               │  + EntityId pattern   │
│  │  (links to Customer, Vendor,             │  (5 tables)          │
│  │   Employee, Tenant, etc.)                │  ┌─────────────────┐ │
│  │                                          │  │ Can attach to   │ │
│  │  Attachments                             │  │ ANY entity      │ │
│  │  (links to Order, Invoice,               │  │ without schema  │ │
│  │   Project, Contract, etc.)               │  │ changes         │ │
│  │                                          │  │                 │ │
│  │  Tags                                    │  │ Single table    │ │
│  │  (tag any entity)                        │  │ instead of:     │ │
│  │                                          │  │ - OrderAddresses│ │
│  │  Comments                                │  │ - ProductTags   │ │
│  │  (comment on any entity)                 │  │ - CustomerComts │ │
│  │                                          │  └─────────────────┘ │
│  │  StateHistory                            │                       │
│  │  (track state changes for any entity)    │                       │
│  │                                          │                       │
│  └──────────────────────────────────────────┘                       │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘

Example: One Product with polymorphic links
┌─ Product (Id=ABC)
│  ├─ Address (EntityType=Product, EntityId=ABC) → Warehouse location
│  ├─ Attachment (EntityType=Product, EntityId=ABC) → Datasheet PDF
│  ├─ Tag (EntityType=Product, EntityId=ABC) → "Featured", "OnSale"
│  ├─ Comment (EntityType=Product, EntityId=ABC) → "Need restock"
│  └─ StateHistory (EntityType=Product, EntityId=ABC) → Draft → Active
```

---

## 4. Transaction Schema (Financial & Operational Data)

```
Transaction Schema - Multi-Tenant Transactional Data (TenantId NOT NULL)
┌──────────────────────────────────────────────────────────────────────┐
│                                                                      │
│  SALES CYCLE                                                        │
│  ┌────────────────────────────────────────┐                         │
│  │                                        │                         │
│  │  Orders (from Core.Customers)          │  Order & Invoice        │
│  │    ├─ OrderLines (from Core.Products) │  (parent-child)         │
│  │    └─ (linked via Attachments, Tags)  │  ┌──────────────────┐   │
│  │                                        │  │ Orders + Lines   │   │
│  │  Invoices (from Core.Customers)        │  │ Invoices         │   │
│  │    └─ (linked from Orders, optional)  │  │ Payments         │   │
│  │                                        │  └──────────────────┘   │
│  │  Payments (from Invoices)              │                         │
│  │    └─ Track payment status             │                         │
│  │                                        │                         │
│  └────────────────────────────────────────┘                         │
│                                                                      │
│  PURCHASING CYCLE                                                   │
│  ┌────────────────────────────────────────┐                         │
│  │                                        │  PO & Receipt           │
│  │  PurchaseOrders (from Core.Vendors)    │  (parent-child)         │
│  │    ├─ PurchaseOrderLines               │  ┌──────────────────┐   │
│  │    │   (from Core.Products)            │  │ POs + Lines      │   │
│  │    └─ (linked via Attachments, Tags)  │  │ Receipts         │   │
│  │                                        │  │ CreditNotes      │   │
│  │  Receipts (from PurchaseOrders)        │  └──────────────────┘   │
│  │    └─ Track goods received             │                         │
│  │                                        │                         │
│  └────────────────────────────────────────┘                         │
│                                                                      │
│  ADJUSTMENTS                                                        │
│  ┌────────────────────────────────────────┐                         │
│  │                                        │                         │
│  │  CreditNotes (from Invoices)           │  Adjust invoices for    │
│  │    └─ Returns, discounts, adjustments  │  returns, refunds,      │
│  │                                        │  price changes          │
│  │                                        │                         │
│  └────────────────────────────────────────┘                         │
│                                                                      │
│  RELATIONSHIP FLOW:                                                 │
│                                                                      │
│  Core.Customers ──→ Orders ──→ OrderLines ──→ Core.Products       │
│                        │                                            │
│                        └──→ Invoices ──→ Payments                  │
│                                           (Status: Paid/Pending)    │
│                                                                      │
│  Core.Vendors ──→ PurchaseOrders ──→ PurchaseOrderLines ──→ Core.Products
│                        │                                            │
│                        └──→ Receipts                               │
│                                                                      │
│  All linked via Attachments, Tags, Comments via polymorphic tables │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 5. Report Schema (Reporting Infrastructure)

```
Report Schema - Reporting & Dashboards (TenantId NULLABLE)
┌──────────────────────────────────────────────────────────┐
│                                                          │
│  ReportDefinitions                                       │
│  (Define report structure)                               │
│  ├─ QuerySql (raw SQL or stored proc)                  │
│  ├─ Parameters (JSON schema)                           │
│  └─ TenantId: nullable                                 │
│                                                          │
│  ReportSchedules                                         │
│  (Schedule report runs)                                  │
│  ├─ FK→ ReportDefinitions                              │
│  ├─ CronExpression ("0 9 * * *" = daily at 9am)       │
│  ├─ EmailRecipients (JSON)                             │
│  └─ Auto-generate & email daily/weekly/monthly         │
│                                                          │
│  ReportResults                                           │
│  (Cache report data)                                     │
│  ├─ FK→ ReportDefinitions                              │
│  ├─ ResultData (JSON)                                  │
│  ├─ ExecutionTimeMs                                    │
│  └─ Status (Success/Failed/Timeout)                    │
│                                                          │
│  ReportAuditLogs                                         │
│  (Track who ran what)                                    │
│  ├─ FK→ ReportDefinitions                              │
│  ├─ RunBy (User)                                       │
│  ├─ ParameterValues (JSON)                             │
│  └─ ExportFormat (Excel/Pdf/Csv/Json)                  │
│                                                          │
│  DashboardWidgets                                        │
│  (Reusable dashboard components)                         │
│  ├─ WidgetType (Chart, KPI, Table, Gauge, Trend)      │
│  ├─ Query (SQL or API)                                 │
│  ├─ RefreshIntervalSeconds                             │
│  └─ Configuration (JSON)                               │
│                                                          │
└──────────────────────────────────────────────────────────┘

Usage:
1. Define a report: SELECT * FROM Orders WHERE CreatedAt > @StartDate
2. Schedule it: Run daily at 9am, email to managers@company.com
3. Cache results: Store in ReportResults for fast retrieval
4. Audit access: Track who accessed what reports when
5. Build dashboards: Combine widgets into customizable dashboards
```

---

## 6. Auth Schema (Identity & RBAC)

```
Auth Schema - Identity, RBAC, Sessions, Logs (TenantId NOT NULL)
┌──────────────────────────────────────────────────────────┐
│                                                          │
│  USER IDENTITY                                           │
│  ┌───────────────────────────────────────────┐          │
│  │ Users                                     │          │
│  │  ├─ Email (unique)                       │          │
│  │  ├─ PasswordHash + PasswordSalt          │          │
│  │  ├─ MfaEnabled + MfaSecret (TOTP)       │          │
│  │  ├─ EmailConfirmed, PhoneConfirmed      │          │
│  │  └─ AccountStatus (Active/Locked)       │          │
│  │                                          │          │
│  │ UserProfiles (split for flexibility)     │          │
│  │  ├─ FirstName, LastName, DateOfBirth   │          │
│  │  ├─ ProfilePictureUrl                   │          │
│  │  ├─ Department, JobTitle, Manager       │          │
│  │  └─ NotifyEmail, NotifySms, etc.       │          │
│  │                                          │          │
│  │ UserPreferences (key-value)              │          │
│  │  ├─ Theme (Light/Dark)                  │          │
│  │  ├─ Language (en-US, es-ES, etc.)      │          │
│  │  ├─ TimeZone                            │          │
│  │  └─ NotificationFrequency                │          │
│  │                                          │          │
│  └───────────────────────────────────────────┘          │
│                                                          │
│  ROLE-BASED ACCESS CONTROL (RBAC)                      │
│  ┌───────────────────────────────────────────┐          │
│  │ Roles ──→ RolePermissions ──→ Permissions │          │
│  │  (Admin,  (junction)         (Module.     │          │
│  │   Manager,                    Action)     │          │
│  │   User)                                   │          │
│  │    ↓                                      │          │
│  │ UserRoles (Users have multiple roles)    │          │
│  │                                          │          │
│  │ Example:                                  │          │
│  │  Role "Manager"                          │          │
│  │    ├─ Permission "Orders.View"           │          │
│  │    ├─ Permission "Orders.Create"         │          │
│  │    ├─ Permission "Orders.Approve"        │          │
│  │    └─ Permission "Reports.Export"        │          │
│  │                                          │          │
│  │  User "john@company.com"                │          │
│  │    ├─ Role "Manager"                     │          │
│  │    ├─ Role "Approver"                    │          │
│  │    └─ Gets all permissions from both    │          │
│  │                                          │          │
│  └───────────────────────────────────────────┘          │
│                                                          │
│  SESSIONS & TOKENS                                       │
│  ┌───────────────────────────────────────────┐          │
│  │ RefreshTokens (long-lived tokens)        │          │
│  │  ├─ Token (hashed JWT)                   │          │
│  │  ├─ ExpiryDate                           │          │
│  │  ├─ DeviceInfo (audit trail)             │          │
│  │  ├─ IpAddress                            │          │
│  │  └─ IsRevoked (logout per device)        │          │
│  │                                          │          │
│  │ VerificationCodes (OTP & codes)          │          │
│  │  ├─ CodeType (EmailVerify, PasswordReset)│          │
│  │  ├─ Code (6-digit OTP)                   │          │
│  │  ├─ ExpiryDate                           │          │
│  │  └─ IsUsed, UsedAt                       │          │
│  │                                          │          │
│  │ ExternalLogins (OAuth linking)           │          │
│  │  ├─ Provider (Google, Microsoft, GitHub) │          │
│  │  ├─ ProviderUserId                       │          │
│  │  └─ LinkedAt                             │          │
│  │                                          │          │
│  └───────────────────────────────────────────┘          │
│                                                          │
│  LOGGING                                                │
│  ┌───────────────────────────────────────────┐          │
│  │ AuditLogs (auth-specific events)         │          │
│  │  ├─ EventType (Login, Logout, ChangePassword)       │
│  │  ├─ IpAddress, UserAgent                │          │
│  │  └─ Timestamp                           │          │
│  │                                          │          │
│  │ ActivityLogs (usage tracking)            │          │
│  │  ├─ ActivityType (PageView, DataExport, etc.)      │
│  │  └─ ResourceType + ResourceId            │          │
│  │                                          │          │
│  │ NotificationLogs (delivery tracking)    │          │
│  │  ├─ Channel (Email, SMS, Push, etc.)    │          │
│  │  ├─ Status (Queued, Sent, Delivered)    │          │
│  │  ├─ ProviderReference (SES message ID)  │          │
│  │  └─ SentAt, DeliveredAt                 │          │
│  │                                          │          │
│  └───────────────────────────────────────────┘          │
│                                                          │
└──────────────────────────────────────────────────────────┘

Authentication Flow:
1. User logs in → UserVerify in Users table
2. Create JWT access token + RefreshToken
3. Store RefreshToken in RefreshTokens table (with device info)
4. On token expiry → Use RefreshToken to get new access token
5. On logout → Revoke RefreshToken (IsRevoked=1)
6. Track activity in AuditLogs & ActivityLogs
```

---

## 7. Sales Schema (Team-Specific Example)

```
Sales Schema - Team Namespace (TenantId NOT NULL)
┌────────────────────────────────────────────┐
│                                            │
│  SalesOrders (Sales pipeline)              │
│  (different from Transaction.Orders)       │
│  ├─ FK→ Core.Customers                    │
│  ├─ FK→ Core.Products (optional)           │
│  ├─ SalesPersonId → Auth.Users            │
│  ├─ Status (Pipeline, Qualified, Won)    │
│  ├─ Amount (estimated revenue)            │
│  ├─ Probability (0-100% for forecasting)  │
│  └─ Notes (deal stage, next steps)        │
│                                            │
│  vs. Transaction.Orders:                  │
│  • SalesOrders = deals in progress        │
│  • Transaction.Orders = confirmed sales   │
│  • (both exist simultaneously)             │
│                                            │
│  EXTENSIBLE:                               │
│  Projects can add:                        │
│  • Marketing schema (Campaigns, Leads)    │
│  • HR schema (Employees, Payroll)         │
│  • Finance schema (GL, JournalEntries)    │
│  • Each gets its own namespace            │
│  • All use same auth.Users & core.Tenants│
│                                            │
└────────────────────────────────────────────┘
```

---

## 8. Key Patterns Summary

### Polymorphic Linking (Shared Infrastructure Pattern)

```
BEFORE (v1 - rigid schema):
┌─ OrderAddresses
├─ InvoiceAddresses
├─ CustomerAddresses
├─ VendorAddresses
├─ EmployeeAddresses
└─ ... (separate table per entity type)

AFTER (v4 - flexible schema):
┌─ Addresses (single table)
├─ EntityType='Order', EntityId=123
├─ EntityType='Invoice', EntityId=456
├─ EntityType='Customer', EntityId=789
├─ EntityType='Vendor', EntityId=101
└─ EntityType='Employee', EntityId=202
   (no schema changes needed for new entity types!)
```

### HierarchyId Trees (Unlimited Nesting)

```
Before: Flat structure with ParentId foreign key
Tenants
├─ Id=1, Code='acme', ParentId=null
├─ Id=2, Code='acme-us', ParentId=1
├─ Id=3, Code='acme-us-ca', ParentId=2
└─ (hard to query ancestors/descendants)

After: HierarchyId tree paths
Tenants
├─ NodePath=/1/, Code='acme' (root)
├─ NodePath=/1/1/, Code='acme-us' (child)
├─ NodePath=/1/1/1/, Code='acme-us-ca' (grandchild)
└─ (efficient ancestor/descendant queries: WHERE NodePath.IsDescendantOf('/1/'))
```

### Soft Delete Pattern (Reversible)

```
Old way (hard delete):
DELETE FROM Products WHERE Id=123
→ Data is gone forever, referential integrity breaks

New way (soft delete):
UPDATE Products SET IsDeleted=1, DeletedAt=SYSUTCDATETIME(), DeletedBy=@userId
WHERE Id=123
→ Data preserved, can restore, audit trail intact
→ Queries filter: WHERE IsDeleted=0 by default
```

---

## 9. Tenant Isolation Strategy

### Multi-Tenancy Patterns

```
Master Schema (TenantId NULLABLE):
┌─ NULL = Global (default/fallback)
│  Example: Countries (everyone uses same countries)
├─ 'tenant-a' = Tenant A override
│  Example: Translations with tenant-specific terminology
└─ 'tenant-b' = Tenant B override

All other schemas (TenantId NOT NULL):
┌─ 'tenant-a' = Only Tenant A's data
├─ 'tenant-b' = Only Tenant B's data
└─ Data is row-level partitioned by TenantId
   (cannot see each other's data unless explicitly joined)

Row-Level Isolation Pattern (in service layer):
WHERE TenantId = @currentTenantId
  AND IsDeleted = 0

Every query automatically filters to current tenant + non-deleted rows.
```

---

## Summary

**62 Total Tables:**
- Master (17): Reference data, TenantId nullable
- Core (18): Config + business entities, TenantId not null
- Transaction (8): Orders, invoices, payments, TenantId not null
- Report (5): Reporting infrastructure, TenantId nullable
- Auth (13): Users, roles, permissions, logs, TenantId not null
- Sales (1): Example team schema, TenantId not null

**Key Patterns:**
✅ Polymorphic linking (Addresses, Tags, Comments, Attachments)
✅ HierarchyId trees (Tenants, Lookups, Categories, EntityStates)
✅ Soft delete (IsDeleted, DeletedAt, DeletedBy)
✅ Audit columns (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
✅ Multi-tenancy (TenantId partitioning)
✅ State machines (EntityStates + StateHistory)
✅ Template system (multi-channel, multi-language)
✅ Flexible config (TenantSettings key-value, FeatureFlags)

---

**All schemas reviewed and ready for SQL script generation!**
