namespace SmartWorkz.Core.Shared.CQRS;

/// <summary>
/// Handler for processing a specific command type.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle, must implement <see cref="ICommand"/>.</typeparam>
/// <remarks>
/// Command handlers encapsulate the logic for handling state-changing operations.
/// Each command type should have exactly one handler, but handlers can be registered
/// multiple times in the dependency injection container if needed for different scenarios.
/// </remarks>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown.</param>
    /// <returns>A task that represents the asynchronous handle operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
