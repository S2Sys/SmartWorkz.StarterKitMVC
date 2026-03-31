# SmartWorkz StarterKitMVC v4 - LEAN Schema Review (Revised)

**Date:** 2026-03-31
**Purpose:** Minimal v4.0 schema with ONE dummy table per schema, all non-essential tables removed
**Status:** Ready for Implementation

---

## Overview - SIMPLIFIED

| Schema | Tables | Purpose | Change from v1 |
|--------|--------|---------|-----------------|
| **Master** | 18 | Global reference data + Config + Navigation | GEO: Countries + GeoHierarchy (Option C Hybrid); Config moved from Core; SeoMeta, Tags moved to Shared; Menus/MenuItems for dynamic navigation |
| **Shared** | 7 | Polymorphic infrastructure (reusable across all schemas) | Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions, SeoMeta, Tags |
| **Transaction** | 1 | ONE dummy transactional table | LEAN: removed all but Orders |
| **Report** | 4 | SQL reports + Dashboard APIs + Scheduling + Execution history | ReportDefinitions, ReportSchedules, ReportExecutions, ReportMetadata |
| **Auth** | 13 | Identity + RBAC + logs | UNCHANGED |
| **TOTAL** | **43** | Single database, clean minimal structure with production-ready reporting + dynamic navigation | 5 schemas (Master reference + config + navigation, Shared polymorphic, Transaction, Report, Auth) |

---

## 1. Master Schema (15 tables)

### Purpose
Global reference data used across all tenants. TenantId NULLABLE for global + tenant-specific overrides.
**Note:** Option C (Hybrid) Geo approach consolidates 3 tables (Countries, States, Cities) into 2 tables (Countries + GeoHierarchy).

---

### 1.1 Geo Reference - OPTION C (HYBRID: 2 tables)

#### Countries (Reference Data - Fast Lookups)
```sql
Countries
├─ CountryId (GUID)
├─ CountryCode2 (CHAR 2, unique) -- 'US', 'CA', 'GB'
├─ CountryCode3 (CHAR 3, unique) -- 'USA', 'CAN', 'GBR'
├─ CountryName (NVARCHAR 100)
├─ PhoneCode (VARCHAR 20)
├─ CurrencyCode (CHAR 3)
├─ TimeZone (VARCHAR 50, nullable)
├─ Population (BIGINT, nullable)
├─ IsActive (BIT)
└─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
```

#### GeoHierarchy (Flexible State/City/District - HierarchyId Tree)
```sql
GeoHierarchy
├─ GeoHierarchyId (GUID)
├─ FK → Countries (CountryId) -- Link to country
├─ NodePath (HierarchyId) -- Tree: /1/ (State) → /1/1/ (City) → /1/1/1/ (District)
├─ GeoType (VARCHAR 50) -- 'State', 'City', 'District', 'Region', 'Neighborhood'
├─ Code (VARCHAR 20) -- 'CA', 'NY', 'SF'
├─ Name (NVARCHAR 100)
├─ Latitude, Longitude (DECIMAL, nullable)
├─ TimeZone (VARCHAR 50, nullable)
├─ Population (BIGINT, nullable)
├─ IsActive (BIT)
└─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted

Example Hierarchy (USA):
/1/                     States
├─ /1/1/                California (State)
│  ├─ /1/1/1/           San Francisco (City)
│  ├─ /1/1/2/           Los Angeles (City)
│  └─ /1/1/3/           Bay Area (District/Region)
├─ /1/2/                New York (State)
│  ├─ /1/2/1/           New York City (City)
│  └─ /1/2/2/           Manhattan (District)
└─ /1/3/                Texas (State)
   └─ /1/3/1/           Dallas (City)

Example Hierarchy (UK):
/1/                     Regions
├─ /1/1/                England (Region)
│  ├─ /1/1/1/           Greater London (County)
│  │  ├─ /1/1/1/1/      Westminster (City)
│  │  └─ /1/1/1/2/      Tower Hamlets (City)
│  └─ /1/1/2/           Greater Manchester (County)
└─ /1/2/                Scotland (Region)
   └─ /1/2/1/           Edinburgh (City)
```

**Why Option C (Hybrid)?**
- ✅ Countries: Fast lookups for currency, phone code (indexed)
- ✅ GeoHierarchy: Flexible State/City/District structure (HierarchyId, no schema changes)
- ✅ Handles global variations (USA States, UK Districts, varying depths)
- ✅ Address FK → GeoHierarchy for any geo level
- ✅ Efficient: 2 tables instead of 3, better than single large table

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

### 1.7 UrlRedirects (1 table)

**SeoMeta moved to Shared schema** for polymorphic linking across Products, Categories, MenuItems, BlogPosts, etc. See Section 2.1 and [SEO-POLYMORPHIC-DESIGN.md](SEO-POLYMORPHIC-DESIGN.md).

```sql
UrlRedirects
├─ UrlRedirectId (GUID)
├─ TenantId (GUID, NOT NULL) -- Tenant-specific redirects
├─ FromPath (VARCHAR 500) -- /old-product-page
├─ ToPath (VARCHAR 500) -- /products/new-product-name
├─ RedirectCode (INT) -- 301 (permanent), 302 (temporary)
├─ IsActive (BIT, default 1)
├─ HitCount (BIGINT, default 0) -- Track redirect usage
├─ Audit: CreatedAt, UpdatedAt
└─ Indexes: (TenantId, FromPath), (ToPath)
```

---

### 1.8 Tenant Configuration (MOVED FROM CORE - Master reference)

```sql
TenantSubscriptions
├─ TenantSubscriptionId (GUID)
├─ FK → Master.Tenants (TenantId)
├─ PlanCode (VARCHAR 50) -- 'Starter', 'Professional', 'Enterprise'
├─ StartDate (DATETIME2)
├─ EndDate (DATETIME2)
├─ Status (VARCHAR 50) -- 'Active', 'Suspended', 'Expired', 'Cancelled'
├─ AutoRenew (BIT)
├─ Notes (NVARCHAR MAX)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
└─ Indexes: (TenantId, Status), (PlanCode)

TenantSettings (key-value store, flexible config)
├─ TenantSettingId (GUID)
├─ FK → Master.Tenants (TenantId)
├─ Key (VARCHAR 255) -- 'EmailFromAddress', 'TimeZone', 'DateFormat'
├─ Value (NVARCHAR MAX)
├─ DataType (VARCHAR 50) -- 'string', 'int', 'bool', 'datetime', 'json'
├─ IsEncrypted (BIT)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
└─ Indexes: (TenantId, Key)

FeatureFlags (tenant-scoped feature toggles)
├─ FeatureFlagId (GUID)
├─ FK → Master.Tenants (TenantId, nullable for global flags)
├─ Name (VARCHAR 100) -- 'AdvancedReporting', 'CustomDomain', '2FA'
├─ IsEnabled (BIT)
├─ RolloutPercent (INT) -- 0-100 for gradual rollout
├─ ValidFrom (DATETIME2, nullable)
├─ ValidTo (DATETIME2, nullable)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
└─ Indexes: (TenantId, Name), (Name, IsEnabled)
```

---

### **Master Schema Summary: 18 tables** (Option C Hybrid Geo + Config + Navigation)
- Geo (2): Countries, GeoHierarchy ← OPTION C (hybrid: reference + hierarchical)
- i18n (2): Languages, Translations
- Hierarchies (4): Lookups, Categories, EntityStates, EntityStateTransitions
- Notifications (3): NotificationChannels, TemplateGroups, Templates
- Tenants (1): Tenants ← MOVED FROM CORE (reference data)
- SEO (1): UrlRedirects ← SeoMeta MOVED TO SHARED for polymorphic linking
- Config (3): TenantSubscriptions, TenantSettings, FeatureFlags (per-tenant operational toggles)
- Navigation (2): Menus, MenuItems (HierarchyId trees, role-based, per-tenant menus)

---

## 2. Shared Schema (7 tables)

### Purpose
Polymorphic infrastructure used across ALL schemas. Enables flexible entity linking without schema changes. SeoMeta and Tags moved here for consistency with polymorphic pattern.

---

### 2.1 Shared Infrastructure (7 tables - polymorphic linking via EntityType+EntityId - REUSABLE ACROSS ALL SCHEMAS)

```sql
Addresses (polymorphic - links to any entity)
├─ AddressId (GUID)
├─ EntityType (VARCHAR 50) -- 'Customer', 'Vendor', 'Employee', 'Order'
├─ EntityId (UNIQUEIDENTIFIER)
├─ Type (VARCHAR 50) -- 'Billing', 'Shipping', 'Home', 'Office'
├─ Street1 (NVARCHAR 255)
├─ Street2 (NVARCHAR 255, nullable)
├─ City (NVARCHAR 100)
├─ FK → Master.GeoHierarchy (GeoHierarchyId, nullable) -- For City level
├─ FK → Master.Countries (CountryId)
├─ PostalCode (VARCHAR 20)
├─ Latitude, Longitude (DECIMAL, nullable)
├─ IsDefault (BIT)
├─ FK → Master.Tenants (TenantId)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
└─ Indexes: (EntityType, EntityId), (TenantId), (Type)

Attachments (polymorphic file references)
├─ AttachmentId (GUID)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice', 'Project'
├─ EntityId (UNIQUEIDENTIFIER)
├─ FileName (NVARCHAR 255)
├─ FileUrl (NVARCHAR 500) -- S3, Azure Blob, or local path
├─ FileSizeBytes (BIGINT)
├─ FileType (VARCHAR 50) -- 'pdf', 'docx', 'image'
├─ UploadedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ UploadedAt (DATETIME2)
├─ FK → Master.Tenants (TenantId)
├─ IsDeleted (BIT)
└─ Indexes: (EntityType, EntityId), (TenantId), (UploadedAt)

Comments (polymorphic commenting system)
├─ CommentId (GUID)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice', 'Project'
├─ EntityId (UNIQUEIDENTIFIER)
├─ Text (NVARCHAR MAX)
├─ CreatedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ CreatedAt (DATETIME2)
├─ ParentCommentId (UNIQUEIDENTIFIER, nullable) -- Nested replies
├─ FK → Master.Tenants (TenantId)
├─ IsDeleted (BIT)
└─ Indexes: (EntityType, EntityId), (TenantId, CreatedAt), (ParentCommentId)

StateHistory (polymorphic state machine tracking)
├─ StateHistoryId (GUID)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice'
├─ EntityId (UNIQUEIDENTIFIER)
├─ FromState (VARCHAR 50)
├─ ToState (VARCHAR 50)
├─ ChangedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ ChangedAt (DATETIME2)
├─ Reason (NVARCHAR MAX, nullable)
├─ FK → Master.Tenants (TenantId)
└─ Indexes: (EntityType, EntityId, ChangedAt DESC), (TenantId), (ToState)

PreferenceDefinitions (extensible user/tenant preferences)
├─ PreferenceDefinitionId (GUID)
├─ Key (VARCHAR 100, unique) -- 'Theme', 'Language', 'TimeZone'
├─ DataType (VARCHAR 50) -- 'string', 'int', 'bool', 'datetime', 'json'
├─ DefaultValue (NVARCHAR MAX)
├─ AllowedValues (VARCHAR MAX, JSON) -- ["Light", "Dark", "Auto"]
├─ Scope (VARCHAR 50) -- 'System', 'User', 'Tenant'
├─ Description (NVARCHAR 500)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
└─ Indexes: Key, Scope

SeoMeta (polymorphic SEO metadata - MOVED FROM MASTER)
├─ SeoMetaId (GUID)
├─ TenantId (GUID, NOT NULL) -- Row-level isolation
├─ EntityType (VARCHAR 50) -- 'Product', 'Category', 'MenuItem', 'BlogPost', 'GeolocationPage'
├─ EntityId (UNIQUEIDENTIFIER) -- Polymorphic link to any entity
├─ Slug (VARCHAR 255) -- URL-friendly identifier
├─ MetaTitle (NVARCHAR 255)
├─ MetaDescription (NVARCHAR 500)
├─ MetaKeywords (NVARCHAR MAX, nullable)
├─ OgTitle (NVARCHAR 255, nullable) -- Open Graph
├─ OgDescription (NVARCHAR 500, nullable)
├─ OgImage (VARCHAR MAX, nullable) -- URL or FK to Attachment
├─ OgType (VARCHAR 50, nullable, default 'website')
├─ CanonicalUrl (VARCHAR MAX, nullable)
├─ SchemaMarkup (NVARCHAR MAX, nullable) -- JSON: schema.org structured data
├─ Robots (VARCHAR 100, nullable, default 'index,follow')
├─ IsActive (BIT, default 1)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
└─ Indexes: (TenantId, EntityType), (TenantId, Slug), (EntityType, EntityId), (IsActive)

Tags (polymorphic tagging - MOVED FROM MASTER)
├─ TagId (GUID)
├─ TenantId (GUID, NOT NULL) -- Row-level isolation (changed from nullable)
├─ EntityType (VARCHAR 50) -- 'Product', 'Order', 'Customer', 'Project', 'BlogPost'
├─ EntityId (UNIQUEIDENTIFIER) -- Polymorphic link to any entity
├─ TagName (NVARCHAR 100) -- Used as filter criteria: 'VIP', 'Rush', 'Urgent'
├─ TagCategory (VARCHAR 50, nullable) -- 'Priority', 'Status', 'Owner', 'Department'
├─ Color (VARCHAR 10, nullable) -- Hex color for UI: '#FF0000', '#00FF00'
├─ DisplayOrder (INT, nullable) -- Sort order for display
├─ IsActive (BIT, default 1)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
└─ Indexes: (TenantId, TagName), (TenantId, EntityType, EntityId), (TagName), (TagCategory)
```

**Why SeoMeta in Shared?**
- ✅ Single table serves Products, Categories, MenuItems, BlogPosts, CustomPages
- ✅ Consistent with Addresses, Comments, Attachments pattern
- ✅ Automatic SEO for any new entity type (no schema changes)
- ✅ TenantId NOT NULL for proper row-level isolation

**Why Tags in Shared?**
- ✅ Single table serves Products, Orders, Customers, Projects, BlogPosts, etc.
- ✅ Consistent with polymorphic linking pattern (EntityType + EntityId)
- ✅ Flexible filtering/categorization for any entity
- ✅ TenantId NOT NULL ensures multi-tenant isolation
- ✅ New entity types automatically get tagging (no schema changes)

**See:** [`docs/srs/SEO-POLYMORPHIC-DESIGN.md`](SEO-POLYMORPHIC-DESIGN.md) for complete examples: Products, Categories/Subcategories (HierarchyId), Geo-based listings, MenuItems, BlogPosts, URL routing strategies.

---

### **Shared Schema Summary: 7 tables**
- Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions, SeoMeta, Tags
- **Used by:** Transaction, Report, Core, and all future domain schemas
- **Pattern:** EntityType + EntityId allows linking to any entity type
- **SEO:** Single SeoMeta table replaces embedded SEO columns, supports polymorphic linking
- **Tagging:** Single Tags table for flexible filtering/categorization across all entities

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

## 4. Report Schema (4 tables)

### Purpose
Flexible reporting framework supporting:
- SQL-based reports (queries, stored procedures, ad-hoc)
- Dashboard APIs (metrics, KPIs, charts, aggregations)
- Report scheduling and execution tracking
- Result caching and export capabilities

---

```sql
ReportDefinitions (report/dashboard metadata)
├─ ReportDefinitionId (GUID)
├─ Code (VARCHAR 100, unique) -- 'SalesReport', 'UserKPIDashboard', 'OrderAnalysis'
├─ Name (NVARCHAR 255)
├─ Type (VARCHAR 50) -- 'SQL', 'Dashboard', 'Stored_Procedure', 'API'
├─ Description (NVARCHAR MAX)
├─ QuerySql (VARCHAR MAX, nullable) -- Raw SQL for SQL/StoredProc types
├─ ApiEndpoint (VARCHAR 500, nullable) -- For Dashboard/API types: /api/v1/metrics/sales
├─ Parameters (VARCHAR MAX, JSON) -- [{"Name": "StartDate", "Type": "date", "Required": true}]
├─ OutputFormat (VARCHAR 50) -- 'Table', 'Chart', 'Metric', 'HTML', 'JSON'
├─ RefreshFrequency (VARCHAR 50) -- 'Realtime', 'Hourly', 'Daily', 'Weekly', 'Monthly', 'OnDemand'
├─ CacheTtlMinutes (INT, nullable) -- Cache duration (NULL = no cache, 0 = always fresh)
├─ Owner (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ FK → Master.Tenants (TenantId, nullable) -- NULL=global, GUID=tenant-specific
├─ IsActive (BIT)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
├─ IsDeleted (BIT)
└─ Indexes: (Code, TenantId), (Type, IsActive), (Owner)

ReportSchedules (scheduling metadata - for background execution)
├─ ReportScheduleId (GUID)
├─ FK → Report.ReportDefinitions (ReportDefinitionId)
├─ CronExpression (VARCHAR 100) -- '0 9 * * 1' (9am Mon), '0 0 * * *' (midnight daily)
├─ ExecutionType (VARCHAR 50) -- 'Scheduled', 'OnDemand'
├─ IsEnabled (BIT)
├─ LastExecutedAt (DATETIME2, nullable)
├─ NextScheduledAt (DATETIME2, nullable)
├─ NotifyEmail (VARCHAR 255, nullable) -- Send results to email
├─ ExportFormat (VARCHAR 50, nullable) -- 'PDF', 'Excel', 'CSV', 'JSON'
├─ FK → Master.Tenants (TenantId)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
├─ IsDeleted (BIT)
└─ Indexes: (ReportDefinitionId, IsEnabled, NextScheduledAt), (TenantId)

ReportExecutions (execution history for auditing and result tracking)
├─ ReportExecutionId (GUID)
├─ FK → Report.ReportDefinitions (ReportDefinitionId)
├─ StartTime (DATETIME2)
├─ EndTime (DATETIME2, nullable)
├─ Status (VARCHAR 50) -- 'Running', 'Completed', 'Failed', 'Cancelled'
├─ RowsReturned (INT, nullable)
├─ ExecutionTimeMs (INT, nullable) -- Query duration
├─ ErrorMessage (NVARCHAR MAX, nullable)
├─ ExecutedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ StorageUrl (VARCHAR 500, nullable) -- S3/Blob path to cached result
├─ StorageExpiresAt (DATETIME2, nullable) -- When to delete cached result
├─ FK → Master.Tenants (TenantId)
├─ IsDeleted (BIT)
└─ Indexes: (ReportDefinitionId, StartTime DESC), (Status), (TenantId, StartTime DESC)

ReportMetadata (extensible metadata for filters, drill-downs, conditional formatting)
├─ ReportMetadataId (GUID)
├─ FK → Report.ReportDefinitions (ReportDefinitionId)
├─ Key (VARCHAR 100) -- 'ColumnOrder', 'Filters', 'Drill_Down_Config', 'Conditional_Formatting'
├─ Value (NVARCHAR MAX, JSON) -- Complex config as JSON
├─ Type (VARCHAR 50) -- 'Column', 'Filter', 'Drill_Down', 'Conditional', 'Visualization'
├─ FK → Master.Tenants (TenantId, nullable)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
└─ Indexes: (ReportDefinitionId, Key), (TenantId)
```

---

### **Report Schema Summary: 4 tables** (Production-ready patterns)

| Table | Purpose | Key Use Case |
|-------|---------|--------------|
| **ReportDefinitions** | Report/dashboard metadata | Define once, use everywhere (SQL, API, Dashboard) |
| **ReportSchedules** | Scheduling & automation | Background reports, email delivery, export jobs |
| **ReportExecutions** | Audit trail & history | Track execution, cache results, debug failures |
| **ReportMetadata** | Extensible configuration | Filters, drill-downs, column order, charts, conditional formatting |

### **Common Report Patterns Supported**

#### 1️⃣ **SQL-Based Reports**
```json
{
  "ReportCode": "SalesAnalysis",
  "ReportType": "SQL",
  "QuerySql": "SELECT ProductCategory, SUM(Amount) FROM Orders WHERE Date >= @StartDate",
  "Parameters": [{"Name": "StartDate", "Type": "date"}],
  "OutputFormat": "Table",
  "CacheTtlMinutes": 60
}
```
**Use Case:** Custom analytics, business intelligence, ad-hoc queries

#### 2️⃣ **Dashboard Metrics/KPIs**
```json
{
  "ReportCode": "SalesDashboard",
  "ReportType": "Dashboard",
  "ApiEndpoint": "/api/v1/metrics/sales",
  "OutputFormat": "JSON",
  "RefreshFrequency": "Realtime",
  "CacheTtlMinutes": 5,
  "Metadata": {
    "Widgets": [
      {"Type": "Metric", "Label": "Total Revenue", "Value": "{{SUM(Amount)}}"},
      {"Type": "Chart", "ChartType": "Line", "Label": "Revenue Trend"},
      {"Type": "Table", "Label": "Top Products"}
    ]
  }
}
```
**Use Case:** Executive dashboards, live KPIs, real-time metrics

#### 3️⃣ **Stored Procedure Reports**
```json
{
  "ReportCode": "MonthlyFinancials",
  "ReportType": "Stored_Procedure",
  "QuerySql": "EXEC sp_GenerateMonthlyFinancials @Month, @Year",
  "Parameters": [
    {"Name": "Month", "Type": "int"},
    {"Name": "Year", "Type": "int"}
  ],
  "OutputFormat": "Table"
}
```
**Use Case:** Complex calculations, regulatory reports, financial statements

#### 4️⃣ **Scheduled/Automated Reports**
```json
{
  "ReportCode": "DailyRevenueEmail",
  "ReportType": "SQL",
  "RefreshFrequency": "Daily",
  "ScheduleCron": "0 9 * * *",
  "ExportFormat": "PDF",
  "NotifyEmail": "finance@company.com",
  "CacheTtlMinutes": 1440
}
```
**Schedule:** 9am daily, export to PDF, email results
**Execution Tracking:** ReportExecutions table tracks success/failure

#### 5️⃣ **Master-Detail Reports with Drill-Down**
```json
{
  "ReportCode": "SalesOrderDetail",
  "ReportType": "SQL",
  "OutputFormat": "Table",
  "Metadata": {
    "ColumnOrder": ["OrderNumber", "OrderDate", "TotalAmount", "Status"],
    "Drill_Down": {
      "OrderNumber": "/api/v1/orders/{id}/lines"
    },
    "Conditional_Formatting": {
      "Status": {"Completed": "#90EE90", "Pending": "#FFD700", "Failed": "#FF6B6B"}
    }
  }
}
```
**Use Case:** Click OrderNumber → see OrderLines, color-coded status

#### 6️⃣ **Filter & Parameter Reports**
```json
{
  "Parameters": [
    {"Name": "StartDate", "Type": "date", "Required": true},
    {"Name": "EndDate", "Type": "date", "Required": true},
    {"Name": "Category", "Type": "string", "Required": false, "AllowedValues": ["Electronics", "Clothing", "Books"]},
    {"Name": "MinAmount", "Type": "decimal", "Required": false, "Default": 0}
  ]
}
```
**API Call:** `/api/v1/reports/SalesReport?StartDate=2026-01-01&EndDate=2026-03-31&Category=Electronics`

---

### **Key Design Benefits**

✅ **Unified Framework** — SQL reports, Dashboards, APIs use same ReportDefinitions table
✅ **Scheduling Built-in** — ReportSchedules table handles background execution
✅ **Audit Trail** — ReportExecutions tracks every run (status, timing, errors)
✅ **Result Caching** — StorageUrl + CacheTtlMinutes for performance
✅ **Extensible Metadata** — ReportMetadata handles filters, drill-downs, visualizations without schema changes
✅ **Multi-tenant** — TenantId support for tenant-specific reports
✅ **Export Ready** — PDF, Excel, CSV, JSON formats supported
✅ **Real-time + Scheduled** — RefreshFrequency handles both live dashboards and batch reports

---

### **Real-World Example: Sales Dashboard**

```
1. Define Report (ReportDefinitions)
   ReportCode: "SalesDashboard"
   ReportType: "Dashboard"
   ApiEndpoint: "/api/v1/metrics/sales"
   RefreshFrequency: "Hourly"
   CacheTtlMinutes: 60

2. API Handler gets /api/v1/metrics/sales
   → Queries ReportDefinitions for "SalesDashboard"
   → Checks ReportExecutions for cached result
   → If not cached, executes ApiEndpoint
   → Stores result in ReportExecutions.StorageUrl
   → Returns JSON with widgets/metrics

3. Dashboard renders
   Metric: Total Revenue (from cache)
   Chart: Revenue Trend (last 30 days)
   Table: Top 10 Products (drill-down to product detail)
   Filters: Date Range, Category, Min Amount

4. When expired (CacheTtlMinutes)
   Background job in ReportSchedules
   Executes fresh query, updates cache
   Logs execution in ReportExecutions
```

---

### **Teams Extend With (Phase 1+)**

- **ReportDistribution** — Email, Slack, Teams notifications
- **ReportPermissions** — Row-level security (who can see what reports)
- **ReportTemplates** — Pre-built templates (Sales, Finance, HR)
- **DashboardWidgets** — Custom widget definitions
- **DataSourceConfig** — Support multiple databases, APIs, data warehouses

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
Master Schema (17 tables - Option C Geo + Config + Navigation)
├─ Geo Reference (2): Countries, GeoHierarchy ← OPTION C (Hybrid: Reference + HierarchyId)
├─ Localization (2): Languages, Translations
├─ Hierarchies (4): Lookups, Categories, EntityStates, EntityStateTransitions
├─ Notifications (3): NotificationChannels, TemplateGroups, Templates
├─ Tenants (1): Tenants ← MOVED FROM CORE (reference data)
├─ SEO (1): UrlRedirects ← SeoMeta MOVED TO SHARED
├─ Config (3): TenantSubscriptions, TenantSettings, FeatureFlags ← MOVED FROM CORE
└─ Navigation (2): Menus, MenuItems ← NEW (HierarchyId trees, role-based)

Shared Schema (7 tables - Polymorphic Infrastructure)
├─ Addresses (polymorphic address linking)
├─ Attachments (polymorphic file references)
├─ Comments (polymorphic commenting system)
├─ StateHistory (polymorphic state machine tracking)
├─ PreferenceDefinitions (extensible preferences)
├─ SeoMeta ← MOVED FROM MASTER (polymorphic SEO metadata)
└─ Tags ← MOVED FROM MASTER (polymorphic tagging/categorization)

Transaction Schema (1 table - LEAN)
└─ Orders (ONE dummy transactional table)

Report Schema (4 tables - Production-ready Reporting)
├─ ReportDefinitions (SQL, Dashboard, Stored Procedure report types)
├─ ReportSchedules (background job scheduling with cron)
├─ ReportExecutions (audit trail, performance metrics, result caching)
└─ ReportMetadata (filters, drill-downs, conditional formatting)

Auth Schema (13 tables - UNCHANGED)
├─ Identity (3): Users, UserProfiles, UserPreferences
├─ RBAC (4): Roles, Permissions, RolePermissions, UserRoles
├─ Sessions (3): RefreshTokens, VerificationCodes, ExternalLogins
└─ Logging (3): AuditLogs, ActivityLogs, NotificationLogs

TOTAL: 43 tables (LEAN design with maximum flexibility)
- Master: 18 tables (global reference data, geo, i18n, hierarchies, notifications, tenants, config, navigation)
- Shared: 7 tables (polymorphic infrastructure)
- Transaction: 1 table (Orders dummy)
- Report: 4 tables (ReportDefinitions, ReportSchedules, ReportExecutions, ReportMetadata)
- Auth: 13 tables (identity, RBAC, logs)
Single Database: StarterKitMVC
All tables with audit columns, soft delete, TenantId for row-level isolation
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
├─ Addresses (EntityType='Order', EntityId=123) ← Billing, Shipping addresses
├─ Attachments (EntityType='Order', EntityId=123) ← Invoices, Labels
├─ Comments (EntityType='Order', EntityId=123) ← Customer notes, support
├─ Tags (EntityType='Order', EntityId=123) ← 'VIP', 'Rush', 'Urgent'
├─ SeoMeta (EntityType='Order', EntityId=123) ← Order detail page SEO
└─ StateHistory (EntityType='Order', EntityId=123) ← Draft → Shipped → Delivered

When teams add Customers:
├─ Addresses (EntityType='Customer', EntityId=456) ← Home, Office, Billing
├─ Comments (EntityType='Customer', EntityId=456) ← Notes, feedback
├─ Tags (EntityType='Customer', EntityId=456) ← 'Premium', 'At-Risk', 'New'
└─ SeoMeta (EntityType='Customer', EntityId=456) ← Customer profile page SEO

When teams add Products:
├─ Attachments (EntityType='Product', EntityId=789) ← Images, datasheets
├─ Comments (EntityType='Product', EntityId=789) ← Reviews, ratings
├─ Tags (EntityType='Product', EntityId=789) ← 'Featured', 'Sale', 'New Arrival'
└─ SeoMeta (EntityType='Product', EntityId=789) ← Product detail page SEO
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
