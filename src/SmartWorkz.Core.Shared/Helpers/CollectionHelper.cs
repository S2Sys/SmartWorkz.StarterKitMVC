namespace SmartWorkz.Shared;

public static class CollectionHelper
{
    public static bool IsNullOrEmpty<T>(IEnumerable<T>? collection)
        => collection == null || !collection.Any();

    public static bool IsNotNullOrEmpty<T>(IEnumerable<T>? collection)
        => collection != null && collection.Any();

    public static IEnumerable<T> EmptyIfNull<T>(IEnumerable<T>? collection)
        => collection ?? Enumerable.Empty<T>();

    public static List<T> ToListIfNull<T>(IEnumerable<T>? collection)
        => (collection as List<T>) ?? collection?.ToList() ?? new List<T>();

    public static IEnumerable<IEnumerable<T>> Chunk<T>(IEnumerable<T> source, int chunkSize)
    {
        while (source.Any())
        {
            yield return source.Take(chunkSize);
            source = source.Skip(chunkSize);
        }
    }

    public static IEnumerable<(T, T)> Pairs<T>(IEnumerable<T> source)
    {
        var list = source.ToList();
        for (int i = 0; i < list.Count - 1; i++)
        {
            yield return (list[i], list[i + 1]);
        }
    }

    public static HashSet<T> ToHashSet<T>(IEnumerable<T> source)
        => new(source);

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> pairs) where TKey : notnull
    {
        return pairs.ToDictionary(x => x.Key, x => x.Value);
    }
}
