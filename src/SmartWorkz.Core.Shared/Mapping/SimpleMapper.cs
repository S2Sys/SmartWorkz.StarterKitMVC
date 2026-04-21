using System.Collections.Concurrent;

namespace SmartWorkz.Shared;

/// <summary>
/// A simple in-memory mapper that supports registering and executing mapping profiles.
/// </summary>
public class SimpleMapper : IMapper
{
    private readonly ConcurrentDictionary<(Type, Type), Delegate> _mappings = new();

    public TTarget Map<TSource, TTarget>(TSource source)
        where TSource : class
        where TTarget : class
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var key = (typeof(TSource), typeof(TTarget));
        if (_mappings.TryGetValue(key, out var mapping))
        {
            var func = mapping as Func<TSource, TTarget>;
            return func!(source);
        }

        throw new InvalidOperationException($"No mapping registered for {typeof(TSource).Name} to {typeof(TTarget).Name}");
    }

    public object Map(object source, Type sourceType, Type targetType)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var key = (sourceType, targetType);
        if (_mappings.TryGetValue(key, out var mapping))
        {
            return mapping.DynamicInvoke(source)!;
        }

        throw new InvalidOperationException($"No mapping registered for {sourceType.Name} to {targetType.Name}");
    }

    public async Task<TTarget> MapAsync<TSource, TTarget>(
        TSource source,
        CancellationToken cancellationToken = default)
        where TSource : class
        where TTarget : class
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var key = (typeof(TSource), typeof(TTarget));
        if (_mappings.TryGetValue(key, out var mapping))
        {
            var func = mapping as Func<TSource, Task<TTarget>>;
            if (func != null)
                return await func(source);

            // Fallback to sync mapping wrapped in a task
            var syncFunc = mapping as Func<TSource, TTarget>;
            if (syncFunc != null)
                return syncFunc(source);
        }

        throw new InvalidOperationException($"No mapping registered for {typeof(TSource).Name} to {typeof(TTarget).Name}");
    }

    public IEnumerable<TTarget> MapCollection<TSource, TTarget>(IEnumerable<TSource> sources)
        where TSource : class
        where TTarget : class
    {
        if (sources == null)
            throw new ArgumentNullException(nameof(sources));

        return sources.Select(s => Map<TSource, TTarget>(s));
    }

    public async Task<IEnumerable<TTarget>> MapCollectionAsync<TSource, TTarget>(
        IEnumerable<TSource> sources,
        CancellationToken cancellationToken = default)
        where TSource : class
        where TTarget : class
    {
        if (sources == null)
            throw new ArgumentNullException(nameof(sources));

        var results = new List<TTarget>();
        foreach (var source in sources)
        {
            var mapped = await MapAsync<TSource, TTarget>(source, cancellationToken);
            results.Add(mapped);
        }

        return results;
    }

    public void RegisterProfile<TSource, TTarget>(IMapperProfile<TSource, TTarget> profile)
        where TSource : class
        where TTarget : class
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        var key = (typeof(TSource), typeof(TTarget));
        var syncFunc = new Func<TSource, TTarget>(src =>
            profile.Map(src));

        _mappings[key] = syncFunc;
    }
}
