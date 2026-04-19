namespace SmartWorkz.Core.Shared.Mapping;

/// <summary>
/// Mapping service abstraction for transforming objects between types.
/// Supports registration of mapping profiles and bidirectional conversions.
/// </summary>
public interface IMapper
{
    /// <summary>Map source object to target type.</summary>
    TTarget Map<TSource, TTarget>(TSource source) where TSource : class where TTarget : class;

    /// <summary>Map source object to target type using dynamic type.</summary>
    object Map(object source, Type sourceType, Type targetType);

    /// <summary>Map asynchronously with potential async operations in profile.</summary>
    Task<TTarget> MapAsync<TSource, TTarget>(TSource source, CancellationToken cancellationToken = default)
        where TSource : class where TTarget : class;

    /// <summary>Map collection of sources to targets.</summary>
    IEnumerable<TTarget> MapCollection<TSource, TTarget>(IEnumerable<TSource> sources)
        where TSource : class where TTarget : class;

    /// <summary>Map collection asynchronously.</summary>
    Task<IEnumerable<TTarget>> MapCollectionAsync<TSource, TTarget>(
        IEnumerable<TSource> sources,
        CancellationToken cancellationToken = default)
        where TSource : class where TTarget : class;

    /// <summary>Register a mapping profile.</summary>
    void RegisterProfile<TSource, TTarget>(IMapperProfile<TSource, TTarget> profile)
        where TSource : class where TTarget : class;
}
