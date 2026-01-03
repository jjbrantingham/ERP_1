# Generate API Documentation Skill

## Purpose
Generate comprehensive API documentation using Swagger/OpenAPI.

## Configure Swagger

```csharp
// In Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERP SaaS API",
        Version = "v1",
        Description = "Enterprise Resource Planning API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

## XML Comments Example

```csharp
/// <summary>
/// Creates a new invoice
/// </summary>
/// <param name="command">Invoice creation details</param>
/// <returns>The ID of the created invoice</returns>
/// <response code="201">Invoice created successfully</response>
/// <response code="400">Invalid request data</response>
[HttpPost]
[ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand command)
{
    var id = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id }, id);
}
```

## Related Skills
- create-api-endpoint
- update-readme
