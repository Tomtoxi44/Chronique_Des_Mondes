# SignalR - Communication Temps Réel

## Vue d'ensemble

**SignalR** permet la communication bidirectionnelle en temps réel entre le serveur et les clients Blazor. Le système utilise **trois hubs spécialisés** pour isoler les responsabilités et optimiser les performances.

### Principes de conception

1. **3 Hubs séparés** : SessionHub, CombatHub, NotificationHub (isolation des responsabilités)
2. **Groupes SignalR** : Isolation par session/combat pour éviter les fuites d'informations
3. **Authentification requise** : Tous les hubs nécessitent un token JWT
4. **Reconnexion automatique** : Gestion des déconnexions avec retry policy
5. **Typage fort** : Interfaces partagées entre serveur et client

---

## 1. Architecture des Hubs

### 1.1 Vue d'ensemble

```
┌─────────────────────────────────────────────────────────────┐
│                    Blazor Client (Web)                      │
├─────────────────────────────────────────────────────────────┤
│  SessionConnection  │  CombatConnection  │  NotificationConn│
└──────────┬──────────┴──────────┬─────────┴─────────┬────────┘
           │                     │                   │
           │ WebSocket           │ WebSocket         │ WebSocket
           │                     │                   │
┌──────────▼─────────────────────▼───────────────────▼────────┐
│                      SignalR Hubs                           │
├─────────────────────────────────────────────────────────────┤
│  SessionHub         │  CombatHub         │  NotificationHub │
│  - JoinSession      │  - JoinCombat      │  - GetNotifs     │
│  - SendMessage      │  - NextTurn        │  - MarkAsRead    │
│  - RollDice         │  - DealDamage      │                  │
│  - ProposeTradeThe  │  - UpdateInit      │                  │
└─────────────────────────────────────────────────────────────┘
```

---

### 1.2 SessionHub

Gère les communications durant une session de jeu (messages, jets de dés, échanges).

**Emplacement :** `Cdm.ApiService/Hubs/SessionHub.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class SessionHub : Hub
{
    private readonly ISessionService _sessionService;
    private readonly IDiceService _diceService;
    private readonly IEquipmentService _equipmentService;
    private readonly ILogger<SessionHub> _logger;
    
    public SessionHub(
        ISessionService sessionService,
        IDiceService diceService,
        IEquipmentService equipmentService,
        ILogger<SessionHub> logger)
    {
        _sessionService = sessionService;
        _diceService = diceService;
        _equipmentService = equipmentService;
        _logger = logger;
    }
    
    // Connexion à une session
    public async Task JoinSession(int sessionId)
    {
        var userId = GetUserId();
        
        // Vérifier que l'utilisateur a accès à cette session
        var hasAccess = await _sessionService.UserHasAccessAsync(sessionId, userId);
        if (!hasAccess)
        {
            throw new HubException("Access denied to this session");
        }
        
        var groupName = GetSessionGroupName(sessionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined session {SessionId}", userId, sessionId);
        
        // Notifier les autres participants
        await Clients.OthersInGroup(groupName).SendAsync("UserJoined", new
        {
            userId,
            username = Context.User.Identity.Name,
            joinedAt = DateTime.UtcNow
        });
    }
    
    // Quitter une session
    public async Task LeaveSession(int sessionId)
    {
        var userId = GetUserId();
        var groupName = GetSessionGroupName(sessionId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} left session {SessionId}", userId, sessionId);
        
        // Notifier les autres participants
        await Clients.OthersInGroup(groupName).SendAsync("UserLeft", new
        {
            userId,
            username = Context.User.Identity.Name,
            leftAt = DateTime.UtcNow
        });
    }
    
    // Envoyer un message dans le chat
    public async Task SendMessage(int sessionId, string message)
    {
        var userId = GetUserId();
        var username = Context.User.Identity.Name;
        var groupName = GetSessionGroupName(sessionId);
        
        _logger.LogInformation("User {UserId} sent message in session {SessionId}", userId, sessionId);
        
        // Diffuser à tous les participants
        await Clients.Group(groupName).SendAsync("MessageReceived", new
        {
            userId,
            username,
            message,
            timestamp = DateTime.UtcNow
        });
    }
    
    // Lancer des dés
    public async Task RollDice(int sessionId, int characterId, string diceNotation, string context)
    {
        var userId = GetUserId();
        var groupName = GetSessionGroupName(sessionId);
        
        // Lancer les dés côté serveur
        var result = await _diceService.RollAsync(new DiceRollRequest
        {
            SessionId = sessionId,
            CharacterId = characterId,
            UserId = userId,
            DiceNotation = diceNotation,
            RollType = RollType.Generic,
            Context = context
        });
        
        _logger.LogInformation(
            "User {UserId} rolled {Notation} for character {CharacterId}: {Total}",
            userId, diceNotation, characterId, result.TotalResult);
        
        // Diffuser le résultat à tous
        await Clients.Group(groupName).SendAsync("DiceRolled", new
        {
            characterId,
            characterName = result.CharacterName,
            diceNotation,
            results = result.Results,
            modifiers = result.Modifiers,
            totalResult = result.TotalResult,
            context,
            rolledAt = result.RolledAt
        });
    }
    
    // Proposer un échange d'équipement
    public async Task ProposeEquipmentTrade(
        int sessionId,
        int fromCharacterId,
        int toCharacterId,
        List<EquipmentItem> proposedItems,
        List<EquipmentItem> requestedItems)
    {
        var userId = GetUserId();
        var groupName = GetSessionGroupName(sessionId);
        
        var proposal = await _equipmentService.CreateProposalAsync(new EquipmentProposalRequest
        {
            SessionId = sessionId,
            FromCharacterId = fromCharacterId,
            ToCharacterId = toCharacterId,
            ProposedItems = proposedItems,
            RequestedItems = requestedItems
        });
        
        if (!proposal.IsSuccess)
        {
            throw new HubException(proposal.Error);
        }
        
        _logger.LogInformation(
            "Equipment trade proposed from character {From} to {To}",
            fromCharacterId, toCharacterId);
        
        // Notifier uniquement le destinataire
        var targetConnectionIds = await GetConnectionIdsForCharacter(toCharacterId);
        await Clients.Clients(targetConnectionIds).SendAsync("TradeProposalReceived", new
        {
            proposalId = proposal.Data.Id,
            fromCharacterId,
            fromCharacterName = proposal.Data.FromCharacterName,
            proposedItems,
            requestedItems,
            createdAt = proposal.Data.CreatedAt
        });
    }
    
    // Accepter/Refuser une proposition
    public async Task RespondToTrade(int proposalId, bool accept)
    {
        var userId = GetUserId();
        
        var result = accept
            ? await _equipmentService.AcceptProposalAsync(proposalId, userId)
            : await _equipmentService.DeclineProposalAsync(proposalId, userId);
        
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error);
        }
        
        var groupName = GetSessionGroupName(result.Data.SessionId);
        
        // Notifier tous les participants de la session
        await Clients.Group(groupName).SendAsync("TradeResolved", new
        {
            proposalId,
            accepted = accept,
            fromCharacterId = result.Data.FromCharacterId,
            toCharacterId = result.Data.ToCharacterId,
            resolvedAt = DateTime.UtcNow
        });
    }
    
    // Gestion de la déconnexion
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} disconnected", userId);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    // Helpers
    private int GetUserId()
    {
        return int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
    
    private static string GetSessionGroupName(int sessionId) => $"session_{sessionId}";
    
    private async Task<List<string>> GetConnectionIdsForCharacter(int characterId)
    {
        // Récupérer tous les connectionIds des utilisateurs possédant ce personnage
        // (implémentation dépend de votre système de tracking)
        return new List<string>(); // À implémenter
    }
}
```

---

### 1.3 CombatHub

Gère les actions de combat en temps réel (tours, dégâts, initiative).

**Emplacement :** `Cdm.ApiService/Hubs/CombatHub.cs`

```csharp
[Authorize]
public class CombatHub : Hub
{
    private readonly ICombatService _combatService;
    private readonly ILogger<CombatHub> _logger;
    
    public CombatHub(ICombatService combatService, ILogger<CombatHub> logger)
    {
        _combatService = combatService;
        _logger = logger;
    }
    
    // Rejoindre un combat
    public async Task JoinCombat(int combatId)
    {
        var userId = GetUserId();
        
        // Vérifier l'accès
        var hasAccess = await _combatService.UserHasAccessAsync(combatId, userId);
        if (!hasAccess)
        {
            throw new HubException("Access denied to this combat");
        }
        
        var groupName = GetCombatGroupName(combatId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined combat {CombatId}", userId, combatId);
        
        // Envoyer l'état actuel du combat
        var combatState = await _combatService.GetStateAsync(combatId);
        await Clients.Caller.SendAsync("CombatState", combatState);
        
        // Notifier les autres
        await Clients.OthersInGroup(groupName).SendAsync("ParticipantJoined", new
        {
            userId,
            username = Context.User.Identity.Name,
            joinedAt = DateTime.UtcNow
        });
    }
    
    // Quitter un combat
    public async Task LeaveCombat(int combatId)
    {
        var userId = GetUserId();
        var groupName = GetCombatGroupName(combatId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} left combat {CombatId}", userId, combatId);
        
        await Clients.OthersInGroup(groupName).SendAsync("ParticipantLeft", new
        {
            userId,
            username = Context.User.Identity.Name,
            leftAt = DateTime.UtcNow
        });
    }
    
    // Passer au tour suivant (MJ uniquement)
    public async Task NextTurn(int combatId)
    {
        var userId = GetUserId();
        
        // Vérifier que l'utilisateur est le MJ
        var isGameMaster = await _combatService.IsGameMasterAsync(combatId, userId);
        if (!isGameMaster)
        {
            throw new HubException("Only the Game Master can advance turns");
        }
        
        var result = await _combatService.NextTurnAsync(combatId);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error);
        }
        
        var groupName = GetCombatGroupName(combatId);
        
        _logger.LogInformation(
            "Combat {CombatId} - Turn advanced to participant {ParticipantId}",
            combatId, result.Data.CurrentParticipantId);
        
        // Notifier tous les participants
        await Clients.Group(groupName).SendAsync("TurnChanged", new
        {
            combatId,
            currentRound = result.Data.CurrentRound,
            currentTurn = result.Data.CurrentTurn,
            currentParticipant = new
            {
                id = result.Data.CurrentParticipant.Id,
                name = result.Data.CurrentParticipant.Name,
                characterId = result.Data.CurrentParticipant.CharacterId,
                isNpc = result.Data.CurrentParticipant.IsNpc
            },
            changedAt = DateTime.UtcNow
        });
    }
    
    // Infliger des dégâts (MJ uniquement)
    public async Task DealDamage(int combatId, int participantId, int damage, string damageType = null)
    {
        var userId = GetUserId();
        
        // Vérifier que l'utilisateur est le MJ
        var isGameMaster = await _combatService.IsGameMasterAsync(combatId, userId);
        if (!isGameMaster)
        {
            throw new HubException("Only the Game Master can deal damage");
        }
        
        var result = await _combatService.ApplyDamageAsync(combatId, participantId, damage, damageType);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error);
        }
        
        var groupName = GetCombatGroupName(combatId);
        
        _logger.LogInformation(
            "Combat {CombatId} - {Damage} damage dealt to participant {ParticipantId}",
            combatId, damage, participantId);
        
        // Notifier tous les participants
        await Clients.Group(groupName).SendAsync("ParticipantDamaged", new
        {
            combatId,
            participantId,
            participantName = result.Data.Name,
            damage,
            damageType,
            previousHealth = result.Data.PreviousHealth,
            currentHealth = result.Data.CurrentHealth,
            maxHealth = result.Data.MaxHealth,
            isActive = result.Data.IsActive,
            timestamp = DateTime.UtcNow
        });
        
        // Si le participant est KO
        if (!result.Data.IsActive)
        {
            await Clients.Group(groupName).SendAsync("ParticipantDefeated", new
            {
                combatId,
                participantId,
                participantName = result.Data.Name,
                defeatedAt = DateTime.UtcNow
            });
        }
    }
    
    // Soigner un participant
    public async Task HealParticipant(int combatId, int participantId, int healAmount)
    {
        var userId = GetUserId();
        
        // Vérifier que l'utilisateur est le MJ
        var isGameMaster = await _combatService.IsGameMasterAsync(combatId, userId);
        if (!isGameMaster)
        {
            throw new HubException("Only the Game Master can heal participants");
        }
        
        var result = await _combatService.ApplyHealingAsync(combatId, participantId, healAmount);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error);
        }
        
        var groupName = GetCombatGroupName(combatId);
        
        _logger.LogInformation(
            "Combat {CombatId} - {Heal} HP healed to participant {ParticipantId}",
            combatId, healAmount, participantId);
        
        await Clients.Group(groupName).SendAsync("ParticipantHealed", new
        {
            combatId,
            participantId,
            participantName = result.Data.Name,
            healAmount,
            previousHealth = result.Data.PreviousHealth,
            currentHealth = result.Data.CurrentHealth,
            maxHealth = result.Data.MaxHealth,
            timestamp = DateTime.UtcNow
        });
    }
    
    // Mettre à jour l'ordre d'initiative
    public async Task UpdateInitiative(int combatId, int participantId, int newInitiativeOrder)
    {
        var userId = GetUserId();
        
        var isGameMaster = await _combatService.IsGameMasterAsync(combatId, userId);
        if (!isGameMaster)
        {
            throw new HubException("Only the Game Master can update initiative");
        }
        
        var result = await _combatService.UpdateInitiativeAsync(combatId, participantId, newInitiativeOrder);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error);
        }
        
        var groupName = GetCombatGroupName(combatId);
        
        _logger.LogInformation(
            "Combat {CombatId} - Initiative updated for participant {ParticipantId}",
            combatId, participantId);
        
        // Envoyer le nouvel ordre complet
        await Clients.Group(groupName).SendAsync("InitiativeUpdated", new
        {
            combatId,
            participants = result.Data.Participants.OrderByDescending(p => p.InitiativeOrder),
            updatedAt = DateTime.UtcNow
        });
    }
    
    // Terminer le combat (MJ uniquement)
    public async Task EndCombat(int combatId)
    {
        var userId = GetUserId();
        
        var isGameMaster = await _combatService.IsGameMasterAsync(combatId, userId);
        if (!isGameMaster)
        {
            throw new HubException("Only the Game Master can end combat");
        }
        
        var result = await _combatService.EndCombatAsync(combatId);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error);
        }
        
        var groupName = GetCombatGroupName(combatId);
        
        _logger.LogInformation("Combat {CombatId} ended", combatId);
        
        await Clients.Group(groupName).SendAsync("CombatEnded", new
        {
            combatId,
            endedAt = DateTime.UtcNow,
            summary = result.Data.Summary
        });
    }
    
    // Helpers
    private int GetUserId()
    {
        return int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
    
    private static string GetCombatGroupName(int combatId) => $"combat_{combatId}";
}
```

---

### 1.4 NotificationHub

Gère les notifications utilisateur en temps réel.

**Emplacement :** `Cdm.ApiService/Hubs/NotificationHub.cs`

```csharp
[Authorize]
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationHub> _logger;
    
    public NotificationHub(
        INotificationService notificationService,
        ILogger<NotificationHub> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }
    
    // Connexion au hub (auto-join à son groupe utilisateur)
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var groupName = GetUserGroupName(userId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} connected to notification hub", userId);
        
        // Envoyer les notifications non lues
        var unreadNotifications = await _notificationService.GetUnreadAsync(userId);
        await Clients.Caller.SendAsync("UnreadNotifications", new
        {
            notifications = unreadNotifications,
            count = unreadNotifications.Count
        });
        
        await base.OnConnectedAsync();
    }
    
    // Marquer une notification comme lue
    public async Task MarkAsRead(int notificationId)
    {
        var userId = GetUserId();
        
        var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error);
        }
        
        _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}", notificationId, userId);
        
        // Confirmer au client
        await Clients.Caller.SendAsync("NotificationMarkedAsRead", new
        {
            notificationId,
            readAt = DateTime.UtcNow
        });
    }
    
    // Marquer toutes les notifications comme lues
    public async Task MarkAllAsRead()
    {
        var userId = GetUserId();
        
        var result = await _notificationService.MarkAllAsReadAsync(userId);
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error);
        }
        
        _logger.LogInformation("All notifications marked as read for user {UserId}", userId);
        
        await Clients.Caller.SendAsync("AllNotificationsMarkedAsRead", new
        {
            count = result.Data.Count,
            readAt = DateTime.UtcNow
        });
    }
    
    // Récupérer les notifications récentes
    public async Task GetRecentNotifications(int count = 10)
    {
        var userId = GetUserId();
        
        var notifications = await _notificationService.GetRecentAsync(userId, count);
        
        await Clients.Caller.SendAsync("RecentNotifications", new
        {
            notifications,
            total = notifications.Count
        });
    }
    
    // Déconnexion
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = GetUserId();
        var groupName = GetUserGroupName(userId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    // Helpers
    private int GetUserId()
    {
        return int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
    
    private static string GetUserGroupName(int userId) => $"user_{userId}";
}
```

---

## 2. Configuration côté serveur

### 2.1 Program.cs

```csharp
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Ajouter SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 102400; // 100 KB
});

var app = builder.Build();

// Mapper les hubs
app.MapHub<SessionHub>("/hubs/session");
app.MapHub<CombatHub>("/hubs/combat");
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
```

---

### 2.2 Envoi de notifications depuis les services

Les services peuvent envoyer des notifications via `IHubContext`.

```csharp
public class SessionService : ISessionService
{
    private readonly IHubContext<SessionHub> _sessionHubContext;
    private readonly IHubContext<NotificationHub> _notificationHubContext;
    
    public SessionService(
        IHubContext<SessionHub> sessionHubContext,
        IHubContext<NotificationHub> notificationHubContext)
    {
        _sessionHubContext = sessionHubContext;
        _notificationHubContext = notificationHubContext;
    }
    
    public async Task<Result<Session>> StartSessionAsync(int sessionId, int userId)
    {
        // Démarrer la session
        var session = await _repository.GetByIdAsync(sessionId);
        session.Status = SessionStatus.Active;
        session.StartedAt = DateTime.UtcNow;
        
        await _repository.UpdateAsync(session);
        
        // Notifier tous les participants via SessionHub
        var groupName = $"session_{sessionId}";
        await _sessionHubContext.Clients.Group(groupName).SendAsync("SessionStarted", new
        {
            sessionId,
            startedAt = session.StartedAt
        });
        
        // Envoyer des notifications individuelles via NotificationHub
        var participants = await GetParticipantsAsync(sessionId);
        foreach (var participant in participants)
        {
            var userGroupName = $"user_{participant.UserId}";
            await _notificationHubContext.Clients.Group(userGroupName).SendAsync("NewNotification", new
            {
                type = NotificationType.SessionStarted,
                title = "Session démarrée",
                message = $"La session '{session.Name}' a commencé",
                relatedEntityId = sessionId,
                timestamp = DateTime.UtcNow
            });
        }
        
        return Result<Session>.Success(session);
    }
}
```

---

## 3. Configuration côté client (Blazor)

### 3.1 Service de connexion SessionHub

**Emplacement :** `Cdm.Web/Services/SessionHubService.cs`

```csharp
using Microsoft.AspNetCore.SignalR.Client;

public class SessionHubService : IAsyncDisposable
{
    private HubConnection _connection;
    private readonly ILogger<SessionHubService> _logger;
    private readonly IConfiguration _configuration;
    
    public event Func<UserJoinedEventArgs, Task> OnUserJoined;
    public event Func<UserLeftEventArgs, Task> OnUserLeft;
    public event Func<MessageReceivedEventArgs, Task> OnMessageReceived;
    public event Func<DiceRolledEventArgs, Task> OnDiceRolled;
    public event Func<TradeProposalEventArgs, Task> OnTradeProposalReceived;
    public event Func<TradeResolvedEventArgs, Task> OnTradeResolved;
    
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    
    public SessionHubService(ILogger<SessionHubService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public async Task InitializeAsync(string accessToken)
    {
        var hubUrl = _configuration["ApiService:BaseUrl"] + "/hubs/session";
        
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(accessToken);
            })
            .WithAutomaticReconnect(new[] {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            })
            .Build();
        
        // Enregistrer les handlers
        RegisterHandlers();
        
        // Gérer la reconnexion
        _connection.Reconnecting += OnReconnecting;
        _connection.Reconnected += OnReconnected;
        _connection.Closed += OnClosed;
        
        // Se connecter
        await _connection.StartAsync();
        _logger.LogInformation("SessionHub connection established");
    }
    
    private void RegisterHandlers()
    {
        _connection.On<UserJoinedEventArgs>("UserJoined", async (args) =>
        {
            _logger.LogInformation("User {Username} joined session", args.Username);
            if (OnUserJoined != null)
                await OnUserJoined.Invoke(args);
        });
        
        _connection.On<UserLeftEventArgs>("UserLeft", async (args) =>
        {
            _logger.LogInformation("User {Username} left session", args.Username);
            if (OnUserLeft != null)
                await OnUserLeft.Invoke(args);
        });
        
        _connection.On<MessageReceivedEventArgs>("MessageReceived", async (args) =>
        {
            _logger.LogInformation("Message received from {Username}", args.Username);
            if (OnMessageReceived != null)
                await OnMessageReceived.Invoke(args);
        });
        
        _connection.On<DiceRolledEventArgs>("DiceRolled", async (args) =>
        {
            _logger.LogInformation(
                "Dice rolled: {CharacterName} rolled {Notation} = {Total}",
                args.CharacterName, args.DiceNotation, args.TotalResult);
            if (OnDiceRolled != null)
                await OnDiceRolled.Invoke(args);
        });
        
        _connection.On<TradeProposalEventArgs>("TradeProposalReceived", async (args) =>
        {
            _logger.LogInformation("Trade proposal received from {CharacterName}", args.FromCharacterName);
            if (OnTradeProposalReceived != null)
                await OnTradeProposalReceived.Invoke(args);
        });
        
        _connection.On<TradeResolvedEventArgs>("TradeResolved", async (args) =>
        {
            _logger.LogInformation("Trade {Accepted}", args.Accepted ? "accepted" : "declined");
            if (OnTradeResolved != null)
                await OnTradeResolved.Invoke(args);
        });
    }
    
    // Méthodes client
    public async Task JoinSessionAsync(int sessionId)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to SessionHub");
        
        await _connection.InvokeAsync("JoinSession", sessionId);
    }
    
    public async Task LeaveSessionAsync(int sessionId)
    {
        if (!IsConnected) return;
        
        await _connection.InvokeAsync("LeaveSession", sessionId);
    }
    
    public async Task SendMessageAsync(int sessionId, string message)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to SessionHub");
        
        await _connection.InvokeAsync("SendMessage", sessionId, message);
    }
    
    public async Task RollDiceAsync(int sessionId, int characterId, string diceNotation, string context)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to SessionHub");
        
        await _connection.InvokeAsync("RollDice", sessionId, characterId, diceNotation, context);
    }
    
    public async Task ProposeEquipmentTradeAsync(
        int sessionId,
        int fromCharacterId,
        int toCharacterId,
        List<EquipmentItem> proposedItems,
        List<EquipmentItem> requestedItems)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to SessionHub");
        
        await _connection.InvokeAsync(
            "ProposeEquipmentTrade",
            sessionId,
            fromCharacterId,
            toCharacterId,
            proposedItems,
            requestedItems);
    }
    
    public async Task RespondToTradeAsync(int proposalId, bool accept)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to SessionHub");
        
        await _connection.InvokeAsync("RespondToTrade", proposalId, accept);
    }
    
    // Gestion de la reconnexion
    private Task OnReconnecting(Exception exception)
    {
        _logger.LogWarning(exception, "SessionHub connection lost, attempting to reconnect...");
        return Task.CompletedTask;
    }
    
    private Task OnReconnected(string connectionId)
    {
        _logger.LogInformation("SessionHub reconnected with connection ID: {ConnectionId}", connectionId);
        return Task.CompletedTask;
    }
    
    private Task OnClosed(Exception exception)
    {
        _logger.LogError(exception, "SessionHub connection closed");
        return Task.CompletedTask;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}

// Event Args
public record UserJoinedEventArgs(int UserId, string Username, DateTime JoinedAt);
public record UserLeftEventArgs(int UserId, string Username, DateTime LeftAt);
public record MessageReceivedEventArgs(int UserId, string Username, string Message, DateTime Timestamp);
public record DiceRolledEventArgs(
    int CharacterId,
    string CharacterName,
    string DiceNotation,
    List<int> Results,
    int Modifiers,
    int TotalResult,
    string Context,
    DateTime RolledAt);
public record TradeProposalEventArgs(
    int ProposalId,
    int FromCharacterId,
    string FromCharacterName,
    List<EquipmentItem> ProposedItems,
    List<EquipmentItem> RequestedItems,
    DateTime CreatedAt);
public record TradeResolvedEventArgs(
    int ProposalId,
    bool Accepted,
    int FromCharacterId,
    int ToCharacterId,
    DateTime ResolvedAt);
```

---

### 3.2 Enregistrement des services

**Emplacement :** `Cdm.Web/Program.cs`

```csharp
builder.Services.AddScoped<SessionHubService>();
builder.Services.AddScoped<CombatHubService>();
builder.Services.AddScoped<NotificationHubService>();
```

---

### 3.3 Utilisation dans un composant Blazor

**Emplacement :** `Cdm.Web/Components/Pages/Session.razor`

```razor
@page "/sessions/{SessionId:int}"
@inject SessionHubService SessionHub
@inject IAuthService AuthService
@implements IAsyncDisposable

<h3>Session: @sessionName</h3>

<!-- Chat -->
<div class="chat-container">
    @foreach (var message in messages)
    {
        <div class="message">
            <strong>@message.Username:</strong> @message.Message
            <span class="timestamp">@message.Timestamp.ToLocalTime()</span>
        </div>
    }
</div>

<div class="chat-input">
    <input @bind="newMessage" @onkeydown="HandleKeyDown" placeholder="Entrez votre message..." />
    <button @onclick="SendMessage">Envoyer</button>
</div>

<!-- Dice Roller -->
<div class="dice-roller">
    <input @bind="diceNotation" placeholder="1d20+5" />
    <input @bind="diceContext" placeholder="Contexte (optionnel)" />
    <button @onclick="RollDice">Lancer les dés</button>
</div>

<!-- Dice Results -->
<div class="dice-results">
    @foreach (var roll in diceRolls)
    {
        <div class="dice-roll">
            <strong>@roll.CharacterName</strong> a lancé <strong>@roll.DiceNotation</strong>
            <br />
            Résultats: [@string.Join(", ", roll.Results)]
            @if (roll.Modifiers != 0)
            {
                <span> + @roll.Modifiers</span>
            }
            <br />
            <strong>Total: @roll.TotalResult</strong>
            @if (!string.IsNullOrEmpty(roll.Context))
            {
                <em> (@roll.Context)</em>
            }
        </div>
    }
</div>

@code {
    [Parameter]
    public int SessionId { get; set; }
    
    private string sessionName = "";
    private string newMessage = "";
    private string diceNotation = "1d20";
    private string diceContext = "";
    private int currentCharacterId;
    
    private List<MessageReceivedEventArgs> messages = new();
    private List<DiceRolledEventArgs> diceRolls = new();
    
    protected override async Task OnInitializedAsync()
    {
        // Obtenir le token d'accès
        var token = await AuthService.GetAccessTokenAsync();
        
        // Initialiser la connexion SignalR
        await SessionHub.InitializeAsync(token);
        
        // S'abonner aux événements
        SessionHub.OnMessageReceived += HandleMessageReceived;
        SessionHub.OnDiceRolled += HandleDiceRolled;
        SessionHub.OnUserJoined += HandleUserJoined;
        SessionHub.OnUserLeft += HandleUserLeft;
        
        // Rejoindre la session
        await SessionHub.JoinSessionAsync(SessionId);
    }
    
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(newMessage))
            return;
        
        await SessionHub.SendMessageAsync(SessionId, newMessage);
        newMessage = "";
    }
    
    private async Task RollDice()
    {
        if (string.IsNullOrWhiteSpace(diceNotation))
            return;
        
        await SessionHub.RollDiceAsync(SessionId, currentCharacterId, diceNotation, diceContext);
        diceContext = "";
    }
    
    private Task HandleMessageReceived(MessageReceivedEventArgs args)
    {
        messages.Add(args);
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private Task HandleDiceRolled(DiceRolledEventArgs args)
    {
        diceRolls.Insert(0, args); // Ajouter au début
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private Task HandleUserJoined(UserJoinedEventArgs args)
    {
        messages.Add(new MessageReceivedEventArgs(
            args.UserId,
            "Système",
            $"{args.Username} a rejoint la session",
            args.JoinedAt
        ));
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private Task HandleUserLeft(UserLeftEventArgs args)
    {
        messages.Add(new MessageReceivedEventArgs(
            args.UserId,
            "Système",
            $"{args.Username} a quitté la session",
            args.LeftAt
        ));
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SendMessage();
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        // Se désabonner
        SessionHub.OnMessageReceived -= HandleMessageReceived;
        SessionHub.OnDiceRolled -= HandleDiceRolled;
        SessionHub.OnUserJoined -= HandleUserJoined;
        SessionHub.OnUserLeft -= HandleUserLeft;
        
        // Quitter la session
        if (SessionHub.IsConnected)
        {
            await SessionHub.LeaveSessionAsync(SessionId);
        }
    }
}
```

---

## 4. Patterns de sécurité

### 4.1 Validation d'accès dans les hubs

```csharp
public async Task JoinSession(int sessionId)
{
    var userId = GetUserId();
    
    // 1. Vérifier que l'utilisateur a accès à cette session
    var session = await _sessionRepository.GetByIdAsync(sessionId);
    if (session == null)
    {
        throw new HubException("Session not found");
    }
    
    var campaign = await _campaignRepository.GetByIdAsync(session.CampaignId);
    
    // 2. Vérifier que l'utilisateur est MJ ou participant
    var isGameMaster = campaign.GameMasterId == userId;
    var isParticipant = await _campaignRepository.IsParticipantAsync(campaign.Id, userId);
    
    if (!isGameMaster && !isParticipant)
    {
        throw new HubException("Access denied to this session");
    }
    
    // 3. Ajouter au groupe
    var groupName = GetSessionGroupName(sessionId);
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
}
```

---

### 4.2 Isolation par groupes

```csharp
// ✅ BON: Envoyer uniquement au groupe de la session
await Clients.Group($"session_{sessionId}").SendAsync("MessageReceived", message);

// ❌ MAUVAIS: Envoyer à tous les clients
await Clients.All.SendAsync("MessageReceived", message);
```

---

## 5. Gestion des erreurs et reconnexion

### 5.1 Politique de reconnexion

```csharp
.WithAutomaticReconnect(new[] {
    TimeSpan.Zero,           // Immédiatement
    TimeSpan.FromSeconds(2),  // Après 2 secondes
    TimeSpan.FromSeconds(5),  // Après 5 secondes
    TimeSpan.FromSeconds(10), // Après 10 secondes
    TimeSpan.FromSeconds(30)  // Après 30 secondes
})
```

---

### 5.2 Gestion des erreurs côté client

```csharp
try
{
    await SessionHub.SendMessageAsync(sessionId, message);
}
catch (HubException ex)
{
    // Erreur métier (ex: accès refusé)
    _logger.LogWarning(ex, "Hub exception: {Message}", ex.Message);
    await ShowNotification("Erreur", ex.Message);
}
catch (Exception ex)
{
    // Erreur technique (ex: connexion perdue)
    _logger.LogError(ex, "Unexpected error in SendMessage");
    await ShowNotification("Erreur", "Impossible d'envoyer le message. Vérifiez votre connexion.");
}
```

---

## 6. Performance et optimisation

### 6.1 Limites et throttling

```csharp
// Configuration dans Program.cs
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 102400; // 100 KB max
    options.StreamBufferCapacity = 10; // Buffer pour streaming
});

// Throttling côté serveur
private readonly SemaphoreSlim _messageLimiter = new(10, 10); // Max 10 messages simultanés

public async Task SendMessage(int sessionId, string message)
{
    await _messageLimiter.WaitAsync();
    try
    {
        // Logique d'envoi
    }
    finally
    {
        _messageLimiter.Release();
    }
}
```

---

### 6.2 Compression

```csharp
builder.Services.AddSignalR()
    .AddMessagePackProtocol(); // Protocole binaire plus efficace que JSON
```

---

## Résumé des patterns

| Pattern | Description |
|---------|-------------|
| **3 Hubs séparés** | SessionHub (chat/dés), CombatHub (combat), NotificationHub (notifs) |
| **Groupes SignalR** | `session_{id}`, `combat_{id}`, `user_{id}` pour isolation |
| **Authentication** | JWT token via AccessTokenProvider |
| **Reconnexion automatique** | Retry policy avec backoff exponentiel |
| **IHubContext** | Envoyer des notifications depuis les services |
| **Event-driven client** | Événements C# pour découpler les handlers |
| **Validation d'accès** | Vérifier permissions avant ajout aux groupes |

---

## Prochaines étapes

1. ✅ Implémenter les 3 hubs dans `Cdm.ApiService/Hubs/`
2. ✅ Créer les services clients dans `Cdm.Web/Services/`
3. ✅ Intégrer SignalR dans les composants Blazor
4. 🔄 Tester la reconnexion automatique
5. 🔄 Implémenter le throttling et rate limiting (Phase 6)
6. 🔄 Ajouter des tests unitaires pour les hubs

---

**Document créé le :** 15 octobre 2025  
**Dernière mise à jour :** 15 octobre 2025  
**Version :** 1.0
