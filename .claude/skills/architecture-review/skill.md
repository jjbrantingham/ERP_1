# Architecture Review Skill

## Purpose
Review architectural decisions and provide recommendations for the ERP SaaS application.

## What to Review

### 1. DDD Principles Adherence
- Are aggregates properly designed with clear boundaries?
- Are domain entities and value objects correctly identified?
- Are domain events used for significant state changes?
- Is business logic encapsulated in the domain layer?
- Are invariants enforced within aggregates?

### 2. SOLID Principles
- **Single Responsibility**: Does each class have one reason to change?
- **Open/Closed**: Is code open for extension, closed for modification?
- **Liskov Substitution**: Are derived types substitutable for base types?
- **Interface Segregation**: Are interfaces focused and cohesive?
- **Dependency Inversion**: Do high-level modules depend on abstractions?

### 3. Clean Architecture Layers
- **Domain Layer**: Pure business logic, no infrastructure dependencies
- **Application Layer**: CQRS handlers, DTOs, interfaces
- **Infrastructure Layer**: EF Core, repositories, external services
- **Web Layer**: Controllers, Razor Pages, minimal logic

### 4. Bounded Context Boundaries
- Are bounded contexts clearly defined?
- Is context mapping appropriate?
- Are anti-corruption layers used where needed?
- Is there proper separation between contexts?

### 5. Scalability Considerations
- Can the architecture scale horizontally?
- Are there potential bottlenecks?
- Is caching used appropriately?
- Are async operations used for I/O?

### 6. Maintainability
- Is code organized logically?
- Are dependencies managed properly?
- Is there appropriate separation of concerns?
- Is the code testable?

## Review Checklist

- [ ] Domain model follows DDD patterns
- [ ] SOLID principles are applied
- [ ] Clean Architecture layers are respected
- [ ] No circular dependencies
- [ ] Proper use of interfaces and abstractions
- [ ] Scalability considerations addressed
- [ ] Multi-tenancy properly implemented
- [ ] Security considerations included
- [ ] Error handling strategy defined
- [ ] Logging strategy defined
- [ ] Testing strategy in place

## Output Format

Provide a structured review with:

### 1. Summary
Overall assessment and key findings

### 2. Strengths
What's done well in the current architecture

### 3. Issues Found
Critical issues that need immediate attention

### 4. Recommendations
Specific, actionable improvements with code examples

### 5. Best Practices
Relevant patterns and practices to apply

### 6. Next Steps
Prioritized action items

## Example Usage

```
User: /architecture-review "Proposing to add a new Payment Gateway integration"

Claude: I'll review the proposed Payment Gateway integration architecture...

[Provides detailed review covering all aspects]
```

## Related Skills
- domain-model-design
- database-schema-review
- code-review
- security-audit
