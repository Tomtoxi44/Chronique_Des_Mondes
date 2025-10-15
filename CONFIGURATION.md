# 🔧 Guide de Configuration - Chronique des Mondes

## 📋 Vue d'ensemble

Ce document explique comment configurer les **secrets** et **variables d'environnement** pour le projet Chronique des Mondes avant de le partager publiquement.

---

## 🔒 Secrets à protéger

Avant de rendre votre repository public, assurez-vous que ces informations **ne sont jamais committées** :

1. **Connection String SQL Server** - Identifiants de base de données
2. **JWT Secret Key** - Clé de signature des tokens (32+ caractères)
3. **Azure Communication Services** - Clé d'accès pour l'envoi d'emails
4. **Mots de passe** - Tous types de credentials

---

## 🛠️ Configuration pour le développement local

### Option 1 : User Secrets .NET (Recommandée)

Les User Secrets sont stockés **en dehors du projet** dans votre profil utilisateur et ne sont **jamais committés**.

#### Étape 1 : Initialiser User Secrets

```powershell
# Naviguez vers le projet Common (contient les services partagés)
cd Cdm/Cdm.Common

# Initialisez User Secrets (si pas déjà fait)
dotnet user-secrets init
```

#### Étape 2 : Configurer la connection string SQL Server

```powershell
# Avec authentification Windows (recommandé en développement)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ChroniqueDesMondes;Trusted_Connection=True;TrustServerCertificate=True;"

# OU avec login/password
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ChroniqueDesMondes;User Id=sa;Password=VotreMotDePasse123!;TrustServerCertificate=True;Encrypt=False;"
```

#### Étape 3 : Configurer JWT

```powershell
# Générer une clé secrète aléatoire (PowerShell)
$secretKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
Write-Host $secretKey

# Configurer JWT avec cette clé
dotnet user-secrets set "Jwt:SecretKey" "$secretKey"
dotnet user-secrets set "Jwt:Issuer" "Cdm.ApiService"
dotnet user-secrets set "Jwt:Audience" "Cdm.Web"
dotnet user-secrets set "Jwt:ExpiryDays" "7"
```

#### Étape 4 : Configurer Azure Communication Services (optionnel)

```powershell
# Si vous utilisez l'envoi d'emails via Azure
dotnet user-secrets set "AzureCommunication:ConnectionString" "endpoint=https://votre-ressource.communication.azure.com/;accesskey=VOTRE_CLE"
dotnet user-secrets set "AzureCommunication:SenderEmail" "noreply@votredomaine.com"
```

#### Étape 5 : Vérifier vos secrets

```powershell
# Lister tous les secrets configurés
dotnet user-secrets list

# Afficher le chemin du fichier secrets.json
dotnet user-secrets list --verbose
```

**Emplacement du fichier** :
- Windows : `%APPDATA%\Microsoft\UserSecrets\<user-secrets-id>\secrets.json`
- Linux/Mac : `~/.microsoft/usersecrets/<user-secrets-id>/secrets.json`

---

### Option 2 : Fichier .env.local (Alternative)

Si vous préférez un fichier local :

#### Étape 1 : Créer .env.local

```powershell
# Copier le template
Copy-Item .env.example .env.local
```

#### Étape 2 : Éditer .env.local avec vos valeurs réelles

```ini
ConnectionStrings__DefaultConnection=Server=localhost;Database=ChroniqueDesMondes;Trusted_Connection=True;TrustServerCertificate=True;
Jwt__SecretKey=VOTRE_CLE_SECRETE_MINIMUM_32_CARACTERES
Jwt__Issuer=Cdm.ApiService
Jwt__Audience=Cdm.Web
Jwt__ExpiryDays=7
```

⚠️ **Important** : Le fichier `.env.local` est déjà dans `.gitignore` et ne sera **jamais committé**.

---

## 🚀 Configuration pour la production

### Variables d'environnement Docker

Pour le déploiement en production (Phase 6), utilisez des variables d'environnement Docker :

```yaml
# docker-compose.yml (exemple)
services:
  api:
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Jwt__SecretKey=${JWT_SECRET_KEY}
      - AzureCommunication__ConnectionString=${AZURE_COMM_CONNECTION}
```

Puis créez un fichier `.env` (non committé) :

```ini
DB_CONNECTION_STRING=Server=sql-server;Database=ChroniqueDesMondes;...
JWT_SECRET_KEY=votre-cle-production-super-secrete
AZURE_COMM_CONNECTION=endpoint=https://...
```

---

## 📝 Structure des fichiers de configuration

### Fichiers à NE PAS modifier (safe pour Git)

✅ Ces fichiers contiennent la **structure** mais pas les secrets :

```
Cdm/Cdm.ApiService/appsettings.json
Cdm/Cdm.ApiService/appsettings.Development.json
Cdm/Cdm.Web/appsettings.json
Cdm/Cdm.Web/appsettings.Development.json
.env.example
```

### Fichiers à NE JAMAIS committer (dans .gitignore)

❌ Ces fichiers contiennent vos **vraies valeurs** :

```
.env
.env.local
.env.*.local
appsettings.*.local.json
secrets.json (géré par User Secrets)
```

---

## ✅ Checklist avant de rendre le repo public

Avant de faire `git push` sur un repo public :

- [ ] Vérifier que `.gitignore` contient les patterns de secrets
- [ ] Vérifier qu'aucun fichier `appsettings.*.local.json` n'existe
- [ ] Vérifier qu'aucun fichier `.env` ou `.env.local` n'existe
- [ ] Faire une recherche dans tout le projet pour "password", "secret", "key"
- [ ] Vérifier l'historique Git avec `git log --all --full-history --source -- "*secrets*"`
- [ ] Si des secrets ont été committés par erreur, nettoyer avec `git filter-branch` ou BFG Repo-Cleaner

### Commande de vérification rapide

```powershell
# Rechercher des secrets potentiellement committés
git log --all --full-history --source -- "*appsettings*.json" "*secrets*" "*.env"

# Rechercher dans les fichiers actuels
git grep -i "password\|secret\|connectionstring" -- "*.json" "*.cs"
```

---

## 🤝 Instructions pour votre collègue

Envoyez ces instructions à votre collègue après qu'il ait cloné le repo :

### 1. Cloner le repository

```bash
git clone https://github.com/Tomtoxi44/Chronique_Des_Mondes.git
cd Chronique_Des_Mondes
```

### 2. Configurer les secrets locaux

Suivez la section **"Option 1 : User Secrets .NET"** ci-dessus.

### 3. Créer la base de données

```powershell
# Appliquer les migrations
cd Cdm/Cdm.Migrations
dotnet ef database update --context MigrationsContext

# OU utiliser le worker automatique
cd ../Cdm.MigrationsManager
dotnet run
```

### 4. Lancer le projet

```powershell
# Retour à la racine
cd ../..

# Lancer via Aspire orchestration
dotnet run --project Cdm/Cdm.AppHost
```

Le dashboard Aspire s'ouvrira automatiquement à `https://localhost:17223`.

---

## 🔐 Bonnes pratiques de sécurité

### Génération de clés sécurisées

#### PowerShell (Windows)

```powershell
# JWT Secret Key (64 bytes en Base64)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))

# Alternative : GUID
[Guid]::NewGuid().ToString("N") + [Guid]::NewGuid().ToString("N")
```

#### Bash (Linux/Mac)

```bash
# JWT Secret Key (64 bytes en Base64)
openssl rand -base64 64

# Alternative : UUID
cat /proc/sys/kernel/random/uuid | tr -d '-'
```

### Rotation des secrets

Il est recommandé de changer les secrets périodiquement :

1. **JWT Secret Key** : Tous les 6 mois minimum
2. **Database Password** : Tous les 3 mois en production
3. **Azure Keys** : Selon la politique de votre organisation

---

## 🆘 Dépannage

### "Unable to connect to SQL Server"

```powershell
# Vérifier que SQL Server est démarré
Get-Service -Name "MSSQL*"

# Tester la connexion
sqlcmd -S localhost -E -Q "SELECT @@VERSION"
```

### "Invalid JWT configuration"

```powershell
# Vérifier que les secrets JWT sont configurés
cd Cdm/Cdm.Common
dotnet user-secrets list | Select-String "Jwt"
```

### "Secrets not found"

```powershell
# Réinitialiser User Secrets
cd Cdm/Cdm.Common
dotnet user-secrets clear
dotnet user-secrets init

# Reconfigurer (voir Étape 2-4 ci-dessus)
```

---

## 📚 Ressources

- [.NET User Secrets Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Communication Services](https://learn.microsoft.com/en-us/azure/communication-services/)
- [SQL Server Connection Strings](https://www.connectionstrings.com/sql-server/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)

---

**Document créé le :** 15 octobre 2025  
**Dernière mise à jour :** 15 octobre 2025  
**Version :** 1.0
