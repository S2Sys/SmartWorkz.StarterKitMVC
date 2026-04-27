namespace SmartWorkz.Mobile;

using Contacts;
using Foundation;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#if MACOS

/// <summary>
/// macOS-specific implementation of the contacts service using the Contacts framework.
/// Provides access to system address book contacts via CNContactStore with full search capabilities.
/// </summary>
/// <remarks>
/// Requires Contacts permission in Info.plist (NSContactsUsageDescription) for macOS 10.11+.
/// Supports server-side filtering via CNContact.GetPredicateForContactsMatchingName on macOS.
/// Contact picker is not available through MAUI but can be shown via native UI.
/// </remarks>
public partial class ContactsService
{
    /// <summary>
    /// Platform-specific implementation to retrieve all contacts from the macOS Contacts framework.
    /// Uses CNContactStore with the Contacts Fetch Request API to retrieve unified contacts.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of Contact objects representing all device contacts.</returns>
    /// <remarks>
    /// This method requires Contacts permission. Uses unified contacts which merge duplicates.
    /// Returns empty list if user denies permission or an error occurs.
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
    /// Platform-specific implementation to search device contacts by name using macOS predicates.
    /// Uses CNContact.GetPredicateForContactsMatchingName for server-side filtering.
    /// </summary>
    /// <param name="query">The search query string to match against contact names.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of Contact objects matching the search predicate.</returns>
    /// <remarks>
    /// macOS Contacts framework provides native search via GetPredicateForContactsMatchingName.
    /// Search is case-insensitive and matches against both first and last names.
    /// Requires Contacts permission. Returns empty list if no matches or permission is denied.
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
    /// Platform-specific implementation of contact picker for macOS.
    /// Note: ABPeoplePickerNavigationController is deprecated and not available via MAUI.
    /// </summary>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>Always returns null, as native contact picker is not accessible via MAUI.</returns>
    /// <remarks>
    /// macOS does not provide a modern contact picker UI through the MAUI framework.
    /// Alternative: Display contacts in custom UI for user selection.
    /// </remarks>
    private partial Task<Contact?> PickContactAsyncPlatform(CancellationToken ct)
    {
        _logger.LogWarning("Contact picker UI is not available on macOS via this interface");
        return Task.FromResult<Contact?>(null);
    }

    /// <summary>
    /// Platform-specific availability check for macOS.
    /// </summary>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>Always returns true if running on macOS 10.11 or later with Contacts framework.</returns>
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) => Task.FromResult(true);
}

#endif
