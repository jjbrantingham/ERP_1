namespace ERP.Shared.Resources;

/// <summary>
/// Centralized error messages for the ERP application.
/// Provides consistent error messaging across all layers.
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// Common validation error messages
    /// </summary>
    public static class Validation
    {
        public const string Required = "{0} is required";
        public const string MaxLength = "{0} cannot exceed {1} characters";
        public const string MinLength = "{0} must be at least {1} characters";
        public const string MustBePositive = "{0} must be greater than zero";
        public const string MustBeNonNegative = "{0} must be greater than or equal to zero";
        public const string InvalidFormat = "{0} has an invalid format";
        public const string InvalidRange = "{0} must be between {1} and {2}";
        public const string InvalidEmail = "Email address is invalid";
        public const string InvalidPhone = "Phone number is invalid";
        public const string InvalidCurrency = "Currency must be a 3-letter ISO 4217 code";
        public const string InvalidDate = "Date is invalid";
        public const string FutureDateNotAllowed = "{0} cannot be in the future";
        public const string PastDateNotAllowed = "{0} cannot be in the past";
    }

    /// <summary>
    /// Entity not found error messages
    /// </summary>
    public static class NotFound
    {
        public const string Entity = "{0} with ID {1} not found";
        public const string Project = "Project with ID {0} not found";
        public const string Client = "Client with ID {0} not found";
        public const string Employee = "Employee with ID {0} not found";
        public const string Invoice = "Invoice with ID {0} not found";
        public const string Account = "Account with ID {0} not found";
        public const string Timesheet = "Timesheet with ID {0} not found";
        public const string ExpenseReport = "Expense report with ID {0} not found";
        public const string Vendor = "Vendor with ID {0} not found";
        public const string JournalEntry = "Journal entry with ID {0} not found";
        public const string User = "User with ID {0} not found";
        public const string Tenant = "Tenant with ID {0} not found";
    }

    /// <summary>
    /// Authorization and security error messages
    /// </summary>
    public static class Security
    {
        public const string Unauthorized = "You are not authorized to perform this action";
        public const string Forbidden = "Access to this resource is forbidden";
        public const string InvalidCredentials = "Invalid username or password";
        public const string AccountLocked = "Account is locked due to multiple failed login attempts";
        public const string SessionExpired = "Your session has expired. Please log in again";
        public const string InvalidToken = "Invalid or expired token";
        public const string InsufficientPermissions = "You do not have sufficient permissions to {0}";
        public const string TenantMismatch = "Resource does not belong to your organization";
    }

    /// <summary>
    /// Business rule violation error messages
    /// </summary>
    public static class BusinessRules
    {
        // Invoice errors
        public const string CannotModifyPostedInvoice = "Cannot modify an invoice that has been posted";
        public const string CannotModifyNonDraftInvoice = "Cannot modify non-draft invoices";
        public const string InvoiceMustHaveLineItems = "Invoice must have at least one line item";
        public const string InvoiceAlreadyPaid = "Invoice has already been paid in full";
        public const string PaymentExceedsBalance = "Payment amount exceeds outstanding balance";

        // Journal entry errors
        public const string JournalEntryNotBalanced = "Journal entry debits and credits must be equal";
        public const string CannotModifyPostedJournalEntry = "Cannot modify a posted journal entry";
        public const string JournalEntryMustHaveTwoLines = "At least two journal entry lines are required (debit and credit)";
        public const string DebitOrCreditRequired = "Each line must have either a debit or a credit amount, but not both";

        // Timesheet errors
        public const string CannotModifySubmittedTimesheet = "Cannot modify a timesheet that has been submitted";
        public const string CannotModifyApprovedTimesheet = "Cannot modify an approved timesheet";
        public const string TimesheetDateOverlap = "Timesheet dates overlap with an existing timesheet";
        public const string InvalidWorkHours = "Work hours must be between 0 and 24";

        // Expense errors
        public const string CannotModifySubmittedExpenseReport = "Cannot modify an expense report that has been submitted";
        public const string CannotModifyApprovedExpenseReport = "Cannot modify an approved expense report";
        public const string ExpenseMustHaveReceipt = "Expense amount exceeds threshold and requires a receipt";

        // Project errors
        public const string ProjectAlreadyClosed = "Project is already closed";
        public const string CannotDeleteProjectWithTransactions = "Cannot delete project with associated transactions";
        public const string ProjectBudgetExceeded = "Operation would exceed project budget";

        // Currency errors
        public const string CurrencyMismatch = "Cannot {0} money with different currencies: {1} and {2}";
        public const string UnsupportedCurrency = "Currency {0} is not supported";

        // General business rules
        public const string DuplicateEntry = "{0} already exists";
        public const string ReferentialIntegrity = "Cannot delete {0} because it is referenced by {1}";
        public const string InvalidStateTransition = "Cannot change {0} from {1} to {2}";
        public const string ConcurrencyConflict = "The record has been modified by another user. Please refresh and try again";
    }

    /// <summary>
    /// Technical and system error messages
    /// </summary>
    public static class Technical
    {
        public const string DatabaseError = "A database error occurred. Please try again later";
        public const string NetworkError = "A network error occurred. Please check your connection";
        public const string UnexpectedError = "An unexpected error occurred. Please contact support";
        public const string ServiceUnavailable = "The service is temporarily unavailable. Please try again later";
        public const string TimeoutError = "The operation timed out. Please try again";
        public const string FileUploadError = "File upload failed. Please try again";
        public const string FileTooBig = "File size exceeds maximum allowed size of {0} MB";
        public const string InvalidFileType = "File type {0} is not allowed";
    }

    /// <summary>
    /// Multi-tenancy error messages
    /// </summary>
    public static class MultiTenancy
    {
        public const string TenantNotFound = "Organization not found";
        public const string TenantIdMissing = "Organization ID is required";
        public const string TenantInactive = "Organization is inactive";
        public const string CrossTenantAccess = "Cannot access resources from another organization";
    }

    /// <summary>
    /// Workflow error messages
    /// </summary>
    public static class Workflow
    {
        public const string WorkflowNotFound = "Workflow definition not found";
        public const string InvalidWorkflowState = "Invalid workflow state: {0}";
        public const string CannotCompleteStep = "Cannot complete workflow step: {0}";
        public const string MissingApproval = "Approval is required before proceeding";
        public const string AlreadyApproved = "This item has already been approved";
        public const string AlreadyRejected = "This item has already been rejected";
    }

    /// <summary>
    /// Helper methods for formatting error messages
    /// </summary>
    public static class Formatters
    {
        /// <summary>
        /// Formats an error message with a single parameter
        /// </summary>
        public static string Format(string message, object arg0)
        {
            return string.Format(message, arg0);
        }

        /// <summary>
        /// Formats an error message with two parameters
        /// </summary>
        public static string Format(string message, object arg0, object arg1)
        {
            return string.Format(message, arg0, arg1);
        }

        /// <summary>
        /// Formats an error message with three parameters
        /// </summary>
        public static string Format(string message, object arg0, object arg1, object arg2)
        {
            return string.Format(message, arg0, arg1, arg2);
        }

        /// <summary>
        /// Formats an error message with multiple parameters
        /// </summary>
        public static string Format(string message, params object[] args)
        {
            return string.Format(message, args);
        }
    }
}
