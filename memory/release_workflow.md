---
name: Release Workflow & Versioning Process
description: How to manage releases with changelog, README, and component versioning
type: feedback
---

# Release Workflow for SmartWorkz StarterKitMVC

**Why:** Consistent, professional release management ensures developers understand what changed, can track component versions, and can reference specific releases long-term.

**How to apply:** Follow this workflow for every merge/release to main branch.

## Semantic Versioning (SemVer)

Format: `MAJOR.MINOR.PATCH` (e.g., v4.1.0)

- **Major (4.x.x):** Breaking changes, major architecture shift
- **Minor (4.1.x):** New features, new documentation, new demo pages
- **Patch (4.1.z):** Bug fixes, typo corrections, refactoring

## Release Process (Step-by-Step)

### 1. Feature Development
- Work on feature branch (e.g., `feature/demo-pages`)
- Commit regularly with descriptive messages
- Ensure clean build: `dotnet build` → 0 errors

### 2. Update Changelog

Before merging to main, update `CHANGELOG.md`:

```markdown
## [4.1.0] - 2026-04-02

### Added - Demo Pages & Documentation

#### Public Demo Pages
- **Translations Demo** (`/Demo/Translations`) — description
  - Feature point 1
  - Feature point 2
  - References: [Wiki link]

#### Comprehensive Wiki Documentation (X files, XXXX lines)
- **01-file.md** (XXX lines)
  - Topic 1
  - Topic 2

### Commits
**abc1234:** `type: Message here`
- File changes

### Component Versions (Minor Bump)
| Component | Version | Status |
|-----------|---------|--------|
| Translation System | v1.1.0 | ✅ Complete |
| New Feature | v1.0.0 | ✅ New |
```

### 3. Update README.md

Add version to header:
```markdown
# SmartWorkz StarterKitMVC v4.1.0

Latest: **v4.1.0** (2026-04-02) — Demo pages & wiki docs | [Changelog](CHANGELOG.md)
```

Add component versions table:
```markdown
## Component Versions (v4.1.0)

| Component | Version | Status | Reference |
|-----------|---------|--------|-----------|
| Translation System | v1.1.0 | ✅ Complete | [Wiki](docs/wiki/01...) |
| New Feature | v1.0.0 | ✅ New | [Demo](path) |
```

### 4. Create Release Notes Document

Create `RELEASE-v4.1.0.md` with:
- Release summary
- What's new
- Component versions table
- Commits included
- How to access features
- Build & testing status
- Next steps

### 5. Create Git Commits

#### Commit 1: Feature Code
```bash
git commit -m "feat: Add Validation demo page with MessageKey error messages

- Public/Pages/Demo/Validation.cshtml.cs — Form with validation
- Public/Pages/Demo/Validation.cshtml — View with docs
- Fixed Translations.cshtml.cs to use correct constants

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```

#### Commit 2: Documentation
```bash
git commit -m "docs: Add comprehensive wiki documentation for patterns

- 5 wiki files (1951 lines total)
- 01-translation-system.md (392 lines)
- 02-localized-validation.md (318 lines)
- 03-base-page-pattern.md (305 lines)
- 04-result-pattern.md (349 lines)
- 05-htmx-list-pattern.md (387 lines)

Each includes: Purpose, Quick Start, How It Works, Patterns, Mistakes

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```

#### Commit 3: Release
```bash
git commit -m "chore: Release v4.1.0 with changelog and updated README

- Created CHANGELOG.md with detailed release notes
- Updated README.md with v4.1.0 header
- Added component versions table
- Component versions: all patterns bumped to v1.1.0

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```

#### Commit 4: Release Notes
```bash
git commit -m "docs: Add detailed v4.1.0 release notes document

- Created RELEASE-v4.1.0.md
- Comprehensive release documentation
- Deliverables, versions, commits, how to access

Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>"
```

### 6. Create Git Tag

```bash
git tag -a v4.1.0 -m "Release 4.1.0: Demo pages and comprehensive wiki documentation

v4.1.0 includes:
- Public demo pages (Translations, Validation)
- 5 wiki guides (1951 lines)
- Component versions bumped to v1.1.0
- CHANGELOG.md with semantic versioning
- Updated README.md with component table

Build: 0 Errors, 399 Warnings"
```

### 7. Verification

Before considering release complete:
- ✅ `dotnet build` → 0 errors
- ✅ Demo pages render correctly
- ✅ Wiki files are valid Markdown
- ✅ CHANGELOG.md follows Keep a Changelog format
- ✅ README.md is consistent
- ✅ Git tag created: `git tag -l v4.1.0`
- ✅ All commits are on main branch

## Component Versioning Strategy

Each major component tracks its own version:

```
Translation System v1.1.0
├─ v1.0.0 = Initial implementation
├─ v1.1.0 = Added wiki documentation (minor bump)
└─ v2.0.0 = Breaking change to interface (major bump)

Result Pattern v1.1.0
├─ v1.0.0 = Initial implementation
├─ v1.1.0 = Added wiki documentation (minor bump)
└─ v2.0.0 = Breaking change (major bump)
```

### Bumping Rules

- **Patch (v1.0.1):** Bug fix, typo, refactoring
- **Minor (v1.1.0):** New feature, new documentation, new example
- **Major (v2.0.0):** Breaking change to interface or behavior

### When to Bump

| Type | Example | Action |
|------|---------|--------|
| Bug Fix | Fixed null reference in T() | → Patch (v1.0.1) |
| New Feature | Added cache warmup method | → Minor (v1.1.0) |
| New Docs | Added wiki guide for pattern | → Minor (v1.1.0) |
| New Demo | Added demo page | → Minor (v1.1.0) |
| Interface Change | Renamed T() to Translate() | → Major (v2.0.0) |

## Release Naming Convention

```
RELEASE-v4.1.0.md

Pattern: RELEASE-v{MAJOR}.{MINOR}.{PATCH}.md
```

Contains:
- Release summary
- What's new
- Component versions
- Commits included
- Build status
- How to access

## References in Documentation

When linking to version-specific features in CHANGELOG or README:

```markdown
// ✅ Good - includes version
| Translation System | v1.1.0 | ✅ Complete | [Wiki](docs/wiki/01...) |

// ❌ Bad - no version
| Translation System | Complete | [Wiki](docs/wiki/01...) |
```

When referencing commits:

```markdown
// ✅ Good - includes hash and message
**abc1234:** `feat: Add Validation demo page`

// ❌ Bad - no hash
Commit: "Add Validation demo page"
```

## After Release

1. **Announce changes** → Commit message, git tag, release notes
2. **Archive old docs** → Move v3.x docs to `docs/old/` if major version change
3. **Update memory** → Record what worked, what didn't
4. **Plan next release** → v4.2.0 target features, effort estimate

## Examples from v4.1.0

- **Feature commit:** `3a8badd` — `feat: Add Validation demo page`
- **Docs commit:** `31224e4` — `docs: Add comprehensive wiki documentation`
- **Release commit:** `1e0eb9c` — `chore: Release v4.1.0`
- **Notes commit:** `70909b7` — `docs: Add detailed v4.1.0 release notes`
- **Git tag:** `v4.1.0` — Full release tag
- **Files changed:** 9 new, 2 modified
- **Documentation:** 3 docs files (CHANGELOG, README, RELEASE notes)
- **Wiki:** 5 comprehensive guides (1951 lines)

## Key Principles

1. **Changelog first** — Update before merge to ensure clarity
2. **Component versions** — Track each pattern's evolution independently
3. **Semantic versioning** — Major.Minor.Patch convention
4. **Professional tone** — Release notes are developer-facing docs
5. **Git tags** — Create for every release, include full description
6. **Memory** — Record workflow in this file for consistency
