namespace SmartWorkz.Core.Shared.Sagas;

using Microsoft.Extensions.Logging;
using SmartWorkz.Core.Shared.Events;
using SmartWorkz.Core.Sagas;

/// <summary>
/// Orchestrates the execution of sagas, managing step sequencing, error handling,
/// and compensation/rollback logic for complex distributed processes.
/// </summary>
public class SagaOrchestrator
{
    private readonly ILogger<SagaOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the SagaOrchestrator class.
    /// </summary>
    /// <param name="logger">Logger for saga execution tracking and debugging.</param>
    public SagaOrchestrator(ILogger<SagaOrchestrator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a saga definition with the provided initial state and triggering event.
    /// Manages step execution, error handling, and compensation logic.
    /// </summary>
    /// <typeparam name="TSagaState">The saga state type.</typeparam>
    /// <param name="sagaDefinition">The saga definition blueprint to execute.</param>
    /// <param name="initialState">The initial saga state.</param>
    /// <param name="event">The domain event triggering the saga.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the saga execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public async Task ExecuteSagaAsync<TSagaState>(
        ISagaDefinition<TSagaState> sagaDefinition,
        TSagaState initialState,
        IDomainEvent @event,
        CancellationToken cancellationToken = default)
        where TSagaState : SagaState
    {
        // Input validation
        if (sagaDefinition == null)
        {
            throw new ArgumentNullException(nameof(sagaDefinition), "Saga definition cannot be null.");
        }

        if (initialState == null)
        {
            throw new ArgumentNullException(nameof(initialState), "Saga state cannot be null.");
        }

        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event), "Domain event cannot be null.");
        }

        _logger.LogInformation(
            "Starting saga execution. SagaId={SagaId}, SagaName={SagaName}, EventId={EventId}, AggregateId={AggregateId}",
            initialState.Id,
            sagaDefinition.Name,
            @event.EventId,
            @event.AggregateId);

        try
        {
            // Build the saga definition
            await sagaDefinition.BuildAsync();

            // Execute saga using reflection to get the steps and properly handle the event
            // This is a flexible approach that works with the dynamic nature of saga definitions
            await ExecuteSagaStepsAsync(sagaDefinition, initialState, @event, cancellationToken);

            // Mark saga as completed
            initialState.Status = SagaStatus.Completed;
            initialState.CompletedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation(
                "Saga completed successfully. SagaId={SagaId}, SagaName={SagaName}, Duration={Duration}ms",
                initialState.Id,
                sagaDefinition.Name,
                (initialState.CompletedAt.Value - initialState.StartedAt).TotalMilliseconds);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(
                "Saga execution cancelled. SagaId={SagaId}, SagaName={SagaName}, Message={Message}",
                initialState.Id,
                sagaDefinition.Name,
                ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Saga execution failed. SagaId={SagaId}, SagaName={SagaName}, ErrorMessage={ErrorMessage}",
                initialState.Id,
                sagaDefinition.Name,
                ex.Message);

            initialState.Status = SagaStatus.Compensating;
            initialState.FailureReason = ex.Message;
            initialState.FailureStackTrace = ex.StackTrace;

            // Call the failure handler if defined
            await ExecuteFailureHandlerAsync(sagaDefinition, initialState, ex);

            initialState.Status = SagaStatus.Failed;
            initialState.CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// Executes saga steps by using reflection to access internal step definitions.
    /// </summary>
    private async Task ExecuteSagaStepsAsync<TSagaState>(
        ISagaDefinition<TSagaState> sagaDefinition,
        TSagaState state,
        IDomainEvent @event,
        CancellationToken cancellationToken)
        where TSagaState : SagaState
    {
        // Use reflection to get the steps from the saga definition
        var getStepsMethod = sagaDefinition.GetType().GetMethod("GetSteps");
        if (getStepsMethod == null)
        {
            _logger.LogWarning("Saga definition does not have a GetSteps method.");
            return;
        }

        var stepsObject = getStepsMethod.Invoke(sagaDefinition, null);
        if (stepsObject == null)
        {
            _logger.LogInformation("Saga has no steps defined.");
            return;
        }

        // Get the steps as a list
        var stepsEnumerable = stepsObject as System.Collections.IEnumerable;
        if (stepsEnumerable == null)
        {
            _logger.LogWarning("Could not enumerate saga steps.");
            return;
        }

        var completedSteps = new List<Func<IDomainEvent, TSagaState, Task<StepResult>>>();

        try
        {
            int stepIndex = 0;
            foreach (var step in stepsEnumerable)
            {
                cancellationToken.ThrowIfCancellationRequested();

                stepIndex++;
                _logger.LogInformation(
                    "Executing saga step. SagaId={SagaId}, StepIndex={StepIndex}, EventId={EventId}",
                    state.Id,
                    stepIndex,
                    @event.EventId);

                try
                {
                    // Invoke the step as a delegate
                    var stepDelegate = step as Delegate;
                    if (stepDelegate == null)
                    {
                        _logger.LogWarning("Step at index {StepIndex} is not a valid delegate.", stepIndex);
                        continue;
                    }

                    // Call the step with the event and state
                    var resultTask = stepDelegate.DynamicInvoke(@event, state) as Task<StepResult>;
                    if (resultTask == null)
                    {
                        _logger.LogWarning("Step at index {StepIndex} did not return a Task<StepResult>.", stepIndex);
                        continue;
                    }

                    var result = await resultTask;

                    if (!result.IsSuccess)
                    {
                        _logger.LogError(
                            "Saga step failed. SagaId={SagaId}, StepIndex={StepIndex}, Reason={Reason}",
                            state.Id,
                            stepIndex,
                            result.FailureReason);

                        // Execute compensation handlers for completed steps in reverse order
                        await CompensateCompletedStepsAsync(completedSteps, state, @event,
                            new InvalidOperationException(result.FailureReason));

                        // Execute the step's compensation handler if it has one
                        if (result.CompensationHandler != null)
                        {
                            try
                            {
                                await result.CompensationHandler(state,
                                    new InvalidOperationException(result.FailureReason));
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(
                                    ex,
                                    "Compensation handler failed for step. SagaId={SagaId}, StepIndex={StepIndex}",
                                    state.Id,
                                    stepIndex);
                            }
                        }

                        throw new InvalidOperationException(
                            $"Saga step {stepIndex} failed: {result.FailureReason}",
                            null);
                    }

                    _logger.LogInformation(
                        "Saga step completed successfully. SagaId={SagaId}, StepIndex={StepIndex}",
                        state.Id,
                        stepIndex);

                    // Track completed step for potential rollback
                    completedSteps.Add(stepDelegate as Func<IDomainEvent, TSagaState, Task<StepResult>>);
                }
                catch (Exception stepEx)
                {
                    _logger.LogError(
                        stepEx,
                        "Exception during saga step execution. SagaId={SagaId}, StepIndex={StepIndex}",
                        state.Id,
                        stepIndex);

                    // Execute compensation handlers for completed steps in reverse order
                    await CompensateCompletedStepsAsync(completedSteps, state, @event, stepEx);

                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error during saga step orchestration. SagaId={SagaId}",
                state.Id);
            throw;
        }
    }

    /// <summary>
    /// Executes compensation handlers for all completed steps in reverse order.
    /// </summary>
    private async Task CompensateCompletedStepsAsync<TSagaState>(
        List<Func<IDomainEvent, TSagaState, Task<StepResult>>> completedSteps,
        TSagaState state,
        IDomainEvent @event,
        Exception failureException)
        where TSagaState : SagaState
    {
        _logger.LogInformation(
            "Starting compensation for {CompletedStepCount} completed steps. SagaId={SagaId}",
            completedSteps.Count,
            state.Id);

        // Execute compensation in reverse order (LIFO - Last In, First Out)
        for (int i = completedSteps.Count - 1; i >= 0; i--)
        {
            try
            {
                var step = completedSteps[i];
                if (step != null)
                {
                    // Execute the step again to get its compensation handler
                    _logger.LogInformation(
                        "Compensating step {StepIndex}. SagaId={SagaId}",
                        i + 1,
                        state.Id);

                    var resultTask = step(@event, state);
                    var result = await resultTask;

                    if (result.CompensationHandler != null)
                    {
                        try
                        {
                            await result.CompensationHandler(state, failureException);
                            _logger.LogInformation(
                                "Step {StepIndex} compensation completed successfully. SagaId={SagaId}",
                                i + 1,
                                state.Id);
                        }
                        catch (Exception compensationEx)
                        {
                            _logger.LogError(
                                compensationEx,
                                "Compensation handler failed for step {StepIndex}. SagaId={SagaId}",
                                i + 1,
                                state.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error during compensation of step {StepIndex}. SagaId={SagaId}",
                    i + 1,
                    state.Id);
            }
        }
    }

    /// <summary>
    /// Executes the saga-level failure handler if one is defined.
    /// </summary>
    private async Task ExecuteFailureHandlerAsync<TSagaState>(
        ISagaDefinition<TSagaState> sagaDefinition,
        TSagaState state,
        Exception exception)
        where TSagaState : class, SagaState
    {
        try
        {
            // Use reflection to get the failure handler
            var getFailureHandlerMethod = sagaDefinition.GetType().GetMethod("GetFailureHandler");
            if (getFailureHandlerMethod == null)
            {
                _logger.LogWarning("Saga definition does not have a GetFailureHandler method.");
                return;
            }

            var failureHandlerObject = getFailureHandlerMethod.Invoke(sagaDefinition, null);
            var failureHandler = failureHandlerObject as Func<TSagaState, Exception, Task>;

            if (failureHandler != null)
            {
                _logger.LogInformation(
                    "Executing saga failure handler. SagaId={SagaId}, SagaName={SagaName}",
                    state.Id,
                    sagaDefinition.Name);

                await failureHandler(state, exception);

                _logger.LogInformation(
                    "Saga failure handler completed. SagaId={SagaId}",
                    state.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error executing saga failure handler. SagaId={SagaId}",
                state.Id);
        }
    }
}
