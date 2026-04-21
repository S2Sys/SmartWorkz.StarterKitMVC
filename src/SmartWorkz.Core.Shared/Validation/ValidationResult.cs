namespace SmartWorkz.Shared;

public sealed class ValidationResult
{
    private readonly IReadOnlyCollection<ValidationFailure> _failures;

    public ValidationResult(IEnumerable<ValidationFailure>? failures = null)
    {
        _failures = (failures ?? Enumerable.Empty<ValidationFailure>()).ToList().AsReadOnly();
    }

    public bool IsValid => _failures.Count == 0;
    public IReadOnlyCollection<ValidationFailure> Failures => _failures;

    public static ValidationResult Success() => new();
    public static ValidationResult Failure(ValidationFailure failure) => new(new[] { failure });
    public static ValidationResult Failure(params ValidationFailure[] failures) => new(failures);
}
