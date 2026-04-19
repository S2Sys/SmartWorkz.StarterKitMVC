namespace SmartWorkz.Core.Constants;
public static class AppConstants
{
    public const string DefaultCulture = "en-US";
    public const string DefaultTimeZone = "UTC";
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;
    public const int MinPageSize = 1;

    public static class Cache
    {
        public const string KeyPrefix = "smartworkz:";
        public const int DefaultDurationMinutes = 30;
        public const int LongDurationHours = 24;
        public const int ShortDurationMinutes = 5;
    }

    public static class Validation
    {
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MinNameLength = 1;
        public const int MaxNameLength = 256;
        public const int MaxEmailLength = 256;
        public const int MaxPhoneLength = 20;
    }

    public static class Messages
    {
        public const string Required = "This field is required";
        public const string InvalidFormat = "Invalid format";
        public const string NotFound = "Resource not found";
        public const string Unauthorized = "Unauthorized access";
    }
}
