# Changelog

All notable changes to SmartWorkz StarterKitMVC are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

## [4.1.0] - 2026-04-02

### Added - Demo Pages & Documentation

#### Public Demo Pages
- **Translations Demo** (`/Demo/Translations`) — Live translation system showcase
  - Displays all `MessageKeys` constants with current translated values
  - Supports multi-locale translation viewing
  - References: [Wiki: Translation System](docs/wiki/01-translation-system.md)

- **Validation Demo** (`/Demo/Validation`) — Validation attribute showcase
  - Demonstrates `[Required]`, `[EmailAddress]`, `[StringLength]`, `[Range]`, `[RegularExpression]`, `[Compare]`
  - All messages use `MessageKey` constants for translation
  - Shows client-side and server-side validation flow
  - References: [Wiki: Localized Validation](docs/wiki/02-localized-validation.md)

#### Comprehensive Wiki Documentation (5 files, 1951 lines)
- **01-translation-system.md** (392 lines)
  - Architecture, core components, data flow
  - `ITranslationService`, `T()` helper, `MessageKeys` constants
  - How to add new translation keys
  - Multi-locale support and locale determination
  - Customization: per-tenant overrides, cache warmup
  - Common mistakes and debugging
  - Status: Translation system fully documented ✅

- **02-localized-validation.md** (318 lines)
  - Simple validation approach using `MessageKey` error messages
  - Validation attribute usage patterns
  - Client-side vs server-side validation behavior
  - How custom attributes work
  - Customization: adding new validation types
  - Common patterns (required, email, password confirmation)
  - Status: Validation system fully documented ✅

- **03-base-page-pattern.md** (305 lines)
  - `BasePage` foundation and initialization
  - Properties: `TenantId`, `User`, `CurrentUserId`, `CurrentUserEmail`, `IsAuthenticated`
  - Methods: `T()`, `AddToastSuccess()`, `AddToastError()`, `AddErrors()`
  - Specializations: `SeoBasePage`, `BaseListPage<T>`
  - Common patterns for tenant-aware pages
  - Status: Base page pattern fully documented ✅

- **04-result-pattern.md** (349 lines)
  - `Result` and `Result<T>` classes for explicit success/failure
  - Architecture and quick start examples
  - Patterns: validation, dependency chains, multiple errors, data return
  - Comparison with exception-based error handling
  - Common patterns with repositories and services
  - Status: Result pattern fully documented ✅

- **05-htmx-list-pattern.md** (387 lines)
  - HTMX integration for dynamic list updates
  - Architecture: form submission → HTMX detection → partial response
  - Quick start: search form + list container + partial template
  - HTMX attributes: `hx-get`, `hx-target`, `hx-trigger`, `hx-swap`, `hx-select`, `hx-boost`
  - Common patterns: search, filter+pagination, load more, polling
  - Progressive enhancement (works with/without JavaScript)
  - Status: HTMX list pattern fully documented ✅

### Commits

**3a8badd:** `feat: Add Validation demo page with MessageKey error messages`
- Public/Pages/Demo/Validation.cshtml.cs — Form with 6 input fields demonstrating validation types
- Public/Pages/Demo/Validation.cshtml — View with form and developer documentation
- Fixed Translations.cshtml.cs to use correct MessageKeys constants

**31224e4:** `docs: Add comprehensive wiki documentation for starter kit patterns`
- 5 wiki files (1951 lines total)
- Purpose, Quick Start, How It Works, Patterns, Common Mistakes sections
- Each doc includes "See Also" cross-references

### Changed
- Translations demo: Fixed `MessageKeys.Validation.StringLength` → `MessageKeys.Validation.MinLength` (correct constant)

### Build Status
- ✅ 0 Errors, 399 Warnings (unrelated infrastructure)
- ✅ All demo pages render correctly
- ✅ All wiki files committed and accessible

### Component Versions (Minor Bump)

| Component | Version | Status |
|-----------|---------|--------|
| Translation System | v1.1.0 | ✅ Complete with docs |
| Localized Validation | v1.1.0 | ✅ Complete with docs |
| Base Page Pattern | v1.1.0 | ✅ Complete with docs |
| Result Pattern | v1.1.0 | ✅ Complete with docs |
| HTMX List Pattern | v1.1.0 | ✅ Complete with docs |
| Public Demo Pages | v1.0.0 | ✅ New in 4.1.0 |
| Wiki Documentation | v1.0.0 | ✅ New in 4.1.0 |

---

## [4.0.0] - 2026-03-XX

### Added
- Multi-tenant architecture with clean separation
- Database-backed translation system with `ITranslationService`
- Simple validation using `MessageKey` constants
- `BasePage` pattern with `TenantId`, `T()`, user context
- `Result` and `Result<T>` pattern for service outcomes
- HTMX integration for dynamic list updates
- Admin Products CRUD sample pages
- Public Product Catalog (Index, Details)
- Pagination helper factory (`PaginationModel.FromDto`)
- Password reset email integration
- Comprehensive SQL schema (5 schemas, 42 LEAN tables)

### Technical Stack
- .NET 9 with Entity Framework Core
- SQL Server with HierarchyId for hierarchies
- Bootstrap 5 for responsive UI
- HTMX for progressive enhancement
- Dapper for stored procedures (optional)

### Documentation
- Schema design with Option C Hybrid Geo
- Implementation roadmap (4 phases)
- Menu system design guide
- Database setup scripts

---

## How to Use This Changelog

When merging a feature branch:

1. **Update CHANGELOG.md:**
   - Add new `[X.Y.Z]` section under `[Unreleased]`
   - List Added, Changed, Removed, Fixed sections
   - Include commit hashes and file references
   - Bump component versions (minor version for features, patch for fixes)

2. **Update README.md:**
   - Add component to version table
   - Link to wiki or demo page if applicable
   - Update feature summary if needed

3. **Create commit:**
   ```bash
   git commit -m "chore: Release v4.1.0 with demo pages and wiki docs"
   ```

4. **Create git tag (optional):**
   ```bash
   git tag -a v4.1.0 -m "Release 4.1.0: Demo pages and comprehensive wiki documentation"
   git push origin v4.1.0
   ```

---

## Version Numbering Scheme

- **Major (X):** Breaking changes or major architecture shift
- **Minor (Y):** New features, new documentation, new demo pages
- **Patch (Z):** Bug fixes, typo corrections, refactoring

### Component-Level Versioning

Each major component tracks its own version:
- Translation System v1.1.0
- Base Page Pattern v1.1.0
- Result Pattern v1.1.0
- HTMX List Pattern v1.1.0
- Localized Validation v1.1.0
- Public Demo Pages v1.0.0

When a component reaches v2.0.0, it indicates breaking changes to that component's interface.

---

**SmartWorkz StarterKitMVC** — Enterprise-grade .NET starter kit with comprehensive documentation
