# US-025 - Liste des Personnages

## 📝 Description

**En tant que** Joueur  
**Je veux** consulter la liste de mes personnages  
**Afin de** les gérer facilement

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page "Mes Personnages" (/characters)
- [ ] Affichage grille de cartes personnages avec :
  - [ ] Avatar
  - [ ] Nom
  - [ ] Barre HP (visuelle avec %)
  - [ ] Type de jeu (badge)
  - [ ] Statut campagne (badge si actif)
- [ ] Filtre par type de jeu (dropdown : Tous, Générique, D&D, etc.)
- [ ] Recherche par nom (input texte)
- [ ] Tri par : Nom (A-Z), Date création (récent/ancien), HP (croissant/décroissant)
- [ ] Clic carte → Redirection vers détails personnage
- [ ] Badge "🎮 En campagne" si personnage actif dans campagne
- [ ] Message si aucun personnage : "Aucun personnage. Créez-en un !"

### Techniques
- [ ] Endpoint : `GET /api/characters`
- [ ] Query params : `?gameType=DnD5e&search=Aragorn&sortBy=name&sortOrder=asc`
- [ ] Response 200 :
```json
[
  {
    "id": "guid",
    "name": "Aragorn",
    "currentHP": 45,
    "maxHP": 60,
    "avatarUrl": "...",
    "gameType": "Generic",
    "isInActiveCampaign": true,
    "createdAt": "2025-01-15T10:00:00Z"
  }
]
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CharacterService.GetUserCharacters_ReturnsOnlyOwnCharacters()`
- [ ] `CharacterService.GetUserCharacters_FiltersByGameType()`
- [ ] `CharacterService.GetUserCharacters_SearchesByName()`
- [ ] `CharacterService.GetUserCharacters_SortsCorrectly()`

### Tests d'Intégration
- [ ] `CharacterEndpoint_GetCharacters_ReturnsUserCharacters()`
- [ ] `CharacterEndpoint_GetCharacters_AppliesFilters()`

### Tests E2E
- [ ] Joueur accède /characters → Voit ses personnages
- [ ] Joueur filtre par D&D → Voit seulement D&D
- [ ] Joueur recherche "Ara" → Voit "Aragorn"

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CharacterService.GetUserCharactersAsync(userId, filters, sort)` :
```csharp
public async Task<List<CharacterDto>> GetUserCharactersAsync(
    Guid userId,
    GameType? gameType = null,
    string? search = null,
    string sortBy = "name",
    string sortOrder = "asc")
{
    var query = _context.Characters
        .Where(c => c.OwnerId == userId);
    
    // Filtres
    if (gameType.HasValue)
        query = query.Where(c => c.GameType == gameType.Value);
    
    if (!string.IsNullOrEmpty(search))
        query = query.Where(c => c.Name.Contains(search));
    
    // Tri
    query = sortBy.ToLower() switch
    {
        "name" => sortOrder == "asc" 
            ? query.OrderBy(c => c.Name) 
            : query.OrderByDescending(c => c.Name),
        "createdat" => sortOrder == "asc" 
            ? query.OrderBy(c => c.CreatedAt) 
            : query.OrderByDescending(c => c.CreatedAt),
        "hp" => sortOrder == "asc" 
            ? query.OrderBy(c => c.CurrentHP) 
            : query.OrderByDescending(c => c.CurrentHP),
        _ => query.OrderBy(c => c.Name)
    };
    
    return await query
        .Select(c => c.ToDto())
        .ToListAsync();
}
```
- [ ] Créer endpoint `GET /api/characters` [Authorize]

### Frontend
- [ ] Mettre à jour page `Characters.razor` :
```razor
@page "/characters"
@inject HttpClient Http
@inject NavigationManager Nav

<div class="characters-page">
    <div class="page-header">
        <h1>Mes Personnages</h1>
        <button @onclick="() => Nav.NavigateTo('/characters/create')" class="btn-primary">
            ➕ Créer un Personnage
        </button>
    </div>
    
    <div class="filters">
        <div class="filter-group">
            <label>Type de jeu</label>
            <select @bind="GameTypeFilter" @bind:after="LoadCharacters">
                <option value="">Tous</option>
                <option value="Generic">Générique</option>
                <option value="DnD5e">D&D 5e</option>
                <option value="Pathfinder">Pathfinder</option>
            </select>
        </div>
        
        <div class="filter-group">
            <label>Recherche</label>
            <input @bind="SearchTerm" 
                   @bind:event="oninput" 
                   @bind:after="LoadCharacters" 
                   placeholder="Nom du personnage..." />
        </div>
        
        <div class="filter-group">
            <label>Tri</label>
            <select @bind="SortBy" @bind:after="LoadCharacters">
                <option value="name">Nom</option>
                <option value="createdat">Date création</option>
                <option value="hp">Points de vie</option>
            </select>
            <button @onclick="ToggleSortOrder" class="btn-icon">
                @(SortOrder == "asc" ? "↑" : "↓")
            </button>
        </div>
    </div>
    
    @if (Characters.Any())
    {
        <div class="characters-grid">
            @foreach (var character in Characters)
            {
                <CharacterCard Character="@character" 
                              OnClick="() => Nav.NavigateTo($"/characters/{character.Id}")" />
            }
        </div>
    }
    else
    {
        <div class="empty-state">
            <p>Aucun personnage. Créez-en un !</p>
            <button @onclick="() => Nav.NavigateTo('/characters/create')" class="btn-primary">
                Créer mon premier personnage
            </button>
        </div>
    }
</div>

@code {
    private List<CharacterDto> Characters { get; set; } = new();
    private string GameTypeFilter { get; set; } = "";
    private string SearchTerm { get; set; } = "";
    private string SortBy { get; set; } = "name";
    private string SortOrder { get; set; } = "asc";

    protected override async Task OnInitializedAsync()
    {
        await LoadCharacters();
    }

    private async Task LoadCharacters()
    {
        var url = $"/api/characters?sortBy={SortBy}&sortOrder={SortOrder}";
        
        if (!string.IsNullOrEmpty(GameTypeFilter))
            url += $"&gameType={GameTypeFilter}";
        
        if (!string.IsNullOrEmpty(SearchTerm))
            url += $"&search={Uri.EscapeDataString(SearchTerm)}";
        
        Characters = await Http.GetFromJsonAsync<List<CharacterDto>>(url) ?? new();
    }

    private async Task ToggleSortOrder()
    {
        SortOrder = SortOrder == "asc" ? "desc" : "asc";
        await LoadCharacters();
    }
}
```
- [ ] Mettre à jour composant `CharacterCard.razor` :
```razor
<div class="character-card" @onclick="OnClick">
    <img src="@(Character.AvatarUrl ?? "/images/default-avatar.png")" 
         alt="@Character.Name" 
         class="avatar" />
    
    <div class="card-content">
        <h3>@Character.Name</h3>
        
        @if (Character.IsInActiveCampaign)
        {
            <span class="badge badge-success">🎮 En campagne</span>
        }
        
        <div class="hp-bar">
            <div class="hp-fill" style="width: @GetHPPercentage()%"></div>
            <span>@Character.CurrentHP / @Character.MaxHP HP</span>
        </div>
        
        <span class="game-type">@GetGameTypeLabel(Character.GameType)</span>
    </div>
</div>

@code {
    [Parameter] public CharacterDto Character { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    private int GetHPPercentage()
    {
        if (Character.MaxHP == 0) return 0;
        return (int)((double)Character.CurrentHP / Character.MaxHP * 100);
    }

    private string GetGameTypeLabel(string gameType) => gameType switch
    {
        "Generic" => "Générique",
        "DnD5e" => "D&D 5e",
        "Pathfinder" => "Pathfinder",
        _ => gameType
    };
}
```

### Base de Données
- [ ] Index existant sur `OwnerId` suffit

---

## 🔗 Dépendances

### Dépend de
- [US-023](./US-023-creation-personnage.md) - Personnages créés

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 2

**Détails** :
- Complexité : Faible (CRUD list + filtres simples)
- Effort : 0.5 jour
- Risques : Aucun

---

## 📝 Notes Techniques

### Performance : Pagination Future
Pour l'instant, charger tous personnages (peu probable > 50 par joueur). Si besoin, ajouter pagination :
```csharp
?page=1&pageSize=20
```

### CSS HP Bar
```css
.hp-bar {
    position: relative;
    height: 24px;
    background: #e0e0e0;
    border-radius: 12px;
    overflow: hidden;
}

.hp-fill {
    height: 100%;
    background: linear-gradient(90deg, #4caf50, #8bc34a);
    transition: width 0.3s ease;
}

.hp-bar span {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    font-weight: bold;
    color: #333;
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Liste personnages affichée
- [ ] Filtres fonctionnels (type, recherche)
- [ ] Tri fonctionnel (nom, date, HP)
- [ ] Badge "En campagne" visible
- [ ] Empty state si aucun personnage
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 6
