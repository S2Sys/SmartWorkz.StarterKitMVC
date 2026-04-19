namespace SmartWorkz.Core.Shared.Data;

using System.Data;
using System.Data.Common;

/// <summary>
/// ADO.NET helper for executing queries and managing connections.
/// Works with any IDbProvider implementation.
/// </summary>
public static class AdoHelper
{
    /// <summary>Execute scalar query (returns single value).</summary>
    public static async Task<Result<T?>> ExecuteScalarAsync<T>(
        IDbConnection connection,
        string commandText,
        IDbProvider provider,
        Dictionary<string, object?>? parameters = null,
        CommandType commandType = CommandType.Text,
        int commandTimeout = 30)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = commandTimeout;

            AddParameters(command, parameters, provider);

            if (connection.State != ConnectionState.Open)
                await OpenConnectionAsync(connection);

            var result = await ExecuteScalarAsyncInternal(command);
            return Result.Ok<T?>((T?)Convert.ChangeType(result, typeof(T)));
        }
        catch (Exception ex)
        {
            return Result.Fail<T?>(new Error("ADO.SCALAR_FAILED", ex.Message));
        }
    }

    /// <summary>Execute non-query command (INSERT, UPDATE, DELETE).</summary>
    public static async Task<Result<int>> ExecuteNonQueryAsync(
        IDbConnection connection,
        string commandText,
        IDbProvider provider,
        Dictionary<string, object?>? parameters = null,
        CommandType commandType = CommandType.Text,
        int commandTimeout = 30)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = commandTimeout;

            AddParameters(command, parameters, provider);

            if (connection.State != ConnectionState.Open)
                await OpenConnectionAsync(connection);

            var rowsAffected = await ExecuteNonQueryAsyncInternal(command);
            return Result.Ok(rowsAffected);
        }
        catch (Exception ex)
        {
            return Result.Fail<int>(new Error("ADO.EXECUTE_FAILED", ex.Message));
        }
    }

    /// <summary>Execute query and map results to objects.</summary>
    public static async Task<Result<List<T>>> ExecuteQueryAsync<T>(
        IDbConnection connection,
        string commandText,
        IDbProvider provider,
        Func<IDataReader, T> mapper,
        Dictionary<string, object?>? parameters = null,
        CommandType commandType = CommandType.Text,
        int commandTimeout = 30)
    {
        try
        {
            var results = new List<T>();

            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = commandTimeout;

            AddParameters(command, parameters, provider);

            if (connection.State != ConnectionState.Open)
                await OpenConnectionAsync(connection);

            using var reader = await ExecuteReaderAsyncInternal(command);
            while (await ReadAsyncInternal(reader))
            {
                results.Add(mapper(reader));
            }

            return Result.Ok(results);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<T>>(new Error("ADO.QUERY_FAILED", ex.Message));
        }
    }

    /// <summary>Execute stored procedure.</summary>
    public static async Task<Result<(int RowsAffected, Dictionary<string, object?> OutputParams)>> ExecuteStoredProcedureAsync(
        IDbConnection connection,
        string procedureName,
        IDbProvider provider,
        Dictionary<string, object?>? parameters = null,
        int commandTimeout = 30)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = commandTimeout;

            var outputParams = new Dictionary<string, object?>();
            AddParameters(command, parameters, provider, isStoredProc: true, outputParams);

            if (connection.State != ConnectionState.Open)
                await OpenConnectionAsync(connection);

            var rowsAffected = await ExecuteNonQueryAsyncInternal(command);

            // Extract output parameters
            foreach (IDataParameter param in command.Parameters)
            {
                if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                {
                    outputParams[param.ParameterName] = param.Value;
                }
            }

            return Result.Ok((rowsAffected, outputParams));
        }
        catch (Exception ex)
        {
            return Result.Fail<(int, Dictionary<string, object?>)>(
                new Error("ADO.SPROC_FAILED", ex.Message));
        }
    }

    /// <summary>Execute transaction with multiple commands.</summary>
    public static async Task<Result> ExecuteTransactionAsync(
        IDbConnection connection,
        Func<IDbTransaction, Task> executeTransaction,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        try
        {
            if (connection.State != ConnectionState.Open)
                await OpenConnectionAsync(connection);

            using var transaction = connection.BeginTransaction(isolationLevel);
            try
            {
                await executeTransaction(transaction);
                await CommitTransactionAsync(transaction);
                return Result.Ok();
            }
            catch
            {
                await RollbackTransactionAsync(transaction);
                throw;
            }
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("ADO.TRANSACTION_FAILED", ex.Message));
        }
    }

    // Helper methods for async operations on IDbConnection/IDbCommand
    // These work with DbConnection/DbCommand which support async operations

    private static async Task OpenConnectionAsync(IDbConnection connection)
    {
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync();
        }
        else
        {
            connection.Open();
            await Task.CompletedTask;
        }
    }

    private static async Task<object?> ExecuteScalarAsyncInternal(IDbCommand command)
    {
        if (command is DbCommand dbCommand)
        {
            return await dbCommand.ExecuteScalarAsync();
        }
        else
        {
            return command.ExecuteScalar();
        }
    }

    private static async Task<int> ExecuteNonQueryAsyncInternal(IDbCommand command)
    {
        if (command is DbCommand dbCommand)
        {
            return await dbCommand.ExecuteNonQueryAsync();
        }
        else
        {
            return command.ExecuteNonQuery();
        }
    }

    private static async Task<IDataReader> ExecuteReaderAsyncInternal(IDbCommand command)
    {
        if (command is DbCommand dbCommand)
        {
            return await dbCommand.ExecuteReaderAsync();
        }
        else
        {
            return command.ExecuteReader();
        }
    }

    private static async Task<bool> ReadAsyncInternal(IDataReader reader)
    {
        if (reader is DbDataReader dbReader)
        {
            return await dbReader.ReadAsync();
        }
        else
        {
            return reader.Read();
        }
    }

    private static async Task CommitTransactionAsync(IDbTransaction transaction)
    {
        if (transaction is DbTransaction dbTransaction)
        {
            await dbTransaction.CommitAsync();
        }
        else
        {
            transaction.Commit();
            await Task.CompletedTask;
        }
    }

    private static async Task RollbackTransactionAsync(IDbTransaction transaction)
    {
        if (transaction is DbTransaction dbTransaction)
        {
            await dbTransaction.RollbackAsync();
        }
        else
        {
            transaction.Rollback();
            await Task.CompletedTask;
        }
    }

    // Helper to add parameters
    private static void AddParameters(
        IDbCommand command,
        Dictionary<string, object?>? parameters,
        IDbProvider provider,
        bool isStoredProc = false,
        Dictionary<string, object?>? outputParams = null)
    {
        if (parameters == null || parameters.Count == 0)
            return;

        var prefix = provider.GetParameterPrefix();

        foreach (var kvp in parameters)
        {
            var param = command.CreateParameter();
            param.ParameterName = kvp.Key.StartsWith(prefix) ? kvp.Key : $"{prefix}{kvp.Key}";
            param.Value = kvp.Value ?? DBNull.Value;

            if (isStoredProc && outputParams?.ContainsKey(kvp.Key) == true)
            {
                param.Direction = ParameterDirection.Output;
            }

            command.Parameters.Add(param);
        }
    }
}
