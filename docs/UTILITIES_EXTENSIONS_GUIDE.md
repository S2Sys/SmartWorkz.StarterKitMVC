# SmartWorkz Utilities & Extensions Guide

A complete reference for all utility helpers and extension methods in `SmartWorkz.Core.Shared.Utilities` and `SmartWorkz.Core.Shared.Extensions`.

---

## Overview

SmartWorkz provides a comprehensive set of **static helper utilities** and **extension methods** that simplify common programming tasks with a consistent, error-handling-first approach. Most utilities return `Result<T>` for safe, composable error handling.

### Key Principles

- **Fluent Extensions**: Common types (string, bool, decimal, int, etc.) have chainable extension methods
- **Consistent Error Handling**: Text and utility helpers return `Result<T>` for explicit error states
- **Zero Allocation**: Math and comparison helpers are inline for performance
- **Static Utilities**: Standalone helper classes for complex operations (compression, JSON, enums)

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

#### RemoveWhitespace — Strip All Spaces

Removes all whitespace characters (spaces, tabs, newlines, etc.) from the string.

**Signature:**
```csharp
public static Result<string> RemoveWhitespace(string text)
```

**Example:**
```csharp
var result = TextHelper.RemoveWhitespace("Hello  World  \n  Test");
// Success: "HelloWorldTest"

var compact = TextHelper.RemoveWhitespace("  123  456  ");
// Success: "123456"
```

**Use Cases:**
- Normalize phone numbers for storage
- Compact JSON for transmission
- Clean user input before validation

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

## Slug Generation

### SlugHelper — URL-Friendly Text Conversion

Converts text to URL-friendly slugs with extensive customization options.

#### GenerateSlug — Create Slug with Options

Generates a URL-friendly slug from text with fine-grained configuration.

**Signature:**
```csharp
public static Result<string> GenerateSlug(string text, SlugOptions? options = null)
```

**Parameters:**
- `text` — Input text to convert
- `options` — Configuration (uses defaults if null)

**Returns:** `Result<string>` — Generated slug or error

**Example:**
```csharp
// Default options
var simple = SlugHelper.GenerateSlug("Hello World!");
// Success: "hello-world"

// Custom options
var options = new SlugOptions
{
    Lowercase = true,
    MaxLength = 50,
    Separator = "_",
    RemoveAccents = true,
    RemoveSpecialChars = true
};
var custom = SlugHelper.GenerateSlug("Café & Restaurant!", options);
// Success: "cafe_restaurant"

// Accented text
var accented = SlugHelper.GenerateSlug("Señor José");
// Success: "senor-jose"

// Empty result after processing
var result = SlugHelper.GenerateSlug("!!!"); // Fails: slug becomes empty
```

---

#### ToSlug — Convenience Method

Convenience method that calls `GenerateSlug` with default options.

**Signature:**
```csharp
public static Result<string> ToSlug(string text)
```

**Example:**
```csharp
var slug = SlugHelper.ToSlug("Product Name");
// Success: "product-name"
```

---

#### SlugOptions — Configuration Class

Configures slug generation behavior. All properties have sensible defaults.

**Properties:**

| Property | Type | Default | Purpose |
|----------|------|---------|---------|
| `Lowercase` | `bool` | `true` | Convert to lowercase |
| `MaxLength` | `int` | `100` | Max slug length (0 = unlimited) |
| `Separator` | `string` | `"-"` | Character(s) between words |
| `RemoveAccents` | `bool` | `true` | Remove é, ñ, etc. |
| `RemoveSpecialChars` | `bool` | `true` | Remove punctuation & symbols |

**Example:**
```csharp
// Create custom options
var options = new SlugOptions
{
    Lowercase = true,
    MaxLength = 75,
    Separator = "-",
    RemoveAccents = true,
    RemoveSpecialChars = true
};

var slug = SlugHelper.GenerateSlug("Article Title Here", options);
```

**Use Cases:**
- Generate SEO-friendly URLs from titles
- Create unique identifiers from user input
- Format product names for URLs

---

## Math Utilities

### MathHelper — Arithmetic & Comparison Operations

Static helper for common math operations. Methods are inline for performance.

#### Percentage — Calculate Percentage of Value

Calculates what percentage a given value represents.

**Signature:**
```csharp
public static decimal Percentage(decimal value, decimal percent)
```

**Example:**
```csharp
var discount = MathHelper.Percentage(100m, 20);  // 20% of 100
// Returns: 20m

var tax = MathHelper.Percentage(150m, 10);  // 10% of 150
// Returns: 15m

var portion = MathHelper.Percentage(1000m, 33.33m);
// Returns: 333.30m
```

---

#### PercentageChange — Calculate Change Between Values

Calculates the percentage change from an old value to a new value. Positive result = increase, negative = decrease.

**Signature:**
```csharp
public static decimal PercentageChange(decimal oldValue, decimal newValue)
```

**Example:**
```csharp
var change = MathHelper.PercentageChange(100m, 150m);
// Returns: 50m (50% increase)

var decrease = MathHelper.PercentageChange(150m, 100m);
// Returns: -33.33m (33% decrease)

// Special case: zero old value
var zeroChange = MathHelper.PercentageChange(0m, 50m);
// Returns: 100m (100% change when starting from zero)
```

**Use Cases:**
- Calculate growth rates
- Track inventory changes
- Display price increase/decrease percentages

---

#### RoundTo — Round to Decimal Places

Rounds a decimal value to a specified number of decimal places using "away from zero" rounding.

**Signature:**
```csharp
public static decimal RoundTo(decimal value, int decimals)
```

**Example:**
```csharp
var rounded = MathHelper.RoundTo(3.14159m, 2);
// Returns: 3.14m

var price = MathHelper.RoundTo(19.995m, 2);
// Returns: 20.00m  (away from zero rounding)

var whole = MathHelper.RoundTo(42.7m, 0);
// Returns: 43m
```

**Note:** Uses `MidpointRounding.AwayFromZero` for consistent banker's rounding avoidance.

---

#### Clamp — Constrain Value Within Range

Constrains a value to fall within a specified [min, max] range. Generic and works with any comparable type.

**Signature:**
```csharp
public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
```

**Example:**
```csharp
var value = MathHelper.Clamp(15, 0, 10);
// Returns: 10 (clamped to max)

var within = MathHelper.Clamp(5, 0, 10);
// Returns: 5 (already within range)

var below = MathHelper.Clamp(-5, 0, 10);
// Returns: 0 (clamped to min)

// Works with any IComparable type
var clampDate = MathHelper.Clamp(
    DateTime.Parse("2025-06-15"),
    DateTime.Parse("2025-01-01"),
    DateTime.Parse("2025-12-31")
);
```

---

#### Average — Calculate Mean of Values

Calculates the arithmetic mean of the provided decimal values.

**Signature:**
```csharp
public static decimal Average(params decimal[] values)
```

**Example:**
```csharp
var avg = MathHelper.Average(10m, 20m, 30m);
// Returns: 20m

var ratings = MathHelper.Average(4.5m, 3.8m, 4.2m, 4.9m);
// Returns: 4.35m

// Requires at least one value
var error = MathHelper.Average();  // Throws: ArgumentException
```

---

## Enum Utilities

### EnumHelper — Reflection-Based Enum Operations

Provides utilities for enum introspection and value retrieval.

#### GetDescription — Extract Display Name

Retrieves the `[Description]` attribute text from an enum value, falling back to the enum name.

**Signature:**
```csharp
public static string GetDescription(Enum value)
```

**Example:**
```csharp
public enum OrderStatus
{
    [Description("Pending Review")]
    Pending,
    [Description("In Progress")]
    Processing,
    [Description("Completed")]
    Done
}

var status = OrderStatus.Pending;
var desc = EnumHelper.GetDescription(status);
// Returns: "Pending Review"

// Without attribute
var name = EnumHelper.GetDescription(OrderStatus.Done);
// Returns: "Done" (if no Description attribute)
```

---

#### GetValue<T> — Parse Enum by Name

Attempts to retrieve an enum value by its name, case-insensitive.

**Signature:**
```csharp
public static Result<T> GetValue<T>(string name) where T : Enum
```

**Example:**
```csharp
var result = EnumHelper.GetValue<OrderStatus>("Pending");
// Success: OrderStatus.Pending

var caseInsensitive = EnumHelper.GetValue<OrderStatus>("pending");
// Success: OrderStatus.Pending

var notFound = EnumHelper.GetValue<OrderStatus>("Invalid");
// Fails: Error with message "Enum value 'Invalid' not found in OrderStatus"
```

**Use Cases:**
- Parse user input to enum values
- Load enum values from configuration
- Safely convert strings from API requests

---

#### GetAllValues<T> — Retrieve All Enum Members

Returns all values of the specified enum type as a list.

**Signature:**
```csharp
public static List<T> GetAllValues<T>() where T : Enum
```

**Example:**
```csharp
var allStatuses = EnumHelper.GetAllValues<OrderStatus>();
// Returns: [Pending, Processing, Done]

// Populate dropdown
var options = EnumHelper.GetAllValues<Priority>()
    .Select(p => new SelectListItem
    {
        Value = p.ToString(),
        Text = p.GetDescription()
    });
```

---

#### GetName — Retrieve Enum Member Name

Retrieves the name of an enum value as a string.

**Signature:**
```csharp
public static string GetName(Enum value)
```

**Example:**
```csharp
var name = EnumHelper.GetName(OrderStatus.Processing);
// Returns: "Processing"

// Compared to Description attribute
var desc = EnumHelper.GetDescription(OrderStatus.Processing);
// Returns: "In Progress" (from attribute)
```

---

## Compression Utilities

### CompressHelper — GZip Compression/Decompression

Provides string and byte array compression using GZip format.

#### CompressString — Compress Text to Bytes

Compresses a string to a GZip-compressed byte array.

**Signature:**
```csharp
public static Result<byte[]> CompressString(string text)
```

**Example:**
```csharp
var text = "This is some text to compress";
var result = CompressHelper.CompressString(text);

if (result.IsSuccess)
{
    var compressed = result.Value;
    // Typically 30-50% of original size
}
```

---

#### DecompressString — Decompress Bytes to Text

Decompresses a GZip-compressed byte array back to a string.

**Signature:**
```csharp
public static Result<string> DecompressString(byte[] compressed)
```

**Example:**
```csharp
var original = "Hello World";
var compressed = CompressHelper.CompressString(original).Value;
var decompressed = CompressHelper.DecompressString(compressed).Value;
// Returns: "Hello World"

// Error cases
var empty = CompressHelper.DecompressString(new byte[0]);  // Fails: empty data
```

---

#### CompressBytes — Compress Byte Array

Compresses a byte array using GZip. Does not return a Result (synchronous).

**Signature:**
```csharp
public static byte[] CompressBytes(byte[] data)
```

**Example:**
```csharp
var data = Encoding.UTF8.GetBytes("Content to compress");
var compressed = CompressHelper.CompressBytes(data);
// Returns: GZip-compressed bytes
```

---

#### DecompressBytes — Decompress Byte Array

Decompresses a GZip-compressed byte array. Does not return a Result (synchronous).

**Signature:**
```csharp
public static byte[] DecompressBytes(byte[] compressed)
```

**Example:**
```csharp
var original = Encoding.UTF8.GetBytes("Original data");
var compressed = CompressHelper.CompressBytes(original);
var restored = CompressHelper.DecompressBytes(compressed);
// Returns: [79, 114, 105, 103, ...] (UTF-8 bytes of "Original data")
```

**Use Cases:**
- Store large text in compressed form
- Reduce API response sizes
- Compress cached data in memory
- Compact log file storage

---

## JSON Utilities

### JsonHelper — JSON Serialization & Validation

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

#### IsPrettyJson — Check Formatting

Determines if a JSON string is prettified (contains newlines/indentation).

**Signature:**
```csharp
public static bool IsPrettyJson(string json)
```

**Example:**
```csharp
var pretty = @"{
  ""name"": ""Widget""
}";
var isPretty = JsonHelper.IsPrettyJson(pretty);
// Returns: true

var compact = "{\"name\":\"Widget\"}";
var isCompact = JsonHelper.IsPrettyJson(compact);
// Returns: false
```

**Use Cases:**
- Detect output format before display
- Choose compression strategy for responses
- Validate logging output format

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
| `IfTrue<T>()` | `bool, T, T → T` | `T` (first or second) | `condition.IfTrue("yes", "no")` |

**Examples:**

```csharp
bool isActive = true;

// Display conversions
var display1 = isActive.ToYesNo();  // "Yes"
var display2 = isActive.ToOnOff();  // "On"
var display3 = isActive.ToEnabledDisabled();  // "Enabled"

// Numeric conversion
var numeric = isActive.ToInt();  // 1

// Conditional value selection
var status = isActive.IfTrue("Active", "Inactive");  // "Active"
var level = hasPermission.IfTrue(10, 0);  // Generic: works with any type
```

---

### DecimalExtensions — Decimal Validation & Manipulation

Validate decimal values and perform range operations.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsPositive()` | `decimal → bool` | `true` if > 0 | `5m.IsPositive()` → `true` |
| `IsNegative()` | `decimal → bool` | `true` if < 0 | `(-5m).IsNegative()` → `true` |
| `IsZero()` | `decimal → bool` | `true` if == 0 | `0m.IsZero()` → `true` |
| `Abs()` | `decimal → decimal` | Absolute value | `(-5m).Abs()` → `5m` |
| `Round()` | `decimal, int → decimal` | Rounded value | `3.14159m.Round(2)` → `3.14m` |
| `Clamp()` | `decimal, min, max → decimal` | Constrained value | `15m.Clamp(0, 10)` → `10m` |
| `IsBetween()` | `decimal, min, max → bool` | `true` if in range | `5m.IsBetween(0, 10)` → `true` |

**Examples:**

```csharp
decimal price = 19.99m;

// Validation checks
if (price.IsPositive())
    Console.WriteLine("Price is positive");

if (price.IsBetween(0, 100))
    Console.WriteLine("Price in valid range");

// Manipulation
var rounded = price.Round(0);  // 20m
var clamped = price.Clamp(15, 25);  // 19.99m (within range)
var absolute = (-price).Abs();  // 19.99m
```

---

### GuidExtensions — GUID Checking & Formatting

Validate and format GUIDs conveniently.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsEmpty()` | `Guid → bool` | `true` if Guid.Empty | `Guid.Empty.IsEmpty()` → `true` |
| `IsNotEmpty()` | `Guid → bool` | `true` if not Guid.Empty | `newGuid.IsNotEmpty()` → `true` |
| `IfEmpty()` | `Guid, Guid → Guid` | First or replacement | `emptyId.IfEmpty(defaultId)` |
| `ToShortString()` | `Guid → string` | First 8 chars | `guid.ToShortString()` → `"a1b2c3d4"` |
| `TryParseExact()` | `string → (bool, Guid)` | Parsed or Empty | `GuidExtensions.TryParseExact(str, out result)` |

**Examples:**

```csharp
var id = Guid.NewGuid();
var emptyId = Guid.Empty;

// Validation
if (id.IsNotEmpty())
    Console.WriteLine("ID is valid");

if (emptyId.IsEmpty())
    Console.WriteLine("ID is empty");

// Safe default
var safeId = emptyId.IfEmpty(Guid.NewGuid());  // Gets new GUID if empty

// Formatting
var shortId = id.ToShortString();  // "a1b2c3d4"

// Parsing
if (GuidExtensions.TryParseExact("a1b2c3d4a1b2c3d4a1b2c3d4a1b2c3d4", out var parsed))
    Console.WriteLine($"Parsed: {parsed}");
```

---

### IntExtensions — Integer Validation & Math

Validate and manipulate integer values.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsPositive()` | `int → bool` | `true` if > 0 | `5.IsPositive()` → `true` |
| `IsNegative()` | `int → bool` | `true` if < 0 | `(-5).IsNegative()` → `true` |
| `IsEven()` | `int → bool` | `true` if divisible by 2 | `4.IsEven()` → `true` |
| `IsOdd()` | `int → bool` | `true` if not divisible by 2 | `3.IsOdd()` → `true` |
| `Abs()` | `int → int` | Absolute value | `(-5).Abs()` → `5` |
| `Square()` | `int → int` | Value squared | `5.Square()` → `25` |
| `Clamp()` | `int, min, max → int` | Constrained value | `15.Clamp(0, 10)` → `10` |
| `IsBetween()` | `int, min, max → bool` | `true` if in range | `5.IsBetween(0, 10)` → `true` |

**Examples:**

```csharp
int count = 42;
int index = -5;

// Validation
if (count.IsPositive() && count.IsEven())
    Console.WriteLine("Count is positive and even");

if (index.IsNegative())
    Console.WriteLine("Index is negative");

// Math
var square = count.Square();  // 1764
var safe = index.Abs();  // 5
var bounded = count.Clamp(0, 50);  // 42 (already in range)

// Range checking
if (count.IsBetween(0, 100))
    Console.WriteLine("Count is in valid range");
```

---

### TimeSpanExtensions — Duration Validation & Calculation

Work with time spans for relative date calculations.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsZero()` | `TimeSpan → bool` | `true` if duration is zero | `TimeSpan.Zero.IsZero()` → `true` |
| `IsPositive()` | `TimeSpan → bool` | `true` if > 0 | `TimeSpan.FromHours(1).IsPositive()` → `true` |
| `IsNegative()` | `TimeSpan → bool` | `true` if < 0 | `(-1 * TimeSpan.FromHours(1)).IsNegative()` → `true` |
| `TotalDays()` | `TimeSpan → int` | Days as integer | `TimeSpan.FromDays(5.5).TotalDays()` → `5` |
| `TotalHours()` | `TimeSpan → int` | Hours as integer | `TimeSpan.FromHours(25).TotalHours()` → `25` |
| `TotalMinutes()` | `TimeSpan → int` | Minutes as integer | `TimeSpan.FromMinutes(90).TotalMinutes()` → `90` |
| `TotalSeconds()` | `TimeSpan → int` | Seconds as integer | `TimeSpan.FromSeconds(120).TotalSeconds()` → `120` |
| `FromNow()` | `TimeSpan → DateTime` | Future time | `TimeSpan.FromHours(1).FromNow()` |
| `FromNow()` | `TimeSpan, DateTime → DateTime` | Future from base | `duration.FromNow(baseTime)` |
| `Ago()` | `TimeSpan → DateTime` | Past time | `TimeSpan.FromDays(7).Ago()` |
| `Ago()` | `TimeSpan, DateTime → DateTime` | Past from base | `duration.Ago(baseTime)` |

**Examples:**

```csharp
var duration = TimeSpan.FromDays(3);

// Validation
if (duration.IsPositive())
    Console.WriteLine("Duration is positive");

// Conversions
var hours = duration.TotalHours();  // 72
var days = duration.TotalDays();  // 3

// Date calculations
var futureDate = TimeSpan.FromHours(6).FromNow();
// UTC time 6 hours in the future

var pastDate = TimeSpan.FromDays(7).Ago();
// UTC time 7 days ago

// With specific base time
var baseTime = DateTime.Parse("2025-06-15");
var adjusted = TimeSpan.FromDays(3).FromNow(baseTime);
// "2025-06-18"
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

### CollectionExtensions — Collection Checks & Conversions

Safely check and convert collections.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `IsNullOrEmpty<T>()` | `IEnumerable<T> → bool` | `true` if null or empty | `list.IsNullOrEmpty()` → `true` |
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

// Quick slug conversion
var slug = "Product Name!".ToSlug();  // "product-name"
var articleSlug = "Breaking News: Big Story".ToSlug();  // "breaking-news-big-story"
```

---

### DateTimeExtensions — DateTime Validation & Conversion

Work with DateTime values safely.

| Method | Signature | Returns | Example |
|--------|-----------|---------|---------|
| `ToUtcKind()` | `DateTime → DateTime` | UTC-kind version | `local.ToUtcKind()` |
| `IsBetween()` | `DateTime, start, end → bool` | `true` if in range | `date.IsBetween(start, end)` |

**Examples:**

```csharp
var localTime = DateTime.Parse("2025-06-15 14:30:00");
var start = DateTime.Parse("2025-06-01");
var end = DateTime.Parse("2025-06-30");

// Ensure UTC
var utc = localTime.ToUtcKind();
// Returns DateTime with Kind = Utc

// Date range check
if (localTime.IsBetween(start, end))
    Console.WriteLine("Date is in June");
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
var hasPermission = user.IsActive && user.Role.IsAdminOrEditor();
var displayValue = hasPermission
    .ToYesNo()
    .ToUpperInvariant();  // "YES"
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
```

### TextHelper Pipeline Pattern

Compose TextHelper operations using Result<T> combinators:

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

### SlugHelper Result is Empty

**Problem:** `SlugHelper.GenerateSlug()` returns `"Slug.ResultEmpty"` error

**Cause:** Input contains only special characters that are all removed

**Solution:** Ensure input has alphanumeric content:
```csharp
var input = "!!!___---";  // Fails: no alphanumeric chars
var input2 = "Product-123";  // Succeeds
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

### GZip Decompression Throws

**Problem:** `CompressHelper.DecompressBytes()` throws exception with corrupted data

**Solution:** Verify compressed data integrity:
```csharp
// Safe decompression with error handling
try
{
    var text = CompressHelper.DecompressString(bytes).Value;
}
catch (Exception ex)
{
    // Log and handle corruption
    Console.WriteLine($"Decompression failed: {ex.Message}");
}
```

### Enum GetValue<T> Returns Error

**Problem:** `EnumHelper.GetValue<T>()` returns `"Error.EnumNotFound"`

**Solution:** Verify enum member exists:
```csharp
var result = EnumHelper.GetValue<OrderStatus>("Processing");
// Success if "Processing" exists in enum definition

var invalid = EnumHelper.GetValue<OrderStatus>("InvalidValue");
// Fails: member doesn't exist
```

---

## Performance Considerations

- **TextHelper**: Most operations are O(n) where n is string length
- **SlugHelper**: Remove accents uses Unicode normalization (slightly slower but correct)
- **MathHelper**: All methods are O(1) and inline-friendly
- **CompressHelper**: GZip compression ratio typically 30-50% for text
- **Extensions**: All are inline and have zero allocation overhead
- **Enums**: Reflection-based helpers cache internally on first call

---

## Related Guides

- [Security Guide](SECURITY_GUIDE.md) — Validation and sanitization patterns
- [Data Access Guide](DATA_ACCESS_GUIDE.md) — Working with database values
- [Configuration & Diagnostics](CONFIGURATION_DIAGNOSTICS_GUIDE.md) — Health checks and metrics
