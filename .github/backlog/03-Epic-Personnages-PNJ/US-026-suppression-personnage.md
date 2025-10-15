# US-026 - Suppression de Personnage

## 📝 Description

**En tant que** Joueur  
**Je veux** supprimer un personnage que je n'utilise plus  
**Afin de** nettoyer ma liste de personnages

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans CharacterCard, bouton "🗑️ Supprimer"
- [ ] Clic → Modal confirmation :
  - [ ] "Êtes-vous sûr de vouloir supprimer {CharacterName} ?"
  - [ ] Warning : "Cette action est irréversible"
  - [ ] Bouton "Confirmer" (rouge) / "Annuler"
- [ ] Si personnage dans campagne active :
  - [ ] Bloquer suppression
  - [ ] Message erreur : "❌ Impossible de supprimer un personnage dans une campagne active"
- [ ] Confirmation → Suppression → Notification "✓ {CharacterName} supprimé"
- [ ] Personnage disparaît de la liste instantanément

### Techniques
- [ ] Endpoint : `DELETE /api/characters/{id}`
- [ ] Response 204 : No Content (succès)
- [ ] Response 403 : Si pas propriétaire
- [ ] Response 409 : Si dans campagne active

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CharacterService.DeleteCharacter_ValidRequest_DeletesCharacter()`
- [ ] `CharacterService.DeleteCharacter_NotOwner_ThrowsUnauthorizedException()`
- [ ] `CharacterService.DeleteCharacter_InActiveCampaign_ThrowsConflictException()`

### Tests d'Intégration
- [ ] `CharacterEndpoint_DeleteCharacter_RemovesFromDatabase()`
- [ ] `CharacterEndpoint_DeleteCharacter_InActiveCampaign_ReturnsConflict()`

### Tests E2E
- [ ] Joueur clique "Supprimer" → Confirme → Personnage supprimé
- [ ] Joueur tente supprimer personnage en campagne → Erreur

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `CharacterService.DeleteCharacterAsync(id, userId)` :
```csharp
public async Task DeleteCharacterAsync(Guid id, Guid userId)
{
    var character = await _context.Characters
        .Include(c => c.CampaignPlayers)
            .ThenInclude(cp => cp.Campaign)
        .FirstOrDefaultAsync(c => c.Id == id);
    
    if (character == null)
        throw new NotFoundException("Personnage non trouvé");
    
    if (character.OwnerId != userId)
        throw new UnauthorizedException("Vous n'êtes pas le propriétaire");
    
    // Vérifier si dans campagne active
    var isInActiveCampaign = character.CampaignPlayers
        .Any(cp => cp.Campaign.Status == CampaignStatus.Active && cp.Status == PlayerStatus.Active);
    
    if (isInActiveCampaign)
        throw new ConflictException("Impossible de supprimer un personnage dans une campagne active");
    
    _context.Characters.Remove(character);
    await _context.SaveChangesAsync();
}
```
- [ ] Créer endpoint `DELETE /api/characters/{id}` [Authorize]

### Frontend
- [ ] Ajouter bouton supprimer dans `CharacterCard.razor` :
```razor
<div class="character-card">
    <!-- Contenu existant -->
    
    <div class="card-actions">
        <button @onclick="() => OnEdit.InvokeAsync(Character.Id)" 
                class="btn-icon" 
                title="Modifier">
            ✏️
        </button>
        <button @onclick="() => OnDelete.InvokeAsync(Character.Id)" 
                class="btn-icon btn-danger" 
                title="Supprimer">
            🗑️
        </button>
    </div>
</div>

@code {
    [Parameter] public CharacterDto Character { get; set; }
    [Parameter] public EventCallback<Guid> OnEdit { get; set; }
    [Parameter] public EventCallback<Guid> OnDelete { get; set; }
}
```
- [ ] Créer composant `DeleteCharacterModal.razor` :
```razor
<Modal IsVisible="@IsVisible" OnClose="OnClose">
    <div class="modal-header">
        <h3>Supprimer un personnage</h3>
    </div>
    
    <div class="modal-body">
        <p>Êtes-vous sûr de vouloir supprimer <strong>@CharacterName</strong> ?</p>
        <p class="text-danger">⚠️ Cette action est irréversible.</p>
    </div>
    
    <div class="modal-footer">
        <button @onclick="OnConfirm" class="btn-danger">Confirmer</button>
        <button @onclick="OnClose" class="btn-secondary">Annuler</button>
    </div>
</Modal>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public string CharacterName { get; set; }
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
}
```
- [ ] Mettre à jour `Characters.razor` :
```razor
<DeleteCharacterModal IsVisible="@ShowDeleteModal"
                      CharacterName="@CharacterToDelete?.Name"
                      OnConfirm="ConfirmDelete"
                      OnClose="() => ShowDeleteModal = false" />

@code {
    private bool ShowDeleteModal { get; set; }
    private CharacterDto? CharacterToDelete { get; set; }

    private void ShowDeleteConfirmation(Guid characterId)
    {
        CharacterToDelete = Characters.FirstOrDefault(c => c.Id == characterId);
        ShowDeleteModal = true;
    }

    private async Task ConfirmDelete()
    {
        if (CharacterToDelete == null) return;
        
        try
        {
            var response = await Http.DeleteAsync($"/api/characters/{CharacterToDelete.Id}");
            
            if (response.IsSuccessStatusCode)
            {
                Toast.Success($"✓ {CharacterToDelete.Name} supprimé");
                Characters.Remove(CharacterToDelete);
                ShowDeleteModal = false;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Toast.Error(error);
            }
        }
        catch (Exception ex)
        {
            Toast.Error($"Erreur : {ex.Message}");
        }
    }
}
```

### Base de Données
- [ ] Configurer cascade delete pour relations :
```csharp
modelBuilder.Entity<CampaignPlayer>()
    .HasOne(cp => cp.Character)
    .WithMany(c => c.CampaignPlayers)
    .OnDelete(DeleteBehavior.Restrict); // Empêcher suppression si relations actives
```

---

## 🔗 Dépendances

### Dépend de
- [US-023](./US-023-creation-personnage.md) - Personnages existants

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 2

**Détails** :
- Complexité : Faible (Delete + validation)
- Effort : 0.5 jour
- Risques : Vérification campagnes actives

---

## 📝 Notes Techniques

### Soft Delete Alternative
Au lieu de hard delete, implémenter soft delete :
```csharp
public class Character
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// Filtrer globalement
modelBuilder.Entity<Character>()
    .HasQueryFilter(c => !c.IsDeleted);
```

### Cascade Delete Considerations
- CampaignPlayers : Restrict (manuel pour validation)
- AuditLogs : Cascade (si implémenté)
- SessionEvents : Preserve (anonymiser UserId)

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Modal confirmation fonctionnelle
- [ ] Validation campagne active
- [ ] Suppression bloquée si campagne active
- [ ] Personnage supprimé de la base
- [ ] Liste mise à jour instantanément
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 6
