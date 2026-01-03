using MediatR;

namespace ERP.Application.AUDIT.Commands;

/// <summary>
/// Command to anonymize user data (GDPR Right to be Forgotten).
/// Note: This anonymizes rather than deletes to maintain referential integrity and audit trails.
/// </summary>
public class AnonymizeUserDataCommand : IRequest<bool>
{
    public long UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
