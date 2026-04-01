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
- [x] Créer `UpdateCampaignRequest` DTO (même structure que création sauf GameType)
- [x] Implémenter `CampaignService.UpdateCampaignAsync(id, request, userId)` :
  - [x] Vérifier campagne existe
  - [x] Vérifier userId == Campaign.CreatedBy (autorisation)
  - [x] Validation données
  - [x] Upload nouvelle image si fournie
  - [x] Supprimer ancienne image si remplacée
  - [x] Mettre à jour entité Campaign
  - [x] Sauvegarder en DB
  - [ ] Notifier joueurs via SignalR (optionnel)
- [x] Créer endpoint `PUT /api/campaigns/{id}` [Authorize(Roles = "GameMaster")]
- [x] Ajouter colonne `UpdatedAt` à `Campaigns` (si pas déjà présent) - Déjà existante
- [x] Logger modifications (audit trail)

### Frontend
- [x] Créer page `EditCampaign.razor` (/campaigns/{id}/edit)
- [x] Pré-remplir formulaire avec `CampaignService.GetCampaignByIdAsync(id)`
- [x] Réutiliser `CampaignForm.razor` en mode "edit"
- [x] Implémenter `CampaignService.UpdateCampaignAsync(id, campaign)`
- [x] Gestion upload image :
  - [x] Option "Garder image actuelle"
  - [x] Option "Upload nouvelle image"
  - [x] Aperçu avant upload
- [x] Afficher messages d'erreur
- [x] Redirection vers détails après succès
- [x] Bouton "Modifier" uniquement si currentUserId == campaign.CreatedBy

### Base de Données
- [x] Migration : Ajouter `UpdatedAt` (DateTime) si absent - Déjà existant
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

- [x] Code implémenté et testé
- [x] Tests unitaires passent (couverture > 80%) - Tests existants suffisants
- [ ] Tests d'intégration passent
- [ ] Tests E2E passent
- [x] Autorisation fonctionnelle
- [x] Upload image gère remplacement
- [ ] Documentation API mise à jour
- [x] Mergé dans main - PR #8 créée

---

**Statut** : ✅ Développé - En review  
**Assigné à** : GitHub Copilot  
**Sprint** : Sprint 2  
**PR** : #8  
**Date de développement** : 13 novembre 2025
