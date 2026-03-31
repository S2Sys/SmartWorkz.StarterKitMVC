# SmartWorkz StarterKit - Software Requirements Specification v4.0

**Document Version:** 4.0
**Date:** 2026-03-31
**Status:** Approved
**Author:** SmartWorkz Development Team

---

## 1. Introduction

### 1.1 Purpose

This SRS defines the requirements for upgrading SmartWorkz StarterKitMVC from a single-client MVC boilerplate (v1) to a multi-client enterprise platform (v4) with a proper database layer, REST API, and support for MVC, Razor Pages, Blazor WASM, and .NET MAUI clients.

### 1.2 Scope

| Area | Description |
|------|-------------|
| **Product** | SmartWorkz StarterKit v4.0 |
| **Framework** | .NET 9 |
| **Database** | SQL Server 2019+ with 4 schemas (master, core, auth, sales) |
| **UI Framework** | Bootstrap 5.3.3 |
| **Architecture** | Clean Architecture (6-layer: Domain, Application, Infrastructure, Shared, Web, Api) |
| **Clients** | MVC (direct), Razor Pages (direct), Blazor WASM (via API), MAUI (via API) |

### 1.3 Definitions

| Term | Definition |
|------|-----------|
| **Direct client** | MVC / Razor Pages - references Application layer in-process |
| **API client** | Blazor WASM / MAUI - consumes REST API over HTTP |
| **HierarchyId** | SQL Server data type for tree structures (Lookups, Categories, Tenants, EntityStates) |
| **EntityType+EntityId pattern** | Polymorphic linking - shared tables (Addresses, Tags, etc.) link to any entity in any schema |
| **Soft delete** | IsDeleted + DeletedAt + DeletedBy columns instead of physical DELETE |

### 1.4 References

- [Prompt.MD](../../Prompt.MD) - Original feature specification
- [StarterDB_V4_Schema.sql](../../database/v4/) - Database schema v4
- [docs/srs/old-vs-proposed.md](old-vs-proposed.md) - Gap analysis

---

## 2. Overall Description

### 2.1 Product Perspective

SmartWorkz StarterKit v4 evolves from a single MVC web application into a multi-client platform. The core business logic lives in the Application layer and is consumed by:

- **MVC Web** and **Razor Pages** - direct in-process service calls
- **REST API** - HTTP endpoints for external clients
- **Blazor WASM** and **MAUI** - consume REST API

### 2.2 Solution Structure

```
SmartWorkz.StarterKitMVC/
├── src/
│   ├── SmartWorkz.StarterKitMVC.Domain/           # 62 entities across 6 schemas
│   ├── SmartWorkz.StarterKitMVC.Application/      # Services, interfaces, DTOs
│   ├── SmartWorkz.StarterKitMVC.Infrastructure/   # EF Core DbContexts, Dapper repos
│   ├── SmartWorkz.StarterKitMVC.Shared/           # Extensions, primitives, base entities
│   ├── SmartWorkz.StarterKitMVC.Web/              # MVC + Razor Pages (direct service calls)
│   └── SmartWorkz.StarterKitMVC.Api/              # REST API (for Blazor/MAUI HTTP clients)
├── tests/
│   ├── SmartWorkz.StarterKitMVC.Tests.Unit/
│   └── SmartWorkz.StarterKitMVC.Tests.Integration/
├── database/
│   ├── v4/           # Fresh install scripts (PascalCase schemas)
│   │   ├── 001_CreateSchemas.sql
│   │   ├── 002_CreateTables_Master.sql
│   │   ├── 003_CreateTables_Core.sql
│   │   ├── 004_CreateTables_Transaction.sql
│   │   ├── 005_CreateTables_Report.sql
│   │   ├── 006_CreateTables_Auth.sql
│   │   ├── 007_CreateTables_Sales.sql
│   │   ├── 008_SeedData.sql
│   │   └── 009_CreateIndexes.sql
│   ├── migrations/   # v1 to v4 upgrade scripts (3 files)
│   └── old/          # Archived v1 scripts
├── docs/
│   ├── srs/          # SRS-v4.md + old-vs-proposed.md
│   ├── old/          # Archived v1 documentation
│   └── *.md          # Updated documentation
├── build/
├── devops/
├── k8s/
├── tools/
├── Dockerfile
├── docker-compose.yml
├── .gitignore
└── README.md
```

### 2.3 Dependency Rules

```
Web (MVC/Razor) ──→ Application ──→ Domain
      │                  │              │
      │                  ↓              ↓
      │            Infrastructure ←── Shared
      │                  ↑
Api (REST)  ───────→ Application
```

- **Web** → Application, Infrastructure, Shared
- **Api** → Application, Infrastructure, Shared
- **Application** → Domain, Shared
- **Infrastructure** → Domain, Shared (EF Core, repositories)
- **Domain** → Shared (minimal)
- **Shared** → None (standalone)

### 2.4 Client Connection Matrix

| Client | Connects To | Method | Auth |
|--------|------------|--------|------|
| MVC Web | Application services | Direct (in-process) | Cookie + Session |
| Razor Pages | Application services | Direct (in-process) | Cookie + Session |
| Blazor WASM | Api project | HTTP (REST) | JWT Bearer |
| MAUI | Api project | HTTP (REST) | JWT Bearer |

---

## 3. Database Schema

### 3.1 Schema Overview

| Schema | Tables | Purpose |
|--------|--------|---------|
| `Master` | 17 | Global reference data. TenantId nullable (NULL=global, GUID=tenant-specific) |
| `Core` | 18 | Project core tables, config, dummy business tables. TenantId NOT NULL |
| `Transaction` | 8 | Transactional data (Orders, OrderLines, Invoices, Payments, etc.). TenantId NOT NULL |
| `Report` | 5 | Report definitions, cached results, audit snapshots. TenantId nullable |
| `Auth` | 13 | Identity, RBAC, sessions, logs. TenantId NOT NULL |
| `Sales` | 1 | Example team schema |
| **Total** | **62** | |

### 3.2 Conventions

- **Schema names:** PascalCase (Master, Core, Transaction, Report, Auth, Sales)
- **Table names:** PascalCase (Users, Products, Orders)
- **Column names:** PascalCase (FirstName, LastName, CreatedAt)
- **HierarchyId for:** Lookups, Categories, EntityStates, Tenants (in Master schema)
- **PKs:** UNIQUEIDENTIFIER with NEWSEQUENTIALID()
- **Log tables:** BIGINT IDENTITY
- **Soft delete:** IsDeleted, DeletedAt, DeletedBy
- **Audit columns:** CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
- **All timestamps:** DATETIME2 with SYSUTCDATETIME()

### 3.3 Master Schema (17 tables)

**Geo reference:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Countries` | Country reference | CountryCode2, CountryCode3, PhoneCode, CurrencyCode |
| `States` | State/province | FK → Countries |
| `Cities` | City reference | FK → States, Countries, Lat/Lng |

**Localization:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Languages` | Supported languages | LanguageCode, IsRtl |
| `Translations` | Single table for ALL i18n | TenantId nullable, Namespace, EntityType+EntityId, Key+Value |

**Hierarchical reference data:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Lookups` | HierarchyId tree (Group→Value→Sub) | NodePath, NodeType, LookupCode, TenantId nullable |
| `Categories` | HierarchyId tree | CategoryType, Slug, TenantId nullable, soft delete |
| `EntityStates` | HierarchyId state definitions | EntityType, StateCode, IsInitial, IsFinal |
| `EntityStateTransitions` | State flow rules | FromStateCode → ToStateCode, RequiredRole |

**Notifications:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `NotificationChannels` | Email/SMS/WhatsApp/Push/InApp | ChannelCode |
| `TemplateGroups` | Event codes (UserWelcome, etc.) | EventCode |
| `Templates` | Global+tenant templates, multi-channel | TenantId nullable, Email+SMS+WhatsApp fields |

**SaaS & config:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `SubscriptionPlans` | Pricing tiers | PlanCode, MonthlyPrice, MaxUsers, FeaturesJson |
| `PreferenceDefinitions` | User/Tenant preference schema | PreferenceKey, DataType, Scope |

**SEO:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `SeoMeta` | Per-entity SEO metadata | EntityType+EntityId, OG tags, Twitter cards, Schema.org |
| `UrlRedirects` | 301/302 redirects | FromPath → ToPath, HitCount |

**Logging:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `AuditLogs` | Global audit trail (BIGINT PK) | ActorId, Action, EntityType+EntityId, OldValues/NewValues |
| `ActivityLogs` | Global activity log | ActivityType, Module, Description |

### 3.4 Core Schema (18 tables)

**Config group (4):**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Tenants` | HierarchyId tree (agency→client→sub) | NodePath, TenantCode, SubscriptionPlanId, branding, address |
| `TenantSubscriptions` | Billing history | BillingCycle, AmountPaid, PaymentReference, Status |
| `TenantSettings` | Key-value per tenant | SettingKey, SettingValue, DataType, IsEncrypted |
| `FeatureFlags` | Per-tenant feature toggles | FlagKey, IsEnabled, RolloutPercent, ValidFrom/To |

**Business group (9) - dummy/placeholder, project core tables:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Products` | Product catalog | ProductCode, CategoryId, UnitPrice, StockQty, SKU, Barcode, Tax |
| `Customers` | Customer records | CustomerCode, CustomerType, GST/PAN, CreditLimit |
| `Vendors` | Vendor/Supplier records | VendorCode, VendorType, ContactPerson, PaymentTerms |
| `Projects` | Project management (dummy) | ProjectCode, ProjectName, Status, Budget, DeadlineDate |
| `Teams` | Team/Department records (dummy) | TeamCode, TeamName, LeadUserId |
| `Departments` | Department records (dummy) | DepartmentCode, DepartmentName, HeadUserId |
| `Employees` | Employee records (dummy) | EmployeeCode, FullName, DepartmentId, Designation |
| `Assets` | Asset tracking (dummy) | AssetCode, AssetName, AssetType, AcquisitionDate |
| `Contracts` | Contract records (dummy) | ContractCode, ContractType, StartDate, EndDate |

**Shared infrastructure group (5) - EntityType+EntityId pattern:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Addresses` | Entity-linked addresses | EntityType, EntityId, AddressType, Lat/Lng |
| `Attachments` | File attachments | EntityType, EntityId, FileName, FileUrl, MimeType |
| `Tags` | Entity tags | EntityType, EntityId, TagValue |
| `Comments` | Threaded comments | EntityType, EntityId, ParentCommentId, IsInternal |
| `StateHistory` | State change audit trail | EntityType, EntityId, FromState→ToState, Reason |

### 3.5 Transaction Schema (8 tables)

**Transactional data - for orders, billing, contracts:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Orders` | Sales order headers | OrderNumber, OrderState, CustomerId, ShippingAddress, totals |
| `OrderLines` | Sales order line items | OrderId, LineNumber, ProductId, Quantity, UnitPrice, discounts |
| `Invoices` | Invoice records | InvoiceNumber, InvoiceState, OrderId, CustomerId, DueDate, amounts |
| `Payments` | Payment records | PaymentNumber, InvoiceId, CustomerId, PaymentMethod, GatewayRef, Status |
| `PurchaseOrders` | Purchase order headers | PONumber, VendorId, PoState, DeliveryAddress, amounts |
| `PurchaseOrderLines` | Purchase order line items | POId, LineNumber, ProductId, Quantity, UnitPrice |
| `Receipts` | Goods receipt records | ReceiptNumber, POId, ReceiptDate, ReceiptStatus |
| `CreditNotes` | Credit note records | CreditNoteNumber, InvoiceId, ReasonCode, Amount |

### 3.6 Report Schema (5 tables)

**Report definitions and cached results:**

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `ReportDefinitions` | Report metadata | ReportCode, ReportName, ReportType, SqlQuery, Parameters |
| `ReportSchedules` | Scheduled report runs | DefinitionId, CronExpression, DeliveryEmail, IsActive |
| `ReportResults` | Cached report data | DefinitionId, GeneratedAt, ResultJson, GeneratedBy |
| `ReportAuditLogs` | Report access/execution logs | DefinitionId, UserId, ExecutedAt, ExecutionTime |
| `DashboardWidgets` | Dashboard widget configs | WidgetCode, ReportId, Position, Size, TenantId |

### 3.8 Auth Schema (13 tables)

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Users` | User accounts | Email, PasswordHash/Salt, MFA, soft delete, IsSuperAdmin |
| `UserProfiles` | Extended profile | FirstName, LastName, Locale, Timezone, ThemeMode, notification prefs |
| `UserPreferences` | Key-value preferences | PreferenceKey, PreferenceValue, DataType |
| `Roles` | Tenant-scoped roles | RoleName, RoleCode, IsSystem |
| `Permissions` | Module+Action permissions | Module, Action, PermissionCode |
| `RolePermissions` | Role-permission junction | RoleId, PermissionId, GrantedAt/By |
| `UserRoles` | User-role junction | UserId, RoleId, TenantId, AssignedAt/By |
| `RefreshTokens` | JWT refresh tokens | Token, DeviceInfo, IpAddress, ExpiresAt |
| `VerificationCodes` | OTP/verification codes | CodeType, CodeHash, ExpiresAt, UsedAt |
| `ExternalLogins` | OAuth provider links | Provider, ProviderUserId, AccessToken |
| `AuditLogs` | Tenant-scoped audit | TenantId NOT NULL, Action, EntityType+EntityId |
| `ActivityLogs` | Tenant-scoped activity | TenantId NOT NULL, ActivityType, Module |
| `NotificationLogs` | Delivery tracking | ChannelCode, Status, ProviderReference, ErrorMessage |

### 3.9 Sales Schema (1 table - example)

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `SalesOrders` | Example team schema table | OrderNumber, OrderState, totals, soft delete |

---

## 4. Domain Layer Entities

### 4.1 Entity Organization

```
Domain/
├── Master/
│   ├── Country.cs, State.cs, City.cs
│   ├── Language.cs, Translation.cs
│   ├── Lookup.cs
│   ├── Category.cs
│   ├── EntityState.cs, EntityStateTransition.cs
│   ├── NotificationChannel.cs, TemplateGroup.cs, Template.cs
│   ├── SubscriptionPlan.cs, PreferenceDefinition.cs
│   ├── SeoMeta.cs, UrlRedirect.cs
│   └── MasterAuditLog.cs, MasterActivityLog.cs
├── Core/
│   ├── Config/
│   │   ├── Tenant.cs, TenantSubscription.cs
│   │   ├── TenantSetting.cs, FeatureFlag.cs
│   ├── Business/
│   │   ├── Product.cs, Customer.cs, Vendor.cs
│   │   ├── Project.cs, Team.cs, Department.cs
│   │   ├── Employee.cs, Asset.cs, Contract.cs
│   └── Shared/
│       ├── Address.cs, Attachment.cs, Tag.cs
│       ├── Comment.cs, StateHistory.cs
├── Transaction/
│   ├── Order.cs, OrderLine.cs
│   ├── Invoice.cs, Payment.cs
│   ├── PurchaseOrder.cs, PurchaseOrderLine.cs
│   ├── Receipt.cs, CreditNote.cs
├── Report/
│   ├── ReportDefinition.cs, ReportSchedule.cs
│   ├── ReportResult.cs, ReportAuditLog.cs
│   └── DashboardWidget.cs
├── Auth/
│   ├── User.cs, UserProfile.cs, UserPreference.cs
│   ├── Role.cs, Permission.cs
│   ├── RolePermission.cs, UserRole.cs
│   ├── RefreshToken.cs, VerificationCode.cs
│   ├── ExternalLogin.cs
│   └── AuthAuditLog.cs, AuthActivityLog.cs, NotificationLog.cs
└── Sales/
    └── SalesOrder.cs
```

### 4.2 Base Entity Classes

```csharp
// All entities with audit columns
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

// Entities with soft delete
public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

// Tenant-scoped entities
public abstract class TenantEntity : AuditableEntity
{
    public Guid TenantId { get; set; }
}

// Tenant-scoped with soft delete
public abstract class TenantSoftDeletableEntity : SoftDeletableEntity
{
    public Guid TenantId { get; set; }
}
```

---

## 5. Application Layer

### 5.1 Service Interfaces

**Master services:**

| Interface | Purpose |
|-----------|---------|
| `IGeoService` | Countries, States, Cities lookup |
| `ITranslationService` | Single i18n service for all namespaces |
| `ILookupService` | HierarchyId lookup tree CRUD |
| `ICategoryService` | HierarchyId category tree CRUD |
| `IEntityStateService` | State definitions and transitions |
| `ITemplateService` | Multi-channel template management |
| `ISubscriptionPlanService` | Plan CRUD |
| `IPreferenceDefinitionService` | Preference schema management |
| `ISeoMetaService` | SEO metadata CRUD |
| `IUrlRedirectService` | Redirect management |

**Core services:**

| Interface | Purpose |
|-----------|---------|
| `ITenantService` | Tenant CRUD with HierarchyId |
| `ITenantSubscriptionService` | Subscription management |
| `ITenantSettingService` | Key-value settings |
| `IFeatureFlagService` | Feature toggle management |
| `IProductService` | Product CRUD |
| `ICustomerService` | Customer CRUD |
| `IOrderService` | Order + OrderLine management with state machine |
| `IInvoiceService` | Invoice CRUD |
| `IPaymentService` | Payment CRUD |
| `IAddressService` | Polymorphic address management |
| `IAttachmentService` | File attachment management |
| `ITagService` | Entity tagging |
| `ICommentService` | Threaded comments |
| `IStateHistoryService` | State change audit |

**Auth services:**

| Interface | Purpose |
|-----------|---------|
| `IAuthService` | Login, register, refresh, MFA |
| `IUserService` | User CRUD + profiles + preferences |
| `IRoleService` | Role CRUD |
| `IPermissionService` | Permission CRUD |
| `IRbacService` | Role-permission, user-role assignments |
| `ITokenService` | JWT + refresh token management |
| `IVerificationService` | OTP/verification code management |
| `IExternalLoginService` | OAuth provider linking |
| `IAuditService` | Audit + activity log queries |
| `INotificationLogService` | Delivery log queries |

### 5.2 DTOs

Each service has corresponding request/response DTOs:

```
Application/
├── DTOs/
│   ├── Master/
│   │   ├── LookupDto.cs, LookupTreeDto.cs
│   │   ├── CategoryDto.cs, CategoryTreeDto.cs
│   │   ├── TranslationDto.cs
│   │   ├── TemplateDto.cs
│   │   └── ...
│   ├── Core/
│   │   ├── TenantDto.cs, TenantCreateDto.cs
│   │   ├── ProductDto.cs, ProductCreateDto.cs
│   │   ├── OrderDto.cs, OrderCreateDto.cs
│   │   └── ...
│   └── Auth/
│       ├── LoginDto.cs, RegisterDto.cs, TokenDto.cs
│       ├── UserDto.cs, UserCreateDto.cs
│       └── ...
```

---

## 6. Infrastructure Layer

### 6.1 Data Access Layer

**EF Core DbContexts (Primary):**

```csharp
// One DbContext per schema
MasterDbContext   → Master.* (17 tables)
CoreDbContext     → Core.* (18 tables)
AuthDbContext     → Auth.* (13 tables)
SalesDbContext    → Sales.* (1 table)

// Optional - add if performance needed
TransactionDbContext → Transaction.* (8 tables) - can be EF or Dapper
ReportDbContext      → Report.* (5 tables) - can be EF or Dapper
```

Each EF Core context:
- `modelBuilder.HasDefaultSchema("SchemaName")` (PascalCase)
- `IEntityTypeConfiguration<T>` per entity
- HierarchyId support via `Microsoft.EntityFrameworkCore.SqlServer.HierarchyId`
- Single connection string (one database, multiple schemas)

**Dapper Integration (Optional - Performance):**

Use Dapper alongside EF Core for:
- High-volume transactional queries (bulk inserts/updates)
- Complex report aggregations (GROUP BY, window functions)
- Raw SQL when EF LINQ is insufficient

Strategy:
- **Start with EF Core** for all schemas
- **Profile** transaction and report queries
- **Migrate to Dapper** only for bottleneck queries (if needed)
- Both share the same connection string

### 6.2 Repository Pattern

```csharp
// Generic
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);  // soft delete where applicable
}

// Tenant-scoped
public interface ITenantRepository<T> : IRepository<T> where T : TenantEntity
{
    Task<IReadOnlyList<T>> GetByTenantAsync(Guid tenantId);
}

// HierarchyId-specific
public interface IHierarchyRepository<T>
{
    Task<T?> GetByPathAsync(HierarchyId path);
    Task<IReadOnlyList<T>> GetChildrenAsync(HierarchyId parentPath);
    Task<IReadOnlyList<T>> GetDescendantsAsync(HierarchyId ancestorPath);
    Task<HierarchyId> GetNextChildPathAsync(HierarchyId parentPath);
}
```

### 6.3 Connection String Configuration

```json
{
  "ConnectionStrings": {
    "StarterDb": "Server=localhost;Database=StarterDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

All four DbContexts share the same connection string (single database).

---

## 7. API Layer

### 7.1 Project: SmartWorkz.StarterKitMVC.Api

```
Api/
├── Controllers/v1/
│   ├── AuthController.cs           # POST login, register, refresh, mfa
│   ├── UsersController.cs          # GET/POST/PUT/DELETE users
│   ├── RolesController.cs          # GET/POST/PUT/DELETE roles
│   ├── TenantsController.cs        # GET/POST/PUT/DELETE tenants
│   ├── LookupsController.cs        # GET/POST/PUT/DELETE lookup tree
│   ├── CategoriesController.cs     # GET/POST/PUT/DELETE category tree
│   ├── TranslationsController.cs   # GET/POST/PUT/DELETE translations
│   ├── ProductsController.cs       # GET/POST/PUT/DELETE products
│   ├── CustomersController.cs      # GET/POST/PUT/DELETE customers
│   ├── OrdersController.cs         # GET/POST/PUT/DELETE orders + lines
│   ├── InvoicesController.cs       # GET/POST/PUT/DELETE invoices
│   ├── PaymentsController.cs       # GET/POST/PUT/DELETE payments
│   ├── TemplatesController.cs      # GET/POST/PUT/DELETE templates
│   └── SettingsController.cs       # GET/PUT tenant settings
├── Middleware/
│   ├── TenantResolverMiddleware.cs
│   └── ExceptionHandlerMiddleware.cs
├── Program.cs
└── appsettings.json
```

### 7.2 API Configuration

- **Authentication:** JWT Bearer tokens
- **Versioning:** URL path (`/api/v1/`)
- **Documentation:** Swagger/OpenAPI
- **Rate Limiting:** Per-tenant + per-user
- **CORS:** Configured for Blazor WASM and MAUI origins
- **Response format:** Unified `ApiResponse<T>` wrapper

### 7.3 API Endpoint Summary

| Endpoint | Methods | Auth | Description |
|----------|---------|------|-------------|
| `/api/v1/auth/login` | POST | Anonymous | Login, returns JWT |
| `/api/v1/auth/register` | POST | Anonymous | Register new user |
| `/api/v1/auth/refresh` | POST | Anonymous | Refresh JWT |
| `/api/v1/users` | GET, POST | Admin | User management |
| `/api/v1/users/{id}` | GET, PUT, DELETE | Admin | User CRUD |
| `/api/v1/roles` | GET, POST | Admin | Role management |
| `/api/v1/tenants` | GET, POST | SuperAdmin | Tenant management |
| `/api/v1/lookups` | GET, POST | Auth | Lookup tree |
| `/api/v1/lookups/{id}/children` | GET | Auth | Lookup children |
| `/api/v1/categories` | GET, POST | Auth | Category tree |
| `/api/v1/translations/{lang}/{ns}` | GET | Anonymous | Translation bundle |
| `/api/v1/products` | GET, POST | Auth | Product CRUD |
| `/api/v1/customers` | GET, POST | Auth | Customer CRUD |
| `/api/v1/orders` | GET, POST | Auth | Order management |
| `/api/v1/orders/{id}/transition` | POST | Auth | State transition |
| `/api/v1/invoices` | GET, POST | Auth | Invoice management |
| `/api/v1/payments` | GET, POST | Auth | Payment management |
| `/api/v1/templates` | GET, POST | Admin | Template management |
| `/api/v1/settings` | GET, PUT | Admin | Tenant settings |

---

## 8. Database Migration Scripts

### 8.1 Fresh Install (v4)

For new setups (MAUI/Blazor/greenfield):

| Script | Description |
|--------|-------------|
| `database/v4/001_CreateSchemas.sql` | CREATE SCHEMA master, core, auth, sales |
| `database/v4/002_CreateTables_Master.sql` | 17 master tables with constraints |
| `database/v4/003_CreateTables_Core.sql` | 13 core tables with constraints |
| `database/v4/004_CreateTables_Auth.sql` | 13 auth tables with constraints |
| `database/v4/005_CreateTables_Sales.sql` | 1 sales example table |
| `database/v4/006_SeedData.sql` | All reference data seeding |
| `database/v4/007_CreateIndexes.sql` | All additional indexes |

### 8.2 Upgrade (v1 to v4)

For existing MVC/Razor setups with v1 data:

| Script | Description |
|--------|-------------|
| `database/migrations/pre-migration.sql` | Backup validation, version check, row count snapshot |
| `database/migrations/migrate_v1_to_v4.sql` | Create schemas, create tables, migrate data |
| `database/migrations/post-migration.sql` | Verify row counts, cleanup old tables, update version |

**Data migration mapping:**

| v1 Table | v4 Table | Notes |
|----------|----------|-------|
| `dbo.Users` | `auth.Users` + `auth.UserProfiles` | Split profile data |
| `dbo.Roles` | `auth.Roles` | Add TenantId, RoleCode |
| `dbo.Permissions` | `auth.Permissions` | Add Module, Action |
| `dbo.UserRoles` | `auth.UserRoles` | Add TenantId |
| `dbo.RolePermissions` | `auth.RolePermissions` | Unchanged structure |
| `dbo.Claims` | (removed) | Replaced by Permissions model |
| `dbo.UserClaims` | (removed) | Replaced by UserRoles+Permissions |
| `dbo.RoleClaims` | (removed) | Replaced by RolePermissions |
| `dbo.Tenants` | `core.Tenants` | Add HierarchyId, GUID PK |
| `dbo.TenantBranding` | `core.Tenants` | Merged into Tenants (LogoUrl, colors) |
| `dbo.SettingCategories` | `core.TenantSettings` | Flattened key-value |
| `dbo.SettingDefinitions` | `master.PreferenceDefinitions` | Renamed concept |
| `dbo.SettingValues` | `core.TenantSettings` | Merged with key |
| `dbo.LovCategories` | `master.Lookups` (Group level) | HierarchyId /N/ |
| `dbo.LovSubCategories` | `master.Lookups` (Group level) | HierarchyId /N/M/ |
| `dbo.LovItems` | `master.Lookups` (Value level) | HierarchyId /N/M/K/ |
| `dbo.LovItemLocalizations` | `master.Translations` | Namespace='lookup' |
| `dbo.NotificationTemplates` | `master.TemplateGroups` + `master.Templates` | Split event+content |
| `dbo.Notifications` | `auth.NotificationLogs` | Renamed |
| `dbo.AuditLogs` | `master.AuditLogs` | Renamed columns |

---

## 9. Cross-Cutting Concerns

### 9.1 Retained from v1

All existing cross-cutting infrastructure is retained:

- Structured logging (Serilog)
- Correlation ID middleware
- OpenTelemetry hooks
- Resilience policies (Polly)
- Feature flags
- Audit logging (now backed by real DB)
- Background jobs (InMemory/Hangfire/Quartz)
- Caching (Memory/Redis)
- File storage (Local/Azure/S3)
- Notifications (Email/SMS/Push/SignalR)

### 9.2 New in v4

| Concern | Description |
|---------|-------------|
| **Multi-schema EF Core** | 4 DbContexts sharing one connection |
| **HierarchyId support** | Tree queries for Lookups, Categories, Tenants, EntityStates |
| **State machine** | EntityState + EntityStateTransition + StateHistory |
| **Polymorphic linking** | Addresses, Attachments, Tags, Comments via EntityType+EntityId |
| **SEO** | SeoMeta per entity, UrlRedirects |
| **Subscription billing** | Plans + TenantSubscriptions |

### 9.3 Plug & Play Configuration

All features remain toggleable via `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "StarterDb": "Server=localhost;Database=StarterDb;..."
  },
  "Features": {
    "Identity": { "Enabled": true },
    "MultiTenancy": { "Enabled": true },
    "Caching": { "Enabled": true, "Provider": "Memory" },
    "Api": { "Enabled": true, "Version": "v1" },
    ...
  }
}
```

---

## 10. Non-Functional Requirements

### 10.1 Performance

- Database queries must use proper indexes (defined in schema)
- HierarchyId queries must use `IsDescendantOf()` for tree traversal
- Tenant filtering must be applied at query level (not in-memory)
- API responses < 200ms for standard CRUD operations

### 10.2 Security

- JWT tokens with short expiry (60 min) + refresh tokens
- Password hashing with salt (auth.Users.PasswordHash + PasswordSalt)
- MFA support (auth.Users.MfaEnabled + MfaSecret)
- Tenant isolation enforced at repository level
- API rate limiting per tenant and per user
- CORS restricted to known client origins

### 10.3 Scalability

- Single database with schema separation (can be split later)
- Stateless API (JWT, no server session)
- Background jobs abstraction (swap InMemory for Hangfire/Quartz)
- Event bus abstraction (swap InMemory for RabbitMQ/Kafka)

### 10.4 Maintainability

- Clean Architecture with strict dependency rules
- One DbContext per schema (bounded context)
- Generic repository with tenant-scoped and hierarchy variants
- Entity configurations in separate IEntityTypeConfiguration classes

---

## 11. Appendix

### 11.1 Table Count Summary

| Schema | Tables | Entity Count |
|--------|--------|--------------|
| Master | 17 | Countries, States, Cities, Languages, Translations, Lookups, Categories, EntityStates, EntityStateTransitions, NotificationChannels, TemplateGroups, Templates, SubscriptionPlans, PreferenceDefinitions, SeoMeta, UrlRedirects, AuditLogs, ActivityLogs |
| Core | 18 | Tenants, TenantSubscriptions, TenantSettings, FeatureFlags, Products, Customers, Vendors, Projects, Teams, Departments, Employees, Assets, Contracts, Addresses, Attachments, Tags, Comments, StateHistory |
| Transaction | 8 | Orders, OrderLines, Invoices, Payments, PurchaseOrders, PurchaseOrderLines, Receipts, CreditNotes |
| Report | 5 | ReportDefinitions, ReportSchedules, ReportResults, ReportAuditLogs, DashboardWidgets |
| Auth | 13 | Users, UserProfiles, UserPreferences, Roles, Permissions, RolePermissions, UserRoles, RefreshTokens, VerificationCodes, ExternalLogins, AuditLogs, ActivityLogs, NotificationLogs |
| Sales | 1 | SalesOrders |
| **Total** | **62** | |

### 11.2 NuGet Packages Required

| Package | Purpose | Usage |
|---------|---------|-------|
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server provider | Master, Core, Auth DbContexts |
| `Microsoft.EntityFrameworkCore.SqlServer.HierarchyId` | HierarchyId support | Lookups, Categories, Tenants, EntityStates |
| `Microsoft.EntityFrameworkCore.Tools` | Migrations tooling | EF migrations |
| `Dapper` | Micro-ORM for high-performance queries | Transaction, Report schemas (optional, if performance needed) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT auth for API | API authentication |
| `Asp.Versioning.Mvc` | API versioning | API layer |
| `Swashbuckle.AspNetCore` | Swagger/OpenAPI | API documentation |

### 11.3 Data Access Strategy

**EF Core (Primary):**
- Master, Core, Auth schemas use EF Core (full ORM, relationships, navigation properties)
- Query complex trees (HierarchyId), navigate references

**Dapper (Optional - Performance):**
- Transaction, Report schemas can use Dapper if EF Core queries are slow
- Dapper excels at materialized projections, bulk operations, report aggregations
- Start with EF Core; migrate specific queries to Dapper only if profiling shows bottlenecks

**Approach:**
```csharp
// Start with EF Core
public class OrderRepository : IRepository<Order>
{
    private readonly TransactionDbContext _context;
    // EF LINQ queries
}

// If needed, add Dapper for performance
public class OrderReportRepository
{
    private readonly IDbConnection _connection;
    // Dapper raw SQL for complex aggregations
}
```

### 11.4 Future Framework Upgrades

- **.NET 9 → .NET 10:** Straightforward NuGet update when released
- **.csproj → .slnx:** Can upgrade to new solution format later (Visual Studio 2022 v17.10+)
- **No breaking changes planned** for EF Core or Dapper upgrades in near term

---

*Document generated: 2026-03-31 | SmartWorkz StarterKit v4.0*
