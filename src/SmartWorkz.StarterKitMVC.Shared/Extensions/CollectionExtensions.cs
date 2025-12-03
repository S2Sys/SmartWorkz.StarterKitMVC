using System.Collections.ObjectModel;

namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Extension methods for collections and enumerables.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Converts an enumerable to a read-only collection.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <returns>A read-only collection containing the elements.</returns>
    /// <example>
    /// <code>
    /// var list = new List&lt;string&gt; { "a", "b", "c" };
    /// IReadOnlyCollection&lt;string&gt; readOnly = list.ToReadOnlyCollection();
    /// </code>
    /// </example>
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source) =>
        new ReadOnlyCollection<T>(source.ToList());
}
