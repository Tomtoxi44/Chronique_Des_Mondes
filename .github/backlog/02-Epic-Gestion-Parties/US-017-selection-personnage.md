# US-017 - Sélection Personnage pour Campagne

## 📝 Description

**En tant que** joueur ayant accepté une invitation  
**Je veux** sélectionner le personnage avec lequel je vais jouer dans cette campagne  
**Afin de** finaliser mon inscription et pouvoir participer aux sessions

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Après acceptation invitation → Redirection automatique vers sélection personnage
- [ ] Page affiche liste de mes personnages compatibles avec le système de jeu
- [ ] Chaque carte personnage affiche : Nom, Niveau, HP, Classe (si D&D)
- [ ] Filtre automatique par GameType (personnages D&D pour campagne D&D, etc.)
- [ ] Si aucun personnage compatible → Message + bouton "Créer un personnage"
- [ ] Sélection personnage → Confirmation
- [ ] Création de la relation `CampaignPlayer` (joueur rejoint officiellement)
- [ ] Notification MJ : "{PlayerName} a rejoint avec {CharacterName}"
- [ ] Redirection vers page détails campagne
- [ ] Personnage ne peut être utilisé que dans une campagne active à la fois

### Techniques
- [ ] Endpoint : `POST /api/campaigns/{campaignId}/join`
- [ ] Body : `{ "characterId": "guid" }`
- [ ] Response 200 : `{ "message": "Vous avez rejoint la campagne", "campaignPlayer": {...} }`
- [ ] Response 400 : Personnage incompatible, déjà utilisé ailleurs, etc.
- [ ] Response 403 : Pas d'invitation acceptée

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CampaignService.JoinCampaign_WithValidCharacter_CreatesCampaignPlayer()`
- [ ] `CampaignService.JoinCampaign_IncompatibleGameType_ThrowsException()`
- [ ] `CampaignService.JoinCampaign_CharacterAlreadyInActiveCampaign_ThrowsException()`
- [ ] `CampaignService.JoinCampaign_NoAcceptedInvitation_ThrowsUnauthorizedException()`

### Tests d'Intégration
- [ ] `CampaignEndpoint_JoinCampaign_CreatesRelationInDatabase()`
- [ ] `CampaignEndpoint_JoinCampaign_NotifiesGameMaster()`

### Tests E2E
- [ ] Acceptation invitation → Sélection personnage → Campagne rejointe → Apparaît dans "Mes Parties"
- [ ] Aucun personnage compatible → Création nouveau → Sélection → Rejoindre

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `CampaignPlayer` (table junction) :
```csharp
public class CampaignPlayer
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid CharacterId { get; set; }
    public Character Character { get; set; }
    public DateTime JoinedAt { get; set; }
    public PlayerStatus Status { get; set; } // Active, Inactive, Left
}

public enum PlayerStatus
{
    Active = 0,
    Inactive = 1,
    Left = 2
}
```
- [ ] Créer `CampaignService.JoinCampaignAsync(campaignId, userId, characterId)` :
  - [ ] Vérifier invitation acceptée existe
  - [ ] Vérifier personnage appartient à userId
  - [ ] Vérifier compatibilité GameType (Character.GameType == Campaign.GameType OU Generic)
  - [ ] Vérifier personnage pas déjà dans campagne active
  - [ ] Vérifier campagne pas pleine
  - [ ] Créer CampaignPlayer
  - [ ] Marquer personnage comme "En campagne"
  - [ ] Notifier MJ (SignalR + email)
- [ ] Créer endpoint `POST /api/campaigns/{campaignId}/join` [Authorize]

### Frontend
- [ ] Créer page `SelectCharacter.razor` (/campaigns/{id}/select-character)
- [ ] Créer composant `CharacterSelectionCard.razor` :
```razor
<div class="character-card selectable" @onclick="OnSelect">
    <img src="@Character.AvatarUrl" />
    <h3>@Character.Name</h3>
    <p>Niveau @Character.Level</p>
    <p>HP: @Character.CurrentHP/@Character.MaxHP</p>
    @if (Character.GameType == GameType.DnD5e)
    {
        <p>@Character.ClassName - @Character.RaceName</p>
    }
    <span class="badge-compatible">✓ Compatible</span>
</div>
```
- [ ] Filtrage personnages par GameType côté client
- [ ] Modal confirmation sélection
- [ ] Bouton "Créer un nouveau personnage" si liste vide
- [ ] Implémenter `CampaignService.JoinCampaignAsync(campaignId, characterId)`
- [ ] Toast "Vous avez rejoint la campagne !"
- [ ] Redirection vers `/campaigns/{id}`

### Base de Données
- [ ] Migration : Créer table `CampaignPlayers`
  - [ ] PK composite (CampaignId, UserId)
  - [ ] FK CharacterId → Characters
- [ ] Migration : Ajouter colonne `IsInActiveCampaign` (bool) à `Characters`
- [ ] Index sur `(UserId, Status)` pour requêtes joueur

---

## 🔗 Dépendances

### Dépend de
- [US-016](./US-016-acceptation-invitation.md) - Acceptation invitation
- [US-023](../03-Epic-Personnages-PNJ/US-023-creation-personnage.md) - Création personnage

### Bloque
- [US-018](./US-018-lancement-session.md) - Lancement session

---

## 📊 Estimation

**Story Points** : 3

---

## 📝 Notes Techniques

### Validation Compatibilité
```csharp
if (character.GameType != GameType.Generic && 
    character.GameType != campaign.GameType)
{
    throw new IncompatibleGameTypeException(
        $"Ce personnage ({character.GameType}) n'est pas compatible avec cette campagne ({campaign.GameType})"
    );
}
```

### Vérification Personnage Disponible
```csharp
var isInActiveCampaign = await _db.CampaignPlayers
    .AnyAsync(cp => cp.CharacterId == characterId && 
                    cp.Status == PlayerStatus.Active);

if (isInActiveCampaign)
{
    throw new CharacterBusyException("Ce personnage est déjà utilisé dans une campagne active");
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Validation compatibilité fonctionnelle
- [ ] Notification MJ envoyée
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 3
