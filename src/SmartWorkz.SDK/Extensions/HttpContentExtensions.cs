namespace SmartWorkz.Extensions;

/// <summary>
/// Extension methods for HttpContent to simplify JSON deserialization.
/// </summary>
internal static class HttpContentExtensions
{
    /// <summary>
    /// Reads and deserializes JSON content asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="content">The HTTP content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized object of type T.</returns>
    public static async Task<T?> ReadAsAsync<T>(this HttpContent content, CancellationToken cancellationToken = default)
    {
        var json = await content.ReadAsStringAsync(cancellationToken);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }
}
