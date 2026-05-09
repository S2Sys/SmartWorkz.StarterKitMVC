namespace SmartWorkz.Shared;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Concrete implementation of IQueryDispatcher that routes queries to their handlers.
/// </summary>
/// <remarks>
/// This dispatcher resolves handlers from the dependency injection container at dispatch time,
/// enabling loose coupling between query issuers and handlers. Handlers are invoked via reflection
/// to support flexible handler registration patterns.
/// </remarks>
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryDispatcher>? _logger;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILogger<QueryDispatcher>>();
    }

    public async Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query), "Query cannot be null.");

        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(typeof(TQuery), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            var errorMessage = $"No handler registered for query type '{typeof(TQuery).Name}'.";
            _logger?.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            _logger?.LogDebug("Dispatching query of type '{QueryType}'", typeof(TQuery).Name);

            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod == null)
                throw new InvalidOperationException($"Handler type '{handlerType.Name}' does not have a HandleAsync method.");

            var task = (Task<TResult>?)handleMethod.Invoke(handler, new object[] { query, cancellationToken });
            if (task == null)
                throw new InvalidOperationException($"Handler's HandleAsync method returned null for query type '{typeof(TQuery).Name}'.");

            var result = await task;
            _logger?.LogDebug("Successfully dispatched query of type '{QueryType}'", typeof(TQuery).Name);
            return result;
        }
        catch (Exception ex) when (!(ex is ArgumentNullException))
        {
            _logger?.LogError(ex, "Error dispatching query of type '{QueryType}'", typeof(TQuery).Name);
            throw;
        }
    }
}
