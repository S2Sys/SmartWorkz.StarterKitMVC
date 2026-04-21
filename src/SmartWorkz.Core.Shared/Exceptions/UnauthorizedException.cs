namespace SmartWorkz.Shared;

public class UnauthorizedException : ApplicationException
{
    public string? RequiredPermission { get; }

    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException(string message, string requiredPermission)
        : base(message)
    {
        RequiredPermission = requiredPermission;
    }
}
