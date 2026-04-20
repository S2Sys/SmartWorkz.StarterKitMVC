# Documentation Index

Welcome to SmartWorkz documentation. Start here and navigate by your role and needs.

---

## 🚀 Quick Start (First Time Here?)

1. **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** — One-page cheat-sheet for all common tasks
2. **[DEVELOPER.md](DEVELOPER.md)** — Complete guide (sections 1-5 for beginners, 6+ for advanced)
3. **[TEMPLATES.md](TEMPLATES.md)** — Email/SMS template rendering

---

## 📚 Core Library Reference

| System | Doc | Purpose |
|--------|-----|---------|
| **Development** | [DEVELOPER.md](DEVELOPER.md) | All patterns & systems (beginner → advanced) |
| **Data Access** | [DATA_ACCESS.md](DATA_ACCESS.md) | ADO.NET, Dapper, databases, CSV/XML |
| **HTTP & Networking** | [HTTP_CLIENT.md](HTTP_CLIENT.md) | REST client, retry policies, WebSocket |
| **Security** | [SECURITY.md](SECURITY.md) | JWT, encryption, hashing, password policies |
| **Templates** | [TEMPLATES.md](TEMPLATES.md) | Email/SMS templates with placeholders |
| **Utilities** | [UTILITIES.md](UTILITIES.md) | Helper methods, LINQ extensions, string/date utils |
| **Services** | [SERVICES.md](SERVICES.md) | Email, SMS, logging, health checks, files |

---

## 🏗️ How-To Patterns (Step-by-Step Workflows)

Follow these guides for specific use cases:

1. **[wiki/01-translation-system.md](wiki/01-translation-system.md)** — Multi-language database translations
2. **[wiki/02-localized-validation.md](wiki/02-localized-validation.md)** — Localized validation error messages
3. **[wiki/03-base-page-pattern.md](wiki/03-base-page-pattern.md)** — BasePage inheritance pattern
4. **[wiki/04-result-pattern.md](wiki/04-result-pattern.md)** — Result<T> for safe error handling
5. **[wiki/05-htmx-list-pattern.md](wiki/05-htmx-list-pattern.md)** — Live filtering without page reloads
6. **[wiki/06-password-reset-flow.md](wiki/06-password-reset-flow.md)** — Secure password reset with tokens
7. **[wiki/07-pagination-factory-method.md](wiki/07-pagination-factory-method.md)** — Pagination with HTMX
8. **[wiki/08-simple-form-validation.md](wiki/08-simple-form-validation.md)** — Client + server validation
9. **[wiki/09-multi-tenant-login-flow.md](wiki/09-multi-tenant-login-flow.md)** — Login across multiple tenants
10. **[wiki/10-why-tenantid-in-multiple-tables.md](wiki/10-why-tenantid-in-multiple-tables.md)** — TenantId design rationale
11. **[wiki/11-multi-tenant-architecture.md](wiki/11-multi-tenant-architecture.md)** — Multi-tenant system design
12. **[wiki/12-cache-attribute.md](wiki/12-cache-attribute.md)** — `[Cache]` decorator for response caching
13. **[wiki/13-template-engine.md](wiki/13-template-engine.md)** — Using ITemplateEngine for templates

---

## 🔐 Authorization & Multi-Tenancy

- **[TENANT_AUTHORIZATION.md](TENANT_AUTHORIZATION.md)** — Three-level authorization (Super Admin, Tenant Admin, User)
- **[TENANT_AUTHORIZATION_QUICK_START.md](TENANT_AUTHORIZATION_QUICK_START.md)** — Setup & integration

---

## 🚢 Deployment & Integration

- **[DEPLOYMENT-MULTIVIEW.md](DEPLOYMENT-MULTIVIEW.md)** — Deploying multi-view Grid/List components

---

## 🆕 Phase 1 Critical Infrastructure

Recently added core features:

### 1. **DbProviderFactory Enum**
Type-safe database provider lookup
- **Reference:** [DATA_ACCESS.md#dbproviderfactory](DATA_ACCESS.md#dbproviderfactory--type-safe-enum-overload)
- **Quick Example:**
  ```csharp
  var provider = DbProviderFactory.GetProvider(DatabaseProvider.SqlServer);
  ```

### 2. **HttpStatusCode Enum**
Type-safe retry policies
- **Reference:** [HTTP_CLIENT.md](HTTP_CLIENT.md#http-status-codes-enum)
- **Quick Example:**
  ```csharp
  RetryableStatusCodes = [HttpStatusCode.TooManyRequests, HttpStatusCode.ServiceUnavailable]
  ```

### 3. **DbExtensions Aliases**
Simplified data access methods
- **Reference:** [DATA_ACCESS.md#simplified-aliases](DATA_ACCESS.md#simplified-data-access-with-dbextensions)
- **Quick Example:**
  ```csharp
  var users = await provider.QueryAsync<User>("SELECT * FROM Users");
  ```

### 4. **[Cache] Attribute**
Automatic response caching with one line
- **Reference:** [wiki/12-cache-attribute.md](wiki/12-cache-attribute.md)
- **Quick Example:**
  ```csharp
  [Cache(Seconds = 60)]
  [HttpGet("{id}")]
  public IActionResult GetProduct(int id) { ... }
  ```

### 5. **Template Engine (ITemplateEngine)**
File-based email/SMS templates with placeholders
- **Full API:** [TEMPLATES.md](TEMPLATES.md)
- **How-To:** [wiki/13-template-engine.md](wiki/13-template-engine.md)
- **Quick Example:**
  ```csharp
  var result = await _templateEngine.RenderFileAsync(
      "~/Templates/Emails/welcome.html",
      new { UserName = "Alice" }
  );
  ```

---

## 📖 Documentation by Role

### 🟢 I'm New to SmartWorkz
Start here (in order):
1. [QUICK_REFERENCE.md](QUICK_REFERENCE.md) - 2 min read
2. [DEVELOPER.md](DEVELOPER.md) sections 1-5 - Getting started examples
3. [DEVELOPER.md](DEVELOPER.md) section on your use case

### 🔵 I'm an Experienced .NET Developer
Jump to:
1. [DEVELOPER.md](DEVELOPER.md) sections 6+ - Architecture & advanced patterns
2. [DATA_ACCESS.md](DATA_ACCESS.md) - Database abstractions
3. [SECURITY.md](SECURITY.md) - JWT & encryption
4. [wiki/](wiki/) - Architectural patterns

### 🟣 I'm Building a Feature
1. Find your scenario in [wiki/](wiki/)
2. Reference the appropriate doc (DATA_ACCESS, HTTP_CLIENT, etc.)
3. Check DEVELOPER.md for the underlying pattern

### 🟠 I'm Integrating External Services
1. [HTTP_CLIENT.md](HTTP_CLIENT.md) - REST client & retry policies
2. [SERVICES.md](SERVICES.md) - Email, SMS, file I/O, logging
3. [SECURITY.md](SECURITY.md) - API keys, JWT, encryption

---

## 🔗 Quick Links

- **[GitHub](https://github.com/S2Sys/SmartWorkz.StarterKitMVC)** — Source code
- **[Issues](https://github.com/S2Sys/SmartWorkz.StarterKitMVC/issues)** — Bug reports & feature requests

---

## 📂 Complete File Structure

```
docs/
├── INDEX.md (you are here)
├── DEVELOPER.md (beginner + advanced guide)
├── QUICK_REFERENCE.md (cheat-sheet)
├── DATA_ACCESS.md
├── HTTP_CLIENT.md
├── SECURITY.md
├── SERVICES.md
├── TEMPLATES.md
├── UTILITIES.md
├── TENANT_AUTHORIZATION.md
├── TENANT_AUTHORIZATION_QUICK_START.md
├── DEPLOYMENT-MULTIVIEW.md
├── wiki/ (17 how-to patterns)
│   ├── 01-translation-system.md
│   ├── 02-localized-validation.md
│   ├── ... (01 through 13)
│   └── 13-template-engine.md
└── old/ (archived docs)
```

---

**Last Updated:** 2026-04-20
