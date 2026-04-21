namespace SmartWorkz.Core.Shared.CQRS;

/// <summary>
/// Handler for processing a specific query type and returning results.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle, must implement <see cref="IQuery{TResult}"/>.</typeparam>
/// <typeparam name="TResult">The type of result returned by the query handler.</typeparam>
/// <remarks>
/// Query handlers encapsulate the logic for handling read-only data retrieval operations.
/// Each query type should have exactly one handler, but handlers can be registered
/// multiple times in the dependency injection container if needed for different scenarios.
/// </remarks>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the specified query asynchronously and returns the result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown.</param>
    /// <returns>A task that represents the asynchronous handle operation and contains the query result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
