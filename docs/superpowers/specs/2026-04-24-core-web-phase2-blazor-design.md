# SmartWorkz.Core.Web Phase 2: Blazor Support Design Specification

**Date:** 2026-04-24  
**Status:** Design (Ready for Planning)  
**Phase:** 2 of 3  
**Scope:** Add Blazor Server/WebAssembly component support to Core.Web  
**Timeline:** 2-3 weeks (estimated)

---

## Executive Summary

Phase 1 delivered a robust MVC/Razor Pages foundation with GridComponent, validation services, and tag helpers. Phase 2 will extend Core.Web to support Blazor Server and WebAssembly applications by:

1. **Reusing Phase 1 abstractions** — Validation services, models, base classes remain in SmartWorkz.Core.Web root namespace
2. **Adding Blazor-specific components** — GridComponent adapted for Blazor with event callbacks and two-way binding
3. **Blazor interop services** — JavaScript interop utilities for DOM access, form handling
4. **Namespace strategy** — Blazor-specific code lives in no special namespace for Phase 2 (can split to SmartWorkz.Core.Web.Blazor in Phase 3 if needed)

**Key Principle:** Maximize code reuse from Phase 1. Blazor components will inherit from Phase 1 base classes and use Phase 1 models/services.

---

## Current State (Phase 1 Complete)

### What Exists
```
SmartWorkz.Core.Web/
├── src/
│   ├── Components/
│   │   ├── BaseRazorComponent.cs       ✅ Reusable
│   │   └── GridComponent.razor         ✅ MVC version
│   ├── Services/
│   │   ├── IValidationService.cs       ✅ Reusable
│   │   └── ValidationService.cs        ✅ Reusable
│   ├── TagHelpers/                     ✅ Reusable (MVC-only, tag helpers don't exist in Blazor)
│   ├── Models/
│   │   ├── SortOrder.cs                ✅ Reusable
│   │   ├── GridColumn.cs               ✅ Reusable
│   │   └── GridOptions.cs              ✅ Reusable
│   └── Extensions/
│       └── ValidationExtensions.cs     ✅ Reusable
└── tests/ — 14 tests, all passing
```

### What's Reusable for Blazor
- ✅ BaseRazorComponent (base class for Blazor components)
- ✅ IValidationService + ValidationService (same validation logic)
- ✅ GridColumn, GridOptions, SortOrder models
- ✅ ValidationExtensions

### What Needs Blazor Adaptation
- ❌ GridComponent.razor — MVC version uses ASP.NET Razor syntax, not Blazor (@onclick won't work the same way)
- ❌ Tag helpers — Don't exist in Blazor (use components instead)
- ❌ Form validation display — MVC uses ModelState; Blazor uses component state

---

## Phase 2 Design: What to Build

### New Blazor Components (Adapt from Phase 1)

**1. BlazorGridComponent** — Blazor version of GridComponent
- Inherits from BaseRazorComponent
- Supports two-way binding for sort/filter state
- Event callbacks: OnSortChanged, OnFilterChanged, OnPageChanged, OnRowSelected
- Virtual scrolling support (uses Virtualize component from Blazor)
- Renders as HTML table, not tag helpers
- Supports custom column templates via RenderFragment

**2. BlazorFormComponent** — Form wrapper with validation display
- Inherits from BaseRazorComponent
- Uses EditForm + DataAnnotationsValidator
- Displays validation errors inline
- Callback: OnValidSubmit, OnInvalidSubmit

**3. BlazorStatusBadge** — Blazor component version of status badge tag helper
- Status parameter
- ShowIcon parameter
- CSS class customization
- Renders as <span class="badge">

### Reuse Services Directly
- ✅ IValidationService — No changes needed, works in Blazor
- ✅ ValidationService — No changes needed
- ✅ ValidationExtensions — No changes needed

### Test Strategy
- Bunit tests for each Blazor component (same framework as Phase 1)
- Component rendering tests
- Event callback tests
- Two-way binding tests
- Same TDD approach as Phase 1

---

## Phase 2 Scope & Decisions

### In Scope (MVP)
- [ ] BlazorGridComponent with sorting/filtering/pagination
- [ ] BlazorFormComponent with validation
- [ ] BlazorStatusBadge component
- [ ] 10+ unit tests with Bunit
- [ ] Documentation update showing Blazor examples
- [ ] Example Blazor pages (grid, form, combined)

### Out of Scope (Phase 3)
- [ ] Data binding helpers for nested objects
- [ ] Advanced filtering UI
- [ ] Virtual scrolling optimization (can use Blazor's Virtualize directly)
- [ ] Blazor-specific styling library
- [ ] Separate SmartWorkz.Core.Web.Blazor namespace (can defer if single DLL works)

### Architecture Decisions

**Decision 1: Single DLL or Separate Blazor Project?**
- **Choice:** Single DLL (SmartWorkz.Core.Web)
- **Why:** Code reuse is easier; consumers just `@using SmartWorkz.Core.Web.Components`
- **Tradeoff:** Blazor consumers get all MVC tag helpers (ignorable) but cleaner consuming experience
- **Alternative:** Separate DLL if binary size becomes concern (Phase 3 decision)

**Decision 2: Blazor GridComponent Inheritance**
- **Choice:** Separate component (BlazorGridComponent), not rename existing GridComponent
- **Why:** MVC version uses Razor syntax optimized for server-side; Blazor needs event callbacks
- **Consumer:** Both exist; developers choose which fits their platform
- **Future:** Could consolidate in Phase 3 if shared logic is factored out

**Decision 3: Validation Service Reuse**
- **Choice:** Same IValidationService/ValidationService used by Blazor
- **Why:** Validation logic is platform-agnostic; DataAnnotations work identically in Blazor
- **Benefit:** Reduces duplication, single source of truth

---

## Phase 2 Implementation Roadmap (2-3 weeks)

### Week 1: Blazor Components
- Day 1-2: BlazorGridComponent with tests (sorting/filtering/pagination)
- Day 3-4: BlazorFormComponent with validation integration
- Day 5: BlazorStatusBadge component

### Week 2: Testing & Documentation
- Day 1-2: Comprehensive Bunit tests (10+ tests)
- Day 3-4: Example Blazor pages (grid, form, combined)
- Day 5: Update documentation, Phase 2 completion guide

### Week 3: Polish (Optional, depends on testing feedback)
- Performance optimizations
- Edge case handling
- Additional examples
- Blazor Server vs. WebAssembly testing

---

## Phase 2 File Structure (New Files)

```
src/
├── Components/
│   ├── GridComponent.razor             (Phase 1 - MVC)
│   ├── GridComponent.razor.cs          (Phase 1 - MVC)
│   ├── BlazorGridComponent.razor        🆕 Blazor
│   ├── BlazorGridComponent.razor.cs     🆕 Blazor
│   ├── BlazorFormComponent.razor        🆕 Blazor
│   ├── BlazorFormComponent.razor.cs     🆕 Blazor
│   ├── BlazorStatusBadge.razor          🆕 Blazor
│   └── BaseRazorComponent.cs            (Phase 1 - Shared)
│
├── Services/                            (All Phase 1 - Shared, no changes)
└── Models/                              (All Phase 1 - Shared, no changes)

tests/
├── Components/
│   ├── BaseRazorComponentTests.cs       (Phase 1)
│   ├── GridComponentTests.cs            (Phase 1)
│   ├── BlazorGridComponentTests.cs      🆕 Phase 2
│   ├── BlazorFormComponentTests.cs      🆕 Phase 2
│   └── BlazorStatusBadgeTests.cs        🆕 Phase 2
└── Services/
    └── ValidationServiceTests.cs        (Phase 1)

docs/
├── examples/
│   ├── GridExample.razor                (Phase 1 - MVC)
│   ├── FormExample.razor                (Phase 1 - MVC)
│   ├── BlazorGridExample.razor          🆕 Phase 2
│   ├── BlazorFormExample.razor          🆕 Phase 2
│   └── BlazorCombinedExample.razor      🆕 Phase 2
└── BLAZOR-GUIDE.md                      🆕 Phase 2
```

---

## Phase 2 Key Components: Detailed Specs

### BlazorGridComponent

**File:** `src/Components/BlazorGridComponent.razor`

**Parameters:**
- `Data` (IEnumerable<TItem>) — Data source
- `Columns` (IEnumerable<GridColumn>) — Column definitions
- `Options` (GridOptions) — Grid config
- `TItem` (type parameter) — Data item type

**Callbacks:**
- `OnSortChanged` (EventCallback<(string column, SortOrder order)>)
- `OnFilterChanged` (EventCallback<string>)
- `OnPageChanged` (EventCallback<int>)
- `OnRowSelected` (EventCallback<TItem>)

**Features:**
- Two-way binding for sort/filter state
- Pagination with Next/Previous buttons
- Column sorting with visual indicators (↑ ↓)
- Text filtering across columns
- Custom column templates via RenderFragment
- Bootstrap 5 styling
- Bunit testable

**Implementation Notes:**
- Use EditContext for validation if integrated with forms
- Event callbacks for all state changes (parent controls grid state)
- Supports Blazor Server and WebAssembly

### BlazorFormComponent

**File:** `src/Components/BlazorFormComponent.razor`

**Parameters:**
- `Model` (TModel) — Data model
- `OnValidSubmit` (EventCallback<TModel>)
- `OnInvalidSubmit` (EventCallback)

**Features:**
- Uses Blazor EditForm + DataAnnotationsValidator
- Validation error display
- Form submission callbacks
- Integration with IValidationService for pre-validation

### BlazorStatusBadge

**File:** `src/Components/BlazorStatusBadge.razor`

**Parameters:**
- `Status` (string)
- `ShowIcon` (bool)
- `CssClass` (string)

**Features:**
- Status-based styling (same logic as Phase 1 tag helper)
- Optional icon display
- Bootstrap badge styling

---

## Phase 2 Success Criteria

✅ **Deliverables**
- [ ] BlazorGridComponent fully implemented and tested
- [ ] BlazorFormComponent fully implemented and tested
- [ ] BlazorStatusBadge implemented and tested
- [ ] 10+ Bunit tests, all passing
- [ ] 3+ Blazor example pages
- [ ] Documentation updated with Blazor guide

✅ **Quality**
- [ ] All new tests pass (Bunit)
- [ ] Overall test coverage maintained 70%+
- [ ] Clean Release build (0 errors)
- [ ] NuGet package updated to v1.1.0
- [ ] No breaking changes to Phase 1 APIs

✅ **Code Reuse**
- [ ] Validation services reused (no duplication)
- [ ] Base classes reused
- [ ] Models reused
- [ ] 0 code duplication between MVC and Blazor versions

---

## Risk Analysis & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Event callback complexity | Medium | Medium | Thorough Bunit testing, clear documentation |
| State management confusion | Medium | High | Simple state handling; parent controls grid |
| Performance with large datasets | Medium | Medium | Recommend Virtualize; test with 10K+ rows |
| Blazor Server vs. WebAssembly differences | Low | Medium | Test on both; document differences |

---

## Phase 2 → Phase 3 Considerations

**For future planning:**
- Consider separate SmartWorkz.Core.Web.Blazor namespace if DLL size grows
- Advanced data binding helpers (nested objects, custom types)
- Performance optimizations for large grids (virtual scrolling)
- Blazor-specific styling/theming system
- Integration with Core.Shared patterns (CQRS queries, Caching)

---

## Questions for Alignment

1. **Blazor Server, WebAssembly, or Both?** (Recommend: Both, test on both)
2. **Should BlazorFormComponent handle entire form layout, or just validation display?** (Recommend: Just validation, let consumer build form)
3. **Separate namespace for Blazor or keep in root?** (Recommend: Root for Phase 2, separate in Phase 3)
4. **Virtualization priority?** (Recommend: Use Blazor's built-in Virtualize; Phase 2 doesn't need special optimization)

---

## Success Metrics (After Phase 2)

- SmartWorkz.Core.Web v1.1.0 ships
- Both MVC and Blazor developers can use Core.Web
- 100% code reuse for validation/models
- 24+ unit tests total
- 70%+ overall coverage
- Ready for production use in Blazor applications
- Clear examples showing both platforms
