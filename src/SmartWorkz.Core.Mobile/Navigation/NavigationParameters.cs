namespace SmartWorkz.Mobile;

public sealed class NavigationParameters : Dictionary<string, object?>
{
    public T? Get<T>(string key) =>
        TryGetValue(key, out var val) && val is T typed ? typed : default;

    public bool Contains(string key) => ContainsKey(key);

    public string ToQueryString()
    {
        if (Count == 0) return string.Empty;
        var parts = this
            .Where(kv => kv.Value is not null)
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!.ToString()!)}");
        return "?" + string.Join("&", parts);
    }
}
