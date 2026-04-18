# SmartWorkz StarterKitMVC - Shared Components & Services Design

**Date:** 2026-04-18  
**Scope:** TagHelpers + Supporting Infrastructure for 99% Web Coverage  
**Owner:** Architecture Team  
**Status:** Design Review

---

## 1. Executive Summary

This design introduces a comprehensive component library in the **Shared project** to eliminate boilerplate across Web, Admin, and Public apps. We will create **35 TagHelpers** organized by function, plus **8 supporting services** that handle caching, globalization, validation, accessibility, and icons.

**Phase 1 (MUST HAVE):** Form foundation + display basics (16 components)  
**Phase 2 (GOOD TO HAVE):** Enhanced UX + advanced forms (13 components)  
**Phase 3 (NICE TO HAVE):** Specialized components (6 components)

---

## 2. Architecture

### 2.1 Project Structure

```
SmartWorkz.StarterKitMVC.Shared/
├── TagHelpers/
│   ├── Forms/                          (form controls & validation)
│   ├── Display/                        (alerts, badges, pagination, lists)
│   ├── Data/                           (tables, sorting)
│   ├── Layout/                         (cards, modals, tabs, accordion)
│   ├── Navigation/                     (menus, breadcrumbs)
│   └── Common/                         (buttons, icons, links, spinners)
├── Services/
│   ├── Caching/                        (existing: extend with component keys)
│   ├── Resources/                      (existing: extend for UI strings)
│   ├── Globalization/                  (existing: extend for locale-aware components)
│   ├── Notifications/                  (existing: extend with toast variants)
│   └── Components/                     (NEW: icon, validation, form, accessibility)
└── Utilities/
    └── ComponentHelpers.cs             (shared formatting, sanitization)
```

### 2.2 Integration Points

Each app registers TagHelpers in `_ViewImports.cshtml`:

```csharp
@addTagHelper *, SmartWorkz.StarterKitMVC.Shared
@inject IIconProvider IconProvider
@inject IValidationMessageProvider ValidationMessages
@inject IResourceProvider ResourceProvider
@inject IGlobalizationService GlobalizationService
@inject IAccessibilityService AccessibilityService
```

Then use components:

```html
<!-- Forms -->
<form-tag-helper>
  <form-group-tag-helper for="Email" label="Email Address" required="true" />
  <form-group-tag-helper for="Password" label="Password" type="password" />
  <button-tag-helper type="submit" variant="primary">Login</button-tag-helper>
</form-tag-helper>

<!-- Display -->
<alert-tag-helper type="success" message="Login successful!" dismissible="true" />
<badge-tag-helper type="primary" text="Active" />

<!-- Data -->
<table-tag-helper>
  <thead>
    <tr>
      <th><sortable-header-tag-helper column="Name" /></th>
      <th><sortable-header-tag-helper column="Email" /></th>
    </tr>
  </thead>
  <tbody>
    <!-- rows -->
  </tbody>
</table-tag-helper>
```

---

## 3. Phase 1: MUST HAVE (Core Foundation)

**Rationale:** These 16 components and 3 services handle 80% of UI patterns. Every form needs them. Every app uses them.

### 3.1 Form Components (10 TagHelpers)

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **FormTagHelper** | Wraps `<form>` | Auto CSRF token, consistent classes, method helpers |
| **FormGroupTagHelper** | Label + Input + Error | Responsive layout, required indicator (*), tooltips |
| **LabelTagHelper** | Enhanced `<label>` | Required marker, icon support, tooltip integration |
| **InputTagHelper** | Text/email/password/number inputs | Icon prefix/suffix, character counter, placeholder i18n |
| **SelectTagHelper** | Dropdown/multi-select | Enum mapping, option groups, searchable variant |
| **TextAreaTagHelper** | Multi-line input | Character counter, rows/cols, maxlength |
| **CheckboxTagHelper** | Checkbox + label | Inline/stacked, group mode, indeterminate state |
| **RadioButtonTagHelper** | Radio button group | Horizontal/vertical layout, description support |
| **ValidationMessageTagHelper** | Field error display | Inline/tooltip modes, localized messages, icons |
| **FileInputTagHelper** | File upload | Drag-drop, preview, multiple files, size validation |

### 3.2 Display Components (4 TagHelpers)

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **AlertTagHelper** | Bootstrap alert | Types: success/danger/warning/info, dismissible, icon |
| **BadgeTagHelper** | Status badge | Variants: primary/secondary/danger/warning/success |
| **PaginationTagHelper** | Page navigation | Links/buttons, ellipsis, active state |
| **BreadcrumbTagHelper** | Navigation breadcrumbs | Auto active detection, icon support |

### 3.3 Common Components (2 TagHelpers)

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **ButtonTagHelper** | Standardized `<button>` | Variants, sizes, loading state, icons |
| **IconTagHelper** | Centralized icon rendering | Maps `IconType` enum to `bi-*` classes, sizes |

### 3.4 Supporting Services (3 Services)

| Service | Responsibility |
|---------|-----------------|
| **IIconProvider** | Map `IconType.Success` → `bi-check-circle-fill`, manage icon catalog |
| **IValidationMessageProvider** | Provide localized validation messages ("Email format invalid" in user's language) |
| **IFormComponentProvider** | Bootstrap 5 form defaults (input height, button size, spacing) |

### 3.5 Data Flow

```
View Layer                                    Service Layer
┌──────────────────┐
│ <form-group>     │ ──invoke──> FormGroupTagHelper
│   @for="Email"   │                       │
│   @label="Email" │                       ├──> IFormComponentProvider (styling)
└──────────────────┘                       ├──> IAccessibilityService (aria-labels)
                                           └──> IValidationMessageProvider (errors)
```

### 3.6 File Structure - Phase 1

```
SmartWorkz.StarterKitMVC.Shared/
├── TagHelpers/
│   ├── Forms/
│   │   ├── FormTagHelper.cs
│   │   ├── FormGroupTagHelper.cs
│   │   ├── LabelTagHelper.cs
│   │   ├── InputTagHelper.cs
│   │   ├── SelectTagHelper.cs
│   │   ├── TextAreaTagHelper.cs
│   │   ├── CheckboxTagHelper.cs
│   │   ├── RadioButtonTagHelper.cs
│   │   ├── ValidationMessageTagHelper.cs
│   │   └── FileInputTagHelper.cs
│   ├── Display/
│   │   ├── AlertTagHelper.cs
│   │   ├── BadgeTagHelper.cs
│   │   ├── PaginationTagHelper.cs
│   │   └── BreadcrumbTagHelper.cs
│   └── Common/
│       ├── ButtonTagHelper.cs
│       └── IconTagHelper.cs
├── Services/
│   ├── Components/
│   │   ├── IIconProvider.cs
│   │   ├── IconProvider.cs
│   │   ├── IValidationMessageProvider.cs
│   │   ├── ValidationMessageProvider.cs
│   │   ├── IFormComponentProvider.cs
│   │   ├── FormComponentProvider.cs
│   │   ├── IAccessibilityService.cs
│   │   └── AccessibilityService.cs
```

---

## 4. Phase 2: GOOD TO HAVE (Enhanced UX)

**Rationale:** High-value UX improvements; don't block other work.

### 4.1 Form Components (5 TagHelpers)

- **PasswordStrengthTagHelper** — Requirement checklist (your Register page pattern)
- **SearchInputTagHelper** — Search box with icon + clear button (like `_DataTable`)
- **FormSectionTagHelper** — Grouped form sections with borders/headers
- Plus: Enhanced variants of Phase 1 inputs (autocomplete, masks, async validation)

### 4.2 Display Components (5 TagHelpers)

- **EmptyStateTagHelper** — "No data" messaging with icon + action button
- **ListGroupTagHelper** — Styled lists with variants (active, disabled, links)
- **TooltipTagHelper** — Bootstrap tooltips with localized content
- **SortableHeaderTagHelper** — Table header with sort icons + direction
- **LoadingSpinnerTagHelper** — Spinner variants (inline, overlay, pulse)

### 4.3 Layout Components (3 TagHelpers)

- **AccordionTagHelper** — Collapsible accordion groups
- **TabsTagHelper** (enhanced) — Tab navigation with icons + disabled state
- **FormSectionTagHelper** — Section wrappers for grouped form fields

### 4.4 Supporting Services (2 Services)

- **IAccessibilityService** — Generate ARIA labels, describe form sections
- **IComponentConfigurationService** — Theme colors, button styles, spacing rules

---

## 5. Phase 3: NICE TO HAVE (Specialized)

**Rationale:** Lower-frequency components; implement after Phases 1 & 2 stabilize.

- **DatePickerTagHelper** — Date/datetime selection
- **RatingTagHelper** — Star rating component
- **TimelineTagHelper** — Activity/event timelines
- **PopoverTagHelper** — Rich popovers
- **ProgressTagHelper** — Progress bars
- **CurrencyInputTagHelper** — Money input with formatting

---

## 6. Testing Strategy

### 6.1 Unit Tests (Services)

- **IIconProvider:** Verify icon type → CSS class mapping, cache behavior
- **IValidationMessageProvider:** Test localization, fallback messages, custom validators
- **IFormComponentProvider:** Verify Bootstrap 5 defaults applied correctly

### 6.2 Integration Tests (TagHelpers)

- **FormGroupTagHelper:** Generate correct HTML structure (label + input + error), ARIA attributes
- **InputTagHelper:** Render with icon prefix/suffix, validation classes, data attributes
- **ValidationMessageTagHelper:** Display localized error messages, hide when no error

### 6.3 View Tests (Shared App)

Create a reference app demonstrating each component:
- `/Demo/Forms` — All form controls in a single page
- `/Demo/Display` — Alerts, badges, pagination
- `/Demo/Data` — Tables with sorting
- `/Demo/Layout` — Cards, modals, tabs

---

## 7. Success Criteria

✅ **Phase 1 Complete When:**
- All 16 TagHelpers + 3 services implemented and tested
- Web, Admin, Public apps updated to use components
- Form pages refactored (Login, Register, User Forms, Lookup forms)
- Zero hardcoded HTML for common patterns

✅ **Phase 2 Complete When:**
- All 13 additional TagHelpers + 2 services working
- Advanced form patterns (password strength, search, async validation) in place
- Data tables fully refactored with sortable headers, empty states

✅ **Phase 3 Complete When:**
- Specialized components available for optional use
- Component library documentation complete
- Demo app showcases all 35 components

---

## 8. Risk & Mitigation

| Risk | Mitigation |
|------|-----------|
| TagHelpers become feature-bloated | Stick to scope: forms, display, data, layout, nav. No custom business logic. |
| CSS/Bootstrap coupling | Wrap Bootstrap in IFormComponentProvider; easy to swap if needed |
| Localization strings scattered | Centralize all UI messages in IValidationMessageProvider + IResourceProvider |
| Performance with taghelper rendering | Cache HTML generation in IComponentConfigurationService; measure with profiler |

---

## 9. Dependencies & Prerequisites

- **Existing:** ICacheService, IResourceProvider, IGlobalizationService (in Application layer)
- **New:** All Phase 1 services in Shared.Services.Components
- **Bootstrap 5** — Already in use across Web/Admin/Public
- **.NET 9.0** — Current framework version

---

## 10. Timeline Estimate

| Phase | Components | Estimate |
|-------|-----------|----------|
| **Phase 1** | 16 TagHelpers + 3 services | 2-3 weeks |
| **Phase 2** | 13 TagHelpers + 2 services | 2 weeks |
| **Phase 3** | 6 TagHelpers + utilities | 1-2 weeks |
| **Documentation + Demo App** | Reference implementation | 1 week |

---

## 11. Next Steps

1. ✅ Design approved (this document)
2. 📝 Create implementation plan (specs for each component)
3. 🛠️ Start Phase 1 implementation (Form components first)
4. 🧪 Add unit + integration tests incrementally
5. 🔄 Refactor existing views to use new components
6. 📚 Build demo/reference app
7. 🚀 Phase 2 & 3 rollout

---

## 12. Glossary

- **TagHelper** — ASP.NET Core component that processes custom HTML tags in Razor views
- **Phase** — Logical grouping of components by priority (MUST / GOOD / NICE)
- **Service** — Shared business logic (caching, localization, validation)
- **IProvider** — Interface defining contracts for component-related services
- **Bootstrap 5** — CSS framework used by Web/Admin/Public apps

---

**Document Version:** 1.0  
**Last Updated:** 2026-04-18  
**Next Review:** After Phase 1 implementation
