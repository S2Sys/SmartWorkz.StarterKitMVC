# Project Overview — Architecture & DLL Map

## Three-Layer Architecture

SmartWorkz divides into three layers:

**PRESENTATION** (Web Apps: Admin, Public)  
Razor Pages, Controllers, forms, real-time features

**APPLICATION** (Services, Data, Business Logic)  
Domain entities, repositories, application services, CQRS handlers

**CORE** (Cross-Cutting Concerns)  
Reusable components, validation, caching, logging, webhooks, mobile components

## DLL Reference Map

**SmartWorkz.StarterKitMVC.Domain** (Application Layer)  
Domain entities, enums, invariants — used by Application, Infrastructure

**SmartWorkz.StarterKitMVC.Application** (Application Layer)  
Business logic, CQRS handlers, services — used by Infrastructure, Admin/Public

**SmartWorkz.StarterKitMVC.Infrastructure** (Application Layer)  
Database contexts, repositories, implementations — used by Admin/Public

**SmartWorkz.StarterKitMVC.Shared** (Application Layer)  
DTOs, models, base pages, validators — used by Admin/Public, all projects

**SmartWorkz.Core** (Core Layer)  
Entity base classes, guards, value objects, DDD interfaces — used by all projects

**SmartWorkz.Core.Web** (Core Layer)  
Tag helpers, Blazor components, GraphQL setup — used by Public portal

**SmartWorkz.Core.Shared** (Core Layer)  
Caching, CQRS contracts, logging, webhooks — used by Application, Infrastructure

**SmartWorkz.Core.External** (Core Layer)  
Excel (ClosedXML), PDF (iText7) exporters — used by Infrastructure

**SmartWorkz.Core.MAUI** (Core Layer)  
MAUI components, platform services (iOS/Android) — for mobile apps

## When to Use Which DLL

**Need a domain entity?**  
Use `SmartWorkz.Core` — Entity<TId>, AuditEntity, TenantEntity, DDD patterns

**Need a value object?**  
Use `SmartWorkz.Core` — Money, EmailAddress, Address with Result<T> factories

**Need to query the database?**  
Use `SmartWorkz.Core.Shared` — IQuery<T> / IQueryHandler<TResult> CQRS pattern

**Need caching?**  
Use `SmartWorkz.Core.Shared` — IQueryCacheService (L1 memory, L2 distributed)

**Need structured logging?**  
Use `SmartWorkz.Core.Shared` — Serilog configuration with console + rolling file

**Need to publish events?**  
Use `SmartWorkz.Core.Shared` — IWebhookPublisher with HMAC-SHA256 signing

**Need a Razor form with validation?**  
Use `SmartWorkz.Core.Web` — FormGroupTagHelper, validation extensions

**Need a data grid?**  
Use `SmartWorkz.Core.Web` — GridComponent (sorting, filtering, virtual scroll)

**Need to export to Excel?**  
Use `SmartWorkz.Core.External` — IExcelExporter with styling

**Need to export to PDF?**  
Use `SmartWorkz.Core.External` — IPdfExporter with layout options

**Building a mobile app?**  
Use `SmartWorkz.Core.MAUI` — MAUI components, offline queue, auto-reconnect

## Project Dependency Chain

```
Admin Portal / Public Portal
    ├─ SmartWorkz.StarterKitMVC.Application
    │   ├─ SmartWorkz.StarterKitMVC.Infrastructure
    │   │   ├─ SmartWorkz.StarterKitMVC.Domain
    │   │   └─ SmartWorkz.Core.Shared
    │   └─ SmartWorkz.StarterKitMVC.Domain
    │
    ├─ SmartWorkz.StarterKitMVC.Infrastructure
    │   ├─ SmartWorkz.StarterKitMVC.Domain
    │   └─ SmartWorkz.Core.Shared
    │
    ├─ SmartWorkz.StarterKitMVC.Shared
    │   └─ SmartWorkz.StarterKitMVC.Domain
    │
    └─ SmartWorkz.Core.Web (Public only)
```

Key point: No circular dependencies. All references flow downward.

## Solution File Layout

```
src/
├── SmartWorkz.StarterKitMVC.Admin/          [Web app]
├── SmartWorkz.StarterKitMVC.Public/         [Web app]
├── SmartWorkz.StarterKitMVC.Application/    [Services, CQRS]
├── SmartWorkz.StarterKitMVC.Domain/         [Entities]
├── SmartWorkz.StarterKitMVC.Infrastructure/ [DB, repos]
├── SmartWorkz.StarterKitMVC.Shared/         [DTOs, validators]
├── SmartWorkz.Core/                         [Domain foundation]
├── SmartWorkz.Core.Web/                     [Web components]
├── SmartWorkz.Core.Shared/                  [Caching, CQRS, logging, webhooks]
├── SmartWorkz.Core.External/                [PDF, Excel]
└── SmartWorkz.Core.MAUI/                  [MAUI components]

tests/
├── SmartWorkz.StarterKitMVC.Tests.Unit/
├── SmartWorkz.StarterKitMVC.Tests.Integration/
├── SmartWorkz.Core.Tests/
├── SmartWorkz.Core.Web.Tests/
└── SmartWorkz.Core.External.Tests/

docs/
├── wiki/                                    [This documentation]
└── superpowers/                             [Capability reference]
```

---

**Next:** [03-smartworkz-core.md](./03-smartworkz-core.md)
