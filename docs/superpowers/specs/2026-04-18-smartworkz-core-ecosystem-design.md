# SmartWorkz Core Ecosystem Design
## Multi-Platform Framework & Starter Kits

**Date:** 2026-04-18  
**Scope:** Reusable Core framework (NuGet packages) + Reference Starter Kits  
**Status:** Design Review  
**Version:** 1.0

---

## 1. Executive Summary

This design creates a **reusable Core framework** (`SmartWorkz.Core.*` NuGet packages) that can be referenced by multiple starter kit projects and any external projects. The framework provides:

- **SmartWorkz.Core** — Base models, DTOs, enums, validators, services
- **SmartWorkz.Core.Shared** — Cross-cutting utilities, helpers, primitives
- **SmartWorkz.Core.Web** — TagHelpers, HTTP helpers (Phase 1: implement now)
- **SmartWorkz.Core.MAUI** — XAML controls for mobile (Phase 2: future)
- **SmartWorkz.Core.WPF** — XAML controls for WPF desktop (Phase 3: future)
- **SmartWorkz.Core.WinForms** — WinForms controls for desktop (Phase 3: future)

**Reference Starters** demonstrate each platform:
- `SmartWorkz.StarterKitMVC` (Web) — References Core.Web
- `SmartWorkz.Starter.MAUI` (Mobile) — References Core.MAUI [FUTURE]
- `SmartWorkz.Starter.WPF` (Desktop) — References Core.WPF [FUTURE]
- `SmartWorkz.Starter.WinForms` (Desktop) — References Core.WinForms [FUTURE]

**Current:** All in one solution for unified development  
**Future:** Split into separate solutions per platform/framework

---

## 2. Architecture

### 2.1 Solution Structure (Phase 1)

```
SmartWorkz/                              (One solution, later splits into 4+)
├── Core Framework Packages
│   └── src/
│       ├── SmartWorkz.Core/            (v1.0.0)
│       ├── SmartWorkz.Core.Shared/     (v1.0.0)
│       ├── SmartWorkz.Core.Web/        (v1.0.0)
│       ├── SmartWorkz.Core.MAUI/       (v1.0.0 stub)
│       ├── SmartWorkz.Core.WPF/        (v1.0.0 stub)
│       └── SmartWorkz.Core.WinForms/   (v1.0.0 stub)
│
├── Reference Starter Kits
│   ├── SmartWorkz.StarterKitMVC/       (Web - keep existing structure)
│   ├── SmartWorkz.Starter.MAUI/        (Mobile - future)
│   ├── SmartWorkz.Starter.WPF/         (Desktop - future)
│   └── SmartWorkz.Starter.WinForms/    (Desktop - future)
│
└── Tests/
    ├── SmartWorkz.Core.Tests/
    ├── SmartWorkz.Core.Web.Tests/
    └── (etc)
```

### 2.2 Package Dependency Graph

```
                    SmartWorkz.Core (Base)
                            ↑
                            │ depends on
                            ↓
                SmartWorkz.Core.Shared (Utilities)
                            ↑
                ┌───────────┬───────────┬──────────────┐
                │           │           │              │
    SmartWorkz.Core.Web  Core.MAUI  Core.WPF    Core.WinForms
                │           │           │              │
                ↓           ↓           ↓              ↓
           StarterKitMVC  Starter.   Starter.WPF   Starter.
                MVC       MAUI       (future)     WinForms
                                                   (future)
```

### 2.3 Detailed Project Breakdown

#### **SmartWorkz.Core** (Base Framework)
Dependency-free, framework-agnostic core logic.

```
SmartWorkz.Core/
├── Models/
│   ├── User, Tenant, Role, Permission models
│   └── Domain entities
├── DTOs/
│   ├── AuthDto, MenuDto, ProductDto
│   └── API request/response DTOs
├── Enums/
│   ├── UserStatus, OrderStatus, etc.
│   └── Feature enums
├── Constants/
│   ├── AppConstants, MessageKeys
│   └── Resource keys
├── Validators/
│   ├── EntityValidators, AuthValidators
│   └── Custom validation rules
├── Extensions/
│   ├── StringExtensions, DateTimeExtensions
│   ├── CollectionExtensions, EnumExtensions
│   └── Validation extensions
├── Services/
│   ├── Caching/
│   │   ├── ICacheService
│   │   └── ICacheKeyGenerator
│   ├── Globalization/
│   │   ├── IGlobalizationService
│   │   └── IResourceProvider
│   ├── Notifications/
│   │   └── INotificationService
│   └── Components/
│       ├── IIconProvider
│       ├── IValidationMessageProvider
│       └── IComponentConfigurationService
├── Abstractions/
│   ├── IRepository, IUnitOfWork
│   └── IService interfaces
└── GlobalUsings.cs
```

#### **SmartWorkz.Core.Shared** (Cross-Cutting Utilities)
Helper utilities, base classes, common primitives used by all platforms.

```
SmartWorkz.Core.Shared/
├── Primitives/
│   ├── Result<T>, Result
│   ├── ApiError, ProblemDetails
│   ├── ValidationResult
│   └── CorrelationContext
├── Base Classes/
│   ├── BasePage, BaseViewModel
│   ├── BaseRepository
│   └── AuditableEntity
├── Helpers/
│   ├── ComponentHelpers (format, sanitize)
│   ├── FileHelper
│   ├── JsonHelper
│   └── HtmlHelper
├── Exceptions/
│   ├── BusinessException
│   ├── ValidationException
│   └── NotFoundException
├── Attributes/
│   ├── AuditableAttribute
│   └── Custom attributes
└── Utilities/
    ├── Security utilities
    ├── Encryption helpers
    └── Serialization utilities
```

#### **SmartWorkz.Core.Web** (Web-Only, Phase 1)
ASP.NET MVC/Razor-specific components.

```
SmartWorkz.Core.Web/
├── TagHelpers/
│   ├── Forms/
│   │   ├── FormTagHelper
│   │   ├── FormGroupTagHelper
│   │   ├── InputTagHelper
│   │   ├── SelectTagHelper
│   │   ├── TextAreaTagHelper
│   │   ├── CheckboxTagHelper
│   │   ├── RadioButtonTagHelper
│   │   ├── FileInputTagHelper
│   │   ├── ValidationMessageTagHelper
│   │   └── PasswordStrengthTagHelper
│   ├── Display/
│   │   ├── AlertTagHelper
│   │   ├── BadgeTagHelper
│   │   ├── PaginationTagHelper
│   │   ├── BreadcrumbTagHelper
│   │   ├── ListGroupTagHelper
│   │   ├── EmptyStateTagHelper
│   │   └── TooltipTagHelper
│   ├── Data/
│   │   ├── TableTagHelper
│   │   └── SortableHeaderTagHelper
│   ├── Layout/
│   │   ├── CardTagHelper
│   │   ├── ModalTagHelper
│   │   ├── TabsTagHelper
│   │   ├── AccordionTagHelper
│   │   └── FormSectionTagHelper
│   ├── Navigation/
│   │   └── MenuTagHelper
│   └── Common/
│       ├── ButtonTagHelper
│       ├── IconTagHelper
│       ├── LinkTagHelper
│       ├── LoadingSpinnerTagHelper
│       └── SearchInputTagHelper
├── Services/
│   └── Components/
│       ├── IAccessibilityService
│       ├── IFormComponentProvider
│       └── WebComponentExtensions
├── Http/
│   ├── HttpHelpers
│   └── ApiClient wrapper
└── Extensions/
    ├── AspNetCore extensions
    ├── Razor extensions
    └── View context helpers
```

#### **SmartWorkz.Core.MAUI** (Mobile, Stub)
Will contain MAUI-specific controls and services. Currently placeholder.

#### **SmartWorkz.Core.WPF** (Desktop, Stub)
Will contain WPF controls, converters, and behaviors. Currently placeholder.

#### **SmartWorkz.Core.WinForms** (Desktop, Stub)
Will contain WinForms controls and helpers. Currently placeholder.

---

## 3. Phase Breakdown

### **Phase 1: Web Foundation (NOW)**
**Target:** Create reusable Core framework for web, update StarterKitMVC

| Project | Components | Status |
|---------|-----------|--------|
| SmartWorkz.Core | Base models, DTOs, validators, services | Build |
| SmartWorkz.Core.Shared | Primitives, helpers, exceptions | Build |
| SmartWorkz.Core.Web | 16 core TagHelpers + 3 services | Build |
| SmartWorkz.StarterKitMVC | Update to reference Core.Web, remove duplication | Integrate |
| Tests | Unit + integration tests for all above | Add |

**Deliverable:** First NuGet packages ready for internal use

---

### **Phase 2: Web Enhancement (Later, Q2 2026)**
**Target:** Advanced TagHelpers + mobile framework

| Project | Components | Status |
|---------|-----------|--------|
| SmartWorkz.Core.Web | 13 additional TagHelpers + 2 services | Build |
| SmartWorkz.Core.MAUI | XAML controls, MVVM helpers, foundation | Build |
| SmartWorkz.Starter.MAUI | Reference mobile app | Create |

---

### **Phase 3: Desktop Support (Q3 2026)**
**Target:** WPF and WinForms frameworks + starters

| Project | Components | Status |
|---------|-----------|--------|
| SmartWorkz.Core.WPF | XAML controls, converters | Build |
| SmartWorkz.Core.WinForms | WinForms controls | Build |
| SmartWorkz.Starter.WPF | Reference desktop app | Create |
| SmartWorkz.Starter.WinForms | Reference desktop app | Create |

---

## 4. Integration Guide

### 4.1 StarterKitMVC Integration

**Current State:** SmartWorkz.StarterKitMVC.Shared contains duplicated utilities

**Future State:** StarterKitMVC references Core packages

```xml
<!-- SmartWorkz.StarterKitMVC.Web.csproj -->
<ItemGroup>
    <PackageReference Include="SmartWorkz.Core.Web" Version="1.0.0" />
</ItemGroup>

<!-- SmartWorkz.StarterKitMVC.Admin.csproj -->
<ItemGroup>
    <PackageReference Include="SmartWorkz.Core.Web" Version="1.0.0" />
</ItemGroup>

<!-- SmartWorkz.StarterKitMVC.Public.csproj -->
<ItemGroup>
    <PackageReference Include="SmartWorkz.Core.Web" Version="1.0.0" />
</ItemGroup>
```

**In _ViewImports.cshtml:**
```csharp
@addTagHelper *, SmartWorkz.Core.Web
@inject IIconProvider IconProvider
@inject IValidationMessageProvider ValidationMessages
@inject IResourceProvider ResourceProvider
```

### 4.2 Future Starter Integration

Each starter (MAUI, WPF, WinForms) follows the same pattern:
1. Reference appropriate Core package (Core.MAUI, Core.WPF, etc.)
2. Inject services into DI container
3. Use framework-specific components

---

## 5. Success Criteria

### ✅ Phase 1 Complete
- [ ] SmartWorkz.Core package with all base models, services
- [ ] SmartWorkz.Core.Shared with utilities, primitives
- [ ] SmartWorkz.Core.Web with 16 TagHelpers + 3 services
- [ ] Unit + integration tests (>80% coverage)
- [ ] StarterKitMVC updated to reference Core packages
- [ ] Zero hardcoded HTML for common patterns in Web/Admin/Public

### ✅ Phase 2 Complete
- [ ] 13 additional Core.Web TagHelpers + 2 services
- [ ] SmartWorkz.Core.MAUI foundation + sample controls
- [ ] SmartWorkz.Starter.MAUI reference implementation
- [ ] Documentation for each component

### ✅ Phase 3 Complete
- [ ] SmartWorkz.Core.WPF + Core.WinForms packages
- [ ] Desktop starter kits
- [ ] All 4 platforms (Web, Mobile, WPF, WinForms) with reference apps

---

## 6. Timeline & Solution Split

### **Phase 1: Unified Solution** (Now - May 2026)
All projects in one solution for easier development.

### **Phase 2: Split Strategy** (May 2026 onwards)

Once Core framework stabilizes, split into separate solutions:

```
Solution 1: SmartWorkz.Core/
├── SmartWorkz.Core
├── SmartWorkz.Core.Shared
├── SmartWorkz.Core.Web
├── SmartWorkz.Core.MAUI
├── SmartWorkz.Core.WPF
├── SmartWorkz.Core.WinForms
└── Tests/

Solution 2: SmartWorkz.StarterKitMVC/ (Web)
├── SmartWorkz.StarterKitMVC.Web
├── SmartWorkz.StarterKitMVC.Admin
├── SmartWorkz.StarterKitMVC.Public
└── SmartWorkz.StarterKitMVC.Shared (keep for app-specific logic)

Solution 3: SmartWorkz.Starter.MAUI/
Solution 4: SmartWorkz.Starter.WPF/
Solution 5: SmartWorkz.Starter.WinForms/
```

**NuGet Hosting:**
- Phase 1: Local project references
- Phase 2: Internal NuGet feed (when framework stabilizes)
- Phase 3+: Public nuget.org (optional, if open-sourcing)

---

## 7. Risks & Mitigation

| Risk | Mitigation |
|------|-----------|
| Core becomes bloated | Enforce single responsibility: Core only has framework code, not business logic |
| Circular dependencies | Document dependency graph; enforce layering with architecture tests |
| Solution complexity grows | Phase split strategy keeps each solution manageable |
| Shared project duplication | Move common code to Core packages; remove redundancy |
| External projects can't use Core | Publish to internal NuGet feed early; document versioning |

---

## 8. Dependencies

### Phase 1
- .NET 9.0
- Bootstrap 5 (for Web)
- Existing SmartWorkz.StarterKitMVC infrastructure
- No external NuGet packages required initially

### Phase 2+
- MAUI workload (.NET 9.0+)
- WPF/WinForms (.NET 9.0+)
- Platform-specific dependencies

---

## 9. Next Steps

1. ✅ Design approved (this document)
2. 📝 Create implementation plan (file-by-file specs)
3. 🏗️ Create project structure (SmartWorkz.Core solution)
4. 🛠️ Implement Phase 1 (Core, Core.Shared, Core.Web)
5. 🧪 Add unit + integration tests
6. 🔄 Refactor StarterKitMVC to use Core packages
7. 📚 Document API and usage examples
8. 🚀 Phase 2 & 3 (MAUI, WPF, WinForms)

---

## 10. Glossary

- **Core Framework** — Reusable NuGet packages (SmartWorkz.Core.*)
- **Starter Kit** — Reference implementation (SmartWorkz.Starter.*, SmartWorkz.StarterKitMVC)
- **Platform** — Target environment (Web, Mobile, WPF, WinForms)
- **Phase** — Development stage (1: Web, 2: Mobile, 3: Desktop)
- **TagHelper** — ASP.NET Razor component (Web-only)
- **XAML Control** — WPF/MAUI visual component

---

**Document Version:** 1.0  
**Architecture:** Multi-platform, one solution now, split later  
**Status:** Ready for implementation planning  
**Next Review:** After Phase 1 completion

