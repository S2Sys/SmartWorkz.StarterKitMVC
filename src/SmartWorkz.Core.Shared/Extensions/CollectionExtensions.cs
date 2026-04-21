using System.Collections.ObjectModel;

namespace SmartWorkz.Shared;

public static class CollectionExtensions
{
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        => new ReadOnlyCollection<T>(source.ToList());

    /// <summary>Returns true if the collection is null or has no elements.</summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        => source is null || !source.Any();
}
