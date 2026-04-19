using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Abstract base class for cached Dapper repositories.
/// Provides:
/// - Error-wrapped SP execution helpers with logging
/// - Optional IMemoryCache integration for read operations
/// - Multi-result-set support (QueryMultiple helpers)
/// - Transient error handling and retry context
/// </summary>
public abstract class CachedDapperRepository : ICachedDapperRepository
{
    protected readonly IDbConnection Connection;
    private readonly IMemoryCache? _cache;
    private readonly ILogger _logger;

    protected CachedDapperRepository(
        IDbConnection connection,
        IMemoryCache? cache,
        ILogger logger)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _cache = cache;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─── SP Execution Helpers (No Caching) ──────────────────────────────────────

    /// <summary>
    /// Execute a stored procedure and return typed results.
    /// </summary>
    protected async Task<IEnumerable<T>> QuerySpAsync<T>(
        string spName, object? param = null, int timeoutSeconds = 30)
    {
        try
        {
            return await Connection.QueryAsync<T>(
                spName,
                param,
                commandType: CommandType.StoredProcedure,
                commandTimeout: timeoutSeconds);
        }
        catch (DbException ex)
        {
            var errorNumber = ExtractErrorNumber(ex);
            LogDbError(spName, ex, errorNumber);
            throw new RepositoryException(spName, $"Failed to execute {spName}: {ex.Message}", ex, errorNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing {SpName}", spName);
            throw;
        }
    }

    /// <summary>
    /// Execute a stored procedure and return a single typed result (or null).
    /// </summary>
    protected async Task<T?> QuerySingleSpAsync<T>(
        string spName, object? param = null, int timeoutSeconds = 30)
    {
        try
        {
            return await Connection.QueryFirstOrDefaultAsync<T>(
                spName,
                param,
                commandType: CommandType.StoredProcedure,
                commandTimeout: timeoutSeconds);
        }
        catch (DbException ex)
        {
            var errorNumber = ExtractErrorNumber(ex);
            LogDbError(spName, ex, errorNumber);
            throw new RepositoryException(spName, $"Failed to execute {spName}: {ex.Message}", ex, errorNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing {SpName}", spName);
            throw;
        }
    }

    /// <summary>
    /// Execute a stored procedure with no return value.
    /// </summary>
    protected async Task ExecuteSpAsync(
        string spName, object? param = null, int timeoutSeconds = 30)
    {
        try
        {
            await Connection.ExecuteAsync(
                spName,
                param,
                commandType: CommandType.StoredProcedure,
                commandTimeout: timeoutSeconds);
        }
        catch (DbException ex)
        {
            var errorNumber = ExtractErrorNumber(ex);
            LogDbError(spName, ex, errorNumber);
            throw new RepositoryException(spName, $"Failed to execute {spName}: {ex.Message}", ex, errorNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing {SpName}", spName);
            throw;
        }
    }

    // ─── Cached SP Execution ────────────────────────────────────────────────────

    /// <summary>
    /// Execute a stored procedure and cache the results. Returns cached value on subsequent calls.
    /// </summary>
    protected async Task<IEnumerable<T>> QuerySpCachedAsync<T>(
        string cacheKey, TimeSpan ttl,
        string spName, object? param = null, int timeoutSeconds = 30)
    {
        if (_cache == null)
            return await QuerySpAsync<T>(spName, param, timeoutSeconds);

        if (_cache.TryGetValue(cacheKey, out IEnumerable<T>? cached))
            return cached!;

        var result = await QuerySpAsync<T>(spName, param, timeoutSeconds);
        var list = result.ToList(); // materialize before caching

        _cache.Set(cacheKey, list, ttl);
        return list;
    }

    /// <summary>
    /// Execute a stored procedure and cache the single result.
    /// </summary>
    protected async Task<T?> QuerySingleSpCachedAsync<T>(
        string cacheKey, TimeSpan ttl,
        string spName, object? param = null, int timeoutSeconds = 30)
    {
        if (_cache == null)
            return await QuerySingleSpAsync<T>(spName, param, timeoutSeconds);

        if (_cache.TryGetValue(cacheKey, out T? cached))
            return cached;

        var result = await QuerySingleSpAsync<T>(spName, param, timeoutSeconds);

        if (result != null)
            _cache.Set(cacheKey, result, ttl);

        return result;
    }

    /// <summary>
    /// Invalidate a cached entry.
    /// </summary>
    protected void InvalidateCacheKey(string cacheKey)
    {
        _cache?.Remove(cacheKey);
    }

    // ─── Multi Result Set Support ───────────────────────────────────────────────

    /// <summary>
    /// Open a multi-result-set reader for the given SP.
    /// Caller is responsible for reading grids in order.
    /// </summary>
    protected async Task<SqlMapper.GridReader> QueryMultipleSpAsync(
        string spName, object? param = null, int timeoutSeconds = 30)
    {
        try
        {
            return await Connection.QueryMultipleAsync(
                spName,
                param,
                commandType: CommandType.StoredProcedure,
                commandTimeout: timeoutSeconds);
        }
        catch (DbException ex)
        {
            var errorNumber = ExtractErrorNumber(ex);
            LogDbError(spName, ex, errorNumber);
            throw new RepositoryException(spName, $"Failed to execute {spName}: {ex.Message}", ex, errorNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing {SpName}", spName);
            throw;
        }
    }

    /// <summary>
    /// Execute a SP returning exactly 2 result sets.
    /// </summary>
    protected async Task<(IEnumerable<TFirst> First, IEnumerable<TSecond> Second)> QueryMultipleSpAsync<TFirst, TSecond>(
        string spName, object? param = null, int timeoutSeconds = 30)
    {
        try
        {
            using var reader = await QueryMultipleSpAsync(spName, param, timeoutSeconds);
            var first = await reader.ReadAsync<TFirst>();
            var second = await reader.ReadAsync<TSecond>();
            return (first, second);
        }
        catch (RepositoryException)
        {
            throw; // already wrapped
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading multiple result sets from {SpName}", spName);
            throw;
        }
    }

    /// <summary>
    /// Execute a SP returning a paged result (items + total count as 2 result sets).
    /// </summary>
    protected async Task<(IEnumerable<T> Items, int Total)> QueryPagedSpAsync<T>(
        string spName, object? param = null, int timeoutSeconds = 30)
    {
        try
        {
            using var reader = await QueryMultipleSpAsync(spName, param, timeoutSeconds);
            var items = await reader.ReadAsync<T>();
            var total = await reader.ReadSingleAsync<int>();
            return (items, total);
        }
        catch (RepositoryException)
        {
            throw; // already wrapped
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading paged result from {SpName}", spName);
            throw;
        }
    }

    // ─── Provider-Agnostic Error Helpers ────────────────────────────────────────

    /// <summary>
    /// Extract a numeric error code from any <see cref="DbException"/>.
    /// SQL Server (<see cref="SqlException"/>) populates <c>Number</c>; other providers
    /// may return <c>null</c> (Postgres uses a string <c>SqlState</c>, MySQL / Oracle have
    /// their own <c>Number</c> conventions).
    ///
    /// When the project adds Postgres / Oracle / MySQL support, extend the switch below
    /// with the provider-specific pattern match — no change needed in the catch blocks.
    /// </summary>
    private static int? ExtractErrorNumber(DbException ex) => ex switch
    {
        SqlException se => se.Number,
        _               => null
    };

    /// <summary>
    /// Log a <see cref="DbException"/> with provider-agnostic transient detection.
    /// </summary>
    private void LogDbError(string spName, DbException ex, int? errorNumber)
    {
        // Transient SQL Server error numbers: deadlock (1205), timeout (-2),
        // login timeout (40197), connection dropped (64).
        // Other providers fall through to the non-transient branch — extend per provider.
        var transientNumbers = new[] { 1205, -2, 40197, 64 };
        var isTransient = errorNumber.HasValue && transientNumbers.Contains(errorNumber.Value);

        if (isTransient)
            _logger.LogWarning(ex, "Transient DB error executing {SpName} (Error {ErrorNumber})", spName, errorNumber);
        else
            _logger.LogError(ex, "DB error executing {SpName} (Error {ErrorNumber})", spName, errorNumber);
    }
}

/// <summary>
/// Exception thrown when a repository operation fails.
/// Wraps any <see cref="System.Data.Common.DbException"/> (SQL Server, PostgreSQL,
/// Oracle, MySQL, …) with context about which stored procedure failed.
/// <para>
/// <see cref="SqlErrorNumber"/> is populated for SQL Server; it is <c>null</c> for
/// other providers — pattern-match on <c>Exception.InnerException</c> to reach
/// provider-specific error data (<c>PostgresException.SqlState</c>, etc.).
/// </para>
/// </summary>
public class RepositoryException : Exception
{
    public string StoredProcedure { get; }

    /// <summary>
    /// Numeric error number from the underlying provider when available.
    /// Populated for SQL Server (<see cref="SqlException.Number"/>); null for
    /// providers that don't expose a numeric code.
    /// </summary>
    public int? SqlErrorNumber { get; }

    public RepositoryException(string sp, string message, Exception inner, int? sqlError = null)
        : base(message, inner)
    {
        StoredProcedure = sp;
        SqlErrorNumber = sqlError;
    }
}
