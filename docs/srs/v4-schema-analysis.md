# SmartWorkz StarterKit v4 - Schema Analysis & Enhancement

**Date:** 2026-03-31
**Purpose:** Review v4 schema for generic enterprise coverage and identify missing tables

---

## 1. Current v4 Schema Coverage (62 tables across 6 schemas)

### Master Schema (17 tables)
✅ Countries, States, Cities, Languages, Translations, Lookups, Categories, EntityStates, EntityStateTransitions, NotificationChannels, TemplateGroups, Templates, SubscriptionPlans, PreferenceDefinitions, SeoMeta, UrlRedirects, AuditLogs, ActivityLogs

### Core Schema (18 tables)
✅ Tenants, TenantSubscriptions, TenantSettings, FeatureFlags, Products, Customers, Vendors, Projects, Teams, Departments, Employees, Assets, Contracts, Addresses, Attachments, Tags, Comments, StateHistory

### Transaction Schema (8 tables)
✅ Orders, OrderLines, Invoices, Payments, PurchaseOrders, PurchaseOrderLines, Receipts, CreditNotes

### Report Schema (5 tables)
✅ ReportDefinitions, ReportSchedules, ReportResults, ReportAuditLogs, DashboardWidgets

### Auth Schema (13 tables)
✅ Users, UserProfiles, UserPreferences, Roles, Permissions, RolePermissions, UserRoles, RefreshTokens, VerificationCodes, ExternalLogins, AuditLogs, ActivityLogs, NotificationLogs

### Sales Schema (1 table)
✅ SalesOrders

---

## 2. Missing Tables for Generic Enterprise Apps

### Category A: Critical (Add to existing schemas)

| Table | Schema | Purpose | Justification |
|-------|--------|---------|---------------|
| `Workflows` | Core | Workflow definitions & instances | State machine beyond entity states |
| `WorkflowSteps` | Core | Workflow step definitions | Sequence of approvals/actions |
| `WorkflowInstances` | Transaction | Active/completed workflow runs | Track workflow progress per entity |
| `WorkflowApprovals` | Transaction | Approval records per step | Who approved what, when |
| `Notifications` | Auth | User notification inbox | Different from NotificationLogs (delivery) |
| `Logs` | Auth | Application event logs (non-audit) | Distinct from AuditLogs (higher volume) |
| `ApiKeys` | Auth | API authentication keys | For service-to-service auth (not users) |
| `AuditTrail` | Core | Detailed column-level change tracking | Who changed what field from/to value |
| `DocumentTypes` | Core | Type taxonomy for documents | Bills, Invoices, POs, etc. |
| `Documents` | Transaction | Document records with versioning | Polymorphic - can link to any entity |
| `DocumentVersions` | Transaction | Version history for documents | Track document evolution |
| `BulkOperations` | Transaction | Bulk import/export jobs | Progress tracking for large ops |
| `Enquiries` | Core | Customer inquiry/RFQ records | Pre-order communication |
| `Leads` | Core | Sales leads (CRM) | Before they become customers |
| `Activities` | Core | User activities (calls, emails, meetings) | CRM activity tracking |

### Category B: Highly Recommended (Add)

| Table | Schema | Purpose | Justification |
|-------|--------|---------|---------------|
| `Wishlists` | Core | Customer saved items | E-commerce common feature |
| `Reviews` | Core | Product/entity reviews & ratings | Customer feedback |
| `Coupons` | Core | Discount codes & coupons | Promotional campaigns |
| `CouponUsage` | Transaction | Coupon redemption tracking | Link to orders, promotions |
| `Bundles` | Core | Product bundle definitions | Group products for selling |
| `BundleItems` | Core | Items in a bundle | Foreign keys to Products |
| `ShippingMethods` | Core | Shipping options (Standard, Express) | Selectable at checkout |
| `ShippingRates` | Master | Shipping cost matrix | By country/weight/method |
| `WarehouseLocations` | Core | Warehouse bins/locations | Inventory location codes |
| `StockMovements` | Transaction | Inventory movements in/out | Track stock changes |
| `StockAdjustments` | Transaction | Manual stock corrections | Damage, loss, found items |
| `Queues` | Core | Async job queue entries | Background job tracking |
| `QueueItems` | Core | Messages/items in queue | Retry logic, DLQ |
| `ServiceAgreements` | Core | SLAs, support agreements | Service level definitions |
| `EmailQueue` | Auth | Pending emails to send | Email delivery management |
| `SmsQueue` | Auth | Pending SMS to send | SMS delivery management |

### Category C: Optional (Industry-specific)

| Table | Schema | Purpose | Justification |
|-------|--------|---------|---------------|
| `Subscriptions` (SaaS) | Transaction | Recurring subscriptions | For subscription billing |
| `Invoices` variant | Transaction | Subscription invoice generation | Auto-bill tracking |
| `Agents` (HR/CRM) | Core | Sales agents, support agents | Agent management |
| `Territories` (Sales) | Core | Sales territory definitions | Geographic/account-based |
| `Opportunities` (CRM) | Core | Sales pipeline opportunities | Deal tracking |
| `AccountHierarchy` | Core | Multi-level account structure | Corporate hierarchies |
| `Contacts` (CRM) | Core | Individual contacts per customer | Multiple contacts per company |
| `PriceListVersions` | Master | Price list versioning | Effective date-based pricing |
| `InventoryAllocations` | Transaction | Reserved stock per order | Pre-fulfillment allocation |
| `ReturnAuthorizations` | Transaction | RMA process | Return & refund handling |
| `Adjustments` | Transaction | Debit/credit notes | General ledger integration |
| `GlAccounts` | Core | General ledger account chart | For accounting integration |
| `JournalEntries` | Transaction | GL journal postings | Accounting entries |

---

## 3. Recommended Additions to v4 (Prioritized)

### Phase 1: Core Enterprise (Add immediately)
- `Workflows` + `WorkflowSteps` (Core)
- `WorkflowInstances` + `WorkflowApprovals` (Transaction)
- `Notifications` (Auth) - User inbox (distinct from NotificationLogs)
- `Logs` (Auth) - Application event logging
- `ApiKeys` (Auth) - Service authentication
- `AuditTrail` (Core) - Column-level change tracking
- `DocumentTypes` (Core)
- `Documents` + `DocumentVersions` (Transaction)

**Impact:** +8 tables, covers workflow, document management, advanced auditing, notification delivery

### Phase 2: Commerce & CRM (Add if applicable)
- `Enquiries`, `Leads`, `Activities` (Core)
- `Wishlists`, `Reviews`, `Coupons` (Core)
- `CouponUsage` (Transaction)
- `Bundles`, `BundleItems` (Core)
- `StockMovements`, `StockAdjustments` (Transaction)

**Impact:** +11 tables, covers e-commerce, CRM, inventory management

### Phase 3: Advanced Logistics (Add if needed)
- `ShippingMethods`, `ShippingRates` (Master/Core)
- `WarehouseLocations` (Core)
- `InventoryAllocations`, `ReturnAuthorizations` (Transaction)

**Impact:** +5 tables, covers shipping, warehouse, returns

---

## 4. Enhanced Schema Summary

### Current
- 6 schemas
- 62 tables
- Covers: transactions, users, reports, notifications

### With Phase 1 additions
- 6 schemas
- **70 tables** (+8)
- Adds: workflows, documents, advanced audit, service auth

### With Phase 1 + Phase 2
- 6 schemas
- **81 tables** (+19)
- Adds: e-commerce, CRM, inventory, reviews, coupons

---

## 5. Generic Schema Design Principles (Applied in v4)

### ✅ Already in v4
1. **Polymorphic linking via EntityType+EntityId** (Addresses, Tags, Comments) → any entity can link
2. **HierarchyId trees** (Lookups, Categories, Tenants) → unlimited nesting
3. **TenantId nullable** (Master tables) → global + tenant overrides
4. **Soft delete pattern** (IsDeleted, DeletedAt, DeletedBy) → reversible deletion
5. **Audit columns** (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) → trackable
6. **Status/state machines** (EntityStates, StateHistory) → workflow-ready
7. **Templating system** (Templates, multi-channel) → SMS, Email, Push, WhatsApp
8. **Preferences** (UserPreferences, PreferenceDefinitions) → extensible user config

### ⚠️ Gaps to Address
1. **No column-level change tracking** (only entity-level in AuditLogs) → add AuditTrail
2. **No workflow/approval system** → add Workflows, WorkflowInstances, WorkflowApprovals
3. **No document versioning** → add Documents, DocumentVersions
4. **No service authentication** (only user auth) → add ApiKeys
5. **No application logging** (only audit/activity) → add Logs table
6. **No user notification inbox** (only delivery logs) → add Notifications table

---

## 6. Recommended Final v4 Schema (Enhanced)

### Master Schema (18 tables - add ShippingRates)
Countries, States, Cities, Languages, Translations, Lookups, Categories, EntityStates, EntityStateTransitions, NotificationChannels, TemplateGroups, Templates, SubscriptionPlans, PreferenceDefinitions, SeoMeta, UrlRedirects, **ShippingRates**, AuditLogs, ActivityLogs

### Core Schema (28 tables - add Workflows, Documents, etc.)
Tenants, TenantSubscriptions, TenantSettings, FeatureFlags, Products, Customers, Vendors, Projects, Teams, Departments, Employees, Assets, Contracts, Addresses, Attachments, Tags, Comments, StateHistory, **Workflows, WorkflowSteps, DocumentTypes, Documents, Enquiries, Leads, Activities, Wishlists, Reviews, Coupons, ShippingMethods**

### Transaction Schema (13 tables - add Workflow, Documents, Inventory)
Orders, OrderLines, Invoices, Payments, PurchaseOrders, PurchaseOrderLines, Receipts, CreditNotes, **WorkflowInstances, WorkflowApprovals, DocumentVersions, StockMovements, StockAdjustments**

### Report Schema (5 tables - unchanged)
ReportDefinitions, ReportSchedules, ReportResults, ReportAuditLogs, DashboardWidgets

### Auth Schema (16 tables - add Notifications, Logs, ApiKeys, AuditTrail)
Users, UserProfiles, UserPreferences, Roles, Permissions, RolePermissions, UserRoles, RefreshTokens, VerificationCodes, ExternalLogins, AuditLogs, ActivityLogs, NotificationLogs, **Notifications, Logs, ApiKeys, AuditTrail**

### Sales Schema (1 table - unchanged)
SalesOrders

### **New Total: 81 tables** (from 62)
- Covers ~80% of common enterprise app needs
- Remaining 20% can be added via custom team schemas (like Sales)

---

## 7. Implementation Strategy

### Stage 1: Current (62 tables)
- Launch with current v4 schema
- Sufficient for MVP with Core, Transaction, basic workflows

### Stage 2: Phase 1 (70 tables)
- Add in Sprint 2
- Workflows, Documents, Notifications, ApiKeys, Audit improvements
- Major gap-filler for workflow-heavy apps

### Stage 3: Phase 2 (81 tables)
- Add in Sprint 3-4 if needed
- E-commerce, CRM, advanced inventory
- Choose tables relevant to use case

### Stage 4: Custom team schemas
- Add Sales, Marketing, HR schemas per team
- Each team gets their own schema (scalable model)

---

## 8. What Makes This Schema "Generic"

| Aspect | How It's Generic |
|--------|-----------------|
| **Polymorphism** | EntityType+EntityId allows any table to link Addresses, Tags, Comments, Documents |
| **Hierarchies** | HierarchyId trees in Lookups, Categories, Tenants support unlimited structures |
| **State machines** | EntityStates + StateHistory framework can model any workflow |
| **Templates** | Multi-channel Templates (Email/SMS/WhatsApp/Push) handle all notification types |
| **Preferences** | PreferenceDefinitions + UserPreferences extensible for any user setting |
| **Audit** | AuditLogs + ActivityLogs + (new) AuditTrail cover all change tracking |
| **Tenancy** | TenantId on all tables, nullable in Master for global overrides |
| **Soft delete** | IsDeleted on all business entities enables reversible deletion |
| **Team schemas** | Sales, HR, Marketing can add custom tables in their own schema |

---

## 9. SQL Script Generation Plan

### Current v4 (62 tables)
- `001_CreateSchemas.sql` ✅
- `002_CreateTables_Master.sql` ✅
- `003_CreateTables_Core.sql` (update +10 tables)
- `004_CreateTables_Transaction.sql` (update +5 tables)
- `005_CreateTables_Report.sql` ✅
- `006_CreateTables_Auth.sql` (update +3 tables)
- `007_CreateTables_Sales.sql` ✅
- `008_SeedData.sql` ✅
- `009_CreateIndexes.sql` ✅

### Recommended enhancements
- Update scripts 003, 004, 006 to include Phase 1 tables
- Keep Phase 2 tables in separate optional script `010_Phase2_Commerce.sql`

---

## Summary

**v4 is well-designed and generic.** With Phase 1 additions (8 tables), it covers ~85% of enterprise apps. Phase 2 (11 tables) adds specific domains (e-commerce, CRM). The schema is:

- ✅ Highly reusable (polymorphic linking, hierarchies, state machines)
- ✅ Tenant-aware (multi-tenancy baked in)
- ✅ Audit-ready (multiple audit tables)
- ⚠️ Missing workflow/document versioning (recommend Phase 1)
- ⚠️ Missing e-commerce specifics (recommend Phase 2 if needed)

**Recommendation:** Include Phase 1 (8 tables) in the v4 release. Phase 2 can be added later or selected per use case.

