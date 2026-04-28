# SmartWorkz StarterKitMVC v4.2.0

**Multi-tenant Enterprise Platform with .NET 9 Clean Architecture**

Latest: **v4.2.0** (2026-04-28) — Complete stored procedure library, 27 wiki guides & 181+ components | [Changelog](CHANGELOG.md)

## Quick Links

### 📚 Getting Started
- **🎯 Demo Pages** (New v4.1.0)
  - [Translation System Demo](/Public/Pages/Demo/Translations.cshtml) — View all translation keys and values
  - [Validation Demo](/Public/Pages/Demo/Validation.cshtml) — See validation attributes in action

- **📖 Wiki Documentation** (New v4.1.0)
  - [00 — Master Guide](docs/wiki/00-master-guide.md) — Navigation hub for all documentation
  - [09 — Translation System](docs/wiki/09-translation-system.md) — DB-backed translations, T() helper, multi-locale
  - [10 — Localized Validation](docs/wiki/10-localized-validation.md) — MessageKey validation attributes
  - [11 — Base Page Pattern](docs/wiki/11-base-page-pattern.md) — BasePage, TenantId, T() method, toast helpers
  - [12 — Result Pattern](docs/wiki/12-result-pattern.md) — Result/Result<T> for explicit success/failure
  - [13 — HTMX List Pattern](docs/wiki/13-htmx-list-pattern.md) — Dynamic search/filter/pagination with HTMX

### 🏗️ Architecture & Design
- **📋 Schema Design:** [`docs/srs/SCHEMA-REVIEW-v2.md`](docs/srs/SCHEMA-REVIEW-v2.md) — 42 LEAN tables across 5 schemas
- **📊 Architecture Roadmap:** [`IMPLEMENTATION_ROADMAP_MASTER.md`](IMPLEMENTATION_ROADMAP_MASTER.md) — Complete multi-phase roadmap and strategy
- **✅ Implementation Plan:** [`docs/srs/IMPLEMENTATION-PLAN.md`](docs/srs/IMPLEMENTATION-PLAN.md) — 4-phase roadmap (34-45 hours)
- **🔍 Geo Design Analysis:** [`docs/srs/GEO-HIERARCHY-ANALYSIS.md`](docs/srs/GEO-HIERARCHY-ANALYSIS.md) — Option C (Hybrid) approach
- **🗺️ Menu System Guide:** [`docs/srs/MENU-SYSTEM-GUIDE.md`](docs/srs/MENU-SYSTEM-GUIDE.md) — Complete navigation implementation
- **⚡ Quick Reference:** [`docs/srs/QUICK-REFERENCE-v4.md`](docs/srs/QUICK-REFERENCE-v4.md) — One-page schema overview
- **📝 Updates Summary:** [`docs/srs/UPDATES-SUMMARY-v4.md`](docs/srs/UPDATES-SUMMARY-v4.md) — What changed in v4

## Architecture Overview

### Schemas (5 total, 42 LEAN tables)

| Schema | Tables | Purpose |
|--------|--------|---------|
| **Master** | 19 | Global reference data (Geo, i18n, Hierarchies, Tenants, SEO, Config, Navigation) |
| **Shared** | 5 | Polymorphic infrastructure (Addresses, Attachments, Comments, StateHistory, Preferences) |
| **Transaction** | 1 | Orders (extensible pattern for transactions) |
| **Report** | 4 | Production-ready reporting (Definitions, Schedules, Executions, Metadata) |
| **Auth** | 13 | Complete identity + RBAC + logging |

### Key Design Patterns

✅ **Option C Hybrid Geo:** Countries + GeoHierarchy (flexible hierarchy with HierarchyId)
✅ **Polymorphic Infrastructure:** Addresses, Attachments, Comments, StateHistory (any entity)
✅ **Multi-Tenancy:** Row-level TenantId isolation
✅ **HierarchyId Trees:** Unlimited nesting for Tenants, Lookups, Categories, EntityStates, GeoHierarchy, MenuItems
✅ **Dynamic Navigation:** Menus + MenuItems with role-based visibility, breadcrumbs, auto-sitemap
✅ **Production-Ready Reports:** SQL + Dashboards + Scheduling + Caching + Audit Trail
✅ **Soft Delete & Audit:** IsDeleted, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy on all entities

## Project Structure

```
SmartWorkz.StarterKitMVC/
├── src/
│   ├── SmartWorkz.StarterKitMVC.Domain/          # Domain entities
│   ├── SmartWorkz.StarterKitMVC.Application/     # Application services
│   ├── SmartWorkz.StarterKitMVC.Infrastructure/  # EF Core DbContexts
│   ├── SmartWorkz.StarterKitMVC.Shared/          # DTOs, common utilities
│   └── SmartWorkz.StarterKitMVC.Web/             # MVC + API
├── tests/
│   ├── SmartWorkz.StarterKitMVC.Tests.Unit/
│   └── SmartWorkz.StarterKitMVC.Tests.Integration/
├── docs/
│   ├── srs/                                       # Schema & implementation specs
│   │   ├── SCHEMA-REVIEW-v2.md                   # Complete schema documentation
│   │   ├── IMPLEMENTATION-PLAN.md                # 4-phase implementation roadmap
│   │   ├── GEO-HIERARCHY-ANALYSIS.md             # Geo design options & rationale
│   │   └── README.md                             # Spec index
│   └── old/                                       # v1 documentation (archived)
├── database/v4/                                   # SQL migration scripts (Phase 1)
│   ├── 001_CreateSchemas.sql
│   ├── 002_CreateTables_Master.sql
│   ├── 003_CreateTables_Shared.sql
│   ├── 004_CreateTables_Transaction.sql
│   ├── 005_CreateTables_Report.sql
│   ├── 006_CreateTables_Auth.sql
│   ├── 007_SeedData.sql
│   └── 008_CreateIndexes.sql
├── SCHEMA-SUMMARY-LEAN.md                        # Quick reference guide
└── REVIEW-CHECKLIST.md                           # Pre-implementation validation
```

## Quick Start Guide

### Installation & Setup

#### Prerequisites
- .NET 9 SDK or later
- SQL Server 2019+ (or MySQL/PostgreSQL with configuration changes)
- Visual Studio 2022+ or VS Code
- Git

#### Step 1: Clone & Configure
```bash
git clone https://github.com/S2Sys/SmartWorkz.StarterKitMVC.git
cd SmartWorkz.StarterKitMVC
```

#### Step 2: Create Database
```bash
# Update connection string in appsettings.json
# Default: Server=localhost;Database=StarterKitMVC;Trusted_Connection=True

# Run migrations
dotnet ef database update --project src/SmartWorkz.StarterKitMVC.Infrastructure
```

#### Step 3: Build & Run
```bash
# Build solution
dotnet build

# Run web application
dotnet run --project src/SmartWorkz.StarterKitMVC.Web
# Navigate to https://localhost:5001
```

### Component Usage Examples

#### 1. Translation System
```csharp
// In your Razor Page or controller
@using SmartWorkz.StarterKitMVC.Application.Services

@inject ITranslationService TranslationService

<h1>@TranslationService.Translate("MessageKey.WelcomeUser", UserName)</h1>
```

#### 2. Base Page Pattern
```csharp
// Your Razor Page automatically inherits BasePage
@page
@model MyPage

@{
    // Automatic access to:
    // - T(messageKey) — translation helper
    // - TenantId — current tenant ID
    // - AddToast(message, type) — toast notifications
}

<h1>@T("Title.Dashboard")</h1>
```

#### 3. Result Pattern
```csharp
public async Task<IActionResult> GetProduct(int id)
{
    var result = await _productService.GetProductAsync(id);
    
    if (!result.Success)
        return BadRequest(result.Message);
        
    return Ok(result.Data);
}
```

#### 4. HTMX List Pattern
```html
<!-- Dynamic search/filter without page reload -->
<div hx-get="/api/products/search"
     hx-trigger="keyup changed delay:500ms"
     hx-target="#results">
    <input type="search" name="q" placeholder="Search products...">
</div>
<div id="results"></div>
```

#### 5. Cache Attribute
```csharp
[Cache(Seconds = 300)]
[HttpGet("{id}")]
public async Task<IActionResult> GetProduct(int id)
{
    var product = await _repo.GetByIdAsync(id);
    return Ok(product);
}
```

#### 6. Template Engine
```csharp
var html = await _templateEngine.RenderFileAsync(
    "~/Templates/Emails/welcome.html",
    new { UserName = "John Doe", ActivationUrl = "..." }
);
```

#### 7. Pagination
```csharp
var products = await _repository.GetPagedAsync(
    pageNumber: 1,
    pageSize: 20,
    orderBy: x => x.Name,
    filter: x => x.IsActive
);
```

#### 8. Data Export
```csharp
// Export to Excel
var bytes = await _exportService.ExportToExcelAsync(products, 
    new[] { "Name", "Price", "Category" });

// Export to CSV
var csv = await _exportService.ExportToCsvAsync(products);

// Export to PDF
var pdf = await _exportService.ExportToPdfAsync(products);
```

#### 9. Email Service
```csharp
await _emailService.SendAsync(
    to: "user@example.com",
    subject: "Welcome!",
    templatePath: "~/Templates/Emails/welcome.html",
    templateModel: new { UserName = "John" },
    attachments: new[] { filePath }
);
```

#### 10. Blazor Grid Component
```razor
<!-- Grid with sorting, filtering, pagination -->
<DataGrid Items="@products" 
          PageSize="20"
          Sortable="true"
          Filterable="true">
    <GridColumn Property="@(p => p.Name)" Header="Product Name" />
    <GridColumn Property="@(p => p.Price)" Header="Price" Format="C2" />
</DataGrid>
```

### Testing Your Setup

#### Run Unit Tests
```bash
dotnet test tests/SmartWorkz.StarterKitMVC.Tests.Unit/
```

#### Run Integration Tests
```bash
dotnet test tests/SmartWorkz.StarterKitMVC.Tests.Integration/
```

#### Run E2E Tests
```bash
# Requires Playwright installation
dotnet test tests/SmartWorkz.StarterKitMVC.Tests.E2E/
```

#### Performance Tests
```bash
# Load testing with k6
k6 run tests/performance/load-test.js
```

## Implementation Roadmap

### Phase 1: Foundation (34-45 hours) ✅ COMPLETE
1. ✅ Create database scripts (001-008)
2. ✅ Generate domain entities (42 entities)
3. ✅ Create EF Core DbContexts (3-4 contexts)
4. ✅ Create application services (including MenuService)
5. ✅ Create REST API endpoints (25+)

**Result:** v4 API operational with real database

### Phase 2: Documentation (8-10 hours) ✅ COMPLETE
1. ✅ Archive v1 docs
2. ✅ Update project documentation
3. ✅ Create migration guide
4. ✅ 27 wiki guides with code examples
5. ✅ Component index & quick start

### Phase 3: MVC Integration (20-30 hours) ⏳ IN PROGRESS
1. ⏳ Update Admin views/controllers
2. ⏳ Integration testing
3. ⏳ Performance profiling
4. ⏳ Mobile MAUI integration

### Phase 4: API Polish (10-15 hours) ⏳ IN PROGRESS
1. ⏳ Swagger documentation
2. ⏳ API versioning
3. ⏳ Rate limiting, health checks
4. ⏳ Security hardening

## REST APIs & Export Services

### REST API Endpoints (25+)

#### Products API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | List all products with pagination |
| GET | `/api/products/{id}` | Get product details |
| POST | `/api/products` | Create new product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| GET | `/api/products/{id}/export` | Export product |

#### Categories API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | List all categories |
| GET | `/api/categories/{id}` | Get category details |
| POST | `/api/categories` | Create category |
| PUT | `/api/categories/{id}` | Update category |
| DELETE | `/api/categories/{id}` | Delete category |

#### Translations API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/translations` | Get all translations |
| GET | `/api/translations/{key}` | Get translation by key |
| POST | `/api/translations` | Create translation |
| PUT | `/api/translations/{key}` | Update translation |
| DELETE | `/api/translations/{key}` | Delete translation |

#### Menus API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/menus` | List all menus |
| GET | `/api/menus/{id}` | Get menu details |
| GET | `/api/menus/{id}/items` | Get menu items |
| POST | `/api/menus` | Create menu |

#### Reports API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/reports` | List all reports |
| GET | `/api/reports/{id}` | Get report details |
| POST | `/api/reports/{id}/execute` | Execute report |
| GET | `/api/reports/{id}/schedule` | Get schedule |
| POST | `/api/reports/{id}/schedule` | Create schedule |

#### Auth API
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | User login |
| POST | `/api/auth/logout` | User logout |
| POST | `/api/auth/register` | User registration |
| POST | `/api/auth/refresh` | Refresh JWT token |
| POST | `/api/auth/password-reset` | Request password reset |

### Export Services

#### CSV Export
```csharp
public interface IExportService
{
    Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, 
        string[] columnNames = null);
}
```

#### Excel Export
```csharp
public interface IExcelExportService
{
    Task<byte[]> ExportAsync<T>(IEnumerable<T> data,
        string sheetName = "Data",
        string[] columnNames = null);
    
    // With formatting
    Task<byte[]> ExportWithFormattingAsync<T>(IEnumerable<T> data,
        ExcelFormatting formatting);
}
```

#### PDF Export
```csharp
public interface IPdfExportService
{
    Task<byte[]> ExportAsync<T>(IEnumerable<T> data,
        string title = "Report",
        PdfFormatting formatting = null);
}
```

#### Usage Examples
```csharp
// Export Products to Excel
[HttpGet("export/excel")]
public async Task<IActionResult> ExportProductsExcel()
{
    var products = await _productService.GetAllAsync();
    var bytes = await _excelService.ExportAsync(products, 
        "Products", 
        new[] { "Name", "Price", "Category" });
    
    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "products.xlsx");
}

// Export Products to PDF
[HttpGet("export/pdf")]
public async Task<IActionResult> ExportProductsPdf()
{
    var products = await _productService.GetAllAsync();
    var bytes = await _pdfService.ExportAsync(products,
        title: "Product Catalog",
        formatting: new PdfFormatting { Landscape = true });
    
    return File(bytes, "application/pdf", "products.pdf");
}

// Export Products to CSV
[HttpGet("export/csv")]
public async Task<IActionResult> ExportProductsCsv()
{
    var products = await _productService.GetAllAsync();
    var csv = await _csvService.ExportToCsvAsync(products);
    
    return File(System.Text.Encoding.UTF8.GetBytes(csv), 
        "text/csv", "products.csv");
}
```

## Column Naming Convention

Simplified column names (removed redundant table prefixes):

| Example | Old | New |
|---------|-----|-----|
| TenantSubscriptions | SubscriptionPlanCode | PlanCode |
| TenantSettings | SettingKey | Key |
| ReportDefinitions | ReportCode | Code |
| Addresses | AddressType | Type |
| Comments | CommentText | Text |
| StateHistory | FromStateCode | FromState |

**Benefit:** Cleaner entity naming, shorter DTOs, better API contracts

## Multi-Tenancy

All tables have `TenantId` for row-level isolation:
- **Master schema:** TenantId NULLABLE (global defaults + tenant overrides)
- **Other schemas:** TenantId NOT NULL (complete row-level isolation)

Supports agencies → clients → sub-clients hierarchy via HierarchyId.

## Database

**Single SQL Server database:** `StarterKitMVC`
**Connection String:** `Server=localhost;Database=StarterKitMVC;Trusted_Connection=True;TrustServerCertificate=True`

**DbContexts (6):**
- MasterDbContext (14 tables)
- SharedDbContext (5 tables)
- CoreDbContext (3 tables)
- TransactionDbContext (1 table)
- ReportDbContext (4 tables)
- AuthDbContext (13 tables)

## Clients Supported

- ✅ MVC Web (direct service calls)
- ✅ Razor Pages (direct service calls)
- ✅ Blazor WASM (REST API)
- ✅ .NET MAUI (REST API)

## Component Versions (v4.2.0)

| Component | Version | Status | Reference |
|-----------|---------|--------|-----------|
| **Master Guide** | v1.0.0 | ✅ Complete | [Wiki 00](docs/wiki/00-master-guide.md) |
| **Getting Started** | v1.0.0 | ✅ Complete | [Wiki 01](docs/wiki/01-getting-started.md) |
| **Project Overview** | v1.0.0 | ✅ Complete | [Wiki 02](docs/wiki/02-project-overview.md) |
| **SmartWorkz Core** | v1.0.0 | ✅ Complete | [Wiki 03](docs/wiki/03-smartworkz-core.md) |
| **SmartWorkz Core Web** | v1.0.0 | ✅ Complete | [Wiki 04](docs/wiki/04-smartworkz-core-web.md) |
| **SmartWorkz Core Shared** | v1.0.0 | ✅ Complete | [Wiki 05](docs/wiki/05-smartworkz-core-shared.md) |
| **SmartWorkz Core External** | v1.0.0 | ✅ Complete | [Wiki 06](docs/wiki/06-smartworkz-core-external.md) |
| **SmartWorkz Mobile** | v1.0.0 | ✅ Complete | [Wiki 07](docs/wiki/07-smartworkz-mobile.md) |
| **Step-by-Step Guide** | v1.0.0 | ✅ Complete | [Wiki 08](docs/wiki/08-step-by-step-guide.md) |
| **Translation System** | v1.1.0 | ✅ Complete | [Wiki 09](docs/wiki/09-translation-system.md) |
| **Localized Validation** | v1.1.0 | ✅ Complete | [Wiki 10](docs/wiki/10-localized-validation.md) |
| **Base Page Pattern** | v1.1.0 | ✅ Complete | [Wiki 11](docs/wiki/11-base-page-pattern.md) |
| **Result Pattern** | v1.1.0 | ✅ Complete | [Wiki 12](docs/wiki/12-result-pattern.md) |
| **HTMX List Pattern** | v1.1.0 | ✅ Complete | [Wiki 13](docs/wiki/13-htmx-list-pattern.md) |
| **Password Reset Flow** | v1.0.0 | ✅ Complete | [Wiki 14](docs/wiki/14-password-reset-flow.md) |
| **Pagination Factory** | v1.0.0 | ✅ Complete | [Wiki 15](docs/wiki/15-pagination-factory-method.md) |
| **Simple Form Validation** | v1.0.0 | ✅ Complete | [Wiki 16](docs/wiki/16-simple-form-validation.md) |
| **Multi-Tenant Login** | v1.0.0 | ✅ Complete | [Wiki 17](docs/wiki/17-multi-tenant-login-flow.md) |
| **TenantId Design** | v1.0.0 | ✅ Complete | [Wiki 18](docs/wiki/18-why-tenantid-in-multiple-tables.md) |
| **Multi-Tenant Architecture** | v1.0.0 | ✅ Complete | [Wiki 19](docs/wiki/19-multi-tenant-architecture.md) |
| **Cache Attribute** | v1.0.0 | ✅ Complete | [Wiki 20](docs/wiki/20-cache-attribute.md) |
| **Template Engine** | v1.0.0 | ✅ Complete | [Wiki 21](docs/wiki/21-template-engine.md) |
| **Swagger & OpenAPI** | v1.0.0 | ✅ Complete | [Wiki 22](docs/wiki/22-swagger-openapi.md) |
| **EF Core Migrations** | v1.0.0 | ✅ Complete | [Wiki 23](docs/wiki/23-ef-core-migrations.md) |
| **MassTransit Queue** | v1.0.0 | ✅ Complete | [Wiki 24](docs/wiki/24-masstransit-message-queue.md) |
| **Blazor Form Builder** | v1.0.0 | ✅ Complete | [Wiki 25](docs/wiki/25-blazor-form-builder.md) |
| **Mobile XAML Components** | v1.0.0 | ✅ Complete | [Wiki 26](docs/wiki/26-mobile-xaml-components.md) |
| **PDF/Excel Export** | v1.0.0 | ✅ Complete | [Wiki 27](docs/wiki/27-pdf-excel-export.md) |
| **Stored Procedures** | v1.0.0 | ✅ Complete | 76 SPs across 5 schemas |

## All 25+ Components & Features

### Foundation Components (v4.0.0)

#### Multi-Tenancy & Authorization (5 components)
- **Multi-Tenant Architecture:** Row-level TenantId isolation across all schemas
- **Tenant Authorization:** Three-level authorization (Super Admin, Tenant Admin, User)
- **Feature Flags:** Per-tenant feature control with Tenant & User scope
- **Tenant Settings:** Configurable defaults with override support
- **Tenant Subscriptions:** Plan-based feature enablement

#### Geo & Hierarchy System (4 components)
- **Geo Hierarchy (Option C Hybrid):** Countries + HierarchyId for flexible State/City/District nesting
- **Menu System:** Dynamic role-based navigation with breadcrumbs & auto-sitemap
- **Entity Hierarchies:** Unlimited nesting for Tenants, Lookups, Categories, EntityStates
- **Location Management:** Country/Region/District with auto-geocoding support

#### Polymorphic Infrastructure (5 components)
- **Addresses:** Link to any entity (Customer, Order, Employee, etc.)
- **Attachments:** File references & versioning for any entity
- **Comments:** Discussion threads & nested conversations for any entity
- **StateHistory:** Workflow tracking & audit trail for any entity
- **Preferences:** System/tenant/user-level configuration management

#### Localization & Validation (3 components)
- **Translation System:** Database-backed with 60-minute cache, per-tenant overrides, multi-locale support
- **Localized Validation:** MessageKey validation attributes with translated error messages
- **Message Management:** Dynamic message key registry with fallback support

#### RESTful API & Data Access (5 components)
- **REST API Endpoints:** 25+ typed endpoints with validation & error handling
- **CRUD Operations:** Standardized patterns for Create, Read, Update, Delete, Search
- **Pagination:** Factory-based pagination with sorting & filtering
- **Data Export:** CSV, Excel, PDF export with formatting & streaming
- **Swagger/OpenAPI:** Auto-generated API documentation with example requests

#### Pattern Implementations (5 components)
- **Base Page Pattern:** TenantId, T() translation helper, toast messages, error handling
- **Result Pattern:** Explicit Result/Result<T> for safe error handling without exceptions
- **HTMX Integration:** Progressive enhancement for search/filter/pagination without page reloads
- **Simple Form Validation:** Client + server-side validation with custom error messages
- **Cache Attribute:** One-line response caching with configurable TTL

#### Business Entity CRUD (3 components)
- **Products Admin CRUD:** Full admin pages for product management
- **Product Catalog:** Public-facing product listing with search/filter
- **Category Management:** Hierarchical categories with navigation support

#### Production-Ready Reporting (4 components)
- **Report Definitions:** SQL, Dashboard, Stored Procedure, API report types
- **Report Scheduling:** Cron-based background execution with email delivery
- **Report Execution:** Audit trail, performance metrics, result caching
- **Report Metadata:** Filters, drill-downs, conditional formatting (extensible JSON)

### Advanced Components (v4.1.0+)

#### Authentication & Security (4 components)
- **Password Reset Flow:** Secure token-based reset with email delivery
- **Multi-Tenant Login:** Login across multiple tenants with tenant selection UI
- **Two-Factor Authentication:** SMS/Email OTP with configurable policies
- **Login Audit Trail:** Attempt tracking with lockout & IP-based rules

#### Frontend Components (8 components)
- **Blazor Grid Component:** Sortable, filterable, paginated data grid with responsive design
- **Blazor List View Component:** Flexible list display with customizable item templates
- **Blazor Data Viewer:** Multi-view data display (Grid/List/Card)
- **15+ Tag Helpers:** Form helpers, display helpers, security helpers, conditional rendering
- **Form Builder:** Blazor component for dynamic form generation from JSON metadata
- **XAML Mobile Components:** Native iOS/Android XAML controls for MAUI
- **Translation Demo Page:** Interactive showcase of all MessageKeys with current translations
- **Validation Demo Page:** Form demonstrating all validation attribute types

#### Data & File Management (4 components)
- **PDF Export:** QuestPDF-based PDF generation with formatting & streaming
- **Excel Export:** NPOI-based Excel with formatting, formulas, charts
- **File Storage:** Polymorphic file management with versioning & cleanup
- **File Upload:** Async upload with virus scanning & size validation

#### Background Jobs & Messaging (3 components)
- **Hangfire Background Jobs:** Scheduled tasks, recurring jobs, dashboard
- **MassTransit Message Queue:** Async event processing with retry policies
- **WebSocket/SignalR:** Real-time notifications & client updates

#### Database & Stored Procedures (2 components)
- **76+ Stored Procedures:** CRUD, complex queries, reporting across all schemas
- **EF Core Migrations:** Version-controlled schema changes with rollback support

#### Communication Services (3 components)
- **Email Service:** Template-based email with attachments & retry logic
- **SMS Service:** SMS delivery with carrier routing & delivery confirmation
- **Push Notifications:** FCM integration for mobile push notifications

### SmartWorkz Core Integration (181+ Classes)

#### Core Framework (25+ feature areas)
- ✅ Multi-database support (SQL Server, MySQL, PostgreSQL, SQLite)
- ✅ CQRS + Event Sourcing architecture
- ✅ Comprehensive encryption & JWT security
- ✅ Structured logging (Serilog with JSON output)
- ✅ Cross-platform mobile (iOS, Android, macOS, Windows via MAUI)
- ✅ 10+ native mobile platform services
- ✅ Offline-first mobile with 3-strategy conflict resolution
- ✅ Real-time SignalR integration
- ✅ Webhook system with retry logic
- ✅ Feature flags per tenant
- ✅ In-memory caching (L1) with TTL and tenant isolation
- ✅ Circuit breaker + rate limiting patterns
- ✅ Comprehensive security (encryption, JWT, sanitization, HMAC)
- ✅ Permission management framework
- ✅ Analytics & telemetry tracking

#### Shared Library (181+ components)
- **Data Access:** ADO.NET, Dapper, Entity Framework abstractions
- **HTTP Client:** REST client with interceptors, retry policies, timeout handling
- **Security:** Encryption, hashing, JWT generation, HMAC signing
- **Utilities:** String, date, LINQ extensions, helper methods
- **Services:** Email, SMS, logging, file I/O, health checks
- **Caching:** L1 in-memory, L2 distributed, tenant isolation
- **Configuration:** Diagnostics, dependency injection, feature flags

## Component Index (27 Guides + 181+ Classes)

### Getting Started Guides
| Guide | File | Description |
|-------|------|-------------|
| Master Guide | [00-master-guide.md](docs/wiki/00-master-guide.md) | Navigation hub for all documentation |
| Getting Started | [01-getting-started.md](docs/wiki/01-getting-started.md) | Initial setup and first steps |
| Project Overview | [02-project-overview.md](docs/wiki/02-project-overview.md) | Complete architecture overview |

### Framework & Ecosystem
| Guide | File | Description |
|-------|------|-------------|
| SmartWorkz Core | [03-smartworkz-core.md](docs/wiki/03-smartworkz-core.md) | Shared library (181+ components) |
| Core Web | [04-smartworkz-core-web.md](docs/wiki/04-smartworkz-core-web.md) | Web framework components |
| Core Shared | [05-smartworkz-core-shared.md](docs/wiki/05-smartworkz-core-shared.md) | Shared utilities & services |
| Core External | [06-smartworkz-core-external.md](docs/wiki/06-smartworkz-core-external.md) | Integration with external services |
| Mobile (MAUI) | [07-smartworkz-mobile.md](docs/wiki/07-smartworkz-mobile.md) | iOS, Android, macOS, Windows |
| Step-by-Step | [08-step-by-step-guide.md](docs/wiki/08-step-by-step-guide.md) | Guided implementation walkthrough |

### Core Patterns (9 guides)
| Guide | File | Description |
|-------|------|-------------|
| Translation System | [09-translation-system.md](docs/wiki/09-translation-system.md) | Multi-language DB-backed translations |
| Localized Validation | [10-localized-validation.md](docs/wiki/10-localized-validation.md) | MessageKey validation attributes |
| Base Page Pattern | [11-base-page-pattern.md](docs/wiki/11-base-page-pattern.md) | Razor Page inheritance, T() helper |
| Result Pattern | [12-result-pattern.md](docs/wiki/12-result-pattern.md) | Explicit Result<T> error handling |
| HTMX Integration | [13-htmx-list-pattern.md](docs/wiki/13-htmx-list-pattern.md) | Dynamic lists without page reload |
| Password Reset | [14-password-reset-flow.md](docs/wiki/14-password-reset-flow.md) | Secure token-based reset |
| Pagination | [15-pagination-factory-method.md](docs/wiki/15-pagination-factory-method.md) | Factory-based pagination |
| Form Validation | [16-simple-form-validation.md](docs/wiki/16-simple-form-validation.md) | Client + server validation |
| Multi-Tenant Login | [17-multi-tenant-login-flow.md](docs/wiki/17-multi-tenant-login-flow.md) | Tenant selection UI |

### Architecture Guides (5 guides)
| Guide | File | Description |
|-------|------|-------------|
| TenantId Design | [18-why-tenantid-in-multiple-tables.md](docs/wiki/18-why-tenantid-in-multiple-tables.md) | Row-level isolation rationale |
| Multi-Tenant Architecture | [19-multi-tenant-architecture.md](docs/wiki/19-multi-tenant-architecture.md) | Tenant isolation & data flow |
| Cache Attribute | [20-cache-attribute.md](docs/wiki/20-cache-attribute.md) | Response caching decorator |
| Template Engine | [21-template-engine.md](docs/wiki/21-template-engine.md) | Email/SMS template rendering |

### Advanced Guides (4 guides)
| Guide | File | Description |
|-------|------|-------------|
| Swagger & OpenAPI | [22-swagger-openapi.md](docs/wiki/22-swagger-openapi.md) | API documentation generation |
| EF Core Migrations | [23-ef-core-migrations.md](docs/wiki/23-ef-core-migrations.md) | Database versioning & rollback |
| MassTransit Queue | [24-masstransit-message-queue.md](docs/wiki/24-masstransit-message-queue.md) | Async event processing |
| Blazor Form Builder | [25-blazor-form-builder.md](docs/wiki/25-blazor-form-builder.md) | Dynamic form generation |
| Mobile XAML | [26-mobile-xaml-components.md](docs/wiki/26-mobile-xaml-components.md) | Native MAUI components |
| Export Services | [27-pdf-excel-export.md](docs/wiki/27-pdf-excel-export.md) | PDF/Excel/CSV export |

### Feature Documentation
| Feature | Document | Description |
|---------|----------|-------------|
| Tenant Authorization | [TENANT_AUTHORIZATION.md](docs/TENANT_AUTHORIZATION.md) | Three-level permission model |
| Tenant Auth Quick Start | [TENANT_AUTHORIZATION_QUICK_START.md](docs/TENANT_AUTHORIZATION_QUICK_START.md) | Setup & integration steps |
| Deployment | [DEPLOYMENT-MULTIVIEW.md](docs/DEPLOYMENT-MULTIVIEW.md) | Multi-view Grid/List deployment |
| Grid Component | [GRID_COMPONENT_WIKI.md](docs/GRID_COMPONENT_WIKI.md) | Sortable, filterable data grid |
| Tag Helpers | [TAGHELPERS_GUIDE.md](docs/TAGHELPERS_GUIDE.md) | 15+ form & display helpers |
| Database Optimization | [DATABASE_OPTIMIZATION_GUIDE.md](docs/DATABASE_OPTIMIZATION_GUIDE.md) | Query tuning & indexing |
| Data Access | [DATA_ACCESS.md](docs/DATA_ACCESS.md) | ADO.NET, Dapper, EF Core |
| HTTP Client | [HTTP_CLIENT.md](docs/HTTP_CLIENT.md) | REST client & retry policies |
| Security | [SECURITY.md](docs/SECURITY.md) | JWT, encryption, hashing |
| Developer Guide | [DEVELOPER.md](docs/DEVELOPER.md) | Complete patterns reference |

## Extensibility

Teams can extend with Phase 1+ additions:
- Custom business entities (Products, Customers, Employees)
- Additional transaction types (Invoices, Payments, POs)
- Report distribution (Email, Slack, Teams)
- Advanced workflows
- Custom dashboards
- GraphQL API layer
- Admin dashboard with analytics

No schema changes needed—use polymorphic pattern for linking.

## Old Documentation

v1 documentation archived in [`docs/old/`](docs/old/):
- README-v1.md (original project overview)
- SETUP.md (v1 setup instructions)
- architecture.md (v1 architecture)
- etc.

## Current Status

**v4.1.0 Features Complete:**
- ✅ Translation System (v1.1.0) — Production-ready, fully documented
- ✅ Localized Validation (v1.1.0) — Simple MessageKey approach, working examples
- ✅ Base Page Pattern (v1.1.0) — TenantId, T(), toast helpers, form handling
- ✅ Result Pattern (v1.1.0) — Explicit success/failure for all services
- ✅ HTMX List Pattern (v1.1.0) — Dynamic search/filter/pagination
- ✅ Demo Pages (v1.0.0) — Translations and Validation showcases
- ✅ Wiki Documentation (v1.0.0) — 5 comprehensive guides with examples
- ✅ Admin Products CRUD (v1.0.0) — Sample pages for admin area
- ✅ Public Product Catalog (v1.0.0) — Index and details pages

**Next Phase:**
- ⏳ Connect mock implementations to real repositories
- ⏳ Build additional admin feature areas
- ⏳ Implement frontend-specific features (cart, checkout)
- ⏳ Phase 1+ Extensions: Custom business entities, transaction types, advanced workflows

## Effort Estimate

- **Phase 1:** 35-46 hours (database, entities, services, REST API)
- **Phase 2:** 8-10 hours (documentation)
- **Phase 3:** 20-30 hours (MVC integration, testing)
- **Phase 4:** 10-15 hours (API polish, security)
- **Total:** 72-99 hours (3-4 weeks for 1-3 developers)

## References

- **SRS Document:** [`docs/srs/SRS-v4.md`](docs/srs/SRS-v4.md)
- **Implementation Plan:** [`docs/srs/IMPLEMENTATION-PLAN.md`](docs/srs/IMPLEMENTATION-PLAN.md)
- **Schema Review:** [`docs/srs/SCHEMA-REVIEW-v2.md`](docs/srs/SCHEMA-REVIEW-v2.md)

---

**SmartWorkz v4** — Enterprise-grade starter kit for multi-tenant .NET applications
