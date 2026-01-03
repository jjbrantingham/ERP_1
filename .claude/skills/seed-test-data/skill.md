# Seed Test Data Skill

## Purpose
Generate realistic test data for development and testing environments.

## Data Seeder Example

```csharp
public class TestDataSeeder
{
    private readonly ERPDbContext _context;

    public async Task SeedAsync()
    {
        var tenantId = Guid.NewGuid();

        // Seed clients
        var clients = CreateClients(tenantId, count: 10);
        _context.Clients.AddRange(clients);
        await _context.SaveChangesAsync();

        // Seed projects
        var projects = CreateProjects(tenantId, clients, count: 20);
        _context.Projects.AddRange(projects);
        await _context.SaveChangesAsync();

        // Seed employees
        var employees = CreateEmployees(tenantId, count: 50);
        _context.Employees.AddRange(employees);
        await _context.SaveChangesAsync();

        // Seed timesheets
        var timesheets = CreateTimesheets(tenantId, employees, projects, count: 100);
        _context.Timesheets.AddRange(timesheets);
        await _context.SaveChangesAsync();
    }

    private List<Client> CreateClients(Guid tenantId, int count)
    {
        var faker = new Faker<Client>()
            .RuleFor(c => c.TenantId, tenantId)
            .RuleFor(c => c.CompanyName, f => f.Company.CompanyName())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber());

        return faker.Generate(count);
    }
}
```

## Related Skills
- add-migration
