namespace SmartWorkz.Core.Shared.Data;

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Defines column mapping for CSV operations using a fluent API.
/// Supports mapping object properties to CSV columns with custom headers.
/// Sealed to prevent inheritance and ensure consistent behavior.
/// </summary>
/// <typeparam name="T">The type of objects being mapped.</typeparam>
public sealed class CsvMapping<T> where T : class
{
    private readonly Dictionary<PropertyInfo, string> _mappings = new();
    private readonly Dictionary<string, PropertyInfo> _headerToProperty = new();

    /// <summary>
    /// Gets a dictionary mapping property info to CSV header names.
    /// </summary>
    public IReadOnlyDictionary<PropertyInfo, string> Mappings => _mappings.AsReadOnly();

    /// <summary>
    /// Gets a dictionary mapping CSV header names to property info.
    /// </summary>
    public IReadOnlyDictionary<string, PropertyInfo> HeaderToProperty => _headerToProperty.AsReadOnly();

    /// <summary>
    /// Adds a column mapping for the specified property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyExpression">Expression selecting the property to map.</param>
    /// <param name="csvHeader">The CSV column header name.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when propertyExpression or csvHeader is null.</exception>
    public CsvMapping<T> Column<TProperty>(
        Expression<Func<T, TProperty>> propertyExpression,
        string csvHeader)
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        if (string.IsNullOrWhiteSpace(csvHeader))
            throw new ArgumentException("CSV header cannot be null or empty.", nameof(csvHeader));

        var property = ExtractPropertyInfo(propertyExpression);
        if (property == null)
            throw new ArgumentException("Expression does not resolve to a property.", nameof(propertyExpression));

        _mappings[property] = csvHeader;
        _headerToProperty[csvHeader] = property;

        return this;
    }

    /// <summary>
    /// Extracts property information from a lambda expression.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="expression">The lambda expression.</param>
    /// <returns>The PropertyInfo if the expression resolves to a property; otherwise null.</returns>
    private static PropertyInfo? ExtractPropertyInfo<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var body = expression.Body;

        // Handle simple member access: obj => obj.Property
        if (body is MemberExpression memberExpr && memberExpr.Member is PropertyInfo propInfo)
            return propInfo;

        // Handle property conversion to nullable: obj => (object?)obj.Property
        if (body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression innerMember)
            return innerMember.Member as PropertyInfo;

        return null;
    }

    /// <summary>
    /// Creates a mapping automatically from all public properties of type T.
    /// Property names are used as CSV headers.
    /// </summary>
    /// <returns>A new CsvMapping instance with all properties mapped.</returns>
    public static CsvMapping<T> CreateAuto()
    {
        var mapping = new CsvMapping<T>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            mapping._mappings[property] = property.Name;
            mapping._headerToProperty[property.Name] = property;
        }

        return mapping;
    }
}
