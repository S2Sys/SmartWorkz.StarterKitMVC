# Template Engine Guide

## Overview

The `ITemplateEngine` service provides file and string-based template rendering with placeholder substitution for dynamic content generation. It's optimized for email/SMS bodies, notifications, and any text that needs variable insertion.

**Key features:**
- Dual placeholder syntax: `{Name}` (simple) and `{{KEY}}` (translation keys)
- File-based templates with I/O error handling (`Result<T>`)
- Batch directory rendering for bulk operations
- Case-insensitive placeholder matching
- Parallel file I/O for performance

**Use cases:**
- Email confirmation bodies with customer data
- SMS notifications with localized text
- Dynamic report templates
- Multi-language notification templates

---

## API Reference

### ITemplateEngine Interface

```csharp
namespace SmartWorkz.Core.Shared.Templates;

public interface ITemplateEngine
{
    /// <summary>Render template string with variable replacement.</summary>
    string Render(string content, IDictionary<string, string> values);

    /// <summary>Render template string with object property substitution.</summary>
    string Render(string content, object model);

    /// <summary>Load template file and render with dictionary values.</summary>
    Task<Result<string>> RenderFileAsync(
        string filePath,
        IDictionary<string, string> values,
        CancellationToken ct = default);

    /// <summary>Load template file and render with object properties.</summary>
    Task<Result<string>> RenderFileAsync(
        string filePath,
        object model,
        CancellationToken ct = default);

    /// <summary>Load all template files from a directory.</summary>
    Task<Result<Dictionary<string, string>>> LoadDirectoryAsync(
        string directoryPath,
        string searchPattern = "*.html",
        CancellationToken ct = default);

    /// <summary>Load and render all templates in a directory with provided values.</summary>
    Task<Result<Dictionary<string, string>>> RenderDirectoryAsync(
        string directoryPath,
        IDictionary<string, string> values,
        string searchPattern = "*.html",
        CancellationToken ct = default);
}
```

---

## Methods

### Render (Synchronous)

#### Render with Dictionary

```csharp
public string Render(string content, IDictionary<string, string> values)
```

**Purpose:** Replace placeholders in content using a dictionary of key-value pairs.

**Parameters:**
- `content` — Template string with `{key}` or `{{key}}` placeholders
- `values` — Dictionary of placeholder names to replacement values

**Returns:** Rendered string with all placeholders replaced

**Example:**
```csharp
var engine = new TemplateEngine();
var template = "Hello {Name}, your balance is {Balance}";
var values = new Dictionary<string, string>
{
    { "Name", "Alice" },
    { "Balance", "$50.00" }
};
var result = engine.Render(template, values);
// Result: "Hello Alice, your balance is $50.00"
```

#### Render with Object

```csharp
public string Render(string content, object model)
```

**Purpose:** Replace placeholders using public properties from an object.

**Parameters:**
- `content` — Template string
- `model` — Object whose public properties will be used for placeholders

**Returns:** Rendered string

**Behavior:**
- Properties are matched case-insensitively to placeholders
- Property values are converted to strings using `ToString()`
- If a placeholder doesn't match any property, the placeholder remains unchanged

**Example:**
```csharp
var engine = new TemplateEngine();
var template = "Order {OrderNumber} for {CustomerName} totaling ${Total}";
var order = new { OrderNumber = "ORD-12345", CustomerName = "Bob Smith", Total = "99.99" };
var result = engine.Render(template, order);
// Result: "Order ORD-12345 for Bob Smith totaling $99.99"
```

---

### RenderFileAsync (Asynchronous File-Based)

#### RenderFileAsync with Dictionary

```csharp
public Task<Result<string>> RenderFileAsync(
    string filePath,
    IDictionary<string, string> values,
    CancellationToken ct = default)
```

**Purpose:** Load a template file and render it with dictionary values.

**Parameters:**
- `filePath` — Path to template file (supports `~` for app root)
- `values` — Dictionary of placeholder replacements
- `ct` — Cancellation token

**Returns:** `Result<string>` — Success with rendered content, or Failure with error details

**Error Handling:**
- File not found → `Result.Fail` with "FILE_NOT_FOUND"
- Permission denied → `Result.Fail` with "IO_ERROR"
- Cancelled via token → `Result.Fail` with "OPERATION_CANCELLED"

**Example:**
```csharp
var engine = new TemplateEngine();
var result = await engine.RenderFileAsync(
    "~/Templates/Emails/welcome.html",
    new Dictionary<string, string>
    {
        { "UserName", "Charlie" },
        { "ActivationLink", "https://example.com/activate?token=xyz" }
    }
);

if (result.IsFailure)
{
    _logger.Error($"Template error: {result.Error.Message}");
    return;
}

var emailBody = result.Value;
```

#### RenderFileAsync with Object

```csharp
public Task<Result<string>> RenderFileAsync(
    string filePath,
    object model,
    CancellationToken ct = default)
```

**Purpose:** Load a template file and render it using object properties.

**Parameters:**
- `filePath` — Template file path
- `model` — Object with properties matching placeholders
- `ct` — Cancellation token

**Returns:** `Result<string>`

**Example:**
```csharp
var result = await engine.RenderFileAsync(
    "~/Templates/Emails/invoice.html",
    new
    {
        InvoiceNumber = invoice.Id,
        CustomerName = invoice.Customer.Name,
        Total = $"${invoice.Total:F2}",
        DueDate = invoice.DueDate.ToString("yyyy-MM-dd")
    }
);
```

---

### LoadDirectoryAsync

```csharp
public Task<Result<Dictionary<string, string>>> LoadDirectoryAsync(
    string directoryPath,
    string searchPattern = "*.html",
    CancellationToken ct = default)
```

**Purpose:** Load all template files from a directory into memory.

**Parameters:**
- `directoryPath` — Directory path (supports `~`)
- `searchPattern` — File filter (default: `*.html`)
- `ct` — Cancellation token

**Returns:** `Result<Dictionary<string, string>>` — Dictionary keyed by filename without extension

**Behavior:**
- Reads all matching files in parallel for performance
- Dictionary keys are lowercase filenames (no extension): `"welcome"`, `"password-reset"`
- Directory loading fails if directory doesn't exist or isn't readable

**Example:**
```csharp
var result = await engine.LoadDirectoryAsync("~/Templates/Emails");

if (result.IsFailure)
{
    _logger.Error($"Failed to load templates: {result.Error.Message}");
    return;
}

var templates = result.Value;
// templates["welcome"] = file content
// templates["order-confirmation"] = file content
```

---

### RenderDirectoryAsync

```csharp
public Task<Result<Dictionary<string, string>>> RenderDirectoryAsync(
    string directoryPath,
    IDictionary<string, string> values,
    string searchPattern = "*.html",
    CancellationToken ct = default)
```

**Purpose:** Load all templates from a directory and render them with provided values.

**Parameters:**
- `directoryPath` — Template directory
- `values` — Dictionary of placeholder replacements (applied to all templates)
- `searchPattern` — File filter
- `ct` — Cancellation token

**Returns:** `Result<Dictionary<string, string>>` — Rendered templates keyed by filename

**Behavior:**
- Loads all files in parallel, then renders each
- All templates receive the same `values` dictionary
- If any file fails to load, the entire operation fails

**Example:**
```csharp
var result = await engine.RenderDirectoryAsync(
    "~/Templates/Emails",
    new Dictionary<string, string>
    {
        { "CompanyName", "ACME Corp" },
        { "SupportEmail", "support@acme.com" },
        { "CurrentYear", DateTime.Now.Year.ToString() }
    }
);

if (result.IsSuccess)
{
    var rendered = result.Value;
    // rendered["welcome"] = "Welcome to ACME Corp..."
    // rendered["password-reset"] = "Reset your password at ACME Corp..."
}
```

---

## Placeholder Syntax

### Simple Placeholders: `{PropertyName}`

Matches object property names (case-insensitive).

**Rules:**
- Must be alphanumeric + underscores: `{Name}`, `{OrderID}`, `{user_email}` ✓
- Invalid: `{user-name}`, `{obj.property}`, `{DateTime.Now}` ✗
- Missing placeholders remain unchanged: `"Hello {Missing}"` → `"Hello {Missing}"`

**Examples:**
```csharp
var template = "Hello {Name}! You have {UnreadCount} messages.";
var data = new { Name = "Diana", UnreadCount = "3" };
var result = engine.Render(template, data);
// "Hello Diana! You have 3 messages."
```

### Translation Key Placeholders: `{{KEY_NAME}}`

Used for dictionary-based localization and static text.

**Rules:**
- Same character rules as simple placeholders
- Typically uppercase: `{{WELCOME_MESSAGE}}`, `{{CLICK_HERE}}`
- Used with dictionary values: `{ "WELCOME_MESSAGE", "Welcome!" }`

**Example:**
```csharp
var template = "{{GREETING}} Please visit {{STORE_URL}}";
var values = new Dictionary<string, string>
{
    { "GREETING", "Hello" },
    { "STORE_URL", "https://shop.example.com" }
};
var result = engine.Render(template, values);
// "Hello Please visit https://shop.example.com"
```

### Priority

Both syntaxes can be used in the same template:

```csharp
var template = "{{GREETING}} {UserName}! {{OFFER_TEXT}} {OfferCode}";
var data = new { UserName = "Eve", OfferCode = "SAVE10" };
var values = new Dictionary<string, string>
{
    { "GREETING", "Welcome back" },
    { "OFFER_TEXT", "Use code" }
};

// Merge data and values for rendering
var merged = new Dictionary<string, string>(values)
{
    { "UserName", "Eve" },
    { "OfferCode", "SAVE10" }
};
var result = engine.Render(template, merged);
// "Welcome back Eve! Use code SAVE10"
```

---

## Registration & Dependency Injection

### Manual Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITemplateEngine, TemplateEngine>();
```

### Extension Method (if available)

```csharp
builder.Services.AddSmartWorkzSharedServices();
```

---

## Real-World Examples

### Email Confirmation

```csharp
public class EmailConfirmationService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly IEmailService _emailService;

    public async Task SendConfirmationAsync(User user, string confirmationLink)
    {
        var result = await _templateEngine.RenderFileAsync(
            "~/Templates/Emails/email-confirmation.html",
            new
            {
                UserName = user.FullName,
                ConfirmationLink = confirmationLink,
                ExpiryHours = "24"
            }
        );

        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to load email template: {result.Error.Message}");

        await _emailService.SendAsync(
            user.Email,
            "Confirm Your Email",
            result.Value
        );
    }
}
```

### Localized Notifications

```csharp
public class LocalizedNotificationService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly ITranslationService _translation;
    private readonly IEmailService _emailService;

    public async Task SendWelcomeAsync(User user, string language)
    {
        var template = @"{{GREETING}}
{{INTRO_TEXT}}

{{FEATURES_HEADER}}
- {{FEATURE_1}}
- {{FEATURE_2}}

{{FOOTER_TEXT}}";

        var i18n = await _translation.GetLocalizationAsync(language);
        var body = _templateEngine.Render(template, new Dictionary<string, string>
        {
            { "GREETING", i18n["email.greeting"] }, // "Welcome!"
            { "INTRO_TEXT", i18n["email.intro"] },  // "We're excited..."
            { "FEATURES_HEADER", i18n["email.features"] }, // "Features"
            { "FEATURE_1", i18n["email.feature1"] },
            { "FEATURE_2", i18n["email.feature2"] },
            { "FOOTER_TEXT", i18n["email.footer"] }
        });

        await _emailService.SendAsync(user.Email, i18n["email.subject"], body);
    }
}
```

### Bulk Notifications

```csharp
public class BulkEmailService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly IEmailService _emailService;

    public async Task SendMonthlyReportAsync(List<User> users, string month)
    {
        // Load template once
        var templateResult = await _templateEngine.RenderFileAsync(
            "~/Templates/Emails/monthly-report.html",
            new { Month = month }
        );

        if (templateResult.IsFailure)
            throw new InvalidOperationException("Failed to load template");

        var baseTemplate = templateResult.Value;

        // Render for each user (parallel)
        var tasks = users.Select(user =>
        {
            var personalizedBody = _templateEngine.Render(baseTemplate, new
            {
                UserName = user.Name,
                ReportUrl = $"https://example.com/reports/{user.Id}",
                TotalTransactions = user.Transactions.Count
            });

            return _emailService.SendAsync(
                user.Email,
                $"Your {month} Report",
                personalizedBody
            );
        });

        await Task.WhenAll(tasks);
    }
}
```

---

## Error Handling

All async methods return `Result<T>` for safe error handling:

```csharp
public record Error(string Code, string Message);

var result = await engine.RenderFileAsync("~/Templates/email.html", data);

if (result.IsFailure)
{
    switch (result.Error.Code)
    {
        case "FILE_NOT_FOUND":
            _logger.Error($"Template not found: {result.Error.Message}");
            // Fallback: send plain-text email
            break;
        case "IO_ERROR":
            _logger.Error($"I/O error reading template: {result.Error.Message}");
            // Retry or notify admin
            break;
        case "OPERATION_CANCELLED":
            _logger.Warn("Template operation cancelled");
            break;
        default:
            _logger.Error($"Unexpected error: {result.Error.Message}");
            break;
    }

    // Don't proceed with email sending
    return;
}

// Safe to use result.Value
var body = result.Value;
```

---

## Performance Considerations

### File Path Resolution
Paths support `~` for application root:
- `~/Templates/Emails/welcome.html` → resolves to app root
- `/absolute/path/template.html` → uses absolute path as-is
- `relative/path.html` → uses relative to app root

### Parallel I/O
`LoadDirectoryAsync` and `RenderDirectoryAsync` read files in parallel for performance:

```csharp
// All files loaded concurrently
var result = await engine.LoadDirectoryAsync("~/Templates/Emails");
```

### Caching Strategy
For frequently-used templates, cache the loaded content:

```csharp
// Load once at startup
var templatesResult = await engine.LoadDirectoryAsync("~/Templates/Emails");
var templates = templatesResult.Value;

// Use many times
foreach (var user in users)
{
    var body = engine.Render(templates["welcome"], new { Name = user.Name });
}
```

### Regex Compilation
Regex pattern is compiled once using `[GeneratedRegex]` — no per-call overhead.

---

## Testing

Unit test examples:

```csharp
[TestFixture]
public class TemplateEngineTests
{
    private ITemplateEngine _engine;

    [SetUp]
    public void Setup()
    {
        _engine = new TemplateEngine();
    }

    [Test]
    public void Render_ReplaceSimplePlaceholders()
    {
        var result = _engine.Render(
            "Hello {Name}!",
            new { Name = "Alice" }
        );
        Assert.AreEqual("Hello Alice!", result);
    }

    [Test]
    public void Render_CaseInsensitiveProperties()
    {
        var result = _engine.Render(
            "Hello {name}!",
            new { Name = "Bob" }
        );
        Assert.AreEqual("Hello Bob!", result);
    }

    [Test]
    public void Render_TranslationKeyPlaceholders()
    {
        var result = _engine.Render(
            "{{GREETING}} {{OFFER}}",
            new Dictionary<string, string>
            {
                { "GREETING", "Welcome" },
                { "OFFER", "Save 10%" }
            }
        );
        Assert.AreEqual("Welcome Save 10%", result);
    }

    [Test]
    public void Render_MissingPlaceholdersRemainUnchanged()
    {
        var result = _engine.Render(
            "Hello {Name}, {Missing}",
            new { Name = "Charlie" }
        );
        Assert.AreEqual("Hello Charlie, {Missing}", result);
    }

    [Test]
    public async Task RenderFileAsync_FailureOnMissingFile()
    {
        var result = await _engine.RenderFileAsync(
            "~/NonExistent/template.html",
            new Dictionary<string, string>()
        );
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual("FILE_NOT_FOUND", result.Error.Code);
    }
}
```

---

## Related Guides

- **[Utilities & Extensions Guide](UTILITIES_EXTENSIONS_GUIDE.md)** — Other string/text utilities
- **[Wiki: Template Engine Pattern](wiki/13-template-engine.md)** — Real-world usage patterns
- **[Cache Attribute Guide](wiki/12-cache-attribute.md)** — Caching template renders with `[Cache]`
- **[SmartWorkz Services Complete](SMARTWORKZ_SERVICES_COMPLETE.md)** — Email/SMS service integration
