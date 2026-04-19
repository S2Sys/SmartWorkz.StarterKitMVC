# Permission Service

Entity + action RBAC. Features are the modules ("users", "orders"), Permissions are the verbs on those features ("users.view", "users.create"), and RolePermissions join them to roles. The Auth service projects a user's permissions into JWT claims on login; `PermissionAuthorizationHandler` + `[RequirePermission]` enforce them on every protected call.

## Purpose

- **Entity × Action grid** — `PermissionAction` enum (View / Create / Edit / Delete / Export / Import / Approve / Reject / Publish / Archive / Restore / ManagePermissions / ManageSettings / FullAccess).
- **Hierarchical Features** — parent/child tree for grouping permissions in the admin UI.
- **One-shot CRUD seeding** — `GenerateEntityPermissionsAsync` creates the standard 4 × CRUD permissions for any new entity in one call.
- **Bulk role editing** — `SetRolePermissionsAsync` replaces the whole set in a single call (admin UI).
- **Fast auth checks** — `GetPermissionKeysForRolesAsync` returns a flat `HashSet<string>` for `PermissionMiddleware` to project into claims.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `IPermissionService` | Service contract | [`Application/Authorization/IPermissionService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Authorization/IPermissionService.cs) |
| `PermissionService` | EF + SP implementation | [`Infrastructure/Authorization/PermissionService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Authorization/PermissionService.cs) |
| `Feature` | Module / navigation node (`Auth.Features`) | [`Domain/Authorization/Permission.cs`](../../src/SmartWorkz.StarterKitMVC.Domain/Authorization/Permission.cs) |
| `Permission` | Action on a feature (`Auth.Permissions`) | same file |
| `PermissionAction` enum | View, Create, Edit, Delete, … | same file |
| `RolePermission` | Role → permission join (`Auth.RolePermissions`) | same file |
| `PermissionRequirement` (domain) | Entity + action pair used for `GetPermissionKey()` | same file |
| `PermissionRequirement` (infra) | `IAuthorizationRequirement` holding a permission code | [`Infrastructure/Authorization/PermissionRequirement.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Authorization/PermissionRequirement.cs) |
| `PermissionAuthorizationHandler` | Matches `permission` claims against the requirement | [`Infrastructure/Authorization/PermissionAuthorizationHandler.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Authorization/PermissionAuthorizationHandler.cs) |
| `RequirePermissionAttribute` | Shorthand `[Authorize(Policy = "Permission:...")]` | [`Infrastructure/Authorization/RequirePermissionAttribute.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Authorization/RequirePermissionAttribute.cs) |
| `PermissionMiddleware` | Hydrates the identity with `permission` claims from role list | [`Public/Middleware/PermissionMiddleware.cs`](../../src/SmartWorkz.StarterKitMVC.Public/Middleware/PermissionMiddleware.cs) |

### Permission-key shape

`{entity}.{action}` lower-case — `PermissionRequirement.GetPermissionKey()` normalises both sides:

```csharp
new PermissionRequirement("Users", PermissionAction.Edit).GetPermissionKey();
// → "users.edit"
```

Use the same shape when calling `HasPermissionAsync` / granting permissions.

## DI Registration

Wired by `AddApplicationServices` + the per-host middleware:

```csharp
// Service
services.AddScoped<IPermissionService, PermissionService>();

// Authorization handler (resolves the "Permission:…" policy)
services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddAuthorization();
```

Pipeline — see [`Public/Program.cs`](../../src/SmartWorkz.StarterKitMVC.Public/Program.cs):

```csharp
app.UseAuthentication();
app.UseTenantResolution();
app.UseAuthorization();

app.UseMiddleware<PermissionMiddleware>();   // ← promotes roles → permission claims
```

`PermissionMiddleware` runs **after** authorization so that the cookie/JWT identity is already built; it then adds `permission` claims for every permission key granted to the user's roles.

## Quick Start

### Seed permissions for a new entity

```csharp
public class InstallController : ControllerBase
{
    private readonly IPermissionService _perm;

    [HttpPost("seed-products")]
    public async Task<IActionResult> SeedProducts(CancellationToken ct)
    {
        await _perm.GenerateEntityPermissionsAsync("Products", "Product catalogue", ct);
        return Ok();   // creates products.view, products.create, products.edit, products.delete
    }
}
```

### Grant permissions to a role

```csharp
// Replace the whole set (admin UI "save")
await _perm.SetRolePermissionsAsync(
    roleId: "tenant-admin",
    permissionIds: selectedIds,
    ct);

// Or grant individually
await _perm.GrantPermissionAsync("editor", productEditPermissionId, condition: null, ct);
```

### Protect a controller action (SPA / mobile API)

```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet,    RequirePermission("products.view")]   public Task<IActionResult> List()   => …;
    [HttpPost,   RequirePermission("products.create")] public Task<IActionResult> Create() => …;
    [HttpPut("{id}"),    RequirePermission("products.edit")]   public Task<IActionResult> Update(Guid id) => …;
    [HttpDelete("{id}"), RequirePermission("products.delete")] public Task<IActionResult> Delete(Guid id) => …;
}
```

Resolve the policy registration once:

```csharp
services.AddAuthorization(options =>
{
    // Permission: prefix is matched by RequirePermissionAttribute
    options.AddPolicy("Permission:products.view",   p => p.Requirements.Add(new PermissionRequirement("products.view")));
    options.AddPolicy("Permission:products.create", p => p.Requirements.Add(new PermissionRequirement("products.create")));
    // … etc.
});
```

Or register them dynamically at startup by enumerating `IPermissionService.GetAllPermissionsAsync`.

### Protect a Razor Page

```csharp
[Authorize(Policy = "Permission:products.edit")]
public class EditModel : BasePage { … }
```

### Check a permission inline

```csharp
if (!await _perm.HasPermissionAsync(CurrentUserRoles, "orders.approve"))
    return Forbid();
```

## Method Reference

### Features (modules)

```csharp
Task<List<Feature>> GetAllFeaturesAsync(CancellationToken ct = default);
Task<Feature?>      GetFeatureByIdAsync(Guid id, CancellationToken ct = default);
Task<Feature?>      GetFeatureByKeyAsync(string key, CancellationToken ct = default);
Task<List<Feature>> GetFeatureTreeAsync(CancellationToken ct = default);
Task<Feature>       CreateFeatureAsync(Feature feature, CancellationToken ct = default);
Task<Feature>       UpdateFeatureAsync(Feature feature, CancellationToken ct = default);
Task                DeleteFeatureAsync(Guid id, CancellationToken ct = default);
```

Features are hierarchical via `ParentId` — `GetFeatureTreeAsync` returns the root set with children populated, ideal for the admin left nav.

```csharp
var ordersFeature = new Feature
{
    Key = "orders",
    Name = "Orders",
    Icon = "bi-cart",
    ParentId = null,
    SortOrder = 20
};
await _perm.CreateFeatureAsync(ordersFeature);
```

System features (`IsSystem = true`) cannot be deleted — `DeleteFeatureAsync` will throw.

### Permissions

```csharp
Task<List<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);
Task<List<Permission>> GetPermissionsByEntityAsync(string entity, CancellationToken ct = default);
Task<Permission?>      GetPermissionByIdAsync(Guid id, CancellationToken ct = default);
Task<Permission?>      GetPermissionByKeyAsync(string key, CancellationToken ct = default);
Task<Permission>       CreatePermissionAsync(Permission permission, CancellationToken ct = default);
Task<Permission>       UpdatePermissionAsync(Permission permission, CancellationToken ct = default);
Task                   DeletePermissionAsync(Guid id, CancellationToken ct = default);

Task<List<Permission>> GenerateEntityPermissionsAsync(string entity, string displayName, CancellationToken ct = default);
```

`GenerateEntityPermissionsAsync("Products", "Product catalogue")` is the fast path — creates the standard CRUD set:

| Key | Name | Action |
|-----|------|--------|
| `products.view` | View products | `View` |
| `products.create` | Create products | `Create` |
| `products.edit` | Edit products | `Edit` |
| `products.delete` | Delete products | `Delete` |

For non-CRUD verbs (approve, publish, export), create them individually with `CreatePermissionAsync`.

### Role permissions

```csharp
Task<List<RolePermission>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default);
Task<List<string>>         GetRolePermissionKeysAsync(string roleId, CancellationToken ct = default);

Task SetRolePermissionsAsync(string roleId, List<Guid> permissionIds, CancellationToken ct = default);
Task GrantPermissionAsync(string roleId, Guid permissionId, string? condition = null, CancellationToken ct = default);
Task RevokePermissionAsync(string roleId, Guid permissionId, CancellationToken ct = default);

Task<bool>            HasPermissionAsync(string roleId, string permissionKey, CancellationToken ct = default);
Task<bool>            HasPermissionAsync(IEnumerable<string> roleIds, string permissionKey, CancellationToken ct = default);
Task<HashSet<string>> GetPermissionKeysForRolesAsync(IEnumerable<string> roleIds, CancellationToken ct = default);
```

`SetRolePermissionsAsync` is atomic — it replaces the full set of permissions for the role. Call this from the admin role-editor "save" button.

`GrantPermissionAsync` supports a `condition` (JSON) for row-level scoping — e.g. `{"ownOnly": true}` to restrict users to their own records. The condition is stored; enforcement is the caller's responsibility for now (no condition interpreter yet).

`GetPermissionKeysForRolesAsync` is the hot path. Called from `PermissionMiddleware` and from `AuthService.LoginAsync` (indirectly via `UserRepository.GetUserPermissionsAsync`). Cache-friendly return type — a `HashSet<string>` of keys.

### `PermissionCheckResult`

Returned from richer check paths when the service adds context-sensitive denial reasons:

```csharp
public class PermissionCheckResult
{
    public bool    IsGranted     { get; set; }
    public string? DenialReason  { get; set; }
    public Dictionary<string, object>? Conditions { get; set; }

    public static PermissionCheckResult Granted() => …;
    public static PermissionCheckResult Denied(string reason) => …;
}
```

## Enforcement Pipeline

```
Request → Authentication
        → TenantResolution
        → Authorization (policy Permission:{key})
            → PermissionAuthorizationHandler reads user "permission" claims
        → PermissionMiddleware (adds missing "permission" claims from roles)
        → Route / controller / page
```

### JWT claim shape

`TokenService.GenerateAccessToken` adds one `permission` claim per permission key:

```json
{
  "sub": "…",
  "tenantId": "acme",
  "role":       ["tenant-admin", "editor"],
  "permission": ["products.view", "products.create", "orders.approve"]
}
```

This means SPA / mobile / desktop clients can **also** read the permissions from the decoded JWT to hide/show UI — no extra endpoint required.

### Cookie claim shape

Cookie identity stores the same shape. `PermissionMiddleware` backfills missing permissions from roles on every request so that a permission granted mid-session shows up on the next call, without a re-login.

## Cross-Client Notes

When you add / rename / remove a permission key, update:

| Client | What to change |
|--------|----------------|
| **Razor Pages / MVC** | `[Authorize(Policy = "Permission:...")]` attributes, page-level helpers that branch on role+permission |
| **Angular / React** | Route guards + directives that read the `permission` claim from the JWT |
| **.NET MAUI** | Offline permission snapshot cache (invalidate on refresh-token rotate) |
| **WPF / WinUI** | Command `CanExecute` that checks the cached permission set |

**Permission keys are a public contract.** Renaming one is a breaking change across clients.

## Common Mistakes

- **Casing mismatch** — `Products.view` ≠ `products.view`. Always store and compare lowercased. `PermissionRequirement.GetPermissionKey()` normalises; stick to it.
- **Forgetting `UsePermissions()`** in the pipeline — permissions granted to a role won't show up as claims; `[RequirePermission]` fails even for admins.
- **Wrong middleware order** — `PermissionMiddleware` must run **after** `UseAuthorization()` to have an authenticated identity.
- **Granting via `AddClaim("permission", key)` directly** — bypasses the role/feature audit trail. Always go through `GrantPermissionAsync` / `SetRolePermissionsAsync`.
- **Allowing `IsSystem = false` edit of system permissions** — the service already blocks delete, but ensure the admin UI also hides edit for system permissions you want frozen.
- **Relying on `condition` JSON without implementing an interpreter** — conditions are stored, not enforced automatically. If you ship a `{"ownOnly": true}` condition, add the code that reads `context.User.GetUserId()` and filters the query.
- **Stale claim issues after admin changes a role** — cookies are refreshed on the next request by `PermissionMiddleware`; JWT bearers don't update until the next refresh. Document the TTL in the admin UI so operators know it.

## See Also

- [14 — Auth Service](./14-auth-service.md) — login projects permissions into the JWT
- [16 — Claim Service](./16-claim-service.md) — custom claim types beyond permission keys
- [00 — Getting Started](./00-getting-started.md) — pipeline wiring + DI
- [20 — Middleware Stack](./20-middleware-stack.md) — full middleware order reference *(pending)*
