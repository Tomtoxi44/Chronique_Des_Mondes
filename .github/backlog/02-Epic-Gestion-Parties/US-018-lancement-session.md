# US-018 - Lancement de Session

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** lancer une session de jeu pour ma campagne  
**Afin de** jouer en temps réel avec mes joueurs

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Page détails campagne avec bouton "Lancer une Session"
- [ ] Modal configuration session :
  - [ ] Sélection chapitre de départ (liste déroulante)
  - [ ] Message d'accueil optionnel
  - [ ] Durée estimée (optionnel)
- [ ] Clic "Lancer" → Création session → Redirection interface session live
- [ ] Notifications envoyées à TOUS les joueurs (email + in-app + SignalR) :
  - [ ] Joueurs connectés : Notification temps réel + modal "Rejoindre"
  - [ ] Joueurs déconnectés : Email + notification in-app
- [ ] Interface Session Live (SessionLive.razor) :
  - [ ] **MJ** : Contrôles complets (progression, combat, dés, etc.)
  - [ ] **Joueurs** : Vue lecture + actions limitées
- [ ] Liste participants avec statuts :
  - [ ] ✓ Connecté (vert)
  - [ ] ⏱️ En attente (orange)
  - [ ] ❌ Absent (gris)
- [ ] Chat en temps réel (SignalR)
- [ ] Affichage chapitre actuel
- [ ] Bouton "Terminer la Session" (MJ uniquement)
- [ ] Sauvegarde automatique état session (toutes les 2 minutes)

### Techniques
- [ ] Endpoint : `POST /api/campaigns/{campaignId}/sessions`
- [ ] Body : `{ "startingChapterId": "guid", "welcomeMessage": "...", "estimatedDuration": 180 }`
- [ ] Response 201 : `{ "sessionId": "guid", "status": "Active", "startedAt": "..." }`
- [ ] SignalR : `SessionHub` avec méthodes :
  - [ ] `JoinSession(sessionId)` - Joueur rejoint
  - [ ] `LeaveSession(sessionId)` - Joueur quitte
  - [ ] `SendMessage(sessionId, message)` - Chat
  - [ ] `UpdateSessionState(sessionId, state)` - MJ met à jour
- [ ] WebSocket pour maintenir connexion active

---

## 🧪 Tests

### Tests Unitaires
- [ ] `SessionService.StartSession_WithValidCampaign_CreatesSession()`
- [ ] `SessionService.StartSession_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `SessionService.NotifyPlayers_SendsEmailAndSignalR()`
- [ ] `SessionService.JoinSession_UpdatesParticipantStatus()`

### Tests d'Intégration
- [ ] `SessionEndpoint_StartSession_CreatesInDatabase()`
- [ ] `SessionEndpoint_StartSession_NotifiesAllPlayers()`
- [ ] `SessionHub_JoinSession_UpdatesConnectedPlayers()`

### Tests E2E
- [ ] MJ lance session → Joueur connecté reçoit notification modal → Rejoint → Apparaît dans liste
- [ ] Joueur déconnecté reçoit email → Login → Rejoint session

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `Session` :
```csharp
public class Session
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; }
    public Guid? CurrentChapterId { get; set; }
    public Chapter CurrentChapter { get; set; }
    public SessionStatus Status { get; set; }
    public string WelcomeMessage { get; set; }
    public int? EstimatedDuration { get; set; } // minutes
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string SessionState { get; set; } // JSON pour flexibilité
    
    // Relations
    public ICollection<SessionParticipant> Participants { get; set; }
    public ICollection<SessionEvent> Events { get; set; } // Log
}

public enum SessionStatus
{
    Active = 0,
    Paused = 1,
    Completed = 2
}

public class SessionParticipant
{
    public Guid SessionId { get; set; }
    public Session Session { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public ParticipantStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

public enum ParticipantStatus
{
    Invited = 0,
    Connected = 1,
    Disconnected = 2
}
```
- [ ] Créer `SessionService.StartSessionAsync(campaignId, startingChapterId, welcomeMessage, userId)` :
  - [ ] Vérifier userId == Campaign.CreatedBy
  - [ ] Vérifier pas de session active existante
  - [ ] Créer session
  - [ ] Créer SessionParticipants pour tous CampaignPlayers
  - [ ] Envoyer notifications (email + SignalR)
- [ ] Créer `SessionHub` (SignalR) :
```csharp
public class SessionHub : Hub
{
    public async Task JoinSession(Guid sessionId)
    {
        var userId = Context.User.GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Session-{sessionId}");
        
        // Mettre à jour statut participant
        await _sessionService.UpdateParticipantStatusAsync(sessionId, userId, ParticipantStatus.Connected);
        
        // Notifier groupe
        await Clients.Group($"Session-{sessionId}")
            .SendAsync("PlayerJoined", new { UserId = userId, Username = Context.User.GetUsername() });
    }
    
    public async Task LeaveSession(Guid sessionId)
    {
        var userId = Context.User.GetUserId();
        await _sessionService.UpdateParticipantStatusAsync(sessionId, userId, ParticipantStatus.Disconnected);
        await Clients.Group($"Session-{sessionId}")
            .SendAsync("PlayerLeft", new { UserId = userId });
    }
    
    public async Task SendMessage(Guid sessionId, string message)
    {
        await Clients.Group($"Session-{sessionId}")
            .SendAsync("ReceiveMessage", new
            {
                Username = Context.User.GetUsername(),
                Message = message,
                Timestamp = DateTime.UtcNow
            });
    }
}
```
- [ ] Créer endpoints :
  - [ ] `POST /api/campaigns/{campaignId}/sessions` [Authorize(Roles = "GameMaster")]
  - [ ] `GET /api/sessions/{id}` [Authorize]
  - [ ] `POST /api/sessions/{id}/end` [Authorize(Roles = "GameMaster")]

### Frontend
- [ ] Créer page `SessionLive.razor` (/sessions/{id})
- [ ] Créer composant `SessionControls.razor` (pour MJ) :
  - [ ] Bouton "Chapitre Suivant"
  - [ ] Bouton "Lancer Combat"
  - [ ] Bouton "Terminer Session"
- [ ] Créer composant `ParticipantsList.razor` :
  - [ ] Liste joueurs avec statut couleur
  - [ ] Mise à jour temps réel via SignalR
- [ ] Créer composant `SessionChat.razor` :
  - [ ] Liste messages
  - [ ] Input envoi message
  - [ ] SignalR pour temps réel
- [ ] Créer composant `ChapterDisplay.razor` :
  - [ ] Affichage contenu chapitre actuel
  - [ ] Format Markdown → HTML
- [ ] Implémenter `SignalRSessionService` :
```csharp
public class SignalRSessionService
{
    private HubConnection _connection;

    public async Task ConnectAsync(Guid sessionId)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_config["ApiUrl"]}/hubs/session")
            .Build();

        _connection.On<PlayerJoinedDto>("PlayerJoined", OnPlayerJoined);
        _connection.On<MessageDto>("ReceiveMessage", OnMessageReceived);

        await _connection.StartAsync();
        await _connection.InvokeAsync("JoinSession", sessionId);
    }

    public async Task SendMessageAsync(Guid sessionId, string message)
    {
        await _connection.InvokeAsync("SendMessage", sessionId, message);
    }

    public async Task DisconnectAsync(Guid sessionId)
    {
        await _connection.InvokeAsync("LeaveSession", sessionId);
        await _connection.StopAsync();
    }
}
```
- [ ] Modal "Session Lancée" pour joueurs connectés :
  - [ ] Notification toast + modal
  - [ ] Bouton "Rejoindre la Session"
  - [ ] Redirection vers SessionLive

### Base de Données
- [ ] Migration : Créer tables `Sessions`, `SessionParticipants`, `SessionEvents`
- [ ] Index sur `(CampaignId, Status)` pour requêtes sessions actives
- [ ] Index sur `(SessionId, UserId)` pour participants

---

## 🔗 Dépendances

### Dépend de
- [US-014](./US-014-creation-chapitres.md) - Chapitres
- [US-017](./US-017-selection-personnage.md) - Joueurs dans campagne
- Configuration SignalR Hub

### Bloque
- [US-019](./US-019-progression-chapitres.md) - Progression chapitres
- [US-032](../04-Epic-Combat-Des/US-032-declenchement-combat.md) - Combat

---

## 📊 Estimation

**Story Points** : 8

**Détails** :
- Complexité : Haute (SignalR, temps réel, notifications)
- Effort : 2-3 jours
- Risques : WebSockets, gestion déconnexions

---

## 📝 Notes Techniques

### Email Notification
```
🎲 Session Lancée !

{MJName} a lancé une session pour la campagne {CampaignName}.

Chapitre : {ChapterTitle}
Message du MJ : {WelcomeMessage}

[Rejoindre la Session]

La session est active maintenant !
```

### SignalR Connection Resilience
```csharp
_connection.Closed += async (error) =>
{
    await Task.Delay(Random.Shared.Next(0, 5) * 1000);
    await _connection.StartAsync();
    await _connection.InvokeAsync("JoinSession", _sessionId);
};
```

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] SignalR Hub fonctionnel
- [ ] Notifications temps réel opérationnelles
- [ ] Chat en temps réel fonctionnel
- [ ] Gestion déconnexions/reconnexions
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 4
