namespace SmartWorkz.Core;

public interface IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(SmartWorkz.Shared.Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> FindAllAsync(SmartWorkz.Shared.Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<int> CountAsync(SmartWorkz.Shared.Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(SmartWorkz.Shared.Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
