namespace SmartWorkz.Core.Tests.CQRS;

using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartWorkz.Shared;
using Xunit;

/// <summary>
/// Tests for the query handler registration and dispatch.
/// </summary>
public class QueryHandlerDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_WithValidQuery_CallsCorrectHandler()
    {
        // Arrange
        var query = new TestQuery { Id = "123" };
        var expectedResult = new TestQueryResult { Data = "result-data" };

        var mockHandler = new Mock<IQueryHandler<TestQuery, TestQueryResult>>();
        mockHandler.Setup(h => h.HandleAsync(query, CancellationToken.None))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new QueryHandlerDispatcher(serviceProvider);

        // Act
        var result = await dispatcher.DispatchAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("result-data", result.Data);
        mockHandler.Verify(h => h.HandleAsync(query, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new QueryHandlerDispatcher(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            dispatcher.DispatchAsync<TestQuery, TestQueryResult>(null!));
    }

    [Fact]
    public async Task DispatchAsync_WithNoHandler_ThrowsInvalidOperationException()
    {
        // Arrange
        var query = new TestQuery { Id = "123" };
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new QueryHandlerDispatcher(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.DispatchAsync<TestQuery, TestQueryResult>(query));
    }

    [Fact]
    public async Task DispatchAsync_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        var query = new TestQuery { Id = "123" };
        var cancellationToken = new CancellationToken(false);
        var expectedResult = new TestQueryResult { Data = "result-data" };

        var mockHandler = new Mock<IQueryHandler<TestQuery, TestQueryResult>>();
        mockHandler.Setup(h => h.HandleAsync(query, cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new QueryHandlerDispatcher(serviceProvider);

        // Act
        var result = await dispatcher.DispatchAsync(query, cancellationToken);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(query, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerThrowsException_PropagatesToCaller()
    {
        // Arrange
        var query = new TestQuery { Id = "123" };
        var expectedException = new InvalidOperationException("Handler error");

        var mockHandler = new Mock<IQueryHandler<TestQuery, TestQueryResult>>();
        mockHandler.Setup(h => h.HandleAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new QueryHandlerDispatcher(serviceProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.DispatchAsync(query));
        Assert.Equal("Handler error", exception.Message);
    }

    [Fact]
    public async Task DispatchAsync_WithMultipleQueries_DispatchesEachCorrectly()
    {
        // Arrange
        var query1 = new TestQuery { Id = "123" };
        var query2 = new TestQuery { Id = "456" };
        var result1 = new TestQueryResult { Data = "result-1" };
        var result2 = new TestQueryResult { Data = "result-2" };

        var mockHandler = new Mock<IQueryHandler<TestQuery, TestQueryResult>>();
        mockHandler.Setup(h => h.HandleAsync(query1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);
        mockHandler.Setup(h => h.HandleAsync(query2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2);

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new QueryHandlerDispatcher(serviceProvider);

        // Act
        var resultA = await dispatcher.DispatchAsync(query1);
        var resultB = await dispatcher.DispatchAsync(query2);

        // Assert
        Assert.Equal("result-1", resultA.Data);
        Assert.Equal("result-2", resultB.Data);
    }

    [Fact]
    public async Task DispatchAsync_WithDifferentQueryTypes_UsesCorrectHandlers()
    {
        // Arrange
        var query1 = new TestQuery { Id = "123" };
        var query2 = new AnotherTestQuery { Name = "test" };
        var result1 = new TestQueryResult { Data = "result-1" };
        var result2 = new AnotherTestQueryResult { Value = "value" };

        var mockHandler1 = new Mock<IQueryHandler<TestQuery, TestQueryResult>>();
        mockHandler1.Setup(h => h.HandleAsync(query1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1)
            .Verifiable();

        var mockHandler2 = new Mock<IQueryHandler<AnotherTestQuery, AnotherTestQueryResult>>();
        mockHandler2.Setup(h => h.HandleAsync(query2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler1.Object);
        services.AddScoped(_ => mockHandler2.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new QueryHandlerDispatcher(serviceProvider);

        // Act
        var resultA = await dispatcher.DispatchAsync(query1);
        var resultB = await dispatcher.DispatchAsync(query2);

        // Assert
        mockHandler1.Verify(h => h.HandleAsync(query1, It.IsAny<CancellationToken>()), Times.Once);
        mockHandler2.Verify(h => h.HandleAsync(query2, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test query and handler classes
    public class TestQuery : IQuery<TestQueryResult>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class TestQueryResult
    {
        public string Data { get; set; } = string.Empty;
    }

    public class AnotherTestQuery : IQuery<AnotherTestQueryResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class AnotherTestQueryResult
    {
        public string Value { get; set; } = string.Empty;
    }
}

/// <summary>
/// Query dispatcher for dispatching queries to registered handlers.
/// </summary>
public class QueryHandlerDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryHandlerDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(typeof(TQuery), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
            throw new InvalidOperationException($"No handler registered for query type '{typeof(TQuery).Name}'");

        var method = handlerType.GetMethod("HandleAsync");
        if (method == null)
            throw new InvalidOperationException($"Handler for '{typeof(TQuery).Name}' does not have HandleAsync method");

        var result = await (Task<TResult>)method.Invoke(handler, new object[] { query, cancellationToken })!;
        return result;
    }
}

/// <summary>
/// Query interface marker.
/// </summary>
public interface IQuery<TResult>
{
}

/// <summary>
/// Query handler interface.
/// </summary>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
