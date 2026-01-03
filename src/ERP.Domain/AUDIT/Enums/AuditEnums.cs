namespace ERP.Domain.AUDIT.Enums;

/// <summary>
/// Type of audit event
/// </summary>
public enum AuditEventType : byte
{
    /// <summary>
    /// Entity created
    /// </summary>
    Create = 1,

    /// <summary>
    /// Entity updated/modified
    /// </summary>
    Update = 2,

    /// <summary>
    /// Entity deleted
    /// </summary>
    Delete = 3,

    /// <summary>
    /// User login
    /// </summary>
    Login = 10,

    /// <summary>
    /// User logout
    /// </summary>
    Logout = 11,

    /// <summary>
    /// Failed login attempt
    /// </summary>
    LoginFailed = 12,

    /// <summary>
    /// Password changed
    /// </summary>
    PasswordChanged = 13,

    /// <summary>
    /// Financial transaction posted
    /// </summary>
    FinancialPost = 20,

    /// <summary>
    /// Financial transaction reversed
    /// </summary>
    FinancialReverse = 21,

    /// <summary>
    /// Workflow step approved
    /// </summary>
    WorkflowApproved = 30,

    /// <summary>
    /// Workflow step rejected
    /// </summary>
    WorkflowRejected = 31,

    /// <summary>
    /// Permission granted
    /// </summary>
    PermissionGranted = 40,

    /// <summary>
    /// Permission revoked
    /// </summary>
    PermissionRevoked = 41,

    /// <summary>
    /// Permission changed
    /// </summary>
    PermissionChanged = 42,

    /// <summary>
    /// Invoice posted
    /// </summary>
    InvoicePosted = 22,

    /// <summary>
    /// Invoice voided
    /// </summary>
    InvoiceVoided = 23,

    /// <summary>
    /// Payment received
    /// </summary>
    PaymentReceived = 24,

    /// <summary>
    /// Data exported
    /// </summary>
    DataExported = 50,

    /// <summary>
    /// Data imported
    /// </summary>
    DataImported = 51,

    /// <summary>
    /// Data anonymized (GDPR Right to be Forgotten)
    /// </summary>
    DataAnonymized = 52,

    /// <summary>
    /// Configuration changed
    /// </summary>
    ConfigurationChanged = 60,

    /// <summary>
    /// System event
    /// </summary>
    SystemEvent = 99
}

/// <summary>
/// Severity level of audit event
/// </summary>
public enum AuditSeverity : byte
{
    /// <summary>
    /// Informational event
    /// </summary>
    Information = 1,

    /// <summary>
    /// Warning event
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error event
    /// </summary>
    Error = 3,

    /// <summary>
    /// Critical security event
    /// </summary>
    Critical = 4
}
