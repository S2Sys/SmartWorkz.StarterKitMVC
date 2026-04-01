using System.Data;
using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Generic Dapper base repository. Handles all simple CRUD via reflection.
/// Subclass only to add domain-specific SP-backed methods.
///
/// Entity requirements:
///   - Class decorated with [Table("Schema", "TableName")]
///   - Primary key property decorated with [Key]
///   - IDENTITY columns decorated with [Identity]   → excluded from INSERT
///   - Navigation / computed properties with [NotMapped] → excluded from all SQL
/// </summary>
public abstract class DapperRepository<T> : IDapperRepository<T> where T : class, new()
{
    private readonly string _connectionString;

    // Reflected metadata — computed once per type
    private static readonly string   _schema;
    private static readonly string   _table;
    private static readonly string   _fullTable;
    private static readonly PropertyInfo   _keyProp;
    private static readonly PropertyInfo[] _allProps;       // all mapped props
    private static readonly PropertyInfo[] _insertProps;    // excludes Identity
    private static readonly PropertyInfo[] _updateProps;    // excludes Key + Identity
    private static readonly bool           _hasTenantId;
    private static readonly bool           _hasIsDeleted;

    static DapperRepository()
    {
        var type = typeof(T);

        var tableAttr = type.GetCustomAttribute<TableAttribute>()
            ?? throw new InvalidOperationException(
                $"{type.Name} must have [Table(schema, name)] attribute.");

        _schema    = tableAttr.Schema;
        _table     = tableAttr.Name;
        _fullTable = $"[{_schema}].[{_table}]";

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead
                     && p.GetCustomAttribute<NotMappedAttribute>() == null
                     && !IsNavigationProperty(p))
            .ToArray();

        _allProps    = props;
        _keyProp     = props.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null)
            ?? throw new InvalidOperationException($"{type.Name} must have one [Key] property.");
        _insertProps = props.Where(p => p.GetCustomAttribute<IdentityAttribute>() == null).ToArray();
        _updateProps = props.Where(p => p.GetCustomAttribute<KeyAttribute>()      == null
                                     && p.GetCustomAttribute<IdentityAttribute>() == null).ToArray();

        _hasTenantId  = props.Any(p => p.Name == "TenantId");
        _hasIsDeleted = props.Any(p => p.Name == "IsDeleted");
    }

    protected DapperRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured.");
    }

    protected IDbConnection GetConnection() => new SqlConnection(_connectionString);

    // ─── Public interface ────────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync(object id)
    {
        var sql = $"SELECT {SelectColumns()} FROM {_fullTable} WHERE [{_keyProp.Name}] = @Id"
                + (_hasIsDeleted ? " AND IsDeleted = 0" : "");
        using var conn = GetConnection();
        return await conn.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<IEnumerable<T>> GetAllAsync(string tenantId)
    {
        var where = new StringBuilder("WHERE 1=1");
        if (_hasTenantId)  where.Append(" AND TenantId = @TenantId");
        if (_hasIsDeleted) where.Append(" AND IsDeleted = 0");

        var sql = $"SELECT {SelectColumns()} FROM {_fullTable} {where}";
        using var conn = GetConnection();
        return await conn.QueryAsync<T>(sql, _hasTenantId ? new { TenantId = tenantId } : null);
    }

    public async Task<IEnumerable<T>> FindAsync(object filters)
    {
        var (where, param) = BuildWhere(filters);
        var sql = $"SELECT {SelectColumns()} FROM {_fullTable} {where}";
        using var conn = GetConnection();
        return await conn.QueryAsync<T>(sql, param);
    }

    public async Task<T?> FirstOrDefaultAsync(object filters)
    {
        var (where, param) = BuildWhere(filters);
        var sql = $"SELECT TOP 1 {SelectColumns()} FROM {_fullTable} {where}";
        using var conn = GetConnection();
        return await conn.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    public async Task UpsertAsync(T entity)
    {
        var keyVal = _keyProp.GetValue(entity);
        var isNew  = IsDefaultValue(keyVal);

        SetAuditFields(entity, isNew);

        using var conn = GetConnection();
        if (isNew)
            await conn.ExecuteAsync(BuildInsert(), entity);
        else
            await conn.ExecuteAsync(BuildUpdate(), entity);
    }

    /// <summary>
    /// Automatically sets CreatedAt (insert only) and UpdatedAt (always)
    /// if those properties exist on the entity.
    /// </summary>
    private static void SetAuditFields(T entity, bool isNew)
    {
        var now = DateTime.UtcNow;

        if (isNew)
        {
            var createdAt = typeof(T).GetProperty("CreatedAt");
            if (createdAt?.CanWrite == true && createdAt.GetValue(entity) is DateTime d && d == default)
                createdAt.SetValue(entity, now);
        }

        var updatedAt = typeof(T).GetProperty("UpdatedAt");
        if (updatedAt?.CanWrite == true)
            updatedAt.SetValue(entity, now);
    }

    public async Task SoftDeleteAsync(object id)
    {
        if (!_hasIsDeleted)
            throw new InvalidOperationException($"{typeof(T).Name} has no IsDeleted column.");

        var sql = $"UPDATE {_fullTable} SET IsDeleted = 1, UpdatedAt = GETUTCDATE() "
                + $"WHERE [{_keyProp.Name}] = @Id";
        using var conn = GetConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task DeleteAsync(object id)
    {
        var sql = $"DELETE FROM {_fullTable} WHERE [{_keyProp.Name}] = @Id";
        using var conn = GetConnection();
        await conn.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<bool> ExistsAsync(object filters)
    {
        var (where, param) = BuildWhere(filters);
        var sql = $"SELECT COUNT(1) FROM {_fullTable} {where}";
        using var conn = GetConnection();
        return await conn.ExecuteScalarAsync<int>(sql, param) > 0;
    }

    public async Task<int> CountAsync(object filters)
    {
        var (where, param) = BuildWhere(filters);
        var sql = $"SELECT COUNT(1) FROM {_fullTable} {where}";
        using var conn = GetConnection();
        return await conn.ExecuteScalarAsync<int>(sql, param);
    }

    public async Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(
        object filters, string orderBy, bool descending = false,
        int pageNumber = 1, int pageSize = 20)
    {
        var (where, param) = BuildWhere(filters);
        var dir    = descending ? "DESC" : "ASC";
        // Sanitise orderBy — only allow column names that exist on the entity
        var safeCol = _allProps.FirstOrDefault(p =>
            string.Equals(p.Name, orderBy, StringComparison.OrdinalIgnoreCase))?.Name
            ?? _keyProp.Name;

        var offset = (pageNumber - 1) * pageSize;

        var sqlData  = $"SELECT {SelectColumns()} FROM {_fullTable} {where} "
                     + $"ORDER BY [{safeCol}] {dir} "
                     + $"OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
        var sqlCount = $"SELECT COUNT(1) FROM {_fullTable} {where}";

        using var conn = GetConnection();
        var items = await conn.QueryAsync<T>(sqlData, param);
        var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        return (items, total);
    }

    // ─── SQL builders ────────────────────────────────────────────────────────

    private static string SelectColumns()
        => string.Join(", ", _allProps.Select(p => $"[{p.Name}]"));

    private static string BuildInsert()
    {
        var cols = string.Join(", ", _insertProps.Select(p => $"[{p.Name}]"));
        var vals = string.Join(", ", _insertProps.Select(p => $"@{p.Name}"));
        return $"INSERT INTO {_fullTable} ({cols}) VALUES ({vals})";
    }

    private static string BuildUpdate()
    {
        var sets = string.Join(", ", _updateProps.Select(p => $"[{p.Name}] = @{p.Name}"));
        return $"UPDATE {_fullTable} SET {sets} WHERE [{_keyProp.Name}] = @{_keyProp.Name}";
    }

    private static (string Sql, DynamicParameters Params) BuildWhere(object filters)
    {
        var dp   = new DynamicParameters();
        var sb   = new StringBuilder("WHERE 1=1");
        var type = filters.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var val = prop.GetValue(filters);
            if (val == null) continue;
            sb.Append($" AND [{prop.Name}] = @{prop.Name}");
            dp.Add(prop.Name, val);
        }

        // Auto-append soft-delete filter if not already provided
        if (_hasIsDeleted && type.GetProperty("IsDeleted") == null)
        {
            sb.Append(" AND IsDeleted = 0");
        }

        return (sb.ToString(), dp);
    }

    private static bool IsDefaultValue(object? val)
    {
        if (val == null) return true;
        return val switch
        {
            int    i => i == 0,
            long   l => l == 0,
            string s => string.IsNullOrEmpty(s),
            Guid   g => g == Guid.Empty,
            _        => false
        };
    }

    /// <summary>
    /// Excludes navigation / collection properties from column lists.
    /// Anything that is a class (except string/DateTime) or IEnumerable is excluded.
    /// </summary>
    private static bool IsNavigationProperty(PropertyInfo p)
    {
        var t = p.PropertyType;
        if (t == typeof(string)) return false;
        if (t.IsValueType)       return false;
        if (t == typeof(byte[])) return false;
        // IEnumerable<> or class references → navigation
        return true;
    }
}
