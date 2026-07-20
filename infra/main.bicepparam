using './main.bicep'

// -----------------------------------------------------------------------------
// Paramètres de l'environnement de production `rg-chronique-des-mondes-app`.
// Les Object IDs / SIDs sont des identifiants d'annuaire Entra ID (pas des secrets).
// Pour recréer un environnement neuf, remplacez les valeurs ci-dessous.
// -----------------------------------------------------------------------------

param location = 'francecentral'

// Administrateur Entra ID du serveur SQL (authentification AAD uniquement)
param sqlAadAdminLogin = 'tommy.angibaud@outlook.com'
param sqlAadAdminObjectId = '39d4670f-42d7-4c7c-b8ff-e2b5bf8f26f9'

// Workspace Log Analytics existant lié à Application Insights (évite d'en recréer un).
// Laisser vide sur un environnement neuf pour qu'un workspace dédié soit créé.
param logAnalyticsWorkspaceResourceId = '/subscriptions/065f4efd-7537-4678-9475-9cc1522dc19d/resourceGroups/DefaultResourceGroup-PAR/providers/Microsoft.OperationalInsights/workspaces/DefaultWorkspace-065f4efd-7537-4678-9475-9cc1522dc19d-PAR'

// Les attributions de rôles Key Vault existent déjà sur la prod : ne pas les recréer.
// Mettre à true pour un déploiement from-zero.
param assignKeyVaultRoles = false

// Domaines personnalisés : désactivés par défaut (à activer une fois le DNS en place)
param configureCustomDomains = false
