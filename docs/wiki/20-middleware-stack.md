# Middleware Stack

The order of middleware in `Program.cs` is part of the framework's public contract. Get it wrong and you'll see the wrong `TenantId`, missing permission claims, or unhandled exceptions leaking stack traces to clients.

This page documents every middleware the framework ships, what it does, and where it must sit in the pipeline.

## The canonical order

Public site — see [`Public/Program.cs`](../../src/SmartWorkz.StarterKitMVC.Public/Program.cs):

```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseTenantResolution();          // ← tenant available on HttpContext.Items
app.UseAuthorization();

app.UseMiddleware<PermissionMiddleware>();   // ← permission claims ready

app.MapRazorPages();
```

Admin site — see [`Admin/Program.cs`](../../src/SmartWorkz.StarterKitMVC.Admin/Program.cs):

```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseFileLogging();               // admin-only request/response file log

app.UseRouting();
app.UseAuthentication();
app.UseTenantResolution();

app.UseAuthorizationValidation();   // validates + enriches claims (admin)
app.UseMiddleware<PermissionMiddleware>();   // NOTE: admin runs this BEFORE UseAuthorization
app.UseAuthorization();

app.MapRazorPages();
```

> **Admin vs Public discrepancy.** Admin runs `PermissionMiddleware` **before** `UseAuthorization`; Public runs it **after**. Both work for their respective scenarios but the framework should converge — see the "Common Mistakes" section. If you change either site, match the other or document the divergence explicitly.

## Middleware reference

### 1. `UseHttpsRedirection`, `UseStaticFiles`, `UseRouting`

Framework built-ins. Order: redirect to HTTPS → serve static → pick the endpoint. Always first.

### 2. `UseAuthentication`

Framework built-in. Populates `context.User` from whichever scheme is default:

- **Public / Admin** → cookie (`.Public.Auth` / `.Admin.Auth`) because each host overrides default via `AddAuthentication(...AddCookie)` after `AddApplicationStack`.
- **API / SPA / mobile / desktop consumers** → JWT bearer (the default wired by `AddJwtAuthentication` inside `AddApplicationStack`).

### 3. `UseTenantResolution` — `TenantMiddleware`

| | |
|---|---|
| Where | [`Public/Middleware/TenantMiddleware.cs`](../../src/SmartWorkz.StarterKitMVC.Public/Middleware/TenantMiddleware.cs) + mirror in Admin |
| Extension | `UseTenantResolution()` |
| Contract | Sets `HttpContext.Items["TenantId"]` to the resolved tenant |

Resolution order (first hit wins):

1. `tenant` or `TenantId` claim on the authenticated user — so must run **after** `UseAuthentication`.
2. `X-Tenant-ID` request header — API / integration scenarios.
3. Subdomain — `acme.example.com` → `"acme"` (ignores `www`).
4. Fallback — `"DEFAULT"`.

Must run **before** any code that reads `HttpContext.Items["TenantId"]` — authorization policies, `BasePage.TenantId`, repositories, etc.

### 4. `UseAuthorization`

Framework built-in. Runs policy handlers (including `PermissionAuthorizationHandler`). Needs the user populated (step 2) and tenant resolved (step 3).

### 5. `PermissionMiddleware`

| | |
|---|---|
| Where | [`Public/Middleware/PermissionMiddleware.cs`](../../src/SmartWorkz.StarterKitMVC.Public/Middleware/PermissionMiddleware.cs) — Admin mirror + Web copy |
| Extension | `UsePermissions()` (Public/Web) — Admin uses `app.UseMiddleware<PermissionMiddleware>()` directly |
| Contract | Adds `permission` claims to the `ClaimsIdentity` based on the user's roles |

Looks up `IPermissionService.GetPermissionKeysForRolesAsync(user roles)` and adds each key as a `permission` claim **if not already present**. This keeps the identity fresh between refreshes — a newly granted permission takes effect on the next request, without forcing a re-login for cookie sessions.

For JWT bearers the permissions come from the token claim set; the middleware is still safe to run (it's idempotent).

See [15 — Permission Service](./15-permission-service.md) for the RBAC surface.

### 6. `UseAuthorizationValidation` — `AuthorizationMiddleware` (Admin only)

| | |
|---|---|
| Where | [`Admin/Middleware/AuthorizationMiddleware.cs`](../../src/SmartWorkz.StarterKitMVC.Admin/Middleware/AuthorizationMiddleware.cs) |
| Extension | `UseAuthorizationValidation()` |
| Contract | Validates + enriches roles, claims, and permissions for the authenticated user (uses `IClaimService` + `IMemoryCache`) |

This is the Admin-flavour expansion of `PermissionMiddleware` — it additionally hydrates custom claim types (department, region, etc.) from `IClaimService` with a short-lived `IMemoryCache` layer to avoid per-request DB hits.

Cross-reference: [16 — Claim Service](./16-claim-service.md).

### 7. `UseFileLogging` — `FileLoggingMiddleware` (Admin only)

| | |
|---|---|
| Where | [`Admin/Middleware/FileLoggingMiddleware.cs`](../../src/SmartWorkz.StarterKitMVC.Admin/Middleware/FileLoggingMiddleware.cs) |
| Extension | `UseFileLogging()` |
| Contract | Writes every request + response to `~/Logs/admin-{yyyy-MM-dd}.log` |

Useful for local-dev diagnostics without tailing console output. Registered **after** static-files to avoid noise from CSS / JS requests. Disable or gate on `app.Environment.IsDevelopment()` in production.

### 8. `CorrelationIdMiddleware` (Web-only, opt-in)

| | |
|---|---|
| Where | [`Web/Middleware/CorrelationIdMiddleware.cs`](../../src/SmartWorkz.StarterKitMVC.Web/Middleware/CorrelationIdMiddleware.cs) |
| Contract | Reads `X-Correlation-ID` header (or generates one), stores in `ICorrelationContext`, echoes on the response |

Add at the **top** of the pipeline so downstream logging / exceptions can include the id:

```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
// …
```

Cross-reference: [22 — Shared Primitives](./22-shared-primitives.md) for `ICorrelationContext`.

### 9. `GlobalExceptionHandlingMiddleware` (Web-only, opt-in)

| | |
|---|---|
| Where | [`Web/Middleware/GlobalExceptionHandlingMiddleware.cs`](../../src/SmartWorkz.StarterKitMVC.Web/Middleware/GlobalExceptionHandlingMiddleware.cs) |
| Contract | Converts unhandled exceptions into RFC 7807 `application/problem+json` responses |

Maps:

| Exception | HTTP | Response |
|-----------|------|----------|
| `ValidationException` | 400 | `ValidationProblemDetails` with `errors` dictionary |
| `UnauthorizedAccessException` | 401 | `Unauthorized` problem |
| `ArgumentNullException` / `ArgumentException` | 400 | `ValidationError` with parameter name |
| `InvalidOperationException` | 409 | `Conflict` |
| anything else | 500 | `InternalServerError` (message sanitised) |

Add **first** so it wraps everything else:

```csharp
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseRouting();
// …
```

SPA / mobile / desktop clients should parse `application/problem+json` responses — that's the public contract for errors from the API.

## Recommended pipeline for a new API host

If you spin up a new REST host for SPA / mobile / desktop, use:

```csharp
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();             // JWT bearer (default from AddApplicationStack)
app.UseTenantResolution();
app.UseAuthorization();

app.UseMiddleware<PermissionMiddleware>();

app.MapControllers();
```

## Cross-Client Notes

The pipeline affects every client:

| Client | Why order matters |
|--------|-------------------|
| **Razor Pages / MVC** | `BasePage.TenantId` reads `HttpContext.Items["TenantId"]` — set by `UseTenantResolution` |
| **Angular / React** | Rely on `application/problem+json` from `GlobalExceptionHandlingMiddleware` + `X-Correlation-ID` round-tripped by `CorrelationIdMiddleware` |
| **.NET MAUI** | Same; also uses `X-Tenant-ID` header path in `TenantMiddleware` |
| **WPF / WinUI** | Same as MAUI |

A pipeline reordering is a **breaking change** — document it here and in the PR body.

## Common Mistakes

- **Calling `UseTenantResolution` before `UseAuthentication`** → tenant claim lookup always misses; you fall back to header/subdomain/`DEFAULT`.
- **Calling `UseAuthorization` before `UseAuthentication`** → `[Authorize]` never has a user to check.
- **Dropping `PermissionMiddleware`** → permissions granted in the admin UI never show up in the identity; `[RequirePermission]` always fails.
- **Registering `PermissionMiddleware` twice** in a single host (once via `UsePermissions()` and once via `UseMiddleware<PermissionMiddleware>`) → duplicate permission claims. Pick one.
- **Different order for Admin vs Public** (current state) → works for now but fragile; converge when you can.
- **Missing `UseStatusCodePagesWithReExecute`** in web hosts → 404s fall through to a bare response. Public / Admin both include it for consistency with `/Error`.
- **Leaving `UseFileLogging()` on in production** → writes every request to disk. Gate on `IsDevelopment()`.
- **Forgetting HTTPS + HSTS in production** → cookies marked `Secure` won't set. Both hosts call `UseHsts()` behind an `!IsDevelopment()` check.

## See Also

- [00 — Getting Started](./00-getting-started.md) — minimal pipeline for a new host
- [06 — Multi-Tenant Login Flow](./06-multi-tenant-login-flow.md) — tenant resolution in context
- [14 — Auth Service](./14-auth-service.md) — cookie vs JWT scheme selection
- [15 — Permission Service](./15-permission-service.md) — what `PermissionMiddleware` injects
- [16 — Claim Service](./16-claim-service.md) — what `AuthorizationMiddleware` (Admin) enriches
- [22 — Shared Primitives](./22-shared-primitives.md) — `ICorrelationContext`, `ProblemDetailsResponse`
