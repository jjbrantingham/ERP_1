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
using ERP.Domain.WF.Repositories;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Persistence.Repositories;
using ERP.Infrastructure.Services;
using ERP.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

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

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Fixed window rate limiter for API endpoints
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Stricter rate limit for authentication endpoints
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Global fallback
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
});

// Add response caching
builder.Services.AddResponseCaching();

// Configure output caching for better performance
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));

    // Cache GET requests by default
    options.AddPolicy("default", builder =>
        builder.Expire(TimeSpan.FromSeconds(30)));

    // Longer cache for static data
    options.AddPolicy("static", builder =>
        builder.Expire(TimeSpan.FromMinutes(10)));
});

// Add controllers and API explorer
builder.Services.AddControllers(options =>
{
    // Set maximum request body size to 10MB
    options.MaxModelBindingCollectionSize = 1000;
});
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

// WF repositories
builder.Services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
builder.Services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();

// AUDIT repositories
builder.Services.AddScoped<ERP.Domain.AUDIT.Repositories.IAuditLogRepository, AuditLogRepository>();

// Register authentication services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Register email service
builder.Services.AddScoped<IEmailService, EmailService>();

// Register reporting services
builder.Services.AddScoped<ERP.Application.RPT.Services.IReportPeriodService, ERP.Infrastructure.Services.ReportPeriodService>();

// Register audit and GDPR compliance services
builder.Services.AddScoped<ERP.Application.AUDIT.Services.IDataRetentionPolicyService, ERP.Infrastructure.Services.DataRetentionPolicyService>();

// Register data retention background service
builder.Services.AddHostedService<ERP.Infrastructure.BackgroundServices.DataRetentionBackgroundService>();

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

// Add security headers
app.UseSecurityHeaders();

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

// Use response compression
app.UseResponseCompression();

// Use response caching
app.UseResponseCaching();

// Use output caching
app.UseOutputCache();

// Use rate limiting
app.UseRateLimiter();

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
