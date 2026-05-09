namespace SmartWorkz.Mobile;

using Android.Content;
using Android.Provider;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#if ANDROID

/// <summary>
/// Android-specific implementation of the contacts service using the Android Contacts Provider API.
/// Queries device contacts from the Android content resolver and maps them to Contact objects.
/// </summary>
/// <remarks>
/// Requires READ_CONTACTS permission in AndroidManifest.xml for runtime access to device contacts.
/// Uses Android.Provider.ContactsContract to query the system contacts database via content resolver.
/// Contact picker (PickContactAsync) returns null on Android as the system picker is not exposed via MAUI.
/// </remarks>
public partial class ContactsService
{
    /// <summary>
    /// Platform-specific implementation to retrieve all contacts from the Android Contacts Provider.
    /// Queries the ContactsContract.Contacts content URI and extracts contact information including email and phone.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of Contact objects representing all device contacts.</returns>
    /// <remarks>
    /// This method requires READ_CONTACTS permission. Email and phone numbers are fetched separately
    /// for each contact using the common data kinds URI. Returns empty list if query fails or permission is denied.
    /// </remarks>
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

            using var cursor = Android.App.Application.Context.ContentResolver?.Query(uri, projection, null, null, null);
            if (cursor == null) return contacts;

            var idIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id);
            var nameIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName);

            if (idIndex < 0 || nameIndex < 0) return contacts;

            while (cursor.MoveToNext())
            {
                var id = cursor.GetString(idIndex) ?? string.Empty;
                var name = cursor.GetString(nameIndex) ?? "Unknown";

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

    /// <summary>
    /// Platform-specific implementation to search device contacts by display name.
    /// Uses a LIKE query pattern on the DisplayName column of the Contacts Provider.
    /// </summary>
    /// <param name="query">The search query string to match against contact display names.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of Contact objects matching the search query.</returns>
    /// <remarks>
    /// Search is case-insensitive and uses SQL LIKE pattern matching (e.g., "%query%").
    /// Requires READ_CONTACTS permission. Returns empty list if no matches found or permission is denied.
    /// </remarks>
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

            using var cursor = Android.App.Application.Context.ContentResolver?.Query(uri, projection, selection, selectionArgs, null);
            if (cursor == null) return contacts;

            var idIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id);
            var nameIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName);

            if (idIndex < 0 || nameIndex < 0) return contacts;

            while (cursor.MoveToNext())
            {
                var id = cursor.GetString(idIndex) ?? string.Empty;
                var name = cursor.GetString(nameIndex) ?? "Unknown";

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

    /// <summary>
    /// Platform-specific implementation of contact picker for Android.
    /// Note: Contact picker is not available via Android ContentResolver API in MAUI.
    /// </summary>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>Always returns null, as contact picker is not supported on Android.</returns>
    /// <remarks>
    /// Android does not expose a system contact picker through the MAUI framework.
    /// Users can use the custom UI to display contacts and have them select one from the list.
    /// </remarks>
    private partial Task<Contact?> PickContactAsyncPlatform(CancellationToken ct)
    {
        _logger.LogWarning("Contact picker is not available on Android");
        return Task.FromResult<Contact?>(null);
    }

    /// <summary>
    /// Platform-specific availability check for Android.
    /// </summary>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>Always returns true if Android API level supports Contacts Provider.</returns>
    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) => Task.FromResult(true);

    private static (string firstName, string? lastName) ParseName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return ("Unknown", null);
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? (parts[0], string.Join(" ", parts.Skip(1))) : (fullName, null);
    }

    private static string? GetContactEmail(string contactId)
    {
        if (string.IsNullOrEmpty(contactId)) return null;

        try
        {
            var uri = ContactsContract.CommonDataKinds.Email.ContentUri;
            var projection = new[] { ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data };
            var selection = $"{ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId} = ?";
            var selectionArgs = new[] { contactId };

            using var cursor = Android.App.Application.Context.ContentResolver?.Query(uri, projection, selection, selectionArgs, null);
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
        if (string.IsNullOrEmpty(contactId)) return null;

        try
        {
            var uri = ContactsContract.CommonDataKinds.Phone.ContentUri;
            var projection = new[] { ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Data };
            var selection = $"{ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId} = ?";
            var selectionArgs = new[] { contactId };

            using var cursor = Android.App.Application.Context.ContentResolver?.Query(uri, projection, selection, selectionArgs, null);
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
