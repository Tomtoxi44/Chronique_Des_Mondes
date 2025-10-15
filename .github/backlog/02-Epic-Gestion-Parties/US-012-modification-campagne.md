# US-012 - Modification de Campagne

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** modifier les informations de ma campagne existante  
**Afin de** corriger des erreurs, mettre à jour la description ou changer les paramètres

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page de détails de campagne accessible via "Mes Campagnes" → Clic sur campagne
- [ ] Bouton "Modifier" visible uniquement pour le créateur (MJ)
- [ ] Formulaire pré-rempli avec données actuelles
- [ ] Champs modifiables :
  - [ ] Nom de la campagne
  - [ ] Description
  - [ ] Image de couverture (upload nouvelle ou garder existante)
  - [ ] Visibilité (Publique/Privée)
  - [ ] Nombre de joueurs max
- [ ] Champs non modifiables :
  - [ ] Système de jeu (verrouillé après création)
  - [ ] Créateur
  - [ ] Date de création
- [ ] Validation côté client et serveur
- [ ] Confirmation avant sauvegarde
- [ ] Message de succès après modification
- [ ] Mise à jour temps réel pour joueurs déjà dans la campagne

### Techniques
- [ ] Endpoint : `PUT /api/campaigns/{id}`
- [ ] Body : `{ "name": "Nouveau nom", "description": "...", "visibility": "Public", "maxPlayers": 8, "coverImage": "..." }`
- [ ] Response 200 : Campagne mise à jour
- [ ] Response 400 : Validation échouée
- [ ] Response 403 : Utilisateur n'est pas le créateur
- [ ] Response 404 : Campagne non trouvée

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CampaignService.UpdateCampaign_WithValidData_UpdatesCampaign()`
- [ ] `CampaignService.UpdateCampaign_WithInvalidName_ThrowsValidationException()`
- [ ] `CampaignService.UpdateCampaign_NonCreator_ThrowsUnauthorizedException()`
- [ ] `CampaignService.UpdateCampaign_CannotChangeGameType_ThrowsException()`

### Tests d'Intégration
- [ ] `CampaignEndpoint_UpdateCampaign_SavesChanges()`
- [ ] `CampaignEndpoint_UpdateCampaign_NonCreator_Returns403()`
- [ ] `CampaignEndpoint_UpdateCampaign_UpdatesImage()`

### Tests E2E
- [ ] Modification complète → Vérification affichage → Données mises à jour
- [ ] Tentative modification par non-créateur → Erreur 403
- [ ] Upload nouvelle image → Ancienne image supprimée

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `UpdateCampaignRequest` DTO (même structure que création sauf GameType)
- [ ] Implémenter `CampaignService.UpdateCampaignAsync(id, request, userId)` :
  - [ ] Vérifier campagne existe
  - [ ] Vérifier userId == Campaign.CreatedBy (autorisation)
  - [ ] Validation données
  - [ ] Upload nouvelle image si fournie
  - [ ] Supprimer ancienne image si remplacée
  - [ ] Mettre à jour entité Campaign
  - [ ] Sauvegarder en DB
  - [ ] Notifier joueurs via SignalR (optionnel)
- [ ] Créer endpoint `PUT /api/campaigns/{id}` [Authorize(Roles = "GameMaster")]
- [ ] Ajouter colonne `UpdatedAt` à `Campaigns` (si pas déjà présent)
- [ ] Logger modifications (audit trail)

### Frontend
- [ ] Créer page `EditCampaign.razor` (/campaigns/{id}/edit)
- [ ] OU composant `EditCampaignModal.razor` (modale)
- [ ] Pré-remplir formulaire avec `CampaignService.GetCampaignByIdAsync(id)`
- [ ] Réutiliser `CampaignForm.razor` en mode "edit"
- [ ] Implémenter `CampaignService.UpdateCampaignAsync(id, campaign)`
- [ ] Gestion upload image :
  - [ ] Option "Garder image actuelle"
  - [ ] Option "Upload nouvelle image"
  - [ ] Aperçu avant upload
- [ ] Afficher messages d'erreur
- [ ] Redirection vers détails après succès
- [ ] Bouton "Modifier" uniquement si currentUserId == campaign.CreatedBy

### Base de Données
- [ ] Migration : Ajouter `UpdatedAt` (DateTime) si absent
- [ ] Index sur `UpdatedAt` pour tri récents

---

## 🔗 Dépendances

### Dépend de
- [US-011](./US-011-creation-campagne.md) - Création campagne

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Faible (CRUD standard)
- Effort : 0.5-1 jour
- Risques : Gestion images, autorisation

---

## 📝 Notes Techniques

### Autorisation
```csharp
public async Task<Campaign> UpdateCampaignAsync(Guid id, UpdateCampaignRequest request, Guid userId)
{
    var campaign = await _db.Campaigns.FindAsync(id);
    
    if (campaign == null)
        throw new NotFoundException("Campagne introuvable");
    
    if (campaign.CreatedBy != userId)
        throw new UnauthorizedException("Seul le créateur peut modifier cette campagne");
    
    // Mise à jour...
}
```

### Gestion Image
```csharp
// Si nouvelle image fournie
if (!string.IsNullOrEmpty(request.CoverImage))
{
    // Supprimer ancienne image
    if (!string.IsNullOrEmpty(campaign.CoverImageUrl))
    {
        File.Delete(Path.Combine(_webRoot, campaign.CoverImageUrl));
    }
    
    // Upload nouvelle
    campaign.CoverImageUrl = await _imageService.UploadAsync(request.CoverImage);
}
```

### Notification SignalR (Optionnel)
```csharp
// Notifier tous les joueurs de la campagne
var playerIds = await _db.CampaignPlayers
    .Where(cp => cp.CampaignId == id)
    .Select(cp => cp.UserId)
    .ToListAsync();

foreach (var playerId in playerIds)
{
    await _hubContext.Clients.User(playerId.ToString())
        .SendAsync("CampaignUpdated", campaign);
}
```

### Validation
- Nom : 3-100 caractères (inchangé)
- Description : max 5000 caractères
- MaxPlayers : ne peut pas être inférieur au nombre actuel de joueurs

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent (couverture > 80%)
- [ ] Tests d'intégration passent
- [ ] Tests E2E passent
- [ ] Autorisation fonctionnelle
- [ ] Upload image gère remplacement
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 2
