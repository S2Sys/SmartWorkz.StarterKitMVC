namespace SmartWorkz.Core;

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task SendAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default);
}
