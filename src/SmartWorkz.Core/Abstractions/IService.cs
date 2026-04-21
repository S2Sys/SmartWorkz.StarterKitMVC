namespace SmartWorkz.Core;

/// <summary>
/// Marker interface for all service classes
/// Provides base contract for domain services
/// </summary>
public interface IService
{
}

public interface IService<TEntity, TDto> where TEntity : class, IEntity<int>
{
    Task<SmartWorkz.Core.Result<TDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SmartWorkz.Core.Result<IReadOnlyCollection<TDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SmartWorkz.Core.Result<TDto>> CreateAsync(TDto dto, CancellationToken cancellationToken = default);
    Task<SmartWorkz.Core.Result<TDto>> UpdateAsync(int id, TDto dto, CancellationToken cancellationToken = default);
    Task<SmartWorkz.Core.Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
