# Azure Deployment Guide - ERP SaaS Application

This guide provides detailed step-by-step instructions for deploying the ERP SaaS application to Microsoft Azure.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Architecture Overview](#architecture-overview)
3. [Azure Resources Setup](#azure-resources-setup)
4. [Database Deployment](#database-deployment)
5. [Application Configuration](#application-configuration)
6. [Web Application Deployment](#web-application-deployment)
7. [Post-Deployment Configuration](#post-deployment-configuration)
8. [Security Hardening](#security-hardening)
9. [Monitoring and Diagnostics](#monitoring-and-diagnostics)
10. [Backup and Disaster Recovery](#backup-and-disaster-recovery)
11. [Scaling Considerations](#scaling-considerations)
12. [Cost Optimization](#cost-optimization)
13. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Tools

1. **Azure CLI** (version 2.50.0 or later)
   ```bash
   # Install Azure CLI
   # Windows: Download from https://aka.ms/installazurecliwindows
   # macOS: brew install azure-cli
   # Linux: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

   # Verify installation
   az --version
   ```

2. **.NET SDK 9.0**
   ```bash
   # Verify installation
   dotnet --version
   ```

3. **SQL Server Management Studio (SSMS)** or **Azure Data Studio**
   - Download from: https://aka.ms/ssmsfullsetup

4. **Visual Studio 2022** (optional but recommended)
   - With Azure development workload

### Azure Subscription Requirements

- Active Azure subscription with Owner or Contributor role
- Sufficient quota for:
  - App Service Plan (Standard S1 or higher recommended)
  - Azure SQL Database (Standard S2 or higher recommended)
  - Application Insights
  - Storage Account

### Source Code

- Clone the repository and ensure all code is committed
- Verify the solution builds successfully:
  ```bash
  dotnet build
  dotnet test
  ```

---

## Architecture Overview

The deployed architecture will consist of:

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Front Door (Optional)               │
│                   (CDN + WAF + Load Balancing)              │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                  Azure App Service                           │
│                  (ERP.Web - ASP.NET Core 9.0)               │
│                  - Auto-scaling enabled                      │
│                  - Always On enabled                         │
└────────────────┬───────────────────────┬────────────────────┘
                 │                       │
        ┌────────▼────────┐    ┌────────▼────────────┐
        │ Application     │    │ Azure Key Vault     │
        │ Insights        │    │ (Secrets/Conn Str)  │
        └─────────────────┘    └─────────────────────┘
                 │
        ┌────────▼────────────────────────────┐
        │   Azure SQL Database                │
        │   - Geo-replication (optional)      │
        │   - Automatic backups               │
        │   - Advanced Threat Protection      │
        └─────────────────────────────────────┘
```

---

## Azure Resources Setup

### Step 1: Login to Azure

```bash
# Login to Azure
az login

# Set the subscription (if you have multiple)
az account list --output table
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Verify
az account show
```

### Step 2: Create Resource Group

```bash
# Set variables (customize these)
RESOURCE_GROUP="rg-erp-prod"
LOCATION="eastus"
ENVIRONMENT="production"

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION \
  --tags Environment=$ENVIRONMENT Application=ERP
```

### Step 3: Create Azure SQL Server

```bash
# Set SQL Server variables
SQL_SERVER_NAME="sql-erp-prod-$(date +%s)"  # Must be globally unique
SQL_ADMIN_USER="erpadmin"
SQL_ADMIN_PASSWORD="YourSecureP@ssw0rd123!"  # Change this!
SQL_DATABASE_NAME="ERP-Production"

# Create SQL Server
az sql server create \
  --name $SQL_SERVER_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN_USER \
  --admin-password $SQL_ADMIN_PASSWORD

# Configure firewall to allow Azure services
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Add your client IP (for database setup)
MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowClientIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

### Step 4: Create Azure SQL Database

```bash
# Create database (Standard S2 tier)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DATABASE_NAME \
  --service-objective S2 \
  --backup-storage-redundancy Zone \
  --zone-redundant false \
  --max-size 250GB

# Enable long-term retention (optional)
az sql db ltr-policy set \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --database $SQL_DATABASE_NAME \
  --weekly-retention P4W \
  --monthly-retention P12M \
  --yearly-retention P7Y \
  --week-of-year 1
```

### Step 5: Create App Service Plan

```bash
# Create App Service Plan (Standard S1 - adjust as needed)
APP_SERVICE_PLAN="asp-erp-prod"

az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku S1 \
  --is-linux false \
  --tags Environment=$ENVIRONMENT
```

### Step 6: Create Web App

```bash
# Create Web App
WEB_APP_NAME="app-erp-prod-$(date +%s)"  # Must be globally unique

az webapp create \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNET|9.0"

# Enable Always On
az webapp config set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --always-on true \
  --use-32bit-worker-process false \
  --http20-enabled true
```

### Step 7: Create Application Insights

```bash
# Create Application Insights
APP_INSIGHTS_NAME="appi-erp-prod"

az monitor app-insights component create \
  --app $APP_INSIGHTS_NAME \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --application-type web \
  --retention-time 90

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app $APP_INSIGHTS_NAME \
  --resource-group $RESOURCE_GROUP \
  --query instrumentationKey -o tsv)

echo "Application Insights Key: $INSTRUMENTATION_KEY"
```

### Step 8: Create Azure Key Vault

```bash
# Create Key Vault
KEY_VAULT_NAME="kv-erp-prod-$(date +%s)"

az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enabled-for-template-deployment true

# Store secrets
CONNECTION_STRING="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${SQL_DATABASE_NAME};Persist Security Info=False;User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "ConnectionStrings--DefaultConnection" \
  --value "$CONNECTION_STRING"

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "ApplicationInsights--InstrumentationKey" \
  --value "$INSTRUMENTATION_KEY"

# Generate JWT signing key
JWT_SECRET=$(openssl rand -base64 32)
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "Jwt--SecretKey" \
  --value "$JWT_SECRET"
```

### Step 9: Grant Web App Access to Key Vault

```bash
# Enable managed identity for Web App
az webapp identity assign \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP

# Get the managed identity principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query principalId -o tsv)

# Grant access to Key Vault
az keyvault set-policy \
  --name $KEY_VAULT_NAME \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

---

## Database Deployment

### Step 1: Prepare Database Migration

```bash
# Navigate to Infrastructure project
cd src/ERP.Infrastructure

# Create migration bundle (if not exists)
dotnet ef migrations bundle \
  --startup-project ../ERP.Web \
  --context ERPDbContext \
  --self-contained \
  --output ./efbundle
```

### Step 2: Apply Migrations to Azure SQL

**Option A: Using Migration Bundle**

```bash
# Run migration bundle
./efbundle --connection "$CONNECTION_STRING"
```

**Option B: Using EF Core Tools**

```bash
# Apply migrations directly
dotnet ef database update \
  --startup-project ../ERP.Web \
  --context ERPDbContext \
  --connection "$CONNECTION_STRING"
```

**Option C: Using SSMS/Azure Data Studio**

1. Connect to Azure SQL Database:
   - Server: `{SQL_SERVER_NAME}.database.windows.net`
   - Authentication: SQL Server Authentication
   - Username: `{SQL_ADMIN_USER}`
   - Password: `{SQL_ADMIN_PASSWORD}`

2. Generate migration script:
   ```bash
   dotnet ef migrations script \
     --startup-project ../ERP.Web \
     --context ERPDbContext \
     --idempotent \
     --output migration.sql
   ```

3. Execute the script in SSMS/Azure Data Studio

### Step 3: Seed Initial Data

Create a seed data script `seed-data.sql`:

```sql
-- Create default tenant
INSERT INTO common.Tenants (TenantGuid, CompanyName, IsActive, CreatedDate)
VALUES (NEWID(), 'Default Company', 1, GETUTCDATE());

DECLARE @TenantId BIGINT = SCOPE_IDENTITY();

-- Create default admin user
INSERT INTO identity.Users (TenantId, UserName, Email, PasswordHash, IsActive, EmailConfirmed, CreatedDate)
VALUES (
    @TenantId,
    'admin@erp.com',
    'admin@erp.com',
    'AQAAAAIAAYagAAAAEDummyHashForInitialSetup',  -- Must be changed on first login
    1,
    1,
    GETUTCDATE()
);

-- Create default roles
INSERT INTO identity.Roles (Name, Description, CreatedDate)
VALUES
    ('Administrator', 'System Administrator', GETUTCDATE()),
    ('ProjectManager', 'Project Manager', GETUTCDATE()),
    ('Finance', 'Finance User', GETUTCDATE()),
    ('HR', 'Human Resources', GETUTCDATE());

-- Add more seed data as needed...
```

Execute the seed script:

```bash
# Using sqlcmd
sqlcmd -S ${SQL_SERVER_NAME}.database.windows.net \
  -d $SQL_DATABASE_NAME \
  -U $SQL_ADMIN_USER \
  -P $SQL_ADMIN_PASSWORD \
  -i seed-data.sql
```

---

## Application Configuration

### Step 1: Update appsettings.json

Create `appsettings.Production.json` in the ERP.Web project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""  // Will be loaded from Key Vault
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApplicationInsights": {
    "InstrumentationKey": ""  // Will be loaded from Key Vault
  },
  "Jwt": {
    "Issuer": "https://your-erp-app.azurewebsites.net",
    "Audience": "https://your-erp-app.azurewebsites.net",
    "SecretKey": "",  // Will be loaded from Key Vault
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "HealthChecks": {
    "Enabled": true,
    "EvaluationTimeInSeconds": 30
  }
}
```

### Step 2: Configure Key Vault References

```bash
# Configure App Settings to reference Key Vault
az webapp config appsettings set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "ASPNETCORE_ENVIRONMENT=Production" \
    "ConnectionStrings__DefaultConnection=@Microsoft.KeyVault(VaultName=${KEY_VAULT_NAME};SecretName=ConnectionStrings--DefaultConnection)" \
    "ApplicationInsights__InstrumentationKey=@Microsoft.KeyVault(VaultName=${KEY_VAULT_NAME};SecretName=ApplicationInsights--InstrumentationKey)" \
    "Jwt__SecretKey=@Microsoft.KeyVault(VaultName=${KEY_VAULT_NAME};SecretName=Jwt--SecretKey)"
```

---

## Web Application Deployment

### Step 1: Publish Application

```bash
# Navigate to solution root
cd /home/user/ERP_1

# Clean and publish
dotnet clean
dotnet publish src/ERP.Web/ERP.Web.csproj \
  --configuration Release \
  --output ./publish \
  --runtime win-x64 \
  --self-contained false

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..
```

### Step 2: Deploy to Azure App Service

**Option A: Using Azure CLI**

```bash
az webapp deployment source config-zip \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --src deploy.zip
```

**Option B: Using Azure DevOps Pipeline**

Create `azure-pipelines.yml`:

```yaml
trigger:
  branches:
    include:
      - main
      - master

pool:
  vmImage: 'windows-latest'

variables:
  buildConfiguration: 'Release'
  dotNetVersion: '9.0.x'

stages:
- stage: Build
  jobs:
  - job: Build
    steps:
    - task: UseDotNet@2
      displayName: 'Install .NET SDK'
      inputs:
        version: $(dotNetVersion)

    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: '**/*.csproj'

    - task: DotNetCoreCLI@2
      displayName: 'Build solution'
      inputs:
        command: 'build'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration)'

    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        projects: '**/tests/**/*.csproj'
        arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage"'

    - task: DotNetCoreCLI@2
      displayName: 'Publish application'
      inputs:
        command: 'publish'
        publishWebProjects: true
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        zipAfterPublish: true

    - task: PublishBuildArtifacts@1
      displayName: 'Publish artifacts'
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)'
        ArtifactName: 'drop'

- stage: Deploy
  dependsOn: Build
  condition: succeeded()
  jobs:
  - deployment: DeployToProduction
    environment: 'Production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            displayName: 'Deploy to Azure App Service'
            inputs:
              azureSubscription: 'YOUR_SERVICE_CONNECTION'
              appType: 'webApp'
              appName: '$(WEB_APP_NAME)'
              package: '$(Pipeline.Workspace)/drop/**/*.zip'
```

**Option C: Using GitHub Actions**

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main, master ]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: app-erp-prod
  DOTNET_VERSION: '9.0.x'

jobs:
  build-and-deploy:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Test
      run: dotnet test --configuration Release --no-build --verbosity normal

    - name: Publish
      run: dotnet publish src/ERP.Web/ERP.Web.csproj --configuration Release --output ./publish

    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### Step 3: Verify Deployment

```bash
# Check deployment status
az webapp show \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query state

# Get application URL
APP_URL=$(az webapp show \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query defaultHostName -o tsv)

echo "Application URL: https://$APP_URL"

# Test health endpoint
curl https://$APP_URL/health
```

---

## Post-Deployment Configuration

### Step 1: Configure Custom Domain (Optional)

```bash
# Add custom domain
CUSTOM_DOMAIN="erp.yourcompany.com"

az webapp config hostname add \
  --webapp-name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --hostname $CUSTOM_DOMAIN

# Bind SSL certificate (using App Service Managed Certificate)
az webapp config ssl bind \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --certificate-thumbprint auto \
  --ssl-type SNI
```

### Step 2: Configure CORS

```bash
az webapp cors add \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --allowed-origins "https://erp.yourcompany.com" "https://www.yourcompany.com"
```

### Step 3: Configure Authentication (Azure AD Integration)

```bash
# Enable Azure AD authentication
az webapp auth update \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --enabled true \
  --action LoginWithAzureActiveDirectory

# Configure Azure AD
az webapp auth microsoft update \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --client-id "YOUR_AAD_CLIENT_ID" \
  --client-secret "YOUR_AAD_CLIENT_SECRET" \
  --issuer "https://sts.windows.net/YOUR_TENANT_ID/"
```

### Step 4: Configure Auto-Scaling

```bash
# Create autoscale setting
az monitor autoscale create \
  --resource-group $RESOURCE_GROUP \
  --resource $APP_SERVICE_PLAN \
  --resource-type Microsoft.Web/serverfarms \
  --name autoscale-erp \
  --min-count 2 \
  --max-count 10 \
  --count 2

# Add CPU-based scale-out rule
az monitor autoscale rule create \
  --resource-group $RESOURCE_GROUP \
  --autoscale-name autoscale-erp \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1

# Add CPU-based scale-in rule
az monitor autoscale rule create \
  --resource-group $RESOURCE_GROUP \
  --autoscale-name autoscale-erp \
  --condition "Percentage CPU < 30 avg 5m" \
  --scale in 1
```

---

## Security Hardening

### Step 1: Enable Advanced Threat Protection

```bash
# Enable Advanced Threat Protection for SQL Database
az sql db threat-policy update \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --database $SQL_DATABASE_NAME \
  --state Enabled \
  --storage-account YOUR_STORAGE_ACCOUNT
```

### Step 2: Configure Network Security

```bash
# Restrict SQL Server access to App Service only
az sql server vnet-rule create \
  --name AllowAppService \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --subnet /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Network/virtualNetworks/YOUR_VNET/subnets/YOUR_SUBNET

# Remove AllowAzureServices rule for production
az sql server firewall-rule delete \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices
```

### Step 3: Enable Web Application Firewall (Optional)

```bash
# Create Application Gateway with WAF
az network application-gateway create \
  --name ag-erp-prod \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku WAF_v2 \
  --capacity 2 \
  --vnet-name YOUR_VNET \
  --subnet YOUR_SUBNET \
  --public-ip-address ag-erp-pip

# Configure WAF policy
az network application-gateway waf-policy create \
  --name waf-erp-policy \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION
```

### Step 4: Configure Security Headers

Add to `Program.cs`:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';");
    await next();
});
```

---

## Monitoring and Diagnostics

### Step 1: Configure Application Insights Alerts

```bash
# Create alert for high error rate
az monitor metrics alert create \
  --name "High Error Rate" \
  --resource-group $RESOURCE_GROUP \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Insights/components/$APP_INSIGHTS_NAME \
  --condition "count requests/failed > 10" \
  --window-size 5m \
  --evaluation-frequency 1m

# Create alert for high response time
az monitor metrics alert create \
  --name "High Response Time" \
  --resource-group $RESOURCE_GROUP \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Insights/components/$APP_INSIGHTS_NAME \
  --condition "avg requests/duration > 1000" \
  --window-size 5m \
  --evaluation-frequency 1m
```

### Step 2: Configure Diagnostic Logs

```bash
# Create Log Analytics Workspace
LOG_ANALYTICS_WORKSPACE="law-erp-prod"

az monitor log-analytics workspace create \
  --resource-group $RESOURCE_GROUP \
  --workspace-name $LOG_ANALYTICS_WORKSPACE

# Enable diagnostic settings for App Service
az monitor diagnostic-settings create \
  --name app-diagnostics \
  --resource /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Web/sites/$WEB_APP_NAME \
  --logs '[{"category": "AppServiceHTTPLogs", "enabled": true}, {"category": "AppServiceConsoleLogs", "enabled": true}]' \
  --metrics '[{"category": "AllMetrics", "enabled": true}]' \
  --workspace $LOG_ANALYTICS_WORKSPACE
```

### Step 3: Create Dashboard

```bash
# Create Azure Dashboard (use Azure Portal for visual creation)
# Or use ARM template for automated deployment
```

---

## Backup and Disaster Recovery

### Step 1: Configure App Service Backup

```bash
# Create storage account for backups
BACKUP_STORAGE_ACCOUNT="sterpbackup$(date +%s)"

az storage account create \
  --name $BACKUP_STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS

# Create container
az storage container create \
  --name appbackups \
  --account-name $BACKUP_STORAGE_ACCOUNT

# Configure backup (use Azure Portal or ARM template)
```

### Step 2: Test Database Restore

```bash
# Create database copy for testing
az sql db copy \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DATABASE_NAME \
  --dest-name "${SQL_DATABASE_NAME}-test" \
  --dest-resource-group $RESOURCE_GROUP \
  --dest-server $SQL_SERVER_NAME
```

### Step 3: Document Disaster Recovery Plan

Create `DISASTER_RECOVERY.md` with:
- RTO (Recovery Time Objective) targets
- RPO (Recovery Point Objective) targets
- Failover procedures
- Backup verification procedures
- Contact information

---

## Scaling Considerations

### Vertical Scaling (Scale Up)

```bash
# Upgrade App Service Plan
az appservice plan update \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --sku P1V3

# Upgrade SQL Database
az sql db update \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DATABASE_NAME \
  --service-objective S3
```

### Horizontal Scaling (Scale Out)

```bash
# Manual scale out
az appservice plan update \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --number-of-workers 5
```

### Database Scaling

```bash
# Enable read replicas for reporting
az sql db replica create \
  --name ${SQL_DATABASE_NAME}-readonly \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --source-database $SQL_DATABASE_NAME
```

---

## Cost Optimization

### Step 1: Right-Size Resources

```bash
# Analyze metrics to determine optimal sizing
az monitor metrics list \
  --resource /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Web/sites/$WEB_APP_NAME \
  --metric "CpuPercentage" \
  --start-time "2024-01-01" \
  --end-time "2024-01-31"
```

### Step 2: Enable Azure Hybrid Benefit

```bash
# If you have SQL Server licenses
az sql db update \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DATABASE_NAME \
  --license-type BasePrice
```

### Step 3: Configure Cost Alerts

```bash
# Create budget alert
az consumption budget create \
  --amount 1000 \
  --budget-name monthly-erp-budget \
  --category Cost \
  --time-grain Monthly \
  --start-date 2024-01-01 \
  --end-date 2025-12-31 \
  --resource-group $RESOURCE_GROUP
```

---

## Troubleshooting

### Common Issues

#### 1. Application Won't Start

```bash
# Check application logs
az webapp log tail \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP

# Check deployment logs
az webapp log deployment show \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP
```

#### 2. Database Connection Issues

```bash
# Test database connectivity
az sql db show-connection-string \
  --server $SQL_SERVER_NAME \
  --name $SQL_DATABASE_NAME \
  --client ado.net

# Check firewall rules
az sql server firewall-rule list \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME
```

#### 3. Slow Performance

```bash
# Check App Service metrics
az monitor metrics list \
  --resource /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Web/sites/$WEB_APP_NAME \
  --metric "AverageResponseTime" "CpuPercentage" "MemoryPercentage"

# Check SQL Database performance
az sql db show \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DATABASE_NAME \
  --query "[currentServiceObjectiveName, requestedServiceObjectiveName]"
```

#### 4. Key Vault Access Issues

```bash
# Verify managed identity
az webapp identity show \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP

# Check Key Vault access policies
az keyvault show \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "properties.accessPolicies"
```

---

## Post-Deployment Checklist

- [ ] Application is accessible via HTTPS
- [ ] Database migrations applied successfully
- [ ] Health check endpoint returns 200 OK
- [ ] Application Insights is receiving telemetry
- [ ] Key Vault integration working
- [ ] Authentication and authorization working
- [ ] Custom domain configured (if applicable)
- [ ] SSL certificate installed and valid
- [ ] Auto-scaling configured
- [ ] Backup configured and tested
- [ ] Monitoring alerts configured
- [ ] Diagnostic logs enabled
- [ ] Security headers configured
- [ ] CORS configured correctly
- [ ] Performance baseline established
- [ ] Disaster recovery plan documented
- [ ] Team trained on Azure resources
- [ ] Documentation updated with production URLs

---

## Additional Resources

### Microsoft Documentation

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/sql-database/)
- [Application Insights Documentation](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)
- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)

### Best Practices

- [Azure Well-Architected Framework](https://docs.microsoft.com/azure/architecture/framework/)
- [Security Best Practices](https://docs.microsoft.com/azure/security/fundamentals/best-practices-and-patterns)
- [Cost Optimization](https://docs.microsoft.com/azure/architecture/framework/cost/overview)

### Support

- [Azure Support Plans](https://azure.microsoft.com/support/plans/)
- [Azure Status](https://status.azure.com/)
- [Azure Community Support](https://docs.microsoft.com/answers/products/azure)

---

## Maintenance Tasks

### Daily
- Review Application Insights for errors
- Monitor application health checks
- Check Azure Service Health notifications

### Weekly
- Review cost analysis
- Analyze performance metrics
- Review security alerts

### Monthly
- Test backup restoration
- Review and update scaling rules
- Update dependencies and security patches
- Review access logs and audit trails

---

**Last Updated**: 2026-01-19
**Version**: 1.0
**Maintained By**: DevOps Team
