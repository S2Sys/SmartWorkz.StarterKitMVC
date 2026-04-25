# Result Pattern

The `Result` and `Result<T>` classes provide a structured way to return success/failure outcomes from services without relying on exceptions for control flow. This pattern is used throughout application services.

## Purpose

- **Explicit success/failure:** Every service call returns a predictable outcome
- **Error messages included:** Result carries a message and a collection of errors
- **Type-safe:** Generics for return values; compiler prevents misuse
- **No exception handling needed:** For expected failures (validation, not found, etc.)

## Quick Reference

```csharp
// Create success result
return Result.Ok();
return Result.Ok("User saved successfully");
return Result.Ok(data);

// Create failure result
return Result.Fail("Email already registered");
return Result.Fail(new[] { "Invalid email", "Password too weak" });

// Check result in caller
if (result.IsSuccess)
    // Handle success
if (result.IsFailure)
    // Handle failure
    
var message = result.Message;        // Success message or error description
var errors = result.Errors;          // Collection of error strings
var data = result.Data;              // For Result<T>
```

## Architecture

### Result Class

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; }
    public IReadOnlyList<string> Errors { get; }

    public static Result Ok(string message = "Success")
    public static Result Fail(string error)
    public static Result Fail(IEnumerable<string> errors)
}
```

### Result\<T> Class

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Data { get; }
    public string Message { get; }
    public IReadOnlyList<string> Errors { get; }

    public static Result<T> Ok(T data, string message = "Success")
    public static Result<T> Fail(string error)
    public static Result<T> Fail(IEnumerable<string> errors)
}
```

## Quick Start

### Service Layer

Define a service that returns `Result`:

```csharp
public class UserService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;

    public async Task<Result> RegisterAsync(Guid tenantId, RegisterInput input)
    {
        // Validation
        var exists = await _repo.ExistsByEmailAsync(tenantId, input.Email);
        if (exists)
            return Result.Fail(T(MessageKeys.Auth.EmailAlreadyRegistered));

        if (input.Password != input.ConfirmPassword)
            return Result.Fail(T(MessageKeys.Auth.PasswordMismatch));

        // Success path
        var user = new User
        {
            Email = input.Email,
            PasswordHash = _hasher.Hash(input.Password)
        };

        await _repo.SaveAsync(tenantId, user);
        return Result.Ok(T(MessageKeys.Auth.RegisterSuccess));
    }
}
```

### Page Model

Use the result in a page handler:

```csharp
[BindProperty]
public RegisterInput Input { get; set; }

public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
        return Page();  // Client-side validation failed

    var result = await _userService.RegisterAsync(TenantId, Input);

    if (result.IsFailure)
    {
        AddToastError(result.Message);
        return Page();
    }

    AddToastSuccess(result.Message);
    return Redirect("/login");
}
```

## Patterns

### Pattern 1: Simple Validation

```csharp
public Result ValidateEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return Result.Fail(T(MessageKeys.Validation.Required));

    if (!email.Contains("@"))
        return Result.Fail(T(MessageKeys.Validation.EmailInvalid));

    return Result.Ok();
}
```

### Pattern 2: Dependency Chain

When one service depends on another:

```csharp
public async Task<Result> UpdateUserAsync(Guid tenantId, UpdateInput input)
{
    // First service call
    var emailResult = await ValidateEmailAsync(input.Email);
    if (emailResult.IsFailure)
        return Result.Fail(emailResult.Message);

    // Second service call
    var user = await _repo.GetAsync(tenantId, input.UserId);
    if (user == null)
        return Result.Fail(T(MessageKeys.User.UserNotFound));

    // Success
    user.Email = input.Email;
    await _repo.SaveAsync(tenantId, user);
    return Result.Ok(T(MessageKeys.Crud.SaveSuccess));
}
```

### Pattern 3: Multiple Validations (Collect All Errors)

```csharp
public Result ValidatePassword(string password)
{
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(password))
        errors.Add(T(MessageKeys.Validation.Required));
    else
    {
        if (password.Length < 8)
            errors.Add(T(MessageKeys.Validation.MinLength));
        if (!password.Any(char.IsDigit))
            errors.Add("Password must contain a digit");
    }

    return errors.Any()
        ? Result.Fail(errors)
        : Result.Ok();
}
```

### Pattern 4: Result\<T> with Data Return

```csharp
public async Task<Result<UserDto>> GetUserAsync(Guid tenantId, Guid userId)
{
    var user = await _repo.GetAsync(tenantId, userId);

    if (user == null)
        return Result<UserDto>.Fail(T(MessageKeys.User.UserNotFound));

    var dto = _mapper.Map<UserDto>(user);
    return Result<UserDto>.Ok(dto, T(MessageKeys.Crud.SaveSuccess));
}
```

Usage in page model:

```csharp
public async Task OnGetAsync(Guid userId)
{
    var result = await _userService.GetUserAsync(TenantId, userId);

    if (result.IsFailure)
        return NotFound();

    User = result.Data;  // Access the returned data
}
```

### Pattern 5: Page-Level Error Collection

```csharp
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
        return Page();

    var result = await _service.SaveAsync(TenantId, Input);

    if (result.IsFailure)
    {
        // Add all errors to ModelState for display
        AddErrors(result);
        return Page();
    }

    AddToastSuccess(result.Message);
    return Redirect("/list");
}
```

The `AddErrors()` method (from `BasePage`) does:

```csharp
protected void AddErrors(Result result)
{
    if (result.Errors?.Any() == true)
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error);
}
```

In the view:

```razor
<span asp-validation-summary="All"></span>
```

Shows all collected errors.

## Comparison: Result vs Exceptions

### Result Pattern (Preferred)

```csharp
public async Task<Result> SaveAsync(User user)
{
    if (user.Email == null)
        return Result.Fail("Email is required");  // Expected failure

    await _repo.SaveAsync(user);
    return Result.Ok("User saved");
}
```

**Advantages:**
- Explicit — success and failure paths are obvious
- No exception overhead — for expected failures
- Translatable — messages come from `T()` and database
- Stackable — errors accumulate naturally

### Exception Pattern (Only for Unexpected Errors)

```csharp
public async Task SaveAsync(User user)
{
    if (user.Email == null)
        throw new ArgumentNullException(nameof(user.Email));  // Unexpected

    await _repo.SaveAsync(user);
}
```

**Use only when:**
- The error is truly unexpected (bug, infrastructure failure)
- The exception represents a programmer error (null that should never be null)

## Common Patterns with Repositories

### Repository Returns Result

```csharp
public async Task<Result> DeleteAsync(Guid tenantId, Guid id)
{
    var item = await _context.Items
        .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

    if (item == null)
        return Result.Fail(T(MessageKeys.Crud.NotFound));

    _context.Items.Remove(item);
    await _context.SaveChangesAsync();

    return Result.Ok(T(MessageKeys.Crud.DeleteSuccess));
}
```

### Cascading Results in Service

```csharp
public async Task<Result> ProcessOrderAsync(Guid tenantId, Order order)
{
    // Step 1: Validate inventory
    var stockResult = await CheckStockAsync(tenantId, order.Items);
    if (stockResult.IsFailure)
        return stockResult;  // Return failure as-is

    // Step 2: Process payment
    var paymentResult = await ProcessPaymentAsync(order);
    if (paymentResult.IsFailure)
        return paymentResult;

    // Step 3: Create shipment
    var shipResult = await CreateShipmentAsync(tenantId, order);
    if (shipResult.IsFailure)
        return shipResult;

    return Result.Ok(T(MessageKeys.Crud.SaveSuccess));
}
```

## Common Mistakes

### Mistake 1: Throwing Exceptions for Expected Failures

❌ **Wrong:**
```csharp
public async Task<Result> SaveAsync(User user)
{
    if (user.Email == null)
        throw new InvalidOperationException("Email required");
}
```

✅ **Correct:**
```csharp
public async Task<Result> SaveAsync(User user)
{
    if (user.Email == null)
        return Result.Fail(T(MessageKeys.Validation.Required));
}
```

### Mistake 2: Ignoring Result Status

❌ **Wrong:**
```csharp
var result = await _service.SaveAsync(data);
DoSomethingWithData(result.Data);  // Crashes if failed!
```

✅ **Correct:**
```csharp
var result = await _service.SaveAsync(data);
if (result.IsFailure)
    return Page();
DoSomethingWithData(result.Data);
```

### Mistake 3: Mixing Exceptions and Results

Pick one pattern per service. Don't mix:

❌ **Wrong:**
```csharp
public async Task<Result> SaveAsync(User user)
{
    if (user.Id == Guid.Empty)
        throw new ArgumentException();  // Inconsistent
    
    if (user.Email == null)
        return Result.Fail("Email required");
}
```

✅ **Correct:**
```csharp
public async Task<Result> SaveAsync(User user)
{
    if (user.Id == Guid.Empty)
        return Result.Fail("User ID is required");
    
    if (user.Email == null)
        return Result.Fail("Email required");
}
```

### Mistake 4: Not Translating Error Messages

❌ **Wrong:**
```csharp
return Result.Fail("Email is required");
```

✅ **Correct:**
```csharp
return Result.Fail(T(MessageKeys.Validation.Required));
```

Ensures messages are translated based on user locale and can be changed in the database.

## See Also

- [Base Page Pattern](./03-base-page-pattern.md) — How `AddErrors()` works
- [Translation System](./01-translation-system.md) — How `T()` works
- [Result.cs](../../src/SmartWorkz.StarterKitMVC.Application/Results/Result.cs) — Full implementation
