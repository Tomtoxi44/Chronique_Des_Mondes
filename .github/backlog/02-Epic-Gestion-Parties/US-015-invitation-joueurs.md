# US-015 - Invitation de Joueurs

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** inviter des joueurs à rejoindre ma campagne  
**Afin de** constituer mon groupe de jeu

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page détails campagne avec section "Joueurs"
- [ ] Bouton "+ Inviter un joueur" (visible pour MJ uniquement)
- [ ] Formulaire d'invitation :
  - [ ] Champ email du joueur (validation email)
  - [ ] OU recherche par nom d'utilisateur
  - [ ] Message personnalisé optionnel
  - [ ] Vérification : Email/User existe dans système
  - [ ] Vérification : User pas déjà dans la campagne
  - [ ] Vérification : Pas déjà invité (invitation en attente)
  - [ ] Vérification : Places disponibles (CurrentPlayers < MaxPlayers)
- [ ] Notifications envoyées :
  - [ ] **Email** : Invitation avec lien direct vers acceptation
  - [ ] **In-App** : Notification dans l'application
- [ ] Liste des invitations en attente visible par MJ
- [ ] MJ peut annuler invitation en attente
- [ ] Expiration invitation après 7 jours

### Techniques
- [ ] Endpoint : `POST /api/campaigns/{campaignId}/invitations`
- [ ] Body : `{ "email": "player@example.com", "message": "Rejoins ma campagne !" }`
- [ ] Response 201 : `{ "invitationId": "guid", "status": "Pending", "expiresAt": "..." }`
- [ ] Response 400 : Validation échouée (déjà membre, déjà invité, campagne pleine)
- [ ] Response 403 : Non créateur de la campagne
- [ ] Endpoint : `GET /api/campaigns/{campaignId}/invitations` (liste pour MJ)
- [ ] Endpoint : `DELETE /api/invitations/{id}` (annulation)

---

## 🧪 Tests

### Tests Unitaires
- [ ] `InvitationService.SendInvitation_WithValidEmail_CreatesInvitation()`
- [ ] `InvitationService.SendInvitation_AlreadyMember_ThrowsException()`
- [ ] `InvitationService.SendInvitation_AlreadyInvited_ThrowsException()`
- [ ] `InvitationService.SendInvitation_CampaignFull_ThrowsException()`
- [ ] `InvitationService.SendInvitation_NonCreator_ThrowsUnauthorizedException()`
- [ ] `InvitationService.CancelInvitation_UpdatesStatus()`

### Tests d'Intégration
- [ ] `InvitationEndpoint_SendInvitation_SendsEmailAndNotification()`
- [ ] `InvitationEndpoint_SendInvitation_CreatesInDatabase()`
- [ ] `InvitationEndpoint_CancelInvitation_MarksAsCancelled()`

### Tests E2E
- [ ] Invitation joueur → Email reçu → Notification in-app → Joueur voit invitation
- [ ] Tentative inviter joueur déjà membre → Erreur
- [ ] Annulation invitation → Invitation disparaît pour joueur

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `Invitation` :
```csharp
public class Invitation
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    public Guid InvitedBy { get; set; } // MJ
    public User InvitedByUser { get; set; }
    public Guid? InvitedUserId { get; set; } // Null si user pas encore inscrit
    public User InvitedUser { get; set; }
    public string InvitedEmail { get; set; }
    public string Message { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; } // +7 jours
    public DateTime? RespondedAt { get; set; }
}

public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Cancelled = 3,
    Expired = 4
}
```
- [ ] Créer `InvitationService.SendInvitationAsync(campaignId, email, message, invitedBy)` :
  - [ ] Vérifier utilisateur = créateur campagne
  - [ ] Rechercher user par email
  - [ ] Vérifier pas déjà membre (`CampaignPlayers`)
  - [ ] Vérifier pas déjà invitation pending
  - [ ] Vérifier places disponibles
  - [ ] Créer invitation (expires dans 7 jours)
  - [ ] Envoyer email via Azure Communication Services
  - [ ] Envoyer notification SignalR (si user connecté)
- [ ] Créer `InvitationService.CancelInvitationAsync(invitationId, userId)` :
  - [ ] Vérifier userId = InvitedBy
  - [ ] Marquer Status = Cancelled
  - [ ] Notifier joueur invité
- [ ] Job background : Marquer invitations expirées (daily)
- [ ] Créer endpoints :
  - [ ] `POST /api/campaigns/{campaignId}/invitations` [Authorize(Roles = "GameMaster")]
  - [ ] `GET /api/campaigns/{campaignId}/invitations` [Authorize]
  - [ ] `DELETE /api/invitations/{id}` [Authorize(Roles = "GameMaster")]

### Frontend
- [ ] Créer composant `InvitationManager.razor` (section de CampaignDetail)
- [ ] Créer composant `InvitePlayerModal.razor` :
  - [ ] Champ email avec validation
  - [ ] Champ message optionnel
  - [ ] Bouton "Envoyer l'invitation"
  - [ ] Gestion erreurs (déjà invité, campagne pleine, etc.)
- [ ] Créer composant `InvitationList.razor` :
  - [ ] Liste invitations pending
  - [ ] Statut (En attente, Expire dans X jours)
  - [ ] Bouton "Annuler"
- [ ] Implémenter `InvitationService.SendInvitationAsync(campaignId, invitation)`
- [ ] Implémenter `InvitationService.CancelInvitationAsync(invitationId)`
- [ ] Toast notifications succès/erreur

### Email Template
```html
<h2>Invitation à une campagne</h2>
<p>Bonjour,</p>
<p>{MJ_Name} vous invite à rejoindre la campagne <strong>{CampaignName}</strong>.</p>

<p><strong>Système de jeu :</strong> {GameType}</p>
<p><strong>Nombre de joueurs :</strong> {CurrentPlayers}/{MaxPlayers}</p>

<p><em>Message du MJ :</em><br>{PersonalMessage}</p>

<a href="{AcceptLink}" style="button">Accepter l'invitation</a>
<a href="{DeclineLink}" style="button-secondary">Refuser</a>

<p><small>Cette invitation expire le {ExpiryDate}</small></p>
```

### Base de Données
- [ ] Migration : Créer table `Invitations`
- [ ] Index sur `(CampaignId, InvitedEmail)` unique pour éviter doublons
- [ ] Index sur `(InvitedUserId, Status)` pour requêtes utilisateur
- [ ] Index sur `ExpiresAt` pour job nettoyage

---

## 🔗 Dépendances

### Dépend de
- [US-011](./US-011-creation-campagne.md) - Création campagne
- [US-001](../01-Epic-Authentification/US-001-inscription-utilisateur.md) - Inscription (pour inviter users)
- Configuration Azure Communication Services (emails)

### Bloque
- [US-016](./US-016-acceptation-invitation.md) - Acceptation invitation

---

## 📊 Estimation

**Story Points** : 5

**Détails** :
- Complexité : Moyenne-Haute (emails, notifications, validations)
- Effort : 1-2 jours
- Risques : Délai emails, gestion expirations

---

## 📝 Notes Techniques

### Validation Campagne Pleine
```csharp
var currentPlayerCount = await _db.CampaignPlayers
    .CountAsync(cp => cp.CampaignId == campaignId);

if (currentPlayerCount >= campaign.MaxPlayers)
{
    throw new CampaignFullException("La campagne est complète");
}
```

### Lien d'Invitation
```csharp
var acceptLink = $"{_config["AppUrl"]}/invitations/{invitation.Id}/accept";
var declineLink = $"{_config["AppUrl"]}/invitations/{invitation.Id}/decline";
```

### Notification SignalR
```csharp
if (invitedUser != null)
{
    await _hubContext.Clients.User(invitedUser.Id.ToString())
        .SendAsync("InvitationReceived", new
        {
            InvitationId = invitation.Id,
            CampaignName = campaign.Name,
            GameMasterName = invitedByUser.Username,
            ExpiresAt = invitation.ExpiresAt
        });
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Emails envoyés correctement
- [ ] Notifications SignalR fonctionnelles
- [ ] Validations complètes
- [ ] Job expiration configuré
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 3
