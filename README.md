# 📖 Chronique des Mondes

> Plateforme de gestion de campagnes RPG multi-systèmes avec communication temps réel

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=.net)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-336791?logo=postgresql&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-0078D7)
![Azure](https://img.shields.io/badge/Azure-Deployed-0078D7?logo=microsoftazure)
![FluentUI](https://img.shields.io/badge/FluentUI-4.14-0078D4)

---

## 🎯 Vue d'Ensemble

**Chronique des Mondes** est une application web complète pour gérer des parties de jeux de rôle (D&D, Skyrim, systèmes custom). Elle offre :

- 🌍 **Gestion hiérarchique** : Worlds → Campaigns → Chapters → Events
- 👥 **Système d'invitation** : Tokens uniques pour rejoindre des campagnes
- 🏆 **Achievements** : Système de succès avec 5 niveaux de rareté (Common → Legendary)
- 💬 **Communication temps réel** : Chat, dés, combat, notifications via SignalR
- 🎲 **Gestion de combat** : Initiative, tours, attaques, dégâts en temps réel
- 📊 **Tableaux de bord** : Suivi personnages, événements actifs, progression

**Systèmes de jeu supportés** :
- 🎯 **Generic** - Système générique flexible
- 🐉 **D&D 5e** - Dungeons & Dragons 5ème édition
- ⚔️ **Skyrim** - Elder Scrolls avec mécaniques spécifiques
- 🎲 **Custom** - Systèmes personnalisés
- 📋 **Empty** - Monde vide (salon d'invitation)

---

## ✅ État d'Avancement

**Progression globale : 40% (30/75 tâches)**

| Phase | Backend | Frontend | Tests | Total |
|-------|---------|----------|-------|-------|
| **1-3** MVP | ✅ 100% | ✅ 100% | ✅ 100% | ✅ 100% |
| **4** Hierarchy | ✅ 100% | 🔴 0% | ✅ 85% | 🟡 65% |
| **5** Events/Achievements | ✅ 100% | 🔴 0% | ✅ 85% | 🟡 67% |
| **6** SignalR | ✅ 100% | 🔴 0% | 🔴 0% | 🟡 17% |

### 🟢 Phase 1-3 : MVP ✅ COMPLET (100%)
- ✅ Authentification JWT (Register, Login, Refresh Token)
- ✅ Gestion des profils utilisateurs
- ✅ Création & gestion des personnages (Generic, D&D 5e, Skyrim)
- ✅ Système de lancé de dés
- ✅ Upload d'images (profil, personnages)
- ✅ Auto-migrations Production Azure

### 🟢 Phase 4 : Hierarchy World → Campaign → Chapter ✅ BACKEND (65%)

**Backend : 15/23 tâches** ✅
- ✅ **WorldService** : 7 méthodes (Create, Get, List, Update, Delete, GetCharacters, Upload)
- ✅ **CampaignService** : 8 méthodes + système invitation par token (16 chars)
- ✅ **ChapterService** : 7 méthodes avec auto-increment, start/complete
- ✅ **Tests unitaires** : 12 tests (WorldService, ChapterService)
- ✅ **API REST** : 22 endpoints opérationnels

**Fonctionnalités clés** :
- ✅ CRUD complet pour Worlds, Campaigns, Chapters
- ✅ Système d'invitation campagne par token unique
- ✅ Gestion statuts : Planning, Active, OnHold, Completed, Cancelled
- ✅ Soft delete pour toutes les entités
- ✅ Autorisation GameMaster / Owner

**Frontend : 0/8 tâches** 🔴
- 🔴 Pages World CRUD
- 🔴 Pages Campaign CRUD + invitation
- 🔴 Pages Chapter CRUD
- 🔴 Composants FluentUI (WorldCard, CampaignCard, ChapterTimeline)

---

### 🟢 Phase 5 : Events & Achievements ✅ BACKEND (67%)

**Backend : 10/15 tâches** ✅
- ✅ **EventService** : 10 méthodes (multi-niveau World/Campaign/Chapter)
- ✅ **AchievementService** : 12 méthodes (award, revoke, rarity)
- ✅ **Tests unitaires** : 12 tests (EventService, AchievementService)
- ✅ **API REST** : 20 endpoints opérationnels

**Fonctionnalités clés** :
- ✅ Events multi-niveaux (World/Campaign/Chapter)
- ✅ Events permanents vs temporaires avec expiration
- ✅ Types d'effets : StatModifier, HealthModifier, DiceModifier, Narrative
- ✅ Achievements avec 5 niveaux de rareté (Common → Legendary)
- ✅ Attribution manuelle & révocation
- ✅ Filtrage par personnage actif

**Frontend & Automatisation : 0/5 tâches** 🔴
- 🔴 Pages Events Frontend
- 🔴 Pages Achievements Frontend
- 🔴 Système triggers automatiques (auto-unlock)
- 🔴 Notifications de déblocage

---

### 🟢 Phase 6 : SignalR Temps Réel ✅ BACKEND (17%)

**Backend : 4/24 tâches** ✅
- ✅ **SessionHub** : 6 méthodes (chat, dice, trade, character status)
- ✅ **CombatHub** : 8 méthodes (initiative, turns, attacks, damage)
- ✅ **NotificationHub** : 7 méthodes (user/campaign notifications)
- ✅ **JWT auth** via query string pour WebSockets
- ✅ Configuration Azure-ready (CORS, KeepAlive, Timeout)

**Fonctionnalités clés** :
- ✅ Chat temps réel par chapitre
- ✅ Lancers de dés avec broadcast
- ✅ Propositions d'échange (items/gold)
- ✅ Combat tour par tour avec tracking initiative
- ✅ Attaques, dégâts, HP tracking
- ✅ 8 types de notifications
- ✅ Groupes SignalR : chapter_{id}, combat_{id}, user_{id}, campaign_{id}

**Frontend : 0/20 tâches** 🔴
- 🔴 SessionConnectionService (HubConnection wrapper)
- 🔴 CombatConnectionService
- 🔴 NotificationConnectionService
- 🔴 Composants Session (Chat, DiceRoller)
- 🔴 Composants Combat (Initiative, Actions)
- 🔴 Composants Notifications (Toasts, Center)
- 🔴 Page Session Live
- 🔴 Page Combat Live

---

## 🗄️ Base de Données (PostgreSQL 17)

**12 tables** avec hiérarchie complète :

```
Users (auth & profiles)
  └── Worlds (univers de jeu)
       ├── Campaigns (histoires)
       │    └── Chapters (séances)
       │         ├── Events (World/Campaign/Chapter level)
       │         └── Achievements (World/Campaign/Chapter level)
       ├── Characters
       └── WorldCharacters (junction)
  
Notifications (temps réel)
UserAchievements (unlocks)
Roles, UserRoles (RBAC)
```

**Migrations récentes** :
- ✅ `AddCampaignInviteAndStatus` - Campaign.Status + InviteToken
- ✅ `AddNotifications` - Table Notification avec 8 types
- ✅ Auto-migration en Production via API Program.cs

---

## 🚀 Installation & Développement

### Prérequis
- **.NET 10 SDK**
- **PostgreSQL 17** (ou Docker)
- **Visual Studio 2022** / **Rider** / **VS Code**

### Configuration Locale

1. **Cloner le repository**
   ```bash
   git clone https://github.com/Tomtoxi44/Chronique_Des_Mondes.git
   cd Chronique_Des_Mondes
   ```

2. **Base de données PostgreSQL**
   ```bash
   # Option 1: Docker (recommandé)
   docker run --name cdm-postgres -e POSTGRES_PASSWORD=your_password -p 5432:5432 -d postgres:17

   # Option 2: PostgreSQL local
   # Créer une base "ChroniqueDesMondes"
   ```

3. **Configurer les secrets**
   ```powershell
   cd Cdm/Cdm.Common

   # Générer clé JWT
   $jwtKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))

   # User Secrets
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=ChroniqueDesMondes;Username=postgres;Password=your_password"
   dotnet user-secrets set "Jwt:SecretKey" "$jwtKey"
   dotnet user-secrets set "Jwt:Issuer" "Cdm.ApiService"
   dotnet user-secrets set "Jwt:Audience" "Cdm.Web"
   dotnet user-secrets set "Jwt:ExpiryDays" "7"
   ```

4. **Appliquer migrations**
   ```bash
   cd Cdm/Cdm.Migrations
   dotnet ef database update --startup-project ../Cdm.ApiService
   ```

5. **Lancer l'application (Aspire)**
   ```bash
   cd Cdm/Cdm.AppHost
   dotnet run
   ```
   
   📊 **Aspire Dashboard** : `http://localhost:15001`  
   🎨 **Frontend** : Vérifier dans le Dashboard  
   🔌 **API** : Vérifier dans le Dashboard + `/swagger`

### Tests

```bash
# Tests unitaires
cd Cdm/Cdm.Business.Common.Tests
dotnet test

# Résultats : 24 tests sur 4 services
# ✅ WorldServiceTests (6)
# ✅ ChapterServiceTests (6)
# ✅ EventServiceTests (6)
# ✅ AchievementServiceTests (6)
```

---

## 🏗️ Architecture Technique

### Stack

| Couche | Technologie | Version |
|--------|-------------|---------|
| **Frontend** | Blazor Server + FluentUI | .NET 10 + 4.14 |
| **Backend API** | ASP.NET Core Web API | .NET 10 |
| **Temps Réel** | SignalR (WebSockets) | .NET 10 |
| **Business Logic** | Services C# | .NET 10 |
| **ORM** | Entity Framework Core | 9.0 |
| **Database** | PostgreSQL | 17 |
| **Orchestration** | .NET Aspire | Latest |
| **Cloud** | Azure App Services | Production |

### Structure Projets

```
Cdm/
├── Cdm.Web/                      # 🎨 Frontend Blazor + FluentUI
│   ├── Components/               #    - Pages (Auth, Campaigns, Characters, Worlds)
│   ├── Services/                 #    - API Clients, State Management
│   └── wwwroot/                  #    - Static assets, LogoCdm.svg
│
├── Cdm.ApiService/               # 🔌 Backend REST + SignalR
│   ├── Endpoints/                #    - 42 REST endpoints
│   ├── Hubs/                     #    - 3 SignalR hubs (25+ methods)
│   └── Program.cs                #    - JWT, CORS, auto-migrations
│
├── Cdm.Business.Common/          # 💼 Business Logic (Services)
│   └── Services/                 #    - 44 service methods
│       ├── WorldService
│       ├── CampaignService
│       ├── ChapterService
│       ├── EventService
│       └── AchievementService
│
├── Cdm.Business.Abstraction/     # 📝 Interfaces + DTOs
│   ├── Services/                 #    - I{Entity}Service interfaces
│   └── DTOs/                     #    - Data Transfer Objects
│
├── Cdm.Data.Common/              # 🗄️ EF Core (Models + DbContext)
│   ├── Models/                   #    - 12 entity models
│   └── AppDbContext.cs           #    - Runtime context
│
├── Cdm.Migrations/               # 📊 EF Migrations
│   ├── Migrations/               #    - Database schema versions
│   └── MigrationsContext.cs      #    - Design-time context
│
├── Cdm.Common/                   # 🔧 Shared Services
│   ├── Enums/                    #    - GameType, CampaignStatus, etc.
│   └── Services/                 #    - JWT, Password, Email, ImageStorage
│
├── Cdm.Abstraction/              # 📦 Shared Interfaces
├── Cdm.ServiceDefaults/          # ⚙️ Aspire Defaults
├── Cdm.AppHost/                  # 🚀 Aspire Orchestrator
└── Cdm.Business.Common.Tests/    # 🧪 Unit Tests (24 tests)
```

### Patterns Architecturaux

- **Split DbContext** : MigrationsContext (design-time) vs AppDbContext (runtime)
- **Component-Handler-Service** : Blazor best practice (separation of concerns)
- **TPH Inheritance** : Table Per Hierarchy pour Characters multi-types
- **Soft Delete** : IsDeleted flag + EF global query filters
- **CQRS-like** : DTOs (read) vs Request models (write)
- **Repository Pattern** : Via EF Core DbContext

---

## 📡 API REST (42 endpoints)

### 🔐 Authentication (3 endpoints)
```http
POST /api/auth/register      # Créer compte
POST /api/auth/login         # JWT login
POST /api/auth/refresh       # Refresh token
```

### 🌍 Worlds (7 endpoints)
```http
GET    /api/worlds/my                    # Mes mondes
GET    /api/worlds/{id}                  # Détails monde
GET    /api/worlds/{id}/characters       # Personnages du monde
POST   /api/worlds                       # Créer monde
PUT    /api/worlds/{id}                  # Modifier monde
DELETE /api/worlds/{id}                  # Supprimer (soft delete)
POST   /api/worlds/{id}/upload-image     # Upload image
```

### 📖 Campaigns (8 endpoints)
```http
GET    /api/campaigns/{id}                   # Détails campagne
GET    /api/campaigns/world/{worldId}        # Campagnes du monde
POST   /api/campaigns                        # Créer campagne
PUT    /api/campaigns/{id}                   # Modifier campagne
DELETE /api/campaigns/{id}                   # Supprimer (soft)
POST   /api/campaigns/{id}/invite            # 🆕 Générer token invitation
POST   /api/campaigns/join                   # 🆕 Rejoindre avec token
PATCH  /api/campaigns/{id}/status            # 🆕 Changer statut
```

### 📚 Chapters (7 endpoints)
```http
GET    /api/chapters/{id}                    # Détails chapitre
GET    /api/chapters/campaign/{campaignId}   # Chapitres campagne
POST   /api/chapters                         # Créer chapitre
PUT    /api/chapters/{id}                    # Modifier chapitre
POST   /api/chapters/{id}/complete           # Marquer complété
POST   /api/chapters/{id}/start              # Démarrer chapitre
DELETE /api/chapters/{id}                    # Supprimer (soft)
```

### ⚡ Events (9 endpoints)
```http
GET    /api/events/world/{worldId}                  # Events monde
GET    /api/events/campaign/{campaignId}            # Events campagne
GET    /api/events/chapter/{chapterId}              # Events chapitre
GET    /api/events/character/{characterId}/active   # Events actifs personnage
POST   /api/events                                  # Créer event
PUT    /api/events/{id}                             # Modifier event
DELETE /api/events/{id}                             # Supprimer event
POST   /api/events/{id}/toggle-active               # Toggle actif/inactif
POST   /api/events/{id}/permanent                   # 🆕 Marquer permanent
```

### 🏆 Achievements (11 endpoints)
```http
GET    /api/achievements/world/{worldId}            # Achievements monde
GET    /api/achievements/campaign/{campaignId}      # Achievements campagne
GET    /api/achievements/chapter/{chapterId}        # Achievements chapitre
GET    /api/achievements/user/{userId}              # Achievements utilisateur
POST   /api/achievements                            # Créer achievement
PUT    /api/achievements/{id}                       # Modifier achievement
DELETE /api/achievements/{id}                       # Supprimer
POST   /api/achievements/{id}/award                 # Attribuer manuellement
POST   /api/achievements/{id}/revoke                # Révoquer
POST   /api/achievements/{id}/upload-icon           # Upload icône
GET    /api/achievements/{id}/unlocked-by           # Qui l'a débloqué
```

---

## 🔌 SignalR Hubs (3 hubs, 25+ méthodes)

### 💬 SessionHub (`/hubs/session`)
**Communication temps réel pour chapitres/séances**

**Serveur → Client** :
```csharp
ReceiveMessage(user, message, timestamp)       # Message chat
PlayerJoined(userName)                         # Joueur rejoint
PlayerLeft(userName)                           # Joueur quitte
DiceRolled(user, diceType, result, timestamp)  # Résultat dé
TradeProposed(fromUser, toUser, trade)         # Proposition échange
CharacterStatusUpdated(characterId, status)    # Statut personnage
```

**Client → Serveur** :
```csharp
JoinSession(chapterId)                         # Rejoindre session
LeaveSession(chapterId)                        # Quitter session
SendMessage(chapterId, message)                # Envoyer message
RollDice(chapterId, diceType, count)           # Lancer dés (d4/d6/d8/d10/d12/d20/d100)
ProposeTradeTheory(chapterId, toUserId, trade) # Proposer échange
UpdateCharacterStatus(chapterId, characterId, status)
```

---

### ⚔️ CombatHub (`/hubs/combat`)
**Gestion combat tour par tour**

**Serveur → Client** :
```csharp
CombatStarted(combatId, participants)          # Combat démarre
TurnChanged(combatId, currentTurn, nextChar)   # Tour suivant
AttackPerformed(attacker, target, attackData)  # Attaque effectuée
DamageDealt(target, damage, newHp)             # Dégâts infligés
CombatEnded(combatId, result)                  # Combat terminé
InitiativeUpdated(combatId, initiatives)       # Ordre initiative MAJ
```

**Client → Serveur** :
```csharp
JoinCombat(combatId) / LeaveCombat(combatId)
StartCombat(chapterId, participantIds)         # Démarrer combat
NextTurn(combatId)                             # Passer tour suivant
UpdateInitiative(combatId, characterId, init)  # Modifier initiative
Attack(combatId, targetId, attackData)         # Attaquer
DealDamage(combatId, targetId, damage)         # Infliger dégâts
EndCombat(combatId)                            # Terminer combat
```

---

### 🔔 NotificationHub (`/hubs/notifications`)
**Notifications utilisateur temps réel**

**Types de notifications** (8) :
- `CampaignInvite` - Invitation campagne
- `SessionStarting` - Séance démarre
- `AchievementUnlocked` - Succès débloqué
- `TradeProposed` - Échange proposé
- `MessageMention` - Mention dans chat
- `CombatTurn` - Ton tour de combat
- `SystemAnnouncement` - Annonce système
- `CharacterUpdate` - MAJ personnage

**Serveur → Client** :
```csharp
ReceiveNotification(notification)              # Nouvelle notification
NotificationRead(notificationId)               # Notification lue
AllNotificationsRead()                         # Toutes lues
```

**Client → Serveur** :
```csharp
SendNotificationToUser(userId, notification)
SendNotificationToCampaign(campaignId, notification)
JoinCampaignNotifications(campaignId)          # S'abonner campagne
LeaveCampaignNotifications(campaignId)
MarkAsRead(notificationId)
MarkAllAsRead()
```

**Configuration** :
- JWT auth via query string (`?access_token=xxx`)
- KeepAlive : 15 secondes
- ClientTimeout : 30 secondes
- Groupes : `user_{id}`, `campaign_{id}`, `chapter_{id}`, `combat_{id}`

---

## 🔐 Sécurité

### Architecture Multi-Couches

1. **JWT Authentication**
   - Tokens 7 jours avec claims utilisateur (UserId, Email, Roles)
   - Refresh tokens pour renouvellement
   - SignalR : JWT via query string (`?access_token=xxx`)

2. **BCrypt Password Hashing**
   - Work factor 12
   - Salt unique par utilisateur

3. **Authorization Policies**
   - `IsCharacterOwner` - Vérifie ownership personnage
   - `IsGameMaster` - Vérifie rôle GM campagne
   - `IsWorldOwner` - Vérifie créateur monde
   - `IsCampaignMember` - Vérifie membership campagne

4. **EF Core Query Filters**
   - Isolation automatique des données par UserId
   - Soft delete global (IsDeleted = false)

5. **Server-side Validation**
   - Tous les jets de dés validés côté serveur
   - Anti-cheat pour combat (initiative, dégâts)
   - Validation autorisation à chaque endpoint

6. **Azure Production**
   - HTTPS enforced
   - CORS configuré (origines autorisées)
   - Auto-migration sécurisée (Production only)

⚠️ **Les secrets ne doivent JAMAIS être committés**. Utilisez User Secrets (dev) ou variables d'environnement (prod).

---

## 🌐 Déploiement Production (Azure)

| Service | URL |
|---------|-----|
| **Frontend Blazor** | https://app-chronique-des-mondes-web.azurewebsites.net |
| **API Backend** | https://app-chroniquedesmondes-api-fgd9a7dsghb8g8gh.francecentral-01.azurewebsites.net |
| **PostgreSQL** | Managed Database (privé) |

**Configuration** :
- Auto-migrations via `Program.cs` (Production environment)
- SignalR WebSockets activés sur App Services
- API URL configurée via `appsettings.Production.json`

---

## 🎨 Frontend - FluentUI Components

### Bibliothèques UI
- **Microsoft.FluentUI.AspNetCore.Components** v4.14.0
- **Microsoft.FluentUI.AspNetCore.Components.Icons** v4.14.0

### Pages Actuelles
```
Components/Pages/
├── Auth/
│   ├── Login.razor          ✅ Formulaire connexion
│   └── Register.razor       ✅ Inscription
├── Characters/
│   └── [Gestion personnages] ✅
├── Campaigns/               🔴 À étendre
├── Worlds/                  🔴 À créer
├── Home.razor               ✅
├── Profile.razor            ✅
├── Settings.razor           ✅
└── Dice.razor               ✅
```

### Composants À Créer (Frontend Phase)
- 🔴 **WorldCard** - Carte monde avec image + stats
- 🔴 **CampaignCard** - Statut, progression, invitation
- 🔴 **ChapterTimeline** - Timeline des chapitres
- 🔴 **EventBadge** - Badge event avec type effet
- 🔴 **AchievementCard** - Carte achievement avec rareté (Common → Legendary)
- 🔴 **SessionChat** - Chat temps réel (SignalR)
- 🔴 **CombatTracker** - Tracker initiative + HP
- 🔴 **NotificationToast** - Toast notifications
- 🔴 **NotificationCenter** - Centre notifications

---

## 📝 Conventions de Code

### Naming
```csharp
// Services
I{Entity}Service → {Entity}Service
Example: IWorldService → WorldService

// DTOs
{Entity}Dto, Create{Entity}Request, Update{Entity}Request
Example: WorldDto, CreateWorldRequest, UpdateWorldRequest

// Endpoints
{Entity}Endpoints.MapEndpoints()
Example: WorldEndpoints.MapEndpoints(app)

// Hubs
{Feature}Hub
Example: SessionHub, CombatHub, NotificationHub
```

### Enums Principaux
```csharp
GameType: Generic, D&D5e, Skyrim, Custom, Empty

CampaignStatus: Planning, Active, OnHold, Completed, Cancelled

EventLevel / AchievementLevel: World(0), Campaign(1), Chapter(2)

AchievementRarity: Common, Uncommon, Rare, Epic, Legendary

NotificationType: CampaignInvite, SessionStarting, AchievementUnlocked,
                  TradeProposed, MessageMention, CombatTurn,
                  SystemAnnouncement, CharacterUpdate

EventEffectType: StatModifier, HealthModifier, DiceModifier, Narrative
```

### Tests - AAA Pattern
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var service = CreateService();
    var input = new CreateRequest { ... };

    // Act
    var result = await service.MethodAsync(input);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(expected, result.Property);
}
```

---

## 📊 Métriques Projet

| Catégorie | Quantité | Détails |
|-----------|----------|---------|
| **Services Backend** | 5 services | 44 méthodes métier |
| **API REST** | 42 endpoints | CRUD complet + features |
| **SignalR Hubs** | 3 hubs | 25+ méthodes temps réel |
| **Entités BDD** | 12 tables | PostgreSQL 17 |
| **Migrations EF** | 15+ migrations | Dont 3 récentes |
| **Tests Unitaires** | 24 tests | xUnit + Moq + InMemory |
| **Progression** | 40% | 30/75 tâches (backend complet) |
| **Lignes de Code** | ~15,000+ | Backend + Frontend |

---

## 🤝 Contribution

### Workflow Git
1. Fork le projet
2. Créer une branche : `git checkout -b feature/ma-feature`
3. Commit : `git commit -m 'feat: Add amazing feature'`
4. Push : `git push origin feature/ma-feature`
5. Ouvrir une Pull Request

### Branches
- `main` - Production stable
- `dev` - Développement actif
- `feature/*` - Nouvelles fonctionnalités

### Commit Messages (Conventional Commits)
```
feat: Nouvelle fonctionnalité
fix: Correction bug
chore: Tâche maintenance
test: Ajout tests
docs: Documentation
refactor: Refactoring
style: Formatage code
```

---

## 📄 Licence

Projet privé - Tous droits réservés © 2026

---

## 👥 Auteur

**Tommy ANGIBAUD** - [@Tomtoxi44](https://github.com/Tomtoxi44)

---

## 📚 Documentation Complémentaire

Documentation technique détaillée disponible dans `.github/instructions/technique/` :
- Architecture technique (6 phases)
- Modèle de données (schémas SQL)
- Standards de code
- Sécurité multi-couches
- Patterns Blazor

---

**Dernière mise à jour** : 2026-04-05  
**Version** : 0.4.0 (Backend complet Phase 4-6)

⭐ **Si ce projet vous plaît, donnez-lui une étoile !**

