# Azure Infrastructure Skill

## Purpose
Set up Azure infrastructure using Bicep or ARM templates.

## Key Azure Resources

- **App Service**: Host the web application
- **Azure SQL Database**: Production database
- **Key Vault**: Store secrets
- **Application Insights**: Monitoring and logging
- **Storage Account**: File storage, backups

## Bicep Template Example

```bicep
param location string = resourceGroup().location
param environment string = 'dev'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'erp-plan-${environment}'
  location: location
  sku: {
    name: 'P1v3'
    tier: 'PremiumV3'
  }
  properties: {
    reserved: false
  }
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'erp-app-${environment}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      alwaysOn: true
      minTlsVersion: '1.2'
    }
  }
}

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: 'erp-sql-${environment}'
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: keyVault.getSecret('SqlAdminPassword')
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: 'erp-kv-${environment}'
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
  }
}
```

## Related Skills
- setup-cicd
- docker-setup
