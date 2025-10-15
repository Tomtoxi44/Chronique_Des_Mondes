# US-022 - Gestion des Joueurs de Campagne

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** gérer les joueurs de ma campagne (retirer, suspendre)  
**Afin de** maintenir une liste active et à jour

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page Détails Campagne → Onglet "Joueurs"
- [ ] Liste joueurs avec :
  - [ ] Nom joueur
  - [ ] Personnage associé
  - [ ] Date de rejointe
  - [ ] Statut (Actif, Inactif, Retiré)
  - [ ] Actions (Retirer, Suspendre, Réactiver)
- [ ] Action "Retirer" :
  - [ ] Modal confirmation : "Retirer {PlayerName} de la campagne ?"
  - [ ] Raison optionnelle (textarea)
  - [ ] Bouton "Confirmer" → Mise à jour statut `CampaignPlayer.Status = Left`
  - [ ] Notification email au joueur
  - [ ] Joueur ne peut plus accéder à la campagne
- [ ] Action "Suspendre" (temporaire) :
  - [ ] Modal : Durée suspension (date fin)
  - [ ] `CampaignPlayer.Status = Inactive`
  - [ ] Joueur ne reçoit plus notifications sessions
  - [ ] Peut être réactivé par MJ
- [ ] Action "Réactiver" :
  - [ ] Disponible pour joueurs Inactifs
  - [ ] `CampaignPlayer.Status = Active`
  - [ ] Notification email joueur
- [ ] Filtre par statut (Tous, Actifs, Inactifs, Retirés)
- [ ] Seul le MJ (créateur campagne) peut gérer joueurs

### Techniques
- [ ] Endpoint : `DELETE /api/campaigns/{campaignId}/players/{userId}` [Authorize(Roles = "GameMaster")]
- [ ] Body : `{ "reason": "..." }` (optionnel)
- [ ] Response 200 : `{ "success": true }`
- [ ] Endpoint : `PATCH /api/campaigns/{campaignId}/players/{userId}/status` [Authorize(Roles = "GameMaster")]
- [ ] Body : `{ "status": "Inactive", "suspendedUntil": "2025-02-01T00:00:00Z" }`
- [ ] Response 200 : `{ "status": "Inactive", "updatedAt": "..." }`

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CampaignService.RemovePlayer_ValidRequest_UpdatesStatus()`
- [ ] `CampaignService.RemovePlayer_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `CampaignService.SuspendPlayer_UpdatesStatusAndDate()`
- [ ] `CampaignService.ReactivatePlayer_RestoresActiveStatus()`

### Tests d'Intégration
- [ ] `CampaignEndpoint_RemovePlayer_UpdatesDatabase()`
- [ ] `CampaignEndpoint_RemovePlayer_SendsEmailNotification()`
- [ ] `CampaignEndpoint_SuspendPlayer_UpdatesStatus()`

### Tests E2E
- [ ] MJ clique "Retirer" → Confirmation → Joueur statut Left → Email envoyé
- [ ] MJ suspend joueur → Joueur ne reçoit plus notifications sessions
- [ ] MJ réactive joueur → Joueur reçoit notification + peut rejoindre sessions

---

## 🔧 Tâches Techniques

### Backend
- [ ] Modifier entité `CampaignPlayer` :
```csharp
public class CampaignPlayer
{
    public Guid CampaignId { get; set; }
    public Guid UserId { get; set; }
    public Guid? CharacterId { get; set; }
    public DateTime JoinedAt { get; set; }
    public PlayerStatus Status { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public string? RemovalReason { get; set; }
    public DateTime? RemovedAt { get; set; }
    
    // Navigation
    public Campaign Campaign { get; set; }
    public User User { get; set; }
    public Character? Character { get; set; }
}

public enum PlayerStatus
{
    Active = 0,
    Inactive = 1,
    Left = 2
}
```
- [ ] Créer `CampaignService.RemovePlayerAsync(campaignId, userId, reason, requestUserId)` :
  - [ ] Vérifier requestUserId == Campaign.CreatedBy
  - [ ] Vérifier CampaignPlayer existe
  - [ ] Mettre à jour Status = Left, RemovedAt = Now, RemovalReason
  - [ ] Envoyer email notification
  - [ ] Log action (optional: AuditLog)
- [ ] Créer `CampaignService.UpdatePlayerStatusAsync(campaignId, userId, newStatus, suspendedUntil)` :
  - [ ] Vérifier permissions
  - [ ] Mettre à jour Status, SuspendedUntil
  - [ ] Envoyer notification si réactivation
- [ ] Créer endpoints :
  - [ ] `DELETE /api/campaigns/{campaignId}/players/{userId}` [Authorize(Roles = "GameMaster")]
  - [ ] `PATCH /api/campaigns/{campaignId}/players/{userId}/status` [Authorize(Roles = "GameMaster")]
  - [ ] `GET /api/campaigns/{campaignId}/players` (avec filtres status)

### Frontend
- [ ] Créer page `CampaignPlayers.razor` (/campaigns/{id}/players) :
```razor
<div class="players-page">
    <h2>Joueurs de la Campagne</h2>
    
    <div class="filters">
        <select @bind="StatusFilter" @bind:after="LoadPlayers">
            <option value="">Tous</option>
            <option value="Active">Actifs</option>
            <option value="Inactive">Inactifs</option>
            <option value="Left">Retirés</option>
        </select>
    </div>
    
    <table class="players-table">
        <thead>
            <tr>
                <th>Joueur</th>
                <th>Personnage</th>
                <th>Date de Rejointe</th>
                <th>Statut</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var player in Players)
            {
                <tr>
                    <td>@player.UserName</td>
                    <td>@(player.CharacterName ?? "Aucun")</td>
                    <td>@player.JoinedAt.ToString("dd/MM/yyyy")</td>
                    <td>
                        <span class="badge @player.Status.ToString().ToLower()">
                            @player.Status
                        </span>
                    </td>
                    <td>
                        @if (player.Status == PlayerStatus.Active)
                        {
                            <button @onclick="() => SuspendPlayer(player)">Suspendre</button>
                            <button @onclick="() => RemovePlayer(player)" class="btn-danger">Retirer</button>
                        }
                        @if (player.Status == PlayerStatus.Inactive)
                        {
                            <button @onclick="() => ReactivatePlayer(player)">Réactiver</button>
                            <button @onclick="() => RemovePlayer(player)" class="btn-danger">Retirer</button>
                        }
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```
- [ ] Créer modal `RemovePlayerModal.razor` :
```razor
<Modal Title="Retirer un joueur" IsVisible="@IsVisible" OnClose="OnClose">
    <p>Êtes-vous sûr de vouloir retirer <strong>@PlayerName</strong> de la campagne ?</p>
    <p class="text-muted">Cette action est irréversible. Le joueur ne pourra plus accéder à la campagne.</p>
    
    <div class="form-group">
        <label>Raison (optionnelle)</label>
        <textarea @bind="Reason" rows="3" class="form-control" placeholder="Ex: Absence prolongée..."></textarea>
    </div>
    
    <div class="modal-actions">
        <button @onclick="OnConfirm" class="btn-danger">Confirmer</button>
        <button @onclick="OnClose" class="btn-secondary">Annuler</button>
    </div>
</Modal>
```
- [ ] Créer modal `SuspendPlayerModal.razor` :
```razor
<Modal Title="Suspendre un joueur" IsVisible="@IsVisible" OnClose="OnClose">
    <p>Suspendre <strong>@PlayerName</strong> jusqu'à :</p>
    
    <div class="form-group">
        <label>Date de fin de suspension</label>
        <input type="date" @bind="SuspendedUntil" class="form-control" />
    </div>
    
    <div class="modal-actions">
        <button @onclick="OnConfirm" class="btn-warning">Suspendre</button>
        <button @onclick="OnClose" class="btn-secondary">Annuler</button>
    </div>
</Modal>
```
- [ ] Implémenter logique modals dans `CampaignPlayers.razor` :
```csharp
private async Task RemovePlayer(CampaignPlayerDto player)
{
    _removeModal.Show(player);
}

private async Task OnRemoveConfirmed(Guid userId, string reason)
{
    await Http.DeleteAsync($"/api/campaigns/{CampaignId}/players/{userId}?reason={reason}");
    await LoadPlayers(); // Refresh
    Toast.Success($"{player.UserName} a été retiré de la campagne.");
}
```

### Base de Données
- [ ] Migration : Ajouter colonnes `SuspendedUntil`, `RemovalReason`, `RemovedAt` à `CampaignPlayers`
- [ ] Index sur `(CampaignId, Status)` pour filtres

---

## 🔗 Dépendances

### Dépend de
- [US-017](./US-017-selection-personnage.md) - CampaignPlayer entity

### Bloque
- Aucune

---

## 📊 Estimation

**Story Points** : 2

**Détails** :
- Complexité : Faible (CRUD simple)
- Effort : 0.5-1 jour
- Risques : Aucun

---

## 📝 Notes Techniques

### Email Notification (Retrait)
```
Retrait de la Campagne

Bonjour {PlayerName},

Vous avez été retiré de la campagne "{CampaignName}" par le Maître du Jeu.

Raison : {Reason}

Merci d'avoir participé.

L'équipe Chronique des Mondes
```

### Email Notification (Réactivation)
```
Réactivation dans la Campagne

Bonjour {PlayerName},

Vous avez été réactivé dans la campagne "{CampaignName}" !

Vous pouvez à nouveau participer aux sessions.

[Rejoindre la Campagne]
```

### Permissions Check
```csharp
[Authorize(Roles = "GameMaster")]
public async Task<IActionResult> RemovePlayer(Guid campaignId, Guid userId)
{
    var campaign = await _context.Campaigns.FindAsync(campaignId);
    var requestUserId = User.GetUserId();
    
    if (campaign.CreatedBy != requestUserId)
        return Forbid();
    
    // Continue...
}
```

### Exclusion Joueurs Retirés des Sessions
```csharp
// Dans SessionService.StartSessionAsync
var activePlayers = await _context.CampaignPlayers
    .Where(cp => cp.CampaignId == campaignId && cp.Status == PlayerStatus.Active)
    .ToListAsync();

foreach (var player in activePlayers)
{
    // Créer SessionParticipant
    // Envoyer notification
}
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Actions Retirer/Suspendre/Réactiver fonctionnelles
- [ ] Modals confirmation opérationnelles
- [ ] Notifications email envoyées
- [ ] Filtres par statut fonctionnels
- [ ] Permissions respectées (seul MJ)
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 5
