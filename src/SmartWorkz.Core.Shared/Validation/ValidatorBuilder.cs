using System.Linq.Expressions;

namespace SmartWorkz.Shared;

/// <summary>
/// Fluent validator builder for defining validation rules.
/// Provides an alternative to ValidatorBase for more concise validator definitions.
/// </summary>
public sealed class ValidatorBuilder<T> : IValidator<T> where T : class
{
    private List<Func<T, Task<IEnumerable<ValidationFailure>>>> _ruleBuilders = new();

    /// <summary>
    /// Add a rule for a property using fluent API.
    /// </summary>
    public RuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var ruleBuilder = new RuleBuilder<T, TProperty>(propertyExpression);
        _ruleBuilders.Add(async obj => await ruleBuilder.ValidateAsync(obj));
        return ruleBuilder;
    }

    /// <summary>
    /// Validate instance against all rules.
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
    {
        var failures = new List<ValidationFailure>();

        foreach (var rule in _ruleBuilders)
        {
            var ruleFailures = await rule(instance);
            failures.AddRange(ruleFailures);
        }

        return failures.Count > 0
            ? new ValidationResult(failures)
            : ValidationResult.Success();
    }
}
