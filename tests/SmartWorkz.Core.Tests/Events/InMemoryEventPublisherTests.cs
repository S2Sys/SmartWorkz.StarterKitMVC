using SmartWorkz.Core.Shared.Events;

namespace SmartWorkz.Core.Tests.Events;

/// <summary>
/// Tests for InMemoryEventPublisher and InMemoryEventSubscriber.
/// Verifies event subscription, handler registration, and publishing behavior.
/// </summary>
public class InMemoryEventPublisherTests
{
    private readonly InMemoryEventSubscriber _subscriber;
    private readonly InMemoryEventPublisher _publisher;

    public InMemoryEventPublisherTests()
    {
        _subscriber = new InMemoryEventSubscriber();
        _publisher = new InMemoryEventPublisher(_subscriber);
    }

    #region Subscription Tests

    /// <summary>Test 1: Subscribe registers handler</summary>
    [Fact]
    public void Subscribe_RegistersHandler()
    {
        // Arrange
        var handlerCalled = false;
        Func<TestEvent, CancellationToken, Task> handler = async (e, ct) =>
        {
            handlerCalled = true;
            await Task.CompletedTask;
        };

        // Act
        _subscriber.Subscribe(handler);

        // Assert - handler is registered (verify by checking internal state)
        var handlers = _subscriber.GetHandlers(typeof(TestEvent));
        Assert.NotNull(handlers);
        Assert.Single(handlers);
    }

    /// <summary>Test 2: Multiple handlers for same event type all registered</summary>
    [Fact]
    public void Subscribe_MultipleHandlers_AllRegistered()
    {
        // Arrange
        var handler1 = new Func<TestEvent, CancellationToken, Task>(async (e, ct) => await Task.CompletedTask);
        var handler2 = new Func<TestEvent, CancellationToken, Task>(async (e, ct) => await Task.CompletedTask);
        var handler3 = new Func<TestEvent, CancellationToken, Task>(async (e, ct) => await Task.CompletedTask);

        // Act
        _subscriber.Subscribe(handler1);
        _subscriber.Subscribe(handler2);
        _subscriber.Subscribe(handler3);

        // Assert
        var handlers = _subscriber.GetHandlers(typeof(TestEvent));
        Assert.NotNull(handlers);
        Assert.Equal(3, handlers.Count);
    }

    /// <summary>Test 3: Different event types have separate handler lists</summary>
    [Fact]
    public void Subscribe_DifferentEventTypes_SeparateLists()
    {
        // Arrange
        var testEventHandler = new Func<TestEvent, CancellationToken, Task>(async (e, ct) => await Task.CompletedTask);
        var otherEventHandler = new Func<OtherTestEvent, CancellationToken, Task>(async (e, ct) => await Task.CompletedTask);

        // Act
        _subscriber.Subscribe(testEventHandler);
        _subscriber.Subscribe(otherEventHandler);

        // Assert
        var testHandlers = _subscriber.GetHandlers(typeof(TestEvent));
        var otherHandlers = _subscriber.GetHandlers(typeof(OtherTestEvent));

        Assert.Single(testHandlers);
        Assert.Single(otherHandlers);
        Assert.NotEqual(testHandlers, otherHandlers);
    }

    #endregion

    #region Publishing Tests

    /// <summary>Test 4: Handlers execute in registration order</summary>
    [Fact]
    public async Task PublishAsync_HandlersExecuteInRegistrationOrder()
    {
        // Arrange
        var executionOrder = new List<int>();

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            executionOrder.Add(1);
            await Task.CompletedTask;
        });

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            executionOrder.Add(2);
            await Task.CompletedTask;
        });

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            executionOrder.Add(3);
            await Task.CompletedTask;
        });

        var @event = new TestEvent { Value = "test" };

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
    }

    /// <summary>Test 5: PublishAsync calls all registered handlers</summary>
    [Fact]
    public async Task PublishAsync_CallsAllHandlers()
    {
        // Arrange
        var callCount = 0;

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            Interlocked.Increment(ref callCount);
            await Task.CompletedTask;
        });

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            Interlocked.Increment(ref callCount);
            await Task.CompletedTask;
        });

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            Interlocked.Increment(ref callCount);
            await Task.CompletedTask;
        });

        var @event = new TestEvent();

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        Assert.Equal(3, callCount);
    }

    /// <summary>Test 6: Handler receives correct event data</summary>
    [Fact]
    public async Task PublishAsync_HandlerReceivesCorrectEventData()
    {
        // Arrange
        TestEvent? receivedEvent = null;

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            receivedEvent = e;
            await Task.CompletedTask;
        });

        var publishedEvent = new TestEvent { Value = "test-data" };

        // Act
        await _publisher.PublishAsync(publishedEvent);

        // Assert
        Assert.NotNull(receivedEvent);
        Assert.Equal("test-data", receivedEvent.Value);
    }

    /// <summary>Test 7: Handler exception is caught and thrown as InvalidOperationException</summary>
    [Fact]
    public async Task PublishAsync_HandlerException_ThrowsInvalidOperationException()
    {
        // Arrange
        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Handler error");
        });

        var @event = new TestEvent();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _publisher.PublishAsync(@event));
        Assert.Contains("Handler error", ex.Message);
    }

    /// <summary>Test 8: PublishAsync with no handlers completes successfully</summary>
    [Fact]
    public async Task PublishAsync_NoHandlers_Succeeds()
    {
        // Arrange
        var @event = new TestEvent();

        // Act
        await _publisher.PublishAsync(@event);

        // Assert - no exception should be thrown
    }

    /// <summary>Test 9: Multiple handlers all called in sequence</summary>
    [Fact]
    public async Task PublishAsync_MultipleHandlers_AllCalledSequentially()
    {
        // Arrange
        var callCounts = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            callCounts[1]++;
            await Task.Delay(10, ct);
        });

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            callCounts[2]++;
            await Task.Delay(10, ct);
        });

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            callCounts[3]++;
            await Task.Delay(10, ct);
        });

        var @event = new TestEvent();

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        Assert.All(callCounts.Values, count => Assert.Equal(1, count));
    }

    /// <summary>Test 10: CancellationToken properly propagated to handlers</summary>
    [Fact]
    public async Task PublishAsync_CancellationTokenPropagated()
    {
        // Arrange
        CancellationToken? receivedToken = null;

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            receivedToken = ct;
            await Task.CompletedTask;
        });

        var @event = new TestEvent();
        using var cts = new CancellationTokenSource();

        // Act
        await _publisher.PublishAsync(@event, cts.Token);

        // Assert
        Assert.NotNull(receivedToken);
        Assert.Equal(cts.Token, receivedToken);
    }

    /// <summary>Test 11: Empty event type handled</summary>
    [Fact]
    public async Task PublishAsync_EmptyEvent_Succeeds()
    {
        // Arrange
        var handlerCalled = false;

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            handlerCalled = true;
            await Task.CompletedTask;
        });

        var @event = new TestEvent();

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        Assert.True(handlerCalled);
    }

    /// <summary>Test 12: Handler exceptions don't prevent other handlers from running</summary>
    [Fact]
    public async Task PublishAsync_FirstHandlerException_OtherHandlersStillRun()
    {
        // Arrange
        var executionOrder = new List<int>();

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            executionOrder.Add(1);
            await Task.CompletedTask;
            throw new InvalidOperationException("Error in handler 1");
        });

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            executionOrder.Add(2);
            await Task.CompletedTask;
        });

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            executionOrder.Add(3);
            await Task.CompletedTask;
        });

        var @event = new TestEvent();

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _publisher.PublishAsync(@event));

        // Assert
        // Even with exception, all handlers should have attempted to run
        Assert.Equal(3, executionOrder.Count);
        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
        Assert.Contains("Handler error", ex.Message);
    }

    /// <summary>Test 13: Concurrent subscriptions don't lose handlers</summary>
    [Fact]
    public async Task Subscribe_ConcurrentSubscriptions_AllHandlersRegistered()
    {
        // Arrange
        var subscriber = new InMemoryEventSubscriber();
        var tasks = new List<Task>();
        const int handlerCount = 100;

        // Act
        for (int i = 0; i < handlerCount; i++)
        {
            int capturedIndex = i;
            tasks.Add(Task.Run(() =>
            {
                subscriber.Subscribe<TestEvent>(async (e, ct) =>
                {
                    await Task.CompletedTask;
                });
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var handlers = subscriber.GetHandlers(typeof(TestEvent));
        Assert.Equal(handlerCount, handlers.Count);
    }

    /// <summary>Test 14: PublishAsync completes without exception on success</summary>
    [Fact]
    public async Task PublishAsync_SuccessfulHandlers_NoException()
    {
        // Arrange - Success case
        _subscriber.Subscribe<TestEvent>(async (e, ct) => await Task.CompletedTask);
        var @event = new TestEvent();

        // Act
        var exception = await Record.ExceptionAsync(() => _publisher.PublishAsync(@event));

        // Assert
        Assert.Null(exception);
    }

    /// <summary>Test 15: Handler exception details captured in exception message</summary>
    [Fact]
    public async Task PublishAsync_HandlerException_ExceptionContainsErrorInfo()
    {
        // Arrange
        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Test error message");
        });

        var @event = new TestEvent();

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _publisher.PublishAsync(@event));

        // Assert
        Assert.Contains("Test error message", ex.Message);
        Assert.Contains("Handler error", ex.Message);
    }

    /// <summary>Test 16: Publisher with null subscriber throws ArgumentNullException</summary>
    [Fact]
    public void InMemoryEventPublisher_NullSubscriber_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new InMemoryEventPublisher(null!));
        Assert.Equal("subscriber", ex.ParamName);
    }

    /// <summary>Test 17: Multiple event types don't interfere with each other</summary>
    [Fact]
    public async Task PublishAsync_MultipleEventTypes_NoInterference()
    {
        // Arrange
        var testEventCalled = false;
        var otherEventCalled = false;

        _subscriber.Subscribe<TestEvent>(async (e, ct) =>
        {
            testEventCalled = true;
            await Task.CompletedTask;
        });

        _subscriber.Subscribe<OtherTestEvent>(async (e, ct) =>
        {
            otherEventCalled = true;
            await Task.CompletedTask;
        });

        // Act
        await _publisher.PublishAsync(new TestEvent());
        await _publisher.PublishAsync(new OtherTestEvent());

        // Assert
        Assert.True(testEventCalled);
        Assert.True(otherEventCalled);
    }

    #endregion
}

/// <summary>Test event for use in tests.</summary>
public sealed class TestEvent
{
    public string? Value { get; set; }
}

/// <summary>Alternative test event for use in tests.</summary>
public sealed class OtherTestEvent
{
    public string? Data { get; set; }
}
