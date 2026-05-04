# SmartWorkz.Core.Web - Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close 50% feature gap in SmartWorkz.Core.Web by implementing missing 20 UI components, 70+ unit tests, and export functionality to match industry standards.

**Architecture:** 
- Add missing Blazor components (Modal, Dropdown, DatePicker, etc.) following existing GridComponent pattern
- Extend tag helpers for forms and validation
- Implement export services (CSV, Excel, PDF) in Core.External
- Build comprehensive test suite with 80%+ coverage
- Complete XML documentation for all components

**Tech Stack:** Blazor Razor Components, xUnit, Bootstrap 5, CsvHelper, EPPlus, iText7, Selenium WebDriver

**Timeline:** 6-8 weeks, 2-3 developers  
**Effort:** ~90 developer days  
**Priority:** CRITICAL - Blocking web development

---

## Current State vs Target

### Current
- 5 UI components (Grid, ListView, DataViewer, FormGroup, StatusBadge)
- 13 unit tests (30% coverage)
- GraphQL API complete
- 71% XML documentation
- No export functionality

### Target
- 25-30 UI components (all standard web controls)
- 80+ unit tests (80%+ coverage)
- GraphQL + REST API
- 100% XML documentation
- Complete export suite (CSV, Excel, PDF)

---

## Phase 1: Core Components (2 weeks)

### Task 1: Modal/Dialog Component

**Files:**
- Create: `src/Components/ModalComponent.razor`
- Create: `src/Components/ModalComponent.razor.cs`
- Create: `tests/Components/ModalComponentTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[TestFixture]
public class ModalComponentTests
{
    [Test]
    public void ModalComponent_WithIsOpen_RendersVisible()
    {
        var cut = RenderComponent<ModalComponent>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test Modal")
            .Add(p => p.ChildContent, (RenderFragment)(_ => { })));
        
        cut.MarkupMatches(
            @"<div class=""modal show"" style=""display: block;"">
                <div class=""modal-dialog"">
                    <div class=""modal-content"">
                        <div class=""modal-header"">
                            <h5 class=""modal-title"">Test Modal</h5>
                        </div>
                    </div>
                </div>
            </div>");
    }

    [Test]
    public void ModalComponent_WithIsOpenFalse_RendersHidden()
    {
        var cut = RenderComponent<ModalComponent>(parameters => parameters
            .Add(p => p.IsOpen, false));
        
        cut.MarkupMatches(@"<div class=""modal""></div>");
    }

    [Test]
    public async Task ModalComponent_WithCloseButton_InvokesCallback()
    {
        var closeInvoked = false;
        var cut = RenderComponent<ModalComponent>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeInvoked = true)));
        
        var closeBtn = cut.Find(".btn-close");
        await cut.InvokeAsync(() => closeBtn.Click());
        
        Assert.IsTrue(closeInvoked);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet test tests/Components/ModalComponentTests.cs -v
```

Expected: FAIL with "ModalComponent not found"

- [ ] **Step 3: Create component structure**

```razor
@* src/Components/ModalComponent.razor *@
@namespace SmartWorkz.Core.Web.Components

<div class="modal @(IsOpen ? "show" : "")" style="@(IsOpen ? "display: block;" : "")">
    <div class="modal-dialog @SizeClass">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">@Title</h5>
                <button type="button" class="btn-close" @onclick="OnCloseClick" />
            </div>
            <div class="modal-body">
                @ChildContent
            </div>
            @if (ShowFooter)
            {
                <div class="modal-footer">
                    @if (ShowCancelButton)
                    {
                        <button type="button" class="btn btn-secondary" @onclick="OnCloseClick">Cancel</button>
                    }
                    @if (ShowConfirmButton)
                    {
                        <button type="button" class="btn btn-primary" @onclick="OnConfirmClick">@ConfirmText</button>
                    }
                </div>
            }
        </div>
    </div>
</div>

@if (IsOpen)
{
    <div class="modal-backdrop fade show"></div>
}
```

- [ ] **Step 4: Create component code-behind**

```csharp
// src/Components/ModalComponent.razor.cs
namespace SmartWorkz.Core.Web.Components;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Reusable modal/dialog component with customizable title, body, and footer actions.
/// Supports size variants (small, default, large) and optional confirm/cancel buttons.
/// </summary>
/// <remarks>
/// Example usage:
/// <code>
/// &lt;ModalComponent IsOpen="@showModal" Title="Confirm Action" OnClose="@CloseModal" OnConfirm="@ConfirmAction"&gt;
///     Are you sure you want to delete this item?
/// &lt;/ModalComponent&gt;
/// </code>
/// </remarks>
public partial class ModalComponent : ComponentBase
{
    /// <summary>Gets or sets whether the modal is visible (default: false).</summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>Gets or sets the modal title displayed in header.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>Gets or sets the modal body content.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets whether to show footer with buttons (default: true).</summary>
    [Parameter]
    public bool ShowFooter { get; set; } = true;

    /// <summary>Gets or sets whether to show cancel button (default: true).</summary>
    [Parameter]
    public bool ShowCancelButton { get; set; } = true;

    /// <summary>Gets or sets whether to show confirm button (default: true).</summary>
    [Parameter]
    public bool ShowConfirmButton { get; set; } = true;

    /// <summary>Gets or sets the confirm button text (default: "Confirm").</summary>
    [Parameter]
    public string ConfirmText { get; set; } = "Confirm";

    /// <summary>Gets or sets the modal size: "sm", "lg", or "" (default: "").</summary>
    [Parameter]
    public string? Size { get; set; }

    /// <summary>Gets or sets callback when modal is closed/cancelled.</summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>Gets or sets callback when confirm button is clicked.</summary>
    [Parameter]
    public EventCallback OnConfirm { get; set; }

    /// <summary>Gets or sets CSS class for keyboard dismiss (default: true).</summary>
    [Parameter]
    public bool Keyboard { get; set; } = true;

    private string SizeClass => Size switch
    {
        "sm" => "modal-sm",
        "lg" => "modal-lg",
        "xl" => "modal-xl",
        _ => ""
    };

    private async Task OnCloseClick()
    {
        await OnClose.InvokeAsync();
    }

    private async Task OnConfirmClick()
    {
        await OnConfirm.InvokeAsync();
    }

    protected override void OnKeyDown(KeyboardEventArgs e)
    {
        if (Keyboard && e.Key == "Escape" && IsOpen)
        {
            OnCloseClick();
        }
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

```bash
dotnet test tests/Components/ModalComponentTests.cs::ModalComponentTests::ModalComponent_WithIsOpen_RendersVisible -v
```

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/Components/ModalComponent.razor* tests/Components/ModalComponentTests.cs
git commit -m "feat(web): add ModalComponent with show/hide and confirm/cancel callbacks"
```

---

### Task 2: Dropdown/Select Component

**Files:**
- Create: `src/Components/DropdownComponent.razor`
- Create: `src/Components/DropdownComponent.razor.cs`
- Create: `tests/Components/DropdownComponentTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[TestFixture]
public class DropdownComponentTests
{
    [Test]
    public void DropdownComponent_WithItems_RendersAllOptions()
    {
        var items = new List<SelectItem>
        {
            new("1", "Option 1"),
            new("2", "Option 2"),
            new("3", "Option 3")
        };

        var cut = RenderComponent<DropdownComponent>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.SelectedValue, "1"));

        var options = cut.FindAll("option");
        Assert.AreEqual(3, options.Count);
        Assert.AreEqual("Option 1", options[0].TextContent);
    }

    [Test]
    public async Task DropdownComponent_OnSelectionChange_InvokesCallback()
    {
        var selectedValue = string.Empty;
        var items = new List<SelectItem> { new("1", "Option 1"), new("2", "Option 2") };
        
        var cut = RenderComponent<DropdownComponent>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.OnValueChanged, EventCallback.Factory.Create(this, 
                (string val) => selectedValue = val)));

        var select = cut.Find("select");
        await cut.InvokeAsync(() => 
        {
            select.Value = "2";
            select.Change();
        });

        Assert.AreEqual("2", selectedValue);
    }
}

public record SelectItem(string Value, string Label);
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test tests/Components/DropdownComponentTests.cs -v
```

Expected: FAIL

- [ ] **Step 3: Create dropdown component**

```razor
@* src/Components/DropdownComponent.razor *@
@namespace SmartWorkz.Core.Web.Components

<div class="dropdown-wrapper">
    <label @if (!string.IsNullOrEmpty(Label)) class="form-label">@Label</label>
    <select class="form-control @ErrorClass" 
            @bind="SelectedValue" 
            @onchange="OnSelectionChanged"
            disabled="@IsDisabled">
        @if (!string.IsNullOrEmpty(Placeholder))
        {
            <option value="">@Placeholder</option>
        }
        @foreach (var item in Items ?? new List<SelectItem>())
        {
            <option value="@item.Value">@item.Label</option>
        }
    </select>
    @if (!string.IsNullOrEmpty(ErrorMessage))
    {
        <div class="invalid-feedback d-block">@ErrorMessage</div>
    }
</div>
```

```csharp
// src/Components/DropdownComponent.razor.cs
namespace SmartWorkz.Core.Web.Components;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Dropdown/select component for selecting single value from list of options.
/// Supports labels, placeholders, validation messages, and change callbacks.
/// </summary>
public partial class DropdownComponent : ComponentBase
{
    [Parameter]
    public List<SelectItem>? Items { get; set; }

    [Parameter]
    public string? SelectedValue { get; set; }

    [Parameter]
    public EventCallback<string> OnValueChanged { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? ErrorMessage { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    private string ErrorClass => string.IsNullOrEmpty(ErrorMessage) ? "" : "is-invalid";

    private async Task OnSelectionChanged(ChangeEventArgs e)
    {
        SelectedValue = e.Value?.ToString();
        await OnValueChanged.InvokeAsync(SelectedValue ?? "");
    }
}

public record SelectItem(string Value, string Label);
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Components/DropdownComponentTests.cs -v
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Components/DropdownComponent.razor* tests/Components/DropdownComponentTests.cs
git commit -m "feat(web): add DropdownComponent with selection change callbacks"
```

---

### Task 3: DatePicker Component

**Files:**
- Create: `src/Components/DatePickerComponent.razor`
- Create: `src/Components/DatePickerComponent.razor.cs`
- Create: `tests/Components/DatePickerComponentTests.cs`

- [ ] **Step 1: Write failing test**

```csharp
[TestFixture]
public class DatePickerComponentTests
{
    [Test]
    public void DatePickerComponent_WithDate_RendersInputWithValue()
    {
        var testDate = new DateTime(2026, 04, 28);
        var cut = RenderComponent<DatePickerComponent>(parameters => parameters
            .Add(p => p.SelectedDate, testDate));

        var input = cut.Find("input[type='date']");
        Assert.AreEqual("2026-04-28", input.GetAttribute("value"));
    }

    [Test]
    public async Task DatePickerComponent_OnDateChange_InvokesCallback()
    {
        var selectedDate = DateTime.MinValue;
        var cut = RenderComponent<DatePickerComponent>(parameters => parameters
            .Add(p => p.OnDateChanged, EventCallback.Factory.Create(this, 
                (DateTime date) => selectedDate = date)));

        var input = cut.Find("input[type='date']");
        await cut.InvokeAsync(() => 
        {
            input.Value = "2026-05-15";
            input.Change();
        });

        Assert.AreEqual(2026, selectedDate.Year);
        Assert.AreEqual(5, selectedDate.Month);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test tests/Components/DatePickerComponentTests.cs -v
```

Expected: FAIL

- [ ] **Step 3: Create DatePicker component**

```razor
@* src/Components/DatePickerComponent.razor *@
@namespace SmartWorkz.Core.Web.Components

<div class="datepicker-wrapper">
    @if (!string.IsNullOrEmpty(Label))
    {
        <label class="form-label">@Label</label>
    }
    <input type="date" 
           class="form-control @ErrorClass"
           @bind="DateValue"
           @onchange="OnDateChanged"
           min="@MinDate"
           max="@MaxDate"
           disabled="@IsDisabled" />
    @if (!string.IsNullOrEmpty(ErrorMessage))
    {
        <div class="invalid-feedback d-block">@ErrorMessage</div>
    }
</div>
```

```csharp
// src/Components/DatePickerComponent.razor.cs
namespace SmartWorkz.Core.Web.Components;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Date picker component supporting single date selection with min/max constraints.
/// Supports validation messages and change callbacks.
/// </summary>
public partial class DatePickerComponent : ComponentBase
{
    [Parameter]
    public DateTime? SelectedDate { get; set; }

    [Parameter]
    public EventCallback<DateTime> OnDateChanged { get; set; }

    [Parameter]
    public DateTime? MinDate { get; set; }

    [Parameter]
    public DateTime? MaxDate { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? ErrorMessage { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    private string DateValue
    {
        get => SelectedDate?.ToString("yyyy-MM-dd") ?? "";
        set
        {
            if (DateTime.TryParse(value, out var date))
            {
                SelectedDate = date;
            }
        }
    }

    private string ErrorClass => string.IsNullOrEmpty(ErrorMessage) ? "" : "is-invalid";

    private async Task OnDateChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value?.ToString(), out var date))
        {
            SelectedDate = date;
            await OnDateChanged.InvokeAsync(date);
        }
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Components/DatePickerComponentTests.cs -v
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Components/DatePickerComponent.razor* tests/Components/DatePickerComponentTests.cs
git commit -m "feat(web): add DatePickerComponent with date selection and validation"
```

---

### Task 4-18: Implement Remaining Core Components (Bulk)

Following the same TDD pattern, implement these components (each gets 4-6 hours):

**Components to implement (15 more):**

1. **TimePicker** - Time input with hour/minute selection
2. **Autocomplete** - Search + select from filtered list
3. **FileUpload** - File selection and upload handler
4. **RichTextEditor** - HTML content editing
5. **Toast/Alert** - Dismissible notification component
6. **Breadcrumb** - Navigation breadcrumb trail
7. **Tabs** - Tabbed content display
8. **Accordion** - Collapsible sections
9. **Carousel** - Image/content carousel
10. **ProgressBar** - Linear progress indicator
11. **Spinner/LoadingIndicator** - Loading animation
12. **Badge** - Status/count badge
13. **Alert** - Contextual alert messages
14. **Tooltip** - Hover tooltips
15. **Sidebar Navigation** - Collapsible sidebar menu

**Per-component effort:** 4-6 hours each  
**Total effort:** 60-90 hours  
**Timeline:** 2 weeks with 2 developers

For each component, follow this template:
- [ ] Write failing tests (3-5 test cases minimum)
- [ ] Create `.razor` file with markup
- [ ] Create `.razor.cs` with logic and XML docs
- [ ] Write unit tests
- [ ] Run tests to verify PASS
- [ ] Commit with descriptive message

---

## Phase 2: Export Services (1 week)

### Task 19: CSV Export Service

**Files:**
- Create: `src/Services/IExportService.cs`
- Create: `src/Services/CsvExportService.cs`
- Create: `tests/Services/CsvExportServiceTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
[TestFixture]
public class CsvExportServiceTests
{
    private CsvExportService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new CsvExportService();
    }

    [Test]
    public void CsvExportService_WithData_GeneratesValidCsv()
    {
        var data = new List<ExportItem>
        {
            new(1, "Product A", 100.00m),
            new(2, "Product B", 200.00m)
        };

        var csv = _service.Export(data);

        Assert.IsTrue(csv.Contains("Id,Name,Price"));
        Assert.IsTrue(csv.Contains("1,\"Product A\",100"));
        Assert.IsTrue(csv.Contains("2,\"Product B\",200"));
    }

    [Test]
    public void CsvExportService_WithSpecialChars_EscapesProperly()
    {
        var data = new List<ExportItem> 
        { 
            new(1, "Product \"Deluxe\"", 100.00m) 
        };

        var csv = _service.Export(data);

        Assert.IsTrue(csv.Contains("\"Product \"\"Deluxe\"\"\""));
    }
}

public record ExportItem(int Id, string Name, decimal Price);
```

- [ ] **Step 2: Run test to verify it fails**

```bash
dotnet test tests/Services/CsvExportServiceTests.cs -v
```

Expected: FAIL

- [ ] **Step 3: Implement CSV export**

```csharp
// src/Services/IExportService.cs
namespace SmartWorkz.Core.Web.Services;

public interface IExportService<T> where T : class
{
    /// <summary>Exports collection to CSV format.</summary>
    string ExportToCsv(IEnumerable<T> data);
    
    /// <summary>Exports collection to CSV bytes.</summary>
    byte[] ExportToCsvBytes(IEnumerable<T> data);
    
    /// <summary>Exports collection to Excel format.</summary>
    byte[] ExportToExcel(IEnumerable<T> data);
    
    /// <summary>Exports collection to PDF format.</summary>
    byte[] ExportToPdf(IEnumerable<T> data);
}

// src/Services/CsvExportService.cs
namespace SmartWorkz.Core.Web.Services;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

/// <summary>
/// Exports generic collections to CSV, Excel, and PDF formats.
/// Automatically discovers properties using reflection and includes headers.
/// </summary>
public class ExportService<T> : IExportService<T> where T : class
{
    /// <summary>Exports collection to CSV string format.</summary>
    public string ExportToCsv(IEnumerable<T> data)
    {
        var list = data.ToList();
        if (list.Count == 0)
            return "";

        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase);
        
        // Write header
        var headers = properties.Select(p => p.Name);
        sb.AppendLine(string.Join(",", headers));
        
        // Write rows
        foreach (var item in list)
        {
            var values = properties.Select(p => 
            {
                var value = p.GetValue(item);
                return value == null ? "" : CsvEscape(value.ToString()!);
            });
            sb.AppendLine(string.Join(",", values));
        }
        
        return sb.ToString();
    }

    /// <summary>Exports collection to CSV byte array.</summary>
    public byte[] ExportToCsvBytes(IEnumerable<T> data)
    {
        var csv = ExportToCsv(data);
        return Encoding.UTF8.GetBytes(csv);
    }

    /// <summary>Exports collection to Excel format.</summary>
    public byte[] ExportToExcel(IEnumerable<T> data)
    {
        // Implementation using EPPlus library
        throw new NotImplementedException("Use ExcelExportService in Core.External");
    }

    /// <summary>Exports collection to PDF format.</summary>
    public byte[] ExportToPdf(IEnumerable<T> data)
    {
        // Implementation using iText7 library
        throw new NotImplementedException("Use PdfExportService in Core.External");
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/Services/CsvExportServiceTests.cs -v
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Services/IExportService.cs src/Services/ExportService.cs tests/Services/CsvExportServiceTests.cs
git commit -m "feat(web): add generic export service with CSV support"
```

---

### Task 20: Excel & PDF Export Services (in Core.External)

Move to SmartWorkz.Core.External implementation (see IMPLEMENTATION_PLAN_CORE.EXTERNAL.md)

---

## Phase 3: Test Coverage (1 week)

### Task 21: Component Integration Tests

**Files:**
- Create: `tests/Integration/ComponentIntegrationTests.cs`

- [ ] **Step 1: Write integration test suite**

```csharp
[TestFixture]
public class ComponentIntegrationTests
{
    [Test]
    public void ModalAndDropdown_Together_WorkCorrectly()
    {
        var items = new List<SelectItem> { new("1", "Option 1"), new("2", "Option 2") };
        var selectedValue = "";
        var modalOpen = true;

        var cut = RenderComponent<ModalComponent>(parameters => parameters
            .Add(p => p.IsOpen, modalOpen)
            .Add(p => p.Title, "Select Option"));

        // Render dropdown inside modal
        cut.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            { "ChildContent", (RenderFragment)(_ => builder =>
                builder.OpenComponent<DropdownComponent>(0);
                builder.AddAttribute(1, nameof(DropdownComponent.Items), items);
                builder.AddAttribute(2, nameof(DropdownComponent.OnValueChanged), 
                    EventCallback.Factory.Create(this, (string val) => selectedValue = val));
                builder.CloseComponent();
            )},
            { "IsOpen", true }
        }));

        var dropdown = cut.FindComponent<DropdownComponent>();
        Assert.IsNotNull(dropdown);
    }
}
```

- [ ] **Step 2: Write API integration tests**

```csharp
[TestFixture]
public class GraphQLIntegrationTests
{
    [Test]
    public async Task GridComponent_WithGraphQLQuery_RendersData()
    {
        // Test GridComponent working with GraphQL endpoint
        var products = new List<Product> 
        { 
            new(1, "Product 1", 100),
            new(2, "Product 2", 200)
        };

        // Mock GraphQL response
        var response = new { data = new { products = products } };

        // Verify component can bind to GraphQL data
        var cut = RenderComponent<GridComponent>(parameters => parameters
            .Add(p => p.Data, products.AsQueryable()));

        var rows = cut.FindAll("tr");
        Assert.IsTrue(rows.Count >= 2);
    }
}

public record Product(int Id, string Name, decimal Price);
```

- [ ] **Step 3: Run integration tests**

```bash
dotnet test tests/Integration/ComponentIntegrationTests.cs -v
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add tests/Integration/ComponentIntegrationTests.cs
git commit -m "test(web): add component and GraphQL integration tests"
```

---

### Task 22: E2E Tests with Selenium

**Files:**
- Create: `tests/E2E/GridComponentE2ETests.cs`
- Create: `tests/E2E/ModalComponentE2ETests.cs`

- [ ] **Step 1: Write E2E test for GridComponent**

```csharp
[TestFixture]
public class GridComponentE2ETests
{
    private IWebDriver? _driver;

    [SetUp]
    public void Setup()
    {
        var options = new ChromeOptions();
        options.AddArgument("--no-sandbox");
        _driver = new ChromeDriver(options);
    }

    [TearDown]
    public void Teardown()
    {
        _driver?.Quit();
    }

    [Test]
    public void GridComponent_LoadsAndDisplaysData()
    {
        _driver!.Navigate().GoToUrl("http://localhost:5000/grid-demo");
        
        var grid = _driver.FindElement(By.CssSelector(".grid-container"));
        Assert.IsNotNull(grid);
        
        var rows = _driver.FindElements(By.CssSelector(".grid-row"));
        Assert.IsTrue(rows.Count > 0);
    }

    [Test]
    public void GridComponent_SortingWorks()
    {
        _driver!.Navigate().GoToUrl("http://localhost:5000/grid-demo");
        
        var nameHeader = _driver.FindElement(By.CssSelector("th[data-sort='Name']"));
        nameHeader.Click();
        
        System.Threading.Thread.Sleep(500); // Wait for sort
        
        var rows = _driver.FindElements(By.CssSelector(".grid-row"));
        Assert.IsTrue(rows.Count > 0);
    }
}
```

- [ ] **Step 2: Run E2E tests (requires running application)**

```bash
# Terminal 1: Start the application
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet run --urls=http://localhost:5000

# Terminal 2: Run E2E tests
dotnet test tests/E2E/GridComponentE2ETests.cs -v
```

Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add tests/E2E/GridComponentE2ETests.cs tests/E2E/ModalComponentE2ETests.cs
git commit -m "test(web): add E2E tests for grid and modal components"
```

---

### Task 23: Performance & Load Tests

**Files:**
- Create: `tests/Performance/ComponentPerformanceTests.cs`

- [ ] **Step 1: Write performance test**

```csharp
[TestFixture]
public class ComponentPerformanceTests
{
    [Test]
    public void GridComponent_With10kRows_RendersFast()
    {
        var largeData = Enumerable.Range(1, 10000)
            .Select(i => new { Id = i, Name = $"Item {i}", Price = i * 10m })
            .ToList();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var cut = RenderComponent<GridComponent>(parameters => parameters
            .Add(p => p.Data, largeData.AsQueryable())
            .Add(p => p.VirtualizationEnabled, true)
            .Add(p => p.ItemHeight, 40));

        stopwatch.Stop();

        // Should render in under 200ms with virtualization
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200, 
            $"Rendering took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
    }
}
```

- [ ] **Step 2: Run performance tests**

```bash
dotnet test tests/Performance/ComponentPerformanceTests.cs -v
```

Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add tests/Performance/ComponentPerformanceTests.cs
git commit -m "test(web): add performance benchmarks for virtualized grid"
```

---

## Phase 4: Documentation & Polish (1 week)

### Task 24: Complete XML Documentation

- [ ] **Step 1: Add missing XML docs to 12 undocumented files**

```bash
# For each file in src/ without docs, add complete XML documentation
# Example pattern for each file:

/// <summary>
/// [Component name] - [One-line description of purpose].
/// [2-3 sentences about functionality and use cases.]
/// </summary>
/// <remarks>
/// [Additional context about implementation, patterns used, or constraints.]
/// </remarks>
/// <example>
/// <code>
/// // Example usage
/// &lt;ComponentName Property1="value" OnCallback="@HandleCallback" /&gt;
/// </code>
/// </example>
public partial class ComponentName : ComponentBase
{
    // Add XML docs to all public parameters
    /// <summary>[What this parameter does].</summary>
    [Parameter]
    public string Property1 { get; set; }
}
```

- [ ] **Step 2: Regenerate XML documentation file**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet build /p:GenerateDocumentationFile=true
```

- [ ] **Step 3: Verify 100% coverage**

```bash
# Check that all public members have documentation
# Use Visual Studio's "Documentation" code analysis rule
```

- [ ] **Step 4: Commit**

```bash
git add src/**/*.cs
git commit -m "docs(web): complete XML documentation for all components (100% coverage)"
```

---

### Task 25: API Reference Documentation

**Files:**
- Create: `docs/API-REFERENCE-COMPONENTS.md`

- [ ] **Step 1: Generate API reference from XML docs**

```markdown
# SmartWorkz.Core.Web - Component API Reference

## Overview
Complete reference for all 25+ UI components available in SmartWorkz.Core.Web.

## Components

### ModalComponent
```csharp
<ModalComponent 
    IsOpen="@isOpen"
    Title="Confirm Action"
    OnClose="@HandleClose"
    OnConfirm="@HandleConfirm">
    Are you sure you want to continue?
</ModalComponent>
```

**Parameters:**
- `IsOpen` (bool) - Whether modal is visible (default: false)
- `Title` (string) - Modal header title
- `OnClose` (EventCallback) - Triggered when modal is closed
- `OnConfirm` (EventCallback) - Triggered when confirm button clicked
- ... (all parameters documented)

### DropdownComponent
...

## Export Services

### CSV Export
```csharp
var exporter = new ExportService<Product>();
var csv = exporter.ExportToCsv(products);
var bytes = exporter.ExportToCsvBytes(products);
```

### Excel & PDF
[Reference to Core.External]

## Tag Helpers

### form-group
```html
<form-group label="Email" error-message="@validationError">
    <input type="email" class="form-control" />
</form-group>
```

...
```

- [ ] **Step 2: Add usage examples**

```bash
# Create example pages in docs/examples/
# - ModalExample.razor
# - DropdownExample.razor
# - GridExample.razor
# etc.
```

- [ ] **Step 3: Commit**

```bash
git add docs/API-REFERENCE-COMPONENTS.md docs/examples/
git commit -m "docs(web): create comprehensive API reference and examples for all components"
```

---

### Task 26: Update README

- [ ] **Step 1: Update README.md with new components**

```markdown
# SmartWorkz.Core.Web

ASP.NET Core web components library with 25+ Blazor components, GraphQL API, and export services.

## Features

### UI Components (25+)
- **Data Display**: GridComponent, ListViewComponent, DataViewerComponent
- **Modals & Dialogs**: ModalComponent, AlertComponent
- **Form Controls**: DropdownComponent, DatePickerComponent, TimePickerComponent, FileUploadComponent, RichTextEditorComponent, AutocompleteComponent
- **Navigation**: BreadcrumbComponent, TabsComponent, SidebarNavigationComponent
- **Feedback**: ToastComponent, ProgressBarComponent, SpinnerComponent, BadgeComponent
- **Other**: CarouselComponent, AccordionComponent, TooltipComponent

### APIs
- GraphQL with authentication and rate limiting
- REST endpoints (coming soon)
- Data loaders for N+1 prevention

### Export Services
- CSV export
- Excel export (via Core.External)
- PDF export (via Core.External)

### Validation & Forms
- Data annotation validation
- Custom validation rules
- Form group tag helper
- Status badge tag helper

## Quick Start

```csharp
// Register components
builder.Services.AddSmartWorkzWeb();

// Use in Razor page
@page "/demo"
@using SmartWorkz.Core.Web.Components

<GridComponent Data="@products" />
<ModalComponent @ref="modal" Title="Confirm">
    Are you sure?
</ModalComponent>
```

## Testing

All components are fully tested with 80%+ coverage:

```bash
dotnet test tests/ -v
```

## Documentation

- [API Reference](docs/API-REFERENCE-COMPONENTS.md)
- [Component Examples](docs/examples/)
- [Contributing Guidelines](docs/CONTRIBUTING.md)

---
```

- [ ] **Step 2: Commit**

```bash
git add README.md
git commit -m "docs(web): update README with all 25+ components and features"
```

---

## Summary

### Deliverables
- ✅ 25-30 UI components (100% implemented with tests)
- ✅ 80+ unit tests (80%+ code coverage)
- ✅ 20+ integration tests
- ✅ 10+ E2E tests
- ✅ Performance benchmarks
- ✅ 100% XML documentation
- ✅ Comprehensive API reference
- ✅ Export services (CSV, Excel, PDF)

### Success Metrics
- [ ] All 25+ components implemented
- [ ] 80%+ test coverage
- [ ] All components documented
- [ ] Example pages for each component
- [ ] E2E tests passing
- [ ] Performance benchmarks baseline established

### Timeline
- **Phase 1** (2 weeks): 18 core components
- **Phase 2** (1 week): Export services  
- **Phase 3** (1 week): Tests + coverage
- **Phase 4** (1 week): Documentation

**Total: 5 weeks, 2-3 developers**

---

**Next:** Move to SmartWorkz.Core.Mobile implementation plan
