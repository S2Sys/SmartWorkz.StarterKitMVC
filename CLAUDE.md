# CLAUDE.md

Project memory for Claude Code sessions. Standing rules here apply to every session.

## Wiki is a living contract

`docs/wiki/` has one page per completed service. When you modify the public surface of a service (new method, changed signature, new config key, new failure `MessageKey`, new claim), update its wiki page in the **same PR**. Stale wiki is worse than no wiki.

### Auth service — explicit rule

Any change to auth-related code requires updating `docs/wiki/14-auth-service.md` in the same PR. Scope:

- `Application/Services/IAuthService.cs` + `Infrastructure/Services/AuthService.cs`
- `ITokenService` / `TokenService`, `IPasswordHasher` / `PasswordHasher`
- `Features:Authentication:Jwt:*` configuration
- Cookie auth wiring in `Public/Program.cs` / `Admin/Program.cs`
- Login / Register / Refresh / Reset / Verify pages under `Public/Pages/Account/` or `Admin/Pages/Account/`

The update must include: changed signature, config diff, a minimal working sample, any new `MessageKey`, and a `CHANGELOG.md` entry under the next release. If login flow changed, also update `docs/wiki/06-multi-tenant-login-flow.md`.

## Other project memory

- [memory/MEMORY.md](memory/MEMORY.md) — index of long-form notes
- [memory/wiki_update_rules.md](memory/wiki_update_rules.md) — full wiki-update policy
- [memory/release_workflow.md](memory/release_workflow.md) — semver + changelog + tag workflow
- [memory/missing_stored_procedures.md](memory/missing_stored_procedures.md) — historical SP gap audit
