# Getting Started — Core Setup

Minimum wiring required to use the **SmartWorkz Core** stack (DI, DB, translations, multi-tenancy, `BasePage`) inside a Razor Pages or MVC app.

If you just cloned the starter kit, everything below is already wired in `Public/Program.cs` and `Admin/Program.cs`. This guide is for dropping the Core into an app that does not yet have it.

## Purpose

- Add the full Application + Infrastructure stack with **one DI call**.
- Resolve the current **tenant** per request with **one pipeline call**.
- Give every page model `TenantId`, `T()`, and `CurrentUser*` helpers via `BasePage`.

## Prerequisites

- .NET 9 SDK
- SQL Server (local or remote) with the migrations from `database/v4/` applied
- Project references from your web host to:
  - `SmartWorkz.StarterKitMVC.Application`
  - `SmartWorkz.StarterKitMVC.Infrastructure`
  - `SmartWorkz.StarterKitMVC.Shared`
  - `SmartWorkz.StarterKitMVC.Domain`

## 1. Connection String

Add `DefaultConnection` to `appsettings.json`. This single connection is reused by all DbContexts (Master, Shared, Transaction, Report, Auth) and by Dapper repositories.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StarterKitMVC;Trusted_Connection=True;TrustServerCertificate=True",
    "Redis": ""
  }
}
```

Leave `Redis` empty to fall back to in-memory L2 cache. See [`ServiceCollectionExtensions.AddCacheServices`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/ServiceCollectionExtensions.cs).

JWT/auth settings also live under `Features:Authentication:Jwt` — copy that block from `Public/appsettings.json` if you need bearer auth.

## 2. Register Services (one call)

In `Program.cs`:

```csharp
using SmartWorkz.StarterKitMVC.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Core: DbContexts + Dapper connection + repositories + app services
// + memory/distributed cache + JWT bearer + translation warm-up.
builder.Services.AddApplicationStack(builder.Configuration);
```

`AddApplicationStack` is the only entry point you need. It composes:

| Call | Registers |
|------|-----------|
| `AddInfrastructureServices` | 5 DbContexts + `IDbConnection` (Dapper) |
| `AddRepositories` | Tenant, Product, Category, User, EmailQueue |
| `AddApplicationServices` | `IAuthService`, `ITranslationService`, `IMenuService`, `IPermissionService`, etc. |
| `AddCacheServices` | `IMemoryCache`, Redis **or** SQL distributed cache, `ICacheService` |
| `AddJwtAuthentication` | JWT bearer using `Features:Authentication:Jwt:*` |
| `TranslationCacheWarmupService` | Preloads translations on startup |

See [`ServiceCollectionExtensions.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/ServiceCollectionExtensions.cs) for the exact list.

### Swapping the auth scheme (cookie vs JWT)

`AddApplicationStack` wires **JWT bearer** by default (suitable for APIs). For a cookie-based UI, add the cookie scheme *after* the stack and set it as default — Public does exactly this in [`Public/Program.cs`](../../src/SmartWorkz.StarterKitMVC.Public/Program.cs):

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.Cookie.Name = ".MyApp.Auth";
});
```

## 3. Wire the Pipeline

Order matters. `UseTenantResolution` must run **after** `UseAuthentication` (so it can read the `tenant` claim) and **before** `UseAuthorization` (so policies can see it).

```csharp
var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseTenantResolution();   // ← resolves TenantId into HttpContext.Items
app.UseAuthorization();

app.MapRazorPages();
app.Run();
```

`UseTenantResolution` is an extension on `IApplicationBuilder` defined in `Public/Middleware/TenantMiddleware.cs` (mirror exists in Admin). Resolution order:

1. `tenant` / `TenantId` claim on the authenticated user
2. `X-Tenant-ID` request header
3. Subdomain (e.g. `acme.example.com` → `acme`)
4. Fallback: `DEFAULT`

See [`TenantMiddleware.cs`](../../src/SmartWorkz.StarterKitMVC.Public/Middleware/TenantMiddleware.cs) and [Wiki 06 — Multi-Tenant Login Flow](./06-multi-tenant-login-flow.md).

## 4. Use `BasePage` in Page Models

Every page model inherits from `BasePage` to get `TenantId`, `T()`, toasts, and current-user helpers:

```csharp
using SmartWorkz.StarterKitMVC.Public.Pages;
using SmartWorkz.StarterKitMVC.Shared.Constants;

public class IndexModel : BasePage
{
    public string Greeting { get; private set; } = "";

    public void OnGet()
    {
        Greeting = T(MessageKeys.General.Welcome);
        // TenantId, CurrentUserId, CurrentUserEmail also available.
    }
}
```

Two layers of `BasePage`:

- [`Shared.Pages.BasePage`](../../src/SmartWorkz.StarterKitMVC.Shared/Pages/_Base/BasePage.cs) — tenant + user helpers (no Application dependency).
- [`Public.Pages.BasePage`](../../src/SmartWorkz.StarterKitMVC.Public/Pages/_Base/BasePage.cs) / `Admin.Pages.BasePage` — adds `T()`, `ToastSuccess/Error/Info`, and localized `AddErrors(Result)`.

Inherit from the portal-specific one. Details in [Wiki 03 — Base Page Pattern](./03-base-page-pattern.md).

## 5. Verify

1. App starts without DI exceptions → `AddApplicationStack` resolved.
2. Hit any authorized page → `HttpContext.Items["TenantId"]` is set (check via `@Model.TenantId` or a debugger).
3. Call `T("General.Welcome")` in a page → returns the translated string, or the key itself if the DB has no row for that locale/tenant (safe fallback).
4. `/Demo/Translations` (Public) renders all `MessageKeys` with current values.

## Common Mistakes

- **Forgetting `UseTenantResolution`** → `BasePage.TenantId` always returns `"DEFAULT"` even for logged-in users.
- **Calling `UseTenantResolution` before `UseAuthentication`** → claim-based tenant resolution never fires; you fall through to header/subdomain.
- **Registering DbContexts manually again** → duplicate registrations. `AddApplicationStack` already does this.
- **Missing `DefaultConnection`** → `AddInfrastructureServices` throws at first Dapper request. The exception message tells you which key is missing.
- **Inheriting from `Microsoft.AspNetCore.Mvc.RazorPages.PageModel`** instead of `BasePage` → no `T()`, no `TenantId`, no toasts.

## See Also

- [01 — Translation System](./01-translation-system.md)
- [02 — Localized Validation](./02-localized-validation.md)
- [03 — Base Page Pattern](./03-base-page-pattern.md)
- [04 — Result Pattern](./04-result-pattern.md)
- [06 — Multi-Tenant Login Flow](./06-multi-tenant-login-flow.md)
- [MULTI-TENANT-ARCHITECTURE](./MULTI-TENANT-ARCHITECTURE.md)
