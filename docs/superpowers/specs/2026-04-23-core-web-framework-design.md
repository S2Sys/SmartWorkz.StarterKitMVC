# SmartWorkz.Core.Web Framework Design Specification

**Date:** 2026-04-23  
**Status:** Design Review  
**Scope:** Core.Web as standalone ASP.NET Core MVC/Razor Pages component library  
**Timeline:** 2-week foundation roadmap  

---

## Executive Summary

SmartWorkz.Core.Web is planned as a reusable component and utility library for ASP.NET Core MVC/Razor Pages applications. Currently, it exists only as a README with design intent but no implementation. This specification defines what Core.Web should contain to be a production-ready framework, evaluated through a consumer-independent lens (industry best practices for MVC frameworks).

The design is **consumer-agnostic** — Core.Web will be finalized independently and integrated with StarterKitMVC (or other applications) later.

---

## Scope & Goals

### What Core.Web Will Provide
- **Reusable Razor Components** — Data display, forms, navigation, layout patterns
- **View Utilities** — Tag helpers, HTML helpers, extension methods
- **Service Abstractions** — Validation, rendering, data binding contracts
- **Performance Optimizations** — Virtual scrolling, pagination, caching patterns
- **Quality Standards** — Accessibility, testability, comprehensive documentation

### Out of Scope (Phase 2+)
- Blazor support (MVC/Razor Pages only for Phase 1)
- Mobile-specific components
- Advanced charting/reporting (defer to specialized libraries)
- Authentication/authorization (use ASP.NET Core identity)

### Success Criteria for Phase 1 (Foundation)
- Core abstractions defined and documented
- 3-5 high-value components implemented and tested
- Clear extension points for consumer applications
- Comprehensive README with examples
- NuGet package structure ready

---

## Continuous Review Prompt Framework

This framework evaluates Core.Web completeness across 5 dimensions. Use this to assess any new additions:

### **1. Components**
**What:** Reusable Razor components with well-defined parameters and behavior  
**Questions:**
- Is there a clear purpose statement? (what problem does it solve?)
- Are parameters documented and typed correctly?
- Does it support customization (CSS classes, templates, slots)?
- Is it tested (unit or integration)?
- Is there a usage example in docs?

**Good to Have Examples:**
- GridComponent (sorting, filtering, pagination)
- ListViewComponent (templated list rendering)
- FormComponent (validation error display)
- ModalComponent (dialog pattern)

---

### **2. View Utilities**
**What:** Tag helpers, HTML helpers, extension methods that reduce boilerplate  
**Questions:**
- Does it eliminate repeated HTML across views?
- Is it discoverable (well-named, documented)?
- Can developers understand it without reading source code?
- Does it follow ASP.NET Core conventions?

**Good to Have Examples:**
- `<form-group>` tag helper (wraps label + input + validation)
- `<pagination>` tag helper (renders pagination UI)
- `@Html.StatusBadge(status)` helper (renders status with color/icon)
- Extension methods for common formatting (currency, dates, strings)

---

### **3. Service Abstractions**
**What:** Interfaces and implementations that encapsulate common MVC patterns  
**Questions:**
- Is there a clear contract (interface)?
- Can it be mocked/tested easily?
- Is the default implementation simple and correct?
- Does it expose extension points?

**Good to Have Examples:**
- `IValidationService` (client/server validation rules)
- `IDataBindingService` (map models to component parameters)
- `ICacheService` (query result caching, integrates with Core.Shared.Caching)
- `IBreadcrumbService` (manage navigation breadcrumbs)

---

### **4. Performance & Optimization**
**What:** Patterns and utilities for efficient rendering and data loading  
**Questions:**
- Does it handle large datasets without blocking?
- Are there clear configuration knobs (page size, cache duration)?
- Is there documentation on when to use it?
- Are tradeoffs explained (memory vs. performance)?

**Good to Have Examples:**
- Virtual scrolling for large grids (already in README)
- Pagination component with async loading
- Client-side caching decorator
- Lazy loading patterns for components

---

### **5. Quality Standards**
**What:** Accessibility, testability, and documentation consistency  
**Questions:**
- Does it meet WCAG 2.1 AA accessibility standards?
- Can it be unit tested without complex setup?
- Is there a test example?
- Are edge cases documented?

**Good to Have Examples:**
- ARIA labels on interactive elements
- Keyboard navigation support
- Unit test template for components
- Accessibility checklist in contribution guide

---

## Current State Mapping

### What Exists (From README)
| Category | Item | Status | Notes |
|----------|------|--------|-------|
| **Components** | GridComponent | ✓ Documented | Sorting, filtering, pagination, virtualization |
| | ListViewComponent | ✓ Documented | Custom templates, selection, virtualization |
| | DataViewerComponent | ✓ Documented | Flexible display, formatting, validation |
| **Styling** | Bootstrap 5 integration | ✓ Documented | CSS variable customization |
| **Accessibility** | ARIA labels | ✓ Mentioned | Basic support noted |
| **Testing** | Component testing | ✓ Example | Single test example provided |
| **Performance** | Virtual scrolling | ✓ Documented | Detailed explanation and config |
| **Documentation** | README | ✓ Complete | Comprehensive, includes examples |

### What's Missing (Implementation)
| Category | Item | Priority | Why |
|----------|------|----------|-----|
| **Code** | No .csproj file | Good to Have | Framework doesn't exist as a project yet |
| | No actual components | Good to Have | Only documented, not implemented |
| | No services | Good to Have | No abstractions exist |
| **Structure** | No unit tests | Good to Have | Only doc example exists |
| | No tag helpers | Good to Have | Not mentioned in README |
| | No HTML helpers | Good to Have | Not mentioned in README |
| **Quality** | No accessibility tests | Nice to Have | Manual testing only |
| | No NuGet package | Good to Have | Can't be consumed yet |
| | No contribution guide | Nice to Have | Framework is new |

---

## Gap Analysis: Good to Have vs. Nice to Have

### Good to Have (Enable Core.Web to be Usable)

**Foundation (Week 1-1.5)**
1. **.csproj File & Project Structure** — Set up NuGet-ready library
   - Dependency: Microsoft.AspNetCore.Components, AspNetCore.Mvc.ViewFeatures
   - Target: .NET 9.0+

2. **Component Base Classes** — Abstractions for consistency
   - `BaseRazorComponent` — Lifecycle, parameterization standards
   - `BaseFormComponent` — Validation integration
   - Clear documentation on how to extend

3. **GridComponent Implementation** — From design in README
   - Sorting, filtering, pagination
   - Virtual scrolling for 10K+ rows
   - Configurable column definitions
   - Unit tests (at least 5 critical paths)

4. **Validation Services** — Integration with ASP.NET Core validation
   - `IValidationService` interface
   - Client/server validation rules helper
   - Integration with DataAnnotations

**Consumption (Week 1.5-2)**
5. **Tag Helper Foundation** — Start with high-value ones
   - `<form-group>` tag helper (label + input + validation message)
   - `<status-badge>` tag helper (status display)
   - Documentation + examples

6. **Documentation & Examples**
   - API reference for all components/utilities
   - 3-5 complete usage examples (working Razor Pages)
   - Component composition guide

---

### Nice to Have (Enhance After Foundation)

**Quality Layer (Phase 1.5)**
- Accessibility compliance testing (aXe, Wave)
- Component storybook/showcase page
- Performance benchmarks
- Contributing guide + code style guide

**Feature Layer (Phase 2)**
- ListViewComponent full implementation
- Additional tag helpers (pagination, breadcrumb, etc.)
- HTML helper library
- Extension methods for common formatting
- DataViewerComponent enhancements

---

## 2-Week Foundation Implementation Roadmap

### Week 1: Core Infrastructure

**Day 1-2: Project Setup**
- [ ] Create SmartWorkz.Core.Web.csproj
- [ ] Configure dependencies (.NET 9, AspNetCore packages)
- [ ] Create folder structure (Components/, Services/, Utilities/, Models/)
- [ ] Set up unit test project (xUnit + Bunit)
- [ ] Git commit: "feat: scaffold Core.Web project structure"

**Day 2-3: Abstractions & Base Classes**
- [ ] Create `BaseRazorComponent` with parameter validation
- [ ] Create `BaseFormComponent` with validation context
- [ ] Create `IValidationService` interface
- [ ] Create `ValidationService` implementation
- [ ] Unit tests for service contracts
- [ ] Git commit: "feat: add component base classes and validation service"

**Day 4-5: GridComponent Implementation**
- [ ] Implement GridComponent with properties:
  - `Data<T>` — data source
  - `Columns` — column definition (property name, header, formatting)
  - `Sortable`, `Filterable`, `PageSize` — feature flags
  - `VirtualizationEnabled`, `ItemHeight`, `ContainerHeight` — perf config
- [ ] Implement sorting and filtering logic
- [ ] Implement virtual scrolling (reference: README)
- [ ] Write 5+ unit tests (happy path, virtualization, edge cases)
- [ ] Git commit: "feat: implement GridComponent with virtualization"

**Day 5: Documentation**
- [ ] Update README with implementation status
- [ ] Add GridComponent usage example to docs/examples/
- [ ] Git commit: "docs: add GridComponent usage guide"

---

### Week 2: Utilities & Completion

**Day 1-2: Tag Helpers**
- [ ] Implement `FormGroupTagHelper` (wraps label + input + validation)
- [ ] Implement `StatusBadgeTagHelper` (renders status with styling)
- [ ] Unit tests (parameter binding, content projection)
- [ ] Usage examples in Razor Pages
- [ ] Git commit: "feat: add form-group and status-badge tag helpers"

**Day 2-3: Service Extensions**
- [ ] Create extension methods for common use cases
  - `FormGroupHelper` — generate groups with consistent styling
  - `ValidationHelper` — convert validation results to UI messages
- [ ] Integration tests with real Razor Pages
- [ ] Git commit: "feat: add helper extensions for forms and validation"

**Day 3-4: Examples & Documentation**
- [ ] Create docs/examples/ directory with working Razor Pages
  - Example 1: Simple grid with sorting
  - Example 2: Form with validation
  - Example 3: Combined grid + filters
- [ ] Complete API reference documentation
- [ ] Update README with all features and links
- [ ] Git commit: "docs: add comprehensive examples and API reference"

**Day 4-5: Testing & Polish**
- [ ] Run all tests, achieve 70%+ code coverage on components
- [ ] Fix accessibility issues (ARIA labels on interactive elements)
- [ ] Create CONTRIBUTING.md (coding standards, component checklist)
- [ ] Review and refactor based on test feedback
- [ ] Final git commit: "chore: complete Phase 1 foundation, ready for integration"

---

## Success Criteria (Phase 1)

By end of Week 2:
- [ ] Core.Web is a valid NuGet package (can be published)
- [ ] GridComponent works with sorting, filtering, pagination, virtualization
- [ ] Form validation integrated and tested
- [ ] 2+ tag helpers implemented with examples
- [ ] 70%+ test coverage on components
- [ ] Complete documentation with 3+ working examples
- [ ] Code follows .NET conventions and async best practices
- [ ] All public APIs have XML documentation comments
- [ ] CONTRIBUTING.md defines extension patterns

---

## Code Examples & Patterns

### Example 1: Component Structure (BaseRazorComponent)
```csharp
// SmartWorkz.Core.Web/Components/BaseRazorComponent.cs
public abstract class BaseRazorComponent : ComponentBase
{
    [CascadingParameter] protected ErrorBoundary? ErrorBoundary { get; set; }
    
    protected virtual void OnParameterValidation() { }
    
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        try
        {
            await base.SetParametersAsync(parameters);
            OnParameterValidation();
        }
        catch (Exception ex)
        {
            ErrorBoundary?.HandleException(ex);
        }
    }
}
```

### Example 2: GridComponent Usage
```csharp
// Consumer usage in Razor Pages
<GridComponent Data="@employees" 
    VirtualizationEnabled="true" 
    ItemHeight="40" 
    PageSize="50">
    <Columns>
        <GridColumn Property="nameof(Employee.Name)" Header="Name" Sortable="true" />
        <GridColumn Property="nameof(Employee.Salary)" Header="Salary" Format="C" />
    </Columns>
</GridComponent>
```

### Example 3: Tag Helper Pattern
```csharp
// SmartWorkz.Core.Web/TagHelpers/FormGroupTagHelper.cs
[HtmlTargetElement("form-group")]
public class FormGroupTagHelper : TagHelper
{
    [HtmlAttributeName] public string Label { get; set; }
    [HtmlAttributeName] public string PropertyName { get; set; }
    [ViewContext] public ViewContext ViewContext { get; set; }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();
        var validation = ViewContext.ModelState[PropertyName];
        var errorClass = validation?.ValidationState == ModelValidationState.Invalid ? "is-invalid" : "";
        
        output.TagName = "div";
        output.AddClass("form-group");
        output.Content.SetHtmlContent($@"
            <label>{Label}</label>
            <div class='input-wrapper {errorClass}'>{content}</div>
            {(validation?.Errors.Any() == true ? $"<span class='error'>{validation.Errors[0].ErrorMessage}</span>" : "")}
        ");
    }
}
```

### Example 4: Validation Service
```csharp
// SmartWorkz.Core.Web/Services/ValidationService.cs
public interface IValidationService
{
    bool ValidateProperty<T>(T model, string propertyName, out List<string> errors);
    Dictionary<string, List<string>> ValidateModel<T>(T model);
}

public class ValidationService : IValidationService
{
    public Dictionary<string, List<string>> ValidateModel<T>(T model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        
        return results
            .GroupBy(r => r.MemberNames.First())
            .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage).ToList());
    }
}
```

---

## Dependencies & Assumptions

### Required Dependencies
- Microsoft.AspNetCore.Components (2.3.0+)
- Microsoft.AspNetCore.Mvc.Razor (2.3.0+)
- Microsoft.AspNetCore.Mvc.ViewFeatures (2.3.0+)

### Optional Dependencies (Future)
- SmartWorkz.Core.Shared.Caching (for query result caching)
- SmartWorkz.Core.Shared.Logging (for diagnostic logging)

### Assumptions
- Consumers use .NET 9.0+
- Bootstrap 5 for default styling (customizable)
- ASP.NET Core identity for auth (Core.Web doesn't include auth)
- Project will follow SemVer versioning post-Phase-1

---

## Timeline Summary

| Phase | Duration | Key Deliverable | Integration Gate |
|-------|----------|-----------------|------------------|
| **1: Foundation** | 2 weeks | GridComponent + Tag Helpers + Docs | Ready for StarterKitMVC integration |
| **1.5: Polish** | 1 week (buffer) | Tests, accessibility, examples | Production-ready |
| **2: Features** | TBD | ListViewComponent, more helpers | Consumer feedback integration |

---

## Questions for Design Review

1. **Component Library Choice:** Should Core.Web depend on component libraries (Telerik, Infragistics) or stay pure ASP.NET/Bootstrap?
   - **Recommendation:** Pure ASP.NET + Bootstrap (simpler, open, easier to extend)

2. **Virtual Scrolling Implementation:** Use Virtualize component from Microsoft or custom?
   - **Recommendation:** Microsoft.AspNetCore.Components.Web.Virtualize (battle-tested, less code)

3. **Tag Helper Naming:** Use custom XML namespace (`<sw:form-group>`) or generic (`<form-group>`)?
   - **Recommendation:** Generic `<form-group>` (more intuitive, less ceremony)

---

## Next Steps

1. **User review** of this spec
2. **Writing-plans skill** to detail implementation tasks
3. **Implementation** in isolated branch
4. **Integration** with StarterKitMVC post-Phase-1
