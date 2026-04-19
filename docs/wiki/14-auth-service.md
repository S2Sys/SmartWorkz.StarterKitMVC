# Auth Service

End-to-end authentication: login, registration, refresh tokens, password reset, email verification, profile lookup. Works for **every** client type — cookie for Razor Pages / MVC, JWT bearer for Angular / React / MAUI / WPF / WinUI.

Three collaborating services cover the full surface:

| Service | Role |
|---------|------|
| `IAuthService` | Orchestrates the flows — verifies credentials, issues tokens, queues reset/verify emails |
| `ITokenService` | Creates and validates JWTs, generates refresh tokens |
| `IPasswordHasher` | PBKDF2-SHA256 hashing + constant-time verification |

## Purpose

- One service surface used by web pages, REST controllers, mobile apps, and desktop apps.
- `Result<LoginResponse>` shape means callers never have to `try/catch` for expected failures (bad password, locked account, etc.).
- JWT includes `tenantId`, `role`, and `permission` claims — no extra round-trip to fetch them on each request.
- Refresh tokens are stored per-tenant in `Auth.RefreshTokens` and rotated on every refresh.
- "Forgot password" never reveals whether an email exists.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `IAuthService` | Contract | [`Application/Services/IAuthService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Services/IAuthService.cs) |
| `AuthService` | Implementation | [`Infrastructure/Services/AuthService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/AuthService.cs) |
| `ITokenService` / `TokenService` | JWT + refresh token helpers | [`Application/Services/ITokenService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Services/ITokenService.cs), [`Infrastructure/Services/TokenService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/TokenService.cs) |
| `IPasswordHasher` / `PasswordHasher` | PBKDF2-SHA256, 100k iterations | [`Application/Services/IPasswordHasher.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Services/IPasswordHasher.cs), [`Infrastructure/Services/PasswordHasher.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/PasswordHasher.cs) |
| `AuthDto.cs` | Request / response records | [`Shared/DTOs/AuthDto.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/DTOs/AuthDto.cs) |
| `MessageKeys.Auth` | Every failure message | [`Shared/Constants/MessageKeys.cs`](../../src/SmartWorkz.StarterKitMVC.Shared/Constants/MessageKeys.cs) |

### Login flow

```
LoginAsync(LoginRequest)
    ↓
UserRepository.GetByEmailAsync(email, tenantId)
    ↓
PasswordHasher.Verify(password, user.PasswordHash)   ← constant-time compare
    ↓
Check IsActive / LockoutEnd
    ↓
UserRepository.GetUserRolesAsync + GetUserPermissionsAsync
    ↓
TokenService.GenerateAccessToken(user, roles, permissions)   ← signed JWT
TokenService.GenerateRefreshToken()                           ← 64 random bytes, base64
    ↓
UserRepository.CreateRefreshTokenAsync(...)                   ← stored in Auth.RefreshTokens
    ↓
Return Result.Ok(LoginResponse(accessToken, refreshToken, expiresAt, profile))
```

## DI Registration

Wired by `AddApplicationServices` (called from `AddApplicationStack`):

```csharp
services.AddScoped<IAuthService,   AuthService>();
services.AddScoped<ITokenService,  TokenService>();
services.AddSingleton<IPasswordHasher, PasswordHasher>();   // no state, safe as singleton
```

Plus `AddJwtAuthentication` configures the JWT bearer scheme used by API / SPA / mobile / desktop clients:

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer   = true, ValidIssuer   = issuer,
            ValidateAudience = true, ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.Zero
        };
    });
```

See [`ServiceCollectionExtensions.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Extensions/ServiceCollectionExtensions.cs).

### Cookie scheme for web (Public / Admin)

`AddApplicationStack` wires JWT as default. Razor Pages hosts override with Cookie auth **after** calling `AddApplicationStack` — see [`Public/Program.cs`](../../src/SmartWorkz.StarterKitMVC.Public/Program.cs) and [`00 — Getting Started`](./00-getting-started.md).

## Configuration

```json
"Features": {
  "Authentication": {
    "Jwt": {
      "Enabled": true,
      "Secret": "k9$mL7@vQ2*xR4&pT8#wN5%jB1^yH6!Zu",     // 32+ chars, rotate per env
      "Issuer": "SmartWorkz.StarterKitMVC",
      "Audience": "StarterKitMVC.Users",
      "ExpiryMinutes": 60,
      "RefreshTokenExpiryDays": 7
    }
  }
}
```

Also consumed: `App:BaseUrl` (used in the password-reset link).

## Quick Start

### Web (Razor Pages / MVC) — cookie sign-in after `LoginAsync`

```csharp
public class LoginModel : BasePage
{
    private readonly IAuthService _auth;
    public LoginModel(IAuthService auth) => _auth = auth;

    [BindProperty] public LoginInput Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _auth.LoginAsync(
            new LoginRequest(Input.Email, Input.Password, TenantId));

        if (!result.Success)
        {
            AddErrors(result);                    // uses MessageKey → translated
            return Page();
        }

        // Sign in with cookie scheme — JWT is also in result.Value.AccessToken
        // if you want to forward it to a downstream API call.
        var identity = new ClaimsIdentity(BuildClaims(result.Value!), CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        ToastSuccess(MessageKeys.Auth.LoginSuccess);
        return LocalRedirect(returnUrl ?? "/");
    }
}
```

### SPA / mobile / desktop — JWT bearer

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "alice@acme.test", "password": "...", "tenantId": "acme" }

200 OK
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs…",
  "refreshToken": "p4c2…base64…",
  "expiresAt": "2026-04-19T10:28:00Z",
  "user": { "userId": "…", "email": "…", "roles": [...], "permissions": [...] }
}
```

Client stores both tokens securely (Angular: `HttpOnly` cookie for refresh + memory for access; MAUI: `SecureStorage`; WPF/WinUI: `CredentialLocker` / DPAPI). Send `Authorization: Bearer <accessToken>` on each call.

## Method Reference — `IAuthService`

Every method returns `Result` or `Result<T>` — failure is expected, not exceptional. `MessageKey`s in the result are resolved by `ITranslationService` at render time.

### `LoginAsync(LoginRequest)` → `Result<LoginResponse>`

```csharp
var result = await _auth.LoginAsync(new LoginRequest(email, password, tenantId));
if (!result.Success) return BadRequest(new ApiError(result.MessageKey!));
var (accessToken, refreshToken, expiresAt, profile) = result.Value!;
```

Failures:
| MessageKey | Reason |
|------------|--------|
| `auth.invalid_credentials` | Unknown email or wrong password |
| `auth.account_inactive` | `User.IsActive == false` |
| `auth.account_locked` | `LockoutEnd > now` |

Side effects on success: resets `AccessFailedCount`, bumps `UpdatedAt`, stores a new refresh token in `Auth.RefreshTokens`.

### `RegisterAsync(RegisterRequest)` → `Result<LoginResponse>`

Creates the user with hashed password then calls `LoginAsync`. Fails fast with `auth.email_already_registered` if the email is taken in that tenant.

```csharp
await _auth.RegisterAsync(new RegisterRequest(
    Email: "bob@acme.test",
    Username: "bob",
    Password: "S0me_Str0ng!pw",
    DisplayName: "Bob",
    TenantId: "acme"));
```

> Registration creates a `SecurityStamp` and `ConcurrencyStamp` per user for future rotation / optimistic concurrency.

### `RefreshTokenAsync(RefreshTokenRequest)` → `Result<LoginResponse>`

Takes the **expired** access token + the refresh token, validates them, rotates the refresh token, and returns a new pair.

```csharp
var refreshed = await _auth.RefreshTokenAsync(new RefreshTokenRequest(oldAccessToken, oldRefreshToken));
```

Behaviour:
- Calls `TokenService.GetPrincipalFromExpiredToken` with `ValidateLifetime = false` to extract the user id.
- Verifies the refresh token exists in `Auth.RefreshTokens`.
- **Revokes** the old refresh token before issuing a new one (rotating refresh pattern).

### `RevokeTokenAsync(string userId, string refreshToken)` → `Result`

Explicit sign-out for the current refresh token. Call on logout from SPA/mobile/desktop. Access tokens remain valid until their short TTL expires — keep TTL short (≤ 60 min) for that reason.

### `ForgotPasswordAsync(ForgotPasswordRequest)` → `Result`

Always returns `Result.Ok()` regardless of whether the email exists — this is an **enumeration-safe** endpoint. Queues an email through `IEmailQueueRepository` containing a 2-hour reset link.

### `ResetPasswordAsync(ResetPasswordRequest)` → `Result`

Consumes the token from the email. Failures collapse to `auth.password_reset_invalid` (don't leak which step failed). On success: rehashes the password, rotates `SecurityStamp`, marks the token used.

### `ChangePasswordAsync(string userId, ChangePasswordRequest)` → `Result`

Authenticated user changing their own password. Verifies the current password before applying the new one; rotates `SecurityStamp`.

### `VerifyEmailAsync(VerifyEmailRequest)` → `Result`

Consumes an email-verification token; flips `EmailConfirmed = true`.

### `GetProfileAsync(string userId)` → `Result<UserProfileDto>`

Used by `/api/auth/me` style endpoints and by the web host to rebuild cookie claims after a password change. Returns roles + permissions alongside the profile.

## Method Reference — `ITokenService`

```csharp
string          GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
string          GenerateRefreshToken();
ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
```

### JWT claim shape (important for all clients)

| Claim | Value |
|-------|-------|
| `sub` (`JwtRegisteredClaimNames.Sub`) | `user.UserId` |
| `email` | `user.Email` |
| `jti` | New GUID per token |
| `tenantId` | `user.TenantId` |
| `displayName` | `user.DisplayName` or `user.Username` |
| `role` (one per role) | `r` for each role |
| `permission` (one per permission) | `p` for each permission |

Angular / React / MAUI / WPF clients read `tenantId`, `role`, and `permission` from the JWT directly — no extra round trip.

### Custom access-token claims

If you need additional claims (tenant features, beta flags), subclass `TokenService` and override `GenerateAccessToken`, or add to the claim list in a wrapper service. **Adding a claim is a public-surface change — update this wiki and the SPA/mobile client docs in the same PR.**

## Method Reference — `IPasswordHasher`

```csharp
string Hash(string password);         // $"{base64(salt)}.{base64(hash)}"
bool   Verify(string password, string hash);
```

Defaults:
- PBKDF2 with SHA-256
- 100 000 iterations
- 16-byte salt
- 32-byte hash
- Constant-time comparison via `CryptographicOperations.FixedTimeEquals`

Format: `{SaltBase64}.{HashBase64}` — easy to store in a single `NVARCHAR(256)` column. If you ever change the algorithm or iteration count, **write a re-hash on next login** (verify old, hash with new, upsert) and flag it in the wiki.

## Cross-Client Notes

When anything in this page changes, update client code on every consumer type:

| Client | What to update |
|--------|----------------|
| **Razor Pages / MVC** | Cookie sign-in after `LoginAsync`; `BasePage.AddErrors(Result)` already does the `MessageKey` plumbing |
| **Angular / React** | HTTP interceptor that reads `Authorization: Bearer`, refresh-on-401 flow, `tenantId`/`role`/`permission` claim parsing |
| **.NET MAUI** | `SecureStorage` for both tokens, `HttpClient.DefaultRequestHeaders.Authorization`, biometric unlock hooks |
| **WPF / WinUI** | `CredentialLocker` or DPAPI for both tokens, refresh on 401 before retrying the request |

If you add a new claim or change a DTO, every client gets the change — call it out explicitly in the PR body and the wiki diff.

## Common Mistakes

- **Re-using a refresh token** — they rotate on every call to `RefreshTokenAsync`. Store the new token returned in the response.
- **Long access-token TTLs** (`ExpiryMinutes > 60`) — widens the compromise window. Keep them short and lean on refresh.
- **Hard-coding the JWT secret** in source — always pull from `Features:Authentication:Jwt:Secret` and rotate per environment. Never commit production secrets.
- **Mixing cookie and JWT schemes in the same app without explicit scheme selectors** — `[Authorize(AuthenticationSchemes = …)]` is required when both are registered.
- **Skipping `SecurityStamp` rotation on password change / reset** — leaves old issued tokens valid. The service does this for you; don't bypass it.
- **Relying on `Result.Value.User.Permissions` staying fresh** — the profile is captured at login time; permission changes don't take effect until the next token refresh. For real-time revocation, validate permissions server-side on each call.
- **Exposing `IPasswordHasher` to untrusted input without rate limiting** — PBKDF2 at 100k iterations takes ~50ms per verify. Fine for login at reasonable request rates; add a rate limiter before you expose `/api/auth/login` to the public internet.
- **Not updating `06-multi-tenant-login-flow.md`** when the login flow itself changes. It's the companion doc to this one.

## See Also

- [00 — Getting Started](./00-getting-started.md) — where `AddApplicationStack` + cookie/JWT scheme are wired
- [06 — Multi-Tenant Login Flow](./06-multi-tenant-login-flow.md) — the flow as a whole, including tenant resolution
- [15 — Permission Service](./15-permission-service.md) — where roles → permissions mapping comes from
- [16 — Claim Service](./16-claim-service.md) — custom claim types on top of the defaults above
- [04 — Result Pattern](./04-result-pattern.md) — how the Result<T> shape works
- [01 — Password Reset Flow](./01-password-reset-flow.md) — deep dive on the reset + verify email flow
