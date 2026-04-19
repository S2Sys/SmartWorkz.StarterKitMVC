# CLAUDE.md

Project memory for Claude Code sessions. Standing rules here apply to every session.

## Project positioning — this is a framework

SmartWorkz.StarterKitMVC is being built as a **multi-client framework**, not a single web app. Consumers include:

- **Web**: ASP.NET Core MVC + Razor Pages (Admin / Public), Blazor
- **SPA front-ends**: Angular, React (consuming REST / JWT)
- **Mobile**: .NET MAUI (iOS / Android)
- **Desktop / Windows**: WPF, WinForms, WinUI (consuming REST / JWT)

Every decision should preserve that. Concretely:

- Keep **Application** and **Domain** layers free of ASP.NET types. Web-only concerns (HttpContext, Razor Pages, cookies) live in `Web` / `Public` / `Admin`.
- Expose every service through a REST endpoint when it has non-web consumers. Don't bury logic inside a Razor PageModel.
- Favour **DI-friendly, interface-first** designs so clients can wire alternative implementations (in-memory for tests, provider swaps for Oracle/Postgres, offline cache for mobile, etc.).
- Auth must work with **both** cookie (web) and JWT bearer (SPA / mobile / desktop) schemes. Never hard-code one.
- Config keys in `appsettings.json` are part of the public surface — rename = breaking change.

## Wiki is a living contract

`docs/wiki/` has one page per completed service. **Any touch to the public surface of a service MUST update that service's wiki page in the same PR** — no exceptions. This applies to every service, not just auth. Stale wiki is worse than no wiki.

### What counts as a "touch"

Update the wiki when you change any of:

- A method signature on a public interface (`I*Service`, `I*Repository`)
- A public method's behaviour, side effects, or error contract
- A DI registration (`Add*` extension, lifetime, ordering)
- A config key or default under `Features:*`, `ConnectionStrings:*`, or `App:*`
- A `MessageKey` added, renamed, or removed in `Shared/Constants/MessageKeys.cs`
- A claim name, policy name, or authorization requirement
- Middleware order in any `Program.cs`
- A new `IHostedService`, background job, or scheduled task
- The REST contract (route, verb, request/response DTO) of an API endpoint
- An entity's column mapping if it leaks into DTOs or stored-procedure params

### What the wiki update must include

- **Signature diff** — the new method signature or config shape
- **Config diff** — any added/changed keys + defaults
- **Working sample** — a minimal snippet showing the new behaviour
- **MessageKey additions** — list any new `MessageKeys.*` introduced
- **Cross-client note** — if the change affects SPA / mobile / desktop consumers (e.g. JWT claim shape, DTO field, REST route), say so explicitly
- **`CHANGELOG.md` entry** under the next release

### Wiki-page index (update this list when pages are added)

| Area | Wiki page |
|------|-----------|
| Setup / DI wiring | `00-getting-started.md` |
| Translation system | `01-translation-system.md` |
| Localized validation | `02-localized-validation.md` |
| Base page pattern | `03-base-page-pattern.md` |
| Result pattern | `04-result-pattern.md` |
| HTMX list pattern | `05-htmx-list-pattern.md` |
| Multi-tenant login flow | `06-multi-tenant-login-flow.md` |
| TenantId in multiple tables | `07-why-tenantid-in-multiple-tables.md` |
| Dapper repository | `10-dapper-repository.md` |
| EF Core repository | `11-ef-core-repository.md` |
| Hybrid cache | `12-hybrid-cache.md` |
| Email templates | `13-email-templates.md` |
| Auth service *(pending)* | `14-auth-service.md` |
| Permission service *(pending)* | `15-permission-service.md` |
| Claim service *(pending)* | `16-claim-service.md` |
| Menu service *(pending)* | `17-menu-service.md` |
| SEO meta service *(pending)* | `18-seo-meta-service.md` |
| Tag service *(pending)* | `19-tag-service.md` |
| Middleware stack *(pending)* | `20-middleware-stack.md` |
| Background jobs *(pending)* | `21-background-jobs.md` |
| Shared primitives *(pending)* | `22-shared-primitives.md` |
| Tag helpers + validators *(pending)* | `23-tag-helpers-validators.md` |

When you create a new service or a new wiki page, add the row here so future sessions know it exists.

### If no page exists yet

If you're touching a service that has no wiki page yet, **create the page in the same PR** using the standard structure: **Purpose → Architecture → DI Registration → Quick Start → Method Reference → Provider/Client Swap (where relevant) → Common Mistakes → See Also**.

### PR review signal

A PR that modifies a service's public surface without touching its wiki page should be flagged in review and blocked from merge.

## Other project memory

- [memory/MEMORY.md](memory/MEMORY.md) — index of long-form notes
- [memory/wiki_update_rules.md](memory/wiki_update_rules.md) — full wiki-update policy
- [memory/release_workflow.md](memory/release_workflow.md) — semver + changelog + tag workflow
- [memory/missing_stored_procedures.md](memory/missing_stored_procedures.md) — historical SP gap audit
