using MassTransit;
using Moq;
using SmartWorkz.Shared.Events;
using Microsoft.Extensions.Logging;

namespace SmartWorkz.Core.Tests.Events;

/// <summary>
/// Tests for MassTransitEventPublisher.
/// Verifies distributed event publishing behavior with MassTransit message bus.
/// </summary>
public class MassTransitEventPublisherTests
{
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<ILogger<MassTransitEventPublisher>> _mockLogger;
    private readonly MassTransitEventPublisher _publisher;

    public MassTransitEventPublisherTests()
    {
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<MassTransitEventPublisher>>();
        _publisher = new MassTransitEventPublisher(_mockPublishEndpoint.Object, _mockLogger.Object);
    }

    #region Single Event Publishing Tests

    /// <summary>Test 1: PublishAsync publishes single domain event</summary>
    [Fact]
    public async Task PublishAsync_WithSingleDomainEvent_PublishesToBus()
    {
        // Arrange
        var @event = new TestDomainEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            AggregateId = "agg-123",
            Value = "test"
        };

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        _mockPublishEndpoint.Verify(
            x => x.Publish(@event, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>Test 2: PublishAsync with null event throws ArgumentNullException</summary>
    [Fact]
    public async Task PublishAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Act & Assert
        TestDomainEvent? nullEvent = null;
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _publisher.PublishAsync(nullEvent!));
    }

    /// <summary>Test 3: PublishAsync with cancellation token propagates it</summary>
    [Fact]
    public async Task PublishAsync_WithCancellationToken_PropagatesToken()
    {
        // Arrange
        var @event = new TestDomainEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            AggregateId = "agg-456"
        };
        using var cts = new CancellationTokenSource();

        // Act
        await _publisher.PublishAsync(@event, cts.Token);

        // Assert
        _mockPublishEndpoint.Verify(
            x => x.Publish(@event, cts.Token),
            Times.Once);
    }

    /// <summary>Test 4: PublishAsync logs event publication</summary>
    [Fact]
    public async Task PublishAsync_WithValidEvent_LogsPublication()
    {
        // Arrange
        var @event = new TestDomainEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            AggregateId = "agg-789",
            Value = "logged"
        };

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event published")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>Test 5: PublishAsync handles MassTransit publish exception</summary>
    [Fact]
    public async Task PublishAsync_WhenPublishThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var @event = new TestDomainEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            AggregateId = "agg-error"
        };
        var publishException = new Exception("Publish failed");
        _mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(publishException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _publisher.PublishAsync(@event));

        Assert.Equal("Publish failed", ex.Message);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to publish")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Batch Publishing Tests

    /// <summary>Test 6: PublishAsync with multiple events publishes all</summary>
    [Fact]
    public async Task PublishAsync_WithMultipleEvents_PublishesAll()
    {
        // Arrange
        var events = new[]
        {
            new TestDomainEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTimeOffset.UtcNow,
                AggregateId = "agg-1",
                Value = "event1"
            },
            new TestDomainEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTimeOffset.UtcNow,
                AggregateId = "agg-2",
                Value = "event2"
            },
            new TestDomainEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTimeOffset.UtcNow,
                AggregateId = "agg-3",
                Value = "event3"
            }
        };

        // Act
        await _publisher.PublishAsync(events);

        // Assert
        _mockPublishEndpoint.Verify(
            x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    /// <summary>Test 7: PublishAsync with empty collection completes successfully</summary>
    [Fact]
    public async Task PublishAsync_WithEmptyCollection_Succeeds()
    {
        // Arrange
        var events = Array.Empty<TestDomainEvent>();

        // Act
        await _publisher.PublishAsync(events);

        // Assert
        _mockPublishEndpoint.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>Test 8: PublishAsync with null collection throws ArgumentNullException</summary>
    [Fact]
    public async Task PublishAsync_WithNullCollection_ThrowsArgumentNullException()
    {
        // Act & Assert
        IEnumerable<TestDomainEvent>? nullCollection = null;
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _publisher.PublishAsync(nullCollection!));
    }

    /// <summary>Test 9: PublishAsync publishes events in order</summary>
    [Fact]
    public async Task PublishAsync_WithMultipleEvents_PublishesInOrder()
    {
        // Arrange
        var publishOrder = new List<string>();
        var events = new[]
        {
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-1", Value = "first" },
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-2", Value = "second" },
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-3", Value = "third" }
        };

        _mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TestDomainEvent, CancellationToken>((e, ct) => publishOrder.Add(e.Value!))
            .Returns(Task.CompletedTask);

        // Act
        await _publisher.PublishAsync(events);

        // Assert
        Assert.Equal(new[] { "first", "second", "third" }, publishOrder);
    }

    /// <summary>Test 10: PublishAsync batch with cancellation token propagates</summary>
    [Fact]
    public async Task PublishAsync_BatchWithCancellationToken_PropagatesToken()
    {
        // Arrange
        var events = new[]
        {
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-1" },
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-2" }
        };
        using var cts = new CancellationTokenSource();

        // Act
        await _publisher.PublishAsync(events, cts.Token);

        // Assert
        _mockPublishEndpoint.Verify(
            x => x.Publish(It.IsAny<TestDomainEvent>(), cts.Token),
            Times.Exactly(2));
    }

    /// <summary>Test 11: PublishAsync batch logs each event publication</summary>
    [Fact]
    public async Task PublishAsync_WithMultipleEvents_LogsEachPublication()
    {
        // Arrange
        var events = new[]
        {
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-1" },
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-2" }
        };

        // Act
        await _publisher.PublishAsync(events);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event published")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    /// <summary>Test 12: PublishAsync batch handles partial failures (continues after error)</summary>
    [Fact]
    public async Task PublishAsync_BatchWithPartialFailure_ThrowsAggregateException()
    {
        // Arrange
        var events = new[]
        {
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-1", Value = "first" },
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-2", Value = "second" },
            new TestDomainEvent { EventId = Guid.NewGuid(), OccurredAt = DateTimeOffset.UtcNow, AggregateId = "agg-3", Value = "third" }
        };

        var callCount = 0;
        _mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TestDomainEvent, CancellationToken>((e, ct) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    throw new InvalidOperationException("Second event failed");
                }
            })
            .Returns(Task.CompletedTask);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AggregateException>(
            () => _publisher.PublishAsync(events));

        Assert.NotEmpty(ex.InnerExceptions);
    }

    #endregion

    #region Error Handling Tests

    /// <summary>Test 13: Publisher with null publish endpoint throws ArgumentNullException</summary>
    [Fact]
    public void MassTransitEventPublisher_NullPublishEndpoint_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => new MassTransitEventPublisher(null!, _mockLogger.Object));
        Assert.Equal("publishEndpoint", ex.ParamName);
    }

    /// <summary>Test 14: Publisher with null logger throws ArgumentNullException</summary>
    [Fact]
    public void MassTransitEventPublisher_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => new MassTransitEventPublisher(_mockPublishEndpoint.Object, null!));
        Assert.Equal("logger", ex.ParamName);
    }

    #endregion
}

/// <summary>Test domain event implementing IDomainEvent.</summary>
public sealed class TestDomainEvent : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string? Value { get; set; }
}
