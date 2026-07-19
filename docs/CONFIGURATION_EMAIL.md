# Configuration de l'envoi d'emails (Azure Communication Services + Key Vault)

Nécessaire pour le parcours « mot de passe oublié ». **Sans cette configuration, l'application
fonctionne quand même** : le service bascule automatiquement sur `LoggingEmailService`, et le lien
de réinitialisation apparaît dans les logs de l'API au lieu d'être envoyé.

> ✅ **Statut : opérationnel** (validé le 19/07/2026, mail de réinitialisation reçu).
> Ressources créées dans `rg-chronique-des-mondes-app` : `cdm-email` (Email Communication
> Service), son domaine `AzureManagedDomain`, `cdm-communication` (Communication Services),
> et le coffre `kv-chronique-mondes` (Key Vault).

---

## 1. Architecture retenue

Le **seul secret** (la chaîne de connexion ACS) vit dans **Azure Key Vault**, pas dans la
config. L'API le lit au démarrage via `DefaultAzureCredential` :
- **en production** : identité managée de l'App Service `app-chroniquedesmondes-api` ;
- **en local** : le compte `az login` du développeur.

Le reste (non secret) est en clair dans `appsettings.json`.

| Clé de configuration | Où | Rôle |
|---|---|---|
| `AzureEmail:ConnectionString` | **Key Vault** (secret `AzureEmail--ConnectionString`) | Chaîne de connexion ACS |
| `AzureEmail:FromAddress` | `appsettings.json` | Adresse expéditrice `DoNotReply@<guid>.azurecomm.net` |
| `App:WebBaseUrl` | `appsettings.json` (+ Development) | Base d'URL du front pour le lien de reset |
| `KeyVault:Uri` | `appsettings.json` | `https://kv-chronique-mondes.vault.azure.net/` |
| `KeyVault:TenantId` | `appsettings.json` | `fc672530-…` — force le bon tenant (poste multi-comptes) |

Le branchement est dans `Cdm.ApiService/Program.cs` : si `KeyVault:Uri` est renseigné, les
secrets du coffre alimentent la configuration (`AzureEmail--ConnectionString` →
`AzureEmail:ConnectionString`). Si la chaîne reste absente, `LoggingEmailService` prend le relais.

### Convention de nommage des secrets Key Vault
`:` (hiérarchie de config) → `--` dans le nom du secret. Donc `AzureEmail:ConnectionString`
se stocke sous `AzureEmail--ConnectionString`.

### Rôles RBAC nécessaires
- Développeur (toi) : **Key Vault Secrets Officer** sur le coffre (lire + écrire).
- Identité managée de l'API : **Key Vault Secrets User** (lecture seule).

---

## Annexe — ancienne piste (User Secrets)

Historiquement documentée ci-dessous, mais **remplacée par Key Vault**. Conservée pour référence.

---

## 2. Créer les ressources (Azure CLI)

```bash
# Connexion + extension
az login
az extension add --name communication

# Variables (à adapter)
RG="rg-chroniquedesmondes"
LOC="global"
DATA_LOC="europe"          # emplacement de stockage des données
EMAIL_SVC="cdm-email"
ACS="cdm-communication"

# a) Ressource Email Communication Service
az communication email create \
  --name "$EMAIL_SVC" \
  --resource-group "$RG" \
  --location "$LOC" \
  --data-location "$DATA_LOC"

# b) Domaine géré par Azure (gratuit, préconfiguré SPF/DKIM)
#    Pour un domaine Azure, ces deux valeurs sont imposées :
#    --domain-name AzureManagedDomain  et  --domain-management AzureManaged
az communication email domain create \
  --domain-name AzureManagedDomain \
  --email-service-name "$EMAIL_SVC" \
  --resource-group "$RG" \
  --location "$LOC" \
  --domain-management AzureManaged

# c) Ressource Communication Services
az communication create \
  --name "$ACS" \
  --resource-group "$RG" \
  --location "$LOC" \
  --data-location "$DATA_LOC"

# d) Lier le domaine à la ressource Communication Services
DOMAIN_ID=$(az communication email domain show \
  --domain-name AzureManagedDomain \
  --email-service-name "$EMAIL_SVC" \
  --resource-group "$RG" \
  --query id -o tsv)

az communication update \
  --name "$ACS" \
  --resource-group "$RG" \
  --linked-domains "$DOMAIN_ID"
```

### Récupérer la chaîne de connexion et l'adresse d'expédition

```bash
# Chaîne de connexion (SECRET — ne pas committer, ne pas partager)
az communication list-key --name "$ACS" --resource-group "$RG" --query primaryConnectionString -o tsv

# Domaine généré (l'expéditeur sera donotreply@<ce-domaine>)
az communication email domain show \
  --domain-name AzureManagedDomain \
  --email-service-name "$EMAIL_SVC" \
  --resource-group "$RG" -o json
```

Le domaine Azure généré ressemble à `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.azurecomm.net`,
et l'expéditeur par défaut est `donotreply@` suivi de ce domaine.

---

## 3. Où mettre les secrets

### En local — User Secrets (jamais dans `appsettings.json`)

```bash
cd Cdm/Cdm.ApiService
dotnet user-secrets init
dotnet user-secrets set "AzureEmail:ConnectionString" "<la-chaine-de-connexion>"
dotnet user-secrets set "AzureEmail:FromAddress" "donotreply@<guid>.azurecomm.net"
dotnet user-secrets set "App:WebBaseUrl" "https://localhost:7165"
```

> ⚠️ Ne mets **jamais** la chaîne de connexion dans `appsettings.json` : le fichier est versionné.
> C'était précisément le point 🔴 n°2 de l'audit (clé JWT committée).

### En production — App Settings / Key Vault

```bash
az webapp config appsettings set \
  --name "<ton-app-service>" \
  --resource-group "$RG" \
  --settings \
    "AzureEmail__ConnectionString=<la-chaine>" \
    "AzureEmail__FromAddress=donotreply@<guid>.azurecomm.net" \
    "App__WebBaseUrl=https://<ton-domaine-prod>"
```

> Le double underscore `__` est la notation ASP.NET Core pour la hiérarchie (`:`) en variable
> d'environnement. Idéalement, référence plutôt un secret Key Vault que la valeur en clair.

---

## 4. Vérifier

1. Démarre l'API et va sur `/forgot-password`.
2. Saisis l'adresse d'un compte existant.
3. **Sans configuration** : le lien apparaît dans les logs de l'API (`[Email non configuré] …`).
4. **Avec configuration** : l'email arrive dans la boîte du compte.

Rappel : la page répond toujours la même chose que l'adresse existe ou non — c'est volontaire
(anti-énumération de comptes). Pour savoir si l'envoi a réussi, regarde les logs de l'API.

---

## Sources

- [Add Azure Managed Domains to Email Communication Service](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/add-azure-managed-domains)
- [Connect a verified email domain to send email](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/connect-email-communication-resource)
- [az communication email domain](https://learn.microsoft.com/en-us/cli/azure/communication/email/domain?view=azure-cli-latest)
- [az communication](https://learn.microsoft.com/en-us/cli/azure/communication?view=azure-cli-latest)
