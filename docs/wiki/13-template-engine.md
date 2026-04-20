# Template Engine Pattern

## Overview

The `ITemplateEngine` service renders templates with placeholder substitution, ideal for dynamic email/SMS content. It supports two placeholder styles:

- `{Name}` — simple variable placeholders
- `{{TranslationKey}}` — translation key placeholders (case-insensitive)

Use `ITemplateEngine` when:
- Email/SMS body has dynamic fields (user names, order numbers, links)
- Template body is file-based (not hardcoded in code)
- You need to render the same template with different data

---

## Basic Usage

### Simple Variable Replacement

```csharp
public class OrderEmailService
{
    private readonly ITemplateEngine _templateEngine;

    public async Task SendConfirmationAsync(Order order)
    {
        var template = "Thank you {CustomerName}! Your order {OrderNumber} totaling ${TotalPrice} has been confirmed.";

        var data = new
        {
            CustomerName = order.Customer.Name,
            OrderNumber = order.Number,
            TotalPrice = order.Total.ToString("F2")
        };

        var body = _templateEngine.Render(template, data);
        // Result: "Thank you John Doe! Your order ORD-12345 totaling $99.99 has been confirmed."

        await _emailService.SendAsync(order.Customer.Email, "Order Confirmed", body);
    }
}
```

### Translation Key Placeholders

```csharp
public class NotificationService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly ITranslationService _translation;

    public async Task SendWelcomeAsync(User user)
    {
        var template = "{{WELCOME_MESSAGE}} {{CLICK_HERE}}";

        var body = _templateEngine.Render(template, new Dictionary<string, string>
        {
            { "WELCOME_MESSAGE", _translation.Get("email.welcome", user.Language) },
            { "CLICK_HERE", _translation.Get("email.click_here", user.Language) }
        });

        await _emailService.SendAsync(user.Email, "Welcome", body);
    }
}
```

### File-Based Templates

```csharp
public class InvoiceService
{
    private readonly ITemplateEngine _templateEngine;

    public async Task<Result<string>> GenerateInvoiceAsync(Invoice invoice)
    {
        // Load from file: ~/Templates/Emails/invoice-body.html
        var result = await _templateEngine.RenderFileAsync(
            "~/Templates/Emails/invoice-body.html",
            new
            {
                InvoiceNumber = invoice.Number,
                CustomerName = invoice.Customer.Name,
                Total = invoice.Total,
                DueDate = invoice.DueDate.ToString("yyyy-MM-dd")
            }
        );

        if (result.IsFailure)
            return Result.Fail<string>("Failed to render invoice template");

        return Result.Ok(result.Value);
    }
}
```

---

## Registration

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<ITemplateEngine, TemplateEngine>();
```

Or use the extension method if available:

```csharp
builder.Services.AddSmartWorkzSharedServices();
```

---

## Placeholder Syntax

### Simple Placeholders: `{PropertyName}`

Matches property names from the data object (case-insensitive).

```csharp
var template = "Hello {name}, your balance is {balance}";
var result = _templateEngine.Render(template, new { Name = "Alice", Balance = "$50.00" });
// Result: "Hello Alice, your balance is $50.00"
```

### Translation Key Placeholders: `{{KEY_NAME}}`

Used for dictionary-based localization. The engine looks up the key in the dictionary.

```csharp
var template = "{{GREETING}} {{OFFER}}";
var values = new Dictionary<string, string>
{
    { "GREETING", "Welcome to our store!" },
    { "OFFER", "Use code SAVE10 for 10% off" }
};
var result = _templateEngine.Render(template, values);
// Result: "Welcome to our store! Use code SAVE10 for 10% off"
```

### Placeholder Limitations

- Only alphanumeric characters and underscores: `{Name}`, `{{USER_ID}}` ✓
- Not valid: `{user-name}`, `{obj.Property}`, `{DateTime.Now}` ✗
- For complex formatting, use the data object to pre-format values

---

## Rendering Modes

### Render String (In-Memory)

```csharp
// Synchronous - both dictionary and object
string Render(string content, IDictionary<string, string> values);
string Render(string content, object model);
```

Use for small templates or when you have the content in memory.

### Render File

```csharp
// Asynchronous - returns Result<string> with error handling
Task<Result<string>> RenderFileAsync(
    string filePath,
    IDictionary<string, string> values,
    CancellationToken ct = default);

Task<Result<string>> RenderFileAsync(
    string filePath,
    object model,
    CancellationToken ct = default);
```

Automatically handles file I/O errors, returning `Result.Fail` on missing files or read errors.

### Load Directory

```csharp
// Load all template files from a directory
Task<Result<Dictionary<string, string>>> LoadDirectoryAsync(
    string directoryPath,
    string searchPattern = "*.html",
    CancellationToken ct = default);
```

Returns a dictionary keyed by filename (without extension).

```csharp
var result = await _templateEngine.LoadDirectoryAsync("~/Templates/Emails");
// Result: {
//   "welcome": "<html>...",
//   "password-reset": "<html>...",
//   "order-confirmation": "<html>..."
// }
```

### Render Directory

```csharp
// Load and render all templates in a directory
Task<Result<Dictionary<string, string>>> RenderDirectoryAsync(
    string directoryPath,
    IDictionary<string, string> values,
    string searchPattern = "*.html",
    CancellationToken ct = default);
```

Useful for batch-generating email templates for multiple recipients.

---

## Real-World Example: Multi-Recipient Notifications

```csharp
public class BulkNotificationService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly IEmailService _emailService;

    public async Task SendMonthlyReportAsync(List<User> users)
    {
        // Load template once
        var result = await _templateEngine.RenderFileAsync(
            "~/Templates/Emails/monthly-report.html",
            new { Month = DateTime.Now.ToString("MMMM yyyy") }
        );

        if (result.IsFailure)
            throw new InvalidOperationException($"Template load failed: {result.Error.Message}");

        var baseTemplate = result.Value;

        // Render for each user
        var tasks = users.Select(user =>
        {
            var body = _templateEngine.Render(baseTemplate, new
            {
                UserName = user.Name,
                ReportUrl = $"https://example.com/reports/{user.Id}",
                CustomMetrics = CalculateMetrics(user)
            });

            return _emailService.SendAsync(user.Email, "Your Monthly Report", body);
        });

        await Task.WhenAll(tasks);
    }
}
```

---

## Integration with Email Service

Typical email flow:

1. **Load template** → `RenderFileAsync("~/Templates/...")`
2. **Render with data** → `Render(template, data)`
3. **Send email** → `IEmailService.SendAsync(email, subject, body)`

```csharp
public class OrderConfirmationService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly IEmailService _emailService;

    public async Task ConfirmOrderAsync(Order order)
    {
        // Step 1: Load template
        var templateResult = await _templateEngine.RenderFileAsync(
            "~/Templates/Emails/order-confirmed.html",
            new { CompanyName = "ACME Corp" }
        );

        if (templateResult.IsFailure)
            return; // Handle error

        // Step 2: Render with order-specific data
        var body = _templateEngine.Render(templateResult.Value, new
        {
            OrderNumber = order.Id,
            CustomerName = order.Customer.Name,
            Total = $"${order.Total:F2}",
            ShippingDate = order.ShippingDate?.ToString("MMMM dd, yyyy") ?? "TBD"
        });

        // Step 3: Send
        await _emailService.SendAsync(
            order.Customer.Email,
            "Order Confirmation",
            body
        );
    }
}
```

---

## Error Handling

File operations return `Result<T>`:

```csharp
public record Error(string Code, string Message);

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error? Error { get; }
}
```

Always check `IsFailure`:

```csharp
var result = await _templateEngine.RenderFileAsync("~/Templates/welcome.html", data);

if (result.IsFailure)
{
    _logger.Error($"Template render failed: {result.Error.Code} - {result.Error.Message}");
    // Graceful fallback: send plain-text email
    await _emailService.SendAsync(user.Email, subject, "Welcome to our service!");
    return;
}

await _emailService.SendAsync(user.Email, subject, result.Value);
```

---

## Performance Tips

1. **Cache template files**
   ```csharp
   // Load once at startup
   var templates = await _templateEngine.LoadDirectoryAsync("~/Templates/Emails");
   
   // Render many times
   foreach (var user in users)
   {
       var body = _templateEngine.Render(templates["welcome"], new { Name = user.Name });
   }
   ```

2. **Use file paths for large templates**
   - File I/O is fast for reasonable sizes (~100KB)
   - In-memory rendering better for small, dynamic templates

3. **Pre-format complex values**
   - Don't put logic in the template
   - Format dates, currency, etc. before passing to `Render()`

4. **Batch render with `RenderDirectoryAsync`**
   - More efficient than calling `RenderFileAsync` individually

---

## Testing

Unit test with mock data:

```csharp
[Fact]
public void Render_ReplaceSimplePlaceholders()
{
    // Arrange
    var engine = new TemplateEngine();
    var template = "Hello {Name}, your balance is {Balance}";

    // Act
    var result = engine.Render(template, new { Name = "Alice", Balance = "$50.00" });

    // Assert
    Assert.Equal("Hello Alice, your balance is $50.00", result);
}

[Fact]
public void Render_CaseInsensitivePropertyLookup()
{
    var engine = new TemplateEngine();
    var result = engine.Render("Hello {name}", new { Name = "Bob" });
    Assert.Equal("Hello Bob", result);
}

[Fact]
public void Render_IgnoresMissingPlaceholders()
{
    var engine = new TemplateEngine();
    var result = engine.Render("Hello {Name}, {Missing}", new { Name = "Charlie" });
    Assert.Equal("Hello Charlie, {Missing}", result);
}
```

---

## Reference

- **[Template Engine API Guide](../TEMPLATE_ENGINE_GUIDE.md)** — Full API reference
- **[Cache Attribute Pattern](12-cache-attribute.md)** — Caching template renders
- **[Email Service Guide](../SMARTWORKZ_SERVICES_COMPLETE.md)** — Sending emails
