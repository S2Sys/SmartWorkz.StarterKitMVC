namespace SmartWorkz.Core.Shared.CQRS;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Routes commands to their appropriate handlers via dependency injection.
/// </summary>
/// <remarks>
/// This mediator implementation enables loose coupling between command publishers and handlers.
/// Handlers are resolved from the service provider at dispatch time, supporting multiple handler
/// registrations and flexible dependency injection scenarios.
/// </remarks>
public class MediatorCommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MediatorCommandDispatcher>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorCommandDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
    public MediatorCommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILogger<MediatorCommandDispatcher>>();
    }

    /// <summary>
    /// Dispatches the specified command to its handler asynchronously.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to dispatch.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown.</param>
    /// <returns>A task representing the asynchronous dispatch operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the command type.</exception>
    public async Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command), "Command cannot be null.");
        }

        var handlerType = typeof(ICommandHandler<>).MakeGenericType(typeof(TCommand));

        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            var errorMessage = $"No handler registered for command type '{typeof(TCommand).Name}'.";
            _logger?.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            _logger?.LogDebug("Dispatching command of type '{CommandType}'", typeof(TCommand).Name);

            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod == null)
            {
                throw new InvalidOperationException(
                    $"Handler type '{handlerType.Name}' does not have a HandleAsync method.");
            }

            var task = (Task?)handleMethod.Invoke(handler, new object[] { command, cancellationToken });
            if (task == null)
            {
                throw new InvalidOperationException(
                    $"Handler's HandleAsync method returned null for command type '{typeof(TCommand).Name}'.");
            }

            await task;

            _logger?.LogDebug("Successfully dispatched command of type '{CommandType}'", typeof(TCommand).Name);
        }
        catch (Exception ex) when (!(ex is ArgumentNullException))
        {
            _logger?.LogError(ex, "Error dispatching command of type '{CommandType}'", typeof(TCommand).Name);
            throw;
        }
    }
}
