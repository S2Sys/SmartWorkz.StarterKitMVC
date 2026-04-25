using SQLite;

namespace SmartWorkz.Mobile.Data.Database.SQLite;

/// <summary>
/// SQLite connection wrapper implementing IDbConnection interface.
/// Provides async query execution with parameter support.
/// </summary>
internal class SqliteConnection : IDbConnection
{
    private readonly SQLiteAsyncConnection _connection;
    private bool _disposed;

    public SqliteConnection(SQLiteAsyncConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// Execute non-query SQL statement.
    /// </summary>
    public async Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SqliteConnection));

        try
        {
            if (parameters == null || parameters.Count == 0)
            {
                return await _connection.ExecuteAsync(sql);
            }

            // Convert parameters dictionary to array of values
            var sqlWithoutParams = sql;
            var paramValues = new List<object?>();

            foreach (var param in parameters)
            {
                paramValues.Add(param.Value);
            }

            return await _connection.ExecuteAsync(sql, paramValues.ToArray());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error executing SQL: {sql}", ex);
        }
    }

    /// <summary>
    /// Execute query and return results as list.
    /// </summary>
    public async Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null)
        where T : class, new()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SqliteConnection));

        try
        {
            if (parameters == null || parameters.Count == 0)
            {
                return await _connection.QueryAsync<T>(sql);
            }

            // Use TableQuery with parameters
            var paramValues = new List<object?>();
            foreach (var param in parameters)
            {
                paramValues.Add(param.Value);
            }

            return await _connection.QueryAsync<T>(sql, paramValues.ToArray());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error querying SQL: {sql}", ex);
        }
    }

    /// <summary>
    /// Execute query and return first result or null.
    /// </summary>
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null)
        where T : class, new()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SqliteConnection));

        try
        {
            if (parameters == null || parameters.Count == 0)
            {
                var result = await _connection.QueryAsync<T>(sql);
                return result.FirstOrDefault();
            }

            var paramValues = new List<object?>();
            foreach (var param in parameters)
            {
                paramValues.Add(param.Value);
            }

            var results = await _connection.QueryAsync<T>(sql, paramValues.ToArray());
            return results.FirstOrDefault();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error querying first result from SQL: {sql}", ex);
        }
    }

    /// <summary>
    /// Dispose of connection resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
            }
        }
        catch
        {
            // Ignore disposal errors
        }

        GC.SuppressFinalize(this);
    }
}
