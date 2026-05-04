namespace SmartWorkz.Mobile;

/// <summary>
/// Represents a contact from the device address book.
/// </summary>
public sealed record Contact(
    string Id,
    string FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    string? Address)
{
    /// <summary>
    /// Gets the display name (FirstName + LastName).
    /// </summary>
    public string DisplayName => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";

    /// <summary>
    /// Gets a value indicating whether this contact has at least one communication method.
    /// </summary>
    public bool HasContactInfo => !string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(PhoneNumber);
}
