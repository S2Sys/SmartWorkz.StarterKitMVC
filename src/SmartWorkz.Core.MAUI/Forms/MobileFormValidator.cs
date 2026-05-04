namespace SmartWorkz.Mobile;

using SmartWorkz.Shared;

public sealed class MobileFormValidator<T> : IMobileFormValidator<T>
{
    private readonly IValidator<T> _inner;
    private readonly Dictionary<string, string> _errors = new();

    public MobileFormValidator(IValidator<T> inner)
    {
        _inner = Guard.NotNull(inner, nameof(inner));
    }

    public bool IsValid => _errors.Count == 0;

    public IReadOnlyDictionary<string, string> FieldErrors => _errors;

    public async Task<bool> ValidateAsync(T model, CancellationToken ct = default)
    {
        _errors.Clear();
        var result = await _inner.ValidateAsync(model, ct);
        if (!result.IsValid)
        {
            foreach (var failure in result.Failures)
            {
                _errors.TryAdd(failure.PropertyName, failure.Message);
            }
        }
        return result.IsValid;
    }

    public string? GetError(string propertyName) =>
        _errors.TryGetValue(propertyName, out var msg) ? msg : null;
}
