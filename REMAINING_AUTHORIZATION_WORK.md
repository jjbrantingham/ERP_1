# Remaining Authorization Work - Implementation Guide

**Status**: 7/37 handlers secured (19% complete)
**Completed**: Critical financial handlers (BILL + FIN modules)
**Remaining**: 30 handlers across Identity, PM, HR, CRM, TE, VM, WF, AUDIT modules

---

## Quick Reference: Pattern for Adding Authorization

### Step 1: Add Using Statements
```csharp
using ERP.Application.Common.Interfaces;  // If not present
using ERP.Application.Common.Security;     // Add this
```

### Step 2: Add ICurrentUserService Field
```csharp
public class YourCommandHandler : IRequestHandler<YourCommand, ReturnType>
{
    // ... existing fields ...
    private readonly ICurrentUserService _currentUser;  // Add this

    public YourCommandHandler(
        // ... existing parameters ...
        ICurrentUserService currentUser)  // Add this
    {
        // ... existing assignments ...
        _currentUser = currentUser;  // Add this
    }
}
```

###Step 3: Add Authentication Check at Start of Handle Method
```csharp
public async Task<ReturnType> Handle(YourCommand request, CancellationToken cancellationToken)
{
    // Add this as the FIRST line
    AuthorizationHelper.EnsureAuthenticated(_currentUser);

    // ... rest of handler logic ...
}
```

### Step 4 (Optional): Add Tenant Ownership Verification
For handlers that access existing entities (Update, Delete, Post operations):
```csharp
public async Task Handle(UpdateCommand request, CancellationToken cancellationToken)
{
    AuthorizationHelper.EnsureAuthenticated(_currentUser);

    // Get entity and verify tenant ownership
    var entity = await _repository.GetByIdAsync(request.EntityId, cancellationToken);
    AuthorizationHelper.EnsureTenantOwnership(entity, _currentTenant, "EntityName");

    // ... rest of handler logic ...
}
```

---

## Handlers Requiring Updates (30 Total)

### ✅ Completed (7 handlers)
- ✅ CreateInvoiceCommandHandler
- ✅ PostInvoiceCommandHandler
- ✅ ApplyPaymentCommandHandler
- ✅ CreateAccountCommandHandler
- ✅ CreateJournalEntryCommandHandler
- ✅ PostJournalEntryCommandHandler

### Identity Module (2 handlers - SKIP Login/Register)

**⚠️ SKIP** these (they ARE the authentication endpoints):
- ❌ LoginCommandHandler - No auth needed (this IS the login)
- ❌ RegisterCommandHandler - No auth needed (this IS the registration)

**✏️ UPDATE** these:
1. **ChangePasswordCommandHandler** - Requires authentication
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()

2. **RefreshTokenCommandHandler** - Requires authentication
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()

### Project Management Module (3 handlers)

3. **CreateProjectCommandHandler**
   - File: `src/ERP.Application/PM/Commands/CreateProjectCommandHandler.cs`
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()

4. **CreateWBSItemCommandHandler**
   - File: `src/ERP.Application/PM/Commands/CreateWBSItemCommandHandler.cs`
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()

5. **CreateContractCommandHandler**
   - File: `src/ERP.Application/PM/Commands/CreateContractCommandHandler.cs`
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()

### Human Resources Module (4 handlers)

6. **CreateEmployeeCommandHandler**
   - File: `src/ERP.Application/HR/Commands/CreateEmployeeCommandHandler.cs`
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()

7. **UpdateEmployeeCommandHandler**
   - File: `src/ERP.Application/HR/Commands/UpdateEmployeeCommandHandler.cs`
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()
   - Add tenant ownership check for Employee entity

8. **CreateRateCommandHandler**
   - File: `src/ERP.Application/HR/Commands/CreateRateCommandHandler.cs`
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()

9. **CreateResourceTypeCommandHandler**
   - File: `src/ERP.Application/HR/Commands/CreateResourceTypeCommandHandler.cs`
   - Add ICurrentUserService
   - Add AuthorizationHelper.EnsureAuthenticated()

### CRM Module (3 handlers)

10. **CreateClientCommandHandler**
    - File: `src/ERP.Application/CRM/Commands/CreateClientCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

11. **CreateContactCommandHandler**
    - File: `src/ERP.Application/CRM/Commands/CreateContactCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

12. **CreateNoteCommandHandler**
    - File: `src/ERP.Application/CRM/Commands/CreateNoteCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

### Time & Expense Module (2 handlers)

13. **CreateTimesheetCommandHandler**
    - File: `src/ERP.Application/TE/Commands/CreateTimesheetCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

14. **CreateExpenseReportCommandHandler**
    - File: `src/ERP.Application/TE/Commands/CreateExpenseReportCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

### Vendor Management Module (3 handlers)

15. **CreateVendorCommandHandler**
    - File: `src/ERP.Application/VM/Commands/CreateVendorCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

16. **CreateVendorContactCommandHandler**
    - File: `src/ERP.Application/VM/Commands/CreateVendorContactCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

17. **CreateVendorNoteCommandHandler**
    - File: `src/ERP.Application/VM/Commands/CreateVendorNoteCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

### Workflow Module (5 handlers - some already have auth)

18. **CreateWorkflowDefinitionCommandHandler**
    - File: `src/ERP.Application/WF/CommandHandlers/CreateWorkflowDefinitionCommandHandler.cs`
    - Check if ICurrentUserService exists
    - Add AuthorizationHelper.EnsureAuthenticated() if not present

19. **StartWorkflowCommandHandler**
    - File: `src/ERP.Application/WF/CommandHandlers/StartWorkflowCommandHandler.cs`
    - Check if ICurrentUserService exists
    - Add AuthorizationHelper.EnsureAuthenticated() if not present

20. **DeactivateWorkflowDefinitionCommandHandler**
    - File: `src/ERP.Application/WF/CommandHandlers/DeactivateWorkflowDefinitionCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

21. **ActivateWorkflowDefinitionCommandHandler**
    - File: `src/ERP.Application/WF/CommandHandlers/ActivateWorkflowDefinitionCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

22. **CancelWorkflowCommandHandler**
    - File: `src/ERP.Application/WF/CommandHandlers/CancelWorkflowCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

**NOTE**: ApproveStepCommandHandler and RejectStepCommandHandler already have authorization (checked in earlier review)

### Audit Module (4 handlers)

23. **CreateAuditLogCommandHandler**
    - File: `src/ERP.Application/AUDIT/Handlers/CreateAuditLogCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

24. **PurgeOldAuditLogsCommandHandler**
    - File: `src/ERP.Application/AUDIT/Handlers/PurgeOldAuditLogsCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()
    - **CRITICAL**: This is a destructive operation, needs special authorization

25. **ExportUserDataCommandHandler**
    - File: `src/ERP.Application/AUDIT/Handlers/ExportUserDataCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()

26. **AnonymizeUserDataCommandHandler**
    - File: `src/ERP.Application/AUDIT/Handlers/AnonymizeUserDataCommandHandler.cs`
    - Add ICurrentUserService
    - Add AuthorizationHelper.EnsureAuthenticated()
    - **CRITICAL**: This is a destructive operation, needs special authorization

---

## Detailed Example: Before & After

### BEFORE (Unsecured Handler)
```csharp
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Repositories;

namespace ERP.Application.PM.Commands;

public class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, long>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {
        // SECURITY RISK: No authentication check!

        var project = Project.Create(
            _currentTenant.TenantId,
            // ... rest of parameters
        );

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
```

### AFTER (Secured Handler)
```csharp
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;  // ← ADDED
using ERP.Domain.Common.Interfaces;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Repositories;

namespace ERP.Application.PM.Commands;

public class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, long>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;  // ← ADDED

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)  // ← ADDED
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;  // ← ADDED
    }

    public async Task<long> Handle(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {
        // ✅ SECURITY: Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);  // ← ADDED

        var project = Project.Create(
            _currentTenant.TenantId,
            // ... rest of parameters
        );

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
```

---

## Testing Authorization

After updating handlers, test with:

### 1. Unit Tests
```csharp
[Fact]
public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedException()
{
    // Arrange
    var handler = CreateHandler();
    _currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);
    var command = new CreateProjectCommand { ... };

    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        () => handler.Handle(command, CancellationToken.None));
}
```

### 2. Integration Tests
```csharp
[Fact]
public async Task CreateProject_WithoutAuthToken_Returns401()
{
    // Arrange
    var client = _factory.CreateClient();
    var command = new CreateProjectCommand { ... };

    // Act
    var response = await client.PostAsJsonAsync("/api/v1/projects", command);

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

---

## Common Issues & Solutions

### Issue 1: Compilation Error - ICurrentUserService Not Found
**Solution**: Check that you added the using statement:
```csharp
using ERP.Application.Common.Interfaces;
```

### Issue 2: Compilation Error - AuthorizationHelper Not Found
**Solution**: Add the Security using statement:
```csharp
using ERP.Application.Common.Security;
```

### Issue 3: DI Error - ICurrentUserService Not Registered
**Solution**: Ensure ICurrentUserService is registered in Program.cs:
```csharp
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

### Issue 4: Handler Has ICurrentUserService But Wrong Property Name
**Solution**: Use the standard naming convention `_currentUser` (lowercase 'c')

---

## Verification Checklist

After updating all handlers:
- [ ] All 26 non-auth handlers have `AuthorizationHelper.EnsureAuthenticated()`
- [ ] All update/delete operations verify tenant ownership
- [ ] Unit tests added for authentication scenarios
- [ ] Integration tests verify 401 responses for unauthenticated requests
- [ ] Compilation succeeds with no errors
- [ ] All tests pass

---

## Estimated Effort

- **Time per handler**: 2-3 minutes
- **Total estimated time**: 1-1.5 hours for 26 handlers
- **Complexity**: Low (repetitive pattern)

---

## Next Steps After Completion

1. Run full test suite
2. Create database migration if needed
3. Update API documentation (Swagger)
4. Security audit/penetration testing
5. Deploy to staging for testing
6. Production deployment

---

## Reference Files

- **Authorization Infrastructure**: `src/ERP.Application/Common/Security/AuthorizationHelper.cs`
- **Completed Examples**: Check BILL and FIN handlers in commits 1bf2934
- **Exception Classes**: `src/ERP.Application/Common/Exceptions/`
- **Interface**: `src/ERP.Application/Common/Interfaces/ICurrentUserService.cs`

---

**Last Updated**: 2026-01-04
**Branch**: `claude/start-erp-implementation-5egZQ`
**Status**: 7/37 handlers complete (19%)
