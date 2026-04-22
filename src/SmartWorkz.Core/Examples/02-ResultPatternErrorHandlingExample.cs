namespace SmartWorkz.Core.Examples;

using SmartWorkz.Shared;

/// <summary>
/// Demonstrates the Result<T> pattern for explicit, type-safe error handling
/// without relying on exceptions for expected business errors.
/// </summary>
/// <remarks>
/// Result Pattern Benefits:
/// - Explicit error handling: Caller must check Succeeded before using data
/// - Type-safe: Compile-time verification of success/failure paths
/// - No try-catch overhead: Expected errors are normal return values
/// - Structured errors: Code and message provide context for logging/UI
/// - Composable: Can chain Results for multi-step operations
///
/// Result Pattern Usage:
/// - Result.Ok(): Success, no data
/// - Result.Ok(data): Success with typed data
/// - Result.Fail(code, message): Failure with error details
/// - result.Succeeded: Check if operation succeeded
/// - result.Data: Access data (only valid if Succeeded == true)
/// - result.Error: Access error details (Code, Message)
///
/// When to Use Result Pattern:
/// - Business logic that can fail (validation, business rules)
/// - Value object creation (EmailAddress.Create, Money.Create)
/// - Service method returns (IService<TEntity, TDto>)
/// - Multi-step operations (step 1, step 2, step 3)
///
/// When NOT to Use:
/// - Unexpected exceptions (programming errors, null refs)
/// - Use regular exceptions for those
/// - Use Guard clauses for precondition failures
/// </remarks>
public class ResultPatternErrorHandlingExample
{
    /// <summary>
    /// Example 1: Creating value objects with Result pattern.
    /// </summary>
    public class ValueObjectCreation
    {
        public void Example_EmailValidation()
        {
            // EmailAddress.Create() returns Result<EmailAddress>
            var emailResult = EmailAddress.Create("user@example.com");

            if (emailResult.Succeeded)
            {
                // Success path: Use the validated email value object
                var email = emailResult.Data!;
                // Store email safely, knowing it passed validation
            }
            else
            {
                // Failure path: Handle validation error
                // Error codes: EMAIL_EMPTY, EMAIL_INVALID, EMAIL_TOO_LONG
                switch (emailResult.Error?.Code)
                {
                    case "EMAIL_EMPTY":
                        System.Console.WriteLine("Email is required");
                        break;
                    case "EMAIL_INVALID":
                        System.Console.WriteLine("Invalid email format");
                        break;
                    case "EMAIL_TOO_LONG":
                        System.Console.WriteLine("Email exceeds 256 characters");
                        break;
                }
            }
        }

        public void Example_MoneyCreation()
        {
            // Money.Create() returns Result<Money>
            var moneyResult = Money.Create(amount: 99.99m, currency: "USD");

            if (!moneyResult.Succeeded)
            {
                // Handle error
                System.Console.WriteLine($"Failed to create money: {moneyResult.Error?.Message}");
                return;
            }

            // Money is valid, use it
            var price = moneyResult.Data!;
            System.Console.WriteLine($"Price: {price}");
        }

        public void Example_CurrencyMismatch()
        {
            // Try to mix currencies (should fail)
            var usd = Money.Create(100m, "USD").Data!;
            var eur = Money.Create(100m, "EUR").Data!;

            var addResult = usd.Add(eur);

            if (!addResult.Succeeded)
            {
                // Error code: CURRENCY_MISMATCH
                System.Console.WriteLine($"Cannot add different currencies: {addResult.Error?.Message}");
            }
        }
    }

    /// <summary>
    /// Example 2: Service methods returning Result.
    /// </summary>
    public class ServiceErrorHandling
    {
        /// <summary>
        /// Conceptual user creation service.
        /// In real usage, this would be injected and use actual repository.
        /// </summary>
        public class User : AuditableEntity
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public record UserDto(int Id, string Name, string Email);
        public record CreateUserDto(string Name, string Email);

        /// <summary>
        /// Service method returning Result<TDto> (Result pattern).
        /// Caller must check Succeeded before accessing Data.
        /// </summary>
        public Result<UserDto> CreateUser(CreateUserDto dto)
        {
            // Step 1: Validate input
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Result.Fail<UserDto>("VALIDATION_ERROR", "Name is required");
            }

            // Step 2: Validate email
            var emailResult = EmailAddress.Create(dto.Email);
            if (!emailResult.Succeeded)
            {
                return Result.Fail<UserDto>(
                    emailResult.Error?.Code ?? "INVALID_EMAIL",
                    emailResult.Error?.Message ?? "Email is invalid");
            }

            // Step 3: Create entity (in real usage, repository.AddAsync)
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email
            };

            // Step 4: Map to DTO
            var userDto = new UserDto(user.Id, user.Name, user.Email);

            // Step 5: Return success result
            return Result.Ok(userDto);
        }

        /// <summary>
        /// Example: Handling service method result.
        /// </summary>
        public void Example_HandleServiceResult()
        {
            var createDto = new CreateUserDto("John Doe", "john@example.com");
            var result = CreateUser(createDto);

            // Check success status explicitly
            if (!result.Succeeded)
            {
                System.Console.WriteLine($"Failed: {result.Error?.Message}");
                return;
            }

            // Only access Data if success
            var newUser = result.Data!;
            System.Console.WriteLine($"Created user: {newUser.Name}");
        }
    }

    /// <summary>
    /// Example 3: Chaining Results for multi-step operations.
    /// </summary>
    public class ResultChaining
    {
        public class Order : AuditableEntity
        {
            public int CustomerId { get; set; }
            public decimal Total { get; set; }
        }

        /// <summary>
        /// Multi-step operation: validate email, then create order.
        /// </summary>
        public Result<Order> CreateOrderWithValidation(string customerEmail, decimal amount)
        {
            // Step 1: Validate email
            var emailResult = EmailAddress.Create(customerEmail);
            if (!emailResult.Succeeded)
            {
                // Propagate email validation failure
                return Result.Fail<Order>("INVALID_EMAIL", emailResult.Error?.Message ?? "Invalid email");
            }

            // Step 2: Validate amount
            var moneyResult = Money.Create(amount, "USD");
            if (!moneyResult.Succeeded)
            {
                // Propagate money validation failure
                return Result.Fail<Order>("INVALID_AMOUNT", moneyResult.Error?.Message ?? "Invalid amount");
            }

            // Both validations passed; create order
            var order = new Order
            {
                CustomerId = 1,
                Total = amount
            };

            return Result.Ok(order);
        }

        /// <summary>
        /// Cascading error handling: if any step fails, operation fails.
        /// </summary>
        public void Example_CascadingErrors()
        {
            var result = CreateOrderWithValidation("invalid-email", -100m);

            if (!result.Succeeded)
            {
                // Both email and amount are invalid; first error is reported
                System.Console.WriteLine($"Operation failed: {result.Error?.Message}");
                return;
            }

            System.Console.WriteLine("Order created successfully");
        }
    }

    /// <summary>
    /// Example 4: Transforming and mapping Results.
    /// </summary>
    public class ResultTransformation
    {
        /// <summary>
        /// Transform one Result<T> into another Result<U>.
        /// </summary>
        public Result<int> GetUserId(string email)
        {
            // Validate email
            var emailResult = EmailAddress.Create(email);
            if (!emailResult.Succeeded)
            {
                return Result.Fail<int>("INVALID_EMAIL", "Email format is invalid");
            }

            // In real usage: look up user by email
            // For now, return mock ID
            return Result.Ok(42);
        }

        /// <summary>
        /// Use Result in a chain of operations.
        /// </summary>
        public void Example_ResultChain()
        {
            var userIdResult = GetUserId("john@example.com");

            if (!userIdResult.Succeeded)
            {
                System.Console.WriteLine($"Failed: {userIdResult.Error?.Message}");
                return;
            }

            var userId = userIdResult.Data;
            System.Console.WriteLine($"Found user ID: {userId}");
        }
    }

    /// <summary>
    /// Example 5: Result in HTTP response mapping.
    /// </summary>
    public class ControllerIntegration
    {
        public record CreateUserRequest(string Name, string Email);

        /// <summary>
        /// HTTP POST endpoint using Result pattern.
        /// Maps Result<T> to HTTP response.
        /// </summary>
        public object CreateUserEndpoint(CreateUserRequest request)
        {
            // Call service (returns Result<UserDto>)
            // var result = await userService.CreateAsync(request);

            // For example, simulate a failure
            var result = Result.Fail<ServiceErrorHandling.UserDto>(
                "DUPLICATE_EMAIL",
                "Email already exists");

            // Map Result to HTTP response
            if (!result.Succeeded)
            {
                return new
                {
                    success = false,
                    error = result.Error?.Code,
                    message = result.Error?.Message
                };
            }

            return new
            {
                success = true,
                data = result.Data
            };
        }
    }
}
