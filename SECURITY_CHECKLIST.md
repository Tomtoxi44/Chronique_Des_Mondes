# 🔒 Checklist de Sécurité - Avant Repo Public

## ✅ Fichiers créés

- [x] `.gitignore` - Mis à jour avec patterns de secrets
- [x] `.env.example` - Template de configuration (SAFE pour Git)
- [x] `CONFIGURATION.md` - Guide complet de configuration
- [x] `setup-secrets.ps1` - Script d'installation automatique
- [x] `verify-secrets.ps1` - Script de vérification de sécurité
- [x] `README.md` - Documentation principale mise à jour

## 🔐 Vérifications avant push

### 1. Fichiers à NE JAMAIS committer

- [ ] `.env` ou `.env.local` - Variables d'environnement réelles
- [ ] `appsettings.*.local.json` - Configuration locale avec secrets
- [ ] `secrets.json` - User Secrets .NET
- [ ] `*.pfx` ou `*.key` - Certificats et clés privées
- [ ] `*.db` - Fichiers de base de données locaux

### 2. Vérification manuelle

```powershell
# Exécuter le script de vérification
.\verify-secrets.ps1

# Vérifier les appsettings.json
Get-Content Cdm\*\appsettings.json -Raw | Select-String -Pattern "Password=(?!YOUR_)"

# Vérifier l'historique Git
git log --all --full-history --source -- "*secrets*" "*appsettings*.local.json" "*.env"
```

### 3. Patterns à vérifier dans les fichiers

Recherchez ces patterns et assurez-vous qu'ils contiennent des **placeholders** et non des vraies valeurs :

- `Password=` → Doit être `Password=YOUR_PASSWORD_HERE` ou absent
- `SecretKey` → Doit être `SecretKey=YOUR_SECRET_KEY` ou absent
- `accesskey=` → Doit être un placeholder Azure
- `ConnectionString` → Doit contenir `localhost` ou placeholders

### 4. Configuration User Secrets

Vérifiez que vos secrets sont bien dans User Secrets :

```powershell
cd Cdm\Cdm.Common
dotnet user-secrets list
```

Doit afficher :
- `ConnectionStrings:DefaultConnection`
- `Jwt:SecretKey`
- `Jwt:Issuer`
- `Jwt:Audience`
- (Optionnel) `AzureCommunication:ConnectionString`

## 📤 Instructions pour votre collègue

Une fois le repo public, votre collègue devra :

### 1. Cloner le repo

```bash
git clone https://github.com/Tomtoxi44/Chronique_Des_Mondes.git
cd Chronique_Des_Mondes
```

### 2. Configurer les secrets

**Option A : Script automatique**

```powershell
.\setup-secrets.ps1
```

**Option B : Manuel**

Suivre les instructions dans `CONFIGURATION.md`

### 3. Créer la base de données

```powershell
cd Cdm\Cdm.Migrations
dotnet ef database update --context MigrationsContext
```

### 4. Lancer l'application

```powershell
cd ..\..
dotnet run --project Cdm\Cdm.AppHost
```

## 🚀 Prêt à partager !

Si toutes les vérifications ci-dessus sont ✅, vous pouvez :

```bash
# Ajouter tous les nouveaux fichiers
git add .

# Committer
git commit -m "docs: Add security configuration and setup scripts"

# Pousser vers GitHub
git push origin main
```

## 📝 Fichiers SAFE pour Git

Ces fichiers **peuvent** être committés (ne contiennent pas de secrets) :

✅ `appsettings.json` - Configuration par défaut
✅ `appsettings.Development.json` - Configuration développement (sans secrets)
✅ `.env.example` - Template de configuration
✅ `CONFIGURATION.md` - Documentation
✅ `README.md` - Documentation principale
✅ `.gitignore` - Patterns d'exclusion

## 🔒 Fichiers à NE JAMAIS committer

Ces fichiers sont dans `.gitignore` et ne doivent **jamais** être committés :

❌ `.env` - Variables d'environnement réelles
❌ `.env.local` - Variables locales
❌ `appsettings.*.local.json` - Config locale avec secrets
❌ `secrets.json` - User Secrets
❌ `*.pfx` - Certificats
❌ `*.db` - Bases de données locales

## 🆘 En cas de problème

### Si vous avez déjà committé un secret par erreur

#### Option 1 : Supprimer le dernier commit (si non poussé)

```bash
git reset HEAD~1
# Corriger les fichiers
git add .
git commit -m "docs: Add configuration (without secrets)"
```

#### Option 2 : Nettoyer l'historique (si déjà poussé)

**⚠️ ATTENTION : Cette opération réécrit l'historique Git**

```bash
# Utiliser BFG Repo-Cleaner
java -jar bfg.jar --delete-files appsettings.Production.json
git reflog expire --expire=now --all
git gc --prune=now --aggressive
git push --force
```

#### Option 3 : Créer un nouveau repo (solution radicale)

1. Créer un nouveau repo vide sur GitHub
2. Copier uniquement les fichiers sources (sans `.git/`)
3. Initialiser un nouveau repo local
4. Faire un premier commit propre
5. Pousser vers le nouveau repo

### Vérification finale avant push

```powershell
# 1. Vérifier le status Git
git status

# 2. Vérifier ce qui sera committé
git diff --cached

# 3. Exécuter le script de vérification
.\verify-secrets.ps1

# 4. Si tout est OK, pousser
git push origin main
```

## 📞 Contact

Si votre collègue rencontre des problèmes de configuration, référez-le à :
- `CONFIGURATION.md` - Guide complet
- `.env.example` - Exemples de configuration
- Issues GitHub pour questions spécifiques

---

**✅ Checklist complète ! Vous êtes prêt à rendre votre repo public en toute sécurité.**
