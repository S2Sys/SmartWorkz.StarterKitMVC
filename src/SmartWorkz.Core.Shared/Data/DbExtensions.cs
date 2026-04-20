namespace SmartWorkz.Core.Shared.Data;

using System.Data;

/// <summary>Database extension methods providing shorter aliases for AdoHelper and DapperHelper.</summary>
public static class DbExtensions
{
    /// <summary>Execute a scalar query (single value) using ADO.NET.</summary>
    public static Task<Result<T?>> ScalarAsync<T>(
        this IDbConnection connection,
        string sql,
        IDbProvider provider,
        object? parameters = null)
        => AdoHelper.ExecuteScalarAsync<T>(connection, sql, provider, ToDictionary(parameters));

    /// <summary>Execute a non-query command (INSERT/UPDATE/DELETE) using ADO.NET.</summary>
    public static Task<Result<int>> NonQueryAsync(
        this IDbConnection connection,
        string sql,
        IDbProvider provider,
        object? parameters = null)
        => AdoHelper.ExecuteNonQueryAsync(connection, sql, provider, ToDictionary(parameters));

    /// <summary>Execute a query returning multiple values using Dapper.</summary>
    public static Task<Result<List<T>>> QueryAsync<T>(
        this IDbConnection connection,
        string sql,
        object? parameters = null) where T : class
        => connection.DapperQueryAsync<T>(sql, parameters);

    /// <summary>Execute a query returning a single value using Dapper.</summary>
    public static Task<Result<T?>> QuerySingleAsync<T>(
        this IDbConnection connection,
        string sql,
        object? parameters = null) where T : class
        => connection.DapperQuerySingleAsync<T>(sql, parameters);

    /// <summary>Execute a non-query command using Dapper.</summary>
    public static Task<Result<int>> ExecuteAsync(
        this IDbConnection connection,
        string sql,
        object? parameters = null)
        => connection.DapperExecuteAsync(sql, parameters);

    private static Dictionary<string, object?>? ToDictionary(object? parameters)
    {
        if (parameters == null)
            return null;

        if (parameters is Dictionary<string, object?> dict)
            return dict;

        var result = new Dictionary<string, object?>();
        foreach (var property in parameters.GetType().GetProperties())
        {
            if (property.CanRead)
                result[property.Name] = property.GetValue(parameters);
        }
        return result;
    }
}
