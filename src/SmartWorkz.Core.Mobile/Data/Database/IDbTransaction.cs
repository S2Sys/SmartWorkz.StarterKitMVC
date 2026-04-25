namespace SmartWorkz.Mobile.Data.Database;

/// <summary>Database transaction wrapper.</summary>
public interface IDbTransaction : IAsyncDisposable
{
    /// <summary>Commit changes.</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>Rollback changes.</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>Get connection associated with this transaction.</summary>
    IDbConnection Connection { get; }
}
