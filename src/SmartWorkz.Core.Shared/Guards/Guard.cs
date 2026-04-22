namespace SmartWorkz.Shared;

/// <summary>
/// Utility class providing guard clause validation methods for argument validation at method entry points.
/// </summary>
/// <remarks>
/// Purpose: Guard clauses validate preconditions and invariants at domain boundaries (entity constructors,
/// value object creation, service method entry points). Fail fast, fail loudly — throw on invalid input.
///
/// Design Philosophy:
/// - Guards are defense against programming errors (invalid method calls, bad data)
/// - Each guard method corresponds to a specific validation scenario
/// - Guards throw immediately on failure — do not allow invalid state to propagate
/// - Guards return the validated value for fluent/chaining usage if desired
///
/// When to Use Guards:
/// - Entity/Value Object constructors: Validate all required fields
/// - Service methods: Validate input parameters before processing
/// - Domain boundaries: Ensure data entering domain is valid
/// - NOT for user input validation (use FluentValidation or similar)
/// - NOT for business rule violations (use domain exceptions like InvalidOperationException)
///
/// Guard Method Naming Convention:
/// - NotNull: Reference type (class) is not null
/// - NotNull (generic struct): Nullable&lt;T&gt; (struct) has value
/// - NotEmpty: String is not null/empty/whitespace OR collection has elements
/// - NotDefault: Value is not the default for its type (0, null, Guid.Empty)
/// - InRange: Value falls within [min, max] inclusive
/// - Requires: Custom boolean condition is true
///
/// Exception Behavior:
/// - NotNull (reference): Throws ArgumentNullException
/// - NotNull (struct): Throws ArgumentNullException
/// - NotEmpty (string): Throws ArgumentException with "Value cannot be null or whitespace"
/// - NotEmpty (collection): Throws ArgumentException with "Collection cannot be null or empty"
/// - NotDefault: Throws ArgumentException with "Value cannot be the default value for {TypeName}"
/// - InRange: Throws ArgumentOutOfRangeException with "Value must be between {min} and {max}"
/// - Requires: Throws ArgumentException with custom message
///
/// All methods include paramName for clear error messages identifying the invalid parameter.
///
/// Usage Pattern (typical entity/VO):
/// public class Customer
/// {
///     private Customer(string name, EmailAddress email, Guid id)
///     {
///         Guard.NotEmpty(name, nameof(name));
///         Guard.NotNull(email, nameof(email));
///         Guard.NotDefault(id, nameof(id));
///
///         Name = name;
///         Email = email;
///         Id = id;
///     }
/// }
///
/// Error Message Pattern:
/// - ArgumentNullException: "{paramName} cannot be null. (Parameter '{paramName}')"
/// - ArgumentException: "Value cannot be null or whitespace. (Parameter '{paramName}')"
/// - ArgumentOutOfRangeException: "Value must be between {min} and {max}. (Parameter '{paramName}')"
/// - Requires: Custom message provided by caller
/// </remarks>
public static class Guard
{
    /// <summary>
    /// Validates that a reference type value is not null.
    /// </summary>
    /// <typeparam name="T">The reference type being validated.</typeparam>
    /// <param name="value">The value to check for null.</param>
    /// <param name="paramName">The parameter name for error messages (use nameof(paramName) for clarity).</param>
    /// <returns>The validated non-null value (allows chaining).</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <remarks>
    /// When to Use:
    /// - Validate entity/VO constructor parameters of reference type (string, object, interface)
    /// - Validate service method parameters that must not be null
    /// - Use for any non-nullable reference type parameter
    ///
    /// When NOT to Use:
    /// - Use NotEmpty for string values (validates null, empty, and whitespace)
    /// - Use NotNull with struct overload for nullable value types (int?, Guid?, etc.)
    /// - Do not use for optional parameters that may legitimately be null
    ///
    /// Error Message: "{paramName} cannot be null. (Parameter '{paramName}')"
    ///
    /// Examples:
    /// - Guard.NotNull(customer, nameof(customer))
    /// - Guard.NotNull(emailService, nameof(emailService))
    /// - Guard.NotNull(configuration, nameof(configuration))
    /// - Guard.NotNull(user.Address, nameof(user.Address)) in constructor
    /// </remarks>
    /// <example>
    /// public class OrderService
    /// {
    ///     private readonly IEmailService _emailService;
    ///     private readonly IOrderRepository _repository;
    ///
    ///     public OrderService(IEmailService emailService, IOrderRepository repository)
    ///     {
    ///         Guard.NotNull(emailService, nameof(emailService));
    ///         Guard.NotNull(repository, nameof(repository));
    ///
    ///         _emailService = emailService;
    ///         _repository = repository;
    ///     }
    ///
    ///     public async Task ConfirmOrderAsync(Order order)
    ///     {
    ///         Guard.NotNull(order, nameof(order));
    ///
    ///         // Process order...
    ///         await _emailService.SendAsync(order.CustomerEmail, "Order Confirmed", "...");
    ///     }
    /// }
    /// </example>
    public static T NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    /// <summary>
    /// Validates that a nullable value type (struct) has a value (is not null).
    /// </summary>
    /// <typeparam name="T">The value type (struct) being validated.</typeparam>
    /// <param name="value">The nullable value to check (e.g., int?, Guid?, DateTime?).</param>
    /// <param name="paramName">The parameter name for error messages (use nameof(paramName) for clarity).</param>
    /// <returns>The unwrapped non-null value (allows chaining).</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null (HasValue is false).</exception>
    /// <remarks>
    /// When to Use:
    /// - Validate nullable value type parameters (Guid?, int?, DateTime?, etc.)
    /// - Use when a required field should not be "missing" (unset)
    /// - Common for ID fields and date/time parameters
    ///
    /// When NOT to Use:
    /// - Use NotNull reference overload for reference types (class, string, interface)
    /// - Do not use for optional nullable fields that may legitimately be null
    /// - Use NotDefault if you need to check for non-zero/non-empty value types
    ///
    /// Behavior:
    /// - Checks HasValue property; throws if false
    /// - Returns unwrapped value (value.Value) on success
    /// - Allows subsequent code to treat value as non-nullable T (not T?)
    ///
    /// Error Message: "{paramName} cannot be null. (Parameter '{paramName}')"
    ///
    /// Examples:
    /// - Guard.NotNull(customerId, nameof(customerId)) // Guid?
    /// - Guard.NotNull(orderId, nameof(orderId)) // int?
    /// - Guard.NotNull(createdDate, nameof(createdDate)) // DateTime?
    /// </remarks>
    /// <example>
    /// public class Customer
    /// {
    ///     public Customer(string name, Guid? id, DateTime? created)
    ///     {
    ///         Guard.NotEmpty(name, nameof(name));
    ///         Guard.NotNull(id, nameof(id)); // Ensure ID has value
    ///         Guard.NotNull(created, nameof(created)); // Ensure creation date set
    ///
    ///         Name = name;
    ///         Id = id.Value; // Now safe to unwrap
    ///         CreatedAt = created.Value;
    ///     }
    /// }
    /// </example>
    public static T NotNull<T>(T? value, string paramName) where T : struct
    {
        if (!value.HasValue)
            throw new ArgumentNullException(paramName);
        return value.Value;
    }

    /// <summary>
    /// Validates that a string is not null, empty, or whitespace-only.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The parameter name for error messages (use nameof(paramName) for clarity).</param>
    /// <returns>The validated non-empty string (allows chaining).</returns>
    /// <exception cref="ArgumentException">Thrown when string is null, empty, or contains only whitespace.</exception>
    /// <remarks>
    /// When to Use:
    /// - Validate string parameters that must have meaningful content
    /// - Use for name fields, email addresses (before format validation), identifiers, etc.
    /// - Most common guard for string parameters
    ///
    /// Behavior:
    /// - Rejects null values (null reference)
    /// - Rejects empty strings ("")
    /// - Rejects whitespace-only strings ("   ", "\t", "\n")
    /// - Accepts any string with at least one non-whitespace character
    ///
    /// When NOT to Use:
    /// - Use NotNull if you explicitly allow empty strings (rare)
    /// - This is almost always preferred over NotNull for strings
    ///
    /// Error Message: "Value cannot be null or whitespace. (Parameter '{paramName}')"
    ///
    /// Validation Note:
    /// - This guards against common programming errors (forgetting to initialize, bad string handling)
    /// - Does NOT validate string format/content (email format, length limits, character restrictions)
    /// - Does NOT validate against user input (use FluentValidation or ASP.NET Core validation)
    ///
    /// Examples:
    /// - Guard.NotEmpty(firstName, nameof(firstName))
    /// - Guard.NotEmpty(lastName, nameof(lastName))
    /// - Guard.NotEmpty(username, nameof(username))
    /// - Guard.NotEmpty(product.Description, nameof(product.Description))
    /// </remarks>
    /// <example>
    /// public class ProductName
    /// {
    ///     public string Value { get; }
    ///
    ///     public ProductName(string value)
    ///     {
    ///         Guard.NotEmpty(value, nameof(value));
    ///         Value = value.Trim(); // Safe to trim; we know it's not empty/whitespace
    ///     }
    /// }
    ///
    /// public async Task CreateProductAsync(string name, string description)
    /// {
    ///     Guard.NotEmpty(name, nameof(name));
    ///     Guard.NotEmpty(description, nameof(description));
    ///
    ///     var product = new Product { Name = name, Description = description };
    ///     await _repository.AddAsync(product);
    /// }
    /// </example>
    public static string NotEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        return value;
    }

    /// <summary>
    /// Validates that a collection is not null and contains at least one element.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="value">The collection to validate (IEnumerable&lt;T&gt;).</param>
    /// <param name="paramName">The parameter name for error messages (use nameof(paramName) for clarity).</param>
    /// <returns>The validated non-empty collection (allows chaining).</returns>
    /// <exception cref="ArgumentException">Thrown when collection is null or empty.</exception>
    /// <remarks>
    /// When to Use:
    /// - Validate collection parameters that must contain at least one element
    /// - Use for batch operations, bulk updates, searches with required criteria
    /// - Works with any IEnumerable&lt;T&gt; implementation (List, Array, IQueryable, etc.)
    ///
    /// Behavior:
    /// - Rejects null references
    /// - Rejects empty collections (no elements)
    /// - Accepts any collection with at least one element
    /// - Calls Any() to check for elements; may enumerate collection once
    ///
    /// Performance Consideration:
    /// - Calls .Any() which may enumerate the collection
    /// - For large collections or lazy sequences, consider if validation is necessary
    /// - For List&lt;T&gt;/Array: O(1) check
    /// - For IEnumerable/LINQ: May enumerate up to first element O(n) worst case
    ///
    /// When NOT to Use:
    /// - Use for actual collection emptiness check in logic; use .Any() or .Count directly
    /// - This guards against programming errors (passing uninitialized collections)
    ///
    /// Error Message: "Collection cannot be null or empty. (Parameter '{paramName}')"
    ///
    /// Examples:
    /// - Guard.NotEmpty(customerIds, nameof(customerIds)) // Batch send notification
    /// - Guard.NotEmpty(searchCriteria, nameof(searchCriteria)) // At least one filter required
    /// - Guard.NotEmpty(orderLines, nameof(orderLines)) // Order must have items
    /// </remarks>
    /// <example>
    /// public async Task SendBulkEmailAsync(IEnumerable&lt;string&gt; recipients, string subject, string body)
    /// {
    ///     Guard.NotEmpty(recipients, nameof(recipients)); // At least one recipient required
    ///
    ///     foreach (var recipient in recipients)
    ///     {
    ///         await _emailService.SendAsync(recipient, subject, body);
    ///     }
    /// }
    ///
    /// public class Order
    /// {
    ///     public List&lt;OrderLine&gt; Lines { get; }
    ///
    ///     public Order(List&lt;OrderLine&gt; lines)
    ///     {
    ///         Guard.NotEmpty(lines, nameof(lines)); // Order must have at least one item
    ///         Lines = lines;
    ///     }
    ///
    ///     public decimal GetTotal() => Lines.Sum(l =&gt; l.Price * l.Quantity);
    /// }
    /// </example>
    public static IEnumerable<T> NotEmpty<T>(IEnumerable<T>? value, string paramName)
    {
        if (value is null || !value.Any())
            throw new ArgumentException("Collection cannot be null or empty.", paramName);
        return value;
    }

    /// <summary>
    /// Validates that a value is not the default value for its type (not 0, empty string, null reference, Guid.Empty, etc.).
    /// </summary>
    /// <typeparam name="T">The type of value being validated.</typeparam>
    /// <param name="value">The value to check against default.</param>
    /// <param name="paramName">The parameter name for error messages (use nameof(paramName) for clarity).</param>
    /// <returns>The validated non-default value (allows chaining).</returns>
    /// <exception cref="ArgumentException">Thrown when value equals the default for its type.</exception>
    /// <remarks>
    /// When to Use:
    /// - Validate numeric IDs are non-zero (int, long, etc.)
    /// - Validate Guid/UUID fields are not Guid.Empty
    /// - Validate enum values are not the default (often zero/first value)
    /// - Use for "required identifier" fields in domain entities
    ///
    /// Type-Specific Defaults:
    /// - int, long, decimal, etc.: 0
    /// - Guid: Guid.Empty (00000000-0000-0000-0000-000000000000)
    /// - bool: false
    /// - enum: 0 (or first enum value)
    /// - string: null (not empty string ""; use NotEmpty for that)
    /// - reference types: null
    ///
    /// Behavior:
    /// - Uses EqualityComparer&lt;T&gt;.Default.Equals(value, default)
    /// - Throws ArgumentException if value equals default(T)
    /// - Error message includes type name for clarity
    ///
    /// When NOT to Use:
    /// - Use NotEmpty for string validation (handles null, empty, whitespace)
    /// - Use NotNull for reference types
    /// - Do not use to validate business logic (e.g., "age must be positive"; use Requires or domain exceptions)
    ///
    /// Error Message: "Value cannot be the default value for {TypeName}. (Parameter '{paramName}')"
    ///
    /// Examples:
    /// - Guard.NotDefault(customerId, nameof(customerId)) // int customerId; must not be 0
    /// - Guard.NotDefault(productId, nameof(productId)) // Guid productId; must not be Guid.Empty
    /// - Guard.NotDefault(status, nameof(status)) // OrderStatus status; must not be default/None
    /// </remarks>
    /// <example>
    /// public class Customer
    /// {
    ///     public Guid Id { get; }
    ///     public string Name { get; }
    ///
    ///     public Customer(Guid id, string name)
    ///     {
    ///         Guard.NotDefault(id, nameof(id)); // Ensure ID is not Guid.Empty
    ///         Guard.NotEmpty(name, nameof(name));
    ///
    ///         Id = id;
    ///         Name = name;
    ///     }
    /// }
    ///
    /// public async Task GetOrderAsync(long orderId)
    /// {
    ///     Guard.NotDefault(orderId, nameof(orderId)); // Must not be 0
    ///
    ///     var order = await _repository.GetByIdAsync(orderId);
    ///     return order ?? throw new NotFoundException("Order not found");
    /// }
    /// </example>
    public static T NotDefault<T>(T value, string paramName)
    {
        if (EqualityComparer<T>.Default.Equals(value, default!))
            throw new ArgumentException($"Value cannot be the default value for {typeof(T).Name}.", paramName);
        return value;
    }

    /// <summary>
    /// Validates that a comparable value falls within a specified range (inclusive [min, max]).
    /// </summary>
    /// <typeparam name="T">The comparable type being validated (must implement IComparable&lt;T&gt;).</typeparam>
    /// <param name="value">The value to validate against the range.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="paramName">The parameter name for error messages (use nameof(paramName) for clarity).</param>
    /// <returns>The validated in-range value (allows chaining).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value &lt; min or value &gt; max.</exception>
    /// <remarks>
    /// When to Use:
    /// - Validate numeric ranges (page size, retry count, timeout duration)
    /// - Validate date ranges (dates must be within valid period)
    /// - Validate any comparable value with min/max bounds
    ///
    /// Range Semantics:
    /// - Inclusive on both ends: [min, max]
    /// - Both min and max are allowed values
    /// - Throws if value &lt; min OR value &gt; max
    ///
    /// Comparable Types Supported:
    /// - Numeric: int, long, decimal, double, float
    /// - Temporal: DateTime, DateOnly, TimeOnly, TimeSpan
    /// - String: Alphabetic/lexicographic comparison
    /// - Custom: Any type implementing IComparable&lt;T&gt;
    ///
    /// Error Message: "Value must be between {min} and {max}. (Parameter '{paramName}')"
    /// Exception Type: ArgumentOutOfRangeException (not ArgumentException)
    ///
    /// Design Notes:
    /// - This is a programming guard; use for domain constraints (not user input validation)
    /// - For user input, use FluentValidation or ASP.NET Core validation
    /// - Exception includes the actual value that caused the range violation
    ///
    /// Examples:
    /// - Guard.InRange(pageSize, 1, 100, nameof(pageSize)) // Page size 1-100
    /// - Guard.InRange(timeout, 0, 60, nameof(timeout)) // Timeout 0-60 seconds
    /// - Guard.InRange(quantity, 1, 1000, nameof(quantity)) // Order quantity 1-1000
    /// - Guard.InRange(startDate, minDate, maxDate, nameof(startDate)) // Date range
    /// </remarks>
    /// <example>
    /// public async Task&lt;List&lt;Product&gt;&gt; SearchAsync(int pageNumber, int pageSize)
    /// {
    ///     Guard.InRange(pageNumber, 1, int.MaxValue, nameof(pageNumber));
    ///     Guard.InRange(pageSize, 1, 100, nameof(pageSize)); // Max 100 per page
    ///
    ///     var skip = (pageNumber - 1) * pageSize;
    ///     return await _repository.GetAsync(skip, pageSize);
    /// }
    ///
    /// public class RetryPolicy
    /// {
    ///     public int MaxRetries { get; }
    ///
    ///     public RetryPolicy(int maxRetries)
    ///     {
    ///         Guard.InRange(maxRetries, 1, 10, nameof(maxRetries)); // 1-10 retries allowed
    ///         MaxRetries = maxRetries;
    ///     }
    /// }
    /// </example>
    public static T InRange<T>(T value, T min, T max, string paramName) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.");
        return value;
    }

    /// <summary>
    /// Validates that a custom boolean condition is true; throws with a custom error message if false.
    /// </summary>
    /// <param name="condition">The condition to validate; must be true for validation to pass.</param>
    /// <param name="paramName">The parameter name for error messages (use nameof(paramName) for clarity).</param>
    /// <param name="message">The custom error message to include in the exception.</param>
    /// <exception cref="ArgumentException">Thrown when condition is false, with the provided message.</exception>
    /// <remarks>
    /// When to Use:
    /// - Validate complex conditions not covered by other guard methods
    /// - Validate combinations of parameters (e.g., startDate &lt; endDate)
    /// - Validate business rules at method entry (e.g., "user must be active")
    /// - Provide custom, context-specific error messages
    ///
    /// Behavior:
    /// - Evaluates condition; throws ArgumentException if false
    /// - Includes custom message and paramName in exception
    /// - Does not return a value (void method)
    ///
    /// When NOT to Use:
    /// - Use specific guards (NotNull, NotEmpty, InRange) when applicable
    /// - Do not use for user input validation (use FluentValidation or ASP.NET Core validation)
    /// - Do not use for business logic errors (throw domain exceptions instead)
    ///
    /// Error Message: Custom message provided by caller. Parameter name also included.
    ///
    /// Examples of Valid Conditions:
    /// - startDate &lt; endDate (date range validation)
    /// - item.Quantity &gt; 0 (positive quantity)
    /// - user.Role == Role.Admin (role check)
    /// - collection1.Count == collection2.Count (matching counts)
    /// </remarks>
    /// <example>
    /// public class DateRange
    /// {
    ///     public DateTime StartDate { get; }
    ///     public DateTime EndDate { get; }
    ///
    ///     public DateRange(DateTime startDate, DateTime endDate)
    ///     {
    ///         Guard.Requires(startDate &lt; endDate, nameof(startDate), "Start date must be before end date.");
    ///
    ///         StartDate = startDate;
    ///         EndDate = endDate;
    ///     }
    /// }
    ///
    /// public async Task ProcessOrderAsync(Order order, User user)
    /// {
    ///     Guard.NotNull(order, nameof(order));
    ///     Guard.NotNull(user, nameof(user));
    ///     Guard.Requires(user.IsActive, nameof(user), "User must be active to place orders.");
    ///     Guard.Requires(order.Total &gt; 0, nameof(order), "Order must have items and total &gt; 0.");
    ///
    ///     // Process order...
    /// }
    ///
    /// public class PageRequest
    /// {
    ///     public int PageNumber { get; }
    ///     public int PageSize { get; }
    ///
    ///     public PageRequest(int pageNumber, int pageSize)
    ///     {
    ///         Guard.Requires(pageNumber &gt; 0, nameof(pageNumber), "Page number must be greater than 0.");
    ///         Guard.Requires(pageSize &gt; 0 &amp;&amp; pageSize &lt;= 100, nameof(pageSize), "Page size must be between 1 and 100.");
    ///
    ///         PageNumber = pageNumber;
    ///         PageSize = pageSize;
    ///     }
    /// }
    /// </example>
    public static void Requires(bool condition, string paramName, string message)
    {
        if (!condition)
            throw new ArgumentException(message, paramName);
    }
}
