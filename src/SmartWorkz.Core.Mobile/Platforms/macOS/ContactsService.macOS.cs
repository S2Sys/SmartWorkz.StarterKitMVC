namespace SmartWorkz.Mobile;

using Contacts;
using Foundation;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#if MACOS

/// <summary>
/// macOS platform-specific implementation of the ContactsService.
/// Uses the native Contacts framework (CNContactStore) to access user contacts.
/// </summary>
/// <remarks>
/// This partial class provides platform-specific contact access for macOS applications.
/// It leverages the native Contacts framework to retrieve contacts from the system's
/// address book, including names, email addresses, and phone numbers.
/// </remarks>
public partial class ContactsService
{
    /// <summary>
    /// Retrieves all contacts from the system's address book on macOS.
    /// </summary>
    /// <param name="ct">Cancellation token to allow operation cancellation.</param>
    /// <returns>A read-only list of Contact objects. Returns an empty list if no contacts are found or on error.</returns>
    /// <exception cref="Exception">Caught internally and logged; method returns empty list instead of throwing.</exception>
    /// <remarks>
    /// Fetches contacts with GivenName, FamilyName, EmailAddresses, and PhoneNumbers keys.
    /// If the system contact store is unavailable, returns an empty list and logs the error.
    /// </remarks>
    private partial async Task<IReadOnlyList<Contact>> GetAllContactsAsyncPlatform(CancellationToken ct)
    {
        var contacts = new List<Contact>();
        try
        {
            var store = new CNContactStore();
            var keysToFetch = new[] { CNContactKey.GivenName, CNContactKey.FamilyName, CNContactKey.EmailAddresses, CNContactKey.PhoneNumbers };
            var request = new CNContactFetchRequest(keysToFetch);
            var allContacts = store.GetUnifiedContacts(request, out var error);

            if (allContacts == null) return contacts;

            foreach (var contact in allContacts)
            {
                var email = contact.EmailAddresses.FirstOrDefault()?.Value?.ToString();
                var phone = contact.PhoneNumbers.FirstOrDefault()?.Value.StringValue;

                contacts.Add(new Contact(
                    contact.Identifier,
                    contact.GivenName ?? "Unknown",
                    string.IsNullOrEmpty(contact.FamilyName) ? null : contact.FamilyName,
                    email,
                    phone,
                    null));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load contacts");
        }

        return contacts;
    }

    /// <summary>
    /// Searches for contacts matching the provided query string.
    /// </summary>
    /// <param name="query">The search term to match against contact names (first or last name).</param>
    /// <param name="ct">Cancellation token to allow operation cancellation.</param>
    /// <returns>A read-only list of Contact objects matching the query. Returns an empty list if no matches found or on error.</returns>
    /// <exception cref="Exception">Caught internally and logged; method returns empty list instead of throwing.</exception>
    /// <remarks>
    /// Uses the native CNContact.GetPredicateForContactsMatchingName() method to perform name-based search.
    /// Returns results with GivenName, FamilyName, EmailAddresses, and PhoneNumbers populated.
    /// </remarks>
    private partial async Task<IReadOnlyList<Contact>> SearchContactsAsyncPlatform(string query, CancellationToken ct)
    {
        var contacts = new List<Contact>();
        try
        {
            var store = new CNContactStore();
            var keysToFetch = new[] { CNContactKey.GivenName, CNContactKey.FamilyName, CNContactKey.EmailAddresses, CNContactKey.PhoneNumbers };
            var predicate = CNContact.GetPredicateForContactsMatchingName(query);
            var allContacts = store.GetUnifiedContacts(predicate, keysToFetch, out var error);

            if (allContacts == null) return contacts;

            foreach (var contact in allContacts)
            {
                var email = contact.EmailAddresses.FirstOrDefault()?.Value?.ToString();
                var phone = contact.PhoneNumbers.FirstOrDefault()?.Value.StringValue;

                contacts.Add(new Contact(
                    contact.Identifier,
                    contact.GivenName ?? "Unknown",
                    string.IsNullOrEmpty(contact.FamilyName) ? null : contact.FamilyName,
                    email,
                    phone,
                    null));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search contacts");
        }

        return contacts;
    }

    /// <summary>
    /// Opens a contact picker UI to allow user selection (not available on macOS).
    /// </summary>
    /// <param name="ct">Cancellation token to allow operation cancellation.</param>
    /// <returns>Always returns null; contact picker UI is not available on macOS.</returns>
    /// <remarks>
    /// macOS does not provide a built-in contact picker UI through this interface.
    /// Applications requiring contact selection should implement a custom UI or use alternative methods.
    /// A warning is logged when this method is called to inform developers of the limitation.
    /// </remarks>
    private partial Task<Contact?> PickContactAsyncPlatform(CancellationToken ct)
    {
        _logger.LogWarning("Contact picker UI is not available on macOS via this interface");
        return Task.FromResult<Contact?>(null);
    }

    /// <summary>
    /// Checks if the contacts service is available on the macOS platform.
    /// </summary>
    /// <param name="ct">Cancellation token to allow operation cancellation.</param>
    /// <returns>Always returns true; contacts service is always available on macOS.</returns>
    /// <remarks>
    /// The contacts service is always available on macOS systems that have the Contacts framework.
    /// On macOS, users must grant permission for the application to access contacts through system settings.
    /// </remarks>
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) => Task.FromResult(true);
}

#endif
