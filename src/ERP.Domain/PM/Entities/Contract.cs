using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Enums;

namespace ERP.Domain.PM.Entities;

/// <summary>
/// Contract for a project.
/// </summary>
public class Contract : AggregateRoot
{
    public long ProjectId { get; private set; }
    public string ContractNumber { get; private set; }
    public ContractType ContractType { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public Money? ContractValue { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime? SignedDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? Terms { get; private set; }

    // Navigation
    public Project? Project { get; private set; }

    private Contract()
    {
        ContractNumber = string.Empty;
    }

    public static Contract Create(
        Guid tenantId,
        long projectId,
        string contractNumber,
        ContractType contractType,
        DateTime startDate,
        DateTime endDate,
        string? title = null,
        string? description = null,
        Money? contractValue = null,
        DateTime? signedDate = null,
        string? terms = null)
    {
        return new Contract
        {
            TenantId = tenantId,
            ProjectId = projectId,
            ContractNumber = contractNumber.Trim(),
            ContractType = contractType,
            Title = title?.Trim(),
            Description = description?.Trim(),
            ContractValue = contractValue,
            StartDate = startDate,
            EndDate = endDate,
            SignedDate = signedDate,
            Terms = terms?.Trim(),
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
    }
}
