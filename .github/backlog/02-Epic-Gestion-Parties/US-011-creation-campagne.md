# US-011 - Création de Campagne

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** créer une nouvelle campagne avec nom, description et système de jeu  
**Afin de** organiser mes sessions de jeu de rôle et inviter des joueurs

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Formulaire de création accessible depuis "Mes Campagnes" → "Nouvelle Campagne"
- [ ] Champs obligatoires :
  - [ ] Nom de la campagne (3-100 caractères)
  - [ ] Système de jeu (liste déroulante : Générique, D&D 5e, Pathfinder, etc.)
- [ ] Champs optionnels :
  - [ ] Description (max 5000 caractères)
  - [ ] Image de couverture (jpg/png, max 5MB)
  - [ ] Visibilité (Publique/Privée, défaut: Privée)
  - [ ] Nombre de joueurs max (1-20, défaut: 6)
- [ ] Validation côté client et serveur
- [ ] Aperçu de la campagne avant création
- [ ] Message de confirmation après création
- [ ] Redirection vers la page de détails de la campagne

### Techniques
- [ ] Endpoint : `POST /api/campaigns`
- [ ] Body : 
```json
{
  "name": "La Quête du Dragon",
  "description": "Une aventure épique...",
  "gameType": "DnD5e",
  "visibility": "Private",
  "maxPlayers": 6,
  "coverImage": "base64_or_url"
}
```
- [ ] Response 201 : `{ "id": "guid", "name": "...", "createdAt": "...", "createdBy": "userId" }`
- [ ] Response 400 : Validation échouée
- [ ] Response 401 : Non authentifié

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CampaignService.CreateCampaign_WithValidData_ReturnsCampaign()`
- [ ] `CampaignService.CreateCampaign_WithInvalidName_ThrowsValidationException()`
- [ ] `CampaignService.CreateCampaign_SetsCreatedByCurrentUser()`
- [ ] `CampaignValidator.Validate_WithTooLongName_ReturnsFalse()`

### Tests d'Intégration
- [ ] `CampaignEndpoint_CreateCampaign_SavesInDatabase()`
- [ ] `CampaignEndpoint_CreateCampaign_UploadsImage()`
- [ ] `CampaignEndpoint_Unauthorized_Returns401()`

### Tests E2E
- [ ] Création complète : Formulaire → Remplissage → Upload image → Création → Redirection
- [ ] Validation : Nom trop court → Message d'erreur
- [ ] Liste "Mes Campagnes" affiche la nouvelle campagne

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `Campaign` :
  - [ ] Id (PK, Guid)
  - [ ] Name (string, required)
  - [ ] Description (string, nullable)
  - [ ] GameType (enum: Generic, DnD5e, Pathfinder, etc.)
  - [ ] Visibility (enum: Public, Private)
  - [ ] MaxPlayers (int)
  - [ ] CoverImageUrl (string, nullable)
  - [ ] CreatedBy (FK → Users)
  - [ ] CreatedAt, UpdatedAt
- [ ] Créer `CreateCampaignRequest` DTO
- [ ] Créer `CampaignResponse` DTO
- [ ] Implémenter `CampaignService.CreateCampaignAsync()`
  - [ ] Validation données
  - [ ] Upload image si fournie
  - [ ] Création entité Campaign
  - [ ] Sauvegarde en DB
- [ ] Créer endpoint `POST /api/campaigns`
- [ ] Autorisation : JWT requis, rôle MJ

### Frontend
- [ ] Créer page `CreateCampaign.razor` (/campaigns/create)
- [ ] Créer composant `CampaignForm.razor`
  - [ ] Champs de formulaire
  - [ ] Validation DataAnnotations
  - [ ] Upload image avec aperçu
  - [ ] Sélection système de jeu
- [ ] Implémenter `CampaignService.CreateCampaignAsync()`
- [ ] Afficher messages d'erreur
- [ ] Redirection après succès

### Base de Données
- [ ] Migration : Créer table `Campaigns`
- [ ] Index sur `CreatedBy` pour performance
- [ ] Relation `Campaigns.CreatedBy` → `Users.Id`

---

## 🔗 Dépendances

### Dépend de
- [US-001](../01-Epic-Authentification/US-001-inscription-utilisateur.md) - Inscription
- [US-002](../01-Epic-Authentification/US-002-connexion-utilisateur.md) - Connexion
- [US-006](../01-Epic-Authentification/US-006-gestion-roles.md) - Rôles (MJ)

### Bloque
- [US-012](./US-012-modification-campagne.md) - Modification campagne
- [US-014](./US-014-creation-chapitres.md) - Chapitres
- [US-015](./US-015-invitation-joueurs.md) - Invitations

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne (CRUD, upload image, validation)
- Effort : 1-2 jours
- Risques : Validation du système de jeu, gestion images

---

## 📝 Notes Techniques

### Enum GameType
```csharp
public enum GameType
{
    Generic = 0,
    DnD5e = 1,
    Pathfinder = 2,
    CallOfCthulhu = 3,
    Warhammer = 4,
    Custom = 99
}
```

### Stockage des Images
```
/wwwroot/uploads/campaigns/
  ├── {campaignId}_cover.jpg
  └── default-cover.png
```

### Validation
- Nom : 3-100 caractères
- Description : max 5000 caractères
- MaxPlayers : 1-20
- Image : jpg/png, max 5MB

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent (couverture > 80%)
- [ ] Tests d'intégration passent
- [ ] Tests E2E passent
- [ ] Upload d'image fonctionnel
- [ ] Documentation API mise à jour
- [ ] Déployé en staging
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 2
