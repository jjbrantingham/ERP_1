# Migration Review Skill

## Purpose
Review EF Core migrations before applying to production.

## Review Checklist

- [ ] No data loss (dropping columns/tables)
- [ ] Backward compatible
- [ ] Includes rollback script
- [ ] Tested on production-like data
- [ ] Performance tested (large tables)
- [ ] Indexes added online
- [ ] No table locks during business hours

## Review Process

```csharp
// 1. Review generated migration
dotnet ef migrations add MigrationName --startup-project ../ERP.Web

// 2. Review SQL
dotnet ef migrations script --startup-project ../ERP.Web

// 3. Test on copy of production data

// 4. Create rollback script

// 5. Apply with monitoring
dotnet ef database update --startup-project ../ERP.Web
```

## Related Skills
- add-migration
- database-schema-review
