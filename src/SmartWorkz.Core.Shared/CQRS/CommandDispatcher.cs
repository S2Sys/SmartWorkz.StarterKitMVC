namespace SmartWorkz.Shared;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Concrete implementation of ICommandDispatcher that routes commands to their handlers.
/// </summary>
/// <remarks>
/// This dispatcher resolves handlers from the dependency injection container at dispatch time,
/// enabling loose coupling between command publishers and handlers. Handlers are invoked via reflection
/// to support flexible handler registration patterns.
/// </remarks>
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandDispatcher>? _logger;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILogger<CommandDispatcher>>();
    }

    public async Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command), "Command cannot be null.");

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
                throw new InvalidOperationException($"Handler type '{handlerType.Name}' does not have a HandleAsync method.");

            var task = (Task?)handleMethod.Invoke(handler, new object[] { command, cancellationToken });
            if (task == null)
                throw new InvalidOperationException($"Handler's HandleAsync method returned null for command type '{typeof(TCommand).Name}'.");

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
