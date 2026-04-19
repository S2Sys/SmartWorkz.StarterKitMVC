namespace SmartWorkz.Core.Shared.Base_Classes;

/// <summary>
/// Abstract base class for type conversion between domain objects and DTOs.
/// Enables loose coupling between layers by centralizing conversion logic.
/// </summary>
/// <typeparam name="T">Source type to convert from</typeparam>
public abstract class ValueConverter<T>
{
    /// <summary>Convert a single source object to target type.</summary>
    public abstract TTarget Convert<TTarget>(T source) where TTarget : class;

    /// <summary>Convert a single source object using dynamic target type resolution.</summary>
    public abstract object Convert(T source, Type targetType);

    /// <summary>Convert a collection of source objects to target type.</summary>
    public virtual List<TTarget> ConvertList<TTarget>(IEnumerable<T> sources) where TTarget : class
        => sources.Select(s => Convert<TTarget>(s)).ToList();

    /// <summary>Convert a collection using dynamic target type resolution.</summary>
    public virtual List<object> ConvertList(IEnumerable<T> sources, Type targetType)
        => sources.Select(s => Convert(s, targetType)).ToList();

    /// <summary>Convert from a collection of different source types.</summary>
    public virtual List<TTarget> ConvertFromList<TSource, TTarget>(IEnumerable<TSource> sources)
        where TTarget : class
        where TSource : class
    {
        var converter = Activator.CreateInstance(GetType(), null) as ValueConverter<TSource>;
        return converter?.ConvertList<TTarget>(sources) ?? [];
    }
}
