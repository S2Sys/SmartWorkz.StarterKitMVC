namespace SmartWorkz.Shared;

/// <summary>
/// Combines multiple validators into a single validator.
/// Useful for composing validators from different sources.
/// </summary>
public sealed class CompositeValidator<T> : IValidator<T> where T : class
{
    private List<IValidator<T>> _validators = new();

    public CompositeValidator(params IValidator<T>[] validators)
    {
        _validators = validators.ToList();
    }

    public void AddValidator(IValidator<T> validator) => _validators.Add(validator);

    public async Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
    {
        var allFailures = new List<ValidationFailure>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(instance, cancellationToken);
            if (!result.IsValid)
                allFailures.AddRange(result.Failures);
        }

        return allFailures.Count == 0
            ? ValidationResult.Success()
            : new ValidationResult(allFailures);
    }
}
