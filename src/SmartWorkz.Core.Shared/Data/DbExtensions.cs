namespace SmartWorkz.Core.Shared.Data;

using System.Data;

/// <summary>
/// Extension methods on IDbProvider that provide shorter, more ergonomic aliases
/// for common database operations.
/// </summary>
public static class DbExtensions
{
    /// <summary>
    /// Execute query and return results (ADO.NET alias).
    /// </summary>
    public static async Task<IEnumerable<T>> QueryAsync<T>(
        this IDbProvider provider,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        CancellationToken ct = default)
    {
        var connection = provider.CreateConnection(provider.ConnectionString);
        var parameters = ConvertToParameterDictionary(param, provider);
        var result = await AdoHelper.ExecuteQueryAsync(
            connection,
            sql,
            provider,
            reader => MapDataReader<T>(reader),
            parameters);
        return result.Succeeded ? result.Data : [];
    }

    /// <summary>
    /// Execute scalar query and return single value (ADO.NET alias).
    /// </summary>
    public static async Task<T?> ScalarAsync<T>(
        this IDbProvider provider,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        CancellationToken ct = default)
    {
        var connection = provider.CreateConnection(provider.ConnectionString);
        var parameters = ConvertToParameterDictionary(param, provider);
        var result = await AdoHelper.ExecuteScalarAsync<T>(
            connection,
            sql,
            provider,
            parameters);
        return result.Succeeded ? result.Data : default;
    }

    /// <summary>
    /// Execute non-query command (INSERT, UPDATE, DELETE) returning rows affected (ADO.NET alias).
    /// </summary>
    public static async Task<int> NonQueryAsync(
        this IDbProvider provider,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        CancellationToken ct = default)
    {
        var connection = provider.CreateConnection(provider.ConnectionString);
        var parameters = ConvertToParameterDictionary(param, provider);
        var result = await AdoHelper.ExecuteNonQueryAsync(
            connection,
            sql,
            provider,
            parameters);
        return result.Succeeded ? result.Data : 0;
    }

    /// <summary>
    /// Execute query using Dapper and return results (Dapper alias).
    /// </summary>
    public static async Task<IEnumerable<T>> QueryAsync<T>(
        this IDbProvider provider,
        string sql,
        object? param) where T : class
    {
        var connection = provider.CreateConnection(provider.ConnectionString);
        var result = await DapperHelper.DapperQueryAsync<T>(
            connection,
            sql,
            param);
        return result.Succeeded ? result.Data : [];
    }

    /// <summary>
    /// Execute query using Dapper and return single result (Dapper alias).
    /// </summary>
    public static async Task<T?> QuerySingleAsync<T>(
        this IDbProvider provider,
        string sql,
        object? param = null) where T : class
    {
        var connection = provider.CreateConnection(provider.ConnectionString);
        var result = await DapperHelper.DapperQuerySingleAsync<T>(
            connection,
            sql,
            param);
        return result.Succeeded ? result.Data : null;
    }

    /// <summary>
    /// Execute command using Dapper returning rows affected (Dapper alias).
    /// </summary>
    public static async Task<int> ExecuteAsync(
        this IDbProvider provider,
        string sql,
        object? param = null)
    {
        var connection = provider.CreateConnection(provider.ConnectionString);
        var result = await DapperHelper.DapperExecuteAsync(
            connection,
            sql,
            param);
        return result.Succeeded ? result.Data : 0;
    }

    // Helper methods

    /// <summary>
    /// Convert anonymous object or dictionary to parameter dictionary.
    /// </summary>
    private static Dictionary<string, object?>? ConvertToParameterDictionary(
        object? param,
        IDbProvider provider)
    {
        if (param == null)
            return null;

        if (param is Dictionary<string, object?> dict)
            return dict;

        var paramDict = new Dictionary<string, object?>();
        var properties = param.GetType().GetProperties();

        foreach (var prop in properties)
        {
            paramDict[prop.Name] = prop.GetValue(param);
        }

        return paramDict;
    }

    /// <summary>
    /// Map IDataReader row to object of type T using reflection.
    /// </summary>
    private static T MapDataReader<T>(IDataReader reader)
    {
        var instance = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            try
            {
                var ordinal = reader.GetOrdinal(prop.Name);
                var value = reader.GetValue(ordinal);
                if (value != DBNull.Value)
                {
                    prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));
                }
            }
            catch
            {
                // Property not found in reader or conversion failed - skip
            }
        }

        return instance;
    }
}
