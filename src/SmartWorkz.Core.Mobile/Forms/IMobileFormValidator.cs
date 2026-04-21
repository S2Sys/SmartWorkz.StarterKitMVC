namespace SmartWorkz.Mobile;

public interface IMobileFormValidator<T>
{
    Task<bool> ValidateAsync(T model, CancellationToken ct = default);
    bool IsValid { get; }
    IReadOnlyDictionary<string, string> FieldErrors { get; }
    string? GetError(string propertyName);
}
