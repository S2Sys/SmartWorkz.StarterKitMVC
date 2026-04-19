namespace SmartWorkz.Core.Results;

/// <summary>
/// Represents a structured error with a machine-readable code and human-readable message.
/// </summary>
public sealed class Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public override string ToString() => $"[{Code}] {Message}";
}
