# Wiki Update Rules

Standing rules for keeping `docs/wiki/` in sync with code changes.

## Rule: Auth service enhancements require wiki update

Whenever `IAuthService` / `AuthService` or any directly related auth component changes (new method, changed signature, new flow, new config key, new claim), the corresponding wiki page **must be updated in the same PR**.

**Scope — touching any of these triggers the rule:**
- `src/SmartWorkz.StarterKitMVC.Application/Services/IAuthService.cs`
- `src/SmartWorkz.StarterKitMVC.Infrastructure/Services/AuthService.cs`
- `ITokenService` / `TokenService` (JWT issuance + validation)
- `IPasswordHasher` / `PasswordHasher` (hash algorithm or iteration count)
- `Features:Authentication:Jwt:*` settings
- Cookie auth wiring in `Public/Program.cs` or `Admin/Program.cs`
- Login / Register / Refresh / Reset / Verify page models in `Public/Pages/Account/` or `Admin/Pages/Account/`

**Wiki target:** `docs/wiki/14-auth-service.md` (and `06-multi-tenant-login-flow.md` if login flow changed).

**Update must cover:**
- New/changed method signature + one-line description
- New/changed config keys + default values
- A minimal code sample showing the new behaviour
- Any new failure `MessageKey` added to `Shared/Constants/MessageKeys.cs`
- Entry in `CHANGELOG.md` under the next release

**Commit discipline:** the code change and the wiki change go in the same commit (or adjacent commits on the same PR). A PR that modifies auth code without touching `14-auth-service.md` should be flagged in review.

---

## General principle (applies to all services)

Each completed service listed in `docs/wiki/` has a dedicated page. If you modify the public surface of a service, update its wiki page in the same PR. The wiki is the contract; stale wiki is worse than no wiki.
