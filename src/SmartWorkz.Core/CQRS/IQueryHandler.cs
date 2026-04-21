namespace SmartWorkz.Core;

using SmartWorkz.Core.Shared.CQRS;

/// <summary>
/// Handler for processing a specific query type and returning results.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle, must implement <see cref="IQuery{TResult}"/>.</typeparam>
/// <typeparam name="TResult">The type of result returned by the query handler.</typeparam>
/// <remarks>
/// Query handlers encapsulate the logic for handling read-only data retrieval operations.
/// Each query type should have exactly one handler, but handlers can be registered
/// multiple times in the dependency injection container if needed for different scenarios.
///
/// This interface is re-exported from SmartWorkz.Core.Shared for convenience.
/// </remarks>
public interface IQueryHandler<in TQuery, TResult> : SmartWorkz.Core.Shared.CQRS.IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
}
