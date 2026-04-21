namespace SmartWorkz.Shared;

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SmartWorkz.Core.Shared.Results;

/// <summary>
/// Provides static methods for reading and writing CSV data with support for column mapping,
/// quoted fields, embedded delimiters, and newlines.
/// RFC 4180 compliant CSV parsing and writing.
/// Sealed class to prevent inheritance and ensure consistent behavior.
/// </summary>
public sealed class CsvHelper
{
    private CsvHelper()
    {
        // Prevent instantiation
    }

    /// <summary>
    /// Serializes a collection of objects to CSV format.
    /// </summary>
    /// <typeparam name="T">The type of objects to serialize.</typeparam>
    /// <param name="items">The collection of objects to serialize.</param>
    /// <param name="options">CSV options. If null, defaults are used.</param>
    /// <returns>A Result containing the CSV string if successful; otherwise a failure.</returns>
    public static Result<string> CsvWriter<T>(IEnumerable<T> items, CsvOptions? options = null) where T : class
    {
        try
        {
            if (items == null)
                return Result.Fail<string>("CSV.WRITE.NULL_ITEMS", "Items collection cannot be null.");

            options ??= new CsvOptions();

            var itemList = items.ToList();
            if (itemList.Count == 0)
            {
                // Return empty string for empty list
                return Result.Ok<string>(string.Empty);
            }

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (properties.Length == 0)
                return Result.Fail<string>("CSV.WRITE.NO_PROPERTIES", "Type has no public properties.");

            var sb = new StringBuilder();

            // Write header
            var headers = properties.Select(p => p.Name).ToList();
            WriteRecord(sb, headers, options);

            // Write data rows
            foreach (var item in itemList)
            {
                var values = new List<string>();
                foreach (var property in properties)
                {
                    var value = property.GetValue(item);
                    values.Add(value?.ToString() ?? string.Empty);
                }
                WriteRecord(sb, values, options);
            }

            return Result.Ok(sb.ToString());
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(
                Error.FromException(ex, "CSV.WRITE.FAILED"));
        }
    }

    /// <summary>
    /// Asynchronously deserializes CSV content to a collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to deserialize into.</typeparam>
    /// <param name="content">The CSV content string.</param>
    /// <param name="mapping">Column mapping configuration. If null, property names are used as headers.</param>
    /// <param name="options">CSV options. If null, defaults are used.</param>
    /// <returns>A Result containing the deserialized list if successful; otherwise a failure.</returns>
    public static async Task<Result<List<T>>> CsvReader<T>(
        string content,
        CsvMapping<T>? mapping = null,
        CsvOptions? options = null) where T : class
    {
        try
        {
            if (string.IsNullOrEmpty(content))
                return Result.Ok(new List<T>());

            options ??= new CsvOptions();

            // Parse CSV lines
            var lines = ParseCsvLines(content, options);
            if (lines.Count == 0)
                return Result.Ok(new List<T>());

            // Get property info
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (properties.Length == 0)
                return Result.Fail<List<T>>("CSV.READ.NO_PROPERTIES", "Type has no public properties.");

            // Determine headers and mapping
            var headerLine = options.HasHeader ? lines[0] : null;
            var startIndex = options.HasHeader ? 1 : 0;

            // Create header to property mapping
            var headerToProperty = new Dictionary<int, PropertyInfo>();

            if (headerLine != null)
            {
                // Map headers to properties using provided mapping or by name
                for (int i = 0; i < headerLine.Count; i++)
                {
                    var header = headerLine[i];
                    PropertyInfo? property = null;

                    if (mapping != null && mapping.HeaderToProperty.TryGetValue(header, out var mappedProperty))
                    {
                        property = mappedProperty;
                    }
                    else
                    {
                        // Try to match by property name
                        property = properties.FirstOrDefault(p =>
                            p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                    }

                    if (property != null)
                    {
                        headerToProperty[i] = property;
                    }
                }
            }
            else
            {
                // No headers, map columns by property order
                for (int i = 0; i < properties.Length && i < (lines.Count > 0 ? lines[0].Count : 0); i++)
                {
                    headerToProperty[i] = properties[i];
                }
            }

            // Deserialize data rows
            var result = new List<T>();
            for (int lineIndex = startIndex; lineIndex < lines.Count; lineIndex++)
            {
                var line = lines[lineIndex];
                var instance = Activator.CreateInstance<T>();

                for (int columnIndex = 0; columnIndex < line.Count; columnIndex++)
                {
                    if (!headerToProperty.TryGetValue(columnIndex, out var property))
                        continue;

                    var value = line[columnIndex];
                    if (options.TrimValues)
                        value = value.Trim();

                    try
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            var convertedValue = ConvertValue(value, property.PropertyType);
                            property.SetValue(instance, convertedValue);
                        }
                        else if (IsNullableType(property.PropertyType))
                        {
                            property.SetValue(instance, null);
                        }
                    }
                    catch
                    {
                        // Skip values that cannot be converted
                    }
                }

                result.Add(instance);
            }

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<T>>(
                Error.FromException(ex, "CSV.READ.FAILED"));
        }
    }

    /// <summary>
    /// Parses CSV content into a list of records (each record is a list of field values).
    /// Handles quoted fields with embedded delimiters and newlines.
    /// </summary>
    private static List<List<string>> ParseCsvLines(string content, CsvOptions options)
    {
        var records = new List<List<string>>();
        var currentRecord = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;
        var i = 0;

        while (i < content.Length)
        {
            var c = content[i];

            if (inQuotes)
            {
                if (c == options.QuoteChar)
                {
                    // Check for escaped quote (quote char doubled)
                    if (i + 1 < content.Length && content[i + 1] == options.QuoteChar)
                    {
                        currentField.Append(options.QuoteChar);
                        i += 2;
                        continue;
                    }
                    else
                    {
                        // End of quoted field
                        inQuotes = false;
                        i++;
                        continue;
                    }
                }
                else
                {
                    currentField.Append(c);
                    i++;
                    continue;
                }
            }

            // Not in quotes
            if (c == options.QuoteChar)
            {
                inQuotes = true;
                i++;
                continue;
            }

            if (c == options.Delimiter)
            {
                currentRecord.Add(currentField.ToString());
                currentField.Clear();
                i++;
                continue;
            }

            if (c == '\r')
            {
                // Handle \r\n or just \r
                if (i + 1 < content.Length && content[i + 1] == '\n')
                {
                    i++;
                }
                currentRecord.Add(currentField.ToString());
                if (currentRecord.Count > 0 && currentRecord.Any(f => !string.IsNullOrEmpty(f)))
                {
                    records.Add(currentRecord);
                }
                currentRecord = new List<string>();
                currentField.Clear();
                i++;
                continue;
            }

            if (c == '\n')
            {
                currentRecord.Add(currentField.ToString());
                if (currentRecord.Count > 0 && currentRecord.Any(f => !string.IsNullOrEmpty(f)))
                {
                    records.Add(currentRecord);
                }
                currentRecord = new List<string>();
                currentField.Clear();
                i++;
                continue;
            }

            currentField.Append(c);
            i++;
        }

        // Handle last field and record
        if (currentField.Length > 0 || currentRecord.Count > 0)
        {
            currentRecord.Add(currentField.ToString());
            if (currentRecord.Count > 0 && currentRecord.Any(f => !string.IsNullOrEmpty(f)))
            {
                records.Add(currentRecord);
            }
        }

        return records;
    }

    /// <summary>
    /// Writes a single CSV record (list of field values) to the string builder.
    /// Handles quoting of fields with special characters.
    /// </summary>
    private static void WriteRecord(StringBuilder sb, List<string> fields, CsvOptions options)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            if (i > 0)
                sb.Append(options.Delimiter);

            var field = fields[i] ?? string.Empty;

            // Check if field needs quoting (contains delimiter, quote char, or newline)
            if (field.Contains(options.Delimiter) ||
                field.Contains(options.QuoteChar) ||
                field.Contains('\n') ||
                field.Contains('\r'))
            {
                sb.Append(options.QuoteChar);
                // Escape quote characters by doubling them
                sb.Append(field.Replace(options.QuoteChar.ToString(), $"{options.QuoteChar}{options.QuoteChar}"));
                sb.Append(options.QuoteChar);
            }
            else
            {
                sb.Append(field);
            }
        }

        sb.AppendLine();
    }

    /// <summary>
    /// Converts a string value to the specified type.
    /// </summary>
    private static object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
            return value;

        if (underlyingType == typeof(int))
            return int.Parse(value);

        if (underlyingType == typeof(long))
            return long.Parse(value);

        if (underlyingType == typeof(decimal))
            return decimal.Parse(value);

        if (underlyingType == typeof(double))
            return double.Parse(value);

        if (underlyingType == typeof(float))
            return float.Parse(value);

        if (underlyingType == typeof(bool))
            return bool.Parse(value);

        if (underlyingType == typeof(DateTime))
            return DateTime.Parse(value);

        if (underlyingType == typeof(Guid))
            return Guid.Parse(value);

        if (underlyingType.IsEnum)
            return Enum.Parse(underlyingType, value);

        return value;
    }

    /// <summary>
    /// Determines if a type is nullable (Nullable&lt;T&gt; or reference type).
    /// </summary>
    private static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
    }
}
