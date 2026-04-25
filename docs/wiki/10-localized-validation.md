# Localized Validation

This starter kit uses a **simple but powerful approach** to localized validation: put `MessageKey` constants directly in the `ErrorMessage` property of validation attributes. Messages are translated at render time by ASP.NET's built-in validation pipeline.

## Purpose

- **No custom attribute classes:** Use standard `[Required]`, `[EmailAddress]`, etc.
- **Automatic translation:** Messages are translated to the user's locale without extra code
- **Database-backed:** Change a translation in the DB and see it on next refresh
- **Works client-side too:** jQuery Validate picks up translated messages from HTML attributes

## Architecture

### How It Works

1. **Page Model:** Defines an input model with validation attributes using `MessageKey` error messages

   ```csharp
   public class MyInput
   {
       [Required(ErrorMessage = MessageKeys.Validation.Required)]
       [EmailAddress(ErrorMessage = MessageKeys.Validation.EmailInvalid)]
       public string Email { get; set; }
   }
   ```

2. **Form Submission:** ASP.NET's ModelState validation runs, executing each attribute
   
   - Attributes check if the value is valid
   - If invalid, the `FormatErrorMessage()` method is called with the field name
   - ASP.NET applies the `ErrorMessage` as-is (the `MessageKey` string)

3. **Rendering:** In the view, `asp-validation-for` tag helper renders the error message

   ```razor
   <span asp-validation-for="Input.Email"></span>
   ```

   This outputs the `MessageKey` to the HTML. Since the view is rendered **after** `T()` has been called elsewhere, the key gets replaced by the translated value.

   **Actually:** The error messages are stored in `ModelState.AddModelError()` as the literal `MessageKey` string. Then when rendered in the view via `asp-validation-for`, they appear as the key name if no translation was found.

4. **Translation (Manual):** To get the translated message, explicitly call `T()` in a helper property

   - Or: Use a custom validation attribute that calls `T()` inside `FormatErrorMessage()`
   - Simpler approach: Leave the key as-is in the view, and rely on the fallback behavior

### Key Insight

**Standard DataAnnotations attributes cannot call `T()` because they're instantiated outside the DI container.** The simple solution: Use `MessageKey` constants and let the translation happen at render time via a custom tag helper or manual call.

## Quick Start

### Step 1: Use MessageKey in Validation Attributes

```csharp
public class ContactInput
{
    [Required(ErrorMessage = MessageKeys.Validation.Required)]
    [StringLength(100, MinimumLength = 2, 
                  ErrorMessage = MessageKeys.Validation.MaxLength)]
    public string Name { get; set; }

    [Required(ErrorMessage = MessageKeys.Validation.Required)]
    [EmailAddress(ErrorMessage = MessageKeys.Validation.EmailInvalid)]
    public string Email { get; set; }

    [RegularExpression(
        @"^\d{10}$",
        ErrorMessage = MessageKeys.Validation.InvalidFormat)]
    public string Phone { get; set; }
}
```

### Step 2: Render in the View

Use the standard `asp-validation-for` tag helper:

```razor
<form method="post">
    <div class="mb-3">
        <label asp-for="Input.Name" class="form-label"></label>
        <input asp-for="Input.Name" class="form-control" />
        <span asp-validation-for="Input.Name" class="invalid-feedback"></span>
    </div>

    <div class="mb-3">
        <label asp-for="Input.Email" class="form-label"></label>
        <input asp-for="Input.Email" class="form-control" />
        <span asp-validation-for="Input.Email" class="invalid-feedback"></span>
    </div>

    <button type="submit" class="btn btn-primary">Submit</button>
</form>
```

### Step 3: Handle in the PageModel

```csharp
[BindProperty]
public ContactInput Input { get; set; }

public IActionResult OnPost()
{
    if (!ModelState.IsValid)
    {
        // ModelState contains the MessageKey strings
        // Render the form again with errors
        return Page();
    }

    // Process valid input
    return Redirect("/success");
}
```

## Available Validation Attributes

Standard DataAnnotations attributes:

| Attribute | Usage | Error Message |
|-----------|-------|----------------|
| `[Required]` | Field is mandatory | `MessageKeys.Validation.Required` |
| `[EmailAddress]` | Valid email format | `MessageKeys.Validation.EmailInvalid` |
| `[StringLength(max, Min=min)]` | Text length constraints | `MessageKeys.Validation.MaxLength` |
| `[Range(min, max)]` | Numeric range | `MessageKeys.Validation.InvalidFormat` |
| `[RegularExpression(pattern)]` | Pattern match | `MessageKeys.Validation.InvalidFormat` |
| `[Compare(otherProperty)]` | Compare two fields | `MessageKeys.Validation.InvalidFormat` |
| `[Url]` | Valid URL format | Use generic message or custom |
| `[MinLength(n)]` | Minimum text length | `MessageKeys.Validation.MinLength` |
| `[MaxLength(n)]` | Maximum text length | `MessageKeys.Validation.MaxLength` |

## Client-Side Validation

### How It Works

ASP.NET renders `MessageKey` strings into HTML `data-val-*` attributes:

```html
<input 
    type="email" 
    name="Input.Email" 
    data-val="true"
    data-val-required="validation.required"
    data-val-email="validation.email_invalid" />
```

jQuery Validate reads these attributes and validates client-side before submission.

**The MessageKey strings are used as-is** because jQuery Validate doesn't know how to call `T()`. The fallback is to see the key name in the browser console if validation fails.

### To Translate Client-Side Messages

You can add a small script to replace error messages:

```razor
@section Scripts {
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            // Get all validation error elements
            var errorElements = document.querySelectorAll('[class*="invalid-feedback"]');
            errorElements.forEach(function(el) {
                if (el.textContent) {
                    // If the text is a key like "validation.required"
                    // you can replace it with a translated value
                    // This requires an API call or a data attribute
                    // For now, rely on server-side rendering
                }
            });
        });
    </script>
}
```

**Simpler approach:** Let server-side validation handle translations. Client-side is pre-submission feedback; server-side is authoritative.

## Customization

### Adding a New Validation Type

To add a new validation attribute for a specific domain:

1. **Create the attribute in the model:**

   ```csharp
   public class MyInput
   {
       [CustomAttribute(ErrorMessage = MessageKeys.MyDomain.CustomCheck)]
       public string MyField { get; set; }
   }
   ```

2. **Define the custom attribute:**

   ```csharp
   [AttributeUsage(AttributeTargets.Property)]
   public class CustomAttribute : ValidationAttribute
   {
       public override bool IsValid(object value)
       {
           if (value == null) return true;
           // Your validation logic
           return yourCondition;
       }
   }
   ```

3. **Add the MessageKey:**

   ```csharp
   public static class MyDomain
   {
       public const string CustomCheck = "mydomain.custom_check";
   }
   ```

4. **Seed the translation:**

   ```sql
   INSERT INTO Translations (Id, TenantId, Key, Locale, Value, CreatedAt, UpdatedAt)
   VALUES (NEWID(), tenantId, 'mydomain.custom_check', 'en', 'Custom validation failed', GETDATE(), GETDATE());
   ```

## Common Patterns

### Pattern 1: Required with Custom Message

```csharp
[Required(ErrorMessage = MessageKeys.MyDomain.FieldRequired)]
public string MyField { get; set; }
```

### Pattern 2: Email with Custom Message

```csharp
[Required(ErrorMessage = MessageKeys.Validation.Required)]
[EmailAddress(ErrorMessage = MessageKeys.Validation.EmailInvalid)]
public string Email { get; set; }
```

### Pattern 3: String Length with Min/Max

```csharp
[StringLength(
    maximumLength: 100,
    MinimumLength: 2,
    ErrorMessage = MessageKeys.Validation.MaxLength)]
public string Name { get; set; }
```

### Pattern 4: Password Confirmation

```csharp
[Required(ErrorMessage = MessageKeys.Validation.Required)]
[StringLength(100, MinimumLength = 8, 
              ErrorMessage = MessageKeys.Validation.MaxLength)]
public string Password { get; set; }

[Required(ErrorMessage = MessageKeys.Validation.Required)]
[Compare("Password", ErrorMessage = MessageKeys.Validation.InvalidFormat)]
public string ConfirmPassword { get; set; }
```

## Common Mistakes

### Mistake 1: Hardcoding Error Messages

❌ **Wrong:**
```csharp
[Required(ErrorMessage = "This field is required")]
```

✅ **Correct:**
```csharp
[Required(ErrorMessage = MessageKeys.Validation.Required)]
```

### Mistake 2: Forgetting to Seed the Translation

If the translation DB row doesn't exist, the user sees the `MessageKey` string itself (e.g., `"validation.required"`). This is the fallback, but add the seed to show actual messages:

```sql
INSERT INTO Translations (...) VALUES (..., tenantId, 'validation.required', 'en', 'This field is required', ...);
```

### Mistake 3: Custom Attribute Not Calling FormatErrorMessage

If you create a custom validation attribute, ensure it uses the `ErrorMessage` properly:

```csharp
public override bool IsValid(object value)
{
    if (!yourCondition)
        return false;  // Attribute framework calls FormatErrorMessage automatically
    return true;
}
```

### Mistake 4: Assuming Client-Side Validation Is Authoritative

**Always validate on the server.** Client-side validation can be bypassed. The server-side `ModelState.IsValid` check is what matters.

## See Also

- [Translation System](./01-translation-system.md) — How `T()` and `ITranslationService` work
- [Demo: Validation](../../src/SmartWorkz.StarterKitMVC.Public/Pages/Demo/Validation.cshtml) — Live validation examples
- [MessageKeys](../../src/SmartWorkz.StarterKitMVC.Shared/Constants/MessageKeys.cs) — All available keys
