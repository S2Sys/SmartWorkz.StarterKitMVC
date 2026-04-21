namespace SmartWorkz.Shared;

using System.Data;

public static class DapperHelper
{
    public static async Task<Result<T?>> DapperQuerySingleAsync<T>(
        this IDbConnection connection,
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        int commandTimeout = 30) where T : class
    {
        try
        {
            var method = typeof(Dapper.SqlMapper).GetMethod(
                nameof(Dapper.SqlMapper.QuerySingleOrDefaultAsync),
                [typeof(IDbConnection), typeof(string), typeof(object), typeof(IDbTransaction), typeof(int?), typeof(System.Data.CommandType?)]);

            if (method == null)
                return Result.Fail<T?>(Error.FromException(
                    new InvalidOperationException("Dapper package not installed. Install-Package Dapper"),
                    "DAPPER.NOT_INSTALLED"));

            var result = await (dynamic)method.Invoke(null, new[] { connection, sql, parameters, transaction, (int?)commandTimeout, (System.Data.CommandType?)null })!;
            return Result.Ok<T?>((T?)result);
        }
        catch (Exception ex)
        {
            return Result.Fail<T?>(Error.FromException(ex, "DAPPER.QUERY_FAILED"));
        }
    }

    public static async Task<Result<List<T>>> DapperQueryAsync<T>(
        this IDbConnection connection,
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        int commandTimeout = 30) where T : class
    {
        try
        {
            var method = typeof(Dapper.SqlMapper).GetMethod(
                nameof(Dapper.SqlMapper.QueryAsync),
                new[] { typeof(IDbConnection), typeof(string), typeof(object), typeof(IDbTransaction), typeof(int?), typeof(System.Data.CommandType?) });

            if (method == null)
                return Result.Fail<List<T>>(Error.FromException(
                    new InvalidOperationException("Dapper package not installed. Install-Package Dapper"),
                    "DAPPER.NOT_INSTALLED"));

            var result = await (dynamic)method.Invoke(null, new[] { connection, sql, parameters, transaction, (int?)commandTimeout, (System.Data.CommandType?)null })!;
            return Result.Ok(((IEnumerable<T>)result).ToList());
        }
        catch (Exception ex)
        {
            return Result.Fail<List<T>>(Error.FromException(ex, "DAPPER.QUERY_FAILED"));
        }
    }

    public static async Task<Result<int>> DapperExecuteAsync(
        this IDbConnection connection,
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        int commandTimeout = 30)
    {
        try
        {
            var method = typeof(Dapper.SqlMapper).GetMethod(
                nameof(Dapper.SqlMapper.ExecuteAsync),
                new[] { typeof(IDbConnection), typeof(string), typeof(object), typeof(IDbTransaction), typeof(int?), typeof(System.Data.CommandType?) });

            if (method == null)
                return Result.Fail<int>(Error.FromException(
                    new InvalidOperationException("Dapper package not installed. Install-Package Dapper"),
                    "DAPPER.NOT_INSTALLED"));

            var result = await (dynamic)method.Invoke(null, new[] { connection, sql, parameters, transaction, (int?)commandTimeout, (System.Data.CommandType?)null })!;
            return Result.Ok((int)result);
        }
        catch (Exception ex)
        {
            return Result.Fail<int>(Error.FromException(ex, "DAPPER.EXECUTE_FAILED"));
        }
    }
}
