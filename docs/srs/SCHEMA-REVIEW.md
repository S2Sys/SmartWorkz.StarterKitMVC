# SmartWorkz StarterKitMVC v4 - Complete Schema Review

**Date:** 2026-03-31
**Purpose:** Comprehensive table-by-table review of all 62 tables across 6 schemas
**Status:** Ready for Implementation Review

---

## Overview

| Schema | Tables | Category | TenantId Strategy |
|--------|--------|----------|-------------------|
| **Master** | 17 | Global reference data | NULLABLE (NULL=global, GUID=tenant override) |
| **Core** | 18 | Config + business entities | NOT NULL (multi-tenant) |
| **Transaction** | 8 | Transactional data | NOT NULL (multi-tenant) |
| **Report** | 5 | Reporting + dashboards | NULLABLE (shared reports + tenant-specific) |
| **Auth** | 13 | Identity + RBAC + logs | NOT NULL (per-tenant users/roles) |
| **Sales** | 1 | Example team schema | NOT NULL (can add more team schemas) |
| **TOTAL** | **62** | | |

---

## 1. Master Schema (17 tables)

### Purpose
Global reference data, shared across all tenants. TenantId is nullable to allow global defaults + tenant-specific overrides.

### 1.1 Geo Reference Tables (3 tables)

```sql
-- Core columns all have: Id (GUID), CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, DeletedAt, DeletedBy

Countries
├─ CountryCode2 (CHAR 2, unique)
├─ CountryCode3 (CHAR 3, unique)
├─ CountryName (NVARCHAR 100)
├─ PhoneCode (VARCHAR 20)
├─ CurrencyCode (CHAR 3)
└─ Indexes: CountryCode2, CountryCode3, CurrencyCode

States
├─ FK → Countries (CountryId)
├─ StateCode (VARCHAR 10)
├─ StateName (NVARCHAR 100)
├─ Latitude (DECIMAL 10,8)
├─ Longitude (DECIMAL 11,8)
└─ Indexes: CountryId, StateCode

Cities
├─ FK → Countries (CountryId)
├─ FK → States (StateId, nullable for territories)
├─ CityName (NVARCHAR 100)
├─ Latitude (DECIMAL 10,8)
├─ Longitude (DECIMAL 11,8)
└─ Indexes: CountryId, StateId, CityName
```

**Rationale:** Standard geographic hierarchy. Used by Addresses (polymorphic), shipping rates, tenant locations.

---

### 1.2 Localization Tables (2 tables)

```sql
Languages
├─ LanguageCode (VARCHAR 5, unique: en-US, es-ES, etc.)
├─ LanguageName (NVARCHAR 50)
├─ IsRtl (BIT) -- For Arabic, Hebrew support
└─ Indexes: LanguageCode

Translations
├─ Namespace (VARCHAR 50) -- ui, errors, email, sms, lookup, category, etc.
├─ EntityType (VARCHAR 50) -- NULL for resource keys, or entity type (Product, etc.)
├─ EntityId (UNIQUEIDENTIFIER, nullable) -- Link to any entity via EntityType+EntityId
├─ TranslationKey (NVARCHAR 255) -- "LoginButton", "EmailWelcomeSubject", "ProductName_123"
├─ TranslationValue (NVARCHAR MAX)
├─ FK → Languages (LanguageId)
├─ TenantId (GUID, nullable) -- NULL=global, GUID=tenant override
├─ Indexes: (Namespace, EntityType, EntityId, LanguageId), (TenantId, LanguageId), (TranslationKey)
└─ Rationale: Single table for ALL i18n (replaces v1's multiple LoV localization tables)
```

**Use cases:**
- Resource keys: `Translations.EntityType IS NULL` (e.g., "Common.SaveButton")
- Entity translations: `Translations.EntityType = 'Product'` (e.g., product names per language)
- Tenant-specific translations: `Translations.TenantId = @tenantId` (e.g., company-specific terms)

---

### 1.3 Hierarchical Reference Data (4 tables)

```sql
Lookups (HierarchyId tree)
├─ NodePath (HierarchyId) -- Tree structure: /1/2/3/
├─ NodeType (VARCHAR 50) -- 'Group', 'Value', 'SubValue'
├─ LookupCode (VARCHAR 100) -- 'StatusCodes.Active', 'StatusCodes.Inactive', 'Countries.US'
├─ DisplayText (NVARCHAR 200)
├─ DisplayOrder (INT)
├─ Icon (VARCHAR 100) -- Optional
├─ ColorCode (VARCHAR 10) -- Optional hex color
├─ TenantId (GUID, nullable) -- Global lookups + tenant overrides
├─ IsDeleted, soft delete columns
└─ Indexes: (NodePath), (LookupCode, TenantId), (NodeType)

Example hierarchy:
/1/                          StatusCodes (Group)
├─ /1/1/                     Active (Value)
├─ /1/2/                     Inactive (Value)
├─ /1/3/                     Pending (Value)
/2/                          Currencies (Group)
├─ /2/1/                     USD (Value)
├─ /2/2/                     EUR (Value)

Categories (HierarchyId tree)
├─ NodePath (HierarchyId)
├─ CategoryType (VARCHAR 50) -- 'ProductCategory', 'BlogCategory', 'ContentCategory'
├─ DisplayText (NVARCHAR 200)
├─ Slug (VARCHAR 255, unique per tenant) -- /electronics/computers
├─ Description (NVARCHAR MAX)
├─ TenantId (GUID, nullable)
├─ DisplayOrder (INT)
├─ Icon (VARCHAR 100)
├─ IsDeleted, soft delete
└─ Indexes: (NodePath), (Slug, TenantId), (CategoryType)

EntityStates (HierarchyId, state machine definitions)
├─ NodePath (HierarchyId)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice', 'Customer'
├─ StateCode (VARCHAR 50) -- 'Draft', 'Submitted', 'Approved', 'Rejected', 'Completed'
├─ DisplayText (NVARCHAR 100)
├─ IsInitial (BIT) -- First state when entity created
├─ IsFinal (BIT) -- Terminal state
├─ RequiredRoles (VARCHAR MAX, JSON) -- ["Manager", "Approver"]
├─ Indexes: (EntityType, StateCode), (NodePath)
└─ Rationale: Defines state machine for any entity type (Order, Invoice, Invoice, etc.)

Example states:
/1/                          Order states
├─ /1/1/                     Draft (initial=true)
├─ /1/2/                     Submitted (initial=false)
├─ /1/3/                     Approved (initial=false)
├─ /1/4/                     Rejected (initial=false, final=true)
├─ /1/5/                     Shipped (initial=false)
├─ /1/6/                     Delivered (initial=false, final=true)

EntityStateTransitions
├─ FK → EntityStates (FromStateId, ToStateId)
├─ EntityType (VARCHAR 50) -- Denormalized for query convenience
├─ FromStateCode, ToStateCode (VARCHAR 50)
├─ RequiredRole (VARCHAR 50) -- 'Manager', 'Approver', etc. (can be NULL for anyone)
├─ DisplayText (NVARCHAR 100) -- "Move to Approval"
├─ Indexes: (EntityType, FromStateCode, ToStateCode), (RequiredRole)
└─ Rationale: Defines which state transitions are allowed and who can perform them
```

---

### 1.4 Notification Templates (3 tables)

```sql
NotificationChannels
├─ ChannelCode (VARCHAR 50, unique) -- 'Email', 'SMS', 'WhatsApp', 'Push', 'InApp'
├─ DisplayText (NVARCHAR 100)
├─ IsActive (BIT)
└─ Indexes: ChannelCode

TemplateGroups (event classifications)
├─ EventCode (VARCHAR 100, unique) -- 'UserWelcome', 'OrderConfirmation', 'InvoiceGenerated'
├─ DisplayText (NVARCHAR 100)
├─ Description (NVARCHAR MAX)
└─ Indexes: EventCode

Templates (multi-channel, multi-language)
├─ FK → TemplateGroups (TemplateGroupId)
├─ FK → Languages (LanguageId)
├─ EventCode (VARCHAR 100, denormalized)
├─ Channel (VARCHAR 50) -- Email, SMS, WhatsApp, Push, InApp
├─ Subject (NVARCHAR 255) -- For Email
├─ Body (NVARCHAR MAX) -- Supports {{Variable}} placeholders
├─ PlaceholderSchema (VARCHAR MAX, JSON) -- {"UserName": "string", "OrderId": "guid", ...}
├─ TenantId (GUID, nullable) -- Global templates + tenant overrides
├─ VersionNumber (INT) -- For versioning
├─ IsActive (BIT)
├─ Indexes: (TemplateGroupId, LanguageId, Channel), (EventCode, Channel, TenantId)
└─ Rationale: Single template table for ALL channels (Email, SMS, WhatsApp, Push, InApp)
```

**Example:**
```
TemplateGroupId=1 (UserWelcome)
├─ Template: Channel=Email, Language=en-US
│  Subject: "Welcome {{UserName}}!"
│  Body: "Dear {{UserName}}, Welcome to {{CompanyName}}..."
├─ Template: Channel=Email, Language=es-ES
│  Subject: "¡Bienvenido {{UserName}}!"
│  Body: "Estimado {{UserName}}, Bienvenido a {{CompanyName}}..."
├─ Template: Channel=SMS, Language=en-US
│  Body: "Hi {{UserName}}, Welcome to {{CompanyName}}"
└─ Template: Channel=Push, Language=en-US
   Body: "Welcome {{UserName}}!"
```

---

### 1.5 SaaS & Config (2 tables)

```sql
SubscriptionPlans
├─ PlanCode (VARCHAR 50, unique) -- 'Starter', 'Professional', 'Enterprise'
├─ DisplayText (NVARCHAR 100)
├─ Description (NVARCHAR MAX)
├─ MonthlyPrice (DECIMAL 10,2)
├─ AnnualPrice (DECIMAL 10,2, nullable)
├─ MaxUsers (INT) -- -1 for unlimited
├─ MaxProjects (INT)
├─ MaxStorageMB (BIGINT) -- -1 for unlimited
├─ FeaturesJson (VARCHAR MAX, JSON) -- {"AdvancedReporting": true, "CustomDomain": false, ...}
├─ IsActive (BIT)
├─ DisplayOrder (INT)
└─ Indexes: PlanCode

PreferenceDefinitions (extensible user/tenant preferences)
├─ PreferenceKey (VARCHAR 100, unique) -- 'Theme', 'Language', 'TimeZone', 'NotificationFrequency'
├─ DataType (VARCHAR 50) -- 'string', 'int', 'bool', 'datetime', 'json'
├─ DefaultValue (NVARCHAR MAX)
├─ AllowedValues (VARCHAR MAX, JSON) -- ["Light", "Dark", "Auto"]
├─ Scope (VARCHAR 50) -- 'System', 'User', 'Tenant'
├─ Description (NVARCHAR 500)
└─ Indexes: PreferenceKey
```

---

### 1.6 SEO & URL Management (2 tables)

```sql
SeoMeta (per-entity SEO metadata)
├─ EntityType (VARCHAR 50) -- 'Product', 'BlogPost', 'Page'
├─ EntityId (UNIQUEIDENTIFIER)
├─ MetaTitle (NVARCHAR 255)
├─ MetaDescription (NVARCHAR 500)
├─ MetaKeywords (NVARCHAR MAX)
├─ OgTitle, OgDescription, OgImage (for social sharing)
├─ TwitterCard (VARCHAR 50) -- 'summary', 'summary_large_image', 'app', 'player'
├─ StructuredData (VARCHAR MAX, JSON) -- Schema.org markup
├─ TenantId (GUID, nullable)
└─ Indexes: (EntityType, EntityId), (TenantId)

UrlRedirects
├─ FromPath (VARCHAR 500, unique) -- '/old-product-page'
├─ ToPath (VARCHAR 500) -- '/new-product-page'
├─ RedirectCode (INT) -- 301 (permanent), 302 (temporary)
├─ IsActive (BIT)
├─ HitCount (BIGINT) -- Track redirect usage
├─ CreatedAt (DATETIME2)
├─ Indexes: FromPath, ToPath
└─ Rationale: Handle URL changes, prevent SEO loss
```

---

### 1.7 Logging (2 tables)

```sql
AuditLogs (Master schema level)
├─ Id (BIGINT IDENTITY) -- High volume
├─ EntityType (VARCHAR 100) -- 'User', 'Product', 'Order'
├─ EntityId (UNIQUEIDENTIFIER)
├─ Action (VARCHAR 50) -- 'Create', 'Update', 'Delete'
├─ ChangedValues (VARCHAR MAX, JSON) -- {"FirstName": {"Old": "John", "New": "Jane"}, ...}
├─ PerformedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ Timestamp (DATETIME2)
├─ IpAddress (VARCHAR 50)
├─ UserAgent (VARCHAR MAX)
├─ Indexes: (EntityType, EntityId), (PerformedBy), (Timestamp)
└─ Rationale: Master-level audits for cross-schema entities, analytics

ActivityLogs (Master schema, usage tracking)
├─ Id (BIGINT IDENTITY)
├─ UserId (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ TenantId (UNIQUEIDENTIFIER, FK → Core.Tenants)
├─ ActivityType (VARCHAR 50) -- 'Login', 'ViewPage', 'DownloadFile', 'ExportData'
├─ ResourceType (VARCHAR 100) -- 'Product', 'Order', etc.
├─ ResourceId (UNIQUEIDENTIFIER)
├─ Details (VARCHAR MAX, JSON)
├─ CreatedAt (DATETIME2)
├─ Indexes: (UserId, TenantId), (ActivityType), (CreatedAt)
└─ Rationale: Usage analytics, engagement tracking, compliance logging
```

---

## 2. Core Schema (18 tables)

### Purpose
Project core configuration, business entities, and shared infrastructure for polymorphic linking.

---

### 2.1 Tenant Configuration (4 tables)

```sql
Tenants (HierarchyId tree)
├─ NodePath (HierarchyId) -- Supports agency → client → sub-client hierarchies
├─ TenantCode (VARCHAR 100, unique) -- 'acme-corp', 'acme-corp-us', 'acme-corp-us-ca'
├─ DisplayName (NVARCHAR 200)
├─ Subdomain (VARCHAR 100, unique) -- For tenant isolation (acme.app.com, subsidiary.app.com)
├─ CustomDomain (NVARCHAR 255, unique, nullable) -- Custom domain (acme.com)
├─ LogoUrl (NVARCHAR 500) -- Branding
├─ PrimaryColor (VARCHAR 10) -- Hex color
├─ AccentColor (VARCHAR 10)
├─ IsActive (BIT)
├─ Indexes: (NodePath), (TenantCode), (Subdomain)
└─ Rationale: HierarchyId enables multi-level tenant hierarchies (agencies with sub-clients)

TenantSubscriptions
├─ FK → Tenants (TenantId)
├─ FK → Master.SubscriptionPlans (SubscriptionPlanId)
├─ SubscriptionStartDate (DATETIME2)
├─ SubscriptionEndDate (DATETIME2)
├─ Status (VARCHAR 50) -- 'Active', 'Suspended', 'Expired', 'Cancelled'
├─ AutoRenew (BIT)
├─ Notes (NVARCHAR MAX)
└─ Indexes: (TenantId, Status)

TenantSettings (key-value store, replacing v1's 3-table SettingCategories/Definitions/Values)
├─ FK → Tenants (TenantId)
├─ SettingKey (VARCHAR 255) -- 'EmailFromAddress', 'TimeZone', 'DateFormat'
├─ SettingValue (NVARCHAR MAX) -- Supports: string, int, bool, datetime, json
├─ SettingType (VARCHAR 50) -- 'string', 'int', 'bool', 'datetime', 'list<string>', 'json'
├─ IsEncrypted (BIT) -- For sensitive settings (API keys, passwords)
├─ Indexes: (TenantId, SettingKey)
└─ Rationale: Flexible key-value store, easier to extend than v1's rigid 3-table approach

FeatureFlags (tenant-scoped feature toggles)
├─ FK → Tenants (TenantId, nullable for global flags)
├─ FeatureName (VARCHAR 100) -- 'AdvancedReporting', 'CustomDomain', 'TwoFactorAuth'
├─ IsEnabled (BIT)
├─ RolloutPercent (INT) -- 0-100 for gradual rollout
├─ ValidFrom (DATETIME2, nullable)
├─ ValidTo (DATETIME2, nullable)
├─ Indexes: (TenantId, FeatureName)
└─ Rationale: Enable/disable features per tenant without code deployment
```

---

### 2.2 Business Entities (9 dummy/placeholder tables for starter kit)

```sql
Products
├─ ProductCode (VARCHAR 100, unique)
├─ ProductName (NVARCHAR 255)
├─ Description (NVARCHAR MAX)
├─ Category (VARCHAR 100) -- Link to Categories table (polymorphic)
├─ Price (DECIMAL 10,2)
├─ CostPrice (DECIMAL 10,2)
├─ StockQuantity (INT)
├─ Sku (VARCHAR 100, unique)
├─ Status (VARCHAR 50) -- 'Active', 'Discontinued', 'Draft'
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
└─ Indexes: (ProductCode), (Sku), (TenantId, Status)

Customers
├─ CustomerCode (VARCHAR 100, unique)
├─ FirstName, LastName (NVARCHAR 100)
├─ Email (NVARCHAR 255, unique)
├─ Phone (VARCHAR 20)
├─ CustomerType (VARCHAR 50) -- 'Individual', 'Business'
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
└─ Indexes: (Email), (CustomerCode), (TenantId)

Vendors
├─ VendorCode (VARCHAR 100, unique)
├─ VendorName (NVARCHAR 255)
├─ ContactPerson (NVARCHAR 200)
├─ Email (NVARCHAR 255)
├─ Phone (VARCHAR 20)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
└─ Indexes: (VendorCode), (TenantId)

Projects (example business entity)
├─ ProjectCode (VARCHAR 100, unique)
├─ ProjectName (NVARCHAR 255)
├─ Description (NVARCHAR MAX)
├─ StartDate (DATE)
├─ EndDate (DATE, nullable)
├─ FK → Customers (CustomerId, nullable)
├─ Status (VARCHAR 50) -- 'Planning', 'InProgress', 'OnHold', 'Completed', 'Cancelled'
├─ Budget (DECIMAL 12,2)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
└─ Indexes: (ProjectCode), (TenantId, Status)

Teams, Departments, Employees (HR skeleton)
├─ Similar structure: Code, Name, TenantId, IsDeleted
├─ Relationships: Department → Team → Employee
└─ Used for organizational hierarchy in reporting, assignments

Assets (company assets)
├─ AssetCode (VARCHAR 100, unique)
├─ AssetName (NVARCHAR 255)
├─ AssetType (VARCHAR 50) -- 'Computer', 'Furniture', 'Vehicle'
├─ PurchaseDate (DATE)
├─ PurchasePrice (DECIMAL 12,2)
├─ AssignedTo (UNIQUEIDENTIFIER, FK → Employees, nullable)
├─ Status (VARCHAR 50) -- 'InStock', 'Assigned', 'Retired', 'Lost'
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
└─ Indexes: (AssetCode), (TenantId, Status)

Contracts (supplier/customer agreements)
├─ ContractCode (VARCHAR 100, unique)
├─ ContractName (NVARCHAR 255)
├─ FK → Vendors or Customers (PartyId, PartyType)
├─ StartDate (DATE)
├─ EndDate (DATE)
├─ Value (DECIMAL 12,2)
├─ Status (VARCHAR 50) -- 'Draft', 'Active', 'Expired', 'Terminated'
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
└─ Indexes: (ContractCode), (TenantId, Status)

Note: These are DUMMY/PLACEHOLDER tables. Real projects will add their own business entities.
The important part is the pattern: (Code, Name, Description, Status, TenantId, soft delete columns).
```

---

### 2.3 Shared Infrastructure (5 polymorphic tables)

These tables use the **EntityType + EntityId pattern** to link to any entity in any schema.

```sql
Addresses (polymorphic linking via EntityType+EntityId)
├─ EntityType (VARCHAR 50) -- 'Customer', 'Vendor', 'Employee', 'Company', 'Tenant'
├─ EntityId (UNIQUEIDENTIFIER) -- FK to corresponding entity (e.g., Customers.Id, Vendors.Id)
├─ AddressType (VARCHAR 50) -- 'Billing', 'Shipping', 'Home', 'Office'
├─ Street1 (NVARCHAR 255)
├─ Street2 (NVARCHAR 255, nullable)
├─ City (NVARCHAR 100)
├─ FK → Master.States (StateId, nullable)
├─ FK → Master.Countries (CountryId)
├─ PostalCode (VARCHAR 20)
├─ Latitude (DECIMAL 10,8, nullable)
├─ Longitude (DECIMAL 11,8, nullable)
├─ IsDefault (BIT)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (EntityType, EntityId), (TenantId), (CountryId, StateId)
└─ Rationale: Single table for ALL addresses (customers, vendors, employees, etc.)

Attachments (polymorphic file references)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice', 'Project', 'Contract'
├─ EntityId (UNIQUEIDENTIFIER)
├─ FileName (NVARCHAR 255)
├─ FileUrl (NVARCHAR 500) -- S3, Azure Blob, or local path
├─ FileSizeBytes (BIGINT)
├─ FileType (VARCHAR 50) -- 'pdf', 'docx', 'image', 'spreadsheet'
├─ UploadedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ UploadedAt (DATETIME2)
├─ IsDeleted, soft delete columns
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (EntityType, EntityId), (TenantId)
└─ Rationale: Single table for ALL attachments (not separate OrderAttachments, InvoiceAttachments, etc.)

Tags (polymorphic tagging)
├─ EntityType (VARCHAR 50)
├─ EntityId (UNIQUEIDENTIFIER)
├─ TagName (NVARCHAR 100)
├─ TagCategory (VARCHAR 50, nullable) -- 'Priority', 'Status', 'Owner', 'Department'
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (EntityType, EntityId), (TenantId, TagName)
└─ Rationale: Tag any entity without adding columns to source table

Comments (polymorphic commenting system)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice', 'Project', 'Employee'
├─ EntityId (UNIQUEIDENTIFIER)
├─ CommentText (NVARCHAR MAX)
├─ CommentedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ CommentedAt (DATETIME2)
├─ ParentCommentId (UNIQUEIDENTIFIER, nullable) -- For nested comments/replies
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (EntityType, EntityId), (TenantId, CommentedAt)
└─ Rationale: Single comment system for all entities (no OrderComments, InvoiceComments tables)

StateHistory (polymorphic state machine tracking)
├─ EntityType (VARCHAR 50) -- 'Order', 'Invoice'
├─ EntityId (UNIQUEIDENTIFIER)
├─ FromStateCode (VARCHAR 50)
├─ ToStateCode (VARCHAR 50)
├─ ChangedBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ ChangedAt (DATETIME2)
├─ ChangeReason (NVARCHAR MAX)
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (EntityType, EntityId, ChangedAt), (TenantId)
└─ Rationale: Track state changes for any entity (Order → Approved → Shipped)
```

**Example Usage:**
```
EntityType='Order', EntityId=123ABC
├─ Address (Shipping)
├─ Address (Billing)
├─ Attachment (Invoice PDF)
├─ Attachment (Packing Slip)
├─ Tag (VIP Customer)
├─ Tag (Rush Order)
├─ Comment: "Ship ASAP"
├─ StateHistory: Draft → Submitted → Approved → Shipped
```

---

## 3. Transaction Schema (8 tables)

### Purpose
Transactional data: orders, invoices, payments, purchase orders, receipts, credit notes.

---

```sql
Orders
├─ OrderNumber (VARCHAR 100, unique)
├─ FK → Customers (CustomerId)
├─ OrderDate (DATETIME2)
├─ DeliveryDate (DATE, nullable)
├─ TotalAmount (DECIMAL 12,2)
├─ Status (VARCHAR 50) -- 'Draft', 'Submitted', 'Approved', 'Shipped', 'Delivered'
├─ Notes (NVARCHAR MAX)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (OrderNumber), (CustomerId), (TenantId, Status), (OrderDate)
└─ Rationale: Parent record for order line items

OrderLines
├─ FK → Orders (OrderId)
├─ FK → Products (ProductId)
├─ Quantity (DECIMAL 10,2)
├─ UnitPrice (DECIMAL 10,2)
├─ LineTotal (DECIMAL 12,2) -- Quantity × UnitPrice
├─ Discount (DECIMAL 10,2, nullable)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (OrderId)
└─ Rationale: Line items for an order

Invoices
├─ InvoiceNumber (VARCHAR 100, unique)
├─ FK → Customers (CustomerId)
├─ FK → Orders (OrderId, nullable) -- Can invoice without order (direct invoicing)
├─ InvoiceDate (DATE)
├─ DueDate (DATE)
├─ TotalAmount (DECIMAL 12,2)
├─ PaidAmount (DECIMAL 12,2)
├─ Status (VARCHAR 50) -- 'Draft', 'Issued', 'Overdue', 'Paid', 'Cancelled'
├─ Notes (NVARCHAR MAX)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (InvoiceNumber), (CustomerId), (TenantId, Status), (DueDate)
└─ Rationale: Bill customer for goods/services

Payments
├─ PaymentNumber (VARCHAR 100, unique)
├─ FK → Customers (CustomerId)
├─ FK → Invoices (InvoiceId)
├─ PaymentDate (DATE)
├─ Amount (DECIMAL 12,2)
├─ PaymentMethod (VARCHAR 50) -- 'CreditCard', 'BankTransfer', 'Check', 'Cash'
├─ TransactionReference (VARCHAR 255, nullable)
├─ Status (VARCHAR 50) -- 'Pending', 'Confirmed', 'Failed', 'Reversed'
├─ Notes (NVARCHAR MAX)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (PaymentNumber), (CustomerId), (TenantId, Status), (PaymentDate)
└─ Rationale: Record customer payments

PurchaseOrders
├─ PoNumber (VARCHAR 100, unique)
├─ FK → Vendors (VendorId)
├─ PoDate (DATE)
├─ ExpectedDeliveryDate (DATE, nullable)
├─ TotalAmount (DECIMAL 12,2)
├─ Status (VARCHAR 50) -- 'Draft', 'Submitted', 'Confirmed', 'Received', 'Closed'
├─ Notes (NVARCHAR MAX)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (PoNumber), (VendorId), (TenantId, Status)
└─ Rationale: Parent record for purchase order line items

PurchaseOrderLines
├─ FK → PurchaseOrders (PurchaseOrderId)
├─ FK → Products (ProductId)
├─ Quantity (DECIMAL 10,2)
├─ UnitPrice (DECIMAL 10,2)
├─ LineTotal (DECIMAL 12,2)
├─ Discount (DECIMAL 10,2, nullable)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (PurchaseOrderId)
└─ Rationale: Line items for a purchase order

Receipts
├─ ReceiptNumber (VARCHAR 100, unique)
├─ FK → PurchaseOrders (PurchaseOrderId)
├─ ReceiptDate (DATE)
├─ ReceivedQuantity (DECIMAL 10,2)
├─ Status (VARCHAR 50) -- 'Pending', 'PartiallyReceived', 'FullyReceived'
├─ Notes (NVARCHAR MAX)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (ReceiptNumber), (PurchaseOrderId)
└─ Rationale: Record receipt of goods from vendor

CreditNotes
├─ CreditNoteNumber (VARCHAR 100, unique)
├─ FK → Invoices (InvoiceId)
├─ FK → Customers (CustomerId)
├─ CreditNoteDate (DATE)
├─ Amount (DECIMAL 12,2)
├─ Reason (VARCHAR 100) -- 'ReturnedGoods', 'PriceAdjustment', 'Discount'
├─ Notes (NVARCHAR MAX)
├─ Status (VARCHAR 50) -- 'Draft', 'Issued', 'Applied'
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (CreditNoteNumber), (InvoiceId), (CustomerId)
└─ Rationale: Credit customer for returned goods or adjustments
```

---

## 4. Report Schema (5 tables)

### Purpose
Reporting infrastructure: define reports, schedule runs, cache results, track audit.

---

```sql
ReportDefinitions
├─ ReportCode (VARCHAR 100, unique)
├─ ReportName (NVARCHAR 255)
├─ Description (NVARCHAR MAX)
├─ QuerySql (VARCHAR MAX) -- Raw SQL or stored procedure name
├─ Parameters (VARCHAR MAX, JSON) -- [{"Name": "StartDate", "Type": "date"}, ...]
├─ TenantId (UNIQUEIDENTIFIER, nullable) -- NULL=system report, GUID=tenant-specific
├─ IsActive (BIT)
├─ Indexes: (ReportCode, TenantId)
└─ Rationale: Define report structure without hardcoding

ReportSchedules
├─ FK → ReportDefinitions (ReportDefinitionId)
├─ ScheduleName (NVARCHAR 255)
├─ CronExpression (VARCHAR 100) -- "0 9 * * *" (daily at 9am)
├─ EmailRecipients (VARCHAR MAX, JSON) -- ["user1@example.com", "user2@example.com"]
├─ NextRunTime (DATETIME2)
├─ LastRunTime (DATETIME2, nullable)
├─ IsActive (BIT)
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (ReportDefinitionId, TenantId)
└─ Rationale: Schedule automated report generation and email delivery

ReportResults
├─ FK → ReportDefinitions (ReportDefinitionId)
├─ RunDate (DATETIME2)
├─ ResultData (VARCHAR MAX, JSON) -- Cached report data
├─ RowCount (INT)
├─ ExecutionTimeMs (INT)
├─ Status (VARCHAR 50) -- 'Success', 'Failed', 'Timeout'
├─ ErrorMessage (VARCHAR MAX, nullable)
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (ReportDefinitionId, RunDate), (TenantId)
└─ Rationale: Cache report results for quick retrieval, audit runs

ReportAuditLogs
├─ FK → ReportDefinitions (ReportDefinitionId)
├─ RunDate (DATETIME2)
├─ RunBy (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ ParameterValues (VARCHAR MAX, JSON)
├─ RowsReturned (INT)
├─ ExecutionTimeMs (INT)
├─ ExportFormat (VARCHAR 50) -- 'Excel', 'Pdf', 'Csv', 'Json'
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (ReportDefinitionId, RunDate), (RunBy)
└─ Rationale: Audit who ran what reports and when

DashboardWidgets
├─ WidgetCode (VARCHAR 100, unique)
├─ WidgetName (NVARCHAR 255)
├─ WidgetType (VARCHAR 50) -- 'Chart', 'KPI', 'Table', 'Gauge', 'Trend'
├─ Query (VARCHAR MAX) -- SQL or API endpoint
├─ RefreshIntervalSeconds (INT) -- How often to refresh data
├─ Configuration (VARCHAR MAX, JSON) -- Chart options, colors, etc.
├─ TenantId (UNIQUEIDENTIFIER, nullable)
├─ DisplayOrder (INT)
├─ IsActive (BIT)
├─ Indexes: (WidgetCode, TenantId)
└─ Rationale: Reusable dashboard components
```

---

## 5. Auth Schema (13 tables)

### Purpose
Identity, RBAC, sessions, verification, external logins, audit/activity logging.

---

```sql
Users
├─ Email (NVARCHAR 255, unique)
├─ FirstName (NVARCHAR 100)
├─ LastName (NVARCHAR 100)
├─ PasswordHash (VARCHAR 255)
├─ PasswordSalt (VARCHAR 255)
├─ PasswordChangedAt (DATETIME2, nullable)
├─ IsEmailConfirmed (BIT)
├─ EmailConfirmedAt (DATETIME2, nullable)
├─ PhoneNumber (VARCHAR 20, nullable)
├─ IsPhoneNumberConfirmed (BIT)
├─ PhoneConfirmedAt (DATETIME2, nullable)
├─ MfaEnabled (BIT)
├─ MfaSecret (NVARCHAR 255, nullable) -- For authenticator apps (TOTP)
├─ AccountStatus (VARCHAR 50) -- 'Active', 'Inactive', 'Locked', 'Suspended'
├─ AccountLockedUntil (DATETIME2, nullable)
├─ FailedLoginAttempts (INT)
├─ LastLoginAt (DATETIME2, nullable)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (Email), (TenantId, AccountStatus)
└─ Rationale: Core user identity

UserProfiles (split from Users for flexibility)
├─ FK → Users (UserId)
├─ MiddleName (NVARCHAR 100, nullable)
├─ DateOfBirth (DATE, nullable)
├─ Gender (VARCHAR 10, nullable)
├─ ProfilePictureUrl (NVARCHAR 500, nullable)
├─ Department (NVARCHAR 100, nullable)
├─ JobTitle (NVARCHAR 100, nullable)
├─ Manager (UNIQUEIDENTIFIER, FK → Users, nullable)
├─ PreferredLanguage (VARCHAR 10) -- 'en-US', 'es-ES', etc.
├─ PreferredTimeZone (VARCHAR 50) -- 'America/New_York', 'Europe/London'
├─ NotifyEmail (BIT) -- User prefers email notifications
├─ NotifySms (BIT)
├─ NotifyPush (BIT)
├─ NotifyWhatsApp (BIT)
├─ Indexes: (UserId)
└─ Rationale: Extended profile, notification preferences

UserPreferences
├─ FK → Users (UserId)
├─ FK → Master.PreferenceDefinitions (PreferenceDefinitionId)
├─ PreferenceValue (NVARCHAR MAX)
├─ Indexes: (UserId)
└─ Rationale: Store user preferences (Theme='Dark', Language='es-ES', etc.)

Roles
├─ RoleCode (VARCHAR 100, unique)
├─ RoleName (NVARCHAR 100)
├─ Description (NVARCHAR 500)
├─ IsSystem (BIT) -- TRUE for 'Admin', 'User', 'Guest' (cannot delete)
├─ TenantId (UNIQUEIDENTIFIER, nullable) -- NULL=system roles, GUID=tenant-specific
├─ IsActive (BIT)
├─ Indexes: (RoleCode, TenantId)
└─ Rationale: Define user roles

Permissions
├─ PermissionCode (VARCHAR 100, unique)
├─ DisplayText (NVARCHAR 100)
├─ Module (VARCHAR 50) -- 'Users', 'Reports', 'Products', 'Orders'
├─ Action (VARCHAR 50) -- 'View', 'Create', 'Edit', 'Delete', 'Approve', 'Export'
├─ Description (NVARCHAR 500)
├─ IsSystem (BIT)
├─ Indexes: (PermissionCode), (Module, Action)
└─ Rationale: Replace v1's Claims model with explicit Permissions

RolePermissions (junction table)
├─ FK → Roles (RoleId)
├─ FK → Permissions (PermissionId)
└─ Rationale: Many-to-many relationship

UserRoles (junction table)
├─ FK → Users (UserId)
├─ FK → Roles (RoleId)
├─ AssignedAt (DATETIME2)
├─ AssignedBy (UNIQUEIDENTIFIER, FK → Users)
├─ Indexes: (UserId), (RoleId)
└─ Rationale: Users can have multiple roles

RefreshTokens
├─ FK → Users (UserId)
├─ Token (VARCHAR 500, unique) -- JWT refresh token (hashed)
├─ ExpiryDate (DATETIME2)
├─ DeviceInfo (NVARCHAR 500, nullable) -- Device/OS info for audit
├─ IpAddress (VARCHAR 50)
├─ IsRevoked (BIT)
├─ RevokedAt (DATETIME2, nullable)
├─ Indexes: (UserId, ExpiryDate)
└─ Rationale: Manage long-lived refresh tokens, revoke per device

VerificationCodes
├─ FK → Users (UserId)
├─ CodeType (VARCHAR 50) -- 'EmailVerification', 'PhoneVerification', 'PasswordReset', 'TwoFactor'
├─ Code (VARCHAR 10) -- 6-digit OTP or random string
├─ ExpiryDate (DATETIME2)
├─ IsUsed (BIT)
├─ UsedAt (DATETIME2, nullable)
├─ Purpose (NVARCHAR 255, nullable)
├─ Indexes: (UserId, CodeType, ExpiryDate)
└─ Rationale: OTP for email/phone verification, password reset, 2FA

ExternalLogins
├─ FK → Users (UserId)
├─ Provider (VARCHAR 50) -- 'Google', 'Microsoft', 'GitHub', 'Facebook'
├─ ProviderUserId (VARCHAR 255)
├─ ProviderEmail (NVARCHAR 255)
├─ LinkedAt (DATETIME2)
├─ IsActive (BIT)
├─ Indexes: (UserId), (Provider, ProviderUserId)
└─ Rationale: Link OAuth providers to users

AuditLogs (Auth schema level, for auth-specific events)
├─ Id (BIGINT IDENTITY)
├─ UserId (UNIQUEIDENTIFIER, nullable, FK → Users)
├─ EventType (VARCHAR 50) -- 'Login', 'Logout', 'ChangePassword', 'UpdateProfile', 'RoleAssignment'
├─ EventDetails (VARCHAR MAX, JSON)
├─ IpAddress (VARCHAR 50)
├─ UserAgent (VARCHAR MAX)
├─ Timestamp (DATETIME2)
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (UserId, Timestamp), (EventType), (TenantId)
└─ Rationale: Auth-specific audit trail

ActivityLogs (Auth schema level, for usage tracking)
├─ Id (BIGINT IDENTITY)
├─ UserId (UNIQUEIDENTIFIER, FK → Users)
├─ ActivityType (VARCHAR 50) -- 'PageView', 'DataExport', 'ReportGeneration', 'FileDownload'
├─ ResourceType (VARCHAR 100)
├─ ResourceId (UNIQUEIDENTIFIER, nullable)
├─ Details (VARCHAR MAX, JSON)
├─ CreatedAt (DATETIME2)
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (UserId, CreatedAt), (ActivityType)
└─ Rationale: Track user activity for engagement analytics

NotificationLogs (delivery tracking)
├─ Id (BIGINT IDENTITY)
├─ FK → Users (UserId)
├─ Channel (VARCHAR 50) -- 'Email', 'SMS', 'Push', 'WhatsApp', 'InApp'
├─ EventType (VARCHAR 50) -- 'UserWelcome', 'OrderConfirmation', etc.
├─ Recipient (NVARCHAR 255) -- Email, phone, device ID
├─ Subject (NVARCHAR 255)
├─ Body (NVARCHAR MAX)
├─ Status (VARCHAR 50) -- 'Queued', 'Sent', 'Delivered', 'Failed', 'Bounced'
├─ ErrorMessage (VARCHAR MAX, nullable)
├─ ProviderReference (VARCHAR 255, nullable) -- Email service message ID, etc.
├─ SentAt (DATETIME2, nullable)
├─ DeliveredAt (DATETIME2, nullable)
├─ TenantId (UNIQUEIDENTIFIER)
├─ Indexes: (UserId, Channel, CreatedAt), (Status)
└─ Rationale: Track notification delivery (different from Notifications inbox)
```

---

## 6. Sales Schema (1 table + extensible)

### Purpose
Example team-specific schema. Projects can add Marketing, HR, Finance schemas as needed.

---

```sql
SalesOrders
├─ OrderNumber (VARCHAR 100, unique)
├─ FK → Core.Customers (CustomerId)
├─ FK → Core.Products (ProductId, nullable)
├─ SalesPersonId (UNIQUEIDENTIFIER, FK → Auth.Users)
├─ OrderDate (DATE)
├─ ExpectedCloseDate (DATE)
├─ Probability (INT) -- 0-100, for sales pipeline analysis
├─ Amount (DECIMAL 12,2)
├─ Status (VARCHAR 50) -- 'Pipeline', 'Qualified', 'Proposal', 'Negotiation', 'Won', 'Lost'
├─ Notes (NVARCHAR MAX)
├─ TenantId (UNIQUEIDENTIFIER)
├─ IsDeleted, soft delete columns
├─ Indexes: (OrderNumber), (SalesPersonId), (TenantId, Status)
└─ Rationale: Sales pipeline tracking (separate from Transaction.Orders for accounting)
```

---

## Summary Table

| Schema | Table Count | TenantId Strategy | Key Pattern |
|--------|-------------|-------------------|-------------|
| **Master** | 17 | NULLABLE (global + overrides) | Reference data (Countries, Languages, Lookups, Templates) |
| **Core** | 18 | NOT NULL | Config (Tenants, Settings, Features) + Business (Products, Customers) + Shared (Addresses, Tags, Comments) |
| **Transaction** | 8 | NOT NULL | Orders, Invoices, Payments, POs (financial/operational data) |
| **Report** | 5 | NULLABLE | Reporting infrastructure (Definitions, Results, Widgets) |
| **Auth** | 13 | NOT NULL | Users, Roles, Permissions, Sessions, Logs |
| **Sales** | 1 | NOT NULL | Team-specific (Sales pipeline) |
| **TOTAL** | **62** | | |

---

## Key Design Principles

### ✅ Already Implemented

1. **Polymorphic linking** (Addresses, Attachments, Tags, Comments, StateHistory)
   - EntityType + EntityId allows linking to any entity
   - Single table instead of separate Address/Contact/User tables

2. **HierarchyId trees** (Lookups, Categories, EntityStates, Tenants)
   - Unlimited nesting depth
   - Efficient ancestor/descendant queries

3. **Soft delete** (IsDeleted, DeletedAt, DeletedBy on business entities)
   - Reversible deletion
   - Maintains referential integrity

4. **Audit columns** (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
   - Who made changes and when
   - Compliance/traceability

5. **Multi-tenancy** (TenantId on all tables except Master)
   - Row-level tenant isolation
   - Secure data segregation

6. **Flexible configuration** (TenantSettings key-value, FeatureFlags)
   - No hardcoding of settings
   - Easy customization per tenant

7. **State machines** (EntityStates, EntityStateTransitions, StateHistory)
   - Model any workflow
   - Track state changes over time

8. **Template system** (Templates, multi-channel)
   - Email, SMS, WhatsApp, Push, InApp
   - Global + tenant-specific templates
   - Multi-language support

---

## Phase 1+ Enhancements (FUTURE, not in v4.0)

**Phase 1+ (v4.1):** Workflows, WorkflowInstances, WorkflowApprovals, Notifications inbox, Logs app events, ApiKeys, AuditTrail (column-level)
**Phase 2+ (v4.2):** Wishlists, Reviews, Coupons, CouponUsage, Bundles, BundleItems, ShippingMethods, ShippingRates, WarehouseLocations, StockMovements, StockAdjustments, Queues, QueueItems
**Phase 3+ (v4.3+):** Industry-specific (CRM, Accounting, Advanced Logistics)

---

## ✅ Ready for Review & Implementation

All 62 tables are documented with:
- ✅ Column definitions
- ✅ Relationships (FKs, HierarchyId patterns)
- ✅ Soft delete / audit columns
- ✅ Indexing strategy
- ✅ TenantId isolation strategy
- ✅ Polymorphic linking patterns
- ✅ Use cases and rationale

**Next step:** Proceed to Phase 1 - write SQL scripts (001-009).
