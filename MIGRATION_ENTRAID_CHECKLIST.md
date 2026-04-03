# ✅ Checklist Migration EntraID - Chronique des Mondes

## 📦 Ce qui a été fait automatiquement

### ✅ Packages installés
- [x] `Azure.Identity 1.20.0` ajouté à `Cdm.ApiService`
- [x] `Azure.Identity 1.20.0` ajouté à `Cdm.MigrationsManager`
- [x] `Azure.Identity 1.20.0` ajouté à `Cdm.Migrations`

### ✅ Fichiers de configuration créés/modifiés
- [x] `Cdm.ApiService/appsettings.json` - Ajout de `AzureConnection`
- [x] `Cdm.ApiService/appsettings.Production.json` - Configuration prod avec Managed Identity
- [x] `Cdm.MigrationsManager/appsettings.json` - Ajout de `AzureConnection`
- [x] `Cdm.MigrationsManager/appsettings.Production.json` - Configuration prod avec Managed Identity
- [x] `.github/workflows/azure-deploy.yml` - Pipeline mis à jour pour EntraID

### ✅ Documentation créée
- [x] `AZURE_ENTRAID_SETUP.md` - Guide complet de configuration Azure

---

## 🔧 À FAIRE MAINTENANT sur Azure Portal

### 1️⃣ Activer EntraID sur Azure SQL Server
**Temps estimé : 2 minutes**

1. Aller sur https://portal.azure.com
2. Chercher votre serveur SQL : `cdm-server-sql`
3. Dans le menu de gauche : **"Microsoft Entra ID"**
4. Cliquer **"Set admin"**
5. Sélectionner votre compte utilisateur
6. Cliquer **"Save"**

✅ **Résultat** : Vous pouvez maintenant vous connecter à Azure SQL avec votre compte EntraID

---

### 2️⃣ Vérifier/Créer l'App Registration
**Temps estimé : 3 minutes**

1. Aller dans **Microsoft Entra ID** → **App registrations**
2. **Si l'app existe déjà** : Notez les IDs (passez à l'étape 3)
3. **Si l'app n'existe pas** :
   - Cliquer **"New registration"**
   - Nom : `chronique-des-mondes-deploy`
   - Supported account types : `Single tenant`
   - Cliquer **"Register"**

📝 **Notez ces valeurs** :
- `Application (client) ID` → pour GitHub Secret `AZURE_CLIENT_ID`
- `Directory (tenant) ID` → pour GitHub Secret `AZURE_TENANT_ID`

---

### 3️⃣ Configurer Federated Identity pour GitHub Actions
**Temps estimé : 2 minutes**

1. Dans votre App Registration → **"Certificates & secrets"**
2. Onglet **"Federated credentials"**
3. Cliquer **"Add credential"**
4. Remplir :
   - Federated credential scenario : `GitHub Actions deploying Azure resources`
   - Organization : `<VotreUsernameGitHub>`
   - Repository : `Chronique_Des_Mondes`
   - Entity type : `Branch`
   - Branch name : `main`
   - Name : `github-main-branch`
5. Cliquer **"Add"**

✅ **Résultat** : GitHub Actions peut s'authentifier sans mot de passe

---

### 4️⃣ Donner accès à la base de données
**Temps estimé : 3 minutes**

**Option A : Via Azure Portal (plus simple)**
1. Aller sur votre base de données Azure SQL
2. Cliquer **"Query editor"** (dans le menu de gauche)
3. Se connecter avec **"Microsoft Entra authentication"**
4. Exécuter ce SQL :
```sql
CREATE USER [chronique-des-mondes-deploy] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [chronique-des-mondes-deploy];
```

**Option B : Via Azure Data Studio / SSMS**
1. Installer Azure Data Studio si nécessaire
2. Se connecter à `cdm-server-sql.database.windows.net` avec **Azure Active Directory**
3. Exécuter le même SQL ci-dessus

✅ **Résultat** : L'App Registration peut exécuter les migrations

---

### 5️⃣ Donner permissions Azure à l'App Registration
**Temps estimé : 2 minutes**

1. Aller dans votre **Subscription** Azure
2. Cliquer **"Access control (IAM)"** (menu de gauche)
3. Cliquer **"Add" → "Add role assignment"**
4. Chercher et sélectionner : **"SQL DB Contributor"**
5. Cliquer **"Next"**
6. Cliquer **"Select members"**
7. Chercher : `chronique-des-mondes-deploy`
8. Cliquer **"Select"** puis **"Review + assign"**

✅ **Résultat** : L'App Registration peut accéder aux ressources Azure SQL

---

### 6️⃣ Vérifier les GitHub Secrets
**Temps estimé : 1 minute**

1. Aller sur GitHub : `https://github.com/VotreUsername/Chronique_Des_Mondes`
2. **Settings** → **Secrets and variables** → **Actions**
3. Vérifier que vous avez ces 3 secrets :

| Secret Name | Valeur |
|-------------|--------|
| `AZURE_CLIENT_ID` | Application (client) ID de l'étape 2 |
| `AZURE_TENANT_ID` | Directory (tenant) ID de l'étape 2 |
| `AZURE_SUBSCRIPTION_ID` | Votre Subscription ID Azure |

❌ **À SUPPRIMER** : `SQL_PASSWORD` (plus nécessaire !)

---

### 7️⃣ Configurer Managed Identity pour les App Services
**Temps estimé : 5 minutes**

**Pour l'API (app-chroniquedesmondes-api) :**

1. Aller sur Azure Portal → Votre App Service `app-chroniquedesmondes-api`
2. Menu de gauche : **"Identity"**
3. Onglet **"System assigned"**
4. Status : **"On"**
5. Cliquer **"Save"**
6. **Copier l'Object ID** qui apparaît

**Donner accès à la base de données :**
1. Ouvrir Query Editor sur votre base de données
2. Exécuter :
```sql
CREATE USER [app-chroniquedesmondes-api] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [app-chroniquedesmondes-api];
```

**Répéter pour le Web si nécessaire**

✅ **Résultat** : Vos App Services peuvent se connecter à la BDD sans mot de passe

---

## 🧪 Test de la configuration

### Test en local (optionnel)
```bash
# Installer Azure CLI si pas fait
winget install Microsoft.AzureCLI

# Se connecter à Azure
az login

# Tester la connexion à Azure SQL
# Modifiez temporairement appsettings.Development.json pour utiliser AzureConnection
# Puis lancez votre API
```

### Test dans GitHub Actions
1. Faire un commit et push sur `main`
2. Aller sur GitHub → **Actions**
3. Regarder le workflow **"Build and deploy to Azure - Web & API"**
4. Vérifier que l'étape **"Run database migrations with EntraID"** passe en vert ✅

---

## 🆘 Troubleshooting

### Erreur dans le pipeline : "Cannot create the user from external provider"
→ Vous n'avez pas configuré l'admin EntraID sur SQL Server (étape 1)

### Erreur : "Login failed for user 'NT AUTHORITY\ANONYMOUS LOGON'"
→ L'App Registration n'a pas été ajoutée comme utilisateur SQL (étape 4)

### Erreur : "AADSTS70021: No matching federated identity"
→ Le Federated Credential n'est pas configuré correctement (vérifiez le nom du repo/branch à l'étape 3)

### Erreur en production : "Login failed"
→ La Managed Identity de l'App Service n'a pas été activée (étape 7)

---

## 📊 Résumé des environnements

| Environnement | Authentification | Connection String |
|--------------|------------------|-------------------|
| **Local Dev** | Integrated Security (localdb) | `Server=(localdb)\\mssqllocaldb;...;Integrated Security=True` |
| **Local Azure** | Azure CLI (`az login`) | `Server=cdm-server-sql...;Authentication=Active Directory Default` |
| **CI/CD (GitHub Actions)** | App Registration via Federated Identity | `Server=cdm-server-sql...;Authentication=Active Directory Access Token` |
| **Production (App Service)** | Managed Identity | `Server=cdm-server-sql...;Authentication=Active Directory Managed Identity` |

---

## ✅ Validation finale

Une fois tout fait, cochez :

- [ ] Étape 1 : EntraID activé sur SQL Server
- [ ] Étape 2 : App Registration créée/vérifiée
- [ ] Étape 3 : Federated Identity configurée
- [ ] Étape 4 : App Registration a accès à la BDD
- [ ] Étape 5 : App Registration a les permissions Azure
- [ ] Étape 6 : GitHub Secrets vérifiés
- [ ] Étape 7 : Managed Identity configurée pour App Services
- [ ] Test : Pipeline GitHub Actions réussit
- [ ] Test : Application en production se connecte à la BDD

**🎉 Félicitations ! Votre infrastructure est maintenant sécurisée avec EntraID !**
