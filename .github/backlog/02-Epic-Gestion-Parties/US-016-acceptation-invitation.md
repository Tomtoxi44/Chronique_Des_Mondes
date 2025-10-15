# US-016 - Acceptation d'Invitation

## 📝 Description

**En tant que** joueur invité  
**Je veux** accepter ou refuser une invitation à rejoindre une campagne  
**Afin de** participer aux sessions de jeu

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page "Mes Invitations" accessible depuis le menu
- [ ] Badge notification si invitations en attente
- [ ] Liste des invitations avec détails :
  - [ ] Nom de la campagne
  - [ ] Nom du MJ
  - [ ] Système de jeu
  - [ ] Nombre de joueurs actuel
  - [ ] Message personnalisé du MJ
  - [ ] Date d'expiration
- [ ] Clic sur invitation → Page détails invitation
- [ ] Boutons "Accepter" et "Refuser"
- [ ] Si acceptation :
  - [ ] Redirection vers sélection de personnage (US-017)
  - [ ] OU création personnage si aucun compatible
  - [ ] Notification MJ de l'acceptation
  - [ ] Email confirmation au joueur
- [ ] Si refus :
  - [ ] Modal confirmation
  - [ ] Raison optionnelle
  - [ ] Notification MJ du refus
  - [ ] Invitation marquée "Declined"
- [ ] Lien direct dans email fonctionne même non connecté
- [ ] Redirection login si nécessaire avec retour auto

### Techniques
- [ ] Endpoint : `GET /api/users/my-invitations` [Authorize]
- [ ] Response : Liste invitations pending
- [ ] Endpoint : `POST /api/invitations/{id}/accept` [Authorize]
- [ ] Response 200 : `{ "message": "Invitation acceptée", "nextStep": "selectCharacter" }`
- [ ] Response 400 : Invitation expirée/invalide
- [ ] Endpoint : `POST /api/invitations/{id}/decline` [Authorize]
- [ ] Body : `{ "reason": "Pas disponible actuellement" }` (optionnel)
- [ ] Response 200 : Invitation refusée

---

## 🧪 Tests

### Tests Unitaires
- [ ] `InvitationService.AcceptInvitation_WithValidInvitation_AddsPlayerToCampaign()`
- [ ] `InvitationService.AcceptInvitation_ExpiredInvitation_ThrowsException()`
- [ ] `InvitationService.AcceptInvitation_CampaignFull_ThrowsException()`
- [ ] `InvitationService.DeclineInvitation_UpdatesStatus()`
- [ ] `InvitationService.DeclineInvitation_NotifiesGameMaster()`

### Tests d'Intégration
- [ ] `InvitationEndpoint_AcceptInvitation_CreatesC ampaignPlayer()`
- [ ] `InvitationEndpoint_AcceptInvitation_NotifiesMJ()`
- [ ] `InvitationEndpoint_DeclineInvitation_UpdatesDatabase()`

### Tests E2E
- [ ] Réception invitation → Acceptation → Sélection personnage → Campagne rejointe
- [ ] Réception invitation → Refus → Invitation disparaît
- [ ] Clic lien email non connecté → Login → Acceptation → Succès

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer `InvitationService.AcceptInvitationAsync(invitationId, userId)` :
  - [ ] Vérifier invitation existe et status = Pending
  - [ ] Vérifier invitation.InvitedUserId == userId OU invitation.InvitedEmail == user.Email
  - [ ] Vérifier pas expirée (`ExpiresAt > DateTime.UtcNow`)
  - [ ] Vérifier campagne pas pleine
  - [ ] Marquer invitation Status = Accepted, RespondedAt = now
  - [ ] **NE PAS créer CampaignPlayer ici** (fait dans US-017 après sélection personnage)
  - [ ] Envoyer notification MJ (SignalR + email)
  - [ ] Envoyer email confirmation joueur
- [ ] Créer `InvitationService.DeclineInvitationAsync(invitationId, userId, reason)` :
  - [ ] Vérifications similaires
  - [ ] Marquer Status = Declined
  - [ ] Sauvegarder raison (colonne optionnelle)
  - [ ] Notifier MJ
- [ ] Créer `InvitationService.GetMyInvitationsAsync(userId)` :
  - [ ] Query invitations où InvitedUserId == userId OU InvitedEmail == user.Email
  - [ ] Filtre Status = Pending
  - [ ] Filtre ExpiresAt > now
  - [ ] Include Campaign, InvitedBy
- [ ] Créer endpoints :
  - [ ] `GET /api/users/my-invitations` [Authorize]
  - [ ] `POST /api/invitations/{id}/accept` [Authorize]
  - [ ] `POST /api/invitations/{id}/decline` [Authorize]

### Frontend
- [ ] Créer page `MyInvitations.razor` (/invitations)
- [ ] Créer composant `InvitationCard.razor` :
```razor
<div class="invitation-card">
    <div class="campaign-info">
        <h3>@Invitation.Campaign.Name</h3>
        <p>Invitation de <strong>@Invitation.InvitedBy.Username</strong></p>
        <p>Système : @Invitation.Campaign.GameType</p>
        <p>Joueurs : @Invitation.Campaign.CurrentPlayers/@Invitation.Campaign.MaxPlayers</p>
        @if (!string.IsNullOrEmpty(Invitation.Message))
        {
            <blockquote>@Invitation.Message</blockquote>
        }
        <p class="expiry">Expire dans @GetDaysRemaining() jours</p>
    </div>
    <div class="actions">
        <button class="btn-accept" @onclick="OnAccept">Accepter</button>
        <button class="btn-decline" @onclick="OnDecline">Refuser</button>
    </div>
</div>
```
- [ ] Créer page `InvitationDetail.razor` (/invitations/{id})
  - [ ] Affichage détails complets campagne
  - [ ] Boutons Accepter/Refuser
- [ ] Créer composant `DeclineReasonModal.razor` :
  - [ ] Textarea raison optionnelle
  - [ ] Boutons Confirmer/Annuler
- [ ] Implémenter `InvitationService.GetMyInvitationsAsync()`
- [ ] Implémenter `InvitationService.AcceptInvitationAsync(invitationId)`
- [ ] Implémenter `InvitationService.DeclineInvitationAsync(invitationId, reason)`
- [ ] Badge notification dans NavMenu :
```csharp
@code {
    private int _pendingInvitations;

    protected override async Task OnInitializedAsync()
    {
        _pendingInvitations = await _invitationService.GetPendingCountAsync();
    }
}
```
- [ ] Gestion deep link depuis email (avec redirection post-login)

### Email Templates
**Acceptation (pour MJ) :**
```
{PlayerName} a accepté votre invitation !

Campagne : {CampaignName}
Joueurs actuels : {CurrentPlayers}/{MaxPlayers}

{PlayerName} va maintenant choisir son personnage pour rejoindre la partie.
```

**Refus (pour MJ) :**
```
{PlayerName} a décliné votre invitation à la campagne {CampaignName}.

Raison : {DeclineReason}

Vous pouvez inviter un autre joueur pour compléter votre groupe.
```

### Base de Données
- [ ] Migration : Ajouter colonne `DeclineReason` (string, nullable) à `Invitations`

---

## 🔗 Dépendances

### Dépend de
- [US-015](./US-015-invitation-joueurs.md) - Invitation joueurs

### Bloque
- [US-017](./US-017-selection-personnage.md) - Sélection personnage

---

## 📊 Estimation

**Story Points** : 3

**Détails** :
- Complexité : Moyenne (notifications, validations)
- Effort : 1 jour
- Risques : Gestion expirations, deep links

---

## 📝 Notes Techniques

### Vérification Expiration
```csharp
if (invitation.ExpiresAt <= DateTime.UtcNow)
{
    invitation.Status = InvitationStatus.Expired;
    await _db.SaveChangesAsync();
    throw new InvitationExpiredException("Cette invitation a expiré");
}
```

### Deep Link avec Redirection
```csharp
// Lien email : https://app.com/invitations/abc123/accept
// Si non connecté → Login avec returnUrl

[AllowAnonymous]
public IActionResult AcceptInvitationLink(Guid id)
{
    if (!User.Identity.IsAuthenticated)
    {
        return RedirectToPage("/Login", new { returnUrl = $"/invitations/{id}/accept" });
    }
    
    return RedirectToPage("/Invitations/Accept", new { id });
}
```

### Notification SignalR au MJ
```csharp
await _hubContext.Clients.User(invitation.InvitedBy.ToString())
    .SendAsync("InvitationAccepted", new
    {
        PlayerName = user.Username,
        CampaignId = invitation.CampaignId,
        Message = $"{user.Username} a accepté votre invitation"
    });
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Acceptation/Refus fonctionnels
- [ ] Notifications MJ envoyées
- [ ] Deep links depuis email fonctionnent
- [ ] Badge notification affiché
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 3
