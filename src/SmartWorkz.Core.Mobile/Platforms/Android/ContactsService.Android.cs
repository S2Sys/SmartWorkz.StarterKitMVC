namespace SmartWorkz.Mobile;

using Android.Content;
using Android.Provider;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#if ANDROID

public partial class ContactsService
{
    private partial async Task<IReadOnlyList<Contact>> GetAllContactsAsyncPlatform(CancellationToken ct)
    {
        var contacts = new List<Contact>();
        try
        {
            var uri = ContactsContract.Contacts.ContentUri;
            var projection = new[]
            {
                ContactsContract.Contacts.InterfaceConsts.Id,
                ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber
            };

            using var cursor = Android.App.Application.Context.ContentResolver.Query(uri, projection, null, null, null);
            if (cursor == null) return contacts;

            var idIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id);
            var nameIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName);

            while (cursor.MoveToNext())
            {
                var id = cursor.GetString(idIndex);
                var name = cursor.GetString(nameIndex);

                var (firstName, lastName) = ParseName(name);
                var email = GetContactEmail(id);
                var phone = GetContactPhone(id);

                contacts.Add(new Contact(id, firstName, lastName, email, phone, null));
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
            var uri = ContactsContract.Contacts.ContentUri;
            var projection = new[]
            {
                ContactsContract.Contacts.InterfaceConsts.Id,
                ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber
            };
            var selection = $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} LIKE ?";
            var selectionArgs = new[] { $"%{query}%" };

            using var cursor = Android.App.Application.Context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
            if (cursor == null) return contacts;

            var idIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id);
            var nameIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName);

            while (cursor.MoveToNext())
            {
                var id = cursor.GetString(idIndex);
                var name = cursor.GetString(nameIndex);

                var (firstName, lastName) = ParseName(name);
                var email = GetContactEmail(id);
                var phone = GetContactPhone(id);

                contacts.Add(new Contact(id, firstName, lastName, email, phone, null));
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
        _logger.LogWarning("Contact picker is not available on Android");
        return Task.FromResult<Contact?>(null);
    }

    private partial Task<bool> IsAvailableAsyncPlatform() => Task.FromResult(true);

    private static (string firstName, string? lastName) ParseName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return ("Unknown", null);
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? (parts[0], string.Join(" ", parts.Skip(1))) : (fullName, null);
    }

    private static string? GetContactEmail(string contactId)
    {
        try
        {
            var uri = ContactsContract.CommonDataKinds.Email.ContentUri;
            var projection = new[] { ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data };
            var selection = $"{ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId} = ?";
            var selectionArgs = new[] { contactId };

            using var cursor = Android.App.Application.Context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
            if (cursor?.MoveToFirst() == true)
                return cursor.GetString(0);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting email: {ex}");
        }

        return null;
    }

    private static string? GetContactPhone(string contactId)
    {
        try
        {
            var uri = ContactsContract.CommonDataKinds.Phone.ContentUri;
            var projection = new[] { ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Data };
            var selection = $"{ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId} = ?";
            var selectionArgs = new[] { contactId };

            using var cursor = Android.App.Application.Context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
            if (cursor?.MoveToFirst() == true)
                return cursor.GetString(0);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting phone: {ex}");
        }

        return null;
    }
}

#endif
