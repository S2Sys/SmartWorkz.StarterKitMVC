# Master Guide — SmartWorkz Starter Kit Wiki

Welcome to the SmartWorkz Starter Kit documentation hub. **Start with the SRS Foundation (01-08)** to learn core concepts, then explore the Pattern Library (09-27) for advanced features and architectural patterns.

---

## 🎯 START HERE: SRS Foundation (Zero-to-Hero Series)

New to SmartWorkz? Begin with **files 01-08** in order. This is a complete learning path from setup to building your first feature.

| # | File | Title | Time | What You'll Learn |
|---|------|-------|------|-------------------|
| 01 | [Getting Started](./01-getting-started.md) | Installation & quick-start | 10 min | Clone, run, verify, FAQ |
| 02 | [Project Overview](./02-project-overview.md) | Architecture & DLL map | 15 min | Three-tier architecture, DLL dependencies, when to use what |
| 03 | [SmartWorkz.Core](./03-smartworkz-core.md) | Domain foundation (XML docs) | 25 min | Entity hierarchy, Value Objects, Guard class, EntityState |
| 04 | [SmartWorkz.Core.Web](./04-smartworkz-core-web.md) | Web components (XML docs) | 20 min | Tag Helpers, Blazor Grid, Validation, GraphQL setup |
| 05 | [SmartWorkz.Core.Shared](./05-smartworkz-core-shared.md) | Caching, CQRS, Logging, Webhooks (XML docs) | 25 min | In-memory cache, lightweight CQRS, Serilog config, webhook security |
| 06 | [SmartWorkz.Core.External](./06-smartworkz-core-external.md) | Excel & PDF export (XML docs) | 15 min | IExcelExporter, IPdfExporter, Result<T> pattern |
| 07 | [SmartWorkz.Mobile](./07-smartworkz-mobile.md) | MAUI components & services (XML docs) | 20 min | Six components, offline queue, auto-reconnect, deduplication, VoIP push |
| 08 | [Step-by-Step Guide](./08-step-by-step-guide.md) | Build your first feature | 45 min | 9 concrete steps with exact code: new project → first page → list → export → CQRS |

**Total Time: ~2.5 hours**  
**Outcome: You can build a complete CRUD feature using SmartWorkz patterns.**

---

## Pattern Library (Advanced Features & Reference)

After completing the SRS Foundation, explore these 19 pattern-focused pages for deeper dives into specific areas.

### Web Application Patterns (09-16, 20-22, 25, 27)

**09** — [Translation System](./09-translation-system.md)  
Database-backed multi-language support (Core.Shared)

**10** — [Localized Validation](./10-localized-validation.md)  
Translate validation messages at render time (Core.Web)

**11** — [Base Page Pattern](./11-base-page-pattern.md)  
Common foundation for all Razor pages (Core.Web)

**12** — [Result Pattern](./12-result-pattern.md)  
Structured success/failure outcomes (Core)

**13** — [HTMX List Pattern](./13-htmx-list-pattern.md)  
Dynamic list updates without page reload (Core.Web)

**14** — [Password Reset Flow](./14-password-reset-flow.md)  
Secure password reset via email tokens (Infrastructure)

**15** — [Pagination Factory Method](./15-pagination-factory-method.md)  
Convert API responses to UI view models (Core.Web)

**16** — [Simple Form Validation](./16-simple-form-validation.md)  
Client + server validation with translation (Core.Web)

**20** — [Cache Attribute Pattern](./20-cache-attribute.md)  
Decorator-based HTTP result caching (Core.Shared)

**21** — [Template Engine Pattern](./21-template-engine.md)  
Dynamic template rendering with placeholders (Core.Shared)

**22** — [Swagger/OpenAPI Documentation](./22-swagger-openapi.md)  
Auto-generated API documentation (Core.Web)

**25** — [Blazor Form Builder](./25-blazor-form-builder.md)  
Dynamic form generation and validation (Core.Web)

**27** — [PDF/Excel Export](./27-pdf-excel-export.md)  
Binary file export with styling (Core.External)

### Infrastructure & Multi-Tenancy (17-19, 23-24)

**17** — [Multi-Tenant Login Flow](./17-multi-tenant-login-flow.md)  
How tenants and users interact during login (Core)

**18** — [Why TenantId in Multiple Tables](./18-why-tenantid-in-multiple-tables.md)  
Tenant isolation design rationale (Core)

**19** — [Multi-Tenant Architecture](./19-multi-tenant-architecture.md)  
Visual guide to tenant isolation (Core)

**23** — [EF Core Multi-Context Migrations](./23-ef-core-migrations.md)  
Orchestrate 5 independent DbContext schemas (Infrastructure)

**24** — [MassTransit Message Queue](./24-masstransit-message-queue.md)  
Asynchronous event-driven messaging (Infrastructure)

### Mobile & Cross-Platform (26)

**26** — [Mobile XAML Components](./26-mobile-xaml-components.md)  
.NET MAUI reusable UI components (Core.Mobile)

---

## System Architecture — Three-Tier Overview

```
┌──────────────────────────────────────────────────────────────────────┐
│                   SMARTWORKZ.CORE (DLL Foundation)                   │
│  SmartWorkz.Core → SmartWorkz.Core.Web → SmartWorkz.Core.Shared    │
│  SmartWorkz.Core.External → SmartWorkz.Core.Mobile (MAUI)           │
└──────────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────────┐
│         PUBLIC WEB APP TIER (Razor Pages + HTMX)                     │
│  Pages: Translation, Validation, Base Page, Lists, Export            │
│  Services: Caching, CQRS, Logging, Webhooks                         │
└──────────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────────┐
│      INFRASTRUCTURE TIER (Auth, DB, Messaging)                       │
│  Multi-Tenant Login, EF Core Migrations, MassTransit Queue          │
└──────────────────────────────────────────────────────────────────────┘
                              ↓
┌──────────────────────────────────────────────────────────────────────┐
│         MOBILE TIER (.NET MAUI)                                       │
│  XAML Components, Offline Queue, Auto-Reconnect, VoIP Push          │
└──────────────────────────────────────────────────────────────────────┘
```

---

## File Organization

All 27 wiki pages live in `docs/wiki/`:

```
docs/wiki/
├── 00-master-guide.md                          ← You are here

SRS FOUNDATION (01-08) — Start Here!
├── 01-getting-started.md                       ← Begin here
├── 02-project-overview.md
├── 03-smartworkz-core.md
├── 04-smartworkz-core-web.md
├── 05-smartworkz-core-shared.md
├── 06-smartworkz-core-external.md
├── 07-smartworkz-mobile.md
├── 08-step-by-step-guide.md

PATTERN LIBRARY (09-27) — Reference Material
├── 09-translation-system.md
├── 10-localized-validation.md
├── 11-base-page-pattern.md
├── 12-result-pattern.md
├── 13-htmx-list-pattern.md
├── 14-password-reset-flow.md
├── 15-pagination-factory-method.md
├── 16-simple-form-validation.md
├── 17-multi-tenant-login-flow.md
├── 18-why-tenantid-in-multiple-tables.md
├── 19-multi-tenant-architecture.md
├── 20-cache-attribute.md
├── 21-template-engine.md
├── 22-swagger-openapi.md
├── 23-ef-core-migrations.md
├── 24-masstransit-message-queue.md
├── 25-blazor-form-builder.md
├── 26-mobile-xaml-components.md
└── 27-pdf-excel-export.md
```

---

## Quick Navigation by Role

### Web Developer (Razor Pages)

**Foundation:** Files 01-08 (2.5 hours)  
**Patterns:** 09-12, 13, 15-16, 20-22, 25, 27

### Infrastructure Engineer (Database, Auth, Messaging)

**Foundation:** Files 01-08 (focus on 02-05)  
**Patterns:** 12, 14, 17-19, 23-24

### Mobile Developer (.NET MAUI)

**Foundation:** Files 01-08 (focus on 01-02, 07-08)  
**Patterns:** 09-12, 26

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "Where do I start?" | Read files 01-08 in order (SRS Foundation) |
| "I want to learn <pattern>" | Jump to files 09-27 for specific patterns |
| "What's the architecture?" | Read file 02 (Project Overview) |
| "How do I build a feature?" | Follow file 08 (Step-by-Step Guide) with exact code |
| "Which DLL should I use?" | See file 02 (Decision Matrix) |
| "Validation errors aren't translating" | See file 10 (Localized Validation) |
| "How do multi-tenant systems work?" | See files 17-19 (Multi-Tenant Architecture) |
| "Need to export data" | See file 27 (PDF/Excel Export) |
| "Building a mobile app" | See file 07 (SmartWorkz.Mobile) |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.0 | 2026-04-25 | Reorganized as SRS Foundation (01-08) + Pattern Library (09-27); added XML docs from Core DLLs |
| 1.0 | 2024-Q4 | Initial 19-page wiki |

---

## Contributing

When adding wiki pages:
1. **SRS Foundation (01-08):** Zero-to-hero learning path — keep pedagogical
2. **Pattern Library (09-27):** Reference material — detailed and comprehensive
3. **Links:** Use relative paths: `[Title](./NN-filename.md)`
4. **Code:** Always show complete, working examples with error handling
5. **Cross-references:** List related files at the end of each page

---

## Next Steps

👉 **Start with [01-getting-started.md](./01-getting-started.md)** (10 minutes)

Then continue through files 02-08 in order for a complete zero-to-hero experience.

After SRS Foundation, explore files 09-27 for advanced patterns and features.
