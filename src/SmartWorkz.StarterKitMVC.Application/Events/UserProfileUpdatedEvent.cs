namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published when a user profile is updated.
/// </summary>
public class UserProfileUpdatedEvent
{
    public UserProfileUpdatedEvent(string userId, string email, string firstName, string lastName, string? phoneNumber = null)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public string UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string? PhoneNumber { get; }
    public DateTime UpdatedAt { get; }
}
