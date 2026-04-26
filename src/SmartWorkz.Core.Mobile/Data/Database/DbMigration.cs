namespace SmartWorkz.Mobile.Data.Database;

/// <summary>Base class for database schema migrations.</summary>
public abstract class DbMigration
{
    /// <summary>Migration version number (001, 002, etc).</summary>
    public abstract int Version { get; }

    /// <summary>Description of changes in this migration.</summary>
    public abstract string Description { get; }

    /// <summary>Apply migration to database.</summary>
    public abstract Task UpAsync(IDbConnection connection);

    /// <summary>Revert migration (optional).</summary>
    public virtual Task DownAsync(IDbConnection connection)
    {
        return Task.CompletedTask;
    }
}
