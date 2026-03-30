# SmartWorkz StarterKitMVC v4 - LEAN Schema Review (Revised)

**Date:** 2026-03-31
**Purpose:** Minimal v4.0 schema with ONE dummy table per schema, all non-essential tables removed
**Status:** Ready for Implementation

---

## Overview - SIMPLIFIED

| Schema | Tables | Purpose | Change from v1 |
|--------|--------|---------|-----------------|
| **Master** | 15 | Global reference data + Tags (moved from Core) | NEW: Tags, Tenants |
| **Core** | 8 | Tenant config + shared infrastructure | LEAN: removed business entities |
| **Transaction** | 1 | ONE dummy transactional table | LEAN: removed all but Orders |
| **Report** | 1 | ONE dummy reporting table | LEAN: removed all but ReportDefinitions |
| **Auth** | 13 | Identity + RBAC + logs | UNCHANGED |
| **TOTAL** | **38** | Single database, clean minimal structure | -24 tables (62 → 38) |

---

## 1. Master Schema (15 tables)

### Purpose
Global reference data used across all tenants. TenantId NULLABLE for global + tenant-specific overrides.

---

### 1.1 Geo Reference (3 tables)

```sql
Countries
├─ CountryCode2 (CHAR 2, unique)
├─ CountryCode3 (CHAR 3, unique)
├─ CountryName (NVARCHAR 100)
├─ PhoneCode (VARCHAR 20)
├─ CurrencyCode (CHAR 3)
└─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted

States
├─ FK → Countries (CountryId)
├─ StateCode (VARCHAR 10)
├─ StateName (NVARCHAR 100)
├─ Latitude, Longitude (DECIMAL)
└─ Audit columns

Cities
├─ FK → Countries (CountryId)
├─ FK → States (StateId, nullable)
├─ CityName (NVARCHAR 100)
├─ Latitude, Longitude (DECIMAL)
└─ Audit columns
```

---

### 1.2 Localization (2 tables)

```sql
Languages
├─ LanguageCode (VARCHAR 5, unique: en-US, es-ES, etc.)
├─ LanguageName (NVARCHAR 50)
├─ IsRtl (BIT) -- For Arabic, Hebrew support
└─ Indexes: LanguageCode

Translations (single table for ALL i18n)
├─ Namespace (VARCHAR 50) -- ui, errors, email, sms, lookup, category
├─ EntityType (VARCHAR 50, nullable) -- NULL for resource keys, or entity type
├─ EntityId (UNIQUEIDENTIFIER, nullable) -- Polymorphic linking
├─ TranslationKey (NVARCHAR 255)
├─ TranslationValue (NVARCHAR MAX)
├─ FK → Languages (LanguageId)
├─ TenantId (GUID, nullable) -- NULL=global, GUID=tenant override
└─ Indexes: (Namespace, EntityType, EntityId, LanguageId), (TenantId, LanguageId)
```

---

### 1.3 Hierarchical Reference Data (4 tables)

```sql
Lookups (HierarchyId tree)
├─ NodePath (HierarchyId) -- Tree structure: /1/2/3/
├─ NodeType (VARCHAR 50) -- 'Group', 'Value', 'SubValue'
├─ LookupCode (VARCHAR 100) -- 'StatusCodes.Active', 'Countries.US'
├─ DisplayText (NVARCHAR 200)
├─ DisplayOrder (INT)
├─ Icon (VARCHAR 100, nullable)
├─ ColorCode (VARCHAR 10, nullable)
├─ TenantId (GUID, nullable) -- Global lookups + tenant overrides
└─ Soft delete + Audit columns

Categories (HierarchyId tree)
├─ NodePath (HierarchyId)
├─ CategoryType (VARCHAR 50) -- 'ProductCategory', 'BlogCategory'
├─ DisplayText (NVARCHAR 200)
├─ Slug (VARCHAR 255, unique per tenant)
├─ Description (NVARCHAR MAX)
├─ TenantId (GUID, nullable)
├─ DisplayOrder (INT)
├─ Icon (VARCHAR 100, nullable)
└─ Soft delete + Audit columns

EntityStates (HierarchyId - state machine definitions)
├─ NodePath (HierarchyId)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice', 'Customer'
├─ StateCode (VARCHAR 50) -- 'Draft', 'Submitted', 'Approved'
├─ DisplayText (NVARCHAR 100)
├─ IsInitial (BIT) -- First state when created
├─ IsFinal (BIT) -- Terminal state
├─ RequiredRoles (VARCHAR MAX, JSON) -- ["Manager", "Approver"]
└─ Indexes: (EntityType, StateCode), (NodePath)

EntityStateTransitions (state flow rules)
├─ FK → EntityStates (FromStateId, ToStateId)
├─ EntityType (VARCHAR 50) -- Denormalized for convenience
├─ FromStateCode, ToStateCode (VARCHAR 50)
├─ RequiredRole (VARCHAR 50, nullable) -- 'Manager', 'Approver'
├─ DisplayText (NVARCHAR 100)
└─ Indexes: (EntityType, FromStateCode, ToStateCode)
```

---

### 1.4 Notification Templates (3 tables)

```sql
NotificationChannels
├─ ChannelCode (VARCHAR 50, unique) -- 'Email', 'SMS', 'WhatsApp', 'Push'
├─ DisplayText (NVARCHAR 100)
├─ IsActive (BIT)
└─ Indexes: ChannelCode

TemplateGroups (event classifications)
├─ EventCode (VARCHAR 100, unique) -- 'UserWelcome', 'OrderConfirmation'
├─ DisplayText (NVARCHAR 100)
├─ Description (NVARCHAR MAX)
└─ Indexes: EventCode

Templates (multi-channel, multi-language)
├─ FK → TemplateGroups (TemplateGroupId)
├─ FK → Languages (LanguageId)
├─ EventCode (VARCHAR 100, denormalized)
├─ Channel (VARCHAR 50) -- Email, SMS, WhatsApp, Push, InApp
├─ Subject (NVARCHAR 255, nullable)
├─ Body (NVARCHAR MAX) -- {{Variable}} placeholders
├─ PlaceholderSchema (VARCHAR MAX, JSON)
├─ TenantId (GUID, nullable) -- Global + tenant overrides
├─ VersionNumber (INT)
├─ IsActive (BIT)
└─ Indexes: (TemplateGroupId, LanguageId, Channel), (EventCode, Channel, TenantId)
```

---

### 1.5 Tags (MOVED FROM CORE - Global filtering)

```sql
Tags (polymorphic tagging - global reference)
├─ EntityType (VARCHAR 50) -- 'Order', 'Customer', 'Project', etc.
├─ EntityId (UNIQUEIDENTIFIER)
├─ TagName (NVARCHAR 100) -- Can be used as filter criteria
├─ TagCategory (VARCHAR 50, nullable) -- 'Priority', 'Status', 'Owner', 'Department'
├─ TenantId (GUID, nullable) -- NULL=global tag, GUID=tenant-specific
├─ Indexes: (TagName), (TenantId, TagName)
└─ Rationale: Global tag definitions for filtering across entities
```

---

### 1.6 Tenants (MOVED FROM CORE - Master reference)

```sql
Tenants (HierarchyId tree - moved to Master as reference data)
├─ NodePath (HierarchyId) -- Supports agency → client → sub-client hierarchies
├─ TenantCode (VARCHAR 100, unique) -- 'acme-corp', 'subsidiary'
├─ DisplayName (NVARCHAR 200)
├─ Subdomain (VARCHAR 100, unique, nullable)
├─ CustomDomain (NVARCHAR 255, unique, nullable)
├─ LogoUrl (NVARCHAR 500, nullable)
├─ PrimaryColor (VARCHAR 10, nullable)
├─ AccentColor (VARCHAR 10, nullable)
├─ IsActive (BIT)
├─ Indexes: (NodePath), (TenantCode), (Subdomain)
└─ Note: TenantId=NULL (this IS the tenant definition)
```

---

### 1.7 SeoMeta (2 tables)

```sql
SeoMeta
├─ EntityType (VARCHAR 50) -- 'Product', 'BlogPost', 'Page'
├─ EntityId (UNIQUEIDENTIFIER)
├─ MetaTitle (NVARCHAR 255)
├─ MetaDescription (NVARCHAR 500)
├─ MetaKeywords (NVARCHAR MAX)
├─ OgTitle, OgDescription, OgImage
├─ TwitterCard (VARCHAR 50)
├─ StructuredData (VARCHAR MAX, JSON) -- Schema.org markup
├─ TenantId (GUID, nullable)
└─ Indexes: (EntityType, EntityId), (TenantId)

UrlRedirects
├─ FromPath (VARCHAR 500, unique)
├─ ToPath (VARCHAR 500)
├─ RedirectCode (INT) -- 301, 302
├─ IsActive (BIT)
├─ HitCount (BIGINT)
├─ CreatedAt (DATETIME2)
└─ Indexes: FromPath, ToPath
```

---

### **Master Schema Summary: 15 tables**
- Geo (3): Countries, States, Cities
- i18n (2): Languages, Translations
- Hierarchies (4): Lookups, Categories, EntityStates, EntityStateTransitions
- Notifications (3): NotificationChannels, TemplateGroups, Templates
- Tags (1): Tags ← MOVED FROM CORE
- Tenants (1): Tenants ← MOVED FROM CORE
- SEO (2): SeoMeta, UrlRedirects

---

## 2. Core Schema (8 tables)

### Purpose
Tenant configuration and shared infrastructure for polymorphic linking. All tenant-specific.

---

### 2.1 Tenant Configuration (2 tables)

```sql
TenantSubscriptions
├─ FK → Master.Tenants (TenantId)
├─ SubscriptionPlanCode (VARCHAR 50) -- 'Starter', 'Professional', 'Enterprise'
├─ SubscriptionStartDate (DATETIME2)
├─ SubscriptionEndDate (DATETIME2)
├─ Status (VARCHAR 50) -- 'Active', 'Suspended', 'Expired', 'Cancelled'
├─ AutoRenew (BIT)
├─ Notes (NVARCHAR MAX)
└─ Indexes: (TenantId, Status)

TenantSettings (key-value store, flexible config)
├─ FK → Master.Tenants (TenantId)
├─ SettingKey (VARCHAR 255) -- 'EmailFromAddress', 'TimeZone', 'DateFormat'
├─ SettingValue (NVARCHAR MAX)
├─ SettingType (VARCHAR 50) -- 'string', 'int', 'bool', 'datetime', 'json'
├─ IsEncrypted (BIT)
├─ Indexes: (TenantId, SettingKey)
└─ Rationale: Flexible key-value store, no schema changes for new settings
```

---

### 2.2 Feature Flags (1 table)

```sql
FeatureFlags (tenant-scoped feature toggles)
├─ FK → Master.Tenants (TenantId, nullable for global flags)
├─ FeatureName (VARCHAR 100) -- 'AdvancedReporting', 'CustomDomain', '2FA'
├─ IsEnabled (BIT)
├─ RolloutPercent (INT) -- 0-100 for gradual rollout
├─ ValidFrom (DATETIME2, nullable)
├─ ValidTo (DATETIME2, nullable)
└─ Indexes: (TenantId, FeatureName)
```

---

### 2.3 Shared Infrastructure (5 tables - polymorphic linking via EntityType+EntityId)

```sql
Addresses (polymorphic - links to any entity)
├─ EntityType (VARCHAR 50) -- 'Customer', 'Vendor', 'Employee', 'Order'
├─ EntityId (UNIQUEIDENTIFIER)
├─ AddressType (VARCHAR 50) -- 'Billing', 'Shipping', 'Home', 'Office'
├─ Street1 (NVARCHAR 255)
├─ Street2 (NVARCHAR 255, nullable)
├─ City (NVARCHAR 100)
├─ FK → Master.States (StateId, nullable)
├─ FK → Master.Countries (CountryId)
├─ PostalCode (VARCHAR 20)
├─ Latitude, Longitude (DECIMAL, nullable)
├─ IsDefault (BIT)
├─ FK → Master.Tenants (TenantId)
└─ Indexes: (EntityType, EntityId), (TenantId)

Attachments (polymorphic file references)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice', 'Project'
├─ EntityId (UNIQUEIDENTIFIER)
├─ FileName (NVARCHAR 255)
├─ FileUrl (NVARCHAR 500) -- S3, Azure Blob, or local path
├─ FileSizeBytes (BIGINT)
├─ FileType (VARCHAR 50) -- 'pdf', 'docx', 'image'
├─ UploadedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ UploadedAt (DATETIME2)
├─ FK → Master.Tenants (TenantId)
└─ Indexes: (EntityType, EntityId), (TenantId)

Comments (polymorphic commenting system)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice', 'Project'
├─ EntityId (UNIQUEIDENTIFIER)
├─ CommentText (NVARCHAR MAX)
├─ CommentedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ CommentedAt (DATETIME2)
├─ ParentCommentId (UNIQUEIDENTIFIER, nullable) -- Nested replies
├─ FK → Master.Tenants (TenantId)
└─ Indexes: (EntityType, EntityId), (TenantId, CommentedAt)

StateHistory (polymorphic state machine tracking)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice'
├─ EntityId (UNIQUEIDENTIFIER)
├─ FromStateCode (VARCHAR 50)
├─ ToStateCode (VARCHAR 50)
├─ ChangedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ ChangedAt (DATETIME2)
├─ ChangeReason (NVARCHAR MAX)
├─ FK → Master.Tenants (TenantId)
└─ Indexes: (EntityType, EntityId, ChangedAt), (TenantId)

PreferenceDefinitions (extensible user/tenant preferences)
├─ PreferenceKey (VARCHAR 100, unique) -- 'Theme', 'Language', 'TimeZone'
├─ DataType (VARCHAR 50) -- 'string', 'int', 'bool', 'datetime', 'json'
├─ DefaultValue (NVARCHAR MAX)
├─ AllowedValues (VARCHAR MAX, JSON) -- ["Light", "Dark", "Auto"]
├─ Scope (VARCHAR 50) -- 'System', 'User', 'Tenant'
├─ Description (NVARCHAR 500)
└─ Indexes: PreferenceKey
```

---

### **Core Schema Summary: 8 tables**
- Tenant Config (2): TenantSubscriptions, TenantSettings
- Feature Flags (1): FeatureFlags
- Shared Infrastructure (5): Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions

---

## 3. Transaction Schema (1 table - LEAN)

### Purpose
**ONE dummy transactional table** to demonstrate order/invoice pattern. Teams extend with their domain entities.

---

```sql
Orders (ONE dummy table representing transactions)
├─ OrderNumber (VARCHAR 100, unique)
├─ OrderDate (DATETIME2)
├─ TotalAmount (DECIMAL 12,2)
├─ Status (VARCHAR 50) -- 'Draft', 'Submitted', 'Approved', 'Shipped'
├─ Notes (NVARCHAR MAX)
├─ FK → Master.Tenants (TenantId)
├─ IsDeleted (BIT) -- Soft delete
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
└─ Indexes: (OrderNumber), (TenantId, Status), (OrderDate)

Purpose: Minimal table showing transactional pattern
├─ Linked via Attachments (Order documents)
├─ Linked via Comments (Order discussion)
├─ Linked via StateHistory (Order workflow tracking)
├─ Linked via Tags (Order categorization)
└─ Teams add: OrderLines, Invoices, Payments, POs, etc. as needed
```

---

## 4. Report Schema (1 table - LEAN)

### Purpose
**ONE dummy reporting table** to demonstrate reporting pattern. Teams extend with their report types.

---

```sql
ReportDefinitions (ONE dummy table representing reports)
├─ ReportCode (VARCHAR 100, unique)
├─ ReportName (NVARCHAR 255)
├─ Description (NVARCHAR MAX)
├─ QuerySql (VARCHAR MAX) -- Raw SQL or stored procedure
├─ Parameters (VARCHAR MAX, JSON) -- [{"Name": "StartDate", "Type": "date"}]
├─ FK → Master.Tenants (TenantId, nullable)
├─ IsActive (BIT)
└─ Indexes: (ReportCode, TenantId)

Purpose: Minimal table showing reporting pattern
├─ Can schedule report runs
├─ Can cache results
├─ Can export to Excel/PDF/CSV
└─ Teams extend: ReportSchedules, ReportResults, DashboardWidgets, etc. as needed
```

---

## 5. Auth Schema (13 tables - UNCHANGED)

### Purpose
Identity, RBAC, sessions, verification, external logins, audit/activity logging.

---

```sql
Identity (3 tables):
├─ Users
│  ├─ Email (unique), FirstName, LastName
│  ├─ PasswordHash, PasswordSalt
│  ├─ EmailConfirmed, PhoneConfirmed
│  ├─ MfaEnabled, MfaSecret (TOTP)
│  ├─ AccountStatus (Active/Locked/Suspended)
│  └─ FK → Master.Tenants (TenantId)
│
├─ UserProfiles (extended profile)
│  ├─ FK → Users (UserId)
│  ├─ ProfilePictureUrl, Department, JobTitle, Manager
│  ├─ PreferredLanguage, PreferredTimeZone
│  ├─ NotifyEmail, NotifySms, NotifyPush, NotifyWhatsApp
│  └─ DateOfBirth, Gender, etc.
│
└─ UserPreferences (key-value user preferences)
   ├─ FK → Users (UserId)
   ├─ Theme, Language, TimeZone, etc.
   └─ Stores via FK → Core.PreferenceDefinitions

RBAC (4 tables):
├─ Roles (RoleCode, RoleName, TenantId nullable)
├─ Permissions (PermissionCode, Module, Action)
├─ RolePermissions (junction: Roles ↔ Permissions)
└─ UserRoles (junction: Users ↔ Roles)

Sessions & Auth (3 tables):
├─ RefreshTokens (long-lived tokens, per device)
├─ VerificationCodes (OTP, email verify, password reset)
└─ ExternalLogins (OAuth: Google, Microsoft, GitHub, Facebook)

Logging (3 tables):
├─ AuditLogs (auth-specific: Login, Logout, ChangePassword)
├─ ActivityLogs (usage: PageView, DataExport, ReportGeneration)
└─ NotificationLogs (delivery tracking: Email, SMS, Push status)

All Auth tables have FK → Master.Tenants (TenantId) except Permissions/Roles (which can be system-wide or tenant-specific)
```

---

## Schema Summary - FINAL LEAN VERSION

```
Master Schema (15 tables)
├─ Geo Reference (3): Countries, States, Cities
├─ Localization (2): Languages, Translations
├─ Hierarchies (4): Lookups, Categories, EntityStates, EntityStateTransitions
├─ Notifications (3): NotificationChannels, TemplateGroups, Templates
├─ Tags (1): Tags ← MOVED FROM CORE (global filtering)
├─ Tenants (1): Tenants ← MOVED FROM CORE (reference data)
└─ SEO (2): SeoMeta, UrlRedirects

Core Schema (8 tables)
├─ Tenant Config (2): TenantSubscriptions, TenantSettings
├─ Feature Flags (1): FeatureFlags
└─ Shared Infrastructure (5): Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions

Transaction Schema (1 table - LEAN)
└─ Orders (ONE dummy transactional table)

Report Schema (1 table - LEAN)
└─ ReportDefinitions (ONE dummy reporting table)

Auth Schema (13 tables - UNCHANGED)
├─ Identity (3): Users, UserProfiles, UserPreferences
├─ RBAC (4): Roles, Permissions, RolePermissions, UserRoles
├─ Sessions (3): RefreshTokens, VerificationCodes, ExternalLogins
└─ Logging (3): AuditLogs, ActivityLogs, NotificationLogs

TOTAL: 38 tables (down from 62)
Single Database: StarterKitMVC
All tables with audit columns, soft delete, TenantId for isolation
```

---

## Key Design Decisions (REVISED)

### ✅ What Changed

| Change | Reason |
|--------|--------|
| **Removed 24 tables** | Deferred to Phase 1+ (Workflows, Documents, Inventory, Business entities, Report tables, etc.) |
| **Transaction: 8 → 1 table** | Keep only Orders as dummy; teams add Invoices, Payments, POs as needed |
| **Report: 5 → 1 table** | Keep only ReportDefinitions as dummy; teams add Schedules, Results, Dashboards as needed |
| **Moved Tags to Master** | Global tagging for filter purposes; TenantId nullable for reusability |
| **Moved Tenants to Master** | Reference data, hierarchical; TenantId=NULL (this IS the definition) |
| **Core simplified to 8 tables** | Config (2) + Shared Infrastructure (5) + Preferences (1); no business entities |
| **Removed business entities** | Products, Customers, Vendors, Projects, Teams, Employees, Assets, Contracts — teams add as needed |

### ✅ What Stayed

- Geo reference (Countries, States, Cities) — used by Addresses
- Localization (Languages, Translations) — complete i18n
- Hierarchies (Lookups, Categories, EntityStates, Transitions) — state machine framework
- Notifications (Channels, TemplateGroups, Templates) — multi-channel templates
- SEO (SeoMeta, UrlRedirects) — SEO management
- Auth (13 tables) — identity + RBAC complete
- Shared infrastructure (Addresses, Attachments, Comments, StateHistory) — polymorphic linking

### ✅ Polymorphic Linking Still Works

Even with lean schema, you can link to ANY entity:
```
Orders (transaction dummy)
├─ Addresses (EntityType='Order', EntityId=123)
├─ Attachments (EntityType='Order', EntityId=123)
├─ Comments (EntityType='Order', EntityId=123)
├─ Tags (EntityType='Order', EntityId=123)
└─ StateHistory (EntityType='Order', EntityId=123)

When teams add Customers:
├─ Addresses (EntityType='Customer', EntityId=456)
├─ Comments (EntityType='Customer', EntityId=456)
└─ Tags (EntityType='Customer', EntityId=456)

When teams add Invoices:
├─ Attachments (EntityType='Invoice', EntityId=789)
├─ Comments (EntityType='Invoice', EntityId=789)
└─ StateHistory (EntityType='Invoice', EntityId=789)
```

No schema changes needed as you add entities!

---

## Implementation Simplification

### Phase 1 Effort (REDUCED)

| Task | Hours | Change |
|------|-------|--------|
| Database scripts | 8-10 | ↓ Fewer tables |
| Domain entities | 5-7 | ↓ Fewer entities (38 vs 62) |
| EF Core DbContexts | 6-8 | ↓ Simpler configs |
| Services | 4-6 | ↓ Fewer domain services |
| REST API | 6-8 | ↓ Fewer endpoints (~15) |
| Configuration | 2-3 | ↔ Same |
| **Total Phase 1** | **31-42 hours** | ↓ -10-15 hours saved |

### Phases 2-4 (Unchanged)
- Phase 2: 8 hours (docs + cleanup)
- Phase 3: 20-30 hours (MVC integration)
- Phase 4: 10-15 hours (API polish)

**New Total: 69-95 hours (down from 90-120)**

---

## What Teams Do in Phase 1+

When you need more functionality, simply add tables:

**Phase 1+ Examples:**
```
Transaction expansion:
├─ Add: OrderLines, Invoices, Payments, PurchaseOrders
├─ All link via existing StateHistory, Comments, Attachments, Tags
└─ No shared infrastructure changes needed

Core expansion:
├─ Add: Customers, Products, Vendors, Projects
├─ All use same Addresses, Attachments, Comments pattern
└─ Add to Core schema, or create new team schema

Report expansion:
├─ Add: ReportSchedules, ReportResults, DashboardWidgets
├─ Extend ReportDefinitions, no new patterns needed
└─ Keep in Report schema

Features:
├─ Add Workflows (Phase 1+): WorkflowDefinitions, WorkflowInstances
├─ Add Documents (Phase 1+): DocumentTypes, Documents, DocumentVersions
├─ Add Inventory (Phase 1+): StockMovements, StockAdjustments
└─ All use existing StateHistory, Comments, Attachments
```

---

## ✅ v4.0 Complete & Ready

**Single Database: StarterKitMVC**
- 38 tables across 5 schemas
- Clean, minimal, extensible
- All polymorphic patterns in place
- State machine framework ready
- Multi-tenancy complete
- Full identity + RBAC
- Multi-language i18n
- Email/SMS/Push templates

**Ready for Phase 1 implementation.**
