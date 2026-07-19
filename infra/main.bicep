// =============================================================================
// Chronique des Mondes — Infrastructure as Code (Bicep)
// -----------------------------------------------------------------------------
// Reflet fidèle et versionné du groupe de ressources `rg-chronique-des-mondes-app`.
// Objectif : recréer l'environnement Azure de zéro sans dépendre de la CLI ou du
// portail, et disposer d'une source de vérité unique.
//
// Déploiement :
//   az deployment group create \
//     -g rg-chronique-des-mondes-app \
//     -f infra/main.bicep \
//     -p infra/main.bicepparam
//
// Secrets EXCLUS volontairement (voir infra/README.md) :
//   - kv-chronique-mondes/AzureEmail--ConnectionString : posé à la main / par pipeline.
//     Le Bicep crée le coffre et les droits, jamais le secret.
// =============================================================================

targetScope = 'resourceGroup'

// -----------------------------------------------------------------------------
// Paramètres — aucune valeur d'infrastructure n'est codée en dur dans les ressources
// -----------------------------------------------------------------------------

@description('Région de déploiement de toutes les ressources régionales.')
param location string = 'francecentral'

@description('Identifiant du locataire Entra ID (par défaut celui de l\'abonnement).')
param tenantId string = subscription().tenantId

// --- Nommage (défauts = noms réels de l\'environnement de production) ---------
@description('Nom du plan App Service (Linux).')
param appServicePlanName string = 'ASP-rgchroniquedesmondesapp-bacd'

@description('Nom de la Web App Blazor (front).')
param webAppName string = 'app-chronique-des-mondes-web'

@description('Nom de l\'API App Service.')
param apiAppName string = 'app-chroniquedesmondes-api'

@description('Nom de la ressource Application Insights (liée à l\'API).')
param appInsightsName string = 'app-chroniquedesmondes-api'

@description('Nom du serveur SQL logique.')
param sqlServerName string = 'cdm-server-sql'

@description('Nom de la base de données SQL.')
param sqlDatabaseName string = 'cdm-bdd-sql'

@description('Nom de l\'Email Communication Service.')
param emailServiceName string = 'cdm-email'

@description('Nom du Communication Service.')
param communicationServiceName string = 'cdm-communication'

@description('Nom du Key Vault.')
param keyVaultName string = 'kv-chronique-mondes'

// --- SKU / dimensionnement ---------------------------------------------------
@description('SKU du plan App Service.')
param appServicePlanSku string = 'B1'

@description('Runtime Linux des App Services.')
param linuxFxVersion string = 'DOTNETCORE|10.0'

@description('SKU de la base SQL (nom de niveau).')
param sqlDatabaseSkuName string = 'Basic'

@description('Taille maximale de la base SQL en octets (2 Go par défaut).')
param sqlDatabaseMaxSizeBytes int = 2147483648

@description('Collation de la base de données.')
param sqlDatabaseCollation string = 'French_BIN'

// --- Administration AAD du SQL (identifiants d\'annuaire, non secrets) --------
@description('Login de l\'administrateur Entra ID du serveur SQL (authentification AAD uniquement).')
param sqlAadAdminLogin string

@description('Object ID (SID) de l\'administrateur Entra ID du serveur SQL.')
param sqlAadAdminObjectId string

// --- RBAC Key Vault ----------------------------------------------------------
@description('Object ID du principal humain "dev" recevant le rôle Key Vault Secrets Officer. Par défaut, l\'administrateur SQL.')
#disable-next-line secure-secrets-in-params // Object ID d'annuaire, pas un secret
param keyVaultSecretsOfficerObjectId string = sqlAadAdminObjectId

@description('Créer les attributions de rôles Key Vault. Mettre à false quand elles existent déjà (évite l\'erreur RoleAssignmentExists sur un environnement en place).')
param assignKeyVaultRoles bool = true

// --- Domaines personnalisés (désactivés pour un déploiement from-zero) --------
@description('Configurer les domaines personnalisés + certificats managés sur la Web App. Nécessite que le DNS pointe déjà vers l\'app.')
param configureCustomDomains bool = false

@description('Domaine racine personnalisé de la Web App.')
param customDomainRoot string = 'chroniques-des-mondes.fr'

@description('Sous-domaine www de la Web App.')
param customDomainWww string = 'www.chroniques-des-mondes.fr'

// --- Journalisation / observabilité ------------------------------------------
@description('Resource ID d\'un workspace Log Analytics existant à lier à Application Insights. Laisser vide pour en créer un dédié.')
param logAnalyticsWorkspaceResourceId string = ''

// -----------------------------------------------------------------------------
// Constantes (IDs de rôles intégrés Azure)
// -----------------------------------------------------------------------------
var roleKeyVaultSecretsOfficer = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
var roleKeyVaultSecretsUser = '4633458b-17de-408a-b874-0445c86b69e6'

var createWorkspace = empty(logAnalyticsWorkspaceResourceId)

// =============================================================================
// Observabilité
// =============================================================================

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = if (createWorkspace) {
  name: 'log-${sqlServerName}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Redfield'
    Request_Source: 'IbizaWebAppExtensionCreate'
    IngestionMode: 'LogAnalytics'
    RetentionInDays: 90
    WorkspaceResourceId: createWorkspace ? logAnalytics.id : logAnalyticsWorkspaceResourceId
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// =============================================================================
// Key Vault (RBAC)
// =============================================================================

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    publicNetworkAccess: 'Enabled'
  }
}

// dev humain : Secrets Officer (lecture/écriture des secrets)
resource kvOfficerAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (assignKeyVaultRoles) {
  name: guid(keyVault.id, keyVaultSecretsOfficerObjectId, roleKeyVaultSecretsOfficer)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleKeyVaultSecretsOfficer)
    principalId: keyVaultSecretsOfficerObjectId
    principalType: 'User'
  }
}

// identité managée de l'API : Secrets User (lecture seule)
resource kvApiUserAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (assignKeyVaultRoles) {
  name: guid(keyVault.id, apiApp.id, roleKeyVaultSecretsUser)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleKeyVaultSecretsUser)
    principalId: apiApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// =============================================================================
// SQL Server + base (authentification Entra ID uniquement)
// =============================================================================

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: sqlAadAdminLogin
      sid: sqlAadAdminObjectId
      tenantId: tenantId
      principalType: 'User'
    }
  }
}

resource sqlAadOnlyAuth 'Microsoft.Sql/servers/azureADOnlyAuthentications@2023-08-01-preview' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: true
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: sqlDatabaseSkuName
    tier: sqlDatabaseSkuName
  }
  properties: {
    collation: sqlDatabaseCollation
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: sqlDatabaseMaxSizeBytes
    requestedBackupStorageRedundancy: 'Geo'
    zoneRedundant: false
  }
}

// Autorise les services Azure (App Services) à joindre le serveur SQL.
resource sqlAllowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// =============================================================================
// Communication Services (email)
// =============================================================================

resource emailService 'Microsoft.Communication/emailServices@2023-04-01' = {
  name: emailServiceName
  location: 'global'
  properties: {
    dataLocation: 'France'
  }
}

resource emailDomain 'Microsoft.Communication/emailServices/domains@2023-04-01' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource communicationService 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: communicationServiceName
  location: 'global'
  properties: {
    dataLocation: 'France'
    linkedDomains: [
      emailDomain.id
    ]
  }
}

// =============================================================================
// App Service Plan + App Services (Blazor web + API)
// =============================================================================

resource appServicePlan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: appServicePlanSku
  }
  properties: {
    reserved: true // Linux
  }
}

resource apiApp 'Microsoft.Web/sites@2024-11-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    reserved: true
    keyVaultReferenceIdentity: 'SystemAssigned'
    publicNetworkAccess: 'Enabled'
    // Les app settings (APPLICATIONINSIGHTS_CONNECTION_STRING, ConnectionStrings,
    // références Key Vault…) sont gérés hors Bicep par le pipeline de déploiement,
    // pour ne jamais écraser des valeurs sensibles lors d'un apply.
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      alwaysOn: false
      http20Enabled: false
    }
  }
  tags: {
    'hidden-link: /app-insights-resource-id': appInsights.id
  }
}

resource webApp 'Microsoft.Web/sites@2024-11-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    reserved: true
    keyVaultReferenceIdentity: 'SystemAssigned'
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      alwaysOn: false
      http20Enabled: false
    }
  }
}

// --- Domaines personnalisés + certificats managés (optionnels) ---------------
// Nécessitent que le DNS (A/CNAME + verification) pointe déjà vers la Web App.
resource webAppWwwBinding 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = if (configureCustomDomains) {
  parent: webApp
  name: customDomainWww
  properties: {
    siteName: webAppName
    hostNameType: 'Verified'
    sslState: 'SniEnabled'
  }
}

resource webAppRootBinding 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = if (configureCustomDomains) {
  parent: webApp
  name: customDomainRoot
  properties: {
    siteName: webAppName
    hostNameType: 'Verified'
  }
}

// =============================================================================
// Sorties
// =============================================================================

output apiAppPrincipalId string = apiApp.identity.principalId
output apiAppDefaultHostName string = apiApp.properties.defaultHostName
output webAppDefaultHostName string = webApp.properties.defaultHostName
output keyVaultUri string = keyVault.properties.vaultUri
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output appInsightsConnectionString string = appInsights.properties.ConnectionString
