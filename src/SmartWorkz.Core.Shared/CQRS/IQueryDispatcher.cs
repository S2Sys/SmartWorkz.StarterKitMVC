namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for dispatching queries to their handlers and returning results.
/// </summary>
/// <remarks>
/// The query dispatcher routes queries to their corresponding handlers via dependency injection.
/// Implementations should resolve handlers from the service provider and invoke their HandleAsync method.
/// </remarks>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatches a query to its registered handler asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to dispatch.</typeparam>
    /// <typeparam name="TResult">The type of result returned by the query handler.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    /// <returns>A task representing the asynchronous dispatch operation, containing the query result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handler is registered for the query type.</exception>
    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
