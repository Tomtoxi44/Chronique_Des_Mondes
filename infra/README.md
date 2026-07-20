# Infrastructure as Code — Chronique des Mondes

Ce dossier contient le **template Bicep** décrivant l'environnement Azure du projet.
Il est la **source de vérité versionnée** du groupe de ressources `rg-chronique-des-mondes-app`
(région `francecentral`) et permet de **recréer l'environnement de zéro** sans dépendre de la CLI
au coup par coup ni du portail.

## Fichiers

| Fichier | Rôle |
|---------|------|
| `main.bicep` | Description complète des ressources (paramétrée, rien en dur). |
| `main.bicepparam` | Valeurs de l'environnement de production. |

## Ressources décrites

- **App Service Plan** `ASP-rgchroniquedesmondesapp-bacd` (Linux, B1)
- **Web App** `app-chronique-des-mondes-web` (Blazor, `DOTNETCORE|10.0`)
- **API App** `app-chroniquedesmondes-api` (**identité managée SystemAssigned**, accès Key Vault)
- **Application Insights** `app-chroniquedesmondes-api` (lié à un workspace Log Analytics)
- **SQL Server** `cdm-server-sql` (**authentification Entra ID uniquement**) + base `cdm-bdd-sql` (Basic)
- **Email Communication Service** `cdm-email` + domaine `AzureManagedDomain`
- **Communication Services** `cdm-communication` (domaine lié)
- **Key Vault** `kv-chronique-mondes` (RBAC) + attributions de rôles :
  - dev humain → **Key Vault Secrets Officer**
  - identité managée de l'API → **Key Vault Secrets User**

## Ce qui est volontairement EXCLU du Bicep

- **Le secret `AzureEmail--ConnectionString`** : le Bicep crée le coffre et les droits,
  mais **pas le secret**. Il est posé à la main ou par un pipeline dédié :

  ```bash
  az keyvault secret set \
    --vault-name kv-chronique-mondes \
    --name "AzureEmail--ConnectionString" \
    --value "<chaîne de connexion ACS>"
  ```

- **Les domaines personnalisés + certificats managés** (`chroniques-des-mondes.fr`, `www.…`) :
  désactivés par défaut (`configureCustomDomains = false`) car ils exigent que le DNS pointe
  déjà vers la Web App. À activer une fois l'enregistrement DNS et la vérification en place.

- **Les app settings des App Services** (`APPLICATIONINSIGHTS_CONNECTION_STRING`, `ConnectionStrings`,
  références Key Vault…) : gérés par le pipeline de déploiement, jamais par le Bicep, pour éviter
  d'écraser des valeurs sensibles lors d'un `apply`.

- Les ressources auto-gérées par Azure (règles Smart Detection, advisors SQL, historique de
  déploiement, politiques d'audit par défaut) ne sont pas reproduites : elles se recréent seules.

### Paramètres à connaître

| Paramètre | Prod (`main.bicepparam`) | From-zero |
|-----------|--------------------------|-----------|
| `assignKeyVaultRoles` | `false` — les rôles existent déjà | `true` |
| `logAnalyticsWorkspaceResourceId` | workspace `DefaultWorkspace-…-PAR` existant | `''` → crée un workspace dédié |
| `configureCustomDomains` | `false` | `true` une fois le DNS en place |

## Déploiement

```bash
# Prévisualiser les changements (recommandé avant tout apply)
az deployment group what-if \
  -g rg-chronique-des-mondes-app \
  -f infra/main.bicep \
  -p infra/main.bicepparam

# Appliquer
az deployment group create \
  -g rg-chronique-des-mondes-app \
  -f infra/main.bicep \
  -p infra/main.bicepparam
```

> Le déploiement est **idempotent** : relancé sur l'environnement existant, il ne recrée rien
> mais réaligne la configuration sur le template.

## Recréer un environnement neuf (from-zero)

```bash
az group create -n <nouveau-rg> -l francecentral
az deployment group create -g <nouveau-rg> -f infra/main.bicep \
  -p infra/main.bicepparam \
  -p sqlAadAdminLogin=<admin> -p sqlAadAdminObjectId=<objectId>
```

Puis poser le secret ACS (voir plus haut) et, si besoin, activer les domaines personnalisés.

## Maintenir le Bicep à jour

Quand une ressource est ajoutée/modifiée **à la main** dans Azure, resynchroniser le Bicep :

```bash
# 1. Exporter l'existant
az group export -g rg-chronique-des-mondes-app --skip-all-params > _exported.json

# 2. Convertir en Bicep pour comparer
az bicep decompile --file _exported.json

# 3. Reporter à la main les changements pertinents dans main.bicep, puis :
rm _exported.json _exported.bicep

# 4. Vérifier qu'aucune dérive ne subsiste
az deployment group what-if -g rg-chronique-des-mondes-app \
  -f infra/main.bicep -p infra/main.bicepparam
```

Un `what-if` qui ne remonte **aucun changement** signifie que le Bicep reflète fidèlement l'infra.
