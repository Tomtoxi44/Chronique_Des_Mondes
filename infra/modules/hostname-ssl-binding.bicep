// =============================================================================
// Liaison SNI SSL d'un nom d'hôte personnalisé sur une Web App
// -----------------------------------------------------------------------------
// Module séparé à cause d'une dépendance circulaire côté ARM : un certificat
// managé App Service ne peut être émis que si le nom d'hôte est DÉJÀ lié à
// l'app (sans SSL), alors que la liaison SSL exige l'empreinte du certificat.
// `main.bicep` crée donc la liaison nue, puis le certificat, puis appelle ce
// module pour repasser sur la même liaison en SniEnabled.
// =============================================================================

@description('Nom de la Web App portant le nom d\'hôte.')
param webAppName string

@description('Nom d\'hôte personnalisé à lier (ex. www.exemple.fr).')
param hostName string

@description('Empreinte du certificat managé à lier en SNI SSL.')
param certificateThumbprint string

resource webApp 'Microsoft.Web/sites@2024-11-01' existing = {
  name: webAppName
}

resource sslBinding 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = {
  parent: webApp
  name: hostName
  properties: {
    siteName: webAppName
    hostNameType: 'Verified'
    sslState: 'SniEnabled'
    thumbprint: certificateThumbprint
  }
}
