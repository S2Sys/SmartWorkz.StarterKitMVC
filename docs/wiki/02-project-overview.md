# Project Overview — Architecture & DLL Map

## The Three-Layer Stack

SmartWorkz divides into three interconnected layers, each with a specific responsibility:

```
┌──────────────────────────────────────────────────────────────────┐
│ PRESENTATION LAYER (Web Apps)                                    │
│ SmartWorkz.StarterKitMVC.Admin / .Public                         │
│ Razor Pages, Controllers, real-time signaling                    │
└───────────────────────────────────────┬──────────────────────────┘
                                        │
┌───────────────────────────────────────▼──────────────────────────┐
│ APPLICATION LAYER (Business Logic & Data)                        │
│ SmartWorkz.StarterKitMVC.Application (Services, CQRS handlers)   │
│ SmartWorkz.StarterKitMVC.Infrastructure (DB, Repositories)       │
│ SmartWorkz.StarterKitMVC.Shared (DTOs, Validators, Base Pages)   │
│ SmartWorkz.StarterKitMVC.Domain (Entities, Enums, Guards)        │
└───────────────────────────────────────┬──────────────────────────┘
                                        │
┌───────────────────────────────────────▼──────────────────────────┐
│ CORE LIBRARY LAYER (Cross-Cutting Concerns)                      │
│ SmartWorkz.Core — Domain patterns, entities, guards              │
│ SmartWorkz.Core.Web — Razor/Blazor components, GraphQL           │
│ SmartWorkz.Core.Shared — Caching, CQRS, Logging, Webhooks        │
│ SmartWorkz.Core.External — PDF, Excel, integrations              │
│ SmartWorkz.Core.Mobile — MAUI components, offline queue          │
└──────────────────────────────────────────────────────────────────┘
```

## DLL Reference Map

**SmartWorkz.StarterKitMVC.Domain** (App Layer)  
Domain entities, enums, invariants — consumed by Application, Infrastructure

**SmartWorkz.StarterKitMVC.Application** (App Layer)  
Business logic, CQRS handlers, services — consumed by Infrastructure, Admin/Public

**SmartWorkz.StarterKitMVC.Infrastructure** (App Layer)  
Database contexts, repositories, implementations — consumed by Admin/Public

**SmartWorkz.StarterKitMVC.Shared** (App Layer)  
DTOs, models, base page classes, validators — consumed by Admin/Public, all projects

**SmartWorkz.Core** (Core Layer)  
Entity base classes, guards, value objects, DDD interfaces — consumed by all StarterKit projects

**SmartWorkz.Core.Web** (Core Layer)  
Tag helpers, Blazor components, GraphQL setup — consumed by Public portal

**SmartWorkz.Core.Shared** (Core Layer)  
Caching, CQRS contracts, logging, webhooks — consumed by Application, Infrastructure

**SmartWorkz.Core.External** (Core Layer)  
Excel (ClosedXML), PDF (iText7) exporters — consumed by Infrastructure

**SmartWorkz.Core.Mobile** (Core Layer)  
MAUI components, platform services (iOS/Android) — for mobile apps

## Project Dependency Chain

This is the exact ProjectReference chain from top to bottom (each project references all its dependencies below):

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

**Key point:** No circular dependencies. Data always flows downward; infrastructure doesn't know about presentation.

## When to Use Which DLL

**Need a domain entity (User, Product)?**  
Use `SmartWorkz.Core` — Base class Entity, AuditEntity, TenantEntity, guard clauses

**Need a value object (Money, Email)?**  
Use `SmartWorkz.Core` — Pre-built Money, EmailAddress, Address with factories returning Result<T>

**Need to query the database?**  
Use `SmartWorkz.Core.Shared` + Application services — CQRS: IQuery<T> / IQueryHandler<T,TResult>

**Need caching?**  
Use `SmartWorkz.Core.Shared` — ICacheService (L1: memory, L2: Redis or distributed)

**Need structured logging?**  
Use `SmartWorkz.Core.Shared` — AddStructuredLogging() configures Serilog with console + rolling file

**Need to publish events?**  
Use `SmartWorkz.Core.Shared` — IWebhookPublisher for outbound webhook delivery with retry policy

**Need to render a Razor form with validation?**  
Use `SmartWorkz.Core.Web` — FormGroupTagHelper, validation extensions

**Need to build a dynamic data grid (Blazor)?**  
Use `SmartWorkz.Core.Web` — GridComponent with sorting, filtering, virtual scroll

**Need to export data to Excel?**  
Use `SmartWorkz.Core.External` — IExcelExporter with styling options

**Need to export data to PDF?**  
Use `SmartWorkz.Core.External` — IPdfExporter with layout options

**Building a mobile app?**  
Use `SmartWorkz.Core.Mobile` — MAUI custom components, offline queue, reconnect service

## Solution File Layout

The complete SmartWorkz.StarterKitMVC solution is organized as:

```
SmartWorkz.StarterKitMVC.sln                    [Main solution]

src/  (8 production projects)
├── SmartWorkz.StarterKitMVC.Admin/             [Web app: admin portal]
├── SmartWorkz.StarterKitMVC.Public/            [Web app: public portal]
├── SmartWorkz.StarterKitMVC.Application/       [Services, CQRS, mappers]
├── SmartWorkz.StarterKitMVC.Domain/            [Entities, enums, interfaces]
├── SmartWorkz.StarterKitMVC.Infrastructure/    [EF DbContexts, repos, background jobs]
├── SmartWorkz.StarterKitMVC.Shared/            [DTOs, models, validators, extensions]
├── SmartWorkz.Core.Web/                        [Tag helpers, Blazor components, GraphQL]
├── SmartWorkz.Core.Shared/                     [Caching, CQRS, Logging, Webhooks — split into 4 folders]
├── SmartWorkz.Core.External/                   [PDF, Excel exporters]
└── SmartWorkz.Core.Mobile/                     [MAUI components, platform services]

tests/ (5 test projects)
├── SmartWorkz.StarterKitMVC.Tests.Unit/
├── SmartWorkz.StarterKitMVC.Tests.Integration/
├── SmartWorkz.Core.Tests/
├── SmartWorkz.Core.Web.Tests/
└── SmartWorkz.Core.External.Tests/

docs/
├── wiki/                                        [This documentation]
├── superpowers/                                 [Capability reference]
└── (optional) your-project-docs/
```

**Solution contains 16 projects total** — 8 production + 5 test + 1 sample mobile.

## The Startup Flow — How Everything Wires

When Admin portal starts (`dotnet run`), here's what `Program.cs` does in order:

1. **Logging** — Configure Serilog (console + rolling file)
2. **Add Razor Pages** — Register page model conventions and auth requirements
3. **Add Application Stack** — ONE extension method that:
   - Creates all 5 DbContexts (Auth, Master, Shared, Transaction, Report)
   - Wires Dapper repositories for each entity
   - Registers migration manager
   - Registers PDF/Excel exporters
   - Registers all domain services (permissions, translations, tokens, etc.)
   - Wires ICacheService (memory + Redis)
   - Adds JWT Bearer authentication
   - Configures MassTransit messaging
4. **Add Cookie Auth** — Override JWT default for Razor Pages (session-based, not token-based)
5. **Add Authorization** — RBAC policies and custom requirement handlers
6. **Add Swagger/GraphQL** — (optional, config-gated)
7. **Build app** → **Run migrations** (IMigrationManager.MigrateAsync) → **Warm caches**
8. **Configure middleware** — Routing, auth, tenant resolution, permissions, authorization
9. **Map endpoints** — Razor Pages + Controllers

All this happens in **~200 lines of code** in `Program.cs` because the infrastructure is abstracted into extension methods.

## Key Architectural Decisions

### Multi-Tenancy
Every entity that deals with customer data has a `TenantId` property. The `UseTenantResolution()` middleware extracts the tenant from JWT claims, headers, or subdomain. All queries are implicitly filtered by tenant. See **[Why TenantId in Multiple Tables](#)** for the deep dive.

### Event-Driven Messaging
Events flow through MassTransit (configurable: in-memory, RabbitMQ, Azure Service Bus). Consumers are auto-registered and handle side-effects (send email, update analytics, etc.) without blocking the original request.

### Caching Tiers
L1 = In-memory cache (fast, process-local). L2 = Redis (slower but shared across processes). ICacheService automatically checks L1 first, falls back to L2.

### DDD & Aggregates
Entities inherit from a three-tier chain: `Entity` → `AuditEntity` → `DeletableEntity` → `TenantEntity`. Aggregates are loaded with their child entities in a single repository call. Invariants are enforced via Guard class.

### CQRS Lite
Reads use `IQuery<T>` / `IQueryHandler<T, TResult>` for testability. Writes happen via domain service methods (not yet explicit command handlers, but the pattern is ready for expansion).

---

**Next:** Deep-dive into each Core library starting with [SmartWorkz.Core — Entities, Guards, and Value Objects](03-smartworkz-core.md)
