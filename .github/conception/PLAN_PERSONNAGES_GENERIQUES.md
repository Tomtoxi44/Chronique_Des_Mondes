# Plan d'implémentation : Personnages Génériques

## Vue d'ensemble

Implémentation du système de personnages génériques (base pour D&D et Skyrim plus tard) avec architecture TPH (Table Per Hierarchy).

---

## Phase 1 : Modèle de données

### 1.1 Entités EF Core

#### `Character` (classe de base - TPH)

**Fichier :** `Cdm.Data.Common/Models/Character.cs`

```csharp
public abstract class Character
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    
    // Discriminator pour TPH (Generic/Dnd/Skyrim)
    public GameType GameType { get; set; }
    
    // Stats communes
    public int Level { get; set; } = 1;
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    
    // Métadonnées
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Relations
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<CampaignCharacter> CampaignCharacters { get; set; } = new List<CampaignCharacter>();
}
```

#### `GenericCharacter` (hérite de Character)

**Fichier :** `Cdm.Data.Common/Models/GenericCharacter.cs`

```csharp
public class GenericCharacter : Character
{
    // Attributs flexibles pour système générique
    public string? CharacterClass { get; set; } // Ex: "Guerrier", "Mage"
    public string? Race { get; set; }
    
    // Stats personnalisables (JSON)
    public string? CustomAttributes { get; set; } // JSON libre
    
    // Inventaire simplifié
    public string? Inventory { get; set; } // JSON: ["Épée", "Bouclier"]
    
    // Notes du joueur
    public string? Notes { get; set; }
}
```

#### `CampaignCharacter` (table de liaison)

**Fichier :** `Cdm.Data.Common/Models/CampaignCharacter.cs`

```csharp
public class CampaignCharacter
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int CharacterId { get; set; }
    
    // Rôle dans la campagne
    public bool IsMainCharacter { get; set; } = true; // false = PNJ
    
    // État dans cette campagne spécifique
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }
    
    // Relations
    public virtual Campaign Campaign { get; set; } = null!;
    public virtual Character Character { get; set; } = null!;
}
```

### 1.2 Configuration EF Core

**Fichier :** `Cdm.Data.Common/AppDbContext.cs`

```csharp
// Dans OnModelCreating
modelBuilder.Entity<Character>(entity =>
{
    // TPH configuration
    entity.HasDiscriminator<GameType>(c => c.GameType)
        .HasValue<GenericCharacter>(GameType.Generic)
        .HasValue<DndCharacter>(GameType.Dnd)
        .HasValue<SkyrimCharacter>(GameType.Skyrim);
    
    // Index
    entity.HasIndex(c => c.UserId)
        .HasDatabaseName("IX_Characters_UserId");
    
    entity.HasIndex(c => new { c.GameType, c.IsActive })
        .HasDatabaseName("IX_Characters_GameType_IsActive");
    
    entity.HasIndex(c => c.Name)
        .HasDatabaseName("IX_Characters_Name");
    
    // Relation User
    entity.HasOne(c => c.Owner)
        .WithMany()
        .HasForeignKey(c => c.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    
    // Defaults
    entity.Property(c => c.CreatedAt)
        .HasDefaultValueSql("GETUTCDATE()");
    
    entity.Property(c => c.IsActive)
        .HasDefaultValue(true);
    
    entity.Property(c => c.Level)
        .HasDefaultValue(1);
});

// CampaignCharacter configuration
modelBuilder.Entity<CampaignCharacter>(entity =>
{
    entity.HasOne(cc => cc.Campaign)
        .WithMany()
        .HasForeignKey(cc => cc.CampaignId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasOne(cc => cc.Character)
        .WithMany(c => c.CampaignCharacters)
        .HasForeignKey(cc => cc.CharacterId)
        .OnDelete(DeleteBehavior.Restrict); // Éviter suppression en cascade
    
    entity.HasIndex(cc => new { cc.CampaignId, cc.CharacterId })
        .IsUnique()
        .HasDatabaseName("IX_CampaignCharacters_CampaignId_CharacterId");
});
```

### 1.3 Migration

**Commande :**
```bash
dotnet ef migrations add AddCharactersTable --project Cdm/Cdm.Migrations --startup-project Cdm/Cdm.MigrationsManager --context MigrationsContext
```

---

## Phase 2 : Couche Business

### 2.1 DTOs

#### `Cdm.Business.Abstraction/DTOs/CharacterDto.cs`

```csharp
public class CharacterDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public GameType GameType { get; set; }
    public int Level { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### `Cdm.Business.Abstraction/DTOs/GenericCharacterDto.cs`

```csharp
public class GenericCharacterDto : CharacterDto
{
    public string? CharacterClass { get; set; }
    public string? Race { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
    public List<string>? Inventory { get; set; }
    public string? Notes { get; set; }
}
```

#### `Cdm.Business.Abstraction/DTOs/CreateGenericCharacterDto.cs`

```csharp
public class CreateGenericCharacterDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CharacterClass { get; set; }
    public string? Race { get; set; }
    public int MaxHealth { get; set; } = 100;
    public Dictionary<string, object>? CustomAttributes { get; set; }
    public string? Notes { get; set; }
}
```

#### `Cdm.Business.Abstraction/DTOs/UpdateCharacterDto.cs`

```csharp
public class UpdateCharacterDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? CurrentHealth { get; set; }
    public int? MaxHealth { get; set; }
    public string? Notes { get; set; }
}
```

### 2.2 Service Interface

**Fichier :** `Cdm.Business.Abstraction/Services/ICharacterService.cs`

```csharp
public interface ICharacterService
{
    // Create
    Task<Result<GenericCharacterDto>> CreateGenericCharacterAsync(int userId, CreateGenericCharacterDto dto);
    
    // Read
    Task<Result<GenericCharacterDto>> GetCharacterByIdAsync(int characterId, int userId);
    Task<Result<List<CharacterDto>>> GetUserCharactersAsync(int userId, GameType? gameType = null);
    
    // Update
    Task<Result<GenericCharacterDto>> UpdateCharacterAsync(int characterId, int userId, UpdateCharacterDto dto);
    
    // Delete
    Task<Result<bool>> DeleteCharacterAsync(int characterId, int userId);
    
    // Campaigns
    Task<Result<bool>> AddCharacterToCampaignAsync(int characterId, int campaignId, int userId);
    Task<Result<bool>> RemoveCharacterFromCampaignAsync(int characterId, int campaignId, int userId);
    
    // Ownership check
    Task<bool> UserOwnsCharacterAsync(int characterId, int userId);
}
```

### 2.3 Implémentation du Service

**Fichier :** `Cdm.Business.Common/Services/CharacterService.cs`

```csharp
public class CharacterService : ICharacterService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CharacterService> _logger;
    
    public CharacterService(AppDbContext context, ILogger<CharacterService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Result<GenericCharacterDto>> CreateGenericCharacterAsync(int userId, CreateGenericCharacterDto dto)
    {
        _logger.LogInformation("Creating generic character {Name} for user {UserId}", dto.Name, userId);
        
        var character = new GenericCharacter
        {
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            GameType = GameType.Generic,
            CharacterClass = dto.CharacterClass,
            Race = dto.Race,
            MaxHealth = dto.MaxHealth,
            CurrentHealth = dto.MaxHealth, // Full health at creation
            CustomAttributes = dto.CustomAttributes != null 
                ? JsonSerializer.Serialize(dto.CustomAttributes) 
                : null,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Characters.Add(character);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Character {CharacterId} created successfully", character.Id);
        
        return Result<GenericCharacterDto>.Success(MapToDto(character));
    }
    
    public async Task<Result<GenericCharacterDto>> GetCharacterByIdAsync(int characterId, int userId)
    {
        var character = await _context.Characters
            .OfType<GenericCharacter>()
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId && c.IsActive);
        
        if (character == null)
        {
            return Result<GenericCharacterDto>.Failure("Character not found");
        }
        
        return Result<GenericCharacterDto>.Success(MapToDto(character));
    }
    
    public async Task<Result<List<CharacterDto>>> GetUserCharactersAsync(int userId, GameType? gameType = null)
    {
        var query = _context.Characters
            .Where(c => c.UserId == userId && c.IsActive);
        
        if (gameType.HasValue)
        {
            query = query.Where(c => c.GameType == gameType.Value);
        }
        
        var characters = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        
        var dtos = characters.Select(c => new CharacterDto
        {
            Id = c.Id,
            UserId = c.UserId,
            Name = c.Name,
            Description = c.Description,
            AvatarUrl = c.AvatarUrl,
            GameType = c.GameType,
            Level = c.Level,
            CurrentHealth = c.CurrentHealth,
            MaxHealth = c.MaxHealth,
            CreatedAt = c.CreatedAt
        }).ToList();
        
        return Result<List<CharacterDto>>.Success(dtos);
    }
    
    public async Task<Result<GenericCharacterDto>> UpdateCharacterAsync(int characterId, int userId, UpdateCharacterDto dto)
    {
        var character = await _context.Characters
            .OfType<GenericCharacter>()
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId);
        
        if (character == null)
        {
            return Result<GenericCharacterDto>.Failure("Character not found");
        }
        
        if (!string.IsNullOrEmpty(dto.Name))
            character.Name = dto.Name;
        
        if (dto.Description != null)
            character.Description = dto.Description;
        
        if (dto.CurrentHealth.HasValue)
            character.CurrentHealth = Math.Min(dto.CurrentHealth.Value, character.MaxHealth);
        
        if (dto.MaxHealth.HasValue)
            character.MaxHealth = dto.MaxHealth.Value;
        
        if (dto.Notes != null)
            character.Notes = dto.Notes;
        
        character.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return Result<GenericCharacterDto>.Success(MapToDto(character));
    }
    
    public async Task<Result<bool>> DeleteCharacterAsync(int characterId, int userId)
    {
        var character = await _context.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == userId);
        
        if (character == null)
        {
            return Result<bool>.Failure("Character not found");
        }
        
        // Soft delete
        character.IsActive = false;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Character {CharacterId} deleted by user {UserId}", characterId, userId);
        
        return Result<bool>.Success(true);
    }
    
    public async Task<bool> UserOwnsCharacterAsync(int characterId, int userId)
    {
        return await _context.Characters
            .AnyAsync(c => c.Id == characterId && c.UserId == userId);
    }
    
    // Mapper privé
    private static GenericCharacterDto MapToDto(GenericCharacter character)
    {
        return new GenericCharacterDto
        {
            Id = character.Id,
            UserId = character.UserId,
            Name = character.Name,
            Description = character.Description,
            AvatarUrl = character.AvatarUrl,
            GameType = character.GameType,
            Level = character.Level,
            CurrentHealth = character.CurrentHealth,
            MaxHealth = character.MaxHealth,
            CharacterClass = character.CharacterClass,
            Race = character.Race,
            CustomAttributes = !string.IsNullOrEmpty(character.CustomAttributes)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(character.CustomAttributes)
                : null,
            Inventory = !string.IsNullOrEmpty(character.Inventory)
                ? JsonSerializer.Deserialize<List<string>>(character.Inventory)
                : null,
            Notes = character.Notes,
            CreatedAt = character.CreatedAt
        };
    }
}
```

---

## Phase 3 : API Endpoints

### 3.1 Models de requête/réponse

**Fichier :** `Cdm.ApiService/Endpoints/Models/CreateCharacterRequest.cs`

```csharp
public class CreateCharacterRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? CharacterClass { get; set; }
    public string? Race { get; set; }
    public int MaxHealth { get; set; } = 100;
    public Dictionary<string, object>? CustomAttributes { get; set; }
    public string? Notes { get; set; }
}
```

**Fichier :** `Cdm.ApiService/Endpoints/Models/CharacterResponse.cs`

```csharp
public class CharacterResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string GameType { get; set; } = string.Empty;
    public int Level { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public string? CharacterClass { get; set; }
    public string? Race { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
    public List<string>? Inventory { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 3.2 Endpoints

**Fichier :** `Cdm.ApiService/Endpoints/CharacterEndpoints.cs`

```csharp
public static class CharacterEndpoints
{
    public static void MapCharacterEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/characters")
            .RequireAuthorization()
            .WithTags("Characters");
        
        // GET /api/characters - Liste des personnages de l'utilisateur
        group.MapGet("/", GetMyCharacters);
        
        // GET /api/characters/{id} - Détail d'un personnage
        group.MapGet("/{id:int}", GetCharacterById);
        
        // POST /api/characters - Créer un personnage générique
        group.MapPost("/", CreateCharacter);
        
        // PUT /api/characters/{id} - Modifier un personnage
        group.MapPut("/{id:int}", UpdateCharacter);
        
        // DELETE /api/characters/{id} - Supprimer un personnage
        group.MapDelete("/{id:int}", DeleteCharacter);
    }
    
    private static async Task<IResult> GetMyCharacters(
        HttpContext context,
        ICharacterService characterService,
        [FromQuery] string? gameType = null)
    {
        var userId = GetUserId(context);
        
        GameType? gameTypeEnum = null;
        if (!string.IsNullOrEmpty(gameType) && Enum.TryParse<GameType>(gameType, true, out var parsed))
        {
            gameTypeEnum = parsed;
        }
        
        var result = await characterService.GetUserCharactersAsync(userId, gameTypeEnum);
        
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { error = result.Error });
        }
        
        return Results.Ok(new { characters = result.Data });
    }
    
    private static async Task<IResult> GetCharacterById(
        int id,
        HttpContext context,
        ICharacterService characterService)
    {
        var userId = GetUserId(context);
        var result = await characterService.GetCharacterByIdAsync(id, userId);
        
        if (!result.IsSuccess)
        {
            return Results.NotFound(new { error = result.Error });
        }
        
        return Results.Ok(MapToResponse(result.Data));
    }
    
    private static async Task<IResult> CreateCharacter(
        CreateCharacterRequest request,
        HttpContext context,
        ICharacterService characterService)
    {
        var userId = GetUserId(context);
        
        var dto = new CreateGenericCharacterDto
        {
            Name = request.Name,
            Description = request.Description,
            CharacterClass = request.CharacterClass,
            Race = request.Race,
            MaxHealth = request.MaxHealth,
            CustomAttributes = request.CustomAttributes,
            Notes = request.Notes
        };
        
        var result = await characterService.CreateGenericCharacterAsync(userId, dto);
        
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new { error = result.Error });
        }
        
        return Results.Created($"/api/characters/{result.Data.Id}", MapToResponse(result.Data));
    }
    
    private static async Task<IResult> UpdateCharacter(
        int id,
        UpdateCharacterRequest request,
        HttpContext context,
        ICharacterService characterService)
    {
        var userId = GetUserId(context);
        
        var dto = new UpdateCharacterDto
        {
            Name = request.Name,
            Description = request.Description,
            CurrentHealth = request.CurrentHealth,
            MaxHealth = request.MaxHealth,
            Notes = request.Notes
        };
        
        var result = await characterService.UpdateCharacterAsync(id, userId, dto);
        
        if (!result.IsSuccess)
        {
            return Results.NotFound(new { error = result.Error });
        }
        
        return Results.Ok(MapToResponse(result.Data));
    }
    
    private static async Task<IResult> DeleteCharacter(
        int id,
        HttpContext context,
        ICharacterService characterService)
    {
        var userId = GetUserId(context);
        var result = await characterService.DeleteCharacterAsync(id, userId);
        
        if (!result.IsSuccess)
        {
            return Results.NotFound(new { error = result.Error });
        }
        
        return Results.NoContent();
    }
    
    private static int GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }
    
    private static CharacterResponse MapToResponse(GenericCharacterDto dto)
    {
        return new CharacterResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            AvatarUrl = dto.AvatarUrl,
            GameType = dto.GameType.ToString(),
            Level = dto.Level,
            CurrentHealth = dto.CurrentHealth,
            MaxHealth = dto.MaxHealth,
            CharacterClass = dto.CharacterClass,
            Race = dto.Race,
            CustomAttributes = dto.CustomAttributes,
            Inventory = dto.Inventory,
            Notes = dto.Notes,
            CreatedAt = dto.CreatedAt
        };
    }
}
```

### 3.3 Enregistrement dans Program.cs

**Fichier :** `Cdm.ApiService/Program.cs`

```csharp
// Ajouter après les autres endpoints
app.MapCharacterEndpoints();
```

**Fichier :** `Cdm.ApiService/Program.cs` (DI)

```csharp
// Ajouter dans la configuration des services
builder.Services.AddScoped<ICharacterService, CharacterService>();
```

---

## Phase 4 : Frontend Blazor

### 4.1 API Client

**Fichier :** `Cdm.Web/Services/ApiClients/CharacterApiClient.cs`

```csharp
public class CharacterApiClient : BaseApiClient
{
    public CharacterApiClient(HttpClient httpClient, ILocalStorageService localStorage)
        : base(httpClient, localStorage)
    {
    }
    
    public async Task<List<CharacterDto>?> GetMyCharactersAsync(GameType? gameType = null)
    {
        var query = gameType.HasValue ? $"?gameType={gameType}" : "";
        var response = await GetAsync<GetCharactersResponse>($"/api/characters{query}");
        return response?.Characters;
    }
    
    public async Task<CharacterResponse?> GetCharacterByIdAsync(int id)
    {
        return await GetAsync<CharacterResponse>($"/api/characters/{id}");
    }
    
    public async Task<CharacterResponse?> CreateCharacterAsync(CreateCharacterRequest request)
    {
        return await PostAsync<CharacterResponse>("/api/characters", request);
    }
    
    public async Task<CharacterResponse?> UpdateCharacterAsync(int id, UpdateCharacterRequest request)
    {
        return await PutAsync<CharacterResponse>($"/api/characters/{id}", request);
    }
    
    public async Task<bool> DeleteCharacterAsync(int id)
    {
        var response = await DeleteAsync($"/api/characters/{id}");
        return response.IsSuccessStatusCode;
    }
}
```

### 4.2 Page Liste des Personnages

**Fichier :** `Cdm.Web/Components/Pages/Characters/Index.razor`

```razor
@page "/characters"
@using Cdm.Web.Components.Layout
@layout AppLayout
@attribute [Authorize]

<PageTitle>Mes Personnages - Chronique des Mondes</PageTitle>

<div class="container" style="padding-top: var(--spacing-xl);">
    <div class="page-header">
        <h1><i class="bi bi-person-badge"></i> Mes Personnages</h1>
        <a href="/characters/create" class="btn btn-primary">
            <i class="bi bi-plus-circle"></i> Créer un personnage
        </a>
    </div>
    
    <!-- Filters -->
    <div class="filters-bar">
        <select @bind="selectedGameType" class="form-select">
            <option value="">Tous les types</option>
            <option value="Generic">Générique</option>
            <option value="Dnd">D&D 5e</option>
            <option value="Skyrim">Skyrim</option>
        </select>
    </div>
    
    @if (isLoading)
    {
        <div class="text-center" style="padding: var(--spacing-xxl);">
            <div class="spinner-border"></div>
            <p>Chargement des personnages...</p>
        </div>
    }
    else if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger">
            <i class="bi bi-exclamation-triangle"></i> @errorMessage
        </div>
    }
    else if (characters == null || !characters.Any())
    {
        <div class="empty-state">
            <i class="bi bi-person-x" style="font-size: 4rem;"></i>
            <h3>Aucun personnage</h3>
            <p>Créez votre premier héros pour commencer l'aventure !</p>
            <a href="/characters/create" class="btn btn-primary">
                <i class="bi bi-plus-circle"></i> Créer un personnage
            </a>
        </div>
    }
    else
    {
        <div class="characters-grid">
            @foreach (var character in characters)
            {
                <div class="character-card" @onclick="() => NavigateToCharacter(character.Id)">
                    <div class="character-avatar">
                        @if (!string.IsNullOrEmpty(character.AvatarUrl))
                        {
                            <img src="@character.AvatarUrl" alt="@character.Name" />
                        }
                        else
                        {
                            <div class="avatar-placeholder">
                                <i class="bi bi-person"></i>
                            </div>
                        }
                    </div>
                    
                    <div class="character-info">
                        <h3>@character.Name</h3>
                        
                        @if (!string.IsNullOrEmpty(character.Description))
                        {
                            <p class="description">@character.Description</p>
                        }
                        
                        <div class="character-meta">
                            <span class="badge badge-@character.GameType.ToLower()">
                                @character.GameType
                            </span>
                            <span class="level">Niveau @character.Level</span>
                        </div>
                        
                        <div class="health-bar">
                            <div class="health-fill" style="width: @GetHealthPercentage(character)%"></div>
                            <span class="health-text">@character.CurrentHealth / @character.MaxHealth HP</span>
                        </div>
                    </div>
                    
                    <div class="character-actions">
                        <button class="btn btn-sm btn-outline-primary" @onclick:stopPropagation="true" @onclick="() => EditCharacter(character.Id)">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger" @onclick:stopPropagation="true" @onclick="() => DeleteCharacter(character.Id)">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            }
        </div>
    }
</div>
```

### 4.3 Page Création de Personnage

**Fichier :** `Cdm.Web/Components/Pages/Characters/Create.razor`

```razor
@page "/characters/create"
@using Cdm.Web.Components.Layout
@layout AppLayout
@attribute [Authorize]

<PageTitle>Créer un Personnage - Chronique des Mondes</PageTitle>

<div class="container" style="padding-top: var(--spacing-xl); max-width: 800px;">
    <div class="card">
        <div class="card-header">
            <h2><i class="bi bi-plus-circle"></i> Créer un Personnage</h2>
        </div>
        
        <div class="card-body">
            @if (!string.IsNullOrEmpty(errorMessage))
            {
                <div class="alert alert-danger">
                    <i class="bi bi-exclamation-triangle"></i> @errorMessage
                </div>
            }
            
            <EditForm Model="@model" OnValidSubmit="HandleCreateAsync">
                <DataAnnotationsValidator />
                
                <!-- Nom -->
                <div class="form-group">
                    <label for="name" class="required">Nom du personnage</label>
                    <InputText id="name" @bind-Value="model.Name" class="form-control" placeholder="Ex: Aragorn" />
                    <ValidationMessage For="@(() => model.Name)" />
                </div>
                
                <!-- Description -->
                <div class="form-group">
                    <label for="description">Description</label>
                    <InputTextArea id="description" @bind-Value="model.Description" class="form-control" rows="3" placeholder="Décrivez votre personnage..." />
                </div>
                
                <!-- Classe -->
                <div class="form-group">
                    <label for="characterClass">Classe</label>
                    <InputText id="characterClass" @bind-Value="model.CharacterClass" class="form-control" placeholder="Ex: Guerrier, Mage, Rôdeur" />
                </div>
                
                <!-- Race -->
                <div class="form-group">
                    <label for="race">Race</label>
                    <InputText id="race" @bind-Value="model.Race" class="form-control" placeholder="Ex: Humain, Elfe, Nain" />
                </div>
                
                <!-- Points de vie max -->
                <div class="form-group">
                    <label for="maxHealth" class="required">Points de vie maximum</label>
                    <InputNumber id="maxHealth" @bind-Value="model.MaxHealth" class="form-control" min="1" />
                    <ValidationMessage For="@(() => model.MaxHealth)" />
                </div>
                
                <!-- Notes -->
                <div class="form-group">
                    <label for="notes">Notes</label>
                    <InputTextArea id="notes" @bind-Value="model.Notes" class="form-control" rows="4" placeholder="Notes personnelles..." />
                </div>
                
                <!-- Actions -->
                <div class="form-actions">
                    <button type="submit" class="btn btn-primary" disabled="@isSubmitting">
                        @if (isSubmitting)
                        {
                            <span class="spinner-border spinner-border-sm"></span>
                            <span>Création...</span>
                        }
                        else
                        {
                            <i class="bi bi-check-circle"></i>
                            <span>Créer le personnage</span>
                        }
                    </button>
                    <a href="/characters" class="btn btn-outline-secondary">Annuler</a>
                </div>
            </EditForm>
        </div>
    </div>
</div>
```

---

## Phase 5 : Tests

### 5.1 Tests unitaires du service

**Fichier :** `Cdm.Business.Common.Tests/Services/CharacterServiceTests.cs`

```csharp
public class CharacterServiceTests
{
    [Fact]
    public async Task CreateGenericCharacterAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var logger = Mock.Of<ILogger<CharacterService>>();
        var service = new CharacterService(context, logger);
        
        var dto = new CreateGenericCharacterDto
        {
            Name = "Test Character",
            CharacterClass = "Warrior",
            MaxHealth = 100
        };
        
        // Act
        var result = await service.CreateGenericCharacterAsync(1, dto);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("Test Character");
        result.Data.MaxHealth.Should().Be(100);
        result.Data.CurrentHealth.Should().Be(100); // Full health at creation
    }
    
    [Fact]
    public async Task GetUserCharactersAsync_FiltersCorrectly_ReturnsOnlyUserCharacters()
    {
        // Arrange
        var context = CreateInMemoryContext();
        await SeedCharacters(context);
        
        var service = new CharacterService(context, Mock.Of<ILogger<CharacterService>>());
        
        // Act
        var result = await service.GetUserCharactersAsync(userId: 1);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().AllSatisfy(c => c.UserId.Should().Be(1));
    }
}
```

---

## Checklist d'implémentation

### Phase 1 : Modèle
- [ ] Créer `Character.cs` (classe de base)
- [ ] Créer `GenericCharacter.cs`
- [ ] Créer `CampaignCharacter.cs`
- [ ] Configurer TPH dans `AppDbContext.cs`
- [ ] Créer et appliquer la migration

### Phase 2 : Business
- [ ] Créer DTOs (Character, GenericCharacter, Create, Update)
- [ ] Créer `ICharacterService.cs`
- [ ] Implémenter `CharacterService.cs`
- [ ] Enregistrer dans DI

### Phase 3 : API
- [ ] Créer Request/Response models
- [ ] Créer `CharacterEndpoints.cs`
- [ ] Mapper les endpoints dans `Program.cs`
- [ ] Tester avec Postman/Swagger

### Phase 4 : Frontend
- [ ] Créer `CharacterApiClient.cs`
- [ ] Créer page liste `/characters`
- [ ] Créer page création `/characters/create`
- [ ] Créer page détail `/characters/{id}`
- [ ] Ajouter styles CSS

### Phase 5 : Tests
- [ ] Tests unitaires `CharacterService`
- [ ] Tests d'intégration API
- [ ] Tests E2E Blazor

---

## Prochaines étapes

Après l'implémentation des personnages génériques :

1. **Avatar upload** - Permettre l'upload d'image de personnage
2. **Personnages D&D 5e** - Étendre avec attributs D&D (STR, DEX, etc.)
3. **Liaison aux campagnes** - Interface pour assigner personnages aux campagnes
4. **Sessions** - Utiliser les personnages dans les sessions de jeu

---

**Voulez-vous que je commence l'implémentation de ce plan ?**
