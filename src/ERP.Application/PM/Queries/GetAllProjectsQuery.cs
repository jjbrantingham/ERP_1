namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get all projects.
/// </summary>
public class GetAllProjectsQuery
{
    public bool ActiveOnly { get; set; } = true;
}
