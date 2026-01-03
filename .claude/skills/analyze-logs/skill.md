# Analyze Logs Skill

## Purpose
Analyze application logs to identify issues and patterns.

## KQL Queries for Application Insights

```kql
// Find errors in last 24 hours
traces
| where timestamp > ago(24h)
| where severityLevel >= 3
| summarize count() by message, operation_Name
| order by count_ desc

// Find slow requests
requests
| where timestamp > ago(1h)
| where duration > 1000
| project timestamp, name, duration, resultCode
| order by duration desc

// Find failed operations
dependencies
| where success == false
| summarize count() by name, type
```

## Related Skills
- add-logging
- add-health-checks
