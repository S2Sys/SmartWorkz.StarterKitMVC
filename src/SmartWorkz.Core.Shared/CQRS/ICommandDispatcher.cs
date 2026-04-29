namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for dispatching commands to their handlers.
/// </summary>
/// <remarks>
/// The command dispatcher routes commands to their corresponding handlers via dependency injection.
/// Implementations should resolve handlers from the service provider and invoke their HandleAsync method.
/// </remarks>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches a command to its registered handler asynchronously.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to dispatch.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    /// <returns>A task representing the asynchronous dispatch operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the command type.</exception>
    Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;
}
