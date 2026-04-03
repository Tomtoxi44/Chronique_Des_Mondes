# 🔐 Configuration Azure SQL avec EntraID

## ✅ Étapes à suivre dans Azure Portal

### 1️⃣ Activer l'authentification EntraID sur Azure SQL

1. Allez sur le **portail Azure** → votre serveur SQL `cdm-server-sql.database.windows.net`
2. Dans le menu de gauche, cliquez sur **"Microsoft Entra ID"** (anciennement Azure Active Directory)
3. Cliquez sur **"Set admin"**
4. Sélectionnez votre compte utilisateur comme administrateur EntraID
5. Cliquez **"Save"**

### 2️⃣ Configurer GitHub Actions avec Federated Identity (Pas de secret!)

**Option recommandée : Workload Identity Federation** (authentification sans secret)

1. Allez dans **Microsoft Entra ID** → **App registrations**
2. Cliquez sur votre application `chronique-des-mondes-deploy` (ou créez-la si elle n'existe pas)
3. Notez :
   - **Application (client) ID** → Ce sera `AZURE_CLIENT_ID`
   - **Directory (tenant) ID** → Ce sera `AZURE_TENANT_ID`

4. Allez dans **"Certificates & secrets"** → **"Federated credentials"**
5. Cliquez **"Add credential"**
6. Configurez :
   - **Federated credential scenario** : `GitHub Actions deploying Azure resources`
   - **Organization** : `VotreUsername` (votre nom d'utilisateur GitHub)
   - **Repository** : `Chronique_Des_Mondes` (nom de votre repo)
   - **Entity type** : `Branch`
   - **GitHub branch name** : `main`
   - **Name** : `github-main-branch`

7. Allez dans **"API permissions"** → Vérifiez que vous avez :
   - `Azure SQL Database` → `user_impersonation`

### 3️⃣ Donner les permissions à l'App Registration sur Azure SQL

Connectez-vous à votre base de données Azure SQL avec **Azure Data Studio** ou **SSMS** en utilisant **EntraID** :

```sql
-- Remplacez 'chronique-des-mondes-deploy' par le nom de votre App Registration
CREATE USER [chronique-des-mondes-deploy] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [chronique-des-mondes-deploy];
```

### 4️⃣ Ajouter les permissions Azure à l'App Registration

1. Allez dans votre **Subscription** Azure
2. Cliquez **"Access control (IAM)"**
3. Cliquez **"Add" → "Add role assignment"**
4. Sélectionnez le rôle **"SQL DB Contributor"**
5. Assignez à votre App Registration `chronique-des-mondes-deploy`

### 5️⃣ Configurer les Secrets GitHub Actions

Vous avez déjà ces secrets (vérifiez qu'ils sont corrects) :

- `AZURE_CLIENT_ID` : L'Application (client) ID de votre App Registration
- `AZURE_TENANT_ID` : Le Directory (tenant) ID
- `AZURE_SUBSCRIPTION_ID` : Votre Subscription ID Azure

**Plus besoin de `SQL_PASSWORD` !** 🎉

---

## 🔍 Vérification

Pour vérifier que tout fonctionne, testez la connexion en local :

```bash
# Installez Azure CLI si ce n'est pas fait
winget install Microsoft.AzureCLI

# Connectez-vous
az login

# Obtenez un token d'accès pour SQL
az account get-access-token --resource https://database.windows.net/ --query accessToken -o tsv
```

---

## 📝 Notes importantes

- **En local** : Utilisez `az login` puis la connexion utilisera automatiquement votre identité
- **En CI/CD** : GitHub Actions utilisera l'App Registration avec Federated Identity
- **En production (Azure App Service)** : Activez la Managed Identity et ajoutez-la comme utilisateur SQL

---

## 🆘 Troubleshooting

### Erreur "Login failed for user 'NT AUTHORITY\ANONYMOUS LOGON'"
→ L'App Registration n'a pas été ajoutée comme utilisateur SQL (étape 3)

### Erreur "AADSTS700016: Application not found"
→ Vérifiez que AZURE_CLIENT_ID est correct et que l'app existe

### Erreur "AADSTS70021: No matching federated identity"
→ Vérifiez que le Federated Credential est configuré pour le bon repository/branch

### Erreur "Cannot create the user from external provider"
→ Vous n'êtes pas connecté avec un compte EntraID Admin sur SQL Server (étape 1)
