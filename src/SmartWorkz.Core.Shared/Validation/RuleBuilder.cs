using System.Linq.Expressions;

namespace SmartWorkz.Core.Shared.Validation;

public sealed class RuleBuilder<T, TProperty>
{
    private readonly List<Func<T, Task<ValidationFailure?>>> _rules = new();
    private readonly string _propertyName;
    private readonly Expression<Func<T, TProperty>> _propertyExpression;

    public RuleBuilder(Expression<Func<T, TProperty>> propertyExpression)
    {
        _propertyExpression = propertyExpression;
        _propertyName = GetPropertyName(propertyExpression);
    }

    public RuleBuilder<T, TProperty> NotEmpty()
    {
        _rules.Add(async obj =>
        {
            var value = GetPropertyValue(obj);
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                return new ValidationFailure(_propertyName, $"{_propertyName} cannot be empty");
            return null;
        });
        return this;
    }

    public RuleBuilder<T, TProperty> MaxLength(int maxLength)
    {
        _rules.Add(async obj =>
        {
            var value = GetPropertyValue(obj);
            if (value is string str && str.Length > maxLength)
                return new ValidationFailure(_propertyName, $"{_propertyName} must not exceed {maxLength} characters");
            return null;
        });
        return this;
    }

    public RuleBuilder<T, TProperty> GreaterThanOrEqual(IComparable comparable)
    {
        _rules.Add(async obj =>
        {
            var value = GetPropertyValue(obj);
            if (value is IComparable comp && comp.CompareTo(comparable) < 0)
                return new ValidationFailure(_propertyName, $"{_propertyName} must be greater than or equal to {comparable}");
            return null;
        });
        return this;
    }

    public RuleBuilder<T, TProperty> LessThanOrEqual(IComparable comparable)
    {
        _rules.Add(async obj =>
        {
            var value = GetPropertyValue(obj);
            if (value is IComparable comp && comp.CompareTo(comparable) > 0)
                return new ValidationFailure(_propertyName, $"{_propertyName} must be less than or equal to {comparable}");
            return null;
        });
        return this;
    }

    public RuleBuilder<T, TProperty> Custom(Func<TProperty, Task<bool>> predicate, string errorMessage)
    {
        _rules.Add(async obj =>
        {
            var value = GetPropertyValue(obj);
            if (!await predicate((TProperty)value!))
                return new ValidationFailure(_propertyName, errorMessage);
            return null;
        });
        return this;
    }

    internal async Task<IEnumerable<ValidationFailure>> ValidateAsync(T instance)
    {
        var failures = new List<ValidationFailure>();
        foreach (var rule in _rules)
        {
            var failure = await rule(instance);
            if (failure != null)
                failures.Add(failure);
        }
        return failures;
    }

    private TProperty GetPropertyValue(T obj)
    {
        var compiled = _propertyExpression.Compile();
        return compiled(obj);
    }

    private static string GetPropertyName(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpr)
            return memberExpr.Member.Name;
        throw new ArgumentException("Expression must be a member access");
    }
}
