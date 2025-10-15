# US-027 - Création de PNJ Générique

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** créer des PNJ (Personnages Non-Joueurs) génériques  
**Afin de** les utiliser dans mes chapitres et campagnes

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page "Mes PNJ" (/npcs) avec bouton "➕ Créer un PNJ"
- [ ] Formulaire création avec champs :
  - [ ] Nom (requis, max 50 caractères)
  - [ ] Surnom/Titre (optionnel, ex: "Le Sage", "Chef des bandits")
  - [ ] Description (textarea, optionnel)
  - [ ] Points de Vie (HP) actuels (requis)
  - [ ] Points de Vie (HP) maximum (requis)
  - [ ] Avatar (upload image optionnel)
  - [ ] Type (dropdown : Allié, Ennemi, Neutre)
  - [ ] Comportement (textarea, optionnel) : Stratégie combat, personnalité
  - [ ] Attributs personnalisés (comme personnages joueurs)
- [ ] Validation :
  - [ ] Nom unique par MJ
  - [ ] HP actuel ≤ HP max
- [ ] Bouton "Créer" → Sauvegarde → Redirection vers liste PNJ
- [ ] Notification success : "✓ PNJ {Name} créé"

### Techniques
- [ ] Endpoint : `POST /api/npcs` [Authorize(Roles = "GameMaster")]
- [ ] Body :
```json
{
  "name": "Gandalf",
  "nickname": "Le Gris",
  "description": "Vieux mage puissant",
  "currentHP": 80,
  "maxHP": 100,
  "npcType": "Ally",
  "behavior": "Utilise magie à distance, évite corps-à-corps",
  "customAttributes": {
    "Magie": "95",
    "Force": "40"
  }
}
```
- [ ] Response 201 : NPCDto créé

---

## 🧪 Tests

### Tests Unitaires
- [ ] `NPCService.CreateNPC_ValidData_CreatesNPC()`
- [ ] `NPCService.CreateNPC_DuplicateName_ThrowsValidationException()`
- [ ] `NPCService.CreateNPC_NonGameMaster_ThrowsUnauthorizedException()`

### Tests d'Intégration
- [ ] `NPCEndpoint_CreateNPC_SavesInDatabase()`
- [ ] `NPCEndpoint_CreateNPC_OnlyGameMaster_CanCreate()`

### Tests E2E
- [ ] MJ remplit formulaire → Crée PNJ → Apparaît dans liste
- [ ] Joueur tente créer PNJ → Accès refusé

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `NPC` (hérite de `BaseCharacter`) :
```csharp
public class NPC : BaseCharacter
{
    public Guid CreatedBy { get; set; }
    public User Creator { get; set; }
    
    public string? Nickname { get; set; }
    public string? Description { get; set; }
    public NPCType NPCType { get; set; }
    public string? Behavior { get; set; }
    
    // Relations
    public ICollection<ChapterNPC> ChapterNPCs { get; set; }
}

public enum NPCType
{
    Ally = 0,
    Enemy = 1,
    Neutral = 2
}
```
- [ ] Créer `NPCService.CreateNPCAsync(createDto, userId)` :
```csharp
public async Task<NPCDto> CreateNPCAsync(NPCCreateDto dto, Guid userId)
{
    // Vérifier user a rôle GameMaster
    var user = await _context.Users.FindAsync(userId);
    if (!user.Roles.Contains("GameMaster"))
        throw new UnauthorizedException("Seuls les Maîtres du Jeu peuvent créer des PNJ");
    
    // Valider nom unique pour ce MJ
    var exists = await _context.NPCs
        .AnyAsync(n => n.CreatedBy == userId && n.Name == dto.Name);
    
    if (exists)
        throw new ValidationException("Un PNJ avec ce nom existe déjà");
    
    var npc = new NPC
    {
        Name = dto.Name,
        Nickname = dto.Nickname,
        Description = dto.Description,
        CurrentHP = dto.CurrentHP,
        MaxHP = dto.MaxHP,
        AvatarUrl = dto.AvatarUrl,
        GameType = dto.GameType,
        NPCType = dto.NPCType,
        Behavior = dto.Behavior,
        CustomAttributes = JsonSerializer.Serialize(dto.CustomAttributes ?? new()),
        CreatedBy = userId,
        CreatedAt = DateTime.UtcNow
    };
    
    _context.NPCs.Add(npc);
    await _context.SaveChangesAsync();
    
    return npc.ToDto();
}
```
- [ ] Créer endpoint `POST /api/npcs` [Authorize(Roles = "GameMaster")]

### Frontend
- [ ] Créer page `NPCs.razor` (/npcs) :
```razor
@page "/npcs"
@attribute [Authorize(Roles = "GameMaster")]
@inject HttpClient Http
@inject NavigationManager Nav

<div class="npcs-page">
    <div class="page-header">
        <h1>Mes PNJ</h1>
        <button @onclick="() => Nav.NavigateTo('/npcs/create')" class="btn-primary">
            ➕ Créer un PNJ
        </button>
    </div>
    
    <div class="npcs-grid">
        @foreach (var npc in NPCs)
        {
            <NPCCard NPC="@npc" OnClick="() => Nav.NavigateTo($"/npcs/{npc.Id}")" />
        }
    </div>
</div>

@code {
    private List<NPCDto> NPCs { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        NPCs = await Http.GetFromJsonAsync<List<NPCDto>>("/api/npcs") ?? new();
    }
}
```
- [ ] Créer page `NPCCreate.razor` (/npcs/create) :
```razor
@page "/npcs/create"
@attribute [Authorize(Roles = "GameMaster")]
@inject HttpClient Http
@inject NavigationManager Nav
@inject IToastService Toast

<div class="npc-create">
    <h1>Créer un PNJ</h1>
    
    <EditForm Model="@Model" OnValidSubmit="CreateNPC">
        <DataAnnotationsValidator />
        <ValidationSummary />
        
        <div class="form-group">
            <label>Nom *</label>
            <InputText @bind-Value="Model.Name" class="form-control" />
        </div>
        
        <div class="form-group">
            <label>Surnom / Titre</label>
            <InputText @bind-Value="Model.Nickname" 
                       class="form-control" 
                       placeholder="Ex: Le Sage, Chef des bandits..." />
        </div>
        
        <div class="form-group">
            <label>Description</label>
            <InputTextArea @bind-Value="Model.Description" 
                          class="form-control" 
                          rows="4"
                          placeholder="Apparence, personnalité..." />
        </div>
        
        <div class="form-row">
            <div class="form-group">
                <label>HP Actuels *</label>
                <InputNumber @bind-Value="Model.CurrentHP" class="form-control" />
            </div>
            <div class="form-group">
                <label>HP Maximum *</label>
                <InputNumber @bind-Value="Model.MaxHP" class="form-control" />
            </div>
        </div>
        
        <div class="form-group">
            <label>Type</label>
            <InputSelect @bind-Value="Model.NPCType" class="form-control">
                <option value="@NPCType.Ally">Allié</option>
                <option value="@NPCType.Enemy">Ennemi</option>
                <option value="@NPCType.Neutral">Neutre</option>
            </InputSelect>
        </div>
        
        <div class="form-group">
            <label>Comportement (combat, stratégie)</label>
            <InputTextArea @bind-Value="Model.Behavior" 
                          class="form-control" 
                          rows="3"
                          placeholder="Stratégie de combat, réactions..." />
        </div>
        
        <!-- Section Attributs personnalisés (même que personnages) -->
        
        <div class="form-actions">
            <button type="submit" class="btn-primary">Créer</button>
            <button @onclick="() => Nav.NavigateTo('/npcs')" class="btn-secondary">Annuler</button>
        </div>
    </EditForm>
</div>

@code {
    private NPCCreateDto Model { get; set; } = new();

    private async Task CreateNPC()
    {
        var response = await Http.PostAsJsonAsync("/api/npcs", Model);
        
        if (response.IsSuccessStatusCode)
        {
            Toast.Success($"✓ PNJ {Model.Name} créé");
            Nav.NavigateTo("/npcs");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Toast.Error(error);
        }
    }
}
```
- [ ] Créer composant `NPCCard.razor` :
```razor
<div class="npc-card @GetTypeClass()" @onclick="OnClick">
    <img src="@(NPC.AvatarUrl ?? "/images/default-npc.png")" 
         alt="@NPC.Name" 
         class="avatar" />
    
    <div class="card-content">
        <h3>@NPC.Name</h3>
        
        @if (!string.IsNullOrEmpty(NPC.Nickname))
        {
            <p class="nickname">@NPC.Nickname</p>
        }
        
        <div class="hp-bar">
            <div class="hp-fill" style="width: @GetHPPercentage()%"></div>
            <span>@NPC.CurrentHP / @NPC.MaxHP HP</span>
        </div>
        
        <span class="npc-type">@GetTypeLabel(NPC.NPCType)</span>
    </div>
</div>

@code {
    [Parameter] public NPCDto NPC { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    private int GetHPPercentage() => 
        NPC.MaxHP == 0 ? 0 : (int)((double)NPC.CurrentHP / NPC.MaxHP * 100);

    private string GetTypeClass() => NPC.NPCType.ToString().ToLower();

    private string GetTypeLabel(NPCType type) => type switch
    {
        NPCType.Ally => "Allié",
        NPCType.Enemy => "Ennemi",
        NPCType.Neutral => "Neutre",
        _ => ""
    };
}
```

### Base de Données
- [ ] Migration : NPCs via discriminator dans BaseCharacters
- [ ] Index sur `(CreatedBy, Name)` pour validation unicité

---

## 🔗 Dépendances

### Dépend de
- [US-001](../01-Epic-Authentification/US-001-inscription.md) - Utilisateurs avec rôle GameMaster

### Bloque
- [US-028](./US-028-association-pnj-chapitre.md) - Association PNJ ↔ Chapitre

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (CRUD + permissions)
- Effort : 1-2 jours
- Risques : Validation rôles

---

## 📝 Notes Techniques

### TPH Configuration
```csharp
modelBuilder.Entity<BaseCharacter>()
    .HasDiscriminator<string>("CharacterType")
    .HasValue<Character>("Player")
    .HasValue<NPC>("NPC");

// Index spécifiques NPCs
modelBuilder.Entity<NPC>()
    .HasIndex(n => new { n.CreatedBy, n.Name });
```

### CSS NPC Type Colors
```css
.npc-card.ally { border-left: 4px solid #4caf50; }
.npc-card.enemy { border-left: 4px solid #f44336; }
.npc-card.neutral { border-left: 4px solid #9e9e9e; }
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Formulaire création PNJ fonctionnel
- [ ] Validation rôle GameMaster
- [ ] Attributs personnalisés supportés
- [ ] Liste PNJ affichée
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 7
