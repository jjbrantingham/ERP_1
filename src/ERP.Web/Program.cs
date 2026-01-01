using ERP.Application.Common.Interfaces;
using ERP.Application.CRM.Commands;
using ERP.Application.CRM.Queries;
using ERP.Application.HR.Commands;
using ERP.Application.HR.Queries;
using ERP.Application.Identity.Commands;
using ERP.Application.PM.Commands;
using ERP.Application.PM.Queries;
using ERP.Application.TE.Commands;
using ERP.Application.TE.Queries;
using ERP.Application.VM.Commands;
using ERP.Application.VM.Queries;
using ERP.Application.FIN.Commands;
using ERP.Application.FIN.Queries;
using ERP.Application.BILL.Commands;
using ERP.Application.BILL.Queries;
using ERP.Application.RPT.Queries;
using ERP.Application.DASH.Queries;
using ERP.Domain.CRM.Repositories;
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

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

// Add HttpContextAccessor for tenant resolution
builder.Services.AddHttpContextAccessor();

// Register multi-tenancy service
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

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

// Register command handlers
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<RefreshTokenCommandHandler>();
builder.Services.AddScoped<ChangePasswordCommandHandler>();

// HR command handlers
builder.Services.AddScoped<CreateEmployeeCommandHandler>();
builder.Services.AddScoped<UpdateEmployeeCommandHandler>();
builder.Services.AddScoped<CreateResourceTypeCommandHandler>();
builder.Services.AddScoped<CreateRateCommandHandler>();

// HR query handlers
builder.Services.AddScoped<GetEmployeeByIdQueryHandler>();
builder.Services.AddScoped<GetAllEmployeesQueryHandler>();
builder.Services.AddScoped<GetAllResourceTypesQueryHandler>();
builder.Services.AddScoped<GetEmployeeRatesQueryHandler>();

// CRM command handlers
builder.Services.AddScoped<CreateClientCommandHandler>();
builder.Services.AddScoped<CreateContactCommandHandler>();
builder.Services.AddScoped<CreateNoteCommandHandler>();

// CRM query handlers
builder.Services.AddScoped<GetClientByIdQueryHandler>();
builder.Services.AddScoped<GetAllClientsQueryHandler>();
builder.Services.AddScoped<GetClientContactsQueryHandler>();
builder.Services.AddScoped<GetClientNotesQueryHandler>();

// PM command handlers
builder.Services.AddScoped<CreateProjectCommandHandler>();
builder.Services.AddScoped<CreateWBSItemCommandHandler>();
builder.Services.AddScoped<CreateContractCommandHandler>();

// PM query handlers
builder.Services.AddScoped<GetProjectByIdQueryHandler>();
builder.Services.AddScoped<GetAllProjectsQueryHandler>();
builder.Services.AddScoped<GetProjectWBSItemsQueryHandler>();
builder.Services.AddScoped<GetProjectContractsQueryHandler>();

// VM command handlers
builder.Services.AddScoped<CreateVendorCommandHandler>();
builder.Services.AddScoped<CreateVendorContactCommandHandler>();
builder.Services.AddScoped<CreateVendorNoteCommandHandler>();

// VM query handlers
builder.Services.AddScoped<GetVendorByIdQueryHandler>();
builder.Services.AddScoped<GetAllVendorsQueryHandler>();
builder.Services.AddScoped<GetVendorContactsQueryHandler>();
builder.Services.AddScoped<GetVendorNotesQueryHandler>();

// TE command handlers
builder.Services.AddScoped<CreateTimesheetCommandHandler>();
builder.Services.AddScoped<CreateExpenseReportCommandHandler>();

// TE query handlers
builder.Services.AddScoped<GetTimesheetByIdQueryHandler>();
builder.Services.AddScoped<GetExpenseReportByIdQueryHandler>();

// FIN command handlers
builder.Services.AddScoped<CreateAccountCommandHandler>();
builder.Services.AddScoped<CreateJournalEntryCommandHandler>();
builder.Services.AddScoped<PostJournalEntryCommandHandler>();

// FIN query handlers
builder.Services.AddScoped<GetAccountByIdQueryHandler>();
builder.Services.AddScoped<GetJournalEntryByIdQueryHandler>();

// BILL command handlers
builder.Services.AddScoped<CreateInvoiceCommandHandler>();
builder.Services.AddScoped<PostInvoiceCommandHandler>();
builder.Services.AddScoped<ApplyPaymentCommandHandler>();

// BILL query handlers
builder.Services.AddScoped<GetInvoiceByIdQueryHandler>();

// RPT query handlers
builder.Services.AddScoped<GetBalanceSheetQueryHandler>();
builder.Services.AddScoped<GetIncomeStatementQueryHandler>();
builder.Services.AddScoped<GetProjectProfitabilityQueryHandler>();
builder.Services.AddScoped<GetTimesheetSummaryQueryHandler>();
builder.Services.AddScoped<GetInvoiceSummaryQueryHandler>();

// DASH query handlers
builder.Services.AddScoped<GetExecutiveDashboardQueryHandler>();
builder.Services.AddScoped<GetFinancialDashboardQueryHandler>();
builder.Services.AddScoped<GetProjectDashboardQueryHandler>();
builder.Services.AddScoped<GetEmployeeDashboardQueryHandler>();

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

// Use CORS
app.UseCors("AllowAll");

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
