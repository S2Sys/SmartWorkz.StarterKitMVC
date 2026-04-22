namespace SmartWorkz.Mobile;

/// <summary>
/// Service for accessing device contacts from the address book.
/// </summary>
public interface IContactsService
{
    /// <summary>
    /// Gets all contacts from the device address book.
    /// Requires READ_CONTACTS (Android) or Contacts permission (iOS).
    /// </summary>
    Task<IReadOnlyList<Contact>> GetAllContactsAsync(CancellationToken ct = default);

    /// <summary>
    /// Searches contacts by name or email.
    /// Requires READ_CONTACTS (Android) or Contacts permission (iOS).
    /// </summary>
    Task<IReadOnlyList<Contact>> SearchContactsAsync(string query, CancellationToken ct = default);

    /// <summary>
    /// Picks a single contact from the device.
    /// Requires READ_CONTACTS (Android) or Contacts permission (iOS).
    /// On unsupported platforms, returns null.
    /// </summary>
    Task<Contact?> PickContactAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if contacts service is available on this platform.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
