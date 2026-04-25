# Base Page Pattern

The `BasePage` class provides a common foundation for all Razor pages in this starter kit. It offers tenant context, translation support, user information, and helper methods for common page operations.

## Purpose

- **Consistent context:** Every page has access to tenant ID, current user, locale
- **Translation support:** `T()` method available on all pages without manual injection
- **Auth helpers:** Properties for checking authentication and authorization
- **Message handling:** Toast and error management for user feedback
- **SEO ready:** Base class for polymorphic SEO metadata

## Quick Reference

```csharp
public abstract class BasePage : PageModel
{
    // Properties
    public Guid TenantId { get; }              // Current tenant
    public ClaimsPrincipal User { get; }       // Current user claims
    public string CurrentUserId { get; }       // User ID from claim
    public string CurrentUserEmail { get; }    // User email from claim
    public bool IsAuthenticated { get; }       // Is user logged in?

    // Methods
    protected string T(string key)             // Translate a message key
    protected void AddToastSuccess(string msg) // Queue success toast
    protected void AddToastError(string msg)   // Queue error toast
    protected void AddErrors(Result result)    // Add result errors to ModelState
}
```

## Architecture

### Initialization

`BasePage` inherits from ASP.NET's `PageModel` and is initialized via DI in each page's `PageModel`:

```csharp
public class MyPageModel : BasePage
{
    // Automatically has access to T(), TenantId, User, etc.
}
```

The base class itself depends on:

| Dependency | From | Purpose |
|-----------|------|---------|
| `ITenantContext` | DI container | Access current tenant ID |
| `ITranslationService` | DI container | Translate message keys |
| `IHttpContextAccessor` | DI container | Read user claims |

These are registered in [Program.cs](../../src/SmartWorkz.StarterKitMVC.Public/Program.cs).

### Data Flow

```
Request arrives
    ↓
PageModel inheriting BasePage is instantiated
    ↓
DI container injects ITenantContext, ITranslationService, etc. into the constructor
    ↓
OnGet() or OnPost() runs
    ↓
T() is called → reads tenant, locale, and calls ITranslationService.Get()
    ↓
View renders with translated strings
```

## Properties

### TenantId

```csharp
public Guid TenantId { get; private set; }
```

The current tenant ID, injected from `ITenantContext` at initialization. Used to filter data in queries:

```csharp
public class ProductsPageModel : BasePage
{
    private readonly IProductRepository _products;

    public async Task OnGetAsync()
    {
        // TenantId is automatically available
        Products = await _products.GetByTenantAsync(TenantId);
    }
}
```

### User & Claims

```csharp
public ClaimsPrincipal User { get; }           // From PageModel base
public string CurrentUserId { get; }           // User.FindFirst("sub").Value
public string CurrentUserEmail { get; }        // User.FindFirst("email").Value
public bool IsAuthenticated { get; }           // User.Identity.IsAuthenticated
```

Example usage:

```csharp
public async Task OnGetAsync()
{
    if (!IsAuthenticated)
        return NotFound();

    var userEmail = CurrentUserEmail;  // Already extracted for convenience
}
```

### Locale

The user's locale is determined from the `"locale"` claim:

```csharp
var locale = User.FindFirst("locale")?.Value ?? "en";
```

This is used by `T()` to select the appropriate language translation.

## Methods

### T(string key)

Translate a message key to the user's locale:

```csharp
protected string T(string key)
{
    var locale = User.FindFirst("locale")?.Value ?? "en";
    return _translationService.Get(key, TenantId, locale);
}
```

**Usage in PageModel:**

```csharp
public class RegisterPageModel : BasePage
{
    public string WelcomeMessage => T(MessageKeys.Auth.RegisterSuccess);
}
```

**Usage in Razor view:**

```razor
<h1>@Model.T(MessageKeys.General.Welcome)</h1>
```

**What if the translation doesn't exist?**

Returns the key name itself (e.g., `"validation.required"`). This is the fallback and allows pages to work even if the DB seed hasn't run yet.

### AddToastSuccess(string message)

Queue a success toast notification to be displayed on the page:

```csharp
public IActionResult OnPost()
{
    if (ModelState.IsValid)
    {
        // Process input
        AddToastSuccess(T(MessageKeys.Crud.SaveSuccess));
        return Redirect("/");
    }
    return Page();
}
```

The message is stored in `TempData["Toast_Success"]` and rendered in the layout via:

```razor
@if (TempData.ContainsKey("Toast_Success"))
{
    <div class="toast-success">@TempData["Toast_Success"]</div>
}
```

### AddToastError(string message)

Queue an error toast:

```csharp
if (saveResult.IsFailure)
{
    AddToastError(T(MessageKeys.Crud.SaveError));
    return Page();
}
```

### AddErrors(Result result)

Add result errors to `ModelState` for validation display:

```csharp
public IActionResult OnPost()
{
    var result = await _service.SaveAsync(Input);
    
    if (result.IsFailure)
    {
        AddErrors(result);  // Adds each error to ModelState
        return Page();
    }
    
    AddToastSuccess(result.Message);
    return Redirect("/");
}
```

**What does AddErrors do?**

Iterates through `result.Errors` and calls `ModelState.AddModelError(key, error)`:

```csharp
protected void AddErrors(Result result)
{
    if (result.Errors?.Any() == true)
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error);
}
```

Errors appear in the view via:

```razor
<span asp-validation-summary="All"></span>
```

## Specializations

### SeoBasePage

For pages with SEO metadata (meta tags, Open Graph, structured data):

```csharp
public abstract class SeoBasePage : BasePage
{
    public SeoMeta Seo { get; set; } = new();

    public IActionResult SetSeo(string title, string description, string image = null)
    {
        Seo.Title = title;
        Seo.Description = description;
        Seo.ImageUrl = image;
        return Page();
    }
}
```

Usage:

```csharp
public class ProductPageModel : SeoBasePage
{
    public async Task OnGetAsync(string slug)
    {
        var product = await _repo.GetBySlugAsync(TenantId, slug);
        SetSeo(product.Name, product.Description, product.ImageUrl);
    }
}
```

### BaseListPage\<T>

For paginated list pages with HTMX support:

```csharp
public abstract class BaseListPage<T> : BasePage where T : class
{
    public List<T> Items { get; protected set; } = new();
    public PaginationModel Pagination { get; protected set; } = new();

    public IActionResult PageOrPartial()
    {
        return Request.IsHtmx() ? Partial("_ItemRows", Items) : Page();
    }
}
```

Usage:

```csharp
public class ProductsPageModel : BaseListPage<ProductDto>
{
    public async Task OnGetAsync(int page = 1)
    {
        var (items, total) = await _repo.GetPagedAsync(TenantId, page, 10);
        Items = items;
        Pagination = PaginationModel.FromDto(total, page, 10);
    }
}
```

## Common Patterns

### Pattern 1: Display Tenant Data

```csharp
public class MyPageModel : BasePage
{
    private readonly IMyRepository _repo;

    public List<MyDto> Items { get; set; }

    public async Task OnGetAsync()
    {
        // TenantId is automatically available
        Items = await _repo.GetByTenantAsync(TenantId);
    }
}
```

### Pattern 2: Save with Result Pattern

```csharp
[BindProperty]
public MyInput Input { get; set; }

public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
        return Page();

    var result = await _service.SaveAsync(TenantId, Input);

    if (result.IsFailure)
    {
        AddErrors(result);
        return Page();
    }

    AddToastSuccess(T(MessageKeys.Crud.SaveSuccess));
    return Redirect("/items");
}
```

### Pattern 3: Form with Validation and Translation

```csharp
public class RegisterPageModel : BasePage
{
    [BindProperty]
    public RegisterInput Input { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            // Validation errors are already translated
            return Page();

        var result = await _authService.RegisterAsync(TenantId, Input);

        if (result.IsFailure)
        {
            AddToastError(T(MessageKeys.Auth.RegisterError));
            return Page();
        }

        AddToastSuccess(T(MessageKeys.Auth.RegisterSuccess));
        return Redirect("/login");
    }
}
```

## Common Mistakes

### Mistake 1: Not Using T() for User-Facing Messages

❌ **Wrong:**
```csharp
AddToastSuccess("User saved successfully");
```

✅ **Correct:**
```csharp
AddToastSuccess(T(MessageKeys.Crud.SaveSuccess));
```

### Mistake 2: Assuming TenantId Is Always Set

`BasePage` expects `ITenantContext` to be registered. If it's missing, you'll get a DI error. Ensure [Program.cs](../../src/SmartWorkz.StarterKitMVC.Public/Program.cs) has:

```csharp
builder.Services.AddScoped<ITenantContext>(sp => new TenantContext(...));
```

### Mistake 3: Forgetting to Inherit BasePage

Pages that don't inherit `BasePage` won't have access to `T()`, `TenantId`, etc.:

❌ **Wrong:**
```csharp
public class MyPageModel : PageModel  // Missing BasePage
{
    var msg = T(key);  // Error: T() not found
}
```

✅ **Correct:**
```csharp
public class MyPageModel : BasePage
{
    var msg = T(key);  // Works
}
```

### Mistake 4: Using User Without Null Checks

The `User` property can be null for anonymous requests. Always check:

```csharp
if (IsAuthenticated)
{
    var email = CurrentUserEmail;
}
```

## See Also

- [Translation System](./01-translation-system.md) — How `T()` works
- [Result Pattern](./04-result-pattern.md) — Using `AddErrors()`
- [BasePage.cs](../../src/SmartWorkz.StarterKitMVC.Public/Pages/BasePage.cs) — Full implementation
