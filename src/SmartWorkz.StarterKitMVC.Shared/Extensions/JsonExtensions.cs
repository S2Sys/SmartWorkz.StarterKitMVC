using System.Text.Json;

namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Extension methods for JSON serialization and deserialization.
/// </summary>
public static class JsonExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>JSON string representation of the object.</returns>
    /// <example>
    /// <code>
    /// var user = new { Name = "John", Age = 30 };
    /// var json = user.ToJson();
    /// // Result: {"name":"John","age":30}
    /// </code>
    /// </example>
    public static string ToJson<T>(this T value) => JsonSerializer.Serialize(value, DefaultOptions);

    /// <summary>
    /// Deserializes a JSON string to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Deserialized object or null if deserialization fails.</returns>
    /// <example>
    /// <code>
    /// var json = "{\"name\":\"John\",\"age\":30}";
    /// var user = json.FromJson&lt;User&gt;();
    /// Console.WriteLine(user?.Name); // John
    /// </code>
    /// </example>
    public static T? FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json, DefaultOptions);
}
