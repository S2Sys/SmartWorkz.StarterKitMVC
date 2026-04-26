# Master Guide — SmartWorkz Starter Kit Wiki

Welcome to the SmartWorkz Starter Kit documentation hub. **Start with Foundation Series (01-08)** for a complete zero-to-hero learning path, then explore Pattern Library (09-27) for advanced reference material.

---

## 🎯 START HERE: Foundation Series (01-08)

New to SmartWorkz? Begin with files **01-08 in order**. This is a complete learning path with everything you need to build your first feature.

**01 — [Getting Started](./01-getting-started.md)**  
Prerequisites, clone + run (10 min)

**02 — [Project Overview](./02-project-overview.md)**  
Architecture diagram, DLL map, dependencies (15 min)

**03 — [SmartWorkz.Core](./03-smartworkz-core.md)**  
Entity hierarchy, Value Objects, Guard, EntityState — extracted from XML docs (25 min)

**04 — [SmartWorkz.Core.Web](./04-smartworkz-core-web.md)**  
Tag Helpers, Blazor Grid, Validation, GraphQL — extracted from XML docs (20 min)

**05 — [SmartWorkz.Core.Shared](./05-smartworkz-core-shared.md)**  
Caching, CQRS, Logging, Webhooks — extracted from XML docs (25 min)

**06 — [SmartWorkz.Core.External](./06-smartworkz-core-external.md)**  
Excel & PDF export, Result<T> pattern — extracted from XML docs (15 min)

**07 — [SmartWorkz.Mobile](./07-smartworkz-mobile.md)**  
MAUI components, offline queue, push, background sync — extracted from XML docs (20 min)

**08 — [Step-by-Step Guide](./08-step-by-step-guide.md)**  
Build your first feature with exact code: new project → list → export → CQRS (45 min)

**Total: ~2.5 hours**

---

## Pattern Library (09-27)

After Foundation Series, explore these patterns for specific features.

### Web Application Patterns

**09 — [Translation System](./09-translation-system.md)**  
Database-backed multi-language support

**10 — [Localized Validation](./10-localized-validation.md)**  
Translate validation messages at render time

**11 — [Base Page Pattern](./11-base-page-pattern.md)**  
Common foundation for all Razor pages

**12 — [Result Pattern](./12-result-pattern.md)**  
Structured success/failure outcomes

**13 — [HTMX List Pattern](./13-htmx-list-pattern.md)**  
Dynamic list updates without page reload

**14 — [Password Reset Flow](./14-password-reset-flow.md)**  
Secure password reset via email tokens

**15 — [Pagination Factory Method](./15-pagination-factory-method.md)**  
Convert API responses to UI view models

**16 — [Simple Form Validation](./16-simple-form-validation.md)**  
Client + server validation with translation

**20 — [Cache Attribute Pattern](./20-cache-attribute.md)**  
Decorator-based HTTP result caching

**21 — [Template Engine Pattern](./21-template-engine.md)**  
Dynamic template rendering with placeholders

**22 — [Swagger/OpenAPI Documentation](./22-swagger-openapi.md)**  
Auto-generated API documentation

**25 — [Blazor Form Builder](./25-blazor-form-builder.md)**  
Dynamic form generation and validation

**27 — [PDF/Excel Export](./27-pdf-excel-export.md)**  
Binary file export with styling

### Infrastructure & Multi-Tenancy

**17 — [Multi-Tenant Login Flow](./17-multi-tenant-login-flow.md)**  
How tenants and users interact during login

**18 — [Why TenantId in Multiple Tables](./18-why-tenantid-in-multiple-tables.md)**  
Tenant isolation design rationale

**19 — [Multi-Tenant Architecture](./19-multi-tenant-architecture.md)**  
Visual guide to tenant isolation

**23 — [EF Core Multi-Context Migrations](./23-ef-core-migrations.md)**  
Orchestrate 5 independent DbContext schemas

**24 — [MassTransit Message Queue](./24-masstransit-message-queue.md)**  
Asynchronous event-driven messaging

### Mobile & Cross-Platform

**26 — [Mobile XAML Components](./26-mobile-xaml-components.md)**  
MAUI reusable UI components

---

## File Organization

All 27 wiki pages in `docs/wiki/`:

**Foundation Series (01-08)** — Start here for zero-to-hero  
**Pattern Library (09-27)** — Advanced reference material

---

## Quick Navigation by Role

**Web Developer**  
→ Foundation (01-08), then files 09-12, 13, 15-16, 20-22, 25, 27

**Infrastructure Engineer**  
→ Foundation (01-08, focus 02-05), then files 12, 14, 17-19, 23-24

**Mobile Developer**  
→ Foundation (01-08, focus 01-02, 07-08), then files 09-12, 26

---

## Troubleshooting

**Where do I start?**  
Read files 01-08 in order (Foundation Series)

**I want to learn a specific pattern**  
Jump to files 09-27 for advanced patterns

**What's the architecture?**  
Read file 02 (Project Overview)

**How do I build my first feature?**  
Follow file 08 (Step-by-Step Guide) with exact code

**Which DLL should I use?**  
See file 02 (Decision Matrix)

**Validation errors aren't translating**  
See file 10 (Localized Validation)

**How do multi-tenant systems work?**  
See files 17-19 (Multi-Tenant Architecture)

**Need to export data to Excel/PDF**  
See file 27 (PDF/Excel Export)

**Building a mobile app**  
See file 07 (SmartWorkz.Mobile)

---

## Version History

**v2.0 (2026-04-25)**  
Reorganized as Foundation Series (01-08) + Pattern Library (09-27); added XML docs from Core DLLs

**v1.0 (2024-Q4)**  
Initial 19-page wiki

---

## Next Steps

👉 **Start with [01-getting-started.md](./01-getting-started.md)** (10 min)

Then continue through files 02-08 in order for complete zero-to-hero learning.

After Foundation Series, explore files 09-27 for advanced features.
