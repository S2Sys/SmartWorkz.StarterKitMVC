# Wiki Update Rules

Standing rules for keeping `docs/wiki/` in sync with code. SmartWorkz.StarterKitMVC is a **framework** consumed from Razor Pages, MVC, Angular/React SPAs, .NET MAUI, and Windows desktop apps. The wiki is how those consumers learn the public surface — stale wiki breaks integrations we can't see from this repo.

## The core rule

> **Any change to the public surface of a service MUST update that service's wiki page in the same PR.** No exceptions.

This applies to every service in `docs/wiki/`, not just auth.

## What counts as a "public surface" change

Update the wiki when you modify any of:

1. **Interface / method signatures** on `I*Service`, `I*Repository`, `I*Handler`, or any abstraction registered in DI.
2. **Behaviour** of a public method — side effects, error contract, retry semantics, caching behaviour, eventing.
3. **DI registration** — `Add*` extension methods, service lifetimes (Scoped / Singleton / Transient), registration ordering, new hosted services.
4. **Configuration** — any key under `Features:*`, `ConnectionStrings:*`, `App:*`, `UI:*`; renaming or changing a default is a breaking change.
5. **`MessageKeys`** added, renamed, or removed in `Shared/Constants/MessageKeys.cs`.
6. **Claim names, policy names, roles, permission codes** — anything that shows up on an `AuthenticationTicket` or an `[Authorize]` attribute.
7. **Middleware** — new middleware, reordering of existing middleware in any `Program.cs`.
8. **Background jobs** — new `IHostedService`, new schedule, new queue.
9. **REST contract** — route, HTTP verb, request DTO, response DTO, error shape. Particularly sensitive: SPA / mobile / desktop clients see only this surface.
10. **Entity column mappings** when they leak into DTOs, stored-procedure params, or API responses.

If you're not sure — update the wiki. Lower cost than leaving it stale.

## What the wiki update must include

Every update should cover:

- **Signature diff** — new method signature(s) or config shape.
- **Config diff** — added or changed keys, with defaults.
- **Working sample** — a minimal snippet (ideally ≤20 lines) showing the new behaviour.
- **`MessageKey` additions** — list any new `MessageKeys.*` so translators can seed them.
- **Cross-client note** — explicitly call out impact on consumers outside the web app:
  - Angular / React: DTO field changes, new required headers, JWT claim shape
  - MAUI / Windows: same plus offline caching / retry implications
- **`CHANGELOG.md` entry** under the next unreleased version.

## Special: auth

Auth changes are extra-sensitive because they affect every client type (cookie for web, JWT for SPA / mobile / desktop).

Scope — touching any of these triggers the rule:

- `Application/Services/IAuthService.cs` + `Infrastructure/Services/AuthService.cs`
- `ITokenService` / `TokenService` (JWT issuance + validation)
- `IPasswordHasher` / `PasswordHasher` (hash algorithm or iteration count)
- `Features:Authentication:Jwt:*` settings
- Cookie auth wiring in `Public/Program.cs` or `Admin/Program.cs`
- Login / Register / Refresh / Reset / Verify page models under `Public/Pages/Account/` or `Admin/Pages/Account/`

Wiki targets: `docs/wiki/14-auth-service.md` (create if missing) **and** `docs/wiki/06-multi-tenant-login-flow.md` if the login flow itself changed.

## If no wiki page exists yet

Create it in the same PR, using the standard structure:

1. **Purpose** — one paragraph on what the service is for.
2. **Architecture** — component table: interface, impl, files.
3. **DI Registration** — exact `Add*` call + lifetime.
4. **Quick Start** — smallest working example.
5. **Method Reference** — each public method with a sample.
6. **Provider / Client Swap** — where it matters (DB providers, cache backends, JWT vs cookie, etc.).
7. **Common Mistakes** — real pitfalls from PR history.
8. **See Also** — links to related wiki pages.

## Commit discipline

Code change + wiki change go in the **same commit**, or adjacent commits on the same PR. A PR that modifies a service's public surface without touching its wiki page should be flagged in review and blocked from merge.

## Wiki index

Maintained in [`CLAUDE.md`](../CLAUDE.md). When you add a wiki page, add a row to that index.
