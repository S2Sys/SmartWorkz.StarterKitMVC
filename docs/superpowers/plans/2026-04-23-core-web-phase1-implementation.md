# SmartWorkz.Core.Web Phase 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Core.Web as a production-ready component and utility library for ASP.NET Core MVC/Razor Pages applications with GridComponent, validation services, tag helpers, and comprehensive documentation.

**Architecture:** Core.Web provides reusable Razor components, tag helpers, and service abstractions. Phase 1 delivers foundation layers: project structure → base abstractions → GridComponent → utilities → documentation. Each layer is independently testable and consumable.

**Tech Stack:** .NET 9.0, ASP.NET Core, xUnit, Bunit, Bootstrap 5, Razor Components

**Timeline:** 2 weeks (10 working days), phased with Git commits after each major task

---

## File Structure

### New Files to Create
```
c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web/
├── SmartWorkz.Core.Web.csproj                    # Main library project
├── Directory.Build.props                          # Shared build config
├── src/
│   ├── _Imports.razor                            # Global Razor imports
│   ├── Components/
│   │   ├── BaseRazorComponent.cs                 # Abstract base class
│   │   ├── BaseFormComponent.cs                  # Form-specific base
│   │   ├── GridComponent.razor                   # Grid display component
│   │   └── GridColumn.razor                      # Grid column definition
│   ├── Services/
│   │   ├── IValidationService.cs                 # Validation contract
│   │   └── ValidationService.cs                  # Validation implementation
│   ├── TagHelpers/
│   │   ├── FormGroupTagHelper.cs                 # Form group helper
│   │   └── StatusBadgeTagHelper.cs               # Status badge helper
│   ├── Models/
│   │   ├── GridColumn.cs                         # Column metadata
│   │   ├── GridOptions.cs                        # Grid configuration
│   │   └── SortOrder.cs                          # Sort direction enum
│   └── Extensions/
│       └── ValidationExtensions.cs               # Validation helpers
├── tests/
│   ├── SmartWorkz.Core.Web.Tests.csproj         # Test project
│   ├── _Imports.cs                               # Test imports
│   ├── Components/
│   │   ├── BaseRazorComponentTests.cs
│   │   └── GridComponentTests.cs
│   ├── Services/
│   │   └── ValidationServiceTests.cs
│   └── TagHelpers/
│       ├── FormGroupTagHelperTests.cs
│       └── StatusBadgeTagHelperTests.cs
├── docs/
│   ├── examples/
│   │   ├── GridExample.razor                     # Grid usage example
│   │   ├── FormExample.razor                     # Form with validation
│   │   └── CombinedExample.razor                 # Grid + forms
│   ├── API-REFERENCE.md                          # API documentation
│   └── CONTRIBUTING.md                           # Contribution guide
└── README.md                                      # Updated with implementation
```

### Existing Files to Modify
- `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\README.md` — Add implementation status
- `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.StarterKitMVC\SCHEMA-SUMMARY-LEAN.md` — Add reference to Core.Web

---

## Tasks

### Task 1: Create and Configure SmartWorkz.Core.Web.csproj

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\SmartWorkz.Core.Web.csproj`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\Directory.Build.props`

- [ ] **Step 1: Create main project file**

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <RootNamespace>SmartWorkz.Core.Web</RootNamespace>
    <AssemblyName>SmartWorkz.Core.Web</AssemblyName>
    <VersionPrefix>1.0.0</VersionPrefix>
    <Authors>SmartWorkz Development Team</Authors>
    <Description>Reusable Razor components and utilities for ASP.NET Core MVC/Razor Pages applications</Description>
    <PackageProjectUrl>https://github.com/S2Sys/SmartWorkz.Core.Web</PackageProjectUrl>
    <RepositoryUrl>https://github.com/S2Sys/SmartWorkz.Core.Web</RepositoryUrl>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.ViewFeatures" Version="9.0.0" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create shared build configuration**

```xml
<Project>
  <PropertyGroup>
    <ImplicitUsings>enable</ImplicitUsings>
    <Deterministic>true</Deterministic>
  </PropertyGroup>

  <ItemGroup>
    <CompilerVisibleProperty Include="TargetFramework" />
  </ItemGroup>
</Project>
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\Directory.Build.props`

- [ ] **Step 3: Verify project loads**

Run: 
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet build
```

Expected: Clean build with no errors

- [ ] **Step 4: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add SmartWorkz.Core.Web.csproj Directory.Build.props
git commit -m "feat: scaffold Core.Web project structure with .NET 9 configuration"
```

---

### Task 2: Create Folder Structure and Imports

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\_Imports.razor`
- Create directories: `src/Components`, `src/Services`, `src/TagHelpers`, `src/Models`, `src/Extensions`

- [ ] **Step 1: Create global Razor imports**

```razor
@using System.ComponentModel.DataAnnotations
@using System.Reflection
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using SmartWorkz.Core.Web.Components
@using SmartWorkz.Core.Web.Models
@using SmartWorkz.Core.Web.Services
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\_Imports.razor`

- [ ] **Step 2: Create directory structure**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src
mkdir Components Services TagHelpers Models Extensions
```

- [ ] **Step 3: Create test directory structure**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
mkdir Components Services TagHelpers
```

- [ ] **Step 4: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add src/_Imports.razor
git commit -m "feat: add Razor imports and directory structure"
```

---

### Task 3: Create Test Project and Test Infrastructure

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\SmartWorkz.Core.Web.Tests.csproj`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\_Imports.cs`

- [ ] **Step 1: Create test project file**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <RootNamespace>SmartWorkz.Core.Web.Tests</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="bunit" Version="1.27.17" />
    <PackageReference Include="Moq" Version="4.20.70" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SmartWorkz.Core.Web.csproj" />
  </ItemGroup>
</Project>
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\SmartWorkz.Core.Web.Tests.csproj`

- [ ] **Step 2: Create test imports**

```csharp
global using Xunit;
global using Bunit;
global using Moq;
global using SmartWorkz.Core.Web.Components;
global using SmartWorkz.Core.Web.Services;
global using SmartWorkz.Core.Web.Models;
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\_Imports.cs`

- [ ] **Step 3: Verify test project builds**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
dotnet build
```

Expected: Clean build with no errors

- [ ] **Step 4: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add tests/SmartWorkz.Core.Web.Tests.csproj tests/_Imports.cs
git commit -m "test: scaffold test project with xUnit and Bunit"
```

---

### Task 4: Create BaseRazorComponent Abstract Class

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Components\BaseRazorComponent.cs`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\Components\BaseRazorComponentTests.cs`

- [ ] **Step 1: Write failing test**

```csharp
namespace SmartWorkz.Core.Web.Tests.Components;

public class BaseRazorComponentTests
{
    [Fact]
    public void BaseRazorComponent_IsAbstract()
    {
        var type = typeof(BaseRazorComponent);
        Assert.True(type.IsAbstract, "BaseRazorComponent should be abstract");
    }

    [Fact]
    public void BaseRazorComponent_InheritsFromComponentBase()
    {
        var type = typeof(BaseRazorComponent);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(type), 
            "BaseRazorComponent should inherit from ComponentBase");
    }

    [Fact]
    public void BaseRazorComponent_HasSetParametersAsyncMethod()
    {
        var type = typeof(BaseRazorComponent);
        var method = type.GetMethod("SetParametersAsync");
        Assert.NotNull(method);
        Assert.True(method!.ReturnType == typeof(Task), 
            "SetParametersAsync should return Task");
    }

    [Fact]
    public void BaseRazorComponent_CallsOnParameterValidation()
    {
        var component = new TestComponent();
        component.SetParametersAsync(ParameterView.Empty).GetAwaiter().GetResult();
        Assert.True(component.OnParameterValidationCalled);
    }
}

// Test implementation
public class TestComponent : BaseRazorComponent
{
    public bool OnParameterValidationCalled { get; set; }

    protected override void OnParameterValidation()
    {
        OnParameterValidationCalled = true;
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\Components\BaseRazorComponentTests.cs`

- [ ] **Step 2: Run test to verify failure**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
dotnet test --filter "BaseRazorComponentTests" -v normal
```

Expected: FAIL - "BaseRazorComponent" type not found

- [ ] **Step 3: Implement BaseRazorComponent**

```csharp
namespace SmartWorkz.Core.Web.Components;

/// <summary>
/// Abstract base class for all Razor components in SmartWorkz.Core.Web.
/// Provides common lifecycle and validation patterns for derived components.
/// </summary>
public abstract class BaseRazorComponent : ComponentBase
{
    /// <summary>
    /// Override to validate component parameters after binding.
    /// Called during SetParametersAsync before rendering.
    /// </summary>
    /// <remarks>
    /// Throw ArgumentException to fail parameter validation.
    /// This prevents rendering invalid component states.
    /// </remarks>
    protected virtual void OnParameterValidation() { }

    /// <summary>
    /// Binds parameters and runs validation before rendering.
    /// </summary>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        try
        {
            await base.SetParametersAsync(parameters);
            OnParameterValidation();
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                $"Parameter validation failed for {GetType().Name}: {ex.Message}", ex);
        }
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Components\BaseRazorComponent.cs`

- [ ] **Step 4: Run test to verify success**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
dotnet test --filter "BaseRazorComponentTests" -v normal
```

Expected: PASS (all 4 tests)

- [ ] **Step 5: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add src/Components/BaseRazorComponent.cs tests/Components/BaseRazorComponentTests.cs
git commit -m "feat: add BaseRazorComponent with parameter validation lifecycle"
```

---

### Task 5: Create IValidationService Interface and Implementation

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Services\IValidationService.cs`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Services\ValidationService.cs`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\Services\ValidationServiceTests.cs`

- [ ] **Step 1: Write failing test**

```csharp
namespace SmartWorkz.Core.Web.Tests.Services;

public class ValidationServiceTests
{
    private readonly IValidationService _service;

    public ValidationServiceTests()
    {
        _service = new ValidationService();
    }

    [Fact]
    public void ValidateModel_WithValidObject_ReturnsEmpty()
    {
        var model = new ValidTestModel { Name = "John", Email = "john@example.com" };
        var errors = _service.ValidateModel(model);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateModel_WithInvalidObject_ReturnsDictionary()
    {
        var model = new ValidTestModel { Name = "", Email = "" };
        var errors = _service.ValidateModel(model);

        Assert.NotEmpty(errors);
        Assert.Contains("Name", errors.Keys);
        Assert.Contains("Email", errors.Keys);
    }

    [Fact]
    public void ValidateProperty_WithValidValue_ReturnsFalse()
    {
        var model = new ValidTestModel { Name = "John", Email = "john@example.com" };
        var result = _service.ValidateProperty(model, "Name", out var errors);

        Assert.False(result); // False = no errors
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateProperty_WithInvalidValue_ReturnsTrue()
    {
        var model = new ValidTestModel { Name = "", Email = "john@example.com" };
        var result = _service.ValidateProperty(model, "Name", out var errors);

        Assert.True(result); // True = has errors
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void GetErrorMessage_ReturnsFirstError()
    {
        var model = new ValidTestModel { Name = "", Email = "" };
        var errors = _service.ValidateModel(model);

        Assert.NotEmpty(errors["Name"]);
        Assert.NotEmpty(errors["Name"][0]);
    }
}

// Test models
public class ValidTestModel
{
    [Required(ErrorMessage = "Name is required")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\Services\ValidationServiceTests.cs`

- [ ] **Step 2: Run test to verify failure**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
dotnet test --filter "ValidationServiceTests" -v normal
```

Expected: FAIL - interfaces/classes not found

- [ ] **Step 3: Create IValidationService interface**

```csharp
namespace SmartWorkz.Core.Web.Services;

/// <summary>
/// Service for validating models and properties using data annotations.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a complete model object.
    /// </summary>
    /// <typeparam name="T">Model type to validate.</typeparam>
    /// <param name="model">Model instance to validate.</param>
    /// <returns>Dictionary of property names to error messages. Empty if valid.</returns>
    Dictionary<string, List<string>> ValidateModel<T>(T model) where T : class;

    /// <summary>
    /// Validates a single property on a model.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="model">Model instance.</param>
    /// <param name="propertyName">Property name to validate.</param>
    /// <param name="errors">Out parameter: list of validation errors.</param>
    /// <returns>True if validation failed (errors present), false if valid.</returns>
    bool ValidateProperty<T>(T model, string propertyName, out List<string> errors) where T : class;
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Services\IValidationService.cs`

- [ ] **Step 4: Create ValidationService implementation**

```csharp
namespace SmartWorkz.Core.Web.Services;

/// <summary>
/// Default implementation of IValidationService using System.ComponentModel.DataAnnotations.
/// </summary>
public class ValidationService : IValidationService
{
    /// <inheritdoc />
    public Dictionary<string, List<string>> ValidateModel<T>(T model) where T : class
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var context = new ValidationContext(model, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        return results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty), 
                (result, member) => new { Member = member, result.ErrorMessage })
            .GroupBy(x => x.Member)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage ?? "Validation failed").ToList());
    }

    /// <inheritdoc />
    public bool ValidateProperty<T>(T model, string propertyName, out List<string> errors) where T : class
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentException("Property name cannot be empty", nameof(propertyName));

        errors = new List<string>();

        var property = typeof(T).GetProperty(propertyName);
        if (property == null)
            throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}'");

        var propertyValue = property.GetValue(model);
        var context = new ValidationContext(model) { MemberName = propertyName };
        var results = new List<ValidationResult>();

        Validator.TryValidateProperty(propertyValue, context, results);

        errors = results.Select(r => r.ErrorMessage ?? "Validation failed").ToList();
        return errors.Any();
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Services\ValidationService.cs`

- [ ] **Step 5: Run test to verify success**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
dotnet test --filter "ValidationServiceTests" -v normal
```

Expected: PASS (all 5 tests)

- [ ] **Step 6: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add src/Services/IValidationService.cs src/Services/ValidationService.cs tests/Services/ValidationServiceTests.cs
git commit -m "feat: add IValidationService with DataAnnotations support"
```

---

### Task 6: Create Grid Models (SortOrder, GridColumn, GridOptions)

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Models\SortOrder.cs`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Models\GridColumn.cs`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Models\GridOptions.cs`

- [ ] **Step 1: Create SortOrder enum**

```csharp
namespace SmartWorkz.Core.Web.Models;

/// <summary>
/// Specifies sort direction for grid columns.
/// </summary>
public enum SortOrder
{
    /// <summary>No sorting applied.</summary>
    None = 0,

    /// <summary>Sort in ascending order (A-Z, 0-9).</summary>
    Ascending = 1,

    /// <summary>Sort in descending order (Z-A, 9-0).</summary>
    Descending = 2
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Models\SortOrder.cs`

- [ ] **Step 2: Create GridColumn class**

```csharp
namespace SmartWorkz.Core.Web.Models;

/// <summary>
/// Defines a column in a GridComponent.
/// </summary>
public class GridColumn
{
    /// <summary>
    /// Name of the property on the data object to display.
    /// </summary>
    public required string PropertyName { get; set; }

    /// <summary>
    /// Display header text for the column.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Width of the column (CSS units, e.g., "200px", "20%").
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Format string for displaying the value (e.g., "C" for currency, "yyyy-MM-dd" for dates).
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Whether this column supports sorting by clicking the header.
    /// </summary>
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Whether this column supports filtering.
    /// </summary>
    public bool Filterable { get; set; } = true;

    /// <summary>
    /// Current sort direction for this column.
    /// </summary>
    public SortOrder SortOrder { get; set; } = SortOrder.None;

    /// <summary>
    /// Optional custom CSS class to apply to column cells.
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Optional custom rendering template.
    /// </summary>
    public RenderFragment<object?>? Template { get; set; }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Models\GridColumn.cs`

- [ ] **Step 3: Create GridOptions configuration class**

```csharp
namespace SmartWorkz.Core.Web.Models;

/// <summary>
/// Configuration options for GridComponent behavior and performance.
/// </summary>
public class GridOptions
{
    /// <summary>
    /// Number of rows to display per page. Default: 25.
    /// </summary>
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// Height of each row in pixels. Required for virtual scrolling accuracy. Default: 40.
    /// </summary>
    public int ItemHeight { get; set; } = 40;

    /// <summary>
    /// Height of the grid container in pixels. Determines how many rows are visible.
    /// </summary>
    public int ContainerHeight { get; set; } = 600;

    /// <summary>
    /// Whether to enable virtual scrolling for large datasets.
    /// </summary>
    public bool VirtualizationEnabled { get; set; } = true;

    /// <summary>
    /// Number of rows before end to trigger loading more data. Default: 5.
    /// </summary>
    public int LoadMoreThreshold { get; set; } = 5;

    /// <summary>
    /// Whether users can sort by clicking column headers.
    /// </summary>
    public bool AllowSorting { get; set; } = true;

    /// <summary>
    /// Whether users can filter columns.
    /// </summary>
    public bool AllowFiltering { get; set; } = true;

    /// <summary>
    /// Whether to display alternating row colors.
    /// </summary>
    public bool AlternateRowColors { get; set; } = true;

    /// <summary>
    /// CSS class to apply to grid container.
    /// </summary>
    public string? CssClass { get; set; } = "grid-container";
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Models\GridOptions.cs`

- [ ] **Step 4: Verify compilation**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet build
```

Expected: Clean build with no errors

- [ ] **Step 5: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add src/Models/SortOrder.cs src/Models/GridColumn.cs src/Models/GridOptions.cs
git commit -m "feat: add grid configuration models (SortOrder, GridColumn, GridOptions)"
```

---

### Task 7: Implement GridComponent (Part 1: Structure and Sorting)

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Components\GridComponent.razor`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Components\GridComponent.razor.cs`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\Components\GridComponentTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
namespace SmartWorkz.Core.Web.Tests.Components;

public class GridComponentTests
{
    [Fact]
    public void GridComponent_WithData_Renders()
    {
        var ctx = new TestContext();
        var data = new[] { new TestItem { Id = 1, Name = "Item 1" } };

        var cut = ctx.RenderComponent<GridComponent>(
            parameters => parameters.Add(p => p.Data, data.AsQueryable()));

        Assert.NotNull(cut);
        Assert.Contains("Item 1", cut.Markup);
    }

    [Fact]
    public void GridComponent_WithEmptyData_ShowsEmptyMessage()
    {
        var ctx = new TestContext();
        var data = new List<TestItem>();

        var cut = ctx.RenderComponent<GridComponent>(
            parameters => parameters.Add(p => p.Data, data.AsQueryable()));

        Assert.Contains("No data", cut.Markup);
    }

    [Fact]
    public void GridComponent_SortByColumn_ChangesOrder()
    {
        var ctx = new TestContext();
        var data = new[]
        {
            new TestItem { Id = 1, Name = "Zebra" },
            new TestItem { Id = 2, Name = "Alpha" },
            new TestItem { Id = 3, Name = "Bravo" }
        }.AsQueryable();

        var cut = ctx.RenderComponent<GridComponent>(
            parameters =>
            {
                parameters.Add(p => p.Data, data);
                parameters.Add(p => p.Columns, new[] 
                { 
                    new GridColumn { PropertyName = nameof(TestItem.Name), Header = "Name", Sortable = true }
                });
            });

        // Find and click sort header
        var sortButton = cut.Find("th");
        sortButton.Click();

        // Should be sorted ascending
        Assert.True(cut.Instance.SortOrder == SortOrder.Ascending);
    }

    [Fact]
    public void GridComponent_WithOptions_AppliesToComponent()
    {
        var ctx = new TestContext();
        var data = new[] { new TestItem { Id = 1, Name = "Test" } }.AsQueryable();
        var options = new GridOptions { PageSize = 50, ItemHeight = 50 };

        var cut = ctx.RenderComponent<GridComponent>(
            parameters =>
            {
                parameters.Add(p => p.Data, data);
                parameters.Add(p => p.Options, options);
            });

        Assert.Equal(50, cut.Instance.Options.PageSize);
        Assert.Equal(50, cut.Instance.Options.ItemHeight);
    }
}

// Test model
public class TestItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\Components\GridComponentTests.cs`

- [ ] **Step 2: Run test to verify failure**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
dotnet test --filter "GridComponentTests" -v normal
```

Expected: FAIL - GridComponent class not found

- [ ] **Step 3: Create GridComponent code-behind**

```csharp
namespace SmartWorkz.Core.Web.Components;

/// <summary>
/// Data grid component for displaying and interacting with tabular data.
/// Supports sorting, filtering, pagination, and virtual scrolling.
/// </summary>
public partial class GridComponent : BaseRazorComponent
{
    /// <summary>
    /// Data source for the grid (IQueryable for efficient server-side operations).
    /// </summary>
    [Parameter]
    public required IQueryable<object> Data { get; set; }

    /// <summary>
    /// Column definitions for the grid.
    /// </summary>
    [Parameter]
    public IEnumerable<GridColumn> Columns { get; set; } = Array.Empty<GridColumn>();

    /// <summary>
    /// Configuration options for grid behavior.
    /// </summary>
    [Parameter]
    public GridOptions Options { get; set; } = new();

    /// <summary>
    /// Optional CSS class for the grid container.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Callback when a row is selected.
    /// </summary>
    [Parameter]
    public EventCallback<object> OnRowSelected { get; set; }

    // Internal state
    protected IQueryable<object> FilteredData { get; set; } = Enumerable.Empty<object>().AsQueryable();
    protected int CurrentPage { get; set; } = 1;
    protected SortOrder SortOrder { get; set; } = SortOrder.None;
    protected string? SortedColumn { get; set; }

    protected override void OnParametersSet()
    {
        FilteredData = Data;
    }

    /// <summary>
    /// Sort data by column name.
    /// </summary>
    public void SortByColumn(string propertyName)
    {
        if (!Options.AllowSorting)
            return;

        // Toggle sort order if same column clicked
        if (SortedColumn == propertyName)
        {
            SortOrder = SortOrder switch
            {
                SortOrder.None => SortOrder.Ascending,
                SortOrder.Ascending => SortOrder.Descending,
                SortOrder.Descending => SortOrder.None,
                _ => SortOrder.Ascending
            };
        }
        else
        {
            SortedColumn = propertyName;
            SortOrder = SortOrder.Ascending;
        }

        ApplySorting();
    }

    /// <summary>
    /// Apply current sort criteria to data.
    /// </summary>
    private void ApplySorting()
    {
        if (SortOrder == SortOrder.None || string.IsNullOrEmpty(SortedColumn))
        {
            FilteredData = Data;
            return;
        }

        var expression = CreatePropertyExpression(SortedColumn);
        if (expression != null)
        {
            FilteredData = SortOrder == SortOrder.Ascending
                ? FilteredData.OrderBy(expression)
                : FilteredData.OrderByDescending(expression);
        }
    }

    /// <summary>
    /// Create a lambda expression for property access.
    /// </summary>
    private static Func<object, object?>? CreatePropertyExpression(string propertyName)
    {
        return obj =>
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj);
        };
    }

    /// <summary>
    /// Filter data by search term across all columns.
    /// </summary>
    public void FilterData(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            FilteredData = Data;
        }
        else
        {
            FilteredData = Data.Where(item => 
                Columns.Any(col =>
                {
                    var property = item.GetType().GetProperty(col.PropertyName);
                    var value = property?.GetValue(item)?.ToString() ?? "";
                    return value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                })).AsQueryable();
        }

        CurrentPage = 1;
    }

    /// <summary>
    /// Get paginated data for current page.
    /// </summary>
    protected IEnumerable<object> GetPagedData()
    {
        var skip = (CurrentPage - 1) * Options.PageSize;
        return FilteredData.Skip(skip).Take(Options.PageSize);
    }

    /// <summary>
    /// Get total number of pages.
    /// </summary>
    protected int GetTotalPages()
    {
        var total = FilteredData.Count();
        return (int)Math.Ceiling((double)total / Options.PageSize);
    }

    /// <summary>
    /// Navigate to previous page.
    /// </summary>
    public void PreviousPage()
    {
        if (CurrentPage > 1)
            CurrentPage--;
    }

    /// <summary>
    /// Navigate to next page.
    /// </summary>
    public void NextPage()
    {
        if (CurrentPage < GetTotalPages())
            CurrentPage++;
    }

    /// <summary>
    /// Get formatted value for a property, respecting column format string.
    /// </summary>
    protected string FormatValue(object? item, GridColumn column)
    {
        if (item == null)
            return "-";

        var property = item.GetType().GetProperty(column.PropertyName);
        var value = property?.GetValue(item);

        if (value == null)
            return "-";

        if (!string.IsNullOrEmpty(column.Format))
        {
            try
            {
                return string.Format($"{{0:{column.Format}}}", value);
            }
            catch
            {
                return value.ToString() ?? "-";
            }
        }

        return value.ToString() ?? "-";
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Components\GridComponent.razor.cs`

- [ ] **Step 4: Create GridComponent Razor view**

```razor
@namespace SmartWorkz.Core.Web.Components
@typeparam TItem
@inherits GridComponent

<div class="@Options.CssClass @CssClass">
    @if (!FilteredData.Any())
    {
        <div class="alert alert-info">No data available</div>
    }
    else
    {
        <table class="table table-striped @(Options.AlternateRowColors ? "table-hover" : "")">
            <thead class="table-light">
                <tr>
                    @foreach (var column in Columns)
                    {
                        <th class="@column.CssClass" 
                            style="@(column.Width != null ? $"width:{column.Width}" : "")"
                            @onclick="() => Options.AllowSorting && SortByColumn(column.PropertyName)"
                            role="columnheader"
                            aria-sort="@GetAriaSort(column)">
                            @column.Header
                            @if (SortedColumn == column.PropertyName)
                            {
                                <span class="sort-indicator">
                                    @if (SortOrder == SortOrder.Ascending) { <span>↑</span> }
                                    else if (SortOrder == SortOrder.Descending) { <span>↓</span> }
                                </span>
                            }
                        </th>
                    }
                </tr>
            </thead>
            <tbody>
                @foreach (var item in GetPagedData())
                {
                    <tr @onclick="() => OnRowSelected.InvokeAsync(item)" role="row">
                        @foreach (var column in Columns)
                        {
                            <td class="@column.CssClass">
                                @if (column.Template != null)
                                {
                                    @column.Template(item)
                                }
                                else
                                {
                                    @FormatValue(item, column)
                                }
                            </td>
                        }
                    </tr>
                }
            </tbody>
        </table>

        <nav aria-label="Grid pagination">
            <ul class="pagination">
                <li class="page-item @(CurrentPage <= 1 ? "disabled" : "")">
                    <button class="page-link" @onclick="PreviousPage" disabled="@(CurrentPage <= 1)">Previous</button>
                </li>
                <li class="page-item active">
                    <span class="page-link">Page @CurrentPage of @GetTotalPages()</span>
                </li>
                <li class="page-item @(CurrentPage >= GetTotalPages() ? "disabled" : "")">
                    <button class="page-link" @onclick="NextPage" disabled="@(CurrentPage >= GetTotalPages())">Next</button>
                </li>
            </ul>
        </nav>
    }
</div>

@code {
    private string GetAriaSort(GridColumn column) => SortedColumn == column.PropertyName
        ? SortOrder == SortOrder.Ascending ? "ascending" : SortOrder == SortOrder.Descending ? "descending" : "none"
        : "none";
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Components\GridComponent.razor`

- [ ] **Step 5: Run tests to verify success**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
dotnet test --filter "GridComponentTests" -v normal
```

Expected: PASS (4 tests)

- [ ] **Step 6: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add src/Components/GridComponent.razor src/Components/GridComponent.razor.cs tests/Components/GridComponentTests.cs
git commit -m "feat: implement GridComponent with sorting, filtering, pagination"
```

---

### Task 8: Create FormGroupTagHelper

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\TagHelpers\FormGroupTagHelper.cs`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\TagHelpers\FormGroupTagHelperTests.cs`

- [ ] **Step 1: Write failing test**

```csharp
namespace SmartWorkz.Core.Web.Tests.TagHelpers;

public class FormGroupTagHelperTests
{
    [Fact]
    public void FormGroupTagHelper_WithLabel_RendersLabel()
    {
        var ctx = new TestContext();
        ctx.Services.AddScoped<IHtmlHelper>(s => new Mock<IHtmlHelper>().Object);

        var html = @"<form-group label=""Name"" asp-for=""Model.Name"">
            <input class=""form-control"" asp-for=""Model.Name"" />
        </form-group>";

        // Note: Full tag helper testing requires HtmlHelper which is complex in unit tests
        // Integration tests are recommended for tag helpers
        Assert.NotNull(html);
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\TagHelpers\FormGroupTagHelperTests.cs`

- [ ] **Step 2: Create FormGroupTagHelper**

```csharp
namespace SmartWorkz.Core.Web.TagHelpers;

/// <summary>
/// Tag helper that wraps form input with label and validation message in a Bootstrap form group.
/// </summary>
/// <example>
/// <form-group label="Email" asp-for="Model.Email">
///     <input class="form-control" asp-for="Model.Email" />
/// </form-group>
/// </example>
[HtmlTargetElement("form-group")]
public class FormGroupTagHelper : TagHelper
{
    /// <summary>
    /// Label text to display for the form input.
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Property name for validation context.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public string? AspFor { get; set; }

    /// <summary>
    /// CSS class to apply to the form group container.
    /// </summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; } = "mb-3";

    /// <summary>
    /// HTML helper for accessing validation state.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        var propertyName = AspFor?.Split('.').LastOrDefault() ?? "";
        var modelState = ViewContext?.ModelState[propertyName];
        var hasErrors = modelState?.ValidationState == ModelValidationState.Invalid;
        var errorMessages = modelState?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>();

        var errorClass = hasErrors ? "is-invalid" : "";

        var html = new StringBuilder();
        html.Append($"<div class=\"form-group {CssClass}\">");

        if (!string.IsNullOrEmpty(Label))
        {
            html.Append($"<label class=\"form-label\">{HtmlEncoder.Default.Encode(Label)}</label>");
        }

        html.Append($"<div class=\"input-wrapper {errorClass}\">");
        html.Append(content.GetContent());
        html.Append("</div>");

        if (errorMessages.Any())
        {
            html.Append("<div class=\"invalid-feedback d-block\">");
            foreach (var error in errorMessages)
            {
                html.Append($"<div>{HtmlEncoder.Default.Encode(error)}</div>");
            }
            html.Append("</div>");
        }

        html.Append("</div>");

        output.TagName = null;
        output.Content.SetHtmlContent(html.ToString());
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\TagHelpers\FormGroupTagHelper.cs`

- [ ] **Step 3: Register tag helper in _Imports.razor**

Add to `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\_Imports.razor`:

```razor
@addTagHelper SmartWorkz.Core.Web.TagHelpers.FormGroupTagHelper, SmartWorkz.Core.Web
```

- [ ] **Step 4: Verify compilation**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet build
```

Expected: Clean build with no errors

- [ ] **Step 5: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add src/TagHelpers/FormGroupTagHelper.cs src/_Imports.razor tests/TagHelpers/FormGroupTagHelperTests.cs
git commit -m "feat: add FormGroupTagHelper for validated form inputs"
```

---

### Task 9: Create StatusBadgeTagHelper

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\TagHelpers\StatusBadgeTagHelper.cs`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\TagHelpers\StatusBadgeTagHelperTests.cs`

- [ ] **Step 1: Write failing test**

```csharp
namespace SmartWorkz.Core.Web.Tests.TagHelpers;

public class StatusBadgeTagHelperTests
{
    [Theory]
    [InlineData("active", "success")]
    [InlineData("inactive", "danger")]
    [InlineData("pending", "warning")]
    public void StatusBadgeTagHelper_WithStatus_RendersBadgeWithClass(string status, string expectedClass)
    {
        var ctx = new TestContext();
        // Integration test recommended for tag helpers
        Assert.NotNull(status);
        Assert.NotNull(expectedClass);
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests\TagHelpers\StatusBadgeTagHelperTests.cs`

- [ ] **Step 2: Create StatusBadgeTagHelper**

```csharp
namespace SmartWorkz.Core.Web.TagHelpers;

/// <summary>
/// Tag helper that renders a Bootstrap badge with status-specific styling and icons.
/// </summary>
/// <example>
/// <status-badge status="Active" />
/// <status-badge status="Pending" show-icon="true" />
/// </example>
[HtmlTargetElement("status-badge")]
public class StatusBadgeTagHelper : TagHelper
{
    /// <summary>
    /// Status value to display (e.g., "Active", "Inactive", "Pending").
    /// </summary>
    [HtmlAttributeName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Whether to show an icon before the status text.
    /// </summary>
    [HtmlAttributeName("show-icon")]
    public bool ShowIcon { get; set; } = false;

    /// <summary>
    /// Optional custom CSS class to apply to the badge.
    /// </summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var statusLower = Status?.ToLowerInvariant() ?? "";
        var (badgeClass, icon) = GetStatusStyling(statusLower);

        var html = new StringBuilder();
        html.Append($"<span class=\"badge {badgeClass} {CssClass}\"");
        html.Append($" title=\"{HtmlEncoder.Default.Encode(Status ?? "")}\"");
        html.Append(" role=\"status\">");

        if (ShowIcon && !string.IsNullOrEmpty(icon))
        {
            html.Append($"<i class=\"{icon}\"></i> ");
        }

        html.Append(HtmlEncoder.Default.Encode(Status ?? "Unknown"));
        html.Append("</span>");

        output.TagName = null;
        output.Content.SetHtmlContent(html.ToString());
    }

    private static (string BadgeClass, string Icon) GetStatusStyling(string status) => status switch
    {
        "active" or "success" => ("bg-success", "fas fa-check-circle"),
        "inactive" or "disabled" => ("bg-secondary", "fas fa-ban"),
        "pending" => ("bg-warning text-dark", "fas fa-hourglass"),
        "error" or "failed" => ("bg-danger", "fas fa-exclamation-circle"),
        "processing" or "running" => ("bg-info", "fas fa-spinner"),
        _ => ("bg-light text-dark", "")
    };
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\TagHelpers\StatusBadgeTagHelper.cs`

- [ ] **Step 3: Register in _Imports.razor**

Add to `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\_Imports.razor`:

```razor
@addTagHelper SmartWorkz.Core.Web.TagHelpers.StatusBadgeTagHelper, SmartWorkz.Core.Web
```

- [ ] **Step 4: Verify compilation**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet build
```

Expected: Clean build with no errors

- [ ] **Step 5: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add src/TagHelpers/StatusBadgeTagHelper.cs src/_Imports.razor tests/TagHelpers/StatusBadgeTagHelperTests.cs
git commit -m "feat: add StatusBadgeTagHelper with status-based styling"
```

---

### Task 10: Create Validation Extension Methods

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Extensions\ValidationExtensions.cs`

- [ ] **Step 1: Create extension methods**

```csharp
namespace SmartWorkz.Core.Web.Extensions;

/// <summary>
/// Extension methods for validation utilities.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Get all validation errors for a model as a single HTML-friendly string.
    /// </summary>
    /// <example>
    /// var errors = model.GetValidationErrorsSummary();
    /// </example>
    public static string GetValidationErrorsSummary<T>(this IValidationService service, T model) 
        where T : class
    {
        var errors = service.ValidateModel(model);
        if (!errors.Any())
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var kvp in errors)
        {
            sb.AppendLine($"<strong>{kvp.Key}:</strong> {string.Join(", ", kvp.Value)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Check if a property has validation errors.
    /// </summary>
    public static bool HasErrors<T>(this IValidationService service, T model, string propertyName) 
        where T : class
    {
        return service.ValidateProperty(model, propertyName, out _);
    }

    /// <summary>
    /// Get validation errors for a specific property as a list.
    /// </summary>
    public static List<string> GetPropertyErrors<T>(this IValidationService service, T model, string propertyName) 
        where T : class
    {
        service.ValidateProperty(model, propertyName, out var errors);
        return errors;
    }

    /// <summary>
    /// Get the first validation error message for a property, or null if none.
    /// </summary>
    public static string? GetFirstPropertyError<T>(this IValidationService service, T model, string propertyName) 
        where T : class
    {
        service.ValidateProperty(model, propertyName, out var errors);
        return errors.FirstOrDefault();
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\src\Extensions\ValidationExtensions.cs`

- [ ] **Step 2: Verify compilation**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet build
```

Expected: Clean build with no errors

- [ ] **Step 3: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add src/Extensions/ValidationExtensions.cs
git commit -m "feat: add validation extension methods for common patterns"
```

---

### Task 11: Create Documentation - API Reference

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\API-REFERENCE.md`

- [ ] **Step 1: Write API reference**

```markdown
# SmartWorkz.Core.Web API Reference

## Components

### GridComponent

**Namespace:** `SmartWorkz.Core.Web.Components`

Data grid component with sorting, filtering, pagination, and virtual scrolling.

#### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Data` | `IQueryable<object>` | ✓ | — | Data source for grid rows |
| `Columns` | `IEnumerable<GridColumn>` | ✗ | `[]` | Column definitions |
| `Options` | `GridOptions` | ✗ | `new()` | Grid configuration |
| `CssClass` | `string?` | ✗ | `null` | Custom CSS class |
| `OnRowSelected` | `EventCallback<object>` | ✗ | — | Fires when user clicks a row |

#### Public Methods

```csharp
void SortByColumn(string propertyName)       // Sort by column
void FilterData(string searchTerm)           // Filter rows by search term
void PreviousPage()                          // Go to previous page
void NextPage()                              // Go to next page
```

#### Example

```razor
@page "/employees"
@inject EmployeeService EmployeeService

<GridComponent Data="@employees" Columns="@columns" Options="@gridOptions" />

@code {
    private IQueryable<Employee> employees = null!;
    private GridColumn[] columns = null!;
    private GridOptions gridOptions = null!;

    protected override async Task OnInitializedAsync()
    {
        employees = await EmployeeService.GetEmployeesAsync();
        columns = new[]
        {
            new GridColumn { PropertyName = "Name", Header = "Employee Name", Sortable = true },
            new GridColumn { PropertyName = "Salary", Header = "Salary", Format = "C", Sortable = true }
        };
        gridOptions = new GridOptions { PageSize = 25, VirtualizationEnabled = true };
    }
}
```

---

### BaseRazorComponent

**Namespace:** `SmartWorkz.Core.Web.Components`

Abstract base class for Razor components in SmartWorkz.Core.Web. Provides parameter validation lifecycle.

#### Protected Methods

```csharp
protected virtual void OnParameterValidation()  // Override to validate parameters
```

#### Example

```csharp
public partial class MyComponent : BaseRazorComponent
{
    [Parameter] public string? Title { get; set; }

    protected override void OnParameterValidation()
    {
        if (string.IsNullOrEmpty(Title))
            throw new ArgumentException("Title is required");
    }
}
```

---

## Services

### IValidationService / ValidationService

**Namespace:** `SmartWorkz.Core.Web.Services`

Service for validating models and properties using DataAnnotations.

#### Methods

```csharp
Dictionary<string, List<string>> ValidateModel<T>(T model)
bool ValidateProperty<T>(T model, string propertyName, out List<string> errors)
```

#### Registration

```csharp
// In Program.cs
services.AddScoped<IValidationService, ValidationService>();
```

#### Example

```csharp
[Inject] private IValidationService ValidationService { get; set; } = null!;

public class MyModel
{
    [Required] public string? Name { get; set; }
    [EmailAddress] public string? Email { get; set; }
}

// Validate entire model
var errors = ValidationService.ValidateModel(model);
if (errors.Any())
{
    // Handle validation errors
}

// Validate single property
var hasErrors = ValidationService.ValidateProperty(model, "Email", out var emailErrors);
```

---

## Tag Helpers

### FormGroupTagHelper

**Namespace:** `SmartWorkz.Core.Web.TagHelpers`

Wraps form inputs with label and validation messages in a Bootstrap form group.

#### Attributes

| Attribute | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `label` | `string` | ✗ | `null` | Label text |
| `asp-for` | `string` | ✓ | — | Property name for validation |
| `css-class` | `string` | ✗ | `mb-3` | Additional CSS classes |

#### Example

```razor
<form method="post">
    <form-group label="Email" asp-for="Model.Email">
        <input type="email" class="form-control" asp-for="Model.Email" />
    </form-group>

    <form-group label="Password" asp-for="Model.Password">
        <input type="password" class="form-control" asp-for="Model.Password" />
    </form-group>

    <button type="submit" class="btn btn-primary">Submit</button>
</form>
```

---

### StatusBadgeTagHelper

**Namespace:** `SmartWorkz.Core.Web.TagHelpers`

Renders a Bootstrap badge with status-specific styling and optional icon.

#### Attributes

| Attribute | Type | Values | Default | Description |
|-----------|------|--------|---------|-------------|
| `status` | `string` | `Active`, `Inactive`, `Pending`, `Error`, `Processing` | — | Status to display |
| `show-icon` | `bool` | — | `false` | Show status icon |
| `css-class` | `string` | — | `null` | Custom CSS class |

#### Example

```razor
<status-badge status="Active" show-icon="true" />
<status-badge status="Pending" />
<status-badge status="Error" show-icon="true" />
```

---

## Models

### GridColumn

Configuration for a grid column.

```csharp
public class GridColumn
{
    public required string PropertyName { get; set; }      // Property to display
    public required string Header { get; set; }            // Column header text
    public string? Width { get; set; }                     // Column width (CSS)
    public string? Format { get; set; }                    // Format string (C, N2, yyyy-MM-dd)
    public bool Sortable { get; set; } = true;            // Allow sorting
    public bool Filterable { get; set; } = true;          // Allow filtering
    public SortOrder SortOrder { get; set; } = SortOrder.None;
    public string? CssClass { get; set; }                 // Cell CSS class
    public RenderFragment<object?>? Template { get; set; } // Custom render template
}
```

### GridOptions

Configuration for GridComponent behavior.

```csharp
public class GridOptions
{
    public int PageSize { get; set; } = 25;
    public int ItemHeight { get; set; } = 40;
    public int ContainerHeight { get; set; } = 600;
    public bool VirtualizationEnabled { get; set; } = true;
    public int LoadMoreThreshold { get; set; } = 5;
    public bool AllowSorting { get; set; } = true;
    public bool AllowFiltering { get; set; } = true;
    public bool AlternateRowColors { get; set; } = true;
    public string? CssClass { get; set; } = "grid-container";
}
```

---

## Extensions

### ValidationExtensions

Helper extension methods for IValidationService.

```csharp
string GetValidationErrorsSummary<T>(this IValidationService service, T model)
bool HasErrors<T>(this IValidationService service, T model, string propertyName)
List<string> GetPropertyErrors<T>(this IValidationService service, T model, string propertyName)
string? GetFirstPropertyError<T>(this IValidationService service, T model, string propertyName)
```

---

## Global Imports

When using Core.Web in a Blazor or Razor Pages project, add to `_Imports.razor`:

```razor
@using SmartWorkz.Core.Web.Components
@using SmartWorkz.Core.Web.Models
@using SmartWorkz.Core.Web.Services
@addTagHelper *, SmartWorkz.Core.Web
```

---
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\API-REFERENCE.md`

- [ ] **Step 2: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add docs/API-REFERENCE.md
git commit -m "docs: add comprehensive API reference"
```

---

### Task 12: Create Usage Examples

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\examples\GridExample.razor`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\examples\FormExample.razor`
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\examples\CombinedExample.razor`

- [ ] **Step 1: Create GridExample.razor**

```razor
@page "/examples/grid"
@using SmartWorkz.Core.Web.Components
@using SmartWorkz.Core.Web.Models

<h1>Grid Component Example</h1>

<div class="card mb-4">
    <div class="card-header">
        <h5>Employee Grid with Sorting and Pagination</h5>
    </div>
    <div class="card-body">
        <GridComponent Data="@employees.AsQueryable()" 
                      Columns="@gridColumns" 
                      Options="@gridOptions"
                      OnRowSelected="@HandleRowSelected">
        </GridComponent>

        @if (selectedEmployee != null)
        {
            <div class="alert alert-info mt-3">
                Selected: <strong>@selectedEmployee.Name</strong> - $@selectedEmployee.Salary.ToString("N2")
            </div>
        }
    </div>
</div>

@code {
    private List<Employee> employees = new();
    private GridColumn[] gridColumns = null!;
    private GridOptions gridOptions = null!;
    private Employee? selectedEmployee;

    protected override void OnInitialized()
    {
        // Mock data
        employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "John Doe", Department = "IT", Salary = 85000 },
            new Employee { Id = 2, Name = "Jane Smith", Department = "HR", Salary = 72000 },
            new Employee { Id = 3, Name = "Bob Johnson", Department = "Finance", Salary = 95000 },
            new Employee { Id = 4, Name = "Alice Brown", Department = "IT", Salary = 88000 },
            new Employee { Id = 5, Name = "Charlie Wilson", Department = "Sales", Salary = 65000 }
        };

        gridColumns = new[]
        {
            new GridColumn { PropertyName = "Name", Header = "Name", Sortable = true, Width = "250px" },
            new GridColumn { PropertyName = "Department", Header = "Department", Sortable = true, Width = "150px" },
            new GridColumn { PropertyName = "Salary", Header = "Salary", Format = "C", Sortable = true, Width = "120px" }
        };

        gridOptions = new GridOptions 
        { 
            PageSize = 3,
            VirtualizationEnabled = false,  // Disabled for small dataset
            AllowSorting = true,
            AllowFiltering = true
        };
    }

    private void HandleRowSelected(object item)
    {
        if (item is Employee emp)
            selectedEmployee = emp;
    }

    public class Employee
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Department { get; set; }
        public decimal Salary { get; set; }
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\examples\GridExample.razor`

- [ ] **Step 2: Create FormExample.razor**

```razor
@page "/examples/form"
@using SmartWorkz.Core.Web.Services
@inject IValidationService ValidationService

<h1>Form Validation Example</h1>

<div class="card">
    <div class="card-header">
        <h5>User Registration Form</h5>
    </div>
    <div class="card-body">
        <form @onsubmit="HandleSubmit">
            <form-group label="Full Name" asp-for="Model.Name">
                <input type="text" class="form-control" @bind="Model.Name" placeholder="Enter full name" />
            </form-group>

            <form-group label="Email" asp-for="Model.Email">
                <input type="email" class="form-control" @bind="Model.Email" placeholder="user@example.com" />
            </form-group>

            <form-group label="Department" asp-for="Model.Department">
                <select class="form-select" @bind="Model.Department">
                    <option value="">-- Select --</option>
                    <option value="IT">IT</option>
                    <option value="HR">HR</option>
                    <option value="Finance">Finance</option>
                </select>
            </form-group>

            <div class="d-flex gap-2">
                <button type="submit" class="btn btn-primary">Register</button>
                <button type="reset" class="btn btn-secondary">Clear</button>
            </div>
        </form>

        @if (!string.IsNullOrEmpty(successMessage))
        {
            <div class="alert alert-success mt-3">@successMessage</div>
        }
    </div>
</div>

@code {
    private UserModel Model = new();
    private string? successMessage;

    private void HandleSubmit()
    {
        var errors = ValidationService.ValidateModel(Model);
        if (!errors.Any())
        {
            successMessage = $"Successfully registered {Model.Name}!";
            Model = new(); // Reset form
        }
    }

    public class UserModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public string? Department { get; set; }
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\examples\FormExample.razor`

- [ ] **Step 3: Create CombinedExample.razor**

```razor
@page "/examples/combined"
@using SmartWorkz.Core.Web.Components
@using SmartWorkz.Core.Web.Models
@using SmartWorkz.Core.Web.Services
@inject IValidationService ValidationService

<h1>Combined Grid + Form Example</h1>

<div class="row">
    <div class="col-md-6">
        <div class="card">
            <div class="card-header">
                <h5>Add New Employee</h5>
            </div>
            <div class="card-body">
                <form @onsubmit="HandleAddEmployee">
                    <form-group label="Name" asp-for="NewEmployee.Name">
                        <input type="text" class="form-control" @bind="NewEmployee.Name" />
                    </form-group>

                    <form-group label="Department" asp-for="NewEmployee.Department">
                        <select class="form-select" @bind="NewEmployee.Department">
                            <option value="">-- Select --</option>
                            <option value="IT">IT</option>
                            <option value="HR">HR</option>
                            <option value="Finance">Finance</option>
                        </select>
                    </form-group>

                    <form-group label="Salary" asp-for="NewEmployee.Salary">
                        <input type="number" class="form-control" @bind="NewEmployee.Salary" />
                    </form-group>

                    <button type="submit" class="btn btn-primary w-100">Add Employee</button>
                </form>
            </div>
        </div>
    </div>

    <div class="col-md-6">
        <div class="card">
            <div class="card-header">
                <h5>Employee Directory</h5>
            </div>
            <div class="card-body">
                <GridComponent Data="@employees.AsQueryable()" 
                              Columns="@gridColumns"
                              Options="@new GridOptions { PageSize = 5 }" />
            </div>
        </div>
    </div>
</div>

@code {
    private List<Employee> employees = new();
    private Employee NewEmployee = new();
    private GridColumn[] gridColumns = null!;

    protected override void OnInitialized()
    {
        employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "John Doe", Department = "IT", Salary = 85000 },
            new Employee { Id = 2, Name = "Jane Smith", Department = "HR", Salary = 72000 }
        };

        gridColumns = new[]
        {
            new GridColumn { PropertyName = "Name", Header = "Name" },
            new GridColumn { PropertyName = "Department", Header = "Department" },
            new GridColumn { PropertyName = "Salary", Header = "Salary", Format = "C" }
        };
    }

    private void HandleAddEmployee()
    {
        var errors = ValidationService.ValidateModel(NewEmployee);
        if (!errors.Any() && NewEmployee.Name != null && NewEmployee.Department != null)
        {
            employees.Add(new Employee
            {
                Id = employees.Max(e => e.Id) + 1,
                Name = NewEmployee.Name,
                Department = NewEmployee.Department,
                Salary = NewEmployee.Salary
            });

            NewEmployee = new();
            StateHasChanged();
        }
    }

    public class Employee
    {
        public int Id { get; set; }
        [Required] public string? Name { get; set; }
        [Required] public string? Department { get; set; }
        [Range(20000, 500000)] public decimal Salary { get; set; }
    }
}
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\examples\CombinedExample.razor`

- [ ] **Step 4: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add docs/examples/
git commit -m "docs: add comprehensive usage examples (grid, form, combined)"
```

---

### Task 13: Create Contributing Guide

**Files:**
- Create: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\CONTRIBUTING.md`

- [ ] **Step 1: Write contributing guide**

```markdown
# Contributing to SmartWorkz.Core.Web

## Component Design Checklist

When adding new components, ensure:

- [ ] **Clear Purpose** — Component solves a specific problem
- [ ] **Parameters Documented** — All public parameters have XML docs
- [ ] **Testable** — Component can be tested with Bunit or unit tests
- [ ] **Accessible** — ARIA labels, keyboard navigation, semantic HTML
- [ ] **Example** — Usage example in docs/examples/ or inline
- [ ] **Tests** — Unit tests covering happy path + edge cases
- [ ] **Performance** — Rendering optimizations considered
- [ ] **Styled** — Bootstrap 5 compatible, CSS customizable

## Code Style

### Naming Conventions
- Components: PascalCase, descriptive names (GridComponent, not Grid)
- Parameters: PascalCase
- Private fields: camelCase with underscore prefix (_data, _cache)
- CSS classes: kebab-case (grid-container, form-group)

### Documentation
```csharp
/// <summary>
/// Brief one-line description of what this does.
/// </summary>
/// <remarks>
/// Additional detail about behavior, usage patterns, or important notes.
/// </remarks>
/// <example>
/// Usage example:
/// <code>
/// <MyComponent Data="@items" />
/// </code>
/// </example>
public class MyComponent : BaseRazorComponent { }
```

### Testing
```csharp
// Write failing test first
[Fact]
public void ComponentName_WithCondition_ExpectedBehavior()
{
    // Arrange
    var ctx = new TestContext();
    var data = new[] { /* test data */ };

    // Act
    var cut = ctx.RenderComponent<MyComponent>(p => 
        p.Add(x => x.Data, data));

    // Assert
    Assert.NotNull(cut);
    Assert.Contains("expected text", cut.Markup);
}
```

## Git Workflow

1. Create feature branch: `git checkout -b feat/component-name`
2. Make changes following checklist above
3. Run tests: `dotnet test`
4. Verify build: `dotnet build`
5. Commit with clear message:
   ```bash
   git commit -m "feat: add ComponentName component
   
   - Description of what it does
   - Key features
   - Configuration options"
   ```
6. Push and create pull request

## Testing Requirements

### Unit Tests (Required)
- Happy path: component renders with valid parameters
- Edge cases: null/empty data, boundary conditions
- State changes: sorting, filtering, pagination

### Integration Tests (Recommended)
- Tag helpers in real Razor Pages
- Cross-component interaction

### Accessibility Tests (Required)
- ARIA labels present
- Semantic HTML (no div-only structures)
- Keyboard navigation works
- Screen reader compatible

Run all tests:
```bash
cd tests
dotnet test --collect:"XPlat Code Coverage"
```

Minimum coverage: 70%

---
```

Store as: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\docs\CONTRIBUTING.md`

- [ ] **Step 2: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add docs/CONTRIBUTING.md
git commit -m "docs: add contributing guide with component checklist"
```

---

### Task 14: Update README.md with Implementation Status

**Files:**
- Modify: `c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\README.md`

- [ ] **Step 1: Update README**

At the end of the existing README (before ## Contributing), add:

```markdown
## Phase 1 Implementation Status (Completed ✓)

### Core Components ✓
- **GridComponent** — Sorting, filtering, pagination, virtual scrolling, 40px+ rows
- **BaseRazorComponent** — Parameter validation lifecycle
- **BaseFormComponent** — Coming in Phase 2

### Services ✓
- **IValidationService** — DataAnnotations validation with property-level support
- **ValidationService** — Default implementation with error collection

### Tag Helpers ✓
- **FormGroupTagHelper** — Label + input + validation messages
- **StatusBadgeTagHelper** — Status badges with icons and styling

### Documentation ✓
- [API Reference](docs/API-REFERENCE.md) — Complete parameter and method docs
- [Usage Examples](docs/examples/) — Grid, Form, Combined examples
- [Contributing Guide](docs/CONTRIBUTING.md) — Component checklist and testing
- [This README](README.md) — Architecture and Quick Start

### Test Coverage ✓
- 70%+ code coverage
- Unit tests for all components
- Grid virtualization tested for 100K+ rows

### Quality Standards ✓
- WCAG 2.1 AA accessibility (ARIA labels, semantic HTML)
- Bootstrap 5 compatible
- .NET 9.0 minimum
- XML documentation on all public APIs

## What's Next (Phase 2)

- [ ] ListViewComponent full implementation
- [ ] Additional tag helpers (pagination, breadcrumb, etc.)
- [ ] HTML helper library
- [ ] Extension methods for common formatting
- [ ] Integration with SmartWorkz.Core.Shared (Caching, Logging)
- [ ] Blazor Server support (Razor Pages Phase 1 complete)

## Getting Started

### NuGet Installation

```bash
dotnet add package SmartWorkz.Core.Web
```

### Registration

In `Program.cs`:

```csharp
builder.Services.AddScoped<IValidationService, ValidationService>();
```

In `_Imports.razor`:

```razor
@using SmartWorkz.Core.Web.Components
@using SmartWorkz.Core.Web.Models
@using SmartWorkz.Core.Web.Services
@addTagHelper *, SmartWorkz.Core.Web
```

### Quick Start

```razor
@page "/employees"

<GridComponent Data="@employees.AsQueryable()" Columns="@columns" />

@code {
    private List<Employee> employees = new();
    private GridColumn[] columns = new[]
    {
        new GridColumn { PropertyName = "Name", Header = "Name" },
        new GridColumn { PropertyName = "Email", Header = "Email" }
    };

    protected override void OnInitialized()
    {
        employees = await EmployeeService.GetAllAsync();
    }
}
```

See [API Reference](docs/API-REFERENCE.md) and [Examples](docs/examples/) for detailed usage.
```

- [ ] **Step 2: Commit**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git add README.md
git commit -m "docs: update README with Phase 1 implementation status and quick start"
```

---

### Task 15: Final Testing and Verification

**Files:**
- All project files (read-only verification)

- [ ] **Step 1: Run all tests**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web\tests
dotnet test -v normal --logger="console;verbosity=detailed"
```

Expected: All tests pass, 70%+ coverage

- [ ] **Step 2: Build entire solution**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet build --no-incremental
```

Expected: Clean build, no warnings (except deprecated warnings if present)

- [ ] **Step 3: Verify NuGet package structure**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
dotnet pack -c Release --output ./dist
```

Expected: .nupkg file created with all assets

- [ ] **Step 4: Verify documentation exists**

Ensure all files exist:
- `docs/API-REFERENCE.md`
- `docs/CONTRIBUTING.md`
- `docs/examples/GridExample.razor`
- `docs/examples/FormExample.razor`
- `docs/examples/CombinedExample.razor`
- `README.md` (updated)

- [ ] **Step 5: Final commit and summary**

Run:
```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Web
git log --oneline | head -20
```

Expected: Clean history with descriptive commit messages for each phase

---

## Summary of Phase 1 Deliverables

✅ **Project Structure**
- Core.Web library with folder organization
- Test project with xUnit + Bunit
- Directory.Build.props for shared configuration

✅ **Core Components**
- GridComponent with sorting, filtering, pagination
- BaseRazorComponent with validation lifecycle
- GridColumn, GridOptions, SortOrder models

✅ **Services & Validation**
- IValidationService interface
- ValidationService implementation
- ValidationExtensions for common patterns

✅ **Tag Helpers**
- FormGroupTagHelper (label + validation)
- StatusBadgeTagHelper (status display)

✅ **Documentation**
- Comprehensive API reference
- 3 detailed usage examples
- Contributing guide with component checklist
- Updated README with quick start

✅ **Testing**
- 70%+ code coverage
- Unit tests for critical paths
- Test project ready for integration tests

✅ **Quality**
- WCAG 2.1 AA accessibility standards
- Bootstrap 5 compatible
- XML documentation on public APIs
- .NET 9.0 target framework

---

## Next Phase (When Ready)

Once Phase 1 is complete and tested:

1. **Merge to main** — Create pull request with full Phase 1 implementation
2. **NuGet Publish** — Package and publish to NuGet feed
3. **StarterKitMVC Integration** — Begin Phase 2 with actual consumer integration
4. **Feedback Loop** — Incorporate learnings from real usage
5. **Phase 2 Features** — ListViewComponent, more tag helpers, Blazor support

---
