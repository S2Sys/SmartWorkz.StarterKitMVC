# Core API Reference

## Classes & Interfaces

### IAuditable

- **Namespace:** `SmartWorkz.Core.IAuditable`
- **Summary:** Marks an entity that tracks who created and last modified it.
             Implemented by auditable entity classes to provide creation and modification metadata.

### IEntity`1

- **Namespace:** `SmartWorkz.Core.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### IService

- **Namespace:** `SmartWorkz.Core.IService`
- **Summary:** Marker interface for all service classes
            Provides base contract for domain services

### ISoftDeletable

- **Namespace:** `SmartWorkz.Core.ISoftDeletable`
- **Summary:** Marks an entity that supports soft deletion.
            Infrastructure layer (DbContext, Dapper interceptors) should filter IsDeleted = false automatically.

### ITenantScoped

- **Namespace:** `SmartWorkz.Core.ITenantScoped`
- **Summary:** Marker interface for entities that are scoped to a tenant in a multi-tenant application.

### AuditableEntity

- **Namespace:** `SmartWorkz.Core.AuditableEntity`
- **Summary:** DEPRECATED: Use AuditEntity instead.
             Convenience base class for auditable entities with an integer primary key.

### AuditableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditableEntity`1`
- **Summary:** DEPRECATED: Use AuditEntity{TId} instead.
             Generic base class for auditable, soft-deletable entities.

### AuditDeletableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable domain entities.

### AuditDeletableEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`
- **Summary:** Convenience base class for auditable, soft-deletable entities with an integer primary key.

### AuditDeletableTenantEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable, multi-tenant domain entities.

### AuditDeletableTenantEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`
- **Summary:** Convenience base class for auditable, soft-deletable, multi-tenant entities with an integer primary key.

### AuditEntity`1

- **Namespace:** `SmartWorkz.Core.AuditEntity`1`
- **Summary:** Generic base class for auditable domain entities that track creation and modification metadata.

### AuditEntity

- **Namespace:** `SmartWorkz.Core.AuditEntity`
- **Summary:** Convenience base class for auditable entities with an integer primary key.

### DeletableEntity`1

- **Namespace:** `SmartWorkz.Core.DeletableEntity`1`
- **Summary:** Generic base class for entities with soft delete support (logical delete, not physical delete).

### DeletableEntity

- **Namespace:** `SmartWorkz.Core.DeletableEntity`
- **Summary:** Convenience base class for soft-deletable entities with an integer primary key.

### TenantEntity

- **Namespace:** `SmartWorkz.Core.TenantEntity`
- **Summary:** Convenience base class for multi-tenant entities with an integer primary key.

### Entity`1

- **Namespace:** `SmartWorkz.Core.Entity`1`
- **Summary:** Generic base class for all domain entities.

#### Methods & Properties

- **Equals** - Determines whether the specified object is equal to the current entity.
  - Parameters:
    - `obj`: The object to compare with the current entity.
  - Returns: true if the specified object is an Entity<TId> with the same Id as the current entity;
             otherwise, false.
- **GetHashCode** - Serves as the default hash function.
  - Returns: A hash code for the current entity based on its Id.
- **op_Equality** - Determines whether two entities are equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if both entities are equal (have the same Id); otherwise, false.
- **op_Inequality** - Determines whether two entities are not equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if the entities are not equal (have different Ids); otherwise, false.

### Entity

- **Namespace:** `SmartWorkz.Core.Entity`
- **Summary:** Convenience base class for entities with an integer primary key.

### EntityState

- **Namespace:** `SmartWorkz.Core.EntityState`
- **Summary:** Enumeration representing various state transitions and conditions for domain entities across multiple operational categories.

### EntityStatus

- **Namespace:** `SmartWorkz.Core.EntityStatus`
- **Summary:** [OBSOLETE] Type alias shim for backward compatibility.
             EntityStatus has been replaced by EntityState enum.
             Use EntityState instead — it provides the same functionality with 64 comprehensive states
             across 15 categories (lifecycle, verification, approval, document, security, payment,
             order, inventory, communication, subscription, user account, return/refund, task/job,
             review/rating, shipping).

### ResultStatus

- **Namespace:** `SmartWorkz.Core.ResultStatus`
- **Summary:** Enumeration representing the status of an operation result in domain or service layer responses.

### SortDirection

- **Namespace:** `SmartWorkz.Core.SortDirection`
- **Summary:** Enumeration representing the sort order direction for query results.

### ServiceBase

- **Namespace:** `SmartWorkz.Core.ServiceBase`
- **Summary:** Marker base class for all domain services.

### ServiceBase`2

- **Namespace:** `SmartWorkz.Core.ServiceBase`2`
- **Summary:** Generic base class for CRUD services implementing domain entity operations.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ServiceBase class.
  - Parameters:
    - `repository`: The repository for entity data access. Must not be null.
- **GetByIdAsync** - Retrieves an entity by its identifier and returns the corresponding DTO.
  - Parameters:
    - `id`: The unique identifier of the entity to retrieve. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **GetAllAsync** - Retrieves all entities and returns them as a read-only collection of DTOs.
  - Parameters:
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing a read-only collection of mapped DTOs representing all entities.
             If no entities exist, an empty read-only collection is returned.
- **CreateAsync** - Creates a new entity from the provided DTO and persists it to the database.
  - Parameters:
    - `dto`: The data transfer object containing the entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the newly created entity (with database-generated values).
             Failure: The method throws ArgumentNullException if dto is null (infrastructure failure).
- **UpdateAsync** - Updates an existing entity with values from the provided DTO.
  - Parameters:
    - `id`: The identifier of the entity to update. Must not be zero or default.
    - `dto`: The data transfer object containing the updated entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the updated entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **DeleteAsync** - Deletes an entity by its identifier.
  - Parameters:
    - `id`: The identifier of the entity to delete. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing true if the entity was successfully deleted.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **ApplyUpdates** - Applies properties from a DTO to an existing entity.
  - Parameters:
    - `entity`: The existing domain entity to update. Must not be null.
    - `dto`: The data transfer object containing updated properties. Must not be null.

### Address

- **Namespace:** `SmartWorkz.Core.Address`
- **Summary:** Immutable value object representing a physical mailing address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Address with validated components.
            Constructor is private; use Create() factory method to construct instances.
- **Create** - Factory method to create a validated Address value object.
  - Parameters:
    - `street`: Street address (required, non-empty)
    - `city`: City or municipality name (required, non-empty)
    - `state`: State, province, or region (required, non-empty)
    - `postalCode`: Postal/ZIP code (required, non-empty)
    - `country`: Country name or code (required, non-empty)
  - Returns: Success result containing the Address if all validations pass.
            Failure result with specific error code if any component is empty or null.
- **GetAtomicValues** - Returns the atomic values that define this address's identity.
  - Returns: All address components in order: street, city, state, postal code, country
- **ToString** - Returns the formatted full address string.
  - Returns: Address in format: "{Street}, {City}, {State} {PostalCode}, {Country}"

### EmailAddress

- **Namespace:** `SmartWorkz.Core.EmailAddress`
- **Summary:** Immutable value object representing a valid email address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new EmailAddress with a validated, normalized email.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `value`: Trimmed, lowercase email address
- **Create** - Factory method to create a validated EmailAddress value object.
  - Parameters:
    - `email`: Email address string (required, non-empty, valid format)
  - Returns: Success result containing the EmailAddress if all validations pass.
             Failure result with specific error code if validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this email's identity.
  - Returns: The normalized email address string
- **ToString** - Returns the email address as a string.
  - Returns: The normalized email address (lowercase)

### Money

- **Namespace:** `SmartWorkz.Core.Money`
- **Summary:** Immutable value object representing a monetary amount with currency validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Money value object with amount and currency.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (uppercase)
- **Create** - Factory method to create a validated Money value object.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (required, non-empty)
  - Returns: Success result containing the Money if all validations pass.
             Failure result with specific error code if any validation fails.
- **Add** - Adds two monetary amounts if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to add
  - Returns: Success result containing the sum if currencies match.
             Failure result if currencies differ.
- **Subtract** - Subtracts another monetary amount from this one if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to subtract
  - Returns: Success result containing the difference if currencies match and result is non-negative.
             Failure result if currencies differ or result would be negative.
- **GetAtomicValues** - Returns the atomic values that define this money's identity.
  - Returns: Both amount and currency in order
- **ToString** - Returns the formatted monetary amount with currency.
  - Returns: Amount formatted to 2 decimal places, followed by currency code

### PersonName

- **Namespace:** `SmartWorkz.Core.PersonName`
- **Summary:** Immutable value object representing a person's name with optional middle name.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PersonName with required first and last names, and optional middle name.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null if not provided)
- **Create** - Factory method to create a validated PersonName value object.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null or empty is allowed)
  - Returns: Success result containing the PersonName if all validations pass.
             Failure result with specific error code if any required field is missing.
- **GetAtomicValues** - Returns the atomic values that define this name's identity.
  - Returns: First name, middle name (or empty string if null), and last name in order
- **ToString** - Returns the formatted full name.
  - Returns: Complete name including middle name if present

### PhoneNumber

- **Namespace:** `SmartWorkz.Core.PhoneNumber`
- **Summary:** Immutable value object representing a phone number with digit validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PhoneNumber with a validated, normalized digit sequence.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `formattedNumber`: The phone number as digits only (10-15 digits)
- **Create** - Factory method to create a validated PhoneNumber value object.
  - Parameters:
    - `phone`: Phone number in any format (required, non-empty)
  - Returns: Success result containing the PhoneNumber if all validations pass.
             Failure result with specific error code if any validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this phone number's identity.
  - Returns: The digit-only phone number string
- **ToString** - Returns the phone number as a digit-only string.
  - Returns: The phone number containing only digits (0-9)

### ValueObject

- **Namespace:** `SmartWorkz.Core.ValueObject`
- **Summary:** Base class for all immutable value objects in the domain.
             Provides equality semantics and hashing for value objects based on their atomic components.
- **Example:**
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

#### Methods & Properties

- **GetAtomicValues** - Returns the atomic values that define this value object's identity.
            All properties that contribute to equality must be yielded here.
  - Returns: An enumeration of atomic values in consistent order
- **GetEqualityComponents** - Converts atomic values to equality components, handling nulls safely.
  - Returns: Atomic values with nulls replaced by empty string for consistent hashing
- **Equals** - Determines whether this value object equals another, based on atomic values.
  - Parameters:
    - `other`: Another value object to compare
  - Returns: True if both objects are the same type and have identical atomic values
- **Equals** - Determines whether this value object equals another object.
  - Parameters:
    - `obj`: Any object to compare
  - Returns: True if obj is a value object with identical atomic values
- **GetHashCode** - Computes a hash code based on all atomic values.
  - Returns: A hash code combining all equality components
- **op_Equality** - Determines if two value objects are equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if both are null or have identical atomic values
- **op_Inequality** - Determines if two value objects are not equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if one is null and the other is not, or they have different atomic values

### IAuditable

- **Namespace:** `SmartWorkz.Core.IAuditable`
- **Summary:** Marks an entity that tracks who created and last modified it.
             Implemented by auditable entity classes to provide creation and modification metadata.

### IEntity`1

- **Namespace:** `SmartWorkz.Core.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### IService

- **Namespace:** `SmartWorkz.Core.IService`
- **Summary:** Marker interface for all service classes
            Provides base contract for domain services

### ISoftDeletable

- **Namespace:** `SmartWorkz.Core.ISoftDeletable`
- **Summary:** Marks an entity that supports soft deletion.
            Infrastructure layer (DbContext, Dapper interceptors) should filter IsDeleted = false automatically.

### ITenantScoped

- **Namespace:** `SmartWorkz.Core.ITenantScoped`
- **Summary:** Marker interface for entities that are scoped to a tenant in a multi-tenant application.

### AuditableEntity

- **Namespace:** `SmartWorkz.Core.AuditableEntity`
- **Summary:** DEPRECATED: Use AuditEntity instead.
             Convenience base class for auditable entities with an integer primary key.

### AuditableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditableEntity`1`
- **Summary:** DEPRECATED: Use AuditEntity{TId} instead.
             Generic base class for auditable, soft-deletable entities.

### AuditDeletableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable domain entities.

### AuditDeletableEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`
- **Summary:** Convenience base class for auditable, soft-deletable entities with an integer primary key.

### AuditDeletableTenantEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable, multi-tenant domain entities.

### AuditDeletableTenantEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`
- **Summary:** Convenience base class for auditable, soft-deletable, multi-tenant entities with an integer primary key.

### AuditEntity`1

- **Namespace:** `SmartWorkz.Core.AuditEntity`1`
- **Summary:** Generic base class for auditable domain entities that track creation and modification metadata.

### AuditEntity

- **Namespace:** `SmartWorkz.Core.AuditEntity`
- **Summary:** Convenience base class for auditable entities with an integer primary key.

### DeletableEntity`1

- **Namespace:** `SmartWorkz.Core.DeletableEntity`1`
- **Summary:** Generic base class for entities with soft delete support (logical delete, not physical delete).

### DeletableEntity

- **Namespace:** `SmartWorkz.Core.DeletableEntity`
- **Summary:** Convenience base class for soft-deletable entities with an integer primary key.

### TenantEntity

- **Namespace:** `SmartWorkz.Core.TenantEntity`
- **Summary:** Convenience base class for multi-tenant entities with an integer primary key.

### Entity`1

- **Namespace:** `SmartWorkz.Core.Entity`1`
- **Summary:** Generic base class for all domain entities.

#### Methods & Properties

- **Equals** - Determines whether the specified object is equal to the current entity.
  - Parameters:
    - `obj`: The object to compare with the current entity.
  - Returns: true if the specified object is an Entity<TId> with the same Id as the current entity;
             otherwise, false.
- **GetHashCode** - Serves as the default hash function.
  - Returns: A hash code for the current entity based on its Id.
- **op_Equality** - Determines whether two entities are equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if both entities are equal (have the same Id); otherwise, false.
- **op_Inequality** - Determines whether two entities are not equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if the entities are not equal (have different Ids); otherwise, false.

### Entity

- **Namespace:** `SmartWorkz.Core.Entity`
- **Summary:** Convenience base class for entities with an integer primary key.

### EntityState

- **Namespace:** `SmartWorkz.Core.EntityState`
- **Summary:** Enumeration representing various state transitions and conditions for domain entities across multiple operational categories.

### EntityStatus

- **Namespace:** `SmartWorkz.Core.EntityStatus`
- **Summary:** [OBSOLETE] Type alias shim for backward compatibility.
             EntityStatus has been replaced by EntityState enum.
             Use EntityState instead — it provides the same functionality with 64 comprehensive states
             across 15 categories (lifecycle, verification, approval, document, security, payment,
             order, inventory, communication, subscription, user account, return/refund, task/job,
             review/rating, shipping).

### ResultStatus

- **Namespace:** `SmartWorkz.Core.ResultStatus`
- **Summary:** Enumeration representing the status of an operation result in domain or service layer responses.

### SortDirection

- **Namespace:** `SmartWorkz.Core.SortDirection`
- **Summary:** Enumeration representing the sort order direction for query results.

### ServiceBase

- **Namespace:** `SmartWorkz.Core.ServiceBase`
- **Summary:** Marker base class for all domain services.

### ServiceBase`2

- **Namespace:** `SmartWorkz.Core.ServiceBase`2`
- **Summary:** Generic base class for CRUD services implementing domain entity operations.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ServiceBase class.
  - Parameters:
    - `repository`: The repository for entity data access. Must not be null.
- **GetByIdAsync** - Retrieves an entity by its identifier and returns the corresponding DTO.
  - Parameters:
    - `id`: The unique identifier of the entity to retrieve. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **GetAllAsync** - Retrieves all entities and returns them as a read-only collection of DTOs.
  - Parameters:
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing a read-only collection of mapped DTOs representing all entities.
             If no entities exist, an empty read-only collection is returned.
- **CreateAsync** - Creates a new entity from the provided DTO and persists it to the database.
  - Parameters:
    - `dto`: The data transfer object containing the entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the newly created entity (with database-generated values).
             Failure: The method throws ArgumentNullException if dto is null (infrastructure failure).
- **UpdateAsync** - Updates an existing entity with values from the provided DTO.
  - Parameters:
    - `id`: The identifier of the entity to update. Must not be zero or default.
    - `dto`: The data transfer object containing the updated entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the updated entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **DeleteAsync** - Deletes an entity by its identifier.
  - Parameters:
    - `id`: The identifier of the entity to delete. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing true if the entity was successfully deleted.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **ApplyUpdates** - Applies properties from a DTO to an existing entity.
  - Parameters:
    - `entity`: The existing domain entity to update. Must not be null.
    - `dto`: The data transfer object containing updated properties. Must not be null.

### Address

- **Namespace:** `SmartWorkz.Core.Address`
- **Summary:** Immutable value object representing a physical mailing address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Address with validated components.
            Constructor is private; use Create() factory method to construct instances.
- **Create** - Factory method to create a validated Address value object.
  - Parameters:
    - `street`: Street address (required, non-empty)
    - `city`: City or municipality name (required, non-empty)
    - `state`: State, province, or region (required, non-empty)
    - `postalCode`: Postal/ZIP code (required, non-empty)
    - `country`: Country name or code (required, non-empty)
  - Returns: Success result containing the Address if all validations pass.
            Failure result with specific error code if any component is empty or null.
- **GetAtomicValues** - Returns the atomic values that define this address's identity.
  - Returns: All address components in order: street, city, state, postal code, country
- **ToString** - Returns the formatted full address string.
  - Returns: Address in format: "{Street}, {City}, {State} {PostalCode}, {Country}"

### EmailAddress

- **Namespace:** `SmartWorkz.Core.EmailAddress`
- **Summary:** Immutable value object representing a valid email address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new EmailAddress with a validated, normalized email.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `value`: Trimmed, lowercase email address
- **Create** - Factory method to create a validated EmailAddress value object.
  - Parameters:
    - `email`: Email address string (required, non-empty, valid format)
  - Returns: Success result containing the EmailAddress if all validations pass.
             Failure result with specific error code if validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this email's identity.
  - Returns: The normalized email address string
- **ToString** - Returns the email address as a string.
  - Returns: The normalized email address (lowercase)

### Money

- **Namespace:** `SmartWorkz.Core.Money`
- **Summary:** Immutable value object representing a monetary amount with currency validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Money value object with amount and currency.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (uppercase)
- **Create** - Factory method to create a validated Money value object.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (required, non-empty)
  - Returns: Success result containing the Money if all validations pass.
             Failure result with specific error code if any validation fails.
- **Add** - Adds two monetary amounts if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to add
  - Returns: Success result containing the sum if currencies match.
             Failure result if currencies differ.
- **Subtract** - Subtracts another monetary amount from this one if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to subtract
  - Returns: Success result containing the difference if currencies match and result is non-negative.
             Failure result if currencies differ or result would be negative.
- **GetAtomicValues** - Returns the atomic values that define this money's identity.
  - Returns: Both amount and currency in order
- **ToString** - Returns the formatted monetary amount with currency.
  - Returns: Amount formatted to 2 decimal places, followed by currency code

### PersonName

- **Namespace:** `SmartWorkz.Core.PersonName`
- **Summary:** Immutable value object representing a person's name with optional middle name.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PersonName with required first and last names, and optional middle name.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null if not provided)
- **Create** - Factory method to create a validated PersonName value object.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null or empty is allowed)
  - Returns: Success result containing the PersonName if all validations pass.
             Failure result with specific error code if any required field is missing.
- **GetAtomicValues** - Returns the atomic values that define this name's identity.
  - Returns: First name, middle name (or empty string if null), and last name in order
- **ToString** - Returns the formatted full name.
  - Returns: Complete name including middle name if present

### PhoneNumber

- **Namespace:** `SmartWorkz.Core.PhoneNumber`
- **Summary:** Immutable value object representing a phone number with digit validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PhoneNumber with a validated, normalized digit sequence.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `formattedNumber`: The phone number as digits only (10-15 digits)
- **Create** - Factory method to create a validated PhoneNumber value object.
  - Parameters:
    - `phone`: Phone number in any format (required, non-empty)
  - Returns: Success result containing the PhoneNumber if all validations pass.
             Failure result with specific error code if any validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this phone number's identity.
  - Returns: The digit-only phone number string
- **ToString** - Returns the phone number as a digit-only string.
  - Returns: The phone number containing only digits (0-9)

### ValueObject

- **Namespace:** `SmartWorkz.Core.ValueObject`
- **Summary:** Base class for all immutable value objects in the domain.
             Provides equality semantics and hashing for value objects based on their atomic components.
- **Example:**
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

#### Methods & Properties

- **GetAtomicValues** - Returns the atomic values that define this value object's identity.
            All properties that contribute to equality must be yielded here.
  - Returns: An enumeration of atomic values in consistent order
- **GetEqualityComponents** - Converts atomic values to equality components, handling nulls safely.
  - Returns: Atomic values with nulls replaced by empty string for consistent hashing
- **Equals** - Determines whether this value object equals another, based on atomic values.
  - Parameters:
    - `other`: Another value object to compare
  - Returns: True if both objects are the same type and have identical atomic values
- **Equals** - Determines whether this value object equals another object.
  - Parameters:
    - `obj`: Any object to compare
  - Returns: True if obj is a value object with identical atomic values
- **GetHashCode** - Computes a hash code based on all atomic values.
  - Returns: A hash code combining all equality components
- **op_Equality** - Determines if two value objects are equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if both are null or have identical atomic values
- **op_Inequality** - Determines if two value objects are not equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if one is null and the other is not, or they have different atomic values

### IAuditable

- **Namespace:** `SmartWorkz.Core.IAuditable`
- **Summary:** Marks an entity that tracks who created and last modified it.
             Implemented by auditable entity classes to provide creation and modification metadata.

### IEntity`1

- **Namespace:** `SmartWorkz.Core.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### IService

- **Namespace:** `SmartWorkz.Core.IService`
- **Summary:** Marker interface for all service classes
            Provides base contract for domain services

### ISoftDeletable

- **Namespace:** `SmartWorkz.Core.ISoftDeletable`
- **Summary:** Marks an entity that supports soft deletion.
            Infrastructure layer (DbContext, Dapper interceptors) should filter IsDeleted = false automatically.

### ITenantScoped

- **Namespace:** `SmartWorkz.Core.ITenantScoped`
- **Summary:** Marker interface for entities that are scoped to a tenant in a multi-tenant application.

### AuditableEntity

- **Namespace:** `SmartWorkz.Core.AuditableEntity`
- **Summary:** DEPRECATED: Use AuditEntity instead.
             Convenience base class for auditable entities with an integer primary key.

### AuditableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditableEntity`1`
- **Summary:** DEPRECATED: Use AuditEntity{TId} instead.
             Generic base class for auditable, soft-deletable entities.

### AuditDeletableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable domain entities.

### AuditDeletableEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`
- **Summary:** Convenience base class for auditable, soft-deletable entities with an integer primary key.

### AuditDeletableTenantEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable, multi-tenant domain entities.

### AuditDeletableTenantEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`
- **Summary:** Convenience base class for auditable, soft-deletable, multi-tenant entities with an integer primary key.

### AuditEntity`1

- **Namespace:** `SmartWorkz.Core.AuditEntity`1`
- **Summary:** Generic base class for auditable domain entities that track creation and modification metadata.

### AuditEntity

- **Namespace:** `SmartWorkz.Core.AuditEntity`
- **Summary:** Convenience base class for auditable entities with an integer primary key.

### DeletableEntity`1

- **Namespace:** `SmartWorkz.Core.DeletableEntity`1`
- **Summary:** Generic base class for entities with soft delete support (logical delete, not physical delete).

### DeletableEntity

- **Namespace:** `SmartWorkz.Core.DeletableEntity`
- **Summary:** Convenience base class for soft-deletable entities with an integer primary key.

### TenantEntity

- **Namespace:** `SmartWorkz.Core.TenantEntity`
- **Summary:** Convenience base class for multi-tenant entities with an integer primary key.

### Entity`1

- **Namespace:** `SmartWorkz.Core.Entity`1`
- **Summary:** Generic base class for all domain entities.

#### Methods & Properties

- **Equals** - Determines whether the specified object is equal to the current entity.
  - Parameters:
    - `obj`: The object to compare with the current entity.
  - Returns: true if the specified object is an Entity<TId> with the same Id as the current entity;
             otherwise, false.
- **GetHashCode** - Serves as the default hash function.
  - Returns: A hash code for the current entity based on its Id.
- **op_Equality** - Determines whether two entities are equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if both entities are equal (have the same Id); otherwise, false.
- **op_Inequality** - Determines whether two entities are not equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if the entities are not equal (have different Ids); otherwise, false.

### Entity

- **Namespace:** `SmartWorkz.Core.Entity`
- **Summary:** Convenience base class for entities with an integer primary key.

### EntityState

- **Namespace:** `SmartWorkz.Core.EntityState`
- **Summary:** Enumeration representing various state transitions and conditions for domain entities across multiple operational categories.

### EntityStatus

- **Namespace:** `SmartWorkz.Core.EntityStatus`
- **Summary:** [OBSOLETE] Type alias shim for backward compatibility.
             EntityStatus has been replaced by EntityState enum.
             Use EntityState instead — it provides the same functionality with 64 comprehensive states
             across 15 categories (lifecycle, verification, approval, document, security, payment,
             order, inventory, communication, subscription, user account, return/refund, task/job,
             review/rating, shipping).

### ResultStatus

- **Namespace:** `SmartWorkz.Core.ResultStatus`
- **Summary:** Enumeration representing the status of an operation result in domain or service layer responses.

### SortDirection

- **Namespace:** `SmartWorkz.Core.SortDirection`
- **Summary:** Enumeration representing the sort order direction for query results.

### ServiceBase

- **Namespace:** `SmartWorkz.Core.ServiceBase`
- **Summary:** Marker base class for all domain services.

### ServiceBase`2

- **Namespace:** `SmartWorkz.Core.ServiceBase`2`
- **Summary:** Generic base class for CRUD services implementing domain entity operations.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ServiceBase class.
  - Parameters:
    - `repository`: The repository for entity data access. Must not be null.
- **GetByIdAsync** - Retrieves an entity by its identifier and returns the corresponding DTO.
  - Parameters:
    - `id`: The unique identifier of the entity to retrieve. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **GetAllAsync** - Retrieves all entities and returns them as a read-only collection of DTOs.
  - Parameters:
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing a read-only collection of mapped DTOs representing all entities.
             If no entities exist, an empty read-only collection is returned.
- **CreateAsync** - Creates a new entity from the provided DTO and persists it to the database.
  - Parameters:
    - `dto`: The data transfer object containing the entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the newly created entity (with database-generated values).
             Failure: The method throws ArgumentNullException if dto is null (infrastructure failure).
- **UpdateAsync** - Updates an existing entity with values from the provided DTO.
  - Parameters:
    - `id`: The identifier of the entity to update. Must not be zero or default.
    - `dto`: The data transfer object containing the updated entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the updated entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **DeleteAsync** - Deletes an entity by its identifier.
  - Parameters:
    - `id`: The identifier of the entity to delete. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing true if the entity was successfully deleted.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **ApplyUpdates** - Applies properties from a DTO to an existing entity.
  - Parameters:
    - `entity`: The existing domain entity to update. Must not be null.
    - `dto`: The data transfer object containing updated properties. Must not be null.

### Address

- **Namespace:** `SmartWorkz.Core.Address`
- **Summary:** Immutable value object representing a physical mailing address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Address with validated components.
            Constructor is private; use Create() factory method to construct instances.
- **Create** - Factory method to create a validated Address value object.
  - Parameters:
    - `street`: Street address (required, non-empty)
    - `city`: City or municipality name (required, non-empty)
    - `state`: State, province, or region (required, non-empty)
    - `postalCode`: Postal/ZIP code (required, non-empty)
    - `country`: Country name or code (required, non-empty)
  - Returns: Success result containing the Address if all validations pass.
            Failure result with specific error code if any component is empty or null.
- **GetAtomicValues** - Returns the atomic values that define this address's identity.
  - Returns: All address components in order: street, city, state, postal code, country
- **ToString** - Returns the formatted full address string.
  - Returns: Address in format: "{Street}, {City}, {State} {PostalCode}, {Country}"

### EmailAddress

- **Namespace:** `SmartWorkz.Core.EmailAddress`
- **Summary:** Immutable value object representing a valid email address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new EmailAddress with a validated, normalized email.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `value`: Trimmed, lowercase email address
- **Create** - Factory method to create a validated EmailAddress value object.
  - Parameters:
    - `email`: Email address string (required, non-empty, valid format)
  - Returns: Success result containing the EmailAddress if all validations pass.
             Failure result with specific error code if validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this email's identity.
  - Returns: The normalized email address string
- **ToString** - Returns the email address as a string.
  - Returns: The normalized email address (lowercase)

### Money

- **Namespace:** `SmartWorkz.Core.Money`
- **Summary:** Immutable value object representing a monetary amount with currency validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Money value object with amount and currency.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (uppercase)
- **Create** - Factory method to create a validated Money value object.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (required, non-empty)
  - Returns: Success result containing the Money if all validations pass.
             Failure result with specific error code if any validation fails.
- **Add** - Adds two monetary amounts if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to add
  - Returns: Success result containing the sum if currencies match.
             Failure result if currencies differ.
- **Subtract** - Subtracts another monetary amount from this one if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to subtract
  - Returns: Success result containing the difference if currencies match and result is non-negative.
             Failure result if currencies differ or result would be negative.
- **GetAtomicValues** - Returns the atomic values that define this money's identity.
  - Returns: Both amount and currency in order
- **ToString** - Returns the formatted monetary amount with currency.
  - Returns: Amount formatted to 2 decimal places, followed by currency code

### PersonName

- **Namespace:** `SmartWorkz.Core.PersonName`
- **Summary:** Immutable value object representing a person's name with optional middle name.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PersonName with required first and last names, and optional middle name.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null if not provided)
- **Create** - Factory method to create a validated PersonName value object.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null or empty is allowed)
  - Returns: Success result containing the PersonName if all validations pass.
             Failure result with specific error code if any required field is missing.
- **GetAtomicValues** - Returns the atomic values that define this name's identity.
  - Returns: First name, middle name (or empty string if null), and last name in order
- **ToString** - Returns the formatted full name.
  - Returns: Complete name including middle name if present

### PhoneNumber

- **Namespace:** `SmartWorkz.Core.PhoneNumber`
- **Summary:** Immutable value object representing a phone number with digit validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PhoneNumber with a validated, normalized digit sequence.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `formattedNumber`: The phone number as digits only (10-15 digits)
- **Create** - Factory method to create a validated PhoneNumber value object.
  - Parameters:
    - `phone`: Phone number in any format (required, non-empty)
  - Returns: Success result containing the PhoneNumber if all validations pass.
             Failure result with specific error code if any validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this phone number's identity.
  - Returns: The digit-only phone number string
- **ToString** - Returns the phone number as a digit-only string.
  - Returns: The phone number containing only digits (0-9)

### ValueObject

- **Namespace:** `SmartWorkz.Core.ValueObject`
- **Summary:** Base class for all immutable value objects in the domain.
             Provides equality semantics and hashing for value objects based on their atomic components.
- **Example:**
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

#### Methods & Properties

- **GetAtomicValues** - Returns the atomic values that define this value object's identity.
            All properties that contribute to equality must be yielded here.
  - Returns: An enumeration of atomic values in consistent order
- **GetEqualityComponents** - Converts atomic values to equality components, handling nulls safely.
  - Returns: Atomic values with nulls replaced by empty string for consistent hashing
- **Equals** - Determines whether this value object equals another, based on atomic values.
  - Parameters:
    - `other`: Another value object to compare
  - Returns: True if both objects are the same type and have identical atomic values
- **Equals** - Determines whether this value object equals another object.
  - Parameters:
    - `obj`: Any object to compare
  - Returns: True if obj is a value object with identical atomic values
- **GetHashCode** - Computes a hash code based on all atomic values.
  - Returns: A hash code combining all equality components
- **op_Equality** - Determines if two value objects are equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if both are null or have identical atomic values
- **op_Inequality** - Determines if two value objects are not equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if one is null and the other is not, or they have different atomic values

### IAuditable

- **Namespace:** `SmartWorkz.Core.IAuditable`
- **Summary:** Marks an entity that tracks who created and last modified it.
             Implemented by auditable entity classes to provide creation and modification metadata.

### IEntity`1

- **Namespace:** `SmartWorkz.Core.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### IService

- **Namespace:** `SmartWorkz.Core.IService`
- **Summary:** Marker interface for all service classes
            Provides base contract for domain services

### ISoftDeletable

- **Namespace:** `SmartWorkz.Core.ISoftDeletable`
- **Summary:** Marks an entity that supports soft deletion.
            Infrastructure layer (DbContext, Dapper interceptors) should filter IsDeleted = false automatically.

### ITenantScoped

- **Namespace:** `SmartWorkz.Core.ITenantScoped`
- **Summary:** Marker interface for entities that are scoped to a tenant in a multi-tenant application.

### AuditableEntity

- **Namespace:** `SmartWorkz.Core.AuditableEntity`
- **Summary:** DEPRECATED: Use AuditEntity instead.
             Convenience base class for auditable entities with an integer primary key.

### AuditableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditableEntity`1`
- **Summary:** DEPRECATED: Use AuditEntity{TId} instead.
             Generic base class for auditable, soft-deletable entities.

### AuditDeletableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable domain entities.

### AuditDeletableEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`
- **Summary:** Convenience base class for auditable, soft-deletable entities with an integer primary key.

### AuditDeletableTenantEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable, multi-tenant domain entities.

### AuditDeletableTenantEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`
- **Summary:** Convenience base class for auditable, soft-deletable, multi-tenant entities with an integer primary key.

### AuditEntity`1

- **Namespace:** `SmartWorkz.Core.AuditEntity`1`
- **Summary:** Generic base class for auditable domain entities that track creation and modification metadata.

### AuditEntity

- **Namespace:** `SmartWorkz.Core.AuditEntity`
- **Summary:** Convenience base class for auditable entities with an integer primary key.

### DeletableEntity`1

- **Namespace:** `SmartWorkz.Core.DeletableEntity`1`
- **Summary:** Generic base class for entities with soft delete support (logical delete, not physical delete).

### DeletableEntity

- **Namespace:** `SmartWorkz.Core.DeletableEntity`
- **Summary:** Convenience base class for soft-deletable entities with an integer primary key.

### TenantEntity

- **Namespace:** `SmartWorkz.Core.TenantEntity`
- **Summary:** Convenience base class for multi-tenant entities with an integer primary key.

### Entity`1

- **Namespace:** `SmartWorkz.Core.Entity`1`
- **Summary:** Generic base class for all domain entities.

#### Methods & Properties

- **Equals** - Determines whether the specified object is equal to the current entity.
  - Parameters:
    - `obj`: The object to compare with the current entity.
  - Returns: true if the specified object is an Entity<TId> with the same Id as the current entity;
             otherwise, false.
- **GetHashCode** - Serves as the default hash function.
  - Returns: A hash code for the current entity based on its Id.
- **op_Equality** - Determines whether two entities are equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if both entities are equal (have the same Id); otherwise, false.
- **op_Inequality** - Determines whether two entities are not equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if the entities are not equal (have different Ids); otherwise, false.

### Entity

- **Namespace:** `SmartWorkz.Core.Entity`
- **Summary:** Convenience base class for entities with an integer primary key.

### EntityState

- **Namespace:** `SmartWorkz.Core.EntityState`
- **Summary:** Enumeration representing various state transitions and conditions for domain entities across multiple operational categories.

### EntityStatus

- **Namespace:** `SmartWorkz.Core.EntityStatus`
- **Summary:** [OBSOLETE] Type alias shim for backward compatibility.
             EntityStatus has been replaced by EntityState enum.
             Use EntityState instead — it provides the same functionality with 64 comprehensive states
             across 15 categories (lifecycle, verification, approval, document, security, payment,
             order, inventory, communication, subscription, user account, return/refund, task/job,
             review/rating, shipping).

### ResultStatus

- **Namespace:** `SmartWorkz.Core.ResultStatus`
- **Summary:** Enumeration representing the status of an operation result in domain or service layer responses.

### SortDirection

- **Namespace:** `SmartWorkz.Core.SortDirection`
- **Summary:** Enumeration representing the sort order direction for query results.

### ServiceBase

- **Namespace:** `SmartWorkz.Core.ServiceBase`
- **Summary:** Marker base class for all domain services.

### ServiceBase`2

- **Namespace:** `SmartWorkz.Core.ServiceBase`2`
- **Summary:** Generic base class for CRUD services implementing domain entity operations.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ServiceBase class.
  - Parameters:
    - `repository`: The repository for entity data access. Must not be null.
- **GetByIdAsync** - Retrieves an entity by its identifier and returns the corresponding DTO.
  - Parameters:
    - `id`: The unique identifier of the entity to retrieve. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **GetAllAsync** - Retrieves all entities and returns them as a read-only collection of DTOs.
  - Parameters:
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing a read-only collection of mapped DTOs representing all entities.
             If no entities exist, an empty read-only collection is returned.
- **CreateAsync** - Creates a new entity from the provided DTO and persists it to the database.
  - Parameters:
    - `dto`: The data transfer object containing the entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the newly created entity (with database-generated values).
             Failure: The method throws ArgumentNullException if dto is null (infrastructure failure).
- **UpdateAsync** - Updates an existing entity with values from the provided DTO.
  - Parameters:
    - `id`: The identifier of the entity to update. Must not be zero or default.
    - `dto`: The data transfer object containing the updated entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the updated entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **DeleteAsync** - Deletes an entity by its identifier.
  - Parameters:
    - `id`: The identifier of the entity to delete. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing true if the entity was successfully deleted.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **ApplyUpdates** - Applies properties from a DTO to an existing entity.
  - Parameters:
    - `entity`: The existing domain entity to update. Must not be null.
    - `dto`: The data transfer object containing updated properties. Must not be null.

### Address

- **Namespace:** `SmartWorkz.Core.Address`
- **Summary:** Immutable value object representing a physical mailing address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Address with validated components.
            Constructor is private; use Create() factory method to construct instances.
- **Create** - Factory method to create a validated Address value object.
  - Parameters:
    - `street`: Street address (required, non-empty)
    - `city`: City or municipality name (required, non-empty)
    - `state`: State, province, or region (required, non-empty)
    - `postalCode`: Postal/ZIP code (required, non-empty)
    - `country`: Country name or code (required, non-empty)
  - Returns: Success result containing the Address if all validations pass.
            Failure result with specific error code if any component is empty or null.
- **GetAtomicValues** - Returns the atomic values that define this address's identity.
  - Returns: All address components in order: street, city, state, postal code, country
- **ToString** - Returns the formatted full address string.
  - Returns: Address in format: "{Street}, {City}, {State} {PostalCode}, {Country}"

### EmailAddress

- **Namespace:** `SmartWorkz.Core.EmailAddress`
- **Summary:** Immutable value object representing a valid email address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new EmailAddress with a validated, normalized email.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `value`: Trimmed, lowercase email address
- **Create** - Factory method to create a validated EmailAddress value object.
  - Parameters:
    - `email`: Email address string (required, non-empty, valid format)
  - Returns: Success result containing the EmailAddress if all validations pass.
             Failure result with specific error code if validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this email's identity.
  - Returns: The normalized email address string
- **ToString** - Returns the email address as a string.
  - Returns: The normalized email address (lowercase)

### Money

- **Namespace:** `SmartWorkz.Core.Money`
- **Summary:** Immutable value object representing a monetary amount with currency validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Money value object with amount and currency.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (uppercase)
- **Create** - Factory method to create a validated Money value object.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (required, non-empty)
  - Returns: Success result containing the Money if all validations pass.
             Failure result with specific error code if any validation fails.
- **Add** - Adds two monetary amounts if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to add
  - Returns: Success result containing the sum if currencies match.
             Failure result if currencies differ.
- **Subtract** - Subtracts another monetary amount from this one if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to subtract
  - Returns: Success result containing the difference if currencies match and result is non-negative.
             Failure result if currencies differ or result would be negative.
- **GetAtomicValues** - Returns the atomic values that define this money's identity.
  - Returns: Both amount and currency in order
- **ToString** - Returns the formatted monetary amount with currency.
  - Returns: Amount formatted to 2 decimal places, followed by currency code

### PersonName

- **Namespace:** `SmartWorkz.Core.PersonName`
- **Summary:** Immutable value object representing a person's name with optional middle name.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PersonName with required first and last names, and optional middle name.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null if not provided)
- **Create** - Factory method to create a validated PersonName value object.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null or empty is allowed)
  - Returns: Success result containing the PersonName if all validations pass.
             Failure result with specific error code if any required field is missing.
- **GetAtomicValues** - Returns the atomic values that define this name's identity.
  - Returns: First name, middle name (or empty string if null), and last name in order
- **ToString** - Returns the formatted full name.
  - Returns: Complete name including middle name if present

### PhoneNumber

- **Namespace:** `SmartWorkz.Core.PhoneNumber`
- **Summary:** Immutable value object representing a phone number with digit validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PhoneNumber with a validated, normalized digit sequence.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `formattedNumber`: The phone number as digits only (10-15 digits)
- **Create** - Factory method to create a validated PhoneNumber value object.
  - Parameters:
    - `phone`: Phone number in any format (required, non-empty)
  - Returns: Success result containing the PhoneNumber if all validations pass.
             Failure result with specific error code if any validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this phone number's identity.
  - Returns: The digit-only phone number string
- **ToString** - Returns the phone number as a digit-only string.
  - Returns: The phone number containing only digits (0-9)

### ValueObject

- **Namespace:** `SmartWorkz.Core.ValueObject`
- **Summary:** Base class for all immutable value objects in the domain.
             Provides equality semantics and hashing for value objects based on their atomic components.
- **Example:**
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

#### Methods & Properties

- **GetAtomicValues** - Returns the atomic values that define this value object's identity.
            All properties that contribute to equality must be yielded here.
  - Returns: An enumeration of atomic values in consistent order
- **GetEqualityComponents** - Converts atomic values to equality components, handling nulls safely.
  - Returns: Atomic values with nulls replaced by empty string for consistent hashing
- **Equals** - Determines whether this value object equals another, based on atomic values.
  - Parameters:
    - `other`: Another value object to compare
  - Returns: True if both objects are the same type and have identical atomic values
- **Equals** - Determines whether this value object equals another object.
  - Parameters:
    - `obj`: Any object to compare
  - Returns: True if obj is a value object with identical atomic values
- **GetHashCode** - Computes a hash code based on all atomic values.
  - Returns: A hash code combining all equality components
- **op_Equality** - Determines if two value objects are equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if both are null or have identical atomic values
- **op_Inequality** - Determines if two value objects are not equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if one is null and the other is not, or they have different atomic values

### IAuditable

- **Namespace:** `SmartWorkz.Core.IAuditable`
- **Summary:** Marks an entity that tracks who created and last modified it.
             Implemented by auditable entity classes to provide creation and modification metadata.

### IEntity`1

- **Namespace:** `SmartWorkz.Core.IEntity`1`
- **Summary:** Marks a class as a domain entity with a typed primary key.

### IService

- **Namespace:** `SmartWorkz.Core.IService`
- **Summary:** Marker interface for all service classes
            Provides base contract for domain services

### ISoftDeletable

- **Namespace:** `SmartWorkz.Core.ISoftDeletable`
- **Summary:** Marks an entity that supports soft deletion.
            Infrastructure layer (DbContext, Dapper interceptors) should filter IsDeleted = false automatically.

### ITenantScoped

- **Namespace:** `SmartWorkz.Core.ITenantScoped`
- **Summary:** Marker interface for entities that are scoped to a tenant in a multi-tenant application.

### AuditableEntity

- **Namespace:** `SmartWorkz.Core.AuditableEntity`
- **Summary:** DEPRECATED: Use AuditEntity instead.
             Convenience base class for auditable entities with an integer primary key.

### AuditableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditableEntity`1`
- **Summary:** DEPRECATED: Use AuditEntity{TId} instead.
             Generic base class for auditable, soft-deletable entities.

### AuditDeletableEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable domain entities.

### AuditDeletableEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableEntity`
- **Summary:** Convenience base class for auditable, soft-deletable entities with an integer primary key.

### AuditDeletableTenantEntity`1

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`1`
- **Summary:** Generic base class for auditable, soft-deletable, multi-tenant domain entities.

### AuditDeletableTenantEntity

- **Namespace:** `SmartWorkz.Core.AuditDeletableTenantEntity`
- **Summary:** Convenience base class for auditable, soft-deletable, multi-tenant entities with an integer primary key.

### AuditEntity`1

- **Namespace:** `SmartWorkz.Core.AuditEntity`1`
- **Summary:** Generic base class for auditable domain entities that track creation and modification metadata.

### AuditEntity

- **Namespace:** `SmartWorkz.Core.AuditEntity`
- **Summary:** Convenience base class for auditable entities with an integer primary key.

### DeletableEntity`1

- **Namespace:** `SmartWorkz.Core.DeletableEntity`1`
- **Summary:** Generic base class for entities with soft delete support (logical delete, not physical delete).

### DeletableEntity

- **Namespace:** `SmartWorkz.Core.DeletableEntity`
- **Summary:** Convenience base class for soft-deletable entities with an integer primary key.

### TenantEntity

- **Namespace:** `SmartWorkz.Core.TenantEntity`
- **Summary:** Convenience base class for multi-tenant entities with an integer primary key.

### Entity`1

- **Namespace:** `SmartWorkz.Core.Entity`1`
- **Summary:** Generic base class for all domain entities.

#### Methods & Properties

- **Equals** - Determines whether the specified object is equal to the current entity.
  - Parameters:
    - `obj`: The object to compare with the current entity.
  - Returns: true if the specified object is an Entity<TId> with the same Id as the current entity;
             otherwise, false.
- **GetHashCode** - Serves as the default hash function.
  - Returns: A hash code for the current entity based on its Id.
- **op_Equality** - Determines whether two entities are equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if both entities are equal (have the same Id); otherwise, false.
- **op_Inequality** - Determines whether two entities are not equal.
  - Parameters:
    - `left`: The left entity to compare.
    - `right`: The right entity to compare.
  - Returns: true if the entities are not equal (have different Ids); otherwise, false.

### Entity

- **Namespace:** `SmartWorkz.Core.Entity`
- **Summary:** Convenience base class for entities with an integer primary key.

### EntityState

- **Namespace:** `SmartWorkz.Core.EntityState`
- **Summary:** Enumeration representing various state transitions and conditions for domain entities across multiple operational categories.

### EntityStatus

- **Namespace:** `SmartWorkz.Core.EntityStatus`
- **Summary:** [OBSOLETE] Type alias shim for backward compatibility.
             EntityStatus has been replaced by EntityState enum.
             Use EntityState instead — it provides the same functionality with 64 comprehensive states
             across 15 categories (lifecycle, verification, approval, document, security, payment,
             order, inventory, communication, subscription, user account, return/refund, task/job,
             review/rating, shipping).

### ResultStatus

- **Namespace:** `SmartWorkz.Core.ResultStatus`
- **Summary:** Enumeration representing the status of an operation result in domain or service layer responses.

### SortDirection

- **Namespace:** `SmartWorkz.Core.SortDirection`
- **Summary:** Enumeration representing the sort order direction for query results.

### ServiceBase

- **Namespace:** `SmartWorkz.Core.ServiceBase`
- **Summary:** Marker base class for all domain services.

### ServiceBase`2

- **Namespace:** `SmartWorkz.Core.ServiceBase`2`
- **Summary:** Generic base class for CRUD services implementing domain entity operations.

#### Methods & Properties

- **#ctor** - Initializes a new instance of the ServiceBase class.
  - Parameters:
    - `repository`: The repository for entity data access. Must not be null.
- **GetByIdAsync** - Retrieves an entity by its identifier and returns the corresponding DTO.
  - Parameters:
    - `id`: The unique identifier of the entity to retrieve. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **GetAllAsync** - Retrieves all entities and returns them as a read-only collection of DTOs.
  - Parameters:
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing a read-only collection of mapped DTOs representing all entities.
             If no entities exist, an empty read-only collection is returned.
- **CreateAsync** - Creates a new entity from the provided DTO and persists it to the database.
  - Parameters:
    - `dto`: The data transfer object containing the entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the newly created entity (with database-generated values).
             Failure: The method throws ArgumentNullException if dto is null (infrastructure failure).
- **UpdateAsync** - Updates an existing entity with values from the provided DTO.
  - Parameters:
    - `id`: The identifier of the entity to update. Must not be zero or default.
    - `dto`: The data transfer object containing the updated entity properties. Must not be null.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing the mapped DTO of the updated entity.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **DeleteAsync** - Deletes an entity by its identifier.
  - Parameters:
    - `id`: The identifier of the entity to delete. Must not be zero or default.
    - `cancellationToken`: Cancellation token for the async operation. Defaults to no cancellation.
  - Returns: Success: A Result containing true if the entity was successfully deleted.
             Failure: A Result with error code ENTITY_NOT_FOUND if no entity exists with the given id.
- **ApplyUpdates** - Applies properties from a DTO to an existing entity.
  - Parameters:
    - `entity`: The existing domain entity to update. Must not be null.
    - `dto`: The data transfer object containing updated properties. Must not be null.

### Address

- **Namespace:** `SmartWorkz.Core.Address`
- **Summary:** Immutable value object representing a physical mailing address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Address with validated components.
            Constructor is private; use Create() factory method to construct instances.
- **Create** - Factory method to create a validated Address value object.
  - Parameters:
    - `street`: Street address (required, non-empty)
    - `city`: City or municipality name (required, non-empty)
    - `state`: State, province, or region (required, non-empty)
    - `postalCode`: Postal/ZIP code (required, non-empty)
    - `country`: Country name or code (required, non-empty)
  - Returns: Success result containing the Address if all validations pass.
            Failure result with specific error code if any component is empty or null.
- **GetAtomicValues** - Returns the atomic values that define this address's identity.
  - Returns: All address components in order: street, city, state, postal code, country
- **ToString** - Returns the formatted full address string.
  - Returns: Address in format: "{Street}, {City}, {State} {PostalCode}, {Country}"

### EmailAddress

- **Namespace:** `SmartWorkz.Core.EmailAddress`
- **Summary:** Immutable value object representing a valid email address.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new EmailAddress with a validated, normalized email.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `value`: Trimmed, lowercase email address
- **Create** - Factory method to create a validated EmailAddress value object.
  - Parameters:
    - `email`: Email address string (required, non-empty, valid format)
  - Returns: Success result containing the EmailAddress if all validations pass.
             Failure result with specific error code if validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this email's identity.
  - Returns: The normalized email address string
- **ToString** - Returns the email address as a string.
  - Returns: The normalized email address (lowercase)

### Money

- **Namespace:** `SmartWorkz.Core.Money`
- **Summary:** Immutable value object representing a monetary amount with currency validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new Money value object with amount and currency.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (uppercase)
- **Create** - Factory method to create a validated Money value object.
  - Parameters:
    - `amount`: The monetary amount (must be non-negative)
    - `currency`: The ISO 4217 currency code (required, non-empty)
  - Returns: Success result containing the Money if all validations pass.
             Failure result with specific error code if any validation fails.
- **Add** - Adds two monetary amounts if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to add
  - Returns: Success result containing the sum if currencies match.
             Failure result if currencies differ.
- **Subtract** - Subtracts another monetary amount from this one if they share the same currency.
  - Parameters:
    - `other`: Another Money value object to subtract
  - Returns: Success result containing the difference if currencies match and result is non-negative.
             Failure result if currencies differ or result would be negative.
- **GetAtomicValues** - Returns the atomic values that define this money's identity.
  - Returns: Both amount and currency in order
- **ToString** - Returns the formatted monetary amount with currency.
  - Returns: Amount formatted to 2 decimal places, followed by currency code

### PersonName

- **Namespace:** `SmartWorkz.Core.PersonName`
- **Summary:** Immutable value object representing a person's name with optional middle name.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PersonName with required first and last names, and optional middle name.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null if not provided)
- **Create** - Factory method to create a validated PersonName value object.
  - Parameters:
    - `firstName`: The person's first name (required, non-empty)
    - `lastName`: The person's last name (required, non-empty)
    - `middleName`: The person's middle name (optional; null or empty is allowed)
  - Returns: Success result containing the PersonName if all validations pass.
             Failure result with specific error code if any required field is missing.
- **GetAtomicValues** - Returns the atomic values that define this name's identity.
  - Returns: First name, middle name (or empty string if null), and last name in order
- **ToString** - Returns the formatted full name.
  - Returns: Complete name including middle name if present

### PhoneNumber

- **Namespace:** `SmartWorkz.Core.PhoneNumber`
- **Summary:** Immutable value object representing a phone number with digit validation.
- **Example:**
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

#### Methods & Properties

- **#ctor** - Initializes a new PhoneNumber with a validated, normalized digit sequence.
            Constructor is private; use Create() factory method to construct instances.
  - Parameters:
    - `formattedNumber`: The phone number as digits only (10-15 digits)
- **Create** - Factory method to create a validated PhoneNumber value object.
  - Parameters:
    - `phone`: Phone number in any format (required, non-empty)
  - Returns: Success result containing the PhoneNumber if all validations pass.
             Failure result with specific error code if any validation fails.
- **GetAtomicValues** - Returns the atomic value that defines this phone number's identity.
  - Returns: The digit-only phone number string
- **ToString** - Returns the phone number as a digit-only string.
  - Returns: The phone number containing only digits (0-9)

### ValueObject

- **Namespace:** `SmartWorkz.Core.ValueObject`
- **Summary:** Base class for all immutable value objects in the domain.
             Provides equality semantics and hashing for value objects based on their atomic components.
- **Example:**
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

#### Methods & Properties

- **GetAtomicValues** - Returns the atomic values that define this value object's identity.
            All properties that contribute to equality must be yielded here.
  - Returns: An enumeration of atomic values in consistent order
- **GetEqualityComponents** - Converts atomic values to equality components, handling nulls safely.
  - Returns: Atomic values with nulls replaced by empty string for consistent hashing
- **Equals** - Determines whether this value object equals another, based on atomic values.
  - Parameters:
    - `other`: Another value object to compare
  - Returns: True if both objects are the same type and have identical atomic values
- **Equals** - Determines whether this value object equals another object.
  - Parameters:
    - `obj`: Any object to compare
  - Returns: True if obj is a value object with identical atomic values
- **GetHashCode** - Computes a hash code based on all atomic values.
  - Returns: A hash code combining all equality components
- **op_Equality** - Determines if two value objects are equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if both are null or have identical atomic values
- **op_Inequality** - Determines if two value objects are not equal by value.
  - Parameters:
    - `left`: The first value object (may be null)
    - `right`: The second value object (may be null)
  - Returns: True if one is null and the other is not, or they have different atomic values

