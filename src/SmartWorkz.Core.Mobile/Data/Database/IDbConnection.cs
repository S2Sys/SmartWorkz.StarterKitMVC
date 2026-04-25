namespace SmartWorkz.Mobile.Data.Database;

/// <summary>Database connection wrapper.</summary>
public interface IDbConnection : IAsyncDisposable
{
    /// <summary>Execute non-query SQL.</summary>
    Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null);

    /// <summary>Query with results.</summary>
    Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null)
        where T : class, new();

    /// <summary>Query single result.</summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null)
        where T : class, new();
}
