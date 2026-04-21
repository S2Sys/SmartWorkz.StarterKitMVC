using System.Linq.Expressions;

namespace SmartWorkz.Core;

/// <summary>
/// Base class for all domain services
/// Provides common functionality and conventions for service implementations
/// </summary>
public abstract class ServiceBase : IService
{
    protected ServiceBase()
    {
    }
}

public abstract class ServiceBase<TEntity, TDto> : IService<TEntity, TDto>
    where TEntity : class, IEntity<int>
{
    protected readonly IRepository<TEntity, int> Repository;

    protected ServiceBase(IRepository<TEntity, int> repository)
    {
        Repository = Guard.NotNull(repository, nameof(repository));
    }

    public virtual async Task<Result<TDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail<TDto>(new Error("ENTITY_NOT_FOUND", $"{typeof(TEntity).Name} not found"));

        return Result.Ok(Map(entity));
    }

    public virtual async Task<Result<IReadOnlyCollection<TDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(cancellationToken);
        var dtos = entities.Select(Map).ToList().AsReadOnly();
        return Result.Ok<IReadOnlyCollection<TDto>>(dtos);
    }

    public virtual async Task<Result<TDto>> CreateAsync(TDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = MapToEntity(dto);
        await Repository.AddAsync(entity, cancellationToken);
        return Result.Ok(Map(entity));
    }

    public virtual async Task<Result<TDto>> UpdateAsync(int id, TDto dto, CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail<TDto>(new Error("ENTITY_NOT_FOUND", $"{typeof(TEntity).Name} not found"));

        ApplyUpdates(entity, dto);
        await Repository.UpdateAsync(entity, cancellationToken);
        return Result.Ok(Map(entity));
    }

    public virtual async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.NotDefault(id, nameof(id));

        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Fail<bool>(new Error("ENTITY_NOT_FOUND", $"{typeof(TEntity).Name} not found"));

        await Repository.DeleteAsync(id, cancellationToken);
        return Result.Ok(true);
    }

    protected abstract TDto Map(TEntity entity);
    protected abstract TEntity MapToEntity(TDto dto);
    protected virtual void ApplyUpdates(TEntity entity, TDto dto) { }
}
