using System.Linq.Expressions;

namespace SmartWorkz.Shared;

/// <summary>
/// Single validation rule for a property.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TProperty">Property type</typeparam>
public interface IValidationRule<T, TProperty>
{
    /// <summary>Validate property and return results.</summary>
    Task<IEnumerable<ValidationFailure>> ValidateAsync(T instance);
}

/// <summary>
/// Base implementation for custom validation rules.
/// </summary>
public abstract class ValidationRule<T, TProperty> : IValidationRule<T, TProperty> where T : class
{
    protected Expression<Func<T, TProperty>> _propertyExpression;
    protected string _propertyName;
    protected List<Func<TProperty, Task<bool>>> _conditions = new();

    public ValidationRule(Expression<Func<T, TProperty>> propertyExpression)
    {
        _propertyExpression = propertyExpression;
        _propertyName = GetPropertyName(propertyExpression);
    }

    public async virtual Task<IEnumerable<ValidationFailure>> ValidateAsync(T instance)
    {
        var failures = new List<ValidationFailure>();
        var compiled = _propertyExpression.Compile();
        var propertyValue = compiled(instance);

        foreach (var condition in _conditions)
        {
            var isValid = await condition(propertyValue);
            if (!isValid)
            {
                var failure = GetValidationFailure(propertyValue);
                if (failure != null)
                    failures.Add(failure);
                break;
            }
        }
        return failures;
    }

    protected abstract ValidationFailure? GetValidationFailure(TProperty value);

    protected static string GetPropertyName(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpr)
            return memberExpr.Member.Name;
        throw new ArgumentException("Expression must be a member access");
    }
}
