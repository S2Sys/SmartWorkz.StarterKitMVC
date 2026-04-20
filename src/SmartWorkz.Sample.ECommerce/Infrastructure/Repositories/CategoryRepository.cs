using System.Linq.Expressions;
using SmartWorkz.Core.Abstractions;
using SmartWorkz.Core.Shared.Specifications;
using SmartWorkz.Sample.ECommerce.Domain.Entities;
using SmartWorkz.Sample.ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartWorkz.Sample.ECommerce.Infrastructure.Repositories;

public class CategoryRepository(ECommerceDbContext db) : IRepository<Category, int>
{
    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Categories.Include(c => c.Products).ToListAsync(cancellationToken);

    public async Task<Category?> FindAsync(Specification<Category> specification, CancellationToken cancellationToken = default)
    {
        var query = db.Categories.AsQueryable();

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

    public async Task<IReadOnlyCollection<Category>> FindAllAsync(Specification<Category> specification, CancellationToken cancellationToken = default)
    {
        var query = db.Categories.AsQueryable();

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

    public async Task<int> CountAsync(Specification<Category> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = db.Categories;
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Specification<Category> specification, CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = db.Categories;
        query = specification.Criteria.Aggregate(query, (q, c) => q.Where(c));
        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        db.Categories.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Category> entities, CancellationToken cancellationToken = default)
    {
        db.Categories.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        db.Categories.Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<Category> entities, CancellationToken cancellationToken = default)
    {
        db.Categories.UpdateRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await db.Categories.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (category != null)
        {
            db.Categories.Remove(category);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Category entity, CancellationToken cancellationToken = default)
    {
        db.Categories.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Category> entities, CancellationToken cancellationToken = default)
    {
        db.Categories.RemoveRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Category> ApplyOrdering(IQueryable<Category> query, Specification<Category> specification)
    {
        if (specification.OrderBy != null)
            return query.OrderBy(specification.OrderBy);

        if (specification.OrderByDescending != null)
            return query.OrderByDescending(specification.OrderByDescending);

        return query;
    }
}
