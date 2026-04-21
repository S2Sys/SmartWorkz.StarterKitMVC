namespace SmartWorkz.Shared;

using Microsoft.Extensions.Logging;

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
            cancellationToken.ThrowIfCancellationRequested();

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

        // Create list to track step results WITH their compensation handlers
        var executedSteps = new List<(int StepIndex, StepResult Result, Func<Task>? CompensationHandler)>();

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

                        var failureException = new InvalidOperationException(result.FailureReason);

                        // Execute compensation handlers for completed steps in reverse order
                        await CompensateExecutedStepsAsync(executedSteps, state, failureException);

                        // Execute the step's compensation handler if it has one
                        if (result.CompensationHandler != null)
                        {
                            try
                            {
                                await result.CompensationHandler(state, failureException);
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

                        throw failureException;
                    }

                    _logger.LogInformation(
                        "Saga step completed successfully. SagaId={SagaId}, StepIndex={StepIndex}",
                        state.Id,
                        stepIndex);

                    // Store step result and its compensation handler for potential rollback
                    var compensationHandler = result.CompensationHandler != null
                        ? (() => result.CompensationHandler(state, new InvalidOperationException("Compensation triggered")))
                        : (Func<Task>?)null;

                    executedSteps.Add((stepIndex, result, compensationHandler));
                }
                catch (Exception stepEx)
                {
                    _logger.LogError(
                        stepEx,
                        "Exception during saga step execution. SagaId={SagaId}, StepIndex={StepIndex}",
                        state.Id,
                        stepIndex);

                    // Execute compensation handlers for completed steps in reverse order
                    await CompensateExecutedStepsAsync(executedSteps, state, stepEx);

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
    /// Executes compensation handlers for all executed steps in reverse order.
    /// Uses stored compensation handlers to avoid re-executing steps.
    /// </summary>
    private async Task CompensateExecutedStepsAsync<TSagaState>(
        List<(int StepIndex, StepResult Result, Func<Task>? CompensationHandler)> executedSteps,
        TSagaState state,
        Exception failureException)
        where TSagaState : SagaState
    {
        _logger.LogInformation(
            "Starting compensation for {ExecutedStepCount} executed steps. SagaId={SagaId}",
            executedSteps.Count,
            state.Id);

        // Execute compensation in reverse order (LIFO - Last In, First Out)
        foreach (var executedStep in executedSteps.AsEnumerable().Reverse())
        {
            try
            {
                if (executedStep.CompensationHandler != null)
                {
                    _logger.LogInformation(
                        "Executing compensation for step {StepIndex}. SagaId={SagaId}",
                        executedStep.StepIndex,
                        state.Id);

                    await executedStep.CompensationHandler();

                    _logger.LogInformation(
                        "Compensation for step {StepIndex} completed successfully. SagaId={SagaId}",
                        executedStep.StepIndex,
                        state.Id);
                }
            }
            catch (Exception compensationEx)
            {
                _logger.LogError(
                    compensationEx,
                    "Compensation for step {StepIndex} failed. SagaId={SagaId}",
                    executedStep.StepIndex,
                    state.Id);
                // Log but continue with remaining compensations
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
        where TSagaState : SagaState
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
