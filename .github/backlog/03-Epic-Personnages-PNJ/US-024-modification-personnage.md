# US-024 - Modification de Personnage

## 📝 Description

**En tant que** Joueur  
**Je veux** modifier les informations de mon personnage  
**Afin de** corriger des erreurs ou faire évoluer mon personnage

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans CharacterCard, bouton "✏️ Modifier"
- [ ] Redirection vers formulaire édition pré-rempli
- [ ] Tous les champs modifiables (sauf nom si utilisé dans campagne active)
- [ ] HP actuels : Validation ≤ HP max
- [ ] Attributs personnalisés :
  - [ ] Modifier valeurs existantes
  - [ ] Ajouter nouveaux attributs
  - [ ] Supprimer attributs existants
- [ ] Bouton "Sauvegarder" → Mise à jour → Notification success
- [ ] Bouton "Annuler" → Retour sans sauvegarder
- [ ] Si personnage dans campagne active :
  - [ ] Warning : "⚠️ Ce personnage est dans une campagne active"
  - [ ] Empêcher changement de nom
  - [ ] Autres champs modifiables

### Techniques
- [ ] Endpoint : `PUT /api/characters/{id}`
- [ ] Body : `CharacterUpdateDto` (mêmes champs que Create)
- [ ] Response 200 : `CharacterDto` mis à jour
- [ ] Response 403 : Si pas propriétaire
- [ ] Response 409 : Si changement nom et campagne active

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CharacterService.UpdateCharacter_ValidData_UpdatesCharacter()`
- [ ] `CharacterService.UpdateCharacter_NotOwner_ThrowsUnauthorizedException()`
- [ ] `CharacterService.UpdateCharacter_NameChangeInActiveCampaign_ThrowsConflictException()`
- [ ] `CharacterService.UpdateCharacter_UpdatesCustomAttributes()`

### Tests d'Intégration
- [ ] `CharacterEndpoint_UpdateCharacter_ReturnsUpdated()`
- [ ] `CharacterEndpoint_UpdateCharacter_SavesChanges()`

### Tests E2E
- [ ] Joueur modifie HP → Sauvegarde → Changements visibles
- [ ] Joueur modifie personnage dans campagne active → Nom bloqué
- [ ] Joueur ajoute attribut → Sauvegarde → Attribut présent

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CharacterService.UpdateCharacterAsync(id, updateDto, userId)` :
```csharp
public async Task<CharacterDto> UpdateCharacterAsync(Guid id, CharacterUpdateDto dto, Guid userId)
{
    var character = await _context.Characters
        .Include(c => c.CampaignPlayers)
        .FirstOrDefaultAsync(c => c.Id == id);
    
    if (character == null)
        throw new NotFoundException("Personnage non trouvé");
    
    if (character.OwnerId != userId)
        throw new UnauthorizedException("Vous n'êtes pas le propriétaire");
    
    // Vérifier si changement de nom et campagne active
    if (character.Name != dto.Name && character.IsInActiveCampaign)
        throw new ConflictException("Impossible de renommer un personnage dans une campagne active");
    
    // Mise à jour
    character.Name = dto.Name;
    character.CurrentHP = dto.CurrentHP;
    character.MaxHP = dto.MaxHP;
    character.AvatarUrl = dto.AvatarUrl;
    character.GameType = dto.GameType;
    character.CustomAttributes = JsonSerializer.Serialize(dto.CustomAttributes);
    character.UpdatedAt = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
    return character.ToDto();
}
```
- [ ] Créer endpoint `PUT /api/characters/{id}` [Authorize]

### Frontend
- [ ] Créer page `CharacterEdit.razor` (/characters/{id}/edit) :
```razor
@page "/characters/{id:guid}/edit"
@inject HttpClient Http
@inject NavigationManager Nav
@inject IToastService Toast

<div class="character-edit">
    <h1>Modifier @Character?.Name</h1>
    
    @if (Character?.IsInActiveCampaign == true)
    {
        <div class="alert alert-warning">
            ⚠️ Ce personnage est dans une campagne active. Le nom ne peut pas être modifié.
        </div>
    }
    
    <EditForm Model="@Model" OnValidSubmit="UpdateCharacter">
        <DataAnnotationsValidator />
        <ValidationSummary />
        
        <div class="form-group">
            <label>Nom du personnage *</label>
            <InputText @bind-Value="Model.Name" 
                       class="form-control" 
                       disabled="@(Character?.IsInActiveCampaign == true)" />
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
        
        <!-- Même structure que CharacterCreate pour attributs personnalisés -->
        
        <div class="form-actions">
            <button type="submit" class="btn-primary">Sauvegarder</button>
            <button @onclick="Cancel" class="btn-secondary">Annuler</button>
        </div>
    </EditForm>
</div>

@code {
    [Parameter] public Guid Id { get; set; }
    
    private CharacterDto? Character { get; set; }
    private CharacterUpdateDto Model { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Character = await Http.GetFromJsonAsync<CharacterDto>($"/api/characters/{Id}");
        
        if (Character != null)
        {
            Model = new CharacterUpdateDto
            {
                Name = Character.Name,
                CurrentHP = Character.CurrentHP,
                MaxHP = Character.MaxHP,
                AvatarUrl = Character.AvatarUrl,
                GameType = Character.GameType,
                CustomAttributes = JsonSerializer.Deserialize<Dictionary<string, string>>(Character.CustomAttributes)
            };
        }
    }

    private async Task UpdateCharacter()
    {
        var response = await Http.PutAsJsonAsync($"/api/characters/{Id}", Model);
        
        if (response.IsSuccessStatusCode)
        {
            Toast.Success("✓ Personnage mis à jour");
            Nav.NavigateTo("/characters");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Toast.Error(error);
        }
    }

    private void Cancel() => Nav.NavigateTo("/characters");
}
```

### Base de Données
- [ ] Aucune modification nécessaire

---

## 🔗 Dépendances

### Dépend de
- [US-023](./US-023-creation-personnage.md) - Personnages existants

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Faible (CRUD update)
- Effort : 0.5-1 jour
- Risques : Validation campagne active

---

## 📝 Notes Techniques

### IsInActiveCampaign Flag
```csharp
// Calculé dynamiquement ou mis à jour par trigger
public bool IsInActiveCampaign => CampaignPlayers
    .Any(cp => cp.Campaign.Status == CampaignStatus.Active && cp.Status == PlayerStatus.Active);
```

### Audit Log (Future)
Pour traçabilité, logger les modifications :
```csharp
_auditService.LogAsync(new AuditLog
{
    EntityType = "Character",
    EntityId = character.Id,
    Action = "Update",
    Changes = JsonSerializer.Serialize(new { Old = oldValues, New = newValues }),
    UserId = userId
});
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Formulaire édition fonctionnel
- [ ] Validation campagne active
- [ ] Restriction nom si campagne active
- [ ] Attributs personnalisés modifiables
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 6
