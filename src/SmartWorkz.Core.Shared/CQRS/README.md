# SmartWorkz.Core.Shared.CQRS

Command Query Responsibility Segregation (CQRS) pattern implementation for clean separation of write and read operations.

## Overview

CQRS separates your application into two distinct models:
- **Commands** — Write operations that modify state (Create, Update, Delete)
- **Queries** — Read operations that return data without modification

This separation enables independent optimization of read/write paths and supports complex event sourcing scenarios.

**Key Problems Solved:**
- Clean separation of concerns between reads and writes
- Different scalability profiles for queries vs. commands
- Easier testing and reasoning about data flow
- Foundation for event sourcing and SAGA patterns
- Reduced likelihood of accidental mutations

## CQRS Pattern Overview

```
User Request
    ↓
┌─────────────────────────────────────────────┐
│  Query vs Command Decision                  │
└─────────────────────────────────────────────┘
    ↙                                    ↘
Read (Query)                        Write (Command)
    ↓                                    ↓
IQueryHandler<TQuery, TResult>  ICommandHandler<TCommand>
    ↓                                    ↓
Access Read Model               Modify Write Model
(Cache, Read DB)                (Event Store, Event Log)
    ↓                                    ↓
Return Data                       Publish Events
    ↓                                    ↓
Response                          Event Handlers
                                  (Update Read Model,
                                   Webhooks, Etc.)
```

## IQuery and IQueryHandler

### IQuery Interface

Represents a read-only request that returns a typed result:

```csharp
/// <summary>
/// Marker interface for query objects that return a result without modifying state.
/// </summary>
/// <typeparam name="TResult">The type of result the query returns.</typeparam>
public interface IQuery<out TResult>
{
}
```

### IQueryHandler Interface

Handles a query and returns the result:

```csharp
/// <summary>
/// Handler for a specific query type.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResult">The result type the query returns.</typeparam>
public interface IQueryHandler<in TQuery, out TResult> 
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the query and returns the result.
    /// </summary>
    Task<TResult> HandleAsync(TQuery query);
}
```

## Quick Start

### 1. Define a Query

```csharp
public class GetUserByIdQuery : IQuery<User>
{
    public GetUserByIdQuery(string userId, string tenantId)
    {
        UserId = userId;
        TenantId = tenantId;
    }

    public string UserId { get; }
    public string TenantId { get; }
}
```

### 2. Implement a Query Handler

```csharp
public class GetUserByIdQueryHandler 
    : IQueryHandler<GetUserByIdQuery, User>
{
    private readonly IUserRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IUserRepository repository,
        ICacheService cacheService,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<User> HandleAsync(GetUserByIdQuery query)
    {
        var cacheKey = $"user:{query.UserId}";

        // Try cache first
        var cached = await _cacheService.GetAsync<User>(
            key: cacheKey,
            tenantId: query.TenantId);

        if (cached.IsSuccess && cached.Data != null)
        {
            _logger.LogInformation(
                "Cache hit for user {UserId}",
                query.UserId);
            return cached.Data;
        }

        // Load from database
        var user = await _repository.GetByIdAsync(
            query.UserId,
            query.TenantId);

        if (user == null)
        {
            _logger.LogWarning(
                "User {UserId} not found",
                query.UserId);
            throw new UserNotFoundException(query.UserId);
        }

        // Cache for future requests
        await _cacheService.SetAsync(
            key: cacheKey,
            value: user,
            ttlMinutes: 30,
            tenantId: query.TenantId);

        _logger.LogInformation(
            "User {UserId} loaded and cached",
            query.UserId);

        return user;
    }
}
```

### 3. Register Handlers in DI

```csharp
// Program.cs
services.AddScoped<
    IQueryHandler<GetUserByIdQuery, User>,
    GetUserByIdQueryHandler>();

// Or use AutoMapper/reflection-based registration
services.Scan(scan =>
    scan.FromApplicationDependencies()
        .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime());
```

### 4. Dispatch Query from Controller

```csharp
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IQueryDispatcher _dispatcher;

    public UserController(IQueryDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        var query = new GetUserByIdQuery(id, User.GetTenantId());
        var result = await _dispatcher.SendAsync(query);
        return Ok(result);
    }
}
```

## Handler Registration Patterns

### Pattern 1: Manual Registration

```csharp
services.AddScoped<
    IQueryHandler<GetUserByIdQuery, User>,
    GetUserByIdQueryHandler>();

services.AddScoped<
    IQueryHandler<ListUsersQuery, List<User>>,
    ListUsersQueryHandler>();
```

**Pros:** Explicit, easy to debug  
**Cons:** Repetitive, manual maintenance

### Pattern 2: Convention-Based Assembly Scanning

```csharp
services.Scan(scan =>
    scan.FromAssemblies(typeof(Program).Assembly)
        .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime());
```

**Pros:** Automatic, minimal setup  
**Cons:** Magic, harder to understand registration order

### Pattern 3: Mediator Pattern with Library

```csharp
// Using MediatR library
services.AddMediatR(typeof(Program).Assembly);

// In controller
var result = await _mediator.Send(new GetUserByIdQuery("user-123", "tenant-456"));
```

**Pros:** Industry standard, well-supported  
**Cons:** Adds dependency, potential performance overhead

## Query Dispatching Example

### Simple Query Dispatcher

```csharp
public interface IQueryDispatcher
{
    Task<TResult> SendAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default);
}

public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryDispatcher> _logger;

    public QueryDispatcher(
        IServiceProvider serviceProvider,
        ILogger<QueryDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<TResult> SendAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var handlerType = typeof(IQueryHandler<,>)
            .MakeGenericType(query.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No handler registered for query {query.GetType().Name}");

        var method = handlerType.GetMethod(
            nameof(IQueryHandler<GetUserByIdQuery, User>.HandleAsync),
            new[] { query.GetType() })
            ?? throw new InvalidOperationException(
                $"Query handler method not found");

        _logger.LogDebug(
            "Dispatching query {QueryType}",
            query.GetType().Name);

        try
        {
            var result = await (Task<TResult>)method
                .Invoke(handler, new object[] { query })!;

            _logger.LogDebug(
                "Query {QueryType} completed successfully",
                query.GetType().Name);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Query {QueryType} failed",
                query.GetType().Name);
            throw;
        }
    }
}

// Register dispatcher
services.AddScoped<IQueryDispatcher, QueryDispatcher>();
```

## Query Examples

### Example 1: Search with Pagination

```csharp
public class SearchUsersQuery : IQuery<PagedResult<User>>
{
    public SearchUsersQuery(
        string tenantId,
        string? nameFilter = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        TenantId = tenantId;
        NameFilter = nameFilter;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public string TenantId { get; }
    public string? NameFilter { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
}

public class SearchUsersQueryHandler
    : IQueryHandler<SearchUsersQuery, PagedResult<User>>
{
    private readonly IUserRepository _repository;

    public async Task<PagedResult<User>> HandleAsync(
        SearchUsersQuery query)
    {
        var result = await _repository.SearchAsync(
            query.TenantId,
            query.NameFilter,
            query.PageNumber,
            query.PageSize);

        return result;
    }
}
```

### Example 2: Complex Aggregation

```csharp
public class GetUserWithRelatedDataQuery : IQuery<UserDetailDto>
{
    public GetUserWithRelatedDataQuery(string userId, string tenantId)
    {
        UserId = userId;
        TenantId = tenantId;
    }

    public string UserId { get; }
    public string TenantId { get; }
}

public class GetUserWithRelatedDataQueryHandler
    : IQueryHandler<GetUserWithRelatedDataQuery, UserDetailDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IMapper _mapper;

    public async Task<UserDetailDto> HandleAsync(
        GetUserWithRelatedDataQuery query)
    {
        // Load user and related data in parallel
        var user = await _userRepo.GetByIdAsync(query.UserId);
        var orders = await _orderRepo.GetUserOrdersAsync(query.UserId);
        var preferences = await _userRepo.GetPreferencesAsync(query.UserId);

        // Map to DTO with relationships
        return new UserDetailDto
        {
            User = _mapper.Map<UserDto>(user),
            Orders = _mapper.Map<List<OrderDto>>(orders),
            Preferences = _mapper.Map<PreferencesDto>(preferences)
        };
    }
}
```

## Commands vs Queries

### Commands

```csharp
public class CreateUserCommand : ICommand
{
    public CreateUserCommand(
        string email,
        string firstName,
        string lastName,
        string tenantId)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        TenantId = tenantId;
    }

    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string TenantId { get; }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public async Task HandleAsync(CreateUserCommand command)
    {
        var user = new User
        {
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            TenantId = command.TenantId
        };

        await _repository.AddAsync(user);

        // Publish domain event
        await _eventPublisher.PublishAsync(
            new UserCreatedDomainEvent(user.Id, user.Email));
    }
}
```

**Key Differences:**
- Commands return void (fire-and-forget)
- Commands publish domain events
- Commands modify state
- Queries return data
- Queries should never modify state

## Best Practices

### 1. **Keep Queries Stateless**
```csharp
// ✅ Good: No side effects
public async Task<User> HandleAsync(GetUserByIdQuery query)
{
    return await _repository.GetByIdAsync(query.UserId);
}

// ❌ Bad: Modifies state
public async Task<User> HandleAsync(GetUserByIdQuery query)
{
    user.LastAccessedAt = DateTime.UtcNow;
    await _repository.UpdateAsync(user); // Side effect!
    return user;
}
```

### 2. **Use DTOs for Queries**
```csharp
// ✅ Good: Return DTO, not domain model
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserQuery query)
    {
        var user = await _repository.GetByIdAsync(query.UserId);
        return new UserDto { Id = user.Id, Name = user.Name };
    }
}

// ❌ Bad: Expose domain model directly
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, User>
{
    public async Task<User> HandleAsync(GetUserQuery query)
    {
        return await _repository.GetByIdAsync(query.UserId);
    }
}
```

### 3. **Cache Query Results**
```csharp
// ✅ Good: Leverage cache for read-heavy queries
var cached = await _cacheService.GetAsync<Result>(cacheKey, tenantId);
if (cached.IsSuccess && cached.Data != null)
    return cached.Data;

var result = await _repository.GetAsync();
await _cacheService.SetAsync(cacheKey, result, ttlMinutes: 30);
return result;
```

### 4. **Separate Read and Write Models**
```csharp
// ✅ Good: Different schemas for different needs
// Read model optimized for queries
public class UserReadModel
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
}

// Write model optimized for consistency
public class User
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<Order> Orders { get; set; }
}
```

## Related Modules

- **Logging/** — Log query execution and duration
- **Caching/** — Cache query results
- **Webhooks/** — Publish events from commands
- **EventSourcing/** — Source writes from events

## Common Issues

### Query Handler Not Found
1. Verify handler is registered in DI container
2. Check query and result types match interface
3. Ensure handler class implements `IQueryHandler<,>`
4. Check namespace imports

### Slow Query Performance
1. Add caching with appropriate TTL
2. Check for N+1 problems (load related data in parallel)
3. Add database indexes on frequently queried columns
4. Monitor query execution time in logs

### Stale Read Model Data
1. Reduce cache TTL
2. Implement cache invalidation on writes
3. Use event sourcing for eventual consistency
4. Add cache version headers
