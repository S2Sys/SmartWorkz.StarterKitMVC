# SmartWorkz Utilities & Extensions Guide

A complete reference for all utility helpers and extension methods in `SmartWorkz.Core.Shared.Utilities` and `SmartWorkz.Core.Shared.Extensions`.

---

## Overview

SmartWorkz provides a comprehensive set of **static helper utilities** and **extension methods** that simplify common programming tasks with a consistent, error-handling-first approach. Most utilities return `Result<T>` for safe, composable error handling.

### Key Principles

- **Fluent Extensions**: Common types (string, bool, decimal, int, TimeSpan, DateTime, Guid, etc.) have chainable extension methods
- **Consistent Error Handling**: Text and utility helpers return `Result<T>` for explicit error states
- **Zero Allocation**: Math and comparison helpers are inline for performance
- **Static Utilities**: Standalone helper classes for complex operations (compression, JSON, text processing)

---

## Text Utilities

### TextHelper — Advanced String Processing

The `TextHelper` class provides fluent text manipulation methods. All methods return `Result<string>` for consistent error handling.

#### Truncate — Shorten Text with Suffix

Truncates text to a maximum length and appends a customizable suffix.

**Signature:**
```csharp
public static Result<string> Truncate(string text, int maxLength, string suffix = "...")
```

**Parameters:**
- `text` — Input string to truncate
- `maxLength` — Maximum length including the suffix
- `suffix` — Suffix appended when truncating (default: `"..."`)

**Returns:** `Result<string>` — Truncated text or error

**Example:**
```csharp
var result = TextHelper.Truncate("Hello World", 8, "...");
// Success: "Hello..."

var longText = TextHelper.Truncate("The quick brown fox", 10);
// Success: "The quick..."

// Error cases handled
var empty = TextHelper.Truncate("", 5);  // Fails: input empty
var badLength = TextHelper.Truncate("test", -1);  // Fails: maxLength <= 0
```

**Use Cases:**
- Truncate product descriptions for preview cards
- Shorten titles for mobile displays
- Generate snippets from article content

---

#### Capitalize — Uppercase First Letter

Capitalizes the first letter while preserving the rest of the string.

**Signature:**
```csharp
public static Result<string> Capitalize(string text)
```

**Example:**
```csharp
var result = TextHelper.Capitalize("hello world");
// Success: "Hello world"

var upper = TextHelper.Capitalize("HELLO");
// Success: "HELLO"  (only first letter affected)
```

---

#### Decapitalize — Lowercase First Letter

Lowercases the first letter while preserving the rest of the string.

**Signature:**
```csharp
public static Result<string> Decapitalize(string text)
```

**Example:**
```csharp
var result = TextHelper.Decapitalize("HelloWorld");
// Success: "helloWorld"

var camelCase = TextHelper.Decapitalize("UserName");
// Success: "userName"
```

---

#### StripHtml — Remove HTML Tags and Entities

Removes all HTML tags and decodes HTML entities, then normalizes whitespace.

**Signature:**
```csharp
public static Result<string> StripHtml(string html)
```

**Example:**
```csharp
var result = TextHelper.StripHtml("<p>Hello <strong>World</strong></p>");
// Success: "Hello World"

var entities = TextHelper.StripHtml("Price: &pound;50 &amp; free shipping");
// Success: "Price: £50 & free shipping"

// Normalizes excess whitespace
var spaced = TextHelper.StripHtml("<p>Text    with    spaces</p>");
// Success: "Text with spaces"
```

**Use Cases:**
- Display rich text content as plain text
- Extract content from HTML emails
- Clean user-generated content for display

---

#### Pluralize — Apply Plural Rules

Applies simple English pluralization (appends 's' if count != 1).

**Signature:**
```csharp
public static Result<string> Pluralize(string singular, int count)
```

**Parameters:**
- `singular` — The singular form of the word
- `count` — Count to determine if plural form is needed

**Example:**
```csharp
var result = TextHelper.Pluralize("item", 1);
// Success: "item"

var plural = TextHelper.Pluralize("item", 5);
// Success: "items"

// In UI templates
var message = $"You have {count} {TextHelper.Pluralize("file", count).Value}";
// "You have 1 file" or "You have 5 files"
```

**Note:** This is a simple implementation suitable for basic English pluralization. For complex rules (child → children), consider a dedicated pluralization library.

---

#### TitleCase — Capitalize Each Word

Converts text to title case by capitalizing the first letter of each word.

**Signature:**
```csharp
public static Result<string> TitleCase(string text)
```

**Example:**
```csharp
var result = TextHelper.TitleCase("hello world");
// Success: "Hello World"

var heading = TextHelper.TitleCase("the quick brown fox");
// Success: "The Quick Brown Fox"
```

**Use Cases:**
- Format user-entered names and titles
- Generate heading text from database values
- Display product names consistently

---

#### Reverse — Mirror String Content

Reverses the string character by character.

**Signature:**
```csharp
public static Result<string> Reverse(string text)
```

**Example:**
```csharp
var result = TextHelper.Reverse("Hello");
// Success: "olleH"

var word = TextHelper.Reverse("stressed");
// Success: "desserts"
```

---

#### RemoveWhitespace — Remove All Whitespace

Removes all whitespace characters (spaces, tabs, newlines) from the input string using regex.

**Signature:**
```csharp
public static Result<string> RemoveWhitespace(string text)
```

**Example:**
```csharp
var result = TextHelper.RemoveWhitespace("H e l l o");
// Success: "Hello"

var spaces = TextHelper.RemoveWhitespace("text with   spaces");
// Success: "textwithspaces"

var mixed = TextHelper.RemoveWhitespace("  line1\n  line2  ");
// Success: "line1line2"
```

**Use Cases:**
- Remove formatting whitespace from user input
- Normalize phone numbers or IDs with spaces
- Clean up copy-pasted text with irregular spacing

---

#### WordWrap — Break Text at Word Boundaries

Wraps text to a specified line length while preserving word boundaries.

**Signature:**
```csharp
public static Result<string> WordWrap(string text, int lineLength, string newline = "\n")
```

**Parameters:**
- `text` — Text to wrap
- `lineLength` — Maximum length of each line
- `newline` — Line separator (default: `"\n"`)

**Example:**
```csharp
var text = "The quick brown fox jumps over the lazy dog";
var result = TextHelper.WordWrap(text, 20);
// Success:
// "The quick brown fox\njumps over the lazy\ndog"

// Custom line separator
var wrapped = TextHelper.WordWrap("One two three four five", 10, "<br />");
// "One two<br />three four<br />five"
```

**Use Cases:**
- Format paragraphs for console output
- Generate email text with line length limits
- Prepare content for fixed-width displays

---

#### Repeat — Duplicate String Content

Repeats the input string the specified number of times.

**Signature:**
```csharp
public static Result<string> Repeat(string text, int count)
```

**Example:**
```csharp
var result = TextHelper.Repeat("*", 5);
// Success: "*****"

var pattern = TextHelper.Repeat("--", 3);
// Success: "------"

var separator = TextHelper.Repeat("=", 10);
// Success: "=========="
```

**Use Cases:**
- Generate decorative separators
- Create padding strings
- Duplicate content for testing

---

## JSON Utilities

### JsonHelper — JSON Serialization & Deserialization

Provides JSON serialization, deserialization, and validation.

#### Serialize<T> — Convert Object to JSON

Serializes an object to JSON with optional indentation.

**Signature:**
```csharp
public static Result<string> Serialize<T>(T obj, bool indent = true)
```

**Parameters:**
- `obj` — Object to serialize
- `indent` — Pretty-print JSON (default: `true`)

**Features:**
- Case-insensitive property matching
- Ignores null values
- Converts enums to strings
- Pretty-prints by default

**Example:**
```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

var product = new Product { Name = "Widget", Price = 9.99m };
var result = JsonHelper.Serialize(product);

// Success (indented):
// {
//   "name": "Widget",
//   "price": 9.99
// }

// Compact (not indented)
var compact = JsonHelper.Serialize(product, indent: false);
// "{"name":"Widget","price":9.99}"

// Null values are omitted
var nullProduct = new Product { Name = "Gadget" };
var withNull = JsonHelper.Serialize(nullProduct);
// {"name":"Gadget"} (price omitted because it's decimal default)
```

---

#### Deserialize<T> — Parse JSON to Object

Deserializes a JSON string back to an object of type T.

**Signature:**
```csharp
public static Result<T> Deserialize<T>(string json)
```

**Example:**
```csharp
var json = @"{ ""name"": ""Widget"", ""price"": 9.99 }";
var result = JsonHelper.Deserialize<Product>(json);

if (result.IsSuccess)
{
    var product = result.Value;
    // product.Name = "Widget"
    // product.Price = 9.99m
}

// Error cases
var invalid = JsonHelper.Deserialize<Product>("{invalid json}");
// Fails: Error code "Error.JsonInvalid"

var empty = JsonHelper.Deserialize<Product>("");
// Fails: Error code "Error.JsonEmpty"
```

---

#### IsPrettyJson — Detect Formatted JSON

Determines if a JSON string is prettified (formatted with indentation or line breaks).

**Signature:**
```csharp
public static bool IsPrettyJson(string json)
```

**Example:**
```csharp
var compact = JsonHelper.IsPrettyJson("{\"key\":\"value\"}");
// Returns: false

var indented = JsonHelper.IsPrettyJson(@"
{
  ""key"": ""value""
}");
// Returns: true (contains newlines)

var singleLine = JsonHelper.IsPrettyJson("{\"key\":\"value\"}");
// Returns: false
```

**Use Cases:**
- Detect formatting for consistent JSON storage
- Log formatted vs. compact JSON differently
- Validate JSON source format

---

## Extension Methods Reference

### BoolExtensions — Boolean Display & Conversion

Convert booleans to human-readable strings and conditional values.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `ToYesNo()` | `bool → string` | `"Yes"` or `"No"` | `true.ToYesNo()` → `"Yes"` |
| `ToOnOff()` | `bool → string` | `"On"` or `"Off"` | `false.ToOnOff()` → `"Off"` |
| `ToEnabledDisabled()` | `bool → string` | `"Enabled"` or `"Disabled"` | `true.ToEnabledDisabled()` → `"Enabled"` |
| `ToInt()` | `bool → int` | `1` or `0` | `true.ToInt()` → `1` |
| `IfTrue<T>()` | `bool, T, T → T` | Conditional value | `true.IfTrue("yes", "no")` → `"yes"` |

**Examples:**

```csharp
bool isActive = true;

// Display conversions
var display1 = isActive.ToYesNo();  // "Yes"
var display2 = isActive.ToOnOff();  // "On"
var display3 = isActive.ToEnabledDisabled();  // "Enabled"
var asInt = isActive.ToInt();  // 1

// Conditional value selection
var status = isActive.IfTrue("Active", "Inactive");
// Returns: "Active"

var permission = hasAdminRole.IfTrue(100, 10);  // Admin=100, User=10
// Returns: 100 (if hasAdminRole is true)
```

---

### DateTimeExtensions — DateTime Validation & Conversion

Work with DateTime values for time checks and conversions.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `ToUtcKind()` | `DateTime → DateTime` | UTC-kind DateTime | `dt.ToUtcKind()` |
| `IsBetween()` | `DateTime, start, end → bool` | `true` if in range | `dt.IsBetween(start, end)` → `true` |

**Examples:**

```csharp
var date = new DateTime(2025, 6, 15, 14, 30, 0, DateTimeKind.Local);

// Convert to UTC
var utc = date.ToUtcKind();  // Converts to UTC if not already

// Range checking
var start = new DateTime(2025, 1, 1);
var end = new DateTime(2025, 12, 31);

if (date.IsBetween(start, end))
    Console.WriteLine("Date is in 2025");

// Inclusive range check
var tomorrow = DateTime.UtcNow.AddDays(1);
if (tomorrow.IsBetween(DateTime.UtcNow, DateTime.UtcNow.AddDays(2)))
    Console.WriteLine("Tomorrow is within range");
```

---

### DecimalExtensions — Decimal Validation & Manipulation

Validate decimal values and perform range operations.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsPositive()` | `decimal → bool` | `true` if > 0 | `5m.IsPositive()` → `true` |
| `IsNegative()` | `decimal → bool` | `true` if < 0 | `-5m.IsNegative()` → `true` |
| `IsZero()` | `decimal → bool` | `true` if == 0 | `0m.IsZero()` → `true` |
| `Abs()` | `decimal → decimal` | Absolute value | `-5m.Abs()` → `5m` |
| `Round()` | `decimal, int → decimal` | Rounded value | `3.14159m.Round(2)` → `3.14m` |
| `Clamp()` | `decimal, min, max → decimal` | Constrained value | `15m.Clamp(0, 10)` → `10m` |
| `IsBetween()` | `decimal, min, max → bool` | `true` if in range | `5m.IsBetween(0, 10)` → `true` |

**Examples:**

```csharp
decimal price = 19.99m;

// Validation checks
if (price.IsPositive())
    Console.WriteLine("Price is positive");

if (!price.IsZero())
    Console.WriteLine("Price has a value");

if (price.IsBetween(0, 100))
    Console.WriteLine("Price in valid range");

// Manipulation
var rounded = price.Round(0);  // 20m
var absolute = (-price).Abs();  // 19.99m
var clamped = price.Clamp(15, 25);  // 19.99m (within range)
var high = 150m.Clamp(0, 100);  // 100m (clamped to max)
```

---

### GuidExtensions — GUID Checking & Formatting

Validate and format GUIDs conveniently.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsEmpty()` | `Guid → bool` | `true` if Guid.Empty | `Guid.Empty.IsEmpty()` → `true` |
| `IsNotEmpty()` | `Guid → bool` | `true` if not Guid.Empty | `guid.IsNotEmpty()` → `true` |
| `IfEmpty()` | `Guid, Guid → Guid` | Default if empty | `emptyId.IfEmpty(defaultId)` |
| `ToShortString()` | `Guid → string` | First 8 chars | `guid.ToShortString()` → `"a1b2c3d4"` |
| `TryParseExact()` | `string → (bool, Guid)` | Parse result | `GuidExtensions.TryParseExact(str, out result)` |

**Examples:**

```csharp
var id = Guid.NewGuid();
var emptyId = Guid.Empty;
var defaultId = Guid.Parse("00000000-0000-0000-0000-000000000001");

// Validation
if (emptyId.IsEmpty())
    Console.WriteLine("ID is empty");

if (id.IsNotEmpty())
    Console.WriteLine("ID has a value");

// Use default for empty values
var safeId = emptyId.IfEmpty(defaultId);
// Returns: defaultId (since emptyId was empty)

// Formatting
var shortId = id.ToShortString();  // "a1b2c3d4"

// Parsing
if (GuidExtensions.TryParseExact("a1b2c3d4a1b2c3d4a1b2c3d4a1b2c3d4", out var parsed))
    Console.WriteLine($"Parsed: {parsed}");
else
    Console.WriteLine("Invalid GUID format");
```

---

### IntExtensions — Integer Validation & Math

Validate and manipulate integer values.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsPositive()` | `int → bool` | `true` if > 0 | `5.IsPositive()` → `true` |
| `IsNegative()` | `int → bool` | `true` if < 0 | `-5.IsNegative()` → `true` |
| `IsEven()` | `int → bool` | `true` if divisible by 2 | `4.IsEven()` → `true` |
| `IsOdd()` | `int → bool` | `true` if not divisible by 2 | `3.IsOdd()` → `true` |
| `Abs()` | `int → int` | Absolute value | `-5.Abs()` → `5` |
| `Clamp()` | `int, min, max → int` | Constrained value | `15.Clamp(0, 10)` → `10` |
| `Square()` | `int → int` | Value squared | `5.Square()` → `25` |
| `IsBetween()` | `int, min, max → bool` | `true` if in range | `5.IsBetween(0, 10)` → `true` |

**Examples:**

```csharp
int count = 42;

// Validation
if (count.IsPositive() && count.IsEven())
    Console.WriteLine("Count is positive and even");

if (count.IsOdd())
    Console.WriteLine("Count is odd");

// Math operations
var square = count.Square();  // 1764
var absolute = (-count).Abs();  // 42
var bounded = count.Clamp(0, 50);  // 42 (already in range)

// Range checking
if (count.IsBetween(0, 100))
    Console.WriteLine("Count is in valid range");

// Practical example: pagination
int pageNum = 5;
int totalPages = 3;
var validPage = pageNum.Clamp(1, totalPages);  // 3 (clamped to max)
```

---

### TimeSpanExtensions — Duration Validation & Calculation

Work with time spans for relative date calculations.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsZero()` | `TimeSpan → bool` | `true` if duration is zero | `TimeSpan.Zero.IsZero()` → `true` |
| `IsPositive()` | `TimeSpan → bool` | `true` if > 0 | `TimeSpan.FromHours(1).IsPositive()` → `true` |
| `IsNegative()` | `TimeSpan → bool` | `true` if < 0 | `TimeSpan.FromHours(-1).IsNegative()` → `true` |
| `TotalDays()` | `TimeSpan → int` | Days as int | `TimeSpan.FromDays(3.5).TotalDays()` → `3` |
| `TotalHours()` | `TimeSpan → int` | Hours as int | `TimeSpan.FromHours(25).TotalHours()` → `25` |
| `TotalMinutes()` | `TimeSpan → int` | Minutes as int | `TimeSpan.FromMinutes(90).TotalMinutes()` → `90` |
| `TotalSeconds()` | `TimeSpan → int` | Seconds as int | `TimeSpan.FromSeconds(3661).TotalSeconds()` → `3661` |
| `FromNow()` | `TimeSpan → DateTime` | Future DateTime | `TimeSpan.FromHours(1).FromNow()` |
| `FromNow(baseTime)` | `TimeSpan, DateTime → DateTime` | Future DateTime from base | `TimeSpan.FromHours(1).FromNow(customTime)` |
| `Ago()` | `TimeSpan → DateTime` | Past DateTime | `TimeSpan.FromDays(7).Ago()` |
| `Ago(baseTime)` | `TimeSpan, DateTime → DateTime` | Past DateTime from base | `TimeSpan.FromDays(7).Ago(customTime)` |

**Examples:**

```csharp
var duration = TimeSpan.FromDays(3);

// Validation
if (duration.IsPositive())
    Console.WriteLine("Duration is positive");

if (!duration.IsZero())
    Console.WriteLine("Has duration");

// Convert to integer units
var days = duration.TotalDays();  // 3
var hours = TimeSpan.FromHours(25.5).TotalHours();  // 25
var minutes = TimeSpan.FromMinutes(90).TotalMinutes();  // 90

// Future dates
var inSixHours = TimeSpan.FromHours(6).FromNow();
// DateTime 6 hours in the future (UTC)

var tomorrow = TimeSpan.FromDays(1).FromNow();
// DateTime tomorrow (UTC)

var nextWeek = TimeSpan.FromDays(7).FromNow();
// DateTime 1 week in the future

// Past dates
var lastWeek = TimeSpan.FromDays(7).Ago();
// DateTime 1 week ago (UTC)

var twoHoursAgo = TimeSpan.FromHours(2).Ago();
// DateTime 2 hours ago (UTC)

// With custom base time
var baseDate = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
var future = TimeSpan.FromHours(3).FromNow(baseDate);
// 2025-06-15 15:00:00 UTC

var past = TimeSpan.FromDays(5).Ago(baseDate);
// 2025-06-10 12:00:00 UTC
```

---

### EnumExtensions — Enum Display Names

Extract display names from enum values via attributes.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `GetDescription()` | `Enum → string` | Display name | `status.GetDescription()` |

**Example:**

```csharp
public enum OrderStatus
{
    [Display(Name = "Pending Review")]
    Pending,
    [Display(Name = "In Progress")]
    Processing,
    [Display(Name = "Delivered")]
    Delivered
}

var status = OrderStatus.Processing;
var displayName = status.GetDescription();
// Returns: "In Progress" (from [Display] attribute)

// For dropdowns
var options = new List<SelectListItem>();
foreach (var value in Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>())
{
    options.Add(new SelectListItem
    {
        Value = value.ToString(),
        Text = value.GetDescription()
    });
}
```

---

### StringExtensions — String Validation & Processing

Handle strings safely with common conversions.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsNullOrWhiteSpace()` | `string? → bool` | `true` if null or whitespace | `"  ".IsNullOrWhiteSpace()` → `true` |
| `SafeTrim()` | `string? → string` | Trimmed or empty | `null.SafeTrim()` → `""` |
| `ToSlug()` | `string → string` | URL-safe slug | `"Hello World!".ToSlug()` → `"hello-world"` |

**Examples:**

```csharp
string text = "  Hello World  ";
string? nullable = null;

// Safe whitespace check
if (!text.IsNullOrWhiteSpace())
    Console.WriteLine("Has content");

// Safe trim (handles null)
var trimmed = text.SafeTrim();  // "Hello World"
var fromNull = nullable.SafeTrim();  // ""

// Quick slug conversion (simple: lowercase and replace spaces with hyphens)
var slug = "Product Name!".ToSlug();  // "product-name"
var articleSlug = "Breaking News: Big Story".ToSlug();  // "breaking-news-big-story"
var hyphenated = "HELLO WORLD".ToSlug();  // "hello-world"
```

---

### CollectionExtensions — Collection Checks & Conversions

Safely check and convert collections.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsNullOrEmpty<T>()` | `IEnumerable<T>? → bool` | `true` if null or empty | `list.IsNullOrEmpty()` → `true` |
| `ToReadOnlyCollection<T>()` | `IEnumerable<T> → IReadOnlyCollection<T>` | Read-only wrapper | `list.ToReadOnlyCollection()` |

**Examples:**

```csharp
List<string> items = new();
List<string> filled = new() { "a", "b", "c" };

// Safe null/empty check
if (items.IsNullOrEmpty())
    Console.WriteLine("No items");

if (!filled.IsNullOrEmpty())
    Console.WriteLine($"Has {filled.Count} items");

// Handle null references safely
List<string>? nullable = null;
if (nullable.IsNullOrEmpty())
    Console.WriteLine("Null or empty");

// Convert to read-only
IReadOnlyCollection<string> readOnly = filled.ToReadOnlyCollection();
// Cannot modify: readOnly.Add("d") → Compilation error

// Practical example: API response
public IReadOnlyCollection<UserDto> GetUsers()
{
    var users = _service.FetchUsers();
    return users.ToReadOnlyCollection();  // Prevents external modification
}
```

---

## Common Patterns & Best Practices

### Result<T> Error Handling

All TextHelper and utility helper methods return `Result<T>`. Always check `IsSuccess` before using values:

```csharp
var result = TextHelper.Truncate("Hello", 3);

// Pattern 1: Check IsSuccess
if (result.IsSuccess)
{
    var truncated = result.Value;  // "H..."
}

// Pattern 2: Using Match
result.Match(
    onSuccess: value => Console.WriteLine($"Truncated: {value}"),
    onFailure: error => Console.WriteLine($"Error: {error.Message}")
);

// Pattern 3: Provide default
var safe = result.Match(
    onSuccess: v => v,
    onFailure: _ => "N/A"
);
```

### Chaining Extensions

Extensions allow fluent chaining for elegant code:

```csharp
// Chaining multiple extensions
var date = DateTime.UtcNow
    .ToUtcKind()  // Ensure UTC
    .AddDays(-7);  // Go back a week

if (date.IsBetween(start, end))
{
    // Process...
}

// Bool chaining
var hasPermission = user.IsActive && user.Role == "Admin";
var displayValue = hasPermission
    .ToYesNo();  // "Yes" or "No"

// Decimal validation chain
decimal price = 19.99m;
if (price.IsPositive() && price.IsBetween(0, 100))
{
    var rounded = price.Round(0);
}
```

### Extension Method Null Safety

String extensions handle null safely:

```csharp
// No null reference exception
string? nullable = null;
var safe = nullable.SafeTrim();  // Returns ""

// Chaining from null
if (!nullable.IsNullOrWhiteSpace())
{
    // Never executes
}

// Collection safety
List<string>? items = null;
if (!items.IsNullOrEmpty())
{
    // Safe to use items
}
```

### TextHelper Pipeline Pattern

Compose TextHelper operations using Result<T>:

```csharp
var text = "  Hello World  ";

// Compose operations
var result = TextHelper
    .Capitalize(text.SafeTrim())  // Remove spaces, capitalize
    .Match(
        onSuccess: capitalized => 
            TextHelper.TitleCase(capitalized),
        onFailure: error => error
    );
```

---

## Troubleshooting & Common Issues

### TextHelper Returns "Text.InputEmpty"

**Problem:** `TextHelper.Truncate()` returns failure with error code `"Text.InputEmpty"`

**Solution:** Validate input before calling:
```csharp
if (string.IsNullOrEmpty(input))
    return "N/A";  // Default value

var result = TextHelper.Truncate(input, 100);
```

### JSON Deserialization Fails

**Problem:** `JsonHelper.Deserialize<T>()` returns `"Error.JsonInvalid"`

**Solution:** Check JSON format:
```csharp
// Invalid JSON
var bad = JsonHelper.Deserialize<Product>("{name: Widget}");

// Valid JSON (quoted keys)
var good = JsonHelper.Deserialize<Product>("{\"name\": \"Widget\"}");
```

### Detecting Pretty vs Compact JSON

**Problem:** Need to determine if JSON is formatted

**Solution:** Use `IsPrettyJson()`:
```csharp
var json = await GetJsonFromSource();

if (JsonHelper.IsPrettyJson(json))
{
    // Process formatted JSON
}
else
{
    // Process compact JSON
}
```

### Empty Guid Handling

**Problem:** Checking for empty GUIDs repeatedly

**Solution:** Use `IsEmpty()` and `IfEmpty()`:
```csharp
var userId = GetUserIdFromRequest();

// Check and use default
var safeUserId = userId.IfEmpty(DefaultUserId);

// Or validate
if (userId.IsEmpty())
{
    throw new ArgumentException("User ID required");
}
```

### Range Checking

**Problem:** Multiple conditions for range validation

**Solution:** Use `IsBetween()` extension:
```csharp
// Before
if (price >= 0 && price <= 100)
{
    // Process price
}

// After (cleaner)
if (price.IsBetween(0, 100))
{
    // Process price
}
```

---

## Performance Considerations

- **TextHelper**: Most operations are O(n) where n is string length
- **JsonHelper**: Serialization/deserialization uses System.Text.Json (optimized)
- **Extensions**: All are inline and have zero allocation overhead
- **DateTime/TimeSpan**: All operations are O(1)
- **Guid/Int operations**: All inline, no allocations

---

## Related Guides

- [Security Guide](SECURITY_GUIDE.md) — Validation and sanitization patterns
- [Data Access Guide](DATA_ACCESS_GUIDE.md) — Working with database values
- [Configuration & Diagnostics](CONFIGURATION_DIAGNOSTICS_GUIDE.md) — Health checks and metrics
