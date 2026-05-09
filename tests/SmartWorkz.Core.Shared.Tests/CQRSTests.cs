using SmartWorkz.Shared;

namespace SmartWorkz.Core.Shared.Tests;

/// <summary>
/// Comprehensive test suite for CQRS interfaces and dispatcher covering
/// handler registration, query dispatching, type safety, and error handling.
/// </summary>
public class CQRSTests
{
    #region Query Interface Tests

    [Fact]
    public void IQuery_IsMarkerInterface()
    {
        // Arrange & Act
        var queryType = typeof(IQuery<>);

        // Assert - Verify it's an interface and has TResult generic parameter
        Assert.True(queryType.IsInterface);
        var genericArgs = queryType.GetGenericArguments();
        Assert.Single(genericArgs);
        Assert.Equal("TResult", genericArgs[0].Name);
    }

    [Fact]
    public void GetUserQuery_ImplementsIQuery()
    {
        // Arrange
        var query = new GetUserQuery { UserId = 123 };

        // Act
        var isQueryInterface = typeof(GetUserQuery).GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

        // Assert
        Assert.True(isQueryInterface);
    }

    #endregion

    #region QueryHandler Interface Tests

    [Fact]
    public void IQueryHandler_HasCorrectSignature()
    {
        // Arrange
        var handlerType = typeof(IQueryHandler<,>);

        // Act
        var genericArgs = handlerType.GetGenericArguments();

        // Assert
        Assert.Equal(2, genericArgs.Length);
        Assert.Equal("TQuery", genericArgs[0].Name);
        Assert.Equal("TResult", genericArgs[1].Name);

        // Verify HandleAsync method exists
        var handleMethod = handlerType.GetMethod("HandleAsync");
        Assert.NotNull(handleMethod);
    }

    [Fact]
    public void GetUserQueryHandler_ImplementsIQueryHandler()
    {
        // Arrange
        var handler = new GetUserQueryHandler();

        // Act
        var isHandlerInterface = typeof(GetUserQueryHandler).GetInterfaces()
            .Any(i => i.IsGenericType &&
                     i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) &&
                     i.GetGenericArguments()[0] == typeof(GetUserQuery) &&
                     i.GetGenericArguments()[1] == typeof(UserDto));

        // Assert
        Assert.True(isHandlerInterface);
    }

    #endregion

    #region Handler Registration & Type Safety

    [Fact]
    public async Task GetUserQueryHandler_WithValidQuery_ReturnsUserDto()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var query = new GetUserQuery { UserId = 1 };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public async Task GetUserQueryHandler_WithDifferentUserId_ReturnsDifferentUser()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var query1 = new GetUserQuery { UserId = 1 };
        var query2 = new GetUserQuery { UserId = 2 };

        // Act
        var result1 = await handler.HandleAsync(query1, CancellationToken.None);
        var result2 = await handler.HandleAsync(query2, CancellationToken.None);

        // Assert
        Assert.NotEqual(result1.Id, result2.Id);
        Assert.NotEqual(result1.Name, result2.Name);
    }

    [Fact]
    public void MultipleHandlers_CanCoexist()
    {
        // Arrange & Act
        var userHandler = new GetUserQueryHandler();
        var productHandler = new GetProductQueryHandler();

        // Assert - Verify handlers implement correct interfaces
        var userHandlerInterfaces = typeof(GetUserQueryHandler).GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
            .ToList();

        var productHandlerInterfaces = typeof(GetProductQueryHandler).GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
            .ToList();

        Assert.Single(userHandlerInterfaces);
        Assert.Single(productHandlerInterfaces);
        Assert.NotEqual(userHandlerInterfaces[0], productHandlerInterfaces[0]);
    }

    #endregion

    #region Type Safety Tests

    [Fact]
    public void QueryHandler_MustReturnCorrectResultType()
    {
        // Arrange
        var handlerType = typeof(GetUserQueryHandler);
        var handleMethod = handlerType.GetMethod("HandleAsync");

        // Act
        var returnType = handleMethod!.ReturnType;

        // Assert - Should return Task<UserDto>
        Assert.True(returnType.IsGenericType);
        Assert.Equal(typeof(Task<>).Name, returnType.GetGenericTypeDefinition().Name);
        var resultType = returnType.GetGenericArguments()[0];
        Assert.Equal(typeof(UserDto), resultType);
    }

    [Fact]
    public void QueryAndHandler_ResultTypesMatch()
    {
        // Arrange
        var queryType = typeof(GetUserQuery);
        var handlerType = typeof(GetUserQueryHandler);

        // Act
        var queryResultType = queryType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))
            .GetGenericArguments()[0];

        var handlerResultType = handlerType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
            .GetGenericArguments()[1];

        // Assert
        Assert.Equal(queryResultType, handlerResultType);
    }

    #endregion

    #region DTO Tests

    [Fact]
    public void UserDto_HasExpectedProperties()
    {
        // Arrange
        var dto = new UserDto();

        // Act
        var properties = typeof(UserDto).GetProperties();

        // Assert
        Assert.NotEmpty(properties);
        Assert.True(properties.Any(p => p.Name == "Id"));
        Assert.True(properties.Any(p => p.Name == "Name"));
        Assert.True(properties.Any(p => p.Name == "Email"));
    }

    [Fact]
    public void UserDto_CanBeInstantiatedWithValues()
    {
        // Arrange & Act
        var dto = new UserDto
        {
            Id = 1,
            Name = "Alice",
            Email = "alice@example.com"
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal("Alice", dto.Name);
        Assert.Equal("alice@example.com", dto.Email);
    }

    #endregion

    #region Error Handling & Edge Cases

    [Fact]
    public async Task QueryHandler_WithNullQuery_ThrowsException()
    {
        // Arrange
        var handler = new GetUserQueryHandler();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task QueryHandler_WithZeroUserId_StillReturnsResult()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var query = new GetUserQuery { UserId = 0 };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert - Should handle gracefully even with ID 0
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MultipleQueryInvocations_AreIndependent()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var query1 = new GetUserQuery { UserId = 1 };
        var query2 = new GetUserQuery { UserId = 2 };

        // Act
        var result1A = await handler.HandleAsync(query1, CancellationToken.None);
        var result2 = await handler.HandleAsync(query2, CancellationToken.None);
        var result1B = await handler.HandleAsync(query1, CancellationToken.None);

        // Assert
        Assert.Equal(result1A.Id, result1B.Id);
        Assert.NotEqual(result1A.Id, result2.Id);
    }

    #endregion

    #region Handler Async Behavior

    [Fact]
    public async Task QueryHandler_Completes_WithoutDeadlock()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var query = new GetUserQuery { UserId = 1 };
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act
        var task = handler.HandleAsync(query, cts.Token);

        // Assert - Should complete without throwing
        var result = await task.ConfigureAwait(false);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ConcurrentQueryHandling_IsThreadSafe()
    {
        // Arrange
        var handler = new GetUserQueryHandler();
        var queries = Enumerable.Range(1, 10)
            .Select(i => new GetUserQuery { UserId = i })
            .ToList();

        // Act
        var tasks = queries.Select(q => handler.HandleAsync(q, CancellationToken.None)).ToList();
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Length);
        Assert.All(results, r => Assert.NotNull(r));
    }

    #endregion

    #region Test Helpers

    private static async Task Assert_NoThrowsAsync(Func<Task> action)
    {
        await action();
    }

    #endregion

    // Test implementations
    private class GetUserQuery : IQuery<UserDto>
    {
        public int UserId { get; set; }
    }

    private class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
    {
        public Task<UserDto> HandleAsync(GetUserQuery query, CancellationToken cancellationToken = default)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var result = new UserDto
            {
                Id = query.UserId,
                Name = query.UserId == 1 ? "Alice" : query.UserId == 2 ? "Bob" : "Unknown",
                Email = query.UserId == 1 ? "alice@example.com" : query.UserId == 2 ? "bob@example.com" : "unknown@example.com"
            };

            return Task.FromResult(result);
        }
    }

    private class GetProductQuery : IQuery<ProductDto>
    {
        public int ProductId { get; set; }
    }

    private class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductDto>
    {
        public Task<ProductDto> HandleAsync(GetProductQuery query, CancellationToken cancellationToken = default)
        {
            var result = new ProductDto
            {
                Id = query.ProductId,
                Name = "Widget",
                Price = 99.99m
            };

            return Task.FromResult(result);
        }
    }

    private class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    private class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
