using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.HR.Enums;
using ERP.Domain.HR.ValueObjects;

namespace ERP.Domain.HR.Entities;

/// <summary>
/// Represents an employee in the organization.
/// This is the main aggregate root for the HR module.
/// </summary>
public class Employee : AggregateRoot
{
    public EmployeeNumber EmployeeNumber { get; private set; }
    public long? UserId { get; private set; } // Link to Identity.User
    public long ResourceTypeId { get; private set; }

    // Personal Information
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? MiddleName { get; private set; }
    public Email Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? MobileNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }

    // Employment Information
    public EmploymentType EmploymentType { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateTime HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public string? JobTitle { get; private set; }
    public string? Department { get; private set; }
    public long? ManagerId { get; private set; }

    // Compensation (base salary/hourly rate)
    public Money? BaseSalary { get; private set; }

    // Availability
    public decimal StandardHoursPerWeek { get; private set; }
    public bool IsAvailableForProjects { get; private set; }

    // Additional Information
    public string? Notes { get; private set; }

    // Navigation properties
    public ResourceType? ResourceType { get; private set; }
    public Employee? Manager { get; private set; }

    private readonly List<Rate> _rates = new();
    public IReadOnlyCollection<Rate> Rates => _rates.AsReadOnly();

    // Computed property
    public string FullName => $"{FirstName} {LastName}";

    private Employee()
    {
        EmployeeNumber = new EmployeeNumber("TEMP");
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = new Email("temp@temp.com");
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    public static Employee Create(
        Guid tenantId,
        EmployeeNumber employeeNumber,
        long resourceTypeId,
        string firstName,
        string lastName,
        Email email,
        EmploymentType employmentType,
        DateTime hireDate,
        string? middleName = null,
        string? phoneNumber = null,
        string? mobileNumber = null,
        DateTime? dateOfBirth = null,
        string? jobTitle = null,
        string? department = null,
        long? managerId = null,
        Money? baseSalary = null,
        decimal standardHoursPerWeek = 40,
        long? userId = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (standardHoursPerWeek <= 0 || standardHoursPerWeek > 168)
            throw new ArgumentException("Standard hours per week must be between 0 and 168", nameof(standardHoursPerWeek));

        var employee = new Employee
        {
            TenantId = tenantId,
            EmployeeNumber = employeeNumber,
            UserId = userId,
            ResourceTypeId = resourceTypeId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            MiddleName = middleName?.Trim(),
            Email = email,
            PhoneNumber = phoneNumber?.Trim(),
            MobileNumber = mobileNumber?.Trim(),
            DateOfBirth = dateOfBirth,
            EmploymentType = employmentType,
            Status = EmployeeStatus.Active,
            HireDate = hireDate,
            JobTitle = jobTitle?.Trim(),
            Department = department?.Trim(),
            ManagerId = managerId,
            BaseSalary = baseSalary,
            StandardHoursPerWeek = standardHoursPerWeek,
            IsAvailableForProjects = true,
            Notes = notes?.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        employee.AddDomainEvent(new EmployeeCreatedEvent(employee.Id, employee.EmployeeNumber, employee.FullName));

        return employee;
    }

    /// <summary>
    /// Updates employee personal information.
    /// </summary>
    public void UpdatePersonalInfo(
        string firstName,
        string lastName,
        Email email,
        string? middleName = null,
        string? phoneNumber = null,
        string? mobileNumber = null,
        DateTime? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        MiddleName = middleName?.Trim();
        Email = email;
        PhoneNumber = phoneNumber?.Trim();
        MobileNumber = mobileNumber?.Trim();
        DateOfBirth = dateOfBirth;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates employment information.
    /// </summary>
    public void UpdateEmploymentInfo(
        long resourceTypeId,
        EmploymentType employmentType,
        string? jobTitle = null,
        string? department = null,
        long? managerId = null,
        Money? baseSalary = null,
        decimal standardHoursPerWeek = 40)
    {
        if (standardHoursPerWeek <= 0 || standardHoursPerWeek > 168)
            throw new ArgumentException("Standard hours per week must be between 0 and 168", nameof(standardHoursPerWeek));

        ResourceTypeId = resourceTypeId;
        EmploymentType = employmentType;
        JobTitle = jobTitle?.Trim();
        Department = department?.Trim();
        ManagerId = managerId;
        BaseSalary = baseSalary;
        StandardHoursPerWeek = standardHoursPerWeek;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Links the employee to a user account.
    /// </summary>
    public void LinkToUser(long userId)
    {
        UserId = userId;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Unlinks the employee from a user account.
    /// </summary>
    public void UnlinkFromUser()
    {
        UserId = null;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes the employee's status.
    /// </summary>
    public void ChangeStatus(EmployeeStatus newStatus, string? reason = null)
    {
        var oldStatus = Status;
        Status = newStatus;
        ModifiedDate = DateTime.UtcNow;

        // Set termination date if terminated or resigned
        if (newStatus == EmployeeStatus.Terminated || newStatus == EmployeeStatus.Resigned)
        {
            if (!TerminationDate.HasValue)
            {
                TerminationDate = DateTime.UtcNow;
            }
            IsAvailableForProjects = false;
        }

        AddDomainEvent(new EmployeeStatusChangedEvent(Id, EmployeeNumber, FullName, oldStatus, newStatus, reason));
    }

    /// <summary>
    /// Sets availability for project assignments.
    /// </summary>
    public void SetAvailability(bool isAvailable)
    {
        IsAvailableForProjects = isAvailable;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a rate for the employee.
    /// </summary>
    public void AddRate(Rate rate)
    {
        if (rate.EmployeeId != Id)
            throw new ArgumentException("Rate does not belong to this employee", nameof(rate));

        _rates.Add(rate);
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the current effective rate for a specific rate type.
    /// </summary>
    public Rate? GetCurrentRate(RateType rateType)
    {
        return _rates
            .Where(r => r.RateType == rateType && r.IsEffectiveOn(DateTime.UtcNow))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefault();
    }

    /// <summary>
    /// Updates notes.
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the employee is currently active and available.
    /// </summary>
    public bool IsActiveAndAvailable()
    {
        return Status == EmployeeStatus.Active && IsAvailableForProjects;
    }
}
