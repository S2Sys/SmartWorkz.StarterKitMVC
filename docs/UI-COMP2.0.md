# SmartWorkz.Core.Web | UI Component Library 2.0 Implementation Plan

**Version:** 2.0  
**Status:** Planning  
**Target Release:** 2026-06-30 (8 weeks)  
**Priority:** P0 (Unblocks all feature development)  
**Owners:** Frontend Architecture Team  

---

## EXECUTIVE SUMMARY

SmartWorkz.Core.Web currently has **5 baseline components** (14% coverage). This plan details implementation of **40+ production-grade UI components** to reach **95% coverage** in 8 weeks across 3 phases.

| Phase | Duration | Components | Outcome |
|-------|----------|-----------|---------|
| **Phase 1: Critical** | 2 weeks | Form Inputs, Modal, DataTable, Async, Notifications | Unblocks features |
| **Phase 2: High Priority** | 2 weeks | Pagination, Tabs, File Upload, Validation Summary | Production polish |
| **Phase 3: Production Ready** | 2 weeks | Virtual Scrolling, Keyboard Nav, Dark Mode, Search | Enterprise quality |
| **Phase 4: Documentation** | 2 weeks | Storybook, examples, guides, migration guide | Developer experience |

**Total Investment:** 2 developers × 8 weeks = 16 developer-weeks  
**Expected Outcome:** Enterprise-grade component library with 200+ components/patterns

---

## 1. ARCHITECTURE & DESIGN PRINCIPLES

### 1.1 Component Organization

```
SmartWorkz.Core.Web/
├── src/
│   ├── Components/
│   │   ├── _Imports.razor
│   │   ├── Base/
│   │   │   ├── BaseFormComponent.cs [NEW]
│   │   │   ├── BaseDataComponent.cs [NEW]
│   │   │   └── BasePanelComponent.cs [NEW]
│   │   ├── Forms/ [NEW]
│   │   │   ├── TextInputComponent.razor
│   │   │   ├── SelectComponent.razor
│   │   │   ├── CheckboxComponent.razor
│   │   │   ├── RadioComponent.razor
│   │   │   ├── DatePickerComponent.razor
│   │   │   ├── TextareaComponent.razor
│   │   │   ├── FileInputComponent.razor
│   │   │   └── SearchBoxComponent.razor
│   │   ├── DataDisplay/
│   │   │   ├── DataTableComponent.razor [NEW - SERVER-SIDE]
│   │   │   ├── PaginationComponent.razor [NEW]
│   │   │   ├── GridComponent.razor [EXISTING]
│   │   │   ├── BreadcrumbComponent.razor [NEW]
│   │   │   └── TreeViewComponent.razor [NEW]
│   │   ├── Feedback/
│   │   │   ├── ModalComponent.razor [NEW]
│   │   │   ├── AlertComponent.razor [NEW]
│   │   │   ├── ValidationSummaryComponent.razor [NEW]
│   │   │   └── NotificationContainer.razor [NEW]
│   │   ├── Layout/
│   │   │   ├── TabsComponent.razor [NEW]
│   │   │   ├── AccordionComponent.razor [NEW]
│   │   │   ├── SidebarComponent.razor [NEW]
│   │   │   └── DrawerComponent.razor [NEW]
│   │   ├── Async/
│   │   │   ├── AsyncContentComponent.razor [NEW]
│   │   │   ├── SkeletonComponent.razor [NEW]
│   │   │   ├── SpinnerComponent.razor [NEW]
│   │   │   └── ErrorBoundaryComponent.razor [NEW]
│   │   ├── Upload/
│   │   │   ├── FileUploadComponent.razor [NEW]
│   │   │   └── FileListComponent.razor [NEW]
│   │   ├── Input/
│   │   │   ├── ComboboxComponent.razor [NEW]
│   │   │   ├── TagInputComponent.razor [NEW]
│   │   │   ├── SliderComponent.razor [NEW]
│   │   │   └── RatingComponent.razor [NEW]
│   │   ├── Blazor/
│   │   │   ├── BlazorGridComponent.razor [EXISTING]
│   │   │   ├── BlazorDataTableComponent.razor [NEW]
│   │   │   ├── BlazorFormComponent.razor [NEW]
│   │   │   └── BlazorPageComponent.razor [NEW]
│   │   └── Utilities/
│   │       ├── BadgeComponent.razor [NEW]
│   │       ├── TooltipComponent.razor [NEW]
│   │       ├── PopoverComponent.razor [NEW]
│   │       ├── DropdownComponent.razor [NEW]
│   │       ├── ProgressComponent.razor [NEW]
│   │       └── StepperComponent.razor [NEW]
│   ├── Services/
│   │   ├── IValidationService.cs [EXISTING]
│   │   ├── ValidationService.cs [EXISTING]
│   │   ├── INotificationService.cs [NEW]
│   │   ├── NotificationService.cs [NEW]
│   │   ├── IModalService.cs [NEW]
│   │   ├── ModalService.cs [NEW]
│   │   ├── IThemeService.cs [NEW]
│   │   ├── ThemeService.cs [NEW]
│   │   └── IFileUploadService.cs [NEW]
│   ├── TagHelpers/
│   │   ├── FormGroupTagHelper.cs [EXISTING]
│   │   ├── StatusBadgeTagHelper.cs [EXISTING]
│   │   ├── ButtonTagHelper.cs [NEW]
│   │   ├── BadgeTagHelper.cs [NEW]
│   │   ├── FormBuilderTagHelper.cs [NEW]
│   │   └── NavTagHelper.cs [NEW]
│   ├── Models/
│   │   ├── GridColumn.cs [EXISTING]
│   │   ├── GridOptions.cs [EXISTING]
│   │   ├── SortOrder.cs [EXISTING]
│   │   ├── DataRequest.cs [NEW]
│   │   ├── DataResult<T>.cs [NEW]
│   │   ├── PaginationInfo.cs [NEW]
│   │   ├── NotificationOptions.cs [NEW]
│   │   ├── ModalOptions.cs [NEW]
│   │   ├── FormField.cs [NEW]
│   │   ├── ValidationError.cs [NEW]
│   │   └── FileUploadOptions.cs [NEW]
│   ├── Utilities/
│   │   ├── ComponentExtensions.cs [NEW]
│   │   ├── FormBuilderExtensions.cs [NEW]
│   │   ├── ThemeManager.cs [NEW]
│   │   ├── ValidationExtensions.cs [NEW]
│   │   └── AccessibilityExtensions.cs [NEW]
│   ├── Styles/
│   │   ├── themes/
│   │   │   ├── light.css [NEW]
│   │   │   ├── dark.css [NEW]
│   │   │   └── variables.css [NEW]
│   │   └── components/
│   │       ├── forms.css [NEW]
│   │       ├── tables.css [NEW]
│   │       ├── modals.css [NEW]
│   │       └── utilities.css [NEW]
│   └── wwwroot/
│       ├── js/
│       │   ├── components.js [NEW]
│       │   ├── interop.js [NEW]
│       │   └── accessibility.js [NEW]
│       └── css/
│           └── [CSS compiled from Styles]
├── tests/
│   ├── Components/
│   │   ├── Forms/
│   │   │   ├── TextInputComponentTests.cs [NEW]
│   │   │   ├── SelectComponentTests.cs [NEW]
│   │   │   └── ...
│   │   ├── DataDisplay/
│   │   │   ├── DataTableComponentTests.cs [NEW]
│   │   │   ├── PaginationComponentTests.cs [NEW]
│   │   │   └── ...
│   │   ├── Feedback/
│   │   │   ├── ModalComponentTests.cs [NEW]
│   │   │   └── ...
│   │   └── Services/
│   │       ├── NotificationServiceTests.cs [NEW]
│   │       ├── ModalServiceTests.cs [NEW]
│   │       └── ...
│   ├── Integration/
│   │   ├── FormIntegrationTests.cs [NEW]
│   │   ├── DataTableIntegrationTests.cs [NEW]
│   │   └── ModalIntegrationTests.cs [NEW]
│   └── Performance/
│       ├── VirtualScrollingBenchmarks.cs [NEW]
│       └── RenderingBenchmarks.cs [NEW]
├── docs/
│   ├── COMPONENT-LIBRARY.md [NEW]
│   ├── GETTING-STARTED.md [NEW]
│   ├── MIGRATION-GUIDE.md [NEW]
│   ├── ACCESSIBILITY.md [NEW]
│   ├── THEMING.md [NEW]
│   ├── PERFORMANCE.md [NEW]
│   ├── api/
│   │   ├── forms.md [NEW]
│   │   ├── data-display.md [NEW]
│   │   ├── feedback.md [NEW]
│   │   ├── layout.md [NEW]
│   │   ├── async.md [NEW]
│   │   └── services.md [NEW]
│   └── examples/
│       ├── form-validation.md [NEW]
│       ├── data-table-server-side.md [NEW]
│       ├── modal-workflow.md [NEW]
│       └── theme-customization.md [NEW]
├── storybook/
│   ├── stories/
│   │   ├── Forms.stories.razor [NEW]
│   │   ├── DataDisplay.stories.razor [NEW]
│   │   ├── Feedback.stories.razor [NEW]
│   │   └── ...
│   └── .storybook/
│       ├── main.ts [NEW]
│       └── preview.ts [NEW]
└── SmartWorkz.Core.Web.csproj [MODIFIED]
```

### 1.2 Design Principles

1. **Composability** — Small, focused components that combine
2. **Accessibility First** — WCAG 2.1 AA compliant (ARIA, keyboard nav)
3. **Performance** — Virtual scrolling, lazy loading, memoization
4. **Type Safety** — Strongly-typed parameters; no `object`
5. **Consistency** — Unified patterns across all components
6. **Customization** — CSS variables for theming; slots for content
7. **Documentation** — Every component has examples + live Storybook
8. **Testing** — Unit + integration tests for critical paths

### 1.3 Naming Conventions

**Component Files:**
```
TextInputComponent.razor          (single file, simple logic)
DataTableComponent.razor          (paired with .cs codebehind)
DataTableComponent.razor.cs       (complex logic in codebehind)
```

**Parameters:**
```csharp
[Parameter] public required string Label { get; set; }      // Required params first
[Parameter] public string? Placeholder { get; set; }        // Optional params
[Parameter] public bool IsDisabled { get; set; }            // Boolean: Is/Has prefix
[Parameter] public string CssClass { get; set; } = "";      // Customization
[Parameter] public RenderFragment? ChildContent { get; set; } // Content slots
[Parameter] public EventCallback<T> OnChange { get; set; }   // Event callbacks
```

**CSS Classes:**
```css
.sw-form-group       /* SmartWorkz prefix + semantic name */
.sw-form-group__input
.sw-form-group--error
.sw-form-group--disabled
```

---

## 2. PHASE 1: CRITICAL COMPONENTS (Weeks 1-2)

### 2.1 Form Components (3 Days)

**TextInputComponent**
```csharp
// Components/Forms/TextInputComponent.razor
@inherits BaseFormComponent

<div class="sw-form-group @(HasError ? "sw-form-group--error" : "")">
    @if (!string.IsNullOrEmpty(Label))
    {
        <label class="sw-form-group__label">@Label</label>
    }
    <input type="@Type" 
           class="sw-form-group__input @CustomCssClass"
           value="@Value"
           @onchange="HandleChange"
           @onblur="HandleBlur"
           placeholder="@Placeholder"
           disabled="@IsDisabled"
           aria-label="@(Label ?? Placeholder)"
           aria-invalid="@HasError" />
    @if (HasError)
    {
        <div class="sw-form-group__error" role="alert">
            @foreach (var error in Errors ?? new())
            {
                <span>@error</span>
            }
        </div>
    }
    @if (!string.IsNullOrEmpty(HelpText))
    {
        <small class="sw-form-group__hint">@HelpText</small>
    }
</div>

@code {
    [Parameter] public required string Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? HelpText { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public bool IsDisabled { get; set; }
    [Parameter] public List<string>? Errors { get; set; }
    [Parameter] public string? CustomCssClass { get; set; }
    
    private bool HasError => Errors?.Any() == true;
    
    private async Task HandleChange(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? "";
        await ValueChanged.InvokeAsync(newValue);
    }
    
    private async Task HandleBlur()
    {
        // Optional: trigger validation on blur
        await OnBlur.InvokeAsync();
    }
    
    [Parameter]
    public EventCallback OnBlur { get; set; }
}
```

**SelectComponent**
```csharp
// Components/Forms/SelectComponent.razor
@inherits BaseFormComponent

<div class="sw-form-group">
    @if (!string.IsNullOrEmpty(Label))
    {
        <label class="sw-form-group__label">@Label</label>
    }
    <select class="sw-form-group__input"
            @onchange="HandleChange"
            disabled="@IsDisabled"
            aria-label="@Label">
        @if (!string.IsNullOrEmpty(Placeholder))
        {
            <option value="">@Placeholder</option>
        }
        @foreach (var item in Items ?? new())
        {
            <option value="@item.Value" selected="@(item.Value == SelectedValue)">
                @item.Text
            </option>
        }
    </select>
</div>

@code {
    [Parameter] public required string SelectedValue { get; set; }
    [Parameter] public EventCallback<string> SelectedValueChanged { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public List<SelectOption>? Items { get; set; }
    [Parameter] public bool IsDisabled { get; set; }
    
    private async Task HandleChange(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? "";
        await SelectedValueChanged.InvokeAsync(newValue);
    }
}

public class SelectOption
{
    public required string Text { get; set; }
    public required string Value { get; set; }
}
```

**CheckboxComponent**
```csharp
// Components/Forms/CheckboxComponent.razor
@inherits BaseFormComponent

<div class="sw-checkbox">
    <input type="checkbox"
           id="@CheckboxId"
           class="sw-checkbox__input"
           checked="@IsChecked"
           @onchange="HandleChange"
           disabled="@IsDisabled"
           aria-label="@Label" />
    @if (!string.IsNullOrEmpty(Label))
    {
        <label class="sw-checkbox__label" for="@CheckboxId">
            @Label
        </label>
    }
</div>

@code {
    [Parameter] public required bool IsChecked { get; set; }
    [Parameter] public EventCallback<bool> IsCheckedChanged { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool IsDisabled { get; set; }
    
    private string CheckboxId => $"checkbox_{Guid.NewGuid():N}";
    
    private async Task HandleChange(ChangeEventArgs e)
    {
        var newValue = (bool)(e.Value ?? false);
        await IsCheckedChanged.InvokeAsync(newValue);
    }
}
```

### 2.2 Modal System (2 Days)

**ModalComponent**
```csharp
// Components/Feedback/ModalComponent.razor
@namespace SmartWorkz.Core.Web.Components

<div class="sw-modal-overlay @(IsVisible ? "sw-modal-overlay--visible" : "")"
     @onclick="HandleBackdropClick"
     role="presentation">
    <div class="sw-modal" role="dialog" aria-modal="true" aria-labelledby="@HeaderId">
        <div class="sw-modal__header">
            <h2 id="@HeaderId" class="sw-modal__title">@Title</h2>
            <button type="button"
                    class="sw-modal__close"
                    @onclick="Close"
                    aria-label="Close dialog">
                ×
            </button>
        </div>
        <div class="sw-modal__body">
            @ChildContent
        </div>
        @if (HasFooter)
        {
            <div class="sw-modal__footer">
                @Footer
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public required string Title { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Footer { get; set; }
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public bool CanBackdropClose { get; set; } = true;
    [Parameter] public string? CustomCssClass { get; set; }
    
    private string HeaderId => $"modal_header_{Guid.NewGuid():N}";
    private bool HasFooter => Footer != null;
    
    public async Task Open()
    {
        await IsVisibleChanged.InvokeAsync(true);
    }
    
    public async Task Close()
    {
        await IsVisibleChanged.InvokeAsync(false);
    }
    
    private async Task HandleBackdropClick()
    {
        if (CanBackdropClose)
        {
            await Close();
        }
    }
}
```

**ModalService**
```csharp
// Services/IModalService.cs [NEW]
public interface IModalService
{
    Task<T?> ShowAsync<T>(ModalOptions options, RenderFragment content);
    Task CloseAsync();
}

// Services/ModalService.cs [NEW]
public class ModalService : IModalService
{
    private TaskCompletionSource<object?>? _taskCompletionSource;
    
    public event Action<ModalState>? OnStateChanged;
    
    public async Task<T?> ShowAsync<T>(ModalOptions options, RenderFragment content)
    {
        _taskCompletionSource = new TaskCompletionSource<object?>();
        OnStateChanged?.Invoke(new ModalState { IsVisible = true });
        
        var result = await _taskCompletionSource.Task;
        return (T?)result;
    }
    
    public async Task CloseAsync()
    {
        _taskCompletionSource?.SetResult(null);
        await Task.Delay(300); // Animation
        OnStateChanged?.Invoke(new ModalState { IsVisible = false });
    }
}
```

### 2.3 Async Content Component (2 Days)

**AsyncContentComponent**
```csharp
// Components/Async/AsyncContentComponent.razor
@namespace SmartWorkz.Core.Web.Components
@typeparam TItem

@if (CurrentState == AsyncState.Loading)
{
    @LoadingTemplate
}
else if (CurrentState == AsyncState.Error)
{
    @ErrorTemplate?.Invoke(LastError!)
}
else if (Data != null)
{
    @SuccessTemplate?.Invoke(Data)
}

@code {
    [Parameter] public required Task<TItem?> Data { get; set; }
    [Parameter] public RenderFragment? LoadingTemplate { get; set; }
    [Parameter] public RenderFragment<TItem>? SuccessTemplate { get; set; }
    [Parameter] public RenderFragment<Exception>? ErrorTemplate { get; set; }
    
    private AsyncState CurrentState { get; set; } = AsyncState.Loading;
    private Exception? LastError { get; set; }
    private TItem? LoadedData { get; set; }
    
    protected override async Task OnParametersSetAsync()
    {
        try
        {
            CurrentState = AsyncState.Loading;
            LoadedData = await Data;
            CurrentState = AsyncState.Success;
        }
        catch (Exception ex)
        {
            LastError = ex;
            CurrentState = AsyncState.Error;
        }
    }
}

public enum AsyncState { Loading, Success, Error }
```

**Usage:**
```razor
<AsyncContentComponent Data="@userService.GetUsersAsync()">
    <LoadingTemplate>
        <SkeletonComponent Count="5" Height="50" />
    </LoadingTemplate>
    <SuccessTemplate Context="users">
        <GridComponent Data="@users" />
    </SuccessTemplate>
    <ErrorTemplate Context="error">
        <AlertComponent Type="danger">Error: @error.Message</AlertComponent>
    </ErrorTemplate>
</AsyncContentComponent>
```

### 2.4 Notification Service (1.5 Days)

**NotificationService**
```csharp
// Services/INotificationService.cs [NEW]
public interface INotificationService
{
    Task ShowSuccessAsync(string message, int durationMs = 3000);
    Task ShowErrorAsync(string message, int durationMs = 5000);
    Task ShowWarningAsync(string message, int durationMs = 4000);
    Task ShowInfoAsync(string message, int durationMs = 3000);
    IAsyncEnumerable<Notification> GetNotifications();
}

// Services/NotificationService.cs [NEW]
public class NotificationService : INotificationService
{
    private readonly Channel<Notification> _channel = Channel.CreateUnbounded<Notification>();
    
    public async Task ShowSuccessAsync(string message, int durationMs = 3000)
        => await ShowAsync(new Notification(message, NotificationType.Success, durationMs));
    
    public async Task ShowErrorAsync(string message, int durationMs = 5000)
        => await ShowAsync(new Notification(message, NotificationType.Error, durationMs));
    
    public async Task ShowWarningAsync(string message, int durationMs = 4000)
        => await ShowAsync(new Notification(message, NotificationType.Warning, durationMs));
    
    public async Task ShowInfoAsync(string message, int durationMs = 3000)
        => await ShowAsync(new Notification(message, NotificationType.Info, durationMs));
    
    private async Task ShowAsync(Notification notification)
    {
        await _channel.Writer.WriteAsync(notification);
    }
    
    public async IAsyncEnumerable<Notification> GetNotifications()
    {
        await foreach (var notification in _channel.Reader.ReadAllAsync())
        {
            yield return notification;
        }
    }
}

public record Notification(string Message, NotificationType Type, int DurationMs);
public enum NotificationType { Success, Error, Warning, Info }
```

**NotificationContainer Component**
```razor
@implements IAsyncDisposable
@inject INotificationService NotificationService

<div class="sw-notification-container">
    @foreach (var notification in ActiveNotifications)
    {
        <div class="sw-notification sw-notification--@notification.Type.ToString().ToLower()">
            <span>@notification.Message</span>
            <button @onclick="() => RemoveNotification(notification)" aria-label="Close">×</button>
        </div>
    }
</div>

@code {
    private List<Notification> ActiveNotifications = new();
    private CancellationTokenSource? _cts;
    
    protected override async Task OnInitializedAsync()
    {
        _cts = new CancellationTokenSource();
        _ = ProcessNotifications(_cts.Token);
    }
    
    private async Task ProcessNotifications(CancellationToken ct)
    {
        await foreach (var notification in NotificationService.GetNotifications())
        {
            ActiveNotifications.Add(notification);
            StateHasChanged();
            
            await Task.Delay(notification.DurationMs, ct);
            RemoveNotification(notification);
        }
    }
    
    private void RemoveNotification(Notification notification)
    {
        ActiveNotifications.Remove(notification);
        StateHasChanged();
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

### 2.5 Server-Side DataTable (4 Days)

**DataRequest & DataResult Models**
```csharp
// Models/DataRequest.cs [NEW]
public record DataRequest(
    int Page = 1,
    int PageSize = 50,
    string? SortBy = null,
    string? SortDirection = "asc",
    Dictionary<string, string>? Filters = null);

// Models/DataResult<T>.cs [NEW]
public record DataResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

**DataTableComponent**
```csharp
// Components/DataDisplay/DataTableComponent.razor
@namespace SmartWorkz.Core.Web.Components
@typeparam TItem
@implements IAsyncDisposable

<div class="sw-data-table">
    @if (ShowSearch)
    {
        <div class="sw-data-table__toolbar">
            <SearchBoxComponent @bind-SearchTerm="SearchTerm"
                               OnSearch="HandleSearch"
                               Placeholder="Search..." />
        </div>
    }
    
    @if (IsLoading)
    {
        <SkeletonComponent Count="@PageSize" Height="50" />
    }
    else if (Items?.Any() == true)
    {
        <table class="sw-data-table__table">
            <thead>
                <tr>
                    @foreach (var column in Columns)
                    {
                        <th @onclick="() => HandleSort(column.PropertyName)"
                            style="cursor: pointer;">
                            @column.Header
                            @if (SortBy == column.PropertyName)
                            {
                                <span>@(SortDirection == "asc" ? "↑" : "↓")</span>
                            }
                        </th>
                    }
                </tr>
            </thead>
            <tbody>
                @foreach (var item in Items)
                {
                    <tr @onclick="() => HandleRowClick?.InvokeAsync(item)">
                        @foreach (var column in Columns)
                        {
                            <td>@FormatValue(item, column)</td>
                        }
                    </tr>
                }
            </tbody>
        </table>
        
        <PaginationComponent CurrentPage="@Page"
                            TotalPages="@TotalPages"
                            OnPageChange="HandlePageChange" />
    }
    else
    {
        <AlertComponent Type="info">No data available</AlertComponent>
    }
</div>

@code {
    [Parameter] public required Func<DataRequest, Task<DataResult<TItem>>> OnLoadData { get; set; }
    [Parameter] public IEnumerable<DataTableColumn> Columns { get; set; } = new();
    [Parameter] public int PageSize { get; set; } = 50;
    [Parameter] public bool ShowSearch { get; set; } = true;
    [Parameter] public EventCallback<TItem> HandleRowClick { get; set; }
    
    private IEnumerable<TItem>? Items;
    private int Page { get; set; } = 1;
    private int TotalPages { get; set; }
    private int TotalCount { get; set; }
    private bool IsLoading { get; set; }
    private string? SortBy { get; set; }
    private string SortDirection { get; set; } = "asc";
    private string? SearchTerm { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    private async Task LoadData()
    {
        IsLoading = true;
        var request = new DataRequest(Page, PageSize, SortBy, SortDirection);
        var result = await OnLoadData(request);
        
        Items = result.Items;
        TotalCount = result.TotalCount;
        TotalPages = result.TotalPages;
        IsLoading = false;
    }
    
    private async Task HandlePageChange(int newPage)
    {
        Page = newPage;
        await LoadData();
    }
    
    private async Task HandleSort(string propertyName)
    {
        if (SortBy == propertyName)
            SortDirection = SortDirection == "asc" ? "desc" : "asc";
        else
            SortBy = propertyName;
        
        Page = 1;
        await LoadData();
    }
    
    private async Task HandleSearch()
    {
        Page = 1;
        await LoadData();
    }
    
    private string? FormatValue(TItem item, DataTableColumn column)
    {
        var property = typeof(TItem).GetProperty(column.PropertyName);
        return property?.GetValue(item)?.ToString();
    }
}
```

---

## 3. PHASE 2: HIGH PRIORITY COMPONENTS (Weeks 3-4)

### 3.1 Components to Implement

| Component | Days | Dependency |
|-----------|------|-----------|
| **PaginationComponent** | 1 | None |
| **BreadcrumbComponent** | 1 | None |
| **FormBuilder<T>** | 3 | TextInput, Select, etc |
| **TabsComponent** | 2 | None |
| **AccordionComponent** | 1.5 | None |
| **ValidationSummaryComponent** | 1 | None |
| **FileUploadComponent** | 2 | FileUploadService |
| **TreeViewComponent** | 2 | None |

**Total:** 13.5 days (2 weeks with buffer)

### 3.2 Key Implementation: FormBuilder<T>

```csharp
// Components/Forms/FormBuilderComponent.razor
@namespace SmartWorkz.Core.Web.Components
@typeparam TModel
@implements IAsyncDisposable

<EditForm Model="@Model" OnValidSubmit="HandleSubmit" @ref="form">
    <DataAnnotationsValidator />
    
    <div class="sw-form-builder">
        @foreach (var field in Fields)
        {
            @switch (field.FieldType)
            {
                case FormFieldType.Text:
                case FormFieldType.Email:
                case FormFieldType.Password:
                case FormFieldType.Number:
                    <TextInputComponent 
                        @bind-Value="@GetPropertyValue(field.PropertyName)"
                        Label="@field.Label"
                        Type="@field.FieldType.ToString().ToLower()"
                        Errors="@GetFieldErrors(field.PropertyName)" />
                    break;
                
                case FormFieldType.Select:
                    <SelectComponent 
                        @bind-SelectedValue="@GetPropertyValue(field.PropertyName)"
                        Label="@field.Label"
                        Items="@field.Options"
                        Errors="@GetFieldErrors(field.PropertyName)" />
                    break;
                
                case FormFieldType.Checkbox:
                    <CheckboxComponent 
                        @bind-IsChecked="@GetBoolPropertyValue(field.PropertyName)"
                        Label="@field.Label" />
                    break;
                
                case FormFieldType.Date:
                    <DatePickerComponent 
                        @bind-Value="@GetDatePropertyValue(field.PropertyName)"
                        Label="@field.Label"
                        Errors="@GetFieldErrors(field.PropertyName)" />
                    break;
                
                case FormFieldType.Textarea:
                    <TextareaComponent 
                        @bind-Value="@GetPropertyValue(field.PropertyName)"
                        Label="@field.Label"
                        Rows="4"
                        Errors="@GetFieldErrors(field.PropertyName)" />
                    break;
            }
        }
    </div>
    
    <div class="sw-form-builder__footer">
        <button type="submit" class="btn btn-primary" disabled="@IsSubmitting">
            @(IsSubmitting ? "Submitting..." : "Submit")
        </button>
        @if (OnCancel.HasDelegate)
        {
            <button type="button" class="btn btn-secondary" @onclick="async () => await OnCancel.InvokeAsync()">
                Cancel
            </button>
        }
    </div>
    
    @if (SubmitErrors?.Any() == true)
    {
        <AlertComponent Type="danger">
            <ul>
                @foreach (var error in SubmitErrors)
                {
                    <li>@error</li>
                }
            </ul>
        </AlertComponent>
    }
</EditForm>

@code {
    [Parameter] public required TModel Model { get; set; }
    [Parameter] public required List<FormField> Fields { get; set; }
    [Parameter] public required EventCallback<TModel> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    
    private EditForm? form;
    private bool IsSubmitting { get; set; }
    private List<string>? SubmitErrors { get; set; }
    private Dictionary<string, List<string>> FieldErrors { get; set; } = new();
    
    private async Task HandleSubmit()
    {
        IsSubmitting = true;
        try
        {
            await OnSubmit.InvokeAsync(Model);
            SubmitErrors = null;
        }
        catch (Exception ex)
        {
            SubmitErrors = new() { ex.Message };
        }
        finally
        {
            IsSubmitting = false;
        }
    }
    
    private string? GetPropertyValue(string propertyName)
    {
        return typeof(TModel).GetProperty(propertyName)?.GetValue(Model)?.ToString();
    }
    
    private List<string> GetFieldErrors(string propertyName)
    {
        return FieldErrors.TryGetValue(propertyName, out var errors) ? errors : new();
    }
}
```

---

## 4. PHASE 3: PRODUCTION READY (Weeks 5-6)

### 4.1 Advanced Features

| Feature | Days | Impact |
|---------|------|--------|
| **Virtual Scrolling** | 2 | Handles 100K+ rows |
| **Keyboard Navigation** | 2 | Accessibility WCAG 2.1 AA |
| **Dark Mode Theme System** | 2 | CSS variables + service |
| **Responsive Utilities** | 1.5 | Mobile-first design |
| **Error Boundaries** | 1 | Graceful error handling |
| **Search Component** | 1.5 | Debounced search box |
| **Calendar & DatePicker** | 2 | Full date selection UI |

**Total:** 13.5 days

---

## 5. PHASE 4: DOCUMENTATION & POLISH (Weeks 7-8)

### 5.1 Deliverables

| Artifact | Description | Effort |
|----------|-------------|--------|
| **Storybook** | Interactive component showcase | 3 days |
| **API Documentation** | Auto-generated from XML docs | 2 days |
| **Migration Guide** | Existing → new component library | 2 days |
| **Examples** | Form, DataTable, Modal workflows | 3 days |
| **Accessibility Guide** | WCAG 2.1 AA compliance | 1.5 days |
| **Performance Guide** | Virtual scrolling, lazy loading | 1 day |
| **Theming Guide** | CSS variables, dark mode | 1 day |
| **Testing Guide** | Unit + integration test patterns | 1 day |

---

## 6. TESTING STRATEGY

### 6.1 Unit Tests (Per Component)

```csharp
[TestFixture]
public class TextInputComponentTests
{
    [Test]
    public async Task TextInput_UpdatesValueOnChange()
    {
        // Arrange
        var value = "";
        var cut = RenderComponent<TextInputComponent>(parameters => parameters
            .Add(p => p.Value, value)
            .Add(p => p.ValueChanged, new EventCallback<string>((v) => value = v)));
        
        // Act
        var input = cut.Find("input");
        await input.ChangeAsync(new ChangeEventArgs { Value = "test@example.com" });
        
        // Assert
        Assert.AreEqual("test@example.com", value);
    }
    
    [Test]
    public void TextInput_DisplaysErrors()
    {
        // Arrange
        var cut = RenderComponent<TextInputComponent>(parameters => parameters
            .Add(p => p.Value, "")
            .Add(p => p.Errors, new List<string> { "Email is required" }));
        
        // Assert
        cut.MarkupMatches(@"
            <div class=""sw-form-group sw-form-group--error"">
                <input class=""sw-form-group__input"" />
                <div class=""sw-form-group__error"" role=""alert"">
                    <span>Email is required</span>
                </div>
            </div>
        ");
    }
}
```

### 6.2 Integration Tests

```csharp
[TestFixture]
public class FormIntegrationTests
{
    [Test]
    public async Task FormBuilder_SubmitsValidData()
    {
        // Arrange
        var user = new User();
        var submitted = false;
        
        var cut = RenderComponent<FormBuilderComponent<User>>(parameters => parameters
            .Add(p => p.Model, user)
            .Add(p => p.Fields, new List<FormField>
            {
                new(nameof(User.Email), "Email", FormFieldType.Email),
                new(nameof(User.FirstName), "First Name", FormFieldType.Text)
            })
            .Add(p => p.OnSubmit, new EventCallback<User>((u) => submitted = true)));
        
        // Act
        var form = cut.Find("form");
        await form.SubmitAsync();
        
        // Assert
        Assert.IsTrue(submitted);
    }
}
```

### 6.3 E2E Tests (Playwright)

```csharp
[TestFixture]
public class DataTableE2ETests
{
    [Test]
    public async Task DataTable_FiltersAndPaginates()
    {
        await using var browser = await Playwright.Chromium.LaunchAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        // Arrange
        await page.GotoAsync("https://localhost:5000/data-table-demo");
        
        // Act - Filter
        await page.FillAsync("[placeholder='Search']", "John");
        await page.ClickAsync("button:has-text('Search')");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        var rows = await page.QuerySelectorAllAsync("tbody tr");
        Assert.That(rows, Has.Length.EqualTo(1));
    }
}
```

---

## 7. INTEGRATION & REGISTRATION

### 7.1 DI Registration

```csharp
// Extension method in Program.cs or extension class
public static class SmartWorkzWebServiceCollectionExtensions
{
    public static IServiceCollection AddSmartWorkzComponentLibrary(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IModalService, ModalService>();
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        
        // HttpClient for file uploads
        services.AddHttpClient<IFileUploadService, FileUploadService>();
        
        return services;
    }
}

// Usage in Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSmartWorkzComponentLibrary();
```

### 7.2 Layout Integration (_Layout.cshtml)

```html
<!DOCTYPE html>
<html>
<head>
    <!-- SmartWorkz Component Library styles -->
    <link rel="stylesheet" href="_framework/SmartWorkz.Core.Web/styles/components.css" />
    <link rel="stylesheet" href="_framework/SmartWorkz.Core.Web/themes/light.css" />
    
    <!-- Bootstrap for responsive grid (optional) -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
</head>
<body>
    @RenderBody()
    
    <!-- Notification container -->
    <NotificationContainer />
    
    <!-- Modal container -->
    <ModalContainer />
    
    <!-- SmartWorkz Component Library JS -->
    <script src="_framework/SmartWorkz.Core.Web/js/interop.js"></script>
    <script src="_framework/SmartWorkz.Core.Web/js/accessibility.js"></script>
</body>
</html>
```

---

## 8. MIGRATION STRATEGY

### 8.1 Backwards Compatibility

Existing components remain unchanged during Phase 1-3. Phase 4 includes migration guide:

```csharp
// ❌ OLD (GridComponent - client-side)
<GridComponent Data="@users.AsQueryable()" />

// ✅ NEW (DataTableComponent - server-side)
<DataTableComponent OnLoadData="@LoadUsers" PageSize="50" />

@code {
    private async Task<DataResult<User>> LoadUsers(DataRequest request)
    {
        var result = await userService.GetPagedAsync(request.Page, request.PageSize);
        return new(result.Items, result.TotalCount, request.Page, request.PageSize);
    }
}
```

### 8.2 Feature Flags

Optional feature flags for gradual rollout:

```csharp
[Configuration]
public class ComponentLibraryOptions
{
    public bool UseNewDataTable { get; set; } = false;
    public bool UseNewFormComponents { get; set; } = false;
    public bool EnableDarkMode { get; set; } = true;
}

// Usage
@if (options.UseNewFormComponents)
{
    <FormBuilderComponent ... />
}
else
{
    <LegacyForm ... />
}
```

---

## 9. SUCCESS CRITERIA

### 9.1 Coverage Metrics

| Metric | Target | Current | Gap |
|--------|--------|---------|-----|
| Component Count | 40+ | 5 | 35 |
| Code Coverage | 85%+ | 70% | +15% |
| Test Count | 150+ | 14 | 136+ |
| Documentation | 100% | 60% | 40% |
| Accessibility | WCAG 2.1 AA | None | 100% |
| Performance | <100ms render | Varies | TBD |

### 9.2 Release Criteria

- ✅ All Phase 1-3 components implemented & tested (80+ tests passing)
- ✅ Storybook published with all components documented
- ✅ WCAG 2.1 AA compliance verified (automated + manual audit)
- ✅ Migration guide published & existing apps migrated
- ✅ Performance benchmarks: DataTable <100ms, Modal <50ms
- ✅ Zero breaking changes from v1.0.0 during transition
- ✅ 100% API documentation (XML docs + Storybook)

---

## 10. RESOURCE ALLOCATION

### 10.1 Team Structure

| Role | Count | Responsibility |
|------|-------|-----------------|
| **Lead Component Architect** | 1 | Design, code review, integration |
| **Senior Frontend Developer** | 1 | Implement Phase 1-2 components |
| **Frontend Developer** | 1 | Implement Phase 3 + testing |
| **QA/Test Automation** | 0.5 | E2E tests, performance benchmarks |
| **Technical Writer** | 0.5 | Documentation, Storybook, migration guide |

**Total:** 4 FTE × 2 weeks per phase = 8 FTE-weeks per phase

---

## 11. RISK MITIGATION

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| **Scope Creep** | High | Timeline slip | Sprint-based delivery, strict acceptance criteria |
| **Breaking Changes** | Medium | User migration pain | Feature flags, comprehensive migration guide |
| **Performance Issues** | Medium | User experience | Profiling benchmarks, virtual scrolling |
| **Accessibility Gaps** | Low | Compliance risk | Automated WCAG testing, manual audit |
| **Knowledge Transfer** | Low | Maintenance burden | Comprehensive docs, Storybook, code comments |

---

## 12. TIMELINE & MILESTONES

```
Week 1-2: Phase 1 Critical
├── Day 1-2:   Form Components (TextInput, Select, Checkbox)
├── Day 3-4:   Modal System
├── Day 5-6:   AsyncContent Component
├── Day 7:     Notification Service
└── Day 8-10:  Server-side DataTable
    Status: 🔴 CRITICAL — Unblocks all feature work

Week 3-4: Phase 2 High Priority
├── Day 1:     Pagination Component
├── Day 2:     BreadcrumbComponent
├── Day 3-5:   FormBuilder<T>
├── Day 6-7:   Tabs & Accordion
├── Day 8:     Validation Summary
└── Day 9-10:  File Upload Component
    Status: 🟠 HIGH — Adds polish

Week 5-6: Phase 3 Production Ready
├── Day 1-2:   Virtual Scrolling
├── Day 3-4:   Keyboard Navigation + ARIA
├── Day 5-6:   Dark Mode Theme System
├── Day 7:     Responsive Utilities
├── Day 8-9:   Search Component
└── Day 10:    Calendar & DatePicker
    Status: 🟢 READY — Enterprise quality

Week 7-8: Phase 4 Documentation
├── Day 1-3:   Storybook (50+ component stories)
├── Day 4-5:   Migration Guide + Examples
├── Day 6:     Accessibility Documentation
├── Day 7:     Performance Documentation
└── Day 8:     Launch + rollout
    Status: ✅ SHIPPED

Total: 8 weeks (40 business days)
```

---

## 13. SUCCESS METRICS (POST-LAUNCH)

Track these metrics for 4 weeks after launch:

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Adoption Rate** | 80% of new features use new components | Codebase scanning |
| **Build Time** | <2s per page (with component memoization) | CI/CD metrics |
| **Accessibility Score** | 95+/100 (Lighthouse) | Automated testing |
| **Developer Satisfaction** | 4.5+/5 (survey) | Post-launch survey |
| **Bug Rate** | <0.5% (critical bugs) | Issue tracking |
| **Documentation Completeness** | 100% components documented | Storybook + API docs |

---

## 14. DEPENDENCIES & ASSUMPTIONS

### 14.1 External Dependencies

```xml
<PackageReference Include="HotChocolate.AspNetCore" Version="14.0.0" />
<PackageReference Include="HotChocolate.Types" Version="14.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.2" />
<PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
<PackageReference Include="System.ComponentModel.DataAnnotations" Version="4.7.0" />
```

### 14.2 Assumptions

- ✅ Bootstrap 5.3+ CSS framework available in host app
- ✅ Blazor Server or WebAssembly runtime available
- ✅ .NET 9.0+ runtime on server
- ✅ Team familiar with Razor component patterns
- ✅ Existing apps can adopt new components incrementally

---

## 15. APPENDIX: COMPONENT QUICK REFERENCE

### Form Components
- TextInputComponent (text, email, password, number)
- SelectComponent (dropdown, multi-select variant)
- CheckboxComponent (single + group)
- RadioComponent (group)
- DatePickerComponent (date input + calendar picker)
- TextareaComponent (multiline text)
- FileInputComponent (file upload)

### Data Display
- DataTableComponent (server-side with sort/filter/page)
- PaginationComponent (previous/next + numbered)
- GridComponent (client-side, existing)
- BreadcrumbComponent (navigation path)
- TreeViewComponent (hierarchical data)

### Feedback
- ModalComponent (dialog box)
- AlertComponent (success/error/warning/info)
- ValidationSummaryComponent (all field errors)
- NotificationContainer (toast messages)

### Layout
- TabsComponent (tabbed interface)
- AccordionComponent (collapsible sections)
- SidebarComponent (left navigation)
- DrawerComponent (slide-out panel)

### Async/Loading
- AsyncContentComponent (load/success/error states)
- SkeletonComponent (loading placeholder)
- SpinnerComponent (circular loader)
- ErrorBoundaryComponent (crash boundary)

### Input/Search
- ComboboxComponent (searchable select)
- TagInputComponent (multi-value input)
- SliderComponent (range input)
- RatingComponent (star rating)
- SearchBoxComponent (debounced search)

### Services
- IValidationService (DataAnnotations)
- INotificationService (Toast API)
- IModalService (Dialog API)
- IThemeService (CSS variables)
- IFileUploadService (Chunked upload)

---

## 16. CONCLUSION

This 8-week plan transforms SmartWorkz.Core.Web from a **14% complete baseline library** into a **95% complete enterprise-grade component system** with:

✅ **40+ production components**  
✅ **150+ unit tests** + E2E coverage  
✅ **100% API documentation** + Storybook  
✅ **WCAG 2.1 AA accessibility**  
✅ **High-performance** (virtual scrolling, lazy loading)  
✅ **Dark mode + theming system**  
✅ **Zero breaking changes** to existing APIs  

**Ready to kickoff Phase 1? → Start Week 1 with Form Components + Modal System**

