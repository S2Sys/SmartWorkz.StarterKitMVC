# Claim Service

Custom claim-type registry plus role- and user-level claim management. Claims in this service are the layer **above** permissions — arbitrary key/value pairs (department, region, feature flag, tier, etc.) that you want to project into the `ClaimsPrincipal` and use from policies, UI, or downstream APIs.

## Purpose

- **Define claim types once** (e.g. `department`, `region`, `tier`) with optional predefined values — the admin UI renders a dropdown automatically.
- **Assign claims to roles and users** — user-level claims override role-level claims.
- **Replace per-type atomically** — `SetRoleClaimsAsync("admin", "region", ["na", "emea"])` replaces just that type, leaves the rest alone.
- **Project into the JWT / cookie identity** alongside roles and permissions.
- **Categorise** — Category + SortOrder + Icon so the admin page can group claim types visually.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `IClaimService` | Service contract | [`Application/Authorization/IClaimService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Authorization/IClaimService.cs) |
| `ClaimService` | EF + SP implementation | [`Infrastructure/Authorization/ClaimService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Authorization/ClaimService.cs) |
| `ClaimType` | Definition of a claim type (key, name, category, predefined values) | [`Domain/Authorization/ClaimDefinition.cs`](../../src/SmartWorkz.StarterKitMVC.Domain/Authorization/ClaimDefinition.cs) |
| `ClaimValue` | A predefined value under a claim type | same file |
| `RoleClaim` | `{RoleId, ClaimType, ClaimValue, IsGranted, Condition}` | same file |
| `UserClaim` | `{UserId, ClaimType, ClaimValue, IsGranted}` — overrides role claims | same file |

### Claims vs Permissions

| Permissions | Claims |
|-------------|--------|
| Binary grant of an action (`products.edit`) | Key/value pair with arbitrary semantics (`region=emea`) |
| Single namespace (`{entity}.{action}`) | Namespaced by `ClaimType.Key` (many namespaces) |
| Surfaced as `permission` claim on the identity | Surfaced as `{ClaimType.Key}` claim on the identity |
| Enforced by `[RequirePermission]` + `PermissionAuthorizationHandler` | Enforced by standard ASP.NET policy requirements (`p.RequireClaim("region", "emea")`) |

Use permissions for **what the user can do**, claims for **who the user is** (their department, region, tier, feature-flag set).

## DI Registration

Wired by `AddApplicationServices`:

```csharp
services.AddScoped<IClaimService, ClaimService>();
```

Consumed from the same places that read other identity data — `PermissionMiddleware` can be extended to project additional claim types (or add a dedicated `ClaimMiddleware` when you start using them heavily).

## Quick Start

### Seed a claim type with predefined values

```csharp
// 1. Register the type
var region = await _claims.CreateClaimTypeAsync(new ClaimType
{
    Key = "region",
    Name = "Region",
    Category = "Geography",
    Icon = "bi-globe",
    AllowMultiple = true,
    IsSystem = false
});

// 2. Register allowed values
foreach (var value in new[]
{
    new ClaimValue { Value = "na",   Label = "North America" },
    new ClaimValue { Value = "emea", Label = "Europe, Middle East, Africa" },
    new ClaimValue { Value = "apac", Label = "Asia-Pacific" }
})
    await _claims.AddClaimValueAsync(region.Id, value);
```

### Assign to a role

```csharp
// Admin UI "save" — replace entire region set for the tenant-admin role
await _claims.SetRoleClaimsAsync(
    roleId: "tenant-admin",
    claimType: "region",
    claimValues: new List<string> { "na", "emea" });
```

### Override on a single user

```csharp
await _claims.AddUserClaimAsync(userId, claimType: "region", claimValue: "apac");
```

User claims take precedence — the merging logic should prefer `UserClaim` rows over `RoleClaim` rows when building the principal.

### Use in a policy

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("region.emea", p => p.RequireClaim("region", "emea"));
});

[Authorize(Policy = "region.emea")]
public class EmeaOnlyController : ControllerBase { … }
```

Or inline:

```csharp
if (!User.HasClaim("region", "emea"))
    return Forbid();
```

### Read in a Razor Page

```csharp
public class DashboardModel : BasePage
{
    protected IEnumerable<string> CurrentRegions =>
        User.FindAll("region").Select(c => c.Value);
}
```

### Read in SPA / mobile / desktop

The JWT carries whatever claims you project into it. Parse the token and read the relevant key:

```ts
// Angular example
const token = jwtDecode<SessionClaims>(this.storage.getAccess());
const regions: string[] = Array.isArray(token.region) ? token.region : [token.region];
```

Extend `TokenService.GenerateAccessToken` (or wrap it) to pull these claim types from `IClaimService` when a user logs in — **that change is a public-surface change, update this wiki and the SPA/mobile docs in the same PR**.

## Method Reference

### Claim types

```csharp
Task<List<ClaimType>> GetAllClaimTypesAsync(CancellationToken ct = default);
Task<List<ClaimType>> GetActiveClaimTypesAsync(CancellationToken ct = default);
Task<ClaimType?>      GetClaimTypeByIdAsync(Guid id, CancellationToken ct = default);
Task<ClaimType?>      GetClaimTypeByKeyAsync(string key, CancellationToken ct = default);
Task<List<ClaimType>> GetClaimTypesByCategoryAsync(string category, CancellationToken ct = default);

Task<ClaimType> CreateClaimTypeAsync(ClaimType claimType, CancellationToken ct = default);
Task<ClaimType> UpdateClaimTypeAsync(ClaimType claimType, CancellationToken ct = default);
Task            DeleteClaimTypeAsync(Guid id, CancellationToken ct = default);
```

`ClaimType` fields to mind:

| Field | Meaning |
|-------|---------|
| `Key` | The claim name projected onto the identity (e.g. `"region"`). Lowercase, kebab-case if multi-word. |
| `AllowMultiple` | When false, `SetRoleClaimsAsync` / `SetUserClaimsAsync` should enforce a single value. |
| `Category` | Grouping bucket in the admin UI. |
| `IsSystem` | System types can't be deleted. |
| `PredefinedValues` | Optional list for dropdowns. Free-form types leave it empty. |

### Predefined values

```csharp
Task<ClaimValue> AddClaimValueAsync(Guid claimTypeId, ClaimValue value, CancellationToken ct = default);
Task             RemoveClaimValueAsync(Guid claimTypeId, Guid valueId, CancellationToken ct = default);
```

Deleting a value that's in use on any `RoleClaim`/`UserClaim` is typically destructive — validate from the admin UI before offering the button.

### Role claims

```csharp
Task<List<RoleClaim>> GetRoleClaimsAsync(string roleId, CancellationToken ct = default);
Task<List<RoleClaim>> GetRoleClaimsByTypeAsync(string roleId, string claimType, CancellationToken ct = default);

Task<RoleClaim> AddRoleClaimAsync(string roleId, string claimType, string claimValue, CancellationToken ct = default);
Task            RemoveRoleClaimAsync(Guid claimId, CancellationToken ct = default);
Task            SetRoleClaimsAsync(string roleId, string claimType, List<string> claimValues, CancellationToken ct = default);

Task<bool>            RoleHasClaimAsync(string roleId, string claimType, string claimValue, CancellationToken ct = default);
Task<HashSet<string>> GetClaimValuesForRolesAsync(IEnumerable<string> roleIds, string claimType, CancellationToken ct = default);
```

`SetRoleClaimsAsync` is per-type — it replaces **only** the claims of that type for the role. Other types stay untouched.

`GetClaimValuesForRolesAsync("region", ["admin","editor"])` is the hot path for projecting claims into the identity — returns a flat `HashSet<string>` of values.

### User claims (override role claims)

```csharp
Task<List<UserClaim>> GetUserClaimsAsync(Guid userId, CancellationToken ct = default);

Task<UserClaim> AddUserClaimAsync(Guid userId, string claimType, string claimValue, CancellationToken ct = default);
Task            RemoveUserClaimAsync(Guid claimId, CancellationToken ct = default);
Task            SetUserClaimsAsync(Guid userId, string claimType, List<string> claimValues, CancellationToken ct = default);
```

Convention: if a user has **any** `UserClaim` row for a given `ClaimType`, that replaces the role-level set for that type. Merge logic lives in the code path that builds the `ClaimsPrincipal` (JWT generator / cookie sign-in).

### Entity claim generation

```csharp
Task<List<ClaimValue>> GenerateEntityClaimsAsync(string entity, string displayName, CancellationToken ct = default);
Task<List<string>>     GetEntitiesWithClaimsAsync(CancellationToken ct = default);
```

Seeds a standard CRUD claim set (view/create/edit/delete) under a configurable claim type — useful when you want to model per-entity access as claims instead of permissions. Use `IPermissionService.GenerateEntityPermissionsAsync` if you prefer the permission model.

## Integrating With the Identity

Today, `AuthService.LoginAsync` → `TokenService.GenerateAccessToken` only emits `sub`, `email`, `jti`, `tenantId`, `displayName`, `role`, and `permission`. To emit custom claims you have two options:

1. **Extend `TokenService`** — inject `IClaimService`, enumerate active claim types, and for each merge role + user values. Safest for JWT consumers (one stable claim list per login).
2. **Project at middleware time** — mirror `PermissionMiddleware`: build a `ClaimMiddleware` that enriches the `ClaimsIdentity` on every request. Easier to iterate; heavier per-request cost.

Whichever you choose, document it here and mention cross-client impact: SPA/mobile/desktop clients need to know the claim shape.

## Permission Authorization — `[RequirePermission]` + handler

Permissions and claims are **different** but use similar plumbing. For completeness:

```csharp
// Pipeline
services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddAuthorization();

// Attribute
[RequirePermission("products.edit")]
public IActionResult Update(Guid id) => …;

// Behind the scenes this maps to [Authorize(Policy = "Permission:products.edit")]
// The policy requires a PermissionRequirement, which PermissionAuthorizationHandler
// satisfies by checking for a "permission" claim matching the code.
```

See [15 — Permission Service](./15-permission-service.md) for the full RBAC surface.

## Cross-Client Notes

When you add or rename a claim type, or change which claim types are projected into the identity:

| Client | What to change |
|--------|----------------|
| **Razor Pages / MVC** | `RequireClaim("…")` policies, `User.HasClaim(…)` branches, admin-UI drop-downs that read `ClaimType.PredefinedValues` |
| **Angular / React** | JWT claim parser, route guards, UI show/hide logic |
| **.NET MAUI** | Offline claim snapshot, biometric step-up conditions keyed on a claim |
| **WPF / WinUI** | Command `CanExecute` based on claim presence |

**Rule of thumb:** treat every claim type `Key` as a public identifier. Renaming breaks every downstream consumer.

## Common Mistakes

- **Mixing claims and permissions in the same key namespace** — keep permissions under the `permission` claim type; use distinct claim types (`region`, `department`) for everything else.
- **Forgetting the user-overrides-role rule** — if you don't merge user claims over role claims when building the identity, user-level adjustments won't take effect.
- **Deleting a `ClaimValue` that's in use** — orphans existing `RoleClaim`/`UserClaim` rows pointing to the value's string. The service doesn't cascade — check from the admin UI first.
- **Setting `AllowMultiple = false` but calling `SetRoleClaimsAsync` with a list > 1** — the service stores all of them; validate at the admin UI.
- **Using `ClaimService` to store secrets** — claims land in signed JWTs and cookies visible in browser devtools. Never store credentials, tokens, or PII in claims.
- **Not updating `TokenService` when you want the new claim type in the JWT** — claims stored but not projected won't show up on API calls. Either extend the token, add a middleware, or document why the claim is server-side only.
- **Using string comparison without case-normalising** — store keys lowercase consistently, matching permissions convention.

## See Also

- [14 — Auth Service](./14-auth-service.md) — where claims land in the JWT
- [15 — Permission Service](./15-permission-service.md) — the RBAC surface (permissions vs claims)
- [20 — Middleware Stack](./20-middleware-stack.md) — `PermissionMiddleware` pattern, extend for claims *(pending)*
- [03 — Base Page Pattern](./03-base-page-pattern.md) — `CurrentUserRoles` helper; extend with `CurrentUserClaims(type)` if you use claims heavily
