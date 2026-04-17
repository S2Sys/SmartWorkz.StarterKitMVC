# **V2 COMPLETE SCHEMA REFERENCE - ALL 41 TABLES**

## **SCHEMA OVERVIEW**

| Schema | Tables | Purpose | Key Features |
|--------|--------|---------|--------------|
| **Master** | 19 | Core configuration & master data | Multi-tenancy, hierarchical data (HierarchyID), lookup consolidation |
| **Shared** | 7 | Cross-cutting concerns | Polymorphic patterns, SEO, i18n, audit, file storage |
| **Transaction** | 1 | Financial operations | Payment/refund tracking |
| **Report** | 4 | Analytics & reporting | Report generation, event analytics, scheduling |
| **Auth** | 10 | Authentication & authorization | RBAC, token management, login tracking |
| **TOTAL** | **41** | Complete enterprise application | Production-ready starter kit |

---

## **MASTER SCHEMA (19 Tables)**

### **1. Master.Tenants**
**Purpose:** Multi-tenancy root  
**Usage:** All other tables reference this - core tenant configuration

```
Columns:
- TenantId (PK) NVARCHAR(128)
- Name NVARCHAR(256)
- DisplayName NVARCHAR(256)
- Description NVARCHAR(500)
- IsActive BIT
- Audit columns: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
```

**Example Queries:**
```sql
-- Create new tenant
INSERT INTO Master.Tenants VALUES ('ACME', 'ACME Corp', 'ACME Corporation', 'Main tenant', 1, GETUTCDATE(), 'system', NULL, NULL, 0);

-- Get active tenants
SELECT * FROM Master.Tenants WHERE IsActive = 1 AND IsDeleted = 0;
```

---

### **2. Master.Countries**
**Purpose:** Geographic reference data  
**Usage:** Country selection dropdowns, location validation

```
Columns:
- CountryId (PK) INT IDENTITY
- Code NVARCHAR(2) UNIQUE (US, GB, FR, etc.)
- Name NVARCHAR(100)
- DisplayName NVARCHAR(100)
- FlagEmoji NVARCHAR(10)
- TenantId NVARCHAR(128) FK → Tenants
- Audit columns
```

**Example Queries:**
```sql
-- Get countries for dropdown
SELECT CountryId, DisplayName, Code FROM Master.Countries 
WHERE IsActive = 1 AND IsDeleted = 0 
ORDER BY DisplayName;

-- Get by code
SELECT * FROM Master.Countries WHERE Code = 'US';
```

---

### **3. Master.Configuration**
**Purpose:** Application settings and configuration  
**Usage:** Store API keys, feature parameters, system settings

```
Columns:
- ConfigId (PK) INT IDENTITY
- Key NVARCHAR(256) (e.g., 'EmailSender:ApiKey', 'MaxUploadSize')
- Value NVARCHAR(MAX)
- ConfigType NVARCHAR(50) (String, Int, Bool, Decimal, Json)
- Description NVARCHAR(500)
- TenantId NVARCHAR(128) FK
- UNIQUE(TenantId, Key)
```

**Example Queries:**
```sql
-- Get config by key
SELECT Value FROM Master.Configuration 
WHERE [Key] = 'EmailSender:ApiKey' AND TenantId = 'ACME';

-- Get all configs for tenant
SELECT [Key], Value, ConfigType FROM Master.Configuration 
WHERE TenantId = 'ACME' AND IsActive = 1;
```

---

### **4. Master.FeatureFlags**
**Purpose:** Feature toggles for A/B testing, gradual rollouts  
**Usage:** Enable/disable features per tenant, gradual rollout management

```
Columns:
- FeatureFlagId (PK) INT IDENTITY
- Name NVARCHAR(256) (e.g., 'NewCheckout', 'BetaReporting')
- Description NVARCHAR(500)
- IsEnabled BIT
- TenantId NVARCHAR(128) FK
- ValidFrom DATETIME2 (when to activate)
- ValidTo DATETIME2 (when to deactivate)
```

**Example Queries:**
```sql
-- Check if feature is enabled
SELECT IsEnabled FROM Master.FeatureFlags 
WHERE Name = 'NewCheckout' AND TenantId = 'ACME' 
AND IsEnabled = 1 AND GETUTCDATE() BETWEEN ValidFrom AND ValidTo;

-- List all enabled features
SELECT Name FROM Master.FeatureFlags 
WHERE TenantId = 'ACME' AND IsEnabled = 1 AND IsDeleted = 0;
```

---

### **5. Master.Menus**
**Purpose:** Navigation menu definitions  
**Usage:** Define menu structures (Main, Footer, Sidebar, Admin)

```
Columns:
- MenuId (PK) INT IDENTITY
- Name NVARCHAR(256) (e.g., 'MainNav', 'FooterMenu')
- Description NVARCHAR(500)
- MenuType NVARCHAR(50) (Main, Footer, Sidebar, Admin)
- DisplayOrder INT
- TenantId NVARCHAR(128) FK
- UNIQUE(TenantId, Name)
```

**Example Queries:**
```sql
-- Get main navigation menu
SELECT MenuId, Name FROM Master.Menus 
WHERE MenuType = 'Main' AND TenantId = 'ACME' AND IsActive = 1;
```

---

### **6. Master.Categories**
**Purpose:** Hierarchical product/content categories  
**Usage:** Product categories, blog categories with unlimited nesting

```
Columns:
- CategoryId (PK) INT IDENTITY
- Name NVARCHAR(256)
- Slug NVARCHAR(256) UNIQUE(TenantId, Slug)
- Description NVARCHAR(MAX)
- NodePath HIERARCHYID (e.g., /1/, /1/1/, /1/1/1/)
- Level INT (computed: NodePath.GetLevel()) PERSISTED
- DisplayOrder INT
- Icon NVARCHAR(100)
- ImageUrl NVARCHAR(500)
- TenantId NVARCHAR(128) FK
```

**Example Queries:**
```sql
-- Get root categories
SELECT * FROM Master.Categories 
WHERE Level = 0 AND TenantId = 'ACME' AND IsActive = 1
ORDER BY DisplayOrder;

-- Get subcategories of /1/
SELECT * FROM Master.Categories 
WHERE Level = 1 AND TenantId = 'ACME' AND IsActive = 1
ORDER BY NodePath;

-- Get all descendants of /1/ (children, grandchildren, etc.)
SELECT * FROM Master.Categories 
WHERE NodePath.IsDescendantOf('/1/') = 1 AND TenantId = 'ACME'
ORDER BY NodePath;
```

---

### **7. Master.MenuItems**
**Purpose:** Hierarchical menu items  
**Usage:** Individual menu items with parent-child relationships

```
Columns:
- MenuItemId (PK) INT IDENTITY
- MenuId INT FK → Menus
- ParentMenuItemId INT FK → MenuItems (self-join)
- Title NVARCHAR(256)
- URL NVARCHAR(500)
- Icon NVARCHAR(100)
- DisplayOrder INT
- NodePath HIERARCHYID
- Level INT (computed) PERSISTED
- RequiredRole NVARCHAR(256) (role-based visibility)
- TenantId NVARCHAR(128) FK
```

**Example Queries:**
```sql
-- Get menu items for Main menu
SELECT * FROM Master.MenuItems 
WHERE MenuId = 1 AND IsActive = 1
ORDER BY NodePath;

-- Get root items only
SELECT * FROM Master.MenuItems 
WHERE MenuId = 1 AND Level = 0 AND IsActive = 1
ORDER BY DisplayOrder;

-- Get submenu items
SELECT * FROM Master.MenuItems 
WHERE NodePath.IsDescendantOf('/1/') = 1 AND MenuId = 1
ORDER BY NodePath;
```

---

### **8. Master.GeoHierarchy**
**Purpose:** Geographic hierarchy (Continent → Country → Region → City)  
**Usage:** Regional content, location-based features

```
Columns:
- GeoId (PK) INT IDENTITY
- ParentGeoId INT FK → GeoHierarchy (self-join)
- Name NVARCHAR(256) (e.g., 'North America', 'USA', 'California')
- GeoType NVARCHAR(50) (Continent, Country, Region, City)
- NodePath HIERARCHYID
- Level INT (computed) PERSISTED
- TenantId NVARCHAR(128) FK
```

**Example Queries:**
```sql
-- Get all regions
SELECT * FROM Master.GeoHierarchy 
WHERE GeoType = 'Region' AND IsActive = 1;

-- Get all children of USA
SELECT * FROM Master.GeoHierarchy 
WHERE NodePath.IsDescendantOf(
    (SELECT NodePath FROM Master.GeoHierarchy WHERE Name = 'USA')
) = 1;
```

---

### **9. Master.GeolocationPages**
**Purpose:** Region-specific content pages  
**Usage:** Landing pages per country, regional promotions

```
Columns:
- GeoPageId (PK) INT IDENTITY
- GeoId INT FK → GeoHierarchy
- Title NVARCHAR(256)
- Slug NVARCHAR(256) UNIQUE(TenantId, Slug)
- Content NVARCHAR(MAX)
- TenantId NVARCHAR(128) FK
```

**Example Queries:**
```sql
-- Get page for California
SELECT * FROM Master.GeolocationPages 
WHERE Slug = 'california-landing' AND TenantId = 'ACME';
```

---

### **10. Master.CustomPages**
**Purpose:** CMS pages (static content)  
**Usage:** About, Terms, Privacy, Contact pages

```
Columns:
- PageId (PK) INT IDENTITY
- Title NVARCHAR(256)
- Slug NVARCHAR(256) UNIQUE(TenantId, Slug)
- Content NVARCHAR(MAX)
- MetaDescription NVARCHAR(500)
- TenantId NVARCHAR(128) FK
```

**Example Queries:**
```sql
-- Get page by slug
SELECT * FROM Master.CustomPages 
WHERE Slug = 'about-us' AND TenantId = 'ACME';
```

---

### **11. Master.BlogPosts**
**Purpose:** Blog content management  
**Usage:** Blog posts with publishing workflow

```
Columns:
- PostId (PK) INT IDENTITY
- Title NVARCHAR(256)
- Slug NVARCHAR(256) UNIQUE(TenantId, Slug)
- Content NVARCHAR(MAX)
- Author NVARCHAR(256)
- PublishedAt DATETIME2
- TenantId NVARCHAR(128) FK
```

**Example Queries:**
```sql
-- Get published posts
SELECT * FROM Master.BlogPosts 
WHERE PublishedAt <= GETUTCDATE() AND IsActive = 1
ORDER BY PublishedAt DESC;

-- Get posts for year
SELECT * FROM Master.BlogPosts 
WHERE YEAR(PublishedAt) = 2026 AND TenantId = 'ACME'
ORDER BY PublishedAt DESC;
```

---

### **12. Master.CacheEntries**
**Purpose:** Distributed cache backend  
**Usage:** ASP.NET Core Distributed Cache (IDistributedCache)

```
Columns:
- Id NVARCHAR(449) PK
- Value VARBINARY(MAX)
- ExpiresAtTime DATETIMEOFFSET
- SlidingExpirationInSeconds BIGINT
- AbsoluteExpiration DATETIMEOFFSET
```

**Note:** Fixed schema required by Microsoft.Extensions.Caching.SqlServer

---

### **13. Master.ContentTemplates**
**Purpose:** Email, SMS, Push notification templates  
**Usage:** Reusable message templates for all notification types

```
Columns:
- Id NVARCHAR(256) PK
- Name NVARCHAR(256)
- Description NVARCHAR(500)
- TemplateType NVARCHAR(50) (Email, SMS, Push, Notification, Report)
- Subject NVARCHAR(500)
- HeaderId NVARCHAR(256) FK (optional)
- FooterId NVARCHAR(256) FK (optional)
- BodyContent NVARCHAR(MAX)
- PlainTextContent NVARCHAR(MAX)
- Tags NVARCHAR(MAX) (JSON array)
- Category NVARCHAR(100)
- TenantId NVARCHAR(128) FK
- Version INT
```

**Example Queries:**
```sql
-- Get email templates
SELECT * FROM Master.ContentTemplates 
WHERE TemplateType = 'Email' AND IsActive = 1 AND TenantId = 'ACME';

-- Get template by ID
SELECT * FROM Master.ContentTemplates WHERE Id = 'order-confirmation';
```

---

### **14. Master.ContentTemplateSections**
**Purpose:** Reusable template sections  
**Usage:** Header, Footer, Body sections that can be composed

```
Columns:
- Id NVARCHAR(256) PK
- Name NVARCHAR(256)
- SectionType NVARCHAR(50) (Header, Footer, Body)
- HtmlContent NVARCHAR(MAX)
- IsDefault BIT
- IsActive BIT
- TenantId NVARCHAR(128) FK
```

**Example Queries:**
```sql
-- Get default header section
SELECT HtmlContent FROM Master.ContentTemplateSections 
WHERE SectionType = 'Header' AND IsDefault = 1 AND TenantId = 'ACME';
```

---

### **15. Master.TemplatePlaceholders**
**Purpose:** Define template variables/placeholders  
**Usage:** List available placeholders in templates ({{FirstName}}, {{OrderNumber}})

```
Columns:
- PlaceholderId (PK) INT IDENTITY
- TemplateId NVARCHAR(256) FK → ContentTemplates
- PlaceholderKey NVARCHAR(256) (e.g., 'FirstName', 'OrderNumber')
- DisplayName NVARCHAR(256)
- Description NVARCHAR(500)
- DefaultValue NVARCHAR(500)
- SampleValue NVARCHAR(500)
- PlaceholderType NVARCHAR(50) (Text, Number, Date, Email, Url)
- IsRequired BIT
- DisplayOrder INT
- UNIQUE(TemplateId, PlaceholderKey)
```

**Example Queries:**
```sql
-- Get placeholders for order-confirmation template
SELECT PlaceholderKey, DisplayName, DefaultValue FROM Master.TemplatePlaceholders 
WHERE TemplateId = 'order-confirmation'
ORDER BY DisplayOrder;
```

---

### **16. Master.Lookup ⭐ NEW - Consolidated Lookups**
**Purpose:** Single hierarchical lookup table replacing Currencies, Languages, TimeZones  
**Usage:** All system-wide lookup values with unlimited nesting

```
Columns:
- IntId INT UNIQUE (1-1000 global, 1001+ tenant-specific)
- Id UNIQUEIDENTIFIER PK
- NodePath HIERARCHYID (e.g., /1/, /1/1/, /1/1/1/)
- Level INT (computed) PERSISTED
- CategoryKey NVARCHAR(100) ('currencies', 'languages', 'timezones', 'statuses', etc.)
- SubCategoryKey NVARCHAR(100) (optional for grouping)
- Key NVARCHAR(100) ('USD', 'en-US', 'America/New_York', etc.)
- DisplayName NVARCHAR(500)
- TenantId NVARCHAR(128) FK
- IsGlobalScope BIT (0 = tenant-specific, 1 = global)
- IsActive BIT
- IsDeleted BIT
- SortOrder INT
- Metadata NVARCHAR(MAX) (JSON: {"symbol":"$","decimalPlaces":2})
- LocalizedNames NVARCHAR(MAX) (JSON: {"de":"Dollar","fr":"Dollar"})
```

**ID Allocation:**
- **1-1000:** Master/global lookups (Currencies, Languages, TimeZones)
- **1001+:** Tenant-specific customizations

**Example Queries:**
```sql
-- Get all global currencies
SELECT * FROM Master.Lookup 
WHERE CategoryKey = 'currencies' AND IsGlobalScope = 1 AND IsActive = 1
ORDER BY SortOrder;

-- Get all languages (global + tenant-specific for ACME)
SELECT * FROM Master.Lookup 
WHERE CategoryKey = 'languages' 
  AND (IsGlobalScope = 1 OR TenantId = 'ACME')
  AND IsActive = 1
ORDER BY DisplayName;

-- Get timezones
SELECT * FROM Master.Lookup 
WHERE CategoryKey = 'timezones' AND IsActive = 1
ORDER BY DisplayName;

-- Get root currencies only
SELECT * FROM Master.Lookup 
WHERE CategoryKey = 'currencies' AND Level = 0 AND IsActive = 1;

-- Get all descendants of USD (e.g., USD-JPY exchange rates)
SELECT * FROM Master.Lookup 
WHERE CategoryKey = 'currencies' 
  AND NodePath.IsDescendantOf(
      (SELECT NodePath FROM Master.Lookup WHERE Key = 'USD')
  ) = 1;
```

---

## **SHARED SCHEMA (7 Tables)**

### **17. Shared.SeoMeta**
**Purpose:** SEO metadata for search engines  
**Usage:** Polymorphic - works with any entity (Products, BlogPosts, Pages)

```
Columns:
- SeoMetaId (PK) INT IDENTITY
- EntityType NVARCHAR(100) (e.g., 'Product', 'BlogPost', 'CustomPage')
- EntityId INT (references the entity)
- Slug NVARCHAR(256) UNIQUE(TenantId, Slug)
- Title NVARCHAR(256)
- Description NVARCHAR(500)
- Keywords NVARCHAR(500)
- StructuredData NVARCHAR(MAX) (JSON-LD schema.org)
- MetaRobots NVARCHAR(100) (index,follow or noindex,nofollow)
- CanonicalUrl NVARCHAR(500)
- OgTitle, OgDescription, OgImage (OpenGraph)
- TwitterCard, TwitterTitle, TwitterDescription, TwitterImage
- TenantId NVARCHAR(128) FK
- Indexes: (EntityType, EntityId), (Slug), (TenantId)
```

**Example Queries:**
```sql
-- Get SEO metadata for blog post
SELECT * FROM Shared.SeoMeta 
WHERE EntityType = 'BlogPost' AND EntityId = 123;

-- Get by slug
SELECT * FROM Shared.SeoMeta 
WHERE Slug = 'my-blog-post' AND TenantId = 'ACME';
```

---

### **18. Shared.Tags**
**Purpose:** Polymorphic tagging system  
**Usage:** Tag any entity (Products, Orders, BlogPosts)

```
Columns:
- TagId (PK) INT IDENTITY
- EntityType NVARCHAR(100)
- EntityId INT
- TagName NVARCHAR(256) (e.g., 'Featured', 'Sale', 'New')
- TagCategory NVARCHAR(100) (optional categorization)
- TenantId NVARCHAR(128) FK
- Indexes: (EntityType, EntityId), (TagName), (TenantId)
```

**Example Queries:**
```sql
-- Get all tags on product 123
SELECT TagName FROM Shared.Tags 
WHERE EntityType = 'Product' AND EntityId = 123;

-- Get all products with 'Featured' tag
SELECT DISTINCT EntityId FROM Shared.Tags 
WHERE EntityType = 'Product' AND TagName = 'Featured';
```

---

### **19. Shared.Translations**
**Purpose:** Multi-language content translations  
**Usage:** Translate any entity field to any language

```
Columns:
- TranslationId (PK) INT IDENTITY
- EntityType NVARCHAR(100) (e.g., 'Product', 'BlogPost')
- EntityId INT
- LanguageLookupId UNIQUEIDENTIFIER FK → Master.Lookup(Id)
  (References Lookup where CategoryKey = 'languages')
- FieldName NVARCHAR(256) (e.g., 'Title', 'Description', 'Content')
- TranslatedValue NVARCHAR(MAX)
- TenantId NVARCHAR(128) FK
- UNIQUE(TenantId, EntityType, EntityId, LanguageLookupId, FieldName)
```

**Example Queries:**
```sql
-- Get French translation of product title
SELECT TranslatedValue FROM Shared.Translations 
WHERE EntityType = 'Product' AND EntityId = 123
  AND LanguageLookupId = (
      SELECT Id FROM Master.Lookup 
      WHERE CategoryKey = 'languages' AND Key = 'fr-FR'
  )
  AND FieldName = 'Title';

-- Get all translations for a blog post
SELECT l.[Key], l.DisplayName, t.FieldName, t.TranslatedValue
FROM Shared.Translations t
JOIN Master.Lookup l ON t.LanguageLookupId = l.Id
WHERE t.EntityType = 'BlogPost' AND t.EntityId = 456
ORDER BY l.DisplayName, t.FieldName;
```

---

### **20. Shared.Notifications**
**Purpose:** System notifications  
**Usage:** In-app notifications, email, SMS alerts

```
Columns:
- NotificationId (PK) INT IDENTITY
- NotificationType NVARCHAR(100) (e.g., 'OrderConfirmed', 'ShipmentUpdate')
- RecipientType NVARCHAR(50) (User, Customer, Admin)
- RecipientId NVARCHAR(256)
- Subject NVARCHAR(256)
- Message NVARCHAR(MAX)
- IsRead BIT
- ReadAt DATETIME2
- TenantId NVARCHAR(128) FK
- Indexes: (RecipientType, RecipientId), (IsRead), (TenantId)
```

**Example Queries:**
```sql
-- Get unread notifications for user
SELECT * FROM Shared.Notifications 
WHERE RecipientId = 'user123' AND IsRead = 0
ORDER BY CreatedAt DESC;

-- Mark as read
UPDATE Shared.Notifications 
SET IsRead = 1, ReadAt = GETUTCDATE()
WHERE NotificationId = 1;
```

---

### **21. Shared.AuditLogs**
**Purpose:** Comprehensive audit trail  
**Usage:** Track all entity changes for compliance and debugging

```
Columns:
- AuditLogId (PK) INT IDENTITY
- EntityType NVARCHAR(100)
- EntityId INT
- Action NVARCHAR(50) (Create, Update, Delete)
- OldValues NVARCHAR(MAX) (JSON)
- NewValues NVARCHAR(MAX) (JSON)
- ChangedBy NVARCHAR(256)
- ChangedAt DATETIME2
- IPAddress NVARCHAR(45)
- TenantId NVARCHAR(128) FK
- Indexes: (EntityType, EntityId), (ChangedAt), (TenantId)
```

**Example Queries:**
```sql
-- Get audit history for an entity
SELECT * FROM Shared.AuditLogs 
WHERE EntityType = 'Product' AND EntityId = 123
ORDER BY ChangedAt DESC;

-- Get all changes by user
SELECT * FROM Shared.AuditLogs 
WHERE ChangedBy = 'john@acme.com'
ORDER BY ChangedAt DESC;
```

---

### **22. Shared.FileStorage**
**Purpose:** Document and file management  
**Usage:** Store file metadata (images, documents, attachments)

```
Columns:
- FileId (PK) INT IDENTITY
- FileName NVARCHAR(256)
- FileSize BIGINT
- MimeType NVARCHAR(100) (image/png, application/pdf, etc.)
- FilePath NVARCHAR(500) (disk or cloud path)
- EntityType NVARCHAR(100) (optional - Product, BlogPost, UserAvatar)
- EntityId INT (optional - if attached to entity)
- TenantId NVARCHAR(128) FK
- Indexes: (EntityType, EntityId), (TenantId)
```

**Example Queries:**
```sql
-- Get files for product
SELECT * FROM Shared.FileStorage 
WHERE EntityType = 'Product' AND EntityId = 123;

-- Get user avatar
SELECT FilePath FROM Shared.FileStorage 
WHERE EntityType = 'UserAvatar' AND EntityId = 1;
```

---

### **23. Shared.EmailQueue**
**Purpose:** Email delivery queue  
**Usage:** Reliable async email sending with retry logic

```
Columns:
- EmailQueueId (PK) INT IDENTITY
- ToEmail NVARCHAR(256)
- CcEmail NVARCHAR(500)
- BccEmail NVARCHAR(500)
- Subject NVARCHAR(256)
- Body NVARCHAR(MAX)
- IsHtml BIT
- Status NVARCHAR(50) (Pending, Sent, Failed, Cancelled)
- SendAttempts INT
- LastAttemptAt DATETIME2
- SentAt DATETIME2
- FailureReason NVARCHAR(500)
- TenantId NVARCHAR(128) FK
- Indexes: (Status), (CreatedAt), (TenantId)
```

**Example Queries:**
```sql
-- Get pending emails
SELECT TOP 100 * FROM Shared.EmailQueue 
WHERE Status = 'Pending' AND SendAttempts < 3
ORDER BY CreatedAt;

-- Get failed emails
SELECT * FROM Shared.EmailQueue 
WHERE Status = 'Failed'
ORDER BY CreatedAt DESC;
```

---

## **TRANSACTION SCHEMA (1 Table)**

### **24. Transaction.TransactionLog**
**Purpose:** Financial transaction tracking  
**Usage:** Log all payments, refunds, transfers

```
Columns:
- TransactionLogId (PK) BIGINT IDENTITY
- TransactionType NVARCHAR(50) (Payment, Refund, Transfer)
- EntityType NVARCHAR(100) (Order, Invoice, Account)
- EntityId INT
- Amount DECIMAL(18, 2)
- CurrencyLookupId UNIQUEIDENTIFIER FK → Master.Lookup(Id)
  (References Lookup where CategoryKey = 'currencies')
- Description NVARCHAR(500)
- Status NVARCHAR(50) (Pending, Completed, Failed, Cancelled)
- PaymentMethod NVARCHAR(100) (CreditCard, PayPal, BankTransfer)
- ReferenceNumber NVARCHAR(256) (external transaction ID)
- ProcessedAt DATETIME2
- CompletedAt DATETIME2
- FailureReason NVARCHAR(500)
- TenantId NVARCHAR(128) FK
- Indexes: (TransactionType), (Status), (EntityType, EntityId), (CreatedAt), (TenantId)
```

**Example Queries:**
```sql
-- Get payment history for order
SELECT * FROM [Transaction].TransactionLog 
WHERE EntityType = 'Order' AND EntityId = 123
ORDER BY CreatedAt DESC;

-- Get failed transactions
SELECT * FROM [Transaction].TransactionLog 
WHERE Status = 'Failed'
ORDER BY CreatedAt DESC;

-- Get transaction with currency name
SELECT t.*, l.DisplayName AS CurrencyName
FROM [Transaction].TransactionLog t
LEFT JOIN Master.Lookup l ON t.CurrencyLookupId = l.Id
WHERE t.TransactionLogId = 1;
```

---

## **REPORT SCHEMA (4 Tables)**

### **25. Report.Reports**
**Purpose:** Report definitions  
**Usage:** Define available reports (Sales, Inventory, Customer, Financial)

```
Columns:
- ReportId (PK) INT IDENTITY
- Name NVARCHAR(256)
- Description NVARCHAR(500)
- ReportType NVARCHAR(100) (Sales, Inventory, Customer, Financial)
- QueryDefinition NVARCHAR(MAX) (SQL or SP name)
- TenantId NVARCHAR(128) FK
- Indexes: (ReportType), (TenantId)
```

---

### **26. Report.ReportSchedules**
**Purpose:** Scheduled report generation  
**Usage:** Automate report generation (Daily, Weekly, Monthly)

```
Columns:
- ReportScheduleId (PK) INT IDENTITY
- ReportId INT FK → Reports
- ScheduleName NVARCHAR(256)
- Frequency NVARCHAR(50) (Daily, Weekly, Monthly)
- NextRun DATETIME2
- LastRun DATETIME2
- IsActive BIT
- TenantId NVARCHAR(128) FK
- Indexes: (ReportId), (NextRun), (TenantId)
```

---

### **27. Report.ReportData**
**Purpose:** Generated report data/snapshots  
**Usage:** Store report output for retrieval

```
Columns:
- ReportDataId (PK) BIGINT IDENTITY
- ReportId INT FK → Reports
- GeneratedAt DATETIME2
- DataJson NVARCHAR(MAX) (report data in JSON)
- Summary NVARCHAR(MAX) (summary statistics)
- TenantId NVARCHAR(128) FK
- Indexes: (ReportId), (GeneratedAt), (TenantId)
```

---

### **28. Report.Analytics**
**Purpose:** Event analytics tracking  
**Usage:** Track user events (ProductViewed, AddedToCart, Purchased)

```
Columns:
- AnalyticsId (PK) BIGINT IDENTITY
- EventName NVARCHAR(256) (e.g., 'ProductViewed', 'Purchased')
- EntityType NVARCHAR(100) (Product, Order, Page)
- EntityId INT
- UserId NVARCHAR(256)
- EventData NVARCHAR(MAX) (JSON with event details)
- EventDate DATETIME2
- TenantId NVARCHAR(128) FK
- Indexes: (EventName), (EntityType, EntityId), (UserId), (EventDate), (TenantId)
```

---

## **AUTH SCHEMA (10 Tables)**

### **29. Auth.Users**
**Purpose:** User authentication  
**Usage:** Core user accounts and credentials

```
Columns:
- UserId (PK) NVARCHAR(128)
- UserName NVARCHAR(256) UNIQUE
- NormalizedUserName NVARCHAR(256) UNIQUE
- Email NVARCHAR(256)
- NormalizedEmail NVARCHAR(256)
- EmailConfirmed BIT
- PasswordHash NVARCHAR(MAX)
- SecurityStamp NVARCHAR(MAX)
- ConcurrencyStamp NVARCHAR(MAX)
- PhoneNumber NVARCHAR(20)
- PhoneNumberConfirmed BIT
- TwoFactorEnabled BIT
- LockoutEnd DATETIMEOFFSET
- LockoutEnabled BIT
- AccessFailedCount INT
- DisplayName NVARCHAR(256)
- AvatarUrl NVARCHAR(500)
- Locale NVARCHAR(10) (default: en-US)
- TenantId NVARCHAR(128) FK
- Indexes: (Email), (NormalizedEmail), (TenantId)
```

---

### **30. Auth.Roles**
**Purpose:** Authorization roles  
**Usage:** Define roles (Admin, Manager, User, Guest)

```
Columns:
- RoleId (PK) NVARCHAR(128)
- Name NVARCHAR(256)
- NormalizedName NVARCHAR(256) UNIQUE
- Description NVARCHAR(500)
- TenantId NVARCHAR(128) FK
- IsSystemRole BIT
- Indexes: (Name), (NormalizedName), (TenantId)
```

**System Roles:**
- Admin - Full access
- Manager - Department/team access
- User - Standard user access
- Guest - Public/unauthenticated access

---

### **31. Auth.Permissions**
**Purpose:** Fine-grained permissions  
**Usage:** Define CRUD operations on resources

```
Columns:
- PermissionId (PK) INT IDENTITY
- Name NVARCHAR(256)
- Description NVARCHAR(500)
- PermissionType NVARCHAR(100) (Create, Read, Update, Delete)
- ResourceType NVARCHAR(100) (Product, Order, Report, User)
- TenantId NVARCHAR(128) FK
- IsActive BIT
- UNIQUE(TenantId, PermissionType, ResourceType)
```

**Example Permissions:**
- Create.Product, Read.Product, Update.Product, Delete.Product
- Create.Order, Read.Order, Update.Order, Delete.Order
- Create.Report, Read.Report, Update.Report, Delete.Report
- Create.User, Read.User, Update.User, Delete.User

---

### **32. Auth.UserRoles**
**Purpose:** User-role assignment  
**Usage:** Assign roles to users

```
Columns:
- UserRoleId (PK) INT IDENTITY
- UserId NVARCHAR(128) FK → Users
- RoleId NVARCHAR(128) FK → Roles
- AssignedAt DATETIME2
- TenantId NVARCHAR(128) FK
- UNIQUE(TenantId, UserId, RoleId)
```

---

### **33. Auth.RolePermissions**
**Purpose:** Role-permission assignment  
**Usage:** Grant permissions to roles

```
Columns:
- RolePermissionId (PK) INT IDENTITY
- RoleId NVARCHAR(128) FK → Roles
- PermissionId INT FK → Permissions
- GrantedAt DATETIME2
- TenantId NVARCHAR(128) FK
- UNIQUE(TenantId, RoleId, PermissionId)
```

---

### **34. Auth.UserPermissions**
**Purpose:** Direct user permissions (override roles)  
**Usage:** Grant permissions directly to specific users

```
Columns:
- UserPermissionId (PK) INT IDENTITY
- UserId NVARCHAR(128) FK → Users
- PermissionId INT FK → Permissions
- GrantedAt DATETIME2
- ExpiresAt DATETIME2 (optional - temporary permissions)
- TenantId NVARCHAR(128) FK
- UNIQUE(TenantId, UserId, PermissionId)
```

---

### **35. Auth.AuthTokens ⭐ MERGED**
**Purpose:** Unified token storage (merged 4 tables)  
**Usage:** Password reset, email verification, 2FA, JWT refresh tokens

```
Columns:
- AuthTokenId (PK) INT IDENTITY
- UserId NVARCHAR(128) FK → Users
- Token NVARCHAR(500) UNIQUE
- TokenType NVARCHAR(50) (PasswordReset, EmailVerification, TwoFactor, RefreshToken)
- TokenSubType NVARCHAR(50) (for TwoFactor: Email, Authenticator, SMS)
- ExpiresAt DATETIME2
- UsedAt DATETIME2 (for PasswordReset, EmailVerification)
- VerifiedAt DATETIME2 (for 2FA, EmailVerification)
- RevokedAt DATETIME2 (for RefreshToken)
- Attempts INT (for 2FA - attempt count)
- TenantId NVARCHAR(128) FK
- Indexes: (Token), (UserId), (ExpiresAt), (TokenType), (TenantId)
```

**TokenType Values:**
- **PasswordReset** - Password recovery tokens (24h expiry)
- **EmailVerification** - Email confirmation tokens (7d expiry)
- **TwoFactor** - 2FA codes (5m expiry)
- **RefreshToken** - JWT refresh tokens (7d expiry)

---

### **36. Auth.LoginAttempts**
**Purpose:** Login security tracking  
**Usage:** Detect brute-force attacks, suspicious activity

```
Columns:
- LoginAttemptId (PK) BIGINT IDENTITY
- UserId NVARCHAR(128)
- Email NVARCHAR(256)
- IPAddress NVARCHAR(45)
- UserAgent NVARCHAR(500)
- IsSuccessful BIT
- FailureReason NVARCHAR(256)
- TenantId NVARCHAR(128) FK
- AttemptedAt DATETIME2
- Indexes: (UserId), (Email), (IPAddress), (AttemptedAt), (TenantId)
```

---

### **37. Auth.AuditTrail**
**Purpose:** User activity audit  
**Usage:** Track user actions (Login, Logout, ChangePassword)

```
Columns:
- AuditTrailId (PK) BIGINT IDENTITY
- UserId NVARCHAR(128) FK → Users
- Action NVARCHAR(256) (Login, Logout, ChangePassword, UpdateProfile)
- EntityType NVARCHAR(100)
- EntityId INT
- Changes NVARCHAR(MAX) (JSON)
- IPAddress NVARCHAR(45)
- UserAgent NVARCHAR(500)
- TenantId NVARCHAR(128) FK
- CreatedAt DATETIME2
- Indexes: (UserId), (Action), (CreatedAt), (TenantId)
```

---

### **38. Auth.TenantUsers**
**Purpose:** Tenant membership management  
**Usage:** Manage which users belong to which tenants

```
Columns:
- TenantUserId (PK) INT IDENTITY
- TenantId NVARCHAR(128) FK → Tenants
- UserId NVARCHAR(128) FK → Users
- InvitedAt DATETIME2
- AcceptedAt DATETIME2
- Status NVARCHAR(50) (Active, Inactive, Suspended)
- UNIQUE(TenantId, UserId)
```

---

## **KEY DESIGN PATTERNS**

### **1. Polymorphic Tables**
```
Work with ANY entity type:
- SeoMeta:        EntityType, EntityId
- Tags:           EntityType, EntityId
- Translations:   EntityType, EntityId
- FileStorage:    EntityType, EntityId
- AuditLogs:      EntityType, EntityId
```

### **2. Hierarchical Data (HierarchyID)**
```
Unlimited nesting:
- Categories:     /1/, /1/1/, /1/1/1/
- MenuItems:      /1/, /1/1/, /1/1/1/
- GeoHierarchy:   /1/, /1/1/, /1/1/1/
- Lookup:         /1/, /1/1/, /1/1/1/
```

### **3. Soft Deletes**
```
ALL tables include:
- IsDeleted BIT DEFAULT 0
- CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
→ Never physically delete data
```

### **4. Multi-Tenancy**
```
ALL tables include:
- TenantId NVARCHAR(128) FK → Tenants
→ Complete tenant isolation
```

### **5. Lookup Consolidation**
```
Single Master.Lookup table replaces:
- Currencies  (CategoryKey = 'currencies')
- Languages   (CategoryKey = 'languages')
- TimeZones   (CategoryKey = 'timezones')
- Custom...   (CategoryKey = 'statuses', 'priorities', etc.)
```

### **6. Token Consolidation**
```
Single Auth.AuthTokens table replaces:
- PasswordResetTokens
- EmailVerificationTokens
- TwoFactorTokens
- RefreshTokens

TokenType distinguishes:
- 'PasswordReset'
- 'EmailVerification'
- 'TwoFactor'
- 'RefreshToken'
```

---

## **MATERIALIZED VIEW STRATEGY**

Create materialized views for frequently accessed lookups to improve performance:

```sql
-- View: vw_Currencies (materialized)
CREATE VIEW vw_Currencies AS
SELECT Id, Key, DisplayName, Metadata
FROM Master.Lookup
WHERE CategoryKey = 'currencies' AND IsActive = 1 AND IsDeleted = 0;

-- View: vw_Languages (materialized)
CREATE VIEW vw_Languages AS
SELECT Id, Key, DisplayName, LocalizedNames
FROM Master.Lookup
WHERE CategoryKey = 'languages' AND IsActive = 1 AND IsDeleted = 0;

-- View: vw_TimeZones (materialized)
CREATE VIEW vw_TimeZones AS
SELECT Id, Key, DisplayName, Metadata
FROM Master.Lookup
WHERE CategoryKey = 'timezones' AND IsActive = 1 AND IsDeleted = 0;
```

---

## **SUMMARY**

| Metric | Value |
|--------|-------|
| Total Schemas | 6 |
| Total Tables | 41 |
| Master Tables | 19 |
| Shared Tables | 7 |
| Transaction Tables | 1 |
| Report Tables | 4 |
| Auth Tables | 10 |
| Multi-Tenancy | ✅ All tables |
| Audit Columns | ✅ All tables |
| Soft Deletes | ✅ All tables |
| Hierarchical Data | 4 tables (Categories, MenuItems, GeoHierarchy, Lookup) |
| Polymorphic Tables | 5 tables (SeoMeta, Tags, Translations, FileStorage, AuditLogs) |

---

**Status:** ✅ Production Ready
**Created:** 2026-04-17
**Version:** V2 Complete Schema with fixes applied
