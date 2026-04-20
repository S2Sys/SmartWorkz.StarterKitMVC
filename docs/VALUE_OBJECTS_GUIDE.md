# Value Objects Guide

Complete reference for domain-driven design value objects in SmartWorkz, including all 5 built-in implementations.

---

## What Are Value Objects?

**Value Objects** are domain model objects that:

1. **Have no identity** — Two value objects are equal if their properties match, not their memory reference
2. **Are immutable** — Once created, cannot be changed (create new instance instead)
3. **Are self-validating** — Validation rules are enforced at creation time via `Result<T>`
4. **Encapsulate domain logic** — Rules specific to that concept (e.g., email must be valid)
5. **Can be reused** — No artificial surrogate keys, uses property equality

**Value Objects vs Entities:**

| Aspect | Value Object | Entity |
|--------|--------------|--------|
| Identity | No — equality by value | Yes — has ID |
| Mutability | Immutable | Usually mutable |
| Lifecycle | Created and discarded | Persisted and tracked |
| Equality | `a.Email == b.Email` → true | `a.Id == b.Id` → true |
| Examples | Email, Address, Money, Phone | User, Product, Order |

---

## ValueObject Base Class

All value objects inherit from `ValueObject<T>` abstract base:

```csharp
namespace SmartWorkz.Core.ValueObjects;

public abstract class ValueObject : IEquatable<ValueObject>
{
    // Get all atomic values for comparison
    protected abstract IEnumerable<object> GetAtomicValues();
    
    // Operators for equality
    public static bool operator ==(ValueObject? a, ValueObject? b);
    public static bool operator !=(ValueObject? a, ValueObject? b);
    
    // Standard equality
    public override bool Equals(object? obj);
    public bool Equals(ValueObject? other);
    public override int GetHashCode();
}
```

**Key behavior:**
- Two value objects are equal if all `GetAtomicValues()` match
- Uses constant-time comparison (protected from timing attacks)
- Hashable (can be used in Dictionary/HashSet)
- Supports `==` and `!=` operators

---

## 5 Built-In Value Objects

### 1. Address

Represents a physical mailing address with validation.

```csharp
public sealed class Address : ValueObject
{
    public string Street { get; }      // Street address
    public string City { get; }        // City name
    public string State { get; }       // State/province code
    public string PostalCode { get; }  // ZIP/postal code
    public string Country { get; }     // Country code
    
    // Private constructor — use Create() factory
    private Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
    
    // Factory method with validation
    public static Result<Address> Create(
        string street, 
        string city, 
        string state, 
        string postalCode, 
        string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Fail<Address>(Error.Validation("street_required", "Street is required"));
        
        if (string.IsNullOrWhiteSpace(city))
            return Result.Fail<Address>(Error.Validation("city_required", "City is required"));
        
        if (string.IsNullOrWhiteSpace(state) || state.Length != 2)
            return Result.Fail<Address>(Error.Validation("state_invalid", "State must be 2-letter code"));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            return Result.Fail<Address>(Error.Validation("postal_required", "Postal code is required"));
        
        if (string.IsNullOrWhiteSpace(country))
            return Result.Fail<Address>(Error.Validation("country_required", "Country is required"));
        
        return Result.Ok(new Address(street, city, state, postalCode, country));
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
```

**Usage:**

```csharp
var addressResult = Address.Create(
    street: "123 Main St",
    city: "Springfield",
    state: "IL",
    postalCode: "62701",
    country: "USA"
);

if (addressResult.Succeeded)
{
    var address = addressResult.Data;
    Console.WriteLine($"{address.Street}, {address.City}, {address.State}");
}
else
{
    Console.WriteLine($"Invalid address: {addressResult.Error.Message}");
}
```

---

### 2. EmailAddress

Represents a validated email address.

```csharp
public sealed class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );
    
    public string Value { get; }
    
    private EmailAddress(string value)
    {
        Value = value.ToLowerInvariant();
    }
    
    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail<EmailAddress>(Error.Validation("email_required", "Email is required"));
        
        if (email.Length > 254)
            return Result.Fail<EmailAddress>(Error.Validation("email_too_long", "Email is too long"));
        
        if (!EmailRegex.IsMatch(email))
            return Result.Fail<EmailAddress>(Error.Validation("email_invalid", "Email format is invalid"));
        
        return Result.Ok(new EmailAddress(email));
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
}
```

**Usage:**

```csharp
var emailResult = EmailAddress.Create("user@example.com");

if (emailResult.Succeeded)
{
    var email = emailResult.Data;
    Console.WriteLine($"Email: {email}");
}
```

---

### 3. Money

Represents a monetary amount with a specific currency.

```csharp
public sealed class Money : ValueObject
{
    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY", // ISO 4217 codes
        // ... add all supported currencies
    };
    
    public decimal Amount { get; }
    public string Currency { get; } // ISO 4217 code (USD, EUR, etc.)
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result.Fail<Money>(Error.Validation("amount_negative", "Amount cannot be negative"));
        
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return Result.Fail<Money>(Error.Validation("currency_invalid", "Currency must be 3-letter ISO code"));
        
        if (!ValidCurrencies.Contains(currency.ToUpperInvariant()))
            return Result.Fail<Money>(Error.Validation("currency_unsupported", $"Currency {currency} not supported"));
        
        return Result.Ok(new Money(amount, currency.ToUpperInvariant()));
    }
    
    // Money arithmetic (returns new Money instances)
    public Result<Money> Add(Money other)
    {
        if (other.Currency != Currency)
            return Result.Fail<Money>(Error.Validation("currency_mismatch", "Cannot add different currencies"));
        
        return Result.Ok(new Money(Amount + other.Amount, Currency));
    }
    
    public Result<Money> Subtract(Money other)
    {
        if (other.Currency != Currency)
            return Result.Fail<Money>(Error.Validation("currency_mismatch", "Cannot subtract different currencies"));
        
        var result = Amount - other.Amount;
        if (result < 0)
            return Result.Fail<Money>(Error.Validation("insufficient_funds", "Insufficient funds"));
        
        return Result.Ok(new Money(result, Currency));
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public override string ToString() => $"{Amount:F2} {Currency}";
}
```

**Usage:**

```csharp
// Create money values
var priceResult = Money.Create(29.99m, "USD");
var taxResult = Money.Create(2.40m, "USD");

if (priceResult.Succeeded && taxResult.Succeeded)
{
    var price = priceResult.Data;
    var tax = taxResult.Data;
    
    // Add tax to price
    var totalResult = price.Add(tax);
    if (totalResult.Succeeded)
    {
        Console.WriteLine($"Total: {totalResult.Data}"); // Total: 32.39 USD
    }
}

// Currency mismatch
var eurResult = Money.Create(20.00m, "EUR");
var addResult = price.Add(eurResult.Data);
if (!addResult.Succeeded)
{
    Console.WriteLine("Cannot mix currencies"); // Error: currency_mismatch
}
```

---

### 4. PersonName

Represents a person's name with optional middle name.

```csharp
public sealed class PersonName : ValueObject
{
    public string FirstName { get; }
    public string MiddleName { get; }  // Optional, can be empty
    public string LastName { get; }
    
    public string FullName => 
        string.IsNullOrWhiteSpace(MiddleName) 
            ? $"{FirstName} {LastName}" 
            : $"{FirstName} {MiddleName} {LastName}";
    
    private PersonName(string firstName, string middleName, string lastName)
    {
        FirstName = firstName;
        MiddleName = middleName ?? "";
        LastName = lastName;
    }
    
    public static Result<PersonName> Create(
        string firstName, 
        string lastName, 
        string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Fail<PersonName>(Error.Validation("firstname_required", "First name is required"));
        
        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Fail<PersonName>(Error.Validation("lastname_required", "Last name is required"));
        
        if (firstName.Length > 50)
            return Result.Fail<PersonName>(Error.Validation("firstname_too_long", "First name is too long"));
        
        if (lastName.Length > 50)
            return Result.Fail<PersonName>(Error.Validation("lastname_too_long", "Last name is too long"));
        
        return Result.Ok(new PersonName(firstName, middleName ?? "", lastName));
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FirstName;
        yield return MiddleName;
        yield return LastName;
    }
    
    public override string ToString() => FullName;
}
```

**Usage:**

```csharp
var nameResult = PersonName.Create("John", "Doe", "Michael");

if (nameResult.Succeeded)
{
    var name = nameResult.Data;
    Console.WriteLine($"Full Name: {name}"); // John Michael Doe
}
```

---

### 5. PhoneNumber

Represents a validated phone number (10-15 digits).

```csharp
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; } // Only digits
    
    private PhoneNumber(string value)
    {
        Value = value;
    }
    
    public static Result<PhoneNumber> Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Fail<PhoneNumber>(Error.Validation("phone_required", "Phone number is required"));
        
        // Strip non-digits
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"\D", "");
        
        if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
            return Result.Fail<PhoneNumber>(Error.Validation("phone_invalid", "Phone number must be 10-15 digits"));
        
        return Result.Ok(new PhoneNumber(digitsOnly));
    }
    
    public string Format() => 
        Value.Length switch
        {
            10 => $"({Value.Substring(0, 3)}) {Value.Substring(3, 3)}-{Value.Substring(6)}", // (555) 123-4567
            11 => $"+{Value[0]} ({Value.Substring(1, 3)}) {Value.Substring(4, 3)}-{Value.Substring(7)}", // +1 (555) 123-4567
            _ => Value
        };
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
}
```

**Usage:**

```csharp
var phoneResult = PhoneNumber.Create("555-123-4567");

if (phoneResult.Succeeded)
{
    var phone = phoneResult.Data;
    Console.WriteLine($"Phone: {phone.Format()}"); // Phone: (555) 123-4567
}
```

---

## Using Value Objects in Entities

### With EF Core Owned Types

Embed value objects directly in your entities:

```csharp
public class User
{
    public int Id { get; set; }
    
    // Value object embedded as owned type
    public EmailAddress Email { get; set; } = null!;
    
    // Multiple value objects
    public Address BillingAddress { get; set; } = null!;
    public Address ShippingAddress { get; set; } = null!;
    
    // Composite value object
    public PersonName Name { get; set; } = null!;
    public PhoneNumber Phone { get; set; } = null!;
}
```

### Configure in DbContext

```csharp
public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .OwnsOne(u => u.Email, eo =>
            {
                eo.Property(e => e.Value).HasColumnName("Email");
            })
            .OwnsOne(u => u.Name, no =>
            {
                no.Property(n => n.FirstName).HasColumnName("FirstName");
                no.Property(n => n.MiddleName).HasColumnName("MiddleName");
                no.Property(n => n.LastName).HasColumnName("LastName");
            });
        
        modelBuilder.Entity<User>()
            .OwnsOne(u => u.BillingAddress, ao =>
            {
                ao.Property(a => a.Street).HasColumnName("BillingStreet");
                ao.Property(a => a.City).HasColumnName("BillingCity");
                // ...
            });
    }
}
```

### Creating Entities with Value Objects

```csharp
var emailResult = EmailAddress.Create("user@example.com");
var nameResult = PersonName.Create("John", "Doe");
var addressResult = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA");

if (emailResult.Succeeded && nameResult.Succeeded && addressResult.Succeeded)
{
    var user = new User
    {
        Email = emailResult.Data,
        Name = nameResult.Data,
        BillingAddress = addressResult.Data,
        CreatedAt = DateTime.UtcNow
    };
    
    await _userRepository.AddAsync(user);
}
```

---

## Value Object Equality

Value objects compare by value, not reference:

```csharp
var email1 = EmailAddress.Create("user@example.com").Data;
var email2 = EmailAddress.Create("user@example.com").Data;

// True — same value, different instances
email1 == email2        // true
email1.Equals(email2)   // true
object.ReferenceEquals(email1, email2) // false
```

```csharp
var address1 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Data;
var address2 = Address.Create("123 Main St", "Springfield", "IL", "62701", "USA").Data;
var address3 = Address.Create("456 Oak Ave", "Chicago", "IL", "60601", "USA").Data;

address1 == address2  // true — same values
address1 == address3  // false — different values
```

---

## Best Practices

### 1. Always Use Factory Methods

```csharp
// ✓ Good — validation enforced
var result = EmailAddress.Create("user@example.com");

// ✗ Bad — bypasses validation
var email = new EmailAddress("invalid@@email"); // Not possible — constructor private
```

### 2. Immutable — Create New Instances for Changes

```csharp
var money = Money.Create(100m, "USD").Data;

// ✓ Good — returns new instance
var newMoney = money.Add(Money.Create(50m, "USD").Data).Data;

// ✗ Bad — value objects don't have setters
money.Amount = 150m; // Compilation error
```

### 3. Use Result<T> Pattern

```csharp
// ✓ Good — handle success and failure
var result = Money.Create(-100m, "USD");
if (!result.Succeeded)
{
    Console.WriteLine(result.Error.Message);
}

// ✗ Bad — assumes success
var money = Money.Create(-100m, "USD").Data; // Throws if failed
```

### 4. Embed in Entities, Not Separate Tables

```csharp
// ✓ Good — value object owned by entity
public class Order
{
    public int Id { get; set; }
    public Money Total { get; set; } = null!;  // Single-valued in Order table
}

// ✗ Bad — treats value object as entity
public class MoneyEntity
{
    public int Id { get; set; }  // Unnecessary surrogate key
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}
```

### 5. Use in Collections Safely

Value objects are hashable, so they work great in collections:

```csharp
var addresses = new HashSet<Address>
{
    Address.Create("123 Main", "Springfield", "IL", "62701", "USA").Data,
    Address.Create("456 Oak", "Chicago", "IL", "60601", "USA").Data
};

// Equality comparison works
var addressToCheck = Address.Create("123 Main", "Springfield", "IL", "62701", "USA").Data;
bool found = addresses.Contains(addressToCheck); // true
```

---

## Real-World Example: Order Aggregate

```csharp
public class Order
{
    public int Id { get; private set; }
    
    // Value objects
    public EmailAddress CustomerEmail { get; private set; } = null!;
    public Money Total { get; private set; } = null!;
    public Money Tax { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    
    // Entities
    public List<OrderLine> Lines { get; private set; } = new();
    
    private Order() { } // For EF Core
    
    public static Result<Order> Create(
        EmailAddress email, 
        Address shippingAddress,
        Money total,
        Money tax)
    {
        if (email == null)
            return Result.Fail<Order>(Error.Validation("email_required"));
        
        if (shippingAddress == null)
            return Result.Fail<Order>(Error.Validation("address_required"));
        
        if (total == null || tax == null)
            return Result.Fail<Order>(Error.Validation("amounts_required"));
        
        var order = new Order
        {
            CustomerEmail = email,
            ShippingAddress = shippingAddress,
            Total = total,
            Tax = tax
        };
        
        return Result.Ok(order);
    }
    
    public Result AddLine(OrderLine line)
    {
        // Recalculate total with new line
        var newTotal = Total.Add(line.Amount);
        if (!newTotal.Succeeded)
            return Result.Fail(newTotal.Error);
        
        Total = newTotal.Data;
        Lines.Add(line);
        
        return Result.Ok();
    }
}
```

---

## Troubleshooting

### "Value object equality not working"

**Solution:** Ensure you've overridden `GetAtomicValues()` with all properties:

```csharp
protected override IEnumerable<object> GetAtomicValues()
{
    yield return Property1;
    yield return Property2;
    yield return Property3; // Don't forget any properties
}
```

### "Entity change tracking not detecting updates"

**Solution:** Value objects are immutable. Replace the entire object, don't modify properties:

```csharp
// ✗ Wrong — EF Core won't detect change to immutable value object
order.Total = order.Total; // No-op

// ✓ Right — Create and assign new instance
var newTotal = Money.Create(250m, "USD").Data;
order.Total = newTotal;
```

---

## See Also

- [Domain-Driven Design Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md#domain-driven-design) — DDD patterns
- [Result Pattern Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md#result-pattern) — Error handling
- [EF Core Owned Types](https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities)
- [Domain-Driven Design: Tackling Complexity in the Heart of Software (Eric Evans)](https://en.wikipedia.org/wiki/Domain-driven_design)
