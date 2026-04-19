using System.Linq.Expressions;

namespace SmartWorkz.Core.Shared.Specifications;

/// <summary>
/// Base class for the Specification pattern — encapsulates a business rule as a composable predicate.
///
/// Useful for complex filtering logic that would otherwise scatter across repository methods.
/// The Dapper repositories use object filters (property-bag style) so Specification is
/// most valuable with EF Core's IQueryable pipeline.
///
/// Usage:
///   public class ActiveTenantSpec : Specification&lt;Tenant&gt;
///   {
///       public override Expression&lt;Func&lt;Tenant, bool&gt;&gt; ToExpression()
///           => t => t.IsActive && !t.IsDeleted;
///   }
///
///   var spec = new ActiveTenantSpec().And(new TenantByIdSpec(id));
///   var tenants = dbSet.Where(spec.ToExpression()).ToListAsync();
/// </summary>
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
        => ToExpression().Compile()(entity);

    public Specification<T> And(Specification<T> other)
        => new AndSpecification<T>(this, other);

    public Specification<T> Or(Specification<T> other)
        => new OrSpecification<T>(this, other);

    public Specification<T> Not()
        => new NotSpecification<T>(this);
}

internal sealed class AndSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();
        var param = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

internal sealed class OrSpecification<T>(Specification<T> left, Specification<T> right)
    : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();
        var param = Expression.Parameter(typeof(T));
        var body = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

internal sealed class NotSpecification<T>(Specification<T> inner) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var innerExpr = inner.ToExpression();
        var param = Expression.Parameter(typeof(T));
        var body = Expression.Not(Expression.Invoke(innerExpr, param));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
