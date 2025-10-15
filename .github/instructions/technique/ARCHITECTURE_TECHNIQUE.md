# 🏗️ Architecture Technique - Chronique des Mondes

> **Documentation technique principale du projet**  
> Dernière mise à jour : 15 octobre 2025

---

## 📚 Table des Matières

- [Vue d'Ensemble](#-vue-densemble)
- [Structure des Projets](#-structure-des-projets)
- [Flux de Données Multi-Type](#-flux-de-données-multi-type)
- [Patterns Architecturaux](#-patterns-architecturaux)
- [Technologies Utilisées](#-technologies-utilisées)

---

## 🎯 Vue d'Ensemble

**Chronique des Mondes** est une plateforme de jeu de rôle (JDR) multi-systèmes construite avec une architecture modulaire permettant de supporter différents types de jeux (Générique, D&D 5e, Skyrim, etc.).

### Principes Architecturaux

1. **🔹 Séparation des Responsabilités** : Chaque couche a un rôle bien défini
2. **🔹 Extensibilité** : Ajout facile de nouveaux types de jeux
3. **🔹 Maintenabilité** : Code organisé et documenté
4. **🔹 Performance** : Optimisations dès la conception (cache, async, projections)
5. **🔹 Sécurité** : Validation multicouche et isolation des données

### Architecture Globale

```
┌─────────────────────────────────────────────────────────────┐
│                    ASPIRE ORCHESTRATION                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Cdm.AppHost : Service Discovery + Health Checks        │ │
│  │ Cdm.ServiceDefaults : Configuration partagée           │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│              │     │              │     │              │
│  FRONTEND    │────▶│   API        │────▶│  DATABASE    │
│  Blazor      │     │   Service    │     │  SQL Server  │
│  Server      │     │              │     │              │
│              │◀────│  SignalR     │     │              │
└──────────────┘     └──────────────┘     └──────────────┘
                              │
                     ┌────────┴────────┐
                     │                 │
                     ▼                 ▼
              ┌────────────┐    ┌────────────┐
              │ Business   │    │  Workers   │
              │ Logic      │    │ Background │
              │ Multi-Type │    │  Services  │
              └────────────┘    └────────────┘
```

---

## 📁 Structure des Projets

### Vue d'Ensemble

```
Cdm/
├── Aspire/                          # 🚀 Orchestration
│   ├── Cdm.AppHost/                 
│   └── Cdm.ServiceDefaults/         
│
├── Back/                            # ⚙️ Backend
│   ├── Api/
│   │   ├── Cdm.ApiService/          # 🌐 API REST + SignalR
│   │   ├── Cdm.Business.Common/     # 📊 Logique métier commune
│   │   ├── Cdm.Business.Abstraction/# 🔌 Interfaces communes
│   │   ├── Cdm.Business.Dnd/        # 🎲 Logique D&D
│   │   ├── Cdm.Business.Skyrim/     # 🏔️ Logique Skyrim
│   │   └── Cdm.Data.Common/         # 💾 Accès données commun
│   │
│   ├── Cdm.Data.Dnd/                # 💾 Données D&D
│   ├── Cdm.Data.Skyrim/             # 💾 Données Skyrim
│   │
│   └── Migrations/
│       ├── Cdm.Migrations/          # 🔄 Contexte migrations
│       └── Cdm.MigrationsManager/   # 🤖 Worker migrations
│
├── Common/
│   └── Cdm.Common/                  # 🛠️ Utilitaires (JWT, Email, etc.)
│
└── Front/
    └── Cdm.Web/                     # 🎨 Blazor Server
        └── Components/
            ├── Pages/               
            ├── Shared/              
            └── Layout/              
```

### 🚀 Aspire / Orchestration

#### `Cdm.AppHost`
**Responsabilité** : Orchestrer tous les services avec Aspire

```csharp
// AppHost.cs
var builder = DistributedApplication.CreateBuilder(args);

// Base de données
var db = builder.AddConnectionString("DefaultConnection");

// Migrations (appliquées au démarrage)
var migrations = builder.AddProject<Projects.Cdm_MigrationsManager>("migrations")
    .WithReference(db)
    .WaitFor(db);

// API Service (attend les migrations)
var apiService = builder.AddProject<Projects.Cdm_ApiService>("apiservice")
    .WithReference(migrations)
    .WaitForCompletion(migrations);

// Frontend (attend l'API)
builder.AddProject<Projects.Cdm_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

// Documentation API (Scalar)
builder.AddScalarApiReference(options => options.WithTheme(ScalarTheme.Purple))
    .WithApiReference(apiService);

builder.Build().Run();
```

**Points Clés** :
- ✅ Ordre de démarrage garanti : DB → Migrations → API → Frontend
- ✅ Service Discovery automatique
- ✅ Health checks intégrés
- ✅ Dashboard Aspire en développement

#### `Cdm.ServiceDefaults`
**Responsabilité** : Configuration partagée par tous les services

```csharp
// Extensions.cs
public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
{
    builder.ConfigureOpenTelemetry(); // Logs, Metrics, Traces
    builder.AddDefaultHealthChecks(); // /health, /alive
    builder.Services.AddServiceDiscovery(); // Service-to-service
    
    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        http.AddStandardResilienceHandler(); // Retry, timeout, etc.
        http.AddServiceDiscovery();
    });
    
    return builder;
}
```

**Utilisé par** : Tous les projets (`builder.AddServiceDefaults()`)

---

### ⚙️ Backend / API

#### `Cdm.ApiService`
**Responsabilité** : Exposer les endpoints REST et SignalR

**Structure** :
```
Cdm.ApiService/
├── Program.cs                    # Configuration et démarrage
├── Endpoints/                    # Endpoints groupés par domaine
│   ├── AuthEndpoints.cs
│   ├── CampaignEndpoints.cs
│   ├── CharacterEndpoints.cs
│   ├── SessionEndpoints.cs
│   ├── CombatEndpoints.cs
│   ├── SpellEndpoints.cs
│   └── EquipmentEndpoints.cs
├── Hubs/                         # SignalR Hubs
│   ├── SessionHub.cs
│   ├── CombatHub.cs
│   └── NotificationHub.cs
└── Middleware/                   # Middleware custom
    └── GameTypeMiddleware.cs     # Extraction du header X-Game-Type
```

**Exemple - CharacterEndpoints.cs** :
```csharp
public static class CharacterEndpoints
{
    public static void MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/characters")
            .RequireAuthorization()
            .WithTags("Characters");

        group.MapGet("/", GetCharacters)
            .WithName("GetCharacters")
            .WithOpenApi();

        group.MapGet("/{id}", GetCharacter)
            .WithName("GetCharacter")
            .RequireAuthorization("IsCharacterOwner")
            .WithOpenApi();

        group.MapPost("/", CreateCharacter)
            .WithName("CreateCharacter")
            .WithOpenApi();

        group.MapPut("/{id}", UpdateCharacter)
            .WithName("UpdateCharacter")
            .RequireAuthorization("IsCharacterOwner")
            .WithOpenApi();

        group.MapDelete("/{id}", DeleteCharacter)
            .WithName("DeleteCharacter")
            .RequireAuthorization("IsCharacterOwner")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateCharacter(
        HttpContext httpContext,
        ICharacterService characterService,
        CreateCharacterRequest request)
    {
        // Le header X-Game-Type a été extrait par le middleware
        var gameType = httpContext.Items["GameType"] as GameType?;
        
        var result = await characterService.CreateAsync(request, gameType);
        
        return result.IsSuccess
            ? Results.Created($"/api/characters/{result.Value.Id}", result.Value)
            : Results.BadRequest(result.Errors);
    }
}
```

**Program.cs** :
```csharp
var builder = WebApplication.CreateBuilder(args);

// Service Defaults (Aspire)
builder.AddServiceDefaults();

// Services
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
// ... autres services

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* config JWT */ });

// Authorization avec Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsCharacterOwner", policy =>
        policy.Requirements.Add(new CharacterOwnerRequirement()));
    // ... autres policies
});

// SignalR
builder.Services.AddSignalR();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware
app.UseMiddleware<GameTypeMiddleware>(); // Extraction X-Game-Type
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapCharacterEndpoints();
app.MapCampaignEndpoints();
// ... autres endpoints

// SignalR Hubs
app.MapHub<SessionHub>("/hubs/session");
app.MapHub<CombatHub>("/hubs/combat");
app.MapHub<NotificationHub>("/hubs/notification");

// Health checks
app.MapDefaultEndpoints();

app.Run();
```

---

### 📊 Backend / Business Logic

#### Architecture des Couches Métier

```
┌──────────────────────────────────────────────────────────┐
│              Cdm.Business.Abstraction                     │
│  Interfaces communes pour tous les types de jeu          │
│  ICharacterService, ICombatService, etc.                 │
└────────────────────┬─────────────────────────────────────┘
                     │
                     │ Implément
                     ▼
┌──────────────────────────────────────────────────────────┐
│              Cdm.Business.Common                          │
│  Logique métier COMMUNE à tous les types de jeu          │
│  - CRUD Characters, Campaigns, Sessions                  │
│  - Orchestration des règles spécifiques                  │
│  - Validation de compatibilité (tags)                    │
└────┬──────────────────────────────────────────────┬──────┘
     │                                              │
     │ Délègue aux                                  │
     │ règles spécifiques                           │
     ▼                                              ▼
┌──────────────────┐                    ┌──────────────────┐
│ Cdm.Business.Dnd │                    │Cdm.Business.     │
│                  │                    │Skyrim            │
│ Règles D&D 5e:   │                    │                  │
│ - Calculs auto   │                    │ Règles Skyrim:   │
│ - Initiative     │                    │ - Compétences    │
│ - Jets d'attaque │                    │ - Magicka        │
│ - Dégâts         │                    │ - Niveau         │
└──────────────────┘                    └──────────────────┘
```

#### `Cdm.Business.Common`

**Responsabilité** : Logique métier commune (CRUD, orchestration)

**Exemple - CharacterService.cs** :
```csharp
public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _repository;
    private readonly IDndCharacterValidator _dndValidator;
    private readonly ISkyrimCharacterValidator _skyrimValidator;
    
    public async Task<Result<Character>> CreateAsync(
        CreateCharacterRequest request,
        GameType? gameType)
    {
        // 1. Validation commune
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Fail("Name is required");
        
        // 2. Validation spécifique selon le type de jeu
        if (gameType == GameType.DnD)
        {
            var validationResult = await _dndValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return Result.Fail(validationResult.Errors);
        }
        else if (gameType == GameType.Skyrim)
        {
            var validationResult = await _skyrimValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return Result.Fail(validationResult.Errors);
        }
        
        // 3. Création de l'entité
        var character = new Character
        {
            UserId = request.UserId,
            Name = request.Name,
            Life = request.Life,
            GameType = gameType ?? GameType.Generic,
            AttributesJson = JsonSerializer.Serialize(request.Attributes),
            CreatedAt = DateTime.UtcNow
        };
        
        // 4. Sauvegarde
        await _repository.AddAsync(character);
        
        return Result.Success(character);
    }
    
    public async Task<Result<Character>> GetByIdAsync(int id, int userId)
    {
        var character = await _repository.GetByIdAsync(id);
        
        if (character == null)
            return Result.Fail("Character not found");
        
        // Vérification de propriété
        if (character.UserId != userId)
            return Result.Fail("Unauthorized");
        
        return Result.Success(character);
    }
    
    // Autres méthodes : Update, Delete, List, etc.
}
```

#### `Cdm.Business.Dnd` / `Cdm.Business.Skyrim`

**Responsabilité** : Règles métier SPÉCIFIQUES à un type de jeu

**Exemple - DndCombatService.cs** :
```csharp
public class DndCombatService : IDndCombatService
{
    public async Task<InitiativeResult> CalculateInitiativeAsync(
        int characterId)
    {
        var character = await _characterRepository.GetByIdAsync(characterId);
        var attributes = JsonSerializer.Deserialize<DndAttributes>(
            character.AttributesJson);
        
        // Calcul D&D : 1d20 + modificateur Dextérité
        var roll = _diceService.Roll("1d20");
        var dexModifier = CalculateModifier(attributes.Dexterity);
        
        return new InitiativeResult
        {
            CharacterId = characterId,
            Roll = roll,
            Modifier = dexModifier,
            Total = roll + dexModifier
        };
    }
    
    public async Task<AttackResult> CalculateAttackAsync(
        int attackerId,
        int targetId,
        int weaponId)
    {
        var attacker = await GetCharacterWithAttributesAsync(attackerId);
        var target = await GetCharacterWithAttributesAsync(targetId);
        var weapon = await _weaponRepository.GetByIdAsync(weaponId);
        
        // Jet d'attaque : 1d20 + modificateur + bonus de maîtrise
        var attackRoll = _diceService.Roll("1d20");
        var modifier = GetWeaponModifier(attacker, weapon);
        var proficiencyBonus = CalculateProficiencyBonus(attacker.Level);
        
        var totalAttack = attackRoll + modifier + proficiencyBonus;
        var targetAC = target.ArmorClass;
        
        var hit = totalAttack >= targetAC;
        
        if (!hit)
            return new AttackResult { Hit = false, Damage = 0 };
        
        // Calcul des dégâts
        var damageRoll = _diceService.Roll(weapon.DamageFormula);
        var isCritical = attackRoll == 20;
        var damage = isCritical ? damageRoll * 2 : damageRoll;
        
        return new AttackResult
        {
            Hit = true,
            Damage = damage + modifier,
            IsCritical = isCritical
        };
    }
    
    private int CalculateModifier(int abilityScore)
    {
        return (abilityScore - 10) / 2;
    }
    
    private int CalculateProficiencyBonus(int level)
    {
        return (level - 1) / 4 + 2; // +2 au niveau 1, +3 au 5, etc.
    }
}
```

---

## 🔄 Flux de Données Multi-Type

### Principe du Header `X-Game-Type`

Le client envoie un header HTTP qui détermine quelle couche métier utiliser :

```
X-Game-Type: generic  →  Logique générique uniquement
X-Game-Type: dnd      →  Logique commune + règles D&D
X-Game-Type: skyrim   →  Logique commune + règles Skyrim
```

### Middleware d'Extraction

```csharp
// Cdm.ApiService/Middleware/GameTypeMiddleware.cs
public class GameTypeMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Game-Type", out var gameTypeHeader))
        {
            if (Enum.TryParse<GameType>(gameTypeHeader, true, out var gameType))
            {
                context.Items["GameType"] = gameType;
            }
        }
        
        await _next(context);
    }
}
```

### Flux Complet - Exemple Combat D&D

```
1. Client Blazor
   POST /api/combat/attack
   Headers: { X-Game-Type: dnd }
   Body: { attackerId: 5, targetId: 12, weaponId: 3 }
   
2. GameTypeMiddleware
   → Extrait "dnd" et stocke dans HttpContext.Items
   
3. CombatEndpoints.Attack()
   → Récupère le GameType depuis HttpContext.Items
   → Appelle CombatService.ExecuteAttackAsync(gameType, request)
   
4. CombatService (Cdm.Business.Common)
   → Vérifie le GameType
   → Délègue à DndCombatService si gameType == DnD
   
5. DndCombatService (Cdm.Business.Dnd)
   → Récupère les données via Cdm.Data.Common
   → Applique les règles D&D (jets, modificateurs, CA)
   → Peut charger des données spécifiques via Cdm.Data.Dnd
   
6. Retour
   → DndCombatService → CombatService → CombatEndpoints
   → Conversion en DTO
   → Retour JSON au client
   
7. Notification temps réel
   → CombatHub.NotifyAttackResult(combatId, result)
   → Tous les clients du groupe "combat-{id}" reçoivent la notif
```

---

## 🎯 Patterns Architecturaux

### 1. Repository Pattern

**Pourquoi** : Abstraction de l'accès aux données

```csharp
public interface ICharacterRepository
{
    Task<Character> GetByIdAsync(int id);
    Task<List<Character>> GetByUserIdAsync(int userId);
    Task AddAsync(Character character);
    Task UpdateAsync(Character character);
    Task DeleteAsync(int id);
}

public class CharacterRepository : ICharacterRepository
{
    private readonly MigrationsContext _context;
    
    public async Task<Character> GetByIdAsync(int id)
    {
        return await _context.Characters
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    // ... autres méthodes
}
```

### 2. Result Pattern

**Pourquoi** : Gestion explicite des erreurs sans exceptions

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public List<string> Errors { get; }
    
    public static Result<T> Success(T value) => 
        new Result<T>(true, value, null);
    
    public static Result<T> Fail(params string[] errors) => 
        new Result<T>(false, default, errors.ToList());
}

// Utilisation
public async Task<Result<Character>> CreateCharacterAsync(...)
{
    if (!IsValid(request))
        return Result<Character>.Fail("Invalid request");
    
    var character = await _repository.AddAsync(...);
    return Result<Character>.Success(character);
}
```

### 3. Strategy Pattern (pour les règles de jeu)

**Pourquoi** : Changement dynamique de comportement selon le type de jeu

```csharp
public interface ICombatStrategy
{
    Task<AttackResult> ExecuteAttackAsync(int attackerId, int targetId);
    Task<InitiativeResult> CalculateInitiativeAsync(int characterId);
}

public class DndCombatStrategy : ICombatStrategy { /* ... */ }
public class SkyrimCombatStrategy : ICombatStrategy { /* ... */ }
public class GenericCombatStrategy : ICombatStrategy { /* ... */ }

// Factory
public class CombatStrategyFactory
{
    public ICombatStrategy GetStrategy(GameType gameType)
    {
        return gameType switch
        {
            GameType.DnD => new DndCombatStrategy(),
            GameType.Skyrim => new SkyrimCombatStrategy(),
            _ => new GenericCombatStrategy()
        };
    }
}
```

### 4. Unit of Work (pour les transactions)

**Pourquoi** : Transactions atomiques multi-repository

```csharp
public interface IUnitOfWork : IDisposable
{
    ICharacterRepository Characters { get; }
    IEquipmentRepository Equipment { get; }
    IInventoryRepository Inventory { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

---

## 🛠️ Technologies Utilisées

### Backend
- **.NET 10** : Framework principal
- **Aspire** : Orchestration et observabilité
- **Entity Framework Core 10** : ORM
- **SignalR** : Communication temps réel
- **FluentValidation** : Validation des modèles
- **BCrypt.Net** : Hashing des mots de passe
- **JWT** : Authentification

### Frontend
- **Blazor Server** : Framework UI
- **SignalR Client** : Communication temps réel
- **Bootstrap 5** : Framework CSS

### Base de Données
- **SQL Server** : Stockage principal
- **Ports** : 3307 (Prod), 3308 (Dev)

### DevOps
- **Aspire Dashboard** : Monitoring (dev)
- **OpenTelemetry** : Logs, metrics, traces
- **Docker** : Conteneurisation (future)

---

## 📝 Prochaines Étapes

Pour approfondir l'architecture, consultez les documents suivants :

1. **[Modèle de Données](./MODELE_DONNEES.md)** : Schéma complet de la base de données
2. **[API & Endpoints](./API_ENDPOINTS.md)** : Documentation des endpoints REST
3. **[SignalR & Temps Réel](./SIGNALR_TEMPS_REEL.md)** : Hubs et communication temps réel
4. **[Sécurité](./SECURITE.md)** : Authentification, autorisations, validation
5. **[Frontend Blazor](./FRONTEND_BLAZOR.md)** : Structure et conventions
6. **[Standards de Code](./STANDARDS_CODE.md)** : Conventions et best practices

---

**Version** : 1.0  
**Dernière mise à jour** : 15 octobre 2025  
**Auteurs** : Équipe Chronique des Mondes
