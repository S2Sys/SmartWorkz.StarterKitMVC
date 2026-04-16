namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Marker interface for cached Dapper repositories.
/// Implementations inherit from CachedDapperRepository and gain:
/// - Error handling (SqlException → RepositoryException with context)
/// - Logging on all SP calls
/// - Optional IMemoryCache integration for read queries
/// - Multi-result-set support (QueryMultipleAsync for 2+ result sets)
/// </summary>
public interface ICachedDapperRepository
{
}
