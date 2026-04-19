namespace SmartWorkz.Core.Shared.Utilities;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides utilities for JSON serialization and deserialization.
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes an object to JSON. Optionally indents the output for readability.
    /// </summary>
    public static Result<string> Serialize<T>(T obj, bool indent = true)
    {
        try
        {
            if (obj == null)
                return Result.Ok("null");

            var options = indent ? PrettyOptions : DefaultOptions;
            var json = JsonSerializer.Serialize(obj, options);
            return Result.Ok(json);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Error.JsonSerialize", ex.Message);
        }
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T.
    /// </summary>
    public static Result<T> Deserialize<T>(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return Result.Fail<T>("Error.JsonEmpty", "JSON cannot be null or empty");

            var result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            if (result == null)
                return Result.Fail<T>("Error.JsonDeserialize", "Deserialization resulted in null");

            return Result.Ok(result);
        }
        catch (JsonException ex)
        {
            return Result.Fail<T>("Error.JsonInvalid", $"Invalid JSON format: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail<T>("Error.JsonDeserialize", ex.Message);
        }
    }

    /// <summary>
    /// Determines if a JSON string is prettified (formatted with indentation).
    /// </summary>
    public static bool IsPrettyJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        var trimmed = json.Trim();
        return trimmed.Contains('\n') || trimmed.Contains('\r');
    }
}
