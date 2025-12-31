namespace ERP.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a user is not authorized to perform an operation.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("You are not authorized to perform this operation.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
