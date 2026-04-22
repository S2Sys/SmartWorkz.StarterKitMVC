# Value Objects: Domain-Driven Design Deep Dive

## 1. What are Value Objects?

A **Value Object** is an immutable object in the domain model that is compared by its values rather than by identity. Two value objects with identical values are considered equal, regardless of whether they are the same object instance in memory.

### Definition

Value objects represent small, self-contained domain concepts that:
- Have no unique identity (no Id property)
- Are immutable (cannot be changed after creation)
- Are compared by their values
- Are atomic (represent a single, indivisible domain concept)

### Examples

| Value Object | Represents | Atomic Values |
|--------------|-----------|---------------|
| **Money** | Monetary amount with currency | Amount + Currency |
| **EmailAddress** | Email contact point | Email string |
| **PersonName** | Person's full name | FirstName + LastName + MiddleName |
| **Address** | Physical mailing location | Street + City + State + PostalCode + Country |
| **PhoneNumber** | Telephone contact number | Digits only (normalized) |

### Value Object Equality

```csharp
// Two Money instances with same values are equal
var price1 = Money.Create(99.99m, "USD").Value;
var price2 = Money.Create(99.99m, "USD").Value;
bool equal = price1 == price2;  // true (same values)

// Different instances are still equal if values match
var money1 = Money.Create(50.0m, "EUR").Value;
var money2 = Money.Create(50.0m, "EUR").Value;
bool same = ReferenceEquals(money1, money2);  // false (different instances)
bool equals = money1 == money2;  // true (same values, equality by value)

// Same currency is required for equality
var usd = Money.Create(100m, "USD").Value;
var eur = Money.Create(100m, "EUR").Value;
bool currencyMatters = usd == eur;  // false (different currencies)
```

---

## 2. Why Use Value Objects?

### Type Safety

Without value objects, critical domain concepts become strings or numbers, losing semantic meaning:

```csharp
// Anti-pattern: Using primitives (loses domain meaning)
public class Customer
{
    public string Email { get; set; }  // Could be invalid format
    public decimal Price { get; set; }  // Currency unknown
    public string PhoneNumber { get; set; }  // Format unknown
}

// Validation must happen everywhere
if (!EmailRegex.IsMatch(customer.Email))
    throw new ArgumentException("Invalid email");

// Problem: Email could be invalid and entity is in an inconsistent state

// Solution: Use value objects for type safety
public class Customer
{
    public EmailAddress Email { get; set; }  // Guaranteed valid
    public Money Price { get; set; }  // Amount + Currency together
    public PhoneNumber Phone { get; set; }  // Guaranteed valid format
}

// Email is validated during creation, never invalid
var emailResult = EmailAddress.Create(input);
if (emailResult.IsSuccess)
    customer.Email = emailResult.Value;  // Always valid
```

### Domain Modeling

Value objects express domain concepts more explicitly than primitives:

```csharp
// Without value objects (loses domain meaning)
var orderTotal = 99.99m;  // What is this? Currency? Precision?
var itemPrice = "99.99";  // String? Why?
var customerName = "John Smith";  // FirstName? LastName? Both?

// With value objects (domain-clear)
var orderTotal = Money.Create(99.99m, "USD").Value;  // Amount + Currency together
var itemPrice = Money.Create(price, currency).Value;  // Monetary value
var customerName = PersonName.Create("John", "Smith").Value;  // First + Last together
```

### Validation Encapsulation

Validation is centralized in the value object, not scattered throughout the codebase:

```csharp
// Without value objects (validation scattered)
public class UserService
{
    public async Task<Result<UserDto>> RegisterAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail("Email required");
        if (!EmailRegex.IsMatch(email))
            return Result.Fail("Email invalid");
        if (email.Length > 256)
            return Result.Fail("Email too long");
        
        // ... more validation ...
    }
}

public class UserValidator
{
    public bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        if (!EmailRegex.IsMatch(email))
            return false;
        if (email.Length > 256)
            return false;
        return true;
    }
}

// Problem: Validation logic is duplicated in multiple places

// Solution: Value object encapsulates validation
public sealed class EmailAddress : ValueObject
{
    public static Result<EmailAddress> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail<EmailAddress>(
                new Error("EMAIL_EMPTY", "Email cannot be empty"));
        
        var trimmed = email.Trim().ToLowerInvariant();
        
        if (!Regex.IsMatch(trimmed, EmailPattern))
            return Result.Fail<EmailAddress>(
                new Error("EMAIL_INVALID", "Email format is invalid"));
        
        if (trimmed.Length > 256)
            return Result.Fail<EmailAddress>(
                new Error("EMAIL_TOO_LONG", "Email exceeds 256 characters"));
        
        return Result.Ok(new EmailAddress(trimmed));
    }
}

// Validation happens once, when value object is created
var emailResult = EmailAddress.Create(userInput);
if (emailResult.IsSuccess)
    customer.Email = emailResult.Value;  // Always valid
```

### Self-Documenting Code

Value objects make code intent explicit:

```csharp
// Without value objects (ambiguous)
public class Order
{
    public decimal Amount { get; set; }  // USD? EUR? Cents?
    public string ShippingAddress { get; set; }  // Format unknown
    public string PhoneNumber { get; set; }  // International? Domestic?
}

// With value objects (intent clear)
public class Order
{
    public Money Total { get; set; }  // Amount + Currency together
    public Address ShippingAddress { get; set; }  // Structure defined
    public PhoneNumber ContactPhone { get; set; }  // Digits only, validated
}
```

---

## 3. Immutability Pattern

### Private Constructor, Public Readonly Properties

Value objects use a private constructor and expose properties as readonly:

```csharp
public sealed class Money : ValueObject
{
    // Private constructor: clients cannot instantiate directly
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    // Public readonly properties: cannot be changed after creation
    public decimal Amount { get; }
    public string Currency { get; }
    
    // Factory method: only way to create instances
    public static Result<Money> Create(decimal amount, string? currency)
    {
        // Validation logic
        // ...
        return Result.Ok(new Money(amount, currencyCode));
    }
}

// Usage: Cannot create directly
// var money = new Money(100, "USD");  // Compile error: private constructor

// Must use factory method
var moneyResult = Money.Create(100, "USD");  // Correct

// Cannot modify after creation
// money.Amount = 200;  // Compile error: property has no setter
```

### Why Immutability Matters

1. **Thread-safe:** Immutable objects can be safely shared across threads
2. **Hashable:** Can be used as dictionary keys without issues
3. **Predictable:** Value never unexpectedly changes
4. **Equality:** Immutability ensures equality comparisons are stable

```csharp
// Immutability enables safe dictionary usage
var pricesByProduct = new Dictionary<string, Money>();
var price1 = Money.Create(99.99m, "USD").Value;
pricesByProduct["Laptop"] = price1;

// Key is immutable, so no unexpected hash collisions
var price2 = Money.Create(99.99m, "USD").Value;
bool found = pricesByProduct.TryGetValue("Laptop", out var price);
// Works reliably because Money is immutable

// If Money were mutable:
// price1.Amount = 200;  // Hash code changes, dictionary lookup fails!
```

---

## 4. Validation Pattern

### Create() Factory Method Returns Result<T>

Value objects use a static `Create()` factory method that returns `Result<ValueObject>`. This allows validation to fail gracefully without throwing exceptions:

```csharp
public sealed class EmailAddress : ValueObject
{
    private EmailAddress(string value) => Value = value;
    
    public string Value { get; }
    
    // Factory method: only way to create instances
    public static Result<EmailAddress> Create(string? email)
    {
        // Validation check 1: Not empty
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail<EmailAddress>(
                new Error("EMAIL_EMPTY", "Email address cannot be empty"));
        
        var trimmed = email.Trim().ToLowerInvariant();
        
        // Validation check 2: Format
        if (!Regex.IsMatch(trimmed, EmailPattern))
            return Result.Fail<EmailAddress>(
                new Error("EMAIL_INVALID", "Email address format is invalid"));
        
        // Validation check 3: Length
        if (trimmed.Length > 256)
            return Result.Fail<EmailAddress>(
                new Error("EMAIL_TOO_LONG", "Email address must not exceed 256 characters"));
        
        // All validations passed
        return Result.Ok(new EmailAddress(trimmed));
    }
}
```

### Validation Errors as Error Objects

Each validation error has a code and message for clarity:

```csharp
// Error codes should be specific and actionable
public record Error(string Code, string Message);

// Example validation errors
var invalidEmail = Result.Fail<EmailAddress>(
    new Error("EMAIL_EMPTY", "Email address cannot be empty"));

var invalidFormat = Result.Fail<EmailAddress>(
    new Error("EMAIL_INVALID", "Email address format is invalid"));

var tooLong = Result.Fail<EmailAddress>(
    new Error("EMAIL_TOO_LONG", "Email address must not exceed 256 characters"));
```

### Example: Validation in Create() Method

```csharp
var emailResult = EmailAddress.Create("invalid");
if (emailResult.IsSuccess)
{
    // Email is valid, use it
    customer.Email = emailResult.Value;
}
else
{
    // Email is invalid, handle error
    switch (emailResult.Error.Code)
    {
        case "EMAIL_EMPTY":
            logger.LogWarning("Email is required");
            break;
        case "EMAIL_INVALID":
            logger.LogWarning("Email format is invalid");
            break;
        case "EMAIL_TOO_LONG":
            logger.LogWarning("Email exceeds 256 characters");
            break;
    }
}
```

---

## 5. Equality Pattern

### ValueObject Base Class Implements Equality

SmartWorkz.Core provides a `ValueObject` base class that implements equality by comparing atomic values:

```csharp
public abstract class ValueObject
{
    // Abstract method: derived classes define which properties define equality
    protected abstract IEnumerable<object?> GetAtomicValues();
    
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;
        
        var otherValueObject = (ValueObject)obj;
        
        // Compare atomic values
        return GetAtomicValues()
            .SequenceEqual(otherValueObject.GetAtomicValues());
    }
    
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    
    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !(left == right);
}
```

### Override GetAtomicValues() or GetEqualityComponents()

Derived classes define which properties are compared:

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    // Define atomic values for equality
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;  // Amount is part of identity
        yield return Currency;  // Currency is part of identity
    }
}

// Usage: Two Money instances with same values are equal
var money1 = Money.Create(99.99m, "USD").Value;
var money2 = Money.Create(99.99m, "USD").Value;
bool equal = money1 == money2;  // true (same Amount + Currency)

var money3 = Money.Create(99.99m, "EUR").Value;
bool notEqual = money1 == money3;  // false (different Currency)
```

### Example: PersonName Equality

```csharp
public sealed class PersonName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string? MiddleName { get; }
    
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return FirstName;
        yield return MiddleName ?? string.Empty;  // Normalize null to empty
        yield return LastName;
    }
}

// Usage: Two persons with same name are equal
var name1 = PersonName.Create("John", "Doe").Value;
var name2 = PersonName.Create("John", "Doe").Value;
bool equal = name1 == name2;  // true

// Middle name matters
var name3 = PersonName.Create("John", "Doe", "Michael").Value;
bool notEqual = name1 == name3;  // false (name1 has no middle name)
```

---

## 6. ToString() Pattern

### User-Friendly String Representation

Value objects override `ToString()` to provide user-friendly representations:

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public override string ToString() => $"{Amount:F2} {Currency}";
}

// Usage in logging and error messages
var price = Money.Create(99.99m, "USD").Value;
Console.WriteLine(price);  // "99.99 USD"
logger.LogInformation($"Product price: {price}");  // "Product price: 99.99 USD"

public sealed class PersonName : ValueObject
{
    public override string ToString() => FullName;  // "John Doe" or "John Michael Doe"
}

public sealed class Address : ValueObject
{
    public override string ToString() => FullAddress;  // "123 Main St, Springfield, IL 62701, USA"
}
```

---

## 7. Value Object Examples in SmartWorkz.Core

### Money

Represents monetary amounts with currency validation.

```csharp
// Create a money value
var priceResult = Money.Create(99.99m, "USD");
if (priceResult.IsSuccess)
{
    var price = priceResult.Value;
    Console.WriteLine(price.Amount);  // 99.99
    Console.WriteLine(price.Currency);  // "USD"
    Console.WriteLine(price.ToString());  // "99.99 USD"
}

// Arithmetic operations with currency validation
var cost = Money.Create(50.00m, "USD").Value;
var tax = Money.Create(5.00m, "USD").Value;
var totalResult = cost.Add(tax);
if (totalResult.IsSuccess)
    Console.WriteLine(totalResult.Value);  // "55.00 USD"

// Currency mismatch fails gracefully
var eur = Money.Create(100m, "EUR").Value;
var mixResult = cost.Add(eur);
// mixResult.IsSuccess = false
// mixResult.Error.Code = "CURRENCY_MISMATCH"
```

### EmailAddress

Represents valid email addresses with format validation.

```csharp
// Create an email with validation
var emailResult = EmailAddress.Create("john@example.com");
if (emailResult.IsSuccess)
{
    var email = emailResult.Value;
    Console.WriteLine(email.Value);  // "john@example.com"
    
    // Email is normalized to lowercase
    var email2 = EmailAddress.Create("JOHN@EXAMPLE.COM").Value;
    bool equal = email == email2;  // true (both "john@example.com")
}

// Invalid email is caught
var invalidResult = EmailAddress.Create("not-an-email");
// invalidResult.IsSuccess = false
// invalidResult.Error.Code = "EMAIL_INVALID"
```

### PersonName

Represents person names with optional middle name.

```csharp
// Create a simple name
var nameResult = PersonName.Create("John", "Doe");
if (nameResult.IsSuccess)
{
    var name = nameResult.Value;
    Console.WriteLine(name.FirstName);  // "John"
    Console.WriteLine(name.LastName);  // "Doe"
    Console.WriteLine(name.MiddleName);  // null
    Console.WriteLine(name.FullName);  // "John Doe"
}

// Create with middle name
var fullNameResult = PersonName.Create("Jane", "Doe", "Marie");
if (fullNameResult.IsSuccess)
{
    var name = fullNameResult.Value;
    Console.WriteLine(name.FullName);  // "Jane Marie Doe"
}
```

### Address

Represents physical mailing addresses.

```csharp
// Create an address
var addressResult = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA");
if (addressResult.IsSuccess)
{
    var address = addressResult.Value;
    Console.WriteLine(address.Street);  // "123 Main St"
    Console.WriteLine(address.City);  // "Springfield"
    Console.WriteLine(address.FullAddress);  // "123 Main St, Springfield, IL 62701, USA"
}

// All components are required
var invalidResult = Address.Create("", "Springfield", "IL", "62701", "USA");
// invalidResult.IsSuccess = false
// invalidResult.Error.Code = "STREET_EMPTY"
```

### PhoneNumber

Represents phone numbers with digit validation.

```csharp
// Accepts multiple formats, normalizes to digits
var phoneResult = PhoneNumber.Create("(555) 123-4567");
if (phoneResult.IsSuccess)
{
    var phone = phoneResult.Value;
    Console.WriteLine(phone.FormattedNumber);  // "5551234567"
}

// Different formats are equal if digits match
var phone1 = PhoneNumber.Create("(555) 123-4567").Value;
var phone2 = PhoneNumber.Create("555-123-4567").Value;
bool equal = phone1 == phone2;  // true (same digits)

// Invalid length is caught
var tooShort = PhoneNumber.Create("123");  // Too few digits
// tooShort.IsSuccess = false
// tooShort.Error.Code = "PHONE_TOO_SHORT"
```

---

## 8. Value Object vs Entity

| Aspect | Value Object | Entity |
|--------|-------------|--------|
| **Equality** | By value (all components) | By ID |
| **Identity** | No unique identifier | Unique ID property |
| **Mutable** | Immutable (always) | Usually mutable |
| **Lifecycle** | Created, used, discarded | Created, updated, deleted |
| **Comparison** | Two Money(100, "USD") are equal | Two User(1) and User(2) are different |
| **Example** | Money, EmailAddress, Address | User, Order, Product |

### Entity Example

```csharp
public class Customer : AuditableEntity<int>
{
    public int Id { get; set; }  // Identity
    public EmailAddress Email { get; set; }  // Value object
    public PersonName Name { get; set; }  // Value object
    public Address BillingAddress { get; set; }  // Value object
}

// Customers are compared by ID (entity)
var customer1 = new Customer { Id = 1, Email = "john@example.com", Name = "John Doe" };
var customer2 = new Customer { Id = 2, Email = "john@example.com", Name = "John Doe" };
bool equal = customer1 == customer2;  // false (different IDs, different entities)

// But if we compare value objects inside
bool sameEmail = customer1.Email == customer2.Email;  // true (same email value)
bool sameName = customer1.Name == customer2.Name;  // true (same name value)
```

---

## 9. Factory Method Pattern

### Static Create() Method

Value objects use a static `Create()` factory method as the only way to instantiate them:

```csharp
public sealed class PhoneNumber : ValueObject
{
    // Private constructor: clients cannot instantiate directly
    private PhoneNumber(string formattedNumber)
    {
        FormattedNumber = formattedNumber;
    }
    
    public string FormattedNumber { get; }
    
    // Factory method: validates and creates instance
    public static Result<PhoneNumber> Create(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Result.Fail<PhoneNumber>(
                new Error("PHONE_EMPTY", "Phone number cannot be empty"));
        
        var digitsOnly = Regex.Replace(phone, @"\D", "");
        
        if (digitsOnly.Length < 10)
            return Result.Fail<PhoneNumber>(
                new Error("PHONE_TOO_SHORT", "Phone number must contain at least 10 digits"));
        
        if (digitsOnly.Length > 15)
            return Result.Fail<PhoneNumber>(
                new Error("PHONE_TOO_LONG", "Phone number must not exceed 15 digits"));
        
        return Result.Ok(new PhoneNumber(digitsOnly));
    }
}

// Usage
var phoneResult = PhoneNumber.Create("+1 (555) 123-4567");
if (phoneResult.IsSuccess)
    customer.Phone = phoneResult.Value;
```

---

## 10. Integration with Entities

### Entities Contain Value Objects

Entities reference value objects to represent domain concepts:

```csharp
public class Customer : AuditableEntity<int>
{
    public EmailAddress Email { get; set; }  // Value object
    public PersonName Name { get; set; }  // Value object
    public Address BillingAddress { get; set; }  // Value object
    public Address ShippingAddress { get; set; }  // Value object
    public PhoneNumber Phone { get; set; }  // Value object
}

// Type safety: Can only assign EmailAddress to Email property
// customer.Email = "invalid@email";  // Compile error
// customer.Email = EmailAddress.Create("invalid@email").Value;  // Runtime check

// Value objects are compared by value
var customer1 = new Customer { Email = EmailAddress.Create("john@example.com").Value };
var customer2 = new Customer { Email = EmailAddress.Create("john@example.com").Value };
bool sameEmail = customer1.Email == customer2.Email;  // true
```

---

## 11. Database Persistence

### EF Core Value Object Ownership

Entity Framework Core supports value object ownership via `.OwnsOne()` or `[Owned]` attribute:

```csharp
// Option 1: Fluent API configuration
modelBuilder.Entity<Customer>()
    .OwnsOne(c => c.Email);  // Email columns stored in Customer table
    
modelBuilder.Entity<Customer>()
    .OwnsOne(c => c.BillingAddress);  // Address columns: BillingAddress_Street, BillingAddress_City, etc.

// Option 2: Attribute configuration
public class Customer : AuditableEntity<int>
{
    [Owned]
    public EmailAddress Email { get; set; }
    
    [Owned]
    public PersonName Name { get; set; }
}

// Result: Customer table has columns:
// - Id, TenantId, IsDeleted, ...
// - Email (from EmailAddress.Value)
// - Name_FirstName, Name_LastName, Name_MiddleName (from PersonName)
// - BillingAddress_Street, BillingAddress_City, ... (from Address)
```

### Serialization for NoSQL

For NoSQL databases, value objects can be serialized to JSON:

```csharp
// MongoDB example
public class Order
{
    public ObjectId Id { get; set; }
    public Money Total { get; set; }  // Serialized as JSON
    public Address ShippingAddress { get; set; }  // Serialized as JSON
}

// In database:
// {
//   "_id": ObjectId("..."),
//   "total": { "amount": 99.99, "currency": "USD" },
//   "shippingAddress": {
//     "street": "123 Main St",
//     "city": "Springfield",
//     ...
//   }
// }
```

---

## 12. API Contracts

### DTOs Map Entity Value Objects to JSON

DTOs flatten value objects into primitive types for JSON serialization:

```csharp
// Entity with value objects
public class Customer : AuditableEntity<int>
{
    public EmailAddress Email { get; set; }
    public PersonName Name { get; set; }
    public Address BillingAddress { get; set; }
}

// DTO flattens value objects
public class CustomerDto
{
    public int Id { get; set; }
    public string Email { get; set; }  // EmailAddress.Value
    public string FirstName { get; set; }  // PersonName.FirstName
    public string LastName { get; set; }  // PersonName.LastName
    public string? MiddleName { get; set; }  // PersonName.MiddleName
    public string Street { get; set; }  // Address.Street
    public string City { get; set; }  // Address.City
    public string State { get; set; }  // Address.State
    public string PostalCode { get; set; }  // Address.PostalCode
    public string Country { get; set; }  // Address.Country
}

// Service maps entity to DTO
protected override CustomerDto Map(Customer entity) =>
    new CustomerDto
    {
        Id = entity.Id,
        Email = entity.Email.Value,
        FirstName = entity.Name.FirstName,
        LastName = entity.Name.LastName,
        MiddleName = entity.Name.MiddleName,
        Street = entity.BillingAddress.Street,
        City = entity.BillingAddress.City,
        State = entity.BillingAddress.State,
        PostalCode = entity.BillingAddress.PostalCode,
        Country = entity.BillingAddress.Country
    };
```

### Reconstruction from DTO

When receiving a DTO, reconstruct value objects:

```csharp
// Service maps DTO to entity
protected override Customer MapToEntity(CustomerDto dto)
{
    // Reconstruct value objects from DTO
    var emailResult = EmailAddress.Create(dto.Email);
    if (!emailResult.IsSuccess)
        throw new ArgumentException(emailResult.Error.Message);
    
    var nameResult = PersonName.Create(dto.FirstName, dto.LastName, dto.MiddleName);
    if (!nameResult.IsSuccess)
        throw new ArgumentException(nameResult.Error.Message);
    
    var addressResult = Address.Create(
        dto.Street, dto.City, dto.State, dto.PostalCode, dto.Country);
    if (!addressResult.IsSuccess)
        throw new ArgumentException(addressResult.Error.Message);
    
    return new Customer
    {
        Email = emailResult.Value,
        Name = nameResult.Value,
        BillingAddress = addressResult.Value
    };
}
```

---

## 13. Error Handling with Value Objects

### Create() Returns Result<T>

Value object creation returns `Result<T>` to handle validation errors gracefully:

```csharp
public async Task<Result<CustomerDto>> RegisterAsync(RegisterDto dto)
{
    // Validate email
    var emailResult = EmailAddress.Create(dto.Email);
    if (!emailResult.IsSuccess)
        return Result.Fail<CustomerDto>(emailResult.Error);
    
    // Validate name
    var nameResult = PersonName.Create(dto.FirstName, dto.LastName);
    if (!nameResult.IsSuccess)
        return Result.Fail<CustomerDto>(nameResult.Error);
    
    // Validate address
    var addressResult = Address.Create(
        dto.Street, dto.City, dto.State, dto.PostalCode, dto.Country);
    if (!addressResult.IsSuccess)
        return Result.Fail<CustomerDto>(addressResult.Error);
    
    // All value objects created successfully
    var customer = new Customer
    {
        Email = emailResult.Value,
        Name = nameResult.Value,
        BillingAddress = addressResult.Value
    };
    
    await repository.AddAsync(customer);
    return Result.Ok(Map(customer));
}

// Usage with error handling
var result = await customerService.RegisterAsync(registerDto);
if (result.IsSuccess)
    return CreatedAtAction("GetCustomer", new { id = result.Value.Id });

// Graceful error handling
return result.Error.Code switch
{
    "EMAIL_INVALID" => BadRequest(new { error = result.Error.Message }),
    "EMAIL_TOO_LONG" => BadRequest(new { error = result.Error.Message }),
    "FIRST_NAME_EMPTY" => BadRequest(new { error = result.Error.Message }),
    "LAST_NAME_EMPTY" => BadRequest(new { error = result.Error.Message }),
    "STREET_EMPTY" => BadRequest(new { error = result.Error.Message }),
    _ => StatusCode(500, new { error = "An error occurred" })
};
```

---

## 14. Best Practices

### Always Immutable

Value objects must never be mutable:

```csharp
// Good: Immutable value object
public sealed class Money : ValueObject
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public decimal Amount { get; }  // Read-only property
    public string Currency { get; }
    
    public static Result<Money> Create(decimal amount, string currency) => /* ... */;
}

// Bad: Mutable value object (breaks equality)
public class BadMoney
{
    public decimal Amount { get; set; }  // WRONG: Has setter
    public string Currency { get; set; }
}

// Problem with mutability:
var price1 = new BadMoney { Amount = 100m, Currency = "USD" };
var price2 = new BadMoney { Amount = 100m, Currency = "USD" };
var dict = new Dictionary<BadMoney, string> { { price1, "laptop" } };

// Hash code changes when mutated
price1.Amount = 200;  // Hash code changed
dict.TryGetValue(price1, out var value);  // Lookup fails!
```

### Always Validate in Create()

All validation logic belongs in the factory method:

```csharp
// Good: Validation in Create()
public static Result<Money> Create(decimal amount, string? currency)
{
    if (amount < 0)
        return Result.Fail<Money>(new Error("NEGATIVE", "Amount cannot be negative"));
    
    if (string.IsNullOrWhiteSpace(currency))
        return Result.Fail<Money>(new Error("EMPTY_CURRENCY", "Currency is required"));
    
    // ... more validation
    
    return Result.Ok(new Money(amount, currency.ToUpperInvariant()));
}

// Bad: Validation outside Create() (scattered logic)
var money = new Money(amount, currency);
if (money.Amount < 0)
    throw new ArgumentException("Amount cannot be negative");
if (string.IsNullOrWhiteSpace(money.Currency))
    throw new ArgumentException("Currency is required");
```

### Always Use Value Objects for Domain Concepts

Don't use primitives for important domain concepts:

```csharp
// Good: Use value objects for domain concepts
public class Customer : AuditableEntity<int>
{
    public EmailAddress Email { get; set; }  // Type-safe
    public PersonName Name { get; set; }  // Type-safe
    public Money CreditLimit { get; set; }  // Type-safe
}

// Bad: Use primitives (loses meaning)
public class Customer : AuditableEntity<int>
{
    public string Email { get; set; }  // Could be invalid
    public string Name { get; set; }  // Ambiguous (first? last? both?)
    public decimal CreditLimit { get; set; }  // Currency unknown
}
```

### Never Expose Setters

Value object properties should be read-only:

```csharp
// Good: Read-only properties
public sealed class EmailAddress : ValueObject
{
    public string Value { get; }  // No setter
}

// Bad: Writable properties (breaks immutability)
public sealed class BadEmail : ValueObject
{
    public string Value { get; set; }  // WRONG: Can be modified
}

// Problem: Once created as valid, could be set to invalid value
var email = EmailAddress.Create("john@example.com").Value;
email.Value = "invalid";  // Compile error with good design
```

### Never Use Null (Use Value Semantics or Nullable)

Value objects should either always be non-null or use nullable types:

```csharp
// Good: Required value objects
public class Customer
{
    public EmailAddress Email { get; set; }  // Cannot be null
    public PersonName Name { get; set; }  // Cannot be null
    
    // Constructor ensures non-null
    public Customer(EmailAddress email, PersonName name)
    {
        Email = Guard.NotNull(email, nameof(email));
        Name = Guard.NotNull(name, nameof(name));
    }
}

// Good: Optional value objects use nullable
public class Customer
{
    public PhoneNumber? Phone { get; set; }  // Nullable, allowed to be null
    public Address? ShippingAddress { get; set; }  // Nullable
}

// Bad: Null checks everywhere
public class BadCustomer
{
    public string? Email { get; set; }  // Could be null, invalid, or both
    
    // Must check null everywhere
    if (customer.Email != null && EmailValidator.Validate(customer.Email))
    {
        // Use email
    }
}

// Good approach: Value object guarantees validity
var email = EmailAddress.Create(input);
if (email.IsSuccess)
    customer.Email = email.Value;  // Cannot be invalid or null
```

### Make Atomic (Single Concept)

Value objects should represent a single, indivisible domain concept:

```csharp
// Good: Atomic value objects
public sealed class Money : ValueObject
{
    // Amount + Currency together form one concept
    public decimal Amount { get; }
    public string Currency { get; }
}

// Good: Each value object is atomic
public sealed class PersonName : ValueObject
{
    public string FirstName { get; }  // Part of name concept
    public string LastName { get; }
    public string? MiddleName { get; }
}

// Bad: Mixing unrelated concepts (not atomic)
public sealed class BadValueObject : ValueObject
{
    public string FirstName { get; }  // Person name
    public string Email { get; }  // Email (different concept)
    public string PhoneNumber { get; }  // Phone (different concept)
    // Violates Single Responsibility Principle
}
```

---

## 15. Anti-Patterns

### Anti-Pattern 1: Mutable Value Objects

```csharp
// BAD: Mutable value object
public class BadMoney
{
    public decimal Amount { get; set; }  // Mutable
    public string Currency { get; set; }  // Mutable
    
    public override bool Equals(object? obj) =>
        obj is BadMoney m && m.Amount == Amount && m.Currency == Currency;
}

// Problem: Hash code becomes invalid when mutated
var price = new BadMoney { Amount = 100m, Currency = "USD" };
var dict = new Dictionary<BadMoney, string> { { price, "item" } };

price.Amount = 200;  // Hash code changed
dict.ContainsKey(price);  // False! (lookup fails)
```

### Anti-Pattern 2: Validation Outside Create()

```csharp
// BAD: Validation scattered everywhere
public class BadEmail
{
    public string Value { get; }
    
    public BadEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email required");
        // No format validation
        Value = value;
    }
}

public class UserService
{
    public void Register(BadEmail email)
    {
        if (!EmailValidator.IsValid(email.Value))  // Duplicate validation
            throw new ArgumentException("Invalid email");
    }
}

// Problem: Validation logic is duplicated, hard to maintain

// GOOD: Validation in factory method
public sealed class EmailAddress : ValueObject
{
    public static Result<EmailAddress> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail<EmailAddress>(
                new Error("EMAIL_EMPTY", "Email is required"));
        
        if (!Regex.IsMatch(email, EmailPattern))
            return Result.Fail<EmailAddress>(
                new Error("EMAIL_INVALID", "Invalid email format"));
        
        return Result.Ok(new EmailAddress(email.ToLowerInvariant()));
    }
}
```

### Anti-Pattern 3: Using Value Objects for Unrelated Fields

```csharp
// BAD: Value object mixing unrelated concepts
public sealed class BadConcept : ValueObject
{
    public string FirstName { get; }  // Person name
    public string Email { get; }  // Email address
    public string PhoneNumber { get; }  // Phone
    // These are separate domain concepts, not atomic
}

// GOOD: Separate value objects for each concept
public sealed class PersonName : ValueObject { /* ... */ }
public sealed class EmailAddress : ValueObject { /* ... */ }
public sealed class PhoneNumber : ValueObject { /* ... */ }

public class Customer
{
    public PersonName Name { get; set; }  // Clear intent
    public EmailAddress Email { get; set; }  // Clear intent
    public PhoneNumber Phone { get; set; }  // Clear intent
}
```

### Anti-Pattern 4: Allowing Null Without Explicit Intent

```csharp
// BAD: Null checks everywhere
public class BadCustomer
{
    public string? Email { get; set; }  // Nullable string
    
    public void SendEmail()
    {
        if (Email != null && EmailValidator.Validate(Email))
        {
            // Send email
        }
    }
}

// GOOD: Value object guarantees validity or explicit nullable
public class Customer
{
    public EmailAddress Email { get; set; }  // Cannot be null or invalid
    
    public void SendEmail()
    {
        // No null checks needed
        SendEmailTo(Email.Value);
    }
}

public class CustomerWithOptionalEmail
{
    public EmailAddress? Email { get; set; }  // Explicitly nullable
    
    public void SendEmailIfAvailable()
    {
        if (Email != null)  // Explicit intent: email is optional
            SendEmailTo(Email.Value);
    }
}
```

---

## Summary

Value objects are a cornerstone of Domain-Driven Design. They provide:

- **Type Safety:** Domain concepts cannot be misused
- **Immutability:** Thread-safe, hashable, and predictable
- **Validation Encapsulation:** All validation in one place
- **Self-Documenting Code:** Clear domain intent
- **Reusability:** Same validation logic used everywhere

SmartWorkz.Core provides a robust foundation for building value objects with the `ValueObject` base class and the `Result` pattern for graceful error handling.

Use value objects for all important domain concepts, keep them immutable, validate in the factory method, and never expose setters. This ensures your domain model is both powerful and easy to maintain.
