namespace SmartWorkz.Shared;

/// <summary>
/// Shared configuration constants used throughout SmartWorkz.Shared.
/// Enables centralized management of default values and limits.
/// </summary>
public static class SharedConstants
{
    public static class Pagination
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int MinPageSize = 1;
        public const int DefaultPage = 1;
    }

    public static class Validation
    {
        public const int MinNameLength = 1;
        public const int MaxNameLength = 255;
        public const int MinEmailLength = 5;
        public const int MaxEmailLength = 254;
        public const int MinPhoneLength = 10;
        public const int MaxPhoneLength = 20;
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MaxCommentLength = 5000;
        public const int MaxDescriptionLength = 2000;
        public const int MaxAddressLength = 500;
    }

    public static class Security
    {
        public const int PasswordHashIterations = 10000;
        public const int EncryptionKeyLength = 32; // 256 bits
        public const int SaltLength = 16; // 128 bits
    }

    public static class Formatting
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string TimeFormat = "HH:mm:ss";
        public const string CurrencyFormat = "C2";
        public const string PercentFormat = "P2";
    }

    public static class Caching
    {
        public const string KeyPrefix = "sw:";
        public const int DefaultDurationMinutes = 30;
        public const int ShortDurationMinutes = 5;
        public const int LongDurationMinutes = 120;
    }

    public static class Messages
    {
        public const string Success = "Operation completed successfully.";
        public const string Error = "An error occurred while processing your request.";
        public const string NotFound = "The requested resource was not found.";
        public const string Unauthorized = "You are not authorized to perform this action.";
        public const string ValidationFailed = "One or more validation errors occurred.";
        public const string OperationCancelled = "The operation was cancelled.";
        public const string DuplicateFound = "A record with this value already exists.";
    }
}
