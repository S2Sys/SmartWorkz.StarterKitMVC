# Tag Helpers + Validators

Small surface, but used on every form and every API controller. Treat both as stable public API.

## What's in the box

| Type | File | Purpose |
|------|------|---------|
| `EnumSelectTagHelper` | [`Shared/TagHelpers/EnumSelectTagHelper.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/TagHelpers/EnumSelectTagHelper.cs) | Render `<select>` options from an enum type |
| `AuthValidators` | [`Shared/Validation/AuthValidators.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/Validation/AuthValidators.cs) | Shape checks for auth DTOs (called from controllers / page models) |
| `EntityValidators` | [`Shared/Validation/EntityValidators.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/Validation/EntityValidators.cs) | Shape checks for business DTOs (tenant, product, category, menu, SEO, tag, user) |

These are **not** a replacement for data-annotation attributes — those still drive model binding + ModelState. The static validators are a quick "is this request well-formed?" check at the service boundary.

## `EnumSelectTagHelper`

### Registration

Add the tag helpers assembly in `_ViewImports.cshtml` for each Razor host (Admin, Public):

```razor
@addTagHelper *, SmartWorkz.StarterKitMVC.Shared
```

No DI registration required — tag helpers are discovered by attribute.

### Usage

Two styles. Either works; pick whichever matches the context.

```razor
@* Typed — preferred when the enum lives in this assembly *@
<select asp-for="Status" enum-select-for="typeof(UserStatus)"></select>

@* With a blank placeholder (default) *@
<select asp-for="Status" enum-select-for="typeof(UserStatus)" blank-text="-- Select status --"></select>

@* Without the blank option *@
<select asp-for="Status" enum-select-for="typeof(UserStatus)" add-blank="false"></select>
```

Produces:

```html
<select name="Status" id="Status">
  <option value="">-- Select status --</option>
  <option value="Active">Active</option>
  <option value="Suspended">Suspended</option>
  <option value="Deleted">Deleted</option>
</select>
```

Behaviour:

- Values are the enum member names (exact casing).
- Labels are the same names with spaces inserted before capital letters (`LoggedIn` → `Logged In`).
- Current value is pre-selected by matching the `value` attribute rendered by `asp-for`.
- If `EnumType` is null or not an enum, the helper is a no-op.

### When to use it

- Admin forms over lookup-like enums (`UserStatus`, `OrderStatus`, `PermissionAction`).
- Any place a dropdown maps 1:1 to an enum and you don't want to maintain a parallel `SelectListItem[]` in the page model.

### When NOT to use it

- Non-enum lookups (countries, roles, tenants) — use a LoV service populated from the DB.
- Localized labels — the helper formats the enum name, not a translation. If you need localized text, build the list in the page model and translate via `T(MessageKeys.Enum.UserStatus.Active)`.

### Cross-client notes

This is server-rendered only. SPA / mobile / desktop clients that render their own dropdowns should either:

- Fetch the enum list from an API endpoint (`GET /api/lookups/enums/UserStatus`), or
- Hard-code the mapping (acceptable when the enum is stable).

## `AuthValidators` — static shape checks for auth DTOs

```csharp
public static class AuthValidators
{
    bool IsValidLogin(LoginRequest);
    bool IsValidRegister(RegisterRequest);   // password >= 8, email format
    bool IsValidEmail(string);
    bool IsValidPassword(string);            // >= 8, 1 upper, 1 lower, 1 digit
    bool IsValidRefreshToken(RefreshTokenRequest);
    bool IsValidForgotPassword(ForgotPasswordRequest);
    bool IsValidResetPassword(ResetPasswordRequest);
    bool IsValidChangePassword(ChangePasswordRequest);  // current != new, password policy
    bool IsValidVerifyEmail(VerifyEmailRequest);
}
```

Purpose: catch obviously malformed requests **before** hitting the service layer (which would otherwise throw or run a wasted DB round-trip).

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    if (!AuthValidators.IsValidLogin(request))
        return BadRequest(new ApiError
        {
            Code = MessageKeys.Validation.Required,
            Message = "Email, password, and tenantId are required."
        });

    var result = await _auth.LoginAsync(request);
    return result.Succeeded
        ? Ok(result.Data)
        : Unauthorized(new ApiError { Code = result.MessageKey! });
}
```

### What they check vs what data annotations cover

| Concern | Data annotations | `AuthValidators` |
|---------|------------------|------------------|
| Required field present | ✅ `[Required]` | ✅ `IsNullOrWhiteSpace` |
| Email format | ✅ `[EmailAddress]` | ✅ `MailAddress` parse |
| Password policy (8+ / upper / lower / digit) | ❌ | ✅ `IsValidPassword` |
| Cross-field rule (`Current != New`) | ❌ (needs custom attr) | ✅ `IsValidChangePassword` |

Use both — annotations let `[ApiController]` auto-return 400s; validators cover the rules annotations can't express.

### Password policy

`IsValidPassword` enforces: `length ≥ 8`, at least one uppercase, one lowercase, one digit. It does **not** require a symbol, and it does **not** check against a breach list. If you tighten the policy, update this wiki **and** `Features:Identity:PasswordPolicy` defaults in `appsettings.json`.

## `EntityValidators` — shape checks for business DTOs

```csharp
public static class EntityValidators
{
    bool IsValidTenantDto(TenantDto);       // name ≤ 256
    bool IsValidProductDto(ProductDto);     // name ≤ 256, sku ≤ 50, price ≥ 0
    bool IsValidCategoryDto(CategoryDto);
    bool IsValidMenuDto(MenuDto);
    bool IsValidSeoMetaDto(SeoMetaDto);     // title OR description must be present
    bool IsValidTagDto(TagDto);             // tagName ≤ 100
    bool IsValidUserProfileDto(UserProfileDto);
}
```

Same pattern — guard the service boundary:

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] ProductDto dto)
{
    if (!EntityValidators.IsValidProductDto(dto))
        return BadRequest(new ApiError { Code = "PRODUCT_INVALID", Message = "Name, SKU, and TenantId are required." });

    // … hand off to service …
}
```

Add a validator when you add a new DTO. Delete one if the DTO is retired. Both are public surface — any signature change ripples to the web, SPA, mobile, desktop clients that use the same DTO shapes.

## Cross-Client Notes

| Client | What this page touches |
|--------|------------------------|
| **Razor Pages / MVC** | `EnumSelectTagHelper` server-rendered dropdowns; `*Validators` guarding form posts |
| **Angular / React** | Mirror the `IsValidPassword` rule in a reactive form validator; enum dropdowns rendered client-side from a lookup API |
| **.NET MAUI** | Same mirror logic in platform views; enums either embedded or fetched at startup |
| **WPF / WinUI** | Same |

If you change the password policy, change it **everywhere** in the same PR. If you add a new enum, expose it via a lookup endpoint and document here.

## Common Mistakes

- **Skipping data annotations because you called `AuthValidators`** — `[Required]` + `[EmailAddress]` drive `ModelState` and automatic 400 responses. Validators complement, don't replace them.
- **Relying on `EnumSelectTagHelper` for localized labels** — it formats, doesn't translate. Build a localized list in the page model when you need multi-locale labels.
- **Hand-rolling password rules in a new place** — always call `AuthValidators.IsValidPassword`. Drift between this policy and SPA / mobile clients is a common source of "my valid password was rejected" tickets.
- **Adding a DTO without a matching validator** — leaves the service boundary unchecked. Add one in the same PR.
- **Using `MailAddress` for validation in hot paths** — it allocates; fine per-request, not fine in a tight loop.
- **Trusting client-side validation alone** — always re-run server-side. SPA / mobile clients can skip checks when devtools are open.

## See Also

- [02 — Localized Validation](./02-localized-validation.md) — data-annotation attributes + `MessageKeys.Validation.*`
- [03 — Base Page Pattern](./03-base-page-pattern.md) — `AddErrors(Result)` surfaces service failures into `ModelState`
- [14 — Auth Service](./14-auth-service.md) — consumes `AuthValidators` indirectly via the login/register flows
- [22 — Shared Primitives](./22-shared-primitives.md) — `ApiError` / `ProblemDetailsResponse` wrap the validator results
