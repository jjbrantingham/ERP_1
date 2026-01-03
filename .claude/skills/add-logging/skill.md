# Add Logging Skill

## Purpose
Add comprehensive structured logging to the application.

## Logging Patterns

```csharp
public class InvoiceService
{
    private readonly ILogger<InvoiceService> _logger;

    public async Task<long> CreateInvoiceAsync(CreateInvoiceCommand command)
    {
        _logger.LogInformation("Creating invoice for project {ProjectId}, client {ClientId}",
            command.ProjectId, command.ClientId);

        try
        {
            var invoice = await GenerateInvoice(command);

            _logger.LogInformation("Invoice {InvoiceId} created successfully with number {InvoiceNumber}",
                invoice.Id, invoice.InvoiceNumber);

            return invoice.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for project {ProjectId}", command.ProjectId);
            throw;
        }
    }
}
```

## Log Levels

- **Trace**: Very detailed (development only)
- **Debug**: Diagnostic information
- **Information**: General flow
- **Warning**: Unexpected but handled
- **Error**: Errors and exceptions
- **Critical**: Critical failures

## Related Skills
- analyze-logs
- add-health-checks
