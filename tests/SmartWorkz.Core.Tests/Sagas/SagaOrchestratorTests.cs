namespace SmartWorkz.Core.Tests.Sagas;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Shared.Events;
using SmartWorkz.Shared.Sagas;
using Xunit;

/// <summary>
/// Tests for the SagaOrchestrator implementation.
/// Validates saga orchestration, step execution, compensation, and error handling.
/// </summary>
public class SagaOrchestratorTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<SagaOrchestrator> _logger;

    public SagaOrchestratorTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = _loggerFactory.CreateLogger<SagaOrchestrator>();
    }

    [Fact]
    public async Task ExecuteSagaAsync_WithValidSagaAndState_ExecutesAllStepsSuccessfully()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };

        var sagaDefinition = new TestSagaDefinition();
        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                state.Step1Executed = true;
                await Task.Delay(10);
                return StepResult.Success();
            });

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                state.Step2Executed = true;
                await Task.Delay(10);
                return StepResult.Success();
            });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, @event);

        // Assert
        Assert.True(initialState.Step1Executed);
        Assert.True(initialState.Step2Executed);
        Assert.Equal(SagaStatus.Completed, initialState.Status);
    }

    [Fact]
    public async Task ExecuteSagaAsync_WithNullSagaDefinition_ThrowsArgumentNullException()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1" };
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };
        var orchestrator = new SagaOrchestrator(_logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            orchestrator.ExecuteSagaAsync(null!, initialState, @event));
    }

    [Fact]
    public async Task ExecuteSagaAsync_WithNullState_ThrowsArgumentNullException()
    {
        // Arrange
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };
        var sagaDefinition = new TestSagaDefinition();
        var orchestrator = new SagaOrchestrator(_logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            orchestrator.ExecuteSagaAsync(sagaDefinition, null!, @event));
    }

    [Fact]
    public async Task ExecuteSagaAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1" };
        var sagaDefinition = new TestSagaDefinition();
        var orchestrator = new SagaOrchestrator(_logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, null!));
    }

    [Fact]
    public async Task ExecuteSagaAsync_WhenStepFails_TriggersCompensationHandlers()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };

        var sagaDefinition = new TestSagaDefinition();
        var compensationCalled = false;

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                state.Step1Executed = true;
                await Task.Delay(10);
                return StepResult.Success();
            });

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                await Task.Delay(10);
                return StepResult.Failure("Step 2 failed", async (s, ex) =>
                {
                    compensationCalled = true;
                    state.Step2Compensated = true;
                    await Task.CompletedTask;
                });
            });

        sagaDefinition.OnFailure(async (state, ex) =>
        {
            state.Status = SagaStatus.Failed;
            await Task.CompletedTask;
        });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, @event);

        // Assert
        Assert.True(initialState.Step1Executed);
        Assert.True(compensationCalled);
        Assert.True(initialState.Step2Compensated);
        Assert.Equal(SagaStatus.Failed, initialState.Status);
    }

    [Fact]
    public async Task ExecuteSagaAsync_WhenCompensationFails_StillMarksSagaAsFailed()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };

        var sagaDefinition = new TestSagaDefinition();

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                state.Step1Executed = true;
                await Task.Delay(10);
                return StepResult.Success();
            });

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                await Task.Delay(10);
                return StepResult.Failure("Step 2 failed", async (s, ex) =>
                {
                    throw new InvalidOperationException("Compensation failed");
                });
            });

        sagaDefinition.OnFailure(async (state, ex) =>
        {
            state.Status = SagaStatus.Failed;
            state.FailureReason = ex.Message;
            await Task.CompletedTask;
        });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, @event);

        // Assert
        Assert.True(initialState.Step1Executed);
        Assert.Equal(SagaStatus.Failed, initialState.Status);
    }

    [Fact]
    public async Task ExecuteSagaAsync_TracksCompletedStepsForAudit()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };

        var sagaDefinition = new TestSagaDefinition();
        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                state.CompletedSteps.Add("Step1");
                await Task.Delay(10);
                return StepResult.Success();
            });

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                state.CompletedSteps.Add("Step2");
                await Task.Delay(10);
                return StepResult.Success();
            });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, @event);

        // Assert
        Assert.Equal(2, initialState.CompletedSteps.Count);
        Assert.Contains("Step1", initialState.CompletedSteps);
        Assert.Contains("Step2", initialState.CompletedSteps);
    }

    [Fact]
    public async Task ExecuteSagaAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // Cancel after 50ms

        var sagaDefinition = new TestSagaDefinition();
        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                try
                {
                    await Task.Delay(1000, cts.Token); // Long delay with cancellation token
                }
                catch (OperationCanceledException)
                {
                    throw; // Re-throw to propagate cancellation
                }
                state.Step1Executed = true;
                return StepResult.Success();
            });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, @event, cts.Token));
    }

    [Fact]
    public async Task ExecuteSagaAsync_WithMultipleEvents_ProcessesEventSequence()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var event1 = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };
        var event2 = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };

        var sagaDefinition = new TestSagaDefinition();
        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                state.EventsProcessed.Add(evt.EventId);
                await Task.Delay(10);
                return StepResult.Success();
            });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, event1);
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, event2);

        // Assert
        Assert.Equal(2, initialState.EventsProcessed.Count);
        Assert.Contains(event1.EventId, initialState.EventsProcessed);
        Assert.Contains(event2.EventId, initialState.EventsProcessed);
    }

    [Fact]
    public async Task ExecuteSagaAsync_WhenStepThrowsException_CatchesAndCompensates()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };

        var sagaDefinition = new TestSagaDefinition();
        var compensationCalled = false;

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                state.Step1Executed = true;
                await Task.Delay(10);
                return StepResult.Success();
            });

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Unhandled exception in step");
            });

        sagaDefinition.OnFailure(async (state, ex) =>
        {
            state.Status = SagaStatus.Failed;
            state.FailureReason = ex.Message;
            compensationCalled = true;
            await Task.CompletedTask;
        });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, @event);

        // Assert
        Assert.True(initialState.Step1Executed);
        Assert.True(compensationCalled);
        Assert.Equal(SagaStatus.Failed, initialState.Status);
        Assert.Contains("Unhandled exception", initialState.FailureReason);
    }

    [Fact]
    public async Task ExecuteSagaAsync_WithAsyncOperations_AwaitsProperly()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var @event = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };
        var executionTimes = new List<DateTime>();

        var sagaDefinition = new TestSagaDefinition();
        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                executionTimes.Add(DateTime.UtcNow);
                await Task.Delay(50);
                state.Step1Executed = true;
                return StepResult.Success();
            });

        sagaDefinition.DefineStep<TestOrderEvent>(
            async (evt, state) =>
            {
                executionTimes.Add(DateTime.UtcNow);
                await Task.Delay(50);
                state.Step2Executed = true;
                return StepResult.Success();
            });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, @event);

        // Assert
        Assert.Equal(2, executionTimes.Count);
        Assert.True(executionTimes[1] >= executionTimes[0].AddMilliseconds(40)); // Sequential execution
    }

    [Fact]
    public async Task ExecuteSagaAsync_WithMultipleEventTypes_FailsOnTypeMismatch()
    {
        // Arrange
        var initialState = new TestSagaState { Id = "saga-1", OrderId = "order-123" };
        var orderEvent = new TestOrderEvent { EventId = Guid.NewGuid(), AggregateId = "order-123" };

        var sagaDefinition = new TestSagaDefinition();
        sagaDefinition.DefineStep<TestPaymentEvent>(
            async (evt, state) =>
            {
                state.Step1Executed = true;
                await Task.Delay(10);
                return StepResult.Success();
            });

        sagaDefinition.OnFailure(async (state, ex) =>
        {
            state.Status = SagaStatus.Failed;
            state.FailureReason = ex.Message;
            await Task.CompletedTask;
        });

        var orchestrator = new SagaOrchestrator(_logger);

        // Act - ExecuteSagaAsync doesn't throw, it sets the status to Failed
        await orchestrator.ExecuteSagaAsync(sagaDefinition, initialState, orderEvent);

        // Assert - Saga should be marked as failed due to event type mismatch
        Assert.Equal(SagaStatus.Failed, initialState.Status);
        Assert.Contains("Event type mismatch", initialState.FailureReason);
        Assert.False(initialState.Step1Executed); // Step wasn't executed
    }

    // Test data classes
    public class TestSagaState : SagaState
    {
        public string OrderId { get; set; } = string.Empty;
        public bool Step1Executed { get; set; }
        public bool Step2Executed { get; set; }
        public bool Step2Compensated { get; set; }
        public List<string> CompletedSteps { get; set; } = new();
        public List<Guid> EventsProcessed { get; set; } = new();
    }

    public class TestOrderEvent : IDomainEvent
    {
        public Guid EventId { get; set; }
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
        public string AggregateId { get; set; } = string.Empty;
    }

    public class TestPaymentEvent : IDomainEvent
    {
        public Guid EventId { get; set; }
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
        public string AggregateId { get; set; } = string.Empty;
    }

    public class TestSagaDefinition : ISagaDefinition<TestSagaState>
    {
        private readonly List<Func<IDomainEvent, TestSagaState, Task<StepResult>>> _steps = new();
        private Func<TestSagaState, Exception, Task>? _failureHandler;

        public string Name => "TestSaga";

        public void DefineStep<TEvent>(Func<TEvent, TestSagaState, Task<StepResult>> handler) where TEvent : IDomainEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            // Cast the generic handler to work with TestOrderEvent
            _steps.Add(async (@event, state) =>
            {
                if (@event is TEvent typedEvent)
                {
                    return await handler(typedEvent, state);
                }
                // Fail on type mismatch instead of silently succeeding
                throw new InvalidOperationException($"Event type mismatch: expected {typeof(TEvent).Name}, got {@event.GetType().Name}");
            });
        }

        public void OnFailure(Func<TestSagaState, Exception, Task> compensationHandler)
        {
            _failureHandler = compensationHandler ?? throw new ArgumentNullException(nameof(compensationHandler));
        }

        public async Task<ISagaDefinition<TestSagaState>> BuildAsync()
        {
            await Task.CompletedTask;
            return this;
        }

        public IReadOnlyList<Func<IDomainEvent, TestSagaState, Task<StepResult>>> GetSteps() => _steps.AsReadOnly();
        public Func<TestSagaState, Exception, Task>? GetFailureHandler() => _failureHandler;
    }
}

