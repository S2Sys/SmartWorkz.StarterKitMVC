namespace SmartWorkz.Core.Shared.Data;

using System.Data;
using Dapper;

/// <summary>
/// Helper for executing multiple queries in a single database roundtrip.
/// Eliminates N+1 query problems by batching queries together.
/// </summary>
public static class QueryMultipleHelper
{
    /// <summary>
    /// Execute multiple queries and return results as tuple.
    /// Single roundtrip, single SQL execution, improved performance.
    /// </summary>
    /// <example>
    /// var sql = @"
    ///     SELECT * FROM Users WHERE Department = @Dept;
    ///     SELECT * FROM Orders WHERE CreatedDate > @Date;
    ///     SELECT * FROM Products WHERE IsActive = 1";
    ///
    /// var (users, orders, products) = await QueryMultipleAsync{User, Order, Product}(
    ///     connection,
    ///     sql,
    ///     new { Dept = "Sales", Date = DateTime.Now.AddMonths(-1) }
    /// );
    /// </example>
    public static async Task<(List<T1>, List<T2>)> QueryMultipleAsync<T1, T2>(
        IDbConnection connection,
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using (var reader = await connection.QueryMultipleAsync(sql, param, commandTimeout: commandTimeout, commandType: commandType))
        {
            var result1 = (await reader.ReadAsync<T1>()).ToList();
            var result2 = (await reader.ReadAsync<T2>()).ToList();
            return (result1, result2);
        }
    }

    /// <summary>Execute 3 queries in single roundtrip.</summary>
    public static async Task<(List<T1>, List<T2>, List<T3>)> QueryMultipleAsync<T1, T2, T3>(
        IDbConnection connection,
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using (var reader = await connection.QueryMultipleAsync(sql, param, commandTimeout: commandTimeout, commandType: commandType))
        {
            var result1 = (await reader.ReadAsync<T1>()).ToList();
            var result2 = (await reader.ReadAsync<T2>()).ToList();
            var result3 = (await reader.ReadAsync<T3>()).ToList();
            return (result1, result2, result3);
        }
    }

    /// <summary>Execute 4 queries in single roundtrip.</summary>
    public static async Task<(List<T1>, List<T2>, List<T3>, List<T4>)> QueryMultipleAsync<T1, T2, T3, T4>(
        IDbConnection connection,
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using (var reader = await connection.QueryMultipleAsync(sql, param, commandTimeout: commandTimeout, commandType: commandType))
        {
            var result1 = (await reader.ReadAsync<T1>()).ToList();
            var result2 = (await reader.ReadAsync<T2>()).ToList();
            var result3 = (await reader.ReadAsync<T3>()).ToList();
            var result4 = (await reader.ReadAsync<T4>()).ToList();
            return (result1, result2, result3, result4);
        }
    }

    /// <summary>Execute 5 queries in single roundtrip.</summary>
    public static async Task<(List<T1>, List<T2>, List<T3>, List<T4>, List<T5>)> QueryMultipleAsync<T1, T2, T3, T4, T5>(
        IDbConnection connection,
        string sql,
        object? param = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        using (var reader = await connection.QueryMultipleAsync(sql, param, commandTimeout: commandTimeout, commandType: commandType))
        {
            var result1 = (await reader.ReadAsync<T1>()).ToList();
            var result2 = (await reader.ReadAsync<T2>()).ToList();
            var result3 = (await reader.ReadAsync<T3>()).ToList();
            var result4 = (await reader.ReadAsync<T4>()).ToList();
            var result5 = (await reader.ReadAsync<T5>()).ToList();
            return (result1, result2, result3, result4, result5);
        }
    }
}

/// <summary>Result wrapper for query operations.</summary>
public class QueryResult<T>
{
    public bool IsSuccess { get; set; }
    public List<T> Data { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public static QueryResult<T> Ok(List<T> data) => new() { IsSuccess = true, Data = data };
    public static QueryResult<T> Fail(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
