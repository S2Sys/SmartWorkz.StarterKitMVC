# Shared Primitives

The types in `Shared/` are the framework's **public vocabulary** — every service returns, accepts, or wraps them. Mistakes here ripple through every client (web, SPA, mobile, desktop). Treat them as a stable API.

## What's in the box

| Type | File | Used for |
|------|------|----------|
| `Result` / `Result<T>` | [`Shared/Models/ValidationResult.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/Models/ValidationResult.cs) | Service-to-caller success/failure with `MessageKey` |
| `ApiError` | [`Shared/Primitives/ApiError.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/Primitives/ApiError.cs) | Serialised error for API responses |
| `ProblemDetailsResponse` | [`Shared/Primitives/ProblemDetails.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/Primitives/ProblemDetails.cs) | RFC 7807 error envelope used by `GlobalExceptionHandlingMiddleware` |
| `ICorrelationContext` / `CorrelationContext` | [`Shared/Primitives/CorrelationContext.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/Primitives/CorrelationContext.cs) | Per-request correlation id for distributed tracing |
| `MessageKeys` | [`Shared/Constants/MessageKeys.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/Constants/MessageKeys.cs) | All translatable strings |

## `Result` / `Result<T>` — the service return type

Every service in the framework returns `Result` (no payload) or `Result<T>` (with typed payload). It's the single shape every caller — Razor page, REST controller, Angular, MAUI — expects.

```csharp
public class Result
{
    public bool Succeeded { get; }
    public string? MessageKey { get; }
    public IReadOnlyList<string> Errors { get; } = [];

    public static Result Ok();
    public static Result Fail(string messageKey, params string[] errors);

    public static Result<T> Ok<T>(T data);
    public static Result<T> Fail<T>(string messageKey, params string[] errors);
}

public class Result<T> : Result { public T? Data { get; } }
```

Why `MessageKey` instead of a literal string: the web host translates at render time via `ITranslationService`, SPA/mobile clients look up a resource bundle by the same key. **Never return a user-readable string from a service** — always a key.

### Service pattern

```csharp
public async Task<Result<UserProfileDto>> GetProfileAsync(string userId)
{
    var user = await _users.GetByIdAsync(userId);
    if (user is null)
        return Result.Fail<UserProfileDto>(MessageKeys.User.UserNotFound);

    return Result.Ok(Map(user));
}
```

### Razor page caller

```csharp
var r = await _auth.GetProfileAsync(userId);
if (!r.Succeeded) { AddErrors(r); return Page(); }   // BasePage helper binds to ModelState
var profile = r.Data!;
```

### REST controller caller

```csharp
var r = await _auth.GetProfileAsync(userId);
if (!r.Succeeded)
    return BadRequest(new ApiError { Code = r.MessageKey!, Message = r.MessageKey! });
return Ok(r.Data);
```

### Rules

- **Only one `Result` type.** An earlier duplicate at `Shared.Primitives.Result` (record struct + `Error(Code, Message)`) was removed — it had zero production usages.
- **No exceptions for expected failures.** If the outcome is a business state (not found, invalid credentials, concurrency conflict), return `Result.Fail(messageKey)`. Reserve exceptions for truly exceptional conditions.
- **Errors list is for secondary details** — e.g. field-level validation failures. Primary failure code goes in `MessageKey`.

## `ApiError` — the over-the-wire error

```csharp
public sealed class ApiError
{
    public string Code { get; init; }
    public string Message { get; init; }
    public string? TraceId { get; init; }
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }
}
```

Use from controller exception branches when you're returning a simple JSON error (as opposed to the richer `ProblemDetailsResponse` shape). Angular / React / MAUI / WPF clients deserialise it to a typed error model and surface `Details` as per-field validation.

```csharp
return BadRequest(new ApiError
{
    Code = "USER_NOT_FOUND",
    Message = "User was not found.",
    TraceId = HttpContext.TraceIdentifier,
    Details = new Dictionary<string, string[]>
    {
        ["email"] = new[] { "No user exists for this email." }
    }
});
```

## `ProblemDetailsResponse` — RFC 7807 envelope

This is what `GlobalExceptionHandlingMiddleware` writes to `application/problem+json`. Factories for each common case:

```csharp
ProblemDetailsResponse.ValidationError(detail, errors, instance, traceId);   // 400
ProblemDetailsResponse.Unauthorized(detail, instance, traceId);              // 401
ProblemDetailsResponse.Forbidden(detail, instance, traceId);                 // 403
ProblemDetailsResponse.NotFound(detail, instance, traceId);                  // 404
ProblemDetailsResponse.Conflict(detail, instance, traceId);                  // 409
ProblemDetailsResponse.InternalServerError(detail, instance, traceId);       // 500
```

Fields:

| Field | Notes |
|-------|-------|
| `Type` | URI identifying the error type — convention: `https://api.example.com/errors/{slug}` |
| `Title` | Short human title |
| `Status` | HTTP status code |
| `Detail` | Longer explanation |
| `Instance` | The request path |
| `Errors` | Per-field error dict (validation shape) |
| `TraceId` | Correlation id — set this to `context.TraceIdentifier` or `ICorrelationContext.CorrelationId` |
| `Timestamp` | Server-side `DateTime.UtcNow` auto-populated |

SPA / mobile / desktop clients parse this shape directly. Renaming a field is a breaking change.

## `ICorrelationContext` — distributed tracing id

Scoped per request, populated by `CorrelationIdMiddleware` (see [20 — Middleware Stack](./20-middleware-stack.md)).

```csharp
public interface ICorrelationContext
{
    string? CorrelationId { get; set; }
}
```

DI registration (if using):

```csharp
services.AddScoped<ICorrelationContext, CorrelationContext>();
```

Use from any service that logs, emits events, or calls a downstream HTTP service — include the id in every log scope and in outbound `X-Correlation-ID` headers:

```csharp
public class AuditService
{
    private readonly ICorrelationContext _corr;
    private readonly ILogger<AuditService> _log;

    public void LogAction(string action)
        => _log.LogInformation("[{CorrelationId}] {Action}", _corr.CorrelationId, action);
}
```

For background jobs, capture the id at enqueue time and rebuild a new `CorrelationContext` inside the worker scope — never share across threads.

## `MessageKeys` — the translation contract

`Shared/Constants/MessageKeys.cs` holds nested static classes, each constant being a dot-delimited key resolved at render time by `ITranslationService`:

```csharp
public static class MessageKeys
{
    public static class Auth
    {
        public const string InvalidCredentials = "auth.invalid_credentials";
        public const string LoginSuccess       = "auth.login_success";
        // …
    }

    public static class Validation { … }
    public static class Crud       { … }
    public static class User       { … }
    public static class Tenant     { … }
    public static class General    { … }
    public static class Template   { … }
    public static class EmailQueue { … }
    public static class Cache      { … }
}
```

### Rules

- **Never hard-code a literal** in a service or a `[Required(ErrorMessage = "…")]`. Always reference `MessageKeys.*`.
- **Adding a key is a public-surface change** — translators must seed it in the DB. List the additions in the wiki + CHANGELOG entry.
- **Renaming a key is a breaking change** — bump semver appropriately and flag every caller (including SPA / mobile / desktop resource bundles).
- **Keep values lowercase with dots** — `"auth.login_success"`, not `"Auth.LoginSuccess"`.

See [01 — Translation System](./01-translation-system.md) for how keys resolve to localized strings.

## Cross-Client Notes

Every primitive on this page **lands at a client boundary**:

| Client | Touches |
|--------|---------|
| **Razor Pages / MVC** | `Result` / `Result<T>` bound to `ModelState` via `BasePage.AddErrors(result)` |
| **Angular / React** | `ApiError` + `ProblemDetailsResponse` in HTTP interceptors; `MessageKey` in i18n bundles; `X-Correlation-ID` in request log |
| **.NET MAUI** | Same as Angular/React; `MessageKey` goes into `.resx` or a JSON locale file |
| **WPF / WinUI** | Same as MAUI |

Add / rename / remove any primitive, and the entire client fleet needs a matching update. Note it here and in the changelog.

## Common Mistakes

- **Introducing a second `Result` shape** — there is exactly one (`Shared.Models.Result`). If you need a different shape (record struct, `Error` object, etc.), change this one and migrate every caller; don't create a parallel type.
- **Returning a literal string as an error** instead of a `MessageKey` — breaks localization everywhere.
- **Swallowing the `MessageKey`** on the caller side — page model should always `AddErrors(result)` so `ModelState` gets it; API should return `ApiError { Code = result.MessageKey }`.
- **Forgetting to set `TraceId`** on `ApiError` / `ProblemDetailsResponse` — makes client-side support tickets impossible to correlate with server logs.
- **Storing request data on `CorrelationContext`** — it's meant for the id only. If you need per-request state, create a separate scoped service.
- **Extending `MessageKeys` without updating translations** — the key fallback is the key itself, so users will see `auth.new_message` verbatim. Always seed translations in the same PR.
- **Breaking `ProblemDetailsResponse` field names** — they're the public API. Renaming `Detail` or `Errors` breaks every consumer.

## See Also

- [01 — Translation System](./01-translation-system.md) — `MessageKeys` → localized strings
- [02 — Localized Validation](./02-localized-validation.md) — `MessageKeys.Validation.*` in attributes
- [04 — Result Pattern](./04-result-pattern.md) — using `Shared.Models.Result` in flows
- [14 — Auth Service](./14-auth-service.md) — canonical consumer of `Result<T>`
- [20 — Middleware Stack](./20-middleware-stack.md) — where `CorrelationIdMiddleware` populates `ICorrelationContext`
