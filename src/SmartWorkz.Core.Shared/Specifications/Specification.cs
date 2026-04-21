using System.Linq.Expressions;

namespace SmartWorkz.Shared;

public abstract class Specification<T> where T : class
{
    public List<Expression<Func<T, bool>>> Criteria { get; } = new();
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int? Take { get; private set; }
    public int? Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected virtual void AddCriteria(Expression<Func<T, bool>> criteria) => Criteria.Add(criteria);
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression) => OrderByDescending = orderByDescendingExpression;

    public Expression<Func<T, bool>> GetCriteria()
    {
        if (Criteria.Count == 0)
            return _ => true;

        var combined = Criteria[0];
        foreach (var criteria in Criteria.Skip(1))
        {
            combined = CombineWithAnd(combined, criteria);
        }

        return combined;
    }

    private static Expression<Func<T, bool>> CombineWithAnd(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T));
        var leftInvoke = Expression.Invoke(left, param);
        var rightInvoke = Expression.Invoke(right, param);
        var and = Expression.And(leftInvoke, rightInvoke);
        return Expression.Lambda<Func<T, bool>>(and, param);
    }

    public Specification<T> And(Specification<T> other)
    {
        foreach (var criteria in other.Criteria)
            Criteria.Add(criteria);
        return this;
    }

    public Specification<T> Or(Specification<T> other)
    {
        var combined = GetCriteria();
        foreach (var otherCriteria in other.Criteria)
        {
            var param = Expression.Parameter(typeof(T));
            var leftInvoke = Expression.Invoke(combined, param);
            var rightInvoke = Expression.Invoke(otherCriteria, param);
            var or = Expression.Or(leftInvoke, rightInvoke);
            combined = Expression.Lambda<Func<T, bool>>(or, param);
        }
        Criteria.Clear();
        Criteria.Add(combined);
        return this;
    }

    public Specification<T> Not()
    {
        var combined = GetCriteria();
        var param = Expression.Parameter(typeof(T));
        var invoked = Expression.Invoke(combined, param);
        var not = Expression.Not(invoked);
        var negated = Expression.Lambda<Func<T, bool>>(not, param);
        Criteria.Clear();
        Criteria.Add(negated);
        return this;
    }
}
