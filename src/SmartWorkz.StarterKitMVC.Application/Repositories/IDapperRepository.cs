namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Attribute to map a class to its database table.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TableAttribute : Attribute
{
    public string Schema { get; }
    public string Name   { get; }
    public TableAttribute(string schema, string name) { Schema = schema; Name = name; }
}

/// <summary>
/// Marks a property as the primary key column.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class KeyAttribute : Attribute { }

/// <summary>
/// Marks a property as a database-generated IDENTITY column — excluded from INSERT.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IdentityAttribute : Attribute { }

/// <summary>
/// Marks a property as not mapped to any database column.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotMappedAttribute : Attribute { }

/// <summary>
/// Generic Dapper repository for simple single-table CRUD.
/// Use this for all tables. Only add domain-specific methods to subclasses
/// when the query is too complex for the generic interface (joins, CTEs, etc.).
/// </summary>
public interface IDapperRepository<T> where T : class
{
    /// <summary>Get a single row by primary key value.</summary>
    Task<T?> GetByIdAsync(object id);

    /// <summary>Get all non-deleted rows for a tenant.</summary>
    Task<IEnumerable<T>> GetAllAsync(string tenantId);

    /// <summary>
    /// Find rows matching all properties of the filter object.
    /// e.g. FindAsync(new { TenantId = "DEFAULT", IsActive = true })
    /// </summary>
    Task<IEnumerable<T>> FindAsync(object filters);

    /// <summary>Return first match or null.</summary>
    Task<T?> FirstOrDefaultAsync(object filters);

    /// <summary>
    /// INSERT if key is default/null, UPDATE if key has a value.
    /// Never touches IDENTITY or NotMapped columns.
    /// </summary>
    Task UpsertAsync(T entity);

    /// <summary>Soft delete — sets IsDeleted = 1, UpdatedAt = now.</summary>
    Task SoftDeleteAsync(object id);

    /// <summary>Hard delete — physically removes the row.</summary>
    Task DeleteAsync(object id);

    /// <summary>Returns true if any row matches the filter.</summary>
    Task<bool> ExistsAsync(object filters);

    /// <summary>Count rows matching the filter.</summary>
    Task<int> CountAsync(object filters);

    /// <summary>Paged query. pageNumber is 1-based.</summary>
    Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(
        object filters, string orderBy, bool descending = false,
        int pageNumber = 1, int pageSize = 20);
}
