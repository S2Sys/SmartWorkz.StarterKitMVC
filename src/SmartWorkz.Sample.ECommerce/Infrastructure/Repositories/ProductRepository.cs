using System.Linq.Expressions;
using SmartWorkz.Core;
using SmartWorkz.Shared;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartWorkz.Sample.ECommerce.Infrastructure.Repositories;

public class ProductRepository(ECommerceDbContext db) : IRepository<Product, int>
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Products.Include(p => p.Category).ToListAsync(cancellationToken);

    public async Task<Product?> FindAsync(Specification<Product> specification, CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsQueryable();

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

    public async Task<IReadOnlyCollection<Product>> FindAllAsync(Specification<Product> specification, CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsQueryable();

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

    public async Task<int> CountAsync(Specification<Product> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = db.Products;
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Specification<Product> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = db.Products;
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        db.Products.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        db.Products.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        db.Products.Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        db.Products.UpdateRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (product != null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
    {
        db.Products.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        db.Products.RemoveRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Product> ApplyOrdering(IQueryable<Product> query, Specification<Product> specification)
    {
        if (specification.OrderBy != null)
            return query.OrderBy(specification.OrderBy);

        if (specification.OrderByDescending != null)
            return query.OrderByDescending(specification.OrderByDescending);

        return query;
    }
}

