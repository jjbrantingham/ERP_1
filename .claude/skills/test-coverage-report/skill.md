# Test Coverage Report Skill

## Purpose
Analyze test coverage and suggest improvements.

## Generate Coverage

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

## Coverage Goals

- **Domain Layer**: > 90%
- **Application Layer**: > 80%
- **Controllers**: > 70%
- **Overall**: > 75%

## Analyze Results

Identify untested:
- Critical business logic
- Edge cases
- Error handling paths
- Multi-tenancy checks

## Related Skills
- generate-unit-tests
- generate-integration-tests
