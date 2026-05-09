namespace SmartWorkz.Mobile;

using Contacts;
using Foundation;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#if IOS && !__MACCATALYST__

/// <summary>
/// iOS-specific implementation of the contacts service using the Contacts framework (iOS 9+).
/// Provides unified access to device contacts via CNContactStore with first and last name, email, and phone.
/// </summary>
/// <remarks>
/// Requires Contacts permission in Info.plist (NSContactsUsageDescription) for iOS 10+.
/// Uses CNContactStore.GetUnifiedContacts to retrieve all contacts with specified key properties.
/// Returns null for contact picker as UIContactPickerViewController is not exposed via MAUI.
/// </remarks>
public partial class ContactsService
{
    /// <summary>
    /// Platform-specific implementation to retrieve all contacts from the iOS Contacts framework.
    /// Uses CNContactStore to fetch unified contacts with email and phone information.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of Contact objects representing all device contacts.</returns>
    /// <remarks>
    /// This method requires Contacts permission. Uses unified contacts which merge duplicate contacts across accounts.
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
            NSError error = null;
            var allContacts = store.GetUnifiedContacts(null, keysToFetch, out error);

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
    /// Platform-specific implementation to search device contacts by first or last name.
    /// Fetches all contacts and filters locally using case-insensitive string matching.
    /// </summary>
    /// <param name="query">The search query to match against contact first or last names.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of Contact objects matching the search query.</returns>
    /// <remarks>
    /// Search is case-insensitive using OrdinalIgnoreCase comparison.
    /// Requires Contacts permission. Note: iOS Contacts framework does not expose a server-side search API,
    /// so all contacts are fetched and filtered locally.
    /// Returns empty list if no matches found or permission is denied.
    /// </remarks>
    private partial async Task<IReadOnlyList<Contact>> SearchContactsAsyncPlatform(string query, CancellationToken ct)
    {
        var contacts = new List<Contact>();
        try
        {
            var store = new CNContactStore();
            var keysToFetch = new[] { CNContactKey.GivenName, CNContactKey.FamilyName, CNContactKey.EmailAddresses, CNContactKey.PhoneNumbers };
            var request = new CNContactFetchRequest(keysToFetch);
            NSError error = null;
            var allContacts = store.GetUnifiedContacts(null, keysToFetch, out error);

            if (allContacts == null) return contacts;

            // Filter locally since CNContact.GetPredicateForContactsMatchingName is not available
            var queryLower = query.ToLowerInvariant();
            var filtered = allContacts.Where(c =>
                (c.GivenName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.FamilyName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));

            foreach (var contact in filtered)
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
    /// Platform-specific implementation of contact picker for iOS.
    /// Note: UIContactPickerViewController is not available via MAUI framework.
    /// </summary>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>Always returns null, as native contact picker is not accessible via MAUI.</returns>
    /// <remarks>
    /// iOS provides UIContactPickerViewController for native contact picking, but it is not exposed
    /// through the MAUI framework. Alternative: Display contacts list in custom UI and let users select.
    /// </remarks>
    private partial Task<Contact?> PickContactAsyncPlatform(CancellationToken ct)
    {
        _logger.LogWarning("Contact picker UI is not available on iOS via this interface");
        return Task.FromResult<Contact?>(null);
    }

    /// <summary>
    /// Platform-specific availability check for iOS.
    /// </summary>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>Always returns true if running on iOS 9 or later.</returns>
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) => Task.FromResult(true);
}

#endif
