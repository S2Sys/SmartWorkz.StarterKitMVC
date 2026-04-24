namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published when a new user registers in the system.
/// </summary>
public class UserRegisteredEvent
{
    public UserRegisteredEvent(string userId, string email, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        RegisteredAt = DateTime.UtcNow;
    }

    public string UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime RegisteredAt { get; }
}
