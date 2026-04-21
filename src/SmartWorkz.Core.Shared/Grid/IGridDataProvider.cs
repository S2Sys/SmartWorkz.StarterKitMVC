using SmartWorkz.Core.Shared.Results;

namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for grid data fetching. Implementations handle API calls or in-memory queries.
/// Enables platform independence: Web uses HTTP, MAUI uses direct API client, Desktop uses local DB.
/// </summary>
public interface IGridDataProvider
{
    /// <summary>
    /// Fetch paged grid data based on request (sorting, filtering, pagination).
    /// </summary>
    /// <typeparam name="T">Data type of grid items.</typeparam>
    /// <param name="request">Grid request with sorting, paging, and filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing GridResponse or error details.</returns>
    Task<Result<GridResponse<T>>> GetDataAsync<T>(GridRequest request, CancellationToken cancellationToken = default);
}
