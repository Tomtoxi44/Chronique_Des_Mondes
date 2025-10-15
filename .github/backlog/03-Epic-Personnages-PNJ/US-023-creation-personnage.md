# US-023 - Création de Personnage Générique

## 📝 Description

**En tant que** Joueur  
**Je veux** créer un personnage générique avec des attributs de base  
**Afin de** participer aux campagnes de JDR

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page "Mes Personnages" avec bouton "➕ Créer un Personnage"
- [ ] Formulaire création avec champs :
  - [ ] Nom (requis, max 50 caractères)
  - [ ] Points de Vie (HP) actuels (requis, nombre entier positif)
  - [ ] Points de Vie (HP) maximum (requis, nombre entier positif)
  - [ ] Avatar (upload image optionnel, max 2MB)
  - [ ] Type de jeu (sélection : Générique, D&D, Pathfinder, etc.)
  - [ ] Attributs personnalisés (optionnels) :
    - [ ] Bouton "➕ Ajouter un attribut"
    - [ ] Champs dynamiques : Nom attribut + Valeur
    - [ ] Ex: Force=15, Agilité=12, Intelligence=14
- [ ] Validation :
  - [ ] Nom unique par joueur
  - [ ] HP actuel ≤ HP max
  - [ ] Nom attributs uniques dans le même personnage
- [ ] Bouton "Créer" → Sauvegarde → Redirection vers liste personnages
- [ ] Notification success : "✓ {CharacterName} créé avec succès"

### Techniques
- [ ] Endpoint : `POST /api/characters`
- [ ] Body :
```json
{
  "name": "Aragorn",
  "currentHP": 45,
  "maxHP": 60,
  "gameType": "Generic",
  "avatarUrl": "https://...",
  "customAttributes": {
    "Force": "18",
    "Agilité": "14",
    "Intelligence": "12",
    "Charisme": "16"
  }
}
```
- [ ] Response 201 : `{ "id": "guid", "name": "...", ... }`
- [ ] Response 400 : Validation errors

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CharacterService.CreateCharacter_ValidData_CreatesCharacter()`
- [ ] `CharacterService.CreateCharacter_DuplicateName_ThrowsValidationException()`
- [ ] `CharacterService.CreateCharacter_InvalidHP_ThrowsValidationException()`
- [ ] `CharacterService.CreateCharacter_CustomAttributes_SavedAsJSON()`

### Tests d'Intégration
- [ ] `CharacterEndpoint_CreateCharacter_ReturnsCreated()`
- [ ] `CharacterEndpoint_CreateCharacter_SavesInDatabase()`

### Tests E2E
- [ ] Joueur remplit formulaire → Clique "Créer" → Personnage apparaît dans liste
- [ ] Joueur entre nom existant → Erreur "Nom déjà utilisé"
- [ ] Joueur entre HP actuel > HP max → Erreur "HP invalides"

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `Character` (hérite de `BaseCharacter`) :
```csharp
public abstract class BaseCharacter
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public string? AvatarUrl { get; set; }
    public GameType GameType { get; set; }
    public string CustomAttributes { get; set; } // JSON
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Character : BaseCharacter
{
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
    public bool IsInActiveCampaign { get; set; } // Flag
    
    // Relations
    public ICollection<CampaignPlayer> CampaignPlayers { get; set; }
}
```
- [ ] Créer `CharacterService.CreateCharacterAsync(createDto, userId)` :
  - [ ] Valider nom unique pour userId
  - [ ] Valider CurrentHP ≤ MaxHP
  - [ ] Valider CustomAttributes (JSON valide)
  - [ ] Créer entité Character
  - [ ] Sauvegarder avec OwnerId = userId
- [ ] Créer endpoint `POST /api/characters` [Authorize]
- [ ] Créer `CharacterCreateDto` :
```csharp
public class CharacterCreateDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; }
    
    [Required, Range(0, 9999)]
    public int CurrentHP { get; set; }
    
    [Required, Range(1, 9999)]
    public int MaxHP { get; set; }
    
    public string? AvatarUrl { get; set; }
    
    public GameType GameType { get; set; } = GameType.Generic;
    
    public Dictionary<string, string>? CustomAttributes { get; set; }
}
```

### Frontend
- [ ] Créer page `Characters.razor` (/characters) :
```razor
<div class="characters-page">
    <div class="page-header">
        <h1>Mes Personnages</h1>
        <button @onclick="NavigateToCreate" class="btn-primary">
            ➕ Créer un Personnage
        </button>
    </div>
    
    <div class="characters-grid">
        @foreach (var character in Characters)
        {
            <CharacterCard Character="@character" OnClick="() => NavigateToDetails(character.Id)" />
        }
    </div>
</div>
```
- [ ] Créer page `CharacterCreate.razor` (/characters/create) :
```razor
<EditForm Model="@Model" OnValidSubmit="CreateCharacter">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="form-group">
        <label>Nom du personnage *</label>
        <InputText @bind-Value="Model.Name" class="form-control" />
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
        <label>Type de jeu</label>
        <InputSelect @bind-Value="Model.GameType" class="form-control">
            <option value="Generic">Générique</option>
            <option value="DnD5e">D&D 5e</option>
            <option value="Pathfinder">Pathfinder</option>
        </InputSelect>
    </div>
    
    <div class="form-group">
        <label>Avatar (optionnel)</label>
        <InputFile OnChange="HandleAvatarUpload" accept="image/*" />
    </div>
    
    <div class="custom-attributes">
        <h3>Attributs personnalisés</h3>
        @foreach (var attr in CustomAttributes)
        {
            <div class="attribute-row">
                <input @bind="attr.Key" placeholder="Nom" />
                <input @bind="attr.Value" placeholder="Valeur" />
                <button @onclick="() => RemoveAttribute(attr)" class="btn-danger">✕</button>
            </div>
        }
        <button @onclick="AddAttribute" class="btn-secondary">➕ Ajouter un attribut</button>
    </div>
    
    <div class="form-actions">
        <button type="submit" class="btn-primary">Créer</button>
        <button @onclick="NavigateBack" class="btn-secondary">Annuler</button>
    </div>
</EditForm>

@code {
    private CharacterCreateDto Model { get; set; } = new();
    private List<KeyValuePair<string, string>> CustomAttributes { get; set; } = new();

    private void AddAttribute()
    {
        CustomAttributes.Add(new KeyValuePair<string, string>("", ""));
    }

    private void RemoveAttribute(KeyValuePair<string, string> attr)
    {
        CustomAttributes.Remove(attr);
    }

    private async Task CreateCharacter()
    {
        Model.CustomAttributes = CustomAttributes.ToDictionary(x => x.Key, x => x.Value);
        
        var response = await Http.PostAsJsonAsync("/api/characters", Model);
        
        if (response.IsSuccessStatusCode)
        {
            Toast.Success($"✓ {Model.Name} créé avec succès");
            NavigationManager.NavigateTo("/characters");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Toast.Error(error);
        }
    }
}
```
- [ ] Créer composant `CharacterCard.razor` :
```razor
<div class="character-card" @onclick="OnClick">
    <img src="@Character.AvatarUrl" alt="@Character.Name" class="avatar" />
    <h3>@Character.Name</h3>
    <div class="hp-bar">
        <div class="hp-fill" style="width: @GetHPPercentage()%"></div>
        <span>@Character.CurrentHP / @Character.MaxHP HP</span>
    </div>
    <span class="game-type">@Character.GameType</span>
</div>

@code {
    [Parameter] public CharacterDto Character { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }

    private int GetHPPercentage() => (int)((double)Character.CurrentHP / Character.MaxHP * 100);
}
```

### Base de Données
- [ ] Migration : Créer table `BaseCharacters` avec discriminator
- [ ] Index sur `(OwnerId, Name)` pour validation unicité
- [ ] Index sur `OwnerId` pour requêtes liste personnages

---

## 🔗 Dépendances

### Dépend de
- [US-001](../01-Epic-Authentification/US-001-inscription.md) - Utilisateurs créés

### Bloque
- [US-024](./US-024-modification-personnage.md) - Modification
- [US-017](../02-Epic-Gestion-Parties/US-017-selection-personnage.md) - Sélection pour campagne

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (CRUD + JSON custom fields)
- Effort : 1-2 jours
- Risques : Validation JSON custom attributes

---

## 📝 Notes Techniques

### CustomAttributes JSON Example
```json
{
  "Force": "18",
  "Agilité": "14",
  "Intelligence": "12",
  "Sagesse": "10",
  "Charisme": "16",
  "Notes": "Guerrier nordique"
}
```

### TPH (Table Per Hierarchy) Configuration
```csharp
modelBuilder.Entity<BaseCharacter>()
    .HasDiscriminator<string>("CharacterType")
    .HasValue<Character>("Player")
    .HasValue<NPC>("NPC");
```

### Avatar Upload (Future Enhancement)
Pour l'instant, AvatarUrl peut être une URL externe. Plus tard, implémenter upload vers Blob Storage Azure.

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Formulaire création fonctionnel
- [ ] Validation côté client et serveur
- [ ] Attributs personnalisés sauvegardés en JSON
- [ ] Unicité nom par joueur respectée
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 6
