# SmartWorkz.Core.Shared - Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans

**Goal:** Complete backend infrastructure by implementing CQRS command/query handlers, UserService, distributed caching (Redis), message queue consumers, and 20+ unit tests to reach 80%+ code coverage.

**Architecture:** 
- CQRS pattern with QueryDispatcher and CommandDispatcher
- IQuery/ICommand/IQueryHandler/ICommandHandler implementations
- Redis L2 distributed cache with L1 memory cache fallback
- MassTransit consumer patterns for event processing
- Comprehensive service layer with unit tests

**Tech Stack:** .NET 9, EF Core, Redis, MassTransit, xUnit, Moq

**Timeline:** 4-5 weeks, 2-3 developers  
**Effort:** ~80 developer days  
**Priority:** CRITICAL - Blocking backend development

---

## Current State vs Target

### Current
- CQRS abstractions only (no handlers/dispatchers)
- 14 files with 93% XML docs
- QueryCacheService (L1 only, no L2)
- UserService interface only (no implementation)
- WebhookPublisher complete
- 0 tests

### Target
- Full CQRS with dispatchers and handlers
- QueryHandler and CommandHandler implementations
- IUserService fully implemented with multi-tenancy
- Redis distributed cache (L1 + L2)
- MassTransit event consumers
- 20+ unit tests with 80%+ coverage
- 100% XML docs maintained

---

## Phase 1: CQRS Implementation (1 week)

### Task 1: QueryDispatcher Implementation

**Files:**
- Create: `src/CQRS/QueryDispatcher.cs`
- Create: `src/CQRS/IQueryDispatcher.cs`
- Create: `tests/CQRS/QueryDispatcherTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
[TestFixture]
public class QueryDispatcherTests
{
    private IServiceProvider _serviceProvider = null!;
    private IQueryDispatcher _dispatcher = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        // Register test query handler
        services.AddScoped<IQueryHandler<GetTestQuery, TestResult>, GetTestQueryHandler>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        
        _serviceProvider = services.BuildServiceProvider();
        _dispatcher = _serviceProvider.GetRequiredService<IQueryDispatcher>();
    }

    [Test]
    public async Task QueryDispatcher_WithValidQuery_InvokesHandler()
    {
        var query = new GetTestQuery { Id = 1 };
        var result = await _dispatcher.DispatchAsync<GetTestQuery, TestResult>(query);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Test", result.Name);
    }

    [Test]
    public void QueryDispatcher_WithUnregisteredQuery_ThrowsInvalidOperation()
    {
        var query = new UnregisteredQuery();
        
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _dispatcher.DispatchAsync<UnregisteredQuery, object>(query));
    }

    [Test]
    public async Task QueryDispatcher_WithCancellationToken_PassesToHandler()
    {
        var cts = new CancellationTokenSource();
        var query = new GetTestQuery { Id = 1 };

        var result = await _dispatcher.DispatchAsync<GetTestQuery, TestResult>(
            query, cts.Token);

        Assert.IsNotNull(result);
    }
}

// Test types
public class GetTestQuery : IQuery<TestResult>
{
    public int Id { get; set; }
}

public record TestResult(int Id, string Name);

public class GetTestQueryHandler : IQueryHandler<GetTestQuery, TestResult>
{
    public Task<TestResult> HandleAsync(GetTestQuery query, CancellationToken ct = default)
    {
        return Task.FromResult(new TestResult(query.Id, "Test"));
    }
}

public class UnregisteredQuery : IQuery<object> { }
```

- [ ] **Step 2: Implement QueryDispatcher**

```csharp
// src/CQRS/IQueryDispatcher.cs
namespace SmartWorkz.Core.Shared.CQRS;

/// <summary>
/// Routes queries to registered handlers using service container resolution.
/// Supports async handler invocation with cancellation token support.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatches a query to its registered handler asynchronously.
    /// </summary>
    /// <typeparam name="TQuery">Query type implementing IQuery&lt;TResult&gt;.</typeparam>
    /// <typeparam name="TResult">Result type returned by query handler.</typeparam>
    /// <param name="query">Query instance to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token for handler.</param>
    /// <returns>Query result from handler.</returns>
    /// <exception cref="InvalidOperationException">When no handler registered for query type.</exception>
    Task<TResult> DispatchAsync<TQuery, TResult>(
        TQuery query, 
        CancellationToken cancellationToken = default) 
        where TQuery : IQuery<TResult>;
}

// src/CQRS/QueryDispatcher.cs
namespace SmartWorkz.Core.Shared.CQRS;

using System.Reflection;

/// <summary>
/// Routes queries to handlers using reflection-based service resolution.
/// Discovers IQueryHandler implementations from service container.
/// </summary>
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _handlerCache = new();
    private readonly object _cacheLock = new();

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResult> DispatchAsync<TQuery, TResult>(
        TQuery query, 
        CancellationToken cancellationToken = default) 
        where TQuery : IQuery<TResult>
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryType = typeof(TQuery);
        var handlerType = GetHandlerType(queryType, typeof(TResult));

        if (handlerType == null)
            throw new InvalidOperationException(
                $"No handler registered for query {queryType.Name} -> {typeof(TResult).Name}");

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"Handler {handlerType.Name} could not be resolved from service container");

        var handleMethod = handlerType
            .GetMethod("HandleAsync", 
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { queryType, typeof(CancellationToken) },
                null)
            ?? throw new InvalidOperationException(
                $"Handler {handlerType.Name} missing HandleAsync method");

        var result = handleMethod.Invoke(handler, new object[] { query, cancellationToken });
        if (result is Task<TResult> task)
        {
            return await task.ConfigureAwait(false);
        }

        throw new InvalidOperationException(
            $"Handler method did not return Task<{typeof(TResult).Name}>");
    }

    private Type? GetHandlerType(Type queryType, Type resultType)
    {
        lock (_cacheLock)
        {
            if (_handlerCache.TryGetValue(queryType, out var cached))
                return cached;
        }

        var handlerInterfaceType = typeof(IQueryHandler<,>)
            .MakeGenericType(queryType, resultType);

        var handler = _serviceProvider.GetService(handlerInterfaceType)?.GetType();

        if (handler != null)
        {
            lock (_cacheLock)
            {
                _handlerCache[queryType] = handler;
            }
        }

        return handler;
    }
}
```

- [ ] **Step 3: Run tests**

```bash
cd c:\Users\tsent\source\repos\S2Sys\SmartWorkz.Core.Shared
dotnet test tests/CQRS/QueryDispatcherTests.cs -v
```

Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add src/CQRS/QueryDispatcher.cs src/CQRS/IQueryDispatcher.cs tests/CQRS/QueryDispatcherTests.cs
git commit -m "feat(shared): implement QueryDispatcher with reflection-based handler routing"
```

---

### Task 2: CommandDispatcher Implementation (Similar to QueryDispatcher)

- [ ] **Step 1:** Write tests for CommandDispatcher
- [ ] **Step 2:** Implement CommandDispatcher following Query pattern
- [ ] **Step 3:** Run tests
- [ ] **Step 4:** Commit

**Effort:** 1-2 days

---

### Task 3: DI Extension for CQRS

**Files:**
- Create: `src/Extensions/CqrsServiceCollectionExtensions.cs`

```csharp
// src/Extensions/CqrsServiceCollectionExtensions.cs
namespace SmartWorkz.Core.Shared.Extensions;

using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Shared.CQRS;

/// <summary>
/// Extension methods for registering CQRS components in dependency injection.
/// Automatically discovers and registers all query and command handlers.
/// </summary>
public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    /// Registers CQRS dispatcher, query handlers, and command handlers.
    /// Performs assembly scanning to discover handlers implementing IQueryHandler and ICommandHandler.
    /// </summary>
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var scanAssemblies = assemblies.Length > 0 
            ? assemblies 
            : new[] { System.Reflection.Assembly.GetCallingAssembly() };

        // Register dispatchers
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        // Auto-register query handlers
        foreach (var assembly in scanAssemblies)
        {
            var queryHandlerType = typeof(IQueryHandler<,>);
            var handlers = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == queryHandlerType))
                .ToList();

            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces()
                    .Where(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == queryHandlerType)
                    .ToList();

                foreach (var iface in interfaces)
                {
                    services.AddScoped(iface, handler);
                }
            }
        }

        // Auto-register command handlers (similar pattern)
        var commandHandlerType = typeof(ICommandHandler<>);
        foreach (var assembly in scanAssemblies)
        {
            var handlers = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == commandHandlerType))
                .ToList();

            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces()
                    .Where(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == commandHandlerType)
                    .ToList();

                foreach (var iface in interfaces)
                {
                    services.AddScoped(iface, handler);
                }
            }
        }

        return services;
    }
}
```

**Effort:** 1 day

---

## Phase 2: IUserService Implementation (2-3 days)

### Task 4: UserService Implementation

**Files:**
- Modify: `src/Services/IUserService.cs` (add more methods)
- Create: `src/Services/UserService.cs`
- Create: `tests/Services/UserServiceTests.cs`

- [ ] **Step 1:** Expand IUserService interface

```csharp
public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto> CreateUserAsync(CreateUserCommand command);
    Task<UserDto> UpdateUserAsync(string id, UpdateUserCommand command);
    Task DeleteUserAsync(string id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string id);
}
```

- [ ] **Step 2:** Implement UserService with caching and multi-tenancy

```csharp
public class UserService : IUserService
{
    private readonly IDbContext _db;
    private readonly IQueryCacheService _cache;
    private readonly ITenantContext _tenantContext;

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var tenantId = _tenantContext.CurrentTenantId;
        var cacheKey = $"users:all:tenant:{tenantId}";
        
        return await _cache.GetOrSetAsync(
            cacheKey,
            async () => await _db.Users
                .Where(u => u.TenantId == tenantId)
                .AsNoTracking()
                .Select(u => new UserDto(
                    u.Id, u.Email, u.FirstName, u.LastName))
                .ToListAsync(),
            TimeSpan.FromHours(1));
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        var tenantId = _tenantContext.CurrentTenantId;
        var cacheKey = $"user:{id}:tenant:{tenantId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async () => await _db.Users
                .Where(u => u.Id == id && u.TenantId == tenantId)
                .AsNoTracking()
                .Select(u => new UserDto(
                    u.Id, u.Email, u.FirstName, u.LastName))
                .FirstOrDefaultAsync(),
            TimeSpan.FromHours(1));
    }

    public async Task<UserDto> CreateUserAsync(CreateUserCommand command)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            TenantId = _tenantContext.CurrentTenantId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        InvalidateUserCache();

        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName);
    }

    // ... other methods with cache invalidation

    private void InvalidateUserCache()
    {
        var tenantId = _tenantContext.CurrentTenantId;
        _cache.Invalidate($"users:all:tenant:{tenantId}");
        // Invalidate other cache keys as needed
    }
}
```

- [ ] **Step 3:** Write unit tests

```csharp
[TestFixture]
public class UserServiceTests
{
    private UserService _service = null!;
    private Mock<IDbContext> _mockDb = null!;
    private Mock<IQueryCacheService> _mockCache = null!;
    private Mock<ITenantContext> _mockTenantContext = null!;

    [SetUp]
    public void Setup()
    {
        _mockDb = new Mock<IDbContext>();
        _mockCache = new Mock<IQueryCacheService>();
        _mockTenantContext = new Mock<ITenantContext>();

        _mockTenantContext.Setup(x => x.CurrentTenantId).Returns("tenant-1");

        _service = new UserService(
            _mockDb.Object,
            _mockCache.Object,
            _mockTenantContext.Object);
    }

    [Test]
    public async Task GetUserByIdAsync_WithValidId_ReturnsCachedUser()
    {
        var userId = "user-123";
        var expectedUser = new UserDto(
            userId, "user@example.com", "John", "Doe");

        _mockCache.Setup(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<UserDto>>>(),
            It.IsAny<TimeSpan>()))
            .ReturnsAsync(expectedUser);

        var result = await _service.GetUserByIdAsync(userId);

        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.Id);
        _mockCache.Verify(x => x.GetOrSetAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<UserDto>>>(),
            It.IsAny<TimeSpan>()), Times.Once);
    }

    [Test]
    public async Task GetUserByIdAsync_WithNullId_ThrowsArgumentNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _service.GetUserByIdAsync(null!));
    }
}
```

- [ ] **Step 4:** Run tests and commit

```bash
dotnet test tests/Services/UserServiceTests.cs -v
git add src/Services/ tests/Services/UserServiceTests.cs
git commit -m "feat(shared): implement UserService with multi-tenancy and caching"
```

---

## Phase 3: Redis Distributed Cache (2-3 days)

### Task 5: Redis Cache Service

**Files:**
- Create: `src/Caching/DistributedCacheService.cs`
- Create: `tests/Caching/DistributedCacheServiceTests.cs`

```csharp
// src/Caching/DistributedCacheService.cs
namespace SmartWorkz.Core.Shared.Caching;

using Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// Distributed cache service using Redis L2 cache with optional L1 memory cache fallback.
/// Implements cache-aside pattern and supports async operations.
/// </summary>
public class DistributedCacheService : IDistributedCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache? _memoryCache;
    private readonly JsonSerializerOptions _jsonOptions;

    public DistributedCacheService(
        IDistributedCache distributedCache,
        IMemoryCache? memoryCache = null)
    {
        _distributedCache = distributedCache ?? 
            throw new ArgumentNullException(nameof(distributedCache));
        _memoryCache = memoryCache;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        // Try L1 memory cache first
        if (_memoryCache?.TryGetValue(key, out T? l1Value) == true)
            return l1Value;

        // Try L2 Redis cache
        var bytes = await _distributedCache.GetAsync(key);
        if (bytes == null)
            return default;

        var json = Encoding.UTF8.GetString(bytes);
        var value = JsonSerializer.Deserialize<T>(json, _jsonOptions);

        // Populate L1 cache from L2
        if (value != null && _memoryCache != null)
        {
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
        }

        return value;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        var cacheOptions = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
            cacheOptions.AbsoluteExpirationRelativeToNow = expiration;

        // Set in L2 Redis
        await _distributedCache.SetAsync(key, bytes, cacheOptions);

        // Set in L1 memory cache
        if (_memoryCache != null)
        {
            var l1Expiration = expiration ?? TimeSpan.FromHours(1);
            _memoryCache.Set(key, value, l1Expiration);
        }
    }

    public async Task RemoveAsync(string key)
    {
        await _distributedCache.RemoveAsync(key);
        _memoryCache?.Remove(key);
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        if (value != null)
            await SetAsync(key, value, expiration);

        return value;
    }
}

// DI Extension
public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedCache(
        this IServiceCollection services,
        string redisConnectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        services.AddScoped<IDistributedCacheService, DistributedCacheService>();

        return services;
    }
}
```

- [ ] **Tests and commit** (1-2 days)

---

## Phase 4: Message Queue Consumers (2-3 days)

### Task 6: MassTransit Consumer Implementations

**Files:**
- Create: `src/Messaging/Consumers/UserCreatedEventConsumer.cs`
- Create: `src/Messaging/Consumers/UserDeletedEventConsumer.cs`
- Create: `tests/Messaging/ConsumerTests.cs`

```csharp
// src/Messaging/Consumers/UserCreatedEventConsumer.cs
namespace SmartWorkz.Core.Shared.Messaging.Consumers;

using MassTransit;
using SmartWorkz.Core.Shared.Events;

/// <summary>
/// Consumes UserCreated events and triggers post-creation workflows.
/// Handles email notification, audit logging, and user profile initialization.
/// </summary>
public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventConsumer> _logger;
    private readonly IUserService _userService;

    public UserCreatedEventConsumer(
        ILogger<UserCreatedEventConsumer> logger,
        IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing UserCreated event for user {UserId}",
                @event.UserId);

            // Send welcome email
            // Initialize user profile
            // Update analytics

            _logger.LogInformation(
                "Successfully processed UserCreated event for user {UserId}",
                @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing UserCreated event for user {UserId}",
                @event.UserId);
            throw;
        }
    }
}

public record UserCreatedEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
```

- [ ] **Implementation** (2-3 days for multiple consumers)

---

## Phase 5: Unit Tests (2-3 days)

### Task 7: Comprehensive Test Suite

- QueryDispatcher tests (4 tests)
- CommandDispatcher tests (4 tests)
- UserService tests (5 tests)
- DistributedCacheService tests (6 tests)
- MassTransit consumer tests (5 tests)
- Integration tests (4 tests)

**Total: 20-28 unit tests**
**Target Coverage: 80%+**

---

## Summary

### Deliverables
- ✅ QueryDispatcher implementation
- ✅ CommandDispatcher implementation
- ✅ CQRS DI extensions
- ✅ UserService full implementation
- ✅ DistributedCacheService (Redis L2)
- ✅ MassTransit consumers (5+ event handlers)
- ✅ 20+ unit tests
- ✅ 100% XML documentation

### Success Metrics
- [ ] QueryDispatcher fully functional
- [ ] CommandDispatcher fully functional
- [ ] UserService with multi-tenancy
- [ ] Redis cache working with L1 fallback
- [ ] 5+ event consumers implemented
- [ ] 80%+ test coverage
- [ ] All documentation complete

### Timeline
- **Phase 1** (1 week): CQRS implementation
- **Phase 2** (2-3 days): UserService
- **Phase 3** (2-3 days): Redis cache
- **Phase 4** (2-3 days): Message consumers
- **Phase 5** (2-3 days): Tests

**Total: 4-5 weeks, 2-3 developers**

---

**Next:** Move to SmartWorkz.Core.External implementation plan
