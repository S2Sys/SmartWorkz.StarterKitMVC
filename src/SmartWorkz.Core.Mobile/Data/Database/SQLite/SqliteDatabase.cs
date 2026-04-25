using SQLite;

namespace SmartWorkz.Mobile.Data.Database.SQLite;

/// <summary>
/// SQLite database implementation for cross-platform mobile applications.
/// Provides async operations with transaction support and automatic migrations.
/// Thread-safe and IAsyncDisposable compliant.
/// </summary>
public class SqliteDatabase : IDatabase, IAsyncDisposable
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _databasePath;
    private readonly SemaphoreSlim _connectionLock;
    private readonly DbMigrationRunner _migrationRunner;
    private bool _disposed;

    public bool IsConnected => _connection != null && !_disposed;
    public string? DatabasePath => _databasePath;

    private int _version;
    public int Version
    {
        get => _version;
        private set => _version = value;
    }

    public SqliteDatabase(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentNullException(nameof(databasePath));

        _databasePath = databasePath;
        _connectionLock = new SemaphoreSlim(1, 1);
        _migrationRunner = new DbMigrationRunner(this);
        _version = 0;
    }

    /// <summary>
    /// Open database connection asynchronously.
    /// Creates or opens the SQLite database file.
    /// </summary>
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsConnected)
            return;

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (IsConnected)
                return;

            // Create directory if it doesn't exist (for file-based databases)
            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(directory) && directory != ":memory:")
            {
                Directory.CreateDirectory(directory);
            }

            // Create and open connection
            _connection = new SQLiteAsyncConnection(_databasePath, false);

            // Set pragmas for better concurrency
            await _connection.ExecuteAsync("PRAGMA journal_mode = WAL");
            await _connection.ExecuteAsync("PRAGMA synchronous = NORMAL");
            await _connection.ExecuteAsync("PRAGMA cache_size = -64000");
            await _connection.ExecuteAsync("PRAGMA foreign_keys = ON");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _connection = null;
            throw new InvalidOperationException(
                $"Failed to open database at path: {_databasePath}", ex);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Close database connection asynchronously.
    /// </summary>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection = null;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error closing database connection.", ex);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Execute raw SQL with optional parameters.
    /// Returns number of rows affected.
    /// </summary>
    public async Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
            throw new InvalidOperationException("Database is not connected.");

        try
        {
            var connection = _connection!;

            if (parameters == null || parameters.Count == 0)
            {
                return await connection.ExecuteAsync(sql);
            }

            // Convert parameters to array
            var paramValues = parameters.Values.ToArray();
            return await connection.ExecuteAsync(sql, paramValues);
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
    public async Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
            throw new InvalidOperationException("Database is not connected.");

        try
        {
            var connection = _connection!;

            if (parameters == null || parameters.Count == 0)
            {
                return await connection.QueryAsync<T>(sql);
            }

            var paramValues = parameters.Values.ToArray();
            return await connection.QueryAsync<T>(sql, paramValues);
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
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
            throw new InvalidOperationException("Database is not connected.");

        try
        {
            var connection = _connection!;

            if (parameters == null || parameters.Count == 0)
            {
                var results = await connection.QueryAsync<T>(sql);
                return results.FirstOrDefault();
            }

            var paramValues = parameters.Values.ToArray();
            var resultList = await connection.QueryAsync<T>(sql, paramValues);
            return resultList.FirstOrDefault();
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
    /// Create new connection for independent query execution.
    /// </summary>
    public IDbConnection CreateConnection()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Database is not connected.");

        return new SqliteConnection(_connection!);
    }

    /// <summary>
    /// Begin a new transaction.
    /// </summary>
    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
            throw new InvalidOperationException("Database is not connected.");

        try
        {
            await _connection!.ExecuteAsync("BEGIN TRANSACTION", null, cancellationToken);
            return new SqliteTransaction(_connection);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error beginning transaction.", ex);
        }
    }

    /// <summary>
    /// Run all pending database migrations.
    /// Ensures schema is up-to-date with all pending migrations applied.
    /// </summary>
    public async Task RunMigrationsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
            throw new InvalidOperationException("Database is not connected.");

        try
        {
            await _migrationRunner.RunMigrationsAsync(cancellationToken);
            _version = await _migrationRunner.GetCurrentVersionAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error running migrations.", ex);
        }
    }

    /// <summary>
    /// Dispose database resources asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            if (IsConnected)
            {
                await CloseAsync();
            }

            _connectionLock?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }

        GC.SuppressFinalize(this);
    }
}
