namespace ERP.Shared.Constants;

/// <summary>
/// Business constants used throughout the ERP application
/// </summary>
public static class BusinessConstants
{
    /// <summary>
    /// Financial and Currency Constants
    /// </summary>
    public static class Currency
    {
        /// <summary>
        /// Default currency code (US Dollar)
        /// </summary>
        public const string DefaultCurrency = "USD";

        /// <summary>
        /// Set of supported ISO 4217 currency codes
        /// </summary>
        public static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
        {
            "USD", // US Dollar
            "EUR", // Euro
            "GBP", // British Pound
            "CAD", // Canadian Dollar
            "AUD", // Australian Dollar
            "JPY", // Japanese Yen
            "CHF", // Swiss Franc
            "CNY"  // Chinese Yuan
        };

        /// <summary>
        /// ISO 4217 currency code length
        /// </summary>
        public const int CurrencyCodeLength = 3;

        /// <summary>
        /// Standard decimal precision for monetary amounts
        /// </summary>
        public const int MoneyPrecision = 18;

        /// <summary>
        /// Standard decimal scale for monetary amounts
        /// </summary>
        public const int MoneyScale = 2;
    }

    /// <summary>
    /// String length constraints
    /// </summary>
    public static class Lengths
    {
        /// <summary>
        /// Maximum length for short names (e.g., first name, last name)
        /// </summary>
        public const int ShortName = 100;

        /// <summary>
        /// Maximum length for standard names (e.g., project name, client name)
        /// </summary>
        public const int StandardName = 200;

        /// <summary>
        /// Maximum length for descriptions (e.g., line item descriptions, notes)
        /// </summary>
        public const int Description = 500;

        /// <summary>
        /// Maximum length for long descriptions (e.g., project descriptions, notes)
        /// </summary>
        public const int LongDescription = 4000;

        /// <summary>
        /// Maximum length for email addresses
        /// </summary>
        public const int Email = 256;

        /// <summary>
        /// Maximum length for phone numbers
        /// </summary>
        public const int Phone = 20;

        /// <summary>
        /// Maximum length for postal/zip codes
        /// </summary>
        public const int PostalCode = 10;

        /// <summary>
        /// Maximum length for reference numbers (e.g., PO number, invoice reference)
        /// </summary>
        public const int ReferenceNumber = 100;

        /// <summary>
        /// Maximum length for account codes
        /// </summary>
        public const int AccountCode = 20;
    }

    /// <summary>
    /// Business limits and constraints
    /// </summary>
    public static class Limits
    {
        /// <summary>
        /// Maximum number of line items allowed in an invoice
        /// </summary>
        public const int MaxInvoiceLineItems = 1000;

        /// <summary>
        /// Maximum number of timesheet entries allowed in a single timesheet
        /// </summary>
        public const int MaxTimesheetEntries = 500;

        /// <summary>
        /// Maximum number of expense items allowed in an expense report
        /// </summary>
        public const int MaxExpenseItems = 200;

        /// <summary>
        /// Maximum number of journal entry lines allowed
        /// </summary>
        public const int MaxJournalEntryLines = 1000;

        /// <summary>
        /// Maximum discount percentage allowed (0-100)
        /// </summary>
        public const decimal MaxDiscountPercent = 100m;

        /// <summary>
        /// Minimum discount percentage allowed
        /// </summary>
        public const decimal MinDiscountPercent = 0m;
    }

    /// <summary>
    /// Date and time constants
    /// </summary>
    public static class DateTime
    {
        /// <summary>
        /// Maximum number of days in the future for entry dates
        /// </summary>
        public const int MaxFutureDays = 1;

        /// <summary>
        /// Maximum number of years in the past for historical data
        /// </summary>
        public const int MaxPastYears = 10;

        /// <summary>
        /// Default fiscal year end month (December)
        /// </summary>
        public const int FiscalYearEndMonth = 12;

        /// <summary>
        /// Default fiscal year end day
        /// </summary>
        public const int FiscalYearEndDay = 31;
    }

    /// <summary>
    /// Security and authentication constants
    /// </summary>
    public static class Security
    {
        /// <summary>
        /// Minimum password length
        /// </summary>
        public const int MinPasswordLength = 8;

        /// <summary>
        /// Maximum password length
        /// </summary>
        public const int MaxPasswordLength = 128;

        /// <summary>
        /// JWT token expiration time in hours
        /// </summary>
        public const int JwtExpirationHours = 24;

        /// <summary>
        /// Refresh token expiration time in days
        /// </summary>
        public const int RefreshTokenExpirationDays = 30;

        /// <summary>
        /// Maximum failed login attempts before account lockout
        /// </summary>
        public const int MaxFailedLoginAttempts = 5;

        /// <summary>
        /// Account lockout duration in minutes
        /// </summary>
        public const int LockoutDurationMinutes = 15;
    }

    /// <summary>
    /// Pagination constants
    /// </summary>
    public static class Pagination
    {
        /// <summary>
        /// Default page size for paginated results
        /// </summary>
        public const int DefaultPageSize = 20;

        /// <summary>
        /// Maximum page size allowed
        /// </summary>
        public const int MaxPageSize = 100;

        /// <summary>
        /// Minimum page size
        /// </summary>
        public const int MinPageSize = 1;
    }

    /// <summary>
    /// Number formats and patterns
    /// </summary>
    public static class Formats
    {
        /// <summary>
        /// Invoice number format pattern
        /// </summary>
        public const string InvoiceNumberFormat = "INV-{0:yyyyMM}-{1:D6}";

        /// <summary>
        /// Journal entry number format pattern
        /// </summary>
        public const string JournalEntryNumberFormat = "JE-{0:yyyyMMdd}-{1:D6}";

        /// <summary>
        /// Project number format pattern
        /// </summary>
        public const string ProjectNumberFormat = "PRJ-{0:yyyyMMdd}-{1}";

        /// <summary>
        /// Date format for display (ISO 8601)
        /// </summary>
        public const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Date and time format for display (ISO 8601)
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
    }
}
