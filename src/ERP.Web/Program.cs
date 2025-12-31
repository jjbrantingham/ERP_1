using ERP.Application.Common.Interfaces;
using ERP.Application.Identity.Commands;
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
// Will add more repositories as we build modules
// builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

// Register authentication services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Register command handlers
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<RefreshTokenCommandHandler>();
builder.Services.AddScoped<ChangePasswordCommandHandler>();

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
