namespace SmartWorkz.Mobile.Data.Database;

/// <summary>Database transaction wrapper.</summary>
public interface IDbTransaction : IAsyncDisposable
{
    /// <summary>Commit changes.</summary>
    Task CommitAsync();

    /// <summary>Rollback changes.</summary>
    Task RollbackAsync();

    /// <summary>Get connection associated with this transaction.</summary>
    IDbConnection Connection { get; }
}
