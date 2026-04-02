# 🎲 Chronique des Mondes

Plateforme de gestion de parties de jeu de rôle (JDR) multi-systèmes avec support temps réel.

## 📋 Description

**Chronique des Mondes** est une application d'assistance aux jeux de rôle avec deux interfaces complémentaires :

- 🖥️ **Desktop (Maître de Jeu)** - Interface complète pour créer et gérer des mondes, campagnes et sessions
- 📱 **Mobile (Joueurs)** - Interface optimisée pour participer aux sessions et gérer ses personnages

L'application suit une hiérarchie **Monde > Campagne > Chapitre** et supporte plusieurs systèmes de jeu :

- 🎯 **Empty** - Monde vide servant de salon d'invitation
- 🎲 **Custom** - Systèmes de jeu personnalisés
- 🐉 **D&D 5e** - Dungeons & Dragons 5ème édition avec règles automatisées
- ⚔️ **Skyrim** - Elder Scrolls avec mécaniques spécifiques

## 🚀 Démarrage rapide

### Prérequis

- **.NET 10 SDK** ou supérieur
- **SQL Server** (Express, Developer ou LocalDB)
- **Visual Studio 2025** ou **VS Code** avec extension C#

### Installation

1. **Cloner le repository**

```bash
git clone https://github.com/Tomtoxi44/Chronique_Des_Mondes.git
cd Chronique_Des_Mondes
```

2. **Configurer les secrets**

#### Option A : Script automatique (recommandé)

```powershell
# Exécuter le script de configuration
.\setup-secrets.ps1
```

Le script vous guidera pour :
- Générer une clé JWT sécurisée
- Configurer la connection string SQL Server
- (Optionnel) Configurer Azure Communication Services

#### Option B : Configuration manuelle

```powershell
cd Cdm/Cdm.Common

# Générer une clé JWT
$jwtKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))

# Configurer les secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ChroniqueDesMondes;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:SecretKey" "$jwtKey"
dotnet user-secrets set "Jwt:Issuer" "Cdm.ApiService"
dotnet user-secrets set "Jwt:Audience" "Cdm.Web"
dotnet user-secrets set "Jwt:ExpiryDays" "7"
```

📚 **Pour plus de détails, consultez [CONFIGURATION.md](CONFIGURATION.md)**

3. **Créer la base de données**

```powershell
cd Cdm/Cdm.Migrations
dotnet ef database update --context MigrationsContext
```

4. **Lancer l'application**

```powershell
# Retour à la racine
cd ../..

# Lancer via Aspire orchestration
dotnet run --project Cdm/Cdm.AppHost
```

Le **Dashboard Aspire** s'ouvrira automatiquement à `https://localhost:17223` 🎉

## 🏗️ Architecture

### Stack technique

- **.NET 10** - Framework principal
- **Aspire** - Orchestration et observabilité
- **Blazor Server** - Frontend interactif
- **SignalR** - Communication temps réel
- **Entity Framework Core 10** - ORM avec SQL Server
- **JWT** - Authentification avec BCrypt (work factor 12)

### Projets de la solution

```
Cdm/
├── Cdm.AppHost           # Orchestration Aspire
├── Cdm.ApiService        # API REST + SignalR hubs
├── Cdm.Web               # Frontend Blazor Server
├── Cdm.Data.Common       # EF Core (AppDbContext)
├── Cdm.Migrations        # EF Core (MigrationsContext)
├── Cdm.Business.Common   # Logique métier
├── Cdm.Common            # Services partagés (JWT, Email, Password)
├── Cdm.Abstraction       # Interfaces
└── Cdm.ServiceDefaults   # Aspire defaults
```

### Patterns architecturaux

- **TPH Inheritance** - Table Per Hierarchy pour les personnages multi-types
- **Split Context** - Séparation MigrationsContext / AppDbContext
- **Component-Handler-Service** - Pattern Blazor pour séparation des responsabilités
- **Result<T>** - Gestion d'erreurs sans exceptions
- **Repository Pattern** - Abstraction de la couche données

## 📚 Documentation technique

Toute la documentation détaillée se trouve dans `.github/instructions/technique/` :

- **[ARCHITECTURE_TECHNIQUE.md](`.github/instructions/technique/ARCHITECTURE_TECHNIQUE.md`)** - Vue d'ensemble de l'architecture (6 phases)
- **[MODELE_DONNEES.md](`.github/instructions/technique/MODELE_DONNEES.md`)** - Schéma de base de données (17 tables, SQL DDL)
- **[API_ENDPOINTS.md](`.github/instructions/technique/API_ENDPOINTS.md`)** - 45+ endpoints REST avec exemples
- **[SIGNALR_TEMPS_REEL.md](`.github/instructions/technique/SIGNALR_TEMPS_REEL.md`)** - 3 hubs temps réel (Session, Combat, Notification)
- **[SECURITE.md](`.github/instructions/technique/SECURITE.md`)** - Architecture de sécurité multi-couches
- **[FRONTEND_BLAZOR.md](`.github/instructions/technique/FRONTEND_BLAZOR.md`)** - Patterns Blazor Server
- **[STANDARDS_CODE.md](`.github/instructions/technique/STANDARDS_CODE.md`)** - Conventions de code .NET

## 🔐 Sécurité

Le projet implémente une **sécurité multi-couches** :

1. **JWT Authentication** - Tokens 7 jours avec claims utilisateur
2. **BCrypt Hashing** - Work factor 12 pour les mots de passe
3. **Authorization Policies** - 4 handlers (IsCharacterOwner, IsGameMaster, etc.)
4. **EF Core Query Filters** - Isolation automatique des données par UserId
5. **Server-side Validation** - FluentValidation + checks manuels
6. **Anti-cheat** - Tous les jets de dés validés côté serveur

⚠️ **Les secrets ne doivent JAMAIS être committés**. Utilisez User Secrets (développement) ou variables d'environnement (production).

## 🎮 Fonctionnalités

### Hiérarchie de jeu

- **Monde** : Univers de jeu avec règles spécifiques (D&D, Skyrim, Custom, Empty)
  - Le créateur devient automatiquement Maître du Jeu
  - Peut être créé vide pour servir de salon d'invitation
  - Contient plusieurs campagnes
- **Campagne** : Histoire/aventure dans un monde
  - Peut impacter le monde
  - Contient plusieurs chapitres
- **Chapitre** : Séances/épisodes d'une campagne
  - Contenu organisé avec événements et PNJ
  - Sessions en ligne avec salon vocal

### Système de personnages

- **Personnage de base** : Créé par l'utilisateur (nom, prénom, caractéristiques)
- **Duplication lors de l'intégration** : Quand un personnage rejoint un monde, une copie est créée avec les règles du monde appliquées
- **Règle de verrouillage** : Un personnage ayant intégré un monde ne peut plus rejoindre un autre monde
- **Personnage original** : Reste disponible pour créer des copies dans d'autres mondes

### Phase 1-3 (MVP - Complété)

- ✅ Authentification JWT avec inscription/connexion
- ✅ Gestion de personnages multi-types (Generic, D&D 5e, Skyrim)
- ✅ Création et gestion de campagnes
- ✅ Interface utilisateur Blazor avec thème médiéval fantasy
- ✅ Profil utilisateur avec avatar

### Phase 4-6 (En cours)

- 🔄 Hiérarchie Monde > Campagne > Chapitre
- 🔄 Système d'événements (Monde, Campagne, Chapitre)
- 🔄 Système de succès avec attribution automatique/manuelle
- 🔄 Système de combat avec jets de dés automatiques
- 🔄 Sessions de jeu avec invitations par token
- 🔄 Chat temps réel via SignalR
- 🔄 Système de notifications

## 🧪 Tests

```powershell
# Exécuter tous les tests
dotnet test

# Avec couverture de code
dotnet test /p:CollectCoverage=true
```

Les tests suivent le pattern **AAA** (Arrange-Act-Assert) avec **xUnit** et **FluentAssertions**.

## 🤝 Contribution

Les contributions sont les bienvenues ! Veuillez :

1. Fork le projet
2. Créer une branche (`git checkout -b feature/amazing-feature`)
3. Suivre les conventions dans [STANDARDS_CODE.md](`.github/instructions/technique/STANDARDS_CODE.md`)
4. Committer vos changements (`git commit -m 'Add amazing feature'`)
5. Pousser vers la branche (`git push origin feature/amazing-feature`)
6. Ouvrir une Pull Request

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.

## 👥 Auteurs

- **Tommy ANGIBAUD** - [@Tomtoxi44](https://github.com/Tomtoxi44)

## 🙏 Remerciements

- Microsoft pour .NET Aspire et Blazor Server
- La communauté .NET pour les outils et bibliothèques

---

**⭐ Si ce projet vous plaît, n'hésitez pas à lui donner une étoile !**

