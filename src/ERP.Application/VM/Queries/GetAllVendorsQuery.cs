namespace ERP.Application.VM.Queries;

/// <summary>
/// Query to get all vendors.
/// </summary>
public class GetAllVendorsQuery
{
    public bool ActiveOnly { get; set; } = true;
}
