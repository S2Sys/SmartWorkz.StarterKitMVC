namespace SmartWorkz.Mobile.Data.Database;

/// <summary>
/// Cross-platform database abstraction.
/// Supports SQLite on all platforms with synchronized access.
/// </summary>
public interface IDatabase
{
    /// <summary>Open database connection asynchronously.</summary>
    Task OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Close database connection asynchronously.</summary>
    Task CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>Check if database is connected.</summary>
    bool IsConnected { get; }

    /// <summary>Get database version.</summary>
    int Version { get; }

    /// <summary>Execute migrations up to current schema version.</summary>
    Task RunMigrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Create new connection for query execution.</summary>
    IDbConnection CreateConnection();

    /// <summary>Execute raw SQL with parameters.</summary>
    Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Query with results.</summary>
    Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>Query single result or null.</summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>Begin transaction.</summary>
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Database path (for debugging).</summary>
    string? DatabasePath { get; }
}
