namespace SmartWorkz.Shared;

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

    /// <summary>Execute query returning multiple result sets (2 sets).</summary>
    public static async Task<Result<(List<T1>, List<T2>)>> ExecuteQueryMultipleAsync<T1, T2>(
        IDbConnection connection,
        string commandText,
        IDbProvider provider,
        Func<IDataReader, T1> mapper1,
        Func<IDataReader, T2> mapper2,
        Dictionary<string, object?>? parameters = null,
        CommandType commandType = CommandType.Text,
        int commandTimeout = 30)
        where T1 : class
        where T2 : class
    {
        try
        {
            if (mapper1 == null)
                return Result.Fail<(List<T1>, List<T2>)>(
                    new Error("ADO.MAPPER_NULL", "mapper1 cannot be null"));
            if (mapper2 == null)
                return Result.Fail<(List<T1>, List<T2>)>(
                    new Error("ADO.MAPPER_NULL", "mapper2 cannot be null"));

            var results1 = new List<T1>();
            var results2 = new List<T2>();

            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = commandTimeout;

            AddParameters(command, parameters, provider);

            if (connection.State != ConnectionState.Open)
                await OpenConnectionAsync(connection);

            using var reader = await ExecuteReaderAsyncInternal(command, CommandBehavior.SequentialAccess);

            // Read first result set
            while (await ReadAsyncInternal(reader))
            {
                results1.Add(mapper1(reader));
            }

            // Move to next result set
            if (!await NextResultAsyncInternal(reader))
                return Result.Ok((results1, results2));

            // Read second result set
            while (await ReadAsyncInternal(reader))
            {
                results2.Add(mapper2(reader));
            }

            return Result.Ok((results1, results2));
        }
        catch (Exception ex)
        {
            return Result.Fail<(List<T1>, List<T2>)>(
                new Error("ADO.QUERY_MULTIPLE_FAILED", ex.Message));
        }
    }

    /// <summary>Execute query returning multiple result sets (3 sets).</summary>
    public static async Task<Result<(List<T1>, List<T2>, List<T3>)>> ExecuteQueryMultipleAsync<T1, T2, T3>(
        IDbConnection connection,
        string commandText,
        IDbProvider provider,
        Func<IDataReader, T1> mapper1,
        Func<IDataReader, T2> mapper2,
        Func<IDataReader, T3> mapper3,
        Dictionary<string, object?>? parameters = null,
        CommandType commandType = CommandType.Text,
        int commandTimeout = 30)
        where T1 : class
        where T2 : class
        where T3 : class
    {
        try
        {
            if (mapper1 == null)
                return Result.Fail<(List<T1>, List<T2>, List<T3>)>(
                    new Error("ADO.MAPPER_NULL", "mapper1 cannot be null"));
            if (mapper2 == null)
                return Result.Fail<(List<T1>, List<T2>, List<T3>)>(
                    new Error("ADO.MAPPER_NULL", "mapper2 cannot be null"));
            if (mapper3 == null)
                return Result.Fail<(List<T1>, List<T2>, List<T3>)>(
                    new Error("ADO.MAPPER_NULL", "mapper3 cannot be null"));

            var results1 = new List<T1>();
            var results2 = new List<T2>();
            var results3 = new List<T3>();

            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = commandTimeout;

            AddParameters(command, parameters, provider);

            if (connection.State != ConnectionState.Open)
                await OpenConnectionAsync(connection);

            using var reader = await ExecuteReaderAsyncInternal(command, CommandBehavior.SequentialAccess);

            // Read first result set
            while (await ReadAsyncInternal(reader))
            {
                results1.Add(mapper1(reader));
            }

            // Move to second result set
            if (!await NextResultAsyncInternal(reader))
                return Result.Ok((results1, results2, results3));

            // Read second result set
            while (await ReadAsyncInternal(reader))
            {
                results2.Add(mapper2(reader));
            }

            // Move to third result set
            if (!await NextResultAsyncInternal(reader))
                return Result.Ok((results1, results2, results3));

            // Read third result set
            while (await ReadAsyncInternal(reader))
            {
                results3.Add(mapper3(reader));
            }

            return Result.Ok((results1, results2, results3));
        }
        catch (Exception ex)
        {
            return Result.Fail<(List<T1>, List<T2>, List<T3>)>(
                new Error("ADO.QUERY_MULTIPLE_FAILED", ex.Message));
        }
    }

    /// <summary>Execute query returning multiple result sets (4 sets).</summary>
    public static async Task<Result<(List<T1>, List<T2>, List<T3>, List<T4>)>> ExecuteQueryMultipleAsync<T1, T2, T3, T4>(
        IDbConnection connection,
        string commandText,
        IDbProvider provider,
        Func<IDataReader, T1> mapper1,
        Func<IDataReader, T2> mapper2,
        Func<IDataReader, T3> mapper3,
        Func<IDataReader, T4> mapper4,
        Dictionary<string, object?>? parameters = null,
        CommandType commandType = CommandType.Text,
        int commandTimeout = 30)
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        try
        {
            if (mapper1 == null)
                return Result.Fail<(List<T1>, List<T2>, List<T3>, List<T4>)>(
                    new Error("ADO.MAPPER_NULL", "mapper1 cannot be null"));
            if (mapper2 == null)
                return Result.Fail<(List<T1>, List<T2>, List<T3>, List<T4>)>(
                    new Error("ADO.MAPPER_NULL", "mapper2 cannot be null"));
            if (mapper3 == null)
                return Result.Fail<(List<T1>, List<T2>, List<T3>, List<T4>)>(
                    new Error("ADO.MAPPER_NULL", "mapper3 cannot be null"));
            if (mapper4 == null)
                return Result.Fail<(List<T1>, List<T2>, List<T3>, List<T4>)>(
                    new Error("ADO.MAPPER_NULL", "mapper4 cannot be null"));

            var results1 = new List<T1>();
            var results2 = new List<T2>();
            var results3 = new List<T3>();
            var results4 = new List<T4>();

            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = commandTimeout;

            AddParameters(command, parameters, provider);

            if (connection.State != ConnectionState.Open)
                await OpenConnectionAsync(connection);

            using var reader = await ExecuteReaderAsyncInternal(command, CommandBehavior.SequentialAccess);

            // Read first result set
            while (await ReadAsyncInternal(reader))
            {
                results1.Add(mapper1(reader));
            }

            // Move to second result set
            if (!await NextResultAsyncInternal(reader))
                return Result.Ok((results1, results2, results3, results4));

            // Read second result set
            while (await ReadAsyncInternal(reader))
            {
                results2.Add(mapper2(reader));
            }

            // Move to third result set
            if (!await NextResultAsyncInternal(reader))
                return Result.Ok((results1, results2, results3, results4));

            // Read third result set
            while (await ReadAsyncInternal(reader))
            {
                results3.Add(mapper3(reader));
            }

            // Move to fourth result set
            if (!await NextResultAsyncInternal(reader))
                return Result.Ok((results1, results2, results3, results4));

            // Read fourth result set
            while (await ReadAsyncInternal(reader))
            {
                results4.Add(mapper4(reader));
            }

            return Result.Ok((results1, results2, results3, results4));
        }
        catch (Exception ex)
        {
            return Result.Fail<(List<T1>, List<T2>, List<T3>, List<T4>)>(
                new Error("ADO.QUERY_MULTIPLE_FAILED", ex.Message));
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

    private static async Task<IDataReader> ExecuteReaderAsyncInternal(IDbCommand command, CommandBehavior behavior)
    {
        if (command is DbCommand dbCommand)
        {
            return await dbCommand.ExecuteReaderAsync(behavior);
        }
        else
        {
            return command.ExecuteReader(behavior);
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

    private static async Task<bool> NextResultAsyncInternal(IDataReader reader)
    {
        if (reader is DbDataReader dbReader)
        {
            return await dbReader.NextResultAsync();
        }
        else
        {
            return reader.NextResult();
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
