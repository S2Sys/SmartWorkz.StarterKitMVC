namespace SmartWorkz.Core.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : class, IEntity<TId>;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
