# Security Audit Skill

## Purpose
Perform comprehensive security review focusing on common vulnerabilities and ERP-specific security requirements.

## OWASP Top 10 (2021) Checklist

### A01:2021 - Broken Access Control
- [ ] Authorization checks on all endpoints
- [ ] Role-based access control (RBAC) implemented
- [ ] Multi-tenancy isolation enforced
- [ ] No direct object references without authorization
- [ ] CORS properly configured
- [ ] Rate limiting on sensitive endpoints

**Check for:**
```csharp
// BAD - No authorization check
public async Task<Invoice> GetInvoice(long id)
{
    return await _context.Invoices.FindAsync(id);
}

// GOOD - Authorization with tenant isolation
[Authorize(Roles = "Finance,Administrator")]
public async Task<Invoice> GetInvoice(long id)
{
    var invoice = await _context.Invoices
        .Where(i => i.TenantId == _currentTenant.TenantId)
        .FirstOrDefaultAsync(i => i.Id == id);

    if (invoice == null)
        throw new NotFoundException();

    return invoice;
}
```

### A02:2021 - Cryptographic Failures
- [ ] Passwords hashed with strong algorithm (bcrypt, Argon2)
- [ ] Sensitive data encrypted at rest
- [ ] HTTPS enforced (HSTS enabled)
- [ ] Secure connection strings (use Key Vault)
- [ ] No sensitive data in logs
- [ ] Secure random number generation

**Check for:**
```csharp
// BAD - Plain text password
user.Password = request.Password;

// GOOD - Hashed password
user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

// BAD - Logging sensitive data
_logger.LogInformation("User {Email} logged in with password {Password}", email, password);

// GOOD - No sensitive data in logs
_logger.LogInformation("User {Email} logged in successfully", email);
```

### A03:2021 - Injection
- [ ] Parameterized queries (EF Core handles this)
- [ ] Input validation on all user input
- [ ] Output encoding for HTML
- [ ] No dynamic SQL construction
- [ ] Validate file uploads
- [ ] Sanitize user-provided content

**Check for:**
```csharp
// BAD - SQL Injection risk
var sql = $"SELECT * FROM Users WHERE Email = '{email}'";

// GOOD - Parameterized (EF Core)
var user = await _context.Users
    .Where(u => u.Email == email)
    .FirstOrDefaultAsync();

// BAD - XSS risk
<div>@Html.Raw(Model.UserInput)</div>

// GOOD - Encoded output
<div>@Model.UserInput</div>
```

### A04:2021 - Insecure Design
- [ ] Security requirements defined
- [ ] Threat modeling performed
- [ ] Secure defaults
- [ ] Defense in depth
- [ ] Principle of least privilege

### A05:2021 - Security Misconfiguration
- [ ] Error messages don't expose sensitive info
- [ ] Unnecessary features disabled
- [ ] Default credentials changed
- [ ] Security headers configured
- [ ] Dependencies up to date
- [ ] Proper environment configuration

**Check for:**
```csharp
// BAD - Exposing stack trace
catch (Exception ex)
{
    return BadRequest(ex.ToString());
}

// GOOD - Generic error message
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing request");
    return StatusCode(500, "An error occurred processing your request");
}
```

### A06:2021 - Vulnerable and Outdated Components
- [ ] NuGet packages up to date
- [ ] Known vulnerabilities patched
- [ ] Dependency scanning enabled
- [ ] Remove unused dependencies

### A07:2021 - Identification and Authentication Failures
- [ ] Strong password policy
- [ ] Multi-factor authentication available
- [ ] Session management secure
- [ ] Account lockout after failed attempts
- [ ] Secure password reset
- [ ] No default credentials

**Check for:**
```csharp
// GOOD - Password requirements
options.Password.RequireDigit = true;
options.Password.RequiredLength = 12;
options.Password.RequireNonAlphanumeric = true;
options.Password.RequireUppercase = true;
options.Password.RequireLowercase = true;

// GOOD - Account lockout
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
options.Lockout.MaxFailedAccessAttempts = 5;
```

### A08:2021 - Software and Data Integrity Failures
- [ ] Code signing
- [ ] Secure CI/CD pipeline
- [ ] Dependency integrity checks
- [ ] No untrusted deserialization

### A09:2021 - Security Logging and Monitoring Failures
- [ ] Security events logged
- [ ] Failed login attempts logged
- [ ] Authorization failures logged
- [ ] Log integrity protected
- [ ] Alerting configured
- [ ] Logs reviewed regularly

**Check for:**
```csharp
// GOOD - Log security events
_logger.LogWarning("Failed login attempt for user {Email} from IP {IP}",
    email, HttpContext.Connection.RemoteIpAddress);

_logger.LogWarning("Unauthorized access attempt to {Resource} by user {UserId}",
    resourceId, userId);
```

### A10:2021 - Server-Side Request Forgery (SSRF)
- [ ] Validate URLs
- [ ] Whitelist allowed hosts
- [ ] No user-controlled redirects

## ERP-Specific Security Requirements

### Multi-Tenancy Security
```csharp
// CRITICAL - Always check tenant isolation

// BAD - Cross-tenant access possible
var project = await _context.Projects.FindAsync(id);

// GOOD - Tenant-filtered query
var project = await _context.Projects
    .Where(p => p.TenantId == _currentTenant.TenantId)
    .FirstOrDefaultAsync(p => p.Id == id);

// BEST - Global query filter in DbContext
modelBuilder.Entity<Project>()
    .HasQueryFilter(p => p.TenantId == CurrentTenantId);
```

### Financial Data Security
- [ ] Audit trail for all financial transactions
- [ ] No DELETE on financial records (soft delete only)
- [ ] Immutable once posted
- [ ] Separation of duties enforced
- [ ] Double-entry validation

```csharp
// GOOD - Audit financial changes
public async Task PostInvoice(long invoiceId)
{
    var invoice = await GetInvoiceAsync(invoiceId);

    if (invoice.Status == InvoiceStatus.Posted)
        throw new InvalidOperationException("Invoice already posted");

    invoice.Post(_currentUser.UserId);

    await _auditService.LogAsync(new AuditEntry
    {
        EntityType = nameof(Invoice),
        EntityId = invoiceId,
        Action = "Posted",
        UserId = _currentUser.UserId,
        Changes = JsonSerializer.Serialize(new { OldStatus = "Draft", NewStatus = "Posted" })
    });

    await _unitOfWork.SaveChangesAsync();
}
```

### PII (Personally Identifiable Information) Protection
- [ ] Sensitive employee data encrypted
- [ ] SSN/Tax ID masked in UI
- [ ] Salary information restricted
- [ ] GDPR compliance (right to be forgotten)
- [ ] Data retention policies

```csharp
// GOOD - Mask sensitive data
public class EmployeeDto
{
    public string SSN => $"***-**-{_ssn?.Substring(7, 4)}";
    public decimal? Salary => User.IsInRole("HR") ? _salary : null;
}
```

## Security Headers

Check that these headers are configured:

```csharp
// In Program.cs or middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");

    await next();
});

// HSTS
app.UseHsts();
app.UseHttpsRedirection();
```

## Common Vulnerabilities to Check

### 1. Mass Assignment
```csharp
// BAD - Can set any property
[HttpPost]
public async Task<IActionResult> Create(Employee employee)
{
    await _context.Employees.AddAsync(employee);
    await _context.SaveChangesAsync();
    return Ok();
}

// GOOD - Use specific DTO
[HttpPost]
public async Task<IActionResult> Create(CreateEmployeeCommand command)
{
    var employee = new Employee
    {
        TenantId = _currentTenant.TenantId,
        FirstName = command.FirstName,
        LastName = command.LastName
        // Only set allowed properties
    };
    // ...
}
```

### 2. Insecure Direct Object References
```csharp
// BAD
[HttpGet("{id}")]
public async Task<IActionResult> GetEmployee(long id)
{
    return Ok(await _context.Employees.FindAsync(id));
}

// GOOD
[HttpGet("{id}")]
[Authorize]
public async Task<IActionResult> GetEmployee(long id)
{
    var employee = await _context.Employees
        .Where(e => e.TenantId == _currentTenant.TenantId)
        .Where(e => e.Id == id)
        .FirstOrDefaultAsync();

    if (employee == null)
        return NotFound();

    // Check if user has permission to view this employee
    if (!User.IsInRole("HR") && employee.Id != _currentUser.EmployeeId)
        return Forbid();

    return Ok(employee);
}
```

### 3. Missing Rate Limiting
```csharp
// GOOD - Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

## Security Audit Report Format

### 1. Executive Summary
- Overall security posture
- Critical vulnerabilities count
- High/Medium/Low severity counts

### 2. Critical Vulnerabilities
List all critical security issues requiring immediate attention.

### 3. High Priority Issues
Security issues that should be addressed soon.

### 4. Medium Priority Issues
Issues to address in next sprint.

### 5. Recommendations
Specific fixes with code examples.

### 6. Best Practices
Security patterns to implement.

## Example Usage

```
User: /security-audit "Billing module"

Claude: I'll perform a comprehensive security audit of the Billing module...

CRITICAL VULNERABILITIES:
1. Missing tenant isolation in InvoicesController.GetById()
   Location: src/ERP.Web/Controllers/InvoicesController.cs:45
   Risk: Cross-tenant data access
   Fix: [Provides code example]

[Complete audit report]
```

## Related Skills
- code-review
- architecture-review
- verify-tenant-isolation
