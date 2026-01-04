namespace ERP.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access a resource they don't have permission to access.
/// This is typically used for authorization failures (403 Forbidden).
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("Access to this resource is forbidden")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }

    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
