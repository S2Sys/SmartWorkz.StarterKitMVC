namespace SmartWorkz.Shared;

using System.Data;
using Microsoft.Data.SqlClient;

/// <summary>
/// Factory for creating database provider instances.
/// Resolves provider name from connection string or explicit specification.
/// </summary>
public static class DbProviderFactory
{
    private static readonly Dictionary<string, IDbProvider> _providers = new();

    static DbProviderFactory()
    {
        // Register default providers
        Register(DatabaseProvider.SqlServer, new SqlServerDbProvider());
        Register(DatabaseProvider.MySql, new MySqlDbProvider());
        Register(DatabaseProvider.PostgreSql, new PostgreSqlDbProvider());
        Register(DatabaseProvider.Sqlite, new SqliteDbProvider());
        Register(DatabaseProvider.Oracle, new OracleDbProvider());
    }

    /// <summary>Register custom provider implementation.</summary>
    public static void Register(DatabaseProvider provider, IDbProvider implementation)
        => _providers[provider.ToString()] = implementation;

    /// <summary>Get provider by name.</summary>
    public static IDbProvider? GetProvider(string providerName)
        => _providers.TryGetValue(providerName, out var provider) ? provider : null;

    /// <summary>Get provider by enum value.</summary>
    public static IDbProvider? GetProvider(DatabaseProvider provider) =>
        GetProvider(provider.ToString());

    /// <summary>Get provider from connection string (detects provider automatically).</summary>
    public static IDbProvider GetProviderFromConnectionString(string connectionString)
    {
        if (connectionString.Contains("Initial Catalog") || connectionString.Contains("Server="))
            return _providers["SqlServer"];
        if (connectionString.Contains("Server=") && connectionString.Contains("Port="))
            return _providers["PostgreSql"];
        if (connectionString.Contains("Host="))
            return _providers["MySql"];
        if (connectionString.Contains("Data Source="))
            return _providers["Sqlite"];

        throw new InvalidOperationException($"Cannot determine provider from connection string");
    }

    // Concrete implementations
    private sealed class SqlServerDbProvider : IDbProvider
    {
        private readonly string _connectionString;

        public string ProviderName => "SqlServer";
        public string ConnectionString => _connectionString;

        public SqlServerDbProvider(string connectionString = "")
        {
            _connectionString = connectionString;
        }

        public string GetParameterPrefix() => "@";
        public string GetLastInsertIdSql() => "SELECT @@IDENTITY";
        public string GetPaginationSql(string baseSql, int pageNumber, int pageSize)
            => $"{baseSql} OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
        public string FormatIdentifier(string name) => $"[{name}]";
        public IDbConnection CreateConnection(string connectionString) => new SqlConnection(connectionString);
        public async Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            try
            {
                using var conn = CreateConnection(connectionString) as SqlConnection;
                if (conn != null)
                {
                    await conn.OpenAsync(cancellationToken);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }
    }

    private sealed class MySqlDbProvider : IDbProvider
    {
        private readonly string _connectionString;

        public string ProviderName => "MySql";
        public string ConnectionString => _connectionString;

        public MySqlDbProvider(string connectionString = "")
        {
            _connectionString = connectionString;
        }

        public string GetParameterPrefix() => "@";
        public string GetLastInsertIdSql() => "SELECT LAST_INSERT_ID()";
        public string GetPaginationSql(string baseSql, int pageNumber, int pageSize)
            => $"{baseSql} LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}";
        public string FormatIdentifier(string name) => $"`{name}`";
        public IDbConnection CreateConnection(string connectionString)
            => throw new NotSupportedException("MySql requires MySql.Data NuGet package");
        public async Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
            => false;
    }

    private sealed class PostgreSqlDbProvider : IDbProvider
    {
        private readonly string _connectionString;

        public string ProviderName => "PostgreSql";
        public string ConnectionString => _connectionString;

        public PostgreSqlDbProvider(string connectionString = "")
        {
            _connectionString = connectionString;
        }

        public string GetParameterPrefix() => "$";
        public string GetLastInsertIdSql() => "SELECT lastval()";
        public string GetPaginationSql(string baseSql, int pageNumber, int pageSize)
            => $"{baseSql} LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}";
        public string FormatIdentifier(string name) => $"\"{name}\"";
        public IDbConnection CreateConnection(string connectionString)
            => throw new NotSupportedException("PostgreSQL requires Npgsql NuGet package");
        public async Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
            => false;
    }

    private sealed class SqliteDbProvider : IDbProvider
    {
        private readonly string _connectionString;

        public string ProviderName => "Sqlite";
        public string ConnectionString => _connectionString;

        public SqliteDbProvider(string connectionString = "")
        {
            _connectionString = connectionString;
        }

        public string GetParameterPrefix() => "@";
        public string GetLastInsertIdSql() => "SELECT last_insert_rowid()";
        public string GetPaginationSql(string baseSql, int pageNumber, int pageSize)
            => $"{baseSql} LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}";
        public string FormatIdentifier(string name) => $"[{name}]";
        public IDbConnection CreateConnection(string connectionString)
            => throw new NotSupportedException("SQLite requires System.Data.SQLite NuGet package");
        public async Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
            => false;
    }

    private sealed class OracleDbProvider : IDbProvider
    {
        private readonly string _connectionString;

        public string ProviderName => "Oracle";
        public string ConnectionString => _connectionString;

        public OracleDbProvider(string connectionString = "")
        {
            _connectionString = connectionString;
        }

        public string GetParameterPrefix() => ":";
        public string GetLastInsertIdSql() => throw new NotSupportedException("Oracle requires explicit sequence");
        public string GetPaginationSql(string baseSql, int pageNumber, int pageSize)
            => $"SELECT * FROM ({baseSql}) WHERE ROWNUM <= {pageSize * pageNumber} MINUS SELECT * FROM ({baseSql}) WHERE ROWNUM < {(pageNumber - 1) * pageSize}";
        public string FormatIdentifier(string name) => name.ToUpper();
        public IDbConnection CreateConnection(string connectionString)
            => throw new NotSupportedException("Oracle requires Oracle.ManagedDataAccess NuGet package");
        public async Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
            => false;
    }
}
