using System.Linq.Expressions;

namespace SmartWorkz.Core.Shared.Validation;

public abstract class ValidatorBase<T> : IValidator<T>
{
    private readonly List<Func<T, Task<IEnumerable<ValidationFailure>>>> _ruleBuilders = new();

    protected RuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> property)
    {
        var ruleBuilder = new RuleBuilder<T, TProperty>(property);
        _ruleBuilders.Add(async obj => await ruleBuilder.ValidateAsync(obj));
        return ruleBuilder;
    }

    public virtual async Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
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
