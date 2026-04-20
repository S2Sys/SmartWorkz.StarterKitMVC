using System.Linq.Expressions;
using SmartWorkz.Core.Abstractions;
using SmartWorkz.Core.Shared.Specifications;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartWorkz.Sample.ECommerce.Infrastructure.Repositories;

public class CustomerRepository(ECommerceDbContext db) : IRepository<Customer, int>
{
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await db.Customers.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Customers.Include(c => c.Orders).ToListAsync(cancellationToken);

    public async Task<Customer?> FindAsync(Specification<Customer> specification, CancellationToken cancellationToken = default)
    {
        var query = db.Customers.AsQueryable();

        // Apply eager loading includes
        foreach (var include in specification.Includes)
            query = query.Include(include);

        // Apply criteria filters
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));

        // Apply ordering
        query = ApplyOrdering(query, specification);

        // Apply paging
        if (specification.Skip.HasValue) query = query.Skip(specification.Skip.Value);
        if (specification.Take.HasValue) query = query.Take(specification.Take.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Customer>> FindAllAsync(Specification<Customer> specification, CancellationToken cancellationToken = default)
    {
        var query = db.Customers.AsQueryable();

        // Apply eager loading includes
        foreach (var include in specification.Includes)
            query = query.Include(include);

        // Apply criteria filters
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));

        // Apply ordering
        query = ApplyOrdering(query, specification);

        // Apply paging
        if (specification.Skip.HasValue) query = query.Skip(specification.Skip.Value);
        if (specification.Take.HasValue) query = query.Take(specification.Take.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Specification<Customer> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query = db.Customers;
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Specification<Customer> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query = db.Customers;
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        db.Customers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Customer> entities, CancellationToken cancellationToken = default)
    {
        db.Customers.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        db.Customers.Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<Customer> entities, CancellationToken cancellationToken = default)
    {
        db.Customers.UpdateRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await db.Customers.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (customer != null)
        {
            db.Customers.Remove(customer);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        db.Customers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Customer> entities, CancellationToken cancellationToken = default)
    {
        db.Customers.RemoveRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Customer> ApplyOrdering(IQueryable<Customer> query, Specification<Customer> specification)
    {
        if (specification.OrderBy != null)
            return query.OrderBy(specification.OrderBy);

        if (specification.OrderByDescending != null)
            return query.OrderByDescending(specification.OrderByDescending);

        return query;
    }
}
