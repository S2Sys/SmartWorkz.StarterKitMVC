namespace SmartWorkz.Core;

public interface IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> FindAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> FindAllAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
