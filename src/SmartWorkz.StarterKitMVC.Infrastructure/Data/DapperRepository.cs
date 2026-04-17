using System.Data;
using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Data;

/// <summary>
/// Abstract base class for Dapper-based repositories.
/// Provides CRUD operations using stored procedures and raw SQL with soft delete support.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public abstract class DapperRepository<T> : IDapperRepository<T> where T : class
{
    protected readonly IDbConnection Connection;
    protected readonly ILogger Logger;
    protected string TableName { get; set; }
    protected string Schema { get; set; } = "dbo";
    protected string IdColumn { get; set; } = "Id";

    protected DapperRepository(IDbConnection connection, ILogger logger)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get entity by ID (respects soft deletes)
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(object id)
    {
        try
        {
            var sql = $"SELECT * FROM [{Schema}].[{TableName}] WHERE [{IdColumn}] = @Id AND IsDeleted = 0";
            return await Connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error retrieving entity by ID from {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Get all active entities for a tenant (respects soft deletes and multi-tenancy)
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(string tenantId)
    {
        try
        {
            var sql = $"SELECT * FROM [{Schema}].[{TableName}] WHERE TenantId = @TenantId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
            return await Connection.QueryAsync<T>(sql, new { TenantId = tenantId });
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error retrieving all entities from {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Find entities matching filter conditions
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(object filters)
    {
        try
        {
            var (sql, param) = BuildWhereClause(filters);
            var query = $"SELECT * FROM [{Schema}].[{TableName}] {sql} AND IsDeleted = 0";
            return await Connection.QueryAsync<T>(query, param);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error finding entities in {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Get first entity matching filter conditions
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultAsync(object filters)
    {
        try
        {
            var (sql, param) = BuildWhereClause(filters);
            var query = $"SELECT TOP 1 * FROM [{Schema}].[{TableName}] {sql} AND IsDeleted = 0";
            return await Connection.QueryFirstOrDefaultAsync<T>(query, param);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error retrieving first entity from {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Upsert (insert or update) an entity using the stored procedure sp_[TableName]_Upsert
    /// </summary>
    public virtual async Task UpsertAsync(T entity)
    {
        try
        {
            var spName = $"[{Schema}].[sp_{TableName}_Upsert]";
            var parameters = ExtractParameters(entity);

            await Connection.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error upserting entity in {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Soft delete an entity (sets IsDeleted = 1)
    /// </summary>
    public virtual async Task SoftDeleteAsync(object id)
    {
        try
        {
            var sql = $"""
                UPDATE [{Schema}].[{TableName}]
                SET IsDeleted = 1, UpdatedAt = @UpdatedAt
                WHERE [{IdColumn}] = @Id
                """;
            await Connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error soft deleting entity from {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Hard delete an entity (physically removes the row)
    /// </summary>
    public virtual async Task DeleteAsync(object id)
    {
        try
        {
            var sql = $"DELETE FROM [{Schema}].[{TableName}] WHERE [{IdColumn}] = @Id";
            await Connection.ExecuteAsync(sql, new { Id = id });
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error deleting entity from {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Check if entity exists matching filter conditions
    /// </summary>
    public virtual async Task<bool> ExistsAsync(object filters)
    {
        try
        {
            var (sql, param) = BuildWhereClause(filters);
            var query = $"SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM [{Schema}].[{TableName}] {sql} AND IsDeleted = 0) THEN 1 ELSE 0 END AS BIT)";
            return await Connection.QueryFirstAsync<bool>(query, param);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error checking entity existence in {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Count entities matching filter conditions
    /// </summary>
    public virtual async Task<int> CountAsync(object filters)
    {
        try
        {
            var (sql, param) = BuildWhereClause(filters);
            var query = $"SELECT COUNT(*) FROM [{Schema}].[{TableName}] {sql} AND IsDeleted = 0";
            return await Connection.QueryFirstAsync<int>(query, param);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error counting entities in {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Get paged entities matching filter conditions
    /// </summary>
    public virtual async Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(
        object filters, string orderBy, bool descending = false,
        int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var (whereClause, param) = BuildWhereClause(filters);
            var direction = descending ? "DESC" : "ASC";
            var offset = (pageNumber - 1) * pageSize;

            var countQuery = $"SELECT COUNT(*) FROM [{Schema}].[{TableName}] {whereClause} AND IsDeleted = 0";
            var dataQuery = $"""
                SELECT * FROM [{Schema}].[{TableName}]
                {whereClause} AND IsDeleted = 0
                ORDER BY [{orderBy}] {direction}
                OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY
                """;

            var total = await Connection.QueryFirstAsync<int>(countQuery, param);
            var items = await Connection.QueryAsync<T>(dataQuery, param);

            return (items, total);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error retrieving paged entities from {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Build WHERE clause and parameters from filter object
    /// </summary>
    protected virtual (string WhereClause, DynamicParameters Parameters) BuildWhereClause(object filters)
    {
        var parameters = new DynamicParameters();
        var conditions = new List<string>();

        if (filters != null)
        {
            var properties = filters.GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var value = prop.GetValue(filters);
                if (value != null)
                {
                    conditions.Add($"[{prop.Name}] = @{prop.Name}");
                    parameters.Add($"@{prop.Name}", value);
                }
            }
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "WHERE 1=1";
        return (whereClause, parameters);
    }

    /// <summary>
    /// Extract parameters from entity for stored procedure call
    /// </summary>
    protected virtual DynamicParameters ExtractParameters(T entity)
    {
        var parameters = new DynamicParameters();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            // Skip navigation properties and ICollections
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                continue;

            var value = prop.GetValue(entity);
            parameters.Add($"@{prop.Name}", value);
        }

        return parameters;
    }

    /// <summary>
    /// Execute a raw SQL query
    /// </summary>
    protected virtual async Task<IEnumerable<T>> ExecuteQueryAsync(string sql, object param = null)
    {
        try
        {
            return await Connection.QueryAsync<T>(sql, param);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error executing query on {TableName}", TableName);
            throw;
        }
    }

    /// <summary>
    /// Execute a stored procedure and return results
    /// </summary>
    protected virtual async Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string spName, object param = null)
    {
        try
        {
            return await Connection.QueryAsync<T>(spName, param, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error executing stored procedure {SpName}", spName);
            throw;
        }
    }

    /// <summary>
    /// Execute a stored procedure without return value
    /// </summary>
    protected virtual async Task ExecuteStoredProcedureNonQueryAsync(string spName, object param = null)
    {
        try
        {
            await Connection.ExecuteAsync(spName, param, commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "Error executing non-query stored procedure {SpName}", spName);
            throw;
        }
    }
}
