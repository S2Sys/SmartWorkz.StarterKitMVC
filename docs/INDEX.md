# SmartWorkz Documentation Index

Welcome to the SmartWorkz.StarterKitMVC documentation. This index organizes all docs by audience and purpose.

---

## 🚀 Getting Started (New Developers)

Start here if you're new to SmartWorkz:

1. **[SmartWorkz Junior Developer Guide](SMARTWORKZ_JUNIOR_DEV_GUIDE.md)** — Copy-paste oriented intro covering common tasks, patterns, and examples
2. **[SmartWorkz Quick Reference](SMARTWORKZ_QUICK_REFERENCE.md)** — One-page cheat-sheet for common operations (Result pattern, caching, HTTP, data access)
3. **[SmartWorkz Core Developer Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md)** — Complete reference guide covering all core systems

---

## 📚 Core Library API Reference

Detailed guides to each SmartWorkz.Core subsystem:

### Data Access & Queries
- **[Data Access Guide](DATA_ACCESS_GUIDE.md)** — ADO.NET, Dapper, DbProviderFactory, database abstraction, CSV/XML

### HTTP & Networking
- **[HTTP Client Guide](HTTP_CLIENT_GUIDE.md)** — IHttpClient fluent API, retry policies, status code handling, WebSocket client

### Security & Encryption
- **[Security Guide](SECURITY_GUIDE.md)** — JWT, AES-256, hashing, HMAC, password policies, input sanitization

### Domain-Driven Design & Domain Logic
- **[Domain Abstractions Guide](DOMAIN_ABSTRACTIONS_GUIDE.md)** — Base classes (AggregateRoot, Entity, ValueObject), domain events, AppConstants
- **[Value Objects Guide](VALUE_OBJECTS_GUIDE.md)** — Pre-built value objects with validation

### Object Mapping & Validation
- **[Mapping & Guards Guide](MAPPING_GUARDS_GUIDE.md)** — IMapper profiles, Guard clauses, ValidatorBase rules

### Utilities & Extensions
- **[Utilities & Extensions Guide](UTILITIES_EXTENSIONS_GUIDE.md)** — Helper methods, LINQ extensions, string/datetime utilities, **ITemplateEngine**
- **[Template Engine Guide](TEMPLATE_ENGINE_GUIDE.md)** — Full reference for ITemplateEngine, placeholder syntax, file/directory rendering

### Configuration & Diagnostics
- **[Configuration & Diagnostics Guide](CONFIGURATION_DIAGNOSTICS_GUIDE.md)** — IConfigurationHelper, DiagnosticsHelper, MetricsHelper, correlation context

### Services & Background Processing
- **[SmartWorkz Services Complete](SMARTWORKZ_SERVICES_COMPLETE.md)** — Email, SMS, Database, File I/O, Logging, Notifications services

### Web Components & UI
- **[Grid Component Wiki](GRID_COMPONENT_WIKI.md)** — Full Grid reference for tabular data display
- **[Grid Component Usage](GRID_COMPONENT_USAGE.md)** — Quick-start examples for Grid component
- **[TagHelpers Guide](TAGHELPERS_GUIDE.md)** — 15+ custom HTML TagHelpers for forms, validation, UI components

---

## 🏗️ How-To Patterns (Application Workflows)

Architectural patterns and best practices for building features:

1. **[01-translation-system.md](wiki/01-translation-system.md)** — Database-backed translations with per-tenant overrides
2. **[02-localized-validation.md](wiki/02-localized-validation.md)** — Localized validation messages using DataAnnotations
3. **[03-base-page-pattern.md](wiki/03-base-page-pattern.md)** — BasePage class and inheritance pattern for Razor Pages
4. **[04-result-pattern.md](wiki/04-result-pattern.md)** — Result<T> monadic pattern for outcomes (success/failure without exceptions)
5. **[05-htmx-list-pattern.md](wiki/05-htmx-list-pattern.md)** — Live list filtering with HTMX (no full-page reloads)
6. **[06-password-reset-flow.md](wiki/06-password-reset-flow.md)** — Secure password reset flow with token email
7. **[07-pagination-factory-method.md](wiki/07-pagination-factory-method.md)** — PaginationResponse factory for Razor Pages + HTMX
8. **[08-simple-form-validation.md](wiki/08-simple-form-validation.md)** — Client + server form validation with localization
9. **[09-multi-tenant-login-flow.md](wiki/09-multi-tenant-login-flow.md)** — Login when same email exists across multiple tenants
10. **[10-why-tenantid-in-multiple-tables.md](wiki/10-why-tenantid-in-multiple-tables.md)** — Architectural rationale for TenantId column patterns
11. **[11-multi-tenant-architecture.md](wiki/11-multi-tenant-architecture.md)** — Multi-tenant design with diagrams and entity relationships
12. **[12-cache-attribute.md](wiki/12-cache-attribute.md)** — Using [Cache] decorator for automatic response caching
13. **[13-template-engine.md](wiki/13-template-engine.md)** — Using ITemplateEngine for email/SMS templates

---

## 🔐 Tenant Authorization & Multi-Tenancy

- **[Tenant Authorization Complete Guide](TENANT_AUTHORIZATION.md)** — Three-level authorization (Super Admin, Tenant Admin, Tenant User)
- **[Tenant Authorization Quick-Start](TENANT_AUTHORIZATION_QUICK_START.md)** — Setup and integration steps

---

## 🚢 Deployment & Integration

- **[Multi-View Data Components Deployment](DEPLOYMENT-MULTIVIEW.md)** — Integrating multi-view Grid/List components

---

## 📋 Phase 1 Critical Infrastructure (New Features)

Recently added to SmartWorkz.Core.Shared:

### 1. DB Provider Enum Overload
Type-safe database provider selection via `GetProvider(DatabaseProvider)` instead of string literals.
**See:** [Data Access Guide — DB Provider Enum](DATA_ACCESS_GUIDE.md#db-provider-enum-overload)

### 2. HTTP Status Codes Enum
Type-safe retry policies using `List<HttpStatusCode>` instead of `List<int>`.
**See:** [HTTP Client Guide — Status Code Enum](HTTP_CLIENT_GUIDE.md#http-status-codes-enum)

### 3. Simplified Data Access Aliases
Short-form data access methods: `QueryAsync()`, `ScalarAsync()`, `NonQueryAsync()` instead of verbose `ExecuteQueryAsync()`.
**See:** [Data Access Guide — DbExtensions](DATA_ACCESS_GUIDE.md#simplified-data-access-aliases)

### 4. Cache Attribute
MVC action decorator `[Cache(Seconds=60)]` for automatic response caching.
**See:** [Template Engine Guide](TEMPLATE_ENGINE_GUIDE.md) and [Wiki: Cache Attribute Pattern](wiki/12-cache-attribute.md)

### 5. Template Engine
File-based templates with `{Name}` and `{{Key}}` placeholder support for email/SMS.
**See:** [Template Engine Guide](TEMPLATE_ENGINE_GUIDE.md) and [Wiki: Template Engine Pattern](wiki/13-template-engine.md)

---

## 📁 Archive & Reference

- **docs/old/** — Historical documentation (versions 1–3, archived)
- **docs/srs/** — System requirements and business specifications
- **docs/superpowers/** — AI-generated implementation plans and design specs (2026-04-XX)

---

## 🔗 Quick Links

- **GitHub:** [S2Sys/SmartWorkz.StarterKitMVC](https://github.com/S2Sys/SmartWorkz.StarterKitMVC)
- **Main Branch:** [main](https://github.com/S2Sys/SmartWorkz.StarterKitMVC/tree/main)
- **Issues & Discussions:** [GitHub Issues](https://github.com/S2Sys/SmartWorkz.StarterKitMVC/issues)

---

**Last Updated:** 2026-04-20
