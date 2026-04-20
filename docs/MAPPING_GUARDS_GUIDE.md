# Mapping & Guards Guide

Complete reference for object mapping, guard clauses, and validation in SmartWorkz, covering automatic type conversion, fail-fast argument validation, and composable validation rules.

---

## Overview

SmartWorkz provides a multi-layered validation and object transformation approach:

1. **IMapper** — Service-based object transformation between types
2. **IMapperProfile** — Pluggable mapping strategies for type pairs
3. **Guard** — Fast-fail argument validation (entry point checks)
4. **ValidatorBase & ValidatorBuilder** — Declarative validation rules
5. **CompositeValidator** — Combine multiple validators
6. **ValidationRules** — Regex patterns for common formats

**Validation Pipeline:**

```
Entry Point
    ↓
Guard.NotNull/NotEmpty/InRange (Fail Fast)
    ↓
IValidator<T>.ValidateAsync() (Declarative Rules)
    ↓
ValidationResult (Success or Failures)
    ↓
API Response (200 OK or 400 Bad Request)
```

---

## Guard Clauses: Fail Fast Argument Validation

Guard clauses are static methods that throw immediately when input is invalid. Use them at method entry points to prevent invalid state propagation.

**Why Guards?**
- Fail immediately with clear exceptions
- No return value needed (throws or returns validated value)
- Immutable return type (enables method chaining)
- Prevent silent failures and corruption of business logic

### Guard Class Reference

```csharp
namespace SmartWorkz.Core.Shared.Guards;

public static class Guard
{
    // Validation methods throw on failure, return value on success
}
```

---

### NotNull<T>(value, paramName)

Validates that a reference type or nullable struct is not null.

**Signatures:**

```csharp
// For reference types
public static T NotNull<T>(T? value, string paramName) where T : class

// For nullable structs
public static T NotNull<T>(T? value, string paramName) where T : struct
```

**Behavior:**
- Throws `ArgumentNullException(paramName)` if value is null
- Returns the non-null value
- Enables immutable pattern: `this.userId = Guard.NotNull(userId, nameof(userId));`

**Usage:**

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository? repository)
    {
        // Guard at entry — fail immediately if null
        _repository = Guard.NotNull(repository, nameof(repository));
    }
    
    public async Task<UserDto> GetUserAsync(string userId)
    {
        userId = Guard.NotNull(userId, nameof(userId)); // Reference type
        
        var user = await _repository.GetByIdAsync(userId);
        return user;
    }
    
    public void UpdateUserAge(int? newAge)
    {
        // For nullable struct
        var age = Guard.NotNull(newAge, nameof(newAge));
        // age is now int (non-nullable)
    }
}
```

---

### NotEmpty(value, paramName)

Validates that a string is not null, empty, or whitespace.

**Signature:**

```csharp
public static string NotEmpty(string? value, string paramName)
```

**Behavior:**
- Throws `ArgumentException` if null, empty, or whitespace-only
- Returns the trimmed string
- Prevents silent failures from empty usernames, emails, etc.

**Usage:**

```csharp
public async Task RegisterUserAsync(string email, string password)
{
    email = Guard.NotEmpty(email, nameof(email));       // "user@example.com"
    password = Guard.NotEmpty(password, nameof(password)); // "SecurePass123!"
    
    // Safe — both are guaranteed non-empty strings
    var user = new User { Email = email, PasswordHash = HashPassword(password) };
    await _repository.SaveAsync(user);
}

// ❌ Will throw ArgumentException("Email is required", "email")
// RegisterUserAsync("   ", "password");

// ✓ Passes validation
// RegisterUserAsync("user@example.com", "password");
```

---

### NotEmpty<T>(value, paramName)

Validates that a collection is not null and contains at least one element.

**Signature:**

```csharp
public static IEnumerable<T> NotEmpty<T>(IEnumerable<T>? value, string paramName)
```

**Behavior:**
- Throws `ArgumentException` if null or has zero elements
- Returns the collection as-is
- Uses `.Any()` for O(1) check on indexed collections

**Usage:**

```csharp
public async Task BulkCreateProductsAsync(IEnumerable<ProductDto> products)
{
    products = Guard.NotEmpty(products, nameof(products));
    // Now guaranteed non-null with at least 1 item
    
    foreach (var productDto in products)
    {
        var product = new Product 
        { 
            Name = productDto.Name,
            Sku = productDto.Sku 
        };
        await _repository.AddAsync(product);
    }
}

// Throws ArgumentException on empty or null
// BulkCreateProductsAsync(new List<ProductDto>());  // ❌
// BulkCreateProductsAsync(null);                     // ❌

// ✓ Passes validation
// BulkCreateProductsAsync(new[] { product1, product2 });
```

---

### NotDefault<T>(value, paramName)

Validates that a value is not the default for its type (0 for int, Guid.Empty for Guid, null for reference types, etc.).

**Signature:**

```csharp
public static T NotDefault<T>(T value, string paramName)
```

**Behavior:**
- Throws `ArgumentException` if value equals `default(T)`
- Uses `EqualityComparer<T>.Default` for proper comparison
- Useful for required IDs, required enums, non-zero prices

**Usage:**

```csharp
public async Task UpdateProductAsync(Guid productId, decimal newPrice)
{
    productId = Guard.NotDefault(productId, nameof(productId));     // Not Guid.Empty
    newPrice = Guard.NotDefault(newPrice, nameof(newPrice));        // Not 0m
    
    var product = await _repository.GetByIdAsync(productId);
    product.Price = newPrice;
    await _repository.SaveAsync(product);
}

// ❌ Throws ArgumentException
// UpdateProductAsync(Guid.Empty, 19.99m);
// UpdateProductAsync(Guid.NewGuid(), 0m);

// ✓ Passes validation
// UpdateProductAsync(Guid.NewGuid(), 19.99m);
```

---

### InRange<T>(value, min, max, paramName)

Validates that a value is within an inclusive range [min, max].

**Signature:**

```csharp
public static T InRange<T>(T value, T min, T max, string paramName) 
    where T : IComparable<T>
```

**Behavior:**
- Throws `ArgumentOutOfRangeException` if value < min or value > max
- Uses `IComparable<T>` for type-safe comparison
- Works with int, decimal, double, DateTime, etc.

**Usage:**

```csharp
public async Task ListProductsAsync(int pageNumber, int pageSize)
{
    pageNumber = Guard.InRange(pageNumber, 1, int.MaxValue, nameof(pageNumber));
    pageSize = Guard.InRange(pageSize, 1, 100, nameof(pageSize));
    
    // Now safe to use — pageNumber >= 1 and pageSize in [1,100]
    var skip = (pageNumber - 1) * pageSize;
    var products = await _repository.GetPageAsync(skip, pageSize);
}

// Throws ArgumentOutOfRangeException
// ListProductsAsync(0, 50);           // ❌ pageNumber must be >= 1
// ListProductsAsync(1, 101);          // ❌ pageSize must be <= 100

// ✓ Passes validation
// ListProductsAsync(1, 50);
```

**DateTime example:**

```csharp
public void SetDeadline(DateTime deadline)
{
    var now = DateTime.UtcNow;
    deadline = Guard.InRange(deadline, now, now.AddYears(1), nameof(deadline));
    // deadline is guaranteed to be 1 year from now, at most
}
```

---

### Requires(condition, paramName, message)

Custom condition validation — throws if boolean condition is false.

**Signature:**

```csharp
public static void Requires(bool condition, string paramName, string message)
```

**Behavior:**
- Throws `ArgumentException` if condition is false
- No return value — used for side-effect validation
- Use for multi-parameter dependencies and business rules

**Usage:**

```csharp
public void SetDateRange(DateTime startDate, DateTime endDate)
{
    Guard.NotDefault(startDate, nameof(startDate));
    Guard.NotDefault(endDate, nameof(endDate));
    
    // Custom validation — endDate must be after startDate
    Guard.Requires(
        endDate > startDate, 
        nameof(endDate),
        "End date must be after start date"
    );
    
    // All guards passed — safe to proceed
}

// ❌ Throws ArgumentException with custom message
// SetDateRange(DateTime.Now.AddDays(5), DateTime.Now);

// ✓ Passes validation
// SetDateRange(DateTime.Now, DateTime.Now.AddDays(5));
```

**Combining Guards:**

```csharp
public void CreateUserAccount(
    string email, 
    string password, 
    string confirmPassword)
{
    email = Guard.NotEmpty(email, nameof(email));
    password = Guard.NotEmpty(password, nameof(password));
    confirmPassword = Guard.NotEmpty(confirmPassword, nameof(confirmPassword));
    
    // Multi-parameter dependency
    Guard.Requires(
        password == confirmPassword,
        nameof(confirmPassword),
        "Passwords do not match"
    );
    
    // All validations passed
    var user = new User { Email = email, PasswordHash = HashPassword(password) };
}
```

---

## IMapper: Service-Based Object Transformation

The `IMapper` interface provides a service-oriented approach to object transformation between types.

### IMapper Interface Reference

```csharp
namespace SmartWorkz.Core.Shared.Mapping;

public interface IMapper
{
    // Synchronous mapping
    TTarget Map<TSource, TTarget>(TSource source) 
        where TSource : class 
        where TTarget : class;
    
    object Map(object source, Type sourceType, Type targetType);
    
    // Asynchronous mapping (for profiles with async operations)
    Task<TTarget> MapAsync<TSource, TTarget>(
        TSource source, 
        CancellationToken cancellationToken = default)
        where TSource : class 
        where TTarget : class;
    
    // Collection mapping
    IEnumerable<TTarget> MapCollection<TSource, TTarget>(
        IEnumerable<TSource> sources)
        where TSource : class 
        where TTarget : class;
    
    Task<IEnumerable<TTarget>> MapCollectionAsync<TSource, TTarget>(
        IEnumerable<TSource> sources,
        CancellationToken cancellationToken = default)
        where TSource : class 
        where TTarget : class;
    
    // Register mapping profiles
    void RegisterProfile<TSource, TTarget>(
        IMapperProfile<TSource, TTarget> profile)
        where TSource : class 
        where TTarget : class;
}
```

---

### Map<TSource, TTarget>(source)

Maps a single source object to a target type synchronously.

**Usage:**

```csharp
public class UserService
{
    private readonly IMapper _mapper;
    
    public UserService(IMapper mapper)
    {
        _mapper = Guard.NotNull(mapper, nameof(mapper));
    }
    
    public UserDto GetUserDto(User user)
    {
        user = Guard.NotNull(user, nameof(user));
        
        // Map entity to DTO
        var dto = _mapper.Map<User, UserDto>(user);
        return dto;
    }
}

// In controller
public IActionResult GetUser(string userId)
{
    var user = _userRepository.GetById(userId);
    return Ok(_userService.GetUserDto(user));
}
```

---

### MapAsync<TSource, TTarget>(source)

Maps asynchronously — use when profile contains async operations (e.g., async validation, external API calls).

**Usage:**

```csharp
public class OrderService
{
    private readonly IMapper _mapper;
    
    public OrderService(IMapper mapper)
    {
        _mapper = Guard.NotNull(mapper, nameof(mapper));
    }
    
    public async Task<OrderDto> GetOrderDtoAsync(Order order, CancellationToken ct)
    {
        order = Guard.NotNull(order, nameof(order));
        
        // Profile may await price calculations, shipping lookups, etc.
        var dto = await _mapper.MapAsync<Order, OrderDto>(order, ct);
        return dto;
    }
}

// In controller
public async Task<IActionResult> GetOrderAsync(string orderId)
{
    var order = await _orderRepository.GetByIdAsync(orderId);
    var dto = await _orderService.GetOrderDtoAsync(order, HttpContext.RequestAborted);
    return Ok(dto);
}
```

---

### MapCollection<TSource, TTarget>(sources)

Maps a collection of items synchronously.

**Usage:**

```csharp
public async Task<IEnumerable<ProductDto>> GetProductDtosAsync(
    IEnumerable<Product> products)
{
    products = Guard.NotEmpty(products, nameof(products));
    
    // Maps all products to DTOs
    var dtos = _mapper.MapCollection<Product, ProductDto>(products);
    return dtos;
}

// In repository
public async Task<IActionResult> ListProducts(int pageNumber, int pageSize)
{
    pageNumber = Guard.InRange(pageNumber, 1, int.MaxValue, nameof(pageNumber));
    pageSize = Guard.InRange(pageSize, 1, 100, nameof(pageSize));
    
    var skip = (pageNumber - 1) * pageSize;
    var products = await _productRepository.GetPageAsync(skip, pageSize);
    
    var dtos = _mapper.MapCollection<Product, ProductDto>(products);
    return Ok(new { data = dtos, page = pageNumber, pageSize = pageSize });
}
```

---

### MapCollectionAsync<TSource, TTarget>(sources)

Maps a collection asynchronously.

**Usage:**

```csharp
public async Task<IEnumerable<OrderDto>> GetOrderDtosAsync(
    IEnumerable<Order> orders,
    CancellationToken ct)
{
    orders = Guard.NotEmpty(orders, nameof(orders));
    
    // Each order mapping may have async operations
    var dtos = await _mapper.MapCollectionAsync<Order, OrderDto>(orders, ct);
    return dtos;
}
```

---

### RegisterProfile<TSource, TTarget>(profile)

Registers a custom mapping profile for a source-target type pair.

**Usage:**

```csharp
// In startup or dependency injection setup
var mapper = new MapperService();

mapper.RegisterProfile(
    new UserToUserDtoProfile()
);

// Now mapper.Map<User, UserDto>() uses the registered profile
```

---

## IMapperProfile: Custom Mapping Strategies

Mapper profiles define the transformation logic between specific source and target types.

### IMapperProfile Interface Reference

```csharp
namespace SmartWorkz.Core.Shared.Mapping;

/// <summary>
/// Base profile interface (untyped).
/// </summary>
public interface IMapperProfile
{
    Type SourceType { get; }
    Type TargetType { get; }
}

/// <summary>
/// Typed profile for strong typing.
/// </summary>
public interface IMapperProfile<TSource, TTarget> : IMapperProfile
    where TSource : class
    where TTarget : class
{
    /// <summary>
    /// Transform source to target synchronously.
    /// </summary>
    TTarget Map(TSource source);
    
    /// <summary>
    /// Transform source to target asynchronously.
    /// </summary>
    Task<TTarget> MapAsync(TSource source, CancellationToken cancellationToken = default);
}
```

---

### Creating a Custom Mapper Profile

Implement `IMapperProfile<TSource, TTarget>` to define transformation logic.

**Basic Profile:**

```csharp
public class UserToUserDtoProfile : IMapperProfile<User, UserDto>
{
    public Type SourceType => typeof(User);
    public Type TargetType => typeof(UserDto);
    
    public UserDto Map(User source)
    {
        Guard.NotNull(source, nameof(source));
        
        return new UserDto
        {
            Id = source.Id,
            Email = source.Email,
            FirstName = source.FirstName,
            LastName = source.LastName,
            IsActive = source.IsActive,
            CreatedOn = source.CreatedOn
        };
    }
    
    public Task<UserDto> MapAsync(User source, CancellationToken cancellationToken = default)
    {
        // For simple transformations, MapAsync can just call Map
        return Task.FromResult(Map(source));
    }
}
```

**Profile with Nested Mapping:**

```csharp
public class OrderToOrderDtoProfile : IMapperProfile<Order, OrderDto>
{
    private readonly IMapper _mapper;
    
    public Type SourceType => typeof(Order);
    public Type TargetType => typeof(OrderDto);
    
    public OrderToOrderDtoProfile(IMapper mapper)
    {
        _mapper = Guard.NotNull(mapper, nameof(mapper));
    }
    
    public OrderDto Map(Order source)
    {
        Guard.NotNull(source, nameof(source));
        
        return new OrderDto
        {
            Id = source.Id,
            OrderNumber = source.OrderNumber,
            OrderDate = source.OrderDate,
            TotalAmount = source.TotalAmount,
            
            // Map customer using registered profile
            Customer = _mapper.Map<Customer, CustomerDto>(source.Customer),
            
            // Map nested collection
            Items = _mapper.MapCollection<OrderItem, OrderItemDto>(source.Items)
                .ToList()
        };
    }
    
    public Task<OrderDto> MapAsync(Order source, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Map(source));
    }
}
```

**Async Profile with External Operations:**

```csharp
public class ProductToProductDtoProfile : IMapperProfile<Product, ProductDto>
{
    private readonly IInventoryService _inventoryService;
    
    public Type SourceType => typeof(Product);
    public Type TargetType => typeof(ProductDto);
    
    public ProductToProductDtoProfile(IInventoryService inventoryService)
    {
        _inventoryService = Guard.NotNull(inventoryService, nameof(inventoryService));
    }
    
    public ProductDto Map(Product source)
    {
        Guard.NotNull(source, nameof(source));
        
        return new ProductDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Price = source.Price
            // StockLevel is populated in MapAsync
        };
    }
    
    public async Task<ProductDto> MapAsync(Product source, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(source, nameof(source));
        
        var dto = Map(source);
        
        // Async operation — fetch current stock level
        dto.StockLevel = await _inventoryService
            .GetStockLevelAsync(source.Id, cancellationToken);
        
        return dto;
    }
}
```

---

## Validation: ValidatorBase & ValidatorBuilder

SmartWorkz provides two approaches to building validators:

1. **ValidatorBase<T>** — Inheritance-based validators (traditional approach)
2. **ValidatorBuilder<T>** — Fluent builder-based validators (inline approach)

Both use the same rule system and return `ValidationResult`.

---

### ValidatorBase<T>: Class-Based Validators

Create validators by subclassing `ValidatorBase<T>` and defining rules in the constructor.

**Base Class Reference:**

```csharp
namespace SmartWorkz.Core.Shared.Validation;

public abstract class ValidatorBase<T> : IValidator<T>
{
    // Add a validation rule for a property
    protected RuleBuilder<T, TProperty> RuleFor<TProperty>(
        Expression<Func<T, TProperty>> property);
    
    // Validate an instance
    public virtual async Task<ValidationResult> ValidateAsync(
        T instance, 
        CancellationToken cancellationToken = default);
}
```

**Creating a ValidatorBase<T> Subclass:**

```csharp
public class RegisterRequestValidator : ValidatorBase<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Custom(async email => 
            {
                // Custom async validation
                var pattern = ValidationRules.EmailPattern;
                return Regex.IsMatch(email, pattern);
            }, 
            "Email format is invalid");
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MaxLength(128)
            .Custom(async pwd => pwd.Length >= 8, "Password must be at least 8 characters");
        
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}

// Usage
var validator = new RegisterRequestValidator();
var registerRequest = new RegisterRequest
{
    Email = "user@example.com",
    Password = "SecurePass123!",
    TenantId = "tenant-123"
};

var result = await validator.ValidateAsync(registerRequest);
if (!result.IsValid)
{
    foreach (var failure in result.Failures)
    {
        Console.WriteLine($"{failure.PropertyName}: {failure.Message}");
    }
}
```

---

### ValidatorBuilder<T>: Fluent Inline Validators

Create validators inline using fluent API without creating a new class.

**Class Reference:**

```csharp
namespace SmartWorkz.Core.Shared.Validation;

public sealed class ValidatorBuilder<T> : IValidator<T> where T : class
{
    /// <summary>
    /// Add a rule for a property using fluent API.
    /// </summary>
    public RuleBuilder<T, TProperty> RuleFor<TProperty>(
        Expression<Func<T, TProperty>> propertyExpression);
    
    /// <summary>
    /// Validate instance against all rules.
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(
        T instance, 
        CancellationToken cancellationToken = default);
}
```

**Usage:**

```csharp
// Create validator inline
var validator = new ValidatorBuilder<RegisterRequest>()
    .RuleFor(x => x.Email)
        .NotEmpty()
        .Custom(async email => Regex.IsMatch(email, ValidationRules.EmailPattern), 
                "Invalid email format")
    .RuleFor(x => x.Password)
        .NotEmpty()
        .MaxLength(128)
    .RuleFor(x => x.TenantId)
        .NotEmpty();

// Use it
var result = await validator.ValidateAsync(registerRequest);
```

**Dependency Injection with ValidatorBuilder:**

```csharp
// In controller or service
public class UserController : Controller
{
    private readonly IValidator<CreateUserRequest> _validator;
    
    public UserController()
    {
        // Define validator at initialization
        _validator = new ValidatorBuilder<CreateUserRequest>()
            .RuleFor(x => x.Email)
                .NotEmpty()
                .Custom(async e => IsValidEmail(e), "Email must be valid")
            .RuleFor(x => x.Password)
                .NotEmpty()
                .MaxLength(128)
            .RuleFor(x => x.TenantId)
                .NotEmpty();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _validator.ValidateAsync(request);
        
        if (!result.IsValid)
        {
            return BadRequest(new
            {
                errors = result.Failures.Select(f => new
                {
                    field = f.PropertyName,
                    message = f.Message
                })
            });
        }
        
        // Process request
        return Ok();
    }
}
```

---

### RuleBuilder<T, TProperty>: Available Rules

The fluent rule builder provides chainable validation methods.

**Signature:**

```csharp
public sealed class RuleBuilder<T, TProperty>
{
    public RuleBuilder<T, TProperty> NotEmpty();
    public RuleBuilder<T, TProperty> MaxLength(int maxLength);
    public RuleBuilder<T, TProperty> GreaterThanOrEqual(IComparable comparable);
    public RuleBuilder<T, TProperty> LessThanOrEqual(IComparable comparable);
    public RuleBuilder<T, TProperty> Custom(
        Func<TProperty, Task<bool>> predicate, 
        string errorMessage);
}
```

---

### NotEmpty()

Validates that a property is not null, empty string, or whitespace.

```csharp
RuleFor(x => x.Name)
    .NotEmpty();  // Name cannot be null, "", or "   "
```

---

### MaxLength(maxLength)

Validates that a string property does not exceed maximum length.

```csharp
RuleFor(x => x.Description)
    .MaxLength(500);  // Description.Length <= 500
```

---

### GreaterThanOrEqual(comparable)

Validates that a comparable value is >= the specified minimum.

```csharp
RuleFor(x => x.Price)
    .GreaterThanOrEqual(0);  // Price >= 0

RuleFor(x => x.StartDate)
    .GreaterThanOrEqual(DateTime.UtcNow);  // Cannot be in past
```

---

### LessThanOrEqual(comparable)

Validates that a comparable value is <= the specified maximum.

```csharp
RuleFor(x => x.PageSize)
    .LessThanOrEqual(100);  // PageSize <= 100

RuleFor(x => x.Discount)
    .LessThanOrEqual(1.0m);  // Discount <= 100%
```

---

### Custom(predicate, errorMessage)

Custom validation logic — predicate returns true if valid.

```csharp
RuleFor(x => x.Email)
    .Custom(async email => 
    {
        var pattern = ValidationRules.EmailPattern;
        return Regex.IsMatch(email, pattern);
    }, 
    "Email format is invalid");

RuleFor(x => x.Password)
    .Custom(async pwd => pwd.Length >= 8, 
            "Password must be at least 8 characters");

// Async predicate — can call async services
RuleFor(x => x.Username)
    .Custom(async username => 
    {
        var exists = await _userService.UsernameExistsAsync(username);
        return !exists;  // Valid if username is NOT taken
    },
    "Username is already taken");
```

---

## CompositeValidator: Combining Multiple Validators

Merge multiple validators into one, combining all failures.

**Class Reference:**

```csharp
namespace SmartWorkz.Core.Shared.Validation;

public sealed class CompositeValidator<T> : IValidator<T> where T : class
{
    public CompositeValidator(params IValidator<T>[] validators);
    
    public void AddValidator(IValidator<T> validator);
    
    public async Task<ValidationResult> ValidateAsync(
        T instance, 
        CancellationToken cancellationToken = default);
}
```

---

### Creating and Using CompositeValidator

```csharp
public class ProductService
{
    private readonly IValidator<ProductDto> _validator;
    
    public ProductService(
        IValidator<ProductDto> basicValidator,
        IValidator<ProductDto> businessRulesValidator)
    {
        // Combine multiple validators
        _validator = new CompositeValidator<ProductDto>(
            basicValidator,
            businessRulesValidator
        );
    }
    
    public async Task<bool> ValidateProductAsync(ProductDto product)
    {
        var result = await _validator.ValidateAsync(product);
        
        return result.IsValid;
    }
}
```

**Adding validators dynamically:**

```csharp
var composite = new CompositeValidator<User>();

composite.AddValidator(new UserBasicValidator());
composite.AddValidator(new UserSecurityValidator());
composite.AddValidator(new UserAuditValidator());

var result = await composite.ValidateAsync(user);

if (!result.IsValid)
{
    // result.Failures contains all failures from all validators
    foreach (var failure in result.Failures)
    {
        Console.WriteLine($"{failure.PropertyName}: {failure.Message}");
    }
}
```

**Failure collection behavior:**

```csharp
var result = await composite.ValidateAsync(invalidUser);

// ValidationResult.Failures is a merged collection from all validators
// If validator1 returns: Email (invalid), Password (too short)
// If validator2 returns: Username (taken)
// Then result.Failures contains all 3 failures

Console.WriteLine(result.IsValid);           // false
Console.WriteLine(result.Failures.Count);    // 3

// Access individual failures
var emailFailure = result.Failures.FirstOrDefault(f => f.PropertyName == "Email");
if (emailFailure != null)
{
    Console.WriteLine(emailFailure.Message);  // "Invalid email format"
}
```

---

## ValidationRules: Pre-Built Regex Patterns

Static regex patterns for common validation scenarios.

**Class Reference:**

```csharp
namespace SmartWorkz.Core.Shared.Validation;

public static class ValidationRules
{
    /// <summary>Email address regex pattern (RFC 5322 simplified).</summary>
    public const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
    
    /// <summary>URL regex pattern.</summary>
    public const string UrlPattern = 
        @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b" +
        @"([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$";
    
    /// <summary>Phone number pattern (10-15 digits).</summary>
    public const string PhonePattern = @"^\d{10,15}$";
    
    /// <summary>Strong password pattern (at least 8 chars, uppercase, number, special).</summary>
    public const string StrongPasswordPattern = 
        @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{8,}$";
    
    /// <summary>Postal code pattern (US).</summary>
    public const string PostalCodePattern = @"^\d{5}(-\d{4})?$";
}
```

---

### Using ValidationRules Patterns

```csharp
var validator = new ValidatorBuilder<UserProfile>()
    .RuleFor(x => x.Email)
        .NotEmpty()
        .Custom(async email => 
            Regex.IsMatch(email, ValidationRules.EmailPattern),
            "Invalid email format")
    
    .RuleFor(x => x.PhoneNumber)
        .NotEmpty()
        .Custom(async phone => 
            Regex.IsMatch(phone, ValidationRules.PhonePattern),
            "Phone must be 10-15 digits")
    
    .RuleFor(x => x.Website)
        .Custom(async url => 
            string.IsNullOrEmpty(url) || 
            Regex.IsMatch(url, ValidationRules.UrlPattern),
            "Invalid URL format")
    
    .RuleFor(x => x.Password)
        .NotEmpty()
        .Custom(async pwd => 
            Regex.IsMatch(pwd, ValidationRules.StrongPasswordPattern),
            "Password must contain uppercase, number, and special character");

var result = await validator.ValidateAsync(userProfile);
```

---

## ValidationResult: Success and Failure

Represents the outcome of validation.

**Class Reference:**

```csharp
namespace SmartWorkz.Core.Shared.Validation;

public sealed class ValidationResult
{
    public ValidationResult(IEnumerable<ValidationFailure>? failures = null);
    
    public bool IsValid => _failures.Count == 0;
    public IReadOnlyCollection<ValidationFailure> Failures { get; }
    
    public static ValidationResult Success();
    public static ValidationResult Failure(ValidationFailure failure);
    public static ValidationResult Failure(params ValidationFailure[] failures);
}
```

**ValidationFailure:**

```csharp
public sealed class ValidationFailure
{
    public ValidationFailure(string propertyName, string message);
    
    public string PropertyName { get; }
    public string Message { get; }
    
    public override string ToString() => $"{PropertyName}: {Message}";
}
```

---

### Using ValidationResult

```csharp
var result = await validator.ValidateAsync(user);

if (result.IsValid)
{
    // Process the valid user
    await _userService.CreateAsync(user);
    return Ok(new { message = "User created successfully" });
}
else
{
    // Build error response
    var errors = result.Failures.Select(f => new
    {
        field = f.PropertyName,
        message = f.Message
    });
    
    return BadRequest(new { errors });
}
```

**Example error response:**

```json
{
  "errors": [
    {
      "field": "Email",
      "message": "Invalid email format"
    },
    {
      "field": "Password",
      "message": "Password must be at least 8 characters"
    }
  ]
}
```

---

## Complete Validation Pipeline Example

Full flow from entry point through validation to API response.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<RegisterRequest> _validator;
    
    public UsersController(
        IUserService userService,
        IValidator<RegisterRequest> validator)
    {
        _userService = Guard.NotNull(userService, nameof(userService));
        _validator = Guard.NotNull(validator, nameof(validator));
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        // Step 1: Guard — fail fast on null input
        request = Guard.NotNull(request, nameof(request));
        
        // Step 2: Validate — declarative rules
        var validationResult = await _validator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
            // Step 3: Return 400 with validation errors
            return BadRequest(new
            {
                success = false,
                errors = validationResult.Failures.Select(f => new
                {
                    field = f.PropertyName,
                    message = f.Message
                }).ToArray()
            });
        }
        
        // Step 4: Business logic — all input is validated
        try
        {
            var user = await _userService.RegisterAsync(
                request.Email,
                request.Password,
                request.TenantId
            );
            
            return Created($"/api/users/{user.Id}", new
            {
                success = true,
                data = user
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred"
            });
        }
    }
}

// Validator
public class RegisterRequestValidator : ValidatorBase<RegisterRequest>
{
    private readonly IUserService _userService;
    
    public RegisterRequestValidator(IUserService userService)
    {
        _userService = Guard.NotNull(userService, nameof(userService));
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .Custom(async email => 
                Regex.IsMatch(email, ValidationRules.EmailPattern),
                "Invalid email format")
            .Custom(async email =>
            {
                var exists = await _userService.EmailExistsAsync(email);
                return !exists;
            },
            "Email already registered");
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MaxLength(128)
            .Custom(async pwd =>
                Regex.IsMatch(pwd, ValidationRules.StrongPasswordPattern),
                "Password must contain uppercase letter, digit, and special character");
        
        RuleFor(x => x.TenantId)
            .NotEmpty();
    }
}
```

---

## Best Practices

### 1. Guard at Entry Points

Use Guards immediately in public method signatures to prevent invalid state propagation.

```csharp
public async Task<UserDto> GetUserAsync(string userId)
{
    // ✓ Guard first — fail immediately on invalid input
    userId = Guard.NotEmpty(userId, nameof(userId));
    
    var user = await _repository.GetByIdAsync(userId);
    // Now safe — userId is guaranteed non-empty
    
    return _mapper.Map<User, UserDto>(user);
}
```

### 2. Separate Guard from Validation

- **Guard** — Quick structural checks at method entry (null, empty, range)
- **Validate** — Complex business rule validation (async, multi-field, database checks)

```csharp
public async Task CreateUserAsync(RegisterRequest request)
{
    // Guard — immediate checks
    request = Guard.NotNull(request, nameof(request));
    
    // Validate — business rules
    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        throw new ValidationException(validationResult.Failures);
    
    // Safe to proceed with business logic
}
```

### 3. Use Result Pattern with Mapping

Map validation failures to Result<T> for consistent error handling.

```csharp
public async Task<Result<User>> RegisterUserAsync(RegisterRequest request)
{
    request = Guard.NotNull(request, nameof(request));
    
    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        // Convert validation failures to Result
        var errors = validationResult.Failures
            .Select(f => Error.Validation(f.PropertyName, f.Message))
            .ToArray();
        
        return Result.Fail<User>(errors);
    }
    
    // Business logic
    var user = new User { Email = request.Email };
    await _repository.SaveAsync(user);
    
    return Result.Ok(user);
}
```

### 4. Compose Validators for Complex Scenarios

Use CompositeValidator when you have multiple validation concerns.

```csharp
// Basic validation + security validation + business rules
var validator = new CompositeValidator<Order>(
    new OrderBasicValidator(),
    new OrderSecurityValidator(),
    new OrderBusinessRulesValidator()
);

var result = await validator.ValidateAsync(order);
// All failures from all validators are combined
```

### 5. Leverage Async Validation

Use async custom validators for database/API checks.

```csharp
RuleFor(x => x.Username)
    .NotEmpty()
    .Custom(async username =>
    {
        // Async check — is username taken?
        var exists = await _userService.UsernameExistsAsync(username);
        return !exists;  // Valid if NOT taken
    },
    "Username is already taken");
```

---

## Common Patterns

### Pattern 1: Guard + ValidatorBase

```csharp
public class UserService
{
    private readonly IValidator<User> _validator;
    
    public async Task<Result<User>> CreateUserAsync(User user)
    {
        user = Guard.NotNull(user, nameof(user));
        
        var validation = await _validator.ValidateAsync(user);
        if (!validation.IsValid)
            return Result.Fail<User>(validation.Failures
                .Select(f => Error.Validation(f.PropertyName, f.Message))
                .ToArray());
        
        await _repository.SaveAsync(user);
        return Result.Ok(user);
    }
}
```

### Pattern 2: Guard + ValidatorBuilder (Inline)

```csharp
public class QuickValidationService
{
    public async Task<bool> IsValidEmailAsync(string email)
    {
        email = Guard.NotEmpty(email, nameof(email));
        
        var validator = new ValidatorBuilder<EmailRequest>()
            .RuleFor(x => x.Value)
                .Custom(async _ => 
                    Regex.IsMatch(email, ValidationRules.EmailPattern),
                    "Invalid format");
        
        var result = await validator.ValidateAsync(new { Value = email });
        return result.IsValid;
    }
}
```

### Pattern 3: Mapping + Guard

```csharp
public class ProductController : Controller
{
    private readonly IMapper _mapper;
    
    public IActionResult GetProduct(string id)
    {
        id = Guard.NotEmpty(id, nameof(id));
        
        var product = _productRepository.GetById(id);
        Guard.NotNull(product, nameof(product));  // 404 if not found
        
        var dto = _mapper.Map<Product, ProductDto>(product);
        return Ok(dto);
    }
}
```

---

## Troubleshooting

### "ArgumentNullException: Value cannot be null"

**Cause:** Guard.NotNull failed on null input

```csharp
// ❌ This throws
var service = new UserService(null);

// ✓ Fix: Ensure dependency is registered in DI
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<UserService>();
```

### "ArgumentException: Value cannot be null or whitespace"

**Cause:** Guard.NotEmpty failed on empty/whitespace string

```csharp
// ❌ This throws
var user = _userService.GetUser("   ");

// ✓ Fix: Trim and check before calling
var userId = userInput.Trim();
if (!string.IsNullOrEmpty(userId))
    var user = _userService.GetUser(userId);
```

### "ValidationResult has failures but IsValid is true"

**Cause:** ValidationResult was constructed incorrectly (check implementation)

```csharp
// Verify Failures count matches IsValid
if (result.Failures.Count > 0 && result.IsValid)
    // This indicates a bug in ValidatorBase or ValidatorBuilder
    throw new InvalidOperationException("ValidationResult is inconsistent");
```

### Custom validator not running

**Cause:** Custom predicate must return async Task<bool>

```csharp
// ❌ Wrong — synchronous lambda
RuleFor(x => x.Email)
    .Custom(email => email.Contains("@"), "Invalid email");

// ✓ Correct — async lambda
RuleFor(x => x.Email)
    .Custom(async email => email.Contains("@"), "Invalid email");
```

---

## Quick Reference Table

| Feature | Use Case | Example |
|---------|----------|---------|
| Guard.NotNull | Null check at method entry | `user = Guard.NotNull(user, nameof(user))` |
| Guard.NotEmpty | Non-empty string/collection | `email = Guard.NotEmpty(email, nameof(email))` |
| Guard.InRange | Value within bounds | `pageSize = Guard.InRange(pageSize, 1, 100, nameof(pageSize))` |
| Guard.NotDefault | Non-default value | `id = Guard.NotDefault(id, nameof(id))` |
| Guard.Requires | Custom condition | `Guard.Requires(pwd == confirm, nameof(confirm), "Passwords don't match")` |
| IMapper.Map | Single entity transform | `dto = mapper.Map<User, UserDto>(user)` |
| IMapper.MapCollection | Batch entity transform | `dtos = mapper.MapCollection<User, UserDto>(users)` |
| ValidatorBase | Class-based validator | `public class UserValidator : ValidatorBase<User> { ... }` |
| ValidatorBuilder | Inline fluent validator | `new ValidatorBuilder<User>().RuleFor(x => x.Name).NotEmpty()` |
| CompositeValidator | Combine multiple validators | `new CompositeValidator<T>(val1, val2, val3)` |
| ValidationRules | Pre-built regex patterns | `Regex.IsMatch(email, ValidationRules.EmailPattern)` |

