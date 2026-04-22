namespace SmartWorkz.Mobile;

using Contacts;
using Foundation;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#if MACOS

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

    private partial Task<Contact?> PickContactAsyncPlatform(CancellationToken ct)
    {
        _logger.LogWarning("Contact picker UI is not available on macOS via this interface");
        return Task.FromResult<Contact?>(null);
    }

    private partial Task<bool> IsAvailableAsyncPlatform() => Task.FromResult(true);
}

#endif
