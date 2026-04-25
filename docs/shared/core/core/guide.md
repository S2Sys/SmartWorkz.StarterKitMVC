# Core Usage Guide

## Overview

Marks an entity that tracks who created and last modified it.
             Implemented by auditable entity classes to provide creation and modification metadata.

## Examples

### Address

```csharp
var addressResult = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA");
            
             if (addressResult.IsSuccess)
             {
                 var address = addressResult.Value;
                 Console.WriteLine(address.FullAddress);  // "123 Main St, Springfield, IL 62701, USA"
                 order.ShippingAddress = address;
             }
             else
             {
                 // Handle validation error (e.g., STREET_EMPTY, CITY_EMPTY, etc.)
                 logger.LogError(addressResult.Error.Message);
             }
            
             // Address equality is value-based
             var address1 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             var address2 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             bool areEqual = address1 == address2;  // true (same address values)
```

### EmailAddress

```csharp
var emailResult = EmailAddress.Create("customer@example.com");
            
             if (emailResult.IsSuccess)
             {
                 var email = emailResult.Value;
                 Console.WriteLine(email.Value);  // "customer@example.com"
                 customer.Email = email;
             }
             else
             {
                 // Handle validation error
                 switch (emailResult.Error.Code)
                 {
                     case "EMAIL_EMPTY":
                         logger.LogError("Email address is required");
                         break;
                     case "EMAIL_INVALID":
                         logger.LogError("Email format is invalid");
                         break;
                     case "EMAIL_TOO_LONG":
                         logger.LogError("Email must not exceed 256 characters");
                         break;
                 }
             }
            
             // Email equality is case-insensitive (normalized during creation)
             var email1 = EmailAddress.Create("John@Example.COM").Value;
             var email2 = EmailAddress.Create("john@example.com").Value;
             bool areEqual = email1 == email2;  // true (both stored as "john@example.com")
```

### Money

```csharp
// Creating a money value
             var priceResult = Money.Create(99.99m, "USD");
             if (priceResult.IsSuccess)
             {
                 order.Total = priceResult.Value;  // 99.99 USD
             }
             else
             {
                 logger.LogError(priceResult.Error.Message);  // "Amount cannot be negative"
             }
            
             // Performing arithmetic operations
             var cost = Money.Create(50.00m, "USD").Value;
             var tax = Money.Create(5.00m, "USD").Value;
             var totalResult = cost.Add(tax);
            
             if (totalResult.IsSuccess)
             {
                 order.Total = totalResult.Value;  // 55.00 USD
             }
             else
             {
                 logger.LogError(totalResult.Error.Message);  // Possible: "Cannot add money with different currencies"
             }
            
             // Attempting to mix currencies fails safely
             var usd = Money.Create(100m, "USD").Value;
             var eur = Money.Create(100m, "EUR").Value;
             var mixResult = usd.Add(eur);  // IsSuccess = false, Error.Code = "CURRENCY_MISMATCH"
```

### PersonName

```csharp
// Creating a name without middle name
             var nameResult = PersonName.Create("John", "Doe");
             if (nameResult.IsSuccess)
             {
                 customer.Name = nameResult.Value;  // "John Doe"
             }
            
             // Creating a name with middle name
             var fullNameResult = PersonName.Create("John", "Doe", "Michael");
             if (fullNameResult.IsSuccess)
             {
                 customer.Name = fullNameResult.Value;  // "John Michael Doe"
             }
             else
             {
                 logger.LogError(fullNameResult.Error.Message);  // "First name cannot be empty"
             }
            
             // Accessing name components
             var name = PersonName.Create("Jane", "Smith", "Marie").Value;
             Console.WriteLine(name.FirstName);   // "Jane"
             Console.WriteLine(name.MiddleName);  // "Marie"
             Console.WriteLine(name.LastName);    // "Smith"
             Console.WriteLine(name.FullName);    // "Jane Marie Smith"
            
             // Name equality is value-based
             var name1 = PersonName.Create("John", "Doe").Value;
             var name2 = PersonName.Create("John", "Doe").Value;
             bool areEqual = name1 == name2;  // true (same name values)
```

### PhoneNumber

```csharp
// Creating a phone number with various formats
             var phoneResult = PhoneNumber.Create("(555) 123-4567");  // Accepts any format
             if (phoneResult.IsSuccess)
             {
                 customer.Phone = phoneResult.Value;  // Stores as "5551234567"
                 Console.WriteLine(phoneResult.Value.FormattedNumber);  // "5551234567"
             }
             else
             {
                 logger.LogError(phoneResult.Error.Message);  // Possible: "Phone number must contain at least 10 digits"
             }
            
             // More format examples
             var validFormats = new[]
             {
                 "555-123-4567",      // With hyphens
                 "(555) 123-4567",    // With parentheses and spaces
                 "5551234567",        // Digits only
                 "+1 555 123 4567",   // With plus and spaces
                 "555.123.4567"       // With dots
             };
            
             // Phone number equality is based on digits
             var phone1 = PhoneNumber.Create("(555) 123-4567").Value;
             var phone2 = PhoneNumber.Create("555-123-4567").Value;
             bool areEqual = phone1 == phone2;  // true (both have same digits: "5551234567")
```

### ValueObject

```csharp
Derived classes follow this pattern:
             
             public sealed class EmailAddress : ValueObject
             {
                 private EmailAddress(string value) => Value = value;
            
                 public string Value { get; }
            
                 public static Result<EmailAddress> Create(string? email)
                 {
                     if (string.IsNullOrWhiteSpace(email))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_EMPTY", "Email cannot be empty"));
            
                     var trimmed = email.Trim().ToLowerInvariant();
            
                     if (!IsValidFormat(trimmed))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_INVALID", "Email format is invalid"));
            
                     return Result.Ok<EmailAddress>(new EmailAddress(trimmed));
                 }
            
                 protected override IEnumerable<object?> GetAtomicValues()
                 {
                     yield return Value;
                 }
             }
```

### Address

```csharp
var addressResult = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA");
            
             if (addressResult.IsSuccess)
             {
                 var address = addressResult.Value;
                 Console.WriteLine(address.FullAddress);  // "123 Main St, Springfield, IL 62701, USA"
                 order.ShippingAddress = address;
             }
             else
             {
                 // Handle validation error (e.g., STREET_EMPTY, CITY_EMPTY, etc.)
                 logger.LogError(addressResult.Error.Message);
             }
            
             // Address equality is value-based
             var address1 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             var address2 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             bool areEqual = address1 == address2;  // true (same address values)
```

### EmailAddress

```csharp
var emailResult = EmailAddress.Create("customer@example.com");
            
             if (emailResult.IsSuccess)
             {
                 var email = emailResult.Value;
                 Console.WriteLine(email.Value);  // "customer@example.com"
                 customer.Email = email;
             }
             else
             {
                 // Handle validation error
                 switch (emailResult.Error.Code)
                 {
                     case "EMAIL_EMPTY":
                         logger.LogError("Email address is required");
                         break;
                     case "EMAIL_INVALID":
                         logger.LogError("Email format is invalid");
                         break;
                     case "EMAIL_TOO_LONG":
                         logger.LogError("Email must not exceed 256 characters");
                         break;
                 }
             }
            
             // Email equality is case-insensitive (normalized during creation)
             var email1 = EmailAddress.Create("John@Example.COM").Value;
             var email2 = EmailAddress.Create("john@example.com").Value;
             bool areEqual = email1 == email2;  // true (both stored as "john@example.com")
```

### Money

```csharp
// Creating a money value
             var priceResult = Money.Create(99.99m, "USD");
             if (priceResult.IsSuccess)
             {
                 order.Total = priceResult.Value;  // 99.99 USD
             }
             else
             {
                 logger.LogError(priceResult.Error.Message);  // "Amount cannot be negative"
             }
            
             // Performing arithmetic operations
             var cost = Money.Create(50.00m, "USD").Value;
             var tax = Money.Create(5.00m, "USD").Value;
             var totalResult = cost.Add(tax);
            
             if (totalResult.IsSuccess)
             {
                 order.Total = totalResult.Value;  // 55.00 USD
             }
             else
             {
                 logger.LogError(totalResult.Error.Message);  // Possible: "Cannot add money with different currencies"
             }
            
             // Attempting to mix currencies fails safely
             var usd = Money.Create(100m, "USD").Value;
             var eur = Money.Create(100m, "EUR").Value;
             var mixResult = usd.Add(eur);  // IsSuccess = false, Error.Code = "CURRENCY_MISMATCH"
```

### PersonName

```csharp
// Creating a name without middle name
             var nameResult = PersonName.Create("John", "Doe");
             if (nameResult.IsSuccess)
             {
                 customer.Name = nameResult.Value;  // "John Doe"
             }
            
             // Creating a name with middle name
             var fullNameResult = PersonName.Create("John", "Doe", "Michael");
             if (fullNameResult.IsSuccess)
             {
                 customer.Name = fullNameResult.Value;  // "John Michael Doe"
             }
             else
             {
                 logger.LogError(fullNameResult.Error.Message);  // "First name cannot be empty"
             }
            
             // Accessing name components
             var name = PersonName.Create("Jane", "Smith", "Marie").Value;
             Console.WriteLine(name.FirstName);   // "Jane"
             Console.WriteLine(name.MiddleName);  // "Marie"
             Console.WriteLine(name.LastName);    // "Smith"
             Console.WriteLine(name.FullName);    // "Jane Marie Smith"
            
             // Name equality is value-based
             var name1 = PersonName.Create("John", "Doe").Value;
             var name2 = PersonName.Create("John", "Doe").Value;
             bool areEqual = name1 == name2;  // true (same name values)
```

### PhoneNumber

```csharp
// Creating a phone number with various formats
             var phoneResult = PhoneNumber.Create("(555) 123-4567");  // Accepts any format
             if (phoneResult.IsSuccess)
             {
                 customer.Phone = phoneResult.Value;  // Stores as "5551234567"
                 Console.WriteLine(phoneResult.Value.FormattedNumber);  // "5551234567"
             }
             else
             {
                 logger.LogError(phoneResult.Error.Message);  // Possible: "Phone number must contain at least 10 digits"
             }
            
             // More format examples
             var validFormats = new[]
             {
                 "555-123-4567",      // With hyphens
                 "(555) 123-4567",    // With parentheses and spaces
                 "5551234567",        // Digits only
                 "+1 555 123 4567",   // With plus and spaces
                 "555.123.4567"       // With dots
             };
            
             // Phone number equality is based on digits
             var phone1 = PhoneNumber.Create("(555) 123-4567").Value;
             var phone2 = PhoneNumber.Create("555-123-4567").Value;
             bool areEqual = phone1 == phone2;  // true (both have same digits: "5551234567")
```

### ValueObject

```csharp
Derived classes follow this pattern:
             
             public sealed class EmailAddress : ValueObject
             {
                 private EmailAddress(string value) => Value = value;
            
                 public string Value { get; }
            
                 public static Result<EmailAddress> Create(string? email)
                 {
                     if (string.IsNullOrWhiteSpace(email))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_EMPTY", "Email cannot be empty"));
            
                     var trimmed = email.Trim().ToLowerInvariant();
            
                     if (!IsValidFormat(trimmed))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_INVALID", "Email format is invalid"));
            
                     return Result.Ok<EmailAddress>(new EmailAddress(trimmed));
                 }
            
                 protected override IEnumerable<object?> GetAtomicValues()
                 {
                     yield return Value;
                 }
             }
```

### Address

```csharp
var addressResult = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA");
            
             if (addressResult.IsSuccess)
             {
                 var address = addressResult.Value;
                 Console.WriteLine(address.FullAddress);  // "123 Main St, Springfield, IL 62701, USA"
                 order.ShippingAddress = address;
             }
             else
             {
                 // Handle validation error (e.g., STREET_EMPTY, CITY_EMPTY, etc.)
                 logger.LogError(addressResult.Error.Message);
             }
            
             // Address equality is value-based
             var address1 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             var address2 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             bool areEqual = address1 == address2;  // true (same address values)
```

### EmailAddress

```csharp
var emailResult = EmailAddress.Create("customer@example.com");
            
             if (emailResult.IsSuccess)
             {
                 var email = emailResult.Value;
                 Console.WriteLine(email.Value);  // "customer@example.com"
                 customer.Email = email;
             }
             else
             {
                 // Handle validation error
                 switch (emailResult.Error.Code)
                 {
                     case "EMAIL_EMPTY":
                         logger.LogError("Email address is required");
                         break;
                     case "EMAIL_INVALID":
                         logger.LogError("Email format is invalid");
                         break;
                     case "EMAIL_TOO_LONG":
                         logger.LogError("Email must not exceed 256 characters");
                         break;
                 }
             }
            
             // Email equality is case-insensitive (normalized during creation)
             var email1 = EmailAddress.Create("John@Example.COM").Value;
             var email2 = EmailAddress.Create("john@example.com").Value;
             bool areEqual = email1 == email2;  // true (both stored as "john@example.com")
```

### Money

```csharp
// Creating a money value
             var priceResult = Money.Create(99.99m, "USD");
             if (priceResult.IsSuccess)
             {
                 order.Total = priceResult.Value;  // 99.99 USD
             }
             else
             {
                 logger.LogError(priceResult.Error.Message);  // "Amount cannot be negative"
             }
            
             // Performing arithmetic operations
             var cost = Money.Create(50.00m, "USD").Value;
             var tax = Money.Create(5.00m, "USD").Value;
             var totalResult = cost.Add(tax);
            
             if (totalResult.IsSuccess)
             {
                 order.Total = totalResult.Value;  // 55.00 USD
             }
             else
             {
                 logger.LogError(totalResult.Error.Message);  // Possible: "Cannot add money with different currencies"
             }
            
             // Attempting to mix currencies fails safely
             var usd = Money.Create(100m, "USD").Value;
             var eur = Money.Create(100m, "EUR").Value;
             var mixResult = usd.Add(eur);  // IsSuccess = false, Error.Code = "CURRENCY_MISMATCH"
```

### PersonName

```csharp
// Creating a name without middle name
             var nameResult = PersonName.Create("John", "Doe");
             if (nameResult.IsSuccess)
             {
                 customer.Name = nameResult.Value;  // "John Doe"
             }
            
             // Creating a name with middle name
             var fullNameResult = PersonName.Create("John", "Doe", "Michael");
             if (fullNameResult.IsSuccess)
             {
                 customer.Name = fullNameResult.Value;  // "John Michael Doe"
             }
             else
             {
                 logger.LogError(fullNameResult.Error.Message);  // "First name cannot be empty"
             }
            
             // Accessing name components
             var name = PersonName.Create("Jane", "Smith", "Marie").Value;
             Console.WriteLine(name.FirstName);   // "Jane"
             Console.WriteLine(name.MiddleName);  // "Marie"
             Console.WriteLine(name.LastName);    // "Smith"
             Console.WriteLine(name.FullName);    // "Jane Marie Smith"
            
             // Name equality is value-based
             var name1 = PersonName.Create("John", "Doe").Value;
             var name2 = PersonName.Create("John", "Doe").Value;
             bool areEqual = name1 == name2;  // true (same name values)
```

### PhoneNumber

```csharp
// Creating a phone number with various formats
             var phoneResult = PhoneNumber.Create("(555) 123-4567");  // Accepts any format
             if (phoneResult.IsSuccess)
             {
                 customer.Phone = phoneResult.Value;  // Stores as "5551234567"
                 Console.WriteLine(phoneResult.Value.FormattedNumber);  // "5551234567"
             }
             else
             {
                 logger.LogError(phoneResult.Error.Message);  // Possible: "Phone number must contain at least 10 digits"
             }
            
             // More format examples
             var validFormats = new[]
             {
                 "555-123-4567",      // With hyphens
                 "(555) 123-4567",    // With parentheses and spaces
                 "5551234567",        // Digits only
                 "+1 555 123 4567",   // With plus and spaces
                 "555.123.4567"       // With dots
             };
            
             // Phone number equality is based on digits
             var phone1 = PhoneNumber.Create("(555) 123-4567").Value;
             var phone2 = PhoneNumber.Create("555-123-4567").Value;
             bool areEqual = phone1 == phone2;  // true (both have same digits: "5551234567")
```

### ValueObject

```csharp
Derived classes follow this pattern:
             
             public sealed class EmailAddress : ValueObject
             {
                 private EmailAddress(string value) => Value = value;
            
                 public string Value { get; }
            
                 public static Result<EmailAddress> Create(string? email)
                 {
                     if (string.IsNullOrWhiteSpace(email))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_EMPTY", "Email cannot be empty"));
            
                     var trimmed = email.Trim().ToLowerInvariant();
            
                     if (!IsValidFormat(trimmed))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_INVALID", "Email format is invalid"));
            
                     return Result.Ok<EmailAddress>(new EmailAddress(trimmed));
                 }
            
                 protected override IEnumerable<object?> GetAtomicValues()
                 {
                     yield return Value;
                 }
             }
```

### Address

```csharp
var addressResult = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA");
            
             if (addressResult.IsSuccess)
             {
                 var address = addressResult.Value;
                 Console.WriteLine(address.FullAddress);  // "123 Main St, Springfield, IL 62701, USA"
                 order.ShippingAddress = address;
             }
             else
             {
                 // Handle validation error (e.g., STREET_EMPTY, CITY_EMPTY, etc.)
                 logger.LogError(addressResult.Error.Message);
             }
            
             // Address equality is value-based
             var address1 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             var address2 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             bool areEqual = address1 == address2;  // true (same address values)
```

### EmailAddress

```csharp
var emailResult = EmailAddress.Create("customer@example.com");
            
             if (emailResult.IsSuccess)
             {
                 var email = emailResult.Value;
                 Console.WriteLine(email.Value);  // "customer@example.com"
                 customer.Email = email;
             }
             else
             {
                 // Handle validation error
                 switch (emailResult.Error.Code)
                 {
                     case "EMAIL_EMPTY":
                         logger.LogError("Email address is required");
                         break;
                     case "EMAIL_INVALID":
                         logger.LogError("Email format is invalid");
                         break;
                     case "EMAIL_TOO_LONG":
                         logger.LogError("Email must not exceed 256 characters");
                         break;
                 }
             }
            
             // Email equality is case-insensitive (normalized during creation)
             var email1 = EmailAddress.Create("John@Example.COM").Value;
             var email2 = EmailAddress.Create("john@example.com").Value;
             bool areEqual = email1 == email2;  // true (both stored as "john@example.com")
```

### Money

```csharp
// Creating a money value
             var priceResult = Money.Create(99.99m, "USD");
             if (priceResult.IsSuccess)
             {
                 order.Total = priceResult.Value;  // 99.99 USD
             }
             else
             {
                 logger.LogError(priceResult.Error.Message);  // "Amount cannot be negative"
             }
            
             // Performing arithmetic operations
             var cost = Money.Create(50.00m, "USD").Value;
             var tax = Money.Create(5.00m, "USD").Value;
             var totalResult = cost.Add(tax);
            
             if (totalResult.IsSuccess)
             {
                 order.Total = totalResult.Value;  // 55.00 USD
             }
             else
             {
                 logger.LogError(totalResult.Error.Message);  // Possible: "Cannot add money with different currencies"
             }
            
             // Attempting to mix currencies fails safely
             var usd = Money.Create(100m, "USD").Value;
             var eur = Money.Create(100m, "EUR").Value;
             var mixResult = usd.Add(eur);  // IsSuccess = false, Error.Code = "CURRENCY_MISMATCH"
```

### PersonName

```csharp
// Creating a name without middle name
             var nameResult = PersonName.Create("John", "Doe");
             if (nameResult.IsSuccess)
             {
                 customer.Name = nameResult.Value;  // "John Doe"
             }
            
             // Creating a name with middle name
             var fullNameResult = PersonName.Create("John", "Doe", "Michael");
             if (fullNameResult.IsSuccess)
             {
                 customer.Name = fullNameResult.Value;  // "John Michael Doe"
             }
             else
             {
                 logger.LogError(fullNameResult.Error.Message);  // "First name cannot be empty"
             }
            
             // Accessing name components
             var name = PersonName.Create("Jane", "Smith", "Marie").Value;
             Console.WriteLine(name.FirstName);   // "Jane"
             Console.WriteLine(name.MiddleName);  // "Marie"
             Console.WriteLine(name.LastName);    // "Smith"
             Console.WriteLine(name.FullName);    // "Jane Marie Smith"
            
             // Name equality is value-based
             var name1 = PersonName.Create("John", "Doe").Value;
             var name2 = PersonName.Create("John", "Doe").Value;
             bool areEqual = name1 == name2;  // true (same name values)
```

### PhoneNumber

```csharp
// Creating a phone number with various formats
             var phoneResult = PhoneNumber.Create("(555) 123-4567");  // Accepts any format
             if (phoneResult.IsSuccess)
             {
                 customer.Phone = phoneResult.Value;  // Stores as "5551234567"
                 Console.WriteLine(phoneResult.Value.FormattedNumber);  // "5551234567"
             }
             else
             {
                 logger.LogError(phoneResult.Error.Message);  // Possible: "Phone number must contain at least 10 digits"
             }
            
             // More format examples
             var validFormats = new[]
             {
                 "555-123-4567",      // With hyphens
                 "(555) 123-4567",    // With parentheses and spaces
                 "5551234567",        // Digits only
                 "+1 555 123 4567",   // With plus and spaces
                 "555.123.4567"       // With dots
             };
            
             // Phone number equality is based on digits
             var phone1 = PhoneNumber.Create("(555) 123-4567").Value;
             var phone2 = PhoneNumber.Create("555-123-4567").Value;
             bool areEqual = phone1 == phone2;  // true (both have same digits: "5551234567")
```

### ValueObject

```csharp
Derived classes follow this pattern:
             
             public sealed class EmailAddress : ValueObject
             {
                 private EmailAddress(string value) => Value = value;
            
                 public string Value { get; }
            
                 public static Result<EmailAddress> Create(string? email)
                 {
                     if (string.IsNullOrWhiteSpace(email))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_EMPTY", "Email cannot be empty"));
            
                     var trimmed = email.Trim().ToLowerInvariant();
            
                     if (!IsValidFormat(trimmed))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_INVALID", "Email format is invalid"));
            
                     return Result.Ok<EmailAddress>(new EmailAddress(trimmed));
                 }
            
                 protected override IEnumerable<object?> GetAtomicValues()
                 {
                     yield return Value;
                 }
             }
```

### Address

```csharp
var addressResult = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA");
            
             if (addressResult.IsSuccess)
             {
                 var address = addressResult.Value;
                 Console.WriteLine(address.FullAddress);  // "123 Main St, Springfield, IL 62701, USA"
                 order.ShippingAddress = address;
             }
             else
             {
                 // Handle validation error (e.g., STREET_EMPTY, CITY_EMPTY, etc.)
                 logger.LogError(addressResult.Error.Message);
             }
            
             // Address equality is value-based
             var address1 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             var address2 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Value;
             bool areEqual = address1 == address2;  // true (same address values)
```

### EmailAddress

```csharp
var emailResult = EmailAddress.Create("customer@example.com");
            
             if (emailResult.IsSuccess)
             {
                 var email = emailResult.Value;
                 Console.WriteLine(email.Value);  // "customer@example.com"
                 customer.Email = email;
             }
             else
             {
                 // Handle validation error
                 switch (emailResult.Error.Code)
                 {
                     case "EMAIL_EMPTY":
                         logger.LogError("Email address is required");
                         break;
                     case "EMAIL_INVALID":
                         logger.LogError("Email format is invalid");
                         break;
                     case "EMAIL_TOO_LONG":
                         logger.LogError("Email must not exceed 256 characters");
                         break;
                 }
             }
            
             // Email equality is case-insensitive (normalized during creation)
             var email1 = EmailAddress.Create("John@Example.COM").Value;
             var email2 = EmailAddress.Create("john@example.com").Value;
             bool areEqual = email1 == email2;  // true (both stored as "john@example.com")
```

### Money

```csharp
// Creating a money value
             var priceResult = Money.Create(99.99m, "USD");
             if (priceResult.IsSuccess)
             {
                 order.Total = priceResult.Value;  // 99.99 USD
             }
             else
             {
                 logger.LogError(priceResult.Error.Message);  // "Amount cannot be negative"
             }
            
             // Performing arithmetic operations
             var cost = Money.Create(50.00m, "USD").Value;
             var tax = Money.Create(5.00m, "USD").Value;
             var totalResult = cost.Add(tax);
            
             if (totalResult.IsSuccess)
             {
                 order.Total = totalResult.Value;  // 55.00 USD
             }
             else
             {
                 logger.LogError(totalResult.Error.Message);  // Possible: "Cannot add money with different currencies"
             }
            
             // Attempting to mix currencies fails safely
             var usd = Money.Create(100m, "USD").Value;
             var eur = Money.Create(100m, "EUR").Value;
             var mixResult = usd.Add(eur);  // IsSuccess = false, Error.Code = "CURRENCY_MISMATCH"
```

### PersonName

```csharp
// Creating a name without middle name
             var nameResult = PersonName.Create("John", "Doe");
             if (nameResult.IsSuccess)
             {
                 customer.Name = nameResult.Value;  // "John Doe"
             }
            
             // Creating a name with middle name
             var fullNameResult = PersonName.Create("John", "Doe", "Michael");
             if (fullNameResult.IsSuccess)
             {
                 customer.Name = fullNameResult.Value;  // "John Michael Doe"
             }
             else
             {
                 logger.LogError(fullNameResult.Error.Message);  // "First name cannot be empty"
             }
            
             // Accessing name components
             var name = PersonName.Create("Jane", "Smith", "Marie").Value;
             Console.WriteLine(name.FirstName);   // "Jane"
             Console.WriteLine(name.MiddleName);  // "Marie"
             Console.WriteLine(name.LastName);    // "Smith"
             Console.WriteLine(name.FullName);    // "Jane Marie Smith"
            
             // Name equality is value-based
             var name1 = PersonName.Create("John", "Doe").Value;
             var name2 = PersonName.Create("John", "Doe").Value;
             bool areEqual = name1 == name2;  // true (same name values)
```

### PhoneNumber

```csharp
// Creating a phone number with various formats
             var phoneResult = PhoneNumber.Create("(555) 123-4567");  // Accepts any format
             if (phoneResult.IsSuccess)
             {
                 customer.Phone = phoneResult.Value;  // Stores as "5551234567"
                 Console.WriteLine(phoneResult.Value.FormattedNumber);  // "5551234567"
             }
             else
             {
                 logger.LogError(phoneResult.Error.Message);  // Possible: "Phone number must contain at least 10 digits"
             }
            
             // More format examples
             var validFormats = new[]
             {
                 "555-123-4567",      // With hyphens
                 "(555) 123-4567",    // With parentheses and spaces
                 "5551234567",        // Digits only
                 "+1 555 123 4567",   // With plus and spaces
                 "555.123.4567"       // With dots
             };
            
             // Phone number equality is based on digits
             var phone1 = PhoneNumber.Create("(555) 123-4567").Value;
             var phone2 = PhoneNumber.Create("555-123-4567").Value;
             bool areEqual = phone1 == phone2;  // true (both have same digits: "5551234567")
```

### ValueObject

```csharp
Derived classes follow this pattern:
             
             public sealed class EmailAddress : ValueObject
             {
                 private EmailAddress(string value) => Value = value;
            
                 public string Value { get; }
            
                 public static Result<EmailAddress> Create(string? email)
                 {
                     if (string.IsNullOrWhiteSpace(email))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_EMPTY", "Email cannot be empty"));
            
                     var trimmed = email.Trim().ToLowerInvariant();
            
                     if (!IsValidFormat(trimmed))
                         return Result.Fail<EmailAddress>(new Error("EMAIL_INVALID", "Email format is invalid"));
            
                     return Result.Ok<EmailAddress>(new EmailAddress(trimmed));
                 }
            
                 protected override IEnumerable<object?> GetAtomicValues()
                 {
                     yield return Value;
                 }
             }
```

## API Reference

See [API Reference](./api-reference.md) for complete documentation.

