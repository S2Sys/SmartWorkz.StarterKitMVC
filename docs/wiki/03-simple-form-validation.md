# Simple Form Validation

## Purpose
Validate form inputs on both client and server using standard DataAnnotations with error messages from the translation database. Translate validation messages in `OnPostAsync` rather than custom validation attributes.

## Philosophy
For a starter kit, keep validation simple: use built-in `[Required]`, `[StringLength]`, `[Range]`, etc., with `ErrorMessage` pointing to translation keys. Translate those keys in `OnPostAsync` using `T()`. No custom attributes needed for 95% of cases.

## Quick Start

### Define InputModel with MessageKey ErrorMessages
```csharp
public class InputModel
{
    [Required(ErrorMessage = "validation.required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "validation.string_length")]
    public string Name { get; set; }

    [Required(ErrorMessage = "validation.required")]
    [EmailAddress(ErrorMessage = "validation.email_invalid")]
    public string Email { get; set; }

    [Required(ErrorMessage = "validation.required")]
    [Range(0.01, 999999.99, ErrorMessage = "validation.invalid_format")]
    public decimal Price { get; set; }
}
```

### In OnPostAsync, Translate and Add Errors
```csharp
[HttpPost]
public async Task<IActionResult> OnPostAsync()
{
    // Validate using DataAnnotations
    if (!ModelState.IsValid)
    {
        // Translate all error messages
        AddErrors(ModelState);
        return Page();
    }

    // Process form...
    return RedirectToPage("./Index");
}
```

The `AddErrors(ModelState)` method (from `BasePage`) translates all `ModelState` error keys:
```csharp
// BasePage.cs
protected void AddErrors(ModelStateDictionary modelState)
{
    foreach (var entry in modelState)
    {
        foreach (var error in entry.Value.Errors)
        {
            if (error.ErrorMessage != null)
            {
                var translated = T(error.ErrorMessage); // Lookup in DB
                Errors.Add(translated);
            }
        }
    }
}
```

### In Razor View, Display Errors
```html
<form method="post">
    <div class="mb-3">
        <label asp-for="Input.Name" class="form-label"></label>
        <input asp-for="Input.Name" class="form-control">
        <span asp-validation-for="Input.Name" class="text-danger small"></span>
    </div>

    <!-- Toast/error messages from AddErrors() -->
    @if (Errors.Any())
    {
        <div class="alert alert-danger">
            @foreach (var err in Errors)
            {
                <p>@err</p>
            }
        </div>
    }

    <button type="submit">Submit</button>
</form>
```

## How It Works

### Validation Flow

1. **User submits form** → `asp-for` binds to `InputModel`
2. **ASP.NET validates** → `[Required]`, `[StringLength]`, etc. run
3. **Client-side (JS)** → `asp-validation-for` shows errors from `ErrorMessage` attribute
4. **Server-side** → If client disabled, validation runs again
5. **OnPostAsync checks** → `if (!ModelState.IsValid)`
6. **Call AddErrors()** → Iterates `ModelState.Values`, translates each error key via `T()`
7. **Render view** → Error messages shown from `Errors` collection

### Translation Lookup Path

Error message key (e.g., `"validation.required"`) is looked up in database:

```csharp
// Assume Translations table has:
// | Key                    | Locale | Value                    |
// |------------------------|--------|--------------------------|
// | validation.required    | en     | This field is required   |
// | validation.required    | fr     | Ce champ est obligatoire |

var translated = T("validation.required");
// Returns "This field is required" (en) or "Ce champ est obligatoire" (fr)
// Falls back to key name if not found
```

## Built-In Attributes

| Attribute | Check | ErrorMessage Key | Example |
|-----------|-------|------------------|---------|
| `[Required]` | Not null/empty | `validation.required` | `[Required(ErrorMessage = "validation.required")]` |
| `[StringLength(max)]` | Length ≤ max | `validation.max_length` | `[StringLength(100)]` |
| `[StringLength(max, MinimumLength=min)]` | min ≤ length ≤ max | `validation.string_length` | `[StringLength(100, MinimumLength=3)]` |
| `[EmailAddress]` | Valid email format | `validation.email_invalid` | `[EmailAddress(ErrorMessage = "validation.email_invalid")]` |
| `[Range(min, max)]` | Value in range | `validation.invalid_format` | `[Range(0.01, 999999.99)]` |
| `[RegularExpression(pattern)]` | Regex match | `validation.invalid_format` | `[RegularExpression(@"^[A-Z][0-9]+$")]` |
| `[Compare(otherProperty)]` | Matches other field | `validation.invalid_format` | `[Compare(nameof(Password))]` |

## Example: Product Create Form

### InputModel
```csharp
public class InputModel
{
    [Required(ErrorMessage = "validation.required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "validation.string_length")]
    public string SKU { get; set; }

    [Required(ErrorMessage = "validation.required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "validation.string_length")]
    public string Name { get; set; }

    [Required(ErrorMessage = "validation.required")]
    [Range(0.01, 999999.99, ErrorMessage = "validation.invalid_format")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "validation.required")]
    [Range(0, 999999, ErrorMessage = "validation.invalid_format")]
    public int Stock { get; set; }
}
```

### OnPostAsync Handler
```csharp
[HttpPost]
public async Task<IActionResult> OnPostAsync()
{
    // Validate
    if (!ModelState.IsValid)
    {
        // Show translated validation errors
        AddErrors(ModelState);
        Categories = await _categoryRepository.GetAllAsync(TenantId);
        return Page();
    }

    // Build entity and save
    var product = new Product
    {
        SKU = Input.SKU,
        Name = Input.Name,
        Price = Input.Price,
        Stock = Input.Stock,
        TenantId = TenantId,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = UserId
    };

    var result = await _repository.CreateAsync(product);

    if (result == null)
    {
        AddToast(T("toast.error"), T("error.failed_to_create"), "danger");
        return Page();
    }

    AddToast(T("toast.success"), $"Product '{product.Name}' created.", "success");
    return RedirectToPage("./Index");
}
```

## Client-Side Rendering

The `asp-validation-for` tag helper reads `ErrorMessage` at **render time** and embeds it into HTML:

```html
<input asp-for="Input.Name" class="form-control" />
<span asp-validation-for="Input.Name" class="text-danger small"></span>
```

Generates:
```html
<input id="Input_Name"
       name="Input.Name"
       class="form-control"
       type="text"
       data-val="true"
       data-val-required="validation.required"
       data-val-stringlength="validation.string_length"
       data-val-stringlength-min="3"
       data-val-stringlength-max="200" />

<span class="text-danger small"
      data-valmsg-for="Input.Name"
      data-valmsg-replace="true"></span>
```

jQuery Validate (loaded by `_ValidationScriptsPartial.cshtml`) reads `data-val-*` attributes and shows errors **on blur/change**.

## Customization

### Add Custom Validation Message
No custom attribute needed — use `[CustomValidation]`:

```csharp
public class InputModel
{
    [Required(ErrorMessage = "validation.required")]
    public string Email { get; set; }

    [CustomValidation(typeof(EmailValidator), nameof(EmailValidator.ValidateUnique))]
    public string Email { get; set; } // Overkill for a starter kit!
}
```

**Better approach for a starter kit:** Validate uniqueness in `OnPostAsync`:

```csharp
[HttpPost]
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        AddErrors(ModelState);
        return Page();
    }

    // Custom validation
    if (await _repository.ExistsAsync(Input.Email, TenantId))
    {
        Errors.Add(T("validation.email_already_exists"));
        return Page();
    }

    // Save...
}
```

### Adding a New MessageKey
1. Add key to `Shared/Constants/MessageKeys.cs`:
   ```csharp
   public static class Validation
   {
       public const string Required = "validation.required";
       public const string EmailAlreadyExists = "validation.email_already_exists";
   }
   ```

2. Add translation to database `Translations` table:
   ```sql
   INSERT INTO Shared.Translations (TenantId, Key, Locale, Value)
   VALUES ('default', 'validation.email_already_exists', 'en', 'Email already registered');
   ```

3. Use in code:
   ```csharp
   Errors.Add(T(MessageKeys.Validation.EmailAlreadyExists));
   ```

## Common Mistakes

❌ **Hardcoding error messages in attributes:**
```csharp
[Required(ErrorMessage = "This field is required")]
```
✅ Use translation keys instead:
```csharp
[Required(ErrorMessage = "validation.required")]
```

❌ **Not calling `AddErrors(ModelState)` in OnPostAsync:**
```csharp
if (!ModelState.IsValid)
{
    return Page(); // Validation errors lost, UI shows nothing
}
```
✅ Always translate and add:
```csharp
if (!ModelState.IsValid)
{
    AddErrors(ModelState);
    return Page();
}
```

❌ **Forgetting to check `ModelState.IsValid` server-side:**
```csharp
// User disables JS, submits invalid form
var product = new Product { SKU = Input.SKU }; // SKU is empty!
await _repository.CreateAsync(product); // Bug: invalid data saved
```
✅ Always validate before processing:
```csharp
if (!ModelState.IsValid)
{
    AddErrors(ModelState);
    return Page();
}
```

❌ **Using custom validation attributes for everything:**
```csharp
[LocalizedRequired]
[LocalizedStringLength]
[LocalizedEmailAddress]
// Over-engineered for a starter kit
```
✅ Use built-in attributes + simple translation in OnPostAsync.

## See Also
- [Translation System](01-translation-system.md) — `T()` helper, `MessageKeys`
- [Base Page Pattern](03-base-page-pattern.md) — `AddErrors()`, `Errors` collection
- [Products Create Sample](../../src/SmartWorkz.StarterKitMVC.Admin/Pages/Products/Create.cshtml.cs) — working example
