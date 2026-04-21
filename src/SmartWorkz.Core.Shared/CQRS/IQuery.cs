namespace SmartWorkz.Core.Shared.CQRS;

/// <summary>
/// Marker interface for query objects that return a result without modifying state.
/// </summary>
/// <typeparam name="TResult">The type of result the query returns.</typeparam>
/// <remarks>
/// Queries are part of the CQRS pattern and represent read-only operations that do not modify state.
/// Each query should be handled by exactly one query handler.
/// </remarks>
public interface IQuery<out TResult>
{
}
