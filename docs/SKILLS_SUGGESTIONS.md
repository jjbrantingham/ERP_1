# ERP SaaS Application - Claude Code Skills Suggestions

## Overview

This document outlines suggested Claude Code skills to streamline development, testing, and maintenance of the ERP SaaS application. Skills are organized by development lifecycle phase.

---

## 1. Architecture & Design Skills

### `skill: architecture-review`
**Purpose**: Review architectural decisions and provide recommendations

**Description**:
- Analyze proposed architectural changes
- Check alignment with DDD principles
- Verify adherence to SOLID principles
- Suggest improvements for scalability and maintainability
- Review bounded context boundaries

**When to use**:
- Before starting a new module
- When refactoring existing code
- When adding major new features
- During code review of architectural changes

**Example invocation**:
```
/architecture-review "Proposing to add a new Payment Gateway integration"
```

---

### `skill: domain-model-design`
**Purpose**: Design domain models following DDD patterns

**Description**:
- Help design aggregates, entities, and value objects
- Ensure invariants are properly enforced
- Suggest appropriate aggregate boundaries
- Design domain events
- Review entity relationships

**When to use**:
- Designing new bounded contexts
- Adding new domain entities
- Refactoring existing domain models

**Example invocation**:
```
/domain-model-design "Design a new Subscription billing model"
```

---

### `skill: database-schema-review`
**Purpose**: Review database schema changes

**Description**:
- Check for proper indexing
- Verify foreign key relationships
- Ensure multi-tenancy support
- Review performance implications
- Check for data integrity constraints

**When to use**:
- Creating new migrations
- Modifying existing tables
- Performance optimization

**Example invocation**:
```
/database-schema-review "Adding new tables for recurring billing"
```

---

## 2. Development Skills

### `skill: create-aggregate`
**Purpose**: Generate a complete aggregate with all DDD patterns

**Description**:
- Create aggregate root entity
- Generate related entities and value objects
- Add domain events
- Create repository interface
- Generate EF Core configuration
- Create migration
- Generate unit tests

**When to use**:
- Starting a new module
- Adding a new aggregate to existing module

**Example invocation**:
```
/create-aggregate "Subscription" in "Billing" context
```

---

### `skill: create-cqrs-handlers`
**Purpose**: Generate CQRS command and query handlers

**Description**:
- Create command/query classes
- Generate handler implementations
- Add FluentValidation validators
- Create DTOs
- Add AutoMapper profiles
- Generate unit tests

**When to use**:
- Implementing new business operations
- Adding new queries

**Example invocation**:
```
/create-cqrs-handlers "CreateInvoice" command for Billing module
```

---

### `skill: create-api-endpoint`
**Purpose**: Generate complete API endpoint with all layers

**Description**:
- Create controller action
- Add request/response models
- Add Swagger documentation
- Implement authorization
- Add validation
- Create integration tests

**When to use**:
- Exposing new API endpoints
- Adding new controller actions

**Example invocation**:
```
/create-api-endpoint POST /api/invoices/generate
```

---

### `skill: create-ui-page`
**Purpose**: Generate complete UI page with Tailwind CSS

**Description**:
- Create Razor Page or Blazor component
- Add form with validation
- Style with Tailwind CSS
- Add responsive design
- Implement client-side logic
- Add accessibility features

**When to use**:
- Creating new UI pages
- Building new features

**Example invocation**:
```
/create-ui-page "Invoice List" page with filters and pagination
```

---

### `skill: add-migration`
**Purpose**: Create and review EF Core migrations

**Description**:
- Generate migration from model changes
- Review migration for correctness
- Check for data loss risks
- Add custom migration code if needed
- Update database

**When to use**:
- After domain model changes
- Adding new entities
- Modifying existing entities

**Example invocation**:
```
/add-migration "Add Subscription tables"
```

---

## 3. Code Quality & Review Skills

### `skill: code-review`
**Purpose**: Comprehensive code review

**Description**:
- Check code quality
- Verify SOLID principles
- Check for security vulnerabilities
- Review error handling
- Verify logging
- Check performance implications
- Ensure test coverage

**When to use**:
- Before merging pull requests
- After completing a feature
- During refactoring

**Example invocation**:
```
/code-review
```

---

### `skill: security-audit`
**Purpose**: Security-focused code review

**Description**:
- Check for SQL injection vulnerabilities
- Verify XSS prevention
- Check authentication/authorization
- Review data encryption
- Check for OWASP Top 10 vulnerabilities
- Verify multi-tenancy isolation
- Review API security

**When to use**:
- Before production deployment
- After security-sensitive changes
- Regular security audits

**Example invocation**:
```
/security-audit "Billing module"
```

---

### `skill: performance-review`
**Purpose**: Analyze and optimize performance

**Description**:
- Identify N+1 query problems
- Review database query efficiency
- Check for inefficient algorithms
- Suggest caching opportunities
- Review memory usage
- Analyze API response times

**When to use**:
- Performance optimization sprints
- Before production deployment
- When performance issues are reported

**Example invocation**:
```
/performance-review "Invoice generation process"
```

---

### `skill: refactor-suggestion`
**Purpose**: Suggest refactoring opportunities

**Description**:
- Identify code smells
- Suggest design pattern applications
- Recommend code organization improvements
- Identify duplicate code
- Suggest simplifications

**When to use**:
- During technical debt reduction
- When code becomes hard to maintain
- Regular refactoring sessions

**Example invocation**:
```
/refactor-suggestion "ProjectsController.cs"
```

---

## 4. Testing Skills

### `skill: generate-unit-tests`
**Purpose**: Generate comprehensive unit tests

**Description**:
- Create unit tests for domain logic
- Add tests for edge cases
- Use xUnit, FluentAssertions, Moq
- Achieve high code coverage
- Follow AAA pattern (Arrange, Act, Assert)

**When to use**:
- After writing new domain logic
- After creating new handlers
- When improving test coverage

**Example invocation**:
```
/generate-unit-tests for "Invoice" aggregate
```

---

### `skill: generate-integration-tests`
**Purpose**: Generate integration tests

**Description**:
- Create tests for database operations
- Test API endpoints
- Use WebApplicationFactory
- Test multi-tenancy isolation
- Test transaction behavior

**When to use**:
- After creating new repositories
- After creating new API endpoints
- Testing database migrations

**Example invocation**:
```
/generate-integration-tests for "InvoicesController"
```

---

### `skill: generate-functional-tests`
**Purpose**: Generate end-to-end functional tests

**Description**:
- Create user workflow tests
- Test complete business scenarios
- Use Playwright or Selenium
- Test UI interactions
- Test cross-module workflows

**When to use**:
- Testing user stories
- Before major releases
- Regression testing

**Example invocation**:
```
/generate-functional-tests "Create and approve timesheet workflow"
```

---

### `skill: test-coverage-report`
**Purpose**: Analyze test coverage and suggest improvements

**Description**:
- Generate coverage report
- Identify untested code
- Suggest critical tests
- Prioritize coverage improvements

**When to use**:
- Before releases
- During quality improvement sprints
- Regular coverage reviews

**Example invocation**:
```
/test-coverage-report
```

---

## 5. DevOps & Deployment Skills

### `skill: setup-cicd`
**Purpose**: Set up or improve CI/CD pipeline

**Description**:
- Create GitHub Actions workflows
- Add build, test, deploy stages
- Configure environment-specific settings
- Add security scanning
- Configure deployment approvals

**When to use**:
- Initial project setup
- Adding new environments
- Improving deployment process

**Example invocation**:
```
/setup-cicd for Azure App Service
```

---

### `skill: docker-setup`
**Purpose**: Create Docker configuration

**Description**:
- Create Dockerfile
- Create docker-compose.yml
- Configure for development and production
- Add health checks
- Optimize image size

**When to use**:
- Initial project setup
- Adding containerization
- Local development setup

**Example invocation**:
```
/docker-setup for ERP application
```

---

### `skill: azure-infrastructure`
**Purpose**: Set up Azure infrastructure

**Description**:
- Create ARM templates or Bicep files
- Configure App Service
- Set up Azure SQL Database
- Configure Application Insights
- Set up Key Vault
- Configure networking

**When to use**:
- Initial infrastructure setup
- Adding new environments
- Infrastructure changes

**Example invocation**:
```
/azure-infrastructure for production environment
```

---

## 6. Documentation Skills

### `skill: generate-api-docs`
**Purpose**: Generate or update API documentation

**Description**:
- Generate OpenAPI/Swagger documentation
- Add XML comments
- Create API usage examples
- Document authentication
- Create Postman collection

**When to use**:
- After API changes
- Before releases
- For external API consumers

**Example invocation**:
```
/generate-api-docs for "Invoices API"
```

---

### `skill: update-readme`
**Purpose**: Update README and setup documentation

**Description**:
- Update setup instructions
- Add new features to README
- Update prerequisites
- Add troubleshooting section
- Update architecture diagram

**When to use**:
- After major changes
- For new team members
- Before releases

**Example invocation**:
```
/update-readme with new billing module
```

---

### `skill: generate-changelog`
**Purpose**: Generate CHANGELOG from commits

**Description**:
- Parse git commits
- Group by type (features, fixes, breaking changes)
- Follow Keep a Changelog format
- Add version numbers
- Generate release notes

**When to use**:
- Before releases
- For version updates
- Release documentation

**Example invocation**:
```
/generate-changelog for version 1.2.0
```

---

## 7. Database Management Skills

### `skill: seed-test-data`
**Purpose**: Generate test data for development/testing

**Description**:
- Create realistic test data
- Respect multi-tenancy
- Maintain referential integrity
- Create various scenarios
- Generate data seeding migrations

**When to use**:
- Development environment setup
- Testing scenarios
- Demo data creation

**Example invocation**:
```
/seed-test-data for "Billing module" with 100 invoices
```

---

### `skill: optimize-queries`
**Purpose**: Analyze and optimize database queries

**Description**:
- Review EF Core queries
- Identify N+1 problems
- Suggest eager/explicit loading
- Recommend projections
- Add missing indexes
- Review execution plans

**When to use**:
- Performance optimization
- After adding new queries
- When database performance issues occur

**Example invocation**:
```
/optimize-queries in "InvoiceRepository"
```

---

### `skill: migration-review`
**Purpose**: Review migrations before applying

**Description**:
- Check for data loss risks
- Verify backward compatibility
- Check for performance issues
- Suggest rollback scripts
- Validate migration order

**When to use**:
- Before applying migrations to production
- During migration creation
- Migration troubleshooting

**Example invocation**:
```
/migration-review "20240115_AddSubscriptions"
```

---

## 8. Business Logic Skills

### `skill: validate-business-rules`
**Purpose**: Review business rule implementation

**Description**:
- Verify domain invariants
- Check calculation accuracy
- Review workflow logic
- Validate state transitions
- Check for edge cases

**When to use**:
- Implementing complex business logic
- After business rule changes
- During financial logic implementation

**Example invocation**:
```
/validate-business-rules for "Invoice calculation"
```

---

### `skill: financial-accuracy-check`
**Purpose**: Verify financial calculations and accounting rules

**Description**:
- Check double-entry bookkeeping
- Verify calculation formulas
- Check rounding
- Validate account balances
- Review journal entry generation

**When to use**:
- Implementing financial features
- After accounting logic changes
- Before financial reports

**Example invocation**:
```
/financial-accuracy-check for "Journal Entry posting"
```

---

## 9. Monitoring & Debugging Skills

### `skill: add-logging`
**Purpose**: Add comprehensive logging

**Description**:
- Add structured logging
- Add correlation IDs
- Log appropriate levels
- Add performance metrics
- Avoid logging sensitive data

**When to use**:
- Implementing new features
- Debugging production issues
- Improving observability

**Example invocation**:
```
/add-logging to "InvoiceGenerationService"
```

---

### `skill: add-health-checks`
**Purpose**: Add health check endpoints

**Description**:
- Create health check classes
- Check database connectivity
- Check external services
- Configure health check UI
- Add custom health checks

**When to use**:
- Initial setup
- Adding new dependencies
- Improving monitoring

**Example invocation**:
```
/add-health-checks for database and external APIs
```

---

### `skill: analyze-logs`
**Purpose**: Analyze application logs for issues

**Description**:
- Parse log files
- Identify error patterns
- Find performance bottlenecks
- Suggest fixes
- Create alerts

**When to use**:
- Debugging production issues
- Performance troubleshooting
- Error analysis

**Example invocation**:
```
/analyze-logs from Application Insights for last 24 hours
```

---

## 10. Multi-Tenancy Skills

### `skill: verify-tenant-isolation`
**Purpose**: Verify multi-tenancy isolation

**Description**:
- Check global query filters
- Verify tenant context
- Test cross-tenant access prevention
- Review tenant resolution
- Audit tenant data leakage

**When to use**:
- Implementing new features
- Security audits
- Before production deployment

**Example invocation**:
```
/verify-tenant-isolation in "Projects module"
```

---

### `skill: tenant-migration`
**Purpose**: Migrate tenant data

**Description**:
- Create tenant migration scripts
- Verify data integrity
- Handle tenant-specific customizations
- Create rollback procedures
- Test migration process

**When to use**:
- Onboarding new tenants
- Data migration
- Tenant offboarding

**Example invocation**:
```
/tenant-migration for tenant "ACME Corp"
```

---

## Skill Usage Workflow

### Typical Development Workflow

1. **Planning Phase**
   ```
   /architecture-review "New feature description"
   /domain-model-design "Feature domain model"
   ```

2. **Implementation Phase**
   ```
   /create-aggregate "AggregateName"
   /create-cqrs-handlers "CommandName"
   /create-api-endpoint "POST /api/resource"
   /create-ui-page "Page name"
   ```

3. **Testing Phase**
   ```
   /generate-unit-tests for "ClassName"
   /generate-integration-tests for "ControllerName"
   /test-coverage-report
   ```

4. **Review Phase**
   ```
   /code-review
   /security-audit "ModuleName"
   /performance-review "FeatureName"
   /verify-tenant-isolation
   ```

5. **Deployment Phase**
   ```
   /migration-review "MigrationName"
   /generate-changelog for version X.Y.Z
   /update-readme
   ```

---

## Custom Skill Creation

### Recommended Custom Skills

#### `skill: erp-standard-check`
Check compliance with ERP-specific coding standards:
- Multi-tenancy implementation
- Audit trail requirements
- Financial calculation standards
- Security requirements

#### `skill: create-financial-report`
Generate financial report templates:
- Create report class
- Generate SQL/LINQ query
- Create export functionality
- Add UI components

#### `skill: timesheet-workflow-test`
Test complete timesheet workflow:
- Entry → Submission → Approval → Posting

---

## Skill Configuration Examples

### .claude/skills/code-review/skill.md
```markdown
# Code Review Skill

Review code for:
- SOLID principles
- DDD patterns adherence
- Security vulnerabilities
- Performance issues
- Test coverage
- Multi-tenancy compliance
- Error handling
- Logging
- Documentation

Provide specific actionable feedback with code examples.
```

### .claude/skills/create-aggregate/skill.md
```markdown
# Create Aggregate Skill

Generate a complete DDD aggregate including:

1. Aggregate root class with:
   - Strong-typed ID
   - Business logic methods
   - Domain events
   - Invariant enforcement

2. Related entities and value objects

3. Repository interface

4. EF Core entity configuration

5. Database migration

6. Unit tests

7. Integration tests

Ask for:
- Aggregate name
- Bounded context
- Key properties
- Business rules
```

---

## Conclusion

These skills provide comprehensive support throughout the development lifecycle of the ERP SaaS application. They help maintain:

- **Consistency**: Standard patterns across the codebase
- **Quality**: Automated reviews and testing
- **Speed**: Rapid generation of boilerplate
- **Best Practices**: Adherence to DDD, SOLID, and security principles
- **Documentation**: Up-to-date documentation

Customize and extend these skills as the project evolves and team needs change.
