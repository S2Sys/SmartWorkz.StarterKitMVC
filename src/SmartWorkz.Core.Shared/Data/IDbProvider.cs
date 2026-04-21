namespace SmartWorkz.Shared;

/// <summary>
/// Abstraction for database provider-specific operations.
/// Supports multiple providers: SQL Server, MySQL, PostgreSQL, SQLite, Oracle.
/// </summary>
public interface IDbProvider
{
    /// <summary>Provider type identifier (e.g., "SqlServer", "MySql").</summary>
    string ProviderName { get; }

    /// <summary>Connection string for this provider instance.</summary>
    string ConnectionString { get; }

    /// <summary>Create connection with connection string.</summary>
    IDbConnection CreateConnection(string connectionString);

    /// <summary>Get parameter prefix for this provider (@, :, $).</summary>
    string GetParameterPrefix();

    /// <summary>Get SQL for last inserted ID based on provider.</summary>
    string GetLastInsertIdSql();

    /// <summary>Get SQL for pagination based on provider.</summary>
    string GetPaginationSql(string baseSql, int pageNumber, int pageSize);

    /// <summary>Format table/column name for provider (e.g., [brackets] for SQL Server).</summary>
    string FormatIdentifier(string name);

    /// <summary>Test connection validity.</summary>
    Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default);
}

/// <summary>Enum of supported database providers.</summary>
public enum DatabaseProvider
{
    SqlServer,
    MySql,
    PostgreSql,
    Sqlite,
    Oracle
}
