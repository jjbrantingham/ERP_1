# ERP SaaS Application for Project-Based Businesses

A comprehensive Enterprise Resource Planning (ERP) SaaS solution designed specifically for project-based businesses managing fixed-price and hourly projects.

## 🎯 Overview

This ERP system provides complete business management capabilities including:

- **Project Management** - Manage billable, overhead, and proposal projects with WBS and contracts
- **Human Resources** - Employee management with resource types and rate tracking
- **Client & Contact Management** - Full CRM capabilities
- **Vendor Management** - Contractor and vendor relationship management
- **Time & Expense Tracking** - Timesheet and expense report management with approval workflows
- **Financial Management** - Complete accounting with chart of accounts, general ledger, AR/AP
- **Billing & Invoicing** - Multiple billing modes (T&M, Fixed Price, % Complete, Milestone)
- **Reporting & Analytics** - Comprehensive financial and operational reports

## 🏗️ Architecture

### Technology Stack

- **Backend**: ASP.NET Core 9.0, C# 13
- **Frontend**: Razor Pages/Blazor + Tailwind CSS
- **Database**: Azure SQL Server
- **ORM**: Entity Framework Core 9.0
- **Architecture**: Domain-Driven Design (DDD) with CQRS
- **Cloud Platform**: Microsoft Azure
- **Patterns**: Repository, Unit of Work, CQRS, Event Sourcing

### Key Design Principles

- **Domain-Driven Design (DDD)** with clear bounded contexts
- **CQRS** for separation of read and write operations
- **Multi-tenant** architecture with complete data isolation
- **Event-driven** architecture using domain events
- **Clean Architecture** with clear layer separation
- **RESTful API** design

## 📁 Project Structure

```
ERP_1/
├── src/
│   ├── ERP.Web/              # Presentation layer
│   ├── ERP.Application/      # Application layer (CQRS, Use Cases)
│   ├── ERP.Domain/           # Domain layer (Entities, Aggregates)
│   ├── ERP.Infrastructure/   # Infrastructure layer (EF Core, External Services)
│   └── ERP.Shared/           # Shared kernel
├── tests/
│   ├── ERP.UnitTests/
│   ├── ERP.IntegrationTests/
│   └── ERP.FunctionalTests/
├── docs/
│   ├── ARCHITECTURE.md       # Detailed architecture documentation
│   ├── DOMAIN_MODEL.md       # Domain model design
│   ├── DATABASE_SCHEMA.md    # Complete database schema
│   ├── IMPLEMENTATION_ROADMAP.md  # Phase-by-phase implementation plan
│   └── SKILLS_SUGGESTIONS.md # Claude Code skills for development
├── CLAUDE.md                 # Claude Code configuration
└── README.md
```

## 📚 Documentation

### Planning Documents

- **[Architecture Plan](docs/ARCHITECTURE.md)** - Comprehensive architectural design including technology stack, patterns, and infrastructure
- **[Domain Model](docs/DOMAIN_MODEL.md)** - Detailed domain model design with all aggregates, entities, and value objects
- **[Database Schema](docs/DATABASE_SCHEMA.md)** - Complete database schema with all tables, relationships, and indexes
- **[Implementation Roadmap](docs/IMPLEMENTATION_ROADMAP.md)** - 11-phase implementation plan spanning 48 weeks
- **[Skills Suggestions](docs/SKILLS_SUGGESTIONS.md)** - Recommended Claude Code skills for streamlined development
- **[Claude Configuration](CLAUDE.md)** - Development guidelines and coding standards for Claude Code

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or VS Code with C# extensions
- Azure SQL Database or SQL Server
- Node.js (for Tailwind CSS)
- Git
- Azure CLI (for cloud deployment)

### Initial Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ERP_1
   ```

2. **Create the solution structure**
   ```bash
   # Follow Phase 0 of the Implementation Roadmap
   # See docs/IMPLEMENTATION_ROADMAP.md for detailed steps
   ```

3. **Configure Azure resources**
   - Azure SQL Database
   - Azure App Service
   - Azure Storage Account
   - Application Insights

4. **Set up local development**
   ```bash
   # Install dependencies
   dotnet restore

   # Set up Tailwind CSS
   npm install

   # Apply database migrations
   dotnet ef database update --project src/ERP.Infrastructure --startup-project src/ERP.Web

   # Run the application
   dotnet run --project src/ERP.Web
   ```

## 🏛️ Bounded Contexts

The application is organized into 8 bounded contexts:

1. **Project Management** (`pm`) - Projects, WBS, Contracts, Resource Allocations
2. **Human Resources** (`hr`) - Employees, Resource Types, Rates
3. **Customer Relationship Management** (`crm`) - Clients, Contacts, Notes
4. **Vendor Management** (`vm`) - Contractors, Vendors
5. **Time & Expense** (`te`) - Timesheets, Expense Reports, Approvals
6. **Financial Management** (`fin`) - Chart of Accounts, GL, AR, AP
7. **Billing** (`bill`) - Invoices, Payments, Multiple Billing Modes
8. **Reporting** (`rpt`) - Financial Reports, Dashboards, Analytics

## 🎨 Key Features

### Multi-Tenancy
- Complete tenant data isolation
- Row-level security
- Tenant-specific customizations
- Scalable multi-tenant architecture

### Project Types
- **Billable Projects** - Revenue-generating client projects
- **Overhead Projects** - Internal projects, holidays, time off
- **Proposal Projects** - Pre-sales project planning and estimation

### Billing Modes
- **Time & Material** - Bill based on hours and expenses
- **Fixed Price** - Flat fee for project delivery
- **Percent Complete** - Bill based on project completion percentage
- **Milestone-Based** - Bill when specific milestones are achieved
- **Retainer** - Monthly recurring billing

### Financial Management
- Double-entry bookkeeping
- Chart of accounts with hierarchical structure
- General ledger with journal entries
- Accounts receivable aging
- Accounts payable management
- Financial reporting (P&L, Balance Sheet, Cash Flow)

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/ERP.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Coverage Goals
- Unit Tests: 80%+ coverage
- Integration Tests: All repositories and API endpoints
- Functional Tests: Critical user workflows

## 🔒 Security

- JWT-based authentication
- Role-based authorization (RBAC)
- Claims-based fine-grained permissions
- Multi-tenant data isolation
- Encryption at rest and in transit
- Comprehensive audit logging
- OWASP Top 10 compliance
- Regular security audits

## 📊 Performance

- Async/await throughout
- Optimized database queries with proper indexing
- Pagination on all list operations
- Response caching
- Distributed caching with Redis
- Database connection pooling
- Query optimization with EF Core

## 🔄 Development Workflow

### Branch Strategy
- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `hotfix/*` - Production hotfixes

### Commit Convention
Follow Conventional Commits:
- `feat:` - New features
- `fix:` - Bug fixes
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `docs:` - Documentation
- `chore:` - Maintenance tasks

## 📈 Implementation Phases

The project follows an 11-phase implementation plan:

1. **Phase 0**: Foundation & Setup (Weeks 1-2)
2. **Phase 1**: Core Foundation (Weeks 3-6)
3. **Phase 2**: Human Resources Module (Weeks 7-9)
4. **Phase 3**: CRM Module (Weeks 10-12)
5. **Phase 4**: Project Management Module (Weeks 13-17)
6. **Phase 5**: Vendor Management Module (Weeks 18-19)
7. **Phase 6**: Time & Expense Module (Weeks 20-24)
8. **Phase 7**: Financial Management Module (Weeks 25-30)
9. **Phase 8**: Billing Module (Weeks 31-35)
10. **Phase 9**: Reporting Module (Weeks 36-40)
11. **Phase 10**: Polish & Optimization (Weeks 41-44)
12. **Phase 11**: Beta Testing & Launch (Weeks 45-48)

See [Implementation Roadmap](docs/IMPLEMENTATION_ROADMAP.md) for detailed phase breakdown.

## 🛠️ Development Tools

### Recommended Claude Code Skills

The project includes 30+ custom Claude Code skills to streamline development:

- **Architecture & Design**: `architecture-review`, `domain-model-design`
- **Development**: `create-aggregate`, `create-cqrs-handlers`, `create-api-endpoint`
- **Code Quality**: `code-review`, `security-audit`, `performance-review`
- **Testing**: `generate-unit-tests`, `generate-integration-tests`
- **DevOps**: `setup-cicd`, `docker-setup`, `azure-infrastructure`
- **Database**: `add-migration`, `optimize-queries`, `seed-test-data`

See [Skills Suggestions](docs/SKILLS_SUGGESTIONS.md) for complete list.

## 📦 Dependencies

### Core Dependencies
- Microsoft.EntityFrameworkCore
- Microsoft.AspNetCore.Identity
- MediatR
- FluentValidation
- AutoMapper
- Serilog
- Swashbuckle (Swagger)

### Development Dependencies
- xUnit
- FluentAssertions
- Moq
- Coverlet (code coverage)

## 🌐 Deployment

### Azure Resources Required
- Azure SQL Database
- Azure App Service (Linux)
- Azure Storage Account
- Azure Application Insights
- Azure Key Vault
- Azure Redis Cache (optional)

### Environment Configuration
- Development
- Staging
- Production

See deployment documentation in `docs/ARCHITECTURE.md`.

## 📝 License

[Specify your license here]

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes following conventional commits
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 👥 Team

Recommended team structure:
- Technical Lead / Architect (1)
- Senior Backend Developers (2-3)
- Frontend Developers (2)
- QA Engineers (2)
- DevOps Engineer (1)
- UX/UI Designer (1)
- Product Owner (1)

## 📞 Support

For questions or support, please:
- Check the documentation in the `docs/` folder
- Review the [Claude Configuration](CLAUDE.md) for development guidelines
- Open an issue on GitHub

## 🎯 Success Metrics

### Technical Metrics
- Code coverage > 80%
- API response time < 200ms (95th percentile)
- Page load time < 2 seconds
- 99.9% uptime

### Quality Metrics
- Bug density < 1 bug per 1000 lines of code
- MTTR < 24 hours for critical bugs
- Technical debt ratio < 5%

---

**Built with ASP.NET Core, Domain-Driven Design, and Claude Code**

For detailed technical information, see the [Architecture Documentation](docs/ARCHITECTURE.md).
