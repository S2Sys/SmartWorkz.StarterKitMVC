# Translation System

This starter kit uses a **database-backed translation system** for user-facing messages, supporting multi-language and per-tenant overrides without requiring .resx files.

## Purpose

- **Single source of truth:** All translatable strings live in the database, not scattered across .resx files
- **Runtime updates:** Change a translation and see it immediately on refresh (with caching)
- **Per-tenant support:** Different tenants can override translations without code changes
- **Multi-language:** Support any number of locales via the `locale` user claim

## Architecture

### Core Components

| Component | Role |
|-----------|------|
| `MessageKeys.cs` (Shared) | Constants for all translatable keys (e.g., `MessageKeys.Validation.Required`) |
| `ITranslationService` (Application) | Core service to fetch translated strings from DB |
| `TranslationRepository` (Persistence) | Query translations table |
| `Translations` table (DB) | Stores all translation strings per tenant/locale |
| `T()` method (BasePage) | Helper available on all pages to translate a key |

### Data Flow

```
Code calls T(MessageKeys.Validation.Required)
    ↓
BasePage.T() calls ITranslationService.Get(key, tenantId, locale)
    ↓
ITranslationService checks 60-minute cache
    ↓
Cache miss → Query DB: SELECT Value FROM Translations WHERE TenantId=X AND Key=Y AND Locale=Z
    ↓
Cache hit/miss → Return translated string or fallback to key name
```

## Quick Start

### Displaying a Translated Message

In any Razor page (inheriting `BasePage`):

```razor
<h1>@Model.T(MessageKeys.General.Save)</h1>
```

Or in C# code (PageModel inheriting `BasePage`):

```csharp
public class MyPageModel : BasePage
{
    public string Message => T(MessageKeys.Auth.LoginSuccess);
}
```

### Using in Validation

All validation attributes use `MessageKey` constants in `ErrorMessage`:

```csharp
public class RegisterInput
{
    [Required(ErrorMessage = MessageKeys.Validation.Required)]
    [EmailAddress(ErrorMessage = MessageKeys.Validation.EmailInvalid)]
    public string Email { get; set; }
}
```

The message is automatically translated at render time. See [Localized Validation](./02-localized-validation.md).

### Viewing All Available Keys

Visit `/Demo/Translations` (no auth required) to see all `MessageKeys` constants and their current translated values.

## How It Works

### The T() Method (BasePage)

Located in [BasePage.cs](../../src/SmartWorkz.StarterKitMVC.Public/Pages/BasePage.cs), `T()` is a simple wrapper:

```csharp
protected string T(string key)
{
    var locale = User.FindFirst("locale")?.Value ?? "en";
    return _translationService.Get(key, TenantId, locale);
}
```

**Key points:**
- Reads tenant ID from the DI-injected `TenantContext`
- Reads locale from the user claim `"locale"` (defaults to `"en"`)
- Returns the translated string or the key name itself if not found

### ITranslationService.Get()

```csharp
public string Get(string key, Guid tenantId, string locale)
{
    var cacheKey = $"translation:{tenantId}:{locale}:{key}";
    if (_cache.TryGetValue(cacheKey, out var cached))
        return (string)cached;

    var translation = _repository.GetByKey(tenantId, key, locale);
    var value = translation?.Value ?? key;  // Fallback to key name
    
    _cache.Set(cacheKey, value, TimeSpan.FromMinutes(60));
    return value;
}
```

**Behavior:**
- Checks a 60-minute in-memory cache first
- Falls back to DB query if not cached
- Returns the translation value or the key name as fallback
- Stores result in cache for next 60 minutes

### Database Schema

```sql
CREATE TABLE Translations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Key NVARCHAR(255) NOT NULL,
    Locale NVARCHAR(10) NOT NULL,  -- e.g., 'en', 'es', 'fr'
    Value NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    UNIQUE(TenantId, Key, Locale)
);
```

## Adding a New Translation Key

### Step 1: Define the Key

Add a new constant to [MessageKeys.cs](../../src/SmartWorkz.StarterKitMVC.Shared/Constants/MessageKeys.cs):

```csharp
public static class MyFeature
{
    public const string ActionSuccess = "myfeature.action_success";
    public const string ActionFailed  = "myfeature.action_failed";
}
```

### Step 2: Seed the Translation

In your database seed file (e.g., `database/v1/XXX_SeedTranslations.sql`):

```sql
INSERT INTO Translations (Id, TenantId, Key, Locale, Value, CreatedAt, UpdatedAt)
VALUES
    (NEWID(), (SELECT Id FROM Tenants WHERE Code = 'DEFAULT'), 
     'myfeature.action_success', 'en', 'Action completed successfully', GETDATE(), GETDATE()),
    (NEWID(), (SELECT Id FROM Tenants WHERE Code = 'DEFAULT'), 
     'myfeature.action_failed', 'en', 'Action failed', GETDATE(), GETDATE());
```

### Step 3: Use the Key

```csharp
// In code
var message = T(MessageKeys.MyFeature.ActionSuccess);

// In validation attributes
[Required(ErrorMessage = MessageKeys.MyFeature.ActionSuccess)]

// In Razor pages
<span>@Model.T(MessageKeys.MyFeature.ActionSuccess)</span>
```

## Caching

### TTL: 60 Minutes

Translations are cached for 60 minutes. To refresh immediately in development, restart the app or manually clear the cache.

### Cache Key Format

```
translation:{tenantId}:{locale}:{key}
```

Example: `translation:12345678-1234-1234-1234-123456789012:en:validation.required`

## Fallback Behavior

If a translation is **not found in the database**:

1. `T()` returns the **key name itself** (e.g., `"validation.required"`)
2. This allows pages to work even if the seed hasn't run yet
3. Add the translation to the DB when you're ready

**Example:**

```razor
<!-- If translation not found, shows: "validation.required" -->
<p>@Model.T("validation.required")</p>
```

## Multi-Locale Support

### How Locales are Determined

User's locale is read from the `"locale"` claim (usually set during login):

```csharp
var locale = User.FindFirst("locale")?.Value ?? "en";
```

### Adding a New Locale

1. **Seed translations for the new locale:**
   
   ```sql
   INSERT INTO Translations (Id, TenantId, Key, Locale, Value, CreatedAt, UpdatedAt)
   VALUES
       (NEWID(), tenantId, 'validation.required', 'es', 'Requerido', GETDATE(), GETDATE()),
       (NEWID(), tenantId, 'validation.required', 'fr', 'Requis', GETDATE(), GETDATE());
   ```

2. **Update user claim during login:**
   
   ```csharp
   claims.Add(new Claim("locale", userLocaleFromDB));  // e.g., "es", "fr", "en"
   ```

3. **Users with that locale will see the new locale's translations**

## Customization

### Override Translations per Tenant

Tenants can have custom translations by setting a different value in the DB for the same key:

```sql
-- Tenant A sees "Save"
INSERT INTO Translations (...) VALUES (..., 'DEFAULT', 'general.save', 'en', 'Save', ...);

-- Tenant B sees "Submit"  
INSERT INTO Translations (...) VALUES (..., tenantIdB, 'general.save', 'en', 'Submit', ...);
```

When querying, the service always filters by `TenantId`, so each tenant gets their own overrides.

### Cache Warmup

For large translation sets, pre-load the cache at startup:

```csharp
// In Program.cs after building the app
await app.Services.GetRequiredService<ITranslationService>().WarmupCacheAsync(tenantId);
```

(Not implemented by default, but easily added if needed.)

## Common Mistakes

### Mistake 1: Hardcoding Strings

❌ **Wrong:**
```razor
<label>Please enter your email</label>
```

✅ **Correct:**
```razor
<label>@Model.T(MessageKeys.Validation.Required)</label>
```

### Mistake 2: Forgetting to Seed the Translation

If the DB row doesn't exist, `T()` returns the key name. This is intentional fallback, but add the seed to fix:

```sql
-- Missing → add this
INSERT INTO Translations (...) VALUES (..., tenantId, 'mykey', 'en', 'My Label', ...);
```

### Mistake 3: Wrong Locale Claim

If users see English even after setting `locale` claim, check:

1. Is the claim being set during login?
2. Is the translation DB row seeded for that locale?

```csharp
// Debug: print the user's locale
var locale = User.FindFirst("locale")?.Value ?? "default";
System.Diagnostics.Debug.WriteLine($"User locale: {locale}");
```

### Mistake 4: Cache Not Clearing

If you update a translation in the database and don't see the change, **restart the app** to clear the 60-minute cache.

## See Also

- [Localized Validation](./02-localized-validation.md) — Using `MessageKey` in validation attributes
- [Demo: Translations](../../src/SmartWorkz.StarterKitMVC.Public/Pages/Demo/Translations.cshtml) — View all keys and their values
- [Demo: Validation](../../src/SmartWorkz.StarterKitMVC.Public/Pages/Demo/Validation.cshtml) — See validation in action
