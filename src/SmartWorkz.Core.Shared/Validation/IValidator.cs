namespace SmartWorkz.Core.Shared.Validation;

public interface IValidator<T>
{
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
