using MediatR;

namespace ERP.Application.AUDIT.Commands;

/// <summary>
/// Command to export all user data (GDPR Right to Data Portability).
/// </summary>
public class ExportUserDataCommand : IRequest<string>
{
    public long UserId { get; set; }
}
