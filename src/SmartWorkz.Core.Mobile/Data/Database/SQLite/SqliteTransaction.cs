using SQLite;

namespace SmartWorkz.Mobile.Data.Database.SQLite;

/// <summary>
/// SQLite transaction wrapper implementing IDbTransaction interface.
/// Manages transaction lifecycle with commit and rollback support.
/// </summary>
internal class SqliteTransaction : IDbTransaction
{
    private readonly SQLiteAsyncConnection _connection;
    private readonly SqliteConnection _transactionConnection;
    private bool _disposed;
    private bool _committed;
    private bool _rolledBack;

    public IDbConnection Connection => _transactionConnection;

    public SqliteTransaction(SQLiteAsyncConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _transactionConnection = new SqliteConnection(connection);
    }

    /// <summary>
    /// Commit transaction changes.
    /// </summary>
    public async Task CommitAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SqliteTransaction));

        if (_committed || _rolledBack)
            throw new InvalidOperationException("Transaction has already been committed or rolled back.");

        try
        {
            await _connection.ExecuteAsync("COMMIT");
            _committed = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error committing transaction.", ex);
        }
    }

    /// <summary>
    /// Rollback transaction changes.
    /// </summary>
    public async Task RollbackAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SqliteTransaction));

        if (_committed || _rolledBack)
            throw new InvalidOperationException("Transaction has already been committed or rolled back.");

        try
        {
            await _connection.ExecuteAsync("ROLLBACK");
            _rolledBack = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error rolling back transaction.", ex);
        }
    }

    /// <summary>
    /// Dispose of transaction resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            // If not committed or rolled back, rollback to ensure cleanup
            if (!_committed && !_rolledBack)
            {
                await _connection.ExecuteAsync("ROLLBACK").ConfigureAwait(false);
            }

            await _transactionConnection.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            // Ignore disposal errors
        }

        GC.SuppressFinalize(this);
    }
}
