using ERP.Domain.HR.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Employee entity.
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", "hr");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("EmployeeId")
            .ValueGeneratedOnAdd();

        // Employee Number value object
        builder.OwnsOne(e => e.EmployeeNumber, en =>
        {
            en.Property(n => n.Value)
                .HasColumnName("EmployeeNumber")
                .IsRequired()
                .HasMaxLength(50);

            en.HasIndex(n => n.Value)
                .IsUnique()
                .HasDatabaseName("IX_Employees_EmployeeNumber");
        });

        // Email value object
        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property(em => em.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);

            email.HasIndex(em => em.Value)
                .IsUnique()
                .HasDatabaseName("IX_Employees_Email");
        });

        // Personal information
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.MiddleName)
            .HasMaxLength(100);

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(e => e.MobileNumber)
            .HasMaxLength(20);

        builder.Property(e => e.DateOfBirth)
            .HasColumnType("date");

        // Employment information
        builder.Property(e => e.EmploymentType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(e => e.HireDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(e => e.TerminationDate)
            .HasColumnType("date");

        builder.Property(e => e.JobTitle)
            .HasMaxLength(200);

        builder.Property(e => e.Department)
            .HasMaxLength(200);

        // Base salary value object
        builder.OwnsOne(e => e.BaseSalary, salary =>
        {
            salary.Property(m => m.Amount)
                .HasColumnName("BaseSalaryAmount")
                .HasPrecision(18, 2);

            salary.Property(m => m.Currency)
                .HasColumnName("BaseSalaryCurrency")
                .HasMaxLength(3);
        });

        builder.Property(e => e.StandardHoursPerWeek)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(40);

        builder.Property(e => e.IsAvailableForProjects)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.Notes)
            .HasMaxLength(4000);

        // Foreign keys
        builder.Property(e => e.UserId);
        builder.Property(e => e.ResourceTypeId).IsRequired();
        builder.Property(e => e.ManagerId);

        // Relationships
        builder.HasOne(e => e.ResourceType)
            .WithMany()
            .HasForeignKey(e => e.ResourceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Rates)
            .WithOne(r => r.Employee)
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes (Email and EmployeeNumber indexes are configured in their respective OwnsOne blocks)
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ResourceTypeId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Department);
        builder.HasIndex(e => e.ManagerId);
        builder.HasIndex(e => e.IsAvailableForProjects);

        builder.HasIndex(e => new { e.FirstName, e.LastName });

        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);

        // Row version for optimistic concurrency
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(e => e.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
