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
| `modules/hostname-ssl-binding.bicep` | Re-liaison d'un nom d'hôte en SNI SSL une fois son certificat émis. |

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
- **Domaines personnalisés** de la Web App : `chroniques-des-mondes.fr` et
  `www.chroniques-des-mondes.fr`, chacun avec son **certificat managé App Service**
  (gratuit, renouvellement automatique) lié en **SNI SSL**

## Domaine public

Le site est servi sur **`https://chroniques-des-mondes.fr`** (et `www.…`). Le domaine est
enregistré chez **OVH**, dont les serveurs DNS (`ns110.ovh.net` / `dns110.ovh.net`) font
autorité. La zone doit contenir :

| Enregistrement | Type | Valeur | Rôle |
|----------------|------|--------|------|
| `@` | `A` | IP entrante de la Web App (`az webapp show … --query inboundIpAddress`) | Domaine racine → App Service |
| `asuid` | `TXT` | `customDomainVerificationId` de la Web App | Preuve de propriété de la racine |
| `www` | `CNAME` | `app-chronique-des-mondes-web.azurewebsites.net` | Sous-domaine www → App Service |
| `asuid.www` | `TXT` | même `customDomainVerificationId` | Preuve de propriété du www |

> ⚠️ L'IP entrante d'un App Service peut changer si l'app est supprimée/recréée ou déplacée
> de plan. Après une telle opération, remettre l'enregistrement `A` à jour chez OVH, sinon
> la racine tombe (le `www`, en CNAME, suit automatiquement).

**Le point qui casse tout et ne se voit pas :** un nom d'hôte peut être lié à l'app *sans*
certificat. Le DNS répond, mais `httpsOnly` redirige vers HTTPS et le navigateur refuse la
connexion — le domaine paraît mort alors que l'app tourne. Vérification :

```bash
az webapp config hostname list -g rg-chronique-des-mondes-app \
  --webapp-name app-chronique-des-mondes-web -o table   # sslState doit valoir SniEnabled partout
```

Réparation (le certificat managé existe déjà mais n'est pas lié) :

```bash
az webapp config ssl bind -g rg-chronique-des-mondes-app -n app-chronique-des-mondes-web \
  --certificate-thumbprint <empreinte> --ssl-type SNI
```

S'il n'existe pas encore :

```bash
az webapp config ssl create -g rg-chronique-des-mondes-app \
  --name app-chronique-des-mondes-web --hostname chroniques-des-mondes.fr
```

## Ce qui est volontairement EXCLU du Bicep

- **Le secret `AzureEmail--ConnectionString`** : le Bicep crée le coffre et les droits,
  mais **pas le secret**. Il est posé à la main ou par un pipeline dédié :

  ```bash
  az keyvault secret set \
    --vault-name kv-chronique-mondes \
    --name "AzureEmail--ConnectionString" \
    --value "<chaîne de connexion ACS>"
  ```

- **La zone DNS elle-même** : elle vit chez OVH, hors d'Azure. Le Bicep décrit les liaisons
  et les certificats côté App Service, jamais les enregistrements DNS.

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
| `configureCustomDomains` | `true` — la prod sert bien les deux domaines | `false` tant que le DNS ne pointe pas encore vers l'app |
| `configureManagedCertificates` | `true` — certificats managés liés en SNI SSL | `false` pour lier les domaines sans HTTPS (transitoire uniquement) |

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
