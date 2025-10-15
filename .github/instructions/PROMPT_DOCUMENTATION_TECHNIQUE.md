# 📝 Prompt pour Création de Documentation Technique

## 🎯 Contexte

Tu es un architecte logiciel expert en .NET 10, Aspire, Blazor Server et Entity Framework Core. Tu dois créer une **documentation technique complète** pour le projet **Chronique des Mondes**, une plateforme de jeu de rôle (JDR) multi-systèmes.

## 📋 Informations de Base

### Projet
- **Nom** : Chronique des Mondes
- **Technologies** : .NET 10, Aspire, Blazor Server, Entity Framework Core, SignalR, SQL Server
- **Architecture** : Microservices orchestrés par Aspire AppHost
- **Objectif** : Plateforme JDR supportant plusieurs systèmes (Générique, D&D 5e, Skyrim)

### Documents de Référence
- **Spécification Fonctionnelle** : `.github/instructions/SPECIFICATION_FONCTIONNELLE.md`
- **Roadmap** : `.github/ROADMAP.md`
- **Instructions Copilot** : `.github/copilot-instructions.md`

## 🏗️ Architecture Définie

### Structure des Projets

```
Cdm/
├── Aspire/
│   ├── Cdm.AppHost/              # Orchestration Aspire
│   └── Cdm.ServiceDefaults/      # Configurations partagées (health checks, telemetry)
│
├── Back/
│   ├── Api/
│   │   ├── Cdm.ApiService/       # API REST avec endpoints groupés
│   │   │   └── Endpoints/        # CampaignEndpoints.cs, CharacterEndpoints.cs, etc.
│   │   │
│   │   ├── Cdm.Business.Common/  # Logique métier commune à tous les types de JDR
│   │   │
│   │   ├── Cdm.Business.Abstraction/ # Interfaces communes pour toutes les couches métier
│   │   │
│   │   ├── Cdm.Business.Dnd/     # Logique métier spécifique D&D
│   │   ├── Cdm.Business.Skyrim/  # Logique métier spécifique Skyrim
│   │   │
│   │   └── Cdm.Data.Common/      # Couche d'accès données commune
│   │       ├── AppDbContext.cs   # Contexte runtime (vide actuellement)
│   │       └── Models/           # Entités communes (User, ACharacter, etc.)
│   │
│   ├── Cdm.Data.Dnd/             # Accès données spécifique D&D
│   ├── Cdm.Data.Skyrim/          # Accès données spécifique Skyrim
│   │
│   └── Migrations/
│       ├── Cdm.Migrations/       # MigrationsContext avec tous les DbSets
│       └── Cdm.MigrationsManager/ # Worker service pour appliquer les migrations
│
└── Common/
    └── Cdm.Common/               # Utilitaires partagés (EmailService, JwtService, etc.)
│
└── Front/
    └── Cdm.Web/                  # Blazor Server
        └── Components/
            ├── Pages/            # Pages routables (/Campaigns/, /Characters/, etc.)
            │   └── Models/       # ViewModels pour les pages
            └── Layout/           # Layouts et navigation
```

### Flux de Données Multi-Type

**Principe** : Une clé dans le header HTTP (`X-Game-Type: dnd|skyrim|generic`) détermine quelle couche métier est appelée.

```
┌─────────────────┐
│ Cdm.ApiService  │ ← Endpoints communs (pas de duplication par type de jeu)
└────────┬────────┘
         │ Header: X-Game-Type = "dnd"
         ▼
┌──────────────────────┐
│ Cdm.Business.Common  │ ← Logique commune (CRUD Character, Campaign, etc.)
└──────────┬───────────┘
           │
           ├─────────────────┐
           │                 │
           ▼                 ▼
  ┌─────────────────┐  ┌────────────────┐
  │ Cdm.Data.Common │  │ Cdm.Business.  │ ← Règles métier spécifiques
  │                 │  │ Dnd/Skyrim     │   (calculs automatiques, etc.)
  │ (Récupère data) │  └────────┬───────┘
  └─────────────────┘           │
           ▲                    │
           │                    ▼
           │           ┌────────────────┐
           └───────────│ Cdm.Data.Dnd/  │ ← Données spécifiques
                       │ Skyrim         │   (règles, monstres officiels)
                       └────────────────┘
```

**Exemple concret - Création de personnage D&D** :
1. `POST /api/characters` avec header `X-Game-Type: dnd`
2. `Cdm.ApiService` → `CharacterEndpoints.CreateCharacter()`
3. `Cdm.Business.Common.CharacterService.CreateAsync()` :
   - Désérialise le JSON selon le type (`GameType.DnD`)
   - Valide le modèle D&D
   - Appelle `Cdm.Business.Dnd.DndCharacterValidator.Validate()`
   - Sauvegarde via `Cdm.Data.Common`
4. Retour au client avec le personnage créé

## 🎯 Décisions Architecturales Validées

### 1. Gestion des Systèmes de Jeu

**Modèle de Données** : Option A - Table unique avec JSON flexible
```csharp
// Cdm.Data.Common/Models/Character.cs
public abstract class ACharacter
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public int Life { get; set; }
    public GameType GameType { get; set; } // Tag : Generic, DnD, Skyrim
    
    [Column(TypeName = "nvarchar(max)")]
    public string? AttributesJson { get; set; } // Attributs spécifiques (force, dex, etc.)
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Tags { get; set; } // JSON pour catégorisation
}
```

**Validation de Compatibilité** : 
- Match sur les tags (`Character.GameType` == `Campaign.GameType`)
- Filtre pré-appliqué dans l'IHM (n'affiche que les persos compatibles)
- Validation métier dans `Cdm.Business.Common` avec retour d'erreur si incompatible

### 2. Communication Temps Réel - SignalR

**Hubs Séparés** :
```csharp
// Cdm.ApiService/Hubs/
├── SessionHub.cs    # Gestion des sessions (lancement, rejoindre, etc.)
├── CombatHub.cs     # Gestion des combats (tours, actions, initiative)
└── NotificationHub.cs # Notifications générales
```

**Gestion des Groupes** : Option A - Groupes par Session
```csharp
// CombatHub.cs
public async Task JoinCombat(int sessionId, int combatId)
{
    await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
    await Groups.AddToGroupAsync(Context.ConnectionId, $"combat-{combatId}");
}

public async Task NotifyPlayerTurn(int combatId, int playerId)
{
    await Clients.Group($"combat-{combatId}")
        .SendAsync("PlayerTurnNotification", playerId);
}
```

### 3. Système de Dés

**Exécution** : Côté serveur pour éviter la triche
```csharp
// Cdm.Business.Common/Services/DiceService.cs
public class DiceRoll
{
    public int SessionId { get; set; }
    public int? CombatId { get; set; }
    public int UserId { get; set; }
    public string DiceFormula { get; set; } // "2d6+3", "1d20", etc.
    public int Result { get; set; }
    public DateTime RolledAt { get; set; }
    public GameType GameType { get; set; }
}
```

**Stockage** : Table dédiée `DiceRolls` pour historique et statistiques

### 4. Sorts et Équipements

**Modèle** : Table unique avec flag `IsOfficial`
```csharp
public class Spell
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsOfficial { get; set; } // true = bibliothèque officielle
    public int? CreatorId { get; set; }  // null si officiel, userId sinon
    public GameType GameType { get; set; } // Tag : Generic, DnD, Skyrim
    
    [Column(TypeName = "nvarchar(max)")]
    public string? SpecificAttributesJson { get; set; } // Niveau, école, etc. pour D&D
}

// Table de liaison
public class CharacterSpell
{
    public int CharacterId { get; set; }
    public int SpellId { get; set; }
    public DateTime LearnedAt { get; set; }
}
```

**Échanges d'Équipements** : Option A - Transactions EF Core
```csharp
public class EquipmentProposal
{
    public int Id { get; set; }
    public int ProposerUserId { get; set; } // MJ
    public int ReceiverUserId { get; set; } // Joueur
    public int EquipmentId { get; set; }
    public int Quantity { get; set; }
    public ProposalStatus Status { get; set; } // Pending, Accepted, Rejected
    public DateTime ProposedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public class EquipmentTrade
{
    public int Id { get; set; }
    public int OfferorUserId { get; set; }
    public int RequestorUserId { get; set; }
    public List<TradeItem> OfferItems { get; set; } // Items offerts
    public List<TradeItem> RequestItems { get; set; } // Items demandés
    public TradeStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### 5. État de Session

**Option A** : Base de Données + Cache In-Memory
```csharp
// Tables
public class Session
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int GameMasterId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public SessionStatus Status { get; set; } // Active, Paused, Completed
}

public class SessionState
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int CurrentChapterId { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string StateJson { get; set; } // Snapshot complet de l'état
    
    public DateTime SavedAt { get; set; }
}

// Service avec cache
public class SessionStateService
{
    private readonly IMemoryCache _cache;
    private readonly MigrationsContext _context;
    
    public async Task<SessionState> GetActiveStateAsync(int sessionId)
    {
        if (_cache.TryGetValue($"session-{sessionId}", out SessionState state))
            return state;
        
        // Charge depuis DB et met en cache
        var dbState = await _context.SessionStates
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        
        state = JsonSerializer.Deserialize<SessionState>(dbState.StateJson);
        _cache.Set($"session-{sessionId}", state, TimeSpan.FromMinutes(10));
        
        return state;
    }
}
```

### 6. Background Jobs

**Option A** : Hosted Services .NET (intégré)
```csharp
// Cdm.ApiService/BackgroundServices/StatisticsAggregatorService.cs
public class StatisticsAggregatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var statisticsService = scope.ServiceProvider.GetRequiredService<IStatisticsService>();
            
            await statisticsService.AggregateMonthlyStatsAsync();
            await statisticsService.GenerateMonthlyReportsAsync();
            
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}

// Program.cs
builder.Services.AddHostedService<StatisticsAggregatorService>();
```

**Services à créer** :
- `StatisticsAggregatorService` : Agrégation stats mensuelles/annuelles
- `ReportGeneratorService` : Génération et envoi des rapports par email
- `CleanupService` : Nettoyage des sessions inactives

### 7. Frontend Blazor

**Structure des Composants** :
```
Cdm.Web/Components/
├── Pages/
│   ├── Campaigns/
│   │   ├── Models/                   # ViewModels
│   │   │   └── CampaignViewModel.cs
│   │   ├── CampaignList.razor
│   │   ├── CampaignList.razor.cs     # Code-behind
│   │   ├── CampaignList.razor.resx   # Ressources i18n
│   │   ├── CampaignDetail.razor
│   │   └── CampaignDetail.razor.handler.cs # Handler si logique complexe
│   │
│   ├── Characters/
│   ├── Sessions/
│   └── Combat/
│
├── Shared/                           # Composants réutilisables
│   ├── CharacterCard.razor
│   ├── DiceRoller.razor
│   └── NotificationToast.razor
│
└── Layout/
    ├── MainLayout.razor
    └── NavMenu.razor
```

**Gestion de l'État** :
- Services Scoped pour l'état de la page
- `CascadingValue` pour l'utilisateur courant
- SignalR pour état partagé temps réel

### 8. Sécurité et Autorisations

**Architecture Multicouche** :

1. **Policies ASP.NET Core** :
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsCampaignOwner", policy =>
        policy.Requirements.Add(new CampaignOwnerRequirement()));
    
    options.AddPolicy("IsCampaignPlayer", policy =>
        policy.Requirements.Add(new CampaignPlayerRequirement()));
});
```

2. **Authorization Handlers** :
```csharp
public class CampaignOwnerHandler : AuthorizationHandler<CampaignOwnerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CampaignOwnerRequirement requirement)
    {
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var campaignId = GetCampaignIdFromContext();
        
        var campaign = await _campaignService.GetByIdAsync(campaignId);
        if (campaign.CreatorId == userId)
            context.Succeed(requirement);
    }
}
```

3. **Query Filters EF Core** :
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Character>()
        .HasQueryFilter(c => c.UserId == _currentUserId);
    
    modelBuilder.Entity<Spell>()
        .HasQueryFilter(s => s.IsOfficial || s.CreatorId == _currentUserId);
}
```

## 🗄️ Base de Données

### Connexions

**Production** :
```json
"ConnectionStrings": {
  "ProdDatabase": "Server=79.137.75.13;Port=3307;Database=prod_db;User Id=prod_user;Password=Prod_pa$$word_52Cdm;"
}
```

**Développement** :
```json
"ConnectionStrings": {
  "DevDatabase": "Server=79.137.75.13;Port=3308;Database=dev_db;User Id=dev_user;Password=Dev_pa$$word_52Cdm;"
}
```

### Pattern Migrations

**IMPORTANT** : Séparation définitive des contextes
- **MigrationsContext** (`Cdm.Migrations`) : Contient TOUS les DbSets, utilisé pour générer les migrations
- **AppDbContext** (`Cdm.Data.Common`) : Utilisé par l'application au runtime (actuellement vide)

**Commandes EF Core** :
```bash
# Ajouter une migration
dotnet ef migrations add MigrationName --project Cdm\Cdm.Migrations --startup-project Cdm\Cdm.MigrationsManager

# Supprimer la dernière migration
dotnet ef migrations remove --project Cdm\Cdm.Migrations --startup-project Cdm\Cdm.MigrationsManager

# Appliquer les migrations (automatique au démarrage via Aspire)
# Géré par Cdm.MigrationsManager Worker
```

## 📝 Tâche à Accomplir

**Créer un document technique complet** nommé `SPECIFICATION_TECHNIQUE.md` dans `.github/instructions/` qui inclut :

### 1. Vue d'Ensemble Architecture
- Diagramme global avec tous les projets
- Flux de données multi-type (Générique/D&D/Skyrim)
- Patterns utilisés et justifications

### 2. Backend (.NET 10)
- **Structure détaillée des projets** avec responsabilités
- **Modèle de données complet** :
  - Schéma des tables principales
  - Relations entre entités
  - Stratégie pour attributs spécifiques par type de jeu (JSON)
- **Couches métier** :
  - `Cdm.Business.Common` : Services communs
  - `Cdm.Business.Dnd/Skyrim` : Logique spécialisée
  - Interfaces dans `Cdm.Abstraction`
- **API Endpoints** :
  - Convention de nommage
  - Groupement par domaine
  - Header `X-Game-Type` pour routage
- **SignalR Hubs** :
  - SessionHub, CombatHub, NotificationHub
  - Gestion des groupes
- **Background Services** :
  - Statistiques, Rapports, Cleanup

### 3. Frontend (Blazor Server)
- **Structure des composants** avec exemples
- **Convention de nommage** (.razor, .razor.cs, .razor.handler.cs)
- **Gestion de l'état** (Services Scoped, CascadingValue)
- **Communication SignalR** côté client

### 4. Base de Données
- **Schéma complet** avec toutes les tables
- **Indexes** et optimisations
- **Pattern Migrations** (MigrationsContext vs AppDbContext)
- **Query Filters** pour isolation des données

### 5. Sécurité
- **Authentification JWT** (déjà implémenté)
- **Authorization Policies** et Handlers
- **Validation multicouche** (API → Business → Data)
- **Protection anti-triche** (dés côté serveur)

### 6. Performances
- **Caching** (IMemoryCache pour état de session)
- **Optimisations EF Core** (AsNoTracking, projections)
- **SignalR scaling** (groupes par session/combat)

### 7. DevOps
- **Aspire Orchestration** (AppHost, ServiceDefaults)
- **Environnements** (Dev/Prod avec connexions différentes)
- **Monitoring** (Aspire Dashboard en dev)

### 8. Standards de Code
- **Conventions de nommage** (.NET)
- **Organisation des fichiers**
- **Patterns recommandés** avec exemples
- **Anti-patterns à éviter**

### 9. Exemples Concrets
- Création d'un endpoint complet (de A à Z)
- Ajout d'un nouveau type de jeu (ex: Pathfinder)
- Implémentation d'une fonctionnalité avec SignalR

### 10. Checklist Développeur
- [ ] Points à vérifier avant chaque PR
- [ ] Commandes utiles
- [ ] Résolution de problèmes courants

## 🎨 Format du Document

- **Markdown** structuré avec table des matières
- **Diagrammes** en ASCII ou Mermaid si nécessaire
- **Exemples de code** commentés et complets
- **Notes importantes** en callouts (> ⚠️, > ✅, > 💡)
- **Liens** vers les fichiers existants du projet

## ✅ Critères de Qualité

Le document doit être :
- ✅ **Complet** : Couvre tous les aspects techniques
- ✅ **Actionnable** : Un développeur peut l'utiliser immédiatement
- ✅ **Maintenable** : Facile à mettre à jour
- ✅ **Pédagogique** : Explique les "pourquoi" pas seulement les "comment"
- ✅ **Cohérent** : Aligné avec la spéc fonctionnelle et la roadmap

## 🚀 Go !

Crée maintenant le fichier `.github/instructions/SPECIFICATION_TECHNIQUE.md` avec tout le contenu nécessaire !
