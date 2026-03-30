# SmartWorkz.StarterKitMVC – High-Level Architecture

## Solution Layout
- **Root**
  - `SmartWorkz.StarterKitMVC.sln`
  - `src/`
    - `SmartWorkz.StarterKitMVC.Web` – ASP.NET Core MVC host (composition root, middleware, UI shells)
    - `SmartWorkz.StarterKitMVC.Application` – Application services, use-case orchestration, DTOs, contracts
    - `SmartWorkz.StarterKitMVC.Domain` – Domain models & core abstractions (LoV, Settings, Identity, Events)
    - `SmartWorkz.StarterKitMVC.Infrastructure` – Implementations (logging, settings, event bus, notifications, storage, HttpClient)
    - `SmartWorkz.StarterKitMVC.Shared` – Cross-cutting primitives, extension methods, base types
  - `build/` – DevOps / pipelines / infra-as-code (planned)
  - `docs/` – Documentation (this file, more to come)

## Dependency Rules
- `SmartWorkz.StarterKitMVC.Web`
  - Depends on: `Application`, `Infrastructure`, `Shared`
  - Responsibilities: startup, DI, middleware, MVC areas, views, HTTP endpoints.
- `SmartWorkz.StarterKitMVC.Application`
  - Depends on: `Domain`, `Shared`
  - Responsibilities: application services, cross-cutting contracts, DTOs, coordinators.
- `SmartWorkz.StarterKitMVC.Domain`
  - Depends on: `Shared` (optionally kept minimal)
  - Responsibilities: core models & interfaces for LoV, Settings, Identity, Events, multi-tenancy hooks.
- `SmartWorkz.StarterKitMVC.Infrastructure`
  - Depends on: `Domain`, `Shared`
  - Responsibilities: technical implementations (logging, telemetry, persistence abstractions, event bus, notifications, HttpClient pipeline, background jobs).
- `SmartWorkz.StarterKitMVC.Shared`
  - Depends on: none
  - Responsibilities: reusable helpers/extensions, base result & error types, common primitives.

## Cross-Cutting Concerns
Implemented across layers via interfaces in `Domain`/`Application` and implementations in `Infrastructure`:
- Logging, correlation ID, telemetry
- HttpClient pipeline and API error model
- Global Settings & LoV systems
- Identity shell (no domain-specific auth logic)
- Event bus & notification hub abstractions
- Plugin/module system & feature flags

This architecture intentionally avoids CRUD and domain-specific logic, focusing only on reusable enterprise-grade infrastructure.
