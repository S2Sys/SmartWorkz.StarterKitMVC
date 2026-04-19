# Email Templates

Database-backed email templates with placeholder rendering, header/footer sections, JSON import/export, and a pluggable sender that hands off to the notification queue.

## Purpose

- **Single source of truth for email content:** subject, HTML body, plain-text body all stored per template ID.
- **Reusable sections:** headers + footers are separate entities — one change updates every template that includes them.
- **Placeholder substitution:** `{{Name}}` tokens replaced at render time with a `Dictionary<string, object>`.
- **Swap storage without changing callers:** `ContentTemplateRepository` (SQL) or `JsonEmailTemplateRepository` (file) behind the same `IEmailTemplateRepository` interface.
- **One call to send:** `ITemplatedEmailSender.SendTemplatedEmailAsync(templateId, recipient, data)` handles render + queue.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `IEmailTemplateService` | Template/section CRUD + rendering + import/export | [`Application/EmailTemplates/IEmailTemplateService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/EmailTemplates/IEmailTemplateService.cs) |
| `ITemplatedEmailSender` | Send-to-recipient(s) convenience over service + queue | [`Application/EmailTemplates/ITemplatedEmailSender.cs`](../../src/SmartWorkz.StarterKitMVC.Application/EmailTemplates/ITemplatedEmailSender.cs) |
| `IEmailTemplateRepository` | Storage contract | [`Application/EmailTemplates/IEmailTemplateRepository.cs`](../../src/SmartWorkz.StarterKitMVC.Application/EmailTemplates/IEmailTemplateRepository.cs) |
| `EmailTemplateService` | Rendering engine + orchestration | [`Infrastructure/EmailTemplates/EmailTemplateService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/EmailTemplates/EmailTemplateService.cs) |
| `ContentTemplateRepository` | SQL-backed store (Dapper, `Master.ContentTemplates`) | same folder |
| `JsonEmailTemplateRepository` | File-backed store (dev/tests) | same folder |
| `TemplatedEmailSender` | Composes + enqueues via `IEmailQueueRepository` | same folder |
| `DefaultEmailTemplates` | Seed data: built-in templates + header/footer sections | same folder |
| `EmailTemplateRenderResult` | Record returned by render — `Success`, `Subject`, `HtmlBody`, `PlainTextBody`, `Errors` | `Application/EmailTemplates/` |

### Flow

```
SendTemplatedEmailAsync(templateId, recipient, data)
    ↓
EmailTemplateService.RenderTemplateAsync(templateId, data)
    ↓
  Repository.GetTemplateByIdAsync → template
  Replace {{Tokens}} → Subject / HtmlBody / PlainTextBody
  Wrap HTML with header + footer sections
    ↓
EmailQueue.EnqueueAsync(recipient, rendered)
    ↓
Background worker dispatches via SMTP / SendGrid / etc.
```

## DI Registration

Wired by `AddApplicationServices` → `AddEmailTemplates`:

```csharp
// Inside AddApplicationServices in ServiceCollectionExtensions.cs
services.AddEmailTemplates(useSqlRepository: true);
```

`AddEmailTemplates` registers three services:

```csharp
// SQL (default) — Dapper-backed, reads from Master.ContentTemplates
services.AddScoped<IEmailTemplateRepository, ContentTemplateRepository>();
// …or JSON for dev
services.AddSingleton<IEmailTemplateRepository>(sp => new JsonEmailTemplateRepository(storagePath));

services.AddScoped<IEmailTemplateService, EmailTemplateService>();
services.AddScoped<ITemplatedEmailSender, TemplatedEmailSender>();
```

See [`EmailTemplateServiceExtensions.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/EmailTemplates/EmailTemplateServiceExtensions.cs).

### Seeding defaults at startup

Call once after `app.Build()` (dev only — production should seed via migration):

```csharp
await app.Services.SeedDefaultEmailTemplatesAsync();
```

This inserts every template in `DefaultEmailTemplates.AllTemplates` and every section in `DefaultEmailTemplates.AllSections` if missing.

## Quick Start

```csharp
public class AccountService
{
    private readonly ITemplatedEmailSender _mail;

    public AccountService(ITemplatedEmailSender mail) => _mail = mail;

    public Task SendWelcomeAsync(User user, CancellationToken ct) =>
        _mail.SendTemplatedEmailAsync(
            templateId: "welcome-email",
            recipient:  user.Email,
            data: new Dictionary<string, object>
            {
                ["UserName"] = user.DisplayName,
                ["LoginUrl"] = $"https://{user.TenantId}.app.example.com/login",
                ["SupportEmail"] = "support@example.com"
            },
            cancellationToken: ct);
}
```

The template at id `welcome-email` might look like:

```html
<!-- Subject -->
Welcome to {{AppName}}, {{UserName}}!

<!-- HtmlBody (wrapped by Header + Footer sections at render time) -->
<p>Hi {{UserName}},</p>
<p>Your account is ready. <a href="{{LoginUrl}}">Log in here</a>.</p>
<p>Need help? Email {{SupportEmail}}.</p>
```

Unknown tokens (tokens in the body that aren't in the data dictionary) stay as `{{Token}}` — useful to spot missing data in QA before production.

## Method Reference — `IEmailTemplateService`

### Template CRUD

```csharp
Task<IReadOnlyList<EmailTemplate>> GetAllTemplatesAsync(CancellationToken ct = default);
Task<EmailTemplate?>               GetTemplateByIdAsync(string id, CancellationToken ct = default);
Task<IReadOnlyList<EmailTemplate>> GetTemplatesByCategoryAsync(string category, CancellationToken ct = default);

Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template, CancellationToken ct = default);
Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template, CancellationToken ct = default);
Task<bool>          DeleteTemplateAsync(string id, CancellationToken ct = default);
Task<EmailTemplate> CloneTemplateAsync(string sourceId, string newId, string newName, CancellationToken ct = default);
```

```csharp
var newTemplate = new EmailTemplate
{
    Id = "password-reset",
    Name = "Password reset",
    Category = "account",
    Subject = "Reset your password",
    HtmlBody = "<p>Hi {{UserName}}, click <a href=\"{{ResetUrl}}\">here</a>.</p>",
    HeaderSectionId = "default-header",
    FooterSectionId = "default-footer"
};
await templateService.CreateTemplateAsync(newTemplate);
```

### Section CRUD (headers & footers)

```csharp
Task<IReadOnlyList<EmailTemplateSection>> GetAllSectionsAsync(SectionType? type = null, CancellationToken ct = default);
Task<EmailTemplateSection?>               GetSectionByIdAsync(string id, CancellationToken ct = default);
Task<EmailTemplateSection>                CreateSectionAsync(EmailTemplateSection section, CancellationToken ct = default);
Task<EmailTemplateSection>                UpdateSectionAsync(EmailTemplateSection section, CancellationToken ct = default);
Task<bool>                                DeleteSectionAsync(string id, CancellationToken ct = default);
```

A template references a header + footer by ID. Updating the section changes every email that uses it.

### Rendering

```csharp
Task<EmailTemplateRenderResult> RenderTemplateAsync(
    string templateId, IDictionary<string, object> data, CancellationToken ct = default);

Task<EmailTemplateRenderResult> RenderPreviewAsync(
    EmailTemplate template, CancellationToken ct = default);     // uses sample data

Task<IReadOnlyList<string>> ValidateTemplateAsync(
    EmailTemplate template, IDictionary<string, object> data, CancellationToken ct = default);
```

```csharp
var result = await templateService.RenderTemplateAsync("welcome-email", data, ct);
if (!result.Success)
    _logger.LogError("Template render failed: {Errors}", string.Join(", ", result.Errors));

// Use the rendered output
await _smtp.SendAsync(to, result.Subject, result.HtmlBody, result.PlainTextBody);
```

`EmailTemplateRenderResult`:

```csharp
public sealed record EmailTemplateRenderResult(
    string Subject,
    string HtmlBody,
    string? PlainTextBody,
    bool   Success,
    IReadOnlyList<string> Errors);

// Factories
EmailTemplateRenderResult.Ok(subject, htmlBody, plainTextBody);
EmailTemplateRenderResult.Fail("Missing {{UserName}}");
```

### Placeholders

```csharp
IReadOnlyList<TemplatePlaceholder> GetSystemPlaceholders();             // built-ins (e.g. CurrentYear, AppName)
IReadOnlyList<string>              ExtractPlaceholders(string content); // parses {{Tokens}} out of text
```

Use in the admin editor to show authors which tokens are available and which the draft is missing:

```csharp
var authored = templateService.ExtractPlaceholders(template.HtmlBody);
var missing = await templateService.ValidateTemplateAsync(template, sampleData);
```

### Import / Export

```csharp
Task<string> ExportAllAsync(CancellationToken ct = default);                    // JSON snapshot of all templates + sections
Task<int>    ImportAsync(string json, bool overwrite = false, CancellationToken ct = default);
```

```csharp
// Back up to disk
var json = await templateService.ExportAllAsync();
await File.WriteAllTextAsync("templates-backup.json", json);

// Restore
var imported = await templateService.ImportAsync(json, overwrite: true);
_logger.LogInformation("Imported {Count} templates", imported);
```

## Method Reference — `ITemplatedEmailSender`

```csharp
Task<bool> SendTemplatedEmailAsync(
    string templateId, string recipient,
    IDictionary<string, object> data, CancellationToken ct = default);

Task<int> SendTemplatedEmailAsync(
    string templateId, IEnumerable<string> recipients,
    IDictionary<string, object> data, CancellationToken ct = default);     // same data for all

Task<int> SendPersonalizedEmailsAsync(
    string templateId,
    IDictionary<string, IDictionary<string, object>> recipientData,
    CancellationToken ct = default);                                       // per-recipient data
```

```csharp
// Single
await mail.SendTemplatedEmailAsync("welcome-email", "alice@acme.test", data);

// Broadcast (identical data)
var sent = await mail.SendTemplatedEmailAsync("monthly-digest",
    new[] { "a@x.test", "b@x.test" }, data);

// Personalized
var perUser = new Dictionary<string, IDictionary<string, object>>
{
    ["alice@acme.test"] = new Dictionary<string, object> { ["Name"] = "Alice" },
    ["bob@acme.test"]   = new Dictionary<string, object> { ["Name"] = "Bob"   }
};
await mail.SendPersonalizedEmailsAsync("welcome-email", perUser);
```

Each call pushes to the `EmailQueue` table. The background email dispatcher (SMTP / SendGrid / etc., configured under `Features:Notifications:Email`) owns actual delivery and retries.

## Storage Swap — SQL vs JSON

### SQL Server (recommended; default)

```csharp
services.AddEmailTemplates(useSqlRepository: true);
```

Templates live in `Master.ContentTemplates` and `Master.ContentTemplateSections`. Migrations create these during schema setup. Fully tenant-aware via the standard `TenantId` column.

### JSON file (dev / offline)

```csharp
services.AddEmailTemplates(
    useSqlRepository: false,
    storagePath: Path.Combine(AppContext.BaseDirectory, "email-templates.json"));
```

Single file, singleton lifetime, good for local dev without a DB. Not tenant-aware — one file for the whole app.

### Other backends (Azure Blob, S3, etc.)

Implement `IEmailTemplateRepository` (9 methods covering template + section CRUD + list + get-by-category). Register it yourself **before** calling `AddEmailTemplates` — or skip the helper entirely and register the service + sender manually:

```csharp
services.AddSingleton<IEmailTemplateRepository, MyBlobTemplateRepository>();
services.AddScoped<IEmailTemplateService, EmailTemplateService>();
services.AddScoped<ITemplatedEmailSender, TemplatedEmailSender>();
```

## Samples from the Codebase

- **Seed data:** [`DefaultEmailTemplates.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/EmailTemplates/DefaultEmailTemplates.cs) — shows the shape of `EmailTemplate` + `EmailTemplateSection`.
- **SQL repository:** [`ContentTemplateRepository.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/EmailTemplates/ContentTemplateRepository.cs) — Dapper + SPs.
- **Sender wiring:** [`TemplatedEmailSender.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/EmailTemplates/TemplatedEmailSender.cs) — how render result becomes a queue row.
- **Password reset flow:** uses the sender directly; see [01 — Password Reset Flow](./01-password-reset-flow.md).

## Common Mistakes

- **Forgetting to seed defaults** in a fresh DB — `GetTemplateByIdAsync("welcome-email")` returns null, `SendTemplatedEmailAsync` fails. Run `SeedDefaultEmailTemplatesAsync` or include seeds in migrations.
- **Using the JSON repo in production** — it's singleton, not tenant-scoped, and not safe under concurrent writes.
- **Mixing `{{ Token }}` and `{{Token}}` spacing** — the current replacer matches the exact token text; whitespace variations are NOT normalized.
- **Leaving default-header / default-footer orphaned** — deleting a section that's still referenced by templates will leave them rendering with no wrapper. Either enforce FK cascade or check `ValidateTemplateAsync`.
- **Calling the sender synchronously from hot code paths** — it still hits the queue table. For tight loops, batch via `SendPersonalizedEmailsAsync` or buffer the calls yourself.
- **Trying to embed Razor / partial views** — the engine is a simple `{{Token}}` replacer, not a Razor runtime. For HTML/CSS-heavy templates, compose sections (header/body/footer) instead.

## See Also

- [00 — Getting Started](./00-getting-started.md) — `AddEmailTemplates` fires inside `AddApplicationStack`
- [01 — Password Reset Flow](./01-password-reset-flow.md) — real consumer of `ITemplatedEmailSender`
- [04 — Result Pattern](./04-result-pattern.md) — returning send failures as `Result.Failure(MessageKeys.EmailQueue.*)`
