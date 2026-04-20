using SmartWorkz.Core.Shared.Specifications;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartWorkz.Sample.ECommerce.Infrastructure.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Order?> FindAsync(Specification<Order> specification, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Order>> FindAllAsync(Specification<Order> specification, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Specification<Order> specification, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Specification<Order> specification, CancellationToken cancellationToken = default);
    Task AddAsync(Order entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Order entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default);
}

public class OrderRepository(ECommerceDbContext db) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync(cancellationToken);

    public async Task<Order?> FindAsync(Specification<Order> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product);
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        if (specification.Skip.HasValue) query = query.Skip(specification.Skip.Value);
        if (specification.Take.HasValue) query = query.Take(specification.Take.Value);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> FindAllAsync(Specification<Order> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product);
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        if (specification.Skip.HasValue) query = query.Skip(specification.Skip.Value);
        if (specification.Take.HasValue) query = query.Take(specification.Take.Value);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Specification<Order> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = db.Orders;
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Specification<Order> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = db.Orders;
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        db.Orders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        db.Orders.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        db.Orders.Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        db.Orders.UpdateRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await db.Orders.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (order != null)
        {
            db.Orders.Remove(order);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Order entity, CancellationToken cancellationToken = default)
    {
        db.Orders.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        db.Orders.RemoveRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }
}
