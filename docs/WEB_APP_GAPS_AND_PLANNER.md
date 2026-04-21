# 🌐 SmartWorkz Web App - Gaps Analysis & Action Planner

**Document Type:** Gap Analysis + Implementation Roadmap  
**Status:** Active Planning  
**Scope:** Web App Only (MVP Focus)  
**Date:** April 21, 2026  
**Version:** 1.0

---

## EXECUTIVE SUMMARY

**Current Web Implementation:** 60% Complete  
**Production Ready:** ⚠️ Needs Critical Fixes  
**Time to MVP:** 8-12 weeks  
**Budget to MVP:** $8,000-12,000

### Quick Stats
```
Components Built:     15 ✅
Components Needed:    8 ❌
Services Built:       3 ✅
Services Needed:      5 ❌
Pages Available:      25 ✅
Pages Gap:            5-7 ❌
API Endpoints:        0 ❌
API Endpoints Needed: 20+ ❌
Tests Written:        0 ❌
Tests Needed:         50+ ❌
```

---

## PART 1: CURRENT WEB IMPLEMENTATION AUDIT

### ✅ What's Working Well

#### **1. Component Library (15 Components)**
```
Data Display (7):
  ✅ CardComponent      - Flexible card with image, icon, badge
  ✅ DashboardComponent - Statistics with trends
  ✅ TableComponent     - Full-featured grid with sort/pagination
  ✅ TabsComponent      - Multiple layouts and styles
  ✅ AccordionComponent - Collapsible sections with animations
  ✅ TreeViewComponent  - Hierarchical data display
  ✅ TimelineComponent  - Chronological events

Grid/List Views (3):
  ✅ GridComponent      - Advanced data grid
  ✅ ListViewComponent  - Card-based layout
  ✅ DataViewerComponent- All-in-one dual-view

Helper Components (5):
  ✅ FilterBuilder      - Dynamic filter UI
  ✅ GridColumn         - Column configuration
  ✅ GridFilter         - Filter UI
  ✅ GridRowSelector    - Selection UI
  ✅ TreeNode           - Recursive rendering
```

#### **2. Tag Helpers (18 Total)**
```
Form Helpers (9):
  ✅ form-tag, input-tag, label-tag, select-tag
  ✅ checkbox-tag, radio-tag, textarea-tag
  ✅ file-input-tag, validation-message-tag

Display Helpers (4):
  ✅ alert-tag, badge-tag, pagination-tag, icon-tag

Navigation Helpers (2):
  ✅ breadcrumb-tag, grid-tag

Common Helpers (3):
  ✅ button-tag, and 2 more
```

#### **3. Core Services (3)**
```
✅ ITableDataService
   - ApplyFiltersAsync, ApplySortingAsync, ApplyPaginationAsync
   
✅ ITreeViewService
   - FlattenTree, FindNodeById, SearchNodes, FilterByProperty
   
✅ IDataFormatterService
   - FormatCurrency, FormatDate, FormatPercentage, FormatBytes
   - FormatBoolean, FormatTimeSpan
```

#### **4. Foundation Infrastructure**
```
✅ Multi-tenancy support (built-in)
✅ Authentication (Cookie + JWT)
✅ Authorization (RBAC + Permissions)
✅ Database structure (EF Core + Dapper)
✅ Responsive design (480px+ mobile)
✅ Bootstrap 5 integration
✅ WCAG 2.1 AA accessibility
✅ 100% XML documentation
```

#### **5. Web Projects**
```
✅ SmartWorkz.Core.Web
   - Reusable component library
   - 48 files (.cs + .razor)
   - Well-organized structure

✅ SmartWorkz.StarterKitMVC.Public
   - Public-facing website
   - 25 Razor pages
   - 5 MVC controllers
   - Authentication/authorization

✅ SmartWorkz.StarterKitMVC.Admin
   - Admin dashboard
   - User/role management
   - Tenant management
```

---

## PART 2: CRITICAL GAPS ANALYSIS

### 🔴 CRITICAL GAPS (Block MVP)

#### **1. No REST API Endpoints** 🔴 CRITICAL

**Current State:**
```
❌ No /api/products endpoint
❌ No /api/users endpoint
❌ No /api/categories endpoint
❌ No /api/orders endpoint
❌ No authentication API
❌ No error response standardization
```

**Impact:**
```
- Can't serve mobile apps
- Can't integrate with external systems
- Can't build SPA front-end
- Limits scalability
```

**Required Endpoints (MVP):**
```
Products:
  GET    /api/products              (list with pagination)
  GET    /api/products/{id}         (detail)
  POST   /api/products              (create)
  PUT    /api/products/{id}         (update)
  DELETE /api/products/{id}         (delete)

Categories:
  GET    /api/categories            (list)
  GET    /api/categories/{id}       (detail)

Users:
  GET    /api/users/me              (current user)
  GET    /api/users/{id}            (user profile)
  POST   /api/auth/login            (authentication)
  POST   /api/auth/refresh          (token refresh)

Orders (if applicable):
  GET    /api/orders                (list)
  POST   /api/orders                (create)
  GET    /api/orders/{id}           (detail)

Total: 12+ endpoints minimum
```

**Effort:** 40 hours (2 weeks)  
**Priority:** P0 - Blocks mobile development

---

#### **2. No Input Validation Layer** 🔴 CRITICAL

**Current State:**
```
❌ No FluentValidation integration
❌ Scattered validation logic
❌ No global validation middleware
❌ No standardized error responses
❌ Client-side validation only (in forms)
```

**Impact:**
```
- Invalid data reaches database
- Inconsistent error messages
- Security vulnerabilities
- Difficult to maintain
```

**Required Implementation:**
```csharp
// Needed: FluentValidation setup
public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name too long");
        
        RuleFor(x => x.Price)
            .NotEmpty().WithMessage("Price is required")
            .GreaterThan(0).WithMessage("Price must be positive");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description too long");
    }
}

// Needed: Validation middleware
public class ValidationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            // Handle validation errors globally
            var response = new ErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = "Validation failed",
                Errors = ex.Errors.ToDictionary(...)
            };
            
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

**Effort:** 16 hours (1 week)  
**Priority:** P0 - Blocks API deployment

---

#### **3. No Global Error Handling** 🔴 CRITICAL

**Current State:**
```
❌ No exception handler middleware
❌ Default error pages
❌ No error code standardization
❌ No structured error logging
❌ Inconsistent error responses
```

**Impact:**
```
- Users see ugly error pages
- Hard to debug production issues
- No error tracking
- Poor API usability
```

**Required Implementation:**
```csharp
// Needed: Global exception handler middleware
public class ExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var response = HandleException(ex);
            context.Response.StatusCode = response.StatusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
    
    private ErrorResponse HandleException(Exception ex) => ex switch
    {
        ValidationException => new ErrorResponse { Code = "VALIDATION_ERROR", ... },
        NotFoundException => new ErrorResponse { Code = "NOT_FOUND", ... },
        UnauthorizedException => new ErrorResponse { Code = "UNAUTHORIZED", ... },
        ForbiddenException => new ErrorResponse { Code = "FORBIDDEN", ... },
        _ => new ErrorResponse { Code = "INTERNAL_ERROR", ... }
    };
}
```

**Effort:** 8 hours (0.5 weeks)  
**Priority:** P0 - Blocks API launch

---

#### **4. No Testing Infrastructure** 🔴 CRITICAL

**Current State:**
```
❌ Zero unit tests
❌ Zero integration tests
❌ No test helpers/fixtures
❌ No mock implementations
❌ No test data builders
```

**Impact:**
```
- Can't verify quality
- Risky to refactor
- Hard to onboard developers
- Bugs slip to production
```

**Required Implementation:**
```
Tests Needed (MVP):
  - 10 Component tests (Card, Table, etc.)
  - 15 Service tests (DataFormatter, Tree ops, etc.)
  - 20 API endpoint tests (CRUD operations)
  - 10 Validation tests
  Total: 55+ tests

Test Framework: xUnit + Moq + bUnit (for Blazor)

Example Test:
[Fact]
public async Task GetProducts_WithValidRequest_ReturnsPagedResult()
{
    // Arrange
    var service = new ProductService(_mockRepo.Object);
    var request = new GetProductsRequest { Page = 1, PageSize = 10 };
    
    // Act
    var result = await service.GetProductsAsync(request);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(10, result.Items.Count);
}
```

**Effort:** 60 hours (3-4 weeks)  
**Priority:** P1 - Blocks scaling

---

### 🟠 HIGH PRIORITY GAPS (Should Have)

#### **5. Missing Core Components** 🟠 HIGH

**Current State:**
```
❌ ModalComponent (dialogs/popups)
❌ ToastComponent (notifications)
❌ DrawerComponent (side navigation)
❌ LoadingComponent (skeleton loaders)
❌ EmptyStateComponent (empty data UI)
❌ ErrorBoundary (error handling)
```

**Impact:**
```
- Limited user experience
- Hard to show confirmations
- Hard to show notifications
- Poor error feedback
```

**Effort:** 30 hours (2 weeks)  
**Priority:** P1 - Improves UX significantly

---

#### **6. Missing Form Components** 🟠 HIGH

**Current State:**
```
❌ DatePickerComponent
❌ TimePickerComponent
❌ ColorPickerComponent
❌ SearchComponent (with debounce)
❌ AutocompleteComponent
❌ FileUploadComponent
```

**Impact:**
```
- Limited form capabilities
- More custom code needed
- Poor data input experience
```

**Effort:** 40 hours (2-3 weeks)  
**Priority:** P1 - Blocks feature development

---

#### **7. Missing Web Services** 🟠 HIGH

**Current State:**
```
❌ IModalService (open/close modals)
❌ IToastService (show notifications)
❌ IConfirmationService (show confirmations)
❌ ILoadingService (show loading UI)
❌ IClipboardService (copy to clipboard)
❌ IDownloadService (file downloads)
```

**Impact:**
```
- More JavaScript in components
- Harder to manage modal state
- No centralized notification logic
```

**Effort:** 20 hours (1-2 weeks)  
**Priority:** P1 - Improves code quality

---

#### **8. Package Version Issues** 🟠 HIGH

**Current State:**
```
❌ Microsoft.AspNetCore.Mvc.Razor Version 2.3.0
   Should be: 9.0.0 (matches .NET 9.0)
   
❌ Microsoft.AspNetCore.Mvc.ViewFeatures Version 2.3.9
   Should be: 9.0.0
   
❌ Bootstrap 5 (version unclear, likely via CDN)
   Should be: Explicit package reference
```

**Impact:**
```
- Build warnings
- Runtime compatibility issues
- Feature incompatibilities
```

**Effort:** 2 hours (quick fix)  
**Priority:** P1 - Fix before launch

---

### 🟡 MEDIUM PRIORITY GAPS (Nice to Have)

#### **9. No Security Headers** 🟡 MEDIUM

**Missing:**
```
❌ Content-Security-Policy
❌ X-Frame-Options
❌ X-Content-Type-Options
❌ Strict-Transport-Security
❌ X-XSS-Protection
```

**Effort:** 4 hours  
**Priority:** P2 - Add before scale

---

#### **10. No HTTP Caching** 🟡 MEDIUM

**Missing:**
```
❌ ETags
❌ Cache-Control headers
❌ Response compression (gzip)
❌ Cache busting strategy
```

**Effort:** 8 hours  
**Priority:** P2 - Improves performance

---

#### **11. No Real-time Features** 🟡 MEDIUM

**Missing:**
```
❌ SignalR integration
❌ WebSocket support
❌ Live notifications
❌ Live data updates
```

**Effort:** 20 hours  
**Priority:** P2 - Future enhancement

---

#### **12. No Deployment Pipeline** 🟡 MEDIUM

**Missing:**
```
❌ Docker support (Dockerfile)
❌ CI/CD pipeline (GitHub Actions)
❌ Automated testing in pipeline
❌ Automated deployment
❌ Environment configuration
```

**Effort:** 16 hours  
**Priority:** P2 - Add when scaling

---

## PART 3: IMPLEMENTATION ROADMAP

### Phase 1: MVP (Weeks 1-4)

**WEEK 1: API Layer + Validation**

```
Tasks:
  ✅ Create API Controllers
     - ProductsController (CRUD)
     - CategoriesController (CRUD)
     - UsersController (profile, auth)
     
  ✅ Create DTOs
     - CreateProductRequest
     - UpdateProductRequest
     - ProductResponse
     - PagedResult<T>
     
  ✅ Add validation
     - FluentValidation setup
     - Validation middleware
     - Error standardization
     
  ✅ Fix package versions
     - Update Mvc.Razor to 9.0.0
     - Update Mvc.ViewFeatures to 9.0.0
     - Add explicit Bootstrap NuGet

Time: 40 hours
Cost: $2,000
Deliverable: Working API with 12+ endpoints
```

**WEEK 2: Error Handling + Tests**

```
Tasks:
  ✅ Global exception handler
  ✅ Error response formatting
  ✅ Structured logging setup
  ✅ Write first 15 API tests
  ✅ Write 10 component tests
  ✅ Write 5 service tests

Time: 32 hours
Cost: $1,600
Deliverable: Error handling + 30 tests
```

**WEEK 3: Web Pages (MVP)**

```
Tasks:
  ✅ Home/landing page
  ✅ Login page (use existing auth)
  ✅ Product list page (use Table component)
  ✅ Product detail page
  ✅ Product create/edit page
  ✅ Category management page (if time)

Time: 32 hours
Cost: $1,600
Deliverable: Working web application
```

**WEEK 4: Polish + Launch**

```
Tasks:
  ✅ Write remaining 10 tests
  ✅ Bug fixes from testing
  ✅ Performance optimization
  ✅ Accessibility audit
  ✅ Responsive design check
  ✅ Deploy to staging
  ✅ Ready for production

Time: 24 hours
Cost: $1,200
Deliverable: Production-ready application
```

**PHASE 1 TOTALS:**
```
Duration: 4 weeks
Developers: 1-2
Cost: $6,400
Lines of Code: 2,500+
API Endpoints: 12+
Tests: 30+
Pages: 5+
```

---

### Phase 2: Enhance (Weeks 5-8)

**WEEK 5: Missing Components**

```
Add:
  ✅ ModalComponent
  ✅ ToastComponent
  ✅ DrawerComponent
  ✅ LoadingComponent
  ✅ EmptyStateComponent

Time: 32 hours
Cost: $1,600
```

**WEEK 6: Web Services**

```
Add:
  ✅ IModalService
  ✅ IToastService
  ✅ IConfirmationService
  ✅ ILoadingService
  ✅ IClipboardService

Time: 20 hours
Cost: $1,000
```

**WEEK 7: Form Components**

```
Add:
  ✅ DatePickerComponent
  ✅ SearchComponent
  ✅ FileUploadComponent
  ✅ Autocomplete improvements

Time: 24 hours
Cost: $1,200
```

**WEEK 8: Security + Optimization**

```
Add:
  ✅ Security headers middleware
  ✅ HTTP caching strategy
  ✅ Response compression
  ✅ Performance profiling

Time: 12 hours
Cost: $600
```

**PHASE 2 TOTALS:**
```
Duration: 4 weeks
Cost: $4,400
Components Added: 8
Services Added: 5
Performance: +30%
Security: Hardened
```

---

### Phase 3: Scale (Weeks 9-12+)

```
Optional Enhancements:
  - SignalR for real-time
  - Advanced caching
  - Database optimization
  - Load testing
  - CI/CD pipeline
  - Docker containerization
  - Mobile API optimization
```

---

## PART 4: ACTION PLAN BY PRIORITY

### 🔴 CRITICAL (Do First - Blocks MVP)

| # | Task | Effort | Cost | Duration |
|---|------|--------|------|----------|
| 1 | Create REST API endpoints | 40 hrs | $2,000 | Week 1-2 |
| 2 | Add input validation | 16 hrs | $800 | Week 1 |
| 3 | Global error handling | 8 hrs | $400 | Week 1 |
| 4 | Write 30+ tests | 30 hrs | $1,500 | Week 2-3 |
| 5 | Fix package versions | 2 hrs | $100 | Week 1 |
| **TOTAL** | | **96 hrs** | **$4,800** | **3 weeks** |

**Expected Outcome:** Production-ready MVP web app + API

---

### 🟠 HIGH (Do Next - Improves Experience)

| # | Task | Effort | Cost | Duration |
|---|------|--------|------|----------|
| 6 | Add 5 missing components | 30 hrs | $1,500 | Week 5 |
| 7 | Add web services | 20 hrs | $1,000 | Week 6 |
| 8 | Add form components | 40 hrs | $2,000 | Week 7 |
| **TOTAL** | | **90 hrs** | **$4,500** | **3 weeks** |

**Expected Outcome:** Enhanced web app with rich components

---

### 🟡 MEDIUM (Do Later - Nice to Have)

| # | Task | Effort | Cost | Duration |
|---|------|--------|------|----------|
| 9 | Security headers | 4 hrs | $200 | Week 8 |
| 10 | HTTP caching | 8 hrs | $400 | Week 8 |
| 11 | Real-time features | 20 hrs | $1,000 | Week 9+ |
| 12 | Deployment pipeline | 16 hrs | $800 | Week 9+ |
| **TOTAL** | | **48 hrs** | **$2,400** | **4+ weeks** |

**Expected Outcome:** Scaled, optimized application

---

## PART 5: DETAILED IMPLEMENTATION CHECKLIST

### ✅ WEEK 1: API + Validation

- [ ] Create ProductsController with CRUD endpoints
- [ ] Create CategoriesController
- [ ] Create AuthController (login, refresh)
- [ ] Create UserController (profile)
- [ ] Create DTOs for all endpoints
- [ ] Setup FluentValidation
- [ ] Create validation middleware
- [ ] Standardize error response format
- [ ] Fix package versions (2.3.0 → 9.0.0)
- [ ] Test all endpoints (manual)
- [ ] Total: 40 hours

### ✅ WEEK 2: Error Handling + Tests

- [ ] Implement global exception handler middleware
- [ ] Add structured logging (Serilog or similar)
- [ ] Write 15 API endpoint tests
- [ ] Write 10 component tests
- [ ] Write 5 service tests
- [ ] Test error scenarios
- [ ] Test validation failures
- [ ] Test unauthorized access
- [ ] Total: 32 hours

### ✅ WEEK 3: Web Pages

- [ ] Create Home page
- [ ] Create Login page
- [ ] Create Products list page (use Table)
- [ ] Create Product detail page
- [ ] Create Product create/edit page
- [ ] Integrate with API
- [ ] Test all pages
- [ ] Test responsive design
- [ ] Total: 32 hours

### ✅ WEEK 4: Polish + Launch

- [ ] Fix all reported bugs
- [ ] Performance optimization
- [ ] Accessibility audit
- [ ] Security review
- [ ] Load testing (basic)
- [ ] Deploy to staging
- [ ] User acceptance testing
- [ ] Deploy to production
- [ ] Monitor errors
- [ ] Total: 24 hours

---

## PART 6: RESOURCE ALLOCATION

### Development Team (MVP - 4 weeks)

```
Senior Developer (Lead):
  - API design & implementation
  - Architecture decisions
  - Code review
  - 20 hours/week
  
Mid-level Developer:
  - Feature implementation
  - Testing
  - UI/UX implementation
  - 20 hours/week
  
QA/Tester (Part-time):
  - Manual testing
  - Bug reporting
  - 10 hours/week
  
Total: 2-2.5 FTE for 4 weeks
```

### Tools & Infrastructure

```
Development:
  ✅ Visual Studio 2022 (already have)
  ✅ SQL Server (already have)
  ✅ Git (already have)
  
Testing:
  + xUnit (testing framework)
  + Moq (mocking library)
  
Deployment:
  + SQL Server hosting
  + IIS or Azure App Service
  + Domain + SSL
  
Monitoring:
  + Application Insights (basic)
  + ELK Stack or similar (optional)
```

---

## PART 7: SUCCESS CRITERIA

### MVP Launch (Week 4)

- [x] All critical gaps fixed
- [x] 12+ API endpoints working
- [x] 5+ web pages functional
- [x] 30+ tests passing
- [x] Zero critical bugs
- [x] Responsive on mobile
- [x] Accessible (WCAG 2.1 AA)
- [x] Performance acceptable (<3s load time)

### Phase 2 Complete (Week 8)

- [ ] 8 new components added
- [ ] 5 new services added
- [ ] 60+ total tests passing
- [ ] All high-priority gaps closed
- [ ] Performance optimized (+30%)
- [ ] User feedback incorporated
- [ ] Ready for feature requests

---

## PART 8: RISK MITIGATION

### Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| API design changes | High | Medium | Start with simple endpoints, evolve |
| Test coverage gaps | Medium | High | Write tests incrementally |
| Performance issues | Medium | Medium | Load test each week |
| Breaking changes | Low | High | Use versioning from day 1 |

### Schedule Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Scope creep | High | High | Strict MVP scope, defer enhancements |
| Team delays | Medium | High | Parallel work, no blockers |
| Bug discovery | Medium | Medium | 1-week buffer in schedule |

### Mitigation Strategy

```
Week-by-week reviews:
  ✅ Every Friday: Review progress
  ✅ Every Friday: Identify blockers
  ✅ Adjust scope if needed
  ✅ Add contingency time if slipping

Daily standups:
  ✅ 15-minute sync
  ✅ What did you do yesterday
  ✅ What will you do today
  ✅ Any blockers?
```

---

## PART 9: BUDGET SUMMARY

### Development Cost Breakdown

```
Phase 1 (MVP):
  Week 1 (API + Validation):    $2,000
  Week 2 (Errors + Tests):      $1,600
  Week 3 (Web Pages):           $1,600
  Week 4 (Polish + Launch):     $1,200
  Subtotal:                     $6,400

Phase 2 (Enhanced):
  Week 5 (Components):          $1,600
  Week 6 (Services):            $1,000
  Week 7 (Forms):               $1,200
  Week 8 (Security):              $600
  Subtotal:                     $4,400

Phase 3 (Optional):
  Advanced features:            $2,400+

Contingency (10%):              $1,100

TOTAL (Phases 1-2):            $12,000
```

### ROI Analysis

```
Time Investment:
  Phase 1: 4 weeks (128 hours)
  Phase 2: 4 weeks (88 hours)
  Total: 8 weeks (216 hours)

Cost Savings (vs. from scratch):
  Reusable architecture:        $8,000 saved
  Components library:           $3,000 saved
  Auth/DB setup:                $2,000 saved
  Total Savings:               $13,000 saved

ROI:
  Investment: $12,000
  Savings: $13,000
  Net Benefit: $1,000+
  ROI: 108% in 8 weeks
```

---

## PART 10: QUICK START GUIDE

### To Get Started Today:

```bash
# 1. Switch to main
git checkout main

# 2. Create feature branch
git checkout -b feat/api-endpoints

# 3. Create API Controllers folder
mkdir -p src/SmartWorkz.StarterKitMVC.Public/ApiControllers

# 4. Start with ProductsController
# (See implementation details in Part 1)

# 5. Build and test
dotnet build
dotnet test

# 6. Create PR and review
# (Once tests pass)
```

---

## SUMMARY

### Current Status
- ✅ Foundation solid (60% complete)
- ⚠️ Critical gaps identified (12 total)
- 🔴 MVP blocked by: API layer, validation, error handling, tests

### Path Forward
1. **Week 1-2:** Build API + validation (Critical)
2. **Week 3:** Build web pages (MVP ready)
3. **Week 4:** Polish + launch (Production ready)
4. **Week 5-8:** Enhance with components (Phase 2)

### Timeline & Budget
- **Duration:** 8 weeks to Phase 2 complete
- **Cost:** $12,000 (2 developers)
- **Result:** Production-ready web app + enhanced UX

### Next Steps
1. Approve roadmap
2. Allocate resources
3. Create GitHub issues (one per week)
4. Start Week 1 implementation
5. Daily standups + weekly reviews

---

**Document Created:** April 21, 2026  
**Status:** Ready for Implementation  
**Version:** 1.0  
**Approver:** _________________  
**Date Approved:** _________________
