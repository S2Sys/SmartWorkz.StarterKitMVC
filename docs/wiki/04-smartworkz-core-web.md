# SmartWorkz.Core.Web — Web Components & Services

## Assembly Reference

**ProjectReference:** `src/SmartWorkz.Core.Web/SmartWorkz.Core.Web.csproj`  
**Namespace:** `SmartWorkz.Core.Web`  
**Target Framework:** .NET 9 (Razor SDK)  
**Key Dependencies:**
- `HotChocolate.AspNetCore` v14.0.0 — GraphQL
- `HotChocolate.Types` v14.0.0 — GraphQL types
- `System.IdentityModel.Tokens.Jwt` v8.2.2 — JWT parsing

**Used by:** SmartWorkz.StarterKitMVC.Public portal (Admin uses traditional Razor Pages without these components).

---

## Tag Helpers

### FormGroupTagHelper — Validation-Aware Form Fields

Renders a Bootstrap 5 form group with label, input, and error messages.

```csharp
[HtmlTargetElement("form-group")]
public class FormGroupTagHelper : TagHelper
{
    [HtmlAttributeName("label")]
    public string? Label { get; set; }
    
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }
    
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }
}
```

**Usage in Razor Page:**
```html
<form-group label="Product Name" asp-for="ProductName" css-class="mb-3">
</form-group>
```

**Renders as:**
```html
<div class="mb-3">
    <label for="ProductName" class="form-label">Product Name</label>
    <input type="text" id="ProductName" name="ProductName" class="form-control" />
    <!-- ModelState errors automatically displayed below -->
</div>
```

**Features:**
- Auto-applies `is-invalid` class if field has errors
- Lists all validation errors from `ModelState[fieldname]`
- Integrates with ASP.NET Core data annotations

---

### StatusBadgeTagHelper — Status Indicators

Renders a Bootstrap badge with optional icon, colored by status.

```csharp
[HtmlTargetElement("status-badge")]
public class StatusBadgeTagHelper : TagHelper
{
    [HtmlAttributeName("status")]
    public string? Status { get; set; }  // "active", "inactive", "pending", "error", "processing"
    
    [HtmlAttributeName("show-icon")]
    public bool ShowIcon { get; set; } = true;
    
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }
}
```

**Status → Color Mapping:**

| Status | Badge Class | Icon |
|--------|-------------|------|
| `active` | `badge bg-success` | ✓ (checkmark) |
| `inactive` | `badge bg-secondary` | ○ (circle) |
| `pending` | `badge bg-warning` | ⏳ (hourglass) |
| `error` | `badge bg-danger` | ✗ (X) |
| `processing` | `badge bg-info` | ⟳ (spinner) |

**Usage:**
```html
<status-badge status="active" show-icon="true"></status-badge>
<!-- Renders: <span class="badge bg-success"><i class="fa-check"></i> Active</span> -->
```

---

## Blazor Components

### BaseRazorComponent — Component Base Class

Abstract base for all custom Blazor components with parameter validation.

```csharp
public abstract class BaseRazorComponent : ComponentBase
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
        OnParameterValidation();
    }
    
    protected virtual void OnParameterValidation()
    {
        // Override to validate parameters. Throw if invalid.
    }
}
```

**Usage:**
```csharp
public partial class ProductCard : BaseRazorComponent
{
    [Parameter]
    public int ProductId { get; set; }
    
    protected override void OnParameterValidation()
    {
        if (ProductId <= 0)
            throw new ArgumentException("ProductId must be > 0");
    }
}
```

---

### GridComponent — Data Grid with Sorting & Filtering

Blazor component for displaying and interacting with tabular data.

```csharp
<GridComponent @ref="grid"
    Data="products"
    Columns="gridColumns"
    Options="gridOptions"
    OnRowSelected="HandleRowSelected" />
```

**Parameters:**

| Parameter | Type | Purpose |
|-----------|------|---------|
| `Data` | `IQueryable<object>` | Data source (IEnumerable converted internally) |
| `Columns` | `IEnumerable<GridColumn>` | Column definitions (see below) |
| `Options` | `GridOptions` | Grid behavior configuration |
| `CssClass` | `string?` | Additional CSS for grid container |
| `OnRowSelected` | `EventCallback<object>` | Fires when user clicks a row |

**GridColumn Structure:**
```csharp
public class GridColumn
{
    public string PropertyName { get; set; }    // Data field
    public string Header { get; set; }          // Column heading
    public string? Format { get; set; }         // Optional: "currency", "date", custom
    public bool Sortable { get; set; } = true;
    public int Width { get; set; } = 120;       // In pixels
}
```

**GridOptions:**
```csharp
public class GridOptions
{
    public int PageSize { get; set; } = 25;
    public bool AllowSorting { get; set; } = true;
    public bool AllowFiltering { get; set; } = true;
    public bool VirtualizationEnabled { get; set; } = true;  // For 1000+ rows
    public bool AlternateRowColors { get; set; } = true;
}
```

**GridComponent Public Methods:**
```csharp
public void SortByColumn(string propertyName);      // Programmatic sort
public void FilterData(string searchTerm);           // Filter all columns by term
public IEnumerable<T> GetPagedData();               // Get current page data
public void PreviousPage() / void NextPage();       // Manual pagination
```

**Example:**
```csharp
@code {
    private GridComponent? grid;
    private List<GridColumn> columns = new()
    {
        new GridColumn { PropertyName = "Id", Header = "ID" },
        new GridColumn { PropertyName = "Name", Header = "Product Name" },
        new GridColumn { PropertyName = "Price", Header = "Price", Format = "currency" }
    };
    
    private async Task HandleRowSelected(object row)
    {
        var product = row as ProductDto;
        await JS.InvokeVoidAsync("console.log", $"Selected: {product.Name}");
    }
}
```

---

## Validation Services

### IValidationService — Model Validation Contract

```csharp
public interface IValidationService
{
    Dictionary<string, List<string>> ValidateModel<T>(T model);
    bool ValidateProperty<T>(T model, string propertyName, out List<string> errors);
}
```

**Usage:**
```csharp
var service = _serviceProvider.GetRequiredService<IValidationService>();
var errors = service.ValidateModel(myDto);
if (errors.Count == 0)
{
    // Valid; proceed
}
else
{
    foreach (var (field, msgs) in errors)
    {
        Console.WriteLine($"{field}: {string.Join(", ", msgs)}");
    }
}
```

---

### ValidationExtensions — Helper Methods

```csharp
public static class ValidationExtensions
{
    public static string GetValidationErrorsSummary<T>(this IValidationService service, T model);
    public static bool HasErrors<T>(this IValidationService service, T model, string propertyName);
    public static List<string> GetPropertyErrors<T>(this IValidationService service, T model, string propertyName);
    public static string? GetFirstPropertyError<T>(this IValidationService service, T model, string propertyName);
}
```

**Usage in Razor:**
```html
@{
    var validation = _validationService;
}
@if (validation.HasErrors(model, nameof(model.Email)))
{
    <div class="alert alert-danger">
        @validation.GetFirstPropertyError(model, nameof(model.Email))
    </div>
}
```

---

## GraphQL Setup

### AddGraphQLServices Extension

Registers HotChocolate GraphQL with pre-configured types and error handling.

```csharp
public static void AddGraphQLServices(this IServiceCollection services, IConfiguration configuration)
{
    // Registers:
    // - GraphQL Query type
    // - Built-in types: User, Transaction, Product, Report
    // - Error filter (masks internal errors, returns correlation ID)
    // - Introspection (enabled only in Development)
}
```

**Usage in Program.cs:**
```csharp
builder.Services.AddGraphQLServices(builder.Configuration);

var app = builder.Build();
app.UseRouting();
app.MapGraphQL();  // GraphQL endpoint at /graphql
```

---

### GraphQLAuthenticationMiddleware — JWT Validation

Validates Bearer tokens and attaches user claims to the GraphQL request context.

```csharp
public static class GraphQLAuthenticationMiddleware
{
    public static ClaimsPrincipal? ValidateToken(string token, string? issuer, string? audience);
    public static string? ExtractBearerToken(HttpContext context);
}
```

**How it works:**
1. Extracts "Bearer xyz..." from `Authorization` header
2. Parses JWT (validates signature, issuer, audience, expiration)
3. Returns `ClaimsPrincipal` with claims from token payload
4. Returns null if token is invalid

**Usage:**
```csharp
var token = GraphQLAuthenticationMiddleware.ExtractBearerToken(context);
if (token != null)
{
    var principal = GraphQLAuthenticationMiddleware.ValidateToken(token, "MyApp", "MyApp.Users");
    // Use principal.Claims to authorize resolver
}
```

---

## SortOrder Enum

```csharp
public enum SortOrder
{
    None,
    Ascending,
    Descending
}
```

Used by GridComponent for column sorting state.

---

## Complete GridComponent Example

**Razor Page with Grid:**
```csharp
@page "/products"
@model ProductsPageModel
@{
    ViewData["Title"] = "Products";
}

<div class="container mt-4">
    <h1>Products</h1>
    
    <GridComponent @ref="grid"
        Data="Model.Products"
        Columns="Model.GridColumns"
        Options="Model.GridOptions"
        OnRowSelected="Model.HandleRowSelected" />
</div>

@section Scripts {
    <script src="_framework/blazor.web.js"></script>
}
```

**Code-Behind:**
```csharp
public class ProductsPageModel : PageModel
{
    private readonly IProductService _productService;
    public IEnumerable<ProductDto> Products { get; set; }
    public List<GridColumn> GridColumns { get; set; }
    public GridOptions GridOptions { get; set; }
    
    public async Task OnGetAsync()
    {
        Products = await _productService.GetAllAsync();
        
        GridColumns = new()
        {
            new GridColumn { PropertyName = nameof(ProductDto.Id), Header = "ID" },
            new GridColumn { PropertyName = nameof(ProductDto.Name), Header = "Name" },
            new GridColumn { PropertyName = nameof(ProductDto.Price), Header = "Price", Format = "currency" },
            new GridColumn { PropertyName = nameof(ProductDto.CreatedAt), Header = "Created", Format = "date" }
        };
        
        GridOptions = new GridOptions
        {
            PageSize = 50,
            VirtualizationEnabled = true,
            AllowSorting = true
        };
    }
    
    public async Task HandleRowSelected(ProductDto product)
    {
        // Navigate to detail page, etc.
    }
}
```

---

**Next:** [SmartWorkz.Core.Shared — Caching, CQRS, Logging, Webhooks](05-smartworkz-core-shared.md)
