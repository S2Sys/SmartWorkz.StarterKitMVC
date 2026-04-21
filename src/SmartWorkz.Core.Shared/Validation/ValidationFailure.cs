namespace SmartWorkz.Shared;

public sealed class ValidationFailure
{
    public ValidationFailure(string propertyName, string message)
    {
        PropertyName = Guard.NotEmpty(propertyName, nameof(propertyName));
        Message = Guard.NotEmpty(message, nameof(message));
    }

    public string PropertyName { get; }
    public string Message { get; }

    public override string ToString() => $"{PropertyName}: {Message}";
}
