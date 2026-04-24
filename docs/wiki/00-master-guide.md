# Master Guide — SmartWorkz Starter Kit Wiki

Welcome to the SmartWorkz Starter Kit documentation hub! This guide aggregates all 19 wiki pages across three architectural tiers: Web Application, Infrastructure, and Mobile. Use this page to find the right documentation for your role and navigate quickly to features you're implementing.

---

## System Architecture — Three-Tier Overview

```
┌──────────────────────────────────────────────────────────────────────┐
│                   PUBLIC WEB APP TIER                                │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  Routing  │  Controllers  │  Views  │  Form Builder │  Export │  │
│  │   (01-03) │  (API-14)     │  (05)   │    (17)       │  (19)   │  │
│  └────────────────────────────────────────────────────────────────┘  │
│                                                                        │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  Validation  │  Translation  │  Base Page  │  Password Reset  │  │
│  │    (08)      │     (01-02)    │    (03)     │      (06)        │  │
│  └────────────────────────────────────────────────────────────────┘  │
│                                                                        │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  List Patterns  │  Pagination  │  Template Engine  │  Caching  │  │
│  │    (05, 08)     │    (07)      │     (13)          │   (12)    │  │
│  └────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
                              ↓    ↑
┌──────────────────────────────────────────────────────────────────────┐
│               INFRASTRUCTURE TIER                                     │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  Multi-Tenant Auth  │  Login Flow  │  Tenant Architecture     │  │
│  │     (09, 10)        │    (09)      │      (10-11)             │  │
│  └────────────────────────────────────────────────────────────────┘  │
│                                                                        │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  Database Migrations  │  Result Pattern  │  Message Queue      │  │
│  │       (15)            │     (04)         │     (16)            │  │
│  └────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
                              ↓    ↑
┌──────────────────────────────────────────────────────────────────────┐
│                    MOBILE APP TIER                                    │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  XAML Components (18)                                          │  │
│  │  ┌──────────────────────────────────────────────────────────┐ │  │
│  │  │ Button │ Entry │ Picker │ Dialog │ List │ Validation    │ │  │
│  │  │ Command Integration  │  .NET MAUI Platform Optimization │ │  │
│  │  └──────────────────────────────────────────────────────────┘ │  │
│  └────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Complete Table of Contents

### Phase 1.0: Core Patterns & Infrastructure (Pages 01-11, 13)

#### Web Application Fundamentals (01-08)

| # | Page | Title | Purpose |
|---|------|-------|---------|
| 01 | [Translation System](./01-translation-system.md) | Database-backed multi-language support | Single source of truth for user-facing messages with per-tenant overrides |
| 02 | [Localized Validation](./02-localized-validation.md) | Translate validation messages at render time | Apply MessageKey constants to validation attributes for automatic translation |
| 03 | [Base Page Pattern](./03-base-page-pattern.md) | Common foundation for all Razor pages | Tenant context, translation helpers, auth properties on every page |
| 04 | [Result Pattern](./04-result-pattern.md) | Structured success/failure outcomes | Type-safe error handling without exception-based control flow |
| 05 | [HTMX List Pattern](./05-htmx-list-pattern.md) | Dynamic list updates without page reload | AJAX-based search/filter with progressive enhancement |
| 06 | [Password Reset Flow](./06-password-reset-flow.md) | Secure password reset via email tokens | Request reset → validate token → set new password workflow |
| 07 | [Pagination Factory Method](./07-pagination-factory-method.md) | Convert API responses to UI view models | Transform PaginationResponse<T> to PaginationModel for rendering |
| 08 | [Simple Form Validation](./08-simple-form-validation.md) | Client + server validation with translation | DataAnnotations with database-backed error messages |

#### Multi-Tenant Architecture (09-11)

| # | Page | Title | Purpose |
|---|------|-------|---------|
| 09 | [Multi-Tenant Login Flow](./09-multi-tenant-login-flow.md) | How tenants and users interact during login | Explains routing when same email exists in multiple tenants |
| 10 | [Why TenantId in Multiple Tables](./10-why-tenantid-in-multiple-tables.md) | Tenant isolation design rationale | Clarifies when and why TenantId appears in Users + TenantUsers tables |
| 11 | [Multi-Tenant Architecture](./11-multi-tenant-architecture.md) | Visual guide to tenant isolation | Entity relationships and database schema for multi-tenant support |

#### Shared Services (12-13)

| # | Page | Title | Purpose |
|---|------|-------|---------|
| 12 | [Cache Attribute Pattern](./12-cache-attribute.md) | Decorator-based HTTP result caching | `[Cache]` attribute for read-only endpoints without manual code |
| 13 | [Template Engine Pattern](./13-template-engine.md) | Dynamic template rendering with placeholders | Render email/SMS templates with {Name} and {{TranslationKey}} substitution |

### Phase 2.0: Advanced Features (Pages 14-19)

#### API & Integration (14-16)

| # | Page | Title | Purpose |
|---|------|-------|---------|
| 14 | [Swagger/OpenAPI Documentation](./14-swagger-openapi.md) | Auto-generated API documentation | RESTful endpoint discovery with JWT auth and XML doc support |
| 15 | [EF Core Multi-Context Migrations](./15-ef-core-migrations.md) | Orchestrate 5 independent DbContext schemas | Migration sequencing across Auth, Master, Shared, Transaction, Report contexts |
| 16 | [MassTransit Message Queue](./16-masstransit-message-queue.md) | Asynchronous event-driven messaging | Publisher-subscriber pattern for decoupled business events |

#### Dynamic UI & Export (17-19)

| # | Page | Title | Purpose |
|---|------|-------|---------|
| 17 | [Blazor Form Builder](./17-blazor-form-builder.md) | Dynamic form generation and validation | Convert form definitions to interactive forms with conditional fields |
| 18 | [Mobile XAML Components](./18-mobile-xaml-components.md) | .NET MAUI reusable UI components | Six production-ready components: Button, Entry, Picker, Dialog, List, Validation |
| 19 | [PDF/Excel Export](./19-pdf-excel-export.md) | Binary file export with fluent API | Export collections to PDF (iText7) or Excel (ClosedXML) with styling |

---

## Getting Started: Role-Based Learning Paths

### Path A: New to SmartWorkz (Onboarding)

Start here if you're unfamiliar with the starter kit architecture. Follow this 5-page sequence to understand the foundation:

1. **[Base Page Pattern](./03-base-page-pattern.md)** — Understand the page model foundation (5 min)
2. **[Result Pattern](./04-result-pattern.md)** — Learn structured error handling (10 min)
3. **[Translation System](./01-translation-system.md)** — See how multi-language works (10 min)
4. **[Multi-Tenant Architecture](./11-multi-tenant-architecture.md)** — Grasp tenant isolation visually (10 min)
5. **[HTMX List Pattern](./05-htmx-list-pattern.md)** — Build your first dynamic list (15 min)

**Time:** ~50 minutes  
**Outcome:** You understand pages, error handling, multi-tenancy, and AJAX patterns.

---

### Path B: Building a New Feature

You're adding a feature to the web app (e.g., a user management page). Follow this 7-page sequence:

1. **[Base Page Pattern](./03-base-page-pattern.md)** — Structure your PageModel (5 min)
2. **[Localized Validation](./02-localized-validation.md)** — Add validation to your form (10 min)
3. **[Simple Form Validation](./08-simple-form-validation.md)** — Implement form submission (10 min)
4. **[Result Pattern](./04-result-pattern.md)** — Handle service responses (5 min)
5. **[HTMX List Pattern](./05-htmx-list-pattern.md)** — Add search/filter (15 min)
6. **[Pagination Factory Method](./07-pagination-factory-method.md)** — Enable pagination (10 min)
7. **[Cache Attribute Pattern](./12-cache-attribute.md)** — Cache read-only endpoints (5 min)

**Time:** ~60 minutes  
**Outcome:** You can build a complete CRUD page with validation, filtering, and caching.

**Optional enhancements:**
- Add export via [PDF/Excel Export](./19-pdf-excel-export.md) (15 min)
- Trigger async work with [MassTransit Message Queue](./16-masstransit-message-queue.md) (15 min)
- Build dynamic forms with [Blazor Form Builder](./17-blazor-form-builder.md) (20 min)

---

### Path C: Mobile Developer

You're building a .NET MAUI app using SmartWorkz services. Follow this 5-page sequence:

1. **[Mobile XAML Components](./18-mobile-xaml-components.md)** — Explore six reusable components (15 min)
2. **[Result Pattern](./04-result-pattern.md)** — Understand service response types (10 min)
3. **[Multi-Tenant Architecture](./11-multi-tenant-architecture.md)** — Understand tenant context (10 min)
4. **[Translation System](./01-translation-system.md)** — Implement localization (10 min)
5. **[MassTransit Message Queue](./16-masstransit-message-queue.md)** — Handle async events (15 min)

**Time:** ~60 minutes  
**Outcome:** You can build a mobile app using SmartWorkz components and services.

**API Integration:**
- Reference [Swagger/OpenAPI Documentation](./14-swagger-openapi.md) for backend endpoints (10 min)
- Use [Password Reset Flow](./06-password-reset-flow.md) for authentication (10 min)

---

## Feature Dependency Map

This table shows cross-references between features. If you use Feature X, also review Feature Y for integration context.

| Feature | Primary Page | Dependencies | Recommended Reading | Reason |
|---------|--------------|--------------|---------------------|--------|
| **Translation** | 01 | Localization, Validation, Base Page | 02, 03, 08 | Translations appear in validation messages and page titles |
| **Form Validation** | 08 | Translation, Localized Validation, Base Page | 02, 03, 01 | Error messages come from translation database |
| **HTMX Lists** | 05 | Pagination, Result Pattern, Caching | 07, 04, 12 | Lists need pagination and benefit from caching |
| **Pagination** | 07 | HTMX List, Base Page | 05, 03 | Pagination UI works with AJAX-based lists |
| **Result Pattern** | 04 | Base Page | 03 | All services return Result; pages need to handle it |
| **Multi-Tenant** | 09, 10, 11 | Base Page, Login Flow, Result Pattern | 03, 09, 04 | Tenant context flows through all pages |
| **API Export** | 14 | Result Pattern, Controller Integration, Swagger | 04, 19 | Swagger documents your export endpoints |
| **PDF/Excel Export** | 19 | Result Pattern, Pagination, Base Page | 04, 07, 03 | Export filtered/paginated data via Result<byte[]> |
| **Blazor Forms** | 17 | Base Page, Validation, Result Pattern | 03, 08, 04 | Dynamic forms need validation and result handling |
| **Async Messages** | 16 | Result Pattern, Multi-Tenant | 04, 11 | Publish events that respect tenant isolation |
| **Database** | 15 | Multi-Tenant, Result Pattern | 11, 04 | Migrations affect multi-tenant schema |
| **Caching** | 12 | Base Page, API/Swagger, Result Pattern | 03, 14, 04 | Cache API responses for performance |
| **Mobile Components** | 18 | Result Pattern, Translation, Multi-Tenant | 04, 01, 11 | Components consume same services as web app |

---

## Integration Points Between Phases

### Phase 1 → Phase 2 Transitions

**Database Layer:**
- Phase 1: Basic tables with TenantId
- Phase 2: [EF Core Migrations](./15-ef-core-migrations.md) orchestrates schema evolution across 5 contexts

**API Layer:**
- Phase 1: Basic controllers return Result<T>
- Phase 2: [Swagger/OpenAPI](./14-swagger-openapi.md) auto-documents all endpoints

**Messaging:**
- Phase 1: Services call each other directly
- Phase 2: [MassTransit Message Queue](./16-masstransit-message-queue.md) decouples services via events

**Forms:**
- Phase 1: Hand-coded Razor forms with static validation
- Phase 2: [Blazor Form Builder](./17-blazor-form-builder.md) generates forms dynamically

**Export:**
- Phase 1: Data displayed in lists via [HTMX](./05-htmx-list-pattern.md)
- Phase 2: [PDF/Excel Export](./19-pdf-excel-export.md) lets users download filtered data

**Mobile:**
- Phase 1: Web-only app
- Phase 2: [Mobile XAML Components](./18-mobile-xaml-components.md) share validation + Result types with web

---

## Quick Links by Tier

### Web App Tier (Wikis 01-08, 12-14, 17, 19)

**Learn the Patterns First:**
- [Translation System](./01-translation-system.md) — Multi-language messages
- [Base Page Pattern](./03-base-page-pattern.md) — Page model foundation
- [Result Pattern](./04-result-pattern.md) — Error handling

**Build Pages:**
- [HTMX List Pattern](./05-htmx-list-pattern.md) — Dynamic lists
- [Simple Form Validation](./08-simple-form-validation.md) — Form submission
- [Pagination Factory Method](./07-pagination-factory-method.md) — Paginated data

**Enhance with Services:**
- [Cache Attribute Pattern](./12-cache-attribute.md) — Performance
- [Template Engine](./13-template-engine.md) — Email/SMS rendering
- [Swagger/OpenAPI](./14-swagger-openapi.md) — API documentation
- [Blazor Form Builder](./17-blazor-form-builder.md) — Dynamic forms
- [PDF/Excel Export](./19-pdf-excel-export.md) — Data export

**Authentication:**
- [Password Reset Flow](./06-password-reset-flow.md) — Secure reset process

---

### Infrastructure Tier (Wikis 09-11, 15-16)

**Multi-Tenant Setup:**
- [Multi-Tenant Login Flow](./09-multi-tenant-login-flow.md) — Login architecture
- [Why TenantId in Multiple Tables](./10-why-tenantid-in-multiple-tables.md) — Design rationale
- [Multi-Tenant Architecture](./11-multi-tenant-architecture.md) — Visual guide

**Database & Messaging:**
- [EF Core Multi-Context Migrations](./15-ef-core-migrations.md) — Schema management
- [MassTransit Message Queue](./16-masstransit-message-queue.md) — Event-driven design

---

### Mobile Tier (Wiki 18)

**Build Mobile Apps:**
- [Mobile XAML Components](./18-mobile-xaml-components.md) — Six reusable components for .NET MAUI

**Integrate with Backend:**
- Reference [Swagger/OpenAPI](./14-swagger-openapi.md) for endpoints
- Use [Translation System](./01-translation-system.md) for localization
- Handle [Result Pattern](./04-result-pattern.md) responses from services

---

## Troubleshooting & FAQ

### "I'm seeing validation errors but they're not translated"
See [Localized Validation](./02-localized-validation.md) — error messages must use MessageKey constants.

### "My list isn't filtering dynamically"
See [HTMX List Pattern](./05-htmx-list-pattern.md) — ensure `Request.IsHtmx()` is checked and `PageOrPartial()` returns correct view.

### "I need to add a new database table"
See [EF Core Multi-Context Migrations](./15-ef-core-migrations.md) — identify which DbContext owns the table and use MigrationManager.

### "My PDF exports show numbers as text in formulas"
See [PDF/Excel Export - Gotcha 1](./19-pdf-excel-export.md#gotcha-1-excelexporter-converts-values-to-tostring) — use typed properties, not ToString().

### "How do I export multiple sheets in Excel?"
See [PDF/Excel Export - Example 2](./19-pdf-excel-export.md#example-2-multi-sheet-excel-with-exportmultipleasync) — use `ExportMultipleAsync()` with sheet dictionary.

### "Password reset emails aren't being sent"
See [Password Reset Flow - Troubleshooting](./06-password-reset-flow.md#troubleshooting) — emails are queued asynchronously; check MassTransit status.

### "My mobile app can't get tenant data"
See [Multi-Tenant Architecture](./11-multi-tenant-architecture.md) — ensure tenant context is passed in API calls and validated server-side.

### "I need dynamic forms that change based on user input"
See [Blazor Form Builder](./17-blazor-form-builder.md) — use conditional field visibility and dynamic validation rules.

### "Should I cache this API endpoint?"
See [Cache Attribute Pattern](./12-cache-attribute.md) — use `[Cache]` for read-only, low-variance endpoints; avoid for user-specific or rapidly-changing data.

### "How do I trigger async work when a user submits a form?"
See [MassTransit Message Queue](./16-masstransit-message-queue.md) — publish an event that subscribers handle asynchronously.

---

## File Organization

All 19 wiki pages live in `docs/wiki/` directory:

```
docs/wiki/
├── 00-master-guide.md              ← You are here
├── 01-translation-system.md
├── 02-localized-validation.md
├── 03-base-page-pattern.md
├── 04-result-pattern.md
├── 05-htmx-list-pattern.md
├── 06-password-reset-flow.md
├── 07-pagination-factory-method.md
├── 08-simple-form-validation.md
├── 09-multi-tenant-login-flow.md
├── 10-why-tenantid-in-multiple-tables.md
├── 11-multi-tenant-architecture.md
├── 12-cache-attribute.md
├── 13-template-engine.md
├── 14-swagger-openapi.md
├── 15-ef-core-migrations.md
├── 16-masstransit-message-queue.md
├── 17-blazor-form-builder.md
├── 18-mobile-xaml-components.md
└── 19-pdf-excel-export.md
```

Each page is self-contained with:
- **Overview** — Purpose and when to use
- **Architecture** — Design patterns and data flow
- **Quick Start** — Minimal working example
- **Configuration** — Options and defaults
- **Usage Examples** — Real-world code samples
- **API Reference** — Methods and signatures
- **Integration Notes** — Cross-references to related pages
- **Troubleshooting** — Common issues and solutions
- **Best Practices** — Do's and don'ts

---

## Conventions

### Link Format

All internal links use relative paths in `./NN-*.md` format:

```markdown
See [Result Pattern](./04-result-pattern.md) for error handling.
```

Do NOT use absolute paths or `#` anchors that might break.

### Code Examples

All C# examples follow these patterns:
- **Async/await:** Methods are `async Task<>` or `async Task`
- **Result handling:** Check `result.IsFailure` or `result.IsSuccess`
- **DI pattern:** Services injected via constructor
- **Error messages:** Use `Result.Fail()` for business errors

### ASCII Diagrams

Architecture diagrams use box-drawing characters:
```
┌─────┐     ┌─────┐
│  A  │────▶│  B  │
└─────┘     └─────┘
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024-Q4 | Initial 19-page wiki with Phase 1.0 and Phase 2.0 features |

---

## Contributing to the Wiki

When adding or updating wiki pages:

1. **Title Format:** Use `# Topic Name` for the main heading
2. **Sections:** Follow: Overview → Architecture → Quick Start → Config/API → Examples → Integration → Troubleshooting → Best Practices
3. **Links:** Update [Master Guide - Table of Contents](#complete-table-of-contents) when adding new pages
4. **Examples:** Include working C# code with error handling
5. **Dependencies:** List cross-references in Integration Notes section

---

## Next Steps

1. **Pick your role:** Onboarding (Path A), Feature Builder (Path B), or Mobile Dev (Path C)
2. **Follow the learning path:** Read pages in recommended order
3. **Refer to troubleshooting:** If you hit issues, search this master guide or jump to specific pages
4. **Contribute:** Update docs when you discover new patterns or gotchas

Happy building! 🚀
