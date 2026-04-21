namespace SmartWorkz.Shared;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON serialization utilities using System.Text.Json.
/// Provides consistent serialization options across the application.
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>Serialize object to JSON string.</summary>
    public static string Serialize<T>(T? obj, bool pretty = false)
    {
        var options = pretty ? PrettyOptions : DefaultOptions;
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>Serialize object to JSON string with dynamic type.</summary>
    public static string Serialize(object? obj, Type type, bool pretty = false)
    {
        var options = pretty ? PrettyOptions : DefaultOptions;
        return JsonSerializer.Serialize(obj, type, options);
    }

    /// <summary>Deserialize JSON string to object.</summary>
    public static T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, DefaultOptions);

    /// <summary>Deserialize JSON string to object with dynamic type.</summary>
    public static object? Deserialize(string json, Type type)
        => JsonSerializer.Deserialize(json, type, DefaultOptions);

    /// <summary>Deserialize JSON asynchronously from stream.</summary>
    public static async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        => await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions, cancellationToken);

    /// <summary>Serialize asynchronously to stream.</summary>
    public static async Task SerializeAsync<T>(Stream stream, T? obj, CancellationToken cancellationToken = default)
        => await JsonSerializer.SerializeAsync(stream, obj, DefaultOptions, cancellationToken);

    /// <summary>Check if string is valid JSON.</summary>
    public static bool IsValidJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Parse JSON and extract value at specified path (dot notation).</summary>
    public static object? GetValueByPath(string json, string path)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var pathParts = path.Split('.');
            var current = (JsonElement?)doc.RootElement;

            foreach (var part in pathParts)
            {
                if (current?.ValueKind != JsonValueKind.Object)
                    return null;

                if (!current.Value.TryGetProperty(part, out var next))
                    return null;

                current = next;
            }

            return current?.GetRawText();
        }
        catch
        {
            return null;
        }
    }
}
