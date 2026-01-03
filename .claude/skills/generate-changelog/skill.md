# Generate Changelog Skill

## Purpose
Generate CHANGELOG from git commits following Keep a Changelog format.

## Changelog Format

```markdown
# Changelog

All notable changes to this project will be documented in this file.

## [1.2.0] - 2026-01-03

### Added
- Phase 15: Reporting & Analytics Module
- Project profitability reports
- Budget variance tracking
- Export functionality (PDF, Excel, CSV)

### Changed
- Improved invoice generation performance
- Updated timesheet approval workflow

### Fixed
- Multi-tenancy isolation in project queries
- Date range calculation in reports

### Security
- Added rate limiting on API endpoints
- Enhanced password requirements
```

## Generate from Git

```bash
# Get commits since last tag
git log v1.1.0..HEAD --pretty=format:"%s" --reverse
```

## Related Skills
- update-readme
