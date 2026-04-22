namespace SmartWorkz.Mobile;

using Contacts;
using Foundation;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#if __MACCATALYST__

public partial class ContactsService
{
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

            // Filter locally
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

    private partial Task<Contact?> PickContactAsyncPlatform(CancellationToken ct)
    {
        _logger.LogWarning("Contact picker UI is not available on Mac Catalyst via this interface");
        return Task.FromResult<Contact?>(null);
    }

    private partial Task<bool> IsAvailableAsyncPlatform() => Task.FromResult(true);
}

#endif
