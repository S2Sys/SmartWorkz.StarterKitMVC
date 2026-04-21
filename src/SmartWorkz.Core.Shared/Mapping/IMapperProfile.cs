namespace SmartWorkz.Shared;

/// <summary>
/// Profile for defining mapping rules between types.
/// Implemented by concrete profiles that configure source-to-target transformations.
/// </summary>
public interface IMapperProfile
{
    /// <summary>Get the source type for this profile.</summary>
    Type SourceType { get; }

    /// <summary>Get the target type for this profile.</summary>
    Type TargetType { get; }
}

/// <summary>Typed mapper profile for strong typing.</summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TTarget">Target type</typeparam>
public interface IMapperProfile<TSource, TTarget> : IMapperProfile
    where TSource : class
    where TTarget : class
{
    /// <summary>Transform source to target synchronously.</summary>
    TTarget Map(TSource source);

    /// <summary>Transform source to target asynchronously.</summary>
    Task<TTarget> MapAsync(TSource source, CancellationToken cancellationToken = default);
}
