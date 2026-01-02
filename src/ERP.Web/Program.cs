using ERP.Application.Common.Interfaces;
using ERP.Application.BILL.Commands;
using ERP.Domain.CRM.Repositories;
using FluentValidation;
using ERP.Domain.HR.Repositories;
using ERP.Domain.PM.Repositories;
using ERP.Domain.TE.Repositories;
using ERP.Domain.VM.Repositories;
using ERP.Domain.FIN.Repositories;
using ERP.Domain.BILL.Repositories;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Persistence.Repositories;
using ERP.Infrastructure.Services;
using ERP.Web.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Configure CORS - Secure configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        // Get allowed origins from configuration
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:5173" }; // Default for dev

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for authentication cookies/tokens
    });
});

// Configure JWT Authentication (when package is available)
/* Uncomment when Microsoft.AspNetCore.Authentication.JwtBearer package is installed
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
*/

builder.Services.AddAuthorization(options =>
{
    // Add custom authorization policies here
    // Example: options.AddPolicy("AdminOnly", policy => policy.RequireRole("System Administrator"));
});

// Add controllers and API explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger/OpenAPI (when package is available)
// builder.Services.AddSwaggerGen();

// Add HttpContextAccessor for tenant and user resolution
builder.Services.AddHttpContextAccessor();

// Register multi-tenancy and user services
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Register DbContext
builder.Services.AddDbContext<ERPDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    // Use SQL Server
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });

    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// HR repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IResourceTypeRepository, ResourceTypeRepository>();
builder.Services.AddScoped<IRateRepository, RateRepository>();

// CRM repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();

// PM repositories
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IWBSItemRepository, WBSItemRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();

// VM repositories
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorContactRepository, VendorContactRepository>();
builder.Services.AddScoped<IVendorNoteRepository, VendorNoteRepository>();

// TE repositories
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<IExpenseReportRepository, ExpenseReportRepository>();

// FIN repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

// BILL repositories
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Register authentication services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Register MediatR with auto-discovery from Application assembly
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ERP.Application.BILL.Commands.CreateInvoiceCommand).Assembly);
});

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssembly(typeof(ERP.Application.BILL.Commands.CreateInvoiceCommand).Assembly);

// Add logging (Serilog when package is available)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ERPDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline

// Use global exception handling middleware
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    // Enable Swagger in development (when package is available)
    // app.UseSwagger();
    // app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Use CORS with secure policy
app.UseCors("AllowedOrigins");

// Use tenant resolution middleware
app.UseTenantResolution();

// Use authentication and authorization (will configure later)
// app.UseAuthentication();
// app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

// Default route for testing
app.MapGet("/", () => Results.Ok(new
{
    Application = "ERP SaaS Application",
    Version = "1.0.0",
    Status = "Running",
    Environment = app.Environment.EnvironmentName
}));

// Initialize database (run migrations and seed data)
// Set AUTO_MIGRATE=true in environment variables to enable
if (app.Configuration.GetValue<bool>("AutoMigrate", false) ||
    Environment.GetEnvironmentVariable("AUTO_MIGRATE") == "true")
{
    app.Logger.LogInformation("Auto-migration enabled, initializing database...");
    await app.InitializeDatabaseAsync();
}

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }
