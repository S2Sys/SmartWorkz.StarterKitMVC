# Phase 1: Critical Components Implementation Sprint

**Duration:** 2 weeks (10 business days)  
**Status:** 🔴 ACTIVE  
**Target:** 5 critical components unblock all feature development

---

## SPRINT GOALS

1. ✅ Implement 5 form input components (TextInput, Select, Checkbox, Radio, DatePicker)
2. ✅ Build Modal/Dialog system with service API
3. ✅ Create AsyncContent loading component
4. ✅ Implement Notification service + UI
5. ✅ Build server-side DataTable component
6. ✅ 100% unit test coverage (50+ tests)
7. ✅ Complete API documentation

**Total Components:** 5 + 3 services = 8 deliverables  
**Estimated Tests:** 50+ unit + integration tests  
**Success Metric:** All components render correctly, all tests pass, zero defects

---

## DAILY BREAKDOWN

### **Day 1: Project Setup & BaseFormComponent (3 hrs)**

**Tasks:**
1. Create folder structure (Forms/, Async/, Feedback/, Services/)
2. Create BaseFormComponent base class
3. Set up component testing framework (bUnit)
4. Create Models/FormField.cs, Models/DataRequest.cs
5. Set up CSS variables for theming

**Deliverables:**
- ✅ src/Components/Base/BaseFormComponent.cs
- ✅ src/Models/FormField.cs
- ✅ src/Models/DataRequest.cs
- ✅ src/Styles/variables.css
- ✅ tests/Components/Forms/TextInputComponentTests.cs (empty, ready)

**File Checklist:**
```
✅ Components/Base/BaseFormComponent.cs
✅ Components/Forms/ [FOLDER]
✅ Services/ [UPDATE: Add to folder]
✅ Models/FormField.cs
✅ Models/DataRequest.cs
✅ Models/DataResult<T>.cs
✅ Styles/variables.css
✅ tests/Components/Forms/ [FOLDER]
```

---

### **Day 2-3: Form Input Components (5 hrs)**

**Components to Implement:**

#### TextInputComponent
- Text, email, password, number types
- Label, placeholder, help text
- Error display with validation
- Disabled state
- CSS binding for error class
- Tests: 4 unit tests

#### SelectComponent
- Dropdown from SelectOption list
- Label, placeholder
- Error display
- Disabled state
- Tests: 3 unit tests

#### CheckboxComponent
- Single checkbox
- Label, disabled state
- Tests: 2 unit tests

#### RadioComponent
- Radio group
- Label per option
- Selected value binding
- Tests: 3 unit tests

**Files to Create:**
```
✅ Components/Forms/TextInputComponent.razor
✅ Components/Forms/SelectComponent.razor
✅ Components/Forms/CheckboxComponent.razor
✅ Components/Forms/RadioComponent.razor
✅ Models/SelectOption.cs
✅ tests/Components/Forms/TextInputComponentTests.cs
✅ tests/Components/Forms/SelectComponentTests.cs
✅ tests/Components/Forms/CheckboxComponentTests.cs
✅ tests/Components/Forms/RadioComponentTests.cs
```

**Test Count:** 12 unit tests

---

### **Day 4: DatePicker Component (3 hrs)**

**Features:**
- Date input with calendar popup
- Min/max date validation
- Format: ISO 8601 (YYYY-MM-DD)
- Label, placeholder, disabled
- Error display
- Focus management for accessibility

**Files:**
```
✅ Components/Forms/DatePickerComponent.razor
✅ Components/Forms/DatePickerComponent.razor.cs
✅ wwwroot/js/datepicker.js
✅ tests/Components/Forms/DatePickerComponentTests.cs
```

**Test Count:** 4 unit tests

---

### **Day 5: Modal System with Service (4 hrs)**

**ModalComponent:**
- Open/close animation
- Backdrop click handling (optional)
- Header, body, footer sections
- Focus trap (accessibility)
- Escape key to close

**ModalService:**
- Show/close API
- Task-based result handling
- State management
- Event notifications

**Files:**
```
✅ Components/Feedback/ModalComponent.razor
✅ Services/IModalService.cs
✅ Services/ModalService.cs
✅ Models/ModalOptions.cs
✅ Models/ModalResult.cs
✅ tests/Services/ModalServiceTests.cs
✅ tests/Components/Feedback/ModalComponentTests.cs
```

**Test Count:** 6 unit tests + 2 integration tests

---

### **Day 6: AsyncContent & Skeleton (3 hrs)**

**AsyncContentComponent:**
- Generic `<TItem>`
- Load/success/error states
- Template slots (LoadingTemplate, SuccessTemplate, ErrorTemplate)
- Auto-refresh capability

**SkeletonComponent:**
- Configurable count and height
- Shimmer animation
- Loading placeholder

**Files:**
```
✅ Components/Async/AsyncContentComponent.razor
✅ Components/Async/SkeletonComponent.razor
✅ Models/AsyncState.cs
✅ tests/Components/Async/AsyncContentComponentTests.cs
✅ tests/Components/Async/SkeletonComponentTests.cs
```

**Test Count:** 5 unit tests

---

### **Day 7: Notification Service & Container (3 hrs)**

**NotificationService:**
- ShowSuccessAsync(), ShowErrorAsync(), ShowWarningAsync(), ShowInfoAsync()
- Configurable duration
- Channel-based notification stream

**NotificationContainer:**
- Renders notifications
- Auto-dismiss after duration
- Manual dismiss button
- Queue management

**Files:**
```
✅ Services/INotificationService.cs
✅ Services/NotificationService.cs
✅ Models/Notification.cs
✅ Models/NotificationType.cs
✅ Components/Feedback/NotificationContainer.razor
✅ tests/Services/NotificationServiceTests.cs
✅ tests/Components/Feedback/NotificationContainerTests.cs
```

**Test Count:** 6 unit tests

---

### **Day 8-9: Server-Side DataTable (6 hrs)**

**DataTableComponent:**
- Generic `<TItem>`
- Server-side pagination (via OnLoadData callback)
- Sorting (click column header)
- Filtering (search box)
- Loading states (SkeletonComponent)
- Row click callback
- Responsive layout

**Features:**
- DataRequest model (page, pageSize, sortBy, sortDirection, filters)
- DataResult<T> model (items, totalCount, totalPages)
- Error handling (ErrorTemplate)
- No data state (AlertComponent)

**Files:**
```
✅ Components/DataDisplay/DataTableComponent.razor
✅ Components/DataDisplay/DataTableComponent.razor.cs
✅ Components/DataDisplay/PaginationComponent.razor
✅ Models/DataTableColumn.cs
✅ tests/Components/DataDisplay/DataTableComponentTests.cs
✅ tests/Components/DataDisplay/DataTableIntegrationTests.cs
```

**Test Count:** 8 unit + 4 integration tests

---

### **Day 10: Testing, Documentation & Integration (4 hrs)**

**Tasks:**
1. Run all tests (50+ tests must pass)
2. Calculate code coverage (target 85%+)
3. Write XML documentation for all components
4. Create DI registration extension
5. Update Program.cs with AddSmartWorkzComponentLibrary()
6. Create integration test for full Form → Modal → DataTable workflow
7. Final code review

**Documentation:**
```
✅ docs/api/forms.md
✅ docs/api/feedback.md
✅ docs/api/async.md
✅ docs/api/data-display.md
✅ docs/examples/form-validation.md
✅ docs/examples/modal-workflow.md
✅ docs/examples/data-table-server-side.md
```

---

## COMPONENT SPECIFICATIONS

### TextInputComponent

**Parameters:**
```csharp
[Parameter] public required string Value { get; set; }
[Parameter] public EventCallback<string> ValueChanged { get; set; }
[Parameter] public string? Label { get; set; }
[Parameter] public string? Placeholder { get; set; }
[Parameter] public string? HelpText { get; set; }
[Parameter] public string Type { get; set; } = "text";
[Parameter] public bool IsDisabled { get; set; }
[Parameter] public List<string>? Errors { get; set; }
[Parameter] public string? CustomCssClass { get; set; }
[Parameter] public EventCallback OnBlur { get; set; }
```

**Rendering:**
```html
<div class="sw-form-group {error}">
  <label>@Label</label>
  <input type="@Type" @bind-value="@Value" />
  {errors}
  <small>@HelpText</small>
</div>
```

**Tests:**
- ✅ Renders with label
- ✅ Updates value on change
- ✅ Displays errors with is-invalid class
- ✅ Respects disabled state

---

### SelectComponent

**Parameters:**
```csharp
[Parameter] public required string SelectedValue { get; set; }
[Parameter] public EventCallback<string> SelectedValueChanged { get; set; }
[Parameter] public string? Label { get; set; }
[Parameter] public string? Placeholder { get; set; }
[Parameter] public List<SelectOption>? Items { get; set; }
[Parameter] public bool IsDisabled { get; set; }
```

**Tests:**
- ✅ Renders options
- ✅ Updates selected value
- ✅ Displays placeholder
- ✅ Respects disabled state

---

### CheckboxComponent

**Parameters:**
```csharp
[Parameter] public required bool IsChecked { get; set; }
[Parameter] public EventCallback<bool> IsCheckedChanged { get; set; }
[Parameter] public string? Label { get; set; }
[Parameter] public bool IsDisabled { get; set; }
```

**Tests:**
- ✅ Renders checkbox with label
- ✅ Updates checked state
- ✅ Unique ID generation

---

### ModalComponent

**Parameters:**
```csharp
[Parameter] public required string Title { get; set; }
[Parameter] public RenderFragment? ChildContent { get; set; }
[Parameter] public RenderFragment? Footer { get; set; }
[Parameter] public bool IsVisible { get; set; }
[Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
[Parameter] public bool CanBackdropClose { get; set; } = true;
```

**Methods:**
```csharp
public async Task Open() { ... }
public async Task Close() { ... }
```

**Tests:**
- ✅ Opens/closes
- ✅ Backdrop click closes (if enabled)
- ✅ Escape key closes
- ✅ Focus trap

---

### AsyncContentComponent<TItem>

**Parameters:**
```csharp
[Parameter] public required Task<TItem?> Data { get; set; }
[Parameter] public RenderFragment? LoadingTemplate { get; set; }
[Parameter] public RenderFragment<TItem>? SuccessTemplate { get; set; }
[Parameter] public RenderFragment<Exception>? ErrorTemplate { get; set; }
```

**States:**
- Loading → show LoadingTemplate
- Success → show SuccessTemplate with data
- Error → show ErrorTemplate with exception

**Tests:**
- ✅ Shows loading state
- ✅ Shows success state with data
- ✅ Shows error state on exception
- ✅ Refetches on parameter change

---

### NotificationService

**Methods:**
```csharp
Task ShowSuccessAsync(string message, int durationMs = 3000);
Task ShowErrorAsync(string message, int durationMs = 5000);
Task ShowWarningAsync(string message, int durationMs = 4000);
Task ShowInfoAsync(string message, int durationMs = 3000);
IAsyncEnumerable<Notification> GetNotifications();
```

**Usage:**
```csharp
@inject INotificationService notificationService

await notificationService.ShowSuccessAsync("User created successfully");
```

---

### DataTableComponent<TItem>

**Parameters:**
```csharp
[Parameter] public required Func<DataRequest, Task<DataResult<TItem>>> OnLoadData { get; set; }
[Parameter] public IEnumerable<DataTableColumn> Columns { get; set; } = new();
[Parameter] public int PageSize { get; set; } = 50;
[Parameter] public bool ShowSearch { get; set; } = true;
[Parameter] public EventCallback<TItem> OnRowClick { get; set; }
```

**Features:**
- Server-side pagination
- Sorting (click header)
- Searching (input box)
- Loading states
- Empty state

---

## TESTING STRATEGY

### Unit Tests (40+ tests)

```csharp
[TestFixture]
public class TextInputComponentTests
{
    [Test]
    public async Task TextInput_UpdatesValueOnChange() { }
    
    [Test]
    public void TextInput_DisplaysErrors() { }
    
    [Test]
    public void TextInput_AppliesErrorClass() { }
    
    [Test]
    public void TextInput_RespectsDisabledState() { }
}
```

### Integration Tests (10+ tests)

```csharp
[TestFixture]
public class FormIntegrationTests
{
    [Test]
    public async Task Form_SubmitsValidData() { }
    
    [Test]
    public async Task Modal_ShowsFormAndSubmits() { }
    
    [Test]
    public async Task DataTable_LoadsAndDisplaysData() { }
}
```

### Coverage Target

- **Line Coverage:** 85%+
- **Branch Coverage:** 80%+
- **Critical Paths:** 100%

---

## CODE REVIEW CHECKLIST

### Before Committing

- ✅ All tests pass (dotnet test)
- ✅ Code coverage 85%+ (coverlet)
- ✅ XML documentation complete
- ✅ No compiler warnings
- ✅ Nullable reference types enabled
- ✅ No hardcoded values
- ✅ Accessibility: ARIA labels on inputs
- ✅ No magic numbers
- ✅ Consistent naming conventions
- ✅ Max line length 120 chars

### After Merging

- ✅ CI pipeline passes (GitHub Actions)
- ✅ No breaking changes to existing APIs
- ✅ Deployment to staging successful
- ✅ Smoke tests pass

---

## DEFINITION OF DONE

Each component is **Done** when:

1. **Code Complete**
   - ✅ Component implemented with all parameters
   - ✅ No TODO comments
   - ✅ All public methods documented

2. **Tested**
   - ✅ Unit tests written (4+ per component)
   - ✅ All tests passing
   - ✅ Edge cases covered
   - ✅ Code coverage 85%+

3. **Documented**
   - ✅ XML documentation on all public members
   - ✅ Usage example in code comment
   - ✅ API doc created (forms.md, etc)

4. **Reviewed**
   - ✅ Code review approved
   - ✅ No critical/high issues
   - ✅ Accessibility verified

5. **Integrated**
   - ✅ Registered in DI container
   - ✅ Added to _Imports.razor
   - ✅ No regressions in existing features

---

## GIT COMMIT STRATEGY

**One commit per component/feature:**

```bash
# Day 2: Form inputs
git commit -m "feat: add TextInput, Select, Checkbox, Radio components"

# Day 4: DatePicker
git commit -m "feat: add DatePickerComponent with calendar UI"

# Day 5: Modal
git commit -m "feat: add ModalComponent and ModalService"

# Day 6: Async
git commit -m "feat: add AsyncContentComponent and SkeletonComponent"

# Day 7: Notifications
git commit -m "feat: add NotificationService with container component"

# Day 8-9: DataTable
git commit -m "feat: add DataTableComponent with server-side paging"

# Day 10: Tests & Docs
git commit -m "test: add 50+ unit tests for Phase 1 components"
git commit -m "docs: add API documentation for forms, async, feedback"
```

---

## RISK MITIGATION

| Risk | Mitigation |
|------|-----------|
| **Scope creep** | Daily standup to track scope |
| **Test delays** | TDD: write tests before code |
| **Accessibility gaps** | Review WCAG 2.1 AA during code review |
| **Integration issues** | Integration tests on Day 10 |
| **Documentation falls behind** | Doc-as-you-go (XML + markdown) |

---

## SUCCESS METRICS (Day 10)

| Metric | Target | How to Measure |
|--------|--------|----------------|
| **All tests passing** | 100% | `dotnet test` |
| **Code coverage** | 85%+ | `coverlet` report |
| **Components complete** | 5/5 | All in src/ |
| **Services complete** | 3/3 | Notification, Modal, Validation |
| **Documentation** | 100% | XML + markdown |
| **Zero defects** | Bug-free | Code review approval |

---

## DAILY STANDUP TEMPLATE

```
Date: [DATE]
Duration: 10 min

✅ Completed Yesterday:
- [Task completed]

🚧 In Progress Today:
- [Current task]
- [Blockers?]

📋 Plan for Tomorrow:
- [Next task]

🔴 Blockers:
- [If any]

📊 Progress:
- [X/10 components done]
```

---

## DELIVERABLES CHECKLIST

### Code
- [ ] TextInputComponent.razor
- [ ] SelectComponent.razor
- [ ] CheckboxComponent.razor
- [ ] RadioComponent.razor
- [ ] DatePickerComponent.razor
- [ ] ModalComponent.razor
- [ ] ModalService.cs
- [ ] AsyncContentComponent.razor
- [ ] SkeletonComponent.razor
- [ ] NotificationService.cs
- [ ] NotificationContainer.razor
- [ ] DataTableComponent.razor
- [ ] PaginationComponent.razor

### Models
- [ ] FormField.cs
- [ ] DataRequest.cs
- [ ] DataResult<T>.cs
- [ ] SelectOption.cs
- [ ] ModalOptions.cs
- [ ] ModalResult.cs
- [ ] Notification.cs
- [ ] DataTableColumn.cs

### Services
- [ ] IModalService.cs
- [ ] INotificationService.cs
- [ ] (Update) IValidationService.cs

### Tests
- [ ] 50+ unit tests (all passing)
- [ ] 4+ integration tests
- [ ] 85%+ code coverage

### Documentation
- [ ] XML docs on all public APIs
- [ ] docs/api/forms.md
- [ ] docs/api/feedback.md
- [ ] docs/api/async.md
- [ ] docs/api/data-display.md
- [ ] docs/examples/form-validation.md
- [ ] docs/examples/modal-workflow.md
- [ ] docs/examples/data-table-server-side.md

### Configuration
- [ ] DI registration (AddSmartWorkzComponentLibrary)
- [ ] _Imports.razor updated
- [ ] Program.cs configured
- [ ] CSS variables defined

---

## TIMELINE SUMMARY

| Day | Component | Effort | Tests | Status |
|-----|-----------|--------|-------|--------|
| 1 | Setup + BaseFormComponent | 3h | 0 | 🟡 Pending |
| 2-3 | Form Inputs (4 components) | 5h | 12 | 🟡 Pending |
| 4 | DatePicker | 3h | 4 | 🟡 Pending |
| 5 | Modal System | 4h | 8 | 🟡 Pending |
| 6 | Async + Skeleton | 3h | 5 | 🟡 Pending |
| 7 | Notifications | 3h | 6 | 🟡 Pending |
| 8-9 | DataTable + Pagination | 6h | 12 | 🟡 Pending |
| 10 | Tests + Documentation | 4h | 3+ | 🟡 Pending |

**Total:** 31 hours (2 weeks)  
**Tests:** 50+ (all required)

---

## NEXT: START DEVELOPMENT

Ready to begin? Start with **Day 1: Setup** → Create folder structure, BaseFormComponent, test framework.

