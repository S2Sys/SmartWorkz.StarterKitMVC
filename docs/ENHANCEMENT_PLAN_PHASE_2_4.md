# SmartWorkz.Core.Web - Enhancement Plan (Phase 2-4)

**Status:** Phase 1 ✅ COMPLETE
**Next:** Phase 2 - Modal & Overlay Components

---

## Phase Overview

| Phase | Components | Timeline | Effort |
|-------|-----------|----------|--------|
| ✅ **Phase 1** | Card, Dashboard, Table, Tabs, Accordion, Tree, Timeline | Complete | 51 hours |
| 📅 **Phase 2** | Modal, Drawer, Tooltip, Popover, Toast | 1-2 weeks | 40 hours |
| 📅 **Phase 3** | DatePicker, TimePicker, ColorPicker, RangeSlider, Tags, Autocomplete | 2-3 weeks | 60 hours |
| 📅 **Phase 4** | Unit Tests (100+), Demo Site, Integration Tests | 2 weeks | 50 hours |

**Total Effort:** 201 hours (~1 month full-time, 2-3 months part-time)

---

## Phase 2: Modal & Overlay Components (NEXT)

### 2.1 ModalDialogComponent
**Purpose:** Display modal dialogs with customizable size and actions
**Priority:** HIGH
**Effort:** 8 hours

#### Features
- [ ] Customizable size (small, medium, large, full)
- [ ] Header with title and close button
- [ ] Body with flexible content
- [ ] Footer with action buttons
- [ ] Backdrop click handling
- [ ] Scrollable body for long content
- [ ] Keyboard navigation (ESC to close)
- [ ] Focus management
- [ ] Nested modal support
- [ ] Callback events (OnOpen, OnClose, OnAction)

#### Code Structure
```
Components/Overlay/
├── ModalDialogComponent.razor
├── ModalDialogComponent.razor.cs
└── Models/
    └── ModalOptions.cs
```

#### Example Usage
```razor
<sw:Modal Title="Confirm Delete"
          Size="ModalSize.Medium"
          IsOpen="@showModal"
          OnClose="@HandleClose">
    <ModalBody>Are you sure?</ModalBody>
    <ModalFooter>
        <button @onclick="@HandleConfirm">Confirm</button>
        <button @onclick="@HandleCancel">Cancel</button>
    </ModalFooter>
</sw:Modal>
```

---

### 2.2 DrawerComponent
**Purpose:** Side drawer/sidebar for navigation and forms
**Priority:** HIGH
**Effort:** 8 hours

#### Features
- [ ] Position (left, right, top, bottom)
- [ ] Customizable width/height
- [ ] Backdrop toggle
- [ ] Header with title
- [ ] Scrollable body
- [ ] Footer section
- [ ] Close button
- [ ] Overlay effect
- [ ] Animation
- [ ] Keyboard ESC support

#### Example Usage
```razor
<sw:Drawer Title="Settings"
           Position="DrawerPosition.Right"
           IsOpen="@showDrawer"
           OnClose="@HandleDrawerClose">
    <DrawerBody><!-- Content --></DrawerBody>
</sw:Drawer>
```

---

### 2.3 TooltipComponent
**Purpose:** Contextual information on hover
**Priority:** MEDIUM
**Effort:** 6 hours

#### Features
- [ ] Position (top, bottom, left, right)
- [ ] Content text or template
- [ ] Trigger on hover/focus/click
- [ ] Delay before show/hide
- [ ] Auto-hide timeout
- [ ] Theme (light/dark)
- [ ] Arrow indicator
- [ ] Keyboard accessible

#### Example Usage
```razor
<sw:Tooltip Content="Save changes" Position="TooltipPosition.Top">
    <button>Save</button>
</sw:Tooltip>
```

---

### 2.4 PopoverComponent
**Purpose:** Rich content popover with trigger control
**Priority:** MEDIUM
**Effort:** 8 hours

#### Features
- [ ] Position (top, bottom, left, right)
- [ ] Header with title
- [ ] Body content
- [ ] Close button
- [ ] Dismissable (click outside)
- [ ] Trigger modes (click, hover, focus)
- [ ] Animations
- [ ] Theme support

#### Example Usage
```razor
<sw:Popover Title="Info"
            Position="PopoverPosition.Top"
            Trigger="PopoverTrigger.Click">
    <PopoverContent>
        <p>This is a rich popover content</p>
    </PopoverContent>
    <button>Show Popover</button>
</sw:Popover>
```

---

### 2.5 ToastNotificationComponent
**Purpose:** Non-intrusive notifications
**Priority:** HIGH
**Effort:** 10 hours

#### Features
- [ ] Message types (success, error, warning, info)
- [ ] Auto-dismiss with timeout
- [ ] Position (top-right, bottom-left, etc.)
- [ ] Icon for message type
- [ ] Progress bar for timeout
- [ ] Action button
- [ ] Close button
- [ ] Toast queue management
- [ ] Accessible announcements

#### Example Usage
```razor
<sw:Toast Message="Changes saved successfully"
          Type="ToastType.Success"
          Duration="3000" />

// Or via service
@inject IToastService ToastService
await ToastService.ShowAsync("Success!", ToastType.Success);
```

---

## Phase 3: Advanced Form Components

### 3.1 DatePickerComponent
**Priority:** HIGH
**Effort:** 12 hours
**Features:** Calendar UI, Range selection, Keyboard nav, Localization

### 3.2 TimePickerComponent
**Priority:** MEDIUM
**Effort:** 8 hours
**Features:** Hour/minute input, Format options, Validation

### 3.3 ColorPickerComponent
**Priority:** MEDIUM
**Effort:** 10 hours
**Features:** Gradient picker, Palette, RGB/Hex input, Preview

### 3.4 RangeSliderComponent
**Priority:** LOW
**Effort:** 8 hours
**Features:** Min/max, Step, Labels, Tooltip, Keyboard nav

### 3.5 TagsInputComponent
**Priority:** HIGH
**Effort:** 10 hours
**Features:** Add/remove tags, Autocomplete, Validation, Custom formatting

### 3.6 AutocompleteComponent
**Priority:** HIGH
**Effort:** 12 hours
**Features:** Filtering, Async search, Keyboard nav, Grouping

---

## Phase 4: Testing & Documentation

### 4.1 Unit Tests
**Priority:** CRITICAL
**Effort:** 30 hours

#### Component Tests (70 tests total)
- [ ] Card component tests (8 tests)
- [ ] Dashboard component tests (8 tests)
- [ ] Table component tests (15 tests)
- [ ] Tabs component tests (10 tests)
- [ ] Accordion component tests (8 tests)
- [ ] Tree component tests (12 tests)
- [ ] Timeline component tests (9 tests)

#### Service Tests (30+ tests)
- [ ] TableDataService tests (10 tests)
- [ ] TreeViewService tests (10 tests)
- [ ] DataFormatterService tests (10 tests)

### 4.2 Integration Tests
**Priority:** HIGH
**Effort:** 15 hours

- [ ] Multi-component interactions
- [ ] State management
- [ ] Event propagation
- [ ] Service integration

### 4.3 Demo Application
**Priority:** MEDIUM
**Effort:** 5 hours

- [ ] Interactive component showcase
- [ ] Real-world examples
- [ ] Performance demo
- [ ] Accessibility demo

---

## Development Checklist Template

Use this checklist for each new component:

```markdown
## [ComponentName] Development

- [ ] Create component.razor file
- [ ] Create component.razor.cs code-behind
- [ ] Create model classes
- [ ] Add XML documentation
- [ ] Write 8-10 unit tests
- [ ] Create usage examples
- [ ] Add to usage guide
- [ ] Test accessibility (keyboard, ARIA)
- [ ] Test responsive design
- [ ] Code review
- [ ] Merge to main branch
```

---

## Quality Metrics to Maintain

### Code Quality
- ✅ 100% XML documentation
- ✅ Zero compiler warnings
- ✅ #nullable enable on all files
- ✅ No hardcoded strings

### Testing
- ✅ 80%+ code coverage
- ✅ All public methods tested
- ✅ Edge cases covered
- ✅ Integration tests included

### Accessibility
- ✅ WCAG 2.1 AA compliant
- ✅ Full keyboard navigation
- ✅ ARIA labels on interactive elements
- ✅ Color contrast verified

### Documentation
- ✅ API reference complete
- ✅ Usage examples provided
- ✅ Architecture documented
- ✅ Changelog maintained

---

## Dependencies to Add

### For Phase 2 (Modal & Overlay)
- No new dependencies required
- Uses existing Bootstrap 5
- Can optionally add AnimationLibrary for advanced effects

### For Phase 3 (Form Components)
- Consider: DatePicker.js or Flatpickr
- Consider: Color picker library
- Consider: Autocomplete.js (optional)

### For Phase 4 (Testing)
- xUnit (already present)
- Moq (already present)
- bUnit (Blazor testing)
- Playwright (E2E testing - optional)

---

## Architecture Guidelines

### Component Naming
```
[Entity]Component.razor          // Main component
[Entity]Component.razor.cs       // Code-behind
Models/[Entity]Options.cs        // Configuration
Models/[Entity]EventArgs.cs      // Event arguments
Services/I[Entity]Service.cs     // Service interface
Services/[Entity]Service.cs      // Service implementation
```

### File Locations
```
Components/
├── Data/           (Existing - Phase 1)
│   ├── CardComponent.razor
│   ├── DashboardComponent.razor
│   ├── TableComponent.razor
│   ├── TabsComponent.razor
│   ├── AccordionComponent.razor
│   ├── TreeViewComponent.razor
│   ├── TimelineComponent.razor
│   └── Models/
├── Overlay/        (Phase 2)
│   ├── ModalDialogComponent.razor
│   ├── DrawerComponent.razor
│   ├── TooltipComponent.razor
│   ├── PopoverComponent.razor
│   ├── ToastComponent.razor
│   └── Models/
└── Forms/          (Phase 3)
    ├── DatePickerComponent.razor
    ├── TimePickerComponent.razor
    ├── ColorPickerComponent.razor
    ├── RangeSliderComponent.razor
    ├── TagsInputComponent.razor
    ├── AutocompleteComponent.razor
    └── Models/
```

---

## Git Commit Message Format

Use this format for commit messages:

```
feat: add [ComponentName] component

- Brief description of what was added
- Key features listed
- Related documentation updates

[Brief test summary]

https://claude.ai/code/session_[SESSION_ID]
```

Example:
```
feat: add ModalDialogComponent with backdrop and animations

- Customizable size (small, medium, large)
- Header, body, footer sections
- Keyboard navigation (ESC to close)
- Focus management for accessibility
- 10 unit tests with 95% coverage

https://claude.ai/code/session_012bwn9tpF4sNPukCo4GC2B2
```

---

## Session Re-review Checklist

Before starting a new session, run through this checklist:

- [ ] Check git status (working tree clean)
- [ ] Verify all previous changes are committed
- [ ] Review FEATURED_PAGES_SUMMARY.md
- [ ] Check Phase 1 completion (all 7 components working)
- [ ] Review test coverage requirements
- [ ] Verify documentation is up-to-date
- [ ] Check browser compatibility
- [ ] Run accessibility audit
- [ ] Update ENHANCEMENT_PLAN_PHASE_2_4.md if needed
- [ ] Plan next component to implement

---

## Maintenance & Support

### Bug Fixes
If bugs are found:
1. Create issue with reproduction steps
2. Add test case that fails
3. Fix the bug
4. Verify test passes
5. Add to bug fix commit message

### Enhancement Requests
If enhancements are requested:
1. Document requirement
2. Add to relevant phase
3. Estimate effort
4. Plan implementation
5. Track in this document

---

## Resources & References

- **Bootstrap 5 Docs:** https://getbootstrap.com/
- **Accessibility Guidelines:** https://www.w3.org/WAI/WCAG21/quickref/
- **Blazor Docs:** https://learn.microsoft.com/en-us/aspnet/core/blazor/
- **.NET 9 Docs:** https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9
- **CSS Best Practices:** https://developer.mozilla.org/en-US/docs/Learn/CSS/

---

## Success Criteria for Each Phase

### Phase 2 Success
- [ ] 5 new components implemented
- [ ] All components tested (50+ tests)
- [ ] 100% XML documentation
- [ ] Usage guide updated
- [ ] 0 accessibility issues

### Phase 3 Success
- [ ] 6 form components implemented
- [ ] Form validation working
- [ ] Date/time localization
- [ ] 60+ tests written
- [ ] Complete usage examples

### Phase 4 Success
- [ ] 100+ unit tests
- [ ] 80%+ code coverage
- [ ] Integration tests passing
- [ ] Demo site created
- [ ] All documentation complete

---

**Last Updated:** April 20, 2026
**Version:** 1.0
**Status:** Ready for Phase 2 implementation

---
