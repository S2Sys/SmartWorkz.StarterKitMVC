namespace SmartWorkz.Shared;

using SmartWorkz.Core.Shared.Events;

/// <summary>
/// Defines the blueprint for a saga orchestration.
/// A saga is a pattern for managing distributed transactions and long-running processes
/// by coordinating multiple steps with built-in compensation mechanisms.
/// </summary>
/// <typeparam name="TSagaState">The state object that flows through the saga steps.</typeparam>
public interface ISagaDefinition<TSagaState> where TSagaState : class
{
    /// <summary>
    /// Gets the name of this saga for logging and identification purposes.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Defines a step in the saga that will be executed when a specific event type is received.
    /// Steps are executed sequentially in the order they were defined.
    /// </summary>
    /// <typeparam name="TEvent">The domain event type that triggers this step.</typeparam>
    /// <param name="handler">
    /// The async handler function that processes the event and updates the saga state.
    /// Returns a StepResult indicating success or failure.
    /// </param>
    void DefineStep<TEvent>(Func<TEvent, TSagaState, Task<StepResult>> handler) where TEvent : IDomainEvent;

    /// <summary>
    /// Defines the failure handler that will be called if any step fails.
    /// Used for compensation logic and saga-level error handling.
    /// </summary>
    /// <param name="compensationHandler">
    /// The async handler that receives the current saga state and the exception that occurred.
    /// Responsible for compensation/rollback logic.
    /// </param>
    void OnFailure(Func<TSagaState, Exception, Task> compensationHandler);

    /// <summary>
    /// Builds and returns the saga definition for execution.
    /// Can be used for async initialization or validation.
    /// </summary>
    /// <returns>A task that completes with the configured saga definition.</returns>
    Task<ISagaDefinition<TSagaState>> BuildAsync();

    /// <summary>
    /// Gets the list of saga steps in execution order.
    /// </summary>
    /// <returns>A read-only list of saga step handlers.</returns>
    IReadOnlyList<Func<IDomainEvent, TSagaState, Task<StepResult>>> GetSteps();

    /// <summary>
    /// Gets the failure compensation handler if defined.
    /// </summary>
    /// <returns>The failure handler function, or null if not defined.</returns>
    Func<TSagaState, Exception, Task>? GetFailureHandler();
}
