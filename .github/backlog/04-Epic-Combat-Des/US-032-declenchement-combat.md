# US-032 - Déclenchement de Combat

## 📝 Description

**En tant que** Maître du Jeu  
**Je veux** déclencher un combat pendant une session  
**Afin de** gérer des affrontements structurés avec mes joueurs

---

## ✅ Critères d'Acceptation

### Fonctionnels
- [ ] Dans SessionLive, bouton "⚔️ Démarrer un Combat" (visible uniquement pour MJ)
- [ ] Clic → Modal configuration combat :
  - [ ] Nom du combat (ex: "Attaque des gobelins")
  - [ ] Sélection PNJ ennemis (liste PNJ du chapitre actuel)
  - [ ] Checkbox multi-sélection
  - [ ] Nombre d'exemplaires par PNJ (ex: 3x Gobelin)
  - [ ] HP individuels ou partagés
- [ ] Bouton "Lancer le Combat" → Création combat → Redirection CombatView
- [ ] Notification temps réel à TOUS les joueurs : "⚔️ Combat commencé !"
- [ ] Modal joueurs : "Rejoindre le Combat" → Redirection CombatView
- [ ] Interface CombatView avec :
  - [ ] **MJ** : Contrôles complets (gérer tours, HP, actions PNJ)
  - [ ] **Joueurs** : Actions limitées (leur tour uniquement)
  - [ ] Liste participants (Joueurs + PNJ)
  - [ ] Ordre d'initiative (initialisé vide, géré par US-033)
  - [ ] Bouton "Terminer le Combat" (MJ uniquement)

### Techniques
- [ ] Endpoint : `POST /api/sessions/{sessionId}/combats`
- [ ] Body :
```json
{
  "name": "Attaque des gobelins",
  "npcParticipants": [
    {
      "npcId": "guid",
      "count": 3,
      "sharedHP": false
    }
  ]
}
```
- [ ] Response 201 : `{ "combatId": "guid", "status": "Active", ... }`
- [ ] SignalR `CombatHub.CombatStarted` :
```csharp
await Clients.Group($"Session-{sessionId}")
    .SendAsync("CombatStarted", new
    {
        CombatId = combat.Id,
        CombatName = combat.Name,
        ParticipantsCount = combat.Participants.Count
    });
```

---

## 🧪 Tests

### Tests Unitaires
- [ ] `CombatService.StartCombat_ValidSession_CreatesCombat()`
- [ ] `CombatService.StartCombat_NonGameMaster_ThrowsUnauthorizedException()`
- [ ] `CombatService.StartCombat_CreatesParticipants()`
- [ ] `CombatService.StartCombat_MultipleNPCInstances_CreatesMultiple()`

### Tests d'Intégration
- [ ] `CombatEndpoint_StartCombat_CreatesInDatabase()`
- [ ] `CombatEndpoint_StartCombat_NotifiesPlayers()`

### Tests E2E
- [ ] MJ démarre combat → Joueurs reçoivent notification → Rejoignent CombatView
- [ ] MJ sélectionne 3 gobelins → 3 CombatParticipants créés

---

## 🔧 Tâches Techniques

### Backend
- [ ] Créer entité `Combat` :
```csharp
public class Combat
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Session Session { get; set; }
    
    public string Name { get; set; }
    public CombatStatus Status { get; set; }
    public int CurrentRound { get; set; }
    public int CurrentTurnIndex { get; set; }
    
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    // Relations
    public ICollection<CombatParticipant> Participants { get; set; }
    public ICollection<CombatRound> Rounds { get; set; }
}

public enum CombatStatus
{
    Active = 0,
    Paused = 1,
    Completed = 2
}
```
- [ ] Créer entité `CombatParticipant` :
```csharp
public class CombatParticipant
{
    public Guid Id { get; set; }
    public Guid CombatId { get; set; }
    public Combat Combat { get; set; }
    
    public Guid? CharacterId { get; set; } // Joueur
    public Character? Character { get; set; }
    
    public Guid? NPCId { get; set; } // PNJ
    public NPC? NPC { get; set; }
    
    public string DisplayName { get; set; } // Ex: "Gobelin 1", "Gobelin 2"
    public int InitiativeScore { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public bool IsDefeated { get; set; }
    public ParticipantType Type { get; set; } // Player, Ally, Enemy
}

public enum ParticipantType
{
    Player = 0,
    Ally = 1,
    Enemy = 2
}
```
- [ ] Créer `CombatService.StartCombatAsync(sessionId, dto, userId)` :
```csharp
public async Task<CombatDto> StartCombatAsync(Guid sessionId, CombatStartDto dto, Guid userId)
{
    var session = await _context.Sessions
        .Include(s => s.Campaign)
        .Include(s => s.Participants)
            .ThenInclude(p => p.User)
                .ThenInclude(u => u.Characters)
        .FirstOrDefaultAsync(s => s.Id == sessionId);
    
    if (session?.Campaign.CreatedBy != userId)
        throw new UnauthorizedException();
    
    if (session.Status != SessionStatus.Active)
        throw new ValidationException("La session doit être active");
    
    var combat = new Combat
    {
        SessionId = sessionId,
        Name = dto.Name,
        Status = CombatStatus.Active,
        CurrentRound = 1,
        CurrentTurnIndex = 0,
        StartedAt = DateTime.UtcNow
    };
    
    _context.Combats.Add(combat);
    
    // Ajouter joueurs comme participants
    foreach (var sessionParticipant in session.Participants.Where(p => p.Status == ParticipantStatus.Connected))
    {
        var character = sessionParticipant.User.Characters.FirstOrDefault(); // Simplification
        if (character != null)
        {
            combat.Participants.Add(new CombatParticipant
            {
                CombatId = combat.Id,
                CharacterId = character.Id,
                DisplayName = character.Name,
                InitiativeScore = 0, // À définir par US-033
                CurrentHP = character.CurrentHP,
                MaxHP = character.MaxHP,
                Type = ParticipantType.Player
            });
        }
    }
    
    // Ajouter PNJ ennemis
    foreach (var npcDto in dto.NPCParticipants)
    {
        var npc = await _context.NPCs.FindAsync(npcDto.NPCId);
        if (npc == null) continue;
        
        for (int i = 1; i <= npcDto.Count; i++)
        {
            combat.Participants.Add(new CombatParticipant
            {
                CombatId = combat.Id,
                NPCId = npc.Id,
                DisplayName = npcDto.Count > 1 ? $"{npc.Name} {i}" : npc.Name,
                InitiativeScore = 0,
                CurrentHP = npcDto.SharedHP && i > 1 ? 0 : npc.CurrentHP, // Logique shared HP
                MaxHP = npc.MaxHP,
                Type = npc.NPCType == NPCType.Enemy ? ParticipantType.Enemy : ParticipantType.Ally
            });
        }
    }
    
    await _context.SaveChangesAsync();
    
    // Notifier via SignalR
    await _combatHub.Clients.Group($"Session-{sessionId}")
        .SendAsync("CombatStarted", new
        {
            CombatId = combat.Id,
            CombatName = combat.Name,
            ParticipantsCount = combat.Participants.Count
        });
    
    // Log SessionEvent
    _context.SessionEvents.Add(new SessionEvent
    {
        SessionId = sessionId,
        Type = SessionEventType.CombatStarted,
        Data = JsonSerializer.Serialize(new { CombatId = combat.Id, Name = combat.Name }),
        OccurredAt = DateTime.UtcNow,
        TriggeredBy = userId
    });
    
    await _context.SaveChangesAsync();
    
    return combat.ToDto();
}
```
- [ ] Créer endpoint `POST /api/sessions/{sessionId}/combats` [Authorize(Roles = "GameMaster")]
- [ ] Créer `CombatHub` SignalR :
```csharp
public class CombatHub : Hub
{
    public async Task JoinCombat(Guid combatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Combat-{combatId}");
    }
    
    public async Task LeaveCombat(Guid combatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Combat-{combatId}");
    }
}
```

### Frontend
- [ ] Créer modal `StartCombatModal.razor` (dans SessionLive) :
```razor
<Modal IsVisible="@IsVisible" OnClose="OnClose" Size="large">
    <div class="modal-header">
        <h3>⚔️ Démarrer un Combat</h3>
    </div>
    
    <EditForm Model="@Model" OnValidSubmit="StartCombat">
        <div class="modal-body">
            <div class="form-group">
                <label>Nom du combat *</label>
                <InputText @bind-Value="Model.Name" 
                          class="form-control" 
                          placeholder="Ex: Attaque des gobelins" />
            </div>
            
            <div class="form-group">
                <label>PNJ ennemis</label>
                <div class="npc-selection">
                    @foreach (var npc in AvailableNPCs)
                    {
                        <div class="npc-item">
                            <input type="checkbox" 
                                   @onchange="e => ToggleNPC(npc.Id, e.Value)" />
                            <img src="@(npc.AvatarUrl ?? "/images/default-npc.png")" 
                                 alt="@npc.Name" 
                                 class="npc-avatar-small" />
                            <span>@npc.Name</span>
                            
                            @if (SelectedNPCs.ContainsKey(npc.Id))
                            {
                                <input type="number" 
                                       @bind="SelectedNPCs[npc.Id].Count" 
                                       min="1" 
                                       max="10"
                                       class="npc-count" />
                                <label>
                                    <input type="checkbox" 
                                           @bind="SelectedNPCs[npc.Id].SharedHP" />
                                    HP partagés
                                </label>
                            }
                        </div>
                    }
                </div>
            </div>
        </div>
        
        <div class="modal-footer">
            <button type="submit" class="btn-primary">⚔️ Lancer le Combat</button>
            <button @onclick="OnClose" type="button" class="btn-secondary">Annuler</button>
        </div>
    </EditForm>
</Modal>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public Guid SessionId { get; set; }
    [Parameter] public EventCallback<Guid> OnCombatStarted { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private CombatStartDto Model { get; set; } = new();
    private List<NPCDto> AvailableNPCs { get; set; } = new();
    private Dictionary<Guid, NPCParticipantDto> SelectedNPCs { get; set; } = new();

    protected override async Task OnParametersSetAsync()
    {
        if (IsVisible)
        {
            // Charger PNJ du chapitre actuel
            AvailableNPCs = await Http.GetFromJsonAsync<List<NPCDto>>(
                $"/api/sessions/{SessionId}/available-npcs") ?? new();
        }
    }

    private void ToggleNPC(Guid npcId, object? isChecked)
    {
        if (isChecked is bool b && b)
            SelectedNPCs[npcId] = new NPCParticipantDto { NPCId = npcId, Count = 1, SharedHP = false };
        else
            SelectedNPCs.Remove(npcId);
    }

    private async Task StartCombat()
    {
        Model.NPCParticipants = SelectedNPCs.Values.ToList();
        
        var response = await Http.PostAsJsonAsync($"/api/sessions/{SessionId}/combats", Model);
        
        if (response.IsSuccessStatusCode)
        {
            var combat = await response.Content.ReadFromJsonAsync<CombatDto>();
            await OnCombatStarted.InvokeAsync(combat!.Id);
        }
    }
}
```
- [ ] Créer page `CombatView.razor` (/combats/{id}) :
```razor
@page "/combats/{id:guid}"
@inject HttpClient Http
@inject NavigationManager Nav
@inject SignalRCombatService SignalR

<div class="combat-view">
    @if (Combat != null)
    {
        <div class="combat-header">
            <h1>⚔️ @Combat.Name</h1>
            <span class="round-indicator">Round @Combat.CurrentRound</span>
        </div>
        
        <div class="combat-content">
            <!-- Initiative Tracker (US-033) -->
            <InitiativeTracker CombatId="@Id" Participants="@Combat.Participants" />
            
            <!-- Combat Actions (US-035) -->
            <CombatActions CombatId="@Id" CurrentParticipant="@GetCurrentParticipant()" />
            
            <!-- Combat Log -->
            <CombatLog CombatId="@Id" />
        </div>
        
        @if (IsGameMaster)
        {
            <div class="combat-controls">
                <button @onclick="EndCombat" class="btn-danger">Terminer le Combat</button>
            </div>
        }
    }
</div>

@code {
    [Parameter] public Guid Id { get; set; }
    
    private CombatDto? Combat { get; set; }
    private bool IsGameMaster { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Combat = await Http.GetFromJsonAsync<CombatDto>($"/api/combats/{Id}");
        await SignalR.ConnectAsync(Id);
    }

    private CombatParticipantDto? GetCurrentParticipant()
    {
        // Logique pour obtenir participant actuel (US-034)
        return null;
    }

    private async Task EndCombat()
    {
        // US-039
    }
}
```

### Base de Données
- [ ] Migration : Créer tables `Combats`, `CombatParticipants`, `CombatRounds`
- [ ] Index sur `SessionId` pour requêtes combats par session

---

## 🔗 Dépendances

### Dépend de
- [US-018](../02-Epic-Gestion-Parties/US-018-lancement-session.md) - Session active
- [US-027](../03-Epic-Personnages-PNJ/US-027-creation-pnj.md) - PNJ disponibles

### Bloque
- [US-033](./US-033-initiative-manuelle.md) - Initiative
- [US-034](./US-034-gestion-tours.md) - Tours de combat

---

## 📊 Estimation

**Story Points** : 8

**Détails** :
- Complexité : Haute (Combat entity, participants, SignalR)
- Effort : 2-3 jours
- Risques : Synchronisation temps réel

---

## 📝 Notes Techniques

### Shared HP pour PNJ
Si `SharedHP = true` pour 3 gobelins :
- 1 seul `CombatParticipant` avec HP × 3
- Ou 3 `CombatParticipant` partageant même pool HP (référence commune)

**Retenu** : 3 participants distincts, HP mise à jour manuellement par MJ

---

## ✅ Definition of Done

- [ ] Code implémenté et testé
- [ ] Tests unitaires passent
- [ ] Tests d'intégration passent
- [ ] Modal démarrage combat fonctionnelle
- [ ] Sélection PNJ multi-instances
- [ ] Combat créé en base avec participants
- [ ] Notification SignalR envoyée
- [ ] CombatView accessible
- [ ] Documentation API mise à jour
- [ ] Mergé dans main

---

**Statut** : 📝 Planifié  
**Assigné à** : À définir  
**Sprint** : Sprint 9
